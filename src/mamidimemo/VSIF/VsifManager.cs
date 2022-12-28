// copyright-holders:K.Ito
using FTD2XX_NET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using zanac.MAmidiMEmo.ComponentModel;
using zanac.MAmidiMEmo.Gui;
using zanac.MAmidiMEmo.Instruments;
using zanac.MAmidiMEmo.Properties;
using zanac.MAmidiMEmo.VSIF;

namespace zanac.MAmidiMEmo.VSIF
{

    public static class VsifManager
    {
        //http://analoghome.blogspot.com/2017/08/ftdi-ft232r-usb-to-serial-bridge.html
        //The maximum BAUD rate for the FT232R chip is 3M BAUD

        public const int FTDI_BAUDRATE_GEN = 115200 / 16;
        public const int FTDI_BAUDRATE_GEN_MUL = 100;

        public const int FTDI_BAUDRATE_NES = 57600 / 16;
        public const int FTDI_BAUDRATE_NES_MUL = 100;

        public const int FTDI_BAUDRATE_C64 = 31250 / 16;
        public const int FTDI_BAUDRATE_C64_MUL = 100;

        //public const int FTDI_BAUDRATE_MSX = 9600 / 16;
        public const int FTDI_BAUDRATE_MSX = 38400 / 16;
        public const int FTDI_BAUDRATE_MSX_MUL = 100;

        private static object lockObject = new object();

