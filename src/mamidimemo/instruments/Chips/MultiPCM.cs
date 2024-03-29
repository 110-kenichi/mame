// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FM_SoundConvertor;
using Kermalis.SoundFont2;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.MusicTheory;
using Newtonsoft.Json;
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
using zanac.MAmidiMEmo.VSIF;

//https://wiki.superfamicom.org/spc700-reference

namespace zanac.MAmidiMEmo.Instruments.Chips
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    [InstLock]
    public class MultiPCM : InstrumentBase
    {
        public override string Name => "MultiPCM";

        public override string Group => "PCM";

        public override InstrumentType InstrumentType => InstrumentType.MultiPCM;

        [Browsable(false)]
        public override string ImageKey => "MultiPCM";

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        protected override string SoundInterfaceTagNamePrefix => "multipcm_";

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
                return 31;
            }
        }

        private object sndEnginePtrLock = new object();

        private int waveMemorySize = 4 * 1024 * 1024;


        /// <summary>
        /// 
        /// </summary>
        public enum MasterClockType : uint
        {
            Default = 39513600,
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
                    SetClock(UnitNumber, (uint)(value / 4));
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
                Timbres = (MultiPCMTimbre[])value;
            }
        }

        private MultiPCMTimbre[] f_Timbres;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category(" Timbres")]
        [Description("Timbres")]
        [EditorAttribute(typeof(TimbresArrayUITypeEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(ExpandableCollectionConverter))]
        public MultiPCMTimbre[] Timbres
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
                using (var obj = JsonConvert.DeserializeObject<MultiPCM>(serializeData))
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
        private delegate void delegate_multipcm_device_write(uint unitNumber, uint address, byte data);

        private static int[] SlotToChannel = new int[]{
            00, 1, 2, 3, 4, 5, 6,
            08, 9,10,11,12,13,14,
            16,17,18,19,20,21,22,
            24,25,26,27,28,29,30,
        };

        /// <summary>
        /// 
        /// </summary>
        private void MultiPCMRegWriteData(uint unitNumber, int slot, uint reg, byte data)
        {
            try
            {
                Program.SoundUpdating();
                multipcm_reg_write(unitNumber, 1, (byte)SlotToChannel[slot]);
                multipcm_reg_write(unitNumber, 2, (byte)reg);
                multipcm_reg_write(unitNumber, 0, (byte)data);
            }
            finally
            {
                Program.SoundUpdated();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void MultiPCMMemWriteData(uint unitNumber, uint address, byte data)
        {
            try
            {
                Program.SoundUpdating();
                multipcm_mem_write(unitNumber, address, data);
            }
            finally
            {
                Program.SoundUpdated();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private static delegate_multipcm_device_write multipcm_reg_write
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        private static delegate_multipcm_device_write multipcm_mem_write
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        static MultiPCM()
        {
            IntPtr funcPtr = MameIF.GetProcAddress("multipcm_device_reg_write");
            if (funcPtr != IntPtr.Zero)
                multipcm_reg_write = (delegate_multipcm_device_write)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_multipcm_device_write));

            funcPtr = MameIF.GetProcAddress("multipcm_device_mem_write");
            if (funcPtr != IntPtr.Zero)
                multipcm_mem_write = (delegate_multipcm_device_write)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_multipcm_device_write));
        }

        private MultiPCMSoundManager soundManager;

        /// <summary>
        /// 
        /// </summary>
        public MultiPCM(uint unitNumber) : base(unitNumber)
        {
            SetDevicePassThru(false);

            MasterClock = (uint)MasterClockType.Default;

            GainLeft = DEFAULT_GAIN;
            GainRight = DEFAULT_GAIN;

            readSoundFontForTimbre = new ToolStripMenuItem(Resources.ImportSF2Timbre);
            readSoundFontForTimbre.Click += ReadSoundFontForTimbre_Click;

            readSoundFontForDrumTimbre = new ToolStripMenuItem(Resources.ImportSF2Drum);
            readSoundFontForDrumTimbre.Click += ReadSoundFontForDrumTimbre_Click;

            Timbres = new MultiPCMTimbre[256];
            for (int i = 0; i < 256; i++)
                Timbres[i] = new MultiPCMTimbre();

            setPresetInstruments();

            this.soundManager = new MultiPCMSoundManager(this);
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
                    soundManager = null;
                }

                // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                // TODO: 大きなフィールドを null に設定します。

                disposedValue = true;
            }
        }

        // TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
        ~MultiPCM()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(false);
        }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public override void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(true);

            base.Dispose();

            // TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
            GC.SuppressFinalize(this);
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        internal override void PrepareSound()
        {
            base.PrepareSound();

            initGlobalRegisters();
        }

        private void initGlobalRegisters()
        {
            lock (sndEnginePtrLock)
                lastTransferPcmData = new sbyte[] { };

            if (!IsDisposing)
                updatePcmData(null);
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
        private void updatePcmData(MultiPCMTimbre timbre)
        {
            List<sbyte> pcmData = new List<sbyte>();
            uint nextStartAddress = (uint)0x2000;
            if (nextStartAddress == 0)
                nextStartAddress += 4;
            for (int i = 0; i < Timbres.Length; i++)
            {
                var tim = Timbres[i];

                tim.PcmAddressStart = 0;
                tim.PcmAddressEnd = 0;
                if (tim.PcmData12.Length != 0)
                {
                    int tlen = tim.PcmData12.Length;
                    if (nextStartAddress + tlen - 1 < waveMemorySize)   //MAX 4MB
                    {
                        tim.PcmAddressStart = nextStartAddress;
                        tim.PcmAddressEnd = (uint)(0x10000 - ((tlen * 2) / 3));

                        //Write PCM data
                        for (int j = 0; j < tlen; j++)
                            pcmData.Add((sbyte)tim.PcmData12[j]);

                        nextStartAddress = (uint)(nextStartAddress + tlen);
                    }
                    else
                    {
                        MessageBox.Show(Resources.AdpcmBufferExceeded, "Warning", MessageBoxButtons.OK);
                        break;
                    }
                }
                else if (tim.PcmData.Length != 0)
                {
                    int tlen = tim.PcmData.Length;
                    if (nextStartAddress + tlen - 1 < waveMemorySize)   //MAX 4MB
                    {
                        tim.PcmAddressStart = nextStartAddress;
                        tim.PcmAddressEnd = (uint)(0x10000 - tlen);

                        //Write PCM data
                        pcmData.AddRange(tim.PcmData);

                        nextStartAddress = (uint)(nextStartAddress + tlen);
                    }
                    else
                    {
                        MessageBox.Show(Resources.AdpcmBufferExceeded, "Warning", MessageBoxButtons.OK);
                        break;
                    }
                }
            }
            FormMain.OutputLog(this, "Remaining PCM RAM capacity is " + (waveMemorySize - nextStartAddress) + " bytes");

            if (pcmData.Count != 0)
            {
                //transferPcmOnlyDiffData(pcmData.ToArray(), null);

                FormMain.OutputLog(this, Resources.UpdatingADPCM);
                //if (Program.IsWriteLockHeld())
                //{
                try
                {
                    FormMain.AppliactionForm.Enabled = false;
                    using (FormProgress f = new FormProgress())
                    {
                        f.StartPosition = FormStartPosition.CenterScreen;
                        f.Message = Resources.UpdatingADPCM;
                        f.Show();
                        transferPcmOnlyDiffData(pcmData.ToArray(), f);
                    }
                }
                finally
                {
                    FormMain.AppliactionForm.Enabled = true;
                }
                //}
                //else
                //{
                //    FormProgress.RunDialog(Resources.UpdatingADPCM,
                //            new Action<FormProgress>((f) =>
                //            {
                //                transferPcmOnlyDiffData(pcmData.ToArray(), f);
                //            }));
                //}
                //FormMain.OutputLog(this, string.Format(Resources.AdpcmBufferUsedSPC700, pcmData.Count / 1024));
            }
        }

        private sbyte[] lastTransferPcmData;

        private void transferPcmOnlyDiffData(sbyte[] transferData, FormProgress fp)
        {
            for (int i = 0; i < transferData.Length; i++)
            {
                if (i >= lastTransferPcmData.Length || transferData[i] != lastTransferPcmData[i])
                {
                    sendPcmData(transferData, i, fp);
                    lastTransferPcmData = transferData;
                    break;
                }
            }
        }

        private void sendPcmData(sbyte[] transferData, int i, FormProgress fp)
        {
            int endAddress = transferData.Length;
            if (endAddress > waveMemorySize)
                endAddress = waveMemorySize;

            //Transfer
            int startAddress = i;
            int len = endAddress - startAddress;
            int index = 0;
            int percentage = 0;
            int lastPercentage = 0;
            uint baseAddress = 0x2000;

            for (int adr = startAddress; adr < endAddress; adr++)
            {
                MultiPCMMemWriteData(UnitNumber, (uint)(baseAddress + adr), (byte)transferData[adr]);

                percentage = (100 * index) / len;
                if (percentage != lastPercentage)
                {
                    if (fp != null)
                    {
                        fp.Percentage = percentage;
                        Application.DoEvents();
                    }
                }
                lastPercentage = percentage;
                index++;
            }
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
        }

        internal override void ResetAll()
        {
            ClearWrittenDataCache();
            PrepareSound();
        }

        /// <summary>
        /// 
        /// </summary>
        private class MultiPCMSoundManager : SoundManagerBase
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

            private static SoundList<MultiPCMSound> instOnSounds = new SoundList<MultiPCMSound>(8);

            private MultiPCM parentModule;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            public MultiPCMSoundManager(MultiPCM parent) : base(parent)
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
                    MultiPCMTimbre timbre = (MultiPCMTimbre)bts[i];

                    tindex++;
                    var emptySlot = searchEmptySlot(note);
                    if (emptySlot.slot < 0)
                        continue;

                    MultiPCMSound snd = new MultiPCMSound(emptySlot.inst, this, timbre, tindex - 1, note, emptySlot.slot, (byte)ids[i]);
                    instOnSounds.Add(snd);

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
            private (MultiPCM inst, int slot) searchEmptySlot(TaggedNoteOnEvent note)
            {
                return SearchEmptySlotAndOffForLeader(parentModule, instOnSounds, note, 28);
            }

            internal override void ProcessAllSoundOff()
            {
                var me = new ControlChangeEvent((SevenBitNumber)120, (SevenBitNumber)0);
                ProcessControlChange(me);

                for (int i = 0; i < 28; i++)
                {
                    //koff
                    parentModule.MultiPCMRegWriteData(parentModule.UnitNumber, i, 4, 0x00);
                }
            }

        }


        /// <summary>
        /// 
        /// </summary>
        private class MultiPCMSound : SoundBase
        {

            private MultiPCM parentModule;

            private byte timbreIndex;

            private MultiPCMTimbre timbre;

            private double baseFreq;

            private uint loopPoint;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentModule"></param>
            /// <param name="noteOnEvent"></param>
            /// <param name="programNumber"></param>
            /// <param name="slot"></param>
            public MultiPCMSound(MultiPCM parentModule, MultiPCMSoundManager manager, TimbreBase timbre, int tindex, TaggedNoteOnEvent noteOnEvent, int slot, byte timbreIndex) : base(parentModule, manager, timbre, tindex, noteOnEvent, slot)
            {
                this.parentModule = parentModule;
                this.timbreIndex = timbreIndex;
                this.timbre = (MultiPCMTimbre)timbre;

                baseFreq = this.timbre.BaseFreqency;
                loopPoint = this.timbre.LoopPoint;
            }

            /// <summary>
            /// 
            /// </summary>
            public override void OnPanpotUpdated()
            {
                uint adrs = (uint)(timbreIndex * 12);

                int p = CalcCurrentPanpot() + 4;
                int pan = (int)(p >> 3) - 8;
                if (pan <= -8)
                    pan = -7;
                else if (pan >= 8)
                    pan = 7;

                parentModule.MultiPCMRegWriteData(parentModule.UnitNumber, Slot, 0, (byte)((byte)pan << 4));
            }

            /// <summary>
            /// 
            /// </summary>
            public override void KeyOn()
            {
                base.KeyOn();

                {
                    uint adrs = (uint)(timbreIndex * 12);
                    //start address
                    if (timbre.PcmData12.Length != 0)
                        //12bit linear
                        parentModule.MultiPCMMemWriteData(parentModule.UnitNumber, adrs + 0, (byte)(((timbre.PcmAddressStart >> 16) & 0xff) | 0x40));
                    else if (timbre.PcmData.Length != 0)
                        //8bit linear
                        parentModule.MultiPCMMemWriteData(parentModule.UnitNumber, adrs + 0, (byte)((timbre.PcmAddressStart >> 16) & 0xff));
                    else
                        return;
                    parentModule.MultiPCMMemWriteData(parentModule.UnitNumber, adrs + 1, (byte)((timbre.PcmAddressStart >> 8) & 0xff));
                    parentModule.MultiPCMMemWriteData(parentModule.UnitNumber, adrs + 2, (byte)((timbre.PcmAddressStart) & 0xff));
                    //loop address
                    if (timbre.LoopEnable)
                    {
                        parentModule.MultiPCMMemWriteData(parentModule.UnitNumber, adrs + 3, (byte)((timbre.LoopPoint >> 8) & 0xff));
                        parentModule.MultiPCMMemWriteData(parentModule.UnitNumber, adrs + 4, (byte)((timbre.LoopPoint) & 0xff));
                    }
                    else
                    {
                        parentModule.MultiPCMMemWriteData(parentModule.UnitNumber, adrs + 3, (byte)((timbre.PcmAddressEnd >> 8) & 0xff));
                        parentModule.MultiPCMMemWriteData(parentModule.UnitNumber, adrs + 4, (byte)((timbre.PcmAddressEnd) & 0xff));
                    }
                    //end address
                    parentModule.MultiPCMMemWriteData(parentModule.UnitNumber, adrs + 5, (byte)((timbre.PcmAddressEnd >> 8) & 0xff));
                    parentModule.MultiPCMMemWriteData(parentModule.UnitNumber, adrs + 6, (byte)((timbre.PcmAddressEnd) & 0xff));

                    parentModule.MultiPCMMemWriteData(parentModule.UnitNumber, adrs + 7, (byte)(((timbre.LFO << 3) | timbre.VIB) & 0xff));
                    parentModule.MultiPCMMemWriteData(parentModule.UnitNumber, adrs + 8, (byte)(((timbre.AR << 4) | timbre.D1R) & 0xff));
                    parentModule.MultiPCMMemWriteData(parentModule.UnitNumber, adrs + 9, (byte)(((timbre.DL << 4) | timbre.D2R) & 0xff));
                    parentModule.MultiPCMMemWriteData(parentModule.UnitNumber, adrs + 10, (byte)(((timbre.KSR << 4) | timbre.RR) & 0xff));
                    parentModule.MultiPCMMemWriteData(parentModule.UnitNumber, adrs + 11, (byte)(timbre.AM & 0xff));
                }

                OnVolumeUpdated();
                OnPanpotUpdated();
                OnPitchUpdated();

                //prognum no
                parentModule.MultiPCMRegWriteData(parentModule.UnitNumber, Slot, 1, timbreIndex);

                //KON
                parentModule.MultiPCMRegWriteData(parentModule.UnitNumber, Slot, 4, 0x80);
            }


            public override void OnSoundParamsUpdated()
            {
                //OnVolumeUpdated();
                //OnPanpotUpdated();
                //OnPitchUpdated();

                {
                    uint adrs = (uint)(timbreIndex * 12);

                    parentModule.MultiPCMMemWriteData(parentModule.UnitNumber, adrs + 7, (byte)(((timbre.LFO << 3) | timbre.VIB) & 0xff));
                    parentModule.MultiPCMMemWriteData(parentModule.UnitNumber, adrs + 8, (byte)(((timbre.AR << 4) | timbre.D1R) & 0xff));
                    parentModule.MultiPCMMemWriteData(parentModule.UnitNumber, adrs + 9, (byte)(((timbre.DL << 4) | timbre.D2R) & 0xff));
                    parentModule.MultiPCMMemWriteData(parentModule.UnitNumber, adrs + 10, (byte)(((timbre.KSR << 4) | timbre.RR) & 0xff));
                    parentModule.MultiPCMMemWriteData(parentModule.UnitNumber, adrs + 11, (byte)(timbre.AM & 0xff));
                }

                base.OnSoundParamsUpdated();
            }

            /// <summary>
            /// 
            /// </summary>
            public override void OnVolumeUpdated()
            {
                byte vol = (byte)(127 - Math.Round(127d * CalcCurrentVolume()));
                parentModule.MultiPCMRegWriteData(parentModule.UnitNumber, Slot, 5, (byte)(vol << 1)); //Direct Set
                //parentModule.MultiPCMRegWriteData(parentModule.UnitNumber, Slot, 5, (byte)((vol << 1) | 1)); //Interpolate Set
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="slot"></param>
            public override void OnPitchUpdated()
            {
                var cfreq = CalcCurrentFrequency() * timbre.SampleRate / (parentModule.MasterClock / (4 * 224));

                int fn = 0;
                int oct = 1;
                if (cfreq >= baseFreq)
                {
                    var log = Math.Log(cfreq / baseFreq, 2);
                    fn = (int)(1024d * (Math.Pow(2, (log % 1d)) - 1d));
                    oct = 1 + (int)Math.Floor(log);
                }
                else
                {
                    var log = Math.Log(baseFreq / cfreq, 2);
                    fn = (int)(1024d * (Math.Pow(2, 1d - (log % 1d)) - 1d));
                    oct = 1 - (int)Math.Ceiling(log);
                }
                if (oct < -7)
                    oct = -7;
                if (oct > 7)
                    oct = 7;

                parentModule.MultiPCMRegWriteData(parentModule.UnitNumber, Slot, 3, (byte)((oct << 4) | ((fn >> 6) & 0xf)));
                parentModule.MultiPCMRegWriteData(parentModule.UnitNumber, Slot, 2, (byte)((fn << 2) & 0xff));

                base.OnPitchUpdated();
            }

            /// <summary>
            /// 
            /// </summary>
            public override void SoundOff()
            {
                base.SoundOff();

                byte bitPos = (byte)(1 << Slot);

                //KOFF
                parentModule.MultiPCMRegWriteData(parentModule.UnitNumber, Slot, 4, 0x00);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        [JsonConverter(typeof(NoTypeConverterJsonConverter<MultiPCMTimbre>))]
        [DataContract]
        [InstLock]
        public class MultiPCMTimbre : TimbreBase
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
            [Description("PCM base frequency @ 44.1KHz [Hz]")]
            [DefaultValue(typeof(double), "440")]
            [DoubleSlideParametersAttribute(100, 2000, 1)]
            [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public double BaseFreqency
            {
                get;
                set;
            } = 440;

            private uint f_SampleRate = 44100;

            [DataMember]
            [Category("Sound")]
            [Description("Set PCM samplerate [Hz]")]
            [DefaultValue(typeof(uint), "44100")]
            [SlideParametersAttribute(4000, 44100)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public uint SampleRate
            {
                get => f_SampleRate;
                set
                {
                    f_SampleRate = value;
                }
            }

            private bool f_LoopEnable;

            [DataMember]
            [Category("Sound")]
            [Description("Loop point enable")]
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

            private uint f_LoopPoint;

            [DataMember]
            [Category("Sound")]
            [Description("Loop start offset address")]
            [DefaultValue(typeof(uint), "0")]
            [SlideParametersAttribute(0, 64 * 1024)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public uint LoopPoint
            {
                get => f_LoopPoint;
                set
                {
                    f_LoopPoint = value;
                    if (f_LoopPoint > 64 * 1024)
                        f_LoopPoint = 64 * 1024;
                }
            }

            private sbyte[] f_pcmData = new sbyte[0];

            [TypeConverter(typeof(LoadDataTypeConverter))]
            [Editor(typeof(PcmFileLoaderUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DataMember]
            [Category("Sound")]
            [Description("Signed 8bit Mono Raw PCM Data/Unsigned 8bit Mono Wave Data (MAX 64kb)")]
            [PcmFileLoaderEditor("Audio File(*.raw, *.wav)|*.raw;*.wav", 0, 8, 1, 65535)]
            public sbyte[] PcmData
            {
                get
                {
                    return f_pcmData;
                }
                set
                {
                    f_pcmData = value;

                    var inst = (MultiPCM)this.Instrument;
                    if (inst != null)
                        inst.updatePcmData(this);
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

            private byte[] f_PcmData12 = new byte[0];

            [TypeConverter(typeof(LoadDataTypeConverter))]
            [Editor(typeof(PcmFileLoaderUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DataMember]
            [Category("Sound")]
            [Description("12bit Raw PCM Data. (MAX 64K samples)")]
            [PcmFileLoaderEditor("Audio File(*.raw)|*.raw", 0, 0, 0, 65535)]
            public byte[] PcmData12
            {
                get
                {
                    return f_PcmData12;
                }
                set
                {
                    f_PcmData12 = value;

                    var inst = (MultiPCM)this.Instrument;
                    if (inst != null)
                        inst.updatePcmData(this);
                }
            }

            public bool ShouldSerializePcmData12()
            {
                return PcmData12.Length != 0;
            }

            public void ResetPcmData12()
            {
                f_PcmData12 = new byte[0];
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

            [DataMember]
            [Browsable(false)]
            public uint PcmAddressStart
            {
                get;
                set;
            }

            [DataMember]
            [Browsable(false)]
            public uint PcmAddressEnd
            {
                get;
                set;
            }

            private byte f_AR = 15;

            [DataMember]
            [Category("Sound")]
            [Description("Attack rate of ADSR(0-15)")]
            [DefaultValue((byte)15)]
            [SlideParametersAttribute(0, 15)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte AR
            {
                get
                {
                    return f_AR;
                }
                set
                {
                    f_AR = (byte)(value & 0xf);
                }
            }

            private byte f_D1R = 4;

            [DataMember]
            [Category("Sound")]
            [Description("Decay 1 rate of ADSR(0-15)")]
            [DisplayName("D1R(DR)[D1R]")]
            [DefaultValue((byte)4)]
            [SlideParametersAttribute(0, 0xf)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte D1R
            {
                get
                {
                    return f_D1R;
                }
                set
                {
                    f_D1R = (byte)(value & 0xf);
                }
            }

            private byte f_DL = 7;

            [DataMember]
            [Category("Sound")]
            [Description("Decay level of ADSR(0-15)")]
            [DefaultValue((byte)7)]
            [SlideParametersAttribute(0, 0xf)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte DL
            {
                get
                {
                    return f_DL;
                }
                set
                {
                    f_DL = (byte)(value & 0xf);
                }
            }

            private byte f_D2R = 1;

            [DataMember]
            [Category("Sound")]
            [Description("Decay 2 rate of ADSR(0-15)")]
            [DisplayName("D2R(SR)[D2R]")]
            [DefaultValue((byte)1)]
            [SlideParametersAttribute(0, 0xf)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte D2R
            {
                get
                {
                    return f_D2R;
                }
                set
                {
                    f_D2R = (byte)(value & 0xf);
                }
            }

            private byte f_RR = 7;

            [DataMember]
            [Category("Sound")]
            [Description("Release rate of ADSR(0-15)")]
            [DefaultValue((byte)7)]
            [SlideParametersAttribute(0, 0xf)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte RR
            {
                get
                {
                    return f_RR;
                }
                set
                {
                    f_RR = (byte)(value & 0xf);
                }
            }

            private byte f_KSR = 0x0;

            [DataMember]
            [Category("Sound")]
            [Description("Key rate correction(0-15)")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 0xf)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte KSR
            {
                get
                {
                    return f_KSR;
                }
                set
                {
                    f_KSR = (byte)(value & 0xf);
                }
            }

            private byte f_LFO = 0x0;

            [DataMember]
            [Category("Sound")]
            [Description("LFO depth(0-7)")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 0x7)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte LFO
            {
                get
                {
                    return f_LFO;
                }
                set
                {
                    f_LFO = (byte)(value & 0x7);
                }
            }

            private byte f_VIB = 0x0;

            [DataMember]
            [Category("Sound")]
            [Description("Vibrato depth(0-7)")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 0x7)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte VIB
            {
                get
                {
                    return f_VIB;
                }
                set
                {
                    f_VIB = (byte)(value & 0x7);
                }
            }

            private byte f_AM = 0x0;

            [DataMember]
            [Category("Sound")]
            [Description("Tremolo depth(0-7)")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 0x7)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte AM
            {
                get
                {
                    return f_AM;
                }
                set
                {
                    f_AM = (byte)(value & 0x7);
                }
            }

            /*
            [DataMember]
            [Category("Chip")]
            [Description("Global Settings")]
            public MultiPCMGlobalSettings GlobalSettings
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
                GlobalSettings.InjectFrom(new LoopInjection(), new MultiPCMGlobalSettings());
            }
            */

            /// <summary>
            /// 
            /// </summary>
            public MultiPCMTimbre()
            {
                //GlobalSettings = new MultiPCMGlobalSettings();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="serializeData"></param>
            public override void RestoreFrom(string serializeData)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<MultiPCMTimbre>(serializeData);
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


        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [JsonConverter(typeof(NoTypeConverterJsonConverter<MultiPCMGlobalSettings>))]
        [DataContract]
        [InstLock]
        public class MultiPCMGlobalSettings : ContextBoundObject
        {
            [DataMember]
            [Category("Chip")]
            [Description("Override global settings")]
            [DefaultValue(false)]
            public bool Enable
            {
                get;
                set;
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
            List<ToolStripMenuItem> menus = new System.Collections.Generic.List<ToolStripMenuItem>(base.GetInstrumentMenus());

            menus.AddRange(new ToolStripMenuItem[] {
                readSoundFontForTimbre,
                readSoundFontForDrumTimbre
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

                    loadPcm(openFileDialog.FileName, offset, false);
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

                    loadPcm(openFileDialog.FileName, offset, true);
                }
                for (int i = 0; i < 128; i++)
                {
                    var tim = (MultiPCMTimbre)Timbres[i + 128];

                    DrumTimbres[i].TimbreNumber = (ProgramAssignmentNumber)(i + 128);
                    DrumTimbres[i].BaseNote =
                        (NoteNames)(byte)Math.Round(MidiManager.CalcNoteNumberFromFrequency(tim.BaseFreqency));
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
        /// <param name="offset"></param>
        private void loadPcm(string fileName, int offset, bool drum)
        {
            var sf2 = new SF2(fileName);

            var spl = sf2.SoundChunk.SMPLSubChunk.Samples;
            int tn = 0;
            int num = 0;
            foreach (var s in sf2.HydraChunk.SHDRSubChunk.Samples)
            {
                if (s.SampleType == SF2SampleLink.MonoSample ||
                    s.SampleType == SF2SampleLink.LeftSample)
                {
                    var tim = new MultiPCMTimbre();

                    double baseFreq = 440.0 * Math.Pow(2.0, (((double)s.OriginalKey - 69.0) / 12.0) + (double)(s.PitchCorrection / 100));
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

                    if (((len * 3) + 1) / 2 < 65536)
                    {
                        byte[] samples = new byte[((len * 3) + 1) / 2];
                        int idx = 0;
                        for (uint i = 0; i < len; i++)
                        {
                            int data = (int)spl[start + i];

                            if ((i & 1) == 0)
                            {
                                samples[idx++] = (byte)(data >> 8);
                                samples[idx] = (byte)((data >> 4) & 0x0f);
                            }
                            else
                            {
                                samples[idx++] |= (byte)(data & 0xf0);
                                samples[idx++] = (byte)(data >> 8);
                            }
                        }
                        tim.PcmData12 = samples;

                        tim.LoopPoint = loopP;
                        tim.LoopEnable = true;
                        if (s.LoopStart == s.LoopEnd)
                        {
                            tim.D1R = 0xf;
                            tim.DL = 0;
                            tim.LoopPoint = (uint)(len - 1);
                            tim.LoopEnable = false;
                        }
                    }
                    else
                    {
                        sbyte[] samples = new sbyte[len];
                        for (uint i = 0; i < len; i++)
                            samples[i] = (sbyte)(spl[start + i] >> 8);
                        tim.PcmData = samples;

                        tim.LoopPoint = loopP;
                        tim.LoopEnable = true;
                        if (s.LoopStart == s.LoopEnd)
                        {
                            tim.D1R = 0xf;
                            tim.DL = 0;
                            tim.LoopPoint = (uint)(len - 1);
                            tim.LoopEnable = false;
                        }
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
            updatePcmData(null);
            MessageBox.Show(string.Format(Resources.TimbreLoaded, num));
        }

        #endregion
    }

}
