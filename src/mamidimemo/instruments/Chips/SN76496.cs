// copyright-holders:K.Ito
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;
using System.Linq;
using System.Net;
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
using zanac.MAmidiMEmo.Properties;
using zanac.MAmidiMEmo.Scci;
using zanac.MAmidiMEmo.Util;
using zanac.MAmidiMEmo.VSIF;
using static zanac.MAmidiMEmo.Instruments.Chips.YM2413;

//http://www.smspower.org/Development/SN76489
//http://www.st.rim.or.jp/~nkomatsu/peripheral/SN76489.html

namespace zanac.MAmidiMEmo.Instruments.Chips
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    [InstLock]
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

        private PortId portId = PortId.No1;

        [DataMember]
        [Category("Chip(Dedicated)")]
        [Description("Set FTDI or COM Port No for \"VSIF - Genesis/SMS\".\r\n" +
            "See the manual about the VSIF.")]
        [DefaultValue(PortId.No1)]
        public PortId PortId
        {
            get
            {
                return portId;
            }
            set
            {
                if (portId != value)
                {
                    portId = value;
                    setSoundEngine(SoundEngine);
                }
            }
        }

        private object sndEnginePtrLock = new object();

        private VsifClient vsifClient;

        private SoundEngineType f_SoundEngineType;

        private SoundEngineType f_CurrentSoundEngineType;

        [DataMember]
        [Category("Chip(Dedicated)")]
        [Description("Select a sound engine type.\r\n" +
            "Supports \"Software\" and \"VSIF - SMS\" or \"VSIF - Genesis\"")]
        [DefaultValue(SoundEngineType.Software)]
        [TypeConverter(typeof(EnumConverterSoundEngineTypeSN76496))]
        public SoundEngineType SoundEngine
        {
            get
            {
                return f_SoundEngineType;
            }
            set
            {
                if (f_SoundEngineType != value)
                {
                    setSoundEngine(value);
                }
            }
        }

        [Category("Chip(Dedicated)")]
        [Description("Current sound engine type.")]
        [DefaultValue(SoundEngineType.Software)]
        [RefreshProperties(RefreshProperties.All)]
        public SoundEngineType CurrentSoundEngine
        {
            get
            {
                return f_CurrentSoundEngineType;
            }
        }

        private void setSoundEngine(SoundEngineType value)
        {
            AllSoundOff();

            lock (sndEnginePtrLock)
            {
                if (vsifClient != null)
                {
                    vsifClient.Dispose();
                    vsifClient = null;
                }

                f_SoundEngineType = value;

                switch (f_SoundEngineType)
                {
                    case SoundEngineType.Software:
                        f_CurrentSoundEngineType = f_SoundEngineType;
                        SetDevicePassThru(false);
                        break;
                    case SoundEngineType.VSIF_SMS:
                        vsifClient = VsifManager.TryToConnectVSIF(VsifSoundModuleType.SMS, PortId, false);
                        if (vsifClient != null)
                        {
                            f_CurrentSoundEngineType = f_SoundEngineType;
                            SetDevicePassThru(true);
                        }
                        else
                        {
                            f_CurrentSoundEngineType = SoundEngineType.Software;
                            SetDevicePassThru(false);
                        }
                        break;
                    case SoundEngineType.VSIF_Genesis:
                        vsifClient = VsifManager.TryToConnectVSIF(VsifSoundModuleType.Genesis, PortId, false);
                        if (vsifClient != null)
                        {
                            f_CurrentSoundEngineType = f_SoundEngineType;
                            SetDevicePassThru(true);
                        }
                        else
                        {
                            f_CurrentSoundEngineType = SoundEngineType.Software;
                            SetDevicePassThru(false);
                        }
                        break;
                    case SoundEngineType.VSIF_Genesis_Low:
                        vsifClient = VsifManager.TryToConnectVSIF(VsifSoundModuleType.Genesis_Low, PortId, false);
                        if (vsifClient != null)
                        {
                            f_CurrentSoundEngineType = f_SoundEngineType;
                            SetDevicePassThru(true);
                        }
                        else
                        {
                            f_CurrentSoundEngineType = SoundEngineType.Software;
                            SetDevicePassThru(false);
                        }
                        break;
                    case SoundEngineType.VSIF_Genesis_FTDI:
                        vsifClient = VsifManager.TryToConnectVSIF(VsifSoundModuleType.Genesis_FTDI, PortId, false);
                        if (vsifClient != null)
                        {
                            f_CurrentSoundEngineType = f_SoundEngineType;
                            SetDevicePassThru(true);
                        }
                        else
                        {
                            f_CurrentSoundEngineType = SoundEngineType.Software;
                            SetDevicePassThru(false);
                        }
                        break;
                    case SoundEngineType.VSIF_MSX_FTDI:
                        vsifClient = VsifManager.TryToConnectVSIF(VsifSoundModuleType.MSX_FTDI, PortId, false);
                        if (vsifClient != null)
                        {
                            f_CurrentSoundEngineType = f_SoundEngineType;
                            SetDevicePassThru(true);
                        }
                        else
                        {
                            f_CurrentSoundEngineType = SoundEngineType.Software;
                            SetDevicePassThru(false);
                        }
                        break;
                }
            }
            PrepareSound();
        }

        private int f_ftdiClkWidth = 9;

        [DataMember]
        [Category("Chip(Dedicated)")]
        [SlideParametersAttribute(1, 100)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue(9)]
        [Description("Set FTDI Clock Width[%].")]
        public int FtdiClkWidth
        {
            get
            {
                return f_ftdiClkWidth;
            }
            set
            {
                f_ftdiClkWidth = value;
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
        /// <param name="address"></param>
        /// <param name="data"></param>
        internal override void DirectAccessToChip(uint address, uint data)
        {
            Sn76496WriteData(UnitNumber, (byte)data);
        }

        /// <summary>
        /// 
        /// </summary>
        private void Sn76496WriteData(uint unitNumber, byte data)
        {
            Sn76496WriteData(unitNumber, data, true);
        }

        /// <summary>
        /// 
        /// </summary>
        private void Sn76496WriteData(uint unitNumber, byte data, bool useCache)
        {
            WriteData(0, data, useCache, new Action(() =>
            {
                lock (sndEnginePtrLock)
                {
                    switch (CurrentSoundEngine)
                    {
                        case SoundEngineType.VSIF_SMS:
                            vsifClient.WriteData(0, 0xff, data, f_ftdiClkWidth);
                            break;
                        case SoundEngineType.VSIF_Genesis:
                        case SoundEngineType.VSIF_Genesis_Low:
                        case SoundEngineType.VSIF_Genesis_FTDI:
                            vsifClient.WriteData(0, 0x04 * 5, data, f_ftdiClkWidth);
                            break;
                        case SoundEngineType.VSIF_MSX_FTDI:
                            vsifClient.WriteData(0xc, 0, data, f_ftdiClkWidth);
                            break;
                    }
                }

                DeferredWriteData(Sn76496_write, unitNumber, data);
            }));

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

            Timbres = new SN76496Timbre[InstrumentBase.DEFAULT_MAX_TIMBRES];
            for (int i = 0; i < InstrumentBase.DEFAULT_MAX_TIMBRES; i++)
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

            lock (sndEnginePtrLock)
                if (vsifClient != null)
                    vsifClient.Dispose();

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
            ClearWrittenDataCache();
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

                int tindex = 0;
                foreach (SN76496Timbre timbre in parentModule.GetBaseTimbres(note))
                {
                    tindex++;
                    var emptySlot = searchEmptySlot(note, timbre);
                    if (emptySlot.slot < 0)
                        continue;

                    SN76496Sound snd = new SN76496Sound(emptySlot.inst, this, timbre, tindex - 1, note, emptySlot.slot);
                    switch (((SN76496Timbre)timbre).SoundType)
                    {
                        case SoundType.PSG:
                            psgOnSounds.Add(snd);
                            FormMain.OutputDebugLog(parentModule, "KeyOn PSG ch" + emptySlot + " " + note.ToString());
                            break;
                        case SoundType.NOISE:
                            noiseOnSounds.Add(snd);
                            FormMain.OutputDebugLog(parentModule, "KeyOn NOISE ch" + emptySlot + " " + note.ToString());
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

            internal override void ProcessAllSoundOff()
            {
                var me = new ControlChangeEvent((SevenBitNumber)120, (SevenBitNumber)0);
                ProcessControlChange(me);

                for (int i = 0; i < 3; i++)
                    parentModule.Sn76496WriteData(parentModule.UnitNumber, (byte)(0x80 | i << 5 | 0x1f));
                parentModule.Sn76496WriteData(parentModule.UnitNumber, (byte)(0x80 | 3 << 5 | 0x1f));
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
            public SN76496Sound(SN76496 parentModule, SN76496SoundManager manager, TimbreBase timbre, int tindex, TaggedNoteOnEvent noteOnEvent, int slot) : base(parentModule, manager, timbre, tindex, noteOnEvent, slot)
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

                OnPitchUpdated();

                OnVolumeUpdated();
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
                byte fv = (byte)((14 - (int)Math.Round(14 * CalcCurrentVolume())) & 0xf);

                parentModule.Sn76496WriteData(parentModule.UnitNumber, (byte)(0x80 | Slot << 5 | 0x10 | fv));
            }

            /// <summary>
            /// 
            /// </summary>
            private void updateNoiseVolume()
            {
                byte fv = (byte)((14 - (int)Math.Round(14 * CalcCurrentVolume())) & 0xf);

                //var exp = parentModule.Expressions[NoteOnEvent.Channel] / 127d;
                //var vol = parentModule.Volumes[NoteOnEvent.Channel] / 127d;
                //var vel = NoteOnEvent.Velocity / 127d;

                //byte fv = (byte)((14 - (int)Math.Round(14 * vol * vel * exp)) & 0xf);

                parentModule.Sn76496WriteData(parentModule.UnitNumber, (byte)(0x80 | (Slot + 3) << 5 | 0x10 | fv));
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
                parentModule.Sn76496WriteData(parentModule.UnitNumber, (byte)(0x80 | Slot << 5 | n & 0xf));
                parentModule.Sn76496WriteData(parentModule.UnitNumber, (byte)((n >> 4) & 0x3f));
            }


            /// <summary>
            /// 
            /// </summary>
            /// <param name="slot"></param>
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

                int v = noteNum % 4;

                parentModule.Sn76496WriteData(parentModule.UnitNumber, (byte)(0x80 | (Slot + 3) << 5 | timbre.FB << 2 | v));
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
                            parentModule.Sn76496WriteData(parentModule.UnitNumber, (byte)(0x80 | Slot << 5 | 0x1f));
                            break;
                        }
                    case SoundType.NOISE:
                        {
                            parentModule.Sn76496WriteData(parentModule.UnitNumber, (byte)(0x80 | (Slot + 3) << 5 | 0x1f));
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
        [InstLock]
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

        private class EnumConverterSoundEngineTypeSN76496 : EnumConverter<SoundEngineType>
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                var sc = new StandardValuesCollection(new SoundEngineType[] {
                    SoundEngineType.Software,
                    SoundEngineType.VSIF_SMS,
                    SoundEngineType.VSIF_Genesis,
                    SoundEngineType.VSIF_Genesis_Low,
                    SoundEngineType.VSIF_Genesis_FTDI,
                    SoundEngineType.VSIF_MSX_FTDI,
               });

                return sc;
            }
        }

    }

}