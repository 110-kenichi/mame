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

//http://www.smspower.org/Development/SN76489
//http://www.st.rim.or.jp/~nkomatsu/peripheral/SN76489.html

namespace zanac.MAmidiMEmo.Instruments.Chips
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class SN76496 : InstrumentBase
    {

        public override string Name => "SN76496";

        public override string Group => "PSG";

        public override InstrumentType InstrumentType => InstrumentType.SN76496;

        [Browsable(false)]
        public override string ImageKey => "SN76496";

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        protected override string SoundInterfaceTagNamePrefix => "sn76496_";

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
                return 3;
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
        [Category("Chip")]
        [Description("Timbres (0-127)")]
        [EditorAttribute(typeof(DummyEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(ExpandableCollectionConverter))]
        public SN76496Timbre[] Timbres
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
                using (var obj = JsonConvert.DeserializeObject<SN76496>(serializeData))
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
        private delegate void delegate_sn76496_write(uint unitNumber, byte data);


        /// <summary>
        /// 
        /// </summary>
        private static delegate_sn76496_write Sn76496_write
        {
            get;
            set;
        }


        /// <summary>
        /// 
        /// </summary>
        private static void Sn76496WriteData(uint unitNumber, byte data)
        {
            DeferredWriteData(Sn76496_write, unitNumber, data);
            /*
            try
            {
                Program.SoundUpdating();
                Sn76496_write(unitNumber, data);
            }
            finally
            {
                Program.SoundUpdated();
            }*/
        }

        /// <summary>
        /// 
        /// </summary>
        static SN76496()
        {
            IntPtr funcPtr = MameIF.GetProcAddress("sn76496_write");
            if (funcPtr != IntPtr.Zero)
            {
                Sn76496_write = (delegate_sn76496_write)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_sn76496_write));
            }
        }

        private SN76496SoundManager soundManager;

        private const float DEFAULT_GAIN = 1.1f;

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
        public SN76496(uint unitNumber) : base(unitNumber)
        {
            GainLeft = DEFAULT_GAIN;
            GainRight = DEFAULT_GAIN;

            Timbres = new SN76496Timbre[InstrumentBase.MAX_TIMBRES];
            for (int i = 0; i < InstrumentBase.MAX_TIMBRES; i++)
                Timbres[i] = new SN76496Timbre();
            setPresetInstruments();

            this.soundManager = new SN76496SoundManager(this);
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
            Timbres[0].SoundType = SoundType.PSG;
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
        private class SN76496SoundManager : SoundManagerBase
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

            private static SoundList<SN76496Sound> psgOnSounds = new SoundList<SN76496Sound>(3);

            private static SoundList<SN76496Sound> noiseOnSounds = new SoundList<SN76496Sound>(1);

            private SN76496 parentModule;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            public SN76496SoundManager(SN76496 parent) : base(parent)
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

                foreach (SN76496Timbre timbre in parentModule.GetBaseTimbres(note))
                {
                    var emptySlot = searchEmptySlot(note, timbre);
                    if (emptySlot.slot < 0)
                        continue;

                    SN76496Sound snd = new SN76496Sound(emptySlot.inst, this, timbre, note, emptySlot.slot);
                    switch (((SN76496Timbre)timbre).SoundType)
                    {
                        case SoundType.PSG:
                            psgOnSounds.Add(snd);
                            FormMain.OutputDebugLog("KeyOn PSG ch" + emptySlot + " " + note.ToString());
                            break;
                        case SoundType.NOISE:
                            noiseOnSounds.Add(snd);
                            FormMain.OutputDebugLog("KeyOn NOISE ch" + emptySlot + " " + note.ToString());
                            break;
                    }
                    snd.KeyOn();
                    rv.Add(snd);
                }

                return rv.ToArray();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            private (SN76496 inst, int slot) searchEmptySlot(TaggedNoteOnEvent note, SN76496Timbre timbre)
            {
                var emptySlot = (parentModule, -1);

                switch (timbre.SoundType)
                {
                    case SoundType.PSG:
                        {
                            emptySlot = SearchEmptySlotAndOffForLeader(parentModule, psgOnSounds, note, 3);
                            break;
                        }
                    case SoundType.NOISE:
                        {
                            emptySlot = SearchEmptySlotAndOffForLeader(parentModule, noiseOnSounds, note, 1);
                            break;
                        }
                }

                return emptySlot;
            }

            internal override void AllSoundOff()
            {
                var me = new ControlChangeEvent((SevenBitNumber)120, (SevenBitNumber)0);
                ControlChange(me);

                for (int i = 0; i < 3; i++)
                    Sn76496WriteData(parentModule.UnitNumber, (byte)(0x80 | i << 5 | 0x1f));
                Sn76496WriteData(parentModule.UnitNumber, (byte)(0x80 | 3 << 5 | 0x1f));
            }

        }


        /// <summary>
        /// 
        /// </summary>
        private class SN76496Sound : SoundBase
        {

            private SN76496 parentModule;

            private SN76496Timbre timbre;

            private SoundType lastSoundType;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentModule"></param>
            /// <param name="noteOnEvent"></param>
            /// <param name="programNumber"></param>
            /// <param name="slot"></param>
            public SN76496Sound(SN76496 parentModule, SN76496SoundManager manager, TimbreBase timbre, TaggedNoteOnEvent noteOnEvent, int slot) : base(parentModule, manager, timbre, noteOnEvent, slot)
            {
                this.parentModule = parentModule;
                this.timbre = (SN76496Timbre)timbre;

                lastSoundType = this.timbre.SoundType;
            }

            /// <summary>
            /// 
            /// </summary>
            public override void KeyOn()
            {
                base.KeyOn();

                OnVolumeUpdated();

                OnPitchUpdated();
            }

            public override void OnSoundParamsUpdated()
            {
                base.OnSoundParamsUpdated();

                OnVolumeUpdated();

                OnPitchUpdated();
            }

            /// <summary>
            /// 
            /// </summary>
            public override void OnVolumeUpdated()
            {
                if (IsSoundOff)
                    return;

                switch (lastSoundType)
                {
                    case SoundType.PSG:
                        updatePsgVolume();
                        break;
                    case SoundType.NOISE:
                        updateNoiseVolume();
                        break;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            private void updatePsgVolume()
            {
                if (IsSoundOff)
                    return;
                byte fv = (byte)((14 - (int)Math.Round(14 * CalcCurrentVolume())) & 0xf);

                Sn76496WriteData(parentModule.UnitNumber, (byte)(0x80 | Slot << 5 | 0x10 | fv));
            }

            /// <summary>
            /// 
            /// </summary>
            private void updateNoiseVolume()
            {
                if (IsSoundOff)
                    return;

                byte fv = (byte)((14 - (int)Math.Round(14 * CalcCurrentVolume())) & 0xf);

                //var exp = parentModule.Expressions[NoteOnEvent.Channel] / 127d;
                //var vol = parentModule.Volumes[NoteOnEvent.Channel] / 127d;
                //var vel = NoteOnEvent.Velocity / 127d;

                //byte fv = (byte)((14 - (int)Math.Round(14 * vol * vel * exp)) & 0xf);

                Sn76496WriteData(parentModule.UnitNumber, (byte)(0x80 | (Slot + 3) << 5 | 0x10 | fv));
            }

            /// <summary>
            /// 
            /// </summary>
            public override void OnPitchUpdated()
            {
                switch (lastSoundType)
                {
                    case SoundType.PSG:
                        updatePsgPitch();
                        break;
                    case SoundType.NOISE:
                        updateNoisePitch();
                        break;
                }

                base.OnPitchUpdated();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="slot"></param>
            private void updatePsgPitch()
            {
                double freq = CalcCurrentFrequency();
                freq = Math.Round(3579545 / (freq * 32));
                if (freq > 0x3ff)
                    freq = 0x3ff;
                var n = (ushort)freq;
                Sn76496WriteData(parentModule.UnitNumber, (byte)(0x80 | Slot << 5 | n & 0xf));
                Sn76496WriteData(parentModule.UnitNumber, (byte)((n >> 4) & 0x3f));
            }


            /// <summary>
            /// 
            /// </summary>
            /// <param name="slot"></param>
            private void updateNoisePitch()
            {
                int nn = NoteOnEvent.NoteNumber;
                if (ParentModule.ChannelTypes[NoteOnEvent.Channel] == ChannelType.Drum)
                    nn = (int)ParentModule.DrumTimbres[NoteOnEvent.NoteNumber].BaseNote;

                int v = nn % 3;

                Sn76496WriteData(parentModule.UnitNumber, (byte)(0x80 | (Slot + 3) << 5 | timbre.FB << 2 | v));
            }


            /// <summary>
            /// 
            /// </summary>
            public override void SoundOff()
            {
                base.SoundOff();

                switch (lastSoundType)
                {
                    case SoundType.PSG:
                        {
                            Sn76496WriteData(parentModule.UnitNumber, (byte)(0x80 | Slot << 5 | 0x1f));
                            break;
                        }
                    case SoundType.NOISE:
                        {
                            Sn76496WriteData(parentModule.UnitNumber, (byte)(0x80 | (Slot + 3) << 5 | 0x1f));
                            break;
                        }
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        [JsonConverter(typeof(NoTypeConverterJsonConverter<SN76496Timbre>))]
        [DataContract]
        public class SN76496Timbre : TimbreBase
        {
            [DataMember]
            [Category("Sound")]
            [Description("Sound Type")]
            [DefaultValue(SoundType.PSG)]
            public SoundType SoundType
            {
                get;
                set;
            }

            private byte f_FB;

            [DataMember]
            [Category("Sound")]
            [Description("Feedback (0-1)")]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue((byte)0)]
            public byte FB
            {
                get
                {
                    return f_FB;
                }
                set
                {
                    f_FB = (byte)(value & 1);
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public SN76496Timbre()
            {
                this.SDS.FxS = new BasicFxSettings();
            }

            public override void RestoreFrom(string serializeData)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<SN76496Timbre>(serializeData);
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
        public enum SoundType
        {
            PSG,
            NOISE,
        }

    }
}