        private static List<VsifClient> vsifClients = new List<VsifClient>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iSoundChipType"></param>
        /// <param name="clock"></param>
        /// <returns></returns>
        public static VsifClient TryToConnectVSIF(VsifSoundModuleType soundModule, PortId comPort)
        {
            return TryToConnectVSIF(soundModule, comPort, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iSoundChipType"></param>
        /// <param name="clock"></param>
        /// <returns></returns>
        public static VsifClient TryToConnectVSIF(VsifSoundModuleType soundModule, PortId comPort, bool shareOnly)
        {
            lock (lockObject)
            {
                foreach (var c in vsifClients)
                {
                    if (c.SoundModuleType == soundModule)
                    {
                        if (c.SerialPort.PortName.Equals("COM" + (int)(comPort + 1)))
                        {
                            c.ReferencedCount++;
                            return c;
                        }
                        if (c.SerialPort.PortName.Equals("FTDI_COM" + (int)comPort))
                        {
                            c.ReferencedCount++;
                            return c;
                        }
                    }
                }
                if (shareOnly)
                    return null;

                try
                {
                    switch (soundModule)
                    {
                        case VsifSoundModuleType.SMS:
                            {
                                SerialPort sp = null;
                                sp = new SerialPort("COM" + ((int)comPort + 1));
                                sp.WriteTimeout = 100;
                                sp.ReadTimeout = 100;
                                sp.BaudRate = 115200;
                                sp.StopBits = StopBits.Two;
                                //sp.BaudRate = 57600;
                                //sp.StopBits = StopBits.One;
                                sp.Parity = Parity.None;
                                sp.DataBits = 8;
                                sp.Handshake = Handshake.None;
                                sp.Open();
                                var client = new VsifClient(soundModule, new PortWriterGenesis(sp));
                                client.Disposed += Client_Disposed;
                                vsifClients.Add(client);
                                return client;
                            }
                        case VsifSoundModuleType.Genesis:
                            {
                                SerialPort sp = null;
                                sp = new SerialPort("COM" + ((int)comPort + 1));
                                sp.WriteTimeout = 500;
                                sp.ReadTimeout = 500;
                                //sp.BaudRate = 230400;
                                sp.BaudRate = 163840;
                                sp.StopBits = StopBits.One;
                                sp.Parity = Parity.None;
                                sp.DataBits = 8;
                                sp.Open();
                                var client = new VsifClient(soundModule, new PortWriterGenesis(sp));
                                client.Disposed += Client_Disposed;
                                vsifClients.Add(client);
                                return client;
                            }
                        case VsifSoundModuleType.Genesis_Low:
                            {
                                SerialPort sp = null;
                                sp = new SerialPort("COM" + ((int)comPort + 1));
                                sp.WriteTimeout = 500;
                                sp.ReadTimeout = 500;
                                sp.BaudRate = 115200;
                                sp.StopBits = StopBits.One;
                                sp.Parity = Parity.None;
                                sp.DataBits = 8;
                                sp.Open();
                                var client = new VsifClient(soundModule, new PortWriterGenesis(sp));
                                client.Disposed += Client_Disposed;
                                vsifClients.Add(client);
                                return client;
                            }
                        case VsifSoundModuleType.Genesis_FTDI:
                            {
                                var ftdi = new FTD2XX_NET.FTDI();
                                var stat = ftdi.OpenByIndex((uint)comPort);
                                if (stat == FTDI.FT_STATUS.FT_OK)
                                {
                                    ftdi.SetBitMode(0x00, FTDI.FT_BIT_MODES.FT_BIT_MODE_RESET);
                                    ftdi.SetBitMode(0xff, FTDI.FT_BIT_MODES.FT_BIT_MODE_ASYNC_BITBANG);
                                    ftdi.SetBaudRate(FTDI_BAUDRATE_GEN * FTDI_BAUDRATE_GEN_MUL);
                                    ftdi.SetTimeouts(500, 500);
                                    ftdi.SetLatency(0);

                                    var client = new VsifClient(soundModule, new PortWriterGenesis(ftdi, comPort));
                                    
                                    client.WriteData(0, 0x14, (byte)(0x80 | 0 << 5 | 0x1f), 100);
                                    client.WriteData(0, 0x14, (byte)(0x80 | 1 << 5 | 0x1f), 100);
                                    client.WriteData(0, 0x14, (byte)(0x80 | 2 << 5 | 0x1f), 100);
https://twitter.com/home                                    client.WriteData(0, 0x14, (byte)(0x80 | 3 << 5 | 0x1f), 100);

                                    client.Disposed += Client_Disposed;
                                    vsifClients.Add(client);
                                    return client;
                                }
                            }
                            break;
                        case VsifSoundModuleType.MSX_FTDI:
                        case VsifSoundModuleType.P6_FTDI:
                            {
                                var ftdi = new FTD2XX_NET.FTDI();
                                var stat = ftdi.OpenByIndex((uint)comPort);
                                if (stat == FTDI.FT_STATUS.FT_OK)
                                {
                                    ftdi.SetBitMode(0x00, FTDI.FT_BIT_MODES.FT_BIT_MODE_RESET);
                                    ftdi.SetBitMode(0xff, FTDI.FT_BIT_MODES.FT_BIT_MODE_ASYNC_BITBANG);
                                    ftdi.SetBaudRate(FTDI_BAUDRATE_MSX * FTDI_BAUDRATE_MSX_MUL);
                                    ftdi.SetTimeouts(500, 500);
                                    ftdi.SetLatency(0);

                                    var client = new VsifClient(soundModule, new PortWriterMsx(ftdi, comPort));
                                    client.WriteData(0, 0, 0, (int)100);  //Dummy

                                    client.Disposed += Client_Disposed;
                                    vsifClients.Add(client);
                                    return client;
                                }
                            }
                            break;
                        case VsifSoundModuleType.NES_FTDI_INDIRECT:
                        case VsifSoundModuleType.NES_FTDI_DIRECT:
                            {
                                var ftdi = new FTD2XX_NET.FTDI();
                                var stat = ftdi.OpenByIndex((uint)comPort);
                                if (stat == FTDI.FT_STATUS.FT_OK)
                                {
                                    ftdi.SetBitMode(0x00, FTDI.FT_BIT_MODES.FT_BIT_MODE_RESET);
                                    ftdi.SetBitMode(0xff, FTDI.FT_BIT_MODES.FT_BIT_MODE_ASYNC_BITBANG);
                                    ftdi.SetBaudRate(FTDI_BAUDRATE_NES * FTDI_BAUDRATE_NES_MUL);
                                    ftdi.SetTimeouts(500, 500);
                                    ftdi.SetLatency(0);
                                    byte ps = 0;
                                    ftdi.GetPinStates(ref ps);
                                    if ((ps & 0x10) == 0x10)
                                    {
                                        uint dummy = 0;
                                        ftdi.Write(new byte[] { 0x00 }, 1, ref dummy);
                                    }

                                    VsifClient client = null;
                                    switch (soundModule)
                                    {
                                        case VsifSoundModuleType.NES_FTDI_DIRECT:
                                            client = new VsifClient(soundModule, new PortWriterNesDirect(ftdi, comPort));
                                            break;
                                        case VsifSoundModuleType.NES_FTDI_INDIRECT:
                                            client = new VsifClient(soundModule, new PortWriterNesIndirect(ftdi, comPort));
                                            break;
                                    }

                                    client.Disposed += Client_Disposed;
                                    vsifClients.Add(client);
                                    return client;
                                }
                            }
                            break;
                        case VsifSoundModuleType.C64_FTDI:
                            {
                                var ftdi = new FTD2XX_NET.FTDI();
                                var stat = ftdi.OpenByIndex((uint)comPort);
                                if (stat == FTDI.FT_STATUS.FT_OK)
                                {
                                    ftdi.SetBitMode(0x00, FTDI.FT_BIT_MODES.FT_BIT_MODE_RESET);
                                    ftdi.SetBitMode(0xff, FTDI.FT_BIT_MODES.FT_BIT_MODE_ASYNC_BITBANG);
                                    ftdi.SetBaudRate(FTDI_BAUDRATE_C64 * FTDI_BAUDRATE_C64_MUL);
                                    ftdi.SetTimeouts(500, 500);
                                    ftdi.SetLatency(0);
                                    {
                                        uint dummy = 0;
                                        ftdi.Write(new byte[] { 0x1f }, 1, ref dummy);
                                    }

                                    VsifClient client = new VsifClient(soundModule, new PortWriterC64(ftdi, comPort));

                                    client.Disposed += Client_Disposed;
                                    vsifClients.Add(client);
                                    return client;
                                }
                            }
                            break;
                    }

                    //sp.Write(new byte[] { (byte)'M', (byte)'a', (byte)'M', (byte)'i' }, 0, 4);
                    //sp.BaseStream.WriteByte((byte)soundModule);
                    //Thread.Sleep(100);
                    //var ret = sp.BaseStream.ReadByte();
                    //if (ret == 0x0F)   //OK
                }
                catch (Exception ex)
                {
                    if (ex.GetType() == typeof(Exception))
                        throw;
                    else if (ex.GetType() == typeof(SystemException))
                        throw;
                }
                return null;
            }
        }

        private static void Client_Disposed(object sender, EventArgs e)
        {
            lock (lockObject)
            {
                foreach (var c in vsifClients)
                {
                    if (c == sender)
                    {
                        vsifClients.Remove(c);
                        break;
                    }
                }
            }
        }
    }

    public enum VsifSoundModuleType
    {
        None,
        SMS,
        Genesis,
        Genesis_FTDI,
        Genesis_Low,
        NES_FTDI_INDIRECT,
        NES_FTDI_DIRECT,
        MSX_FTDI,
        C64_FTDI,
        P6_FTDI,
    }


}
