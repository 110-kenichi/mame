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
using System.Windows.Forms.Design;
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

//http://hp.vector.co.jp/authors/VA042397/nes/apu.html
//https://wiki.nesdev.com/w/index.php/APU
//http://offgao.blog112.fc2.com/blog-entry-40.html

namespace zanac.MAmidiMEmo.Instruments.Chips
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public partial class RP2A03 : InstrumentBase
    {

        public override string Name => "RP2A03";

        public override string Group => "PSG";

        public override InstrumentType InstrumentType => InstrumentType.RP2A03;

        [Browsable(false)]
        public override string ImageKey => "RP2A03";

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
                return 6;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        protected override string SoundInterfaceTagNamePrefix
        {
            get
            {
                return "nes_apu_";
            }
        }

        [DataMember]
        [Category("Chip")]
        [Description("Assign PCM data to DPCM soundtype instrument.\r\n" +
            "Delta PCM Data (Max 4081 bytes)")]
        [Editor(typeof(PcmTableUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [PcmTableEditor("DMC File(*.dmc)|*.dmc")]
        [TypeConverter(typeof(CustomObjectTypeConverter))]
        public DPcmSoundTable DeltaPcmSoundTable
        {
            get;
            set;
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

        public RP2A03Timbre[] f_Timbres;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("Chip")]
        [Description("Timbres (0-127)")]
        [EditorAttribute(typeof(DummyEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(ExpandableCollectionConverter))]
        public RP2A03Timbre[] Timbres
        {
            get
            {
                return f_Timbres;
            }
            set
            {
                f_Timbres = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void ResetTimbres()
        {
            for (int i = 0; i < InstrumentBase.DEFAULT_MAX_TIMBRES; i++)
                Timbres[i] = new RP2A03Timbre();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serializeData"></param>
        public override void RestoreFrom(string serializeData)
        {
            try
            {
                using (var obj = JsonConvert.DeserializeObject<RP2A03>(serializeData))
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
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delegate_RP2A03_write(uint unitNumber, uint address, byte data);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate byte delegate_RP2A03_read(uint unitNumber, uint address);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="length"></param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delegate_RP2A03_SetDPCM(uint unitNumber, byte[] data, uint length);

        /// <summary>
        /// 
        /// </summary>
        private static delegate_RP2A03_write RP2A03_write
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        private static delegate_RP2A03_read RP2A03_read
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        private static delegate_RP2A03_SetDPCM RP2A03_SetDPCM
        {
            get;
            set;
        }


        /// <summary>
        /// 
        /// </summary>
        private static void RP2A03WriteData(uint unitNumber, uint address, byte data)
        {
            DeferredWriteData(RP2A03_write, unitNumber, address, data);
            /*
            try
            {
                Program.SoundUpdating();
                RP2A03_write(unitNumber, address, data);
            }
            finally
            {
                Program.SoundUpdated();
            }*/
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unitNumber"></param>
        /// <param name="address"></param>
        /// <returns></returns>
        private static byte RP2A03ReadData(uint unitNumber, uint address)
        {
            try
            {
                Program.SoundUpdating();

                FlushDeferredWriteData();

                return RP2A03_read(unitNumber, address);
            }
            finally
            {
                Program.SoundUpdated();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="unitNumber"></param>
        /// <param name="data"></param>
        private static void RP2A03SetDpcm(uint unitNumber, byte[] data)
        {
            DeferredWriteData(RP2A03_SetDPCM, unitNumber, data, (uint)data.Length);
            /*
            try
            {
                Program.SoundUpdating();
                RP2A03_SetDPCM(unitNumber, data, (uint)data.Length);
            }
            finally
            {
                Program.SoundUpdated();
            }
            */
        }

        /// <summary>
        /// 
        /// </summary>
        static RP2A03()
        {
            IntPtr funcPtr = MameIF.GetProcAddress("nes_apu_regwrite");
            if (funcPtr != IntPtr.Zero)
            {
                RP2A03_write = Marshal.GetDelegateForFunctionPointer<delegate_RP2A03_write>(funcPtr);
            }
            funcPtr = MameIF.GetProcAddress("nes_apu_regread");
            if (funcPtr != IntPtr.Zero)
            {
                RP2A03_read = Marshal.GetDelegateForFunctionPointer<delegate_RP2A03_read>(funcPtr);
            }
            funcPtr = MameIF.GetProcAddress("nes_apu_set_dpcm");
            if (funcPtr != IntPtr.Zero)
            {
                RP2A03_SetDPCM = Marshal.GetDelegateForFunctionPointer<delegate_RP2A03_SetDPCM>(funcPtr);
            }
        }


        private RP2A03SoundManager soundManager;

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

        /// <summary>
        /// 
        /// </summary>
        public RP2A03(uint unitNumber) : base(unitNumber)
        {
            GainLeft = DEFAULT_GAIN;
            GainRight = DEFAULT_GAIN;

            Timbres = new RP2A03Timbre[InstrumentBase.DEFAULT_MAX_TIMBRES];
            for (int i = 0; i < InstrumentBase.DEFAULT_MAX_TIMBRES; i++)
                Timbres[i] = new RP2A03Timbre();
            DeltaPcmSoundTable = new DPcmSoundTable();

            setPresetInstruments();

            this.soundManager = new RP2A03SoundManager(this);
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
        private class RP2A03SoundManager : SoundManagerBase
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

            private static SoundList<RP2A03Sound> sqOnSounds = new SoundList<RP2A03Sound>(2);

            private static SoundList<RP2A03Sound> triOnSounds = new SoundList<RP2A03Sound>(1);

            private static SoundList<RP2A03Sound> noiseOnSounds = new SoundList<RP2A03Sound>(1);

            private static SoundList<RP2A03Sound> dpcmOnSounds = new SoundList<RP2A03Sound>(1);

            private static SoundList<RP2A03Sound> fdsOnSounds = new SoundList<RP2A03Sound>(1);

            private static SoundList<RP2A03Sound> vrc6SqOnSounds = new SoundList<RP2A03Sound>(2);

            private static SoundList<RP2A03Sound> vrc6SawOnSounds = new SoundList<RP2A03Sound>(1);

            private RP2A03 parentModule;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            public RP2A03SoundManager(RP2A03 parent) : base(parent)
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

                foreach (RP2A03Timbre timbre in parentModule.GetBaseTimbres(note))
                {
                    var emptySlot = searchEmptySlot(note, timbre);
                    if (emptySlot.slot < 0)
                        continue;

                    RP2A03Sound snd = new RP2A03Sound(emptySlot.inst, this, timbre, note, emptySlot.slot);
                    switch (timbre.ToneType)
                    {
                        case ToneType.SQUARE:
                            sqOnSounds.Add(snd);
                            FormMain.OutputDebugLog("KeyOn SQ ch" + emptySlot + " " + note.ToString());
                            break;
                        case ToneType.TRIANGLE:
                            triOnSounds.Add(snd);
                            FormMain.OutputDebugLog("KeyOn Tri ch" + emptySlot + " " + note.ToString());
                            break;
                        case ToneType.NOISE:
                            noiseOnSounds.Add(snd);
                            FormMain.OutputDebugLog("KeyOn Noise ch" + emptySlot + " " + note.ToString());
                            break;
                        case ToneType.DPCM:
                            dpcmOnSounds.Add(snd);
                            FormMain.OutputDebugLog("KeyOn DPCM ch" + emptySlot + " " + note.ToString());
                            break;
                        case ToneType.FDS:
                            fdsOnSounds.Add(snd);
                            FormMain.OutputDebugLog("KeyOn FDS ch" + emptySlot + " " + note.ToString());
                            break;
                        case ToneType.VRC6_SQ:
                            vrc6SqOnSounds.Add(snd);
                            FormMain.OutputDebugLog("KeyOn VRC6(SQ) ch" + emptySlot + " " + note.ToString());
                            break;
                        case ToneType.VRC6_SAW:
                            vrc6SawOnSounds.Add(snd);
                            FormMain.OutputDebugLog("KeyOn VRC6(Saw) ch" + emptySlot + " " + note.ToString());
                            break;
                    }
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
            private (RP2A03 inst, int slot) searchEmptySlot(TaggedNoteOnEvent note, RP2A03Timbre timbre)
            {
                var emptySlot = (parentModule, -1);

                switch (timbre.ToneType)
                {
                    case ToneType.SQUARE:
                        {
                            emptySlot = SearchEmptySlotAndOffForLeader(parentModule, sqOnSounds, note, 2);
                            break;
                        }
                    case ToneType.TRIANGLE:
                        {
                            emptySlot = SearchEmptySlotAndOffForLeader(parentModule, triOnSounds, note, 1);
                            break;
                        }
                    case ToneType.NOISE:
                        {
                            emptySlot = SearchEmptySlotAndOffForLeader(parentModule, noiseOnSounds, note, 1);
                            break;
                        }
                    case ToneType.DPCM:
                        {
                            emptySlot = SearchEmptySlotAndOffForLeader(parentModule, dpcmOnSounds, note, 1);
                            break;
                        }
                    case ToneType.FDS:
                        {
                            emptySlot = SearchEmptySlotAndOffForLeader(parentModule, fdsOnSounds, note, 1);
                            break;
                        }
                    case ToneType.VRC6_SQ:
                        {
                            emptySlot = SearchEmptySlotAndOffForLeader(parentModule, vrc6SqOnSounds, note, 2);
                            break;
                        }
                    case ToneType.VRC6_SAW:
                        {
                            emptySlot = SearchEmptySlotAndOffForLeader(parentModule, vrc6SawOnSounds, note, 1);
                            break;
                        }
                }

                return emptySlot;
            }

            internal override void AllSoundOff()
            {
                var me = new ControlChangeEvent((SevenBitNumber)120, (SevenBitNumber)0);
                ControlChange(me);

                RP2A03WriteData(parentModule.UnitNumber, 0x08, 0x80);
                RP2A03WriteData(parentModule.UnitNumber, 0x15, 0);

                RP2A03WriteData(parentModule.UnitNumber, 0x83, 0xc0);

                RP2A03WriteData(parentModule.UnitNumber, (uint)(0x9002 + (0 << 12)), 0x00);
                RP2A03WriteData(parentModule.UnitNumber, (uint)(0x9002 + (1 << 12)), 0x00);

                RP2A03WriteData(parentModule.UnitNumber, 0xb002, 0x00);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private class RP2A03Sound : SoundBase
        {

            private RP2A03 parentModule;

            private RP2A03Timbre timbre;

            private ToneType lastToneType;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentModule"></param>
            /// <param name="noteOnEvent"></param>
            /// <param name="programNumber"></param>
            /// <param name="slot"></param>
            public RP2A03Sound(RP2A03 parentModule, RP2A03SoundManager manager, TimbreBase timbre, TaggedNoteOnEvent noteOnEvent, int slot) : base(parentModule, manager, timbre, noteOnEvent, slot)
            {
                this.parentModule = parentModule;
                this.timbre = (RP2A03Timbre)timbre;

                lastToneType = this.timbre.ToneType;
            }

            /// <summary>
            /// 
            /// </summary>
            public override void KeyOn()
            {
                base.KeyOn();

                switch (lastToneType)
                {
                    case ToneType.SQUARE:
                        {
                            byte data = (byte)RP2A03ReadData(parentModule.UnitNumber, 0x15);
                            RP2A03WriteData(parentModule.UnitNumber, 0x15, (byte)(data | (1 << Slot)));

                            RP2A03WriteData(parentModule.UnitNumber, (uint)((Slot * 4) + 0x01),
                                (byte)(timbre.SQSweep.Enable << 7 | timbre.SQSweep.UpdateRate << 4 |
                                timbre.SQSweep.Direction << 3 | timbre.SQSweep.Range));

                            //Volume
                            updateSqVolume();
                            //Freq
                            updateSqPitch();

                            break;
                        }
                    case ToneType.TRIANGLE:
                        {
                            byte data = (byte)RP2A03ReadData(parentModule.UnitNumber, 0x15);
                            RP2A03WriteData(parentModule.UnitNumber, 0x15, (byte)(data | (1 << 2)));

                            RP2A03WriteData(parentModule.UnitNumber, (uint)((2 * 4) + 0x00),
                                (byte)(timbre.LengthCounterDisable << 7 | timbre.TriCounterLength));

                            //Volume
                            updateTriVolume();

                            //Freq
                            updateTriPitch();

                            break;
                        }
                    case ToneType.NOISE:
                        {
                            byte data = (byte)RP2A03ReadData(parentModule.UnitNumber, 0x15);
                            RP2A03WriteData(parentModule.UnitNumber, 0x15, (byte)(data | 8));

                            //Volume
                            updateNoiseVolume();
                            //Freq
                            updateNoisePitch();

                            break;
                        }
                    case ToneType.DPCM:
                        {
                            //https://wiki.nesdev.com/w/index.php/APU_DMC

                            int noteNum = NoteOnEvent.NoteNumber;

                            //keyoff
                            byte data = (byte)(RP2A03ReadData(parentModule.UnitNumber, 0x15) & ~(1 << 4));
                            RP2A03WriteData(parentModule.UnitNumber, 0x15, (byte)data);

                            // Loop / Smple Rate
                            RP2A03WriteData(parentModule.UnitNumber, (uint)0x10,
                                (byte)(timbre.DeltaPcmLoopEnable << 6 | timbre.DeltaPcmBitRate));

                            //Size
                            if (parentModule.DeltaPcmSoundTable.PcmTimbres[noteNum].PcmData != null)
                            {
                                RP2A03SetDpcm(parentModule.UnitNumber, parentModule.DeltaPcmSoundTable.PcmTimbres[noteNum].PcmData);
                                int sz = parentModule.DeltaPcmSoundTable.PcmTimbres[noteNum].PcmData.Length - 1;
                                if (sz > 4081)
                                    sz = 4081;
                                if (sz >= 16)
                                {
                                    RP2A03WriteData(parentModule.UnitNumber, (uint)0x13, (byte)((sz / 16) & 0xff));
                                    //keyon
                                    RP2A03WriteData(parentModule.UnitNumber, 0x15, (byte)(data | 16));
                                }
                            }

                            break;
                        }
                    case ToneType.FDS:
                        {
                            //Set WSG
                            RP2A03WriteData(parentModule.UnitNumber, 0x89, (byte)0x80);
                            for (int i = 0; i < timbre.FDS.WsgData.Length; i++)
                                RP2A03WriteData(parentModule.UnitNumber, (uint)(0x40 + i), (byte)(timbre.FDS.WsgData[i]));
                            RP2A03WriteData(parentModule.UnitNumber, 0x89, (byte)0x03);

                            //Set LFO
                            if (timbre.FDS.LfoFreq == 0)
                            {
                                RP2A03WriteData(parentModule.UnitNumber, 0x87, (byte)0x80);
                            }
                            else
                            {
                                RP2A03WriteData(parentModule.UnitNumber, 0x84, (byte)(0x80 | timbre.FDS.LfoGain));
                                RP2A03WriteData(parentModule.UnitNumber, 0x85, (byte)0x00);
                                RP2A03WriteData(parentModule.UnitNumber, 0x87, (byte)0x87);
                                for (int i = 0; i < timbre.FDS.LfoData.Length; i++)
                                    RP2A03WriteData(parentModule.UnitNumber, (uint)(0x88), (byte)(timbre.FDS.LfoData[i]));
                                RP2A03WriteData(parentModule.UnitNumber, 0x86, (byte)(timbre.FDS.LfoFreq & 0xff));
                                RP2A03WriteData(parentModule.UnitNumber, 0x87, (byte)((timbre.FDS.LfoFreq >> 8) & 0xf));
                            }

                            //Volume
                            updateFdsVolume();
                            //Freq
                            updateFdsPitch();
                            break;
                        }
                    case ToneType.VRC6_SQ:
                        updateVrc6SQVolume();
                        updateVrc6SQPitch();
                        break;
                    case ToneType.VRC6_SAW:
                        updateVrc6SawVolume();
                        updateVrc6SawPitch();
                        break;
                }
            }


            public override void OnSoundParamsUpdated()
            {
                base.OnSoundParamsUpdated();

                switch (lastToneType)
                {
                    case ToneType.SQUARE:
                        {
                            RP2A03WriteData(parentModule.UnitNumber, (uint)((Slot * 4) + 0x01),
                                (byte)(timbre.SQSweep.Enable << 7 | timbre.SQSweep.UpdateRate << 4 |
                                timbre.SQSweep.Direction << 3 | timbre.SQSweep.Range));

                            //Volume
                            updateSqVolume();
                            //Freq
                            updateSqPitch();

                            break;
                        }
                    case ToneType.TRIANGLE:
                        {
                            //Volume
                            updateTriVolume();
                            //Freq
                            updateTriPitch();

                            break;
                        }
                    case ToneType.NOISE:
                        {
                            //Volume
                            updateNoiseVolume();
                            //Freq
                            updateNoisePitch();

                            break;
                        }
                    case ToneType.DPCM:
                        {
                            //https://wiki.nesdev.com/w/index.php/APU_DMC

                            // Loop / Smple Rate
                            RP2A03WriteData(parentModule.UnitNumber, (uint)0x10,
                                (byte)(timbre.DeltaPcmLoopEnable << 6 | timbre.DeltaPcmBitRate));

                            break;
                        }
                    case ToneType.FDS:
                        {
                            //Volume
                            updateFdsVolume();
                            //Freq
                            updateFdsPitch();
                            break;
                        }
                    case ToneType.VRC6_SQ:
                        updateVrc6SQVolume();
                        updateVrc6SQPitch();
                        break;
                    case ToneType.VRC6_SAW:
                        updateVrc6SawVolume();
                        updateVrc6SawPitch();
                        break;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public override void OnVolumeUpdated()
            {
                switch (lastToneType)
                {
                    case ToneType.SQUARE:
                        updateSqVolume();
                        break;
                    case ToneType.TRIANGLE:
                        updateTriVolume();
                        break;
                    case ToneType.NOISE:
                        updateNoiseVolume();
                        break;
                    case ToneType.DPCM:
                        updateDpcmVolume();
                        break;
                    case ToneType.FDS:
                        updateFdsVolume();
                        break;
                    case ToneType.VRC6_SQ:
                        updateVrc6SQVolume();
                        break;
                    case ToneType.VRC6_SAW:
                        updateVrc6SawVolume();
                        break;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            private void updateSqVolume()
            {
                byte fv = (byte)((byte)Math.Round(timbre.Volume * CalcCurrentVolume()) & 0xf);

                byte dd = timbre.DecayDisable;
                byte ld = timbre.LengthCounterDisable;
                byte dc = timbre.SQDutyCycle;

                if (FxEngine != null && FxEngine.Active)
                {
                    var eng = (NesFxEngine)FxEngine;
                    if (eng.DutyValue != null)
                        dc = (byte)(eng.DutyValue.Value & 3);
                }

                RP2A03WriteData(parentModule.UnitNumber, (uint)((Slot * 4) + 0x00), (byte)(dc << 6 | ld << 5 | dd << 4 | fv));
            }

            /// <summary>
            /// 
            /// </summary>
            private void updateTriVolume()
            {
                var fv = Math.Round(timbre.Volume * CalcCurrentVolume());

                if (fv < 0.01)
                {
                    byte data = (byte)(RP2A03ReadData(parentModule.UnitNumber, 0x15) & ~(1 << 2));
                    RP2A03WriteData(parentModule.UnitNumber, 0x15, data);
                }
                else
                {
                    byte data = (byte)RP2A03ReadData(parentModule.UnitNumber, 0x15);
                    RP2A03WriteData(parentModule.UnitNumber, 0x15, (byte)(data | (1 << 2)));

                    RP2A03WriteData(parentModule.UnitNumber, (uint)((2 * 4) + 0x00),
                        (byte)(timbre.LengthCounterDisable << 7 | timbre.TriCounterLength));
                }
            }

            /// <summary>
            /// 
            /// </summary>
            private void updateNoiseVolume()
            {
                byte fv = (byte)((byte)Math.Round(timbre.Volume * CalcCurrentVolume()) & 0xf);

                byte dd = timbre.DecayDisable;
                byte ld = timbre.LengthCounterDisable;

                RP2A03WriteData(parentModule.UnitNumber, (uint)(0x0c), (byte)(ld << 5 | dd << 4 | fv));
            }

            /// <summary>
            /// 
            /// </summary>
            private void updateDpcmVolume()
            {
                var vol = parentModule.Volumes[NoteOnEvent.Channel];

                RP2A03WriteData(parentModule.UnitNumber, (uint)(0x11), vol);
            }


            /// <summary>
            /// 
            /// </summary>
            private void updateFdsVolume()
            {
                RP2A03WriteData(parentModule.UnitNumber, 0x80, (byte)0xbf);
                RP2A03WriteData(parentModule.UnitNumber, 0x89, calcFdsVol());
            }

            private byte calcFdsVol()
            {
                var cv = CalcCurrentVolume();
                if (cv > 0.4 - 0.2)
                {
                    byte fv = 3;
                    if (cv > 0.5 - 0.05)
                    {
                        if (cv > 0.6666666666666667 - 0.0833333333333333)
                        {
                            if (cv > 1.0 - 0.1666666666666667)
                                fv = 0;
                            else
                                fv = 1;
                        }
                        else
                        {
                            fv = 2;
                        }
                    }
                    return fv;
                }
                else
                {
                    return 4;   //HACK:
                }
            }

            private void updateVrc6SQVolume()
            {
                byte fv = (byte)((byte)Math.Round(15 * CalcCurrentVolume()) & 0xf);

                byte dc = timbre.VRC6.SQDuty;
                if (FxEngine != null && FxEngine.Active)
                {
                    var eng = (NesFxEngine)FxEngine;
                    if (eng.DutyValue != null)
                        dc = eng.DutyValue.Value;
                }

                RP2A03WriteData(parentModule.UnitNumber, (uint)(0x9000 + (Slot << 12)), (byte)(dc << 4 | fv));
            }

            private void updateVrc6SawVolume()
            {
                byte fv = (byte)((byte)Math.Round(15 * CalcCurrentVolume()) & 0xf);

                RP2A03WriteData(parentModule.UnitNumber, (uint)(0xb000), fv);
            }

            /// <summary>
            /// 
            /// </summary>
            public override void OnPitchUpdated()
            {
                switch (lastToneType)
                {
                    case ToneType.SQUARE:
                        updateSqPitch();
                        break;
                    case ToneType.TRIANGLE:
                        updateTriPitch();
                        break;
                    case ToneType.NOISE:
                        updateNoisePitch();
                        break;
                    case ToneType.FDS:
                        updateFdsPitch();
                        break;
                    case ToneType.VRC6_SQ:
                        updateVrc6SQPitch();
                        break;
                    case ToneType.VRC6_SAW:
                        updateVrc6SawPitch();
                        break;
                }

                base.OnPitchUpdated();
            }

            private void updateSqPitch()
            {
                double freq = CalcCurrentFrequency();
                freq = Math.Round((21477272d / 12d) / (freq * 16)) - 1;
                if (freq > 0x7ff)
                    freq = 0x7ff;
                var n = (ushort)freq;

                RP2A03WriteData(parentModule.UnitNumber, (uint)((Slot * 4) + 0x02), (byte)(n & 0xff));
                RP2A03WriteData(parentModule.UnitNumber, (uint)((Slot * 4) + 0x03), (byte)((timbre.PlayLength << 3) | (n >> 8) & 0x7));
            }

            private void updateTriPitch()
            {
                double freq = CalcCurrentFrequency();
                freq = Math.Round((21477272d / 12d) / (freq * 32)) - 1;
                if (freq > 0x7ff)
                    freq = 0x7ff;
                var n = (ushort)freq;

                RP2A03WriteData(parentModule.UnitNumber, (uint)((2 * 4) + 0x02), (byte)(n & 0xff));
                RP2A03WriteData(parentModule.UnitNumber, (uint)((2 * 4) + 0x03), (byte)((timbre.PlayLength << 3) | (n >> 8) & 0x7));
            }

            private void updateNoisePitch()
            {
                double d = CalcCurrentPitchDeltaNoteNumber() * 63d;

                int kf = 0;
                if (d > 0)
                    kf = (int)d % 63;
                else if (d < 0)
                    kf = 63 + ((int)d % 63);

                int noted = (int)d / 63;
                if (d < 0)
                    noted -= 1;

                int nn = NoteOnEvent.NoteNumber;
                if (ParentModule.ChannelTypes[NoteOnEvent.Channel] == ChannelType.Drum)
                    nn = (int)ParentModule.DrumTimbres[NoteOnEvent.NoteNumber].BaseNote;
                int noteNum = nn + noted;
                if (noteNum > 127)
                    noteNum = 127;
                else if (noteNum < 0)
                    noteNum = 0;
                int n = 31 - (noteNum % 32);

                /*
                var pitch = (int)(parentModule.Pitchs[NoteOnEvent.Channel] - 8192) / (8192 / 32);
                int n = 31 - ((NoteOnEvent.NoteNumber + pitch) % 32);
                */

                var nt = timbre.NoiseType;
                if (FxEngine != null && FxEngine.Active)
                {
                    var eng = (NesFxEngine)FxEngine;
                    if (eng.DutyValue != null)
                        nt = (byte)(eng.DutyValue.Value & 1);
                }

                RP2A03WriteData(parentModule.UnitNumber, (uint)((3 * 4) + 0x02), (byte)((nt << 7) | (n & 0xf)));
                RP2A03WriteData(parentModule.UnitNumber, (uint)((3 * 4) + 0x03), (byte)(timbre.PlayLength << 3));
            }

            private void updateFdsPitch()
            {
                double freq = CalcCurrentFrequency();
                // p = 65536 * f / 1789773d
                freq = Math.Round(64 * 65536 * freq / (21477272d / 12d));
                if (freq > 0x7ff)
                    freq = 0x7ff;
                var n = (ushort)freq;

                RP2A03WriteData(parentModule.UnitNumber, 0x82, (byte)(n & 0xff));
                RP2A03WriteData(parentModule.UnitNumber, 0x83, (byte)((n >> 8) & 0x7));
            }

            private void updateVrc6SQPitch()
            {
                double freq = CalcCurrentFrequency();
                freq = Math.Round((21477272d / 12d) / (16 * freq)) - 1;
                if (freq > 0x7ff)
                    freq = 0x7ff;
                var n = (ushort)freq;

                RP2A03WriteData(parentModule.UnitNumber, (uint)(0x9001 + (Slot << 12)), (byte)(n & 0xff));
                RP2A03WriteData(parentModule.UnitNumber, (uint)(0x9002 + (Slot << 12)), (byte)(0x80 | (n >> 8) & 0x7));
            }

            private void updateVrc6SawPitch()
            {
                double freq = CalcCurrentFrequency();
                //t = (CPU / (14 * f)) - 1
                freq = Math.Round(((21477272d / 12d) / (14 * freq)) - 1);
                if (freq > 0x7ff)
                    freq = 0x7ff;
                var n = (ushort)freq;

                RP2A03WriteData(parentModule.UnitNumber, (uint)(0xB001), (byte)(n & 0xff));
                RP2A03WriteData(parentModule.UnitNumber, (uint)(0xB002), (byte)(0x80 | (n >> 8) & 0x7));
            }

            public override void SoundOff()
            {
                base.SoundOff();

                switch (lastToneType)
                {
                    case ToneType.SQUARE:
                        {
                            byte data = (byte)(RP2A03ReadData(parentModule.UnitNumber, 0x15) & ~(1 << Slot));
                            RP2A03WriteData(parentModule.UnitNumber, 0x15, data);
                            break;
                        }
                    case ToneType.TRIANGLE:
                        {
                            RP2A03WriteData(parentModule.UnitNumber, 0x08, 0x80);

                            byte data = (byte)(RP2A03ReadData(parentModule.UnitNumber, 0x15) & ~(1 << 2));
                            RP2A03WriteData(parentModule.UnitNumber, 0x15, data);
                            break;
                        }
                    case ToneType.NOISE:
                        {
                            byte data = (byte)(RP2A03ReadData(parentModule.UnitNumber, 0x15) & ~8);
                            RP2A03WriteData(parentModule.UnitNumber, 0x15, data);
                            break;
                        }
                    case ToneType.FDS:
                        {
                            RP2A03WriteData(parentModule.UnitNumber, 0x83, 0xc0);
                            break;
                        }
                    case ToneType.VRC6_SQ:
                        {
                            RP2A03WriteData(parentModule.UnitNumber, (uint)(0x9002 + (Slot << 12)), 0x00);
                            break;
                        }
                    case ToneType.VRC6_SAW:
                        {
                            RP2A03WriteData(parentModule.UnitNumber, 0xb002, 0x00);
                            break;
                        }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [JsonConverter(typeof(NoTypeConverterJsonConverter<RP2A03Timbre>))]
        [DataContract]
        public class RP2A03Timbre : TimbreBase
        {

            [DataMember]
            [Category("Sound")]
            [Description("Sound Type")]
            [DefaultValue(ToneType.SQUARE)]
            public ToneType ToneType
            {
                get;
                set;
            }

            private byte f_Volume = 15;

            [Browsable(false)]
            [DataMember]
            [Category("Sound(SQ/Noise)")]
            [Description("Square/Noise Volume (0-15)")]
            [DefaultValue((byte)15)]
            [SlideParametersAttribute(0, 15)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte Volume
            {
                get
                {
                    return f_Volume;
                }
                set
                {
                    f_Volume = (byte)(value & 15);
                }
            }

            private byte f_DecayDisable = 1;

            [DataMember]
            [Category("Sound(SQ)")]
            [Description("Square/Noise Envelope Decay Disable (0:Enable 1:Disable)")]
            [DefaultValue((byte)1)]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte DecayDisable
            {
                get
                {
                    return f_DecayDisable;
                }
                set
                {
                    f_DecayDisable = (byte)(value & 1);
                }
            }


            private byte f_LengthDisable = 1;

            //[Browsable(false)]
            [DataMember]
            [Category("Sound(SQ/Tri)")]
            [Description("Square/Tri Length Counter Clock Disable (0:Enable 1:Disable)")]
            [DefaultValue((byte)1)]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte LengthCounterDisable
            {
                get
                {
                    return f_LengthDisable;
                }
                set
                {
                    f_LengthDisable = (byte)(value & 1);
                }
            }

            /*
            private byte f_NoiseEnvDecayEnable;

            [DataMember]
            [Category("Sound(Noise)")]
            [Description("Noise Envelope Decay Looping Enable (0:Disable 1:Enable)")]
            public byte NoiseEnvDecayEnable
            {
                get
                {
                    return f_NoiseEnvDecayEnable;
                }
                set
                {
                    f_NoiseEnvDecayEnable = (byte)(value & 1);
                }
            }
            */

            private byte f_SQDutyCycle;

            [DataMember]
            [Category("Sound(SQ)")]
            [Description("Square Duty Cycle (0:87.5% 1:75% 2:50% 3:25%)")]
            [SlideParametersAttribute(0, 3)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue((byte)0)]
            public byte SQDutyCycle
            {
                get
                {
                    return f_SQDutyCycle;
                }
                set
                {
                    f_SQDutyCycle = (byte)(value & 3);
                }
            }

            /// <summary>
            /// 
            /// </summary>
            [DataMember]
            [Category("Sound(SQ)")]
            [Description("Square Wave Sweep Settings")]
            public SQSweepSettings SQSweep
            {
                get;
                private set;
            }

            private byte f_NoiseType;

            [DataMember]
            [Category("Sound(Noise)")]
            [Description("Noise Type (0:32k bit 1:93bit)")]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue((byte)0)]
            public byte NoiseType
            {
                get
                {
                    return f_NoiseType;
                }
                set
                {
                    f_NoiseType = (byte)(value & 1);
                }
            }


            private byte f_PlayLength;

            //[Browsable(false)]
            [DataMember]
            [Category("Sound")]
            [Description("Square/Tri Play Length (0-31)")]
            [SlideParametersAttribute(0, 31)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue((byte)0)]
            public byte PlayLength
            {
                get
                {
                    return f_PlayLength;
                }
                set
                {
                    f_PlayLength = (byte)(value & 31);
                }
            }


            private byte f_TriCounterLength = 127;

            //[Browsable(false)]
            [DataMember]
            [Category("Sound(Tri)")]
            [Description("Tri Linear Counter Length (0-127)")]
            [DefaultValue((byte)127)]
            [SlideParametersAttribute(0, 127)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte TriCounterLength
            {
                get
                {
                    return f_TriCounterLength;
                }
                set
                {
                    f_TriCounterLength = (byte)(value & 127);
                }
            }


            private byte f_DeltaPcmBitRate = 15;

            [DataMember]
            [Category("Sound(DPCM)")]
            [Description("DPCM Sample Bit Rate (0:4KHz-15:32KHz)")]
            [DefaultValue((byte)15)]
            [SlideParametersAttribute(0, 15)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte DeltaPcmBitRate
            {
                get
                {
                    return f_DeltaPcmBitRate;
                }
                set
                {
                    f_DeltaPcmBitRate = (byte)(value & 15);
                }
            }

            private byte f_DeltaPcmLoop;

            [DataMember]
            [Category("Sound(DPCM)")]
            [Description("DPCM Loop Play (0:Off 1:On)")]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue((byte)0)]
            public byte DeltaPcmLoopEnable
            {
                get
                {
                    return f_DeltaPcmLoop;
                }
                set
                {
                    f_DeltaPcmLoop = (byte)(value & 1);
                }
            }

            [DataMember]
            [Category("Sound(FDS)")]
            [Description("FDS Tone Settings")]
            public FdsSettings FDS
            {
                get;
                set;
            }

            [DataMember]
            [Category("Sound(VRC6)")]
            [Description("VRC6 Tone Settings")]
            public Vrc6Settings VRC6
            {
                get;
                set;
            }

            /// <summary>
            /// 
            /// </summary>
            public RP2A03Timbre()
            {
                SQSweep = new SQSweepSettings();
                SDS.FxS = new NesFxSettings();
                FDS = new FdsSettings();
                VRC6 = new Vrc6Settings();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="serializeData"></param>
            public override void RestoreFrom(string serializeData)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<RP2A03Timbre>(serializeData);
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


        [JsonConverter(typeof(NoTypeConverterJsonConverter<FdsSettings>))]
        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [DataContract]
        [MidiHook]
        public class FdsSettings : ContextBoundObject
        {

            private byte[] f_WsgData = new byte[64];

            [TypeConverter(typeof(ArrayConverter))]
            [Editor(typeof(WsgUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [WsgBitWideAttribute(6)]
            [DataMember]
            [Category("Sound(FDS)")]
            [Description("Wave Table (64 samples, 0-63 levels)")]
            public byte[] WsgData
            {
                get
                {
                    return f_WsgData;
                }
                set
                {
                    f_WsgData = value;
                }
            }

            public bool ShouldSerializeWsgData()
            {
                foreach (var dt in WsgData)
                {
                    if (dt != 0)
                        return true;
                }
                return false;
            }

            public void ResetWsgData()
            {
                f_WsgData = new byte[64];
            }

            [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
            typeof(UITypeEditor)), Localizable(false)]
            [Category("Sound(FDS)")]
            [Description("FDS Wave Table (64 samples, 0-63 levels)")]
            [IgnoreDataMember]
            [JsonIgnore]
            public string WsgDataSerializeData
            {
                get
                {
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < WsgData.Length; i++)
                    {
                        if (sb.Length != 0)
                            sb.Append(' ');
                        sb.Append(WsgData[i].ToString((IFormatProvider)null));
                    }
                    return sb.ToString();
                }
                set
                {
                    string[] vals = value.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    List<byte> vs = new List<byte>();
                    foreach (var val in vals)
                    {
                        byte v = 0;
                        if (byte.TryParse(val, out v))
                            vs.Add(v);
                    }
                    for (int i = 0; i < Math.Min(WsgData.Length, vs.Count); i++)
                        WsgData[i] = vs[i] > 63 ? (byte)63 : vs[i];
                }
            }

            private uint f_LfoFreq;

            [DataMember]
            [Category("Sound(FDS)")]
            [Description("FDS LFO Frequency(0 - 4095)")]
            [SlideParametersAttribute(0, 4095)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public uint LfoFreq
            {
                get
                {
                    return f_LfoFreq;
                }
                set
                {
                    f_LfoFreq = (uint)(value & 4095);
                }
            }

            public bool ShouldSerializeLfoFreq()
            {
                return f_LfoFreq != 0;
            }

            public void ResetLfoFreq()
            {
                f_LfoFreq = 0;
            }


            private byte f_LfoGain;

            [DataMember]
            [Category("Sound(FDS)")]
            [Description("FDS LFO Gain(0 - 63)")]
            [SlideParametersAttribute(0, 63)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue((byte)0)]
            public byte LfoGain
            {
                get
                {
                    return f_LfoGain;
                }
                set
                {
                    f_LfoGain = (byte)(value & 63);
                }
            }

            private sbyte[] f_LfoData = new sbyte[32];

            [TypeConverter(typeof(ArrayConverter))]
            [Editor(typeof(WsgUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [WsgBitWideAttribute(3)]
            [DataMember]
            [Category("Sound(FDS)")]
            [Description("FDS LFO Table (32 steps, 0-7 levels)")]
            public sbyte[] LfoData
            {
                get
                {
                    return f_LfoData;
                }
                set
                {
                    f_LfoData = value;
                }
            }

            public bool ShouldSerializeLfoData()
            {
                foreach (var dt in LfoData)
                {
                    if (dt != 0)
                        return true;
                }
                return false;
            }

            public void ResetLfoData()
            {
                f_LfoData = new sbyte[32];
            }

            [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
            typeof(UITypeEditor)), Localizable(false)]
            [Category("Sound(FDS)")]
            [Description("FDS LFO Table (32 steps, 0-7 levels)")]
            [IgnoreDataMember]
            [JsonIgnore]
            public string LfoDataSerializeData
            {
                get
                {
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < LfoData.Length; i++)
                    {
                        if (sb.Length != 0)
                            sb.Append(' ');
                        sb.Append(LfoData[i].ToString((IFormatProvider)null));
                    }
                    return sb.ToString();
                }
                set
                {
                    string[] vals = value.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    var vs = new List<sbyte>();
                    foreach (var val in vals)
                    {
                        sbyte v = 0;
                        if (sbyte.TryParse(val, out v))
                            vs.Add(v);
                    }
                    for (int i = 0; i < Math.Min(WsgData.Length, vs.Count); i++)
                    {
                        var val = vs[i];
                        if (val > 3)
                            val = 3;
                        else if (val < -4)
                            val = -4;
                        LfoData[i] = val;
                    }
                }
            }

        }


        [JsonConverter(typeof(NoTypeConverterJsonConverter<Vrc6Settings>))]
        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [DataContract]
        [MidiHook]
        public class Vrc6Settings : ContextBoundObject
        {

            private byte f_SQDuty;

            [DataMember]
            [Category("Sound(VRC6)")]
            [Description("SQ Duty(0 - 7)\r\n" +
                "0	 1/16     6.25 %\r\n" +
                "1   2/16    12.50 %\r\n" +
                "2   3/16    18.75 %\r\n" +
                "3   4/16    25.00 %\r\n" +
                "4   5/16    31.25 %\r\n" +
                "5   6/16    37.50 %\r\n" +
                "6   7/16    43.75 %\r\n" +
                "7   8/16    50.00 %")]
            [SlideParametersAttribute(0, 7)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue((byte)0)]
            public byte SQDuty
            {
                get
                {
                    return f_SQDuty;
                }
                set
                {
                    f_SQDuty = (byte)(value & 0x7);
                }
            }

        }


        [JsonConverter(typeof(NoTypeConverterJsonConverter<NesFxSettings>))]
        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [DataContract]
        [MidiHook]
        public class NesFxSettings : BasicFxSettings
        {

            private string f_DutyEnvelopes;

            [DataMember]
            [Description("Set duty/noise envelop by text. Input duty/noise value and split it with space like the Famitracker.\r\n" +
                       "0-3(0-7:VRC6) \"|\" is repeat point. \"/\" is release point.")]
            [Editor(typeof(EnvelopeUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [EnvelopeEditorAttribute(0, 7)]
            public string DutyEnvelopes
            {
                get
                {
                    return f_DutyEnvelopes;
                }
                set
                {
                    if (f_DutyEnvelopes != value)
                    {
                        DutyEnvelopesRepeatPoint = -1;
                        DutyEnvelopesReleasePoint = -1;
                        if (value == null)
                        {
                            DutyEnvelopesNums = new int[] { };
                            f_DutyEnvelopes = string.Empty;
                            return;
                        }
                        f_DutyEnvelopes = value;
                        string[] vals = value.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        List<int> vs = new List<int>();
                        for (int i = 0; i < vals.Length; i++)
                        {
                            string val = vals[i];
                            if (val.Equals("|", StringComparison.Ordinal))
                                DutyEnvelopesRepeatPoint = vs.Count;
                            else if (val.Equals("/", StringComparison.Ordinal))
                                DutyEnvelopesReleasePoint = vs.Count;
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
                        DutyEnvelopesNums = vs.ToArray();

                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < DutyEnvelopesNums.Length; i++)
                        {
                            if (sb.Length != 0)
                                sb.Append(' ');
                            if (DutyEnvelopesRepeatPoint == i)
                                sb.Append("| ");
                            if (DutyEnvelopesReleasePoint == i)
                                sb.Append("/ ");
                            sb.Append(DutyEnvelopesNums[i].ToString((IFormatProvider)null));
                        }
                        f_DutyEnvelopes = sb.ToString();
                    }
                }
            }

            public bool ShouldSerializeDutyEnvelopes()
            {
                return !string.IsNullOrEmpty(DutyEnvelopes);
            }

            public void ResetDutyEnvelopes()
            {
                DutyEnvelopes = null;
            }

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            public int[] DutyEnvelopesNums { get; set; } = new int[] { };

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int DutyEnvelopesRepeatPoint { get; set; } = -1;

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int DutyEnvelopesReleasePoint { get; set; } = -1;

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override AbstractFxEngine CreateEngine()
            {
                return new NesFxEngine(this);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public class NesFxEngine : BasicFxEngine
        {
            private NesFxSettings settings;

            /// <summary>
            /// 
            /// </summary>
            public NesFxEngine(NesFxSettings settings) : base(settings)
            {
                this.settings = settings;
            }

            private uint f_dutyCounter;

            public byte? DutyValue
            {
                get;
                private set;
            }

            protected override bool ProcessCore(SoundBase sound, bool isKeyOff, bool isSoundOff)
            {
                bool process = base.ProcessCore(sound, isKeyOff, isSoundOff);

                DutyValue = null;
                if (settings.DutyEnvelopesNums.Length > 0)
                {
                    if (!isKeyOff)
                    {
                        var vm = settings.DutyEnvelopesNums.Length;
                        if (settings.DutyEnvelopesReleasePoint >= 0)
                            vm = settings.DutyEnvelopesReleasePoint;
                        if (f_dutyCounter >= vm)
                        {
                            if (settings.DutyEnvelopesRepeatPoint >= 0)
                                f_dutyCounter = (uint)settings.DutyEnvelopesRepeatPoint;
                            else
                                f_dutyCounter = (uint)vm;
                        }
                    }
                    else
                    {
                        if (settings.DutyEnvelopesReleasePoint < 0)
                            f_dutyCounter = (uint)settings.DutyEnvelopesNums.Length;

                        //if (f_dutyCounter >= settings.DutyEnvelopesNums.Length)
                        //{
                        //    if (settings.DutyEnvelopesRepeatPoint >= 0)
                        //        f_dutyCounter = (uint)settings.DutyEnvelopesRepeatPoint;
                        //}
                    }
                    if (f_dutyCounter < settings.DutyEnvelopesNums.Length)
                    {
                        int vol = settings.DutyEnvelopesNums[f_dutyCounter++];

                        DutyValue = (byte)vol;
                        process = true;
                    }
                }

                return process;
            }
        }

        [JsonConverter(typeof(NoTypeConverterJsonConverter<SQSweepSettings>))]
        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [DataContract]
        [MidiHook]
        public class SQSweepSettings : ContextBoundObject
        {

            private byte f_Enable;

            [DataMember]
            [Category("Sound(SQ)")]
            [Description("Square Sweep Enable (0:Disable 1:Enable)")]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue((byte)0)]
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

            private byte f_UpdateRate;

            [DataMember]
            [Category("Sound(SQ)")]
            [Description("Square Sweep Update Rate (0-7)")]
            [SlideParametersAttribute(0, 7)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue((byte)0)]
            public byte UpdateRate
            {
                get
                {
                    return f_UpdateRate;
                }
                set
                {
                    f_UpdateRate = (byte)(value & 7);
                }
            }

            private byte f_Direction;

            [DataMember]
            [Category("Sound(SQ)")]
            [Description("Wave Length (0:Decrease 1:Increse)")]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue((byte)0)]
            public byte Direction
            {
                get
                {
                    return f_Direction;
                }
                set
                {
                    f_Direction = (byte)(value & 1);
                }
            }

            private byte f_Range;

            [DataMember]
            [Category("Sound(SQ)")]
            [Description("Wave Length (0-7)")]
            [SlideParametersAttribute(0, 7)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue((byte)0)]
            public byte Range
            {
                get
                {
                    return f_Range;
                }
                set
                {
                    f_Range = (byte)(value & 7);
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public enum ToneType
        {
            SQUARE,
            TRIANGLE,
            NOISE,
            DPCM,
            FDS,
            VRC6_SQ,
            VRC6_SAW,
        }

        /// <summary>
        /// 
        /// </summary>
        [DataContract]
        public class DPcmSoundTable : PcmTimbreTableBase
        {
            /// <summary>
            /// 
            /// </summary>
            public DPcmSoundTable()
            {
                for (int i = 0; i < 128; i++)
                    PcmTimbres[i] = new DeltaPcmTimbre(i);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [DataContract]
        public class DeltaPcmTimbre : PcmTimbreBase
        {

            private byte[] f_DeltaPcmData;

            /// <summary>
            /// 
            /// </summary>
            [DataMember]
            [Browsable(false)]
            public override byte[] PcmData
            {
                get
                {
                    return f_DeltaPcmData;
                }
                set
                {
                    if (value != null)
                    {
                        List<byte> al = new List<byte>(value);
                        //Max 4081
                        if (al.Count > 4081)
                            al.RemoveRange(4081, al.Count - 4081);
                        //Pad
                        if ((al.Count - 1) % 16 != 0)
                        {
                            for (int i = 0; i < (al.Count - 1) % 16; i++)
                                al.Add(0);
                        }
                        f_DeltaPcmData = al.ToArray();
                    }
                    else
                    {
                        f_DeltaPcmData = value;
                    }
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="noteNumber"></param>
            public DeltaPcmTimbre(int noteNumber) : base(noteNumber)
            {
            }
        }

    }
}