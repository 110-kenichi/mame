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

//  CS Lo -> Hi via MPSSE
//  Set Address: A0 Hi
//  Set Data: A0 Lo
//  Data: GPIO(D0 - D7) via GPIO

namespace zanac.MAmidiMEmo.eVSIF
{

    public static class eVsifManager
    {

        private static object lockObject = new object();

        private static List<eVsifClient> vsifClients = new List<eVsifClient>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iSoundChipType"></param>
        /// <param name="clock"></param>
        /// <returns></returns>
        public static eVsifClient[] TryGetConnectedVSIF(eVsifSoundModuleType soundModule)
        {
            List<eVsifClient> list = new List<eVsifClient>();

            lock (lockObject)
            {
                foreach (var c in vsifClients)
                {
                    if (c.SoundModuleType == soundModule)
                        list.Add(c);
                }
            }

            return list.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iSoundChipType"></param>
        /// <param name="clock"></param>
        /// <returns></returns>
        public static eVsifClient TryToConnecteVSIF(eVsifSoundModuleType soundModule, PortId portId, int slotId)
        {
            return TryToConnecteVSIF(soundModule, portId, slotId, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iSoundChipType"></param>
        /// <param name="clock"></param>
        /// <returns></returns>
        public static eVsifClient TryToConnecteVSIF(eVsifSoundModuleType soundModule, PortId portId, int slotId, bool shareOnly)
        {
            lock (lockObject)
            {
                foreach (var c in vsifClients)
                {
                    if (c.SoundModuleType == soundModule)
                    {
                        if (c.DataWriter.PortName.Equals("FTDI_COM" + (int)portId))
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
                        case eVsifSoundModuleType.YM2413:
                            {
                                var ftdi = new FTD2XX_NET.FTDI();
                                var stat = ftdi.OpenByIndex((uint)portId);
                                if (stat == FTDI.FT_STATUS.FT_OK)
                                {
                                    ftdi.SetBitMode(0x00, FTDI.FT_BIT_MODES.FT_BIT_MODE_RESET);
                                    ftdi.SetBitMode(0x00, FTDI.FT_BIT_MODES.FT_BIT_MODE_MPSSE);
                                    ftdi.SetBaudRate(3000000);
                                    ftdi.SetTimeouts(500, 500);
                                    ftdi.SetLatency(0);
                                    ftdi.Write(new byte[] { 0x80, });

                                    VsifClient client = new VsifClient(soundModule, new PortWriterC64(ftdi, portId));

                                    client.Disposed += Client_Disposed;
                                    vsifClients.Add(client);
                                    return client;
                                }
                            }
                            break;
                    }
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

    public enum eVsifSoundModuleType
    {
        None,
        YM2413,
    }


}
