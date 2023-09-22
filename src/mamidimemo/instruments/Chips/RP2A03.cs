// copyright-holders:K.Ito
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using FM_SoundConvertor;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Tools;
using Newtonsoft.Json;
using Omu.ValueInjecter;
using Omu.ValueInjecter.Injections;
using zanac.MAmidiMEmo.ComponentModel;
using zanac.MAmidiMEmo.Gui;
using zanac.MAmidiMEmo.Instruments.Envelopes;
using zanac.MAmidiMEmo.Mame;
using zanac.MAmidiMEmo.Midi;
using zanac.MAmidiMEmo.Properties;
using zanac.MAmidiMEmo.VSIF;
using static zanac.MAmidiMEmo.Instruments.Chips.NAMCO_CUS30;
using static zanac.MAmidiMEmo.Instruments.Chips.YM2612;

//http://hp.vector.co.jp/authors/VA042397/nes/apu.html
//https://wiki.nesdev.com/w/index.php/APU
//http://offgao.blog112.fc2.com/blog-entry-40.html

namespace zanac.MAmidiMEmo.Instruments.Chips
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    [InstLock]
    public partial class RP2A03 : InstrumentBase
    {
        private const int MAX_DAC_VOICES = 8;

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


        private PortId portId = PortId.No1;

        [DataMember]
        [Category("Chip(Dedicated)")]
        [Description("Set FTDI No for \"VSIF - NES\".\r\n" +
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
                    case SoundEngineType.VSIF_NES_FTDI:
                    case SoundEngineType.VSIF_NES_FTDI_FDS:
                        vsifClient = VsifManager.TryToConnectVSIF(VsifSoundModuleType.NES_FTDI_DIRECT, PortId, false);
                        if (vsifClient != null)
                        {
                            if (vsifClient.DataWriter.FtdiDeviceType == FTD2XX_NET.FTDI.FT_DEVICE.FT_DEVICE_232R)
                            {
                                if (FtdiClkWidth < 11)
                                    FtdiClkWidth = 11;
                            }
                            else
                            {
                                if (FtdiClkWidth < 27)
                                    FtdiClkWidth = 27;
                            }

                            f_CurrentSoundEngineType = f_SoundEngineType;
                            SetDevicePassThru(true);
                        }
                        else
                        {
                            f_CurrentSoundEngineType = SoundEngineType.Software;
                            SetDevicePassThru(false);
                        }
                        break;
                    case SoundEngineType.VSIF_NES_FTDI_VRC6:
                    case SoundEngineType.VSIF_NES_FTDI_MMC5:
                        vsifClient = VsifManager.TryToConnectVSIF(VsifSoundModuleType.NES_FTDI_INDIRECT, PortId, false);
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
            ignoreUpdatePcmData = true;
            ClearWrittenDataCache();
            ignoreUpdatePcmData = false;
            PrepareSound();
        }

        private int f_ftdiClkWidth = VsifManager.FTDI_BAUDRATE_NES_CLK_WIDTH;

        [DataMember]
        [Category("Chip(Dedicated)")]
        [SlideParametersAttribute(1, 100)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue(VsifManager.FTDI_BAUDRATE_NES_CLK_WIDTH)]
        [Description("Set FTDI Clock Width[%].\r\n" +
            "FT232R:11~\r\n" +
            "FT232H:27~")]
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


        private byte f_FdsMasterVolume;

        [DataMember]
        [Category("Chip(Dedicated)")]
        [Description("FDS Master Volume(0: full; 1: 2/3; 2: 2/4; 3: 2/5)")]
        [SlideParametersAttribute(0, 3)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((byte)0)]
        public byte FdsMasterVolume
        {
            get
            {
                return f_FdsMasterVolume;
            }
            set
            {
                if (f_FdsMasterVolume != (byte)(value & 3))
                {
                    f_FdsMasterVolume = (byte)(value & 3);
                    if(SoundEngine == SoundEngineType.VSIF_NES_FTDI_FDS)
                        RP2A03WriteData(UnitNumber, 0x89, f_FdsMasterVolume);
                }
            }
        }

        private bool f_UseAltVRC6Cart;

        [DataMember]
        [Category("Chip(Dedicated)")]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue(false)]
        [Description("Use alternative VRC6 Cart.")]
        public bool UseAltVRC6Cart
        {
            get
            {
                return f_UseAltVRC6Cart;
            }
            set
            {
                f_UseAltVRC6Cart = value;
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

        /// <summary>
        /// 
        /// </summary>
        public enum MasterClockType : uint
        {
            NTSC = 1789773,
            PAL = 1662607,
        }

        private uint f_MasterClock = (uint)MasterClockType.NTSC;

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
            return MasterClock != (uint)MasterClockType.NTSC;
        }

        public void ResetMasterClock()
        {
            MasterClock = (uint)MasterClockType.NTSC;
        }


        private DPcmSoundTable deltaPcmSoundTable;

        [DataMember]
        [Category("Chip")]
        [Description("Assign PCM data to DPCM soundtype instrument.\r\n" +
                    "Delta PCM Data (Max 4081 bytes)")]
        [Editor(typeof(PcmTableUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [PcmTableEditor("DMC File(*.dmc)|*.dmc")]
        [TypeConverter(typeof(CustomObjectTypeConverter))]
        public DPcmSoundTable DeltaPcmSoundTable
        {
            get
            {
                return deltaPcmSoundTable;
            }
            set
            {
                deltaPcmSoundTable = value;
                updateDpcmData();
            }
        }

        private byte[] lastTransferPcmData;

        /// <summary>
        /// 
        /// </summary>
        private void updateDpcmData()
        {
            lock (sndEnginePtrLock)
            {
                if (CurrentSoundEngine != SoundEngineType.VSIF_NES_FTDI_FDS &&
                    CurrentSoundEngine != SoundEngineType.VSIF_NES_FTDI_MMC5)
                    return;
            }
            List<byte> pcmData = new List<byte>();
            uint nextMinAddress = 0xC000;
            uint nextMaxAddress = 0xC000 + 0x2000;
            uint nextStartAddress = nextMinAddress;
            int bankLen = 0;
            for (int i = 0; i < deltaPcmSoundTable.PcmTimbres.Length; i++)
            {
                var tim = (DeltaPcmTimbre)deltaPcmSoundTable.PcmTimbres[i];

                tim.SampleAddress = nextStartAddress;
                if (tim.PcmData != null && tim.PcmData.Length != 0)
                {
                    int tlen = tim.PcmData.Length;
                    int pad = (0x40 - (tlen & 0x3f)) & 0x3f;    //64 byte pad
                    if (nextStartAddress + tlen + pad - 1 < nextMaxAddress)   //MAX 8KB
                    {
                        tim.SampleAddress = nextStartAddress;

                        //Write PCM data
                        pcmData.AddRange(tim.PcmData);
                        //Add 64 byte pad
                        for (int j = 0; j < pad; j++)
                            pcmData.Add(0xAA);  //Adds silent data

                        bankLen += tim.PcmData.Length + pad;

                        nextStartAddress = (uint)(nextMinAddress + bankLen);
                    }
                    else
                    {
                        if (CurrentSoundEngine == SoundEngineType.VSIF_NES_FTDI_FDS)
                        {
                            MessageBox.Show(Resources.DpcmBufferExceeded, "Warning", MessageBoxButtons.OK);
                            break;
                        }
                        else if (CurrentSoundEngine == SoundEngineType.VSIF_NES_FTDI_MMC5)
                        {
                            nextMinAddress += 0x10000;
                            nextMaxAddress += 0x10000;
                            nextStartAddress = nextMinAddress;

                            if (nextMinAddress >= 0x8C000)
                            {
                                MessageBox.Show(Resources.DpcmBufferExceeded, "Warning", MessageBoxButtons.OK);
                                break;
                            }
                            else
                            {
                                i--;
                                bankLen = 0;
                                continue;
                            }
                        }
                        break;
                    }
                }
            }
            if (pcmData.Count != 0)
            {
                int cnt = pcmData.Count;
                for (int i = cnt; i <= (cnt | 0x1fff); i++)
                    pcmData.Add(0xAA);  //Adds silent data

                if (lastTransferPcmData != null)
                {
                    bool diff = false;
                    for (int i = pcmData.Count; i < pcmData.Count; i++)
                    {
                        if (pcmData[i] != lastTransferPcmData[i])
                        {
                            diff = true;
                            break;
                        }
                    }
                    if (diff)
                        return;
                }
                lastTransferPcmData = pcmData.ToArray();

                try
                {
                    FormMain.AppliactionForm.Enabled = false;
                    using (FormProgress f = new FormProgress())
                    {
                        f.StartPosition = FormStartPosition.CenterScreen;
                        f.Message = Resources.UpdatingADPCM;
                        f.Show();

                        lock (sndEnginePtrLock)
                        {
                            if (CurrentSoundEngine == SoundEngineType.VSIF_NES_FTDI_FDS)
                            {
                                //Write DPCM
                                vsifClient.WriteData(0, (byte)0x16, 0, f_ftdiClkWidth);
                                for (int i = 0; i < pcmData.Count; i += 4)
                                {
                                    vsifClient.WriteData(0, pcmData[i + 0], pcmData[i + 1], f_ftdiClkWidth);
                                    vsifClient.WriteData(0, pcmData[i + 2], pcmData[i + 3], f_ftdiClkWidth);
                                    vsifClient.FlushDeferredWriteDataAndWait();
                                    vsifClient.RawWriteData(new byte[] { 0xff }, f_ftdiClkWidth);   //dummy wait
                                }
                            }
                            else if (CurrentSoundEngine == SoundEngineType.VSIF_NES_FTDI_MMC5)
                            {
                                for (int bn = 0; bn < pcmData.Count >> 13; bn++)
                                {
                                    //Switch bank
                                    vsifClient.WriteData(0, (byte)0x17, (byte)bn, f_ftdiClkWidth);

                                    //Write DPCM
                                    vsifClient.WriteData(0, (byte)0x16, 0, f_ftdiClkWidth);
                                    for (int i = bn * 0x2000; i < (bn * 0x2000) + 0x2000; i += 4)
                                    {
                                        vsifClient.WriteData(0, pcmData[i + 0], pcmData[i + 1], f_ftdiClkWidth);
                                        vsifClient.WriteData(0, pcmData[i + 2], pcmData[i + 3], f_ftdiClkWidth);
                                        vsifClient.FlushDeferredWriteDataAndWait();
                                        vsifClient.RawWriteData(new byte[] { 0xff }, f_ftdiClkWidth);   //dummy wait
                                    }
                                }
                            }
                        }
                    }
                }
                finally
                {
                    FormMain.AppliactionForm.Enabled = true;
                }
                /*
                FormProgress.RunDialog(Resources.UpdatingDPCM,
                        new Action<FormProgress>((f) =>
                        {
                            lock (sndEnginePtrLock)
                            {
                                if (CurrentSoundEngine == SoundEngineType.VSIF_NES_FTDI_FDS)
                                {
                                    //Write DPCM
                                    vsifClient.WriteData(0, (byte)0x16, 0, f_ftdiClkWidth);
                                    for (int i = 0; i < pcmData.Count; i += 2)
                                        vsifClient.WriteData(0, pcmData[i + 0], pcmData[i + 1], f_ftdiClkWidth);
                                }
                                else if (CurrentSoundEngine == SoundEngineType.VSIF_NES_FTDI_MMC5)
                                {
                                    for (int bn = 0; bn < pcmData.Count >> 13; bn++)
                                    {
                                        //Switch bank
                                        vsifClient.WriteData(0, (byte)0x17, (byte)bn, f_ftdiClkWidth);

                                        //Write DPCM
                                        vsifClient.WriteData(0, (byte)0x16, 0, f_ftdiClkWidth);
                                        for (int i = bn * 0x2000; i < (bn * 0x2000) + 0x2000; i += 2)
                                            vsifClient.WriteData(0, pcmData[i + 0], pcmData[i + 1], f_ftdiClkWidth);
                                    }
                                }
                            }
                        }));
                */
                FormMain.OutputLog(this, string.Format(Resources.DpcmBufferUsed, pcmData.Count));

                ClearWrittenDataCache();
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
                Timbres = (RP2A03Timbre[])value;
            }
        }

        public RP2A03Timbre[] f_Timbres;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category(" Timbres")]
        [Description("Timbres")]
        [EditorAttribute(typeof(TimbresArrayUITypeEditor), typeof(UITypeEditor))]
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
        /// <param name="unitNumber"></param>
        /// <param name="address"></param>
        /// <param name="data"></param>
        private void RP2A03WriteData(uint unitNumber, uint address, byte data)
        {
            RP2A03WriteData(unitNumber, address, data, true, false);
        }

        /// <summary>
        /// 
        /// </summary>
        private void RP2A03WriteData(uint unitNumber, uint address, byte data, bool useCache, bool sendVsifOnly)
        {
            WriteData(address, data, useCache, new Action(() =>
            {
                switch (CurrentSoundEngine)
                {
                    case SoundEngineType.VSIF_NES_FTDI:
                        switch (address)
                        {
                            case 0x11:
                                lock (sndEnginePtrLock)
                                    vsifClient.WriteData(new PortWriteData[] { new PortWriteData() { Address = (byte)address, Data = data, Type = 1, Wait = f_ftdiClkWidth } });
                                break;
                            case uint cmd when 0x0 <= address && address <= 0x15:
                                lock (sndEnginePtrLock)
                                    vsifClient.WriteData(0, (byte)address, data, f_ftdiClkWidth);
                                break;
                        }
                        break;
                    case SoundEngineType.VSIF_NES_FTDI_FDS:
                        switch (address)
                        {
                            case 0x11:
                                lock (sndEnginePtrLock)
                                    vsifClient.WriteData(new PortWriteData[] { new PortWriteData() { Address = (byte)address, Data = data, Type = 1, Wait = f_ftdiClkWidth } });
                                break;
                            case uint cmd when 0x0 <= address && address <= 0xff:
                                lock (sndEnginePtrLock)
                                    vsifClient.WriteData(0, (byte)address, data, f_ftdiClkWidth);
                                break;
                        }
                        break;
                    case SoundEngineType.VSIF_NES_FTDI_VRC6:
                        if (!UseAltVRC6Cart)
                        {
                            switch (address)
                            {
                                case 0x11:
                                    lock (sndEnginePtrLock)
                                        vsifClient.WriteData(new PortWriteData[] { new PortWriteData() { Address = (byte)address, Data = data, Type = 1, Wait = f_ftdiClkWidth } });
                                    break;
                                case uint cmd when 0x0 <= address && address <= 0x15:
                                    lock (sndEnginePtrLock)
                                        vsifClient.WriteData(0, (byte)address, data, f_ftdiClkWidth);
                                    break;
                                case uint cmd when 0x9000 <= address && address <= 0x9003:
                                    lock (sndEnginePtrLock)
                                        vsifClient.WriteData(0, (byte)(24 + (cmd & 0x03)), data, f_ftdiClkWidth);
                                    break;
                                case uint cmd when 0xa000 <= address && address <= 0xa003:
                                    lock (sndEnginePtrLock)
                                        vsifClient.WriteData(0, (byte)(28 + (cmd & 0x03)), data, f_ftdiClkWidth);
                                    break;
                                case uint cmd when 0xb000 <= address && address <= 0xb003:
                                    lock (sndEnginePtrLock)
                                        vsifClient.WriteData(0, (byte)(32 + (cmd & 0x03)), data, f_ftdiClkWidth);
                                    break;
                            }
                        }
                        else
                        {
                            switch (address)
                            {
                                case 0x11:
                                    lock (sndEnginePtrLock)
                                        vsifClient.WriteData(new PortWriteData[] { new PortWriteData() { Address = (byte)address, Data = data, Type = 1, Wait = f_ftdiClkWidth } });
                                    break;
                                case uint cmd when 0x0 <= address && address <= 0x15:
                                    lock (sndEnginePtrLock)
                                        vsifClient.WriteData(0, (byte)address, data, f_ftdiClkWidth);
                                    break;

                                case 0x9001:
                                    lock (sndEnginePtrLock)
                                        vsifClient.WriteData(0, (byte)(24 + (0x02 & 0x03)), data, f_ftdiClkWidth);
                                    break;
                                case 0x9002:
                                    lock (sndEnginePtrLock)
                                        vsifClient.WriteData(0, (byte)(24 + (0x01 & 0x03)), data, f_ftdiClkWidth);
                                    break;
                                case 0xa001:
                                    lock (sndEnginePtrLock)
                                        vsifClient.WriteData(0, (byte)(28 + (0x02 & 0x03)), data, f_ftdiClkWidth);
                                    break;
                                case 0xa002:
                                    lock (sndEnginePtrLock)
                                        vsifClient.WriteData(0, (byte)(28 + (0x01 & 0x03)), data, f_ftdiClkWidth);
                                    break;
                                case 0xb001:
                                    lock (sndEnginePtrLock)
                                        vsifClient.WriteData(0, (byte)(32 + (0x02 & 0x03)), data, f_ftdiClkWidth);
                                    break;
                                case 0xb002:
                                    lock (sndEnginePtrLock)
                                        vsifClient.WriteData(0, (byte)(32 + (0x01 & 0x03)), data, f_ftdiClkWidth);
                                    break;

                                case uint cmd when 0x9000 <= address && address <= 0x9003:
                                    lock (sndEnginePtrLock)
                                        vsifClient.WriteData(0, (byte)(24 + (cmd & 0x03)), data, f_ftdiClkWidth);
                                    break;
                                case uint cmd when 0xa000 <= address && address <= 0xa003:
                                    lock (sndEnginePtrLock)
                                        vsifClient.WriteData(0, (byte)(28 + (cmd & 0x03)), data, f_ftdiClkWidth);
                                    break;
                                case uint cmd when 0xb000 <= address && address <= 0xb003:
                                    lock (sndEnginePtrLock)
                                        vsifClient.WriteData(0, (byte)(32 + (cmd & 0x03)), data, f_ftdiClkWidth);
                                    break;
                            }
                        }
                        break;
                    case SoundEngineType.VSIF_NES_FTDI_MMC5:
                        switch (address)
                        {
                            case 0x11:
                                lock (sndEnginePtrLock)
                                    vsifClient.WriteData(new PortWriteData[] { new PortWriteData() { Address = (byte)address, Data = data, Type = 1, Wait = f_ftdiClkWidth } });
                                break;
                            case uint cmd when 0x0 <= address && address <= 0x17:
                                lock (sndEnginePtrLock)
                                    vsifClient.WriteData(0, (byte)address, data, f_ftdiClkWidth);
                                break;
                            case uint cmd when 0x5000 <= address && address <= 0x5007:
                                lock (sndEnginePtrLock)
                                    vsifClient.WriteData(0, (byte)(24 + (cmd & 0x07)), data, f_ftdiClkWidth);
                                break;
                            case uint cmd when 0x5010 <= address && address <= 0x5011:
                                lock (sndEnginePtrLock)
                                    vsifClient.WriteData(0, (byte)(28 + (cmd & 0x01)), data, f_ftdiClkWidth);
                                break;
                        }
                        break;
                }
                if (!sendVsifOnly)
                    DeferredWriteData(RP2A03_write, unitNumber, address, data);
            }));

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
        private byte RP2A03ReadData(uint unitNumber, uint address)
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
            deltaPcmSoundTable = new DPcmSoundTable();

            this.pcmEngine = new PcmEngine(this);
            this.pcmEngine.StartEngine();

            setPresetInstruments();

            this.soundManager = new RP2A03SoundManager(this);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Dispose()
        {
            soundManager?.Dispose();
            pcmEngine?.Dispose();

            lock (sndEnginePtrLock)
            {
                if (vsifClient != null)
                    vsifClient.Dispose();
            }

            base.Dispose();
        }

        internal override void PrepareSound()
        {
            base.PrepareSound();

            initGlobalRegisters();
        }

        private void initGlobalRegisters()
        {


            lock (sndEnginePtrLock)
                lastTransferPcmData = new byte[] { };

            if (!IsDisposing && !ignoreUpdatePcmData)
                updateDpcmData();

            if (SoundEngine == SoundEngineType.VSIF_NES_FTDI_FDS)
                RP2A03WriteData(UnitNumber, 0x89, f_FdsMasterVolume);
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

        private bool ignoreUpdatePcmData;

        /// <summary>
        /// 
        /// </summary>
        protected override void ClearWrittenDataCache()
        {
            base.ClearWrittenDataCache();

            if (!IsDisposing && !ignoreUpdatePcmData)
                updateDpcmData();
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

            private static SoundList<RP2A03Sound> dacOnSounds = new SoundList<RP2A03Sound>(MAX_DAC_VOICES);

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

                int tindex = 0;
                foreach (RP2A03Timbre timbre in parentModule.GetBaseTimbres(note))
                {
                    tindex++;
                    var emptySlot = searchEmptySlot(note, timbre);
                    if (emptySlot.slot < 0)
                        continue;

                    RP2A03Sound snd = new RP2A03Sound(emptySlot.inst, this, timbre, tindex - 1, note, emptySlot.slot);
                    switch (timbre.ToneType)
                    {
                        case ToneType.SQUARE:
                            sqOnSounds.Add(snd);
                            FormMain.OutputDebugLog(parentModule, "KeyOn SQ ch" + emptySlot + " " + note.ToString());
                            break;
                        case ToneType.TRIANGLE:
                            triOnSounds.Add(snd);
                            FormMain.OutputDebugLog(parentModule, "KeyOn Tri ch" + emptySlot + " " + note.ToString());
                            break;
                        case ToneType.NOISE:
                            noiseOnSounds.Add(snd);
                            FormMain.OutputDebugLog(parentModule, "KeyOn Noise ch" + emptySlot + " " + note.ToString());
                            break;
                        case ToneType.DPCM:
                            dpcmOnSounds.Add(snd);
                            FormMain.OutputDebugLog(parentModule, "KeyOn DPCM ch" + emptySlot + " " + note.ToString());
                            break;
                        case ToneType.FDS:
                            fdsOnSounds.Add(snd);
                            FormMain.OutputDebugLog(parentModule, "KeyOn FDS ch" + emptySlot + " " + note.ToString());
                            break;
                        case ToneType.VRC6_SQ:
                            vrc6SqOnSounds.Add(snd);
                            FormMain.OutputDebugLog(parentModule, "KeyOn VRC6(SQ) ch" + emptySlot + " " + note.ToString());
                            break;
                        case ToneType.VRC6_SAW:
                            vrc6SawOnSounds.Add(snd);
                            FormMain.OutputDebugLog(parentModule, "KeyOn VRC6(Saw) ch" + emptySlot + " " + note.ToString());
                            break;
                        case ToneType.DAC:
                            dacOnSounds.Add(snd);
                            FormMain.OutputDebugLog(parentModule, "KeyOn DAC ch" + emptySlot + " " + note.ToString());
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
                    case ToneType.DAC:
                        {
                            return SearchEmptySlotAndOffForLeader(parentModule, dacOnSounds, note, MAX_DAC_VOICES);
                        }
                }

                return emptySlot;
            }

            internal override void ProcessAllSoundOff()
            {
                var me = new ControlChangeEvent((SevenBitNumber)120, (SevenBitNumber)0);
                ProcessControlChange(me);

                parentModule.RP2A03WriteData(parentModule.UnitNumber, 0x08, 0x80);
                parentModule.RP2A03WriteData(parentModule.UnitNumber, 0x15, 0);

                parentModule.RP2A03WriteData(parentModule.UnitNumber, 0x83, 0xc0);

                parentModule.RP2A03WriteData(parentModule.UnitNumber, (uint)(0x9002 + (0 << 12)), 0x00);
                parentModule.RP2A03WriteData(parentModule.UnitNumber, (uint)(0x9002 + (1 << 12)), 0x00);

                parentModule.RP2A03WriteData(parentModule.UnitNumber, 0xb002, 0x00);

                for (int i = 0; i < MAX_DAC_VOICES; i++)
                    parentModule.pcmEngine.Stop(i);
            }
        }

        private PcmEngine pcmEngine;


        /// <summary>
        /// 
        /// </summary>
        private class PcmEngine : IDisposable
        {
            private RP2A03Sound sound;

            private object engineLockObject;

            private bool stopEngineFlag;

            private bool disposedValue;


            private RP2A03 parentModule;

            private uint unitNumber;

            private SampleData[] currentSampleData;

            /// <summary>
            /// 
            /// </summary>
            public PcmEngine(RP2A03 parentModule)
            {
                this.parentModule = parentModule;
                unitNumber = parentModule.UnitNumber;
                engineLockObject = new object();
                stopEngineFlag = true;
                currentSampleData = new SampleData[RP2A03.MAX_DAC_VOICES];
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
            public void Play(RP2A03Sound sound, TaggedNoteOnEvent note, int slot, RP2A03Timbre pcmTimbre, double freq, double volume)
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

                    var sd = new SampleData(sound, note, pcmTimbre.DAC.PcmData, pcmTimbre.DAC.SampleRate, false, basefreq);
                    sd.Gain = pcmTimbre.DAC.PcmGain;
                    sd.Pitch = freq / basefreq;
                    sd.Volume = volume;
                    sd.LoopEnabled = pcmTimbre.DAC.LoopEnabled;
                    sd.LoopPoint = pcmTimbre.DAC.LoopPoint;
                    currentSampleData[slot] = sd;

                    //keyoff
                    byte data = (byte)(parentModule.RP2A03ReadData(parentModule.UnitNumber, 0x15) & ~(1 << 4));
                    parentModule.RP2A03WriteData(parentModule.UnitNumber, 0x15, (byte)data);

                    //var data = new PortWriteData() { Type = (byte)6, Address = (byte)slot, Data = 1, Tag = new Dictionary<string, object>() };
                    //data.Tag["PcmData"] = pcmTimbre.DAC.PcmData;
                    //data.Tag["PcmGain"] = pcmTimbre.DAC.PcmGain;
                    //parentModule.XgmWriter?.RecordData(data);
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
                    if(currentSampleData[slot] != null)
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

                    //parentModule.XgmWriter?.RecordData(new PortWriteData()
                    //{ Type = (byte)6, Address = (byte)slot, Data = 0 });
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
                int overflowed = 0;

                long freq, before, after;
                double dbefore;
                QueryPerformanceFrequency(out freq);
                QueryPerformanceCounter(out before);
                dbefore = before;
                while (!stopEngineFlag)
                {
                    if (disposedValue)
                        break;

                    int dacData = 0;
                    bool playDac = false;
                    uint sampleRate = 11025;

                    {
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
                                dacData += val;

                                playDac = true;
                            }
                        }

                        if (playDac || overflowed != 0)
                        {
                            //dacData += overflowed;
                            overflowed = 0;
                            if (dacData > sbyte.MaxValue)
                            {
                                //overflowed = dacData - sbyte.MaxValue;
                                dacData = sbyte.MaxValue;
                            }
                            else if (dacData < sbyte.MinValue)
                            {
                                //overflowed = dacData - sbyte.MinValue;
                                dacData = sbyte.MinValue;
                            }
                            parentModule.RP2A03WriteData(unitNumber, 0x11, (byte)((dacData + 0x80) >> 1), false, false);
                        }
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

        /// <summary>
        /// 
        /// </summary>
        private class SampleData
        {
            private RP2A03Sound sound;

            public byte[] PcmData
            {
                get;
                private set;
            }

            public uint SampleRate
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
            public SampleData(RP2A03Sound sound, TaggedNoteOnEvent note, byte[] pcmData, uint sampleRate, bool disableVelocity, double baseFreq)
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

            public byte? GetDacData(uint sampleRate)
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
        private class RP2A03Sound : SoundBase
        {

            private RP2A03 parentModule;

            private RP2A03Timbre timbre;

            private ToneType lastToneType;

            private sbyte[] lastLfoData;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentModule"></param>
            /// <param name="noteOnEvent"></param>
            /// <param name="programNumber"></param>
            /// <param name="slot"></param>
            public RP2A03Sound(RP2A03 parentModule, RP2A03SoundManager manager, TimbreBase timbre, int tindex, TaggedNoteOnEvent noteOnEvent, int slot) : base(parentModule, manager, timbre, tindex, noteOnEvent, slot)
            {
                this.parentModule = parentModule;
                this.timbre = (RP2A03Timbre)timbre;

                lastToneType = this.timbre.ToneType;
                lastLfoData = this.timbre.FDS.LfoData;
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
                            byte data = (byte)parentModule.RP2A03ReadData(parentModule.UnitNumber, 0x15);
                            parentModule.RP2A03WriteData(parentModule.UnitNumber, 0x15, (byte)(data | (1 << Slot)));

                            parentModule.RP2A03WriteData(parentModule.UnitNumber, (uint)((Slot * 4) + 0x01),
                                (byte)(timbre.SQSweep.Enable << 7 | timbre.SQSweep.UpdateRate << 4 |
                                timbre.SQSweep.Direction << 3 | timbre.SQSweep.Range));

                            //Volume
                            updateSqVolume();
                            //Freq
                            updateSqPitch(true);

                            break;
                        }
                    case ToneType.TRIANGLE:
                        {
                            byte data = (byte)parentModule.RP2A03ReadData(parentModule.UnitNumber, 0x15);
                            parentModule.RP2A03WriteData(parentModule.UnitNumber, 0x15, (byte)(data | (1 << 2)));

                            parentModule.RP2A03WriteData(parentModule.UnitNumber, (uint)((2 * 4) + 0x00),
                                (byte)(timbre.LengthCounterDisable << 7 | timbre.TriCounterLength));

                            //Volume
                            updateTriVolume();

                            //Freq
                            updateTriPitch();

                            break;
                        }
                    case ToneType.NOISE:
                        {
                            byte data = (byte)parentModule.RP2A03ReadData(parentModule.UnitNumber, 0x15);
                            parentModule.RP2A03WriteData(parentModule.UnitNumber, 0x15, (byte)(data | 8));

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
                            byte data = (byte)(parentModule.RP2A03ReadData(parentModule.UnitNumber, 0x15) & ~(1 << 4));
                            parentModule.RP2A03WriteData(parentModule.UnitNumber, 0x15, (byte)data);

                            // Loop / Smple Rate
                            parentModule.RP2A03WriteData(parentModule.UnitNumber, (uint)0x10,
                                (byte)(timbre.DeltaPcmLoopEnable << 6 | timbre.DeltaPcmBitRate));

                            //Size
                            var dpcm = (DeltaPcmTimbre)parentModule.DeltaPcmSoundTable.PcmTimbres[noteNum];
                            if (dpcm.PcmData != null)
                            {
                                RP2A03SetDpcm(parentModule.UnitNumber, dpcm.PcmData);
                                int sz = dpcm.PcmData.Length - 1;
                                if (sz > 4081)
                                    sz = 4081;
                                if (sz >= 16)
                                {
                                    parentModule.RP2A03WriteData(parentModule.UnitNumber, (uint)0x12, (byte)((dpcm.SampleAddress >> 6) & 0xff), true, true);
                                    parentModule.RP2A03WriteData(parentModule.UnitNumber, (uint)0x13, (byte)((sz / 16) & 0xff));

                                    if (parentModule.CurrentSoundEngine == SoundEngineType.VSIF_NES_FTDI_MMC5)
                                    {
                                        //Switch bank
                                        parentModule.RP2A03WriteData(parentModule.UnitNumber, 0x17, (byte)(dpcm.SampleAddress >> 16), true, true);
                                    }
                                    //keyon
                                    parentModule.RP2A03WriteData(parentModule.UnitNumber, 0x15, (byte)(data | 16));
                                }
                            }

                            break;
                        }
                    case ToneType.FDS:
                        {
                            //Set WSG
                            parentModule.RP2A03WriteData(parentModule.UnitNumber, 0x89, (byte)(0x80 | parentModule.FdsMasterVolume));
                            for (int i = 0; i < timbre.FDS.WsgData.Length; i++)
                                parentModule.RP2A03WriteData(parentModule.UnitNumber, (uint)(0x40 + i), (byte)(timbre.FDS.WsgData[i]));
                            parentModule.RP2A03WriteData(parentModule.UnitNumber, 0x89, parentModule.FdsMasterVolume);
                            parentModule.RP2A03WriteData(parentModule.UnitNumber, 0x85, (byte)timbre.FDS.LfoBias);

                            //Set LFO
                            if (timbre.FDS.LfoFreq == 0 && timbre.FDS.LfoFreqMultiply == 0)
                            {
                                parentModule.RP2A03WriteData(parentModule.UnitNumber, 0x86, (byte)0);
                                parentModule.RP2A03WriteData(parentModule.UnitNumber, 0x87, (byte)0);
                            }
                            else
                            {
                                parentModule.RP2A03WriteData(parentModule.UnitNumber, 0x84, (byte)(0x80 | timbre.FDS.LfoGain));

                                parentModule.RP2A03WriteData(parentModule.UnitNumber, 0x87, (byte)0x80);
                                for (int i = 0; i < lastLfoData.Length; i++)
                                    parentModule.RP2A03WriteData(parentModule.UnitNumber, (uint)(0x88), (byte)(lastLfoData[i] & 7), false, false);

                                double dlfrq = timbre.FDS.LfoFreq;
                                if (timbre.FDS.LfoFreqMultiply > 0)
                                    dlfrq = calcFdsPitch() * timbre.FDS.LfoFreqMultiply;
                                ushort lfrq = (ushort)Math.Round(dlfrq);
                                if (lfrq > 0xfff)
                                    lfrq = 0xfff;
                                parentModule.RP2A03WriteData(parentModule.UnitNumber, 0x86, (byte)(lfrq & 0xff));
                                parentModule.RP2A03WriteData(parentModule.UnitNumber, 0x87, (byte)((lfrq >> 8) & 0xf));
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
                    case ToneType.DAC:
                        {
                            parentModule.pcmEngine.Play(this, NoteOnEvent, Slot, timbre, CalcCurrentFrequency(), CalcCurrentVolume());
                            break;
                        }
                }
            }


            public override void OnSoundParamsUpdated()
            {
                switch (lastToneType)
                {
                    case ToneType.SQUARE:
                        {
                            parentModule.RP2A03WriteData(parentModule.UnitNumber, (uint)((Slot * 4) + 0x01),
                                (byte)(timbre.SQSweep.Enable << 7 | timbre.SQSweep.UpdateRate << 4 |
                                timbre.SQSweep.Direction << 3 | timbre.SQSweep.Range));

                            break;
                        }
                    case ToneType.TRIANGLE:
                        {
                            break;
                        }
                    case ToneType.NOISE:
                        {
                            break;
                        }
                    case ToneType.DPCM:
                        {
                            //https://wiki.nesdev.com/w/index.php/APU_DMC

                            // Loop / Smple Rate
                            parentModule.RP2A03WriteData(parentModule.UnitNumber, (uint)0x10,
                                (byte)(timbre.DeltaPcmLoopEnable << 6 | timbre.DeltaPcmBitRate));

                            break;
                        }
                    case ToneType.FDS:
                        {
                            //Set WSG
                            parentModule.RP2A03WriteData(parentModule.UnitNumber, 0x89, (byte)(0x80 | parentModule.FdsMasterVolume));
                            for (int i = 0; i < timbre.FDS.WsgData.Length; i++)
                                parentModule.RP2A03WriteData(parentModule.UnitNumber, (uint)(0x40 + i), (byte)(timbre.FDS.WsgData[i]));
                            parentModule.RP2A03WriteData(parentModule.UnitNumber, 0x89, parentModule.FdsMasterVolume);

                            //Set LFO
                            var eng = FxEngine as NesFxEngine;
                            var lfreq = timbre.FDS.LfoFreq;
                            var lmul = timbre.FDS.LfoFreqMultiply;
                            if (eng != null && eng.Active)
                            {
                                bool changed = false;
                                if (eng.LfoFreqValue != null)
                                {
                                    lfreq = (byte)(eng.LfoFreqValue.Value & 63);
                                    changed = true;
                                }
                                if (eng.LfoFreqMultiplyValue != null)
                                {
                                    lmul = eng.LfoFreqMultiplyValue.Value;
                                    changed = true;
                                }
                                if (changed)
                                {
                                    //Set LFO
                                    if (lmul > 0)
                                    {
                                        var dlfrq = calcFdsPitch() * lmul;
                                        lfreq = (uint)Math.Round(dlfrq);
                                        if (lfreq > 0xfff)
                                            lfreq = 0xfff;
                                    }
                                    parentModule.RP2A03WriteData(parentModule.UnitNumber, 0x86, (byte)(lfreq & 0xff));
                                    parentModule.RP2A03WriteData(parentModule.UnitNumber, 0x87, (byte)((lfreq >> 8) & 0xf));
                                }
                                if (eng.LfoGainValue != null)
                                {
                                    var lgain = (byte)(eng.LfoGainValue.Value & 63);
                                    parentModule.RP2A03WriteData(parentModule.UnitNumber, 0x84, (byte)(0x80 | lgain));
                                }
                                if (eng.LfoBiasValue != null)
                                {
                                    var lbias = (sbyte)(eng.LfoBiasValue.Value & 0x7f);
                                    parentModule.RP2A03WriteData(parentModule.UnitNumber, 0x85, (byte)lbias);
                                }
                            }

                            if (lfreq == 0 && lmul == 0)
                            {
                                parentModule.RP2A03WriteData(parentModule.UnitNumber, 0x86, (byte)0x00);
                                parentModule.RP2A03WriteData(parentModule.UnitNumber, 0x87, (byte)0x00);
                            }
                            else
                            {
                                if (eng.MorphValue != null)
                                {
                                    var no = (byte)(eng.MorphValue.Value & 3);
                                    {
                                        var ov = parentModule.RP2A03ReadData(parentModule.UnitNumber, 0x87);
                                        parentModule.RP2A03WriteData(parentModule.UnitNumber, 0x87, (byte)0x80);

                                        //lastLfoTable = no;
                                        //sbyte[] lfoData;
                                        switch (no)
                                        {
                                            case 1:
                                            case 2:
                                            case 3:
                                                lastLfoData = timbre.FDS.LfoMorphData[no - 1].LfoData;
                                                break;
                                            default:
                                                lastLfoData = timbre.FDS.LfoData;
                                                break;
                                        }

                                        for (int i = 0; i < lastLfoData.Length; i++)
                                            parentModule.RP2A03WriteData(parentModule.UnitNumber, (uint)(0x88), (byte)(lastLfoData[i] & 0x7), false, false);

                                        parentModule.RP2A03WriteData(parentModule.UnitNumber, 0x87, (byte)(ov & 0xf));
                                    }
                                }
                            }

                            break;
                        }
                    case ToneType.VRC6_SQ:
                        break;
                    case ToneType.VRC6_SAW:
                        break;
                    case ToneType.DAC:
                        {
                            break;
                        }
                }

                base.OnSoundParamsUpdated();
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
                    case ToneType.DAC:
                        {
                            parentModule.pcmEngine.Volume(Slot, CalcCurrentVolume());
                            break;
                        }
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

                parentModule.RP2A03WriteData(parentModule.UnitNumber, (uint)((Slot * 4) + 0x00), (byte)(dc << 6 | ld << 5 | dd << 4 | fv), false, false);
            }

            /// <summary>
            /// 
            /// </summary>
            private void updateTriVolume()
            {
                if (IsSoundOff)
                    return;

                var fv = Math.Round(timbre.Volume * CalcCurrentVolume());

                if (fv < 0.01)
                {
                    byte data = (byte)(parentModule.RP2A03ReadData(parentModule.UnitNumber, 0x15) & ~(1 << 2));
                    parentModule.RP2A03WriteData(parentModule.UnitNumber, 0x15, data);
                }
                else
                {
                    byte data = (byte)parentModule.RP2A03ReadData(parentModule.UnitNumber, 0x15);
                    parentModule.RP2A03WriteData(parentModule.UnitNumber, 0x15, (byte)(data | (1 << 2)));

                    parentModule.RP2A03WriteData(parentModule.UnitNumber, (uint)((2 * 4) + 0x00),
                        (byte)(timbre.LengthCounterDisable << 7 | timbre.TriCounterLength), false, false);
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

                parentModule.RP2A03WriteData(parentModule.UnitNumber, (uint)(0x0c), (byte)(ld << 5 | dd << 4 | fv));
            }

            /// <summary>
            /// 
            /// </summary>
            private void updateDpcmVolume()
            {
                var vol = parentModule.Volumes[NoteOnEvent.Channel];

                parentModule.RP2A03WriteData(parentModule.UnitNumber, (uint)(0x11), vol);
            }


            /// <summary>
            /// 
            /// </summary>
            private void updateFdsVolume()
            {
                parentModule.RP2A03WriteData(parentModule.UnitNumber, 0x80, (byte)(0x80 + (int)(CalcCurrentVolume() * 32)));
                //parentModule.RP2A03WriteData(parentModule.UnitNumber, 0x89, 0); //calcFdsVol());
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

                parentModule.RP2A03WriteData(parentModule.UnitNumber, (uint)(0x9000 + (Slot << 12)), (byte)(dc << 4 | fv));
            }

            private void updateVrc6SawVolume()
            {
                byte fv = (byte)((byte)Math.Round(63 * CalcCurrentVolume()) & 0x3f);

                parentModule.RP2A03WriteData(parentModule.UnitNumber, (uint)(0xb000), fv);
            }

            /// <summary>
            /// 
            /// </summary>
            public override void OnPitchUpdated()
            {
                switch (lastToneType)
                {
                    case ToneType.SQUARE:
                        updateSqPitch(false);
                        break;
                    case ToneType.TRIANGLE:
                        updateTriPitch();
                        break;
                    case ToneType.NOISE:
                        updateNoisePitch();
                        break;
                    case ToneType.FDS:
                        if (timbre.FDS.LfoFreqMultiply > 0)
                        {
                            double dlfrq = calcFdsPitch() * timbre.FDS.LfoFreqMultiply;
                            ushort lfrq = (ushort)Math.Round(dlfrq);
                            if (lfrq > 0xfff)
                                lfrq = 0xfff;
                            parentModule.RP2A03WriteData(parentModule.UnitNumber, 0x86, (byte)(lfrq & 0xff));
                            parentModule.RP2A03WriteData(parentModule.UnitNumber, 0x87, (byte)((lfrq >> 8) & 0xf));
                        }
                        updateFdsPitch();
                        break;
                    case ToneType.VRC6_SQ:
                        updateVrc6SQPitch();
                        break;
                    case ToneType.VRC6_SAW:
                        updateVrc6SawPitch();
                        break;
                    case ToneType.DAC:
                        {
                            parentModule.pcmEngine.Pitch(Slot, CalcCurrentFrequency());
                            break;
                        }
                }

                base.OnPitchUpdated();
            }

            private void updateSqPitch(bool keyOn)
            {
                double freq = CalcCurrentFrequency();
                freq = Math.Round(parentModule.MasterClock / (freq * 16)) - 1;
                var n = (ushort)freq;
                if (n > 0x7f0)
                    n = 0x7f0;

                parentModule.RP2A03WriteData(parentModule.UnitNumber, (uint)((Slot * 4) + 0x02), (byte)(n & 0xff), false, false);
                parentModule.RP2A03WriteData(parentModule.UnitNumber, (uint)((Slot * 4) + 0x03), (byte)((timbre.PlayLength << 3) | (n >> 8) & 0x7), !keyOn, false);
            }

            private void updateTriPitch()
            {
                double freq = CalcCurrentFrequency();
                freq = Math.Round(parentModule.MasterClock / (freq * 32)) - 1;
                var n = (ushort)freq;
                if (n > 0x7ff)
                    n = 0x7ff;

                parentModule.RP2A03WriteData(parentModule.UnitNumber, (uint)((2 * 4) + 0x02), (byte)(n & 0xff), false, false);
                parentModule.RP2A03WriteData(parentModule.UnitNumber, (uint)((2 * 4) + 0x03), (byte)((timbre.PlayLength << 3) | (n >> 8) & 0x7), false, false);
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

                parentModule.RP2A03WriteData(parentModule.UnitNumber, (uint)((3 * 4) + 0x02), (byte)((nt << 7) | (n & 0xf)));
                parentModule.RP2A03WriteData(parentModule.UnitNumber, (uint)((3 * 4) + 0x03), (byte)(timbre.PlayLength << 3), false, false);
            }

            private void updateFdsPitch()
            {
                if (IsSoundOff)
                    return;
                ushort n = calcFdsPitch();

                parentModule.RP2A03WriteData(parentModule.UnitNumber, 0x82, (byte)(n & 0xff));
                parentModule.RP2A03WriteData(parentModule.UnitNumber, 0x83, (byte)((n >> 8) & 0xf));

                return;
            }

            private ushort calcFdsPitch()
            {
                double freq = CalcCurrentFrequency();
                // p = 65536 * f / 1789773d
                freq = Math.Round(64 * 65536 * freq / parentModule.MasterClock);
                var n = (ushort)freq;
                if (n > 0xfff)
                    n = 0xfff;
                return n;
            }

            private void updateVrc6SQPitch()
            {
                if(IsSoundOff)
                    return;
                double freq = CalcCurrentFrequency();
                freq = Math.Round(parentModule.MasterClock / (16 * freq)) - 1;
                var n = (ushort)freq;
                if (n > 0xfff)
                    n = 0xfff;

                parentModule.RP2A03WriteData(parentModule.UnitNumber, (uint)(0x9001 + (Slot << 12)), (byte)(n & 0xff), false, false);
                parentModule.RP2A03WriteData(parentModule.UnitNumber, (uint)(0x9002 + (Slot << 12)), (byte)(0x80 | (n >> 8) & 0xf), false, false);
            }

            private void updateVrc6SawPitch()
            {
                if (IsSoundOff)
                    return;
                double freq = CalcCurrentFrequency();
                //t = (CPU / (14 * f)) - 1
                freq = Math.Round((parentModule.MasterClock / (14 * freq)) - 1);
                var n = (ushort)freq;
                if (n > 0xfff)
                    n = 0xfff;

                parentModule.RP2A03WriteData(parentModule.UnitNumber, (uint)(0xB001), (byte)(n & 0xff), false, false);
                parentModule.RP2A03WriteData(parentModule.UnitNumber, (uint)(0xB002), (byte)(0x80 | ((n >> 8) & 0xf)), false, false);
            }

            public override void SoundOff()
            {
                base.SoundOff();

                switch (lastToneType)
                {
                    case ToneType.SQUARE:
                        {
                            byte data = (byte)(parentModule.RP2A03ReadData(parentModule.UnitNumber, 0x15) & ~(1 << Slot));
                            parentModule.RP2A03WriteData(parentModule.UnitNumber, 0x15, data);
                            break;
                        }
                    case ToneType.TRIANGLE:
                        {
                            parentModule.RP2A03WriteData(parentModule.UnitNumber, 0x08, 0x80);

                            byte data = (byte)(parentModule.RP2A03ReadData(parentModule.UnitNumber, 0x15) & ~(1 << 2));
                            parentModule.RP2A03WriteData(parentModule.UnitNumber, 0x15, data);
                            break;
                        }
                    case ToneType.NOISE:
                        {
                            byte data = (byte)(parentModule.RP2A03ReadData(parentModule.UnitNumber, 0x15) & ~8);
                            parentModule.RP2A03WriteData(parentModule.UnitNumber, 0x15, data);
                            break;
                        }
                    case ToneType.FDS:
                        {
                            parentModule.RP2A03WriteData(parentModule.UnitNumber, 0x83, 0xc0);
                            break;
                        }
                    case ToneType.VRC6_SQ:
                        {
                            parentModule.RP2A03WriteData(parentModule.UnitNumber, (uint)(0x9002 + (Slot << 12)), 0x00);
                            break;
                        }
                    case ToneType.VRC6_SAW:
                        {
                            parentModule.RP2A03WriteData(parentModule.UnitNumber, 0xb002, 0x00);
                            break;
                        }
                    case ToneType.DAC:
                        {
                            parentModule.pcmEngine.Stop(Slot);
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
        [InstLock]
        public class RP2A03Timbre : TimbreBase
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
            [Description("DPCM Sample Bit Rate\r\n" +
                "00:  4181.71 Hz C-8  -1.78c\r\n" +
                "01:  4709.93 Hz D-8  +4.16c\r\n" +
                "02:  5264.04 Hz E-8  -3.29c\r\n" +
                "03:  5593.04 Hz F-8  +1.67c\r\n" +
                "04:  6257.95 Hz G-8  -3.86c\r\n" +
                "05:  7046.35 Hz A-8  +1.56c\r\n" +
                "06:  7919.35 Hz B-8  +3.77c\r\n" +
                "07:  8363.42 Hz C-9  -1.78c\r\n" +
                "08:  9419.86 Hz D-9  +4.16c\r\n" +
                "09: 11186.10 Hz F-9  +1.67c\r\n" +
                "10: 12604.00 Hz G-9  +8.29c\r\n" +
                "11: 13982.60 Hz A-9  -12.0c\r\n" +
                "12: 16884.60 Hz C-10 +14.5c\r\n" +
                "13: 21306.80 Hz E-10 +17.2c\r\n" +
                "14: 24858.00 Hz G-10 -15.9c\r\n" +
                "15: 33143.90 Hz C-11 -17.9c")]
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

            [DataMember]
            [Category("Sound(DAC)")]
            [Description("DAC Settings")]
            public DacSettings DAC
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
                FDS = new FdsSettings();
                VRC6 = new Vrc6Settings();
                DAC = new DacSettings();
            }

            protected override void InitializeFxS()
            {
                SDS.FxS = new NesFxSettings();
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


        [JsonConverter(typeof(NoTypeConverterJsonConverter<FdsSettings>))]
        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [DataContract]
        [InstLock]
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

            private sbyte f_LfoBias;

            [DataMember]
            [Category("Sound(FDS)")]
            [Description("Set FDS LFO Bias Directly(-64 - 63) when LfoGain and LfoFreq is 0")]
            [SlideParametersAttribute(-64, 63)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue(typeof(sbyte), "0")]
            public sbyte LfoBias
            {
                get
                {
                    return f_LfoBias;
                }
                set
                {
                    if (value > 63)
                        value = 63;
                    else if (value < -64)
                        value = -64;
                    f_LfoBias = (sbyte)value;
                }
            }

            private double f_LfoFreqMultiply;

            [DataMember]
            [Category("Sound(FDS)")]
            [Description("Synchronize LFO frequency with multiplied Note frequency when set the value.")]
            [DoubleSlideParametersAttribute(0, 8, 0.01)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public double LfoFreqMultiply
            {
                get
                {
                    return f_LfoFreqMultiply;
                }
                set
                {
                    if (value >= 0)
                        f_LfoFreqMultiply = value;
                }
            }

            public bool ShouldSerializeLfoFreqMultiply()
            {
                return LfoFreqMultiply != 0;
            }

            public void ResetLfoFreqMultiply()
            {
                LfoFreqMultiply = 0;
            }

            private sbyte[] f_LfoData = new sbyte[32];

            [TypeConverter(typeof(ArrayConverter))]
            [Editor(typeof(WsgUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [WsgBitWideAttribute(3)]
            [DataMember]
            [Category("Sound(FDS)")]
            [Description("FDS LFO Table (32 steps, -3～0～+3 levels)\r\n" +
                "-4 resets LFO bias.")]
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
                    for (int i = 0; i < Math.Min(LfoData.Length, vs.Count); i++)
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

            private NesLfoMorphData[] f_lfoMorphData = new NesLfoMorphData[3] {
                new NesLfoMorphData(),
                new NesLfoMorphData(),
                new NesLfoMorphData()
            };

            [TypeConverter(typeof(ArrayConverter))]
            [EditorAttribute(typeof(DummyEditor), typeof(UITypeEditor))]
            [DataMember]
            [Category("Sound(FDS)")]
            [Description("FDS LFO Morph Table")]
            public NesLfoMorphData[] LfoMorphData
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
                f_lfoMorphData = new NesLfoMorphData[3] {
                    new NesLfoMorphData(),
                    new NesLfoMorphData(),
                    new NesLfoMorphData()
                };
            }

        }

        [JsonConverter(typeof(NoTypeConverterJsonConverter<Vrc6Settings>))]
        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [DataContract]
        [InstLock]
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

        [JsonConverter(typeof(NoTypeConverterJsonConverter<DacSettings>))]
        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [DataContract]
        [InstLock]
        public class DacSettings : ContextBoundObject
        {

            private byte[] f_PcmData = new byte[0];

            [TypeConverter(typeof(LoadDataTypeConverter))]
            [Editor(typeof(PcmFileLoaderUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DataMember]
            [Category("Sound(PCM)")]
            [Description("Set DAC PCM data. Unigned 8bit PCM Raw Data or WAV Data. (1ch)")]
            [PcmFileLoaderEditor("Audio File(*.raw, *.wav)|*.raw;*.wav", 0, 8, 1, 0)]
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
            [DefaultValue(typeof(uint), "11025")]
            [SlideParametersAttribute(4000, 14000)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public uint SampleRate
            {
                get;
                set;
            } = 11025;

            private float f_PcmGain = 1.0f;

            [DataMember]
            [Category("Sound(PCM)")]
            [Description("Set DAC PCM gain(0.0-*).")]
            [EditorAttribute(typeof(DoubleSlideEditor), typeof(UITypeEditor))]
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

        [JsonConverter(typeof(NoTypeConverterJsonConverter<NesLfoMorphData>))]
        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [DataContract]
        [InstLock]
        public class NesLfoMorphData : ContextBoundObject
        {

            private sbyte[] f_lfodata = new sbyte[32];

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
                f_lfodata = new sbyte[32];
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
                    for (int i = 0; i < Math.Min(LfoData.Length, vs.Count); i++)
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


        [JsonConverter(typeof(NoTypeConverterJsonConverter<NesFxSettings>))]
        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [DataContract]
        [InstLock]
        public class NesFxSettings : BasicFxSettings
        {

            private string f_DutyEnvelopes;

            [DataMember]
            [Description("Set duty/noise envelop by text. Input duty/noise value and split it with space like the FamiTracker.\r\n" +
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
                            if (i < DutyEnvelopesNums.Length)
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


            private string f_MorphEnvelopes;

            [DataMember]
            [Description("Set FDS wave table number by text. Input wave table number and split it with space like the FamiTracker.\r\n" +
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


            private string f_LfoFreqEnvelopes;

            [DataMember]
            [Description("Set FDS LFO Frequency envelop by text. Input FDS LFO Frequency value and split it with space like the FamiTracker.\r\n" +
                       "0-4095 \"|\" is repeat point. \"/\" is release point.")]
            [Editor(typeof(EnvelopeUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [EnvelopeEditorAttribute(0, 4095)]
            public string LfoFreqEnvelopes
            {
                get
                {
                    return f_LfoFreqEnvelopes;
                }
                set
                {
                    if (f_LfoFreqEnvelopes != value)
                    {
                        LfoFreqEnvelopesRepeatPoint = -1;
                        LfoFreqEnvelopesReleasePoint = -1;
                        if (value == null)
                        {
                            LfoFreqEnvelopesNums = new int[] { };
                            f_LfoFreqEnvelopes = string.Empty;
                            return;
                        }
                        f_LfoFreqEnvelopes = value;
                        string[] vals = value.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        List<int> vs = new List<int>();
                        for (int i = 0; i < vals.Length; i++)
                        {
                            string val = vals[i];
                            if (val.Equals("|", StringComparison.Ordinal))
                                LfoFreqEnvelopesRepeatPoint = vs.Count;
                            else if (val.Equals("/", StringComparison.Ordinal))
                                LfoFreqEnvelopesReleasePoint = vs.Count;
                            else
                            {
                                int v;
                                if (int.TryParse(val, out v))
                                {
                                    if (v < 0)
                                        v = 0;
                                    else if (v > 4095)
                                        v = 4095;
                                    vs.Add(v);
                                }
                            }
                        }
                        LfoFreqEnvelopesNums = vs.ToArray();

                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < LfoFreqEnvelopesNums.Length; i++)
                        {
                            if (sb.Length != 0)
                                sb.Append(' ');
                            if (LfoFreqEnvelopesRepeatPoint == i)
                                sb.Append("| ");
                            if (LfoFreqEnvelopesReleasePoint == i)
                                sb.Append("/ ");
                            if (i < LfoFreqEnvelopesNums.Length)
                                sb.Append(LfoFreqEnvelopesNums[i].ToString((IFormatProvider)null));
                        }
                        f_LfoFreqEnvelopes = sb.ToString();
                    }
                }
            }

            public bool ShouldSerializeLfoFreqEnvelopes()
            {
                return !string.IsNullOrEmpty(LfoFreqEnvelopes);
            }

            public void ResetLfoFreqEnvelopes()
            {
                LfoFreqEnvelopes = null;
            }

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            public int[] LfoFreqEnvelopesNums { get; set; } = new int[] { };

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int LfoFreqEnvelopesRepeatPoint { get; set; } = -1;

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int LfoFreqEnvelopesReleasePoint { get; set; } = -1;


            private string f_LfoGainEnvelopes;

            [DataMember]
            [Description("Set FDS LFO Gain envelop by text. Input FDS LFO Gain value and split it with space like the FamiTracker.\r\n" +
                       "0-63 \"|\" is repeat point. \"/\" is release point.")]
            [Editor(typeof(EnvelopeUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [EnvelopeEditorAttribute(0, 63)]
            public string LfoGainEnvelopes
            {
                get
                {
                    return f_LfoGainEnvelopes;
                }
                set
                {
                    if (f_LfoGainEnvelopes != value)
                    {
                        LfoGainEnvelopesRepeatPoint = -1;
                        LfoGainEnvelopesReleasePoint = -1;
                        if (value == null)
                        {
                            LfoGainEnvelopesNums = new int[] { };
                            f_LfoGainEnvelopes = string.Empty;
                            return;
                        }
                        f_LfoGainEnvelopes = value;
                        string[] vals = value.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        List<int> vs = new List<int>();
                        for (int i = 0; i < vals.Length; i++)
                        {
                            string val = vals[i];
                            if (val.Equals("|", StringComparison.Ordinal))
                                LfoGainEnvelopesRepeatPoint = vs.Count;
                            else if (val.Equals("/", StringComparison.Ordinal))
                                LfoGainEnvelopesReleasePoint = vs.Count;
                            else
                            {
                                int v;
                                if (int.TryParse(val, out v))
                                {
                                    if (v < 0)
                                        v = 0;
                                    else if (v > 63)
                                        v = 63;
                                    vs.Add(v);
                                }
                            }
                        }
                        LfoGainEnvelopesNums = vs.ToArray();

                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < LfoGainEnvelopesNums.Length; i++)
                        {
                            if (sb.Length != 0)
                                sb.Append(' ');
                            if (LfoGainEnvelopesRepeatPoint == i)
                                sb.Append("| ");
                            if (LfoGainEnvelopesReleasePoint == i)
                                sb.Append("/ ");
                            if (i < LfoGainEnvelopesNums.Length)
                                sb.Append(LfoGainEnvelopesNums[i].ToString((IFormatProvider)null));
                        }
                        f_LfoGainEnvelopes = sb.ToString();
                    }
                }
            }

            public bool ShouldSerializeLfoGainEnvelopes()
            {
                return !string.IsNullOrEmpty(LfoGainEnvelopes);
            }

            public void ResetLfoGainEnvelopes()
            {
                LfoGainEnvelopes = null;
            }

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            public int[] LfoGainEnvelopesNums { get; set; } = new int[] { };

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int LfoGainEnvelopesRepeatPoint { get; set; } = -1;

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int LfoGainEnvelopesReleasePoint { get; set; } = -1;


            private string f_LfoBiasEnvelopes;

            [DataMember]
            [Description("Set FDS LFO Bias envelop by text. Input FDS LFO Bias value and split it with space like the FamiTracker.\r\n" +
                       "-64 - 63 \"|\" is repeat point. \"/\" is release point.")]
            [Editor(typeof(EnvelopeUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [EnvelopeEditorAttribute(-64, 63)]
            public string LfoBiasEnvelopes
            {
                get
                {
                    return f_LfoBiasEnvelopes;
                }
                set
                {
                    if (f_LfoBiasEnvelopes != value)
                    {
                        LfoBiasEnvelopesRepeatPoint = -1;
                        LfoBiasEnvelopesReleasePoint = -1;
                        if (value == null)
                        {
                            LfoBiasEnvelopesNums = new int[] { };
                            f_LfoBiasEnvelopes = string.Empty;
                            return;
                        }
                        f_LfoBiasEnvelopes = value;
                        string[] vals = value.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        List<int> vs = new List<int>();
                        for (int i = 0; i < vals.Length; i++)
                        {
                            string val = vals[i];
                            if (val.Equals("|", StringComparison.Ordinal))
                                LfoBiasEnvelopesRepeatPoint = vs.Count;
                            else if (val.Equals("/", StringComparison.Ordinal))
                                LfoBiasEnvelopesReleasePoint = vs.Count;
                            else
                            {
                                int v;
                                if (int.TryParse(val, out v))
                                {
                                    if (v < -64)
                                        v = -64;
                                    else if (v > 63)
                                        v = 63;
                                    vs.Add(v);
                                }
                            }
                        }
                        LfoBiasEnvelopesNums = vs.ToArray();

                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < LfoBiasEnvelopesNums.Length; i++)
                        {
                            if (sb.Length != 0)
                                sb.Append(' ');
                            if (LfoBiasEnvelopesRepeatPoint == i)
                                sb.Append("| ");
                            if (LfoBiasEnvelopesReleasePoint == i)
                                sb.Append("/ ");
                            if (i < LfoBiasEnvelopesNums.Length)
                                sb.Append(LfoBiasEnvelopesNums[i].ToString((IFormatProvider)null));
                        }
                        f_LfoBiasEnvelopes = sb.ToString();
                    }
                }
            }

            public bool ShouldSerializeLfoBiasEnvelopes()
            {
                return !string.IsNullOrEmpty(LfoBiasEnvelopes);
            }

            public void ResetLfoBiasEnvelopes()
            {
                LfoBiasEnvelopes = null;
            }

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            public int[] LfoBiasEnvelopesNums { get; set; } = new int[] { };

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int LfoBiasEnvelopesRepeatPoint { get; set; } = -1;

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int LfoBiasEnvelopesReleasePoint { get; set; } = -1;


            private string f_lfoFreqMultiplyEnvelopes;

            [DataMember]
            [Description("Set FDS LFO Freq Multiply envelop by text. Input FDS LFO Freq Multiply  value and split it with space like the FamiTracker.\r\n" +
                       "\"|\" is repeat point. \"/\" is release point.")]
            public string LfoFreqMultiplyEnvelopes
            {
                get
                {
                    return f_lfoFreqMultiplyEnvelopes;
                }
                set
                {
                    if (f_lfoFreqMultiplyEnvelopes != value)
                    {
                        LfoFreqMultiplyEnvelopesRepeatPoint = -1;
                        LfoFreqMultiplyEnvelopesReleasePoint = -1;
                        if (value == null)
                        {
                            LfoFreqMultiplyEnvelopesNums = new double[] { };
                            f_lfoFreqMultiplyEnvelopes = string.Empty;
                            return;
                        }
                        f_lfoFreqMultiplyEnvelopes = value;
                        string[] vals = value.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        List<double> vs = new List<double>();
                        for (int i = 0; i < vals.Length; i++)
                        {
                            string val = vals[i];
                            if (val.Equals("|", StringComparison.Ordinal))
                                LfoFreqMultiplyEnvelopesRepeatPoint = vs.Count;
                            else if (val.Equals("/", StringComparison.Ordinal))
                                LfoFreqMultiplyEnvelopesReleasePoint = vs.Count;
                            else
                            {
                                double v;
                                if (double.TryParse(val, out v))
                                {
                                    if (v < 0)
                                        v = 0;
                                    vs.Add(v);
                                }
                            }
                        }
                        LfoFreqMultiplyEnvelopesNums = vs.ToArray();

                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < LfoFreqMultiplyEnvelopesNums.Length; i++)
                        {
                            if (sb.Length != 0)
                                sb.Append(' ');
                            if (LfoFreqMultiplyEnvelopesRepeatPoint == i)
                                sb.Append("| ");
                            if (LfoFreqMultiplyEnvelopesReleasePoint == i)
                                sb.Append("/ ");
                            if (i < LfoFreqMultiplyEnvelopesNums.Length)
                                sb.Append(LfoFreqMultiplyEnvelopesNums[i].ToString((IFormatProvider)null));
                        }
                        f_lfoFreqMultiplyEnvelopes = sb.ToString();
                    }
                }
            }

            public bool ShouldSerializeLfoFreqMultiplyEnvelopes()
            {
                return !string.IsNullOrEmpty(LfoFreqMultiplyEnvelopes);
            }

            public void ResetLfoFreqMultiplyEnvelopes()
            {
                LfoFreqMultiplyEnvelopes = null;
            }

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            public double[] LfoFreqMultiplyEnvelopesNums { get; set; } = new double[] { };

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int LfoFreqMultiplyEnvelopesRepeatPoint { get; set; } = -1;

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int LfoFreqMultiplyEnvelopesReleasePoint { get; set; } = -1;

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


            private uint f_morphCounter;

            public byte? MorphValue
            {
                get;
                private set;
            }

            private uint f_lfoFreqCounter;

            public byte? LfoFreqValue
            {
                get;
                private set;
            }

            private uint f_lfoGainCounter;

            public byte? LfoGainValue
            {
                get;
                private set;
            }


            private uint f_lfoBiasCounter;

            public sbyte? LfoBiasValue
            {
                get;
                private set;
            }

            private uint f_lfoFreqMultiplyCounter;

            public double? LfoFreqMultiplyValue
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
                        //if (settings.DutyEnvelopesReleasePoint < 0)
                        //    f_dutyCounter = (uint)settings.DutyEnvelopesNums.Length;

                        if (f_dutyCounter < settings.DutyEnvelopesNums.Length)
                        {
                            if (settings.DutyEnvelopesReleasePoint >= 0 && f_dutyCounter <= (uint)settings.DutyEnvelopesReleasePoint)
                                f_dutyCounter = (uint)settings.DutyEnvelopesReleasePoint;
                            else if (settings.DutyEnvelopesReleasePoint < 0)
                                f_dutyCounter = (uint)settings.DutyEnvelopesNums.Length;
                        }
                    }
                    if (f_dutyCounter < settings.DutyEnvelopesNums.Length)
                    {
                        int vol = settings.DutyEnvelopesNums[f_dutyCounter++];

                        DutyValue = (byte)vol;
                        process = true;
                    }
                }


                MorphValue = null;
                if (settings.MorphEnvelopesNums.Length > 0)
                {
                    if (!isKeyOff)
                    {
                        var vm = settings.MorphEnvelopesNums.Length;
                        if (settings.MorphEnvelopesReleasePoint >= 0)
                            vm = settings.MorphEnvelopesReleasePoint;
                        if (f_morphCounter >= vm)
                        {
                            if (settings.MorphEnvelopesRepeatPoint >= 0)
                                f_morphCounter = (uint)settings.MorphEnvelopesRepeatPoint;
                            else
                                f_morphCounter = (uint)vm;
                        }
                    }
                    else
                    {
                        //if (settings.MorphEnvelopesReleasePoint < 0)
                        //    f_lfoCounter = (uint)settings.MorphEnvelopesNums.Length;

                        if (f_morphCounter < settings.MorphEnvelopesNums.Length)
                        {
                            if (settings.MorphEnvelopesReleasePoint >= 0 && f_morphCounter <= (uint)settings.MorphEnvelopesReleasePoint)
                                f_morphCounter = (uint)settings.MorphEnvelopesReleasePoint;
                            else if (settings.MorphEnvelopesReleasePoint < 0)
                                f_morphCounter = (uint)settings.MorphEnvelopesNums.Length;
                        }
                    }
                    if (f_morphCounter < settings.MorphEnvelopesNums.Length)
                    {
                        int vol = settings.MorphEnvelopesNums[f_morphCounter++];

                        MorphValue = (byte)vol;
                        process = true;
                    }
                }


                LfoFreqValue = null;
                if (settings.LfoFreqEnvelopesNums.Length > 0)
                {
                    if (!isKeyOff)
                    {
                        var vm = settings.LfoFreqEnvelopesNums.Length;
                        if (settings.LfoFreqEnvelopesReleasePoint >= 0)
                            vm = settings.LfoFreqEnvelopesReleasePoint;
                        if (f_lfoFreqCounter >= vm)
                        {
                            if (settings.LfoFreqEnvelopesRepeatPoint >= 0)
                                f_lfoFreqCounter = (uint)settings.LfoFreqEnvelopesRepeatPoint;
                            else
                                f_lfoFreqCounter = (uint)vm;
                        }
                    }
                    else
                    {
                        //if (settings.LfoFreqEnvelopesReleasePoint < 0)
                        //    f_lfoCounter = (uint)settings.LfoFreqEnvelopesNums.Length;

                        if (f_lfoFreqCounter < settings.LfoFreqEnvelopesNums.Length)
                        {
                            if (settings.LfoFreqEnvelopesReleasePoint >= 0 && f_lfoFreqCounter <= (uint)settings.LfoFreqEnvelopesReleasePoint)
                                f_lfoFreqCounter = (uint)settings.LfoFreqEnvelopesReleasePoint;
                            else if (settings.LfoFreqEnvelopesReleasePoint < 0)
                                f_lfoFreqCounter = (uint)settings.LfoFreqEnvelopesNums.Length;

                        }
                    }
                    if (f_lfoFreqCounter < settings.LfoFreqEnvelopesNums.Length)
                    {
                        int vol = settings.LfoFreqEnvelopesNums[f_lfoFreqCounter++];

                        LfoFreqValue = (byte)vol;
                        process = true;
                    }
                }

                LfoGainValue = null;
                if (settings.LfoGainEnvelopesNums.Length > 0)
                {
                    if (!isKeyOff)
                    {
                        var vm = settings.LfoGainEnvelopesNums.Length;
                        if (settings.LfoGainEnvelopesReleasePoint >= 0)
                            vm = settings.LfoGainEnvelopesReleasePoint;
                        if (f_lfoGainCounter >= vm)
                        {
                            if (settings.LfoGainEnvelopesRepeatPoint >= 0)
                                f_lfoGainCounter = (uint)settings.LfoGainEnvelopesRepeatPoint;
                            else
                                f_lfoGainCounter = (uint)vm;
                        }
                    }
                    else
                    {
                        //if (settings.LfoGainEnvelopesReleasePoint < 0)
                        //    f_lfoCounter = (uint)settings.LfoGainEnvelopesNums.Length;

                        if (f_lfoGainCounter < settings.LfoGainEnvelopesNums.Length)
                        {
                            if (settings.LfoGainEnvelopesReleasePoint >= 0 && f_lfoGainCounter <= (uint)settings.LfoGainEnvelopesReleasePoint)
                                f_lfoGainCounter = (uint)settings.LfoGainEnvelopesReleasePoint;
                            else if (settings.LfoGainEnvelopesReleasePoint < 0)
                                f_lfoGainCounter = (uint)settings.LfoGainEnvelopesNums.Length;

                        }
                    }
                    if (f_lfoGainCounter < settings.LfoGainEnvelopesNums.Length)
                    {
                        int vol = settings.LfoGainEnvelopesNums[f_lfoGainCounter++];

                        LfoGainValue = (byte)vol;
                        process = true;
                    }
                }

                LfoBiasValue = null;
                if (settings.LfoBiasEnvelopesNums.Length > 0)
                {
                    if (!isKeyOff)
                    {
                        var vm = settings.LfoBiasEnvelopesNums.Length;
                        if (settings.LfoBiasEnvelopesReleasePoint >= 0)
                            vm = settings.LfoBiasEnvelopesReleasePoint;
                        if (f_lfoBiasCounter >= vm)
                        {
                            if (settings.LfoBiasEnvelopesRepeatPoint >= 0)
                                f_lfoBiasCounter = (uint)settings.LfoBiasEnvelopesRepeatPoint;
                            else
                                f_lfoBiasCounter = (uint)vm;
                        }
                    }
                    else
                    {
                        //if (settings.LfoBiasEnvelopesReleasePoint < 0)
                        //    f_lfoCounter = (uint)settings.LfoBiasEnvelopesNums.Length;

                        if (f_lfoBiasCounter < settings.LfoBiasEnvelopesNums.Length)
                        {
                            if (settings.LfoBiasEnvelopesReleasePoint >= 0 && f_lfoBiasCounter <= (uint)settings.LfoBiasEnvelopesReleasePoint)
                                f_lfoBiasCounter = (uint)settings.LfoBiasEnvelopesReleasePoint;
                            else if (settings.LfoBiasEnvelopesReleasePoint < 0)
                                f_lfoBiasCounter = (uint)settings.LfoBiasEnvelopesNums.Length;

                        }
                    }
                    if (f_lfoBiasCounter < settings.LfoBiasEnvelopesNums.Length)
                    {
                        int vol = settings.LfoBiasEnvelopesNums[f_lfoBiasCounter++];

                        LfoBiasValue = (sbyte)vol;
                        process = true;
                    }
                }

                LfoFreqMultiplyValue = null;
                if (settings.LfoFreqMultiplyEnvelopesNums.Length > 0)
                {
                    if (!isKeyOff)
                    {
                        var vm = settings.LfoFreqMultiplyEnvelopesNums.Length;
                        if (settings.LfoFreqMultiplyEnvelopesReleasePoint >= 0)
                            vm = settings.LfoFreqMultiplyEnvelopesReleasePoint;
                        if (f_lfoFreqMultiplyCounter >= vm)
                        {
                            if (settings.LfoFreqMultiplyEnvelopesRepeatPoint >= 0)
                                f_lfoFreqMultiplyCounter = (uint)settings.LfoFreqMultiplyEnvelopesRepeatPoint;
                            else
                                f_lfoFreqMultiplyCounter = (uint)vm;
                        }
                    }
                    else
                    {
                        //if (settings.LfoFreqMultiplyEnvelopesReleasePoint < 0)
                        //    f_lfoCounter = (uint)settings.LfoFreqMultiplyEnvelopesNums.Length;

                        if (f_lfoFreqMultiplyCounter < settings.LfoFreqMultiplyEnvelopesNums.Length)
                        {
                            if (settings.LfoFreqMultiplyEnvelopesReleasePoint >= 0 && f_lfoFreqMultiplyCounter <= (uint)settings.LfoFreqMultiplyEnvelopesReleasePoint)
                                f_lfoFreqMultiplyCounter = (uint)settings.LfoFreqMultiplyEnvelopesReleasePoint;
                            else if (settings.LfoFreqMultiplyEnvelopesReleasePoint < 0)
                                f_lfoFreqMultiplyCounter = (uint)settings.LfoFreqMultiplyEnvelopesNums.Length;
                        }
                    }
                    if (f_lfoFreqMultiplyCounter < settings.LfoFreqMultiplyEnvelopesNums.Length)
                    {
                        double vol = settings.LfoFreqMultiplyEnvelopesNums[f_lfoFreqMultiplyCounter++];

                        LfoFreqMultiplyValue = vol;
                        process = true;
                    }
                }

                return process;
            }
        }

        [JsonConverter(typeof(NoTypeConverterJsonConverter<SQSweepSettings>))]
        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [DataContract]
        [InstLock]
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

            private byte f_Range = 7;

            [DataMember]
            [Category("Sound(SQ)")]
            [Description("Wave Length (0-7)")]
            [SlideParametersAttribute(0, 7)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue((byte)7)]
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
            DAC,
        }

        /// <summary>
        /// 
        /// </summary>
        [DataContract]
        [InstLock]
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
        [InstLock]
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
                                al.Add(0xAA);
                        }
                        f_DeltaPcmData = al.ToArray();
                    }
                    else
                    {
                        f_DeltaPcmData = value;
                    }
                }
            }

            [IgnoreDataMember]
            [JsonIgnore]
            [Browsable(false)]
            public uint SampleAddress
            {
                get;
                set;
            }

            [IgnoreDataMember]
            [JsonIgnore]
            [Browsable(false)]
            public int SampleLength
            {
                get
                {
                    if (f_DeltaPcmData == null)
                        return 0;
                    else
                        return f_DeltaPcmData.Length;
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


        private class EnumConverterSoundEngineTypeRP2A03 : EnumConverter<SoundEngineType>
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                var sc = new StandardValuesCollection(new SoundEngineType[] {
                    SoundEngineType.Software,
                    SoundEngineType.VSIF_NES_FTDI,
                    SoundEngineType.VSIF_NES_FTDI_FDS,
                    SoundEngineType.VSIF_NES_FTDI_VRC6,
                    SoundEngineType.VSIF_NES_FTDI_MMC5,
               });

                return sc;
            }
        }
    }

}