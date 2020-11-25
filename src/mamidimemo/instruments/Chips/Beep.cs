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
    public class Beep : InstrumentBase
    {

        public override string Name => "Beep";

        public override string Group => "PSG";

        public override InstrumentType InstrumentType => InstrumentType.Beep;

        [Browsable(false)]
        public override string ImageKey => "Beep";

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        protected override string SoundInterfaceTagNamePrefix => "beep_";

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
                return 14;
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
        public BeepTimbre[] Timbres
        {
            get;
            set;
        }


        private const float DEFAULT_GAIN = 0.1f;

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
                using (var obj = JsonConvert.DeserializeObject<Beep>(serializeData))
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
        private delegate void delegate_beep_set_clock(uint unitNumber, int state, uint frequency);


        /// <summary>
        /// 
        /// </summary>
        private static delegate_beep_set_clock Beep_set_clock
        {
            get;
            set;
        }


        /// <summary>
        /// 
        /// </summary>
        private static void BeepSetClock(uint unitNumber, int state, uint frequency)
        {
            DeferredWriteData(Beep_set_clock, unitNumber, state, frequency);
            /*
            try
            {
                Program.SoundUpdating();
                Beep_set_clock(unitNumber, state, frequency);
            }
            finally
            {
                Program.SoundUpdated();
            }*/
        }

        /// <summary>
        /// 
        /// </summary>
        static Beep()
        {
            IntPtr funcPtr = MameIF.GetProcAddress("beep_set_clock");
            if (funcPtr != IntPtr.Zero)
            {
                Beep_set_clock = (delegate_beep_set_clock)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_beep_set_clock));
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

        private BeepSoundManager soundManager;

        /// <summary>
        /// 
        /// </summary>
        public Beep(uint unitNumber) : base(unitNumber)
        {
            GainLeft = DEFAULT_GAIN;
            GainRight = DEFAULT_GAIN;

            Timbres = new BeepTimbre[InstrumentBase.DEFAULT_MAX_TIMBRES];
            for (int i = 0; i < InstrumentBase.DEFAULT_MAX_TIMBRES; i++)
                Timbres[i] = new BeepTimbre();
            setPresetInstruments();

            this.soundManager = new BeepSoundManager(this);
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
        private class BeepSoundManager : SoundManagerBase
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

            private static SoundList<BeepSound> psgOnSounds = new SoundList<BeepSound>(1);

            private Beep parentModule;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            public BeepSoundManager(Beep parent) : base(parent)
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

                foreach (BeepTimbre timbre in parentModule.GetBaseTimbres(note))
                {
                    var emptySlot = searchEmptySlot(note);
                    if (emptySlot.slot < 0)
                        continue;

                    BeepSound snd = new BeepSound(emptySlot.inst, this, timbre, note, emptySlot.slot);
                    psgOnSounds.Add(snd);

                    FormMain.OutputDebugLog(parentModule, "KeyOn PSG ch" + emptySlot + " " + note.ToString());
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
            private (Beep inst, int slot) searchEmptySlot(TaggedNoteOnEvent note)
            {
                return SearchEmptySlotAndOffForLeader(parentModule, psgOnSounds, note, 1);
            }

            internal override void AllSoundOff()
            {
                var me = new ControlChangeEvent((SevenBitNumber)120, (SevenBitNumber)0);
                ControlChange(me);

                BeepSetClock(parentModule.UnitNumber, 0, 0);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private class BeepSound : SoundBase
        {

            private Beep parentModule;

            private BeepTimbre timbre;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentModule"></param>
            /// <param name="noteOnEvent"></param>
            /// <param name="programNumber"></param>
            /// <param name="slot"></param>
            public BeepSound(Beep parentModule, BeepSoundManager manager, TimbreBase timbre, TaggedNoteOnEvent noteOnEvent, int slot) : base(parentModule, manager, timbre, noteOnEvent, slot)
            {
                this.parentModule = parentModule;
                this.timbre = (BeepTimbre)timbre;
            }

            /// <summary>
            /// 
            /// </summary>
            public override void KeyOn()
            {
                base.KeyOn();

                OnPitchUpdated();
            }

            /// <summary>
            /// 
            /// </summary>
            public override void OnPitchUpdated()
            {
                double freq = CalcCurrentFrequency();

                BeepSetClock(parentModule.UnitNumber, IsSoundOff ? 0 : 1, (uint)freq);

                base.OnPitchUpdated();
            }

            /// <summary>
            /// 
            /// </summary>
            public override void SoundOff()
            {
                base.SoundOff();

                BeepSetClock(parentModule.UnitNumber, 0, 0);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        [JsonConverter(typeof(NoTypeConverterJsonConverter<BeepTimbre>))]
        [DataContract]
        public class BeepTimbre : TimbreBase
        {
            /// <summary>
            /// 
            /// </summary>
            public BeepTimbre()
            {
                this.SDS.FxS = new BasicFxSettings();
            }

            public override void RestoreFrom(string serializeData)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<BeepTimbre>(serializeData);
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