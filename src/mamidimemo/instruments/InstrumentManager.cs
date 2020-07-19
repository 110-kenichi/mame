// copyright-holders:K.Ito
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using zanac.MAmidiMEmo.ComponentModel;
using zanac.MAmidiMEmo.Properties;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Threading;
using zanac.MAmidiMEmo.Mame;
using Melanchall.DryWetMidi.Core;

namespace zanac.MAmidiMEmo.Instruments
{
    public static class InstrumentManager
    {
        /// <summary>
        /// Exclusive control between GUI/MIDI Event/Other managed threads for Instrument objects;
        /// </summary>
        public static object ExclusiveLockObject = new object();

        private static List<List<InstrumentBase>> instruments = new List<List<InstrumentBase>>();

        public static GeneralPurposeControlSettings GPCS
        {
            get;
            set;
        } = new GeneralPurposeControlSettings();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr delegate_getLastOutputBuffer(string tagName, int insts, uint deviceID, uint unitNumber);

        private static delegate_getLastOutputBuffer getLastOutputBuffer;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int delegate_getLastOutputBufferSamples(string tagName);

        private static delegate_getLastOutputBufferSamples getLastOutputBufferSamples;

        /// <summary>
        /// 
        /// </summary>
        static InstrumentManager()
        {
            var funcPtr = MameIF.GetProcAddress("getLastOutputBuffer");
            if (funcPtr != IntPtr.Zero)
                getLastOutputBuffer = Marshal.GetDelegateForFunctionPointer<delegate_getLastOutputBuffer>(funcPtr);

            funcPtr = MameIF.GetProcAddress("getLastOutputBufferSamples");
            if (funcPtr != IntPtr.Zero)
                getLastOutputBufferSamples = Marshal.GetDelegateForFunctionPointer<delegate_getLastOutputBufferSamples>(funcPtr);

            Midi.MidiManager.MidiEventReceived += MidiManager_MidiEventReceived;

            for (int i = 0; i < Enum.GetNames(typeof(InstrumentType)).Length; i++)
                instruments.Add(new List<InstrumentBase>());

            NrpnLsb = new byte[16];
            NrpnMsb = new byte[16];
            RpnLsb = new byte[16];
            RpnMsb = new byte[16];
            DataLsb = new byte[16];
            DataMsb = new byte[16];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<InstrumentBase> GetAllInstruments()
        {
            List<InstrumentBase> insts = new List<InstrumentBase>();
            lock (InstrumentManager.ExclusiveLockObject)
            {
                foreach (List<InstrumentBase> i in instruments)
                    insts.AddRange(i);
            }
            return insts.AsEnumerable();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<InstrumentBase> GetInstruments(uint deviceId)
        {
            lock (InstrumentManager.ExclusiveLockObject)
            {
                return instruments[(int)deviceId - 1];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<InstrumentBase> ClearAllInstruments()
        {
            List<InstrumentBase> insts = new List<InstrumentBase>();
            lock (InstrumentManager.ExclusiveLockObject)
            {
                for (int i = instruments.Count - 1; i >= 0; i--)
                {
                    for (int j = instruments[i].Count - 1; j >= 0; j--)
                    {
                        instruments[i][j].Dispose();
                        instruments[i].RemoveAt(j);
                    }
                }
            }
            return insts.AsEnumerable();
        }

        /// <summary>
        /// 
        /// </summary>
        public static void RestoreSettings(EnvironmentSettings settings)
        {
            lock (InstrumentManager.ExclusiveLockObject)
            {
                if (settings.Instruments != null)
                {
                    foreach (int v in Enum.GetValues(typeof(InstrumentType)))
                    {
                        if (v < settings.Instruments.Count && settings.Instruments[v] != null)
                        {
                            try
                            {
                                //clear current insts
                                for (int i = instruments[v].Count - 1; i >= 0; i--)
                                {
                                    instruments[v][i].Dispose();
                                    instruments[v].RemoveAt(i);
                                }
                                //prepare new insts
                                foreach (InstrumentBase inst in settings.Instruments[v])
                                    inst.PrepareSound();
                                //set new insts
                                InstrumentManager.instruments[v] = settings.Instruments[v];
                            }
                            catch (Exception ex)
                            {
                                if (ex.GetType() == typeof(Exception))
                                    throw;
                                else if (ex.GetType() == typeof(SystemException))
                                    throw;


                                System.Windows.Forms.MessageBox.Show(ex.ToString());
                            }
                        }
                    }
                }

                InstrumentChanged?.Invoke(typeof(InstrumentManager), EventArgs.Empty);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static void SaveSettings(EnvironmentSettings settings)
        {
            settings.Instruments = instruments;
        }

        /// <summary>
        /// 
        /// </summary>
        public static event EventHandler<EventArgs> InstrumentAdded;

        /// <summary>
        /// 
        /// </summary>
        public static event EventHandler<EventArgs> InstrumentRemoved;

        /// <summary>
        /// 
        /// </summary>
        public static event EventHandler<EventArgs> InstrumentChanged;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="instrumentType"></param>
        public static InstrumentBase AddInstrument(InstrumentType instrumentType)
        {
            lock (InstrumentManager.ExclusiveLockObject)
            {
                if (instruments[(int)instrumentType].Count < 8)
                {
                    Assembly asm = Assembly.GetAssembly(typeof(InstrumentType));
                    string name = Enum.GetName(typeof(InstrumentType), instrumentType);
                    Type t = asm.GetType("zanac.MAmidiMEmo.Instruments.Chips." + name);

                    var inst = (InstrumentBase)Activator.CreateInstance(t, (uint)instruments[(int)instrumentType].Count);
                    inst.PrepareSound();
                    instruments[(int)instrumentType].Add(inst);
                    InstrumentAdded?.Invoke(typeof(InstrumentManager), EventArgs.Empty);
                    if (VgmRecodring)
                        inst.StartVgmRecordingTo(LastVgmOutputDir);

                    return inst;
                }
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        public static bool VgmRecodring
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public static string LastVgmOutputDir
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="outputDir"></param>
        public static void StartVgmRecordingTo(string outputDir)
        {
            if (!VgmRecodring)
            {
                try
                {
                    Program.SoundUpdating();

                    VgmRecodring = true;
                    LastVgmOutputDir = outputDir;

                    foreach (var inst in InstrumentManager.GetAllInstruments())
                        inst.StartVgmRecordingTo(outputDir);
                }
                finally
                {
                    Program.SoundUpdated();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="outputDir"></param>
        public static void StopVgmRecording()
        {
            if (VgmRecodring)
            {
                try
                {
                    Program.SoundUpdating();

                    VgmRecodring = false;

                    foreach (var inst in InstrumentManager.GetAllInstruments())
                        inst.StopVgmRecording();
                }
                finally
                {
                    Program.SoundUpdated();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instrumentType"></param>
        public static void RemoveInstrument(InstrumentType instrumentType)
        {
            lock (InstrumentManager.ExclusiveLockObject)
            {
                var list = instruments[(int)instrumentType];
                list[list.Count - 1].Dispose();
                list.RemoveAt(list.Count - 1);
                InstrumentRemoved?.Invoke(typeof(InstrumentManager), EventArgs.Empty);
            }
        }

        public static byte[] DataLsb
        {
            get;
        }

        public static byte[] DataMsb
        {
            get;
        }

        public static byte[] NrpnLsb
        {
            get;
        }

        public static byte[] NrpnMsb
        {
            get;
        }

        public static byte[] RpnLsb
        {
            get;
        }

        public static byte[] RpnMsb
        {
            get;
        }

        private static DataEntryType lastDateEntryType;

        /// <summary>
        /// 
        /// </summary>
        private enum DataEntryType
        {
            None,
            Nrpn,
            Rpn
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="midiEvent"></param>
        private static void MidiManager_MidiEventReceived(object sender, MidiEvent e)
        {
            if (e is ActiveSensingEvent)
                return;

            lock (InstrumentManager.ExclusiveLockObject)
            {
                processCC(e);

                foreach (var i in instruments)
                    i.ForEach((dev) => { dev.NotifyMidiEvent(e); });
            }
        }

        private static void processCC(MidiEvent e)
        {
            ControlChangeEvent midiEvent = e as ControlChangeEvent;
            if (midiEvent != null)
            {
                switch (midiEvent.ControlNumber)
                {
                    case 6:    //Data Entry MSB
                        DataMsb[midiEvent.Channel] = midiEvent.ControlValue;

                        switch (lastDateEntryType)
                        {
                            case DataEntryType.Nrpn:
                                processNrpn(midiEvent, null);
                                break;
                            case DataEntryType.Rpn:
                                break;
                        }
                        break;
                    case 38:    //Data Entry LSB
                        DataLsb[midiEvent.Channel] = midiEvent.ControlValue;

                        switch (lastDateEntryType)
                        {
                            case DataEntryType.Nrpn:
                                processNrpn(null, midiEvent);
                                break;
                            case DataEntryType.Rpn:
                                break;
                        }
                        break;

                    case 96:    //Data Increment

                        break;

                    case 97:    //Data Decrement

                        break;

                    case 98:    //NRPN LSB
                        NrpnLsb[midiEvent.Channel] = midiEvent.ControlValue;
                        lastDateEntryType = DataEntryType.Nrpn;
                        break;
                    case 99:    //NRPN MSB
                        NrpnMsb[midiEvent.Channel] = midiEvent.ControlValue;
                        lastDateEntryType = DataEntryType.Nrpn;
                        break;
                    case 100:    //RPN LSB
                        RpnLsb[midiEvent.Channel] = midiEvent.ControlValue;
                        lastDateEntryType = DataEntryType.Rpn;
                        break;
                    case 101:    //RPN MSB
                        RpnMsb[midiEvent.Channel] = midiEvent.ControlValue;
                        lastDateEntryType = DataEntryType.Rpn;
                        break;
                }
            }
        }

        private static void processNrpn(ControlChangeEvent dataMsb, ControlChangeEvent dataLsb)
        {
            if (dataMsb != null)
            {
                switch (NrpnMsb[dataMsb.Channel])
                {
                    case 64:    // Inst Add/Del for Inst
                        {
                            break;
                        }
                    case 65:    // Ch On/Off for Inst (ch 1-7)
                        {
                            foreach (var inst in InstrumentManager.GetAllInstruments())
                            {
                                if (inst.DeviceID == NrpnLsb[dataMsb.Channel])  // for Device ID
                                {
                                    if (inst.UnitNumber == DataLsb[dataMsb.Channel])    // for Unit No
                                    {
                                        for (int i = 0; i < 6; i++)
                                            inst.Channels[i] = (dataMsb.ControlValue & (1 << i)) != 0;
                                        break;
                                    }
                                }
                            }
                            break;
                        }
                    case 66:    // Ch On/Off for Inst (ch 8-14)
                        {
                            foreach (var inst in InstrumentManager.GetAllInstruments())
                            {
                                if (inst.DeviceID == NrpnLsb[dataMsb.Channel])  // for Device ID
                                {
                                    if (inst.UnitNumber == DataLsb[dataMsb.Channel])    // for Unit No
                                    {
                                        for (int i = 0; i < 6; i++)
                                            inst.Channels[i + 6] = (dataMsb.ControlValue & (1 << i)) != 0;
                                        break;
                                    }
                                }
                            }
                            break;
                        }
                    case 67:    // Ch On/Off for Inst (ch 15-16)
                        {
                            foreach (var inst in InstrumentManager.GetAllInstruments())
                            {
                                if (inst.DeviceID == NrpnLsb[dataMsb.Channel])  // for Device ID
                                {
                                    if (inst.UnitNumber == DataLsb[dataMsb.Channel])    // for Unit No
                                    {
                                        for (int i = 0; i < 1; i++)
                                            inst.Channels[i + 15] = (dataMsb.ControlValue & (1 << i)) != 0;
                                        break;
                                    }
                                }
                            }
                            break;
                        }
                }
            }
            if (dataLsb != null)
            {
                switch (NrpnMsb[dataLsb.Channel])
                {
                    case 65:
                        {
                            break;
                        }
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static int[][] GetLastOutputBuffer(InstrumentBase inst)
        {
            int[][] retbuf = new int[2][];
            int cnt = 0;
            lock (InstrumentManager.ExclusiveLockObject)
                cnt = instruments.Count;
            uint did = inst == null ? uint.MaxValue : inst.DeviceID;
            uint un = inst == null ? uint.MaxValue : inst.UnitNumber;
            try
            {
                Program.SoundUpdating();
                copyData(retbuf, "lspeaker", 0, inst, cnt, did, un);
                copyData(retbuf, "rspeaker", 1, inst, cnt, did, un);
            }
            finally
            {
                Program.SoundUpdated();
            }
            return retbuf;
        }

        private static void copyData(int[][] retbuf, string name, int ch, InstrumentBase inst, int cnt, uint did, uint un)
        {
            int num = getLastOutputBufferSamples(name);
            if (num != 0)
            {
                var pbuf = getLastOutputBuffer(name, cnt, did, un);
                if (pbuf != IntPtr.Zero)
                {
                    retbuf[ch] = new int[num];
                    Marshal.Copy(pbuf, retbuf[ch], 0, num);
                }
            }
        }
    }
}
