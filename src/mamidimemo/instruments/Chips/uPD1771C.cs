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

//http://takeda-toshiya.my.coocan.jp/scv/scv.pdf

namespace zanac.MAmidiMEmo.Instruments.Chips
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    [InstLock]
    public class uPD1771C : InstrumentBase
    {

        public override string Name => "uPD1771C";

        public override string Group => "PSG";

        public override InstrumentType InstrumentType => InstrumentType.uPD1771C;

        [Browsable(false)]
        public override string ImageKey => "uPD1771C";

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        protected override string SoundInterfaceTagNamePrefix => "upd1771_";

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
                return 28;
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
        [DataMember]
        [Category(" Timbres")]
        [Description("Timbres")]
        [EditorAttribute(typeof(DummyEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(ExpandableCollectionConverter))]
        public uPD1771Timbre[] Timbres
        {
            get;
            set;
        }


        private const float DEFAULT_GAIN = 0.7f;

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
        /// <param name="serializeData"></param>
        public override void RestoreFrom(string serializeData)
        {
            try
            {
                using (var obj = JsonConvert.DeserializeObject<uPD1771C>(serializeData))
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
        private delegate void delegate_uPD1771_write(uint unitNumber, byte data);


        /// <summary>
        /// 
        /// </summary>
        private static delegate_uPD1771_write uPD1771_write
        {
            get;
            set;
        }


        /// <summary>
        /// 
        /// </summary>
        private static void uPD1771Write(uint unitNumber, byte data)
        {
            //DeferredWriteData(uPD1771_write, unitNumber, data);
            ///*
            try
            {
                Program.SoundUpdating();
                uPD1771_write(unitNumber, data);
            }
            finally
            {
                Program.SoundUpdated();
            }//*/
        }

        /// <summary>
        /// 
        /// </summary>
        static uPD1771C()
        {
            IntPtr funcPtr = MameIF.GetProcAddress("uPD1771_write");
            if (funcPtr != IntPtr.Zero)
            {
                uPD1771_write = (delegate_uPD1771_write)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_uPD1771_write));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Dispose()
        {
            soundManager?.Dispose();
            base.Dispose();
        }

        private uPD1771SoundManager soundManager;

        /// <summary>
        /// 
        /// </summary>
        public uPD1771C(uint unitNumber) : base(unitNumber)
        {
            GainLeft = DEFAULT_GAIN;
            GainRight = DEFAULT_GAIN;

            Timbres = new uPD1771Timbre[InstrumentBase.DEFAULT_MAX_TIMBRES];
            for (int i = 0; i < InstrumentBase.DEFAULT_MAX_TIMBRES; i++)
                Timbres[i] = new uPD1771Timbre();
            setPresetInstruments();

            this.soundManager = new uPD1771SoundManager(this);
        }

        /// <summary>
        /// 
        /// </summary>
        private void setPresetInstruments()
        {
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
        private class uPD1771SoundManager : SoundManagerBase
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

            private static SoundList<uPD1771Sound> psgOnSounds = new SoundList<uPD1771Sound>(1);

            private uPD1771C parentModule;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            public uPD1771SoundManager(uPD1771C parent) : base(parent)
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
                foreach (uPD1771Timbre timbre in parentModule.GetBaseTimbres(note))
                {
                    tindex++;
                    var emptySlot = searchEmptySlot(note);
                    if (emptySlot.slot < 0)
                        continue;

                    uPD1771Sound snd = new uPD1771Sound(emptySlot.inst, this, timbre, tindex - 1, note, emptySlot.slot);
                    psgOnSounds.Add(snd);

                    FormMain.OutputDebugLog(parentModule, "KeyOn PSG ch" + emptySlot + " " + note.ToString());
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
            private (uPD1771C inst, int slot) searchEmptySlot(TaggedNoteOnEvent note)
            {
                return SearchEmptySlotAndOffForLeader(parentModule, psgOnSounds, note, 1);
            }

            internal override void ProcessAllSoundOff()
            {
                var me = new ControlChangeEvent((SevenBitNumber)120, (SevenBitNumber)0);
                ProcessControlChange(me);

                uPD1771Write(parentModule.UnitNumber, 0);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private class uPD1771Sound : SoundBase
        {

            private uPD1771C parentModule;

            private uPD1771Timbre timbre;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentModule"></param>
            /// <param name="noteOnEvent"></param>
            /// <param name="programNumber"></param>
            /// <param name="slot"></param>
            public uPD1771Sound(uPD1771C parentModule, uPD1771SoundManager manager, TimbreBase timbre, int tindex, TaggedNoteOnEvent noteOnEvent, int slot) : base(parentModule, manager, timbre, tindex, noteOnEvent, slot)
            {
                this.parentModule = parentModule;
                this.timbre = (uPD1771Timbre)timbre;
            }

            /// <summary>
            /// 
            /// </summary>
            public override void KeyOn()
            {
                base.KeyOn();

                byte vol = (byte)(((byte)Math.Round(31 * CalcCurrentVolume()) & 0x1f));

                byte freq = convertFrequency(CalcCurrentFrequency());

                switch (timbre.SoundType)
                {
                    case SoundType.Tone:
                        uPD1771Write(parentModule.UnitNumber, 0x02);

                        uPD1771Write(parentModule.UnitNumber,
                            (byte)(((int)timbre.ToneType << 5) | timbre.Offset));

                        uPD1771Write(parentModule.UnitNumber, freq);

                        uPD1771Write(parentModule.UnitNumber, vol);

                        break;
                    case SoundType.Noise:
                        uPD1771Write(parentModule.UnitNumber, 0x01);

                        uPD1771Write(parentModule.UnitNumber, 0xe0);

                        uPD1771Write(parentModule.UnitNumber, freq);

                        uPD1771Write(parentModule.UnitNumber, vol);

                        uPD1771Write(parentModule.UnitNumber, timbre.NoisePeriod1);
                        uPD1771Write(parentModule.UnitNumber, timbre.NoisePeriod2);
                        uPD1771Write(parentModule.UnitNumber, timbre.NoisePeriod3);

                        uPD1771Write(parentModule.UnitNumber, timbre.NoiseVolume1);
                        uPD1771Write(parentModule.UnitNumber, timbre.NoiseVolume2);
                        uPD1771Write(parentModule.UnitNumber, timbre.NoiseVolume3);

                        break;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="note"></param>
            /// <returns></returns>
            private byte convertFrequency(double freq)
            {
                double f = ((6 * 1000 * 1000 / 4) / (32 * freq));
                if (f > 255d)
                    f = 255d;
                return (byte)Math.Round(f);
            }

            /// <summary>
            /// 
            /// </summary>
            public override void SoundOff()
            {
                base.SoundOff();

                uPD1771Write(parentModule.UnitNumber, 0);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        [JsonConverter(typeof(NoTypeConverterJsonConverter<uPD1771Timbre>))]
        [DataContract]
        public class uPD1771Timbre : TimbreBase
        {

            [DataMember]
            [Category("Sound")]
            [Description("Sound Type")]
            [DefaultValue(SoundType.Tone)]
            public SoundType SoundType
            {
                get;
                set;
            }

            [DataMember]
            [Category("Sound")]
            [Description("Tone Type")]
            [DefaultValue(ToneType.Tone1)]
            public ToneType ToneType
            {
                get;
                set;
            }

            private byte f_Offset;

            [DataMember]
            [Category("Sound")]
            [Description("Set Offset for Tone (0-31)")]
            [SlideParametersAttribute(0, 31)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue((byte)0)]
            public byte Offset
            {
                get
                {
                    return f_Offset;
                }
                set
                {
                    f_Offset = (byte)(value & 31);
                }
            }

            private byte f_NoisePeriod1 = 64;

            [DataMember]
            [Category("Sound")]
            [Description("Set Period for Noise1 (0-255)")]
            [SlideParametersAttribute(0, 255)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue((byte)64)]
            public byte NoisePeriod1
            {
                get
                {
                    return f_NoisePeriod1;
                }
                set
                {
                    f_NoisePeriod1 = (byte)(value & 255);
                }
            }

            private byte f_NoisePeriod2 = 32;

            [DataMember]
            [Category("Sound")]
            [Description("Set Period for Noise2 (0-255)")]
            [SlideParametersAttribute(0, 255)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue((byte)32)]
            public byte NoisePeriod2
            {
                get
                {
                    return f_NoisePeriod2;
                }
                set
                {
                    f_NoisePeriod2 = (byte)(value & 255);
                }
            }


            private byte f_NoisePeriod3 = 8;

            [DataMember]
            [Category("Sound")]
            [Description("Set Period for Noise2 (0-255)")]
            [SlideParametersAttribute(0, 255)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue((byte)8)]
            public byte NoisePeriod3
            {
                get
                {
                    return f_NoisePeriod3;
                }
                set
                {
                    f_NoisePeriod3 = (byte)(value & 255);
                }
            }


            private byte f_NoiseVolume1 = 31;

            [DataMember]
            [Category("Sound")]
            [Description("Set Volume for Noise1 (0-31)")]
            [SlideParametersAttribute(0, 31)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue((byte)31)]
            public byte NoiseVolume1
            {
                get
                {
                    return f_NoiseVolume1;
                }
                set
                {
                    f_NoiseVolume1 = (byte)(value & 0x1f);
                }
            }

            private byte f_NoiseVolume2 = 31;

            [DataMember]
            [Category("Sound")]
            [Description("Set Volume for Noise2 (0-31)")]
            [SlideParametersAttribute(0, 31)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue((byte)31)]
            public byte NoiseVolume2
            {
                get
                {
                    return f_NoiseVolume2;
                }
                set
                {
                    f_NoiseVolume2 = (byte)(value & 0x1f);
                }
            }


            private byte f_NoiseVolume3 = 31;

            [DataMember]
            [Category("Sound")]
            [Description("Set Volume for Noise2 (0-31)")]
            [SlideParametersAttribute(0, 31)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue((byte)31)]
            public byte NoiseVolume3
            {
                get
                {
                    return f_NoiseVolume3;
                }
                set
                {
                    f_NoiseVolume3 = (byte)(value & 0x1f);
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public uPD1771Timbre()
            {
                this.SDS.FxS = new BasicFxSettings();
            }

            public override void RestoreFrom(string serializeData)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<uPD1771Timbre>(serializeData);
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
        [Flags]
        public enum SoundType
        {
            Tone = 0,
            Noise = 1,
        }


        /// <summary>
        /// 
        /// </summary>
        [Flags]
        public enum ToneType
        {
            Tone1,
            Tone2,
            Tone3,
            Tone4,
            Tone5,
            Tone6,
            Tone7,
        }
    }
}