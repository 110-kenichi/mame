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

//http://www.oplx.com/opl2/docs/adlib_sb.txt
//ftp://ftp.oldskool.org/pub/drivers/Yamaha/yamaha_ym3812.pdf

namespace zanac.MAmidiMEmo.Instruments.Chips
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class YM3812 : InstrumentBase
    {

        public override string Name => "YM3812";

        public override string Group => "FM";

        public override InstrumentType InstrumentType => InstrumentType.YM3812;

        [Browsable(false)]
        public override string ImageKey => "YM3812";

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        protected override string SoundInterfaceTagNamePrefix => "ym3812_";

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
                return 8;
            }
        }

        private byte f_AMD;

        /// <summary>
        /// AM Depth (0-1)
        /// </summary>
        [DataMember]
        [Category("Chip")]
        [Description("AM depth (0:1dB 1:4.8dB)")]
        [SlideParametersAttribute(0, 1)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((byte)0)]
        public byte AMD
        {
            get
            {
                return f_AMD;
            }
            set
            {
                var v = (byte)(value & 1);
                if (f_AMD != v)
                {
                    f_AMD = v;
                    YM3812WriteData(UnitNumber, 0xBD, 0, 0, (byte)(AMD << 7 | VIB << 6));
                }
            }
        }

        private byte f_VIB;

        /// <summary>
        /// Vibrato depth (0:7 cent 1:14 cent)
        /// </summary>
        [DataMember]
        [Category("Chip")]
        [Description("Vibrato depth (0:7 cent 1:14 cent)")]
        [SlideParametersAttribute(0, 1)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((byte)0)]
        public byte VIB
        {
            get
            {
                return f_VIB;
            }
            set
            {
                var v = (byte)(value & 1);
                if (f_VIB != v)
                {
                    f_VIB = v;
                    YM3812WriteData(UnitNumber, 0xBD, 0, 0, (byte)(AMD << 7 | VIB << 6));
                }
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

        [DataMember]
        [Category("Chip")]
        [Description("Timbres (0-127)")]
        [EditorAttribute(typeof(DummyEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(ExpandableCollectionConverter))]
        public YM3812Timbre[] Timbres
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override TimbreBase GetTimbre(int channel)
        {
            var pn = (SevenBitNumber)ProgramNumbers[channel];
            return Timbres[pn];
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="serializeData"></param>
        public override void RestoreFrom(string serializeData)
        {
            try
            {
                using (var obj = JsonConvert.DeserializeObject<YM3812>(serializeData))
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
        private delegate void delegate_YM3812_write(uint unitNumber, uint address, byte data);

        /// <summary>
        /// 
        /// </summary>
        private static delegate_YM3812_write YM3812_write
        {
            get;
            set;
        }

        private static byte[] addressTable = new byte[] { 0x00, 0x01, 0x02, 0x08, 0x09, 0x0a, 0x10, 0x11, 0x12 };

        /// <summary>
        /// 
        /// </summary>
        private static void YM3812WriteData(uint unitNumber, byte address, int op, int slot, byte data)
        {
            try
            {
                //Channel        1   2   3   4   5   6   7   8   9
                //Operator 1    00  01  02  08  09  0A  10  11  12
                //Operator 2    03  04  05  0B  0C  0D  13  14  15

                Program.SoundUpdating();
                YM3812_write(unitNumber, 0, (byte)(address + (op * 3) + addressTable[slot]));
                YM3812_write(unitNumber, 1, data);
            }
            finally
            {
                Program.SoundUpdated();
            }
        }


        private const float DEFAULT_GAIN = 1.5f;

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

        /// <summary>
        /// 
        /// </summary>
        static YM3812()
        {
            IntPtr funcPtr = MameIF.GetProcAddress("ym3812_write");
            if (funcPtr != IntPtr.Zero)
                YM3812_write = (delegate_YM3812_write)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_YM3812_write));
        }

        private YM3812SoundManager soundManager;

        /// <summary>
        /// 
        /// </summary>
        public YM3812(uint unitNumber) : base(unitNumber)
        {
            GainLeft = DEFAULT_GAIN;
            GainRight = DEFAULT_GAIN;

            Timbres = new YM3812Timbre[128];
            for (int i = 0; i < 128; i++)
                Timbres[i] = new YM3812Timbre();
            setPresetInstruments();

            this.soundManager = new YM3812SoundManager(this);
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
            Timbres[0].FB = 0;
            Timbres[0].ALG = 1;

            Timbres[0].Ops[0].AM = 0;
            Timbres[0].Ops[0].VR = 0;
            Timbres[0].Ops[0].EG = 0;
            Timbres[0].Ops[0].KSR = 0;
            Timbres[0].Ops[0].MFM = 1;
            Timbres[0].Ops[0].KSL = 0;
            Timbres[0].Ops[0].TL = 0;
            Timbres[0].Ops[0].AR = 15;
            Timbres[0].Ops[0].DR = 0;
            Timbres[0].Ops[0].SL = 0;
            Timbres[0].Ops[0].RR = 7;
            Timbres[0].Ops[0].WS = 1;

            Timbres[0].Ops[1].AM = 0;
            Timbres[0].Ops[1].VR = 0;
            Timbres[0].Ops[1].EG = 0;
            Timbres[0].Ops[1].KSR = 0;
            Timbres[0].Ops[1].MFM = 1;
            Timbres[0].Ops[1].KSL = 0;
            Timbres[0].Ops[1].TL = 0;
            Timbres[0].Ops[1].AR = 15;
            Timbres[0].Ops[1].DR = 0;
            Timbres[0].Ops[1].SL = 7;
            Timbres[0].Ops[1].RR = 7;
            Timbres[0].Ops[1].WS = 1;
        }

        internal override void PrepareSound()
        {
            base.PrepareSound();

            YM3812WriteData(UnitNumber, (byte)0x01, 0, 0, (byte)0x20);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected override void OnNoteOnEvent(NoteOnEvent midiEvent)
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
        private class YM3812SoundManager : SoundManagerBase
        {
            private SoundList<YM3812Sound> fmOnSounds = new SoundList<YM3812Sound>(9);

            private YM3812 parentModule;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            public YM3812SoundManager(YM3812 parent) : base(parent)
            {
                this.parentModule = parent;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="note"></param>
            public override SoundBase SoundOn(NoteOnEvent note)
            {
                int emptySlot = searchEmptySlot(note);
                if (emptySlot < 0)
                    return null;

                var pn = parentModule.ProgramNumbers[note.Channel];
                var timbre = parentModule.Timbres[pn];
                YM3812Sound snd = new YM3812Sound(parentModule, this, timbre, note, emptySlot);
                fmOnSounds.Add(snd);
                FormMain.OutputDebugLog("KeyOn FM ch" + emptySlot + " " + note.ToString());
                snd.KeyOn();

                return snd;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            private int searchEmptySlot(NoteOnEvent note)
            {
                int emptySlot = -1;

                var pn = parentModule.ProgramNumbers[note.Channel];

                var timbre = parentModule.Timbres[pn];
                emptySlot = SearchEmptySlotAndOff(fmOnSounds, note, parentModule.CalcMaxVoiceNumber(note.Channel, 9));
                return emptySlot;
            }

            internal override void AllSoundOff()
            {
                var me = new ControlChangeEvent((SevenBitNumber)120, (SevenBitNumber)0);
                ControlChange(me);

                for (int i = 0; i < 9; i++)
                {
                    YM3812WriteData(parentModule.UnitNumber, (byte)(0xB0 + i), 0, 0, (byte)(0));
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private class YM3812Sound : SoundBase
        {

            private YM3812 parentModule;

            private SevenBitNumber programNumber;

            private YM3812Timbre timbre;

            private byte lastFreqData;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentModule"></param>
            /// <param name="noteOnEvent"></param>
            /// <param name="programNumber"></param>
            /// <param name="slot"></param>
            public YM3812Sound(YM3812 parentModule, YM3812SoundManager manager, TimbreBase timbre, NoteOnEvent noteOnEvent, int slot) : base(parentModule, manager, timbre, noteOnEvent, slot)
            {
                this.parentModule = parentModule;
                this.programNumber = (SevenBitNumber)parentModule.ProgramNumbers[noteOnEvent.Channel];
                this.timbre = parentModule.Timbres[programNumber];
            }

            /// <summary>
            /// 
            /// </summary>
            public override void KeyOn()
            {
                base.KeyOn();

                var gs = timbre.GlobalSettings;
                if (gs.Enable)
                {
                    Program.SoundUpdating();
                    if (gs.AMD.HasValue)
                        parentModule.AMD = gs.AMD.Value;
                    if (gs.VIB.HasValue)
                        parentModule.VIB = gs.VIB.Value;
                    Program.SoundUpdated();
                }

                //
                SetTimbre();
                //Volume
                OnVolumeUpdated();
                //Freq
                OnPitchUpdated();
            }

            public override void OnSoundParamsUpdated()
            {
                base.OnSoundParamsUpdated();

                var gs = timbre.GlobalSettings;
                if (gs.Enable)
                {
                    Program.SoundUpdating();
                    if (gs.AMD.HasValue)
                        parentModule.AMD = gs.AMD.Value;
                    if (gs.VIB.HasValue)
                        parentModule.VIB = gs.VIB.Value;
                    Program.SoundUpdated();
                }

                //
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

                var v = CalcCurrentVolume();
                for (int op = 0; op < 2; op++)
                {
                    YM3812Operator o = timbre.Ops[op];
                    //$40+: Scaling level/ total level
                    if (timbre.ALG == 1 || op == 1)
                        YM3812WriteData(parentModule.UnitNumber, 0x40, op, Slot, (byte)(o.KSL << 6 | (63 - (byte)Math.Round((63 - o.TL) * v))));
                    else
                        YM3812WriteData(parentModule.UnitNumber, 0x40, op, Slot, (byte)(o.KSL << 6 | o.TL));
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="slot"></param>
            public override void OnPitchUpdated()
            {
                double d = CalcCurrentPitch();

                int noteNum = NoteOnEvent.NoteNumber + (int)d;
                if (noteNum > 127)
                    noteNum = 127;
                else if (noteNum < 0)
                    noteNum = 0;
                var nnOn = new NoteOnEvent((SevenBitNumber)noteNum, (SevenBitNumber)127);
                ushort freq = convertFmFrequency(nnOn);
                var octave = nnOn.GetNoteOctave();
                if (octave < 0)
                {
                    octave = 0;
                    freq = freqTable[0];
                }
                if (octave > 7)
                {
                    octave = 7;
                    freq = freqTable[13];
                }
                octave = octave << 2;

                if (d != 0)
                    freq += (ushort)(((double)(convertFmFrequency(nnOn, (d < 0) ? false : true) - freq)) * Math.Abs(d - Math.Truncate(d)));

                //keyon
                lastFreqData = (byte)(0x20 | octave | ((freq >> 8) & 3));
                Program.SoundUpdating();
                YM3812WriteData(parentModule.UnitNumber, (byte)(0xa0 + Slot), 0, 0, (byte)(0xff & freq));
                YM3812WriteData(parentModule.UnitNumber, (byte)(0xb0 + Slot), 0, 0, lastFreqData);
                Program.SoundUpdated();

                base.OnPitchUpdated();
            }

            /// <summary>
            /// 
            /// </summary>
            public void SetTimbre()
            {
                Program.SoundUpdating();
                for (int op = 0; op < 2; op++)
                {
                    YM3812Operator o = timbre.Ops[op];
                    //$20+: Amplitude Modulation / Vibrato / Envelope Generator Type / Keyboard Scaling Rate / Modulator Frequency Multiple
                    YM3812WriteData(parentModule.UnitNumber, 0x20, op, Slot, (byte)((o.AM << 7 | o.EG << 6 | o.KSR | o.MFM)));
                    //$60+: Attack Rate / Decay Rate
                    YM3812WriteData(parentModule.UnitNumber, 0x60, op, Slot, (byte)(o.AR << 4 | o.DR));
                    //$80+: Sustain Level / Release Rate
                    YM3812WriteData(parentModule.UnitNumber, 0x80, op, Slot, (byte)(o.SL << 4 | o.RR));
                    //$e0+: Waveform Select
                    YM3812WriteData(parentModule.UnitNumber, 0xe0, op, Slot, (byte)(o.WS));
                }

                //$C0+: algorithm and feedback
                YM3812WriteData(parentModule.UnitNumber, (byte)(0xc0 + Slot), 0, 0, (byte)(timbre.FB << 1 | timbre.ALG));
                Program.SoundUpdated();
            }

            /// <summary>
            /// 
            /// </summary>
            public override void SoundOff()
            {
                base.SoundOff();

                YM3812WriteData(parentModule.UnitNumber, (byte)(0xB0 + Slot), 0, 0, (byte)(lastFreqData & 0x1f));
            }

            private ushort[] freqTable = new ushort[] {
                0x287/2,
                0x157,
                0x16B,
                0x181,
                0x198,
                0x1B0,
                0x1CA,
                0x1E5,
                0x202,
                0x220,
                0x241,
                0x263,
                0x287,
                0x157*2,
            };

            /// <summary>
            /// 
            /// </summary>
            /// <param name="note"></param>
            /// <param name="freq"></param>
            /// <returns></returns>
            private ushort convertFmFrequency(NoteOnEvent note)
            {
                return freqTable[(int)note.GetNoteName() + 1];
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="note"></param>
            /// <param name="freq"></param>
            /// <returns></returns>
            private ushort convertFmFrequency(NoteOnEvent note, bool plus)
            {
                if (plus)
                    return freqTable[(int)note.GetNoteName() + 2];
                else
                    return freqTable[(int)note.GetNoteName()];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [JsonConverter(typeof(NoTypeConverterJsonConverter<YM3812Timbre>))]
        [DataContract]
        public class YM3812Timbre : TimbreBase
        {
            #region FM Symth

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

            private byte f_ALG;

            [DataMember]
            [Category("Sound")]
            [Description("Algorithm (0-1)\r\n" +
                "0: 1->2 (for Distortion guitar sound)\r\n" +
                "1: 1+2 (for Pipe organ sound)")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte ALG
            {
                get
                {
                    return f_ALG;
                }
                set
                {
                    f_ALG = (byte)(value & 1);
                }
            }

            #endregion

            /// <summary>
            /// 
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Operators")]
            [TypeConverter(typeof(ExpandableCollectionConverter))]
            [DisplayName("Operators(Ops)")]
            public YM3812Operator[] Ops
            {
                get;
                set;
            }

            [DataMember]
            [Category("Chip")]
            [Description("Global Settings")]
            public YM3812GlobalSettings GlobalSettings
            {
                get;
                set;
            }

            /// <summary>
            /// 
            /// </summary>
            public YM3812Timbre()
            {
                Ops = new YM3812Operator[] {
                    new YM3812Operator(),
                    new YM3812Operator() };
                GlobalSettings = new YM3812GlobalSettings();
                this.SDS.FxS = new BasicFxSettings();
            }

            public override void RestoreFrom(string serializeData)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<YM3812Timbre>(serializeData);
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
        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [JsonConverter(typeof(NoTypeConverterJsonConverter<YM3812Operator>))]
        [DataContract]
        [MidiHook]
        public class YM3812Operator : ContextBoundObject
        {

            private byte f_AM;

            /// <summary>
            /// Apply amplitude modulation(0-1)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Apply amplitude modulation (0:Off 1:On)")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 1)]
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

            private byte f_VR;

            /// <summary>
            /// Apply vibrato(0-1)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Vibrato (0:Off 1:On)")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte VR
            {
                get
                {
                    return f_VR;
                }
                set
                {
                    f_VR = (byte)(value & 1);
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

            private byte f_KSR;

            /// <summary>
            /// Keyboard scaling rate (0-1)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Keyboard scaling rate (1: the sound's envelope is foreshortened as it rises in pitch.")]
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

            private byte f_MFM = 1;

            /// <summary>
            /// Modulator Frequency Multiple (0-15)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Modulator Frequency Multiple (0-1-15)")]
            [DefaultValue((byte)1)]
            [SlideParametersAttribute(0, 15)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte MFM
            {
                get
                {
                    return f_MFM;
                }
                set
                {
                    f_MFM = (byte)(value & 15);
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

            private byte f_TL;

            /// <summary>
            /// Total Level(0-127)
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

            private byte f_RR;

            /// <summary>
            /// release rate(0-15)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Release Rate (0-15)")]
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

            private byte f_WS;

            /// <summary>
            /// Waveform Select (0-3)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Waveform Select (0-3)")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 3)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte WS
            {
                get
                {
                    return f_WS;
                }
                set
                {
                    f_WS = (byte)(value & 3);
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
                    var obj = JsonConvert.DeserializeObject<YM3812Operator>(serializeData);
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

        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [JsonConverter(typeof(NoTypeConverterJsonConverter<YM3812GlobalSettings>))]
        [DataContract]
        [MidiHook]
        public class YM3812GlobalSettings : ContextBoundObject
        {
            [DataMember]
            [Category("Chip")]
            [Description("Override global settings")]
            public bool Enable
            {
                get;
                set;
            }

            private byte? f_AMD;

            /// <summary>
            /// AM Depth (0-1)
            /// </summary>
            [DataMember]
            [Category("Chip")]
            [Description("AM depth (0:1dB 1:4.8dB)")]
            [DefaultValue(null)]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte? AMD
            {
                get
                {
                    return f_AMD;
                }
                set
                {
                    byte? v = value;
                    if (value.HasValue)
                        v = (byte)(value & 1);
                    f_AMD = v;
                }
            }

            private byte? f_VIB;

            /// <summary>
            /// Vibrato depth (0:7 cent 1:14 cent)
            /// </summary>
            [DataMember]
            [Category("Chip")]
            [Description("Vibrato depth (0:7 cent 1:14 cent)")]
            [DefaultValue(null)]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte? VIB
            {
                get
                {
                    return f_VIB;
                }
                set
                {
                    byte? v = value;
                    if (value.HasValue)
                        v = (byte)(value & 1);
                    f_VIB = v;
                }
            }
        }

    }
}