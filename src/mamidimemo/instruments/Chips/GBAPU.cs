// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;
using System.Linq;
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
using static zanac.MAmidiMEmo.Instruments.Chips.MT32;

//http://mydocuments.g2.xrea.com/html/gb/soundspec.html
//http://bgb.bircd.org/pandocs.htm#soundcontrolregisters
//https://gbdev.gg8.se/wiki/articles/Gameboy_sound_hardware
//http://mydocuments.g2.xrea.com/
//http://marc.rawer.de/Gameboy/Docs/GBCPUman.pdf
//http://www.devrs.com/gb/files/hosted/GBSOUND.txt

namespace zanac.MAmidiMEmo.Instruments.Chips
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    [InstLock]
    public class GB_APU : InstrumentBase
    {
        public override string Name => "GB_APU";

        public override string Group => "PSG";

        public override InstrumentType InstrumentType => InstrumentType.GB_APU;

        [Browsable(false)]
        public override string ImageKey => "GB_APU";

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        protected override string SoundInterfaceTagNamePrefix => "gbsnd_";

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
                return 5;
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
                Timbres = (GBAPUTimbre[])value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category(" Timbres")]
        [Description("Timbres")]
        [EditorAttribute(typeof(TimbresArrayUITypeEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(ExpandableCollectionConverter))]
        public GBAPUTimbre[] Timbres
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
                using (var obj = JsonConvert.DeserializeObject<GB_APU>(serializeData))
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
        private delegate void delegate_gb_apu_write(uint unitNumber, uint address, byte data);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate byte delegate_gb_apu_read(uint unitNumber, uint address);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delegate_gb_apu_wave_write(uint unitNumber, uint address, byte data);

        /// <summary>
        /// 
        /// </summary>
        private static void GbApuWriteData(uint unitNumber, uint address, byte data)
        {
            DeferredWriteData(GbApu_write, unitNumber, address, data);
            /*
            try
            {
                Program.SoundUpdating();
                GbApu_write(unitNumber, address, data);
            }
            finally
            {
                Program.SoundUpdated();
            }*/
        }

        /// <summary>
        /// 
        /// </summary>
        private static delegate_gb_apu_write GbApu_write
        {
            get;
            set;
        }


        /// <summary>
        /// 
        /// </summary>
        private static void GbApuWaveWriteData(uint unitNumber, uint address, byte data)
        {
            DeferredWriteData(GbApu_wave_write, unitNumber, address, data);
            /*
            try
            {
                Program.SoundUpdating();
                GbApu_wave_write(unitNumber, address, data);
            }
            finally
            {
                Program.SoundUpdated();
            }*/
        }

        /// <summary>
        /// 
        /// </summary>
        private static delegate_gb_apu_wave_write GbApu_wave_write
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        private static byte GbApuReadData(uint unitNumber, uint address)
        {
            try
            {
                Program.SoundUpdating();
                FlushDeferredWriteData();

                return GbApu_read(unitNumber, address);
            }
            finally
            {
                Program.SoundUpdated();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private static delegate_gb_apu_read GbApu_read
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        static GB_APU()
        {
            IntPtr funcPtr = MameIF.GetProcAddress("gb_apu_write");
            if (funcPtr != IntPtr.Zero)
                GbApu_write = (delegate_gb_apu_write)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_gb_apu_write));
            funcPtr = MameIF.GetProcAddress("gb_apu_read");
            if (funcPtr != IntPtr.Zero)
                GbApu_read = (delegate_gb_apu_read)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_gb_apu_read));
            funcPtr = MameIF.GetProcAddress("gb_apu_wave_write");
            if (funcPtr != IntPtr.Zero)
                GbApu_wave_write = (delegate_gb_apu_wave_write)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_gb_apu_wave_write));
        }

        private GBSoundManager soundManager;


        private const float DEFAULT_GAIN = 0.4f;

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
        public GB_APU(uint unitNumber) : base(unitNumber)
        {
            GainLeft = DEFAULT_GAIN;
            GainRight = DEFAULT_GAIN;

            Timbres = new GBAPUTimbre[InstrumentBase.DEFAULT_MAX_TIMBRES];
            for (int i = 0; i < InstrumentBase.DEFAULT_MAX_TIMBRES; i++)
                Timbres[i] = new GBAPUTimbre();
            setPresetInstruments();

            this.soundManager = new GBSoundManager(this);
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
        /// <param name="vgmPath"></param>
        public override void StartVgmRecordingTo(string vgmPath)
        {
            base.StartVgmRecordingTo(vgmPath);

            //Sound On
            GbApuWriteData(UnitNumber, 0x16, 0x80);
            GbApuWriteData(UnitNumber, 0x14, 0x77);
        }

        /// <summary>
        /// 
        /// </summary>
        private void setPresetInstruments()
        {
            Timbres[0].SoundType = SoundType.PSG;

            Timbres[1].SoundType = SoundType.WAV;
            Timbres[1].WsgData = new byte[]
            {
                 7, 10, 12, 13, 14, 13, 12, 10,
                 7,  4,  2,  1,  0,  1,  2,  4,
                 7, 11, 13, 14, 13, 11,  7,  3,
                 1,  0,  1,  3,  7, 14,  7,  0,  };
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

        internal override void ResetAll()
        {
            ClearWrittenDataCache();
            PrepareSound();
        }

        /// <summary>
        /// 
        /// </summary>
        private class GBSoundManager : SoundManagerBase
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

            private static SoundList<GbSound> spsgOnSounds = new SoundList<GbSound>(1);

            private static SoundList<GbSound> psgOnSounds = new SoundList<GbSound>(2);

            private static SoundList<GbSound> wavOnSounds = new SoundList<GbSound>(1);

            private static SoundList<GbSound> noiseOnSounds = new SoundList<GbSound>(1);


            private GB_APU parentModule;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            public GBSoundManager(GB_APU parent) : base(parent)
            {
                this.parentModule = parent;

                //Sound On
                GbApuWriteData(parentModule.UnitNumber, 0x16, 0x80);
                GbApuWriteData(parentModule.UnitNumber, 0x14, 0x77);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="note"></param>
            public override SoundBase[] SoundOn(TaggedNoteOnEvent note)
            {
                List<SoundBase> rv = new List<SoundBase>();

                int tindex = 0;
                foreach (GBAPUTimbre timbre in parentModule.GetBaseTimbres(note))
                {
                    tindex++;
                    var emptySlot = searchEmptySlot(note, timbre);
                    if (emptySlot.slot < 0)
                        continue;

                    GbSound snd = new GbSound(emptySlot.inst, this, timbre, tindex - 1, note, emptySlot.slot);
                    switch (timbre.SoundType)
                    {
                        case SoundType.SPSG:
                            spsgOnSounds.Add(snd);
                            break;
                        case SoundType.PSG:
                            psgOnSounds.Add(snd);
                            break;
                        case SoundType.WAV:
                            wavOnSounds.Add(snd);
                            break;
                        case SoundType.NOISE:
                            noiseOnSounds.Add(snd);
                            break;
                    }

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
            private (GB_APU inst, int slot) searchEmptySlot(TaggedNoteOnEvent note, GBAPUTimbre timbre)
            {
                var emptySlot = (parentModule, -1);

                switch (timbre.SoundType)
                {
                    case SoundType.SPSG:
                        {
                            emptySlot = SearchEmptySlotAndOffForLeader(parentModule, spsgOnSounds, note, 1);
                            break;
                        }
                    case SoundType.PSG:
                        {
                            if (timbre.PartialReserveSPSG)
                                emptySlot = SearchEmptySlotAndOffForLeader(parentModule, psgOnSounds, note, 1);
                            else
                                emptySlot = SearchEmptySlotAndOffForLeader(parentModule, psgOnSounds, note, 2);
                            break;
                        }
                    case SoundType.WAV:
                        {
                            emptySlot = SearchEmptySlotAndOffForLeader(parentModule, wavOnSounds, note, 1);
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

            internal override void ProcessAllSoundOff()
            {
                var me = new ControlChangeEvent((SevenBitNumber)120, (SevenBitNumber)0);
                ProcessControlChange(me);

                for (int i = 0; i < 4; i++)
                {
                    uint reg = (uint)(i * 5);
                    GbApuWriteData(parentModule.UnitNumber, reg + 2, 0x00);
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private class GbSound : SoundBase
        {

            private GB_APU parentModule;

            private GBAPUTimbre timbre;

            private SoundType lastSoundType;

            private int partialReserveSpsg;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentModule"></param>
            /// <param name="noteOnEvent"></param>
            /// <param name="programNumber"></param>
            /// <param name="slot"></param>
            public GbSound(GB_APU parentModule, GBSoundManager manager, TimbreBase timbre, int tindex, TaggedNoteOnEvent noteOnEvent, int slot) : base(parentModule, manager, timbre, tindex, noteOnEvent, slot)
            {
                this.parentModule = parentModule;
                this.timbre = (GBAPUTimbre)timbre;

                lastSoundType = this.timbre.SoundType;
                if (lastSoundType == SoundType.PSG && this.timbre.PartialReserveSPSG)
                    partialReserveSpsg = 1;
            }

            /// <summary>
            /// 
            /// </summary>
            public override void KeyOn()
            {
                base.KeyOn();

                switch (lastSoundType)
                {
                    case SoundType.SPSG:
                    case SoundType.PSG:
                        {
                            uint reg = (uint)((Slot + partialReserveSpsg) * 5);

                            if (lastSoundType == SoundType.SPSG)
                                GbApuWriteData(parentModule.UnitNumber, reg, (byte)(timbre.SPSGSweep.Time << 4 | timbre.SPSGSweep.Dir << 3 | timbre.SPSGSweep.Speed));
                            else
                                GbApuWriteData(parentModule.UnitNumber, reg, 0x00);


                            OnVolumeUpdated();

                            OnPanpotUpdated();

                            UpdatePitch(0x80);

                            if (lastSoundType == SoundType.SPSG)
                                FormMain.OutputDebugLog(parentModule, "KeyOn SPSG ch" + Slot + " " + NoteOnEvent.ToString());
                            else
                                FormMain.OutputDebugLog(parentModule, "KeyOn PSG ch" + (Slot + partialReserveSpsg) + " " + NoteOnEvent.ToString());
                            break;
                        }
                    case SoundType.WAV:
                        {
                            uint reg = (uint)((Slot + 2) * 5);

                            GbApuWriteData(parentModule.UnitNumber, reg, 0x00);
                            GbApuWriteData(parentModule.UnitNumber, reg + 1, timbre.SoundLength);

                            OnVolumeUpdated();

                            OnPanpotUpdated();

                            //Wave
                            for (int i = 0; i < 16; i++)
                                GbApuWaveWriteData(parentModule.UnitNumber, (uint)i, (byte)(((timbre.WsgData[i * 2] & 0xf) << 4) | (timbre.WsgData[(i * 2) + 1] & 0xf)));

                            GbApuWriteData(parentModule.UnitNumber, reg, 0x80);

                            UpdatePitch(0x80);

                            FormMain.OutputDebugLog(parentModule, "KeyOn WAV ch" + Slot + " " + NoteOnEvent.ToString());
                            break;
                        }
                    case SoundType.NOISE:
                        {
                            uint reg = (uint)((Slot + 3) * 5);

                            GbApuWriteData(parentModule.UnitNumber, reg + 1, timbre.SoundLength);

                            OnVolumeUpdated();

                            OnPanpotUpdated();

                            UpdatePitch(0x80);

                            FormMain.OutputDebugLog(parentModule, "KeyOn NOISE ch" + Slot + " " + NoteOnEvent.ToString());
                            break;
                        }
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public override void OnVolumeUpdated()
            {
                switch (lastSoundType)
                {
                    case SoundType.SPSG:
                    case SoundType.PSG:
                        {
                            uint reg = (uint)((Slot + partialReserveSpsg) * 5);

                            byte dt = timbre.Duty;
                            if (FxEngine != null && FxEngine.Active)
                            {
                                var eng = (GbFxEngine)FxEngine;
                                if (eng.DutyValue != null)
                                    dt = eng.DutyValue.Value;
                            }

                            GbApuWriteData(parentModule.UnitNumber, reg + 1, (byte)(dt << 6 | timbre.SoundLength));

                            byte tl = (byte)Math.Round(timbre.EnvInitialVolume * CalcCurrentVolume());

                            byte edir = (byte)(timbre.EnvDirection << 3);
                            byte elen = timbre.EnvLength;

                            GbApuWriteData(parentModule.UnitNumber, reg + 2, (byte)((tl << 4) | edir | elen));
                            break;
                        }
                    case SoundType.WAV:
                        {
                            uint reg = (uint)((Slot + 2) * 5);
                            byte tl = (byte)(Math.Round(3d * CalcCurrentVolume()));
                            switch (tl)
                            {
                                case 3:
                                    tl = 1;
                                    break;
                                case 2:
                                    break;
                                case 1:
                                    tl = 3;
                                    break;
                            }

                            GbApuWriteData(parentModule.UnitNumber, reg + 2, (byte)(tl << 5));
                            break;
                        }
                    case SoundType.NOISE:
                        {
                            uint reg = (uint)((Slot + 3) * 5);
                            byte tl = (byte)Math.Round(timbre.EnvInitialVolume * CalcCurrentVolume());

                            byte edir = (byte)(timbre.EnvDirection << 3);
                            byte elen = timbre.EnvLength;

                            GbApuWriteData(parentModule.UnitNumber, reg + 2, (byte)((tl << 4) | edir | elen));
                            break;
                        }
                }

            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="slot"></param>
            public override void OnPitchUpdated()
            {
                UpdatePitch(0x00);

                base.OnPitchUpdated();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="slot"></param>
            public void UpdatePitch(byte keyOn)
            {
                double freq = CalcCurrentFrequency();

                //Freq
                switch (lastSoundType)
                {
                    case SoundType.SPSG:
                    case SoundType.PSG:
                        {
                            uint reg = (uint)((Slot + partialReserveSpsg) * 5);
                            ushort gfreq = convertPsgFrequency(freq);

                            GbApuWriteData(parentModule.UnitNumber, reg + 3, (byte)(gfreq & 0xff));
                            GbApuWriteData(parentModule.UnitNumber, reg + 4, (byte)(keyOn | (timbre.EnableLength << 6) | ((gfreq >> 8) & 0x07)));
                            break;
                        }
                    case SoundType.WAV:
                        {
                            uint reg = (uint)((Slot + 2) * 5);
                            ushort gfreq = convertWavFrequency(freq);

                            GbApuWriteData(parentModule.UnitNumber, reg + 3, (byte)(gfreq & 0xff));
                            GbApuWriteData(parentModule.UnitNumber, reg + 4, (byte)(keyOn | (timbre.EnableLength << 6) | ((gfreq >> 8) & 0x07)));
                            break;
                        }
                    case SoundType.NOISE:
                        {
                            uint reg = (uint)((Slot + 3) * 5);

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

                            int v = 15 - (noteNum % 16);

                            byte dt = timbre.NoiseCounter;
                            if (FxEngine != null && FxEngine.Active)
                            {
                                var eng = (GbFxEngine)FxEngine;
                                dt = (byte)(eng.DutyValue & 1);
                            }

                            GbApuWriteData(parentModule.UnitNumber, reg + 3, (byte)(v << 4 | dt << 3 | timbre.NoiseDivRatio));
                            GbApuWriteData(parentModule.UnitNumber, reg + 4, (byte)(keyOn | (timbre.EnableLength << 6)));
                            break;
                        }
                }

            }

            /// <summary>
            /// 
            /// </summary>
            public override void OnPanpotUpdated()
            {
                //Pan
                byte? cpan = GbApuReadData(parentModule.UnitNumber, 0x15);
                if (cpan.HasValue)
                {
                    var rslot = Slot + partialReserveSpsg;

                    switch (lastSoundType)
                    {
                        case SoundType.SPSG:
                        case SoundType.PSG:
                            break;
                        case SoundType.WAV:
                            rslot += 2;
                            break;
                        case SoundType.NOISE:
                            rslot += 3;
                            break;
                    }

                    byte mask = (byte)(0x11 << rslot);
                    byte ccpan = (byte)(cpan.Value & (byte)~mask);

                    byte pan = CalcCurrentPanpot();
                    if (pan < 32)
                        pan = 0x10;
                    else if (pan > 96)
                        pan = 0x01;
                    else
                        pan = 0x11;
                    pan = (byte)(pan << rslot);
                    ccpan |= pan;

                    GbApuWriteData(parentModule.UnitNumber, 0x15, ccpan);
                }
            }


            /// <summary>
            /// 
            /// </summary>
            public override void SoundOff()
            {
                base.SoundOff();

                switch (lastSoundType)
                {
                    case SoundType.SPSG:
                    case SoundType.PSG:
                        {
                            uint reg = (uint)((Slot + partialReserveSpsg) * 5);

                            GbApuWriteData(parentModule.UnitNumber, reg + 2, 0x00);

                            break;
                        }
                    case SoundType.WAV:
                        {
                            uint reg = (uint)((Slot + 2) * 5);

                            GbApuWriteData(parentModule.UnitNumber, reg + 2, 0x00);

                            break;
                        }
                    case SoundType.NOISE:
                        {
                            uint reg = (uint)((Slot + 3) * 5);

                            GbApuWriteData(parentModule.UnitNumber, reg + 2, 0x00);

                            break;
                        }
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="note"></param>
            /// <returns></returns>
            private ushort convertPsgFrequency(double freq)
            {
                /*
                 * FF14 - NR14 - Channel 1 Frequency hi (R/W)
                 * Bit 7   - Initial (1=Restart Sound)     (Write Only)
                 * Bit 6   - Counter/consecutive selection (Read/Write)
                 * (1=Stop output when length in NR11 expires)
                 * Bit 2-0 - Frequency's higher 3 bits (x) (Write Only)
                 * Frequency = 131072/(2048-x) Hz
                 */
                double f = (131072d / freq);
                if (f > 2048d)
                    f = 2048d;
                return (ushort)Math.Round(2048d - f);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="note"></param>
            /// <returns></returns>
            private ushort convertWavFrequency(double freq)
            {
                double f = (65536d / freq);
                if (f > 2048d)
                    f = 2048d;
                return (ushort)Math.Round(2048d - (65536d / freq));
            }

        }

        /// <summary>
        /// 
        /// </summary>
        [JsonConverter(typeof(NoTypeConverterJsonConverter<GBAPUTimbre>))]
        [DataContract]
        [InstLock]
        public class GBAPUTimbre : TimbreBase
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

            [DataMember]
            [Category("Sound")]
            [Description("Sound Type (SPSG:Sweep PSG:PSG(2ch) WAV:WAV NOISE:NOISE)")]
            [DefaultValue(SoundType.SPSG)]
            public SoundType SoundType
            {
                get;
                set;
            }

            private bool f_PartialReserveSPSG;

            [DataMember]
            [Category("Sound")]
            [Description("SPSG partial reserve against with PSG.\r\n" +
                "Sweep PSG shared 1ch with PSG." +
                "So, you can choose whether to give priority to SPSG over PSG")]
            [DefaultValue(false)]
            public bool PartialReserveSPSG
            {
                get
                {
                    return f_PartialReserveSPSG;
                }
                set
                {
                    f_PartialReserveSPSG = value;
                }
            }

            private byte f_Duty = 2;

            [DataMember]
            [Category("Sound")]
            [Description("Duty (0:12.5% 1:25% 2:50% 3:75%)")]
            [DefaultValue((byte)2)]
            [SlideParametersAttribute(0, 3)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte Duty
            {
                get
                {
                    return f_Duty;
                }
                set
                {
                    f_Duty = (byte)(value & 3);
                }
            }

            private byte f_SoundLength;

            [Browsable(false)]
            [DataMember]
            [Category("Sound")]
            [Description("Sound Length (0-64,0-255)[(64-N)*(1/256) seconds]")]
            [SlideParametersAttribute(0, 255)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte SoundLength
            {
                get
                {
                    if (SoundType == SoundType.WAV)
                        return f_SoundLength;
                    else
                        return (byte)(f_SoundLength & 63);
                }
                set
                {
                    if (SoundType == SoundType.WAV)
                        f_SoundLength = (byte)(value & 255);
                    else
                        f_SoundLength = (byte)(value & 63);
                }
            }


            private byte f_EnableLength;

            [Browsable(false)]
            [DataMember]
            [Category("Sound")]
            [Description("Whether Sound Length is enable or not (0:Disable 1:Enable)")]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte EnableLength
            {
                get
                {
                    return f_EnableLength;
                }
                set
                {
                    f_EnableLength = (byte)(value & 1);
                }
            }

            private byte f_EnvInitialVolume = 15;

            [DataMember]
            [Category("Sound")]
            [Description("Initial Volume of envelope (0-15(0:No Sound))")]
            [DefaultValue((byte)15)]
            [SlideParametersAttribute(0, 15)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte EnvInitialVolume
            {
                get
                {
                    return f_EnvInitialVolume;
                }
                set
                {
                    f_EnvInitialVolume = (byte)(value & 0xf);
                }
            }

            private byte f_EnvDirection = 1;

            [DataMember]
            [Category("Sound")]
            [Description("Envelope Direction (0=Decrease, 1=Increase)")]
            [DefaultValue((byte)1)]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte EnvDirection
            {
                get
                {
                    return f_EnvDirection;
                }
                set
                {
                    f_EnvDirection = (byte)(value & 1);
                }
            }

            private byte f_EnvLength = 7;

            [DataMember]
            [Category("Sound")]
            [Description("Envelope Length (0-7 (0:Stop)[1step = N*(1/64) sec]")]
            [DefaultValue((byte)7)]
            [SlideParametersAttribute(0, 7)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte EnvLength
            {
                get
                {
                    return f_EnvLength;
                }
                set
                {
                    f_EnvLength = (byte)(value & 7);
                }
            }

            private byte f_NoiseShiftClockFrequency;

            [Browsable(false)]
            [DataMember]
            [Category("Sound(Noise)")]
            [Description("Shift Clock Frequency (0-15) This parameter is affected by Pitch Bend MIDI message")]
            [SlideParametersAttribute(0, 15)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte NoiseShiftClockFrequency
            {
                get
                {
                    return f_NoiseShiftClockFrequency;
                }
                set
                {
                    f_NoiseShiftClockFrequency = (byte)(value & 15);
                }
            }

            private byte f_NoiseCounter;

            [DataMember]
            [Category("Sound(Noise)")]
            [Description("Counter Step/Width (0=15 bits, 1=7 bits)")]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue((byte)0)]
            public byte NoiseCounter
            {
                get
                {
                    return f_NoiseCounter;
                }
                set
                {
                    f_NoiseCounter = (byte)(value & 1);
                }
            }

            private byte f_NoiseDivRatio;

            [Browsable(false)]
            [DataMember]
            [Category("Sound(Noise)")]
            [Description("Dividing Ratio of Frequencies(0-7)")]
            [SlideParametersAttribute(0, 7)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte NoiseDivRatio
            {
                get
                {
                    return f_NoiseDivRatio;
                }
                set
                {
                    f_NoiseDivRatio = (byte)(value & 7);
                }
            }

            private byte[] f_wavedata = new byte[32];

            [TypeConverter(typeof(ArrayConverter))]
            [Editor(typeof(WsgUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [WsgBitWideAttribute(4)]
            [DataMember]
            [Category("Sound")]
            [Description("Wave Table (32 samples, 0-15 levels)")]
            public byte[] WsgData
            {
                get
                {
                    return f_wavedata;
                }
                set
                {
                    f_wavedata = value;
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
                f_wavedata = new byte[32];
            }

            [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
            typeof(UITypeEditor)), Localizable(false)]
            [Category("Sound")]
            [Description("Wave Table (32 samples, 0-15 levels)")]
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
                        WsgData[i] = vs[i] > 15 ? (byte)15 : vs[i];
                }
            }

            /// <summary>
            /// 
            /// </summary>
            [DataMember]
            [Category("Sound(SQ)")]
            [Description("SPSG Sweep Settings")]
            public SPSGSweepSettings SPSGSweep
            {
                get;
                set;
            }


            public GBAPUTimbre()
            {
                SDS.FxS = new GbFxSettings();
                SPSGSweep = new SPSGSweepSettings();
            }


            /// <summary>
            /// 
            /// </summary>
            /// <param name="serializeData"></param>
            public override void RestoreFrom(string serializeData)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<GBAPUTimbre>(serializeData);
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


        [JsonConverter(typeof(NoTypeConverterJsonConverter<SPSGSweepSettings>))]
        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [DataContract]
        [InstLock]
        public class SPSGSweepSettings : ContextBoundObject
        {

            private byte f_SweepTime;

            [DataMember]
            [Category("Sound(Sweep)")]
            [Description("SPSG Sweep Time (0:OFF 1-7:N/128Hz)")]
            [SlideParametersAttribute(0, 7)]
            [DefaultValue((byte)0)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte Time
            {
                get
                {
                    return f_SweepTime;
                }
                set
                {
                    f_SweepTime = (byte)(value & 7);
                }
            }

            private byte f_SweepDir;

            [DataMember]
            [Category("Sound(Sweep)")]
            [Description("SPSG Sweep Increase/Decrease (0: Addition 1: Subtraction)")]
            [SlideParametersAttribute(0, 1)]
            [DefaultValue((byte)0)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte Dir
            {
                get
                {
                    return f_SweepDir;
                }
                set
                {
                    f_SweepDir = (byte)(value & 1);
                }
            }

            private byte f_SweepNumber;

            [DataMember]
            [Category("Sound(Sweep)")]
            [Description("SPSG Number of sweep shift (0-7)")]
            [SlideParametersAttribute(0, 7)]
            [DefaultValue((byte)0)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte Speed
            {
                get
                {
                    return f_SweepNumber;
                }
                set
                {
                    f_SweepNumber = (byte)(value & 7);
                }
            }
        }

        [JsonConverter(typeof(NoTypeConverterJsonConverter<BasicFxSettings>))]
        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [DataContract]
        [InstLock]
        public class GbFxSettings : BasicFxSettings
        {

            private string f_DutyEnvelopes;

            [DataMember]
            [Description("Set duty/noise envelop by text. Input duty/noise value and split it with space like the FamiTracker.\r\n" +
                       "0 ～ 3 \"|\" is repeat point. \"/\" is release point.")]
            [Editor(typeof(EnvelopeUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [EnvelopeEditorAttribute(0, 3)]
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
                                    else if (v > 3)
                                        v = 3;
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
                            if (i < DutyEnvelopesNums.Length)
                                sb.Append(DutyEnvelopesNums[i].ToString((IFormatProvider)null));
                        }
                        f_DutyEnvelopes = sb.ToString();
                    }
                }
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
                return new GbFxEngine(this);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public class GbFxEngine : BasicFxEngine
        {
            private GbFxSettings settings;

            /// <summary>
            /// 
            /// </summary>
            public GbFxEngine(GbFxSettings settings) : base(settings)
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
                        //if (f_dutyCounter < settings.DutyEnvelopesNums.Length)
                        {
                            if (settings.DutyEnvelopesReleasePoint >= 0 && f_dutyCounter <= (uint)settings.DutyEnvelopesReleasePoint)
                                f_dutyCounter = (uint)settings.DutyEnvelopesReleasePoint;
                            //else if (settings.DutyEnvelopesReleasePoint < 0)
                                //f_dutyCounter = (uint)settings.DutyEnvelopesNums.Length;
                        }
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

        /// <summary>
        /// 
        /// </summary>
        public enum SoundType
        {
            SPSG,
            PSG,
            WAV,
            NOISE,
        }

    }
}
