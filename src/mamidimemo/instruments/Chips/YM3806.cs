// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
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
using zanac.MAmidiMEmo.Instruments.Chips.YM;
using zanac.MAmidiMEmo.Instruments.Envelopes;
using zanac.MAmidiMEmo.Mame;
using zanac.MAmidiMEmo.Midi;
using zanac.MAmidiMEmo.Scci;
using static zanac.MAmidiMEmo.Instruments.Chips.YM2610B;

//https://www16.atwiki.jp/mxdrv/pages/24.html
//http://map.grauw.nl/resources/sound/yamaha_ym3806_synthesis.pdf

namespace zanac.MAmidiMEmo.Instruments.Chips
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    [InstLock]
    public class YM3806 : InstrumentBase
    {

        public override string Name => "YM3806";

        public override string Group => "FM";

        public override InstrumentType InstrumentType => InstrumentType.YM3806;

        [Browsable(false)]
        public override string ImageKey => "YM3806";

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        protected override string SoundInterfaceTagNamePrefix => "ymfm_opq_";

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
                return 30;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public enum MasterClockType : uint
        {
            Default = 3579545,
        }

        private uint f_MasterClock;

        /// <summary>
        /// </summary>
        [DataMember]
        [Category("Chip(Dedicated)")]
        [Description("Set Master Clock of this chip")]
        [TypeConverter(typeof(EnumConverter<MasterClockType>))]
        public uint MasterClock
        {
            get
            {
                return f_MasterClock;
            }
            set
            {
                if (f_MasterClock != value)
                {
                    f_MasterClock = value;
                    SetClock(UnitNumber, (uint)value);
                }
            }
        }

        public bool ShouldSerializeMasterClock()
        {
            return MasterClock != (uint)MasterClockType.Default;
        }

        public void ResetMasterClock()
        {
            MasterClock = (uint)MasterClockType.Default;
        }

        private byte f_LFOE;

        /// <summary>
        /// LFO Enable (0:Disable 1:Enable)
        /// </summary>
        [Browsable(false)]
        [DataMember]
        [Category("Chip(Global)")]
        [Description("LFO Enable (0:Disable 1:Enable)")]
        [SlideParametersAttribute(0, 1)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((byte)0)]
        public byte LFOE
        {
            get
            {
                return f_LFOE;
            }
            set
            {
                byte v = (byte)(value & 1);
                if (f_LFOE != v)
                {
                    f_LFOE = v;
                    Ym3806WriteData(UnitNumber, 0x04, 0, 0, (byte)(LFOE << 3 | LFRQ));
                }
            }
        }

        private byte f_LFRQ;

        /// <summary>
        /// LFRQ (0-7)
        /// </summary>
        [DataMember]
        [Category("Chip(Global)")]
        [Description("LFO Freq (0-7)")]
        [SlideParametersAttribute(0, 7)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((byte)0)]
        public byte LFRQ
        {
            get
            {
                return f_LFRQ;
            }
            set
            {
                byte v = (byte)(value & 7);
                if (f_LFRQ != v)
                {
                    f_LFRQ = v;
                    Ym3806WriteData(UnitNumber, 0x04, 0, 0, (byte)(LFOE << 3 | LFRQ));
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
                Timbres = (YM3806Timbre[])value;
            }
        }

        [DataMember]
        [Category(" Timbres")]
        [Description("Timbres")]
        [EditorAttribute(typeof(YM3806UITypeEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(ExpandableCollectionConverter))]
        public YM3806Timbre[] Timbres
        {
            get;
            set;
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
        /// <param name="serializeData"></param>
        public override void RestoreFrom(string serializeData)
        {
            try
            {
                using (var obj = JsonConvert.DeserializeObject<YM3806>(serializeData))
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
        private delegate void delegate_ym3806_write(uint unitNumber, uint address, byte data);


        /// <summary>
        /// 
        /// </summary>
        private static delegate_ym3806_write Ym3806_write
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        private void Ym3806WriteData(uint unitNumber, byte address, int op, int slot, byte data)
        {
            Ym3806WriteData(unitNumber, address, op, slot, data, true);
        }

        /// <summary>
        /// 
        /// </summary>
        private void Ym3806WriteData(uint unitNumber, byte address, int op, int slot, byte data, bool useCache)
        {
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
            byte adr = (byte)(address + (op * 8) + slot);

            WriteData(adr, data, useCache, new Action(() =>
            {
                DeferredWriteData(Ym3806_write, unitNumber, (uint)adr, data);
                //DeferredWriteData(Ym3806_write, unitNumber, (uint)0, adr);
                //DeferredWriteData(Ym3806_write, unitNumber, (uint)1, data);
            }));
            /*
            try
            {
                Program.SoundUpdating();

                Ym3806_write(unitNumber, 0, (byte)(address + (op * 8) + slot));
                Ym3806_write(unitNumber, 1, data);
            }
            finally
            {
                Program.SoundUpdated();
            }*/
        }


        /// <summary>
        /// 
        /// </summary>
        static YM3806()
        {
            IntPtr funcPtr = MameIF.GetProcAddress("ymfm_opq_write");
            if (funcPtr != IntPtr.Zero)
            {
                Ym3806_write = (delegate_ym3806_write)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_ym3806_write));
            }
        }

        private YM3806SoundManager soundManager;

        /// <summary>
        /// 
        /// </summary>
        public YM3806(uint unitNumber) : base(unitNumber)
        {
            SetDevicePassThru(false);

            MasterClock = (uint)MasterClockType.Default;

            GainLeft = DEFAULT_GAIN;
            GainRight = DEFAULT_GAIN;

            Timbres = new YM3806Timbre[InstrumentBase.DEFAULT_MAX_TIMBRES];
            for (int i = 0; i < InstrumentBase.DEFAULT_MAX_TIMBRES; i++)
                Timbres[i] = new YM3806Timbre();

            setPresetInstruments();

            this.soundManager = new YM3806SoundManager(this);
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
            Timbres[0].REV = 0;
            Timbres[0].FB = 0;
            Timbres[0].ALG = 3;
            Timbres[0].PitchShift13 = 0;
            Timbres[0].PitchShift24 = 0;

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
            Timbres[0].Ops[3].TL = 24;
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
            Ym3806WriteData(UnitNumber, 0x03, 0, 0, (byte)(LFOE << 3 | LFRQ));
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
            soundManager?.ProcessAllSoundOff();
            ClearWrittenDataCache();
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
        private class YM3806SoundManager : SoundManagerBase
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

            private static SoundList<YM3806Sound> fmOnSounds = new SoundList<YM3806Sound>(8);

            private YM3806 parentModule;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            public YM3806SoundManager(YM3806 parent) : base(parent)
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
                foreach (YM3806Timbre timbre in parentModule.GetBaseTimbres(note))
                {
                    tindex++;
                    var emptySlot = searchEmptySlot(note);
                    if (emptySlot.slot < 0)
                        continue;

                    YM3806Sound snd = new YM3806Sound(emptySlot.inst, this, timbre, tindex - 1, note, emptySlot.slot);
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
            private (YM3806 inst, int slot) searchEmptySlot(TaggedNoteOnEvent note)
            {
                return SearchEmptySlotAndOffForLeader(parentModule, fmOnSounds, note, 8);
            }

            internal override void ProcessAllSoundOff()
            {
                var me = new ControlChangeEvent((SevenBitNumber)120, (SevenBitNumber)0);
                ProcessControlChange(me);

                for (int i = 0; i < 8; i++)
                {
                    parentModule.Ym3806WriteData(parentModule.UnitNumber, 0x05, 0, 0, (byte)(0x00 | i));

                    for (int op = 0; op < 4; op++)
                        parentModule.Ym3806WriteData(parentModule.UnitNumber, 0x60, op, i, 127);
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private class YM3806Sound : SoundBase
        {
            private YM3806 parentModule;

            private uint unitNumber;

            private YM3806Timbre timbre;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentModule"></param>
            /// <param name="noteOnEvent"></param>
            /// <param name="programNumber"></param>
            /// <param name="slot"></param>
            public YM3806Sound(YM3806 parentModule, YM3806SoundManager manager, TimbreBase timbre, int tindex, TaggedNoteOnEvent noteOnEvent, int slot) : base(parentModule, manager, timbre, tindex, noteOnEvent, slot)
            {
                this.parentModule = parentModule;
                this.timbre = (YM3806Timbre)timbre;

                this.unitNumber = parentModule.UnitNumber;
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
                    if (gs.LFOE.HasValue)
                        parentModule.LFOE = gs.LFOE.Value;
                    if (gs.LFRQ.HasValue)
                        parentModule.LFRQ = gs.LFRQ.Value;
                }

                //
                SetTimbre();
                //Freq
                OnPitchUpdated();
                //Volume
                OnVolumeUpdated();

                byte op = (byte)(timbre.Ops[0].Enable << 3 | timbre.Ops[2].Enable << 4 | timbre.Ops[1].Enable << 5 | timbre.Ops[3].Enable << 6);
                parentModule.Ym3806WriteData(unitNumber, 0x05, 0, 0, (byte)(op | Slot));
            }


            public override void OnSoundParamsUpdated()
            {
                var gs = timbre.GlobalSettings;
                if (gs.Enable)
                {
                    if (gs.LFOE.HasValue)
                        parentModule.LFOE = gs.LFOE.Value;
                    if (gs.LFRQ.HasValue)
                        parentModule.LFRQ = gs.LFRQ.Value;
                }

                parentModule.Ym3806WriteData(unitNumber, 0x18, 0, Slot, (byte)((timbre.REV << 7 | timbre.PMS << 4 | timbre.AMS)));
                for (int op = 0; op < 4; op++)
                {
                    parentModule.Ym3806WriteData(unitNumber, 0x40, op, Slot, (byte)(timbre.Ops[op].DT1));
                    parentModule.Ym3806WriteData(unitNumber, 0x40, op, Slot, (byte)(0x80 + timbre.Ops[op].MUL));

                    var tl = timbre.Ops[op].TL + timbre.Ops[op].GetLSAttenuationValue(NoteOnEvent.NoteNumber);
                    int kvs = timbre.Ops[op].GetKvsAttenuationValue(NoteOnEvent.Velocity);
                    if (kvs > 0)
                        tl += kvs;
                    if (tl > 127)
                        tl = 127;

                    switch (timbre.ALG)
                    {
                        case 0:
                            if (op != 3)
                                parentModule.Ym3806WriteData(unitNumber, 0x60, op, Slot, (byte)tl);
                            break;
                        case 1:
                            if (op != 3)
                                parentModule.Ym3806WriteData(unitNumber, 0x60, op, Slot, (byte)tl);
                            break;
                        case 2:
                            if (op != 3)
                                parentModule.Ym3806WriteData(unitNumber, 0x60, op, Slot, (byte)tl);
                            break;
                        case 3:
                            if (op != 3)
                                parentModule.Ym3806WriteData(unitNumber, 0x60, op, Slot, (byte)tl);
                            break;
                        case 4:
                            if (op != 2 && op != 3)
                                parentModule.Ym3806WriteData(unitNumber, 0x60, op, Slot, (byte)tl);
                            break;
                        case 5:
                            if (op == 3)
                                parentModule.Ym3806WriteData(unitNumber, 0x60, op, Slot, (byte)tl);
                            break;
                        case 6:
                            if (op == 3)
                                parentModule.Ym3806WriteData(unitNumber, 0x60, op, Slot, (byte)tl);
                            break;
                        case 7:
                            break;
                    }
                    parentModule.Ym3806WriteData(unitNumber, 0x80, op, Slot, (byte)((timbre.Ops[op].RS << 6 | timbre.Ops[op].AR)));
                    parentModule.Ym3806WriteData(unitNumber, 0xa0, op, Slot, (byte)((timbre.Ops[op].AM << 7 | timbre.Ops[op].OSCW << 6 | timbre.Ops[op].D1R)));
                    parentModule.Ym3806WriteData(unitNumber, 0xc0, op, Slot, (byte)((timbre.Ops[op].D2R)));
                    parentModule.Ym3806WriteData(unitNumber, 0xe0, op, Slot, (byte)((timbre.Ops[op].SL << 4 | timbre.Ops[op].RR)));
                }

                if (!IsKeyOff)
                {
                    byte open = (byte)(timbre.Ops[0].Enable << 3 | timbre.Ops[2].Enable << 4 | timbre.Ops[1].Enable << 5 | timbre.Ops[3].Enable << 6);
                    parentModule.Ym3806WriteData(unitNumber, 0x05, 0, 0, (byte)(open | Slot));
                }

                base.OnSoundParamsUpdated();
            }


            /// <summary>
            /// 
            /// </summary>
            public override void OnVolumeUpdated()
            {
                List<int> ops = new List<int>();
                List<int> exops = new List<int>();
                switch (timbre.ALG)
                {
                    case 0:
                        ops.Add(3);
                        exops.Add(0);
                        exops.Add(1);
                        exops.Add(2);
                        break;
                    case 1:
                        ops.Add(3);
                        exops.Add(0);
                        exops.Add(1);
                        exops.Add(2);
                        break;
                    case 2:
                        ops.Add(3);
                        exops.Add(0);
                        exops.Add(1);
                        exops.Add(2);
                        break;
                    case 3:
                        ops.Add(3);
                        exops.Add(0);
                        exops.Add(1);
                        exops.Add(2);
                        break;
                    case 4:
                        ops.Add(2);
                        ops.Add(3);
                        exops.Add(0);
                        exops.Add(1);
                        break;
                    case 5:
                        ops.Add(1);
                        ops.Add(2);
                        ops.Add(3);
                        exops.Add(0);
                        break;
                    case 6:
                        ops.Add(1);
                        ops.Add(2);
                        ops.Add(3);
                        exops.Add(0);
                        break;
                    case 7:
                        ops.Add(2);
                        ops.Add(3);
                        exops.Add(0);
                        exops.Add(1);
                        break;
                }
                int velo = 1 + timbre.MDS.VelocitySensitivity;
                foreach (int op in ops)
                {
                    var tl = timbre.Ops[op].TL + timbre.Ops[op].GetLSAttenuationValue(NoteOnEvent.NoteNumber);
                    int kvs = timbre.Ops[op].GetKvsAttenuationValue(NoteOnEvent.Velocity);
                    if (kvs > 0)
                        tl += kvs;
                    if (tl > 127)
                        tl = 127;
                    double vol = 0;
                    if (kvs < 0)
                    {
                        vol = ((127 / velo) - Math.Round(((127 / velo) - (tl / velo)) * CalcCurrentVolume()));
                    }
                    else
                        vol = (127 - Math.Round((127 - (tl + kvs)) * CalcCurrentVolume(true)));
                    //$60+: total level
                    parentModule.Ym3806WriteData(unitNumber, 0x60, op, Slot, (byte)vol);
                }
                if (timbre.UseExprForModulator)
                {
                    //$60+: total level
                    foreach (int op in exops)
                    {
                        if (((1 << op) & (int)timbre.ExprTargetModulators) == 0)
                            continue;

                        //$60+: total level
                        var mul = CalcModulatorMultiply();
                        double vol = timbre.Ops[op].TL;
                        if (mul > 0)
                            vol = vol + ((127 - vol) * mul);
                        else if (mul < 0)
                            vol = vol + ((vol) * mul);
                        vol = Math.Round(vol);
                        parentModule.Ym3806WriteData(unitNumber, 0x60, op, Slot, (byte)vol);
                    }
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="slot"></param>
            public override void OnPitchUpdated()
            {
                int nn = NoteOnEvent.NoteNumber;
                if (ParentModule.ChannelTypes[NoteOnEvent.Channel] == ChannelType.Drum)
                    nn = (int)ParentModule.DrumTimbres[NoteOnEvent.NoteNumber].BaseNote;

                var eng = FxEngine as OpzFxEngine;

                //Operator 2 & 4
                {
                    double d = CalcCurrentPitchDeltaNoteNumber() + ((timbre.PitchShift24 + (eng?.PitchShift24 != null ? eng.PitchShift24.Value : 0)) / 100d);

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
                        if (freq > 0xfff)
                            freq = 0xfff;
                        octave = 7;
                    }
                    octave = octave << 4;

                    parentModule.Ym3806WriteData(unitNumber, 0x20, 0, Slot, (byte)(octave | ((freq >> 8) & 0xf)), false);
                    parentModule.Ym3806WriteData(unitNumber, 0x30, 0, Slot, (byte)(0xff & freq), false);
                }

                //Operator 1 & 3
                {
                    double d = CalcCurrentPitchDeltaNoteNumber() + ((timbre.PitchShift13 + (eng?.PitchShift13 != null ? eng.PitchShift13.Value : 0)) / 100d);

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
                        if (freq > 0xfff)
                            freq = 0xfff;
                        octave = 7;
                    }
                    octave = octave << 4;

                    parentModule.Ym3806WriteData(unitNumber, 0x28, 0, Slot, (byte)(octave | ((freq >> 8) & 0xf)), false);
                    parentModule.Ym3806WriteData(unitNumber, 0x38, 0, Slot, (byte)(0xff & freq), false);
                }


                base.OnPitchUpdated();
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
            public override void OnPanpotUpdated()
            {
                byte pan = CalcCurrentPanpot();
                if (pan < 32)
                    pan = 0x1;
                else if (pan > 96)
                    pan = 0x2;
                else
                    pan = 0x3;
                parentModule.Ym3806WriteData(unitNumber, 0x10, 0, Slot, (byte)(pan << 6 | (timbre.FB << 3) | timbre.ALG));
            }

            /// <summary>
            /// 
            /// </summary>
            public void SetTimbre()
            {
                parentModule.Ym3806WriteData(unitNumber, 0x18, 0, Slot, (byte)((timbre.REV << 7 | timbre.PMS << 4 | timbre.AMS)));
                for (int op = 0; op < 4; op++)
                {
                    parentModule.Ym3806WriteData(unitNumber, 0x40, op, Slot, (byte)(timbre.Ops[op].DT1));
                    parentModule.Ym3806WriteData(unitNumber, 0x40, op, Slot, (byte)(0x80 + timbre.Ops[op].MUL));

                    switch (timbre.ALG)
                    {
                        case 0:
                            if (op != 3 && !timbre.UseExprForModulator)
                                parentModule.Ym3806WriteData(unitNumber, 0x60, op, Slot, (byte)timbre.Ops[op].TL);
                            break;
                        case 1:
                            if (op != 3 && !timbre.UseExprForModulator)
                                parentModule.Ym3806WriteData(unitNumber, 0x60, op, Slot, (byte)timbre.Ops[op].TL);
                            break;
                        case 2:
                            if (op != 3 && !timbre.UseExprForModulator)
                                parentModule.Ym3806WriteData(unitNumber, 0x60, op, Slot, (byte)timbre.Ops[op].TL);
                            break;
                        case 3:
                            if (op != 3 && !timbre.UseExprForModulator)
                                parentModule.Ym3806WriteData(unitNumber, 0x60, op, Slot, (byte)timbre.Ops[op].TL);
                            break;
                        case 4:
                            if (op != 2 && op != 3 && !timbre.UseExprForModulator)
                                parentModule.Ym3806WriteData(unitNumber, 0x60, op, Slot, (byte)timbre.Ops[op].TL);
                            break;
                        case 5:
                            if (op == 0 && !timbre.UseExprForModulator)
                                parentModule.Ym3806WriteData(unitNumber, 0x60, op, Slot, (byte)timbre.Ops[op].TL);
                            break;
                        case 6:
                            if (op == 0 && !timbre.UseExprForModulator)
                                parentModule.Ym3806WriteData(unitNumber, 0x60, op, Slot, (byte)timbre.Ops[op].TL);
                            break;
                        case 7:
                            if (op != 2 && op != 3 && !timbre.UseExprForModulator)
                                parentModule.Ym3806WriteData(unitNumber, 0x60, op, Slot, (byte)timbre.Ops[op].TL);
                            break;
                    }
                    parentModule.Ym3806WriteData(unitNumber, 0x80, op, Slot, (byte)((timbre.Ops[op].RS << 6 | timbre.Ops[op].AR)));
                    parentModule.Ym3806WriteData(unitNumber, 0xa0, op, Slot, (byte)((timbre.Ops[op].AM << 7 | timbre.Ops[op].OSCW << 6 | timbre.Ops[op].D1R)));
                    parentModule.Ym3806WriteData(unitNumber, 0xc0, op, Slot, (byte)((timbre.Ops[op].D2R)));
                    parentModule.Ym3806WriteData(unitNumber, 0xe0, op, Slot, (byte)((timbre.Ops[op].SL << 4 | timbre.Ops[op].RR)));
                }

                OnPanpotUpdated();
            }

            /// <summary>
            /// 
            /// </summary>
            public override void SoundOff()
            {
                base.SoundOff();

                parentModule.Ym3806WriteData(unitNumber, 0x05, 0, 0, (byte)(0x00 | Slot));
            }

            private ushort[] freqTable = new ushort[] {
                0x90a/2,
                0x4ca,
                0x513,
                0x560,
                0x5b2,
                0x609,
                0x665,
                0x6c6,
                0x72d,
                0x79a,
                0x80e,
                0x889,
                0x90a,
                0x4ca*2,
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
                {
                    return freqTable[(int)note.GetNoteName() + 2];
                }
                else
                {
                    return freqTable[(int)note.GetNoteName()];
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        [JsonConverter(typeof(NoTypeConverterJsonConverter<YM3806Timbre>))]
        [DataContract]
        [InstLock]
        public class YM3806Timbre : TimbreBase
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

                //using (var f = new FormYM3806Editor((YM3806)inst, this, true))
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
            [Editor(typeof(YM3806UITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
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
                        nameof(REV),
                        nameof(AMS),
                        nameof(PMS),
                        nameof(PitchShift13),
                        nameof(PitchShift24),

                        "GlobalSettings.EN",
                        "GlobalSettings.LFOE",
                        "GlobalSettings.LFRQ",

                        "Ops[0].EN",
                        "Ops[0].AR",
                        "Ops[0].D1R",
                        "Ops[0].D2R",
                        "Ops[0].RR",
                        "Ops[0].SL",
                        "Ops[0].TL",
                        "Ops[0].RS",
                        "Ops[0].MUL",
                        "Ops[0].DT1",
                        "Ops[0].AM",
                        "Ops[0].OSCW",
                        "Ops[0].LS",
                        "Ops[0].KVS",

                        "Ops[1].EN",
                        "Ops[1].AR",
                        "Ops[1].D1R",
                        "Ops[1].D2R",
                        "Ops[1].RR",
                        "Ops[1].SL",
                        "Ops[1].TL",
                        "Ops[1].RS",
                        "Ops[1].MUL",
                        "Ops[1].DT1",
                        "Ops[1].AM",
                        "Ops[1].OSCW",
                        "Ops[1].LS",
                        "Ops[1].KVS",

                        "Ops[2].EN",
                        "Ops[2].AR",
                        "Ops[2].D1R",
                        "Ops[2].D2R",
                        "Ops[2].RR",
                        "Ops[2].SL",
                        "Ops[2].TL",
                        "Ops[2].RS",
                        "Ops[2].MUL",
                        "Ops[2].DT1",
                        "Ops[2].AM",
                        "Ops[2].OSCW",
                        "Ops[2].LS",
                        "Ops[2].KVS",

                        "Ops[3].EN",
                        "Ops[3].AR",
                        "Ops[3].D1R",
                        "Ops[3].D2R",
                        "Ops[3].RR",
                        "Ops[3].SL",
                        "Ops[3].TL",
                        "Ops[3].RS",
                        "Ops[3].MUL",
                        "Ops[3].DT1",
                        "Ops[3].AM",
                        "Ops[3].OSCW",
                        "Ops[3].LS",
                        "Ops[3].KVS"
                        );
                }
                set
                {
                    SimpleSerializer.DeserializeProps(this, value,
                        nameof(ALG),
                        nameof(FB),
                        nameof(REV),
                        nameof(AMS),
                        nameof(PMS),
                        nameof(PitchShift13),
                        nameof(PitchShift24),

                        "GlobalSettings.EN",
                        "GlobalSettings.LFOE",
                        "GlobalSettings.LFRQ",

                        "Ops[0].EN",
                        "Ops[0].AR",
                        "Ops[0].D1R",
                        "Ops[0].D2R",
                        "Ops[0].RR",
                        "Ops[0].SL",
                        "Ops[0].TL",
                        "Ops[0].RS",
                        "Ops[0].MUL",
                        "Ops[0].DT1",
                        "Ops[0].AM",
                        "Ops[0].OSCW",
                        "Ops[0].LS",
                        "Ops[0].KVS",

                        "Ops[1].EN",
                        "Ops[1].AR",
                        "Ops[1].D1R",
                        "Ops[1].D2R",
                        "Ops[1].RR",
                        "Ops[1].SL",
                        "Ops[1].TL",
                        "Ops[1].RS",
                        "Ops[1].MUL",
                        "Ops[1].DT1",
                        "Ops[1].AM",
                        "Ops[1].OSCW",
                        "Ops[1].LS",
                        "Ops[1].KVS",

                        "Ops[2].EN",
                        "Ops[2].AR",
                        "Ops[2].D1R",
                        "Ops[2].D2R",
                        "Ops[2].RR",
                        "Ops[2].SL",
                        "Ops[2].TL",
                        "Ops[2].RS",
                        "Ops[2].MUL",
                        "Ops[2].DT1",
                        "Ops[2].AM",
                        "Ops[2].OSCW",
                        "Ops[2].LS",
                        "Ops[2].KVS",

                        "Ops[3].EN",
                        "Ops[3].AR",
                        "Ops[3].D1R",
                        "Ops[3].D2R",
                        "Ops[3].RR",
                        "Ops[3].SL",
                        "Ops[3].TL",
                        "Ops[3].RS",
                        "Ops[3].MUL",
                        "Ops[3].DT1",
                        "Ops[3].AM",
                        "Ops[3].OSCW",
                        "Ops[3].LS",
                        "Ops[3].KVS"
                        );
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
                "4: (1->3)+(2->4) (for Flute, bells, chorus, bass drum, snare drum, tom-tom sound)\r\n" +
                "5: (1->2)+(1->3)+(1->4) (for Brass, organ sound)\r\n" +
                "6: (1->2)+3+4 (for Xylophone, tom-tom, organ, vibraphone, snare drum, base drum sound)\r\n" +
                "7: (1->2->4)+3")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 7)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
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


            private byte f_REV;

            [DataMember]
            [Category("Sound")]
            [Description("Reverb (0:Off 1:On)")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte REV
            {
                get
                {
                    return f_REV;
                }
                set
                {
                    f_REV = (byte)(value & 1);
                }
            }

            private byte f_AMS;

            [DataMember]
            [Category("Sound")]
            [Description("Amplitude Modulation Shift (0-3)")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 3)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
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
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 7)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
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

            private int f_PitchShift13;

            [DataMember]
            [Description("Operator 1,3 frequency offset [Cent]")]
            [DefaultValue(0)]
            [SlideParametersAttribute(-1200, 1200)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [Category("Sound")]
            public int PitchShift13
            {
                get => f_PitchShift13;
                set
                {
                    f_PitchShift13 = value;
                    if (f_PitchShift13 < -1200)
                        f_PitchShift13 = -1200;
                    else if (f_PitchShift13 > 1200)
                        f_PitchShift13 = 1200;
                }
            }

            private int f_PitchShift24;

            [DataMember]
            [Description("Operator 2,4 frequency offset [Cent]")]
            [DefaultValue(0)]
            [SlideParametersAttribute(-1200, 1200)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [Category("Sound")]
            public int PitchShift24
            {
                get => f_PitchShift24;
                set
                {
                    f_PitchShift24 = value;
                    if (f_PitchShift24 < -1200)
                        f_PitchShift24 = -1200;
                    else if (f_PitchShift24 > 1200)
                        f_PitchShift24 = 1200;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Operators")]
            [TypeConverter(typeof(ExpandableCollectionConverter))]
            [EditorAttribute(typeof(DummyEditor), typeof(UITypeEditor))]
            [DisplayName("Operators[Ops]")]
            public YM3806Operator[] Ops
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
                var ops = new YM3806Operator[] {
                    new YM3806Operator(),
                    new YM3806Operator(),
                    new YM3806Operator(),
                    new YM3806Operator() };
                for (int i = 0; i < Ops.Length; i++)
                    Ops[i].InjectFrom(new LoopInjection(), ops[i]);
            }

            [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                typeof(UITypeEditor)), Localizable(false)]
            [IgnoreDataMember]
            [JsonIgnore]
            [Description("You can copy and paste this text data to other same type timber.\r\n" +
                "ALG, FB, AR, D1R(DR), D2R(SR), RR, SL, TL, RS(KS), MUL, DT1, AM(AMS), ... , AMS, PMS\r\n" +
                "You can use comma or space chars as delimiter.")]
            public string MmlSerializeData
            {
                get
                {
                    return SimpleSerializer.SerializeProps(this,
                        nameof(ALG),
                        nameof(FB),

                        "Ops[0].AR",
                        "Ops[0].D1R",
                        "Ops[0].D2R",
                        "Ops[0].RR",
                        "Ops[0].SL",
                        "Ops[0].TL",
                        "Ops[0].RS",
                        "Ops[0].MUL",
                        "Ops[0].DT1",
                        "Ops[0].AM",
                        "Ops[0].OSCW",
                        "Ops[0].LS",
                        "Ops[0].KVS",

                        "Ops[1].AR",
                        "Ops[1].D1R",
                        "Ops[1].D2R",
                        "Ops[1].RR",
                        "Ops[1].SL",
                        "Ops[1].TL",
                        "Ops[1].RS",
                        "Ops[1].MUL",
                        "Ops[1].DT1",
                        "Ops[1].AM",
                        "Ops[1].OSCW",
                        "Ops[1].LS",
                        "Ops[1].KVS",

                        "Ops[2].AR",
                        "Ops[2].D1R",
                        "Ops[2].D2R",
                        "Ops[2].RR",
                        "Ops[2].SL",
                        "Ops[2].TL",
                        "Ops[2].RS",
                        "Ops[2].MUL",
                        "Ops[2].DT1",
                        "Ops[2].AM",
                        "Ops[2].OSCW",
                        "Ops[2].LS",
                        "Ops[2].KVS",

                        "Ops[3].AR",
                        "Ops[3].D1R",
                        "Ops[3].D2R",
                        "Ops[3].RR",
                        "Ops[3].SL",
                        "Ops[3].TL",
                        "Ops[3].RS",
                        "Ops[3].MUL",
                        "Ops[3].DT1",
                        "Ops[3].AM",
                        "Ops[3].OSCW",
                        "Ops[3].LS",
                        "Ops[3].KVS",

                        nameof(AMS),
                        nameof(PMS));
                }
                set
                {
                    SimpleSerializer.DeserializeProps(this, value,
                        nameof(ALG),
                        nameof(FB),

                        "Ops[0].AR",
                        "Ops[0].D1R",
                        "Ops[0].D2R",
                        "Ops[0].RR",
                        "Ops[0].SL",
                        "Ops[0].TL",
                        "Ops[0].RS",
                        "Ops[0].MUL",
                        "Ops[0].DT1",
                        "Ops[0].AM",
                        "Ops[0].OSCW",
                        "Ops[0].LS",
                        "Ops[0].KVS",

                        "Ops[1].AR",
                        "Ops[1].D1R",
                        "Ops[1].D2R",
                        "Ops[1].RR",
                        "Ops[1].SL",
                        "Ops[1].TL",
                        "Ops[1].RS",
                        "Ops[1].MUL",
                        "Ops[1].DT1",
                        "Ops[1].AM",
                        "Ops[1].OSCW",
                        "Ops[1].LS",
                        "Ops[1].KVS",

                        "Ops[2].AR",
                        "Ops[2].D1R",
                        "Ops[2].D2R",
                        "Ops[2].RR",
                        "Ops[2].SL",
                        "Ops[2].TL",
                        "Ops[2].RS",
                        "Ops[2].MUL",
                        "Ops[2].DT1",
                        "Ops[2].AM",
                        "Ops[2].OSCW",
                        "Ops[2].LS",
                        "Ops[2].KVS",

                        "Ops[3].AR",
                        "Ops[3].D1R",
                        "Ops[3].D2R",
                        "Ops[3].RR",
                        "Ops[3].SL",
                        "Ops[3].TL",
                        "Ops[3].RS",
                        "Ops[3].MUL",
                        "Ops[3].DT1",
                        "Ops[3].AM",
                        "Ops[3].OSCW",
                        "Ops[3].LS",
                        "Ops[3].KVS",

                        nameof(AMS),
                        nameof(PMS));
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

            /// <summary>
            /// 
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Traget modulator for UseExprForModulator.")]
            [Browsable(true)]
            [Editor(typeof(FlagEnumUIEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public ExprModulators ExprTargetModulators
            {
                get;
                set;
            } = ExprModulators.All;

            public virtual bool ShouldSerializeExprTargetModulators()
            {
                return ExprTargetModulators != ExprModulators.All;
            }

            public virtual void ResetExprTargetModulators()
            {
                ExprTargetModulators = ExprModulators.All;
            }

            [FlagsAttribute]
            public enum ExprModulators
            {
                [Description("Operator 1[Ops[0]")]
                Ops0 = 1,
                [Description("Operator 2[Ops[1]")]
                Ops1 = 2,
                [Description("Operator 3[Ops[2]")]
                Ops2 = 4,
                [Description("All")]
                All = 7,
            }

            [DataMember]
            [Category("Chip(Global)")]
            [Description("Global Settings")]
            public YM3806GlobalSettings GlobalSettings
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
                GlobalSettings.InjectFrom(new LoopInjection(), new YM3806GlobalSettings());
            }

            /// <summary>
            /// 
            /// </summary>
            public YM3806Timbre()
            {
                Ops = new YM3806Operator[] {
                    new YM3806Operator(),
                    new YM3806Operator(),
                    new YM3806Operator(),
                    new YM3806Operator() };
                GlobalSettings = new YM3806GlobalSettings();
            }

            #endregion

            protected override void InitializeFxS()
            {
                SDS.FxS = new OpzFxSettings();
            }

            public override void RestoreFrom(string serializeData)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<YM3806Timbre>(serializeData);
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
        [JsonConverter(typeof(NoTypeConverterJsonConverter<YM3806Operator>))]
        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [DataContract]
        [InstLock]
        public class YM3806Operator : YMOperatorBase, ISerializeDataSaveLoad
        {

            private byte f_Enable = 1;

            /// <summary>
            /// Enable(0-1)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Whether this operator enable or not")]
            [DefaultValue((byte)1)]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
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

            [IgnoreDataMember]
            [JsonIgnore]
            [Browsable(false)]
            public byte EN
            {
                get
                {
                    return Enable;
                }
                set
                {
                    Enable = value;
                }
            }

            private byte f_AR;

            /// <summary>
            /// Attack Rate(0-31)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Attack Rate (0-31)")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 31)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
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

            private byte f_D1R;

            /// <summary>
            /// Decay rate(0-31)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Decay Rate (0-31)")]
            [DisplayName("D1R(DR)[D1R]")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 31)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
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
            /// Sustain Rate(2nd Decay Rate)(0-31)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Sustain Rate(2nd Decay Rate) (0-31)")]
            [DisplayName("D2R(SR)[D2R]")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 31)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
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
            /// sustain level(0-15)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Sustain Level(0-15)")]
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

            private byte f_TL;

            /// <summary>
            /// Total Level(0-127)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Total Level (0-127)")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 127)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
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
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 3)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
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

            private byte f_MUL;

            /// <summary>
            /// Multiply(0-15)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Multiply (0-15)")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 15)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
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

            private byte f_DT1 = 0;

            /// <summary>
            /// Detune(0-63)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("DeTune (0-63)")]
            [DefaultValue((byte)4)]
            [SlideParametersAttribute(0, 63)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte DT1
            {
                get
                {
                    return f_DT1;
                }
                set
                {
                    f_DT1 = (byte)(value & 63);
                }
            }

            private byte f_AM;

            /// <summary>
            /// AMS Enable (0:Disable 1:Enable)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Amplitude Modulation Sensivity (0-1)")]
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

            private byte f_OSCW;

            /// <summary>
            /// Oscillator Wave Form (0-1)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Oscillator Wave Form (0-1)" +
                "0: Sine\r\n" +
                "1: Half Sine")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte OSCW
            {
                get
                {
                    return f_OSCW;
                }
                set
                {
                    f_OSCW = (byte)(value & 1);
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
                "AR, D1R(DR), D2R(SR), RR, SL, TL, RS(KS), MUL, DT1, AM(AMS)\r\n" +
                "You can use comma or space chars as delimiter.")]
            public string MmlSerializeData
            {
                get
                {
                    return SimpleSerializer.SerializeProps(this,
                        nameof(AR),
                        nameof(D1R),
                        nameof(D2R),
                        nameof(RR),
                        nameof(SL),
                        nameof(TL),
                        nameof(RS),
                        nameof(MUL),
                        nameof(DT1),
                        nameof(AM));
                }
                set
                {
                    SimpleSerializer.DeserializeProps(this, value,
                        nameof(AR),
                        nameof(D1R),
                        nameof(D2R),
                        nameof(RR),
                        nameof(SL),
                        nameof(TL),
                        nameof(RS),
                        nameof(MUL),
                        nameof(DT1),
                        nameof(AM)
                        );
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
                    var obj = JsonConvert.DeserializeObject<YM3806Operator>(serializeData);
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
        [JsonConverter(typeof(NoTypeConverterJsonConverter<YM3806GlobalSettings>))]
        [DataContract]
        [InstLock]
        public class YM3806GlobalSettings : ContextBoundObject
        {
            [DataMember]
            [Category("Chip(Global)")]
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

            private byte? f_LFOE;

            /// <summary>
            /// LFO Enable (0:Disable 1:Enable)
            /// </summary>
            [Browsable(false)]
            [DataMember]
            [Category("Chip(Global)")]
            [Description("LFO Enable (0:Disable 1:Enable)")]
            [DefaultValue(null)]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte? LFOE
            {
                get
                {
                    return f_LFOE;
                }
                set
                {
                    byte? v = value;
                    if (value.HasValue)
                        v = (byte)(value & 1);
                    f_LFOE = v;
                }
            }

            private byte? f_LFRQ;

            /// <summary>
            /// LFRQ (0-255)
            /// </summary>
            [DataMember]
            [Category("Chip(Global)")]
            [Description("LFO Freq (0-7)")]
            [DefaultValue(null)]
            [SlideParametersAttribute(0, 7)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte? LFRQ
            {
                get
                {
                    return f_LFRQ;
                }
                set
                {
                    byte? v = value;
                    if (value.HasValue)
                        v = (byte)(value & 7);
                    f_LFRQ = v;
                }
            }
        }

        [JsonConverter(typeof(NoTypeConverterJsonConverter<OpzFxSettings>))]
        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [DataContract]
        [InstLock]
        public class OpzFxSettings : BasicFxSettings
        {

            private string f_PitchShift13;

            [DataMember]
            [Description("Set Pitchshift for Op 1,3 envelop by text. Input pitchshift value and split it with space like the FamiTracker.\r\n" +
                       "-1200 - 1200 \"|\" is repeat point. \"/\" is release point.")]
            [Editor(typeof(EnvelopeUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [EnvelopeEditorAttribute(-1200, 1200)]
            public string PitchShift13
            {
                get
                {
                    return f_PitchShift13;
                }
                set
                {
                    if (f_PitchShift13 != value)
                    {
                        PitchShift13RepeatPoint = -1;
                        PitchShift13ReleasePoint = -1;
                        if (value == null)
                        {
                            PitchShift13Nums = new int[] { };
                            f_PitchShift13 = string.Empty;
                            return;
                        }
                        f_PitchShift13 = value;
                        string[] vals = value.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        List<int> vs = new List<int>();
                        for (int i = 0; i < vals.Length; i++)
                        {
                            string val = vals[i];
                            if (val.Equals("|", StringComparison.Ordinal))
                                PitchShift13RepeatPoint = vs.Count;
                            else if (val.Equals("/", StringComparison.Ordinal))
                                PitchShift13ReleasePoint = vs.Count;
                            else
                            {
                                int v;
                                if (int.TryParse(val, out v))
                                {
                                    if (v < -1200)
                                        v = -1200;
                                    else if (v > 1200)
                                        v = 1200;
                                    vs.Add(v);
                                }
                            }
                        }
                        PitchShift13Nums = vs.ToArray();

                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < PitchShift13Nums.Length; i++)
                        {
                            if (sb.Length != 0)
                                sb.Append(' ');
                            if (PitchShift13RepeatPoint == i)
                                sb.Append("| ");
                            if (PitchShift13ReleasePoint == i)
                                sb.Append("/ ");
                            if (i < PitchShift13Nums.Length)
                                sb.Append(PitchShift13Nums[i].ToString((IFormatProvider)null));
                        }
                        f_PitchShift13 = sb.ToString();
                    }
                }
            }

            public bool ShouldSerializePitchShift13()
            {
                return !string.IsNullOrEmpty(PitchShift13);
            }

            public void ResetPitchShift13()
            {
                PitchShift13 = null;
            }

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            public int[] PitchShift13Nums { get; set; } = new int[] { };

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int PitchShift13RepeatPoint { get; set; } = -1;

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int PitchShift13ReleasePoint { get; set; } = -1;


            private string f_PitchShift24;

            [DataMember]
            [Description("Set Pitchshift for Op 2,4 envelop by text. Input pitchshift value and split it with space like the FamiTracker.\r\n" +
                       "-1200 - 1200 \"|\" is repeat point. \"/\" is release point.")]
            [Editor(typeof(EnvelopeUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [EnvelopeEditorAttribute(-1200, 1200)]
            public string PitchShift24
            {
                get
                {
                    return f_PitchShift24;
                }
                set
                {
                    if (f_PitchShift24 != value)
                    {
                        PitchShift24RepeatPoint = -1;
                        PitchShift24ReleasePoint = -1;
                        if (value == null)
                        {
                            PitchShift24Nums = new int[] { };
                            f_PitchShift24 = string.Empty;
                            return;
                        }
                        f_PitchShift24 = value;
                        string[] vals = value.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        List<int> vs = new List<int>();
                        for (int i = 0; i < vals.Length; i++)
                        {
                            string val = vals[i];
                            if (val.Equals("|", StringComparison.Ordinal))
                                PitchShift24RepeatPoint = vs.Count;
                            else if (val.Equals("/", StringComparison.Ordinal))
                                PitchShift24ReleasePoint = vs.Count;
                            else
                            {
                                int v;
                                if (int.TryParse(val, out v))
                                {
                                    if (v < -1200)
                                        v = -1200;
                                    else if (v > 1200)
                                        v = 1200;
                                    vs.Add(v);
                                }
                            }
                        }
                        PitchShift24Nums = vs.ToArray();

                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < PitchShift24Nums.Length; i++)
                        {
                            if (sb.Length != 0)
                                sb.Append(' ');
                            if (PitchShift24RepeatPoint == i)
                                sb.Append("| ");
                            if (PitchShift24ReleasePoint == i)
                                sb.Append("/ ");
                            if (i < PitchShift24Nums.Length)
                                sb.Append(PitchShift24Nums[i].ToString((IFormatProvider)null));
                        }
                        f_PitchShift24 = sb.ToString();
                    }
                }
            }

            public bool ShouldSerializePitchShift24()
            {
                return !string.IsNullOrEmpty(PitchShift24);
            }

            public void ResetPitchShift24()
            {
                PitchShift24 = null;
            }

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            public int[] PitchShift24Nums { get; set; } = new int[] { };

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int PitchShift24RepeatPoint { get; set; } = -1;

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int PitchShift24ReleasePoint { get; set; } = -1;


            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override AbstractFxEngine CreateEngine()
            {
                return new OpzFxEngine(this);
            }

        }


        /// <summary>
        /// 
        /// </summary>
        public class OpzFxEngine : BasicFxEngine
        {
            private OpzFxSettings settings;

            /// <summary>
            /// 
            /// </summary>
            public OpzFxEngine(OpzFxSettings settings) : base(settings)
            {
                this.settings = settings;
            }

            private uint f_PitchShift13Counter;

            public int? PitchShift13
            {
                get;
                private set;
            }

            private uint f_PitchShift24Counter;

            public int? PitchShift24
            {
                get;
                private set;
            }

            protected override bool ProcessCore(SoundBase sound, bool isKeyOff, bool isSoundOff)
            {
                bool process = base.ProcessCore(sound, isKeyOff, isSoundOff);

                PitchShift13 = null;
                if (settings.PitchShift13Nums.Length > 0)
                {
                    if (!isKeyOff)
                    {
                        var vm = settings.PitchShift13Nums.Length;
                        if (settings.PitchShift13ReleasePoint >= 0)
                            vm = settings.PitchShift13ReleasePoint;
                        if (f_PitchShift13Counter >= vm)
                        {
                            if (settings.PitchShift13RepeatPoint >= 0)
                                f_PitchShift13Counter = (uint)settings.PitchShift13RepeatPoint;
                            else
                                f_PitchShift13Counter = (uint)vm;
                        }
                    }
                    else
                    {
                        //if (settings.PitchShift13ReleasePoint < 0)
                        //    f_PitchShift13Counter = (uint)settings.PitchShift13Nums.Length;

                        if (f_PitchShift13Counter < settings.PitchShift13Nums.Length)
                        {
                            if (settings.PitchShift13ReleasePoint >= 0 && f_PitchShift13Counter <= (uint)settings.PitchShift13ReleasePoint)
                                f_PitchShift13Counter = (uint)settings.PitchShift13ReleasePoint;
                            else
                                f_PitchShift13Counter = (uint)settings.PitchShift13Nums.Length;
                        }
                    }
                    if (f_PitchShift13Counter < settings.PitchShift13Nums.Length)
                    {
                        int vol = settings.PitchShift13Nums[f_PitchShift13Counter++];

                        PitchShift13 = (byte)vol;
                        process = true;
                    }
                }

                PitchShift24 = null;
                if (settings.PitchShift24Nums.Length > 0)
                {
                    if (!isKeyOff)
                    {
                        var vm = settings.PitchShift24Nums.Length;
                        if (settings.PitchShift24ReleasePoint >= 0)
                            vm = settings.PitchShift24ReleasePoint;
                        if (f_PitchShift24Counter >= vm)
                        {
                            if (settings.PitchShift24RepeatPoint >= 0)
                                f_PitchShift24Counter = (uint)settings.PitchShift24RepeatPoint;
                            else
                                f_PitchShift24Counter = (uint)vm;
                        }
                    }
                    else
                    {
                        //if (settings.PitchShift24ReleasePoint < 0)
                        //    f_PitchShift24Counter = (uint)settings.PitchShift24Nums.Length;

                        if (f_PitchShift24Counter < settings.PitchShift24Nums.Length)
                        {
                            if (settings.PitchShift24ReleasePoint >= 0 && f_PitchShift24Counter <= (uint)settings.PitchShift24ReleasePoint)
                                f_PitchShift24Counter = (uint)settings.PitchShift24ReleasePoint;
                            else
                                f_PitchShift24Counter = (uint)settings.PitchShift24Nums.Length;
                        }
                    }
                    if (f_PitchShift24Counter < settings.PitchShift24Nums.Length)
                    {
                        int vol = settings.PitchShift24Nums[f_PitchShift24Counter++];

                        PitchShift24 = (byte)vol;
                        process = true;
                    }
                }

                return process;
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
            YM3806Timbre tim = (YM3806Timbre)timbre;

            tim.ALG = (byte)tone.AL;
            tim.FB = (byte)tone.FB;
            tim.REV = (byte)tone.REV;
            tim.AMS = (byte)tone.AMS;
            tim.PMS = (byte)tone.PMS;
            tim.PitchShift13 = (byte)tone.PitchShift;
            tim.PitchShift24 = (byte)tone.PitchShift2;
            tim.GlobalSettings.Enable = false;
            tim.GlobalSettings.LFOE = (byte?)tone.LFOE;
            tim.GlobalSettings.LFRQ = (byte?)tone.LFRQ;
            if (tim.GlobalSettings.LFRQ > 0 ||
                tim.GlobalSettings.LFOE > 0
                )
                tim.GlobalSettings.Enable = true;

            for (int i = 0; i < 4; i++)
            {
                tim.Ops[i].Enable = 1;
                tim.Ops[i].AR = (byte)tone.aOp[i].AR;
                tim.Ops[i].D1R = (byte)tone.aOp[i].DR;
                tim.Ops[i].D2R = tone.aOp[i].SR < 0 ? (byte)0 : (byte)tone.aOp[i].SR;
                tim.Ops[i].RR = (byte)tone.aOp[i].RR;
                tim.Ops[i].SL = (byte)tone.aOp[i].SL;
                tim.Ops[i].TL = (byte)tone.aOp[i].TL;
                tim.Ops[i].RS = (byte)tone.aOp[i].KS;
                tim.Ops[i].MUL = (byte)tone.aOp[i].ML;
                tim.Ops[i].DT1 = (byte)tone.aOp[i].DT;
                tim.Ops[i].AM = (byte)tone.aOp[i].AM;
                tim.Ops[i].OSCW = (byte)tone.aOp[i].OSCW;
                tim.Ops[i].LS = (byte)tone.aOp[i].LS;
                tim.Ops[i].KVS = (byte)tone.aOp[i].KVS;
            }
            timbre.TimbreName = tone.Name;
        }
    }


}