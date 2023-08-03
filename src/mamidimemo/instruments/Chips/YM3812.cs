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
using FM_SoundConvertor;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.MusicTheory;
using Newtonsoft.Json;
using Omu.ValueInjecter;
using Omu.ValueInjecter.Injections;
using zanac.MAmidiMEmo.ComponentModel;
using zanac.MAmidiMEmo.Gui;
using zanac.MAmidiMEmo.Gui.FMEditor;
using zanac.MAmidiMEmo.Instruments.Envelopes;
using zanac.MAmidiMEmo.Mame;
using zanac.MAmidiMEmo.Midi;
using static zanac.MAmidiMEmo.Instruments.Chips.YM2413;

//http://www.oplx.com/opl2/docs/adlib_sb.txt
//ftp://ftp.oldskool.org/pub/drivers/Yamaha/yamaha_ym3812.pdf
//http://www.shikadi.net/moddingwiki/OPL_chip

namespace zanac.MAmidiMEmo.Instruments.Chips
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    [InstLock]
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
        [Category("Chip(Global)")]
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
        [Category("Chip(Global)")]
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
        public override TimbreBase[] BaseTimbres
        {
            get
            {
                return Timbres;
            }
            set
            {
                Timbres = (YM3812Timbre[])value;
            }
        }

        [DataMember]
        [Category(" Timbres")]
        [Description("Timbres")]
        [EditorAttribute(typeof(YM3812UITypeEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(ExpandableCollectionConverter))]
        public YM3812Timbre[] Timbres
        {
            get;
            set;
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
                    this.InjectFrom(new LoopInjection(new[] { "SerializeData", "SerializeDataSave", "SerializeDataLoad"}), obj);
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
            //Channel        1   2   3   4   5   6   7   8   9
            //Operator 1    00  01  02  08  09  0A  10  11  12
            //Operator 2    03  04  05  0B  0C  0D  13  14  15

            DeferredWriteData(YM3812_write, unitNumber, (uint)0, (byte)(address + (op * 3) + addressTable[slot]));
            DeferredWriteData(YM3812_write, unitNumber, (uint)1, data);
            /*
            try
            {

                Program.SoundUpdating();
                YM3812_write(unitNumber, 0, (byte)(address + (op * 3) + addressTable[slot]));
                YM3812_write(unitNumber, 1, data);
            }
            finally
            {
                Program.SoundUpdated();
            }*/
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

            Timbres = new YM3812Timbre[InstrumentBase.DEFAULT_MAX_TIMBRES];
            for (int i = 0; i < InstrumentBase.DEFAULT_MAX_TIMBRES; i++)
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

            initGlobalRegisters();
        }

        private void initGlobalRegisters()
        {
            //WS Enable
            YM3812WriteData(UnitNumber, (byte)0x01, 0, 0, (byte)0x20);

            YM3812WriteData(UnitNumber, 0xBD, 0, 0, (byte)(AMD << 7 | VIB << 6));
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected override void OnControlChangeEvent(ControlChangeEvent midiEvent)
        {
            base.OnControlChangeEvent(midiEvent);

            soundManager.ProcessControlChange(midiEvent);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataMsb"></param>
        /// <param name="dataLsb"></param>
        protected override void OnNrpnDataEntered(ControlChangeEvent dataMsb, ControlChangeEvent dataLsb)
        {
            base.OnNrpnDataEntered(dataMsb, dataLsb);

            soundManager.ProcessNrpnData(dataMsb, dataLsb);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="caft"></param>
        protected override void OnChannelAfterTouchEvent(ChannelAftertouchEvent caft)
        {
            base.OnChannelAfterTouchEvent(caft);

            soundManager.ProcessChannelAftertouch(caft);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected override void OnPitchBendEvent(PitchBendEvent midiEvent)
        {
            base.OnPitchBendEvent(midiEvent);

            soundManager.ProcessPitchBend(midiEvent);
        }

        internal override void AllSoundOff()
        {
            soundManager.ProcessAllSoundOff();
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void ClearWrittenDataCache()
        {
            base.ClearWrittenDataCache();
            initGlobalRegisters();
        }

        /// <summary>
        /// 
        /// </summary>
        private class YM3812SoundManager : SoundManagerBase
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

            private static SoundList<YM3812Sound> fmOnSounds = new SoundList<YM3812Sound>(9);

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
            public override SoundBase[] SoundOn(TaggedNoteOnEvent note)
            {
                List<SoundBase> rv = new List<SoundBase>();

                int tindex = 0;
                foreach (YM3812Timbre timbre in parentModule.GetBaseTimbres(note))
                {
                    tindex++;
                    var emptySlot = searchEmptySlot(note);
                    if (emptySlot.slot < 0)
                        continue;

                    YM3812Sound snd = new YM3812Sound(emptySlot.inst, this, timbre, tindex - 1, note, emptySlot.slot);
                    fmOnSounds.Add(snd);

                    FormMain.OutputDebugLog(parentModule, "KeyOn FM ch" + emptySlot + " " + note.ToString());
                    rv.Add(snd);
                }
                for (int i = 0; i < rv.Count; i++)
                {
                    var snd = rv[i];
                    if (!snd.IsDisposed)
                    {
                        ProcessKeyOn(snd);
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
            private (YM3812 inst, int slot) searchEmptySlot(TaggedNoteOnEvent note)
            {
                return SearchEmptySlotAndOffForLeader(parentModule, fmOnSounds, note, 9);
            }

            internal override void ProcessAllSoundOff()
            {
                var me = new ControlChangeEvent((SevenBitNumber)120, (SevenBitNumber)0);
                ProcessControlChange(me);

                for (int i = 0; i < 9; i++)
                {
                    YM3812WriteData(parentModule.UnitNumber, (byte)(0xB0 + i), 0, 0, (byte)(0));

                    for (int op = 0; op < 2; op++)
                        YM3812WriteData(parentModule.UnitNumber, 0x40, op, i, 64);
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private class YM3812Sound : SoundBase
        {

            private YM3812 parentModule;

            private YM3812Timbre timbre;

            private byte lastFreqData;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentModule"></param>
            /// <param name="noteOnEvent"></param>
            /// <param name="programNumber"></param>
            /// <param name="slot"></param>
            public YM3812Sound(YM3812 parentModule, YM3812SoundManager manager, TimbreBase timbre, int tindex, TaggedNoteOnEvent noteOnEvent, int slot) : base(parentModule, manager, timbre, tindex, noteOnEvent, slot)
            {
                this.parentModule = parentModule;
                this.timbre = (YM3812Timbre)timbre;
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
                    if (gs.AMD.HasValue)
                        parentModule.AMD = gs.AMD.Value;
                    if (gs.VIB.HasValue)
                        parentModule.VIB = gs.VIB.Value;
                }

                SetTimbre();
                //Volume
                OnVolumeUpdated();
                //Freq
                OnPitchUpdated();
            }

            public override void OnSoundParamsUpdated()
            {

                var gs = timbre.GlobalSettings;
                if (gs.Enable)
                {
                    if (gs.AMD.HasValue)
                        parentModule.AMD = gs.AMD.Value;
                    if (gs.VIB.HasValue)
                        parentModule.VIB = gs.VIB.Value;
                }

                SetTimbre();

                base.OnSoundParamsUpdated();
            }

            /// <summary>
            /// 
            /// </summary>
            public override void OnVolumeUpdated()
            {
                var v = CalcCurrentVolume();
                int velo = 1 + timbre.MDS.VelocitySensitivity;
                for (int op = 0; op < 2; op++)
                {
                    YM3812Operator o = timbre.Ops[op];
                    //$40+: Scaling level/ total level
                    if (timbre.ALG == 1 || op == 1)
                        YM3812WriteData(parentModule.UnitNumber, 0x40, op, Slot, (byte)(o.KSL << 6 | ((63 * 2 / velo) - (byte)Math.Round(((63 * 2 / velo) - (o.TL * 2 / velo)) * v))));
                    else
                    {
                        if (!timbre.UseExprForModulator)
                            YM3812WriteData(parentModule.UnitNumber, 0x40, op, Slot, (byte)(o.KSL << 6 | o.TL));
                        else
                        {
                            var mul = CalcModulatorMultiply();
                            double vol = timbre.Ops[op].TL;
                            if (mul > 0)
                                vol = vol + ((63 - vol) * mul);
                            else if (mul < 0)
                                vol = vol + ((vol) * mul);
                            vol = Math.Round(vol);
                            YM3812WriteData(parentModule.UnitNumber, 0x40, op, Slot, (byte)(o.KSL << 6 | (byte)vol));
                        }
                    }
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="slot"></param>
            public override void OnPitchUpdated()
            {
                double d = CalcCurrentPitchDeltaNoteNumber();

                int nn = NoteOnEvent.NoteNumber;
                if (ParentModule.ChannelTypes[NoteOnEvent.Channel] == ChannelType.Drum)
                    nn = (int)ParentModule.DrumTimbres[NoteOnEvent.NoteNumber].BaseNote;

                int noteNum = nn + (int)d;
                if (noteNum > 127)
                    noteNum = 127;
                else if (noteNum < 0)
                    noteNum = 0;
                var nnOn = new TaggedNoteOnEvent((SevenBitNumber)noteNum, (SevenBitNumber)127);
                int freq = convertFmFrequency(nnOn);
                var octave = nnOn.GetNoteOctave();

                if (d != 0)
                    freq += (ushort)(((double)(convertFmFrequency(nnOn, (d < 0) ? false : true) - freq)) * Math.Abs(d - Math.Truncate(d)));

                if (octave < 0)
                {
                    freq /= 2 * -octave;
                    octave = 0;
                }
                else if (octave > 7)
                {
                    freq *= 2 * (octave - 7);
                    if (freq > 0x3ff)
                        freq = 0x3ff;
                    octave = 7;
                }

                octave = octave << 2;

                //keyon
                byte kon = IsKeyOff ? (byte)0 : (byte)0x20;
                lastFreqData = (byte)(kon | octave | ((freq >> 8) & 3));

                YM3812WriteData(parentModule.UnitNumber, (byte)(0xa0 + Slot), 0, 0, (byte)(0xff & freq));
                YM3812WriteData(parentModule.UnitNumber, (byte)(0xb0 + Slot), 0, 0, lastFreqData);

                base.OnPitchUpdated();
            }

            /// <summary>
            /// 
            /// </summary>
            public void SetTimbre()
            {
                for (int op = 0; op < 2; op++)
                {
                    YM3812Operator o = timbre.Ops[op];
                    //$20+: Amplitude Modulation / Vibrato / Envelope Generator Type / Keyboard Scaling Rate / Modulator Frequency Multiple
                    YM3812WriteData(parentModule.UnitNumber, 0x20, op, Slot, (byte)((o.AM << 7 | o.VR << 6 | o.EG << 5 | o.KSR << 4 | o.MFM)));
                    //$40+: Scaling level/ total level
                    if (!(timbre.ALG == 1 || op == 1))
                    {
                        if (!timbre.UseExprForModulator)
                            YM3812WriteData(parentModule.UnitNumber, 0x40, op, Slot, (byte)(o.KSL << 6 | o.TL));
                    }
                    //$60+: Attack Rate / Decay Rate
                    YM3812WriteData(parentModule.UnitNumber, 0x60, op, Slot, (byte)(o.AR << 4 | o.DR));
                    //$80+: Sustain Level / Release Rate
                    if (o.SR.HasValue && o.EG == 0)
                        YM3812WriteData(parentModule.UnitNumber, 0x80, op, Slot, (byte)(o.SL << 4 | o.SR.Value));
                    else
                        YM3812WriteData(parentModule.UnitNumber, 0x80, op, Slot, (byte)(o.SL << 4 | o.RR));

                    //$e0+: Waveform Select
                    YM3812WriteData(parentModule.UnitNumber, 0xe0, op, Slot, (byte)(o.WS));
                }

                //$C0+: algorithm and feedback
                YM3812WriteData(parentModule.UnitNumber, (byte)(0xc0 + Slot), 0, 0, (byte)(timbre.FB << 1 | timbre.ALG));
            }

            /// <summary>
            /// 
            /// </summary>
            public override void SoundOff()
            {
                base.SoundOff();

                for (int op = 0; op < 2; op++)
                {
                    YM3812Operator o = timbre.Ops[op];
                    if (o.SR.HasValue && o.EG == 0)
                    {
                        YM3812WriteData(parentModule.UnitNumber, 0x20, op, Slot, (byte)((o.AM << 7 | o.VR << 6 | 1 << 5 | o.KSR << 4 | o.MFM)));
                        YM3812WriteData(parentModule.UnitNumber, 0x80, op, Slot, (byte)(o.SL << 4 | o.RR));
                    }
                }

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
            private ushort convertFmFrequency(TaggedNoteOnEvent note)
            {
                return freqTable[(int)note.GetNoteName() + 1];
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
        [InstLock]
        public class YM3812Timbre : TimbreBase
        {
            [Browsable(false)]
            public override bool AssignMIDIChtoSlotNum
            {
                get;
                set;
            }

            [Browsable(false)]
            public override int AssignMIDIChtoSlotNumOffset
            {
                get;
                set;
            }

            #region FM Synth

            /// <summary>
            /// 
            /// </summary>
            /// <param name="inst"></param>
            /// <param name="timbre"></param>
            public override bool CanOpenTimbreEditor(InstrumentBase inst)
            {
                return true;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="inst"></param>
            /// <param name="timbre"></param>
            public override void OpenTimbreEditor(InstrumentBase inst)
            {
                PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties(this);
                PropertyDescriptor pd = pdc["Detailed"];
                UITypeEditor ed = (UITypeEditor)pd.GetEditor(typeof(UITypeEditor));
                RuntimeServiceProvider serviceProvider = new RuntimeServiceProvider(null, this, pd);
                ed.EditValue(serviceProvider, serviceProvider, Detailed);

                //using (var f = new FormYM3812Editor((YM3812)inst, this, true))
                //{
                //    var sd = SerializeData;
                //    var r = f.ShowDialog();
                //    if (r != System.Windows.Forms.DialogResult.OK)
                //        SerializeData = sd;
                //}
            }

            public virtual bool ShouldSerializeDetailed()
            {
                return false;
            }

            [Category("Sound")]
            [Editor(typeof(YM3812UITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [IgnoreDataMember]
            [JsonIgnore]
            [DisplayName("(FM resisters)")]
            [Description("Open FM register editor.")]
            [TypeConverter(typeof(OpenEditorTypeConverter))]
            public string Detailed
            {
                get
                {
                    return SimpleSerializer.SerializeProps(this,
                        nameof(ALG),
                        nameof(FB),

                        "GlobalSettings.EN",
                        "GlobalSettings.AMD",
                        "GlobalSettings.VIB",

                        "Ops[0].AR",
                        "Ops[0].DR",
                        "Ops[0].RR",
                        "Ops[0].SL",
                        "Ops[0].SR",
                        "Ops[0].TL",
                        "Ops[0].KSL",
                        "Ops[0].KSR",
                        "Ops[0].MFM",
                        "Ops[0].AM",
                        "Ops[0].VR",
                        "Ops[0].EG",
                        "Ops[0].WS",

                        "Ops[1].AR",
                        "Ops[1].DR",
                        "Ops[1].RR",
                        "Ops[1].SL",
                        "Ops[1].SR",
                        "Ops[1].TL",
                        "Ops[1].KSL",
                        "Ops[1].KSR",
                        "Ops[1].MFM",
                        "Ops[1].AM",
                        "Ops[1].VR",
                        "Ops[1].EG",
                        "Ops[1].WS");
                }
                set
                {
                    SimpleSerializer.DeserializeProps(this, value,
                        nameof(ALG),
                        nameof(FB),

                        "GlobalSettings.EN",
                        "GlobalSettings.AMD",
                        "GlobalSettings.VIB",

                        "Ops[0].AR",
                        "Ops[0].DR",
                        "Ops[0].RR",
                        "Ops[0].SL",
                        "Ops[0].SR",
                        "Ops[0].TL",
                        "Ops[0].KSL",
                        "Ops[0].KSR",
                        "Ops[0].MFM",
                        "Ops[0].AM",
                        "Ops[0].VR",
                        "Ops[0].EG",
                        "Ops[0].WS",

                        "Ops[1].AR",
                        "Ops[1].DR",
                        "Ops[1].RR",
                        "Ops[1].SL",
                        "Ops[1].SR",
                        "Ops[1].TL",
                        "Ops[1].KSL",
                        "Ops[1].KSR",
                        "Ops[1].MFM",
                        "Ops[1].AM",
                        "Ops[1].VR",
                        "Ops[1].EG",
                        "Ops[1].WS");
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

            #endregion

            /// <summary>
            /// 
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Operators")]
            [TypeConverter(typeof(ExpandableCollectionConverter))]
            [DisplayName("Operators[Ops]")]
            [EditorAttribute(typeof(DummyEditor), typeof(UITypeEditor))]
            public YM3812Operator[] Ops
            {
                get;
                set;
            }

            public virtual bool ShouldSerializeOps()
            {
                foreach (var op in Ops)
                {
                    if (!string.Equals(JsonConvert.SerializeObject(op, Formatting.Indented), "{}"))
                        return true;
                }
                return false;
            }

            public virtual void ResetOps()
            {
                var ops = new YM3812Operator[] {
                    new YM3812Operator(),
                    new YM3812Operator() };
                for (int i = 0; i < Ops.Length; i++)
                    Ops[i].InjectFrom(new LoopInjection(), ops[i]);
            }

            [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                typeof(UITypeEditor)), Localizable(false)]
            [IgnoreDataMember]
            [JsonIgnore]
            [Category("Sound")]
            [Description("You can copy and paste this text data to other same type timber.\r\n" +
                "ALG, FB, AR, DR, RR, SL, TL, KSL, KSR, MFM, AM(AMS), VR, EG(EGT), WS, ...\r\n" +
                "You can use comma or space chars as delimiter.")]
            public string MmlSerializeData
            {
                get
                {
                    return SimpleSerializer.SerializeProps(this,
                        nameof(ALG),
                        nameof(FB),

                        "Ops[0].AR",
                        "Ops[0].DR",
                        "Ops[0].RR",
                        "Ops[0].SL",
                        "Ops[0].SR",
                        "Ops[0].TL",
                        "Ops[0].KSL",
                        "Ops[0].KSR",
                        "Ops[0].MFM",
                        "Ops[0].AM",
                        "Ops[0].VR",
                        "Ops[0].EG",
                        "Ops[0].WS",

                        "Ops[1].AR",
                        "Ops[1].DR",
                        "Ops[1].RR",
                        "Ops[1].SL",
                        "Ops[1].SR",
                        "Ops[1].TL",
                        "Ops[1].KSL",
                        "Ops[1].KSR",
                        "Ops[1].MFM",
                        "Ops[1].AM",
                        "Ops[1].VR",
                        "Ops[1].EG",
                        "Ops[1].WS");
                }
                set
                {
                    SimpleSerializer.DeserializeProps(this, value,
                        nameof(ALG),
                        nameof(FB),

                        "Ops[0].AR",
                        "Ops[0].DR",
                        "Ops[0].RR",
                        "Ops[0].SL",
                        "Ops[0].SR",
                        "Ops[0].TL",
                        "Ops[0].KSL",
                        "Ops[0].KSR",
                        "Ops[0].MFM",
                        "Ops[0].AM",
                        "Ops[0].VR",
                        "Ops[0].EG",
                        "Ops[0].WS",

                        "Ops[1].AR",
                        "Ops[1].DR",
                        "Ops[1].RR",
                        "Ops[1].SL",
                        "Ops[1].SR",
                        "Ops[1].TL",
                        "Ops[1].KSL",
                        "Ops[1].KSR",
                        "Ops[1].MFM",
                        "Ops[1].AM",
                        "Ops[1].VR",
                        "Ops[1].EG",
                        "Ops[1].WS");
                }
            }

            /// <summary>
            /// 
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [DefaultValue(false)]
            [Description("Use MIDI Expresion for Modulator Total Level.")]
            [Browsable(true)]
            public override bool UseExprForModulator
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

            public virtual bool ShouldSerializeGlobalSettings()
            {
                return !string.Equals(JsonConvert.SerializeObject(GlobalSettings, Formatting.Indented), "{}", StringComparison.Ordinal);
            }

            public virtual void ResetGlobalSettings()
            {
                GlobalSettings.InjectFrom(new LoopInjection(), new YM3812GlobalSettings());
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
            }

            public override void RestoreFrom(string serializeData)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<YM3812Timbre>(serializeData);
                    this.InjectFrom(new LoopInjection(new[] { "SerializeData", "SerializeDataSave", "SerializeDataLoad"}), obj);
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
        [InstLock]
        public class YM3812Operator : ContextBoundObject, ISerializeDataSaveLoad
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
            [DefaultValue(null)]
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
                "AR, DR, RR, SL, TL, KSL, KSR, MFM, AM(AMS), VR, EG(EGT), WS\r\n" +
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
                        nameof(TL),
                        nameof(KSL),
                        nameof(KSR),
                        nameof(MFM),
                        nameof(AM),
                        nameof(VR),
                        nameof(EG),
                        nameof(WS));
                }
                set
                {
                    SimpleSerializer.DeserializeProps(this, value,
                        nameof(AR),
                        nameof(DR),
                        nameof(RR),
                        nameof(SL),
                        nameof(TL),
                        nameof(KSL),
                        nameof(KSR),
                        nameof(MFM),
                        nameof(AM),
                        nameof(VR),
                        nameof(EG),
                        nameof(WS));
                }
            }

            [Editor(typeof(SerializeSaveUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [IgnoreDataMember]
            [JsonIgnore]
            [DisplayName("(Save settings)")]
            [Description("Save all parameters as serialize data to the file.")]
            [TypeConverter(typeof(OpenFileBrowserTypeConverter))]
            public string SerializeDataSave
            {
                get
                {
                    return SerializeData;
                }
                set
                {
                    SerializeData = value;
                }
            }


            [Editor(typeof(SerializeLoadUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [IgnoreDataMember]
            [JsonIgnore]
            [DisplayName("(Load settings)")]
            [Description("Load all parameters as serialize data from the file.")]
            [TypeConverter(typeof(OpenFileBrowserTypeConverter))]
            public string SerializeDataLoad
            {
                get
                {
                    return SerializeData;
                }
                set
                {
                    SerializeData = value;
                }
            }

            public virtual bool ShouldSerializeSerializeDataSave()
            {
                return false;
            }

            public virtual bool ShouldSerializeSerializeDataLoad()
            {
                return false;
            }

            [Browsable(false)]
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
                    this.InjectFrom(new LoopInjection(new[] { "SerializeData", "SerializeDataSave", "SerializeDataLoad"}), obj);
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
        [InstLock]
        public class YM3812GlobalSettings : ContextBoundObject
        {
            [DataMember]
            [Category("Chip")]
            [Description("Override global settings")]
            [DefaultValue(false)]
            public bool Enable
            {
                get;
                set;
            }

            [IgnoreDataMember]
            [JsonIgnore]
            [Browsable(false)]
            public byte EN
            {
                get
                {
                    return Enable ? (byte)1 : (byte)0;
                }
                set
                {
                    Enable = value == 0 ? false : true;
                }
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

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        [IgnoreDataMember]
        [JsonIgnore]
        public override bool CanImportToneFile
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timbre"></param>
        /// <param name="tone"></param>
        public override void ImportToneFile(TimbreBase timbre, Tone tone)
        {
            YM3812Timbre tim = (YM3812Timbre)timbre;

            bool native = false;
            if (tone.CNT == -1)
            {
                tim.ALG = (byte)tone.AL;
            }
            else
            {
                tim.ALG = (byte)tone.CNT;
                native = true;
            }

            tim.FB = (byte)tone.FB;
            tim.GlobalSettings.Enable = false;
            tim.GlobalSettings.AMD = null;
            tim.GlobalSettings.VIB = null;

            for (int i = 0; i < 2; i++)
            {
                if (!native)
                {
                    tim.Ops[i].AR = (byte)(tone.aOp[i].AR / 2);
                    tim.Ops[i].DR = (byte)(tone.aOp[i].DR / 2);
                    tim.Ops[i].SR = (byte)(tone.aOp[i].SR / 2);
                    tim.Ops[i].TL = (byte)(tone.aOp[i].TL / 2);
                }
                else
                {
                    tim.Ops[i].AR = (byte)(tone.aOp[i].AR);
                    tim.Ops[i].DR = (byte)(tone.aOp[i].DR);
                    tim.Ops[i].SR = null;
                    tim.Ops[i].TL = (byte)(tone.aOp[i].TL);
                }
                tim.Ops[i].RR = (byte)tone.aOp[i].RR;
                tim.Ops[i].SL = (byte)tone.aOp[i].SL;
                tim.Ops[i].KSL = (byte)tone.aOp[i].KS;
                tim.Ops[i].KSR = 0;
                tim.Ops[i].MFM = (byte)tone.aOp[i].ML;
                tim.Ops[i].AM = (byte)tone.aOp[i].AM;
                tim.Ops[i].VR = 0;
                tim.Ops[i].EG = 0;
                tim.Ops[i].WS = 0;
            }
            timbre.TimbreName = tone.Name;
        }
    }
}