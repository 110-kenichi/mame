// copyright-holders:K.Ito
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;
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

//http://www.retrobits.net/atari/sam.shtml
//https://github.com/s-macke/SAM

namespace zanac.MAmidiMEmo.Instruments.Chips
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class SAM : InstrumentBase
    {

        public override string Name => "SAM";

        public override string Group => "VOICE";

        public override InstrumentType InstrumentType => InstrumentType.SAM;

        [Browsable(false)]
        public override string ImageKey => "SAM";

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        protected override string SoundInterfaceTagNamePrefix => "sam_";

        private static object samLockObject = new object();

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
                return 26;
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
        public SAMTimbre[] Timbres
        {
            get;
            set;
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
                using (var obj = JsonConvert.DeserializeObject<Beep>(serializeData))
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
        private delegate void delegate_sam_start_raw(uint unitNumber, byte channel,
            IntPtr sampledata, uint samples, uint frequency, bool loop);


        /// <summary>
        /// 
        /// </summary>
        private static delegate_sam_start_raw sam_start_raw
        {
            get;
            set;
        }

        private Queue<byte> commandDataQueue = new Queue<byte>();

        /// <summary>
        /// 
        /// </summary>
        private void SAMStartRaw(byte channel,
            IntPtr sampledata, uint length, uint frequency, bool loop)
        {
            DeferredWriteData(sam_start_raw, UnitNumber, channel, sampledata, length, frequency, loop);
            /*
            try
            {
                Program.SoundUpdating();
                sam_start_raw(UnitNumber, channel, sampledata, length, frequency, loop);
            }
            finally
            {
                Program.SoundUpdated();
            }
            //*/
        }

        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delegate_sam_stop(uint unitNumber, int channel);


        /// <summary>
        /// 
        /// </summary>
        private static delegate_sam_stop sam_stop
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        private void SAMStop(int channel)
        {
            try
            {
                Program.SoundUpdating();

                sam_stop(UnitNumber, channel);
            }
            finally
            {
                Program.SoundUpdated();
            }
        }


        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delegate_sam_set_frequency(uint unitNumber, int channel, uint frequency);


        /// <summary>
        /// 
        /// </summary>
        private static delegate_sam_set_frequency sam_set_frequency
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        private void SAMSetFrequency(int channel, uint frequency)
        {
            DeferredWriteData(sam_set_frequency, UnitNumber, channel, frequency);

            /*
            try
            {
                Program.SoundUpdating();

                sam_set_frequency(UnitNumber, channel, frequency);
            }
            finally
            {
                Program.SoundUpdated();
            }
            */
        }

        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delegate_sam_set_volume(uint unitNumber, int channel, float volume);


        /// <summary>
        /// 
        /// </summary>
        private static delegate_sam_set_volume sam_set_volume
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        private void SAMSetVolume(int channel, float volume)
        {
            DeferredWriteData(sam_set_volume, UnitNumber, channel, volume);

            /*
            try
            {
                Program.SoundUpdating();

                sam_set_volume(UnitNumber, channel, volume);
            }
            finally
            {
                Program.SoundUpdated();
            }
            */
        }

        [DllImport("SAM.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        static extern int SaySAM(string input, byte phonetic, byte singMode, byte pitch, byte speed, byte mouse, byte throat);

        [DllImport("SAM.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr GetBuffer();

        [DllImport("SAM.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        static extern int GetBufferLength();

        [DllImport("SAM.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr TextToPhonemes(string input);

        /// <summary>
        /// 
        /// </summary>
        static SAM()
        {
            IntPtr funcPtr = MameIF.GetProcAddress("sam_start_raw");
            if (funcPtr != IntPtr.Zero)
            {
                sam_start_raw = (delegate_sam_start_raw)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_sam_start_raw));
            }
            funcPtr = MameIF.GetProcAddress("sam_stop");
            if (funcPtr != IntPtr.Zero)
            {
                sam_stop = (delegate_sam_stop)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_sam_stop));
            }
            funcPtr = MameIF.GetProcAddress("sam_set_frequency");
            if (funcPtr != IntPtr.Zero)
            {
                sam_set_frequency = (delegate_sam_set_frequency)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_sam_set_frequency));
            }
            funcPtr = MameIF.GetProcAddress("sam_set_volume");
            if (funcPtr != IntPtr.Zero)
            {
                sam_set_volume = (delegate_sam_set_volume)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_sam_set_volume));
            }
        }

        private SAMSoundManager soundManager;

        /// <summary>
        /// 
        /// </summary>
        public SAM(uint unitNumber) : base(unitNumber)
        {
            GainLeft = DEFAULT_GAIN;
            GainRight = DEFAULT_GAIN;

            Timbres = new SAMTimbre[InstrumentBase.DEFAULT_MAX_TIMBRES];
            for (int i = 0; i < InstrumentBase.DEFAULT_MAX_TIMBRES; i++)
                Timbres[i] = new SAMTimbre();
            setPresetInstruments();

            this.soundManager = new SAMSoundManager(this);
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
            Timbres[0].Phonemes = "/HAALAOAO";
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

        protected override void OnNrpnDataEntered(ControlChangeEvent dataMsb, ControlChangeEvent dataLsb)
        {
            base.OnNrpnDataEntered(dataMsb, dataLsb);

            soundManager.ProcessNrpnData(dataMsb, dataLsb);
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
        private class SAMSoundManager : SoundManagerBase
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

            private static SoundList<SAMSound> psgOnSounds = new SoundList<SAMSound>(1);

            private SAM parentModule;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            public SAMSoundManager(SAM parent) : base(parent)
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

                foreach (SAMTimbre timbre in parentModule.GetBaseTimbres(note))
                {
                    var emptySlot = searchEmptySlot(note);
                    if (emptySlot.slot < 0)
                        continue;

                    SAMSound snd = new SAMSound(emptySlot.inst, this, timbre, note, emptySlot.slot);
                    psgOnSounds.Add(snd);

                    FormMain.OutputDebugLog(parentModule, "KeyOn SAM ch" + emptySlot + " " + note.ToString());
                    rv.Add(snd);
                }
                for (int i = 0; i < rv.Count; i++)
                {
                    var snd = rv[i];
                    if (!snd.IsDisposed)
                    {
                        snd.KeyOn();
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
            private (SAM inst, int slot) searchEmptySlot(TaggedNoteOnEvent note)
            {
                return SearchEmptySlotAndOffForLeader(parentModule, psgOnSounds, note, 1);
            }

            internal override void ProcessAllSoundOff()
            {
                var me = new ControlChangeEvent((SevenBitNumber)120, (SevenBitNumber)0);
                ProcessControlChange(me);

                for (int i = 0; i < 8; i++)
                    parentModule.SAMStop(i);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private class SAMSound : SoundBase
        {

            private SAM parentModule;

            private SAMTimbre timbre;

            private IntPtr bufferPtr;

            private object lockObject = new object();

            private bool played;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentModule"></param>
            /// <param name="noteOnEvent"></param>
            /// <param name="programNumber"></param>
            /// <param name="slot"></param>
            public SAMSound(SAM parentModule, SAMSoundManager manager, TimbreBase timbre, TaggedNoteOnEvent noteOnEvent, int slot) : base(parentModule, manager, timbre, noteOnEvent, slot)
            {
                this.parentModule = parentModule;
                this.timbre = (SAMTimbre)timbre;
            }

            public override void Dispose()
            {
                base.Dispose();

                parentModule.SAMStop(Slot);

                lock (lockObject)
                {
                    if (bufferPtr != IntPtr.Zero)
                        Marshal.FreeHGlobal(bufferPtr);
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public override void KeyOn()
            {
                base.KeyOn();

                var tb = NoteOnEvent.Tag as NoteOnTimbreInfo;
                if (tb?.Tag is string phonemes)
                {
                    var len = 0;
                    byte[] tmpBuf = null;
                    lock (samLockObject)
                    {
                        var result = SaySAM(phonemes, 1, 0, 64, 72, 128, 128);
                        if (result != 0)
                            return;

                        len = GetBufferLength() / 50;
                        if (len == 0)
                            return;
                        tmpBuf = new byte[len];
                        Marshal.Copy(GetBuffer(), tmpBuf, 0, len);
                    }
                    short[] data = new short[len];
                    for (int i = 0; i < len; i++)
                        data[i] = (short)((tmpBuf[i] ^ 0x80) << 8);
                    lock (lockObject)
                    {
                        bufferPtr = Marshal.AllocHGlobal(data.Length * 2);
                        Marshal.Copy(data, 0, bufferPtr, data.Length);
                        parentModule.SAMStartRaw((byte)Slot, bufferPtr, (uint)data.Length, 22050, false);
                        played = true;
                    }
                }
                else
                {
                    var data = timbre.WaveData;
                    if (data == null || data.Length == 0)
                        return;
                    lock (lockObject)
                    {
                        bufferPtr = Marshal.AllocHGlobal(data.Length * 2);
                        Marshal.Copy(data, 0, bufferPtr, data.Length);
                        parentModule.SAMStartRaw((byte)Slot, bufferPtr, (uint)data.Length, 22050, false);
                        played = true;
                    }
                }

                parentModule.SAMSetFrequency(Slot, (uint)Math.Round(22050 * (CalcCurrentFrequency() / 440d)));
                //parentModule.SAMSetVolume(Slot, (float)CalcCurrentVolume());
            }

            /// <summary>
            /// 
            /// </summary>
            public override void OnPitchUpdated()
            {
                parentModule.SAMSetFrequency(Slot, (uint)Math.Round(22050 * (CalcCurrentFrequency() / 440d)));

                base.OnPitchUpdated();
            }

            public override void OnVolumeUpdated()
            {
                //parentModule.SAMSetVolume(Slot, (float)CalcCurrentVolume());

                base.OnVolumeUpdated();
            }

            /// <summary>
            /// 
            /// </summary>
            public override void SoundOff()
            {
                base.SoundOff();

                lock (lockObject)
                {
                    if (played)
                        parentModule.SAMStop(Slot);
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        [JsonConverter(typeof(NoTypeConverterJsonConverter<SAMTimbre>))]
        [DataContract]
        public class SAMTimbre : TimbreBase
        {
            private string words;

            [DataMember]
            [Category("Sound")]
            [Description("Set natural words tp convert to Phonemes (MAX: 252 bytes). Base frequency is 440Hz(A4).")]
            [DefaultValue(null)]
            public string Words
            {
                get
                {
                    return words;
                }
                set
                {
                    if (words != value)
                    {
                        var old = words;
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            string cnv = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(value));
                            IntPtr ptr = TextToPhonemes(cnv);
                            if (ptr != IntPtr.Zero)
                            {
                                words = value;
                                var rv = Marshal.PtrToStringAnsi(ptr);
                                Phonemes = rv.Trim();
                                words = value;
                            }
                        }
                        else
                        {
                            words = null;
                            Phonemes = null;
                        }
                    }
                }
            }

            private string phonemes;

            [DataMember]
            [Category("Sound")]
            [Description("Set phonemes (MAX: 252 bytes). Base frequency is 440Hz(A4).\r\n" +
                "See the http://www.retrobits.net/atari/sam.shtml for more details")]
            [DefaultValue(null)]
            [Editor(typeof(PhonemesUITypeEditor), typeof(UITypeEditor)), Localizable(false)]
            public string Phonemes
            {
                get
                {
                    return phonemes;
                }
                set
                {
                    if (phonemes != value)
                    {
                        if (createWaveData(value))
                        {
                            phonemes = value;
                            words = null;
                        }
                    }
                }
            }

            private bool sing;

            [DataMember]
            [Category("Sound")]
            [Description("Set singmode).")]
            [DefaultValue(false)]
            public bool SingMode
            {
                get
                {
                    return sing;
                }
                set
                {
                    if (sing != value)
                    {
                        var old = sing;
                        sing = value;
                        if (!createWaveData(Phonemes))
                            sing = old;
                    }
                }
            }

            private byte pitch = 64;

            [DataMember]
            [Category("Sound")]
            [Description("Set pitch.")]
            [DefaultValue((byte)64)]
            public byte Pitch
            {
                get
                {
                    return pitch;
                }
                set
                {
                    if (pitch != value)
                    {
                        var old = pitch;
                        pitch = value;
                        if (!createWaveData(Phonemes))
                            pitch = old;
                    }
                }
            }

            private byte speed = 72;

            [DataMember]
            [Category("Sound")]
            [Description("Set speed.")]
            [DefaultValue((byte)72)]
            public byte Speed
            {
                get
                {
                    return speed;
                }
                set
                {
                    if (speed != value)
                    {
                        var old = speed;
                        speed = value;
                        if (!createWaveData(Phonemes))
                            speed = old;
                    }
                }
            }

            private byte mouse = 128;

            [DataMember]
            [Category("Sound")]
            [Description("Set mouse.")]
            [DefaultValue((byte)128)]
            public byte Mouse
            {
                get
                {
                    return mouse;
                }
                set
                {
                    if (mouse != value)
                    {
                        var old = mouse;
                        mouse = value;
                        if (!createWaveData(Phonemes))
                            mouse = old;
                    }
                }
            }

            private byte throat = 128;

            [DataMember]
            [Category("Sound")]
            [Description("Set throat.")]
            [DefaultValue((byte)128)]
            public byte Throat
            {
                get
                {
                    return throat;
                }
                set
                {
                    if (throat != value)
                    {
                        var old = throat;
                        throat = value;
                        if (!createWaveData(Phonemes))
                            throat = old;
                    }
                }
            }

            [Browsable(false)]
            [IgnoreDataMember]
            [JsonIgnore]
            public short[] WaveData
            {
                get;
                set;
            }

            [Browsable(false)]
            [IgnoreDataMember]
            [JsonIgnore]
            public int WaveDataLength
            {
                get;
                set;
            }

            private bool createWaveData(string phonemes)
            {
                int result = -1;
                byte[] tmpBuf = null;
                if (!string.IsNullOrWhiteSpace(phonemes))
                {
                    lock (samLockObject)
                    {
                        result = SaySAM(phonemes, 1, SingMode ? (byte)1 : (byte)0, Pitch, Speed, Mouse, Throat);
                        if (result != 0)
                            return false;

                        WaveDataLength = GetBufferLength() / 50;

                        tmpBuf = new byte[WaveDataLength];
                        Marshal.Copy(GetBuffer(), tmpBuf, 0, WaveDataLength);
                    }
                    WaveData = new short[WaveDataLength];
                    for (int i = 0; i < WaveDataLength; i++)
                        WaveData[i] = (short)((tmpBuf[i] ^ 0x80) << 8);
                }
                else
                {
                    WaveData = null;
                    WaveDataLength = 0;
                }
                return true;
            }

            /// <summary>
            /// 
            /// </summary>
            public SAMTimbre()
            {
                this.SDS.FxS = new BasicFxSettings();
            }

            public override void RestoreFrom(string serializeData)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<SAMTimbre>(serializeData);
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

    }
}