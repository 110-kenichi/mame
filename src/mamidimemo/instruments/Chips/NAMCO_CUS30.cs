// copyright-holders:K.Ito
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Linq;
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

//http://fpga.blog.shinobi.jp/fpga/おんげん！
//https://www.walkofmind.com/programming/pie/wsg3.htm

namespace zanac.MAmidiMEmo.Instruments.Chips
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    [InstLock]
    public class NAMCO_CUS30 : InstrumentBase
    {

        public override string Name => "NAMCO_CUS30";

        public override string Group => "WSG";

        public override InstrumentType InstrumentType => InstrumentType.NAMCO_CUS30;

        [Browsable(false)]
        public override string ImageKey => "NAMCO_CUS30";

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        protected override string SoundInterfaceTagNamePrefix => "namco_cus30_";

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
                return 4;
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
        public NAMCO_CUS30Timbre[] Timbres
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
            using (var obj = JsonConvert.DeserializeObject<NAMCO_CUS30>(serializeData))
                this.InjectFrom(new LoopInjection(new[] { "SerializeData", "SerializeDataSave", "SerializeDataLoad"}), obj);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delegate_namco_cus30_w(uint unitNumber, uint address, byte data);


        /// <summary>
        /// 
        /// </summary>
        private static delegate_namco_cus30_w namco_cus30_w
        {
            get;
            set;
        }


        /// <summary>
        /// 
        /// </summary>
        private static void NamcoCus30WriteData(uint unitNumber, uint address, byte data)
        {
            DeferredWriteData(namco_cus30_w, unitNumber, address, data);
            /*
            try
            {
                Program.SoundUpdating();
                namco_cus30_w(unitNumber, address, data);
            }
            finally
            {
                Program.SoundUpdated();
            }*/
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate byte delegate_namco_cus30_r(uint unitNumber, uint address);


        /// <summary>
        /// 
        /// </summary>
        private static delegate_namco_cus30_r namco_cus30_r
        {
            get;
            set;
        }


        /// <summary>
        /// 
        /// </summary>
        private static byte NamcoCus30ReadData(uint unitNumber, uint address)
        {
            try
            {
                Program.SoundUpdating();
                FlushDeferredWriteData();

                return namco_cus30_r(unitNumber, address);
            }
            finally
            {
                Program.SoundUpdated();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        static NAMCO_CUS30()
        {
            IntPtr funcPtr = MameIF.GetProcAddress("namco_cus30_w");
            if (funcPtr != IntPtr.Zero)
                namco_cus30_w = (delegate_namco_cus30_w)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_namco_cus30_w));
            funcPtr = MameIF.GetProcAddress("namco_cus30_r");
            if (funcPtr != IntPtr.Zero)
                namco_cus30_r = (delegate_namco_cus30_r)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_namco_cus30_r));
        }

        private NAMCO_CUS30SoundManager soundManager;

        /// <summary>
        /// 
        /// </summary>
        public NAMCO_CUS30(uint unitNumber) : base(unitNumber)
        {
            Timbres = new NAMCO_CUS30Timbre[InstrumentBase.DEFAULT_MAX_TIMBRES];
            for (int i = 0; i < InstrumentBase.DEFAULT_MAX_TIMBRES; i++)
                Timbres[i] = new NAMCO_CUS30Timbre();
            setPresetInstruments();

            this.soundManager = new NAMCO_CUS30SoundManager(this);
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
            Timbres[0].SoundType = SoundType.WSG;
            Timbres[0].WsgData = new byte[] { 8, 9, 11, 12, 13, 14, 15, 15, 15, 15, 14, 14, 13, 11, 10, 9, 7, 6, 4, 3, 2, 1, 0, 0, 0, 0, 1, 1, 2, 4, 5, 6 };

            Timbres[1].SoundType = SoundType.WSG;
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
            //var its = (HuC6280)InstrumentManager.GetInstruments(16).ElementAt(0);
            //for (int i = 0; i < its.Timbres.Length; i++)
            //{
            //    for (int j = 0; j < 32; j++)
            //        Timbres[i].WsgData[j] = (byte)(its.Timbres[i].WsgData[j] / 2);
            //    if((int)its.Timbres[i].SoundType == 2)
            //        Timbres[i].SoundType = SoundType.NOISE;
            //    else
            //        Timbres[i].SoundType = SoundType.WSG;
            //    Timbres[i].SDS.SerializeData = its.Timbres[i].SDS.SerializeData;
            //}
            //for (int i = 0; i < its.DrumTimbres.Length; i++)
            //{
            //    DrumTimbres[i].BaseNote = its.DrumTimbres[i].BaseNote;
            //    DrumTimbres[i].GateTime = its.DrumTimbres[i].GateTime;
            //    DrumTimbres[i].TimbreNumber = its.DrumTimbres[i].TimbreNumber;
            //}

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
        private class NAMCO_CUS30SoundManager : SoundManagerBase
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

            private static SoundList<NAMCO_CUS30Sound> wsgOnSounds = new SoundList<NAMCO_CUS30Sound>(8);

            private NAMCO_CUS30 parentModule;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            public NAMCO_CUS30SoundManager(NAMCO_CUS30 parent) : base(parent)
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
                foreach (NAMCO_CUS30Timbre timbre in parentModule.GetBaseTimbres(note))
                {
                    tindex++;
                    var emptySlot = searchEmptySlot(note);
                    if (emptySlot.slot < 0)
                        continue;

                    NAMCO_CUS30Sound snd = new NAMCO_CUS30Sound(emptySlot.inst, this, timbre, tindex - 1, note, emptySlot.slot);
                    wsgOnSounds.Add(snd);

                    FormMain.OutputDebugLog(parentModule, "KeyOn WSG ch" + emptySlot + " " + note.ToString());
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
            private (NAMCO_CUS30 inst, int slot) searchEmptySlot(TaggedNoteOnEvent note)
            {
                return SearchEmptySlotAndOffForLeader(parentModule, wsgOnSounds, note, 8);
            }

            internal override void ProcessAllSoundOff()
            {
                var me = new ControlChangeEvent((SevenBitNumber)120, (SevenBitNumber)0);
                ProcessControlChange(me);

                for (int i = 0; i < 8; i++)
                {
                    NamcoCus30WriteData(parentModule.UnitNumber, 0x100 + (uint)i * 8 + 0x00, 0x0);
                    byte org = (byte)(NamcoCus30ReadData(parentModule.UnitNumber, 0x100 + (uint)i * 8 + 0x04) & 0x80);
                    NamcoCus30WriteData(parentModule.UnitNumber, 0x100 + (uint)i * 8 + 0x04, org);
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private class NAMCO_CUS30Sound : SoundBase
        {

            private NAMCO_CUS30 parentModule;

            private NAMCO_CUS30Timbre timbre;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentModule"></param>
            /// <param name="noteOnEvent"></param>
            /// <param name="programNumber"></param>
            /// <param name="slot"></param>
            public NAMCO_CUS30Sound(NAMCO_CUS30 parentModule, NAMCO_CUS30SoundManager manager, TimbreBase timbre, int tindex, TaggedNoteOnEvent noteOnEvent, int slot) : base(parentModule, manager, timbre, tindex, noteOnEvent, slot)
            {
                this.parentModule = parentModule;
                this.timbre = (NAMCO_CUS30Timbre)timbre;
            }

            /// <summary>
            /// 
            /// </summary>
            public override void KeyOn()
            {
                base.KeyOn();

                SetTimbre();
                //Freq
                OnPitchUpdated();
                //Volume
                OnVolumeUpdated();
            }

            /// <summary>
            /// 
            /// </summary>
            public void SetTimbre()
            {
                byte[] wdata = timbre.WsgData;
                for (int i = 0; i < 16; i++)
                    NamcoCus30WriteData(parentModule.UnitNumber, (uint)((Slot * 16) + i), (byte)(((wdata[i * 2] & 0xf) << 4) | (wdata[i * 2 + 1] & 0xf)));
            }

            /// <summary>
            /// 
            /// </summary>
            public override void OnVolumeUpdated()
            {
                if (IsSoundOff)
                    return;

                var vol = CalcCurrentVolume();
                byte pan = CalcCurrentPanpot();

                //if (pan < 63)   //left
                //    fv_r = (byte)((byte)(fv_r * pan / 63) & 0xf);
                //else if (pan > 64)  //right
                //    fv_l = (byte)((byte)(fv_l * (127 - pan) / 63) & 0xf);
                byte fv_l = (byte)Math.Round(15d * vol * Math.Cos(Math.PI / 2 * (pan / 127d)));
                byte fv_r = (byte)Math.Round(15d * vol * Math.Sin(Math.PI / 2 * (pan / 127d)));

                fv_r |= (byte)(NamcoCus30ReadData(parentModule.UnitNumber, 0x100 + (uint)Slot * 8 + 0x04) & 0x80);

                byte noise = NamcoCus30ReadData(parentModule.UnitNumber, 0x100 + (uint)(((Slot - 1) * 8) & 0x3f) + 0x04);
                noise &= 0x7f;
                if (timbre.SoundType == SoundType.NOISE)
                    noise |= 0x80;

                NamcoCus30WriteData(parentModule.UnitNumber, 0x100 + (uint)Slot * 8 + 0x00, fv_l);
                NamcoCus30WriteData(parentModule.UnitNumber, 0x100 + (uint)Slot * 8 + 0x04, fv_r);
                NamcoCus30WriteData(parentModule.UnitNumber, 0x100 + (uint)(((Slot - 1) * 8) & 0x3f) + 0x04, noise);
            }

            public override void OnPanpotUpdated()
            {
                OnVolumeUpdated();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="slot"></param>
            public override void OnPitchUpdated()
            {
                double d = CalcCurrentPitchDeltaNoteNumber();
                int nn = NoteOnEvent.NoteNumber;
                if (ParentModule.ChannelTypes[NoteOnEvent.Channel] == ChannelType.Drum)
                    nn = (int)ParentModule.DrumTimbres[NoteOnEvent.NoteNumber].BaseNote;
                double noteNum = Math.Pow(2.0, ((double)nn + d - 69.0) / 12.0);
                double freq = 440.0 * noteNum;

                //max 1048575(20bit)
                //midi 8.175798915643707 ～ 12543.853951415975Hz
                // A4 440 -> 440 * 500 = 440000
                // A6 1760 -> 1760 * 500 = 880000
                //adjust
                double xfreq = 29.00266666666667 * noteNum;
                freq = Math.Round((freq - xfreq) * 93.75);
                if (freq > 0xfffff)
                    freq = 0xfffff;
                uint n = (uint)freq;

                NamcoCus30WriteData(parentModule.UnitNumber, 0x100 + (uint)Slot * 8 + 0x01, (byte)((byte)((Slot & 0xf) << 4) | ((n >> 16) & 0xf)));
                NamcoCus30WriteData(parentModule.UnitNumber, 0x100 + (uint)Slot * 8 + 0x02, (byte)((n >> 8) & 0xff));
                NamcoCus30WriteData(parentModule.UnitNumber, 0x100 + (uint)Slot * 8 + 0x03, (byte)(n & 0xff));

                base.OnPitchUpdated();
            }

            /// <summary>
            /// 
            /// </summary>
            public override void SoundOff()
            {
                base.SoundOff();

                NamcoCus30WriteData(parentModule.UnitNumber, 0x100 + (uint)Slot * 8 + 0x00, 0x0);
                byte org = (byte)(NamcoCus30ReadData(parentModule.UnitNumber, 0x100 + (uint)Slot * 8 + 0x04) & 0x80);
                NamcoCus30WriteData(parentModule.UnitNumber, 0x100 + (uint)Slot * 8 + 0x04, org);
            }

        }

        /* namcos1 register map
    0x00        ch 0    left volume 0-15
    0x01        ch 0    waveform select((data >> 4) & 15) & frequency ((data & 15) << 16)
    0x02-0x03   ch 0    frequency (0x02 << 8 | 0x03)
    0x04        ch 0    right volume AND (data & 0x0f;)
    0x04        ch 1    noise sw ((data & 0x80) >> 7)

    0x08        ch 1    left volume
    0x09        ch 1    waveform select & frequency
    0x0a-0x0b   ch 1    frequency
    0x0c        ch 1    right volume AND
    0x0c        ch 2    noise sw

    .
    .
    .

    0x38        ch 7    left volume
    0x39        ch 7    waveform select & frequency
    0x3a-0x3b   ch 7    frequency
    0x3c        ch 7    right volume AND
    0x3c        ch 0    noise sw
*/

        /// <summary>
        /// 
        /// </summary>
        [JsonConverter(typeof(NoTypeConverterJsonConverter<NAMCO_CUS30Timbre>))]
        [DataContract]
        [InstLock]
        public class NAMCO_CUS30Timbre : TimbreBase
        {
            private SoundType f_SoundType;

            /// <summary>
            /// 
            /// </summary>
            [Category("Sound")]
            [Description("Sound Type")]
            [DataMember]
            [DefaultValue(SoundType.WSG)]
            public SoundType SoundType
            {
                get
                {
                    return f_SoundType;
                }
                set
                {
                    f_SoundType = value;
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

            public NAMCO_CUS30Timbre()
            {
            }

            public override void RestoreFrom(string serializeData)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<NAMCO_CUS30Timbre>(serializeData);
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

        /// <summary>
        /// 
        /// </summary>
        public enum SoundType
        {
            WSG,
            NOISE,
        }

    }
}