// copyright-holders:K.Ito
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.MusicTheory;
using Newtonsoft.Json;
using Omu.ValueInjecter;
using Omu.ValueInjecter.Injections;
using zanac.MAmidiMEmo.ComponentModel;
using zanac.MAmidiMEmo.Gui;
using zanac.MAmidiMEmo.Instruments.Envelopes;
using zanac.MAmidiMEmo.Instruments.Vst;
using zanac.MAmidiMEmo.Mame;
using zanac.MAmidiMEmo.Midi;


namespace zanac.MAmidiMEmo.Instruments.Chips
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    [InstLock]
    public class MIDITHRU : InstrumentBase
    {

        public override string Name => "MIDITHRU";

        public override string Group => "MIDI";

        public override InstrumentType InstrumentType => 0;

        [Browsable(false)]
        public override string ImageKey => "MIDITHRU";

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        protected override string SoundInterfaceTagNamePrefix => "midithru_";

        /// <summary>
        /// 
        /// </summary>
        [Category("MIDI")]
        [Description("MIDI Device ID")]
        [IgnoreDataMember]
        [JsonIgnore]
        [Browsable(false)]
        public override uint DeviceID
        {
            get
            {
                return 0x1000;
            }
        }

        private string midiOut;

        private OutputDevice outputDevice;

        [DataMember]
        [Category("Chip")]
        [Description("Output input MIDI event to specified MIDI OUT")]
        [DefaultValue(SN_U110_Cards.None)]
        [TypeConverter(typeof(MidiOutConverter))]
        public string MidiOut
        {
            get
            {
                return midiOut;
            }
            set
            {
                if (midiOut != value)
                {
                    midiOut = value;
                    outputDevice = null;
                    foreach (var md in MidiManager.GetOutputMidiDevices())
                    {
                        if (string.Equals(md.Name, value))
                        {
                            outputDevice = md;
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private class MidiOutConverter : TypeConverter
        {

            public override bool CanConvertFrom(ITypeDescriptorContext context, Type srcType)
            {
                return srcType == typeof(string);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                return value.ToString();
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                MIDITHRU midithru = context.Instance as MIDITHRU;

                if (midithru != null)
                    return true;
                else
                    return base.GetStandardValuesSupported();
            }

            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
            {
                return true;
            }

            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                List<string> names = new List<string>();

                foreach (var md in MidiManager.GetOutputMidiDevices())
                {
                    names.Add(md.Name);
                }

                return new StandardValuesCollection(names);
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
            set
            {
                Timbres = (MidiThruTimbre[])value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        public MidiThruTimbre[] Timbres
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
            get
            {
                return base.CombinedTimbres;
            }
            set
            {
                base.CombinedTimbres = value;
            }
        }

        [Browsable(false)]
        public override DrumTimbre[] DrumTimbres
        {
            get
            {
                return base.DrumTimbres;
            }
            set
            {
                base.DrumTimbres = value;
            }
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
            get
            {
                return base.GlobalARP;
            }
            set
            {
                base.GlobalARP = value;
            }
        }


        [Browsable(false)]
        public override ChannelType[] ChannelTypes
        {
            get;
            set;
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

        [Browsable(false)]
        public override float GainLeft { get => base.GainLeft; set => base.GainLeft = value; }

        [Browsable(false)]
        public override float GainRight { get => base.GainRight; set => base.GainRight = value; }

        [Browsable(false)]
        public override FilterMode FilterMode { get => base.FilterMode; set => base.FilterMode = value; }

        [Browsable(false)]
        public override double FilterCutoff { get => base.FilterCutoff; set => base.FilterCutoff = value; }

        [Browsable(false)]
        public override double FilterResonance { get => base.FilterResonance; set => base.FilterResonance = value; }

        [Browsable(false)]
        public override GraphicEqualizerSettings GEQ { get => base.GEQ; set => base.GEQ = value; }

        [Browsable(false)]
        public override VSTPluginCollection VSTPlugins { get => base.VSTPlugins; set => base.VSTPlugins = value; }

        [Browsable(false)]
        public override double ProcessingInterval { get => base.ProcessingInterval; set => base.ProcessingInterval = value; }

        [Browsable(false)]
        public override ushort[] FineTunes { get => base.FineTunes; set => base.FineTunes = value; }

        [Browsable(false)]
        public override byte[] LegatoFootSwitch { get => base.LegatoFootSwitch; set => base.LegatoFootSwitch = value; }

        [Browsable(false)]
        public override SlotAssignmentType[] SlotAssignAlgorithm { get => base.SlotAssignAlgorithm; set => base.SlotAssignAlgorithm = value; }

        [Browsable(false)]
        public override ScaleTuning[] ScaleTunings { get => base.ScaleTunings; set => base.ScaleTunings = value; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serializeData"></param>
        public override void RestoreFrom(string serializeData)
        {
            try
            {
                using (var obj = JsonConvert.DeserializeObject<MIDITHRU>(serializeData))
                    this.InjectFrom(new LoopInjection(new[] { "SerializeData", "SerializeDataSave", "SerializeDataLoad" }), obj);
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
        public override void Dispose()
        {
            base.Dispose();
        }

        private MidiThruSoundManager soundManager;

        /// <summary>
        /// 
        /// </summary>
        public MIDITHRU(uint unitNumber) : base(unitNumber)
        {
            Timbres = new MidiThruTimbre[InstrumentBase.DEFAULT_MAX_TIMBRES];
            for (int i = 0; i < InstrumentBase.DEFAULT_MAX_TIMBRES; i++)
                Timbres[i] = new MidiThruTimbre();

            ChannelTypes[9] = ChannelType.Normal;

            this.soundManager = new MidiThruSoundManager(this);
        }

        protected override void OnMidiEvent(MidiEvent midiEvent)
        {
            var ce = midiEvent as ChannelEvent;

            if (outputDevice != null)
            {
                try
                {
                    if (ce != null)
                    {
                        if (Channels[ce.Channel])
                            outputDevice.SendEvent(midiEvent);
                    }
                    else
                    {
                        outputDevice.SendEvent(midiEvent);
                    }
                }
                catch { }
            }

            if (ce != null && Channels[ce.Channel])
            {
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
            if (outputDevice != null)
                outputDevice.TurnAllNotesOff();

            soundManager.ProcessAllSoundOff();
        }

        /// <summary>
        /// 
        /// </summary>
        private class MidiThruSoundManager : SoundManagerBase
        {
            private SoundList<MidiThruSound> instOnSounds = new SoundList<MidiThruSound>(9999);

            private MIDITHRU parentModule;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            public MidiThruSoundManager(MIDITHRU parent) : base(parent)
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
                foreach (MidiThruTimbre timbre in parentModule.GetBaseTimbres(note))
                {
                    tindex++;
                    int emptySlot = searchEmptySlot(note, timbre);
                    if (emptySlot < 0)
                        continue;

                    MidiThruSound snd = new MidiThruSound(parentModule, this, timbre, tindex - 1, note, emptySlot);
                    instOnSounds.Add(snd);

                    FormMain.OutputDebugLog(parentModule, "KeyOn ch" + emptySlot + " " + note.ToString());
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
            private int searchEmptySlot(TaggedNoteOnEvent note, MidiThruTimbre timbre)
            {
                int emptySlot = -1;

                emptySlot = SearchEmptySlotAndOff(parentModule, instOnSounds, note, 9999);
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
        private class MidiThruSound : SoundBase
        {
            private MIDITHRU parentModule;

            private SevenBitNumber programNumber;

            private MidiThruTimbre timbre;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentModule"></param>
            /// <param name="noteOnEvent"></param>
            /// <param name="programNumber"></param>
            /// <param name="slot"></param>
            public MidiThruSound(MIDITHRU parentModule, MidiThruSoundManager manager, TimbreBase timbre, int tindex, TaggedNoteOnEvent noteOnEvent, int slot) : base(parentModule, manager, timbre, tindex, noteOnEvent, slot)
            {
                this.parentModule = parentModule;
                this.programNumber = (SevenBitNumber)parentModule.ProgramNumbers[noteOnEvent.Channel];
                this.timbre = (MidiThruTimbre)timbre;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [JsonConverter(typeof(NoTypeConverterJsonConverter<MidiThruTimbre>))]
        [DataContract]
        [InstLock]
        public class MidiThruTimbre : TimbreBase
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

            /// <summary>
            /// 
            /// </summary>
            public MidiThruTimbre()
            {
            }

            public override void RestoreFrom(string serializeData)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<MidiThruTimbre>(serializeData);
                    this.InjectFrom(new LoopInjection(new[] { "SerializeData", "SerializeDataSave", "SerializeDataLoad" }), obj);
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