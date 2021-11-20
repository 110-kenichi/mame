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
using zanac.MAmidiMEmo.Midi;
using zanac.MAmidiMEmo.Gui;
using Melanchall.DryWetMidi.Common;

namespace zanac.MAmidiMEmo.Instruments
{
    public static class InstrumentManager
    {
        /// Exclusive control between GUI/MIDI Event/Other managed threads for Instrument objects;

        /// <summary>
        /// Exclusive control for Instrument objects;
        /// </summary>
        public static ReaderWriterLockSlim InstExclusiveLockObject = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

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

        private static readonly delegate_getLastOutputBuffer getLastOutputBuffer;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int delegate_getLastOutputBufferSamples(string tagName);

        private static readonly delegate_getLastOutputBufferSamples getLastOutputBufferSamples;

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

            Midi.MidiManager.MidiEventReceivedA += MidiManager_MidiEventReceivedA;
            Midi.MidiManager.MidiEventReceivedB += MidiManager_MidiEventReceivedB;

            for (int i = 0; i < Enum.GetNames(typeof(InstrumentType)).Length; i++)
                instruments.Add(new List<InstrumentBase>());

            NrpnLsb = new byte[2, 16];
            NrpnMsb = new byte[2, 16];
            RpnLsb = new byte[2, 16];
            RpnMsb = new byte[2, 16];
            DataLsb = new byte[2, 16];
            DataMsb = new byte[2, 16];

            sysExData.Add(new List<byte>());
            sysExData.Add(new List<byte>());

