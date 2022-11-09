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

//https://en.wikipedia.org/wiki/POKEY
//http://ftp.pigwa.net/stuff/collections/SIO2SD_DVD/PC/RMT%201.19/docs/rmt_en.htm
//https://www.atariarchives.org/dere/chapt07.php
//http://user.xmission.com/~trevin/atari/pokey_regs.html

namespace zanac.MAmidiMEmo.Instruments.Chips
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    [InstLock]
    public class POKEY : InstrumentBase
    {

        public override string Name => "POKEY";

        public override string Group => "PSG";

        public override InstrumentType InstrumentType => InstrumentType.POKEY;

        [Browsable(false)]
        public override string ImageKey => "POKEY";

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        protected override string SoundInterfaceTagNamePrefix => "pokey_";

        private PokeyOutputType f_OutputType;

        [DataMember]
        [Category("Filter")]
        [Description("Set output type")]
        [DefaultValue(PokeyOutputType.LEGACY_LINEAR)]
        public PokeyOutputType OutputType
        {
            get
            {
                return f_OutputType;
            }
            set
            {
                if (f_OutputType != value)
                {
                    f_OutputType = value;

                    PokeySetOutputType(UnitNumber, f_OutputType, RegisterR, RegisterC, RegisterV);
                }
            }
        }

        private double f_RegisterR;

        [DataMember]
        [Category("Filter")]
        [Description("Set output type value R")]
        [DefaultValue(3.3 * 1e3)]
        [DoubleSlideParametersAttribute(0, 10 * 1e3, 0.1 * 1e3)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public double RegisterR
        {
            get
            {
                return f_RegisterR;
            }
            set
            {
                if (f_RegisterR != value)
                {
                    f_RegisterR = value;

                    PokeySetOutputType(UnitNumber, f_OutputType, f_RegisterR, f_RegisterC, f_RegisterV);
                }
            }
        }

        private double f_RegisterC;

        [DataMember]
        [Category("Filter")]
        [Description("Set output type value C")]
        [DefaultValue(0.01 * 1e-6)]
        [DoubleSlideParametersAttribute(0, 0.1 * 1e-6, 0.001 * 1e-6)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public double RegisterC
        {
            get
            {
                return f_RegisterC;
            }
            set
            {
                if (f_RegisterC != value)
                {
                    f_RegisterC = value;

                    PokeySetOutputType(UnitNumber, f_OutputType, f_RegisterR, f_RegisterC, f_RegisterV);
                }
            }
        }

        private double f_RegisterV;

        [DataMember]
        [Category("Filter")]
        [Description("Set output type value V")]
        [DefaultValue(5.0)]
        [DoubleSlideParametersAttribute(0, 10, 1)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public double RegisterV
        {
            get
            {
                return f_RegisterV;
            }
            set
            {
                if (f_RegisterV != value)
                {
                    f_RegisterV = value;

                    PokeySetOutputType(UnitNumber, f_OutputType, f_RegisterR, f_RegisterC, f_RegisterV);
                }
            }
        }

        private PokeyClock f_Clock = PokeyClock.NTSC;

        [DataMember]
        [Category("Chip")]
        [Description("Set clock")]
        [DefaultValue(PokeyClock.NTSC)]
        public PokeyClock Clock
        {
            get
            {
                return f_Clock;
            }
            set
            {
                if (f_Clock != value)
                {
                    f_Clock = value;

                    SetClock(UnitNumber, (uint)f_Clock);
                }
            }
        }

        private byte f_AC_15kHz;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("Chip")]
        [Description("AUDCTL Choice of frequency divider rate \"0\" - 64 kHz, \"1\" - 15 kHz 1 (0-1)")]
        [SlideParametersAttribute(0, 1)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((byte)0)]
        public byte AC_15kHz
        {
            get => f_AC_15kHz;
            set
            {
                if (f_AC_15kHz != value)
                {
                    f_AC_15kHz = (byte)(value & 1);
                    var data = PokeyReadData(UnitNumber, 8) & 0xfe;
                    PokeyWriteData(UnitNumber, 8, (byte)(data | f_AC_15kHz));
                }
            }
        }


        private byte f_AC_POLY9;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("Chip")]
        [Description("AUDCTL Switch shift register \"0\" - 17-bit, \"1\" – 9-bit(0-1)")]
        [SlideParametersAttribute(0, 1)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((byte)0)]
        public byte AC_POLY9
        {
            get => f_AC_POLY9;
            set
            {
                if (f_AC_POLY9 != value)
                {
                    f_AC_POLY9 = (byte)(value & 1);
                    var data = PokeyReadData(UnitNumber, 8) & 0x7f;
                    PokeyWriteData(UnitNumber, 8, (byte)((f_AC_POLY9 << 7) | data));
                }
            }
        }

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
                return 18;
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
        public PokeyTimbre[] Timbres
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
                using (var obj = JsonConvert.DeserializeObject<POKEY>(serializeData))
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
        private delegate void delegate_pokey_write(uint unitNumber, int address, byte data);


        /// <summary>
        /// 
        /// </summary>
        private static delegate_pokey_write Pokey_write
        {
            get;
            set;
        }

        private Dictionary<int, byte> writeData = new Dictionary<int, byte>();

        /// <summary>
        /// 
        /// </summary>
        private void PokeyWriteData(uint unitNumber, int address, byte data)
        {
            DeferredWriteData(Pokey_write, unitNumber, address, data);
            writeData[address] = data;
            /*
            try
            {
                Program.SoundUpdating();
                Pokey_write(unitNumber, address, data);
                writeData[address] = data;
            }
            finally
            {
                Program.SoundUpdated();
            }*/
        }

        /// <summary>
        /// 
        /// </summary>
        private byte PokeyReadData(uint unitNumber, int address)
        {
            try
            {
                //Program.SoundUpdating();
                FlushDeferredWriteData();

                if (writeData.ContainsKey(address))
                    return writeData[address];
                else
                    return 0;
            }
            finally
            {
                //Program.SoundUpdated();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unitNumber"></param>
        /// <param name="type"></param>
        /// <param name="r"></param>
        /// <param name="c"></param>
        /// <param name="v"></param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delegate_set_output_type(uint unitNumber, PokeyOutputType type, double r, double c, double v);


        /// <summary>
        /// 
        /// </summary>
        private static delegate_set_output_type Pokey_set_output_type
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        private void PokeySetOutputType(uint unitNumber, PokeyOutputType type, double r, double c, double v)
        {
            DeferredWriteData(Pokey_set_output_type, unitNumber, type, r, c, v);
            /*
            try
            {
                Program.SoundUpdating();
                Pokey_set_output_type(unitNumber, type, r, c, v);
            }
            finally
            {
                Program.SoundUpdated();
            }*/
        }


        /// <summary>
        /// 
        /// </summary>
        static POKEY()
        {
            IntPtr funcPtr = MameIF.GetProcAddress("pokey_write");
            if (funcPtr != IntPtr.Zero)
                Pokey_write = (delegate_pokey_write)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_pokey_write));
            funcPtr = MameIF.GetProcAddress("pokey_set_output_type");
            if (funcPtr != IntPtr.Zero)
                Pokey_set_output_type = (delegate_set_output_type)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_set_output_type));
        }

        private PokeySoundManager soundManager;

        private const float DEFAULT_GAIN = 0.8f;

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

        [Browsable(false)]
        public override FollowerUnit FollowerMode
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public POKEY(uint unitNumber) : base(unitNumber)
        {
            GainLeft = DEFAULT_GAIN;
            GainRight = DEFAULT_GAIN;

            Timbres = new PokeyTimbre[InstrumentBase.DEFAULT_MAX_TIMBRES];
            for (int i = 0; i < InstrumentBase.DEFAULT_MAX_TIMBRES; i++)
                Timbres[i] = new PokeyTimbre();
            setPresetInstruments();

            OutputType = PokeyOutputType.LEGACY_LINEAR;
            RegisterR = 3.3 * 1e3;
            RegisterC = 0.01 * 1e-6;
            RegisterV = 5.0;

            this.soundManager = new PokeySoundManager(this);
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
            Timbres[0].Channel = ChannelType.CH1;
        }

        internal override void PrepareSound()
        {
            base.PrepareSound();

            initGlobalRegisters();
        }

        /// <summary>
        /// 
        /// </summary>
        private void initGlobalRegisters()
        {
            PokeySetOutputType(UnitNumber, f_OutputType, f_RegisterR, f_RegisterC, f_RegisterV);
            PokeyWriteData(UnitNumber, 8, (byte)((f_AC_POLY9 << 7) | f_AC_15kHz));
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
        private class PokeySoundManager : SoundManagerBase
        {
            private SoundList<PokeySound> ch1OnSounds = new SoundList<PokeySound>(1);

            private SoundList<PokeySound> ch2OnSounds = new SoundList<PokeySound>(1);

            private SoundList<PokeySound> ch3OnSounds = new SoundList<PokeySound>(1);

            private SoundList<PokeySound> ch4OnSounds = new SoundList<PokeySound>(1);

            private POKEY parentModule;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            public PokeySoundManager(POKEY parent) : base(parent)
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
                foreach (PokeyTimbre timbre in parentModule.GetBaseTimbres(note))
                {
                    tindex++;
                    int emptySlot = searchEmptySlot(note, timbre);
                    if (emptySlot < 0)
                        continue;

                    PokeySound snd = new PokeySound(parentModule, this, timbre, tindex - 1, note, emptySlot);
                    switch (((PokeyTimbre)timbre).Channel)
                    {
                        case ChannelType.CH1:
                            ch1OnSounds.Add(snd);
                            break;
                        case ChannelType.CH2:
                            ch2OnSounds.Add(snd);
                            break;
                        case ChannelType.CH3:
                            ch3OnSounds.Add(snd);
                            break;
                        case ChannelType.CH4:
                            ch4OnSounds.Add(snd);
                            break;
                    }
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
            private int searchEmptySlot(TaggedNoteOnEvent note, PokeyTimbre timbre)
            {
                int emptySlot = -1;

                switch (((PokeyTimbre)timbre).Channel)
                {
                    case ChannelType.CH1:
                        emptySlot = SearchEmptySlotAndOff(parentModule, ch1OnSounds, note, 1);
                        break;
                    case ChannelType.CH2:
                        emptySlot = SearchEmptySlotAndOff(parentModule, ch2OnSounds, note, 1);
                        break;
                    case ChannelType.CH3:
                        emptySlot = SearchEmptySlotAndOff(parentModule, ch3OnSounds, note, 1);
                        break;
                    case ChannelType.CH4:
                        emptySlot = SearchEmptySlotAndOff(parentModule, ch4OnSounds, note, 1);
                        break;
                }

                return emptySlot;
            }

            internal override void ProcessAllSoundOff()
            {
                var me = new ControlChangeEvent((SevenBitNumber)120, (SevenBitNumber)0);
                ProcessControlChange(me);

                for (int i = 0; i < 4; i++)
                    parentModule.PokeyWriteData(parentModule.UnitNumber, ((int)i * 2) + 1, (byte)(0));
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private class PokeySound : SoundBase
        {

            private POKEY parentModule;

            private PokeyTimbre timbre;

            private ChannelType lastChType;

            private byte lastCH43_21;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentModule"></param>
            /// <param name="noteOnEvent"></param>
            /// <param name="programNumber"></param>
            /// <param name="slot"></param>
            public PokeySound(POKEY parentModule, PokeySoundManager manager, TimbreBase timbre, int tindex, TaggedNoteOnEvent noteOnEvent, int slot) : base(parentModule, manager, timbre, tindex, noteOnEvent, slot)
            {
                this.parentModule = parentModule;
                this.timbre = (PokeyTimbre)timbre;

                lastChType = this.timbre.Channel;
                lastCH43_21 = this.timbre.CH43_21;
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
                    if (gs.Clock.HasValue)
                        parentModule.Clock = gs.Clock.Value;
                    if (gs.AC_15kHz.HasValue)
                        parentModule.AC_15kHz = gs.AC_15kHz.Value;
                    if (gs.AC_POLY9.HasValue)
                        parentModule.AC_POLY9 = gs.AC_POLY9.Value;
                }

                OnVolumeUpdated();

                OnPitchUpdated();
            }

            public override void OnSoundParamsUpdated()
            {
                var gs = timbre.GlobalSettings;
                if (gs.Enable)
                {
                    if (gs.OutputType.HasValue)
                        parentModule.OutputType = gs.OutputType.Value;
                    if (gs.RegisterR.HasValue)
                        parentModule.RegisterR = gs.RegisterR.Value;
                    if (gs.RegisterC.HasValue)
                        parentModule.RegisterC = gs.RegisterC.Value;
                    if (gs.RegisterV.HasValue)
                        parentModule.RegisterV = gs.RegisterV.Value;

                    if (gs.Clock.HasValue)
                        parentModule.Clock = gs.Clock.Value;
                    if (gs.AC_15kHz.HasValue)
                        parentModule.AC_15kHz = gs.AC_15kHz.Value;
                    if (gs.AC_POLY9.HasValue)
                        parentModule.AC_POLY9 = gs.AC_POLY9.Value;
                }

                base.OnSoundParamsUpdated();
            }

            /// <summary>
            /// 
            /// </summary>
            public override void OnVolumeUpdated()
            {
                if (IsSoundOff)
                    return;

                updateVolume();
            }

            /// <summary>
            /// 
            /// </summary>
            private void updateVolume()
            {
                byte fv = (byte)Math.Round(15 * CalcCurrentVolume());

                var tt = timbre.ToneType;
                if (FxEngine != null && FxEngine.Active)
                {
                    var eng = (PokeyFxEngine)FxEngine;
                    if (eng.ToneValue != null)
                        tt = (byte)(eng.ToneValue.Value & 7);
                }

                if (lastCH43_21 == 0)
                    parentModule.PokeyWriteData(parentModule.UnitNumber, ((int)lastChType * 2) + 1, (byte)((tt << 5) | fv));
                else
                {
                    parentModule.PokeyWriteData(parentModule.UnitNumber, (((int)lastChType / 2) * 2) + 1, (byte)((tt << 5) | fv));
                    parentModule.PokeyWriteData(parentModule.UnitNumber, (((int)lastChType / 2) * 2) + 3, (byte)((tt << 5) | fv));
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public override void OnPitchUpdated()
            {
                updatePitch();

                base.OnPitchUpdated();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="slot"></param>
            private void updatePitch()
            {
                double freq = CalcCurrentFrequency();
                if (lastCH43_21 == 0)
                {
                    switch (lastChType)
                    {
                        case ChannelType.CH1:
                            {
                                if (timbre.CH3_1_179 == 0)
                                    freq = Math.Round((parentModule.AC_15kHz == 1 ? 15700 : 63920) / freq / 2 - 1);
                                else
                                    freq = Math.Round((int)parentModule.f_Clock / freq / 2 - 4);

                                if (freq > 0xff)
                                    freq = 0xff;
                                var n = (ushort)freq;
                                parentModule.PokeyWriteData(parentModule.UnitNumber, ((int)lastChType * 2), (byte)(freq));
                            }
                            break;
                        case ChannelType.CH2:
                            {
                                freq = Math.Round((parentModule.AC_15kHz == 1 ? 15700 : 63920) / freq / 2 - 1);
                                if (freq > 0xff)
                                    freq = 0xff;
                                var n = (ushort)freq;
                                parentModule.PokeyWriteData(parentModule.UnitNumber, ((int)lastChType * 2), (byte)(freq));
                            }
                            break;
                        case ChannelType.CH3:
                            {
                                if (timbre.CH3_1_179 == 0)
                                    freq = Math.Round((parentModule.AC_15kHz == 1 ? 15700 : 63920) / freq / 2 - 1);
                                else
                                    freq = Math.Round((int)parentModule.f_Clock / freq / 2 - 4);

                                if (freq > 0xff)
                                    freq = 0xff;
                                var n = (ushort)freq;
                                parentModule.PokeyWriteData(parentModule.UnitNumber, ((int)lastChType * 2), (byte)(freq));
                            }
                            break;
                        case ChannelType.CH4:
                            {
                                freq = Math.Round((parentModule.AC_15kHz == 1 ? 15700 : 63920) / freq / 2 - 1);
                                if (freq > 0xff)
                                    freq = 0xff;
                                var n = (ushort)freq;
                                parentModule.PokeyWriteData(parentModule.UnitNumber, ((int)lastChType * 2), (byte)(freq));
                            }
                            break;
                    }
                }
                else
                {
                    switch (lastChType)
                    {
                        case ChannelType.CH1:
                            {
                                if (timbre.CH3_1_179 == 0)
                                    freq = Math.Round((parentModule.AC_15kHz == 1 ? 15700 : 63920) / freq / 2 - 1);
                                else
                                    freq = Math.Round((int)parentModule.f_Clock / freq / 2 - 7);
                                if (freq > 0xffff)
                                    freq = 0xffff;
                                var n = (ushort)freq;
                                parentModule.PokeyWriteData(parentModule.UnitNumber, 2, (byte)(n >> 8));
                                parentModule.PokeyWriteData(parentModule.UnitNumber, 0, (byte)(n & 0xff));
                            }
                            break;
                        case ChannelType.CH2:
                            {
                                if (lastCH43_21 == 1 && timbre.CH3_1_179 == 1)
                                    freq = Math.Round((int)parentModule.f_Clock / freq / 2 - 7);
                                else
                                    freq = Math.Round((parentModule.AC_15kHz == 1 ? 15700 : 63920) / freq / 2 - 1);
                                if (freq > 0xffff)
                                    freq = 0xffff;
                                var n = (ushort)freq;
                                parentModule.PokeyWriteData(parentModule.UnitNumber, 0, (byte)(n & 0xff));
                                parentModule.PokeyWriteData(parentModule.UnitNumber, 2, (byte)(n >> 8));
                            }
                            break;
                        case ChannelType.CH3:
                            {
                                if (timbre.CH3_1_179 == 0)
                                    freq = Math.Round((parentModule.AC_15kHz == 1 ? 15700 : 63920) / freq / 2 - 1);
                                else
                                    freq = Math.Round((int)parentModule.f_Clock / freq / 2 - 7);
                                if (freq > 0xffff)
                                    freq = 0xffff;
                                var n = (ushort)freq;
                                parentModule.PokeyWriteData(parentModule.UnitNumber, 4, (byte)(n & 0xff));
                                parentModule.PokeyWriteData(parentModule.UnitNumber, 6, (byte)(n >> 8));
                            }
                            break;
                        case ChannelType.CH4:
                            {
                                if (lastCH43_21 == 1 && timbre.CH3_1_179 == 1)
                                    freq = Math.Round((int)parentModule.f_Clock / freq / 2 - 7);
                                else
                                    freq = Math.Round((parentModule.AC_15kHz == 1 ? 15700 : 63920) / freq / 2 - 1);
                                if (freq > 0xffff)
                                    freq = 0xffff;
                                var n = (ushort)freq;
                                parentModule.PokeyWriteData(parentModule.UnitNumber, 4, (byte)(n & 0xff));
                                parentModule.PokeyWriteData(parentModule.UnitNumber, 6, (byte)(n >> 8));
                            }
                            break;
                    }
                }
                {
                    var data = parentModule.PokeyReadData(parentModule.UnitNumber, 8);

                    if (lastChType == ChannelType.CH2 || lastChType == ChannelType.CH4)
                    {
                        data &= 0xff - 2;
                        data |= (byte)(timbre.FI24_13 << 1);

                    }
                    else if (lastChType == ChannelType.CH1 || lastChType == ChannelType.CH3)
                    {
                        data &= 0xff - 4;
                        data |= (byte)(timbre.FI24_13 << 2);
                    }

                    if (lastChType == ChannelType.CH4 || lastChType == ChannelType.CH3)
                    {
                        data &= 0xff - 8;
                        data |= (byte)(lastCH43_21 << 3);
                    }
                    else if (lastChType == ChannelType.CH2 || lastChType == ChannelType.CH1)
                    {
                        data &= 0xff - 16;
                        data |= (byte)(lastCH43_21 << 4);
                    }

                    if (lastChType == ChannelType.CH3 || (lastCH43_21 == 1 && lastChType == ChannelType.CH4))
                    {
                        data &= 0xff - 32;
                        data |= (byte)(timbre.CH3_1_179 << 5);
                    }
                    else if (lastChType == ChannelType.CH1 || (lastCH43_21 == 1 && lastChType == ChannelType.CH2))
                    {
                        data &= 0xff - 64;
                        data |= (byte)(timbre.CH3_1_179 << 6);
                    }
                    parentModule.PokeyWriteData(parentModule.UnitNumber, 8, data);
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public override void SoundOff()
            {
                base.SoundOff();

                var tt = timbre.ToneType;
                if (lastCH43_21 == 0)
                    parentModule.PokeyWriteData(parentModule.UnitNumber, ((int)lastChType * 2) + 1, (byte)((tt << 5) | 0));
                else
                {
                    parentModule.PokeyWriteData(parentModule.UnitNumber, (((int)lastChType / 2) * 2) + 1, (byte)((tt << 5) | 0));
                    parentModule.PokeyWriteData(parentModule.UnitNumber, (((int)lastChType / 2) * 2) + 3, (byte)((tt << 5) | 0));
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [JsonConverter(typeof(NoTypeConverterJsonConverter<PokeyTimbre>))]
        [DataContract]
        [InstLock]
        public class PokeyTimbre : TimbreBase
        {
            [DataMember]
            [Category("Sound")]
            [Description("Sound Type")]
            [DefaultValue(ChannelType.CH1)]
            public ChannelType Channel
            {
                get;
                set;
            }


            private byte f_ToneType = 7;

            [DataMember]
            [Category("Sound")]
            [Description("Shift register settings for noises/distortion (0-7)\r\n" +
                "0  5-bit then 17-bit polynomials\r\n" +
                "1  5-bit poly only\r\n" +
                "2  5-bit then 4-bit polys\r\n" +
                "3  (Same as 1)\r\n" +
                "4  17-bit poly only\r\n" +
                "5  (Same as 7)\r\n" +
                "6  4-bit poly only\r\n" +
                "7  No poly(pure tone)\r\n")]
            [SlideParametersAttribute(0, 7)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue((byte)7)]
            public byte ToneType
            {
                get
                {
                    return f_ToneType;
                }
                set
                {
                    f_ToneType = (byte)(value & 7);
                }
            }


            private byte f_FI;

            [DataMember]
            [Category("Sound")]
            [DisplayName("FI2+4/FI1+3[FI24_13]")]
            [Description("AUDCTL High-pass filter for channel 2 or 1 rated by frequency of channel 4 or 3 (0-1)")]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue((byte)0)]
            public byte FI24_13
            {
                get
                {
                    return f_FI;
                }
                set
                {
                    f_FI = (byte)(value & 1);
                }
            }

            private byte f_CH43_21;

            [DataMember]
            [Category("Sound")]
            [DisplayName("CH4+3/CH2+1[CH43_21]")]
            [Description("AUDCTL Connection of dividers 4+3 or 2+1 to obtain 16-bit accuracy (0-1)")]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue((byte)0)]
            public byte CH43_21
            {
                get
                {
                    return f_CH43_21;
                }
                set
                {
                    f_CH43_21 = (byte)(value & 1);
                }
            }


            private byte f_CH3_1_179;

            [DataMember]
            [Category("Sound")]
            [DisplayName("CH3/CH1 1.79[CH3_1_179]")]
            [Description("AUDCTL Set channel 3 or 1 frequency. \"0\" is 64 kHz. \"1\" is 1.79 MHz NTSC or 1.77 MHz PAL (0-1)")]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue((byte)0)]
            public byte CH3_1_179
            {
                get
                {
                    return f_CH3_1_179;
                }
                set
                {
                    f_CH3_1_179 = (byte)(value & 1);
                }
            }


            [DataMember]
            [Category("Chip")]
            [Description("Global Settings")]
            public PokeyGlobalSettings GlobalSettings
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
                GlobalSettings.InjectFrom(new LoopInjection(), new PokeyGlobalSettings());
            }

            /// <summary>
            /// 
            /// </summary>
            public PokeyTimbre()
            {
                GlobalSettings = new PokeyGlobalSettings();
            }

            protected override void InitializeFxS()
            {
                SDS.FxS = new PokeyFxSettings();
            }

            public override void RestoreFrom(string serializeData)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<PokeyTimbre>(serializeData);
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


        [JsonConverter(typeof(NoTypeConverterJsonConverter<PokeyFxSettings>))]
        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [DataContract]
        [InstLock]
        public class PokeyFxSettings : BasicFxSettings
        {

            private string f_ToneEnvelopes;

            [DataMember]
            [Description("Set ToneType envelop by text. Input ToneType value and split it with space like the FamiTracker.\r\n" +
                       "0-7 \"|\" is repeat point. \"/\" is release point.")]
            [Editor(typeof(EnvelopeUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [EnvelopeEditorAttribute(0, 7)]
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
                                    else if (v > 7)
                                        v = 7;
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
                return new PokeyFxEngine(this);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public class PokeyFxEngine : BasicFxEngine
        {
            private PokeyFxSettings settings;

            /// <summary>
            /// 
            /// </summary>
            public PokeyFxEngine(PokeyFxSettings settings) : base(settings)
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
                        //if (settings.ToneEnvelopesReleasePoint < 0)
                        //    f_toneCounter = (uint)settings.ToneEnvelopesNums.Length;

                        if (f_toneCounter >= settings.ToneEnvelopesNums.Length)
                        {
                            if (settings.ToneEnvelopesReleasePoint >= 0 && f_toneCounter < (uint)settings.ToneEnvelopesReleasePoint)
                                f_toneCounter = (uint)settings.ToneEnvelopesReleasePoint;
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


        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [JsonConverter(typeof(NoTypeConverterJsonConverter<PokeyGlobalSettings>))]
        [DataContract]
        [InstLock]
        public class PokeyGlobalSettings : ContextBoundObject
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

            private PokeyOutputType? f_OutputType;

            [DataMember]
            [Category("Filter")]
            [Description("Set output type")]
            [DefaultValue(null)]
            public PokeyOutputType? OutputType
            {
                get
                {
                    return f_OutputType;
                }
                set
                {
                    if (f_OutputType != value)
                    {
                        f_OutputType = value;
                    }
                }
            }

            private double? f_RegisterR;

            [DataMember]
            [Category("Filter")]
            [Description("Set output type value R")]
            [DefaultValue(null)]
            [DoubleSlideParametersAttribute(0, 10 * 1e3, 0.1 * 1e3)]
            [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public double? RegisterR
            {
                get
                {
                    return f_RegisterR;
                }
                set
                {
                    if (f_RegisterR != value)
                    {
                        f_RegisterR = value;
                    }
                }
            }

            private double? f_RegisterC;

            [DataMember]
            [Category("Filter")]
            [Description("Set output type value C")]
            [DefaultValue(null)]
            [DoubleSlideParametersAttribute(0, 0.1 * 1e-6, 0.001 * 1e-6)]
            [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public double? RegisterC
            {
                get
                {
                    return f_RegisterC;
                }
                set
                {
                    if (f_RegisterC != value)
                    {
                        f_RegisterC = value;
                    }
                }
            }

            private double? f_RegisterV;

            [DataMember]
            [Category("Filter")]
            [Description("Set output type value V")]
            [DefaultValue(null)]
            [DoubleSlideParametersAttribute(0, 10, 1)]
            [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public double? RegisterV
            {
                get
                {
                    return f_RegisterV;
                }
                set
                {
                    if (f_RegisterV != value)
                    {
                        f_RegisterV = value;
                    }
                }
            }

            private PokeyClock? f_Clock = PokeyClock.NTSC;


            [DataMember]
            [Category("Chip")]
            [Description("Set clock")]
            [DefaultValue(null)]
            public PokeyClock? Clock
            {
                get
                {
                    return f_Clock;
                }
                set
                {
                    if (f_Clock != value)
                    {
                        f_Clock = value;
                    }
                }
            }

            private byte f_AC_15kHz;

            /// <summary>
            /// 
            /// </summary>
            [DataMember]
            [Category("Chip")]
            [Description("AUDCTL Choice of frequency divider rate \"0\" - 64 kHz, \"1\" - 15 kHz 1 (0-1)")]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue(null)]
            public byte? AC_15kHz
            {
                get => f_AC_15kHz;
                set
                {
                    if (f_AC_15kHz != value)
                    {
                        f_AC_15kHz = (byte)(value & 1);
                    }
                }
            }


            private byte? f_AC_POLY9;

            /// <summary>
            /// 
            /// </summary>
            [DataMember]
            [Category("Chip")]
            [Description("AUDCTL Switch shift register \"0\" - 17-bit, \"1\" – 9-bit(0-1)")]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue(null)]
            public byte? AC_POLY9
            {
                get => f_AC_POLY9;
                set
                {
                    if (f_AC_POLY9 != value)
                    {
                        f_AC_POLY9 = (byte)(value & 1);
                    }
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public enum ChannelType
        {
            CH1 = 0,
            CH2 = 1,
            CH3 = 2,
            CH4 = 3,
        }


        public enum PokeyClock
        {
            NTSC = 1789790,
            PAL = 1773447,
        }

        public enum PokeyOutputType
        {
            LEGACY_LINEAR = 0,
            RC_LOWPASS,
            OPAMP_C_TO_GROUND,
            OPAMP_LOW_PASS,
            DISCRETE_VAR_R
        }
    }
}