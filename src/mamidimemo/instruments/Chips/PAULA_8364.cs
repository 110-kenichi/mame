//#define USE_PCM_LOOP
// copyright-holders:K.Ito
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Markup;
using System.Xml.Linq;
using FM_SoundConvertor;
using Kermalis.SoundFont2;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.MusicTheory;
using NAudio.SoundFont;
using NAudio.Wave;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Omu.ValueInjecter;
using Omu.ValueInjecter.Injections;
using zanac.MAmidiMEmo.ComponentModel;
using zanac.MAmidiMEmo.Gimic;
using zanac.MAmidiMEmo.Gui;
using zanac.MAmidiMEmo.Instruments.Envelopes;
using zanac.MAmidiMEmo.Mame;
using zanac.MAmidiMEmo.Midi;
using zanac.MAmidiMEmo.Properties;
using zanac.MAmidiMEmo.Scci;
using zanac.MAmidiMEmo.Util;
using zanac.MAmidiMEmo.VSIF;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;
using static zanac.MAmidiMEmo.Instruments.Chips.Beep;
using static zanac.MAmidiMEmo.Instruments.Chips.MultiPCM;
using static zanac.MAmidiMEmo.Instruments.Chips.SIDBase;
using static zanac.MAmidiMEmo.Instruments.Chips.YM2612;
using static zanac.MAmidiMEmo.Instruments.Chips.YM2612.YM2612Timbre;
using static zanac.MAmidiMEmo.Instruments.Chips.YM3812;