            Program.ShuttingDown += Program_ShuttingDown;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Program_ShuttingDown(object sender, EventArgs e)
        {
            List<InstrumentBase> insts = new List<InstrumentBase>();
            try
            {
                InstrumentManager.InstExclusiveLockObject.EnterReadLock();

                foreach (List<InstrumentBase> iss in instruments)
                {
                    foreach (var i in iss)
                        i.Dispose();
                }
            }
            finally
            {
                InstrumentManager.InstExclusiveLockObject.ExitReadLock();
            }
            StopVgmRecording();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<InstrumentBase> GetAllInstruments()
        {
            List<InstrumentBase> insts = new List<InstrumentBase>();
            try
            {
                InstrumentManager.InstExclusiveLockObject.EnterReadLock();

                foreach (List<InstrumentBase> i in instruments)
                    insts.AddRange(i);
            }
            finally
            {
                InstrumentManager.InstExclusiveLockObject.ExitReadLock();
            }
            return insts.AsEnumerable();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<InstrumentBase> GetInstruments(uint deviceId)
        {
            try
            {
                InstrumentManager.InstExclusiveLockObject.EnterReadLock();

                return instruments[(int)deviceId - 1];
            }
            finally
            {
                InstrumentManager.InstExclusiveLockObject.ExitReadLock();
            }
        }

        public static InstrumentBase FindParentInstrument(InstrumentType instrumentType, TimbreBase timbre)
        {
            InstrumentBase inst = null;
            foreach (var i in InstrumentManager.GetInstruments((uint)instrumentType + 1))
            {
                Parallel.ForEach(i.BaseTimbres, t =>
                {
                    if (t == timbre)
                        inst = i;
                    if (inst != null)
                        return;
                });
            }
            return inst;
        }

        public static int FindInstrumentIndex(InstrumentBase instrument, TimbreBase timbre)
        {
            var index = -1;
            Parallel.ForEach(instrument.BaseTimbres, (tim, state, idx) =>
            {
                if (tim == timbre)
                {
                    index = (int)idx;
                    return;
                }
            });
            return index;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<InstrumentBase> ClearAllInstruments()
        {
            List<InstrumentBase> insts = new List<InstrumentBase>();

            try
            {
                InstrumentManager.InstExclusiveLockObject.EnterWriteLock();

                for (int i = instruments.Count - 1; i >= 0; i--)
                {
                    for (int j = instruments[i].Count - 1; j >= 0; j--)
                    {
                        instruments[i][j].Dispose();
                        instruments[i].RemoveAt(j);
                    }
                }
                InstrumentRemoved?.Invoke(typeof(InstrumentManager), EventArgs.Empty);
            }
            finally
            {
                InstrumentManager.InstExclusiveLockObject.ExitWriteLock();
            }

            return insts.AsEnumerable();
        }

        /// <summary>
        /// 
        /// </summary>
        public static void RestoreSettings(EnvironmentSettings settings)
        {
            try
            {
                InstrumentManager.InstExclusiveLockObject.EnterWriteLock();

                if (settings.Instruments != null)
                {
                    foreach (int v in Enum.GetValues(typeof(InstrumentType)))
                    {
                        if (v < settings.Instruments.Count && settings.Instruments[v] != null)
                        {
                            try
                            {
                                Program.SoundUpdating();
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
                            finally
                            {
                                Program.SoundUpdated();
                            }
                        }
                    }
                }

                InstrumentChanged?.Invoke(typeof(InstrumentManager), EventArgs.Empty);
            }
            finally
            {
                InstrumentManager.InstExclusiveLockObject.ExitWriteLock();
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
            try
            {
                InstrumentManager.InstExclusiveLockObject.EnterWriteLock();
                Program.SoundUpdating();

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
            finally
            {
                Program.SoundUpdated();
                InstrumentManager.InstExclusiveLockObject.ExitWriteLock();
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
            try
            {
                InstrumentManager.InstExclusiveLockObject.EnterWriteLock();

                var list = instruments[(int)instrumentType];
                list[list.Count - 1].Dispose();
                list.RemoveAt(list.Count - 1);
                InstrumentRemoved?.Invoke(typeof(InstrumentManager), EventArgs.Empty);
            }
            finally
            {
                InstrumentManager.InstExclusiveLockObject.ExitWriteLock();
            }
        }

        private static byte[,] DataLsb
        {
            get;
        }

        private static byte[,] DataMsb
        {
            get;
        }

        private static byte[,] NrpnLsb
        {
            get;
        }

        private static byte[,] NrpnMsb
        {
            get;
        }

        private static byte[,] RpnLsb
        {
            get;
        }

        private static byte[,] RpnMsb
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
        private static void MidiManager_MidiEventReceivedA(object sender, MidiEvent e)
        {
            if (e is ActiveSensingEvent)
                return;

            try
            {
                //InstrumentManager.ExclusiveLockObject.EnterUpgradeableReadLock();

                FormMain.OutputDebugLog(null, "A: " + e.ToString());

                ProcessSysEx(MidiPort.PortA, e);

                //lock (ExclusiveLockObject)
                ProcessCC(MidiPort.PortA, e);

                /* set GM inst name

                Dictionary<int, string> dnames = new Dictionary<int, string>();
                for (int idx = 0; idx < drumNames.Length; idx++)
                {
                    dnames.Add((int)drumNames[idx + 1], (string)drumNames[idx]);
                    idx++;
                }
                */

                foreach (var i in instruments)
                    i.ForEach((dev) =>
                    {
                        if (dev.MidiPort == Midi.MidiPort.PortAB ||
                            dev.MidiPort == Midi.MidiPort.PortA)
                            dev.NotifyMidiEvent(e);

                        /*
                        foreach (var ti in i)
                        {
                            for (int idx = 0; idx < 128; idx++)
                            {
                                ti.BaseTimbres[idx].TimbreName = gsInstName[idx];

                                //ti.BaseTimbres[idx].TimbreName = gsInstName[idx] + " A";
                                //ti.BaseTimbres[idx + 128].TimbreName = gsInstName[idx] + " B";
                            }
                            for (int idx = 0; idx < 128; idx++)
                            {
                                if (ti.DrumTimbres[idx].TimbreNumber != null)
                                {
                                    if (dnames.ContainsKey(idx))
                                        ti.DrumTimbres[idx].TimbreName = dnames[idx];
                                }
                            }
                        }*/
                    });
            }
            finally
            {
                //InstrumentManager.ExclusiveLockObject.ExitUpgradeableReadLock();
            }
        }

        /*
        private static string[] gsInstName = new string[]
        {
            "Piano 1",
            "Piano 2",
            "Piano 3",
            "Honky-tonk",
            "E.Piano 1",
            "E.Piano 2",
            "Harpsichord",
            "Clav.",
            "Celesta",
            "Glockenspiel",
            "Music Box",
            "Vibraphone",
            "Marimba",
            "Xylophone",
            "Tubular-bell",
            "Santur",
            "Organ 1",
            "Organ 2",
            "Organ 3",
            "Church Org.1",
            "Reed Organ",
            "Accordion Fr",
            "Harmonica",
            "Bandneon",
            "Nylon-str.Gt",
            "Steel-str.Gt",
            "Jazz Gt.",
            "Clean Gt.",
            "Muted Gt.",
            "Overdrive Gt",
            "DistortionGt",
            "Gt.Harmonics",
            "Acoustic Bs.",
            "Fingered Bs.",
            "Picked Bs.",
            "Fretless Bs.",
            "Slap Bass 1",
            "Slap Bass 2",
            "Synth Bass 1",
            "Synth Bass 2",
            "Violin",
            "Viola",
            "Cello",
            "Contrabass",
            "Tremolo Str",
            "PizzicatoStr",
            "Harp",
            "Timpani",
            "Strings",
            "Slow Strings",
            "Syn.Strings1",
            "Syn.Strings2",
            "Choir Aahs",
            "Voice Oohs",
            "SynVox",
            "OrchestraHit",
            "Trumpet",
            "Trombone",
            "Tuba",
            "MutedTrumpet",
            "French Horn",
            "Brass 1",
            "Synth Brass1",
            "Synth Brass2",
            "Soprano Sax",
            "Alto Sax",
            "Tenor Sax",
            "Baritone Sax",
            "Oboe",
            "English Horn",
            "Bassoon",
            "Clarinet",
            "Piccolo",
            "Flute",
            "Recorder",
            "Pan Flute",
            "Bottle Blow",
            "Shakuhachi",
            "Whistle",
            "Ocarina",
            "Square Wave",
            "Saw Wave",
            "Syn.Calliope",
            "Chiffer Lead",
            "Charang",
            "Solo Vox",
            "5th Saw Wave",
            "Bass & Lead",
            "Fantasia",
            "Warm Pad",
            "Polysynth",
            "Space Voice",
            "Bowed Glass",
            "Metal Pad",
            "Halo Pad",
            "Sweep Pad",
            "Ice Rain",
            "Soundtrack",
            "Crystal",
            "Atmosphere",
            "Brightness",
            "Goblin",
            "Echo Drops",
            "Star Theme",
            "Sitar",
            "Banjo",
            "Shamisen",
            "Koto",
            "Kalimba",
            "Bag Pipe",
            "Fiddle",
            "Shanai",
            "Tinkle Bell",
            "Agogo",
            "Steel Drums",
            "Woodblock",
            "Taiko",
            "Melo. Tom 1",
            "Synth Drum",
            "Reverse Cym.",
            "Gt.FretNoise",
            "Breath Noise",
            "Seashore",
            "Bird",
            "Telephone 1",
            "Helicopter",
            "Applause",
            "Gun Shot"
        };

        private static object[] drumNames = new object[]
        {
            "Acou BD", 35,
            "Bass Drum", 36,
            "Rim Shot", 37,
            "Acou SD", 38,
            "Hand Clap", 39,
            "Elec SD", 40,
            "AcouLowTom", 41,
            "Clsd HiHat", 42,
            "HighFloorTom", 43,
            "OpenHiHat2", 44,
            "AcouMidTom", 45,
            "OpenHiHat1", 46,
            "LowMidTom", 47,
            "Acou HiTom", 48,
            "Crash Sym", 49,
            "HiTom", 50,
            "Ride sym", 51,
            "Chinese Sym", 52,
            "Ride Bell", 53,
            "Tambourine", 54,
            "Splash Sym", 55,
            "Cowbell", 56,
            "Crash Sym2", 57,
            "Vibraslap", 58,
            "Ride sym2", 59,
            "High Bongo", 60,
            "Low Bongo", 61,
            "Mt HiConga", 62,
            "High Conga", 63,
            "Low Conga", 64,
            "Hi Timbale", 65,
            "LowTimbale", 66,
            "High Agogo", 67,
            "Low Agogo", 68,
            "Cabasa", 69,
            "Maracas", 70,
            "SmbaWhis S", 71,
            "SmbaWhis L", 72,
            "ShortQuijada", 73,
            "LongQuijada", 74,
            "Claves", 75,
            "HWoodBlock", 76,
            "LWoodBlock", 77,
            "Close Cuica", 78,
            "Open Cuica", 79,
        };
        */

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="midiEvent"></param>
        private static void MidiManager_MidiEventReceivedB(object sender, MidiEvent e)
        {
            if (e is ActiveSensingEvent)
                return;

            try
            {
                //InstrumentManager.ExclusiveLockObject.EnterUpgradeableReadLock();

                FormMain.OutputDebugLog(null, "B: " + e.ToString());

                ProcessSysEx(MidiPort.PortB, e);

                //lock (ExclusiveLockObject)
                ProcessCC(MidiPort.PortB, e);

                foreach (var i in instruments)
                    i.ForEach((dev) =>
                    {
                        if (dev.MidiPort == Midi.MidiPort.PortAB ||
                            dev.MidiPort == Midi.MidiPort.PortB)
                            dev.NotifyMidiEvent(e);
                    });
            }
            finally
            {
                //InstrumentManager.ExclusiveLockObject.ExitUpgradeableReadLock();
            }
        }

        private static List<List<byte>> sysExData = new List<List<byte>>();

        private static void ProcessSysEx(MidiPort port, MidiEvent e)
        {
            SysExEvent midiEvent = e as SysExEvent;
            if (midiEvent != null)
            {
                sysExData[(int)port - 1].AddRange(midiEvent.Data);
                if (midiEvent.Completed)
                {
                    List<byte> data = sysExData[(int)port - 1];
                    try
                    {
                        if (data[data.Count - 1] != 0xf7)   //END SysEx
                            return;

                        if (!(data.Count >= 1))
                            return;
                        switch (data[0])
                        {
                            //All device
                            case 0x7f:
                                if (data.Count <= 1)
                                    break;
                                switch (data[1])
                                {
                                    //All device
                                    case 0x7f:
                                        if (data.Count <= 2)
                                            break;
                                        switch (data[2])
                                        {
                                            //COMMON
                                            case 0x04:
                                                {
                                                    if (data.Count <= 3)
                                                        break;
                                                    switch (data[3])
                                                    {
                                                        //MASTER VOLUME
                                                        case 0x01:
                                                            {
                                                                if (data.Count > 6)
                                                                    InstrumentBase.MasterGain = (float)data[5] / 127f;
                                                                break;
                                                            }
                                                    }
                                                    break;
                                                }
                                        }
                                        break;
                                }
                                break;
                            //GM
                            case 0x7e:
                                if (data.Count <= 1)
                                    break;
                                switch (data[1])
                                {
                                    //All device
                                    case 0x7f:
                                        if (data.Count <= 2)
                                            break;
                                        switch (data[2])
                                        {
                                            //
                                            case 0x09:
                                                if (data.Count <= 3)
                                                    break;
                                                switch (data[3])
                                                {
                                                    //GM RESET
                                                    case 0x01:
                                                        {
                                                            Panic();
                                                            Reset();
                                                            break;
                                                        }
                                                }
                                                break;
                                        }
                                        break;
                                }
                                break;
                            //Roland
                            case 0x41:
                                if (data.Count <= 1)
                                    break;
                                switch (data[1])
                                {
                                    //All device
                                    default:
                                        if (data.Count <= 2)
                                            break;
                                        switch (data[2])
                                        {
                                            //
                                            case 0x42:
                                                if (data.Count <= 3)
                                                    break;
                                                switch (data[3])
                                                {
                                                    case 0x12:
                                                        {
                                                            if (data.Count <= 6)
                                                                break;
                                                            //GS RESET
                                                            if (data[4] == 0x40 && data[5] == 0x00 && data[6] == 0x7f)
                                                            {
                                                                Panic();
                                                                Reset();
                                                            }
                                                            break;
                                                        }
                                                }
                                                break;
                                        }
                                        break;
                                }
                                break;
                        }
                    }
                    finally
                    {
                        data.Clear();
                    }
                }
            }
        }

        private static void ProcessCC(MidiPort port, MidiEvent e)
        {
            ControlChangeEvent midiEvent = e as ControlChangeEvent;
            if (midiEvent != null)
            {
                switch (midiEvent.ControlNumber)
                {
                    case 6:    //Data Entry MSB
                        DataMsb[(int)port - 1, midiEvent.Channel] = midiEvent.ControlValue;

                        switch (lastDateEntryType)
                        {
                            case DataEntryType.Nrpn:
                                ProcessNrpn(port, midiEvent, null);
                                break;
                            case DataEntryType.Rpn:
                                break;
                        }
                        break;
                    case 38:    //Data Entry LSB
                        DataLsb[(int)port - 1, midiEvent.Channel] = midiEvent.ControlValue;

                        switch (lastDateEntryType)
                        {
                            case DataEntryType.Nrpn:
                                ProcessNrpn(port, null, midiEvent);
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
                        NrpnLsb[(int)port - 1, midiEvent.Channel] = midiEvent.ControlValue;
                        lastDateEntryType = DataEntryType.Nrpn;
                        break;
                    case 99:    //NRPN MSB
                        NrpnMsb[(int)port - 1, midiEvent.Channel] = midiEvent.ControlValue;
                        lastDateEntryType = DataEntryType.Nrpn;
                        break;
                    case 100:    //RPN LSB
                        RpnLsb[(int)port - 1, midiEvent.Channel] = midiEvent.ControlValue;
                        lastDateEntryType = DataEntryType.Rpn;
                        break;
                    case 101:    //RPN MSB
                        RpnMsb[(int)port - 1, midiEvent.Channel] = midiEvent.ControlValue;
                        lastDateEntryType = DataEntryType.Rpn;
                        break;
                }
            }
        }

        private static void ProcessNrpn(MidiPort port, ControlChangeEvent dataMsb, ControlChangeEvent dataLsb)
        {
            if (dataMsb != null)
            {
                switch (NrpnMsb[(int)port - 1, dataMsb.Channel])
                {
                    case 64:    // Inst Add/Del for Inst
                        {
                            break;
                        }
                    case 65:    // Ch On/Off for Inst (ch 1-7)
                        {
                            foreach (var inst in InstrumentManager.GetAllInstruments())
                            {
                                if (inst.DeviceID == NrpnLsb[(int)port - 1, dataMsb.Channel])  // for Device ID
                                {
                                    if (inst.UnitNumber == DataLsb[(int)port - 1, dataMsb.Channel])    // for Unit No
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
                                if (inst.DeviceID == NrpnLsb[(int)port - 1, dataMsb.Channel])  // for Device ID
                                {
                                    if (inst.UnitNumber == DataLsb[(int)port - 1, dataMsb.Channel])    // for Unit No
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
                                if (inst.DeviceID == NrpnLsb[(int)port - 1, dataMsb.Channel])  // for Device ID
                                {
                                    if (inst.UnitNumber == DataLsb[(int)port - 1, dataMsb.Channel])    // for Unit No
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
                switch (NrpnMsb[(int)port - 1, dataLsb.Channel])
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

            try
            {
                InstrumentManager.InstExclusiveLockObject.EnterReadLock();
                cnt = instruments.Count;
            }
            finally
            {
                InstrumentManager.InstExclusiveLockObject.ExitReadLock();
            }
            uint did = inst == null ? uint.MaxValue : inst.DeviceID;
            uint un = inst == null ? uint.MaxValue : inst.UnitNumber;
            try
            {
                Program.SoundUpdating();
                CopyData(retbuf, "lspeaker", 0, inst, cnt, did, un);
                CopyData(retbuf, "rspeaker", 1, inst, cnt, did, un);
            }
            finally
            {
                Program.SoundUpdated();
            }
            return retbuf;
        }

        private static void CopyData(int[][] retbuf, string name, int ch, InstrumentBase inst, int cnt, uint did, uint un)
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

        /// <summary>
        /// 
        /// </summary>
        public static void Panic()
        {
            for (int i = 0; i < 16; i++)
            {
                //All Note Off
                var me = new ControlChangeEvent((SevenBitNumber)123, (SevenBitNumber)0);
                me.Channel = (FourBitNumber)i;
                MidiManager.SendMidiEvent(MidiPort.PortAB, me);

                //All Sounds Off
                me = new ControlChangeEvent((SevenBitNumber)120, (SevenBitNumber)0);
                me.Channel = (FourBitNumber)i;
                MidiManager.SendMidiEvent(MidiPort.PortAB, me);
            }
            foreach (var inst in InstrumentManager.GetAllInstruments())
                inst.AllSoundOff();
        }


        /// <summary>
        /// 
        /// </summary>
        public static void Reset()
        {
            foreach (var inst in InstrumentManager.GetAllInstruments())
            {
                for (int i = 0; i < 16; i++)
                {
                    inst.Pitchs[i] = 8192;
                    inst.PitchBendRanges[i] = 2;
                    inst.ProgramNumbers[i] = 0;
                    inst.Volumes[i] = 127;
                    inst.Expressions[i] = 127;
                    inst.Panpots[i] = 64;
                    inst.Modulations[i] = 0;
                    inst.ModulationRates[i] = 64;
                    inst.ModulationDepthes[i] = 64;
                    inst.ModulationDelays[i] = 64;
                    inst.ModulationDepthRangesNote[i] = 0;
                    inst.ModulationDepthRangesCent[i] = 64;
                    inst.Holds[i] = 0;
                    inst.Portamentos[i] = 0;
                    inst.PortamentoTimes[i] = 0;
                    inst.MonoMode[i] = 0;
                    inst.PolyMode[i] = 0;
                }
            }
        }
    }
}
