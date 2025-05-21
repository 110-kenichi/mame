// copyright-holders:K.Ito
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
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

//https://github.com/FluBBaOfWard/VLM5030/blob/main/vlm5030.c

namespace zanac.MAmidiMEmo.Instruments.Chips
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    [InstLock]
    public class VLM5030 : InstrumentBase
    {

        public override string Name => "VLM5030";

        public override string Group => "VOICE";

        public override InstrumentType InstrumentType => Instruments.InstrumentType.VLM5030;

        [Browsable(false)]
        public override string ImageKey => "VLM5030";

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        protected override string SoundInterfaceTagNamePrefix => "vlm_";

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
                return 36;
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
                Timbres = (VLM5030Timbre[])value;
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
        public VLM5030Timbre[] Timbres
        {
            get;
            set;
        }


        private const float DEFAULT_GAIN = 0.5f;

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
                using (var obj = JsonConvert.DeserializeObject<Beep>(serializeData))
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
        private delegate void delegate_vlm_device_write_data_and_play(uint unitNumber, byte[] data, ushort length, byte stat);


        /// <summary>
        /// 
        /// </summary>
        private static delegate_vlm_device_write_data_and_play vlm_device_write_data_and_play
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        private void Vlm5030WriteAndPlay(byte[] data, ushort length, byte stat)
        {

            try
            {
                Program.SoundUpdating();
                vlm_device_write_data_and_play(UnitNumber, data, length, stat);
            }
            finally
            {
                Program.SoundUpdated();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        static VLM5030()
        {
            IntPtr funcPtr = MameIF.GetProcAddress("vlm_device_write_data_and_play");
            if (funcPtr != IntPtr.Zero)
            {
                vlm_device_write_data_and_play = (delegate_vlm_device_write_data_and_play)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_vlm_device_write_data_and_play));
            }
        }

        private VLM5030SoundManager soundManager;

        /// <summary>
        /// 
        /// </summary>
        public VLM5030(uint unitNumber) : base(unitNumber)
        {
            GainLeft = DEFAULT_GAIN;
            GainRight = DEFAULT_GAIN;

            Timbres = new VLM5030Timbre[InstrumentBase.DEFAULT_MAX_TIMBRES];
            for (int i = 0; i < InstrumentBase.DEFAULT_MAX_TIMBRES; i++)
                Timbres[i] = new VLM5030Timbre();
            setPresetInstruments();

            this.soundManager = new VLM5030SoundManager(this);
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
            //Timbres[0].ToneType = ToneType.Custom;
            //Timbres[0].CustomLpcData =
            //    "0 02 00 12 06 03 06\r\n" +
            //    "0 04 00 12 08 04 08\r\n" +
            //    "0 05 00 13 10 06 09\r\n" +
            //    "0 12 35 21 12 07 05 10 05 11 5 1 3\r\n" +
            //    "0 14 36 20 11 10 07 10 03 11 6 2 3\r\n" +
            //    "0 13 36 18 11 13 10 09 02 09 6 3 3\r\n" +
            //    "0 11 36 16 12 15 10 07 01 09 6 2 4\r\n" +
            //    "0 08 34 14 17 15 10 08 01 06 4 2 5\r\n" +
            //    "0 07 30 15 07 15 10 07 05 05 2 3 4\r\n" +
            //    "0 07 28 15 04 15 11 08 04 06 3 3 5\r\n" +
            //    "0 05 26 15 15 15 12 09 07 06 3 3 4\r\n" +
            //    "0 01 00 09 09 11 08\r\n" +
            //    "0 01 00 11 03 05 08\r\n" +
            //    "0 01 00 09 04 07 08\r\n" +
            //    "0 01 00 09 04 07 08\r\n" +
            //    "0 01 00 09 05 08 08\r\n" +
            //    "0 03 00 00 09 11 10\r\n" +
            //    "0 02 00 00 07 10 10";
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
        private class VLM5030SoundManager : SoundManagerBase
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

            private static SoundList<VLM5030Sound> psgOnSounds = new SoundList<VLM5030Sound>(1);

            private VLM5030 parentModule;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            public VLM5030SoundManager(VLM5030 parent) : base(parent)
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
                foreach (VLM5030Timbre timbre in parentModule.GetBaseTimbres(note))
                {
                    tindex++;
                    var emptySlot = searchEmptySlot(note);
                    if (emptySlot.slot < 0)
                        continue;

                    VLM5030Sound snd = new VLM5030Sound(emptySlot.inst, this, timbre, tindex - 1, note, emptySlot.slot);
                    psgOnSounds.Add(snd);

                    FormMain.OutputDebugLog(parentModule, "KeyOn VLM5030 ch" + emptySlot + " " + note.ToString());
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
            private (VLM5030 inst, int slot) searchEmptySlot(TaggedNoteOnEvent note)
            {
                return SearchEmptySlotAndOffForLeader(parentModule, psgOnSounds, note, 1);
            }

            internal override void ProcessAllSoundOff()
            {
                var me = new ControlChangeEvent((SevenBitNumber)120, (SevenBitNumber)0);
                ProcessControlChange(me);

                parentModule.Vlm5030WriteAndPlay(new byte[] { 0x3 }, 1, 1);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private class VLM5030Sound : SoundBase
        {

            private VLM5030 parentModule;

            private VLM5030Timbre timbre;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentModule"></param>
            /// <param name="noteOnEvent"></param>
            /// <param name="programNumber"></param>
            /// <param name="slot"></param>
            public VLM5030Sound(VLM5030 parentModule, VLM5030SoundManager manager, TimbreBase timbre, int tindex, TaggedNoteOnEvent noteOnEvent, int slot) : base(parentModule, manager, timbre, tindex, noteOnEvent, slot)
            {
                this.parentModule = parentModule;
                this.timbre = (VLM5030Timbre)timbre;
            }

            /// <summary>
            /// 
            /// </summary>
            public override void KeyOn()
            {
                base.KeyOn();

                // Set freq
                //if (timbre.BaseFreqency != 0)
                {
                    parentModule.SetClock(parentModule.UnitNumber,
                        (uint)Math.Round(3579545 * (CalcCurrentFrequency() / timbre.BaseFreqency)));
                }

                //START
                parentModule.Vlm5030WriteAndPlay(new byte[] { 0x3 }, 1, 1);
                parentModule.Vlm5030WriteAndPlay(timbre.LpcRawData, (ushort)timbre.LpcRawData.Length, 0);
            }

            /// <summary>
            /// 
            /// </summary>
            public override void OnPitchUpdated()
            {
                //if (timbre.BaseFreqency != 0)
                {
                    parentModule.SetClock(parentModule.UnitNumber,
                        (uint)Math.Round(3579545 * (CalcCurrentFrequency() / timbre.BaseFreqency)));
                }
                base.OnPitchUpdated();
            }

            /// <summary>
            /// 
            /// </summary>
            public override void SoundOff()
            {
                base.SoundOff();

                // end
                parentModule.Vlm5030WriteAndPlay(new byte[] { 0x3 }, 1, 1);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        [JsonConverter(typeof(NoTypeConverterJsonConverter<VLM5030Timbre>))]
        [DataContract]
        [InstLock]
        public class VLM5030Timbre : TimbreBase
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

            private byte[] f_LpcRawData = new byte[0];

            [TypeConverter(typeof(LoadDataTypeConverter))]
            [Editor(typeof(LpcFileLoaderUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DataMember]
            [Category("Sound")]
            [Description("Set raw LPC data.")]
            public byte[] LpcRawData
            {
                get
                {
                    return f_LpcRawData;
                }
                set
                {
                    f_LpcRawData = value;
                }
            }

            public bool ShouldSerializeLpcRawData()
            {
                return LpcRawData.Length != 0;
            }

            public void ResetLpcRawData()
            {
                LpcRawData = new byte[0];
            }

            private String lpcDataInfo;

            [DataMember]
            [Category("Sound")]
            [Description("LpcData information.\r\n*Warning* May contain privacy information. Check the options dialog.")]
            [ReadOnly(true)]
            public String LpcDataInfo
            {
                get
                {
                    if (Settings.Default.DoNotUsePrivacySettings)
                        return null;
                    return lpcDataInfo;
                }
                set
                {
                    lpcDataInfo = value;
                }
            }

            [DataMember]
            [Category("Sound")]
            [Description("Set base frequency [Hz]")]
            [DefaultValue(typeof(double), "440")]
            [DoubleSlideParametersAttribute(100, 2000, 1)]
            [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public double BaseFreqency
            {
                get;
                set;
            } = 440;

            /// <summary>
            /// 
            /// </summary>
            public VLM5030Timbre()
            {
            }

            public override void RestoreFrom(string serializeData)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<VLM5030Timbre>(serializeData);
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
        public enum ToneType
        {
            Preset,
            Custom,
        }


    }
}