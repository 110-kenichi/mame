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
using zanac.MAmidiMEmo.Gimic;
using zanac.VGMPlayer.Properties;

namespace zanac.VGMPlayer
{

    public static class VsifManager
    {
        //http://analoghome.blogspot.com/2017/08/ftdi-ft232r-usb-to-serial-bridge.html
        //The maximum BAUD rate for the FT232R chip is 3M BAUD
        public const int FTDI_BAUDRATE_MAX = 3000000;

        //                double rate = Math.Round(3000000d / ((double)numericUpDownDiv1.Value + (double)numericUpDownDiv2.Value));


        // 260870 = 3000000 / 11.5
        public const int FTDI_BAUDRATE_GEN = 11;

        public const int FTDI_BAUDRATE_NES = 352941;
        public const int FTDI_BAUDRATE_NES_CLK_WIDTH = 11;

        // 240000 = 3000000 / 12.5
        public const int FTDI_BAUDRATE_MSX = 12;

        public const int FTDI_BAUDRATE_88 = 12;

        private static object lockObject = new object();

        private static List<VsifClient> vsifClients = new List<VsifClient>();

        public static IReadOnlyCollection<VsifClient> GetVsifClients()
        {
            return vsifClients.AsReadOnly();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iSoundChipType"></param>
        /// <param name="clock"></param>
        /// <returns></returns>
        public static VsifClient TryToConnectVSIF(VsifSoundModuleType soundModule)
        {
            return TryToConnectVSIF(soundModule, 0, 0, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iSoundChipType"></param>
        /// <param name="clock"></param>
        /// <returns></returns>
        public static VsifClient TryToConnectVSIF(VsifSoundModuleType soundModule, PortId comPort)
        {
            return TryToConnectVSIF(soundModule, comPort, 0, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iSoundChipType"></param>
        /// <param name="clock"></param>
        /// <returns></returns>
        public static VsifClient TryToConnectVSIF(VsifSoundModuleType soundModule, PortId comPort, int offset, bool shareOnly)
        {
            lock (lockObject)
            {
                foreach (var c in vsifClients)
                {
                    if (c.SoundModuleType == soundModule)
                    {
                        if (c.SoundModuleType == VsifSoundModuleType.Gimic)
                        {
                            if (c.DataWriter.PortName.Equals("Gimic"))
                            {
                                c.ReferencedCount++;
                                return c;
                            }
                        }
                        else
                        {
                            if (c.DataWriter.PortName.Equals("COM" + (int)(comPort + 1)))
                            {
                                c.ReferencedCount++;
                                return c;
                            }
                            if (c.DataWriter.PortName.Equals("FTDI_COM" + (int)comPort))
                            {
                                c.ReferencedCount++;
                                return c;
                            }
                        }
                    }
                }
                if (shareOnly)
                    return null;

                try
                {
                    switch (soundModule)
                    {
                        case VsifSoundModuleType.Generic_UART:
                            {
                                SerialPort sp = null;
                                sp = new SerialPort("COM" + ((int)comPort + 1));
                                sp.WriteTimeout = 100;
                                sp.ReadTimeout = 100;
                                sp.BaudRate = 115200;
                                sp.BaudRate = 1200 * (int)Settings.Default.BitBangWaitAY8910;
                                sp.StopBits = StopBits.One;
                                sp.DataBits = 8;
                                sp.Handshake = Handshake.None;
                                sp.Open();
                                var client = new VsifClient(soundModule, new PortWriterGenericUart(sp));
                                client.Disposed += Client_Disposed;
                                vsifClients.Add(client);
                                return client;
                            }
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
                                    var ofst = FTDI_BAUDRATE_GEN + offset;
                                    if (ofst < 0)
                                        ofst = 0;
                                    ftdi.SetBaudRate((uint)(3000000 / (ofst + 0.5)));
                                    ftdi.SetTimeouts(500, 500);
                                    ftdi.SetLatency(0);

                                    var client = new VsifClient(soundModule, new PortWriterGenesis(ftdi, comPort));

                                    client.WriteData(0, 0x14, (byte)(0x80 | 0 << 5 | 0x1f), 100);
                                    client.WriteData(0, 0x14, (byte)(0x80 | 1 << 5 | 0x1f), 100);
                                    client.WriteData(0, 0x14, (byte)(0x80 | 2 << 5 | 0x1f), 100);
                                    client.WriteData(0, 0x14, (byte)(0x80 | 3 << 5 | 0x1f), 100);

                                    //ftdi.Write(new byte[] { (byte)(((0x07 << 1) & 0xe) | 0) }, 1, ref dummy);
                                    //ftdi.Write(new byte[] { (byte)(((0x38 >> 2) & 0xe) | 1) }, 1, ref dummy);
                                    //ftdi.Write(new byte[] { (byte)(((0xC0 >> 5) & 0xe) | 0) }, 1, ref dummy);
                                    //ftdi.Write(new byte[] { 1 }, 1, ref dummy);

                                    client.Disposed += Client_Disposed;
                                    vsifClients.Add(client);
                                    return client;
                                }
                            }
                            break;
                        case VsifSoundModuleType.MSX_FTDI:
                            {
                                var ftdi = new FTD2XX_NET.FTDI();
                                var stat = ftdi.OpenByIndex((uint)comPort);
                                if (stat == FTDI.FT_STATUS.FT_OK)
                                {
                                    ftdi.SetBitMode(0x00, FTDI.FT_BIT_MODES.FT_BIT_MODE_RESET);
                                    ftdi.SetBitMode(0xff, FTDI.FT_BIT_MODES.FT_BIT_MODE_ASYNC_BITBANG);
                                    var ofst = FTDI_BAUDRATE_MSX + offset;
                                    if (ofst < 0)
                                        ofst = 0;
                                    ftdi.SetBaudRate((uint)(3000000 / (ofst + 0.5)));
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
                        case VsifSoundModuleType.PC88_FTDI:
                            {
                                var ftdi = new FTD2XX_NET.FTDI();
                                var stat = ftdi.OpenByIndex((uint)comPort);
                                if (stat == FTDI.FT_STATUS.FT_OK)
                                {
                                    ftdi.SetBitMode(0x00, FTDI.FT_BIT_MODES.FT_BIT_MODE_RESET);
                                    ftdi.SetBitMode(0xff, FTDI.FT_BIT_MODES.FT_BIT_MODE_ASYNC_BITBANG);
                                    var ofst = FTDI_BAUDRATE_88 + offset;
                                    if (ofst < 0)
                                        ofst = 0;
                                    ftdi.SetBaudRate((uint)(3000000 / (ofst + 0.5)));
                                    ftdi.SetTimeouts(500, 500);
                                    ftdi.SetLatency(0);

                                    var client = new VsifClient(soundModule, new PortWriterPC88(ftdi, comPort));
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
                                    ftdi.SetBaudRate(FTDI_BAUDRATE_NES);
                                    ftdi.SetTimeouts(500, 500);
                                    ftdi.SetLatency(0);
                                    byte ps = 0;
                                    ftdi.GetPinStates(ref ps);
                                    if ((ps & 0x10) == 0x10)
                                    {
                                        uint dummy = 0;
                                        ftdi.Write(new byte[] { 0xff }, 1, ref dummy);
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
                        case VsifSoundModuleType.Spfm:
                            {
                                SerialPort sp = new SerialPort("COM" + ((int)comPort + 1));
                                sp.WriteTimeout = 100;
                                sp.ReadTimeout = 100;
                                sp.BaudRate = 250 * 1000 * 1000;
                                sp.StopBits = StopBits.One;
                                sp.Parity = Parity.None;
                                sp.DataBits = 8;
                                sp.Handshake = Handshake.None;
                                sp.WriteBufferSize = 4;
                                sp.Open();
                                sp.Write(new byte[] { 0xff }, 0, 1);
                                var client = new VsifClient(soundModule, new PortWriterSpfm(sp));
                                client.Disposed += Client_Disposed;
                                vsifClients.Add(client);
                                return client;
                            }
                        case VsifSoundModuleType.SpfmLight:
                            {
                                SerialPort sp = new SerialPort("COM" + ((int)comPort + 1));
                                sp.WriteTimeout = 100;
                                sp.ReadTimeout = 100;
                                sp.BaudRate = 250 * 1000 * 1000;
                                sp.StopBits = StopBits.One;
                                sp.Parity = Parity.None;
                                sp.DataBits = 8;
                                sp.Handshake = Handshake.None;
                                sp.WriteBufferSize = 4;
                                sp.Open();
                                sp.Write(new byte[] { 0xff }, 0, 1);
                                sp.Write(new byte[] { 0xfe }, 0, 1);
                                var client = new VsifClient(soundModule, new PortWriterSpfmLight(sp));
                                client.Disposed += Client_Disposed;
                                vsifClients.Add(client);
                                return client;
                            }
                        case VsifSoundModuleType.Gimic:
                            {
                                GimicManager.TryInitializeGimmic();
                                if (GimicManager.IsScciInitialized)
                                {
                                    var client = new VsifClient(soundModule, new PortWriterGimic());
                                    client.Disposed += Client_Disposed;
                                    vsifClients.Add(client);
                                    return client;
                                }
                                break;
                            }
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
        Generic_UART,
        Spfm,
        SpfmLight,
        Gimic,
        PC88_FTDI
    }


}