namespace zanac.MAmidiMEmo.Instruments.Chips
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    [InstLock]
    public class PAULA_8364 : InstrumentBase
    {
        public override string Name => "PAULA_8364";

        public override string Group => "PCM";

        public override InstrumentType InstrumentType => InstrumentType.PAULA_8364;

        [Browsable(false)]
        public override string ImageKey => "PAULA_8364";

        private const int MAX_VOICE = 1;

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        protected override string SoundInterfaceTagNamePrefix => "paula_8364_";

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
                return 34;
            }
        }

        private PortId portId = PortId.No1;

        [DataMember]
        [Category("Chip(Dedicated)")]
        [Description("Set COM Port No for \"VSIF - AMIGA\"\r\n" +
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
            "Supports Software and VSIF.")]
        [DefaultValue(SoundEngineType.Software)]
        [TypeConverter(typeof(EnumConverterSoundEngineTypeSPFM))]
        public SoundEngineType SoundEngine
        {
            get
            {
                return f_SoundEngineType;
            }
            set
            {
                if (f_SoundEngineType != value)
                    setSoundEngine(value);
            }
        }

        private class EnumConverterSoundEngineTypeSPFM : EnumConverter<SoundEngineType>
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                var sc = new StandardValuesCollection(new SoundEngineType[] {
                    SoundEngineType.Software,
                    SoundEngineType.VSIF_AMIGA,
                });

                return sc;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        private void setSoundEngine(SoundEngineType value)
        {
            try
            {
                ignoreUpdatePcmData = true;
                AllSoundOff();
            }
            finally
            {
                ignoreUpdatePcmData = false;
            }

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
                    case SoundEngineType.VSIF_AMIGA:
                        vsifClient = VsifManager.TryToConnectVSIF(VsifSoundModuleType.AMIGA, PortId, false);
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

        [Category("Chip(Dedicated)")]
        [Description("Current sound engine type.")]
        [DefaultValue(SoundEngineType.Software)]
        public SoundEngineType CurrentSoundEngine
        {
            get
            {
                return f_CurrentSoundEngineType;
            }
        }

        private uint targetSampleRate = 16000;

        [DataMember]
        [Category("Chip(Dedicated)")]
        [Description("Set default raw PCM file converting target PCM sample rate [Hz].")]
        [DefaultValue(typeof(uint), "16000")]
        public uint TargetSampleRate
        {
            get
            {
                return targetSampleRate;
            }
            set
            {
                if (value != 0)
                    targetSampleRate = value;
            }
        }

        private bool f_TransferPcmData;

        [Category("Chip(Dedicated)")]
        [Description("Transfer PCM data via Serial cable. **Too slow**\r\n" +
            "Try to use the [Export PCM] on the PAULA_8364 context menu.")]
        [DefaultValue(false)]
        public bool TransferPcmData
        {
            get
            {
                return f_TransferPcmData;
            }
            set
            {
                if (f_TransferPcmData != value)
                {
                    try
                    {
                        ignoreUpdatePcmData = true;
                        AllSoundOff();
                    }
                    finally
                    {
                        ignoreUpdatePcmData = false;
                    }

                    f_TransferPcmData = value;

                    ClearWrittenDataCache();
                    PrepareSound();
                }
            }
        }

        private PAULA_8364_Clock f_MasterClock = PAULA_8364_Clock.PAL;

        [DataMember]
        [Category("Chip")]
        [Description("Set PAULA clock.")]
        [DefaultValue(PAULA_8364_Clock.PAL)]
        public PAULA_8364_Clock MasterClock
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

                    SetClock(UnitNumber, (uint)f_MasterClock);
                }
            }
        }

        private bool f_FilterEnable;

        [DataMember]
        [Category("Chip(Global)")]
        [Description("Filter settings. Available for real machine only.")]
        [DefaultValue(false)]
        public bool FilterEnable
        {
            get
            {
                return f_FilterEnable;
            }
            set
            {
                if (f_FilterEnable != value)
                {
                    f_FilterEnable = value;
                    Paula8364Write(UnitNumber, PAULA_CMD.Filter, 0, (ushort)(f_FilterEnable ? 0x1 : 0x0), true);
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
                Timbres = (PaulaTimbre[])value;
            }
        }

        private PaulaTimbre[] f_Timbres;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category(" Timbres")]
        [Description("Timbres")]
        [EditorAttribute(typeof(TimbresArrayUITypeEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(ExpandableCollectionConverter))]
        public PaulaTimbre[] Timbres
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
                using (var obj = JsonConvert.DeserializeObject<PAULA_8364>(serializeData))
                    this.InjectFrom(new LoopInjection(new[] { "SerializeData", "SerializeDataSave", "SerializeDataLoad" }), obj);
                Paula8364SetCallback(UnitNumber, f_read_byte_callback);
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

        private bool cancelTransferPcmData;

        /// <summary>
        /// 
        /// </summary>
        private void updatePcmData(PaulaTimbre timbre, bool forceClear)
        {
            if (!f_TransferPcmData)
                return;

            cancelTransferPcmData = false;
            if (timbre == null)
            {
                AllSoundOff();
                for (int i = 0; i < Timbres.Length; i++)
                {
                    updatePcmData(Timbres[i], forceClear);
                    if (cancelTransferPcmData)
                        break;
                }
            }
            else if (CurrentSoundEngine != SoundEngineType.Software)
            {
                if (timbre.PcmData.Length > 0)
                    FormMain.OutputLog(this, Resources.UpdatingPCM + " (" + timbre.DisplayName + ")");
                try
                {
                    if (timbre.PcmData.Length > 0)
                    {
                        FormProgress.RunDialog(Resources.UpdatingPCM, (f) =>
                        {
                            f.StartPosition = FormStartPosition.CenterScreen;
                            f.Message = Resources.UpdatingPCM + " (" + timbre.DisplayName + ")";
                            transferPcmData(timbre, f);
                        },
                        () =>
                        {
                            cancelTransferPcmData = true;
                        });
                    }
                    else
                    {
                        if (forceClear)
                            transferPcmData(timbre, null);
                    }
                }
                finally
                {
                    if (timbre.PcmData.Length > 0)
                        FormMain.AppliactionForm.Enabled = true;
                }
                FormMain.OutputLog(this, string.Format(Resources.AdpcmBufferUsed, timbre.PcmData.Length / 1024));
            }
        }

        private void transferPcmData(PaulaTimbre tim, FormProgress fp)
        {
            lock (sndEnginePtrLock)
            {
                switch (CurrentSoundEngine)
                {
                    case SoundEngineType.VSIF_AMIGA:
                        {
                            if (tim.Instrument == null)
                                tim.Instrument = this;
                            tim.Instrument = this;
                            // id, len, loop, data...
                            List<byte> data = new List<byte>
                            {
                                (byte)PAULA_CMD.SetPcmData,
                                //ID
                                (byte)tim.Index,
                                //Length
                                (byte)(tim.PcmData.Length >> 8),
                                (byte)tim.PcmData.Length
                            };
#if USE_PCM_LOOP
                            //Loop
                            if (tim.LoopEnable && tim.LoopPoint < tim.PcmData.Length)
                            {
                                data.Add((byte)(tim.LoopPoint >> 8));
                                data.Add((byte)tim.LoopPoint);
                            }
                            else
                            {
                                data.Add((byte)0xff);
                                data.Add((byte)0xff);
                            }
#else
                            data.Add(0x00);
#endif
                            //Send params
                            vsifClient.RawWriteData(data.ToArray(), null);
                            data.Clear();

                            //Send pcm
                            int percentage = 0;
                            int lastPercentage = 0;
                            for (int i = 0; i < tim.PcmData.Length; i++)
                            {
                                data.Add((byte)tim.PcmData[i]);
                                if (data.Count >= 256)
                                {
                                    vsifClient.RawWriteData(data.ToArray(), null);
                                    data.Clear();

                                    percentage = (100 * i) / tim.PcmData.Length;
                                    if (percentage != lastPercentage)
                                    {
                                        if (fp != null)
                                            fp.Percentage = percentage;

                                        if (cancelTransferPcmData)
                                            fp.Message = "Cancelling...";

                                    }
                                    Application.DoEvents();
                                    lastPercentage = percentage;
                                }
                            }
                            if (data.Count >= 0)
                                vsifClient.RawWriteData(data.ToArray(), null);
                            if (fp != null)
                                fp.Percentage = 100;
                            Application.DoEvents();
#if USE_PCM_LOOP
                            Paula8364Write(UnitNumber, PAULA_CMD.SetPcmDataLoop, (uint)tim.Index, (ushort)(tim.LoopEnable ? tim.LoopPoint : 0xFFFF), true);
#endif
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("General")]
        [Description("Set LFO and Portament processing interval[ms]. If you use a real hardware, please increase to appropriate value.")]
        [DefaultValue(10d)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(UITypeEditor))]
        [DoubleSlideParameters(1d, 50d, 1d)]
        public override double ProcessingInterval
        {
            get
            {
                return base.ProcessingInterval;
            }
            set
            {
                base.ProcessingInterval = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delegate_paula_8364_write(uint unitNumber, uint address, ushort data);

        /// <summary>
        /// 
        /// </summary>
        private void Paula8364Write(uint unitNumber, PAULA_CMD type, uint address, ushort data, bool vsifOnly)
        {
            switch (type)
            {
                case PAULA_CMD.Volume:
                    {
                        byte ch = chConvert[address];

                        lock (sndEnginePtrLock)
                        {
                            switch (CurrentSoundEngine)
                            {
                                case SoundEngineType.VSIF_AMIGA:
                                    // ch, value
                                    vsifClient.RawWriteData(new byte[] { (byte)((byte)type | (ch << 4)),
                                        (byte)data, 0, 0, 0 }, null);
                                    break;
                            }
                        }
                        if (!vsifOnly)
                            DeferredWriteData(paula_8364_write, unitNumber, (uint)((0xa8 + (ch * 0x10)) / 2), data);
                    }
                    break;
                case PAULA_CMD.Pitch:
                    {
                        byte ch = chConvert[address];

                        lock (sndEnginePtrLock)
                        {
                            switch (CurrentSoundEngine)
                            {
                                case SoundEngineType.VSIF_AMIGA:
                                    // ch, value
                                    vsifClient.RawWriteData(new byte[] { (byte)((byte)type | (ch << 4)),
                                        (byte)(data >> 8), (byte)data, 0, 0 }, null);
                                    break;
                            }
                        }
                        if (!vsifOnly)
                            DeferredWriteData(paula_8364_write, unitNumber, (uint)((0xa6 + (ch * 0x10)) / 2), data);
                    }
                    break;
                case PAULA_CMD.Filter:
                    lock (sndEnginePtrLock)
                    {
                        switch (CurrentSoundEngine)
                        {
                            case SoundEngineType.VSIF_AMIGA:
                                // value
                                vsifClient.RawWriteData(new byte[] { (byte)type, (byte)data, 0, 0, 0 }, null);
                                break;
                        }
                    }
                    break;
                case PAULA_CMD.SetPcmDataLoop:
                    lock (sndEnginePtrLock)
                    {
                        switch (CurrentSoundEngine)
                        {
                            case SoundEngineType.VSIF_AMIGA:
                                // id, value
                                vsifClient.RawWriteData(new byte[] { (byte)type, (byte)address, (byte)(data >> 8), (byte)data, 0 }, null);
                                break;
                        }
                    }
                    break;
            }
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
        private static delegate_paula_8364_write paula_8364_write
        {
            get;
            set;
        }

        private static byte[] chConvert = new byte[] { 0, 2, 1, 3 };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delegate_paula_8364_keyon(uint unitNumber, byte ch, byte id, byte vol, ushort period, ushort length, ushort loop);

        /// <summary>
        /// 
        /// </summary>
        private void Paula8364Keyon(uint unitNumber, byte ch, byte id, byte vol, ushort period, ushort length, ushort loop)
        {
            ch = chConvert[ch];
            if (loop >= length)
                loop = 0xffff;
            //#if !USE_PCM_LOOP
            //            loop = 0;
            //#endif

            lock (sndEnginePtrLock)
            {
                switch (CurrentSoundEngine)
                {
                    case SoundEngineType.VSIF_AMIGA:
                        vsifClient.RawWriteData(new byte[] { (byte)((byte)PAULA_CMD.KeyOn | ch << 4), id, vol, (byte)(period >> 8), (byte)period }, null);
                        if (loop > 0)
                            vsifClient.RawWriteData(new byte[] { (byte)((byte)PAULA_CMD.SetLoop | ch << 4), id, (byte)(loop >> 8), (byte)loop, 0 }, null);
                        break;
                }
            }
            DeferredWriteData(paula_8364_keyon, unitNumber, ch, id, vol, period, (ushort)(length >> 1), loop);
        }

        /// <summary>
        /// 
        /// </summary>
        private static delegate_paula_8364_keyon paula_8364_keyon
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
        private delegate void delegate_paula_8364_keyoff(uint unitNumber, byte ch);

        /// <summary>
        /// 
        /// </summary>
        private void Paula8364Keyoff(uint unitNumber, byte ch)
        {
            ch = chConvert[ch];

            lock (sndEnginePtrLock)
            {
                switch (CurrentSoundEngine)
                {
                    case SoundEngineType.VSIF_AMIGA:
                        vsifClient.RawWriteData(new byte[] { (byte)((byte)PAULA_CMD.KeyOff | ch << 4), 0, 0, 0, 0 }, null);
                        break;
                }
            }
            DeferredWriteData(paula_8364_keyoff, unitNumber, ch);
        }

        /// <summary>
        /// 
        /// </summary>
        private static delegate_paula_8364_keyoff paula_8364_keyoff
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
        private delegate int delg_callback(byte pn, int pos);

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
        private static void Paula8364SetCallback(uint unitNumber, delg_callback callback)
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
        static PAULA_8364()
        {
            IntPtr funcPtr = MameIF.GetProcAddress("paula_8364_write");
            if (funcPtr != IntPtr.Zero)
                paula_8364_write = (delegate_paula_8364_write)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_paula_8364_write));

            funcPtr = MameIF.GetProcAddress("paula_8364_keyon");
            if (funcPtr != IntPtr.Zero)
                paula_8364_keyon = (delegate_paula_8364_keyon)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_paula_8364_keyon));

            funcPtr = MameIF.GetProcAddress("paula_8364_keyoff");
            if (funcPtr != IntPtr.Zero)
                paula_8364_keyoff = (delegate_paula_8364_keyoff)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_paula_8364_keyoff));

            funcPtr = MameIF.GetProcAddress("paula_8364_set_callback");
            if (funcPtr != IntPtr.Zero)
                set_callback = (delegate_set_callback)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_set_callback));
        }

        private PAULA_8364SoundManager soundManager;

        private delg_callback f_read_byte_callback;

        /// <summary>
        /// 
        /// </summary>
        public PAULA_8364(uint unitNumber) : base(unitNumber)
        {
            Timbres = new PaulaTimbre[256];
            for (int i = 0; i < 256; i++)
                Timbres[i] = new PaulaTimbre();

            setPresetInstruments();

            this.soundManager = new PAULA_8364SoundManager(this);

            f_read_byte_callback = new delg_callback(read_byte_callback);
            Paula8364SetCallback(UnitNumber, f_read_byte_callback);

            GainLeft = DEFAULT_GAIN;
            GainRight = DEFAULT_GAIN;

            ProcessingInterval = 8;

            readSoundFontForTimbre = new ToolStripMenuItem("Import PCM from SF2 for &Timbre...");
            readSoundFontForTimbre.Click += ReadSoundFontForTimbre_Click;

            readSoundFontForDrumTimbre = new ToolStripMenuItem("Import PCM from SF2 for &DrumTimbre...");
            readSoundFontForDrumTimbre.Click += ReadSoundFontForDrumTimbre_Click;

            exportPcmForAmiga = new ToolStripMenuItem("Export PCM from VSIF &AMIGA...");
            exportPcmForAmiga.Click += ExportPcmForAmiga_Click;
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

                lock (sndEnginePtrLock)
                {
                    if (vsifClient != null)
                        vsifClient.Dispose();
                }

                // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                // TODO: 大きなフィールドを null に設定します。
                Paula8364SetCallback(UnitNumber, null);

                disposedValue = true;
            }
        }

        // TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
        ~PAULA_8364()
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

        /// <summary>
        /// 
        /// </summary>
        internal override void PrepareSound()
        {
            base.PrepareSound();

            initGlobalRegisters();
        }

        private bool ignoreUpdatePcmData;

        private void initGlobalRegisters()
        {
            Paula8364Write(UnitNumber, PAULA_CMD.Filter, 0, (ushort)(f_FilterEnable ? 0x1 : 0x0), true);

            if (!IsDisposing && !ignoreUpdatePcmData)
                updatePcmData(null, false);
        }

        /// <summary>
        /// 
        /// </summary>
        private void setPresetInstruments()
        {

        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pn"></param>
        /// <param name="pos"></param>
        private int read_byte_callback(byte pn, int pos)
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
        private class PAULA_8364SoundManager : SoundManagerBase
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

            private static SoundList<PAULA_8364Sound> instOnSounds = new SoundList<PAULA_8364Sound>(MAX_VOICE);

            private PAULA_8364 parentModule;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            public PAULA_8364SoundManager(PAULA_8364 parent) : base(parent)
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
                    PaulaTimbre timbre = (PaulaTimbre)bts[i];

                    tindex++;
                    var emptySlot = searchEmptySlot(note, timbre);
                    if (emptySlot.slot < 0)
                        continue;

                    PAULA_8364Sound snd = new PAULA_8364Sound(emptySlot.inst, this, timbre, tindex - 1, note, emptySlot.slot, (byte)ids[i]);
                    instOnSounds.Add(snd);

                    //HACK: store pcm data to local buffer to avoid "thread lock"
                    lock (parentModule.tmpPcmDataTable)
                        parentModule.tmpPcmDataTable[ids[i]] = timbre.PcmData;

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
            private (PAULA_8364 inst, int slot) searchEmptySlot(TaggedNoteOnEvent note, PaulaTimbre timbre)
            {
                var emptySlot = (parentModule, -1);

                switch (timbre.PhysicalChannel)
                {
                    case PhysicalChannel.Indeterminatene:
                        {
                            emptySlot = SearchEmptySlotAndOffForLeader(parentModule, instOnSounds, note, 4);
                            break;
                        }

                    case PhysicalChannel.IndeterminateneLeft:
                        {
                            emptySlot = SearchEmptySlotAndOffForLeader(parentModule, instOnSounds, note, 2, -1, 0);
                            break;
                        }
                    case PhysicalChannel.IndeterminateneRight:
                        {
                            emptySlot = SearchEmptySlotAndOffForLeader(parentModule, instOnSounds, note, 4, -1, 2);
                            break;
                        }

                    case PhysicalChannel.Ch1:
                    case PhysicalChannel.Ch2:
                    case PhysicalChannel.Ch3:
                    case PhysicalChannel.Ch4:
                        {
                            emptySlot = SearchEmptySlotAndOffForLeader(parentModule, instOnSounds, note, 4, (int)timbre.PhysicalChannel - 3, 0);
                            break;
                        }
                }
                return emptySlot;
            }

            internal override void ProcessAllSoundOff()
            {
                var me = new ControlChangeEvent((SevenBitNumber)120, (SevenBitNumber)0);
                ProcessControlChange(me);

                for (int i = 0; i < PAULA_8364.MAX_VOICE; i++)
                    paula_8364_keyoff(parentModule.UnitNumber, (byte)i);
            }

        }


        /// <summary>
        /// 
        /// </summary>
        private class PAULA_8364Sound : SoundBase
        {

            private PAULA_8364 parentModule;

            private byte timbreIndex;

            private PaulaTimbre timbre;

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
            public PAULA_8364Sound(PAULA_8364 parentModule, PAULA_8364SoundManager manager, TimbreBase timbre, int tindex, TaggedNoteOnEvent noteOnEvent, int slot, byte timbreIndex) : base(parentModule, manager, timbre, tindex, noteOnEvent, slot)
            {
                this.parentModule = parentModule;
                this.timbreIndex = timbreIndex;
                this.timbre = (PaulaTimbre)timbre;

                baseFreq = this.timbre.BaseFreqency;
                sampleRate = this.timbre.SampleRate;
                loopPoint = this.timbre.LoopPoint;
                loopEn = this.timbre.LoopEnable;
            }

            /// <summary>
            /// 
            /// </summary>
            public override void KeyOn()
            {
                base.KeyOn();

                var vol = (ushort)(CalcCurrentVolume() * 64);
                uint freq = 0;
                freq = (uint)Math.Round((double)parentModule.MasterClock / ((CalcCurrentFrequency() / baseFreq) * sampleRate));
                if (freq > 0xffff)
                    freq = 0xffff;

                ushort pcmLen = (ushort)timbre.PcmData.Length;
                if (pcmLen < 2)
                    pcmLen = 2;
                ushort loop = loopEn ? loopPoint : (ushort)(pcmLen - 2);
                parentModule.Paula8364Keyon(parentModule.UnitNumber,
                    (byte)Slot, timbreIndex, (byte)vol, (ushort)freq, (ushort)timbre.PcmData.Length, loop);
            }

            /// <summary>
            /// 
            /// </summary>
            public override void OnVolumeUpdated()
            {
                var vol = CalcCurrentVolume() * 64;

                parentModule.Paula8364Write(parentModule.UnitNumber, PAULA_CMD.Volume, (uint)Slot, (ushort)vol, false);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="slot"></param>
            public override void OnPitchUpdated()
            {
                uint freq = 0;
                freq = (uint)Math.Round((double)parentModule.MasterClock / (2 * (CalcCurrentFrequency() / baseFreq) * sampleRate));
                if (freq > 0xffff)
                    freq = 0xffff;
                else if (freq == 0)
                    freq = 1;

                parentModule.Paula8364Write(parentModule.UnitNumber, PAULA_CMD.Pitch, (uint)Slot, (ushort)freq, false);

                base.OnPitchUpdated();
            }

            /// <summary>
            /// 
            /// </summary>
            public override void SoundOff()
            {
                base.SoundOff();

                parentModule.Paula8364Keyoff(parentModule.UnitNumber, (byte)Slot);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        [JsonConverter(typeof(NoTypeConverterJsonConverter<PaulaTimbre>))]
        [DataContract]
        [InstLock]
        public class PaulaTimbre : TimbreBase
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
            [Description("Physical Channel")]
            [DefaultValue(PhysicalChannel.Indeterminatene)]
            public PhysicalChannel PhysicalChannel
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
            [DefaultValue(typeof(uint), "14000")]
            [SlideParametersAttribute(4000, 44100)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public uint SampleRate
            {
                get;
                set;
            } = 14000;

            private bool f_LoopEnable;

            [DataMember]
            [Category("Sound")]
            [Description("Loop point enabled\r\n" +
                "LIMITATION: Does not support too short/too early loop point.")]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue(false)]
#if !USE_PCM_LOOP
            //            [Browsable(false)]
#endif
            public bool LoopEnable
            {
                get
                {
                    return f_LoopEnable;
                }
                set
                {
                    if (f_LoopEnable != value)
                    {
                        f_LoopEnable = value;
                        if (Instrument != null)
                        {
                            ((PAULA_8364)Instrument).
                                Paula8364Write(Instrument.UnitNumber, PAULA_CMD.SetPcmDataLoop, (uint)Index, (ushort)(f_LoopEnable ? f_LoopPoint : 0xFFFF), true);
                        }
                    }
                }
            }

            private ushort f_LoopPoint;

            [DataMember]
            [Category("Sound")]
            [Description("Set loop point in WORD (0 - 65534) (65535 is loop off)\r\n" +
                "LIMITATION: Does not support too short/too early loop point.")]
            [DefaultValue(typeof(ushort), "0")]
            [SlideParametersAttribute(0, 65534)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
#if !USE_PCM_LOOP
            //            [Browsable(false)]
#endif
            public ushort LoopPoint
            {
                get
                {
                    return f_LoopPoint;
                }
                set
                {
                    if (f_LoopPoint != value)
                    {
                        f_LoopPoint = value;
                        if (Instrument != null)
                        {
                            ((PAULA_8364)Instrument).
                            Paula8364Write(Instrument.UnitNumber, PAULA_CMD.SetPcmDataLoop, (uint)Index, (ushort)(f_LoopEnable ? f_LoopPoint : 0xFFFF), true);
                        }
                    }
                }
            }

            private sbyte[] f_PcmData = new sbyte[0];

            [TypeConverter(typeof(LoadDataTypeConverter))]
            [Editor(typeof(PcmFileLoaderUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DataMember]
            [Category("Sound")]
            [Description("Signed 8bit PCM Raw Data or WAV Data. (MAX 128K samples, 1ch)\r\n" +
                "Need to increase the Gain value to sound 8bit PCM data.\r\n" +
                "LIMITATION: The length of the PCM must be at least 2 bytes.")]
            [PcmFileLoaderEditor("Audio File(*.raw, *.wav)|*.raw;*.wav", 0, 8, 1, 131071)]
            public sbyte[] PcmData
            {
                get
                {
                    return f_PcmData;
                }
                set
                {
                    bool forceClear = false;
                    if (f_PcmData.Length != 0 && value.Length == 0)
                        forceClear = true;
                    f_PcmData = value;

                    var inst = (PAULA_8364)this.Instrument;
                    if (inst != null)
                    {
                        inst.AllSoundOff();
                        inst.updatePcmData(this, forceClear);
                    }
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

            private String pcmDataInfo;

            [DataMember]
            [Category("Sound")]
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

            /// <summary>
            /// 
            /// </summary>
            public PaulaTimbre()
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
                    var obj = JsonConvert.DeserializeObject<PaulaTimbre>(serializeData);
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

        /// <summary>
        /// 
        /// </summary>
        [DataContract]
        [InstLock]
        public class PAULA_8364PcmSoundTable : PcmTimbreTableBase
        {

            /// <summary>
            /// 
            /// </summary>
            public PAULA_8364PcmSoundTable()
            {
                for (int i = 0; i < 128; i++)
                {
                    var pt = new PAULA_8364PcmTimbre(i);
                    PcmTimbres[i] = pt;
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        [DataContract]
        [InstLock]
        public class PAULA_8364PcmTimbre : PcmTimbreBase
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
            [DefaultValue(typeof(uint), "16000")]
            [SlideParametersAttribute(4000, 44100)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public uint SampleRate
            {
                get;
                set;
            } = 16000;

            [DataMember]
            [Category("Sound")]
            [Description("Set loop point in WORD (0 - 65534) (65535 is loop off)")]
            [DefaultValue(typeof(ushort), "0")]
            [SlideParametersAttribute(0, 65534)]
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
                            using (var dstream = new MemoryStream(value))
                            using (var reader = new NAudio.Wave.WaveFileReader(dstream))
                            {
                                var wf = reader.WaveFormat;

                                byte[] data = null;

                                if (8 != wf.BitsPerSample || 1 != wf.Channels)
                                {
                                    /*
                                    var r = MessageBox.Show(null,
                                        $"Incorrect wave format(Expected Ch=1 Bit=8)\r\n" +
                                        "Do you want to convert?", "Qeuestion", MessageBoxButtons.OKCancel);
                                    if (r == DialogResult.Cancel)
                                    {
                                        throw new FileLoadException(
                                        string.Format($"Incorrect wave format(Expected Ch=1 Bit=8)"));
                                    }
                                    */
                                    int bits = 8;
                                    int rate = wf.SampleRate;
                                    int ch = 1;

                                    WaveFormat format = new WaveFormat(rate, bits, ch);
                                    using (var converter = WaveFormatConversionStream.CreatePcmStream(reader))
                                    {
                                        using (var stream = new WaveFormatConversionProvider(format, converter.ToSampleProvider().ToWaveProvider16()))
                                        {
                                            var tmpdata = new byte[converter.Length];
                                            int rd = stream.Read(tmpdata, 0, tmpdata.Length);
                                            data = new byte[rd];
                                            Array.Copy(tmpdata, data, rd);
                                        }
                                    }
                                }
                                else
                                {
                                    data = new byte[reader.Length];
                                    reader.Read(data, 0, data.Length);
                                }

                                List<byte> al = new List<byte>(data);
                                //Max 128K
                                if (al.Count > 131071)
                                    al.RemoveRange(131071, al.Count - 131071);

                                f_PcmData = al.ToArray();

                                sbyte[] sbuf = new sbyte[f_PcmData.Length];
                                for (int i = 0; i < f_PcmData.Length; i++)
                                    sbuf[i] = (sbyte)(f_PcmData[i] - 0x80);
                                f_PAULA_8364PcmData = sbuf;
                            }
                        }
                        else
                        {
                            f_PcmData = value;
                            sbyte[] sbuf = new sbyte[f_PcmData.Length];
                            for (int i = 0; i < f_PcmData.Length; i++)
                                sbuf[i] = (sbyte)(f_PcmData[i] - 0x80);
                            f_PAULA_8364PcmData = sbuf;
                        }
                    }
                }
            }

            private sbyte[] f_PAULA_8364PcmData = new sbyte[0];

            [Browsable(false)]
            public sbyte[] PAULA_8364PcmData
            {
                get
                {
                    return f_PAULA_8364PcmData;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="noteNumber"></param>
            public PAULA_8364PcmTimbre(int noteNumber) : base(noteNumber)
            {
            }
        }

        #region MENU

        private ToolStripMenuItem readSoundFontForTimbre;

        private ToolStripMenuItem readSoundFontForDrumTimbre;

        private ToolStripMenuItem exportPcmForAmiga;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal override IEnumerable<ToolStripMenuItem> GetInstrumentMenus()
        {
            List<ToolStripMenuItem> menus = new System.Collections.Generic.List<ToolStripMenuItem>(base.GetInstrumentMenus());

            menus.AddRange(new ToolStripMenuItem[] {
                readSoundFontForTimbre,
                readSoundFontForDrumTimbre,
                exportPcmForAmiga
            });

            return menus.ToArray();
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

                System.Windows.Forms.MessageBox.Show(ex.ToString());
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

                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }
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
                    var tim = new PaulaTimbre();


                    double baseFreq = 440.0 * Math.Pow(2.0, (((double)s.OriginalKey - 69.0) / 12.0) + (double)(s.PitchCorrection / 100));
                    tim.BaseFreqency = baseFreq;
                    tim.SampleRate = s.SampleRate;

                    uint start = s.Start;
                    uint end = s.End;
                    if (s.LoopEnd < end && s.LoopStart < s.LoopEnd)
                        end = s.LoopEnd;

                    uint len = end - start + 1;
                    if (len > 131071)
                        len = 131071;
                    uint loopP = s.LoopStart - s.Start;
                    if (loopP > 131071)
                        loopP = 131071;

                    sbyte[] samples = new sbyte[len];
                    for (uint i = 0; i < len; i++)
                        samples[i] = (sbyte)(spl[start + i] >> 8);

                    tim.PcmData = samples;
                    tim.LoopPoint = (ushort)(loopP >> 1);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExportPcmForAmiga_Click(object sender, EventArgs e)
        {
            try
            {
                using (var saveFileDialog = new System.Windows.Forms.SaveFileDialog())
                {
                    saveFileDialog.OverwritePrompt = true;
                    saveFileDialog.FileName = "default.vpcm";
                    saveFileDialog.SupportMultiDottedExtensions = true;
                    saveFileDialog.Title = "Export VSIF AMIGA PCM file";
                    saveFileDialog.Filter = "VSIF AMIGA PCM File(*.vpcm)|*.vpcm";

                    var fr = saveFileDialog.ShowDialog(null);
                    if (fr != DialogResult.OK)
                        return;

                    writePcm(saveFileDialog.FileName);
                }
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
        /// <param name="offset"></param>
        private void writePcm(String filePath)
        {
            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                using (var bw = new BinaryWriter(fs))
                {
                    bw.Write(new char[] { 'V', 'A', 'P', 'C' });
                    foreach (var t in Timbres)
                    {
                        // 00 00 Len
                        // 00 00 Loop
                        // ....
                        bw.Write(((ushort)t.PcmData.Length).ToByteArray().Reverse().ToArray());
                        if (t.LoopEnable)
                            bw.Write(((ushort)t.LoopPoint).ToByteArray().Reverse().ToArray());
                        else
                            bw.Write((ushort)0xFFFF);
                        for (int i = 0; i < t.PcmData.Length; i++)
                            bw.Write(t.PcmData[i]);
                    }
                }
            }
        }

        #endregion

        private static readonly PAULA_8364_Reg[] REG_VOLS = new PAULA_8364_Reg[] {
            PAULA_8364_Reg.REG_AUD0VOL,
            PAULA_8364_Reg.REG_AUD1VOL,
            PAULA_8364_Reg.REG_AUD2VOL,
            PAULA_8364_Reg.REG_AUD3VOL };

        private static readonly PAULA_8364_Reg[] REG_PERS = new PAULA_8364_Reg[] {
            PAULA_8364_Reg.REG_AUD0PER,
            PAULA_8364_Reg.REG_AUD1PER,
            PAULA_8364_Reg.REG_AUD2PER,
            PAULA_8364_Reg.REG_AUD3PER };


        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        [IgnoreDataMember]
        [JsonIgnore]
        public override bool CanImportBinFile
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
        public override String SupportedBinExts
        {
            get
            {
                return "*.raw;*.wav";
            }
        }

        private PaulaCustomToneImporter importer;

        /// <summary>
        /// 
        /// </summary>
        public override CustomToneImporter CustomToneImporter
        {
            get
            {
                if (importer == null)
                {
                    importer = new PaulaCustomToneImporter();
                }
                return importer;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private class PaulaCustomToneImporter : CustomToneImporter
        {

            /// <summary>
            /// 
            /// </summary>
            public override string ExtensionsFilterExt
            {
                get
                {
                    return "*.raw;*.wav;*.8svx;*.iff";
                }
            }

            public override IEnumerable<Tone> ImportTone(string text)
            {
                return null;
            }

            public override IEnumerable<Tone> ImportToneFile(string file)
            {
                return null;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="tones"></param>
            /// <returns></returns>
            public override IEnumerable<TimbreBase> ImportToneFileAsTimbre(string file)
            {
                return importPcmFile(new FileInfo(file));
            }
        }

        private static DialogResult? previousSampleRateAns;

        private static uint littleEndianBytesToBigEndianUInt(byte[] littleEndianBytes)
        {
            // 1. リトルエンディアンのバイト配列をuintとして読み込む (バイト配列を反転させる)
            byte[] reversedBytes = littleEndianBytes.Reverse().ToArray();
            return BitConverter.ToUInt32(reversedBytes, 0);
        }

        private static ushort littleEndianBytesToBigEndianUShort(byte[] littleEndianBytes)
        {
            // 1. リトルエンディアンのバイト配列をuintとして読み込む (バイト配列を反転させる)
            byte[] reversedBytes = littleEndianBytes.Reverse().ToArray();
            return BitConverter.ToUInt16(reversedBytes, 0);
        }

        private static byte[] readChunkData(BinaryReader br)
        {
            uint len = littleEndianBytesToBigEndianUInt(br.ReadBytes((int)4));

            return br.ReadBytes((int)len);
        }

        /* Fibonacci delta encoding for sound data. */
        private static sbyte[] codeToDelta = { -34, -21, -13, -8, -5, -3, -2, -1, 0, 1, 2, 3, 5, 8, 13, 21 };

        /* Unpack Fibonacci-delta encoded data from n byte source buffer into
         * 2*n byte dest buffer, given initial data value x.  It returns the
         * last data value x so you can call it several times to incrementally
         * decompress the data.                                             */
        private static short D1Unpack(sbyte[] source, int n, sbyte[] dest, sbyte x)
        {
            sbyte d;
            int i, lim;

            lim = n << 1;
            for (i = 0; i < lim; ++i)
            { /* Decode a data nibble; high nibble then low nibble. */
                d = source[i >> 1];    /* get a pair of nibbles        */
                if ((i & 1) != 0)              /* select low or high nibble?   */
                    d &= 0xf;       /* mask to get the low nibble   */
                else
                    d >>= 4;       /* shift to get the high nibble */
                x += codeToDelta[d];    /* add in the decoded delta     */
                dest[i] = x;            /* store a 1-byte sample        */
            }
            return x;
        }

        /* Unpack Fibonacci-delta encoded data from n byte source buffer into
         * 2*(n-2) byte dest buffer. Source buffer has a pad byte, an 8-bit
         * initial value, followed by n-2 bytes comprising 2*(n-2) 4-bit
         * encoded samples.                                                 */

        private static void DUnpack(sbyte[] source, sbyte[] dest)
        {
            D1Unpack(source.Take(source.Length - 2).ToArray(), source.Length - 2, dest, source[1]);
        }

        /// <param name="binFile"></param>
        private static List<PaulaTimbre> importPcmFile(FileInfo binFile)
        {
            List<PaulaTimbre> tims = new List<PaulaTimbre>();

            switch (binFile.Extension.ToUpper(CultureInfo.InvariantCulture))
            {
                case ".IFF":
                case ".8SVX":
                    {
                        try
                        {
                            using (var fs = new FileStream(binFile.FullName, FileMode.Open, FileAccess.Read))
                            {
                                var br = new BinaryReader(fs);
                                var tmpdata = br.ReadBytes((int)4);
                                if (!String.Equals(ASCIIEncoding.ASCII.GetString(tmpdata), "FORM", StringComparison.Ordinal))
                                {
                                    System.Windows.Forms.MessageBox.Show("This file(" + binFile.Name + ") is not IFF format.");
                                    return tims;
                                }

                                br.ReadBytes((int)4);

                                tmpdata = br.ReadBytes((int)4);
                                if (!String.Equals(ASCIIEncoding.ASCII.GetString(tmpdata), "8SVX", StringComparison.Ordinal))
                                {
                                    System.Windows.Forms.MessageBox.Show("This file(" + binFile.Name + ") is not 8SVX format.");
                                    return tims;
                                }

                                sbyte[] pcmData = null;
                                String pcmDataInfo = binFile.FullName;
                                String name = null;
                                String memo = null;
                                uint? oneShotHiSamples = null;
                                uint? repeatHiSamples = null;
                                uint? samplesPerHiCycle = null;
                                ushort? samplesPerSec = null;
                                byte? ctOctave = null;
                                byte? compression = null;
                                uint? volume = null;
                                bool? atack = null;
                                bool? release = null;
                                //https://wiki.amigaos.net/wiki/8SVX_IFF_8-Bit_Sampled_Voice
                                while (true)
                                {
                                    var chunk = br.ReadBytes((int)4);
                                    if (chunk.Length < 4)
                                        break;
                                    tmpdata = readChunkData(br);
                                    if (tmpdata.Length == 0)
                                        break;
                                    switch (ASCIIEncoding.ASCII.GetString(chunk))
                                    {
                                        case "VHDR":
                                            /*
                                            uint32_t oneShotHiSamples;
                                            uint32_t repeatHiSamples;
                                            uint32_t samplesPerHiCycle;
                                            uint16_t samplesPerSec;
                                            uint8_t  ctOctave;
                                            uint8_t  compression;
                                            uint32_t volume;
                                            */
                                            oneShotHiSamples = littleEndianBytesToBigEndianUInt(tmpdata.Skip(0).Take(4).ToArray());
                                            repeatHiSamples = littleEndianBytesToBigEndianUInt(tmpdata.Skip(4).Take(4).ToArray());
                                            samplesPerHiCycle = littleEndianBytesToBigEndianUInt(tmpdata.Skip(8).Take(4).ToArray());
                                            samplesPerSec = littleEndianBytesToBigEndianUShort(tmpdata.Skip(12).Take(2).ToArray());
                                            ctOctave = tmpdata[14];
                                            compression = tmpdata[15];
                                            volume = littleEndianBytesToBigEndianUInt(tmpdata.Skip(16).Take(4).ToArray());
                                            break;
                                        case "NAME":
                                            name = ASCIIEncoding.ASCII.GetString(tmpdata).Replace((char)0, ' ').Trim();
                                            break;
                                        case "ANNO":
                                            memo = ASCIIEncoding.ASCII.GetString(tmpdata).Replace((char)0, ' ').Trim();
                                            break;
                                        case "BODY":
                                            pcmData = Array.ConvertAll(tmpdata, b => unchecked((sbyte)b));
                                            break;
                                        case "ATAK":
                                            atack = true;
                                            break;
                                        case "RLSE":
                                            release = true;
                                            break;
                                        default:
                                            //AUTH
                                            break;
                                    }
                                }
                                if (pcmData != null)
                                {
                                    if (compression != null && compression.Value == 1)
                                        DUnpack(pcmData, pcmData);

                                    if (ctOctave != null)
                                    {
                                        int index = 0;
                                        for (int i = 0; i < ctOctave; i++)
                                        {
                                            PaulaTimbre tim = new PaulaTimbre();
                                            int oct = (int)Math.Pow(2, i);
                                            if (oneShotHiSamples != 0)
                                            {
                                                tim.PcmData = pcmData.Skip(index).Take(oct * (int)(oneShotHiSamples + repeatHiSamples)).ToArray();

                                                index += tim.PcmData.Length;

                                                if (oct * oneShotHiSamples < tim.PcmData.Length)
                                                {
                                                    tim.LoopEnable = true;
                                                    tim.LoopPoint = (ushort)(oct * oneShotHiSamples) > 0xFFFF ? (ushort)0xFFFF : (ushort)(oct * oneShotHiSamples);
                                                }
                                                else
                                                {
                                                    tim.LoopEnable = false;
                                                    tim.LoopPoint = 0xffff;
                                                }
                                            }
                                            else
                                            {
                                                tim.PcmData = pcmData;
                                                tim.LoopEnable = false;
                                                tim.LoopPoint = 0xffff;
                                            }

                                            tim.SampleRate = samplesPerSec.Value;
                                            if (samplesPerHiCycle != 0)
                                                tim.BaseFreqency = (double)tim.SampleRate / (double)samplesPerHiCycle;
                                            else
                                                tim.BaseFreqency = (double)440;

                                            if (atack != null || release != null)
                                                tim.SDS.ADSR.Enable = true;
                                            else
                                                tim.SDS.ADSR.Enable = false;

                                            String idx = "";
                                            if (ctOctave != 1)
                                                idx = "(" + (i + 1) + "/" + ctOctave + ")";
                                            if (name == null)
                                                tim.TimbreName = System.IO.Path.GetFileNameWithoutExtension(binFile.Name) + idx;
                                            else
                                                tim.TimbreName = name + idx;
                                            tim.Memo = memo;
                                            tim.PcmDataInfo = pcmDataInfo;

                                            tims.Add(tim);
                                        }
                                    }
                                    else
                                    {
                                        PaulaTimbre tim = new PaulaTimbre();
                                        tim.PcmData = pcmData;

                                        if (atack != null || release != null)
                                            tim.SDS.ADSR.Enable = true;
                                        else
                                            tim.SDS.ADSR.Enable = false;

                                        if (name == null)
                                            tim.TimbreName = System.IO.Path.GetFileNameWithoutExtension(binFile.Name);
                                        else
                                            tim.TimbreName = name;
                                        tim.Memo = memo;
                                        tim.PcmDataInfo = pcmDataInfo;

                                        tims.Add(tim);
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


                            System.Windows.Forms.MessageBox.Show(ex.Message);
                        }
                    }
                    break;
            }
            return tims;
        }

        public override void ImportBinFile(TimbreBase timbre, FileInfo binFile)
        {
            List<PaulaTimbre> tims = new List<PaulaTimbre>();

            switch (binFile.Extension.ToUpper(CultureInfo.InvariantCulture))
            {
                case ".RAW":
                    {
                        PaulaTimbre tim = (PaulaTimbre)timbre;

                        byte[] tmpdata = null;
                        using (var fs = new BinaryReader(new FileStream(binFile.FullName, FileMode.Open, FileAccess.Read)))
                        {
                            tmpdata = fs.ReadBytes((int)fs.BaseStream.Length);
                        }
                        if (String.Equals(ASCIIEncoding.ASCII.GetString(tmpdata, 0, 4), "FORM", StringComparison.Ordinal))
                        {
                            var result = MessageBox.Show("This file(" + binFile.Name + ") may IFF format( *.iff or *.8svx )\r\n" +
                                "Do you want to change *.raw to *.iff?", "Warning", MessageBoxButtons.YesNo);
                            if (result == DialogResult.Yes)
                            {

                                System.IO.File.Move(binFile.FullName, System.IO.Path.ChangeExtension(binFile.FullName, ".iff"));
                                throw new WarningException("Done. Please refresh the view.");
                            }
                        }

                        sbyte[] data = Array.ConvertAll(tmpdata, b => unchecked((sbyte)b));
                        tim.PcmData = data;
                        tim.PcmDataInfo = binFile.FullName;
                        tim.TimbreName = System.IO.Path.GetFileNameWithoutExtension(binFile.Name);
                    }
                    break;
                case ".WAV":
                    {
                        PaulaTimbre tim = (PaulaTimbre)timbre;
                        using (var reader = new NAudio.Wave.WaveFileReader(binFile.FullName))
                        {
                            var wf = reader.WaveFormat;

                            sbyte[] data = null;

                            if (8 != wf.BitsPerSample || 1 != wf.Channels || wf.SampleRate > TargetSampleRate)
                            {
                                /*
                                var r = MessageBox.Show(null,
                                    $"Incorrect wave format(Expected Ch={att.Channels} Bit={att.Bits}, Rate={att.Rate})\r\n" +
                                    "Do you want to convert?", "Qeuestion", MessageBoxButtons.OKCancel);
                                if (r == DialogResult.Cancel)
                                {
                                    throw new FileLoadException(
                                    string.Format($"Incorrect wave format(Expected Ch={att.Channels} Bit={att.Bits}, Rate={att.Rate}"));
                                }*/

                                int bits = 8;
                                int rate = wf.SampleRate;
                                int ch = 1;

                                if (rate > TargetSampleRate)
                                {
                                    DialogResult r;
                                    if ((Control.ModifierKeys & Keys.Alt) == Keys.Alt)
                                        previousSampleRateAns = null;

                                    if (previousSampleRateAns.HasValue)
                                    {
                                        r = previousSampleRateAns.Value;
                                    }
                                    else
                                    {
                                        r = MessageBox.Show(null,
                                            String.Format(Resources.SampleRateOver + "\r\n", TargetSampleRate) +
                                            String.Format(Resources.ConfirmConvertSampleRate, TargetSampleRate), "Qeuestion", MessageBoxButtons.YesNo);
                                        previousSampleRateAns = r;
                                    }
                                    if (r == DialogResult.Yes)
                                    {
                                        rate = (int)TargetSampleRate;
                                    }
                                }

                                wf = new WaveFormat(rate, bits, ch);
                                using (var converter = WaveFormatConversionStream.CreatePcmStream(reader))
                                {
                                    using (var stream = new WaveFormatConversionProvider(wf, converter.ToSampleProvider().ToWaveProvider16()))
                                    {
                                        var tmpdata = new byte[converter.Length];
                                        int rd = stream.Read(tmpdata, 0, tmpdata.Length);
                                        data = new sbyte[rd];
                                        Array.Copy(tmpdata, data, rd);
                                    }
                                }
                            }
                            else
                            {
                                var tmpdata = new byte[reader.Length];
                                reader.Read(tmpdata, 0, tmpdata.Length);
                                data = new sbyte[tmpdata.Length];
                                Array.Copy(tmpdata, data, tmpdata.Length);
                            }

                            tim.PcmData = data;
                            tim.PcmDataInfo = binFile.FullName;
                            tim.SampleRate = (uint)wf.SampleRate;
                            tim.TimbreName = System.IO.Path.GetFileNameWithoutExtension(binFile.Name);
                        }
                    }
                    break;
            }
        }
    }

    public enum PAULA_8364_Reg
    {
        REG_DMACONR = 0x02 / 2,
        REG_ADKCONR = 0x10 / 2,
        REG_DMACON = 0x96 / 2,
        REG_INTREQ = 0x9c / 2,
        REG_ADKCON = 0x9e / 2,
        REG_AUD0LCH = 0xa0 / 2,  // to be moved, not part of paula
        REG_AUD0LCL = 0xa2 / 2,  // to be moved, not part of paula
        REG_AUD0LEN = 0xa4 / 2,
        REG_AUD0PER = 0xa6 / 2,
        REG_AUD0VOL = 0xa8 / 2,
        REG_AUD0DAT = 0xaa / 2,
        REG_AUD1LCH = 0xb0 / 2,  // to be moved, not part of paula
        REG_AUD1LCL = 0xb2 / 2,  // to be moved, not part of paula
        REG_AUD1LEN = 0xb4 / 2,
        REG_AUD1PER = 0xb6 / 2,
        REG_AUD1VOL = 0xb8 / 2,
        REG_AUD1DAT = 0xba / 2,
        REG_AUD2LCH = 0xc0 / 2,  // to be moved, not part of paula
        REG_AUD2LCL = 0xc2 / 2,  // to be moved, not part of paula
        REG_AUD2LEN = 0xc4 / 2,
        REG_AUD2PER = 0xc6 / 2,
        REG_AUD2VOL = 0xc8 / 2,
        REG_AUD2DAT = 0xca / 2,
        REG_AUD3LCH = 0xd0 / 2,  // to be moved, not part of paula
        REG_AUD3LCL = 0xd2 / 2,  // to be moved, not part of paula
        REG_AUD3LEN = 0xd4 / 2,
        REG_AUD3PER = 0xd6 / 2,
        REG_AUD3VOL = 0xd8 / 2,
        REG_AUD3DAT = 0xda / 2
    };

    public enum PAULA_8364_Clock
    {
        PAL = 3546895,
        NTSC = 3579545,
    }

    public enum PAULA_CMD
    {
        Default = 0,
        Volume = 1,
        Pitch = 2,
        KeyOn = 3,
        KeyOff = 4,
        Filter = 5,
        SetPcmData = 6,
        SetPcmDataLoop = 7,
        SetLoop = 8,
    }


    /// <summary>
    /// 
    /// </summary>
    [TypeConverter(typeof(EnumConverter<PhysicalChannel>))]
    public enum PhysicalChannel
    {
        Indeterminatene = 0,
        IndeterminateneLeft = 1,
        IndeterminateneRight = 2,
        [Description("Ch1(Left)")]
        Ch1 = 3,
        [Description("Ch3(Left)")]
        Ch3 = 4,
        [Description("Ch2(Right)")]
        Ch2 = 5,
        [Description("Ch4(Right)")]
        Ch4 = 6,
    }
}
