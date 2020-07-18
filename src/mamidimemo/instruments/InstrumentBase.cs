// copyright-holders:K.Ito
using Jacobi.Vst.Core;
using Jacobi.Vst.Interop.Host;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using zanac.MAmidiMEmo.ComponentModel;
using zanac.MAmidiMEmo.Gui;
using zanac.MAmidiMEmo.Instruments.Vst;
using zanac.MAmidiMEmo.Mame;
using zanac.MAmidiMEmo.Properties;

namespace zanac.MAmidiMEmo.Instruments
{
    [JsonConverter(typeof(NoTypeConverterJsonConverter<InstrumentBase>))]
    [TypeConverter(typeof(CustomExpandableObjectConverter))]
    [MidiHook]
    [DataContract]
    public abstract class InstrumentBase : ContextBoundObject, IDisposable
    {
        public static readonly object VstPluginContextLockObject = new object();

        /// <summary>
        /// 
        /// </summary>
        [Category("General")]
        public abstract string Name
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        [Category("General")]
        public abstract string Group
        {
            get;
        }

        private float f_GainLeft = 1.0f;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("General")]
        [Description("Gain Left ch. (0.0-*) of this Instrument")]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(UITypeEditor))]
        [DoubleSlideParameters(0d, 10d, 0.1d)]
        public virtual float GainLeft
        {
            get
            {
                return f_GainLeft;
            }
            set
            {
                if (f_GainLeft != value)
                {
                    f_GainLeft = value;
                    Program.SoundUpdating();
                    SetOutputGain(UnitNumber, SoundInterfaceTagNamePrefix, 0, value);
                    Program.SoundUpdated();
                }
            }
        }

        public virtual bool ShouldSerializeGainLeft()
        {
            return GainLeft != 1.0f;
        }

        public virtual void ResetGainLeft()
        {
            GainLeft = 1.0f;
        }

        private float f_GainRight = 1.0f;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("General")]
        [Description("Gain Right ch. (0.0-*) of this Instrument")]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(UITypeEditor))]
        [DoubleSlideParameters(0d, 10d, 0.1d)]
        public virtual float GainRight
        {
            get
            {
                return f_GainRight;
            }
            set
            {
                if (f_GainRight != value)
                {
                    f_GainRight = value;
                    Program.SoundUpdating();
                    SetOutputGain(UnitNumber, SoundInterfaceTagNamePrefix, 1, value);
                    Program.SoundUpdated();
                }
            }
        }

        public virtual bool ShouldSerializeGainRight()
        {
            return GainRight != 1.0f;
        }

        public virtual void ResetGainRight()
        {
            GainRight = 1.0f;
        }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("General")]
        [DefaultValue(FollowerUnit.None)]
        [Description("Enable Follower mode. Share free voice channels with specified leader unit and does not accept any Note On MIDI event.\r\n" +
            "Be sure to set same settings with the leader unit.")]
        public virtual FollowerUnit FollowerMode
        {
            get;
            set;
        }

        private FilterMode f_FilterMode = FilterMode.LowPass;

        [DataMember]
        [Category("Filter")]
        [Description("Audio Filter Type")]
        public FilterMode FilterMode
        {
            get => f_FilterMode;
            set
            {
                if (f_FilterMode != value)
                {
                    f_FilterMode = value;
                    Program.SoundUpdating();
                    set_filter(UnitNumber, SoundInterfaceTagNamePrefix, f_FilterMode, FilterCutoff, FilterResonance);
                    Program.SoundUpdated();
                }
            }
        }

        public virtual bool ShouldSerializeFilterMode()
        {
            return FilterMode != FilterMode.LowPass;
        }

        public virtual void ResetFilterMode()
        {
            FilterMode = FilterMode.LowPass;
        }

        private double f_FilterCutOff = 0.99d;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("Filter")]
        [Description("Audio Cutoff Filter (0.1-0.99) of this Instrument")]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(UITypeEditor))]
        [DoubleSlideParameters(0.01d, 0.99d, 0.01d)]
        [DefaultValue(0.99d)]
        public double FilterCutoff
        {
            get
            {
                return f_FilterCutOff;
            }
            set
            {
                double v = value;
                if (v < 0.01d)
                    v = 0.01d;
                else if (v > 0.99d)
                    v = 0.99d;
                if (f_FilterCutOff != v)
                {
                    f_FilterCutOff = v;
                    Program.SoundUpdating();
                    set_filter(UnitNumber, SoundInterfaceTagNamePrefix, FilterMode, f_FilterCutOff, FilterResonance);
                    Program.SoundUpdated();
                }
            }
        }


        private double f_FilterResonance = 0.01d;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("Filter")]
        [Description("Audio Cutoff Filter (0.01-1.00) of this Instrument")]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(UITypeEditor))]
        [DoubleSlideParameters(0.01d, 1.00d, 0.01d)]
        [DefaultValue(0.01d)]
        public double FilterResonance
        {
            get
            {
                return f_FilterResonance;
            }
            set
            {
                double v = value;
                if (v < 0.00d)
                    v = 0.00d;
                else if (v > 1.00d)
                    v = 1.00d;
                if (f_FilterResonance != v)
                {
                    f_FilterResonance = v;
                    Program.SoundUpdating();
                    set_filter(UnitNumber, SoundInterfaceTagNamePrefix, FilterMode, FilterCutoff, f_FilterResonance);
                    Program.SoundUpdated();
                }
            }
        }

        [DataMember]
        [Category("Filter")]
        [Description("Set VST effect plugins. Effects are applied in order from the first VST to the last VST")]
        public VSTPluginCollection VSTPlugins
        {
            get;
            set;
        } = new VSTPluginCollection();

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Description("Memo")]
        public string Memo
        {
            get;
            set;
        }

        [Editor(typeof(FormTextUITypeEditor), typeof(UITypeEditor)), Localizable(false)]
        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [IgnoreDataMember]
        [JsonIgnore]
        [Description("You can copy and paste this text data to other same type Instrument.\r\nNote: Open dropdown editor then copy all text and paste to dropdown editor. Do not copy and paste one liner text.")]
        public string SerializeData
        {
            get
            {
                //return JsonHelper.SerializeToMinimalJson(this);   NG: cant reset child member value
                return JsonConvert.SerializeObject(this, Formatting.Indented);
            }
            set
            {
                JObject jo = JObject.Parse(value);
                jo["UnitNumber"] = UnitNumber;

                RestoreFrom(jo.ToString());
                SetVstFxCallback(UnitNumber, SoundInterfaceTagNamePrefix, f_vst_fx_callback);
                set_device_enable(UnitNumber, SoundInterfaceTagNamePrefix, 1);
            }
        }

        public abstract void RestoreFrom(string serializeData);

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        public abstract string ImageKey
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        public abstract InstrumentType InstrumentType
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        protected abstract string SoundInterfaceTagNamePrefix
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("General")]
        public abstract uint DeviceID
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("MIDI")]
        public uint UnitNumber
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        public abstract TimbreBase[] BaseTimbres
        {
            get;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public TimbreBase GetFinalTimbre(int channel)
        {
            int pn = (int)ProgramAssignments[ProgramNumbers[channel]];

            if ((pn & 0xffff0000) != 0)
            {
                int ptidx = pn & 0xffff;
                if (ptidx >= CombinedTimbres.Length)
                    ptidx = CombinedTimbres.Length - 1;
                var pts = CombinedTimbres[ptidx];
                for (int i = 0; i < pts.BindTimbres.Length; i++)
                {
                    if (pts.BindTimbres[i] != null) //if Timbre assigned
                        return CombinedTimbres[ptidx];
                }
            }

            int btidx = pn & 0xffff;
            if (btidx >= BaseTimbres.Length)
                btidx = BaseTimbres.Length - 1;
            return BaseTimbres[btidx];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual TimbreBase[] GetBaseTimbres(NoteOnEvent ev)
        {
            List<TimbreBase> ts = new List<TimbreBase>();

            switch (ChannelTypes[ev.Channel])
            {
                case ChannelType.Normal:
                    {
                        int pn = (int)ProgramAssignments[ProgramNumbers[ev.Channel]];
                        if ((pn & 0xffff0000) != 0)
                        {
                            int ptidx = pn & 0xffff;
                            if (ptidx >= CombinedTimbres.Length)
                                ptidx = CombinedTimbres.Length - 1;
                            foreach (int? tn in CombinedTimbres[ptidx].BindTimbres)
                            {
                                if (tn != null && tn.Value < BaseTimbres.Length)
                                    ts.Add(BaseTimbres[tn.Value]);
                            }
                            if (ts.Count != 0)
                                return ts.ToArray();
                        }

                        int btidx = pn & 0xffff;
                        if (btidx >= BaseTimbres.Length)
                            btidx = BaseTimbres.Length - 1;
                        ts.Add(BaseTimbres[btidx]);
                        break;
                    }
                case ChannelType.Drum:
                    {
                        int pn = (int)DrumTimbreTable.DrumTimbres[ev.NoteNumber].TimbreNumber;
                        if ((pn & 0xffff0000) != 0)
                        {
                            int ptidx = pn & 0xffff;
                            if (ptidx >= CombinedTimbres.Length)
                                ptidx = CombinedTimbres.Length - 1;
                            foreach (int? tn in CombinedTimbres[ptidx].BindTimbres)
                            {
                                if (tn != null && tn.Value < BaseTimbres.Length)
                                    ts.Add(BaseTimbres[tn.Value]);
                            }
                            if (ts.Count != 0)
                                return ts.ToArray();
                        }

                        int btidx = pn & 0xffff;
                        if (btidx >= BaseTimbres.Length)
                            btidx = BaseTimbres.Length - 1;
                        ts.Add(BaseTimbres[btidx]);
                        break;
                    }
            }

            return ts.ToArray();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual int[] GetBaseTimbreIndexes(NoteOnEvent ev)
        {
            List<int> ts = new List<int>();


            switch (ChannelTypes[ev.Channel])
            {
                case ChannelType.Normal:
                    {
                        int pn = (int)ProgramAssignments[ProgramNumbers[ev.Channel]];

                        if ((pn & 0xffff0000) != 0)
                        {
                            int ptidx = pn & 0xffff;
                            if (ptidx >= CombinedTimbres.Length)
                                ptidx = CombinedTimbres.Length - 1;
                            foreach (int? tn in CombinedTimbres[ptidx].BindTimbres)
                            {
                                if (tn != null && tn.Value < BaseTimbres.Length)
                                    ts.Add((int)tn);
                            }
                            if (ts.Count != 0)
                                return ts.ToArray();
                        }

                        int btidx = pn & 0xffff;
                        if (btidx >= BaseTimbres.Length)
                            btidx = BaseTimbres.Length - 1;
                        ts.Add(btidx);
                        break;
                    }
                case ChannelType.Drum:
                    {
                        int pn = (int)DrumTimbreTable.DrumTimbres[ev.NoteNumber].TimbreNumber;

                        if ((pn & 0xffff0000) != 0)
                        {
                            int ptidx = pn & 0xffff;
                            if (ptidx >= CombinedTimbres.Length)
                                ptidx = CombinedTimbres.Length - 1;
                            foreach (int? tn in CombinedTimbres[ptidx].BindTimbres)
                            {
                                if (tn != null && tn.Value < BaseTimbres.Length)
                                    ts.Add((int)tn);
                            }
                            if (ts.Count != 0)
                                return ts.ToArray();
                        }

                        int btidx = pn & 0xffff;
                        if (btidx >= BaseTimbres.Length)
                            btidx = BaseTimbres.Length - 1;
                        ts.Add(btidx);
                        break;
                    }
            }
            return ts.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("Chip")]
        [Description("Combine multiple Timbres (0-255)\r\n" +
            "Override PatchTimbres to Timbres when you set binding patch numbers.")]
        [EditorAttribute(typeof(DummyEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(ExpandableCollectionConverter))]
        public virtual CombinedTimbre[] CombinedTimbres
        {
            get;
            set;
        }


        [DataMember]
        [Category("Chip")]
        [Description("Drum Timbres table")]
        [Editor(typeof(DrumTableUITypeEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(CustomObjectTypeConverter))]
        public virtual DrumTimbreTable DrumTimbreTable
        {
            get;
            set;
        }


        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("MIDI")]
        [Description("Channel type <MIDI 16ch>")]
        [TypeConverter(typeof(ExpandableCollectionConverter))]
        [CollectionDefaultValue(true)]
        public virtual ChannelType[] ChannelTypes
        {
            get;
            set;
        }

        public bool ShouldSerializeChannelTypes()
        {
            foreach (var dt in ChannelTypes)
            {
                if (dt != ChannelType.Normal)
                    return true;
            }
            return false;
        }

        public void ResetChannelTypes()
        {
            for (int i = 0; i < ChannelTypes.Length; i++)
                ChannelTypes[i] = ChannelType.Normal;
        }


        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("MIDI")]
        [Description("Receving MIDI ch <MIDI 16ch>")]
        [TypeConverter(typeof(ExpandableCollectionConverter))]
        [CollectionDefaultValue(true)]
        public virtual bool[] Channels
        {
            get;
            set;
        }

        public bool ShouldSerializeChannels()
        {
            foreach (var dt in Channels)
            {
                if (dt != true)
                    return true;
            }
            return false;
        }

        public void ResetChannels()
        {
            for (int i = 0; i < Channels.Length; i++)
                Channels[i] = true;
        }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("MIDI")]
        [Description("Pitch (0 - 8192 - 16383) <MIDI 16ch>")]
        [TypeConverter(typeof(MaskableExpandableCollectionConverter))]
        [Mask(16383)]
        [CollectionDefaultValue((ushort)8192)]
        public virtual ushort[] Pitchs
        {
            get;
            set;
        }

        public bool ShouldSerializePitchs()
        {
            foreach (var dt in Pitchs)
            {
                if (dt != 8192)
                    return true;
            }
            return false;
        }

        public void ResetPitchs()
        {
            for (int i = 0; i < Pitchs.Length; i++)
                Pitchs[i] = 8192;
        }

        [DataMember]
        [Category("MIDI")]
        [Description("Pitch bend sensitivity [half note] <MIDI 16ch>")]
        [TypeConverter(typeof(MaskableExpandableCollectionConverter))]
        [Mask(127)]
        [CollectionDefaultValue((byte)2)]
        public virtual byte[] PitchBendRanges
        {
            get;
            set;
        }

        public bool ShouldSerializePitchBendRanges()
        {
            foreach (var dt in PitchBendRanges)
            {
                if (dt != 2)
                    return true;
            }
            return false;
        }

        public void ResetPitchBendRanges()
        {
            for (int i = 0; i < PitchBendRanges.Length; i++)
                Pitchs[i] = 2;
        }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("MIDI")]
        [Description("Assign the Timbre/CombinedTimbre to program number0-127.")]
        [EditorAttribute(typeof(DummyEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(ExpandableCollectionConverter))]
        public virtual ProgramAssignmentNumber[] ProgramAssignments
        {
            get;
            set;
        }

        public virtual bool ShouldSerializeProgramAssignments()
        {
            for (int i = 0; i < ProgramAssignments.Length; i++)
            {
                if ((int)ProgramAssignments[i] != i)
                    return true;
            }
            return false;
        }

        public virtual void ResetProgramAssignments()
        {
            for (int i = 0; i < ProgramAssignments.Length; i++)
                ProgramAssignments[i] = (ProgramAssignmentNumber)i;
        }


        [DataMember]
        [Category("MIDI")]
        [Description("Program number (0-127) <MIDI 16ch>")]
        [TypeConverter(typeof(MaskableExpandableCollectionConverter))]
        [Mask(127)]
        [CollectionDefaultValue((byte)0)]
        public virtual byte[] ProgramNumbers
        {
            get;
            set;
        }

        public bool ShouldSerializeProgramNumbers()
        {
            foreach (var dt in ProgramNumbers)
            {
                if (dt != 0)
                    return true;
            }
            return false;
        }

        public void ResetProgramNumbers()
        {
            for (int i = 0; i < ProgramNumbers.Length; i++)
                Pitchs[i] = 0;
        }

        [DataMember]
        [Category("MIDI")]
        [Description("Volume (0-127) <MIDI 16ch>")]
        [TypeConverter(typeof(MaskableExpandableCollectionConverter))]
        [Mask(127)]
        [CollectionDefaultValue((byte)127)]
        public virtual byte[] Volumes
        {
            get;
            set;
        }

        public bool ShouldSerializeVolumes()
        {
            foreach (var dt in Volumes)
            {
                if (dt != 127)
                    return true;
            }
            return false;
        }

        public void ResetVolumes()
        {
            for (int i = 0; i < Volumes.Length; i++)
                Volumes[i] = 127;
        }

        [DataMember]
        [Category("MIDI")]
        [Description("Volume (0-127) <MIDI 16ch>")]
        [TypeConverter(typeof(MaskableExpandableCollectionConverter))]
        [Mask(127)]
        [CollectionDefaultValue((byte)127)]
        public virtual byte[] Expressions
        {
            get;
            set;
        }


        public bool ShouldSerializeExpressions()
        {
            foreach (var dt in Expressions)
            {
                if (dt != 127)
                    return true;
            }
            return false;
        }

        public void ResetExpressions()
        {
            for (int i = 0; i < Expressions.Length; i++)
                Expressions[i] = 127;
        }

        [DataMember]
        [Category("MIDI")]
        [Description("Volume ((L)0-63(C)64-127(R)) <MIDI 16ch>")]
        [TypeConverter(typeof(MaskableExpandableCollectionConverter))]
        [Mask(127)]
        [CollectionDefaultValue((byte)64)]
        public virtual byte[] Panpots
        {
            get;
            set;
        }

        public bool ShouldSerializePanpots()
        {
            foreach (var dt in Panpots)
            {
                if (dt != 64)
                    return true;
            }
            return false;
        }

        public void ResetPanpots()
        {
            for (int i = 0; i < Panpots.Length; i++)
                Panpots[i] = 64;
        }

        [DataMember]
        [Category("MIDI")]
        [Description("Modulation (0-127) <MIDI 16ch>")]
        [TypeConverter(typeof(MaskableExpandableCollectionConverter))]
        [Mask(127)]
        [CollectionDefaultValue((byte)0)]
        public virtual byte[] Modulations
        {
            get;
            set;
        }

        public bool ShouldSerializeModulations()
        {
            foreach (var dt in Modulations)
            {
                if (dt != 0)
                    return true;
            }
            return false;
        }

        public void ResetModulations()
        {
            for (int i = 0; i < Modulations.Length; i++)
                Modulations[i] = 0;
        }


        [DataMember]
        [Category("MIDI")]
        [Description("Modulation Rate (0-64-127) <MIDI 16ch>")]
        [TypeConverter(typeof(MaskableExpandableCollectionConverter))]
        [Mask(127)]
        [CollectionDefaultValue((byte)64)]
        public virtual byte[] ModulationRates
        {
            get;
            set;
        }


        public bool ShouldSerializeModulationRates()
        {
            foreach (var dt in ModulationRates)
            {
                if (dt != 64)
                    return true;
            }
            return false;
        }

        public void ResetModulationRates()
        {
            for (int i = 0; i < ModulationRates.Length; i++)
                ModulationRates[i] = 64;
        }

        /// <summary>
        /// Hz
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public virtual double GetModulationRateHz(int channel)
        {
            byte val = ModulationRates[channel];

            double rate = Math.Pow(((double)val / 64d), 2) * 6;

            return rate;
        }

        [DataMember]
        [Category("MIDI")]
        [Description("Modulation Depth (0-64-127) <MIDI 16ch>")]
        [TypeConverter(typeof(MaskableExpandableCollectionConverter))]
        [Mask(127)]
        [CollectionDefaultValue((byte)64)]
        public virtual byte[] ModulationDepthes
        {
            get;
            set;
        }

        public bool ShouldSerializeModulationDepthes()
        {
            foreach (var dt in ModulationDepthes)
            {
                if (dt != 64)
                    return true;
            }
            return false;
        }

        public void ResetModulationDepthes()
        {
            for (int i = 0; i < ModulationDepthes.Length; i++)
                ModulationDepthes[i] = 64;
        }

        [DataMember]
        [Category("MIDI")]
        [Description("Modulation Delay (0-64-127) <MIDI 16ch>")]
        [TypeConverter(typeof(MaskableExpandableCollectionConverter))]
        [Mask(127)]
        [CollectionDefaultValue((byte)64)]
        public virtual byte[] ModulationDelays
        {
            get;
            set;
        }

        public bool ShouldSerializeModulationDelays()
        {
            foreach (var dt in ModulationDelays)
            {
                if (dt != 64)
                    return true;
            }
            return false;
        }

        public void ResetModulationDelays()
        {
            for (int i = 0; i < ModulationDelays.Length; i++)
                ModulationDelays[i] = 64;
        }

        /// <summary>
        /// Hz
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public virtual double GetModulationDelaySec(int channel)
        {
            byte val = ModulationDelays[channel];

            double rate = Math.Pow(((double)val / 64d), 3.25) - 1;
            if (rate < 0)
                rate = 0;

            return rate;
        }


        [DataMember]
        [Category("MIDI")]
        [Description("Modulation Depth Range[Half Note] (0-127) <MIDI 16ch>")]
        [TypeConverter(typeof(MaskableExpandableCollectionConverter))]
        [Mask(127)]
        [CollectionDefaultValue((byte)0)]
        public virtual byte[] ModulationDepthRangesNote
        {
            get;
            set;
        }

        public bool ShouldSerializeModulationDepthRangesNote()
        {
            foreach (var dt in ModulationDepthRangesNote)
            {
                if (dt != 0)
                    return true;
            }
            return false;
        }

        public void ResetModulationDepthRangesNote()
        {
            for (int i = 0; i < ModulationDepthRangesNote.Length; i++)
                ModulationDepthRangesNote[i] = 64;
        }


        [DataMember]
        [Category("MIDI")]
        [Description("Modulation Depth Range[Cent] (0-64-127) <MIDI 16ch>")]
        [TypeConverter(typeof(MaskableExpandableCollectionConverter))]
        [Mask(127)]
        [CollectionDefaultValue((byte)64)]
        public virtual byte[] ModulationDepthRangesCent
        {
            get;
            set;
        }


        public bool ShouldSerializeModulationDepthRangesCent()
        {
            foreach (var dt in ModulationDepthRangesCent)
            {
                if (dt != 0x40)
                    return true;
            }
            return false;
        }

        public void ResetModulationDepthRangesCent()
        {
            for (int i = 0; i < ModulationDepthRangesCent.Length; i++)
                ModulationDepthRangesCent[i] = 64;
        }


        [DataMember]
        [Category("MIDI")]
        [Description("Portamento (0:Off 64:On) <MIDI 16ch>")]
        [TypeConverter(typeof(MaskableExpandableCollectionConverter))]
        [Mask(127)]
        [CollectionDefaultValue((byte)0)]
        public virtual byte[] Portamentos
        {
            get;
            set;
        }


        public bool ShouldSerializePortamentos()
        {
            foreach (var dt in Portamentos)
            {
                if (dt != 0)
                    return true;
            }
            return false;
        }

        public void ResetPortamentos()
        {
            for (int i = 0; i < Portamentos.Length; i++)
                Portamentos[i] = 0;
        }


        [DataMember]
        [Category("MIDI")]
        [Description("Portamento Time (0-127) <MIDI 16ch>")]
        [TypeConverter(typeof(MaskableExpandableCollectionConverter))]
        [Mask(127)]
        [CollectionDefaultValue((byte)0)]
        public virtual byte[] PortamentoTimes
        {
            get;
            set;
        }


        public bool ShouldSerializePortamentoTimes()
        {
            foreach (var dt in PortamentoTimes)
            {
                if (dt != 0)
                    return true;
            }
            return false;
        }

        public void ResetPortamentoTimes()
        {
            for (int i = 0; i < PortamentoTimes.Length; i++)
                PortamentoTimes[i] = 0;
        }

        [DataMember]
        [Category("MIDI")]
        [Description("Mono mode (0-127) 0:Disable mono mode <MIDI 16ch>")]
        [TypeConverter(typeof(MaskableExpandableCollectionConverter))]
        [Mask(127)]
        [CollectionDefaultValue((byte)0)]
        public virtual byte[] MonoMode
        {
            get;
            set;
        }


        public bool ShouldSerializeMonoMode()
        {
            foreach (var dt in MonoMode)
            {
                if (dt != 0)
                    return true;
            }
            return false;
        }

        public void ResetMonoMode()
        {
            for (int i = 0; i < MonoMode.Length; i++)
                MonoMode[i] = 0;
        }


        [DataMember]
        [Category("MIDI")]
        [Description("General Purpose Control Settings <MIDI 16ch>\r\n" +
            "Link Data Entry message value with the Instrument property value (Only the property that has a slider editor)\r\n" +
            "eg 1) \"GainLeft,GainRight\" ... You can change Gain property values dynamically via MIDI Control Change No.16-19,80-83 message.\r\n" +
            "eg 2) \"Timbres[0].ALG\" ... You can change Timbre 0 FM synth algorithm values dynamically via MIDI Control Change No.16-19,80-83 message.")]
        [DisplayName("General Purpose Control Settings(GPCS)")]
        public GeneralPurposeControlSettings[] GPCS
        {
            get;
            set;
        }

        public bool ShouldSerializeGPCS()
        {
            foreach (var dt in GPCS)
            {
                if (dt.GeneralPurposeControl1 != null ||
                    dt.GeneralPurposeControl2 != null ||
                    dt.GeneralPurposeControl3 != null ||
                    dt.GeneralPurposeControl4 != null ||
                    dt.GeneralPurposeControl5 != null ||
                    dt.GeneralPurposeControl6 != null ||
                    dt.GeneralPurposeControl7 != null ||
                    dt.GeneralPurposeControl8 != null
                    )
                    return true;
            }
            return false;
        }

        public void ResetGPCS()
        {
            for (int i = 0; i < GPCS.Length; i++)
            {
                GPCS[i].GeneralPurposeControl1 = null;
                GPCS[i].GeneralPurposeControl2 = null;
                GPCS[i].GeneralPurposeControl3 = null;
                GPCS[i].GeneralPurposeControl4 = null;
                GPCS[i].GeneralPurposeControl5 = null;
                GPCS[i].GeneralPurposeControl6 = null;
                GPCS[i].GeneralPurposeControl7 = null;
                GPCS[i].GeneralPurposeControl8 = null;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="maxVoice"></param>
        /// <returns></returns>
        protected int CalcMaxVoiceNumber(int channel, byte maxVoice)
        {
            if (MonoMode[channel] == 0 || MonoMode[channel] > maxVoice)
                return maxVoice;
            else
                return MonoMode[channel];
        }



        [Browsable(false)]
        public byte[] DataLsb
        {
            get;
        }

        [Browsable(false)]
        public byte[] DataMsb
        {
            get;
        }

        [Browsable(false)]
        public byte[] NrpnLsb
        {
            get;
        }

        [Browsable(false)]
        public byte[] NrpnMsb
        {
            get;
        }

        [Browsable(false)]
        public byte[] RpnLsb
        {
            get;
        }

        [Browsable(false)]
        public byte[] RpnMsb
        {
            get;
        }

        private DataEntryType[] lastDateEntryType;

        /// <summary>
        /// 
        /// </summary>
        private enum DataEntryType
        {
            None,
            Nrpn,
            Rpn
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delegate_set_device_enable(uint unitNumber, string tagName, byte enable);

        private static delegate_set_device_enable set_device_enable;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delegate_device_reset(uint unitNumber, string tagName);

        private static delegate_device_reset device_reset;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void delegate_set_output_gain(uint unitNumber, string tagName, int channel, float gain);

        public static delegate_set_output_gain SetOutputGain;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delegate_set_filter(uint unitNumber, string tagName, FilterMode filterMode, double cutoff, double resonance);

        private static delegate_set_filter set_filter;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delegate_set_clock(uint unitNumber, string tagName, uint clock);

        /// <summary>
        /// 
        /// </summary>
        private static delegate_set_clock set_clock;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unitNumber"></param>
        /// <param name="clock"></param>
        protected void SetClock(uint unitNumber, uint clock)
        {
            try
            {
                Program.SoundUpdating();
                set_clock(unitNumber, SoundInterfaceTagNamePrefix, clock);
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
        private delegate void delg_vst_fx_callback(
            IntPtr buffer,
            int samples);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delegate_set_vst_fx_callback(uint unitNumber, string name, delg_vst_fx_callback callback);

        /// <summary>
        /// 
        /// </summary>
        private static void SetVstFxCallback(uint unitNumber, string name, delg_vst_fx_callback callback)
        {
            try
            {
                Program.SoundUpdating();
                set_vst_fx_callback(unitNumber, name, callback);
            }
            finally
            {
                Program.SoundUpdated();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private static delegate_set_vst_fx_callback set_vst_fx_callback
        {
            get;
            set;
        }

        private delg_vst_fx_callback f_vst_fx_callback;

        private GCHandle vstHandle;


        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delg_start_vgm_recording_to(uint unitNumber, string tagName, string vgmPath);

        private delg_start_vgm_recording_to start_vgm_recording_to;

        /// <summary>
        /// 
        /// </summary>
        private delg_start_vgm_recording_to StartVgmRecordingToInternal
        {
            get
            {
                if (start_vgm_recording_to == null)
                {
                    IntPtr funcPtr = MameIF.GetProcAddress("start_vgm_recording_to");
                    if (funcPtr != IntPtr.Zero)
                        start_vgm_recording_to = (delg_start_vgm_recording_to)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delg_start_vgm_recording_to));
                }
                return start_vgm_recording_to;
            }
        }

        private bool vgmRecording;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vgmPath"></param>
        public virtual void StartVgmRecordingTo(string vgmPath)
        {
            vgmRecording = true;

            var now = DateTime.Now;
            string op = Path.Combine(vgmPath, this.Name + "_" + this.UnitNumber + "_" +
                now.ToShortDateString().Replace('/', '-') + "_" + now.ToLongTimeString().Replace(':', '-'));

            StartVgmRecordingToInternal(UnitNumber, SoundInterfaceTagNamePrefix, op);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delg_stop_vgm_recording(uint unitNumber, string tagName);

        private delg_stop_vgm_recording stop_vgm_recording;

        /// <summary>
        /// 
        /// </summary>
        private delg_stop_vgm_recording StopVgmRecordingInternal
        {
            get
            {
                if (stop_vgm_recording == null)
                {
                    IntPtr funcPtr = MameIF.GetProcAddress("stop_vgm_recording");
                    if (funcPtr != IntPtr.Zero)
                        stop_vgm_recording = (delg_stop_vgm_recording)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delg_stop_vgm_recording));
                }
                return stop_vgm_recording;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void StopVgmRecording()
        {
            vgmRecording = false;
            StopVgmRecordingInternal(UnitNumber, SoundInterfaceTagNamePrefix);
        }

        /// <summary>
        /// 
        /// </summary>
        static InstrumentBase()
        {
            IntPtr funcPtr = MameIF.GetProcAddress("set_device_enable");
            if (funcPtr != IntPtr.Zero)
                set_device_enable = Marshal.GetDelegateForFunctionPointer<delegate_set_device_enable>(funcPtr);

            funcPtr = MameIF.GetProcAddress("set_output_gain");
            if (funcPtr != IntPtr.Zero)
                SetOutputGain = Marshal.GetDelegateForFunctionPointer<delegate_set_output_gain>(funcPtr);

            funcPtr = MameIF.GetProcAddress("device_reset");
            if (funcPtr != IntPtr.Zero)
                device_reset = Marshal.GetDelegateForFunctionPointer<delegate_device_reset>(funcPtr);

            funcPtr = MameIF.GetProcAddress("set_filter");
            if (funcPtr != IntPtr.Zero)
                set_filter = Marshal.GetDelegateForFunctionPointer<delegate_set_filter>(funcPtr);

            funcPtr = MameIF.GetProcAddress("set_clock");
            if (funcPtr != IntPtr.Zero)
                set_clock = Marshal.GetDelegateForFunctionPointer<delegate_set_clock>(funcPtr);

            funcPtr = MameIF.GetProcAddress("set_vst_fx_callback");
            if (funcPtr != IntPtr.Zero)
                set_vst_fx_callback = Marshal.GetDelegateForFunctionPointer<delegate_set_vst_fx_callback>(funcPtr);
        }

        public const int MAX_TIMBRES = 256;

        /// <summary>
        /// 
        /// </summary>
        public InstrumentBase(uint unitNumber)
        {
            UnitNumber = unitNumber;

            device_reset(UnitNumber, SoundInterfaceTagNamePrefix);

            SetOutputGain(UnitNumber, SoundInterfaceTagNamePrefix, 1, GainLeft);
            SetOutputGain(UnitNumber, SoundInterfaceTagNamePrefix, 1, GainRight);

            set_filter(UnitNumber, SoundInterfaceTagNamePrefix, FilterMode, FilterCutoff, FilterResonance);

            vstHandle = GCHandle.Alloc(this);

            f_vst_fx_callback = new delg_vst_fx_callback(vst_fx_callback);
            SetVstFxCallback(UnitNumber, SoundInterfaceTagNamePrefix, f_vst_fx_callback);

            CombinedTimbres = new CombinedTimbre[MAX_TIMBRES];
            for (int i = 0; i < MAX_TIMBRES; i++)
                CombinedTimbres[i] = new CombinedTimbre();

            DrumTimbreTable = new DrumTimbreTable();

            ProgramAssignments = new ProgramAssignmentNumber[128];
            for (int i = 0; i < ProgramAssignments.Length; i++)
                ProgramAssignments[i] = (ProgramAssignmentNumber)i;

            ChannelTypes = new ChannelType[] {
                    ChannelType.Normal, ChannelType.Normal, ChannelType.Normal,
                    ChannelType.Normal, ChannelType.Normal, ChannelType.Normal,
                    ChannelType.Normal, ChannelType.Normal, ChannelType.Normal,
                    ChannelType.Normal, ChannelType.Normal, ChannelType.Normal,
                    ChannelType.Normal, ChannelType.Normal, ChannelType.Normal,
                    ChannelType.Normal };

            Channels = new bool[] {
                    true, true, true,
                    true, true, true,
                    true, true, true,
                    true, true, true,
                    true, true, true,true };

            ProgramNumbers = new byte[] {
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0, 0 };
            Volumes = new byte[] {
                    127, 127, 127,
                    127, 127, 127,
                    127, 127, 127,
                    127, 127, 127,
                    127, 127, 127, 127  };
            Expressions = new byte[] {
                    127, 127, 127,
                    127, 127, 127,
                    127, 127, 127,
                    127, 127, 127,
                    127, 127, 127, 127  };
            Panpots = new byte[] {
                    64, 64, 64,
                    64, 64, 64,
                    64, 64, 64,
                    64, 64, 64,
                    64, 64, 64, 64};
            Pitchs = new ushort[] {
                    8192, 8192, 8192,
                    8192, 8192, 8192,
                    8192, 8192, 8192,
                    8192, 8192, 8192,
                    8192, 8192, 8192, 8192};
            PitchBendRanges = new byte[] {
                    2, 2, 2,
                    2, 2, 2,
                    2, 2, 2,
                    2, 2, 2,
                    2, 2, 2, 2};
            DataLsb = new byte[] {
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0, 0};
            DataMsb = new byte[] {
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0, 0};
            NrpnLsb = new byte[] {
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0, 0};
            NrpnMsb = new byte[] {
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0, 0};
            RpnLsb = new byte[] {
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0, 0};
            RpnMsb = new byte[] {
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0, 0};
            Modulations = new byte[] {
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0, 0};
            ModulationRates = new byte[] {
                    64, 64, 64,
                    64, 64, 64,
                    64, 64, 64,
                    64, 64, 64,
                    64, 64, 64, 64};
            ModulationDepthes = new byte[] {
                    64, 64, 64,
                    64, 64, 64,
                    64, 64, 64,
                    64, 64, 64,
                    64, 64, 64, 64};
            ModulationDelays = new byte[] {
                    64, 64, 64,
                    64, 64, 64,
                    64, 64, 64,
                    64, 64, 64,
                    64, 64, 64, 64};
            ModulationDepthRangesNote = new byte[] {
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0, 0};
            ModulationDepthRangesCent = new byte[] {
                    0x40, 0x40, 0x40,
                    0x40, 0x40, 0x40,
                    0x40, 0x40, 0x40,
                    0x40, 0x40, 0x40,
                    0x40, 0x40, 0x40, 0x40};
            Portamentos = new byte[] {
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0, 0};
            PortamentoTimes = new byte[] {
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0, 0};
            MonoMode = new byte[] {
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0,
                    0, 0, 0, 0};
            GPCS = new GeneralPurposeControlSettings[]{
                new GeneralPurposeControlSettings(),
                new GeneralPurposeControlSettings(),
                new GeneralPurposeControlSettings(),
                new GeneralPurposeControlSettings(),
                new GeneralPurposeControlSettings(),
                new GeneralPurposeControlSettings(),
                new GeneralPurposeControlSettings(),
                new GeneralPurposeControlSettings(),
                new GeneralPurposeControlSettings(),
                new GeneralPurposeControlSettings(),
                new GeneralPurposeControlSettings(),
                new GeneralPurposeControlSettings(),
                new GeneralPurposeControlSettings(),
                new GeneralPurposeControlSettings(),
                new GeneralPurposeControlSettings(),
                new GeneralPurposeControlSettings()
            };
            lastDateEntryType = new DataEntryType[]
            {
                DataEntryType.None,
                DataEntryType.None,
                DataEntryType.None,
                DataEntryType.None,
                DataEntryType.None,
                DataEntryType.None,
                DataEntryType.None,
                DataEntryType.None,
                DataEntryType.None,
                DataEntryType.None,
                DataEntryType.None,
                DataEntryType.None,
                DataEntryType.None,
                DataEntryType.None,
                DataEntryType.None,
                DataEntryType.None
            };
        }

        #region IDisposable Support

        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //マネージ状態を破棄します (マネージ オブジェクト)。

                }
                if (vgmRecording)
                    StopVgmRecordingInternal(UnitNumber, SoundInterfaceTagNamePrefix);

                // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                // TODO: 大きなフィールドを null に設定します。
                set_device_enable(UnitNumber, SoundInterfaceTagNamePrefix, 0);

                SetVstFxCallback(UnitNumber, SoundInterfaceTagNamePrefix, null);

                lock (InstrumentBase.VstPluginContextLockObject)
                {
                    foreach (var vp in VSTPlugins)
                        vp.Dispose();
                }

                if (vstHandle != null && vstHandle.IsAllocated)
                    vstHandle.Free();

                disposedValue = true;
            }
        }

        // TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
        ~InstrumentBase()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(false);
        }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public virtual void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(true);
            // TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
            GC.SuppressFinalize(this);
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        internal virtual void PrepareSound()
        {
            set_device_enable(UnitNumber, SoundInterfaceTagNamePrefix, 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        internal void NotifyMidiEvent(MidiEvent midiEvent)
        {
            OnMidiEvent(midiEvent);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected virtual void OnMidiEvent(MidiEvent midiEvent)
        {
            //TODO: key/ch pressure

            switch (midiEvent)
            {
                case SysExEvent sysex:
                    {
                        OnSystemExclusiveEvent(sysex);
                        break;
                    }
            }

            var ce = midiEvent as ChannelEvent;
            if (ce != null && Channels[ce.Channel])
            {
                switch (midiEvent)
                {
                    case NoteOnEvent non:
                        {
                            if (non.Velocity == 0)
                                OnNoteOffEvent(new NoteOffEvent(non.NoteNumber, (SevenBitNumber)0) { Channel = non.Channel, DeltaTime = non.DeltaTime });
                            else
                            {
                                if (FollowerMode != FollowerUnit.None)
                                    break;
                                OnNoteOnEvent(non);
                            }
                            break;
                        }
                    case NoteOffEvent noff:
                        {
                            OnNoteOffEvent(noff);
                            break;
                        }
                    case ControlChangeEvent cont:
                        {
                            OnControlChangeEvent(cont);
                            break;
                        }
                    case ProgramChangeEvent prog:
                        {
                            OnProgramChangeEvent(prog);
                            break;
                        }
                    case PitchBendEvent pitch:
                        {
                            OnPitchBendEvent(pitch);
                            break;
                        }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sysex"></param>
        protected virtual void OnSystemExclusiveEvent(SysExEvent sysex)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected virtual void OnNoteOnEvent(NoteOnEvent midiEvent)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected virtual void OnNoteOffEvent(NoteOffEvent midiEvent)
        {

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected virtual void OnControlChangeEvent(ControlChangeEvent midiEvent)
        {
            switch (midiEvent.ControlNumber)
            {
                case 1:    //Modulation
                    Modulations[midiEvent.Channel] = midiEvent.ControlValue;
                    break;
                case 6:    //Data Entry MSB
                    DataMsb[midiEvent.Channel] = midiEvent.ControlValue;

                    switch (lastDateEntryType[midiEvent.Channel])
                    {
                        case DataEntryType.Nrpn:
                            processNrpn(midiEvent, null);
                            break;
                        case DataEntryType.Rpn:
                            processRpn(midiEvent, null);
                            break;
                    }
                    break;
                case 38:    //Data Entry LSB
                    DataLsb[midiEvent.Channel] = midiEvent.ControlValue;

                    switch (lastDateEntryType[midiEvent.Channel])
                    {
                        case DataEntryType.Nrpn:
                            processNrpn(null, midiEvent);
                            break;
                        case DataEntryType.Rpn:
                            processRpn(null, midiEvent);
                            break;
                    }
                    break;
                case 5:    //Portamento Time
                    PortamentoTimes[midiEvent.Channel] = midiEvent.ControlValue;
                    break;
                case 7:    //Volume
                    Volumes[midiEvent.Channel] = midiEvent.ControlValue;
                    break;
                case 10:    //Panpot
                    Panpots[midiEvent.Channel] = midiEvent.ControlValue;
                    break;
                case 11:    //Expression
                    Expressions[midiEvent.Channel] = midiEvent.ControlValue;
                    break;
                case 65:    //Portamento
                    Portamentos[midiEvent.Channel] = midiEvent.ControlValue;
                    break;
                case 76:    //Modulation Rate
                    ModulationRates[midiEvent.Channel] = midiEvent.ControlValue;
                    break;
                case 77:    //Modulation Depth
                    ModulationDepthes[midiEvent.Channel] = midiEvent.ControlValue;
                    break;
                case 78:    //Modulation Delay
                    ModulationDelays[midiEvent.Channel] = midiEvent.ControlValue;
                    break;

                //Sound Control
                case 91:
                case 92:
                case 93:
                case 94:
                case 95:
                    {
                        foreach (var vp in VSTPlugins)
                        {
                            foreach (var pn in vp.VECCSS[midiEvent.Channel].GetProperties(vp, midiEvent.ControlNumber - 90))
                            {
                                float val = (float)midiEvent.ControlValue / (float)128;
                                vp.Settings.SetPropertyValue(pn, val);
                            }
                        }
                    }
                    break;

                case 96:    //Data Increment

                    break;

                case 97:    //Data Decrement

                    break;

                case 98:    //NRPN LSB
                    NrpnLsb[midiEvent.Channel] = midiEvent.ControlValue;
                    lastDateEntryType[midiEvent.Channel] = DataEntryType.Nrpn;
                    break;
                case 99:    //NRPN MSB
                    NrpnMsb[midiEvent.Channel] = midiEvent.ControlValue;
                    lastDateEntryType[midiEvent.Channel] = DataEntryType.Nrpn;
                    break;

                case 100:    //RPN LSB
                    RpnLsb[midiEvent.Channel] = midiEvent.ControlValue;
                    lastDateEntryType[midiEvent.Channel] = DataEntryType.Rpn;
                    break;
                case 101:    //RPN MSB
                    RpnMsb[midiEvent.Channel] = midiEvent.ControlValue;
                    lastDateEntryType[midiEvent.Channel] = DataEntryType.Rpn;
                    break;
                case 121:    //Reset All Controller
                    for (int i = 0; i < 16; i++)
                    {
                        Volumes[i] = 127;
                        Panpots[i] = 64;
                        Expressions[i] = 127;
                        PitchBendRanges[i] = 2;
                        Pitchs[i] = 8192;
                        Modulations[i] = 0;
                        ModulationRates[i] = 64;
                        ModulationDepthes[i] = 64;
                        ModulationDelays[i] = 64;
                        ModulationDepthRangesNote[i] = 0;
                        ModulationDepthRangesCent[i] = 0x40;
                        Portamentos[i] = 0;
                        PortamentoTimes[i] = 0;
                    }
                    break;
                case 126:    //MONO Mode
                    MonoMode[midiEvent.Channel] = midiEvent.ControlValue;
                    break;
                case 127:    //POLY Mode
                    MonoMode[midiEvent.Channel] = 0;
                    break;
            }
        }

        abstract internal void AllSoundOff();

        private void processNrpn(ControlChangeEvent dataMsb, ControlChangeEvent dataLsb)
        {
            if (dataMsb != null)
            {
                switch (NrpnMsb[dataMsb.Channel])
                {
                    default:
                        break;
                }
            }
            if (dataLsb != null)
            {
                switch (NrpnMsb[dataLsb.Channel])
                {
                    default:
                        break;
                }
            }
        }

        private void processRpn(ControlChangeEvent dataMsb, ControlChangeEvent dataLsb)
        {
            if (dataMsb != null)
            {
                switch (RpnMsb[dataMsb.Channel])
                {
                    case 0:
                        {
                            switch (RpnLsb[dataMsb.Channel])
                            {
                                case 0: //PitchBendRanges Half Note
                                    {
                                        PitchBendRanges[dataMsb.Channel] = dataMsb.ControlValue;
                                        break;
                                    }
                                case 5: //Mod Depth
                                    {
                                        ModulationDepthRangesNote[dataMsb.Channel] = dataMsb.ControlValue;
                                        break;
                                    }
                            }
                            break;
                        }
                }
            }
            if (dataLsb != null)
            {
                switch (RpnMsb[dataLsb.Channel])
                {
                    case 0:
                        {
                            switch (RpnLsb[dataLsb.Channel])
                            {
                                case 0: //PitchBendRanges Cent
                                    {
                                        break;
                                    }
                                case 5: //Mod Depth
                                    {
                                        ModulationDepthRangesCent[dataLsb.Channel] = dataLsb.ControlValue;
                                        break;
                                    }
                            }
                            break;
                        }
                }
            }
        }

        public static PropertyDescriptor GetPropertyDescriptor(PropertyInfo propertyInfo)
        {
            return TypeDescriptor.GetProperties(propertyInfo.DeclaringType)[propertyInfo.Name];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected virtual void OnProgramChangeEvent(ProgramChangeEvent midiEvent)
        {
            ProgramNumbers[midiEvent.Channel] = midiEvent.ProgramNumber;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected virtual void OnPitchBendEvent(PitchBendEvent midiEvent)
        {
            Pitchs[midiEvent.Channel] = midiEvent.PitchValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pn"></param>
        /// <param name="pos"></param>
        private void vst_fx_callback(IntPtr buffer, int samples)
        {
            if (samples == 0)
                return;

            int[][] buf = new int[2][] { new int[samples], new int[samples] };
            IntPtr[] pt = new IntPtr[] { Marshal.ReadIntPtr(buffer), Marshal.ReadIntPtr(buffer + IntPtr.Size) };
            Marshal.Copy(pt[0], buf[0], 0, samples);
            Marshal.Copy(pt[1], buf[1], 0, samples);

            using (VstAudioBufferManager bufA = new VstAudioBufferManager(2, samples))
            using (VstAudioBufferManager bufB = new VstAudioBufferManager(2, samples))
            {
                lock (InstrumentBase.VstPluginContextLockObject)
                {
                    bool processed = false;
                    foreach (var vp in VSTPlugins)
                    {
                        var ctx = vp.PluginContext;
                        if (ctx != null)
                        {
                            int idx = 0;
                            foreach (VstAudioBuffer vab in bufA)
                            {
                                Parallel.ForEach(Partitioner.Create(0, samples), range =>
                                {
                                    for (var i = range.Item1; i < range.Item2; i++)
                                        vab[i] = (float)buf[idx][i] / 32767.0f;
                                });
                                //for (int i = 0; i < samples; i++)
                                //    vab[i] = (float)buf[idx][i] / (float)int.MaxValue;
                                idx++;
                            }
                            break;
                        }
                    }

                    VstAudioBufferManager bufa = bufA;
                    VstAudioBufferManager bufb = bufA;
                    foreach (var vp in VSTPlugins)
                    {
                        var ctx = vp.PluginContext;
                        if (ctx != null)
                        {
                            ctx.Context.PluginCommandStub.SetBlockSize(samples);
                            ctx.Context.PluginCommandStub.ProcessReplacing(bufa.ToArray<VstAudioBuffer>(), bufb.ToArray<VstAudioBuffer>());
                            processed = true;
                        }
                        var tmp = bufa;
                        bufa = bufb;
                        bufb = tmp;
                    }

                    if (processed)
                    {
                        int idx = 0;
                        foreach (VstAudioBuffer vab in bufb)
                        {
                            Parallel.ForEach(Partitioner.Create(0, samples), range =>
                            {
                                for (var i = range.Item1; i < range.Item2; i++)
                                    buf[idx][i] = (int)(vab[i] * 32767.0f);
                            });
                            //for (int i = 0; i < samples; i++)
                            //    buf[idx][i] = (int)(vab[i] * int.MaxValue);
                            idx++;
                        }
                        Marshal.Copy(buf[0], 0, pt[0], samples);
                        Marshal.Copy(buf[1], 0, pt[1], samples);
                    }
                }
            }

        }

    }

    public enum FilterMode
    {
        None = 0,
        LowPass,
        HighPass,
        BandPass,
    };
}
