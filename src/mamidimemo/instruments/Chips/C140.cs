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

namespace zanac.MAmidiMEmo.Instruments.Chips
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class C140 : InstrumentBase
    {
        public override string Name => "C140";

        public override string Group => "PCM";

        public override InstrumentType InstrumentType => InstrumentType.C140;

        [Browsable(false)]
        public override string ImageKey => "C140";

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        protected override string SoundInterfaceTagNamePrefix => "c140_";

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
                return 15;
            }
        }

        private C140Clock f_Clock = C140Clock.Clk_22050Hz;


        [DataMember]
        [Category("Chip")]
        [Description("Set PCM clock. Original clock is 21.333KHz.")]
        [DefaultValue(C140Clock.Clk_22050Hz)]
        public C140Clock Clock
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

        [DataMember]
        [Category("Chip")]
        [Description("Signed 8bit PCM Raw Data or WAV Data. (MAX 64KB, 1ch)")]
        [Editor(typeof(PcmTableUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [PcmTableEditor("Audio File(*.raw, *.wav)|*.raw;*.wav")]
        [TypeConverter(typeof(CustomObjectTypeConverter))]
        public C140PcmSoundTable DrumSoundTable
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

        private C140Timbre[] f_Timbres;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("Chip")]
        [Description("Timbres (0-127)")]
        [EditorAttribute(typeof(DummyEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(ExpandableCollectionConverter))]
        public C140Timbre[] Timbres
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

        private const float DEFAULT_GAIN = 2.5f;

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
                using (var obj = JsonConvert.DeserializeObject<C140>(serializeData))
                    this.InjectFrom(new LoopInjection(new[] { "SerializeData" }), obj);
                C140SetCallback(UnitNumber, f_read_byte_callback);
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
        private delegate void delegate_c140_w(uint unitNumber, uint address, byte data);

        /// <summary>
        /// 
        /// </summary>
        private static void C140WriteData(uint unitNumber, uint address, byte data)
        {
            try
            {
                Program.SoundUpdating();
                c140_w(unitNumber, address, data);
            }
            finally
            {
                Program.SoundUpdated();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private static delegate_c140_w c140_w
        {
            get;
            set;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate sbyte delg_callback(byte pn, int pos);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delegate_set_callback(uint unitNumber, delg_callback callback);

        /// <summary>
        /// 
        /// </summary>
        private static void C140SetCallback(uint unitNumber, delg_callback callback)
        {
            try
            {
                Program.SoundUpdating();
                set_callback(unitNumber, callback);
            }
            finally
            {
                Program.SoundUpdated();
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

        private Dictionary<int, sbyte[]> tmpPcmDataTable = new Dictionary<int, sbyte[]>();

        /// <summary>
        /// 
        /// </summary>
        static C140()
        {
            IntPtr funcPtr = MameIF.GetProcAddress("c140_w");
            if (funcPtr != IntPtr.Zero)
                c140_w = (delegate_c140_w)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_c140_w));

            funcPtr = MameIF.GetProcAddress("c140_set_callback");
            if (funcPtr != IntPtr.Zero)
                set_callback = (delegate_set_callback)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_set_callback));
        }

        private C140SoundManager soundManager;

        private delg_callback f_read_byte_callback;

        /// <summary>
        /// 
        /// </summary>
        public C140(uint unitNumber) : base(unitNumber)
        {
            Timbres = new C140Timbre[128];
            for (int i = 0; i < 128; i++)
                Timbres[i] = new C140Timbre();

            DrumSoundTable = new C140PcmSoundTable();

            setPresetInstruments();

            this.soundManager = new C140SoundManager(this);

            f_read_byte_callback = new delg_callback(read_byte_callback);
            C140SetCallback(UnitNumber, f_read_byte_callback);

            GainLeft = DEFAULT_GAIN;
            GainRight = DEFAULT_GAIN;
        }


        #region IDisposable Support

        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //マネージ状態を破棄します (マネージ オブジェクト)。
                    soundManager?.Dispose();
                }

                // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                // TODO: 大きなフィールドを null に設定します。
                C140SetCallback(UnitNumber, null);

                disposedValue = true;
            }
        }

        // TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
        ~C140()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(false);
        }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public override void Dispose()
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
        /// <param name="pn"></param>
        /// <param name="pos"></param>
        private sbyte read_byte_callback(byte pn, int pos)
        {
            lock (tmpPcmDataTable)
            {
                if (tmpPcmDataTable.ContainsKey(pn))
                {
                    //HACK: Thread UNSAFE
                    sbyte[] pd = tmpPcmDataTable[pn];
                    if (pd != null && pd.Length != 0 && pos < pd.Length)
                        return pd[pos];
                }
            }
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        private void setPresetInstruments()
        {
            Timbres[0].SoundType = SoundType.INST;
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

        internal override void AllSoundOff()
        {
            soundManager.AllSoundOff();
        }

        /// <summary>
        /// 
        /// </summary>
        private class C140SoundManager : SoundManagerBase
        {
            private SoundList<C140Sound> instOnSounds = new SoundList<C140Sound>(24);

            private C140 parentModule;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            public C140SoundManager(C140 parent) : base(parent)
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
                C140Sound snd = new C140Sound(parentModule, this, timbre, note, emptySlot);
                instOnSounds.Add(snd);

                //HACK: store pcm data to local buffer to avoid "thread lock"
                if (timbre.SoundType == SoundType.INST)
                {
                    lock (parentModule.tmpPcmDataTable)
                        parentModule.tmpPcmDataTable[programNumber] = timbre.PcmData;
                }
                else if (timbre.SoundType == SoundType.DRUM)
                {
                    var pct = (C140PcmTimbre)parentModule.DrumSoundTable.PcmTimbres[note.NoteNumber];
                    lock (parentModule.tmpPcmDataTable)
                        parentModule.tmpPcmDataTable[note.NoteNumber + 128] = pct.C140PcmData;
                }

                FormMain.OutputDebugLog("KeyOn INST ch" + emptySlot + " " + note.ToString());
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

                var programNumber = (SevenBitNumber)parentModule.ProgramNumbers[note.Channel];
                var timbre = parentModule.Timbres[programNumber];
                emptySlot = SearchEmptySlotAndOff(instOnSounds, note, parentModule.CalcMaxVoiceNumber(note.Channel, 24));
                return emptySlot;
            }

            internal override void AllSoundOff()
            {
                var me = new ControlChangeEvent((SevenBitNumber)120, (SevenBitNumber)0);
                ControlChange(me);

                for (int i = 0; i < 24; i++)
                {
                    uint reg = (uint)(i * 16);
                    //mode keyoff(0x00)
                    C140WriteData(parentModule.UnitNumber, (reg + 5), 0x00);
                }
            }

        }


        /// <summary>
        /// 
        /// </summary>
        private class C140Sound : SoundBase
        {

            private C140 parentModule;

            private SevenBitNumber programNumber;

            private C140Timbre timbre;

            private SoundType lastSoundType;

            private double baseFreq;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentModule"></param>
            /// <param name="noteOnEvent"></param>
            /// <param name="programNumber"></param>
            /// <param name="slot"></param>
            public C140Sound(C140 parentModule, C140SoundManager manager, TimbreBase timbre, NoteOnEvent noteOnEvent, int slot) : base(parentModule, manager, timbre, noteOnEvent, slot)
            {
                this.parentModule = parentModule;
                this.programNumber = (SevenBitNumber)parentModule.ProgramNumbers[noteOnEvent.Channel];
                this.timbre = (C140Timbre)timbre;

                lastSoundType = this.timbre.SoundType;
                baseFreq = this.timbre.BaseFreqency;
            }

            /// <summary>
            /// 
            /// </summary>
            public override void OnPanpotUpdated()
            {
                OnVolumeUpdated();
            }

            /// <summary>
            /// 
            /// </summary>
            public override void KeyOn()
            {
                base.KeyOn();

                uint reg = (uint)(Slot * 16);

                OnVolumeUpdated();
                OnPanpotUpdated();
                OnPitchUpdated();

                if (lastSoundType == SoundType.INST)
                {
                    //bankno = prognum
                    C140WriteData(parentModule.UnitNumber, (reg + 4), programNumber);
                    //pcm start
                    C140WriteData(parentModule.UnitNumber, (reg + 6), 0);
                    C140WriteData(parentModule.UnitNumber, (reg + 7), 0);
                    //pcm end
                    ushort len = 0;
                    if (timbre.PcmData.Length > 0)
                        len = (ushort)((timbre.PcmData.Length - 1) & 0xffff);
                    C140WriteData(parentModule.UnitNumber, (reg + 8), (byte)(len >> 8));
                    C140WriteData(parentModule.UnitNumber, (reg + 9), (byte)(len & 0xff));
                    //loop
                    ushort lpos = len;
                    if (timbre.LoopEnable)
                        lpos = (ushort)(timbre.LoopPoint & 0xffff);
                    C140WriteData(parentModule.UnitNumber, (reg + 10), (byte)(lpos >> 8));
                    C140WriteData(parentModule.UnitNumber, (reg + 11), (byte)(lpos & 0xff));
                    //mode keyon(0x80)
                    C140WriteData(parentModule.UnitNumber, (reg + 5), (byte)(0x80 + (timbre.LoopEnable ? 0x10 : 0)));
                }
                else if (lastSoundType == SoundType.DRUM)
                {
                    //bankno = prognum
                    int nn = NoteOnEvent.NoteNumber;
                    C140WriteData(parentModule.UnitNumber, (reg + 4), (byte)(nn + 128));
                    //pcm start
                    C140WriteData(parentModule.UnitNumber, (reg + 6), 0);
                    C140WriteData(parentModule.UnitNumber, (reg + 7), 0);
                    //pcm end
                    var pd = parentModule.DrumSoundTable.PcmTimbres[nn].PcmData;
                    ushort len = 0;
                    if (pd != null && pd.Length > 0)
                        len = (ushort)((pd.Length - 1) & 0xffff);
                    C140WriteData(parentModule.UnitNumber, (reg + 8), (byte)(len >> 8));
                    C140WriteData(parentModule.UnitNumber, (reg + 9), (byte)(len & 0xff));
                    //mode keyon(0x80)
                    C140WriteData(parentModule.UnitNumber, (reg + 5), (byte)(0x80));
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public override void OnVolumeUpdated()
            {
                if (IsSoundOff)
                    return;

                uint reg = (uint)(Slot * 16);
                var vol = CalcCurrentVolume();

                int pan = parentModule.Panpots[NoteOnEvent.Channel] - 1;
                if (pan < 0)
                    pan = 0;

                //byte right = (byte)Math.Round(127d * vol * (pan / 127d));
                //byte left = (byte)Math.Round(127d * vol * ((127d - pan) / 127d));
                byte left = (byte)Math.Round(127d * vol * Math.Cos(Math.PI / 2 * (pan / 126d)));
                byte right = (byte)Math.Round(127d * vol * Math.Sin(Math.PI / 2 * (pan / 126d)));
                C140WriteData(parentModule.UnitNumber, (reg + 0), right);
                C140WriteData(parentModule.UnitNumber, (reg + 1), left);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="slot"></param>
            public override void OnPitchUpdated()
            {
                uint reg = (uint)(Slot * 16);

                uint freq = 0;
                if (lastSoundType == SoundType.INST)
                    freq = (uint)Math.Round((CalcCurrentFrequency() / baseFreq) * 32768);
                else if (lastSoundType == SoundType.DRUM)
                    freq = (uint)Math.Round((1d + CalcCurrentPitch()) * 32768);

                if (freq > 0xffffff)
                    freq = 0xffffff;

                C140WriteData(parentModule.UnitNumber, (reg + 12), (byte)((freq >> 16) & 0xff));  //HACK
                C140WriteData(parentModule.UnitNumber, (reg + 2), (byte)((freq >> 8) & 0xff));
                C140WriteData(parentModule.UnitNumber, (reg + 3), (byte)(freq & 0xff));

                base.OnPitchUpdated();
            }

            /// <summary>
            /// 
            /// </summary>
            public override void SoundOff()
            {
                if (IsSoundOff)
                    return;

                base.SoundOff();

                uint reg = (uint)(Slot * 16);
                //mode keyoff(0x00)
                C140WriteData(parentModule.UnitNumber, (reg + 5), 0x00);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        [JsonConverter(typeof(NoTypeConverterJsonConverter<C140Timbre>))]
        [DataContract]
        public class C140Timbre : TimbreBase
        {
            [DataMember]
            [Category("Sound")]
            [Description("Sound Type")]
            [DefaultValue(SoundType.INST)]
            public SoundType SoundType
            {
                get;
                set;
            }

            [DataMember]
            [Category("Sound")]
            [Description("Set PCM base frequency [Hz]")]
            [DefaultValue(typeof(double), "440")]
            [DoubleSlideParametersAttribute(100, 2000, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public double BaseFreqency
            {
                get;
                set;
            } = 440;


            private bool f_LoopEnable;

            [DataMember]
            [Category("Sound")]
            [Description("Loop point enable")]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue(false)]
            public bool LoopEnable
            {
                get
                {
                    return f_LoopEnable;
                }
                set
                {
                    f_LoopEnable = value;
                }
            }

            [DataMember]
            [Category("Sound")]
            [Description("Set loop point (0 - 65535")]
            [DefaultValue(typeof(ushort), "0")]
            [SlideParametersAttribute(0, 65535)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public ushort LoopPoint
            {
                get;
                set;
            }

            private sbyte[] f_PcmData = new sbyte[0];

            [TypeConverter(typeof(TypeConverter))]
            [Editor(typeof(PcmFileLoaderUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DataMember]
            [Category("Sound")]
            [Description("Signed 8bit PCM Raw Data or WAV Data. (MAX 64KB, 1ch)")]
            [PcmFileLoaderEditor("Audio File(*.raw, *.wav)|*.raw;*.wav", 0, 8, 1, 65535)]
            public sbyte[] PcmData
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
                PcmData = new sbyte[0];
            }

            /// <summary>
            /// 
            /// </summary>
            public C140Timbre()
            {
                SDS.FxS = new BasicFxSettings();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="serializeData"></param>
            public override void RestoreFrom(string serializeData)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<C140Timbre>(serializeData);
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
            INST,
            DRUM,
        }


        /// <summary>
        /// 
        /// </summary>
        [DataContract]
        public class C140PcmSoundTable : PcmTimbreTableBase
        {

            /// <summary>
            /// 
            /// </summary>
            public C140PcmSoundTable()
            {
                for (int i = 0; i < 128; i++)
                {
                    var pt = new C140PcmTimbre(i);
                    PcmTimbres[i] = pt;
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        [DataContract]
        public class C140PcmTimbre : PcmTimbreBase
        {

            private byte[] f_PcmData;

            /// <summary>
            /// 
            /// </summary>
            [DataMember]
            [Browsable(false)]
            public override byte[] PcmData
            {
                get
                {
                    return f_PcmData;
                }
                set
                {
                    if (value != null)
                    {
                        if (value[0] == 'R' && value[1] == 'I' && value[2] == 'F' && value[3] == 'F')
                        {
                            var head = WaveFileReader.ReadWaveData(value);

                            if (8 != head.BitPerSample || 1 != head.Channel)
                            {
                                throw new ArgumentOutOfRangeException(
                                    string.Format($"Incorrect wave format(Expected Ch=1 Bit=8)"));
                            }

                            List<byte> al = new List<byte>(head.Data);
                            //Max 64k
                            if (al.Count > 65535)
                                al.RemoveRange(65535, al.Count - 65535);

                            f_PcmData = al.ToArray();

                            sbyte[] sbuf = new sbyte[f_PcmData.Length];
                            for (int i = 0; i < f_PcmData.Length; i++)
                                sbuf[i] = (sbyte)(f_PcmData[i] - 0x80);
                            f_C140PcmData = sbuf;
                        }
                        else
                        {
                            f_PcmData = value;
                            sbyte[] sbuf = new sbyte[f_PcmData.Length];
                            for (int i = 0; i < f_PcmData.Length; i++)
                                sbuf[i] = (sbyte)(f_PcmData[i] - 0x80);
                            f_C140PcmData = sbuf;
                        }
                    }
                }
            }

            private sbyte[] f_C140PcmData = new sbyte[0];

            [Browsable(false)]
            public sbyte[] C140PcmData
            {
                get
                {
                    return f_C140PcmData;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="noteNumber"></param>
            public C140PcmTimbre(int noteNumber) : base(noteNumber)
            {
            }
        }
    }


    public enum C140Clock
    {
        Clk_21333Hz = 21333, //49152000 / 384 / 6,
        Clk_22050Hz = 22050,
        Clk_44100Hz = 44100,
        Clk_48000Hz = 48000,
    }
}
