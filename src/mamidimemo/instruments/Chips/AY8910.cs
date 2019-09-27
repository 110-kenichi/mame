﻿// copyright-holders:K.Ito
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
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Smf;
using Newtonsoft.Json;
using Omu.ValueInjecter;
using Omu.ValueInjecter.Injections;
using zanac.MAmidiMEmo.ComponentModel;
using zanac.MAmidiMEmo.Gui;
using zanac.MAmidiMEmo.Instruments.Envelopes;
using zanac.MAmidiMEmo.Mame;
using zanac.MAmidiMEmo.Midi;

//http://ngs.no.coocan.jp/doc/wiki.cgi/TechHan?page=1%BE%CF+PSG%A4%C8%B2%BB%C0%BC%BD%D0%CE%CF
//https://w.atwiki.jp/msx-sdcc/pages/45.html
//http://f.rdw.se/AY-3-8910-datasheet.pdf

namespace zanac.MAmidiMEmo.Instruments.Chips
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class AY8910 : InstrumentBase
    {

        public override string Name => "AY-3-8910";

        public override string Group => "PSG";

        public override InstrumentType InstrumentType => InstrumentType.AY8910;

        [Browsable(false)]
        public override string ImageKey => "AY-3-8910";

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        protected override string SoundInterfaceTagNamePrefix => "ay8910_";

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
                return 11;
            }
        }
        
        private byte f_EnvelopeFrequencyCoarse = 2;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("Chip")]
        [Description("Set Envelope Coarse Frequency")]
        [SlideParametersAttribute(0, 255, true)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((byte)2)]
        public byte EnvelopeFrequencyCoarse
        {
            get => f_EnvelopeFrequencyCoarse;
            set
            {
                if (f_EnvelopeFrequencyCoarse != value)
                {
                    f_EnvelopeFrequencyCoarse = value;
                    Program.SoundUpdating();
                    Ay8910WriteData(UnitNumber, 0, (byte)(12));
                    Ay8910WriteData(UnitNumber, 1, value);
                    Ay8910WriteData(UnitNumber, 0, (byte)(11));
                    Ay8910WriteData(UnitNumber, 1, EnvelopeFrequencyFine);
                    Program.SoundUpdated();
                }
            }
        }

        private byte f_EnvelopeFrequencyFine;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("Chip")]
        [Description("Set Envelope Fine Frequency")]
        [SlideParametersAttribute(0, 255, true)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((byte)2)]
        public byte EnvelopeFrequencyFine
        {
            get => f_EnvelopeFrequencyFine;
            set
            {
                if (f_EnvelopeFrequencyFine != value)
                {
                    f_EnvelopeFrequencyFine = value;
                    Program.SoundUpdating();
                    Ay8910WriteData(UnitNumber, 0, (byte)(12));
                    Ay8910WriteData(UnitNumber, 1, EnvelopeFrequencyCoarse);
                    Ay8910WriteData(UnitNumber, 0, (byte)(11));
                    Ay8910WriteData(UnitNumber, 1, value);
                    Program.SoundUpdated();
                }
            }
        }

        private byte f_EnvelopeType;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("Chip")]
        [Description("Set Envelope Type (0-15)")]
        [SlideParametersAttribute(0, 15, true)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((byte)0)]
        public byte EnvelopeType
        {
            get => f_EnvelopeType;
            set
            {
                byte v = (byte)(value & 15);
                if (f_EnvelopeType != v)
                {
                    f_EnvelopeType = v;
                    Ay8910WriteData(UnitNumber, 0, (byte)(13));
                    Ay8910WriteData(UnitNumber, 1, EnvelopeType);
                }
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
        public AY8910Timbre[] Timbres
        {
            get;
            private set;
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
                var obj = JsonConvert.DeserializeObject<AY8910>(serializeData);
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
        private delegate void delegate_ay8910_address_data_w(uint unitNumber, int offset, byte data);


        /// <summary>
        /// 
        /// </summary>
        private static delegate_ay8910_address_data_w ay8910_address_data_w
        {
            get;
            set;
        }


        /// <summary>
        /// 
        /// </summary>
        private static void Ay8910WriteData(uint unitNumber, int offset, byte data)
        {
            try
            {
                Program.SoundUpdating();
                ay8910_address_data_w(unitNumber, offset, data);
            }
            finally
            {
                Program.SoundUpdated();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate byte delegate_ay8910_read_ym(uint unitNumber);


        /// <summary>
        /// 
        /// </summary>
        private static delegate_ay8910_read_ym ay8910_read_ym
        {
            get;
            set;
        }


        /// <summary>
        /// 
        /// </summary>
        private static byte Ay8910ReadData(uint unitNumber)
        {
            try
            {
                Program.SoundUpdating();
                return ay8910_read_ym(unitNumber);
            }
            finally
            {
                Program.SoundUpdated();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        static AY8910()
        {
            IntPtr funcPtr = MameIF.GetProcAddress("ay8910_address_data_w");
            if (funcPtr != IntPtr.Zero)
            {
                ay8910_address_data_w = (delegate_ay8910_address_data_w)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_ay8910_address_data_w));
            }
            funcPtr = MameIF.GetProcAddress("ay8910_read_ym");
            if (funcPtr != IntPtr.Zero)
            {
                ay8910_read_ym = (delegate_ay8910_read_ym)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_ay8910_read_ym));
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

        internal override void PrepareSound()
        {
            base.PrepareSound();

            Ay8910WriteData(UnitNumber, 0, (byte)(7));
            Ay8910WriteData(UnitNumber, 1, (byte)(0x3f));
        }


        private AY8910SoundManager soundManager;

        /// <summary>
        /// 
        /// </summary>
        public AY8910(uint unitNumber) : base(unitNumber)
        {
            GainLeft = 2.0f;
            GainRight = 2.0f;

            Timbres = new AY8910Timbre[128];
            for (int i = 0; i < 128; i++)
                Timbres[i] = new AY8910Timbre();
            setPresetInstruments();

            this.soundManager = new AY8910SoundManager(this);
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
        private class AY8910SoundManager : SoundManagerBase
        {
            private SoundList<AY8910Sound> psgOnSounds = new SoundList<AY8910Sound>(3);

            private AY8910 parentModule;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            public AY8910SoundManager(AY8910 parent) : base(parent)
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

                var programNumber = (SevenBitNumber)parentModule.ProgramNumbers[note.Channel];
                var timbre = parentModule.Timbres[programNumber];
                AY8910Sound snd = new AY8910Sound(parentModule, this, timbre, note, emptySlot);
                switch (((AY8910Timbre)timbre).SoundType)
                {
                    case SoundType.PSG:
                    case SoundType.NOISE:
                    case SoundType.ENVELOPE:
                        psgOnSounds.Add(snd);
                        FormMain.OutputDebugLog("KeyOn PSG ch" + emptySlot + " " + note.ToString());
                        break;
                }
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
                switch (timbre.SoundType)
                {
                    case SoundType.PSG:
                    case SoundType.NOISE:
                    case SoundType.ENVELOPE:
                        {
                            emptySlot = SearchEmptySlotAndOff(psgOnSounds, note, 3);
                            break;
                        }
                }
                return emptySlot;
            }


        }


        /// <summary>
        /// 
        /// </summary>
        private class AY8910Sound : SoundBase
        {

            private AY8910 parentModule;

            private SevenBitNumber programNumber;

            private AY8910Timbre timbre;

            private SoundType lastSoundType;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentModule"></param>
            /// <param name="noteOnEvent"></param>
            /// <param name="programNumber"></param>
            /// <param name="slot"></param>
            public AY8910Sound(AY8910 parentModule, AY8910SoundManager manager, TimbreBase timbre, NoteOnEvent noteOnEvent, int slot) : base(parentModule, manager, timbre, noteOnEvent, slot)
            {
                this.parentModule = parentModule;
                this.programNumber = (SevenBitNumber)parentModule.ProgramNumbers[noteOnEvent.Channel];
                this.timbre = parentModule.Timbres[programNumber];

                lastSoundType = this.timbre.SoundType;
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
                    parentModule.EnvelopeType = gs.EnvelopeType;
                    parentModule.EnvelopeFrequencyFine = gs.EnvelopeFrequencyFine;
                    parentModule.EnvelopeFrequencyCoarse = gs.EnvelopeFrequencyCoarse;
                    Program.SoundUpdated();
                }

                UpdatePitch();
                UpdateVolume();
            }

            /// <summary>
            /// 
            /// </summary>
            public override void UpdateVolume()
            {
                switch (lastSoundType)
                {
                    case SoundType.PSG:
                    case SoundType.NOISE:
                    case SoundType.ENVELOPE:
                        updatePsgVolume();
                        break;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            private void updatePsgVolume()
            {
                byte fv = (byte)(((byte)Math.Round(15 * CalcCurrentVolume()) & 0xf));

                Ay8910WriteData(parentModule.UnitNumber, 0, (byte)(8 + Slot));
                switch (lastSoundType)
                {
                    case SoundType.PSG:
                    case SoundType.NOISE:
                        Ay8910WriteData(parentModule.UnitNumber, 1, fv);
                        break;
                    case SoundType.ENVELOPE:
                        Ay8910WriteData(parentModule.UnitNumber, 1, (byte)(0x10 | fv));
                        break;
                }

                //key on
                Ay8910WriteData(parentModule.UnitNumber, 0, (byte)(7));
                byte data = Ay8910ReadData(parentModule.UnitNumber);
                switch (lastSoundType)
                {
                    case SoundType.PSG:
                    case SoundType.ENVELOPE:
                        data &= (byte)(~(1 << Slot));
                        break;
                    case SoundType.NOISE:
                        data &= (byte)(~(8 << Slot));
                        break;
                }
                Ay8910WriteData(parentModule.UnitNumber, 1, data);

                switch (lastSoundType)
                {
                    case SoundType.ENVELOPE:
                        Program.SoundUpdating();
                        Ay8910WriteData(parentModule.UnitNumber, 0, (byte)(12));
                        Ay8910WriteData(parentModule.UnitNumber, 1, parentModule.EnvelopeFrequencyCoarse);
                        Ay8910WriteData(parentModule.UnitNumber, 0, (byte)(11));
                        Ay8910WriteData(parentModule.UnitNumber, 1, parentModule.EnvelopeFrequencyFine);
                        Ay8910WriteData(parentModule.UnitNumber, 0, (byte)(13));
                        Ay8910WriteData(parentModule.UnitNumber, 1, parentModule.EnvelopeType);
                        Program.SoundUpdated();
                        break;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public override void UpdatePitch()
            {
                switch (lastSoundType)
                {
                    case SoundType.PSG:
                    case SoundType.ENVELOPE:
                        updatePsgPitch();
                        break;
                    case SoundType.NOISE:
                        updateNoisePitch();
                        break;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="slot"></param>
            private void updatePsgPitch()
            {
                double freq = CalcCurrentFrequency();

                //freq = 111860.78125 / TP
                //TP = 111860.78125 / freq
                freq = Math.Round(111860.78125 / freq);
                if (freq > 0xfff)
                    freq = 0xfff;
                ushort tp = (ushort)freq;

                Ay8910WriteData(parentModule.UnitNumber, 0, (byte)(0 + (Slot * 2)));
                Ay8910WriteData(parentModule.UnitNumber, 1, (byte)(tp & 0xff));

                Ay8910WriteData(parentModule.UnitNumber, 0, (byte)(1 + (Slot * 2)));
                Ay8910WriteData(parentModule.UnitNumber, 1, (byte)((tp >> 8) & 0xf));
            }


            /// <summary>
            /// 
            /// </summary>
            /// <param name="slot"></param>
            private void updateNoisePitch()
            {
                int v = NoteOnEvent.NoteNumber % 15;

                Ay8910WriteData(parentModule.UnitNumber, 0, (byte)(5));
                Ay8910WriteData(parentModule.UnitNumber, 1, (byte)v);
            }

            /// <summary>
            /// 
            /// </summary>
            public override void SoundOff()
            {
                base.SoundOff();

                Ay8910WriteData(parentModule.UnitNumber, 0, (byte)(7));
                byte data = Ay8910ReadData(parentModule.UnitNumber);
                switch (lastSoundType)
                {
                    case SoundType.PSG:
                    case SoundType.ENVELOPE:
                        data |= (byte)(1 << Slot);
                        break;
                    case SoundType.NOISE:
                        data |= (byte)(8 << Slot);
                        break;
                }
                Ay8910WriteData(parentModule.UnitNumber, 1, data);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        [JsonConverter(typeof(NoTypeConverterJsonConverter<AY8910Timbre>))]
        [DataContract]
        public class AY8910Timbre : TimbreBase
        {
            [DataMember]
            [Category("Sound")]
            [Description("Sound Type")]
            public SoundType SoundType
            {
                get;
                set;
            }

            [DataMember]
            [Category("Chip")]
            [Description("Global Settings")]
            public GlobalSettings GlobalSettings
            {
                get;
                set;
            }

            public AY8910Timbre()
            {
                GlobalSettings = new GlobalSettings();
                this.SDS.FxS = new BasicFxSettings();
            }

            public override void RestoreFrom(string serializeData)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<AY8910Timbre>(serializeData);
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

        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [JsonConverter(typeof(NoTypeConverterJsonConverter<GlobalSettings>))]
        [DataContract]
        [MidiHook]
        public class GlobalSettings : ContextBoundObject
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

            private byte f_EnvelopeFrequencyCoarse = 2;

            /// <summary>
            /// 
            /// </summary>
            [DataMember]
            [Category("Chip")]
            [DefaultValue((byte)2)]
            [Description("Set Envelope Coarse Frequency")]
            [SlideParametersAttribute(0, 255)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte EnvelopeFrequencyCoarse
            {
                get => f_EnvelopeFrequencyCoarse;
                set
                {
                    if (f_EnvelopeFrequencyCoarse != value)
                        f_EnvelopeFrequencyCoarse = value;
                }
            }

            private byte f_EnvelopeFrequencyFine;

            /// <summary>
            /// 
            /// </summary>
            [DataMember]
            [Category("Chip")]
            [Description("Set Envelope Fine Frequency")]
            [SlideParametersAttribute(0, 255)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue((byte)0)]
            public byte EnvelopeFrequencyFine
            {
                get => f_EnvelopeFrequencyFine;
                set
                {
                    if (f_EnvelopeFrequencyFine != value)
                        f_EnvelopeFrequencyFine = value;
                }
            }

            private byte f_EnvelopeType;

            /// <summary>
            /// 
            /// </summary>
            [DataMember]
            [Category("Chip")]
            [Description("Set Envelope Type")]
            [SlideParametersAttribute(0, 15)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue((byte)0)]
            public byte EnvelopeType
            {
                get => f_EnvelopeType;
                set
                {
                    byte v = (byte)(value & 15);
                    if (f_EnvelopeType != v)
                        f_EnvelopeType = v;
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
            ENVELOPE,
        }

    }
}