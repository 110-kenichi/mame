// copyright-holders:K.Ito
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.MusicTheory;
using Newtonsoft.Json;
using Omu.ValueInjecter;
using Omu.ValueInjecter.Injections;
using zanac.MAmidiMEmo.ComponentModel;
using zanac.MAmidiMEmo.Gui;
using zanac.MAmidiMEmo.Instruments.Envelopes;
using zanac.MAmidiMEmo.Mame;
using zanac.MAmidiMEmo.Midi;


namespace zanac.MAmidiMEmo.Instruments.Chips
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class MT32 : InstrumentBase
    {

        public override string Name => "MT32";

        public override string Group => "MIDI";

        public override InstrumentType InstrumentType => InstrumentType.MT32;

        [Browsable(false)]
        public override string ImageKey => "MT32";

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        protected override string SoundInterfaceTagNamePrefix => "mt32_";

        /// <summary>
        /// 
        /// </summary>
        [Category("MIDI")]
        [Description("MIDI Device ID")]
        [IgnoreDataMember]
        [JsonIgnore]
        public override uint DeviceID
        {
            get
            {
                return 20;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        public override TimbreBase[] BaseTimbres
        {
            get
            {
                return Timbres;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        public MT32Timbre[] Timbres
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        public override CombinedTimbre[] CombinedTimbres
        {
            get;
            set;
        }

        [Browsable(false)]
        public override DrumTimbre[] DrumTimbres
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        public override ProgramAssignmentNumber[] ProgramAssignments
        {
            get;
            set;
        }

        [Browsable(false)]
        public override FollowerUnit FollowerMode
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        public override ArpSettings GlobalARP
        {
            get;
            set;
        }

        [Browsable(false)]
        public override ChannelType[] ChannelTypes
        {
            get;
            set;
        }

        [Browsable(false)]
        public override bool[] Channels
        {
            get
            {
                return base.Channels;
            }
        }

        [Browsable(false)]
        public override ushort[] Pitchs
        {
            get
            {
                return base.Pitchs;
            }
        }

        [Browsable(false)]
        public override byte[] PitchBendRanges
        {
            get
            {
                return base.PitchBendRanges;
            }
        }

        [Browsable(false)]
        public override byte[] ProgramNumbers
        {
            get
            {
                return base.ProgramNumbers;
            }
        }

        [Browsable(false)]
        public override byte[] Volumes
        {
            get
            {
                return base.Volumes;
            }
        }

        [Browsable(false)]
        public override byte[] Expressions
        {
            get
            {
                return base.Expressions;
            }
        }


        [Browsable(false)]
        public override byte[] Panpots
        {
            get
            {
                return base.Panpots;
            }
        }


        [Browsable(false)]
        public override byte[] Modulations
        {
            get
            {
                return base.Modulations;
            }
        }

        [Browsable(false)]
        public override byte[] ModulationRates
        {
            get
            {
                return base.ModulationRates;
            }
        }

        [Browsable(false)]
        public override byte[] ModulationDepthes
        {
            get
            {
                return base.ModulationDepthes;
            }
        }


        [Browsable(false)]
        public override byte[] ModulationDelays
        {
            get
            {
                return base.ModulationDelays;
            }
        }


        [Browsable(false)]
        public override byte[] ModulationDepthRangesNote
        {
            get
            {
                return base.ModulationDepthRangesNote;
            }
        }

        [Browsable(false)]
        public override byte[] ModulationDepthRangesCent
        {
            get
            {
                return base.ModulationDepthRangesCent;
            }
        }

        [Browsable(false)]
        public override byte[] Holds
        {
            get
            {
                return base.Holds;
            }
        }

        [Browsable(false)]
        public override byte[] Portamentos
        {
            get
            {
                return base.Portamentos;
            }
        }

        [Browsable(false)]
        public override byte[] PortamentoTimes
        {
            get
            {
                return base.PortamentoTimes;
            }
        }

        [Browsable(false)]
        public override byte[] MonoMode
        {
            get
            {
                return base.MonoMode;
            }
        }

        [Browsable(false)]
        public override byte[] PolyMode
        {
            get
            {
                return base.PolyMode;
            }
        }

        private const float DEFAULT_GAIN = 1.0f;

        public override bool ShouldSerializeGainLeft()
        {
            return GainLeft != DEFAULT_GAIN;
        }

        public override void ResetGainLeft()
        {
            GainLeft = DEFAULT_GAIN;
        }

        public override bool ShouldSerializeGainRight()
        {
            return GainRight != DEFAULT_GAIN;
        }

        public override void ResetGainRight()
        {
            GainRight = DEFAULT_GAIN;
        }

        public override bool ShouldSerializeFilterMode()
        {
            return FilterMode != FilterMode.None;
        }

        public override void ResetFilterMode()
        {
            FilterMode = FilterMode.None;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serializeData"></param>
        public override void RestoreFrom(string serializeData)
        {
            try
            {
                using (var obj = JsonConvert.DeserializeObject<MT32>(serializeData))
                    this.InjectFrom(new LoopInjection(new[] { "SerializeData" }), obj);
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


        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delegate_mt32_play_msg(uint unitNumber, uint msg);

        /// <summary>
        /// 
        /// </summary>
        private static delegate_mt32_play_msg mt32_play_msg
        {
            get;
            set;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delegate_mt32_play_sysex(uint unitNumber, byte[] sysex, int len);

        /// <summary>
        /// 
        /// </summary>
        private static delegate_mt32_play_sysex mt32_play_sysex
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        private static void MT32PlayMsg(uint unitNumber, uint msg)
        {
            mt32_play_msg(unitNumber, msg);
            //DeferredWriteData(mt32_play_msg, unitNumber, msg);
            /*
            try
            {
                Program.SoundUpdating();
                mt32_play_msg(unitNumber, msg);
            }
            finally
            {
                Program.SoundUpdated();
            }*/
        }


        /// <summary>
        /// 
        /// </summary>
        private static void MT32PlaySysEx(uint unitNumber, byte[] sysex)
        {
            mt32_play_sysex(unitNumber, sysex, sysex.Length);
            //DeferredWriteData(mt32_play_sysex, unitNumber, sysex, sysex.Length);
            /*
            try
            {
                Program.SoundUpdating();
                mt32_play_sysex(unitNumber, sysex, sysex.Length);
            }
            finally
            {
                Program.SoundUpdated();
            }*/
        }

        private static FieldInfo channelEventParameters;

        /// <summary>
        /// 
        /// </summary>
        static MT32()
        {
            IntPtr funcPtr = MameIF.GetProcAddress("mt32_play_msg");
            if (funcPtr != IntPtr.Zero)
            {
                mt32_play_msg = (delegate_mt32_play_msg)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_mt32_play_msg));
            }
            funcPtr = MameIF.GetProcAddress("mt32_play_sysex");
            if (funcPtr != IntPtr.Zero)
            {
                mt32_play_sysex = (delegate_mt32_play_sysex)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_mt32_play_sysex));
            }

            channelEventParameters = typeof(ChannelEvent).GetField("_parameters", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
        }

        private MT32SoundManager soundManager;

        /// <summary>
        /// 
        /// </summary>
        public MT32(uint unitNumber) : base(unitNumber)
        {
            GainLeft = DEFAULT_GAIN;
            GainRight = DEFAULT_GAIN;
            FilterMode = FilterMode.None;

            this.soundManager = new MT32SoundManager(this);

            Timbres = new MT32Timbre[InstrumentBase.DEFAULT_MAX_TIMBRES];
            for (int i = 0; i < InstrumentBase.DEFAULT_MAX_TIMBRES; i++)
                Timbres[i] = new MT32Timbre();
        }

        internal override void PrepareSound()
        {
            base.PrepareSound();

            DeferredWriteData(SetOutputGain, UnitNumber, SoundInterfaceTagNamePrefix, 0, GainLeft);
            DeferredWriteData(SetOutputGain, UnitNumber, SoundInterfaceTagNamePrefix, 1, GainRight);
        }

        protected override void OnMidiEvent(MidiEvent midiEvent)
        {
            uint msg = 0;
            switch (midiEvent)
            {
                case SysExEvent sysex:
                    {
                        List<byte> data = new List<byte>();
                        data.Add(0xf0);
                        data.AddRange(sysex.Data);
                        MT32PlaySysEx(UnitNumber, data.ToArray());
                        return;
                    }
                case NoteOffEvent noff:
                    {
                        msg = (uint)((0x80 | noff.Channel) | noff.NoteNumber << 8 | noff.Velocity << 16);
                        break;
                    }
                case TaggedNoteOnEvent non:
                    {
                        msg = (uint)((0x90 | non.Channel) | non.NoteNumber << 8 | non.Velocity << 16);
                        break;
                    }
                case NoteOnEvent non:
                    {
                        msg = (uint)((0x90 | non.Channel) | non.NoteNumber << 8 | non.Velocity << 16);
                        break;
                    }
                case NoteAftertouchEvent na:
                    {
                        msg = (uint)((0xa0 | na.Channel) | na.NoteNumber << 8 | na.AftertouchValue << 16);
                        break;
                    }
                case ControlChangeEvent cc:
                    {
                        msg = (uint)((0xb0 | cc.Channel) | cc.ControlNumber << 8 | cc.ControlValue << 16);
                        break;
                    }
                case ProgramChangeEvent pc:
                    {
                        msg = (uint)((0xc0 | pc.Channel) | pc.ProgramNumber << 8);
                        break;
                    }
                case ChannelAftertouchEvent ca:
                    {
                        msg = (uint)((0xd0 | ca.Channel) | ca.AftertouchValue << 8);
                        break;
                    }
                case PitchBendEvent pb:
                    {
                        msg = (uint)((0xe0 | pb.Channel) | ((pb.PitchValue & 0x7f) << 8) | ((pb.PitchValue >> 7) << 16));
                        break;
                    }
                case TimingClockEvent tc:
                    {
                        msg = (uint)(0xf8);
                        break;
                    }
                case ActiveSensingEvent ase:
                    {
                        msg = (uint)(0xfe);
                        break;
                    }
                case ResetEvent re:
                    {
                        msg = (uint)(0xff);
                        break;
                    }
            }
            MT32PlayMsg(UnitNumber, msg);

            switch (midiEvent)
            {
                case NoteOffEvent noff:
                    {
                        OnNoteOffEvent(noff);
                        break;
                    }
                case NoteOnEvent non:
                    {
                        if (non.Velocity == 0)
                            OnNoteOffEvent(new NoteOffEvent(non.NoteNumber, (SevenBitNumber)0) { Channel = non.Channel, DeltaTime = non.DeltaTime });
                        else
                            OnNoteOnEvent(new TaggedNoteOnEvent(non));
                        break;
                    }
                case TaggedNoteOnEvent non:
                    {
                        if (non.Velocity == 0)
                            OnNoteOffEvent(new NoteOffEvent(non.NoteNumber, (SevenBitNumber)0) { Channel = non.Channel, DeltaTime = non.DeltaTime });
                        else
                            OnNoteOnEvent(non);
                        break;
                    }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected override void OnNoteOnEvent(TaggedNoteOnEvent midiEvent)
        {
            soundManager.ProcessKeyOn(midiEvent);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected override void OnNoteOffEvent(NoteOffEvent midiEvent)
        {
            soundManager.ProcessKeyOff(midiEvent);
        }

        internal override void AllSoundOff()
        {
            soundManager.ProcessAllSoundOff();
        }

        /// <summary>
        /// 
        /// </summary>
        private class MT32SoundManager : SoundManagerBase
        {
            private SoundList<MT32Sound> instOnSounds = new SoundList<MT32Sound>(24);

            private MT32 parentModule;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            public MT32SoundManager(MT32 parent) : base(parent)
            {
                this.parentModule = parent;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="note"></param>
            public override SoundBase[] SoundOn(TaggedNoteOnEvent note)
            {
                List<SoundBase> rv = new List<SoundBase>();

                foreach (MT32Timbre timbre in parentModule.GetBaseTimbres(note))
                {
                    int emptySlot = searchEmptySlot(note, timbre);
                    if (emptySlot < 0)
                        continue;

                    MT32Sound snd = new MT32Sound(parentModule, this, timbre, note, emptySlot);
                    instOnSounds.Add(snd);

                    FormMain.OutputDebugLog(parentModule, "KeyOn ch" + emptySlot + " " + note.ToString());
                    rv.Add(snd);
                }
                for (int i = 0; i < rv.Count; i++)
                {
                    var snd = rv[i];
                    if (!snd.IsDisposed)
                    {
                        snd.KeyOn();
                    }
                    else
                    {
                        rv.Remove(snd);
                        i--;
                    }
                }

                return rv.ToArray();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            private int searchEmptySlot(TaggedNoteOnEvent note, MT32Timbre timbre)
            {
                int emptySlot = -1;

                emptySlot = SearchEmptySlotAndOff(parentModule, instOnSounds, note, 32);
                return emptySlot;
            }

            internal override void ProcessAllSoundOff()
            {
                var me = new ControlChangeEvent((SevenBitNumber)120, (SevenBitNumber)0);
                ProcessControlChange(me);

            }
        }


        /// <summary>
        /// 
        /// </summary>
        private class MT32Sound : SoundBase
        {
            private MT32 parentModule;

            private SevenBitNumber programNumber;

            private MT32Timbre timbre;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentModule"></param>
            /// <param name="noteOnEvent"></param>
            /// <param name="programNumber"></param>
            /// <param name="slot"></param>
            public MT32Sound(MT32 parentModule, MT32SoundManager manager, TimbreBase timbre, TaggedNoteOnEvent noteOnEvent, int slot) : base(parentModule, manager, timbre, noteOnEvent, slot)
            {
                this.parentModule = parentModule;
                this.programNumber = (SevenBitNumber)parentModule.ProgramNumbers[noteOnEvent.Channel];
                this.timbre = (MT32Timbre)timbre;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [JsonConverter(typeof(NoTypeConverterJsonConverter<MT32Timbre>))]
        [DataContract]
        public class MT32Timbre : TimbreBase
        {
            /// <summary>
            /// 
            /// </summary>
            public MT32Timbre()
            {
                this.SDS.FxS = new BasicFxSettings();
            }

            public override void RestoreFrom(string serializeData)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<MT32Timbre>(serializeData);
                    this.InjectFrom(new LoopInjection(new[] { "SerializeData" }), obj);
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
}