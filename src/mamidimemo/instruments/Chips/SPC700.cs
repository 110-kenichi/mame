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

//https://wiki.superfamicom.org/spc700-reference

namespace zanac.MAmidiMEmo.Instruments.Chips
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class SPC700 : InstrumentBase
    {
        public override string Name => "SPC700";

        public override string Group => "PCM";

        public override InstrumentType InstrumentType => InstrumentType.SPC700;

        [Browsable(false)]
        public override string ImageKey => "SPC700";

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        protected override string SoundInterfaceTagNamePrefix => "snes_sound_";

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
                return 17;
            }
        }

        private byte f_LMVOL;

        [DataMember]
        [Category("Chip")]
        [Description("Set Left Output Main Volume")]
        [DefaultValue((byte)127)]
        [SlideParametersAttribute(0, 255)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public byte LMVOL
        {
            get
            {
                return f_LMVOL;
            }
            set
            {
                if (f_LMVOL != value)
                {
                    f_LMVOL = value;

                    SPC700RegWriteData(UnitNumber, 0xc, value);
                }
            }
        }

        private byte f_RMVOL;

        [DataMember]
        [Category("Chip")]
        [Description("Set Right Output Main Volume")]
        [DefaultValue((byte)127)]
        [SlideParametersAttribute(0, 255)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public byte RMVOL
        {
            get
            {
                return f_RMVOL;
            }
            set
            {
                if (f_RMVOL != value)
                {
                    f_RMVOL = value;

                    SPC700RegWriteData(UnitNumber, 0x1c, value);
                }
            }
        }

        private sbyte f_LEVOL;

        [DataMember]
        [Category("Filter")]
        [Description("Set Left Output Echo Volume")]
        [DefaultValue(typeof(sbyte), "127")]
        [SlideParametersAttribute(-128, 128)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public sbyte LEVOL
        {
            get
            {
                return f_LEVOL;
            }
            set
            {
                if (f_LEVOL != value)
                {
                    f_LEVOL = value;

                    SPC700RegWriteData(UnitNumber, 0x2c, (byte)value);
                }
            }
        }

        private sbyte f_REVOL;

        [DataMember]
        [Category("Filter")]
        [Description("Set Right Output Echo Volume")]
        [DefaultValue(typeof(sbyte), "127")]
        [SlideParametersAttribute(-128, 128)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public sbyte REVOL
        {
            get
            {
                return f_REVOL;
            }
            set
            {
                if (f_REVOL != value)
                {
                    f_REVOL = value;

                    SPC700RegWriteData(UnitNumber, 0x3c, (byte)value);
                }
            }
        }

        private byte f_NOISE_CLOCK;

        [DataMember]
        [Category("Chip")]
        [Description("Designates the frequency for the white noise.")]
        [DefaultValue((byte)0)]
        [SlideParametersAttribute(0, 31)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public byte NOISE_CLOCK
        {
            get
            {
                return f_NOISE_CLOCK;
            }
            set
            {
                if (f_NOISE_CLOCK != value)
                {
                    f_NOISE_CLOCK = (byte)(value & 31);

                    SPC700RegWriteData(UnitNumber, (byte)0x6c, (byte)(((~ECEN & 1) << 5) | f_NOISE_CLOCK));
                }
            }
        }

        private byte f_ECEN;

        [DataMember]
        [Category("Filter")]
        [Description("Echo enable")]
        [DefaultValue((byte)1)]
        [SlideParametersAttribute(0, 1)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public byte ECEN
        {
            get
            {
                return f_ECEN;
            }
            set
            {
                if (f_ECEN != value)
                {
                    f_ECEN = (byte)(value & 1);

                    SPC700RegWriteData(UnitNumber, 0x6c, (byte)(((~f_ECEN & 1) << 5) | NOISE_CLOCK));
                }
            }
        }


        private sbyte f_EFB;

        [DataMember]
        [Category("Filter")]
        [Description("Echo Feedback")]
        [DefaultValue(typeof(sbyte), "31")]
        [SlideParametersAttribute(-128, 127)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public sbyte EFB
        {
            get
            {
                return f_EFB;
            }
            set
            {
                if (f_EFB != value)
                {
                    f_EFB = value;

                    SPC700RegWriteData(UnitNumber, (byte)0x0d, (byte)f_EFB);
                }
            }
        }

        private byte f_EDL;

        [DataMember]
        [Category("Filter")]
        [Description("EDL specifies the delay between the main sound and the echoed sound. The delay is calculated as EDL * 16ms.")]
        [DefaultValue((byte)1)]
        [SlideParametersAttribute(0, 15)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public byte EDL
        {
            get
            {
                return f_EDL;
            }
            set
            {
                if (f_EDL != value)
                {
                    f_EDL = (byte)(value & 15);

                    SPC700RegWriteData(UnitNumber, (byte)0x7d, f_EDL);
                }
            }
        }

        private sbyte f_COEF1;

        [DataMember]
        [Category("Filter")]
        [Description("COEF are used by the 8-tap FIR filter.")]
        [DefaultValue(typeof(sbyte), "127")]
        [SlideParametersAttribute(-128, 127)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public sbyte COEF1
        {
            get
            {
                return f_COEF1;
            }
            set
            {
                if (f_COEF1 != value)
                {
                    f_COEF1 = value;

                    SPC700RegWriteData(UnitNumber, (byte)0x0f, (byte)f_COEF1);
                }
            }
        }

        private sbyte f_COEF2;

        [DataMember]
        [Category("Filter")]
        [Description("COEF are used by the 8-tap FIR filter.")]
        [DefaultValue(typeof(sbyte), "0")]
        [SlideParametersAttribute(-128, 127)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public sbyte COEF2
        {
            get
            {
                return f_COEF2;
            }
            set
            {
                if (f_COEF2 != value)
                {
                    f_COEF2 = value;

                    SPC700RegWriteData(UnitNumber, (byte)0x1f, (byte)f_COEF2);
                }
            }
        }

        private sbyte f_COEF3;

        [DataMember]
        [Category("Filter")]
        [Description("COEF are used by the 8-tap FIR filter.")]
        [DefaultValue(typeof(sbyte), "0")]
        [SlideParametersAttribute(-128, 127)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public sbyte COEF3
        {
            get
            {
                return f_COEF3;
            }
            set
            {
                if (f_COEF3 != value)
                {
                    f_COEF3 = value;

                    SPC700RegWriteData(UnitNumber, (byte)0x2f, (byte)f_COEF3);
                }
            }
        }

        private sbyte f_COEF4;

        [DataMember]
        [Category("Filter")]
        [Description("COEF are used by the 8-tap FIR filter.")]
        [DefaultValue(typeof(sbyte), "0")]
        [SlideParametersAttribute(-128, 127)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public sbyte COEF4
        {
            get
            {
                return f_COEF4;
            }
            set
            {
                if (f_COEF4 != value)
                {
                    f_COEF4 = value;

                    SPC700RegWriteData(UnitNumber, (byte)0x3f, (byte)f_COEF4);
                }
            }
        }

        private sbyte f_COEF5;

        [DataMember]
        [Category("Filter")]
        [Description("COEF are used by the 8-tap FIR filter.")]
        [DefaultValue(typeof(sbyte), "0")]
        [SlideParametersAttribute(-128, 127)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public sbyte COEF5
        {
            get
            {
                return f_COEF5;
            }
            set
            {
                if (f_COEF5 != value)
                {
                    f_COEF5 = value;

                    SPC700RegWriteData(UnitNumber, (byte)0x4f, (byte)f_COEF5);
                }
            }
        }


        private sbyte f_COEF6;

        [DataMember]
        [Category("Filter")]
        [Description("COEF are used by the 8-tap FIR filter.")]
        [DefaultValue(typeof(sbyte), "0")]
        [SlideParametersAttribute(-128, 127)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public sbyte COEF6
        {
            get
            {
                return f_COEF6;
            }
            set
            {
                if (f_COEF6 != value)
                {
                    f_COEF6 = value;

                    SPC700RegWriteData(UnitNumber, (byte)0x5f, (byte)f_COEF6);
                }
            }
        }

        private sbyte f_COEF7;

        [DataMember]
        [Category("Filter")]
        [Description("COEF are used by the 8-tap FIR filter.")]
        [DefaultValue(typeof(sbyte), "0")]
        [SlideParametersAttribute(-128, 127)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public sbyte COEF7
        {
            get
            {
                return f_COEF7;
            }
            set
            {
                if (f_COEF7 != value)
                {
                    f_COEF7 = value;

                    SPC700RegWriteData(UnitNumber, (byte)0x6f, (byte)f_COEF7);
                }
            }
        }

        private sbyte f_COEF8;

        [DataMember]
        [Category("Filter")]
        [Description("COEF are used by the 8-tap FIR filter.")]
        [DefaultValue(typeof(sbyte), "0")]
        [SlideParametersAttribute(-128, 127)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public sbyte COEF8
        {
            get
            {
                return f_COEF8;
            }
            set
            {
                if (f_COEF8 != value)
                {
                    f_COEF8 = value;

                    SPC700RegWriteData(UnitNumber, (byte)0x7f, (byte)f_COEF8);
                }
            }
        }

        [DataMember]
        [Category("Chip")]
        [Description("BRR ADPCM Data (MAX 64KB)")]
        [Editor(typeof(PcmTableUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [PcmTableEditor("Audio File(*.brr)|*.brr")]
        //[PcmTableEditor("Audio File(*.brr, *.wav)|*.brr;*.wav")]
        [TypeConverter(typeof(CustomObjectTypeConverter))]
        public SPC700PcmSoundTable DrumSoundTable
        {
            get;
            private set;
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

        private SPC700Timbre[] f_Timbres;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("Chip")]
        [Description("Timbres (0-127)")]
        [EditorAttribute(typeof(DummyEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(ExpandableCollectionConverter))]
        public SPC700Timbre[] Timbres
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
        /// <returns></returns>
        public override TimbreBase GetTimbre(int channel)
        {
            var pn = (SevenBitNumber)ProgramNumbers[channel];
            return Timbres[pn];
        }


        private const float DEFAULT_GAIN = 1.0f;

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
                using (var obj = JsonConvert.DeserializeObject<SPC700>(serializeData))
                    this.InjectFrom(new LoopInjection(new[] { "SerializeData" }), obj);
                SPC700SetCallback(UnitNumber, f_read_byte_callback);
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
        private delegate void delegate_spc_ram_w(uint unitNumber, uint address, byte data);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate byte delegate_spc_ram_r(uint unitNumber, uint address);

        /// <summary>
        /// 
        /// </summary>
        private static void SPC700RamWriteData(uint unitNumber, uint address, byte data)
        {
            try
            {
                Program.SoundUpdating();
                spc_ram_w(unitNumber, address, data);
            }
            finally
            {
                Program.SoundUpdated();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private static void SPC700RegWriteData(uint unitNumber, byte reg, byte data)
        {
            try
            {
                Program.SoundUpdating();
                spc_ram_w(unitNumber, 0xf2, reg);
                spc_ram_w(unitNumber, 0xf3, data);
            }
            finally
            {
                Program.SoundUpdated();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private static byte SPC700RegReadData(uint unitNumber, byte reg)
        {
            try
            {
                Program.SoundUpdating();
                spc_ram_w(unitNumber, 0xf2, reg);
                return spc_ram_r(unitNumber, 0xf3);
            }
            finally
            {
                Program.SoundUpdated();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private static byte SPC700RamReadData(uint unitNumber, uint address)
        {
            try
            {
                Program.SoundUpdating();
                return spc_ram_r(unitNumber, address);
            }
            finally
            {
                Program.SoundUpdated();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private static delegate_spc_ram_w spc_ram_w
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        private static delegate_spc_ram_r spc_ram_r
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
        private delegate byte delg_callback(byte pn, int pos);

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
        private static void SPC700SetCallback(uint unitNumber, delg_callback callback)
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

        private Dictionary<int, byte[]> tmpPcmDataTable = new Dictionary<int, byte[]>();

        /// <summary>
        /// 
        /// </summary>
        static SPC700()
        {
            IntPtr funcPtr = MameIF.GetProcAddress("spc_ram_w");
            if (funcPtr != IntPtr.Zero)
                spc_ram_w = (delegate_spc_ram_w)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_spc_ram_w));

            funcPtr = MameIF.GetProcAddress("spc_ram_r");
            if (funcPtr != IntPtr.Zero)
                spc_ram_r = (delegate_spc_ram_r)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_spc_ram_r));

            funcPtr = MameIF.GetProcAddress("spc700_set_callback");
            if (funcPtr != IntPtr.Zero)
                set_callback = (delegate_set_callback)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_set_callback));
        }

        private SPC700SoundManager soundManager;

        private delg_callback f_read_byte_callback;

        /// <summary>
        /// 
        /// </summary>
        public SPC700(uint unitNumber) : base(unitNumber)
        {
            Timbres = new SPC700Timbre[128];
            for (int i = 0; i < 128; i++)
                Timbres[i] = new SPC700Timbre();

            DrumSoundTable = new SPC700PcmSoundTable();

            setPresetInstruments();

            this.soundManager = new SPC700SoundManager(this);

            f_read_byte_callback = new delg_callback(read_byte_callback);
            SPC700SetCallback(UnitNumber, f_read_byte_callback);

            GainLeft = DEFAULT_GAIN;
            GainRight = DEFAULT_GAIN;

            //DIR 0x200 - 0x5ff
            SPC700RegWriteData(UnitNumber, (byte)0x5d, (byte)2);
            //ESA 0x600 - (0x7B00-1)
            SPC700RegWriteData(UnitNumber, (byte)0x6d, (byte)6);

            EDL = 1;
            EFB = 31;
            NOISE_CLOCK = 0;
            ECEN = 1;
            LEVOL = 127;
            REVOL = 127;
            LMVOL = 127;
            RMVOL = 127;
            COEF1 = 127;
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
                //SPC700SetCallback(UnitNumber, null);

                disposedValue = true;
            }
        }

        // TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
        ~SPC700()
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
        private byte read_byte_callback(byte pn, int pos)
        {
            lock (tmpPcmDataTable)
            {
                if (tmpPcmDataTable.ContainsKey(pn))
                {
                    //HACK: Thread UNSAFE
                    byte[] pd = tmpPcmDataTable[pn];
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
        private class SPC700SoundManager : SoundManagerBase
        {
            private SoundList<SPC700Sound> instOnSounds = new SoundList<SPC700Sound>(8);

            private SPC700 parentModule;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            public SPC700SoundManager(SPC700 parent) : base(parent)
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
                SPC700Sound snd = new SPC700Sound(parentModule, this, timbre, note, emptySlot);
                instOnSounds.Add(snd);

                //HACK: store pcm data to local buffer to avoid "thread lock"
                if (timbre.SoundType == SoundType.INST)
                {
                    lock (parentModule.tmpPcmDataTable)
                        parentModule.tmpPcmDataTable[programNumber] = timbre.AdpcmData;
                }
                else if (timbre.SoundType == SoundType.DRUM)
                {
                    var pct = (SPC700PcmTimbre)parentModule.DrumSoundTable.PcmTimbres[note.NoteNumber];
                    lock (parentModule.tmpPcmDataTable)
                        parentModule.tmpPcmDataTable[note.NoteNumber + 128] = pct.PcmData;
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
                emptySlot = SearchEmptySlotAndOff(instOnSounds, note, parentModule.CalcMaxVoiceNumber(note.Channel, 8));
                return emptySlot;
            }

            internal override void AllSoundOff()
            {
                var me = new ControlChangeEvent((SevenBitNumber)120, (SevenBitNumber)0);
                ControlChange(me);

                for (int i = 0; i < 8; i++)
                {
                    byte bitPos = (byte)(1 << i);

                    byte kon = (byte)(SPC700RegReadData(parentModule.UnitNumber, 0x4c) & ~bitPos);
                    SPC700RegWriteData(parentModule.UnitNumber, 0x4c, kon);
                    byte koff = (byte)(SPC700RegReadData(parentModule.UnitNumber, 0x5c) | bitPos);
                    SPC700RegWriteData(parentModule.UnitNumber, 0x5c, koff);
                }
            }

        }


        /// <summary>
        /// 
        /// </summary>
        private class SPC700Sound : SoundBase
        {

            private SPC700 parentModule;

            private SevenBitNumber programNumber;

            private SPC700Timbre timbre;

            private SoundType lastSoundType;

            private double baseFreq;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentModule"></param>
            /// <param name="noteOnEvent"></param>
            /// <param name="programNumber"></param>
            /// <param name="slot"></param>
            public SPC700Sound(SPC700 parentModule, SPC700SoundManager manager, TimbreBase timbre, NoteOnEvent noteOnEvent, int slot) : base(parentModule, manager, timbre, noteOnEvent, slot)
            {
                this.parentModule = parentModule;
                this.programNumber = (SevenBitNumber)parentModule.ProgramNumbers[noteOnEvent.Channel];
                this.timbre = (SPC700Timbre)timbre;

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
                byte bitPos = (byte)(1 << Slot);

                Program.SoundUpdating();

                var gs = timbre.GlobalSettings;
                if (gs.Enable)
                {
                    if (gs.LMVOL.HasValue)
                        parentModule.LMVOL = gs.LMVOL.Value;
                    if (gs.RMVOL.HasValue)
                        parentModule.RMVOL = gs.RMVOL.Value;
                    if (gs.LEVOL.HasValue)
                        parentModule.LEVOL = gs.LEVOL.Value;
                    if (gs.REVOL.HasValue)
                        parentModule.REVOL = gs.REVOL.Value;
                    if (gs.NOISE_CLOCK.HasValue)
                        parentModule.NOISE_CLOCK = gs.NOISE_CLOCK.Value;
                    if (gs.ECEN.HasValue)
                        parentModule.ECEN = gs.ECEN.Value;
                    if (gs.EFB.HasValue)
                        parentModule.EFB = gs.EFB.Value;
                    if (gs.EDL.HasValue)
                        parentModule.EDL = gs.EDL.Value;
                    if (gs.COEF1.HasValue)
                        parentModule.COEF1 = gs.COEF1.Value;
                    if (gs.COEF2.HasValue)
                        parentModule.COEF2 = gs.COEF2.Value;
                    if (gs.COEF3.HasValue)
                        parentModule.COEF3 = gs.COEF3.Value;
                    if (gs.COEF4.HasValue)
                        parentModule.COEF4 = gs.COEF4.Value;
                    if (gs.COEF5.HasValue)
                        parentModule.COEF5 = gs.COEF5.Value;
                    if (gs.COEF6.HasValue)
                        parentModule.COEF6 = gs.COEF6.Value;
                    if (gs.COEF7.HasValue)
                        parentModule.COEF7 = gs.COEF7.Value;
                    if (gs.COEF8.HasValue)
                        parentModule.COEF8 = gs.COEF8.Value;
                }

                OnVolumeUpdated();
                OnPanpotUpdated();
                OnPitchUpdated();

                if (lastSoundType == SoundType.INST)
                {
                    //prognum no
                    SPC700RegWriteData(parentModule.UnitNumber, (byte)(reg + 4), programNumber);
                }
                else if (lastSoundType == SoundType.DRUM)
                {
                    //prognum no
                    int nn = NoteOnEvent.NoteNumber;
                    SPC700RegWriteData(parentModule.UnitNumber, (byte)(reg + 4), (byte)(nn + 128));
                }
                //loop
                ushort lpos = timbre.LoopPoint;
                SPC700RamWriteData(parentModule.UnitNumber, (uint)(0x200 + (programNumber * 4) + 2), (byte)(lpos & 0xff));
                SPC700RamWriteData(parentModule.UnitNumber, (uint)(0x200 + (programNumber * 4) + 3), (byte)(lpos >> 8));

                //ADSR
                if (timbre.AdsrEnable)
                {
                    SPC700RegWriteData(parentModule.UnitNumber, (byte)(reg + 5), (byte)(0x80 | (timbre.AdsrDR << 4) | timbre.AdsrAR));
                    SPC700RegWriteData(parentModule.UnitNumber, (byte)(reg + 6), (byte)((timbre.AdsrSL << 5) | timbre.AdsrSR));
                }
                else
                {
                    SPC700RegWriteData(parentModule.UnitNumber, (byte)(reg + 5), 0);
                    SPC700RegWriteData(parentModule.UnitNumber, (byte)(reg + 7), 0x7f);
                }
                //PMON
                //byte pmon = SPC700RegReadData(parentModule.UnitNumber, 0x2d);
                //pmon &= (byte)~bitPos;
                //pmon |= (byte)(timbre.PMON << Slot);
                //SPC700RegWriteData(parentModule.UnitNumber, 0x2d, pmon);
                //NON
                byte non = SPC700RegReadData(parentModule.UnitNumber, 0x3d);
                non &= (byte)~bitPos;
                non |= (byte)(timbre.NON << Slot);
                SPC700RegWriteData(parentModule.UnitNumber, 0x3d, non);
                //EON
                byte eon = SPC700RegReadData(parentModule.UnitNumber, 0x3d);
                eon &= (byte)~bitPos;
                eon |= (byte)(timbre.EON << Slot);
                SPC700RegWriteData(parentModule.UnitNumber, 0x4d, eon);

                //KON
                byte koff = (byte)(SPC700RegReadData(parentModule.UnitNumber, 0x5c) & ~bitPos);
                SPC700RegWriteData(parentModule.UnitNumber, 0x5c, koff);
                byte kon = (byte)(SPC700RegReadData(parentModule.UnitNumber, 0x4c) | bitPos);
                SPC700RegWriteData(parentModule.UnitNumber, 0x4c, kon);

                Program.SoundUpdated();
            }


            public override void OnSoundParamsUpdated()
            {
                base.OnSoundParamsUpdated();

                uint reg = (uint)(Slot * 16);
                byte bitPos = (byte)(1 << Slot);

                Program.SoundUpdating();

                var gs = timbre.GlobalSettings;
                if (gs.Enable)
                {
                    if (gs.LMVOL.HasValue)
                        parentModule.LMVOL = gs.LMVOL.Value;
                    if (gs.RMVOL.HasValue)
                        parentModule.RMVOL = gs.RMVOL.Value;
                    if (gs.LEVOL.HasValue)
                        parentModule.LEVOL = gs.LEVOL.Value;
                    if (gs.REVOL.HasValue)
                        parentModule.REVOL = gs.REVOL.Value;
                    if (gs.NOISE_CLOCK.HasValue)
                        parentModule.NOISE_CLOCK = gs.NOISE_CLOCK.Value;
                    if (gs.ECEN.HasValue)
                        parentModule.ECEN = gs.ECEN.Value;
                    if (gs.EFB.HasValue)
                        parentModule.EFB = gs.EFB.Value;
                    if (gs.EDL.HasValue)
                        parentModule.EDL = gs.EDL.Value;
                    if (gs.COEF1.HasValue)
                        parentModule.COEF1 = gs.COEF1.Value;
                    if (gs.COEF2.HasValue)
                        parentModule.COEF2 = gs.COEF2.Value;
                    if (gs.COEF3.HasValue)
                        parentModule.COEF3 = gs.COEF3.Value;
                    if (gs.COEF4.HasValue)
                        parentModule.COEF4 = gs.COEF4.Value;
                    if (gs.COEF5.HasValue)
                        parentModule.COEF5 = gs.COEF5.Value;
                    if (gs.COEF6.HasValue)
                        parentModule.COEF6 = gs.COEF6.Value;
                    if (gs.COEF7.HasValue)
                        parentModule.COEF7 = gs.COEF7.Value;
                    if (gs.COEF8.HasValue)
                        parentModule.COEF8 = gs.COEF8.Value;
                }

                OnVolumeUpdated();
                OnPanpotUpdated();
                OnPitchUpdated();

                //ADSR
                if (timbre.AdsrEnable)
                {
                    SPC700RegWriteData(parentModule.UnitNumber, (byte)(reg + 5), (byte)(0x80 | (timbre.AdsrDR << 4) | timbre.AdsrAR));
                    SPC700RegWriteData(parentModule.UnitNumber, (byte)(reg + 6), (byte)((timbre.AdsrSL << 5) | timbre.AdsrSR));
                }
                else
                {
                    SPC700RegWriteData(parentModule.UnitNumber, (byte)(reg + 5), 0);
                    SPC700RegWriteData(parentModule.UnitNumber, (byte)(reg + 7), 0x7f);
                }
                //PMON
                //byte pmon = SPC700RegReadData(parentModule.UnitNumber, 0x2d);
                //pmon &= (byte)~bitPos;
                //pmon |= (byte)(timbre.PMON << Slot);
                //SPC700RegWriteData(parentModule.UnitNumber, 0x2d, pmon);
                //NON
                byte non = SPC700RegReadData(parentModule.UnitNumber, 0x3d);
                non &= (byte)~bitPos;
                non |= (byte)(timbre.NON << Slot);
                SPC700RegWriteData(parentModule.UnitNumber, 0x3d, non);
                //EON
                byte eon = SPC700RegReadData(parentModule.UnitNumber, 0x3d);
                eon &= (byte)~bitPos;
                eon |= (byte)(timbre.EON << Slot);
                SPC700RegWriteData(parentModule.UnitNumber, 0x4d, eon);

                Program.SoundUpdated();
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

                byte left = (byte)Math.Round(127d * vol * Math.Cos(Math.PI / 2 * (pan / 126d)));
                byte right = (byte)Math.Round(127d * vol * Math.Sin(Math.PI / 2 * (pan / 126d)));
                SPC700RegWriteData(parentModule.UnitNumber, (byte)(reg + 0), left);
                SPC700RegWriteData(parentModule.UnitNumber, (byte)(reg + 1), right);
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
                    freq = (uint)Math.Round(0x1000 * CalcCurrentFrequency() / baseFreq);
                else if (lastSoundType == SoundType.DRUM)
                    freq = (uint)Math.Round(0x1000 * (1d + CalcCurrentPitch()));

                if (freq > 0x3fff)
                    freq = 0x3fff;

                SPC700RegWriteData(parentModule.UnitNumber, (byte)(reg + 2), (byte)(freq & 0xff));
                SPC700RegWriteData(parentModule.UnitNumber, (byte)(reg + 3), (byte)((freq >> 8) & 0xff));

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

                byte bitPos = (byte)(1 << Slot);

                byte kon = (byte)(SPC700RegReadData(parentModule.UnitNumber, 0x4c) & ~bitPos);
                SPC700RegWriteData(parentModule.UnitNumber, 0x4c, kon);
                byte koff = (byte)(SPC700RegReadData(parentModule.UnitNumber, 0x5c) | bitPos);
                SPC700RegWriteData(parentModule.UnitNumber, 0x5c, koff);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        [JsonConverter(typeof(NoTypeConverterJsonConverter<SPC700Timbre>))]
        [DataContract]
        public class SPC700Timbre : TimbreBase
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
            [Description("Set ADPCM base frequency [Hz]")]
            [DefaultValue(typeof(double), "500")]
            [DoubleSlideParametersAttribute(100, 2000, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public double BaseFreqency
            {
                get;
                set;
            } = 500;

            [DataMember]
            [Category("Sound")]
            [Description("Set loop point (0 - 65535) (Need to set loop flag in BRR PCM data)")]
            [DefaultValue(typeof(ushort), "0")]
            [SlideParametersAttribute(0, 65535)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public ushort LoopPoint
            {
                get;
                set;
            }

            private byte[] f_AdcmData = new byte[0];

            [TypeConverter(typeof(TypeConverter))]
            [Editor(typeof(BrrFileLoaderUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DataMember]
            [Category("Sound")]
            [Description("BRR ADPCM Data (MAX 64KB)")]
            [BrrFileLoaderEditor("Audio File(*.brr)|*.brr", 65535)]
            //[BrrFileLoaderEditor("Audio File(*.brr, *.wav)|*.brr;*.wav", 65535)]
            public byte[] AdpcmData
            {
                get
                {
                    return f_AdcmData;
                }
                set
                {
                    f_AdcmData = value;
                }
            }

            public bool ShouldSerializeAdpcmData()
            {
                return AdpcmData.Length != 0;
            }

            public void ResetAdpcmData()
            {
                AdpcmData = new byte[0];
            }

            [DataMember]
            [Category("Sound")]
            [Description("Enable ADSR mode")]
            [DefaultValue(false)]
            public bool AdsrEnable
            {
                get;
                set;
            }

            private byte f_AdsrAR = 15;

            [DataMember]
            [Category("Sound")]
            [Description("Attack rate of ADSR")]
            [DefaultValue((byte)15)]
            [SlideParametersAttribute(0, 15)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte AdsrAR
            {
                get
                {
                    return f_AdsrAR;
                }
                set
                {
                    f_AdsrAR = (byte)(value & 0xf);
                }
            }

            private byte f_AdsrDR = 7;

            [DataMember]
            [Category("Sound")]
            [Description("Decay rate of ADSR")]
            [DefaultValue((byte)7)]
            [SlideParametersAttribute(0, 7)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte AdsrDR
            {
                get
                {
                    return f_AdsrDR;
                }
                set
                {
                    f_AdsrDR = (byte)(value & 7);
                }
            }

            private byte f_AdsrSL = 7;

            [DataMember]
            [Category("Sound")]
            [Description("Sustain level of ADSR")]
            [DefaultValue((byte)7)]
            [SlideParametersAttribute(0, 7)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte AdsrSL
            {
                get
                {
                    return f_AdsrSL;
                }
                set
                {
                    f_AdsrSL = (byte)(value & 7);
                }
            }

            private byte f_AdsrSR = 15;

            [DataMember]
            [Category("Sound")]
            [Description("Sustain rate of ADSR")]
            [DefaultValue((byte)15)]
            [SlideParametersAttribute(0, 31)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte AdsrSR
            {
                get
                {
                    return f_AdsrSR;
                }
                set
                {
                    f_AdsrSR = (byte)(value & 31);
                }
            }

            //private byte f_PMON;

            //[DataMember]
            //[Category("Sound")]
            //[Description("Pitch modulation enable")]
            //[DefaultValue((byte)0)]
            //[SlideParametersAttribute(0, 1)]
            //[EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            //public byte PMON
            //{
            //    get
            //    {
            //        return f_PMON;
            //    }
            //    set
            //    {
            //        f_PMON = (byte)(value & 1);
            //    }
            //}

            private byte f_NON;

            [DataMember]
            [Category("Sound")]
            [Description("Noise enable")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte NON
            {
                get
                {
                    return f_NON;
                }
                set
                {
                    f_NON = (byte)(value & 1);
                }
            }

            private byte f_EON;

            [DataMember]
            [Category("Sound")]
            [Description("Echo enable")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte EON
            {
                get
                {
                    return f_EON;
                }
                set
                {
                    f_EON = (byte)(value & 1);
                }
            }


            [DataMember]
            [Category("Chip")]
            [Description("Global Settings")]
            public SPC700GlobalSettings GlobalSettings
            {
                get;
                set;
            }


            /// <summary>
            /// 
            /// </summary>
            public SPC700Timbre()
            {
                GlobalSettings = new SPC700GlobalSettings();
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
                    var obj = JsonConvert.DeserializeObject<SPC700Timbre>(serializeData);
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
        public class SPC700PcmSoundTable : PcmTimbreTableBase
        {

            /// <summary>
            /// 
            /// </summary>
            public SPC700PcmSoundTable()
            {
                for (int i = 0; i < 128; i++)
                {
                    var pt = new SPC700PcmTimbre(i);
                    PcmTimbres[i] = pt;
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        [DataContract]
        public class SPC700PcmTimbre : PcmTimbreBase
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
                        f_PcmData = value;
                    }
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="noteNumber"></param>
            public SPC700PcmTimbre(int noteNumber) : base(noteNumber)
            {
            }
        }


        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [JsonConverter(typeof(NoTypeConverterJsonConverter<SPC700GlobalSettings>))]
        [DataContract]
        [MidiHook]
        public class SPC700GlobalSettings : ContextBoundObject
        {
            [DataMember]
            [Category("Chip")]
            [Description("Override global settings")]
            public bool Enable
            {
                get;
                set;
            }

            private byte? f_LMVOL;

            [DataMember]
            [Category("Chip")]
            [Description("Set Left Output Main Volume")]
            [DefaultValue(null)]
            [SlideParametersAttribute(0, 255)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte? LMVOL
            {
                get
                {
                    return f_LMVOL;
                }
                set
                {
                    f_LMVOL = value;
                }
            }

            private byte? f_RMVOL;

            [DataMember]
            [Category("Chip")]
            [Description("Set Right Output Main Volume")]
            [DefaultValue(null)]
            [SlideParametersAttribute(0, 255)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte? RMVOL
            {
                get
                {
                    return f_RMVOL;
                }
                set
                {
                    f_RMVOL = value;
                }
            }

            private sbyte? f_LEVOL;

            [DataMember]
            [Category("Filter")]
            [Description("Set Left Output Echo Volume")]
            [DefaultValue(null)]
            [SlideParametersAttribute(-128, 128)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public sbyte? LEVOL
            {
                get
                {
                    return f_LEVOL;
                }
                set
                {
                    f_LEVOL = value;
                }
            }

            private sbyte? f_REVOL;

            [DataMember]
            [Category("Filter")]
            [Description("Set Right Output Echo Volume")]
            [DefaultValue(null)]
            [SlideParametersAttribute(-128, 128)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public sbyte? REVOL
            {
                get
                {
                    return f_REVOL;
                }
                set
                {
                    f_REVOL = value;
                }
            }

            private byte? f_NOISE_CLOCK;

            [DataMember]
            [Category("Chip")]
            [Description("Designates the frequency for the white noise.")]
            [DefaultValue(null)]
            [SlideParametersAttribute(0, 31)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte? NOISE_CLOCK
            {
                get
                {
                    return f_NOISE_CLOCK;
                }
                set
                {
                    byte? v = value;
                    if (value.HasValue)
                        v = (byte)(value & 31);
                    f_NOISE_CLOCK = v;
                }
            }

            private byte? f_ECEN;

            [DataMember]
            [Category("Filter")]
            [Description("Echo enable")]
            [DefaultValue(null)]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte? ECEN
            {
                get
                {
                    return f_ECEN;
                }
                set
                {
                    byte? v = value;
                    if (value.HasValue)
                        v = (byte)(value & 1);
                    f_ECEN = v;
                }
            }


            private sbyte? f_EFB;

            [DataMember]
            [Category("Filter")]
            [Description("Echo Feedback")]
            [DefaultValue(null)]
            [SlideParametersAttribute(-128, 127)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public sbyte? EFB
            {
                get
                {
                    return f_EFB;
                }
                set
                {
                    f_EFB = value;
                }
            }

            private byte? f_EDL;

            [DataMember]
            [Category("Filter")]
            [Description("EDL specifies the delay between the main sound and the echoed sound. The delay is calculated as EDL * 16ms.")]
            [DefaultValue(null)]
            [SlideParametersAttribute(0, 15)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte? EDL
            {
                get
                {
                    return f_EDL;
                }
                set
                {
                    byte? v = value;
                    if (value.HasValue)
                        v = (byte)(value & 15);
                    f_EDL = v;
                }
            }

            private sbyte? f_COEF1;

            [DataMember]
            [Category("Filter")]
            [Description("COEF are used by the 8-tap FIR filter.")]
            [DefaultValue(null)]
            [SlideParametersAttribute(-128, 127)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public sbyte? COEF1
            {
                get
                {
                    return f_COEF1;
                }
                set
                {
                    f_COEF1 = value;
                }
            }

            private sbyte? f_COEF2;

            [DataMember]
            [Category("Filter")]
            [Description("COEF are used by the 8-tap FIR filter.")]
            [DefaultValue(null)]
            [SlideParametersAttribute(-128, 127)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public sbyte? COEF2
            {
                get
                {
                    return f_COEF2;
                }
                set
                {
                    f_COEF2 = value;
                }
            }

            private sbyte? f_COEF3;

            [DataMember]
            [Category("Filter")]
            [Description("COEF are used by the 8-tap FIR filter.")]
            [DefaultValue(null)]
            [SlideParametersAttribute(-128, 127)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public sbyte? COEF3
            {
                get
                {
                    return f_COEF3;
                }
                set
                {
                    f_COEF3 = value;
                }
            }

            private sbyte? f_COEF4;

            [DataMember]
            [Category("Filter")]
            [Description("COEF are used by the 8-tap FIR filter.")]
            [DefaultValue(null)]
            [SlideParametersAttribute(-128, 127)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public sbyte? COEF4
            {
                get
                {
                    return f_COEF4;
                }
                set
                {
                    f_COEF4 = value;
                }
            }

            private sbyte? f_COEF5;

            [DataMember]
            [Category("Filter")]
            [Description("COEF are used by the 8-tap FIR filter.")]
            [DefaultValue(null)]
            [SlideParametersAttribute(-128, 127)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public sbyte? COEF5
            {
                get
                {
                    return f_COEF5;
                }
                set
                {
                    f_COEF5 = value;
                }
            }


            private sbyte? f_COEF6;

            [DataMember]
            [Category("Filter")]
            [Description("COEF are used by the 8-tap FIR filter.")]
            [DefaultValue(null)]
            [SlideParametersAttribute(-128, 127)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public sbyte? COEF6
            {
                get
                {
                    return f_COEF6;
                }
                set
                {
                    f_COEF6 = value;
                }
            }

            private sbyte? f_COEF7;

            [DataMember]
            [Category("Filter")]
            [Description("COEF are used by the 8-tap FIR filter.")]
            [DefaultValue(null)]
            [SlideParametersAttribute(-128, 127)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public sbyte? COEF7
            {
                get
                {
                    return f_COEF7;
                }
                set
                {
                    f_COEF7 = value;
                }
            }

            private sbyte? f_COEF8;

            [DataMember]
            [Category("Filter")]
            [Description("COEF are used by the 8-tap FIR filter.")]
            [DefaultValue(null)]
            [SlideParametersAttribute(-128, 127)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public sbyte? COEF8
            {
                get
                {
                    return f_COEF8;
                }
                set
                {
                    f_COEF8 = value;
                }
            }


        }

    }

}
