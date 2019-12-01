﻿// copyright-holders:K.Ito
using Melanchall.DryWetMidi.Smf;
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

namespace zanac.MAmidiMEmo.Instruments
{
    public static class InstrumentManager
    {
        /// <summary>
        /// Exclusive control between GUI/MIDI Event/Other managed threads for Instrument objects;
        /// </summary>
        public static object ExclusiveLockObject = new object();

        private static List<List<InstrumentBase>> instruments = new List<List<InstrumentBase>>();

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
        public static void AddInstrument(InstrumentType instrumentType)
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MidiManager_MidiEventReceived(object sender, MidiEvent e)
        {
            lock (InstrumentManager.ExclusiveLockObject)
            {
                foreach (var i in instruments)
                    i.ForEach((dev) => { dev.NotifyMidiEvent(e); });
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
