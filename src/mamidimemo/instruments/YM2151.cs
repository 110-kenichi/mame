﻿// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Smf;
using Newtonsoft.Json;
using Omu.ValueInjecter;
using Omu.ValueInjecter.Injections;
using zanac.MAmidiMEmo.ComponentModel;
using zanac.MAmidiMEmo.Gui;
using zanac.MAmidiMEmo.Mame;

//https://www16.atwiki.jp/mxdrv/pages/24.html
//http://map.grauw.nl/resources/sound/yamaha_ym2151_synthesis.pdf

namespace zanac.MAmidiMEmo.Instruments
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class YM2151 : InstrumentBase
    {

        public override string Name => "YM2151";

        public override string Group => "FM";

        public override InstrumentType InstrumentType => InstrumentType.YM2151;

        [Browsable(false)]
        public override string ImageKey => "YM2151";

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        protected override string SoundInterfaceTagNamePrefix => "ym2151_";

        /// <summary>
        /// 
        /// </summary>
        [Category("MIDI")]
        [Description("MIDI Device ID")]
        public override uint DeviceID
        {
            get
            {
                return 1;
            }
        }

        [DataMember]
        [Category("Chip")]
        [Description("Timbres (0-127)")]
        [EditorAttribute(typeof(DummyEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(ExpandableCollectionConverter))]
        public YM2151Timbre[] Timbres
        {
            get;
            set;
        }


        private byte f_LFRQ;

        /// <summary>
        /// LFRQ (0-255)
        /// </summary>
        [DataMember]
        [Category("Chip")]
        [Description("LFO Freq (0-255)")]
        public byte LFRQ
        {
            get
            {
                return f_LFRQ;
            }
            set
            {
                if (f_LFRQ != value)
                {
                    f_LFRQ = value;
                    Ym2151WriteData(UnitNumber, 0x18, 0, 0, LFRQ);
                }
            }
        }


        private byte f_LFOF;

        /// <summary>
        /// Select AMD or PMD(0:AMD 1:PMD)
        /// </summary>
        [DataMember]
        [Category("Chip")]
        [Description("Select AMD or PMD (0:AMD 1:PMD)")]
        public byte LFOF
        {
            get
            {
                return f_LFOF;
            }
            set
            {
                byte v = (byte)(value & 1);
                if (f_LFOF != v)
                {
                    f_LFOF = v;
                    Ym2151WriteData(UnitNumber, 0x19, 0, 0, (byte)(LFOF << 7 | LFOD));
                }
            }
        }

        private byte f_LFOD;


        /// <summary>
        /// LFO Depth(0-127)
        /// </summary>
        [DataMember]
        [Category("Chip")]
        [Description("LFO Depth (0-127)")]
        public byte LFOD
        {
            get
            {
                return f_LFOD;
            }
            set
            {
                byte v = (byte)(value & 127);
                if (f_LFOD != v)
                {
                    f_LFOD = v;
                    Ym2151WriteData(UnitNumber, 0x19, 0, 0, (byte)(LFOF << 7 | LFOD));
                }
            }
        }


        private byte f_LFOW;


        /// <summary>
        /// LFO Wave Type (0:Saw 1:SQ 2:Tri 3:Rnd)
        /// </summary>
        [DataMember]
        [Category("Chip")]
        [Description("LFO Wave Type (0:Saw 1:SQ 2:Tri 3:Rnd)")]
        public byte LFOW
        {
            get
            {
                return f_LFOW;
            }
            set
            {
                byte v = (byte)(value & 3);
                if (f_LFOW != v)
                {
                    f_LFOW = v;
                    Ym2151WriteData(UnitNumber, 0x1B, 0, 0, (byte)LFOW);
                }
            }
        }

        private byte f_NE;

        /// <summary>
        /// Noise Enable (0:Disable 1:Enable)
        /// </summary>
        [Browsable(false)]
        [DataMember]
        [Category("Chip")]
        [Description("Noise Enable (0:Disable 1:Enable)")]
        public byte NE
        {
            get
            {
                return f_NE;
            }
            set
            {
                byte v = (byte)(value & 1);
                if (f_NE != v)
                {
                    f_NE = v;
                    Ym2151WriteData(UnitNumber, 0x0f, 0, 0, (byte)(NE << 7 | NFRQ));
                }
            }
        }

        private byte f_NFRQ;

        /// <summary>
        /// Noise Feequency (0-31)
        /// </summary>
        [Browsable(false)]
        [DataMember]
        [Category("Chip")]
        [Description(" Noise Feequency (0-31)\r\n" +
            "3'579'545/(32*NFRQ)")]
        public byte NFRQ
        {
            get
            {
                return f_NFRQ;
            }
            set
            {
                byte v = (byte)(value & 31);
                if (f_NFRQ != v)
                {
                    f_NFRQ = v;
                    Ym2151WriteData(UnitNumber, 0x0f, 0, 0, (byte)(NE << 7 | NFRQ));
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serializeData"></param>
        public override void RestoreFrom(string serializeData)
        {
            try
            {
                var obj = JsonConvert.DeserializeObject<YM2151>(serializeData);
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
        private delegate void delegate_ym2151_write(uint unitNumber, uint address, byte data);


        /// <summary>
        /// 
        /// </summary>
        private static delegate_ym2151_write Ym2151_write
        {
            get;
            set;
        }


        /// <summary>
        /// 
        /// </summary>
        private static void Ym2151WriteData(uint unitNumber, byte address, int op, int slot, byte data)
        {
            try
            {
                Program.SoundUpdating();

                switch (op)
                {
                    case 0:
                        op = 0;
                        break;
                    case 1:
                        op = 2;
                        break;
                    case 2:
                        op = 1;
                        break;
                    case 3:
                        op = 3;
                        break;
                }
                Ym2151_write(unitNumber, 0, (byte)(address + (op * 8) + slot));
                Ym2151_write(unitNumber, 1, data);
            }
            finally
            {
                Program.SoundUpdated();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        static YM2151()
        {
            IntPtr funcPtr = MameIF.GetProcAddress("ym2151_write");
            if (funcPtr != IntPtr.Zero)
            {
                Ym2151_write = (delegate_ym2151_write)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_ym2151_write));
            }
        }

        private YM2151SoundManager soundManager;

        /// <summary>
        /// 
        /// </summary>
        public YM2151(uint unitNumber) : base(unitNumber)
        {
            GainLeft = 2.0f;
            GainRight = 2.0f;

            Timbres = new YM2151Timbre[128];
            for (int i = 0; i < 128; i++)
                Timbres[i] = new YM2151Timbre();

            setPresetInstruments();

            this.soundManager = new YM2151SoundManager(this);
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
            //Dist Gt
            Timbres[0].PMS = 0;
            Timbres[0].AMS = 0;
            Timbres[0].FB = 0;
            Timbres[0].ALG = 3;

            Timbres[0].Ops[0].Enable = 1;
            Timbres[0].Ops[0].AR = 31;
            Timbres[0].Ops[0].D1R = 0;
            Timbres[0].Ops[0].SL = 0;
            Timbres[0].Ops[0].D2R = 0;
            Timbres[0].Ops[0].RR = 6;

            Timbres[0].Ops[0].MUL = 8;
            Timbres[0].Ops[0].RS = 0;
            Timbres[0].Ops[0].DT1 = 7;
            Timbres[0].Ops[0].AM = 0;
            Timbres[0].Ops[0].DT2 = 0;
            Timbres[0].Ops[0].TL = 56;

            Timbres[0].Ops[1].Enable = 1;
            Timbres[0].Ops[1].AR = 31;
            Timbres[0].Ops[1].D1R = 18;
            Timbres[0].Ops[1].SL = 0;
            Timbres[0].Ops[1].D2R = 0;
            Timbres[0].Ops[1].RR = 6;

            Timbres[0].Ops[1].MUL = 3;
            Timbres[0].Ops[1].RS = 0;
            Timbres[0].Ops[1].DT1 = 7;
            Timbres[0].Ops[1].AM = 0;
            Timbres[0].Ops[1].DT2 = 0;
            Timbres[0].Ops[1].TL = 19;

            Timbres[0].Ops[2].Enable = 1;
            Timbres[0].Ops[2].AR = 31;
            Timbres[0].Ops[2].D1R = 0;
            Timbres[0].Ops[2].SL = 0;
            Timbres[0].Ops[2].D2R = 0;
            Timbres[0].Ops[2].RR = 6;

            Timbres[0].Ops[2].MUL = 3;
            Timbres[0].Ops[2].RS = 0;
            Timbres[0].Ops[2].DT1 = 4;
            Timbres[0].Ops[2].AM = 0;
            Timbres[0].Ops[2].DT2 = 0;
            Timbres[0].Ops[2].TL = 8;

            Timbres[0].Ops[3].Enable = 1;
            Timbres[0].Ops[3].AR = 20;
            Timbres[0].Ops[3].D1R = 17;
            Timbres[0].Ops[3].SL = 0;
            Timbres[0].Ops[3].D2R = 0;
            Timbres[0].Ops[3].RR = 5;

            Timbres[0].Ops[3].MUL = 2;
            Timbres[0].Ops[3].RS = 0;
            Timbres[0].Ops[3].DT1 = 4;
            Timbres[0].Ops[3].AM = 0;
            Timbres[0].Ops[3].DT2 = 0;
            Timbres[0].Ops[3].TL = 24;
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

        /// <summary>
        /// 
        /// </summary>
        private class YM2151SoundManager : SoundManagerBase
        {

            private SoundList<YM2151Sound> fmOnSounds = new SoundList<YM2151Sound>(8);

            private YM2151 parentModule;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            public YM2151SoundManager(YM2151 parent)
            {
                this.parentModule = parent;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="note"></param>
            public override void KeyOn(NoteOnEvent note)
            {
                int emptySlot = searchEmptySlot(note);
                if (emptySlot < 0)
                    return;

                var pn = parentModule.ProgramNumbers[note.Channel];
                var timbre = parentModule.Timbres[pn];
                YM2151Sound snd = new YM2151Sound(parentModule, this, timbre, note, emptySlot);
                fmOnSounds.Add(snd);
                FormMain.OutputDebugLog("KeyOn FM ch" + emptySlot + " " + note.ToString());
                snd.KeyOn();

                base.KeyOn(note);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            private int searchEmptySlot(NoteOnEvent note)
            {
                return SearchEmptySlotAndOff(fmOnSounds, note, 8);
            }

        }


        /// <summary>
        /// 
        /// </summary>
        private class YM2151Sound : SoundBase
        {
            private YM2151 parentModule;

            private SevenBitNumber programNumber;

            private YM2151Timbre timbre;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentModule"></param>
            /// <param name="noteOnEvent"></param>
            /// <param name="programNumber"></param>
            /// <param name="slot"></param>
            public YM2151Sound(YM2151 parentModule, YM2151SoundManager manager, TimbreBase timbre, NoteOnEvent noteOnEvent, int slot) : base(parentModule, manager, timbre , noteOnEvent, slot)
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

                //
                SetTimbre();
                //Freq
                UpdatePitch();
                //Volume
                UpdateVolume();
                //On
                byte op = (byte)(timbre.Ops[0].Enable << 3 | timbre.Ops[2].Enable << 4 | timbre.Ops[1].Enable << 5 | timbre.Ops[3].Enable << 6);
                Ym2151WriteData(parentModule.UnitNumber, 0x01, 0, 0, (byte)0x2);
                Ym2151WriteData(parentModule.UnitNumber, 0x01, 0, 0, (byte)0x0);
                Ym2151WriteData(parentModule.UnitNumber, 0x08, 0, 0, (byte)(op | Slot));
            }

            /// <summary>
            /// 
            /// </summary>
            public override void UpdateVolume()
            {
                List<int> ops = new List<int>();
                switch (timbre.ALG)
                {
                    case 0:
                        ops.Add(3);
                        break;
                    case 1:
                        ops.Add(3);
                        break;
                    case 2:
                        ops.Add(3);
                        break;
                    case 3:
                        ops.Add(3);
                        break;
                    case 4:
                        ops.Add(1);
                        ops.Add(3);
                        break;
                    case 5:
                        ops.Add(1);
                        ops.Add(2);
                        ops.Add(3);
                        break;
                    case 6:
                        ops.Add(1);
                        ops.Add(2);
                        ops.Add(3);
                        break;
                    case 7:
                        ops.Add(0);
                        ops.Add(1);
                        ops.Add(2);
                        ops.Add(3);
                        break;
                }
                var exp = parentModule.Expressions[NoteOnEvent.Channel] / 127d;
                var vol = parentModule.Volumes[NoteOnEvent.Channel] / 127d;
                var vel = NoteOnEvent.Velocity / 127d;
                foreach (int op in ops)
                {
                    //$60+: total level
                    Ym2151WriteData(parentModule.UnitNumber, 0x60, op, Slot, (byte)(127 - Math.Round((127 - timbre.Ops[op].TL) * vol * vel * exp)));
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="slot"></param>
            public override void UpdatePitch()
            {
                double d = CalcCurrentPitch() * 63d;

                int kf = 0;
                if (d > 0)
                    kf = (int)d % 63;
                else if (d < 0)
                    kf = 63 + ((int)d % 63);

                int noted = (int)d / 63;
                if (d < 0)
                    noted -= 1;

                int noteNum = NoteOnEvent.NoteNumber + noted;
                if (noteNum > 127)
                    noteNum = 127;
                else if (noteNum < 0)
                    noteNum = 0;

                var nnOn = new NoteOnEvent((SevenBitNumber)noteNum, (SevenBitNumber)127);

                byte nn = getNoteNum(nnOn.GetNoteName());
                byte octave = (byte)nnOn.GetNoteOctave();
                if (nn == 14)
                {
                    if (octave > 0)
                        octave -= 1;
                    else
                        nn = 0;
                }
                if (octave > 0)
                    octave -= 1;
                Program.SoundUpdating();
                Ym2151WriteData(parentModule.UnitNumber, 0x28, 0, Slot, (byte)((octave << 4) | nn));
                Ym2151WriteData(parentModule.UnitNumber, 0x30, 0, Slot, (byte)(kf << 2));
                Program.SoundUpdated();
            }

            private byte getNoteNum(NoteName noteName)
            {
                byte nn = 0;
                switch (noteName)
                {
                    case NoteName.C:
                        nn = 14;
                        break;
                    case NoteName.CSharp:
                        nn = 0;
                        break;
                    case NoteName.D:
                        nn = 1;
                        break;
                    case NoteName.DSharp:
                        nn = 2;
                        break;
                    case NoteName.E:
                        nn = 4;
                        break;
                    case NoteName.F:
                        nn = 5;
                        break;
                    case NoteName.FSharp:
                        nn = 6;
                        break;
                    case NoteName.G:
                        nn = 8;
                        break;
                    case NoteName.GSharp:
                        nn = 9;
                        break;
                    case NoteName.A:
                        nn = 10;
                        break;
                    case NoteName.ASharp:
                        nn = 12;
                        break;
                    case NoteName.B:
                        nn = 13;
                        break;
                }

                return nn;
            }

            /// <summary>
            /// 
            /// </summary>
            public override void UpdatePanpot()
            {
                byte pan = parentModule.Panpots[NoteOnEvent.Channel];
                if (pan < 32)
                    pan = 0x1;
                else if (pan > 96)
                    pan = 0x2;
                else
                    pan = 0x3;
                Ym2151WriteData(parentModule.UnitNumber, 0x20, 0, Slot, (byte)(pan << 6 | (timbre.FB << 3) | timbre.ALG));
            }

            /// <summary>
            /// 
            /// </summary>
            public void SetTimbre()
            {
                Ym2151WriteData(parentModule.UnitNumber, 0x38, 0, Slot, (byte)((timbre.PMS << 4 | timbre.AMS)));
                for (int op = 0; op < 4; op++)
                {
                    Ym2151WriteData(parentModule.UnitNumber, 0x40, op, Slot, (byte)((timbre.Ops[op].DT1 << 4 | timbre.Ops[op].MUL)));
                    Ym2151WriteData(parentModule.UnitNumber, 0x60, op, Slot, (byte)timbre.Ops[op].TL);
                    Ym2151WriteData(parentModule.UnitNumber, 0x80, op, Slot, (byte)((timbre.Ops[op].RS << 6 | timbre.Ops[op].AR)));
                    Ym2151WriteData(parentModule.UnitNumber, 0xa0, op, Slot, (byte)((timbre.Ops[op].AM << 7 | timbre.Ops[op].D1R)));
                    Ym2151WriteData(parentModule.UnitNumber, 0xc0, op, Slot, (byte)((timbre.Ops[op].DT2 << 7 | timbre.Ops[op].D2R)));
                    Ym2151WriteData(parentModule.UnitNumber, 0xe0, op, Slot, (byte)((timbre.Ops[op].SL << 7 | timbre.Ops[op].RR)));
                }

                UpdatePanpot();
            }

            /// <summary>
            /// 
            /// </summary>
            public override void KeyOff()
            {
                base.KeyOff();

                Ym2151WriteData(parentModule.UnitNumber, 0x08, 0, 0, (byte)(0x00 | Slot));
            }

        }


        /// <summary>
        /// 
        /// </summary>
        [JsonConverter(typeof(NoTypeConverterJsonConverter<YM2151Timbre>))]
        [DataContract]
        public class YM2151Timbre : TimbreBase
        {
            #region FM Symth

            private byte f_FB;

            [DataMember]
            [Category("Sound")]
            [Description("Feedback (0-7)")]
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
            [Description("Algorithm (0-7)\r\n" +
                "0: 1->2->3->4 (for Distortion guitar sound)\r\n" +
                "1: (1+2)->3->4 (for Harp, PSG sound)\r\n" +
                "2: (1+(2->3))->4 (for Bass, electric guitar, brass, piano, woods sound)\r\n" +
                "3: ((1->2)+3)->4 (for Strings, folk guitar, chimes sound)\r\n" +
                "4: (1->2)+(3->4) (for Flute, bells, chorus, bass drum, snare drum, tom-tom sound)\r\n" +
                "5: (1->2)+(1->3)+(1->4) (for Brass, organ sound)\r\n" +
                "6: (1->2)+3+4 (for Xylophone, tom-tom, organ, vibraphone, snare drum, base drum sound)\r\n" +
                "7: 1+2+3+4 (for Pipe organ sound)")]
            public byte ALG
            {
                get
                {
                    return f_ALG;
                }
                set
                {
                    f_ALG = (byte)(value & 7);
                }
            }

            private byte f_AMS;

            [DataMember]
            [Category("Sound")]
            [Description("Amplitude Modulation Sensitivity (0-3)")]
            public byte AMS
            {
                get
                {
                    return f_AMS;
                }
                set
                {
                    f_AMS = (byte)(value & 3);
                }
            }

            private byte f_PMS;

            [DataMember]
            [Category("Sound")]
            [Description("Phase Modulation Sensitivity (0-7)")]
            public byte PMS
            {
                get
                {
                    return f_PMS;
                }
                set
                {
                    f_PMS = (byte)(value & 7);
                }
            }

            /// <summary>
            /// 
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Operators")]
            [TypeConverter(typeof(ExpandableCollectionConverter))]
            [DisplayName("Operators")]
            public YM2151Operator[] Ops
            {
                get;
                private set;
            }

            /// <summary>
            /// 
            /// </summary>
            public YM2151Timbre()
            {
                Ops = new YM2151Operator[] { new YM2151Operator(), new YM2151Operator(), new YM2151Operator(), new YM2151Operator() };
            }

            #endregion

            public override void RestoreFrom(string serializeData)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<YM2151Timbre>(serializeData);
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
        [JsonConverter(typeof(NoTypeConverterJsonConverter<YM2151Operator>))]
        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [DataContract]
        public class YM2151Operator
        {
            private byte f_Enable = 1;

            /// <summary>
            /// Enable(0-1)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Whether this operator enable or not")]
            public byte Enable
            {
                get
                {
                    return f_Enable;
                }
                set
                {
                    f_Enable = (byte)(value & 1);
                }
            }

            private byte f_DT1;

            /// <summary>
            /// Detune1(0-7)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("DeTune 1 (1-4-7)")]
            public byte DT1
            {
                get
                {
                    return f_DT1;
                }
                set
                {
                    f_DT1 = (byte)(value & 7);
                }
            }

            private byte f_MUL;

            /// <summary>
            /// Multiply(0-15)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Multiply (0-15)")]
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

            private byte f_TL;

            /// <summary>
            /// Total Level(0-127)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Total Level (0-127)")]
            public byte TL
            {
                get
                {
                    return f_TL;
                }
                set
                {
                    f_TL = (byte)(value & 127);
                }
            }

            private byte f_RS;

            /// <summary>
            /// Rate Scaling(0-3)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Rate Scaling (0-3)")]
            public byte RS
            {
                get
                {
                    return f_RS;
                }
                set
                {
                    f_RS = (byte)(value & 3);
                }
            }

            private byte f_AR;

            /// <summary>
            /// Attack Rate(0-31)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Attack Rate (0-31)")]
            public byte AR
            {
                get
                {
                    return f_AR;
                }
                set
                {
                    f_AR = (byte)(value & 31);
                }
            }

            private byte f_AM;

            /// <summary>
            /// AMS Enable (0:Disable 1:Enable)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Amplitude Modulation Sensivity (0-1)")]
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

            private byte f_D1R;

            /// <summary>
            /// 1st decay rate(0-31)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("1st Decay Rate (0-31)")]
            public byte D1R
            {
                get
                {
                    return f_D1R;
                }
                set
                {
                    f_D1R = (byte)(value & 31);
                }
            }

            private byte f_D2R;

            /// <summary>
            /// 2nd decay rate(0-31)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("2nd Decay Rate (0-31)")]
            public byte D2R
            {
                get
                {
                    return f_D2R;
                }
                set
                {
                    f_D2R = (byte)(value & 31);
                }
            }

            private byte f_SL;

            /// <summary>
            /// sustain level(0-15)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Sustain Level(0-15)")]
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

            private byte f_DT2;

            /// <summary>
            /// DT2(0-3)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Detune 2 (0-3)")]
            public byte DT2
            {
                get
                {
                    return f_DT2;
                }
                set
                {
                    f_DT2 = (byte)(value & 3);
                }
            }
        }

    }


}