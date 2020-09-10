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

//http://d4.princess.ne.jp/msx/datas/OPLL/YM2413AP.html#31
//http://www.smspower.org/maxim/Documents/YM2413ApplicationManual
//http://hp.vector.co.jp/authors/VA054130/yamaha_curse.html

namespace zanac.MAmidiMEmo.Instruments.Chips
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class YM2413 : InstrumentBase
    {

        public override string Name => "YM2413";

        public override string Group => "FM";

        public override InstrumentType InstrumentType => InstrumentType.YM2413;

        [Browsable(false)]
        public override string ImageKey => "YM2413";

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        protected override string SoundInterfaceTagNamePrefix => "ym2413_";

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
                return 9;
            }
        }

        private byte f_RHY;

        /// <summary>
        /// Rhythm mode
        /// </summary>
        [DataMember]
        [Category("Chip")]
        [Description("Rhythm mode (0:Off(9ch) 1:On(6ch))\r\n" +
            "Set DrumSet to ToneType in Timbre to output drum sound")]
        [SlideParametersAttribute(0, 1)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((byte)0)]
        public byte RHY
        {
            get
            {
                return f_RHY;
            }
            set
            {
                var v = (byte)(value & 1);
                if (f_RHY != v)
                {
                    f_RHY = v;

                    OnControlChangeEvent(new ControlChangeEvent((SevenBitNumber)120, (SevenBitNumber)0));

                    YM2413WriteData(UnitNumber, 0x0e, 0, (byte)(RHY << 5));
                    if (RHY == 1)
                    {
                        YM2413WriteData(UnitNumber, (byte)(0x16), 0, 0x20);
                        YM2413WriteData(UnitNumber, (byte)(0x17), 0, 0x50);
                        YM2413WriteData(UnitNumber, (byte)(0x18), 0, 0xc0);
                        YM2413WriteData(UnitNumber, (byte)(0x26), 0, 0x05);
                        YM2413WriteData(UnitNumber, (byte)(0x27), 0, 0x05);
                        YM2413WriteData(UnitNumber, (byte)(0x28), 0, 0x01);
                    }
                }
            }
        }

        /// <summary>
        /// FrequencyCalculationMode
        /// </summary>
        [DataMember]
        [Category("Chip")]
        [Description("Select Frequency Accuracy Mode (False:3.6MHz mode(Not accurate) True:3.579545MHz mode(Accurate)")]
        [DefaultValue(false)]
        public bool FrequencyAccuracyMode
        {
            get;
            set;
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

        [DataMember]
        [Category("Chip")]
        [Description("Timbres (0-127)")]
        [EditorAttribute(typeof(DummyEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(ExpandableCollectionConverter))]
        public YM2413Timbre[] Timbres
        {
            get;
            set;
        }

        private const float DEFAULT_GAIN = 4.0f;

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

        private byte lastDrumKeyOn;
        private byte lastDrumVolume37;
        private byte lastDrumVolume38;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serializeData"></param>
        public override void RestoreFrom(string serializeData)
        {
            try
            {
                using (var obj = JsonConvert.DeserializeObject<YM2413>(serializeData))
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delegate_YM2413_write(uint unitNumber, uint address, byte data);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate byte delegate_YM2413_read(uint unitNumber, uint address);

        /// <summary>
        /// 
        /// </summary>
        private static delegate_YM2413_write YM2413_write
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        private static delegate_YM2413_read YM2413_read
        {
            get;
            set;
        }

        private static byte[] addressTable = new byte[] { 0x00, 0x01, 0x02, 0x08, 0x09, 0x0a, 0x10, 0x11, 0x12 };

        /// <summary>
        /// 
        /// </summary>
        private static void YM2413WriteData(uint unitNumber, byte address, int slot, byte data)
        {
            DeferredWriteData(YM2413_write, unitNumber, (uint)0, (byte)(address + slot));
            DeferredWriteData(YM2413_write, unitNumber, (uint)1, data);
            /*
            try
            {
                Program.SoundUpdating();
                YM2413_write(unitNumber, 0, (byte)(address + slot));
                YM2413_write(unitNumber, 1, data);
            }
            finally
            {
                Program.SoundUpdated();
            }*/
        }


        /// 
        /// </summary>
        static YM2413()
        {
            IntPtr funcPtr = MameIF.GetProcAddress("ym2413_write");
            if (funcPtr != IntPtr.Zero)
                YM2413_write = (delegate_YM2413_write)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_YM2413_write));
            /*
            funcPtr = MameIF.GetProcAddress("ym2413_read");
            if (funcPtr != IntPtr.Zero)
                YM2413_read = (delegate_YM2413_read)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_YM2413_read));*/
        }

        private YM2413SoundManager soundManager;

        /// <summary>
        /// 
        /// </summary>
        public YM2413(uint unitNumber) : base(unitNumber)
        {
            GainLeft = DEFAULT_GAIN;
            GainRight = DEFAULT_GAIN;

            Timbres = new YM2413Timbre[InstrumentBase.MAX_TIMBRES];
            for (int i = 0; i < InstrumentBase.MAX_TIMBRES; i++)
                Timbres[i] = new YM2413Timbre();
            setPresetInstruments();

            this.soundManager = new YM2413SoundManager(this);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Dispose()
        {
            soundManager?.Dispose();
            base.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        private void setPresetInstruments()
        {
            Timbres[0].Career.AR = 15;
            Timbres[0].Career.DIST = 1;
            Timbres[0].Modulator.AR = 14;
            Timbres[0].Modulator.VIB = 1;
        }

        internal override void PrepareSound()
        {
            base.PrepareSound();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected override void OnNoteOnEvent(TaggedNoteOnEvent midiEvent)
        {
            soundManager.KeyOn(midiEvent);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected override void OnNoteOffEvent(NoteOffEvent midiEvent)
        {
            soundManager.KeyOff(midiEvent);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected override void OnControlChangeEvent(ControlChangeEvent midiEvent)
        {
            base.OnControlChangeEvent(midiEvent);

            soundManager.ControlChange(midiEvent);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected override void OnPitchBendEvent(PitchBendEvent midiEvent)
        {
            base.OnPitchBendEvent(midiEvent);

            soundManager.PitchBend(midiEvent);
        }

        internal override void AllSoundOff()
        {
            soundManager.AllSoundOff();
        }

        /// <summary>
        /// 
        /// </summary>
        private class YM2413SoundManager : SoundManagerBase
        {
            private static SoundList<SoundBase> allSound = new SoundList<SoundBase>(-1);

            /// <summary>
            /// 
            /// </summary>
            protected override SoundList<SoundBase> AllSounds
            {
                get
                {
                    return allSound;
                }
            }

            private static SoundList<YM2413Sound> fmOnSounds = new SoundList<YM2413Sound>(9);

            private static SoundList<YM2413Sound> drumOnSounds = new SoundList<YM2413Sound>(6);

            private YM2413 parentModule;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            public YM2413SoundManager(YM2413 parent) : base(parent)
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

                foreach (YM2413Timbre timbre in parentModule.GetBaseTimbres(note))
                {
                    var emptySlot = searchEmptySlot(note, timbre);
                    if (emptySlot.slot < 0)
                        continue;

                    YM2413Sound snd = new YM2413Sound(emptySlot.inst, this, timbre, note, emptySlot.slot);
                    if (parentModule.RHY == 0)
                    {
                        fmOnSounds.Add(snd);
                    }
                    else
                    {
                        if (timbre.ToneType != ToneType.DrumSet)
                            fmOnSounds.Add(snd);
                    }
                    FormMain.OutputDebugLog("KeyOn FM ch" + emptySlot + " " + note.ToString());
                    snd.KeyOn();
                    rv.Add(snd);
                }

                return rv.ToArray();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            private (YM2413 inst, int slot) searchEmptySlot(TaggedNoteOnEvent note, YM2413Timbre timbre)
            {
                var emptySlot = (parentModule, -1);

                if (parentModule.RHY == 0)
                {
                    emptySlot = SearchEmptySlotAndOffForLeader(parentModule, fmOnSounds, note, 9);
                }
                else
                {
                    if (timbre.ToneType != ToneType.DrumSet)
                        emptySlot = SearchEmptySlotAndOffForLeader(parentModule, fmOnSounds, note, 6);
                    else
                        emptySlot = SearchEmptySlotAndOffForLeader(parentModule, drumOnSounds, note, 6);
                }

                return emptySlot;
            }

            internal override void AllSoundOff()
            {
                var me = new ControlChangeEvent((SevenBitNumber)120, (SevenBitNumber)0);
                ControlChange(me);

                for (int i = 0; i < 9; i++)
                    YM2413WriteData(parentModule.UnitNumber, (byte)(0x20 + i), 0, (byte)(0));
                if (parentModule.RHY != 0)
                    YM2413WriteData(parentModule.UnitNumber, 0xe, 0, (byte)(0x20));
            }

        }


        /// <summary>
        /// 
        /// </summary>
        private class YM2413Sound : SoundBase
        {
            private YM2413 parentModule;

            private YM2413Timbre timbre;

            private byte lastFreqData;

            private ToneType lastToneType;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentModule"></param>
            /// <param name="noteOnEvent"></param>
            /// <param name="programNumber"></param>
            /// <param name="slot"></param>
            public YM2413Sound(YM2413 parentModule, YM2413SoundManager manager, TimbreBase timbre, TaggedNoteOnEvent noteOnEvent, int slot) : base(parentModule, manager, timbre, noteOnEvent, slot)
            {
                this.parentModule = parentModule;
                this.timbre = (YM2413Timbre)timbre;
                lastToneType = this.timbre.ToneType;
            }

            /// <summary>
            /// 
            /// </summary>
            public override void KeyOn()
            {
                base.KeyOn();

                SetTimbre();
                //Volume
                OnVolumeUpdated();
                //Freq & kon
                OnPitchUpdated();
            }

            public override void OnSoundParamsUpdated()
            {
                base.OnSoundParamsUpdated();

                SetTimbre();
                //Volume
                OnVolumeUpdated();
            }

            /// <summary>
            /// 
            /// </summary>
            public override void OnVolumeUpdated()
            {
                if (IsSoundOff)
                    return;

                byte tl = (byte)(15 - (byte)Math.Round(15 * CalcCurrentVolume()));
                if (lastToneType != ToneType.DrumSet)
                {
                    var tt = timbre.ToneType;
                    if (FxEngine != null && FxEngine.Active)
                    {
                        var eng = FxEngine as YM2413FxEngine;
                        if (eng?.ToneValue != null)
                            tt = (ToneType)(eng.ToneValue.Value & 15);
                    }
                    YM2413WriteData(parentModule.UnitNumber, 0x30, Slot, (byte)((int)tt << 4 | tl));
                }
                else if (parentModule.RHY == 1)
                {
                    switch (NoteOnEvent.GetNoteName())
                    {
                        case NoteName.C:    //BD
                            YM2413WriteData(parentModule.UnitNumber, 0x36, 0, tl);
                            break;
                        case NoteName.D:    //SD
                            parentModule.lastDrumVolume37 = (byte)(tl | (parentModule.lastDrumVolume37 & 0xf0));
                            YM2413WriteData(parentModule.UnitNumber, 0x37, 0, parentModule.lastDrumVolume37);
                            break;
                        case NoteName.F:    //TOM
                            parentModule.lastDrumVolume38 = (byte)(tl << 4 | (parentModule.lastDrumVolume38 & 0x0f));
                            YM2413WriteData(parentModule.UnitNumber, 0x38, 0, parentModule.lastDrumVolume38);
                            break;
                        case NoteName.FSharp:    //HH
                            parentModule.lastDrumVolume37 = (byte)(tl << 4 | (parentModule.lastDrumVolume37 & 0x0f));
                            YM2413WriteData(parentModule.UnitNumber, 0x37, 0, parentModule.lastDrumVolume37);
                            break;
                        case NoteName.ASharp:    //Symbal
                            parentModule.lastDrumVolume38 = (byte)(tl | (parentModule.lastDrumVolume38 & 0xf0));
                            YM2413WriteData(parentModule.UnitNumber, 0x38, 0, parentModule.lastDrumVolume38);
                            break;
                    }
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="slot"></param>
            public override void OnPitchUpdated()
            {
                if (lastToneType != ToneType.DrumSet)
                {
                    double d = CalcCurrentPitch();

                    int nn = NoteOnEvent.NoteNumber;
                    if (ParentModule.ChannelTypes[NoteOnEvent.Channel] == ChannelType.Drum)
                        nn = (int)ParentModule.DrumTimbres[NoteOnEvent.NoteNumber].BaseNote;
                    int noteNum = nn + (int)d;
                    if (noteNum > 127)
                        noteNum = 127;
                    else if (noteNum < 0)
                        noteNum = 0;
                    var nnOn = new TaggedNoteOnEvent((SevenBitNumber)noteNum, (SevenBitNumber)127);
                    ushort freq = convertFmFrequency(nnOn);
                    var oct = nnOn.GetNoteOctave();
                    if (oct < 0)
                        oct = 0;
                    byte octave = (byte)(oct << 1);

                    if (d != 0)
                        freq += (ushort)(((double)(convertFmFrequency(nnOn, (d < 0) ? false : true) - freq)) * Math.Abs(d - Math.Truncate(d)));

                    //keyon
                    byte kon = IsKeyOff ? (byte)0 : (byte)0x10;
                    lastFreqData = (byte)(timbre.SUS << 5 | kon | octave | ((freq >> 8) & 1));

                    YM2413WriteData(parentModule.UnitNumber, (byte)(0x10 + Slot), 0, (byte)(0xff & freq));
                    YM2413WriteData(parentModule.UnitNumber, (byte)(0x20 + Slot), 0, lastFreqData);
                }
                else if (parentModule.RHY == 1 && !IsKeyOff)
                {
                    byte kon = 0;
                    switch (NoteOnEvent.GetNoteName())
                    {
                        case NoteName.C:    //BD
                            kon = 0x10;
                            break;
                        case NoteName.D:    //SD
                            kon = 0x08;
                            break;
                        case NoteName.F:    //TOM
                            kon = 0x04;
                            break;
                        case NoteName.FSharp:    //HH
                            kon = 0x01;
                            break;
                        case NoteName.ASharp:    //Symbal
                            kon = 0x02;
                            break;
                    }
                    if (kon != 0)
                    {
                        parentModule.lastDrumKeyOn |= (byte)kon;
                        YM2413WriteData(parentModule.UnitNumber, 0xe, 0, (byte)(0x20 | parentModule.lastDrumKeyOn));  //on
                    }
                }

                base.OnPitchUpdated();
            }

            /// <summary>
            /// 
            /// </summary>
            public void SetTimbre()
            {
                if (lastToneType != ToneType.Custom)
                    return;

                YM2413Modulator m = timbre.Modulator;
                YM2413Career c = timbre.Career;

                //$00+:
                YM2413WriteData(parentModule.UnitNumber, 0x00, 0, (byte)((m.AM << 7 | m.VIB << 6 | m.EG << 5 | m.KSR << 4 | m.MUL)));
                YM2413WriteData(parentModule.UnitNumber, 0x01, 0, (byte)((c.AM << 7 | c.VIB << 6 | c.EG << 5 | c.KSR << 4 | c.MUL)));
                //$02+:
                //YM2413WriteData(parentModule.UnitNumber, 0x02, 0, (byte)((m.KSL << 6 | m.TL)));
                YM2413WriteData(parentModule.UnitNumber, 0x03, 0, (byte)((c.KSL << 6 | c.DIST << 4 | m.DIST << 3 | timbre.FB)));
                //$04+:
                YM2413WriteData(parentModule.UnitNumber, 0x04, 0, (byte)((m.AR << 4 | m.DR)));
                YM2413WriteData(parentModule.UnitNumber, 0x05, 0, (byte)((c.AR << 4 | c.DR)));
                //$06+:
                if (m.SR.HasValue && m.EG == 0)
                    YM2413WriteData(parentModule.UnitNumber, 0x06, 0, (byte)((m.SL << 4 | m.SR.Value)));
                else
                    YM2413WriteData(parentModule.UnitNumber, 0x06, 0, (byte)((m.SL << 4 | m.RR)));

                if (c.SR.HasValue && c.EG == 0)
                    YM2413WriteData(parentModule.UnitNumber, 0x07, 0, (byte)((c.SL << 4 | c.SR.Value)));
                else
                    YM2413WriteData(parentModule.UnitNumber, 0x07, 0, (byte)((c.SL << 4 | c.RR)));
            }

            /// <summary>
            /// 
            /// </summary>
            public override void SoundOff()
            {
                base.SoundOff();

                if (lastToneType != ToneType.DrumSet)
                {
                    YM2413Modulator m = timbre.Modulator;
                    YM2413Career c = timbre.Career;
                    if (m.SR.HasValue && m.EG == 0)
                    {
                        YM2413WriteData(parentModule.UnitNumber, 0x00, 0, (byte)((m.AM << 7 | m.VIB << 6 | 1 << 5 | m.KSR << 4 | m.MUL)));
                        YM2413WriteData(parentModule.UnitNumber, 0x06, 0, (byte)((m.SL << 4 | m.RR)));
                    }
                    if (c.SR.HasValue && c.EG == 0)
                    {
                        YM2413WriteData(parentModule.UnitNumber, 0x01, 0, (byte)((c.AM << 7 | c.VIB << 6 | 1 << 5 | c.KSR << 4 | c.MUL)));
                        YM2413WriteData(parentModule.UnitNumber, 0x07, 0, (byte)((c.SL << 4 | c.RR)));
                    }
                    YM2413WriteData(parentModule.UnitNumber, (byte)(0x20 + Slot), 0, (byte)(timbre.SUS << 5 | lastFreqData & 0x0f));
                }
                else if (parentModule.RHY == 1)
                {
                    byte kon = 0;
                    switch (NoteOnEvent.GetNoteName())
                    {
                        case NoteName.C:    //BD
                            kon = 0x10;
                            break;
                        case NoteName.D:    //SD
                            kon = 0x08;
                            break;
                        case NoteName.F:    //TOM
                            kon = 0x04;
                            break;
                        case NoteName.FSharp:    //HH
                            kon = 0x01;
                            break;
                        case NoteName.ASharp:    //Symbal
                            kon = 0x02;
                            break;
                    }
                    if (kon != 0)
                    {
                        parentModule.lastDrumKeyOn = (byte)(parentModule.lastDrumKeyOn & ~kon);
                        YM2413WriteData(parentModule.UnitNumber, 0xe, 0, (byte)(0x20 | parentModule.lastDrumKeyOn));  //off
                    }
                }
            }


            private ushort[] freqTable1 = new ushort[] {
                326/2,
                173,
                183,
                194,
                205,
                217,
                230,
                244,
                258,
                274,
                290,
                307,
                326,
                173*2,
            };

            private ushort[] freqTable2 = new ushort[] {
                323/2,
                172,
                181,
                192,
                204,
                216,
                229,
                242,
                257,
                272,
                288,
                305,
                323,
                172*2,
            };

            /// <summary>
            /// 
            /// </summary>
            /// <param name="note"></param>
            /// <param name="freq"></param>
            /// <returns></returns>
            private ushort convertFmFrequency(TaggedNoteOnEvent note)
            {
                if (parentModule.FrequencyAccuracyMode)
                    return freqTable1[(int)note.GetNoteName() + 1];
                else
                    return freqTable2[(int)note.GetNoteName() + 1];
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="note"></param>
            /// <param name="freq"></param>
            /// <returns></returns>
            private ushort convertFmFrequency(TaggedNoteOnEvent note, bool plus)
            {
                if (plus)
                {
                    if (parentModule.FrequencyAccuracyMode)
                        return freqTable1[(int)note.GetNoteName() + 2];
                    else
                        return freqTable2[(int)note.GetNoteName() + 2];
                }
                else
                {
                    if (parentModule.FrequencyAccuracyMode)
                        return freqTable1[(int)note.GetNoteName()];
                    else
                        return freqTable2[(int)note.GetNoteName()];
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [JsonConverter(typeof(NoTypeConverterJsonConverter<YM2413Timbre>))]
        [DataContract]
        public class YM2413Timbre : TimbreBase
        {
            #region FM Symth

            private ToneType f_ToneType;

            [DataMember]
            [Category("Sound")]
            [Description("Tone type")]
            [DefaultValue(ToneType.Custom)]
            public ToneType ToneType
            {
                get
                {
                    return f_ToneType;
                }
                set
                {
                    f_ToneType = value;
                }
            }

            private byte f_FB;

            [DataMember]
            [Category("Sound")]
            [Description("Feedback (0-7)")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 7)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte FB
            {
                get
                {
                    return f_FB;
                }
                set
                {
                    f_FB = (byte)(value & 7);
                }
            }

            private byte f_SUS;

            /// <summary>
            /// Sustain Mode (0:Disalbe 1:Enable)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Sustain Mode (0:Disalbe 1:Enable)")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte SUS
            {
                get
                {
                    return f_SUS;
                }
                set
                {
                    f_SUS = (byte)(value & 1);
                }
            }

            #endregion


            /// <summary>
            /// 
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Operators")]
            public YM2413Modulator Modulator
            {
                get;
                set;
            }


            /// <summary>
            /// 
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Operators")]
            public YM2413Career Career
            {
                get;
                set;
            }

            [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                typeof(UITypeEditor)), Localizable(false)]
            [IgnoreDataMember]
            [JsonIgnore]
            [Description("You can copy and paste this text data to other same type timber.\r\n" +
                "FB, AR, DR, RR, SL, SR, TL, KSL, KSR, MUL, AM(AMS), VIB, EG, DIST, AR, DR, RR, SL, SR, KSL, KSR, MUL, AM(AMS), VIB, EG, DIST, SUS")]
            public string MmlSerializeData
            {
                get
                {
                    return SimpleSerializer.SerializeProps(this,
                        "FB",

                        "Modulator.AR",
                        "Modulator.DR",
                        "Modulator.RR",
                        "Modulator.SL",
                        "Modulator.SR",
                        "Modulator.TL",
                        "Modulator.KSL",
                        "Modulator.KSR",
                        "Modulator.MUL",
                        "Modulator.AM",
                        "Modulator.VIB",
                        "Modulator.EG",
                        "Modulator.DIST",

                        "Career.AR",
                        "Career.DR",
                        "Career.RR",
                        "Career.SL",
                        "Career.SR",
                        "Career.KSL",
                        "Career.KSR",
                        "Career.MUL",
                        "Career.AM",
                        "Career.VIB",
                        "Career.EG",
                        "Career.DIST",

                        "SUS");
                }
                set
                {
                    SimpleSerializer.DeserializeProps(this, value,
                        "FB",

                        "Modulator.AR",
                        "Modulator.DR",
                        "Modulator.RR",
                        "Modulator.SL",
                        "Modulator.SR",
                        "Modulator.TL",
                        "Modulator.KSL",
                        "Modulator.KSR",
                        "Modulator.MUL",
                        "Modulator.AM",
                        "Modulator.VIB",
                        "Modulator.EG",
                        "Modulator.DIST",

                        "Career.AR",
                        "Career.DR",
                        "Career.RR",
                        "Career.SL",
                        "Career.SR",
                        "Career.KSL",
                        "Career.KSR",
                        "Career.MUL",
                        "Career.AM",
                        "Career.VIB",
                        "Career.EG",
                        "Career.DIST",

                        "SUS");
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public YM2413Timbre()
            {
                Modulator = new YM2413Modulator();
                Career = new YM2413Career();
                this.SDS.FxS = new YM2413FxSettings();
            }

            public override void RestoreFrom(string serializeData)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<YM2413Timbre>(serializeData);
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

        /// <summary>
        /// 
        /// </summary>
        public enum ToneType
        {
            Custom,
            Violin,
            Guitor,
            Piano,
            Flute,
            Clarinet,
            Oboe,
            Trumpet,
            Organ,
            Horn,
            Symthesizer,
            Harpsichord,
            Vibraphone,
            SynthesizerBass,
            AcousticBass,
            ElectricGuitar,
            DrumSet,
        }

        /// <summary>
        /// 
        /// </summary>
        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [JsonConverter(typeof(NoTypeConverterJsonConverter<YM2413Operator>))]
        [DataContract]
        [MidiHook]
        public class YM2413Operator : ContextBoundObject
        {

            private byte f_AR;

            /// <summary>
            /// Attack Rate (0-15)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Attack Rate (0-15)")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 15)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte AR
            {
                get
                {
                    return f_AR;
                }
                set
                {
                    f_AR = (byte)(value & 15);
                }
            }

            private byte f_DR;

            /// <summary>
            /// Decay Rate (0-15)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Decay Rate (0-15)")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 15)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte DR
            {
                get
                {
                    return f_DR;
                }
                set
                {
                    f_DR = (byte)(value & 15);
                }
            }

            private byte f_RR;

            /// <summary>
            /// release rate(0-15)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Release Rate (0-15)\r\n" +
                "When EG = 0, Used both Sustain Rate & Release Rate")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 15)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte RR
            {
                get
                {
                    return f_RR;
                }
                set
                {
                    f_RR = (byte)(value & 15);
                }
            }

            private byte f_SL;

            /// <summary>
            /// Sustain Level (0-15)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Sustain Level (0-15)")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 15)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte SL
            {
                get
                {
                    return f_SL;
                }
                set
                {
                    f_SL = (byte)(value & 15);
                }
            }

            private byte? f_SR;

            /// <summary>
            /// Sustain rate(0-15)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("When EG = 0 and value is set, Used as Sustain Rate (0-15) when KOFF")]
            [DefaultValue(null)]
            [SlideParametersAttribute(0, 15)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte? SR
            {
                get
                {
                    return f_SR;
                }
                set
                {
                    if (value.HasValue)
                        f_SR = (byte)(value & 15);
                    else
                        f_SR = value;
                }
            }

            private byte f_KSL;

            /// <summary>
            /// Key Scaling Level(0-3)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Key Scaling Level (00:No Change 10:1.5dB/8ve 01:3dB/8ve 11:6dB/8ve)")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 3)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte KSL
            {
                get
                {
                    return f_KSL;
                }
                set
                {
                    f_KSL = (byte)(value & 3);
                }
            }

            private byte f_KSR;

            /// <summary>
            /// Keyboard scaling rate (0-1)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Keyboard Scaling Rate (1: the sound's envelope is foreshortened as it rises in pitch.")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte KSR
            {
                get
                {
                    return f_KSR;
                }
                set
                {
                    f_KSR = (byte)(value & 1);
                }
            }

            private byte f_MUL;

            /// <summary>
            /// Multiply (0-15)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Multiply (0-15)")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 15)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte MUL
            {
                get
                {
                    return f_MUL;
                }
                set
                {
                    f_MUL = (byte)(value & 15);
                }
            }

            private byte f_AM;

            /// <summary>
            /// Apply amplitude modulation(0-1)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Apply amplitude modulation (0:Off 1:On)")]
            [SlideParametersAttribute(0, 1)]
            [DefaultValue((byte)0)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte AM
            {
                get
                {
                    return f_AM;
                }
                set
                {
                    f_AM = (byte)(value & 1);
                }
            }

            private byte f_VIB;

            /// <summary>
            /// Apply vibrato(0-1)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Vibrato (0:Off 1:On)")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte VIB
            {
                get
                {
                    return f_VIB;
                }
                set
                {
                    f_VIB = (byte)(value & 1);
                }
            }

            private byte f_EG;

            /// <summary>
            /// EG Type (0-1)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("EG Type (0:the sound begins to decay immediately after hitting the SUSTAIN phase 1:the sustain level of the voice is maintained until released")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte EG
            {
                get
                {
                    return f_EG;
                }
                set
                {
                    f_EG = (byte)(value & 1);
                }
            }

            private byte f_DIST;

            /// <summary>
            /// Distortion (0:Off 1:On)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Distortion (0:Off 1:On)")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte DIST
            {
                get
                {
                    return f_DIST;
                }
                set
                {
                    f_DIST = (byte)(value & 1);
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        [JsonConverter(typeof(NoTypeConverterJsonConverter<YM2413Modulator>))]
        [DataContract]
        public class YM2413Modulator : YM2413Operator
        {

            private byte f_TL;

            /// <summary>
            /// Total Level (0-63)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Total Level (0-63)")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 63)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte TL
            {
                get
                {
                    return f_TL;
                }
                set
                {
                    f_TL = (byte)(value & 63);
                }
            }


            #region Etc

            [DataMember]
            [Description("Memo")]
            public string Memo
            {
                get;
                set;
            }

            [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                typeof(UITypeEditor)), Localizable(false)]
            [IgnoreDataMember]
            [JsonIgnore]
            [Description("You can copy and paste this text data to other same type timber.\r\n" +
                "AR, DR, RR, SL, SR, TL, KSL, KSR, MUL, AM(AMS), VIB, EG, DIST\r\n" +
                "You can use comma or space chars as delimiter.")]
            public string MmlSerializeData
            {
                get
                {
                    return SimpleSerializer.SerializeProps(this,
                        nameof(AR),
                        nameof(DR),
                        nameof(RR),
                        nameof(SL),
                        nameof(SR),
                        nameof(TL),
                        nameof(KSL),
                        nameof(KSR),
                        nameof(MUL),
                        nameof(AM),
                        nameof(VIB),
                        nameof(EG),
                        nameof(DIST));
                }
                set
                {
                    SimpleSerializer.DeserializeProps(this, value,
                        nameof(AR),
                        nameof(DR),
                        nameof(RR),
                        nameof(SL),
                        nameof(SR),
                        nameof(TL),
                        nameof(KSL),
                        nameof(KSR),
                        nameof(MUL),
                        nameof(AM),
                        nameof(VIB),
                        nameof(EG),
                        nameof(DIST));
                }
            }

            [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                typeof(UITypeEditor)), Localizable(false)]
            [IgnoreDataMember]
            [JsonIgnore]
            [Description("You can copy and paste this text data to other same type timber.\r\nNote: Open dropdown editor then copy all text and paste to dropdown editor. Do not copy and paste one liner text.")]
            public string SerializeData
            {
                get
                {
                    return JsonConvert.SerializeObject(this, Formatting.Indented);
                }
                set
                {
                    RestoreFrom(value);
                }
            }

            public void RestoreFrom(string serializeData)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<YM2413Modulator>(serializeData);
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

            #endregion

        }

        /// <summary>
        /// 
        /// </summary>
        [JsonConverter(typeof(NoTypeConverterJsonConverter<YM2413Career>))]
        [DataContract]
        public class YM2413Career : YM2413Operator
        {

            #region Etc

            [DataMember]
            [Description("Memo")]
            public string Memo
            {
                get;
                set;
            }


            [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                typeof(UITypeEditor)), Localizable(false)]
            [IgnoreDataMember]
            [JsonIgnore]
            [Description("You can copy and paste this text data to other same type timber.\r\n" +
                "AR, DR), RR, SL, SR, KSL, KSR, MUL, AM(AMS), VIB, EG, DIST\r\n" +
                "You can use comma or space chars as delimiter.")]
            public string MmlSerializeData
            {
                get
                {
                    return SimpleSerializer.SerializeProps(this,
                        nameof(AR),
                        nameof(DR),
                        nameof(RR),
                        nameof(SL),
                        nameof(SR),
                        nameof(KSL),
                        nameof(KSR),
                        nameof(MUL),
                        nameof(AM),
                        nameof(VIB),
                        nameof(EG),
                        nameof(DIST));
                }
                set
                {
                    SimpleSerializer.DeserializeProps(this, value,
                        nameof(AR),
                        nameof(DR),
                        nameof(RR),
                        nameof(SL),
                        nameof(SR),
                        nameof(KSL),
                        nameof(KSR),
                        nameof(MUL),
                        nameof(AM),
                        nameof(VIB),
                        nameof(EG),
                        nameof(DIST));
                }
            }

            [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                typeof(UITypeEditor)), Localizable(false)]
            [IgnoreDataMember]
            [JsonIgnore]
            [Description("You can copy and paste this text data to other same type timber.\r\nNote: Open dropdown editor then copy all text and paste to dropdown editor. Do not copy and paste one liner text.")]
            public string SerializeData
            {
                get
                {
                    return JsonConvert.SerializeObject(this, Formatting.Indented);
                }
                set
                {
                    RestoreFrom(value);
                }
            }

            public void RestoreFrom(string serializeData)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<YM2413Career>(serializeData);
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

            #endregion

        }

        [JsonConverter(typeof(NoTypeConverterJsonConverter<YM2413FxSettings>))]
        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [DataContract]
        [MidiHook]
        public class YM2413FxSettings : BasicFxSettings
        {

            private string f_ToneEnvelopes;

            [DataMember]
            [Description("Set ToneType envelop by text. Input ToneType value and split it with space like the Famitracker.\r\n" +
                       "0-15 \"|\" is repeat point. \"/\" is release point.")]
            public string ToneEnvelopes
            {
                get
                {
                    return f_ToneEnvelopes;
                }
                set
                {
                    if (f_ToneEnvelopes != value)
                    {
                        ToneEnvelopesRepeatPoint = -1;
                        ToneEnvelopesReleasePoint = -1;
                        if (value == null)
                        {
                            ToneEnvelopesNums = new int[] { };
                            f_ToneEnvelopes = string.Empty;
                            return;
                        }
                        f_ToneEnvelopes = value;
                        string[] vals = value.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        List<int> vs = new List<int>();
                        for (int i = 0; i < vals.Length; i++)
                        {
                            string val = vals[i];
                            if (val.Equals("|", StringComparison.Ordinal))
                                ToneEnvelopesRepeatPoint = vs.Count;
                            else if (val.Equals("/", StringComparison.Ordinal))
                                ToneEnvelopesReleasePoint = vs.Count;
                            else
                            {
                                int v;
                                if (int.TryParse(val, out v))
                                {
                                    if (v < 0)
                                        v = 0;
                                    else if (v > 15)
                                        v = 15;
                                    vs.Add(v);
                                }
                            }
                        }
                        ToneEnvelopesNums = vs.ToArray();

                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < ToneEnvelopesNums.Length; i++)
                        {
                            if (sb.Length != 0)
                                sb.Append(' ');
                            if (ToneEnvelopesRepeatPoint == i)
                                sb.Append("| ");
                            if (ToneEnvelopesReleasePoint == i)
                                sb.Append("/ ");
                            sb.Append(ToneEnvelopesNums[i].ToString((IFormatProvider)null));
                        }
                        f_ToneEnvelopes = sb.ToString();
                    }
                }
            }

            public bool ShouldSerializeDutyEnvelopes()
            {
                return !string.IsNullOrEmpty(ToneEnvelopes);
            }

            public void ResetDutyEnvelopes()
            {
                ToneEnvelopes = null;
            }

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            public int[] ToneEnvelopesNums { get; set; } = new int[] { };

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int ToneEnvelopesRepeatPoint { get; set; } = -1;

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int ToneEnvelopesReleasePoint { get; set; } = -1;

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override AbstractFxEngine CreateEngine()
            {
                return new YM2413FxEngine(this);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public class YM2413FxEngine : BasicFxEngine
        {
            private YM2413FxSettings settings;

            /// <summary>
            /// 
            /// </summary>
            public YM2413FxEngine(YM2413FxSettings settings) : base(settings)
            {
                this.settings = settings;
            }

            private uint f_toneCounter;

            public byte? ToneValue
            {
                get;
                private set;
            }

            protected override bool ProcessCore(SoundBase sound, bool isKeyOff, bool isSoundOff)
            {
                bool process = base.ProcessCore(sound, isKeyOff, isSoundOff);

                ToneValue = null;
                if (settings.ToneEnvelopesNums.Length > 0)
                {
                    if (!isKeyOff)
                    {
                        var vm = settings.ToneEnvelopesNums.Length;
                        if (settings.ToneEnvelopesReleasePoint >= 0)
                            vm = settings.ToneEnvelopesReleasePoint;
                        if (f_toneCounter >= vm)
                        {
                            if (settings.ToneEnvelopesRepeatPoint >= 0)
                                f_toneCounter = (uint)settings.ToneEnvelopesRepeatPoint;
                            else
                                f_toneCounter = (uint)vm;
                        }
                    }
                    else
                    {
                        if (settings.ToneEnvelopesReleasePoint < 0)
                            f_toneCounter = (uint)settings.ToneEnvelopesNums.Length;

                        if (f_toneCounter >= settings.ToneEnvelopesNums.Length)
                        {
                            if (settings.ToneEnvelopesRepeatPoint >= 0)
                                f_toneCounter = (uint)settings.ToneEnvelopesRepeatPoint;
                        }
                    }
                    if (f_toneCounter < settings.ToneEnvelopesNums.Length)
                    {
                        int vol = settings.ToneEnvelopesNums[f_toneCounter++];

                        ToneValue = (byte)vol;
                        process = true;
                    }
                }

                return process;
            }
        }

    }


}