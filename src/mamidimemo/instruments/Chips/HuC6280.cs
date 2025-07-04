﻿// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
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
using zanac.MAmidiMEmo.Util;
using zanac.MAmidiMEmo.VSIF;
using static zanac.MAmidiMEmo.Instruments.Chips.CM32P;
using static zanac.MAmidiMEmo.Instruments.Chips.RP2A03;

//http://www.magicengine.com/mkit/doc_hard_psg.html

namespace zanac.MAmidiMEmo.Instruments.Chips
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    [InstLock]
    public class HuC6280 : InstrumentBase
    {
        private const int MAX_DAC_VOICES = 6;

        public override string Name => "HuC6280";

        public override string Group => "WSG";

        public override InstrumentType InstrumentType => InstrumentType.HuC6280;

        [Browsable(false)]
        public override string ImageKey => "HuC6280";

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        protected override string SoundInterfaceTagNamePrefix => "c6280_";

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
                return 16;
            }
        }


        private PortId portId = PortId.No1;

        [DataMember]
        [Category("Chip(Dedicated)")]
        [Description("Set COM Port No for \"VSIF - PCE(Turbo Everdrive)\".\r\n" +
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
            "Supports \"Software\" and \"VSIF - NES\"")]
        [DefaultValue(SoundEngineType.Software)]
        [TypeConverter(typeof(EnumConverterSoundEngineTypeRP2A03))]
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
                    case SoundEngineType.VSIF_PCE_TurboEverDrive:
                        vsifClient = VsifManager.TryToConnectVSIF(VsifSoundModuleType.TurboEverDrive, PortId, false);
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
            ClearWrittenDataCache();
            PrepareSound();
        }

        private bool f_EnableSmartDacClip;

        [DataMember]
        [Category("Chip(Dedicated)")]
        [Description("Enabled Smart DAC Clip.")]
        [DefaultValue(false)]
        [Browsable(false)]
        public bool EnableSmartDacClip
        {
            get
            {
                return f_EnableSmartDacClip;
            }
            set
            {
                if (f_EnableSmartDacClip != value)
                {
                    f_EnableSmartDacClip = value;
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
                Timbres = (HuC6280Timbre[])value;
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
        public HuC6280Timbre[] Timbres
        {
            get;
            set;
        }


        private PcmData[] pcmTable;

        private class PcmData
        {
            private double currentPos;

            private byte[] pcmData;

            public double SampleRate
            {
                get;
                set;
            } = 6991.3;

            public byte? PeekDacData()
            {
                uint idx = (uint)Math.Round(currentPos);
                if (idx >= pcmData.Length)
                {
                    if (LoopEnabled && LoopPoint < pcmData.Length)
                    {
                        idx = LoopPoint;
                    }
                    else
                    {
                        return null;
                    }
                }
                return pcmData[idx];
            }

            public byte GetNextData(double sampleRate)
            {
                uint idx = (uint)Math.Round(currentPos);
                if (idx >= pcmData.Length)
                {
                    if (LoopEnabled && LoopPoint < pcmData.Length)
                    {
                        currentPos = LoopPoint;
                        idx = LoopPoint;
                    }
                    else
                    {
                        return 0;
                    }
                }

                if (idx >= pcmData.Length - 1)
                {
                    if (PitchEnabled)
                        currentPos += (Pitch * (double)sampleRate) / (double)SampleRate;
                    else
                        currentPos++;
                    return pcmData[idx];
                }

                var ret = (lerp(pcmData[idx], pcmData[idx + 1], currentPos - idx)) + 128;
                if (PitchEnabled)
                    currentPos += (Pitch * (double)sampleRate) / (double)SampleRate;
                else
                    currentPos++;
                return (byte)Math.Round(ret);
            }

            static double lerp(double y0, double y1, double x)
            {
                y0 -= 128;
                y1 -= 128;
                return y0 + (y1 - y0) * x;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="pcmData"></param>
            public PcmData(byte[] pcmData, TaggedNoteOnEvent note, bool disableVelocity)
            {
                this.pcmData = new byte[pcmData.Length];
                Array.Copy(pcmData, this.pcmData, pcmData.Length);
                Note = note;
                DisableVelocity = disableVelocity;
            }

            public TaggedNoteOnEvent Note
            {
                get;
                private set;
            }

            public bool DisableVelocity
            {
                get;
                private set;
            }

            public float Gain
            {
                get;
                set;
            }

            public double Volume
            {
                get;
                set;
            }

            public double BaseFreq
            {
                get;
                set;
            }

            public double Pitch
            {
                get;
                set;
            }

            public bool PitchEnabled
            {
                get;
                set;
            }

            public bool LoopEnabled
            {
                get;
                set;
            }

            public uint LoopPoint
            {
                get;
                set;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serializeData"></param>
        public override void RestoreFrom(string serializeData)
        {
            try
            {
                using (var obj = JsonConvert.DeserializeObject<HuC6280>(serializeData))
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
        /// <param name="address"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delegate_c6280_w(uint unitNumber, uint address, byte data);

        /// <summary>
        /// 
        /// </summary>
        private void C6280WriteData(uint unitNumber, uint address, byte data)
        {
            C6280WriteData(unitNumber, address, data, true, true);
        }

        /// <summary>
        /// 
        /// </summary>
        private void C6280WriteData(uint unitNumber, uint address,  byte data, bool sendVsif, bool sendEmu)
        {
            bool useCache = false;
            switch (address)
            {
                case 0x800:
                case 0x801:
                case 0x808:
                case 0x809:
                    useCache = true;
                    break;
            }
            WriteData(address, data, useCache, new Action(() =>
            {
                if (sendVsif)
                {
                    lock (sndEnginePtrLock)
                    {
                        switch (CurrentSoundEngine)
                        {
                            case SoundEngineType.VSIF_PCE_TurboEverDrive:
                                lock (sndEnginePtrLock)
                                    vsifClient.WriteData(0, (byte)(address - 0x800), (byte)data, 0);
                                break;
                        }
                    }
                }
                if (sendEmu)
                    DeferredWriteData(C6280_w, unitNumber, address, data);
            }));
            /*
            try
            {
                Program.SoundUpdating();
                if (slot != null)
                    C6280_w(unitNumber, 0x800, (byte)slot.Value);
                C6280_w(unitNumber, address, data);
            }
            finally
            {
                Program.SoundUpdated();
            }*/
        }

        /// <summary>
        /// 
        /// </summary>
        private static delegate_c6280_w C6280_w
        {
            get;
            set;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delg_callback();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delegate_set_callback(uint unitNumber, delg_callback callback);

        /// <summary>
        /// 
        /// </summary>
        private static void C6280SetCallback(uint unitNumber, delg_callback callback)
        {
            set_callback(unitNumber, callback);
        }

        /// <summary>
        /// 
        /// </summary>
        private void pcm_callback()
        {
            double sampleRate = 6991.3;

            PcmData[] lpcmTable = null;
            lock (pcmTable)
            {
                lpcmTable = pcmTable;

                switch (CurrentSoundEngine)
                {
                    case SoundEngineType.VSIF_PCE_TurboEverDrive:
                        break;
                    case SoundEngineType.Software:
                        for (int i = 0; i < lpcmTable.Length; i++)
                        {
                            PcmData pcmData = lpcmTable[i];
                            if (pcmData != null)
                            {
                                if (pcmData.PeekDacData() == null)
                                {
                                    lpcmTable[i] = null;
                                    continue;
                                }

                                C6280WriteData(UnitNumber, 0x800, (byte)i, false, true);

                                var data = pcmData.GetNextData(sampleRate);

                                int val = ((int)data - 0x80);
                                if (pcmData.Gain != 1.0f)
                                    val = (int)Math.Round(val * pcmData.Gain);
                                if (!pcmData.DisableVelocity)
                                    val = (int)Math.Round(((float)val * (float)pcmData.Note.Velocity) / 127f);
                                if (pcmData.Volume != 1.0f)
                                    val = (int)Math.Round(val * pcmData.Volume);
                                if (val > sbyte.MaxValue)
                                    val = sbyte.MaxValue;
                                else if (val < sbyte.MinValue)
                                    val = sbyte.MinValue;

                                C6280WriteData(UnitNumber, 0x806, (byte)(val >> 3), false, true);
                            }
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="data"></param>
        private void playPcmData(int slot, byte[] data, HuC6280Timbre pcmTimbre, TaggedNoteOnEvent note, double freq, double volume)
        {
            lock (pcmTable)
            {
                if (data == null)
                    pcmTable[slot] = null;
                else
                {
                    PcmData pcmData = new PcmData(data, note, false);
                    pcmData.SampleRate = pcmTimbre.DAC.SampleRate;
                    pcmData.Pitch = freq / pcmTimbre.DAC.BaseFreqency;
                    pcmData.PitchEnabled = pcmTimbre.DAC.PitchEnabled;
                    pcmData.Gain = pcmTimbre.DAC.PcmGain;
                    pcmData.Volume = volume;
                    pcmData.LoopEnabled = pcmTimbre.DAC.LoopEnabled;
                    pcmData.LoopPoint = pcmTimbre.DAC.LoopPoint;

                    pcmTable[slot] = pcmData;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private static delegate_set_callback set_callback
        {
            get;
            set;
        }

        private static double[] volumeTable = new double[32];

        /// <summary>
        /// 
        /// </summary>
        static HuC6280()
        {
            IntPtr funcPtr = MameIF.GetProcAddress("c6280_w");
            if (funcPtr != IntPtr.Zero)
                C6280_w = (delegate_c6280_w)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_c6280_w));
            funcPtr = MameIF.GetProcAddress("c6280_set_pcm_callback");
            if (funcPtr != IntPtr.Zero)
                set_callback = (delegate_set_callback)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_set_callback));

            double level = 1;
            double step = 48.0 / 32.0;
            for (int i = 0; i < 30; i++)
            {
                volumeTable[i] = level;
                level /= Math.Pow(10.0, step / 20.0);
            }
            volumeTable[30] = volumeTable[31] = 0;
        }

        private Hu6280SoundManager soundManager;


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

        private delg_callback f_pcm_callback;

        /// <summary>
        /// 
        /// </summary>
        public HuC6280(uint unitNumber) : base(unitNumber)
        {
            GainLeft = DEFAULT_GAIN;
            GainRight = DEFAULT_GAIN;

            Timbres = new HuC6280Timbre[InstrumentBase.DEFAULT_MAX_TIMBRES];
            for (int i = 0; i < InstrumentBase.DEFAULT_MAX_TIMBRES; i++)
                Timbres[i] = new HuC6280Timbre();
            setPresetInstruments();

            this.soundManager = new Hu6280SoundManager(this);

            pcmTable = new PcmData[6];

            this.pcmEngine = new PcmEngine(this);
            this.pcmEngine.StartEngine();

            f_pcm_callback = new delg_callback(pcm_callback);
            C6280SetCallback(UnitNumber, f_pcm_callback);
        }


        private bool disposedValue = false; // 重複する呼び出しを検出するには

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //マネージ状態を破棄します (マネージ オブジェクト)。
                    soundManager?.Dispose();
                    pcmEngine?.Dispose();

                    lock (sndEnginePtrLock)
                    {
                        if (vsifClient != null)
                            vsifClient.Dispose();
                    }
                }

                // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                // TODO: 大きなフィールドを null に設定します。
                C6280SetCallback(UnitNumber, null);

                disposedValue = true;
            }
        }

        // TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
        ~HuC6280()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(false);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(true);
            // TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
            GC.SuppressFinalize(this);
        }

        internal override void PrepareSound()
        {
            base.PrepareSound();

            initGlobalRegisters();
        }

        private void initGlobalRegisters()
        {
            C6280WriteData(UnitNumber, 0x801, (byte)0xff);
        }


        /// <summary>
        /// 
        /// </summary>
        private void setPresetInstruments()
        {
            Timbres[0].SoundType = SoundType.WSG;
            Timbres[0].WsgData = new byte[]
            {
                0x0f, 0x12, 0x15, 0x17, 0x19, 0x1b, 0x1d, 0x1e,
                0x1e, 0x1e, 0x1d, 0x1b, 0x19, 0x17, 0x15, 0x12,
                0x0f, 0x0c, 0x09, 0x07, 0x05, 0x03, 0x01, 0x00,
                0x00, 0x00, 0x01, 0x03, 0x05, 0x07, 0x09, 0x0c  };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vgmPath"></param>
        public override void StartVgmRecordingTo(string vgmPath)
        {
            base.StartVgmRecordingTo(vgmPath);

            //Sound On
            C6280WriteData(UnitNumber, 0x801, (byte)0xff);
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
        private class Hu6280SoundManager : SoundManagerBase
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

            private static SoundList<Hu6280Sound> lfoOnSounds = new SoundList<Hu6280Sound>(1);

            private static SoundList<Hu6280Sound> wsgOnSounds = new SoundList<Hu6280Sound>(2);

            private HuC6280 parentModule;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            public Hu6280SoundManager(HuC6280 parent) : base(parent)
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
                foreach (HuC6280Timbre timbre in parentModule.GetBaseTimbres(note))
                {
                    tindex++;
                    var emptySlot = searchEmptySlot(note, timbre);
                    if (emptySlot.slot < 0)
                        continue;

                    Hu6280Sound snd = new Hu6280Sound(emptySlot.inst, this, timbre, tindex - 1, note, emptySlot.slot);
                    switch (timbre.SoundType)
                    {
                        case SoundType.WSGLFO:
                            lfoOnSounds.Add(snd);
                            break;
                        case SoundType.WSG:
                            wsgOnSounds.Add(snd);
                            break;
                        case SoundType.NOISE:
                            wsgOnSounds.Add(snd);
                            break;
                        case SoundType.PCM:
                            wsgOnSounds.Add(snd);
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
            private (HuC6280 inst, int slot) searchEmptySlot(TaggedNoteOnEvent note, HuC6280Timbre timbre)
            {
                var emptySlot = (parentModule, -1);

                switch (timbre.SoundType)
                {
                    case SoundType.WSGLFO:
                        {
                            emptySlot = SearchEmptySlotAndOffForLeader(parentModule, lfoOnSounds, note, 1);
                            break;
                        }
                    case SoundType.PCM:
                    case SoundType.WSG:
                        {
                            var slot = timbre.AssignMIDIChtoSlotNum ? note.Channel + timbre.AssignMIDIChtoSlotNumOffset : -1;
                            if (slot == -1)
                            {
                                if (timbre.PartialReserveWSGLFO)
                                {
                                    if (timbre.PartialReserveNOISE)
                                        emptySlot = SearchEmptySlotAndOffForLeader(parentModule, wsgOnSounds, note, 2, -1, 2);
                                    else
                                        emptySlot = SearchEmptySlotAndOffForLeader(parentModule, wsgOnSounds, note, 4, -1, 2);
                                }
                                else
                                {
                                    if (timbre.PartialReserveNOISE)
                                        emptySlot = SearchEmptySlotAndOffForLeader(parentModule, wsgOnSounds, note, 4);
                                    else
                                        emptySlot = SearchEmptySlotAndOffForLeader(parentModule, wsgOnSounds, note, 6);
                                }
                            }
                            else
                                emptySlot = SearchEmptySlotAndOffForLeader(parentModule, wsgOnSounds, note, 6, slot, 0);
                            break;
                        }
                    case SoundType.NOISE:
                        {
                            emptySlot = SearchEmptySlotAndOffForLeader(parentModule, wsgOnSounds, note, 6, -1, 4);
                            break;
                        }
                }

                return emptySlot;
            }

            internal override void ProcessAllSoundOff()
            {
                var me = new ControlChangeEvent((SevenBitNumber)120, (SevenBitNumber)0);
                ProcessControlChange(me);

                for (int i = 0; i < 6; i++)
                {
                    parentModule.C6280WriteData(parentModule.UnitNumber, 0x800, (byte)i);
                    parentModule.C6280WriteData(parentModule.UnitNumber, 0x804, (byte)0);
                    parentModule.C6280WriteData(parentModule.UnitNumber, 0x807, (byte)0);
                }

                for (int i = 0; i < MAX_DAC_VOICES; i++)
                    parentModule.pcmEngine.Stop(i);
            }

        }


        /// <summary>
        /// 
        /// </summary>
        private class Hu6280Sound : SoundBase
        {

            private HuC6280 parentModule;

            private HuC6280Timbre timbre;

            private SoundType lastSoundType;

            private byte[] lastLfoData;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentModule"></param>
            /// <param name="noteOnEvent"></param>
            /// <param name="programNumber"></param>
            /// <param name="slot"></param>
            public Hu6280Sound(HuC6280 parentModule, Hu6280SoundManager manager, TimbreBase timbre, int tindex, TaggedNoteOnEvent noteOnEvent, int slot) : base(parentModule, manager, timbre, tindex, noteOnEvent, slot)
            {
                this.parentModule = parentModule;
                this.timbre = (HuC6280Timbre)timbre;

                lastSoundType = this.timbre.SoundType;
                lastLfoData = this.timbre.LfoData;
            }

            /// <summary>
            /// 
            /// </summary>
            public override void KeyOn()
            {
                base.KeyOn();

                parentModule.C6280WriteData(parentModule.UnitNumber, 0x800, (byte)Slot);
                parentModule.C6280WriteData(parentModule.UnitNumber, 0x804, (byte)0);

                switch (lastSoundType)
                {
                    case SoundType.WSGLFO:
                        {
                            //ch0 WSG

                            foreach (var d in timbre.WsgData)
                                parentModule.C6280WriteData(parentModule.UnitNumber, 0x806, d);

                            //ch1 LFO
                            parentModule.C6280WriteData(parentModule.UnitNumber, 0x800, (byte)(Slot + 1));

                            foreach (var d in lastLfoData)
                                parentModule.C6280WriteData(parentModule.UnitNumber, 0x806, d);

                            parentModule.C6280WriteData(parentModule.UnitNumber, 0x808, timbre.LfoFreq);
                            parentModule.C6280WriteData(parentModule.UnitNumber, 0x809, (byte)((timbre.LfoEnable ? 0x00 : 0x80) | timbre.LfoMode));

                            FormMain.OutputDebugLog(parentModule, "KeyOn LFO ch" + Slot + " " + NoteOnEvent.ToString());
                            break;
                        }
                    case SoundType.WSG:
                        {
                            foreach (var d in timbre.WsgData)
                                parentModule.C6280WriteData(parentModule.UnitNumber, 0x806, d);

                            FormMain.OutputDebugLog(parentModule, "KeyOn PSG ch" + Slot + " " + NoteOnEvent.ToString());
                            break;
                        }
                    case SoundType.PCM:
                        {
                            parentModule.playPcmData(Slot, timbre.PcmData, timbre, NoteOnEvent, CalcCurrentFrequency(), CalcCurrentVolume());
                            FormMain.OutputDebugLog(parentModule, "KeyOn PSG(PCM) ch" + Slot + " " + NoteOnEvent.ToString());
                            break;
                        }
                    case SoundType.NOISE:
                        {
                            FormMain.OutputDebugLog(parentModule, "KeyOn NOISE ch" + Slot + " " + NoteOnEvent.ToString());
                            break;
                        }
                }
                OnPanpotUpdated();

                OnPitchUpdated();

                OnVolumeUpdated();
            }


            public override void OnSoundParamsUpdated()
            {
                updateWsgData();

                base.OnSoundParamsUpdated();
            }

            private void updateWsgData()
            {
                if (FxEngine != null && FxEngine.Active)
                {
                    var eng = FxEngine as HuC6280FxEngine;
                    if (eng?.LfoValue != null)
                    {
                        var no = (byte)(eng.LfoValue.Value & 3);
                        {
                            switch (no)
                            {
                                case 1:
                                case 2:
                                case 3:
                                    lastLfoData = timbre.LfoMorphData[no - 1].LfoData;
                                    break;
                                default:
                                    lastLfoData = timbre.LfoData;
                                    break;
                            }

                            //ch1 LFO
                            parentModule.C6280WriteData(parentModule.UnitNumber, 0x800, (byte)(Slot + 1));
                            foreach (var d in lastLfoData)
                                parentModule.C6280WriteData(parentModule.UnitNumber, 0x806, d);

                            parentModule.C6280WriteData(parentModule.UnitNumber, 0x808, timbre.LfoFreq);
                            parentModule.C6280WriteData(parentModule.UnitNumber, 0x809, (byte)((timbre.LfoEnable ? 0x00 : 0x80) | timbre.LfoMode));
                        }
                    }
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public override void OnVolumeUpdated()
            {
                if (IsSoundOff)
                    return;

                var vol = CalcCurrentVolume();
                byte wvol = 31;
                for (int i = volumeTable.Length - 1; i >= 0; i--)
                {
                    if (vol < volumeTable[i])
                    {
                        wvol = (byte)(31 - i);
                        break;
                    }
                }
                if ((wvol & 1) == 1)
                    wvol &= 0xfe;
                else
                    wvol |= 1;

                parentModule.C6280WriteData(parentModule.UnitNumber, 0x800, (byte)Slot);
                switch (lastSoundType)
                {
                    case SoundType.WSGLFO:
                    case SoundType.WSG:
                        {
                            parentModule.C6280WriteData(parentModule.UnitNumber, 0x804, (byte)(0x80 | wvol));
                            break;
                        }
                    case SoundType.PCM:
                        {
                            parentModule.C6280WriteData(parentModule.UnitNumber, 0x804, (byte)(0xc0 | wvol));
                            break;
                        }
                    case SoundType.NOISE:
                        {
                            parentModule.C6280WriteData(parentModule.UnitNumber, 0x804, (byte)(0x80 | wvol));
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
                if (IsSoundOff)
                    return;

                double freq = CalcCurrentFrequency();

                //Freq
                parentModule.C6280WriteData(parentModule.UnitNumber, 0x800, (byte)Slot);
                switch (lastSoundType)
                {
                    case SoundType.WSGLFO:
                    case SoundType.WSG:
                        {
                            ushort wsgfreq = convertWsgFrequency(freq);

                            parentModule.C6280WriteData(parentModule.UnitNumber, 0x802, (byte)(wsgfreq & 0xff));
                            parentModule.C6280WriteData(parentModule.UnitNumber, 0x803, (byte)((wsgfreq >> 8) & 0x0f));
                            break;
                        }
                    case SoundType.PCM:
                        lock (parentModule.pcmTable)
                        {
                            var pcmData = parentModule.pcmTable[Slot];
                            if (pcmData != null)
                                pcmData.Pitch = CalcCurrentFrequency() / timbre.DAC.BaseFreqency;

                        }
                        break;
                    case SoundType.NOISE:
                        {
                            ushort noisefreq = convertNoiseFrequency(freq);

                            parentModule.C6280WriteData(parentModule.UnitNumber, 0x807, (byte)(0x80 | noisefreq));

                            break;
                        }
                }

                base.OnPitchUpdated();
            }

            /// <summary>
            /// 
            /// </summary>
            public override void OnPanpotUpdated()
            {
                if (IsSoundOff)
                    return;

                //Pan
                int pan = CalcCurrentPanpot();

                double left = 0;
                double right = 0;
                if (pan > 64)
                    left = Math.Cos(Math.PI / 2 * (pan / 127d));
                else
                    left = Math.Cos(Math.PI / 2 * (64 / 127d));
                if (pan < 64)
                    right = Math.Sin(Math.PI / 2 * (pan / 127d));
                else
                    right = Math.Sin(Math.PI / 2 * (64 / 127d));

                byte wlvol = 0;
                for (int i = volumeTable.Length - 1; i >= 0; i--)
                {
                    if (left < volumeTable[i])
                    {
                        wlvol = (byte)((31 - i) / 2);
                        break;
                    }
                }
                byte wrvol = 0;
                for (int i = volumeTable.Length - 1; i >= 0; i--)
                {
                    if (right < volumeTable[i])
                    {
                        wrvol = (byte)((31 - i) / 2);
                        break;
                    }
                }

                parentModule.C6280WriteData(parentModule.UnitNumber, 0x800, (byte)Slot);
                switch (lastSoundType)
                {
                    case SoundType.WSGLFO:
                    case SoundType.WSG:
                    case SoundType.PCM:
                        {
                            parentModule.C6280WriteData(parentModule.UnitNumber, 0x805, (byte)(wlvol << 4 | wrvol));
                            break;
                        }
                    case SoundType.NOISE:
                        {
                            parentModule.C6280WriteData(parentModule.UnitNumber, 0x805, (byte)(wlvol << 4 | wrvol));
                            break;
                        }
                }
            }


            /// <summary>
            /// 
            /// </summary>
            public override void SoundOff()
            {
                base.SoundOff();

                parentModule.C6280WriteData(parentModule.UnitNumber, 0x800, (byte)Slot);
                switch (lastSoundType)
                {
                    case SoundType.WSGLFO:
                    case SoundType.WSG:
                        {
                            parentModule.C6280WriteData(parentModule.UnitNumber, 0x804, (byte)0);
                            break;
                        }
                    case SoundType.PCM:
                        {
                            parentModule.C6280WriteData(parentModule.UnitNumber, 0x804, (byte)0);
                            parentModule.playPcmData(Slot, null, timbre, NoteOnEvent, CalcCurrentFrequency(), CalcCurrentVolume());
                            break;
                        }
                    case SoundType.NOISE:
                        {
                            parentModule.C6280WriteData(parentModule.UnitNumber, 0x804, (byte)0);
                            parentModule.C6280WriteData(parentModule.UnitNumber, 0x807, (byte)0);
                            break;
                        }
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="note"></param>
            /// <returns></returns>
            private ushort convertWsgFrequency(double freq)
            {
                double f = (3580000d / (32 * freq));
                if (f > 4095d)
                    f = 4095d;
                return (ushort)Math.Round(f);
            }


            /// <summary>
            /// 
            /// </summary>
            /// <param name="note"></param>
            /// <returns></returns>
            private ushort convertNoiseFrequency(double freq)
            {
                double f = (3580000d / (64 * freq));
                if (f > 4095d)
                    f = 4095d;
                return (ushort)(((ushort)Math.Round(f)) ^ 31);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        [JsonConverter(typeof(NoTypeConverterJsonConverter<HuC6280Timbre>))]
        [DataContract]
        [InstLock]
        public class HuC6280Timbre : TimbreBase
        {

            [DataMember]
            [Category("Sound")]
            [Description("Sound Type")]
            [DefaultValue(SoundType.WSG)]
            public SoundType SoundType
            {
                get;
                set;
            }

            private bool f_PartialReserveLFO;

            [DataMember]
            [Category("Chip")]
            [Description("LFOWSG partial reserve against with WSG.\r\n" +
                "WSG w/ LFO shared 1,2 ch with WSG." +
                "So, you can choose whether to give priority to WSG w/ LFO over WSG")]
            [DefaultValue(false)]
            public bool PartialReserveWSGLFO
            {
                get
                {
                    return f_PartialReserveLFO;
                }
                set
                {
                    f_PartialReserveLFO = value;
                }
            }

            private bool f_PartialReserveNoise;

            [DataMember]
            [Category("Chip")]
            [Description("NOISE partial reserve against with WSG.\r\n" +
                "NOISE shared 5,6 ch with WSG." +
                "So, you can choose whether to give priority to NOISE over WSG")]
            [DefaultValue(false)]
            public bool PartialReserveNOISE
            {
                get
                {
                    return f_PartialReserveNoise;
                }
                set
                {
                    f_PartialReserveNoise = value;
                }
            }

            private byte[] f_wsgdata = new byte[32];

            [TypeConverter(typeof(ArrayConverter))]
            [Editor(typeof(WsgUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [WsgBitWideAttribute(5)]
            [DataMember]
            [Category("Sound")]
            [Description("Wave Table (32 samples, 0-31 levels)")]
            public byte[] WsgData
            {
                get
                {
                    return f_wsgdata;
                }
                set
                {
                    f_wsgdata = value;
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
                f_wsgdata = new byte[32];
            }

            [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
            typeof(UITypeEditor)), Localizable(false)]
            [Category("Sound")]
            [Description("Wave Table (32 samples, 0-31 levels)")]
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
                        WsgData[i] = vs[i] > 31 ? (byte)31 : vs[i];
                }
            }

            private byte[] f_PcmData = new byte[0];

            [TypeConverter(typeof(LoadDataTypeConverter))]
            [Editor(typeof(PcmFileLoaderUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DataMember]
            [Category("Sound")]
            [Description("Unsigned 8bit(Only high 5bit is used) PCM Raw Data or WAV Data. (MAX 64KB, 1ch @ 6991.3Hz)")]
            [PcmFileLoaderEditor("Audio File(*.raw, *.wav)|*.raw;*.wav", 0, 8, 1, 65535)]
            public byte[] PcmData
            {
                get
                {
                    return f_PcmData;
                }
                set
                {
                    f_PcmData = value;
                }
            }

            public bool ShouldSerializePcmData()
            {
                return PcmData.Length != 0;
            }

            public void ResetPcmData()
            {
                PcmData = new byte[0];
            }

            private String pcmDataInfo;

            [DataMember]
            [Category("Sound(PCM)")]
            [Description("PcmData information.\r\n*Warning* May contain privacy information. Check the options dialog.")]
            [ReadOnly(true)]
            public String PcmDataInfo
            {
                get
                {
                    if (Settings.Default.DoNotUsePrivacySettings)
                        return null;
                    return pcmDataInfo;
                }
                set
                {
                    pcmDataInfo = value;
                }
            }

            [DataMember]
            [Category("Sound(PCM)")]
            [Description("PCM Settings")]
            public HuCDacSettings DAC
            {
                get;
                set;
            }

            private bool f_LfoEnable = true;

            [DataMember]
            [Category("Sound(LFO)")]
            [Description("Where the LFO enable or not when the ToneType is WSGLFO.")]
            [DefaultValue(true)]
            public bool LfoEnable
            {
                get
                {
                    return f_LfoEnable;
                }
                set
                {
                    f_LfoEnable = value;
                }
            }

            private byte f_LfoMode = 1;

            [DataMember]
            [Category("Sound(LFO)")]
            [Description("Select LFO mode (0=Disable, 1=On, 2=On(x4), 3=On(x8))")]
            [SlideParametersAttribute(0, 3)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue((byte)1)]
            public byte LfoMode
            {
                get
                {
                    return f_LfoMode;
                }
                set
                {
                    f_LfoMode = (byte)(value & 3);
                }
            }

            private byte f_LfoFreq = 32;

            [DataMember]
            [Category("Sound(LFO)")]
            [Description("Set LFO freqency")]
            [SlideParametersAttribute(0, 255)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue((byte)32)]
            public byte LfoFreq
            {
                get
                {
                    return f_LfoFreq;
                }
                set
                {
                    f_LfoFreq = value;
                }
            }


            private byte[] f_lfodata = new byte[32];

            [TypeConverter(typeof(ArrayConverter))]
            [Editor(typeof(WsgUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [WsgBitWideAttribute(5)]
            [DataMember]
            [Category("Sound")]
            [Description("LFO Table (32 samples, 0-31 levels)")]
            public byte[] LfoData
            {
                get
                {
                    return f_lfodata;
                }
                set
                {
                    f_lfodata = value;
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
                f_lfodata = new byte[32];
            }

            [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
            typeof(UITypeEditor)), Localizable(false)]
            [Category("Sound")]
            [Description("LFO Table (32 samples, 0-31 levels)")]
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
                    List<byte> vs = new List<byte>();
                    foreach (var val in vals)
                    {
                        byte v = 0;
                        if (byte.TryParse(val, out v))
                            vs.Add(v);
                    }
                    for (int i = 0; i < Math.Min(LfoData.Length, vs.Count); i++)
                        LfoData[i] = vs[i] > 31 ? (byte)31 : vs[i];
                }
            }

            private HuC6280LfoMorphData[] f_lfoMorphData = new HuC6280LfoMorphData[3] {
                new HuC6280LfoMorphData(),
                new HuC6280LfoMorphData(),
                new HuC6280LfoMorphData()
            };

            [TypeConverter(typeof(ArrayConverter))]
            [EditorAttribute(typeof(DummyEditor), typeof(UITypeEditor))]
            [DataMember]
            [Category("Sound")]
            [Description("LFO Morph Table")]
            public HuC6280LfoMorphData[] LfoMorphData
            {
                get
                {
                    return f_lfoMorphData;
                }
                set
                {
                    f_lfoMorphData = value;
                }
            }

            public bool ShouldSerializeLfoMorphData()
            {
                foreach (var dt in f_lfoMorphData)
                {
                    if (dt != null && dt.ShouldSerializeLfoData())
                        return true;
                }
                return false;
            }

            public void ResetLfoMorphData()
            {
                f_lfoMorphData = new HuC6280LfoMorphData[3] {
                    new HuC6280LfoMorphData(),
                    new HuC6280LfoMorphData(),
                    new HuC6280LfoMorphData()
                };
            }

            /// <summary>
            /// 
            /// </summary>
            public HuC6280Timbre()
            {
                DAC = new HuCDacSettings();
            }

            protected override void InitializeFxS()
            {
                SDS.FxS = new HuC6280FxSettings();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="serializeData"></param>
            public override void RestoreFrom(string serializeData)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<HuC6280Timbre>(serializeData);
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

        [JsonConverter(typeof(NoTypeConverterJsonConverter<HuC6280LfoMorphData>))]
        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [DataContract]
        [InstLock]
        public class HuC6280LfoMorphData : ContextBoundObject
        {

            private byte[] f_lfodata = new byte[32];

            [TypeConverter(typeof(ArrayConverter))]
            [Editor(typeof(WsgUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [WsgBitWideAttribute(5)]
            [DataMember]
            [Category("Sound")]
            [Description("LFO Table (32 samples, 0-31 levels)")]
            public byte[] LfoData
            {
                get
                {
                    return f_lfodata;
                }
                set
                {
                    f_lfodata = value;
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
                f_lfodata = new byte[32];
            }


            [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
            typeof(UITypeEditor)), Localizable(false)]
            [Category("Sound")]
            [Description("LFO Table (32 samples, 0-31 levels)")]
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
                    List<byte> vs = new List<byte>();
                    foreach (var val in vals)
                    {
                        byte v = 0;
                        if (byte.TryParse(val, out v))
                            vs.Add(v);
                    }
                    for (int i = 0; i < Math.Min(LfoData.Length, vs.Count); i++)
                        LfoData[i] = vs[i] > 31 ? (byte)31 : vs[i];
                }
            }


        }

        [JsonConverter(typeof(NoTypeConverterJsonConverter<HuC6280FxSettings>))]
        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [DataContract]
        [InstLock]
        public class HuC6280FxSettings : BasicFxSettings
        {

            private string f_MorphEnvelopes;

            [DataMember]
            [Description("Set wave table number by text. Input wave table number and split it with space like the FamiTracker.\r\n" +
                       "0-3(0 is normal table) \"|\" is repeat point. \"/\" is release point.")]
            [Editor(typeof(EnvelopeUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [EnvelopeEditorAttribute(0, 3)]
            public string MorphEnvelopes
            {
                get
                {
                    return f_MorphEnvelopes;
                }
                set
                {
                    if (f_MorphEnvelopes != value)
                    {
                        MorphEnvelopesRepeatPoint = -1;
                        MorphEnvelopesReleasePoint = -1;
                        if (value == null)
                        {
                            MorphEnvelopesNums = new int[] { };
                            f_MorphEnvelopes = string.Empty;
                            return;
                        }
                        f_MorphEnvelopes = value;
                        string[] vals = value.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        List<int> vs = new List<int>();
                        for (int i = 0; i < vals.Length; i++)
                        {
                            string val = vals[i];
                            if (val.Equals("|", StringComparison.Ordinal))
                                MorphEnvelopesRepeatPoint = vs.Count;
                            else if (val.Equals("/", StringComparison.Ordinal))
                                MorphEnvelopesReleasePoint = vs.Count;
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
                        MorphEnvelopesNums = vs.ToArray();

                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < MorphEnvelopesNums.Length; i++)
                        {
                            if (sb.Length != 0)
                                sb.Append(' ');
                            if (MorphEnvelopesRepeatPoint == i)
                                sb.Append("| ");
                            if (MorphEnvelopesReleasePoint == i)
                                sb.Append("/ ");
                            if (i < MorphEnvelopesNums.Length)
                                sb.Append(MorphEnvelopesNums[i].ToString((IFormatProvider)null));
                        }
                        f_MorphEnvelopes = sb.ToString();
                    }
                }
            }

            public bool ShouldSerializeMorphEnvelopes()
            {
                return !string.IsNullOrEmpty(MorphEnvelopes);
            }

            public void ResetMorphEnvelopes()
            {
                MorphEnvelopes = null;
            }

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            public int[] MorphEnvelopesNums { get; set; } = new int[] { };

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int MorphEnvelopesRepeatPoint { get; set; } = -1;

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int MorphEnvelopesReleasePoint { get; set; } = -1;

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override AbstractFxEngine CreateEngine()
            {
                return new HuC6280FxEngine(this);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public class HuC6280FxEngine : BasicFxEngine
        {
            private HuC6280FxSettings settings;

            /// <summary>
            /// 
            /// </summary>
            public HuC6280FxEngine(HuC6280FxSettings settings) : base(settings)
            {
                this.settings = settings;
            }

            private uint f_lfoCounter;

            public byte? LfoValue
            {
                get;
                private set;
            }

            protected override bool ProcessCore(SoundBase sound, bool isKeyOff, bool isSoundOff)
            {
                bool process = base.ProcessCore(sound, isKeyOff, isSoundOff);

                LfoValue = null;
                if (settings.MorphEnvelopesNums.Length > 0)
                {
                    if (!isKeyOff)
                    {
                        var vm = settings.MorphEnvelopesNums.Length;
                        if (settings.MorphEnvelopesReleasePoint >= 0)
                            vm = settings.MorphEnvelopesReleasePoint;
                        if (f_lfoCounter >= vm)
                        {
                            if (settings.MorphEnvelopesRepeatPoint >= 0)
                                f_lfoCounter = (uint)settings.MorphEnvelopesRepeatPoint;
                            else
                                f_lfoCounter = (uint)vm - 1;
                        }
                    }
                    else
                    {
                        if (f_lfoCounter < settings.MorphEnvelopesNums.Length)
                        {
                            if (settings.MorphEnvelopesReleasePoint >= 0 && f_lfoCounter <= (uint)settings.MorphEnvelopesReleasePoint)
                                f_lfoCounter = (uint)settings.MorphEnvelopesReleasePoint;
                            else if (settings.MorphEnvelopesReleasePoint < 0 && settings.KeyOffStop)
                                f_lfoCounter = (uint)settings.MorphEnvelopesNums.Length;
                        }
                    }
                    if (f_lfoCounter < settings.MorphEnvelopesNums.Length)
                    {
                        int vol = settings.MorphEnvelopesNums[f_lfoCounter++];

                        LfoValue = (byte)vol;
                        process = true;
                    }
                }

                return process;
            }
        }

        private PcmEngine pcmEngine;


        /// <summary>
        /// 
        /// </summary>
        private class PcmEngine : IDisposable
        {
            private Hu6280Sound sound;

            private object engineLockObject;

            private bool stopEngineFlag;

            private bool disposedValue;


            private HuC6280 parentModule;

            private uint unitNumber;

            private SampleData[] currentSampleData;

            /// <summary>
            /// 
            /// </summary>
            public PcmEngine(HuC6280 parentModule)
            {
                this.parentModule = parentModule;
                unitNumber = parentModule.UnitNumber;
                engineLockObject = new object();
                stopEngineFlag = true;
                currentSampleData = new SampleData[HuC6280.MAX_DAC_VOICES];
            }


            /// <summary>
            /// 
            /// </summary>
            public void StartEngine()
            {
                if (stopEngineFlag)
                {
                    stopEngineFlag = false;
                    Thread t = new Thread(processDac);
                    t.Priority = ThreadPriority.AboveNormal;
                    t.Start();
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public void StopEngine()
            {
                stopEngineFlag = true;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="pcmData"></param>
            public void Play(Hu6280Sound sound, TaggedNoteOnEvent note, int slot, HuC6280Timbre pcmTimbre, double freq, double volume)
            {
                this.sound = sound;
                lock (engineLockObject)
                {
                    /*
                    int nn = (int)NoteNames.C3;
                    double noteNum = Math.Pow(2.0, ((double)nn - 69.0) / 12.0);
                    double basefreq = 440.0 * noteNum;
                    */
                    double basefreq = ((RP2A03Timbre)sound.Timbre).DAC.BaseFreqency;

                    var sd = new SampleData(sound, note, pcmTimbre.PcmData, pcmTimbre.DAC.SampleRate, true, basefreq);
                    sd.Gain = pcmTimbre.DAC.PcmGain;
                    sd.Pitch = freq / basefreq;
                    sd.Volume = volume;
                    sd.LoopEnabled = pcmTimbre.DAC.LoopEnabled;
                    sd.LoopPoint = pcmTimbre.DAC.LoopPoint;
                    currentSampleData[slot] = sd;

                    //keyoff
                    /*
                    byte data = (byte)(parentModule.RP2A03ReadData(parentModule.UnitNumber, 0x15) & ~(1 << 4));
                    parentModule.RP2A03WriteData(parentModule.UnitNumber, 0x15, (byte)data);
                    */
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="pcmData"></param>
            public void Pitch(int slot, double freq)
            {
                lock (engineLockObject)
                {
                    if (currentSampleData[slot] != null)
                        currentSampleData[slot].Pitch = freq / currentSampleData[slot].BaseFreq;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="pcmData"></param>
            public void Volume(int slot, double volume)
            {
                lock (engineLockObject)
                {
                    if (currentSampleData[slot] != null)
                        currentSampleData[slot].Volume = volume;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="pcmData"></param>
            public void Stop(int slot)
            {
                lock (engineLockObject)
                {
                    currentSampleData[slot] = null;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="pcmData"></param>
            public void StopAll()
            {
                lock (engineLockObject)
                {
                    for (int i = 0; i < currentSampleData.Length; i++)
                        currentSampleData[i] = null;
                }
            }

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool QueryPerformanceFrequency(out long frequency);

            /// <summary>
            /// 
            /// </summary>
            private void processDac()
            {
                long freq, before, after;
                double dbefore;
                QueryPerformanceFrequency(out freq);
                QueryPerformanceCounter(out before);
                dbefore = before;
                while (!stopEngineFlag)
                {
                    if (disposedValue)
                        break;

                    //int dacData = 0;
                    //bool playDac = false;
                    double sampleRate = 6991.3;

                    {
                        switch (parentModule.CurrentSoundEngine)
                        {
                            case SoundEngineType.VSIF_PCE_TurboEverDrive:
                                lock (engineLockObject)
                                {
                                    PcmData[] pcmTable = null;
                                    lock (parentModule.pcmTable)
                                    {
                                        pcmTable = parentModule.pcmTable;

                                        for (int i = 0; i < pcmTable.Length; i++)
                                        {
                                            var pcmData = pcmTable[i];
                                            if (pcmData != null)
                                            {
                                                if (pcmData.PeekDacData() == null)
                                                {
                                                    pcmTable[i] = null;
                                                    continue;
                                                }

                                                parentModule.C6280WriteData(parentModule.UnitNumber, 0x800, (byte)i, true, false);

                                                var data = pcmData.GetNextData(sampleRate);

                                                int val = ((int)data - 0x80);
                                                if (pcmData.Gain != 1.0f)
                                                    val = (int)Math.Round(val * pcmData.Gain);
                                                if (!pcmData.DisableVelocity)
                                                    val = (int)Math.Round(((float)val * (float)pcmData.Note.Velocity) / 127f);
                                                if (pcmData.Volume != 1.0f)
                                                    val = (int)Math.Round(val * pcmData.Volume);
                                                if (val > sbyte.MaxValue)
                                                    val = sbyte.MaxValue;
                                                else if (val < sbyte.MinValue)
                                                    val = sbyte.MinValue;

                                                parentModule.C6280WriteData(parentModule.UnitNumber, 0x806, (byte)(val >> 3), true, false);

                                            }
                                        }
                                    }
                                }
                                break;
                        }
                        /*
                        lock (engineLockObject)
                        {
                            foreach (var sd in currentSampleData)
                            {
                                if (sd == null)
                                    continue;

                                var d = sd.PeekDacData();
                                if (d == null)
                                    continue;

                                sampleRate = Math.Max(sampleRate, sd.SampleRate);
                            }

                            List<sbyte> data = new List<sbyte>();
                            foreach (var sd in currentSampleData)
                            {
                                if (sd == null)
                                    continue;

                                var d = sd.GetDacData(sampleRate);
                                if (d == null)
                                    continue;

                                int val = ((int)d.Value - 0x80);
                                if (sd.Gain != 1.0f)
                                    val = (int)Math.Round(val * sd.Gain);
                                if (!sd.DisableVelocity)
                                    val = (int)Math.Round(((float)val * (float)sd.Note.Velocity) / 127f);
                                if (sd.Volume != 1.0f)
                                    val = (int)Math.Round(val * sd.Volume);
                                if (val > sbyte.MaxValue)
                                    val = sbyte.MaxValue;
                                else if (val < sbyte.MinValue)
                                    val = sbyte.MinValue;
                                data.Add((sbyte)val);
                                playDac = true;
                            }
                            dacData = (int)Math.Round(PcmMixer.Mix(data, parentModule.EnableSmartDacClip));
                        }

                        int lastDacData = 0;
                        if (playDac || lastDacData != 0)
                        {
                            if (!playDac)
                                dacData = lastDacData;
                            if (dacData > sbyte.MaxValue)
                            {
                                dacData = sbyte.MaxValue;
                            }
                            else if (dacData < sbyte.MinValue)
                            {
                                dacData = sbyte.MinValue;
                            }
                            if (dacData != lastDacData)
                                parentModule.RP2A03WriteData(unitNumber, 0x11, (byte)((dacData + 0x80) >> 1), false, false);
                            lastDacData = dacData;
                        }
                        */
                    }

                    QueryPerformanceCounter(out after);
                    double nextTime = dbefore + ((double)freq / (double)sampleRate);
                    while (after < nextTime)
                        QueryPerformanceCounter(out after);
                    dbefore = nextTime;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="disposing"></param>
            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        // TODO: マネージド状態を破棄します (マネージド オブジェクト)
                        stopEngineFlag = true;
                    }

                    // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
                    // TODO: 大きなフィールドを null に設定します
                    disposedValue = true;
                }
            }

            // // TODO: 'Dispose(bool disposing)' にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします
            // ~PcmEngine()
            // {
            //     // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            //     Dispose(disposing: false);
            // }

            public void Dispose()
            {
                // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }


        [JsonConverter(typeof(NoTypeConverterJsonConverter<HuCDacSettings>))]
        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [DataContract]
        [InstLock]
        public class HuCDacSettings : ContextBoundObject
        {
            private bool f_PitchEnabled;

            [DataMember]
            [Category("Sound(PCM)")]
            [Description("Enabled Pitch")]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue(false)]
            public bool PitchEnabled
            {
                get
                {
                    return f_PitchEnabled;
                }
                set
                {
                    f_PitchEnabled = value;
                }
            }

            [DataMember]
            [Category("Sound(PCM)")]
            [Description("Set PCM base frequency [Hz]")]
            [DefaultValue(typeof(double), "440")]
            [DoubleSlideParametersAttribute(100, 2000, 1)]
            [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public double BaseFreqency
            {
                get;
                set;
            } = 440;


            [DataMember]
            [Category("Sound(PCM)")]
            [Description("Set DAC PCM sample rate [Hz].")]
            [DefaultValue(typeof(double), "6991.3")]
            [SlideParametersAttribute(4000, 14000)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public double SampleRate
            {
                get;
                set;
            } = 6991.3;

            private float f_PcmGain = 1.0f;

            [DataMember]
            [Category("Sound(PCM)")]
            [Description("Set DAC PCM gain(0.0-*).")]
            [EditorAttribute(typeof(DoubleSlideEditor), typeof(UITypeEditor))]
            [DefaultValue(typeof(float), "1.0")]
            [DoubleSlideParameters(0d, 10d, 0.1d)]
            public float PcmGain
            {
                get
                {
                    return f_PcmGain;
                }
                set
                {
                    if (f_PcmGain != value)
                    {
                        f_PcmGain = value;
                    }
                }
            }

            public virtual bool ShouldSerializePcmGain()
            {
                return f_PcmGain != 1.0f;
            }

            public virtual void ResetGainPcmGain()
            {
                f_PcmGain = 1.0f;
            }

            [DataMember]
            [Category("Sound(PCM)")]
            [Description("Set loop point")]
            [DefaultValue(typeof(uint), "0")]
            [SlideParametersAttribute(0, 65535)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public uint LoopPoint
            {
                get;
                set;
            }

            private bool f_LoopEnabled;

            [DataMember]
            [Category("Sound(PCM)")]
            [Description("Loop point enable")]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue(false)]
            public bool LoopEnabled
            {
                get
                {
                    return f_LoopEnabled;
                }
                set
                {
                    f_LoopEnabled = value;
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        private class SampleData
        {
            private Hu6280Sound sound;

            public byte[] PcmData
            {
                get;
                private set;
            }

            public double SampleRate
            {
                get;
                private set;
            }

            public TaggedNoteOnEvent Note
            {
                get;
                private set;
            }

            public bool DisableVelocity
            {
                get;
                private set;
            }

            public float Gain
            {
                get;
                set;
            }

            public double Volume
            {
                get;
                set;
            }

            public double BaseFreq
            {
                get;
                set;
            }

            public double Pitch
            {
                get;
                set;
            }

            public bool LoopEnabled
            {
                get;
                set;
            }

            public uint LoopPoint
            {
                get;
                set;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="adress"></param>
            /// <param name="size"></param>
            public SampleData(Hu6280Sound sound, TaggedNoteOnEvent note, byte[] pcmData, double sampleRate, bool disableVelocity, double baseFreq)
            {
                this.sound = sound;
                Note = note;
                PcmData = (byte[])pcmData.Clone();
                SampleRate = sampleRate;
                DisableVelocity = disableVelocity;
                BaseFreq = baseFreq;
                Gain = 1;
                Volume = 1;
                Pitch = 1;
                LoopEnabled = false;
                LoopPoint = 0;
            }

            private double index;

            public void Restart()
            {
                index = 0;
            }

            public byte? PeekDacData()
            {
                uint idx = (uint)Math.Round(index);
                if (idx >= PcmData.Length)
                {
                    if (LoopEnabled && LoopPoint < PcmData.Length)
                    {
                        idx = LoopPoint;
                    }
                    else
                    {
                        return null;
                    }
                }
                return PcmData[idx];
            }

            public byte? GetDacData(double sampleRate)
            {
                uint idx = (uint)Math.Round(index);
                if (idx >= PcmData.Length)
                {
                    if (LoopEnabled && LoopPoint < PcmData.Length)
                    {
                        index = LoopPoint;
                        idx = LoopPoint;
                    }
                    else
                    {
                        sound.TrySoundOff();
                        return null;
                    }
                }

                if (idx >= PcmData.Length - 1)
                {
                    index += (Pitch * (double)sampleRate) / (double)SampleRate;
                    return PcmData[idx];
                }

                var ret = (lerp(PcmData[idx], PcmData[idx + 1], index - idx)) + 128;
                index += (Pitch * (double)sampleRate) / (double)SampleRate;
                return (byte)Math.Round(ret);
            }

            static double lerp(double y0, double y1, double x)
            {
                y0 -= 128;
                y1 -= 128;
                return y0 + (y1 - y0) * x;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public enum SoundType
        {
            WSG,
            WSGLFO,
            NOISE,
            PCM
        }

        private class EnumConverterSoundEngineTypeRP2A03 : EnumConverter<SoundEngineType>
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                var sc = new StandardValuesCollection(new SoundEngineType[] {
                    SoundEngineType.Software,
                    SoundEngineType.VSIF_PCE_TurboEverDrive
               });

                return sc;
            }
        }
    }
}
