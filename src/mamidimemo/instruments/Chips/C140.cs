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
using System.Windows.Forms;
using Kermalis.SoundFont2;
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
    [InstLock]
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

        /*

        [DataMember]
        [Category("Chip")]
        [Description("Assign PCM data to DRUM soundtype instrument.\r\n" +
            "Signed 8bit PCM Raw Data or WAV Data. (MAX 64KB, 1ch)")]
        [Editor(typeof(PcmTableUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [PcmTableEditor("Audio File(*.raw, *.wav)|*.raw;*.wav")]
        [TypeConverter(typeof(CustomObjectTypeConverter))]
        public C140PcmSoundTable DrumSoundTable
        {
            get;
            set;
        }
        */

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
        [Category(" Timbres")]
        [Description("Timbres")]
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
            DeferredWriteData(c140_w, unitNumber, address, data);
            /*
            try
            {
                Program.SoundUpdating();
                c140_w(unitNumber, address, data);
            }
            finally
            {
                Program.SoundUpdated();
            }*/
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
            DeferredWriteData(set_callback, unitNumber, callback);
            /*
            try
            {
                Program.SoundUpdating();
                set_callback(unitNumber, callback);
            }
            finally
            {
                Program.SoundUpdated();
            }*/
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
            Timbres = new C140Timbre[256];
            for (int i = 0; i < 256; i++)
                Timbres[i] = new C140Timbre();

            //DrumSoundTable = new C140PcmSoundTable();

            setPresetInstruments();

            this.soundManager = new C140SoundManager(this);

            f_read_byte_callback = new delg_callback(read_byte_callback);
            C140SetCallback(UnitNumber, f_read_byte_callback);

            GainLeft = DEFAULT_GAIN;
            GainRight = DEFAULT_GAIN;

            readSoundFontForTimbre = new ToolStripMenuItem("Import PCM from SF2 for &Timbre...");
            readSoundFontForTimbre.Click += ReadSoundFontForTimbre_Click;

            readSoundFontForDrumTimbre = new ToolStripMenuItem("Import PCM from SF2 for &DrumTimbre...");
            readSoundFontForDrumTimbre.Click += ReadSoundFontForDrumTimbre_Click;
        }

        #region IDisposable Support

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

                    readSoundFontForTimbre?.Dispose();
                    readSoundFontForTimbre = null;

                    readSoundFontForDrumTimbre?.Dispose();
                    readSoundFontForDrumTimbre = null;
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
        private class C140SoundManager : SoundManagerBase
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

            private static SoundList<C140Sound> instOnSounds = new SoundList<C140Sound>(24);

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
            public override SoundBase[] SoundOn(TaggedNoteOnEvent note)
            {
                List<SoundBase> rv = new List<SoundBase>();

                var bts = parentModule.GetBaseTimbres(note);
                var ids = parentModule.GetBaseTimbreIndexes(note);
                int tindex = 0;
                for (int i = 0; i < bts.Length; i++)
                {
                    C140Timbre timbre = (C140Timbre)bts[i];

                    tindex++;
                    var emptySlot = searchEmptySlot(note);
                    if (emptySlot.slot < 0)
                        continue;

                    C140Sound snd = new C140Sound(emptySlot.inst, this, timbre, tindex - 1, note, emptySlot.slot, (byte)ids[i]);
                    instOnSounds.Add(snd);

                    //HACK: store pcm data to local buffer to avoid "thread lock"
                    if (timbre.SoundType == SoundType.INST)
                    {
                        lock (parentModule.tmpPcmDataTable)
                            parentModule.tmpPcmDataTable[ids[i]] = timbre.PcmData;
                    }
                    /*
                    else if (timbre.SoundType == SoundType.DRUM)
                    {
                        var pct = (C140PcmTimbre)parentModule.DrumSoundTable.PcmTimbres[note.NoteNumber];
                        lock (parentModule.tmpPcmDataTable)
                            parentModule.tmpPcmDataTable[note.NoteNumber + 128] = pct.C140PcmData;
                    }
                    */

                    FormMain.OutputDebugLog(parentModule, "KeyOn INST ch" + emptySlot + " " + note.ToString());
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
            private (C140 inst, int slot) searchEmptySlot(TaggedNoteOnEvent note)
            {
                return SearchEmptySlotAndOffForLeader(parentModule, instOnSounds, note, 24);
            }

            internal override void ProcessAllSoundOff()
            {
                var me = new ControlChangeEvent((SevenBitNumber)120, (SevenBitNumber)0);
                ProcessControlChange(me);

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

            private byte timbreIndex;

            private C140Timbre timbre;

            private SoundType lastSoundType;

            private double baseFreq;

            private uint sampleRate;

            private ushort loopPoint;

            private bool loopEn;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentModule"></param>
            /// <param name="noteOnEvent"></param>
            /// <param name="programNumber"></param>
            /// <param name="slot"></param>
            public C140Sound(C140 parentModule, C140SoundManager manager, TimbreBase timbre, int tindex, TaggedNoteOnEvent noteOnEvent, int slot, byte timbreIndex) : base(parentModule, manager, timbre, tindex, noteOnEvent, slot)
            {
                this.parentModule = parentModule;
                this.timbreIndex = timbreIndex;
                this.timbre = (C140Timbre)timbre;

                lastSoundType = this.timbre.SoundType;
                if (lastSoundType == SoundType.INST)
                {
                    baseFreq = this.timbre.BaseFreqency;
                    sampleRate = this.timbre.SampleRate;
                    loopPoint = this.timbre.LoopPoint;
                    loopEn = this.timbre.LoopEnable;
                }
                /*
                else if (lastSoundType == SoundType.DRUM)
                {
                    var pct = (C140PcmTimbre)parentModule.DrumSoundTable.PcmTimbres[noteOnEvent.NoteNumber];
                    baseFreq = pct.BaseFreqency;
                    sampleRate = pct.SampleRate;
                    loopPoint = pct.LoopPoint;
                    loopEn = pct.LoopEnable;
                }*/
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
                    C140WriteData(parentModule.UnitNumber, (reg + 4), timbreIndex);
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
                    if (loopEn)
                        lpos = (ushort)(loopPoint & 0xffff);
                    C140WriteData(parentModule.UnitNumber, (reg + 10), (byte)(lpos >> 8));
                    C140WriteData(parentModule.UnitNumber, (reg + 11), (byte)(lpos & 0xff));
                    //mode keyon(0x80)
                    C140WriteData(parentModule.UnitNumber, (reg + 5), (byte)(0x80 + (timbre.LoopEnable ? 0x10 : 0)));
                }
                /*
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
                    //loop
                    ushort lpos = len;
                    if (loopEn)
                        lpos = (ushort)(loopPoint & 0xffff);
                    C140WriteData(parentModule.UnitNumber, (reg + 10), (byte)(lpos >> 8));
                    C140WriteData(parentModule.UnitNumber, (reg + 11), (byte)(lpos & 0xff));
                    //mode keyon(0x80)
                    C140WriteData(parentModule.UnitNumber, (reg + 5), (byte)(0x80 + (timbre.LoopEnable ? 0x10 : 0)));
                }*/
            }

            /// <summary>
            /// 
            /// </summary>
            public override void OnVolumeUpdated()
            {
                uint reg = (uint)(Slot * 16);
                var vol = CalcCurrentVolume();

                int pan = CalcCurrentPanpot();
                /*
                if (lastSoundType == SoundType.DRUM)
                {
                    var pct = parentModule.DrumSoundTable.PcmTimbres[NoteOnEvent.NoteNumber];
                    pan += pct.PanShift;
                    if (pan < 0)
                        pan = 0;
                    else if (pan > 127)
                        pan = 127;
                }*/

                byte left = (byte)Math.Round(127d * vol * Math.Cos(Math.PI / 2 * (pan / 127d)));
                byte right = (byte)Math.Round(127d * vol * Math.Sin(Math.PI / 2 * (pan / 127d)));
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
                {
                    freq = (uint)Math.Round((CalcCurrentFrequency() / baseFreq) * 32768 * sampleRate / (double)parentModule.Clock);
                }
                /*
                else if (lastSoundType == SoundType.DRUM)
                {
                    double f = MidiManager.CalcCurrentFrequency
                        (MidiManager.CalcNoteNumberFromFrequency(baseFreq) + CalcCurrentPitchDeltaNoteNumber());

                    freq = (uint)Math.Round((f / baseFreq) * 32768 * sampleRate / (double)parentModule.Clock);
                }
                */

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
        [InstLock]
        public class C140Timbre : TimbreBase
        {
            [DataMember]
            [Category("Sound")]
            [Description("Sound Type")]
            [DefaultValue(SoundType.INST)]
            [Browsable(false)]
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
            [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public double BaseFreqency
            {
                get;
                set;
            } = 440;

            [DataMember]
            [Category("Sound")]
            [Description("Set PCM samplerate [Hz]")]
            [DefaultValue(typeof(uint), "22050")]
            [SlideParametersAttribute(4000, 96000)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public uint SampleRate
            {
                get;
                set;
            } = 22050;

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
        [InstLock]
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
        [InstLock]
        public class C140PcmTimbre : PcmTimbreBase
        {

            [DataMember]
            [Category("Sound")]
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
            [Category("Sound")]
            [Description("Set PCM samplerate [Hz]")]
            [DefaultValue(typeof(uint), "22050")]
            [SlideParametersAttribute(4000, 96000)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public uint SampleRate
            {
                get;
                set;
            } = 22050;

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

        #region MENU

        private ToolStripMenuItem readSoundFontForTimbre;

        private ToolStripMenuItem readSoundFontForDrumTimbre;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal override IEnumerable<ToolStripMenuItem> GetInstrumentMenus()
        {
            return new ToolStripMenuItem[] { readSoundFontForTimbre, readSoundFontForDrumTimbre };
        }

        private System.Windows.Forms.OpenFileDialog openFileDialog;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReadSoundFontForTimbre_Click(object sender, EventArgs e)
        {
            try
            {
                int offset = 0;
                using (openFileDialog = new System.Windows.Forms.OpenFileDialog())
                {
                    openFileDialog.SupportMultiDottedExtensions = true;
                    openFileDialog.Title = "Select a SoundFont v2.0 file";
                    openFileDialog.Filter = "SoundFont v2.0 File(*.sf2)|*.sf2";

                    var fr = openFileDialog.ShowDialog(null);
                    if (fr != DialogResult.OK)
                        return;

                    loadPcm(offset, false);
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(Exception))
                    throw;
                else if (ex.GetType() == typeof(SystemException))
                    throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReadSoundFontForDrumTimbre_Click(object sender, EventArgs e)
        {
            try
            {
                int offset = 128;
                using (openFileDialog = new System.Windows.Forms.OpenFileDialog())
                {
                    openFileDialog.SupportMultiDottedExtensions = true;
                    openFileDialog.Title = "Select a SoundFont v2.0 file";
                    openFileDialog.Filter = "SoundFont v2.0 File(*.sf2)|*.sf2";

                    var fr = openFileDialog.ShowDialog(null);
                    if (fr != DialogResult.OK)
                        return;

                    loadPcm(offset, true);
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(Exception))
                    throw;
                else if (ex.GetType() == typeof(SystemException))
                    throw;
            }

            /*
            try
            {
                using (openFileDialog = new System.Windows.Forms.OpenFileDialog())
                {
                    openFileDialog.SupportMultiDottedExtensions = true;
                    openFileDialog.Title = "Select a SoundFont v2.0 file";
                    openFileDialog.Filter = "SoundFont v2.0 File(*.sf2)|*.sf2";

                    var fr = openFileDialog.ShowDialog(null);
                    if (fr != DialogResult.OK)
                        return;

                    var sf2 = new SF2(openFileDialog.FileName);

                    var spl = sf2.SoundChunk.SMPLSubChunk.Samples;
                    int tn = 0;
                    foreach (var s in sf2.HydraChunk.SHDRSubChunk.Samples)
                    {
                        if (s.SampleType == SF2SampleLink.MonoSample ||
                            s.SampleType == SF2SampleLink.LeftSample)
                        {
                            var tim = (C140PcmTimbre)DrumSoundTable.PcmTimbres[tn];

                            double baseFreq = 440.0 * Math.Pow(2.0, ((double)s.OriginalKey - 69.0) / 12.0);
                            tim.BaseFreqency = baseFreq;
                            tim.SampleRate = s.SampleRate;

                            uint start = s.Start;
                            uint end = s.End;
                            if (s.LoopEnd < end && s.LoopStart < s.LoopEnd)
                                end = s.LoopEnd;

                            uint len = end - start + 1;
                            if (len > 65535)
                                len = 65535;
                            uint loopP = s.LoopStart - s.Start;
                            if (loopP > 65535)
                                loopP = 65535;

                            byte[] samples = new byte[len];
                            for (uint i = 0; i < len; i++)
                                samples[i] = (byte)((spl[start + i] >> 8) + 128);

                            tim.PcmData = samples;
                            tim.LoopPoint = (ushort)loopP;
                            tim.LoopEnable = s.LoopStart < s.LoopEnd;
                            var nidx = s.SampleName.IndexOf('\0');
                            if (nidx >= 0)
                                tim.TimbreName = s.SampleName.Substring(0, nidx);
                            else
                                tim.TimbreName = s.SampleName;

                            tn++;

                            if (tn == 128)
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(Exception))
                    throw;
                else if (ex.GetType() == typeof(SystemException))
                    throw;
            }*/
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="offset"></param>
        private void loadPcm(int offset, bool drum)
        {
            var sf2 = new SF2(openFileDialog.FileName);

            var spl = sf2.SoundChunk.SMPLSubChunk.Samples;
            int tn = 0;
            int num = 0;
            foreach (var s in sf2.HydraChunk.SHDRSubChunk.Samples)
            {
                if (s.SampleType == SF2SampleLink.MonoSample ||
                    s.SampleType == SF2SampleLink.LeftSample)
                {
                    var tim = new C140Timbre();

                    double baseFreq = 440.0 * Math.Pow(2.0, ((double)s.OriginalKey - 69.0) / 12.0);
                    tim.BaseFreqency = baseFreq;
                    tim.SampleRate = s.SampleRate;

                    uint start = s.Start;
                    uint end = s.End;
                    if (s.LoopEnd < end && s.LoopStart < s.LoopEnd)
                        end = s.LoopEnd;

                    uint len = end - start + 1;
                    if (len > 65535)
                        len = 65535;
                    uint loopP = s.LoopStart - s.Start;
                    if (loopP > 65535)
                        loopP = 65535;

                    sbyte[] samples = new sbyte[len];
                    for (uint i = 0; i < len; i++)
                        samples[i] = (sbyte)(spl[start + i] >> 8);

                    tim.PcmData = samples;
                    tim.LoopPoint = (ushort)loopP;
                    tim.LoopEnable = s.LoopStart < s.LoopEnd;

                    if (s.LoopStart < s.LoopEnd)
                    {
                        tim.SDS.ADSR.Enable = true;
                        tim.SDS.ADSR.DR = 80;
                        tim.SDS.ADSR.SL = 127;
                    }
                    if (drum)
                    {
                        DrumTimbres[tn].TimbreNumber = (ProgramAssignmentNumber)(tn + offset);
                        DrumTimbres[tn].BaseNote =
                            (NoteNames)(byte)Math.Round(MidiManager.CalcNoteNumberFromFrequency(tim.BaseFreqency));
                    }

                    Timbres[tn + offset] = tim;
                    num++;

                    var nidx = s.SampleName.IndexOf('\0');
                    if (nidx >= 0)
                        tim.TimbreName = s.SampleName.Substring(0, nidx);
                    else
                        tim.TimbreName = s.SampleName;

                    tn++;
                    if (tn == 128)
                        break;
                }
            }
        }

        #endregion

    }


    public enum C140Clock
    {
        Clk_21333Hz = 21333, //49152000 / 384 / 6,
        Clk_22050Hz = 22050,
        Clk_44100Hz = 44100,
        Clk_48000Hz = 48000,
    }
}
