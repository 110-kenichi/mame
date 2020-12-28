// copyright-holders:K.Ito
using MathParserTK;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using zanac.MAmidiMEmo.ComponentModel;
using zanac.MAmidiMEmo.Instruments.Envelopes;
using zanac.MAmidiMEmo.Midi;

namespace zanac.MAmidiMEmo.Instruments
{

    /// <summary>
    /// 
    /// </summary>
    public abstract class SoundManagerBase : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        protected virtual SoundList<SoundBase> AllSounds
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        protected Dictionary<int, ArpEngine> ArpeggiatorsForKeyOn
        {
            get;
            private set;
        }

        /// 
        /// </summary>
        protected Dictionary<int, ArpEngine> ArpeggiatorsForPitch
        {
            get;
            private set;
        }

        /// <summary>
        /// ポルタメント用の最後に鳴ったキー番号
        /// </summary>
        private Dictionary<int, int> LastNoteNumbers = new Dictionary<int, int>();

        private InstrumentBase parentModule;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SoundManagerBase(InstrumentBase parentModule)
        {
            this.parentModule = parentModule;
            AllSounds = new SoundList<SoundBase>(-1);
            ArpeggiatorsForKeyOn = new Dictionary<int, ArpEngine>();
            ArpeggiatorsForPitch = new Dictionary<int, ArpEngine>();
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Dispose()
        {
            {
                foreach (var ch in ArpeggiatorsForKeyOn.Keys)
                {
                    var arp = ArpeggiatorsForKeyOn[ch];
                    if (arp.ArpAction != null)
                        arp.ArpAction = null;
                    var lnote = arp.LastPassedNote;
                    if (lnote != null)
                        keyOffCore(new NoteOffEvent(lnote.NoteNumber, (SevenBitNumber)0) { Channel = lnote.Channel });
                    arp.ClearAddedNotes();
                }
                ArpeggiatorsForKeyOn.Clear();
            }
            {
                foreach (var ch in ArpeggiatorsForPitch.Keys)
                {
                    var arp = ArpeggiatorsForPitch[ch];
                    if (arp.ArpAction != null)
                        arp.ArpAction = null;
                    var lnote = arp.FirstAddedNote;
                    if (lnote != null)
                        keyOffCore(new NoteOffEvent(lnote.NoteNumber, (SevenBitNumber)0) { Channel = lnote.Channel });
                    arp.ClearAddedNotes();
                }
            }

            for (int i = AllSounds.Count - 1; i >= 0; i--)
            {
                var removed = AllSounds[i];
                if (removed.ParentModule.UnitNumber != parentModule.UnitNumber)
                    continue;

                AllSounds.RemoveAt(i);
                removed.Dispose();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        public virtual void ProcessPitchBend(PitchBendEvent midiEvent)
        {
            foreach (var t in AllSounds)
            {
                //if (t.IsSoundOff)
                //    continue;

                if (t.ParentModule.UnitNumber != parentModule.UnitNumber)
                    continue;

                if (t.NoteOnEvent.Channel == midiEvent.Channel)
                    t.OnPitchUpdated();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="value"></param>
        public virtual void ProcessControlChange(ControlChangeEvent midiEvent)
        {
            switch (midiEvent.ControlNumber)
            {
                case 1:    //Modulation
                    foreach (var t in AllSounds)
                    {
                        if (t.IsSoundOff)
                            continue;
                        if (t.ParentModule.UnitNumber != parentModule.UnitNumber)
                            continue;

                        if (t.NoteOnEvent.Channel == midiEvent.Channel)
                            t.ModulationEnabled = midiEvent.ControlValue != 0;
                    }
                    break;
                case 6:    //Data Entry MSB

                    break;
                case 38:   //Data Entry MSB

                    break;
                case 7:    //Volume
                    foreach (var t in AllSounds)
                    {
                        //if (t.IsSoundOff)
                        //    continue;
                        if (t.ParentModule.UnitNumber != parentModule.UnitNumber)
                            continue;

                        if (t.NoteOnEvent.Channel == midiEvent.Channel)
                            t.OnVolumeUpdated();
                    }
                    break;
                case 10:    //Panpot
                    foreach (var t in AllSounds)
                    {
                        //if (t.IsSoundOff)
                        //    continue;
                        if (t.ParentModule.UnitNumber != parentModule.UnitNumber)
                            continue;

                        if (t.NoteOnEvent.Channel == midiEvent.Channel)
                            t.OnPanpotUpdated();
                    }
                    break;
                case 11:    //Expression
                    foreach (var t in AllSounds)
                    {
                        //if (t.IsSoundOff)
                        //    continue;
                        if (t.ParentModule.UnitNumber != parentModule.UnitNumber)
                            continue;

                        if (t.NoteOnEvent.Channel == midiEvent.Channel)
                            t.OnVolumeUpdated();
                    }
                    break;

                //GPCS1
                case 16:
                case 17:
                case 18:
                case 19:
                    {
                        int no = midiEvent.ControlNumber - 16 + 1;
                        processGPCS(midiEvent.Channel, no, midiEvent.ControlValue, 0);
                    }
                    break;
                case 64:    //Holds
                    if (parentModule.Holds[midiEvent.Channel] < 64 && parentModule.LastHolds[midiEvent.Channel] >= 64)
                    {
                        foreach (var t in AllSounds)
                        {
                            if (t.IsSoundOff)
                                continue;
                            if (t.IsKeyOff && t.NoteOnEvent.Channel == midiEvent.Channel)
                                ProcessKeyOff(new NoteOffEvent(t.NoteOnEvent.NoteNumber, (SevenBitNumber)0) { Channel = t.NoteOnEvent.Channel });
                        }
                    }
                    break;
                //GPCS2
                case 80:
                case 81:
                case 82:
                case 83:
                    {
                        int no = midiEvent.ControlNumber - 80 + 1;
                        processGPCS(midiEvent.Channel, no, midiEvent.ControlValue, 0);
                    }
                    break;

                //Sound Control
                case 70:
                case 71:
                case 72:
                case 73:
                case 74:
                case 75:
                case 79:
                    processSCCS(midiEvent.Channel, midiEvent.ControlNumber, midiEvent.ControlValue, 0);
                    break;
                case 84:
                    LastNoteNumbers[midiEvent.Channel] = 0x80 | (int)midiEvent.ControlValue;
                    break;
                case 120:   //All Sounds Off
                case 123:   //All Note Off
                    {
                        //clear all arps
                        stopAllArpForKeyOn();
                        stopAllArpForPitch();
                        foreach (var t in AllSounds)
                        {
                            if (t.IsSoundOff)
                                continue;
                            if (t.ParentModule.UnitNumber != parentModule.UnitNumber)
                                continue;

                            ProcessKeyOff(new NoteOffEvent(t.NoteOnEvent.NoteNumber, (SevenBitNumber)0) { Channel = t.NoteOnEvent.Channel });
                        }
                        break;
                    }
            }
        }

        private ControlChangeEvent[] lastDataLsb = new ControlChangeEvent[16];

        private ControlChangeEvent[] lastDataMsb = new ControlChangeEvent[16];

        public virtual void ProcessNrpnData(ControlChangeEvent dataMsb, ControlChangeEvent dataLsb)
        {
            if (dataMsb != null)
            {
                switch (parentModule.NrpnMsb[dataMsb.Channel])
                {
                    default:
                        break;
                }

                lastDataMsb[dataMsb.Channel] = dataMsb;
            }
            if (dataLsb != null)
            {
                switch (parentModule.NrpnMsb[dataLsb.Channel])
                {
                    case 0:
                        {
                            switch (parentModule.NrpnLsb[dataMsb.Channel])
                            {
                                //GPCS1
                                case 16:
                                case 17:
                                case 18:
                                case 19:
                                    if (lastDataLsb[dataLsb.Channel] != null)
                                    {
                                        int no = parentModule.NrpnLsb[dataMsb.Channel] - 16 + 1;
                                        processGPCS(dataMsb.Channel, no, lastDataMsb[dataMsb.Channel].ControlValue, dataLsb.ControlValue);
                                    }
                                    break;
                                //SCCS
                                case 70:
                                case 71:
                                case 72:
                                case 73:
                                case 74:
                                case 75:
                                case 79:
                                    if (lastDataLsb[dataLsb.Channel] != null)
                                        processSCCS(dataMsb.Channel, parentModule.NrpnLsb[dataMsb.Channel], lastDataMsb[dataMsb.Channel].ControlValue, dataLsb.ControlValue);
                                    break;
                                //GPCS2
                                case 80:
                                case 81:
                                case 82:
                                case 83:
                                    if (lastDataLsb[dataLsb.Channel] != null)
                                    {
                                        int no = parentModule.NrpnLsb[dataMsb.Channel] - 80 + 1;
                                        processGPCS(dataMsb.Channel, no, lastDataMsb[dataMsb.Channel].ControlValue, dataLsb.ControlValue);
                                    }
                                    break;
                            }
                            break;
                        }
                    default:
                        break;
                }

                lastDataLsb[dataLsb.Channel] = dataLsb;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        abstract internal void ProcessAllSoundOff();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        private void processSCCS(int channel, int controlNumber, int msbValue, int lsbValue)
        {
            var tim = parentModule.GetLastTimbre(channel);
            bool process = false;
            foreach (var ipi in tim.SCCS.GetPropertyInfo(tim, controlNumber - 70 + 1))
            {
                if (ipi != null)
                {
                    var pi = ipi.Property;

                    SlideParametersAttribute attribute =
                        Attribute.GetCustomAttribute(pi, typeof(SlideParametersAttribute)) as SlideParametersAttribute;
                    if (attribute != null)
                    {
                        int len = attribute.SliderMax - attribute.SliderMin;
                        int val = len * msbValue / 127;
                        val += (len / 127) * lsbValue / 127;
                        val += attribute.SliderMin;

                        var pd = TypeDescriptor.GetProperties(pi.DeclaringType)[pi.Name];
                        try
                        {
                            //InstrumentManager.ExclusiveLockObject.EnterWriteLock();

                            val = (int)Math.Round(MathParser.DefaultMathParser.Parse(ipi.Formula.Replace(ipi.Symbol, val.ToString())));
                            if (val < attribute.SliderMin)
                                val = attribute.SliderMin;
                            if (val > attribute.SliderMax)
                                val = attribute.SliderMax;

                            pd.SetValue(ipi.Owner, pd.Converter.ConvertFromString(val.ToString()));
                        }
                        finally
                        {
                            //InstrumentManager.ExclusiveLockObject.ExitWriteLock();
                        }
                        process = true;
                    }
                    else
                    {
                        DoubleSlideParametersAttribute dattribute =
                            Attribute.GetCustomAttribute(pi, typeof(DoubleSlideParametersAttribute)) as DoubleSlideParametersAttribute;
                        if (dattribute != null)
                        {
                            double len = dattribute.SliderMax - dattribute.SliderMin;
                            double val = len * (double)msbValue / 127d;
                            val += (len / 127d) * (double)lsbValue / 127d;
                            val += attribute.SliderMin;

                            var pd = TypeDescriptor.GetProperties(pi.DeclaringType)[pi.Name];
                            try
                            {
                                //InstrumentManager.ExclusiveLockObject.EnterWriteLock();

                                val = Math.Round(MathParser.DefaultMathParser.Parse(ipi.Formula.Replace(ipi.Symbol, val.ToString())));
                                if (val < dattribute.SliderMin)
                                    val = dattribute.SliderMin;
                                if (val > dattribute.SliderMax)
                                    val = dattribute.SliderMax;

                                pd.SetValue(ipi.Owner, pd.Converter.ConvertFromString(val.ToString()));
                            }
                            finally
                            {
                                //InstrumentManager.ExclusiveLockObject.ExitWriteLock();
                            }
                            process = true;
                        }
                        else
                        {
                            if (ipi.Property.PropertyType == typeof(bool))
                            {
                                var pd = TypeDescriptor.GetProperties(pi.DeclaringType)[pi.Name];
                                try
                                {
                                    //InstrumentManager.ExclusiveLockObject.EnterWriteLock();

                                    var val = (int)Math.Round(MathParser.DefaultMathParser.Parse(ipi.Formula.Replace(ipi.Symbol, msbValue.ToString())));

                                    pd.SetValue(ipi.Owner, val > 127 / 2);
                                }
                                finally
                                {
                                    //InstrumentManager.ExclusiveLockObject.ExitWriteLock();
                                }
                                process = true;
                            }
                            else if (ipi.Property.PropertyType.IsEnum)
                            {
                                var vals = Enum.GetValues(ipi.Property.PropertyType);
                                int val = vals.Length * msbValue / 127;
                                val += (vals.Length / 127) * lsbValue / 127;

                                var pd = TypeDescriptor.GetProperties(pi.DeclaringType)[pi.Name];
                                try
                                {
                                    //InstrumentManager.ExclusiveLockObject.EnterWriteLock();

                                    pd.SetValue(ipi.Owner, vals.GetValue(val));
                                }
                                finally
                                {
                                    //InstrumentManager.ExclusiveLockObject.ExitWriteLock();
                                }
                                process = true;
                            }
                        }
                    }
                }
            }
            if (process)
            {
                foreach (var t in AllSounds)
                {
                    if (t.ParentModule.UnitNumber != parentModule.UnitNumber)
                        continue;

                    if (t.NoteOnEvent.Channel == channel && t.Timbre == tim)
                        t.OnSoundParamsUpdated();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        /// <param name="no"></param>
        private void processGPCS(int channel, int controlNumber, int msbValue, int lsbValue)
        {
            bool process = false;
            foreach (var ipi in parentModule.GPCS[channel].GetPropertyInfo(parentModule, controlNumber))
            {
                if (ipi != null)
                {
                    var pi = ipi.Property;

                    SlideParametersAttribute attribute =
                        Attribute.GetCustomAttribute(pi, typeof(SlideParametersAttribute)) as SlideParametersAttribute;
                    if (attribute != null)
                    {
                        int len = attribute.SliderMax - attribute.SliderMin;
                        int val = len * msbValue / 127;
                        val += (len / 127) * lsbValue / 127;
                        val += attribute.SliderMin;

                        var pd = TypeDescriptor.GetProperties(pi.DeclaringType)[pi.Name];
                        try
                        {
                            //InstrumentManager.ExclusiveLockObject.EnterWriteLock();

                            val = (int)Math.Round(MathParser.DefaultMathParser.Parse(ipi.Formula.Replace(ipi.Symbol, val.ToString())));
                            if (val < attribute.SliderMin)
                                val = attribute.SliderMin;
                            if (val > attribute.SliderMax)
                                val = attribute.SliderMax;

                            pd.SetValue(ipi.Owner, pd.Converter.ConvertFromString(val.ToString()));
                        }
                        finally
                        {
                            //InstrumentManager.ExclusiveLockObject.ExitWriteLock();
                        }
                        process = true;
                    }
                    else
                    {
                        DoubleSlideParametersAttribute dattribute =
                            Attribute.GetCustomAttribute(pi, typeof(DoubleSlideParametersAttribute)) as DoubleSlideParametersAttribute;
                        if (dattribute != null)
                        {
                            double len = dattribute.SliderMax - dattribute.SliderMin;
                            double val = len * (double)msbValue / 127d;
                            val += (len / 127d) * lsbValue / 127d;
                            val += attribute.SliderMin;

                            var pd = TypeDescriptor.GetProperties(pi.DeclaringType)[pi.Name];
                            try
                            {
                                //InstrumentManager.ExclusiveLockObject.EnterWriteLock();

                                val = Math.Round(MathParser.DefaultMathParser.Parse(ipi.Formula.Replace(ipi.Symbol, val.ToString())));
                                if (val < dattribute.SliderMin)
                                    val = dattribute.SliderMin;
                                if (val > dattribute.SliderMax)
                                    val = dattribute.SliderMax;

                                pd.SetValue(ipi.Owner, pd.Converter.ConvertFromString(val.ToString()));
                            }
                            finally
                            {
                                //InstrumentManager.ExclusiveLockObject.ExitWriteLock();
                            }
                            process = true;
                        }
                        else
                        {
                            if (ipi.Property.PropertyType == typeof(bool))
                            {
                                var pd = TypeDescriptor.GetProperties(pi.DeclaringType)[pi.Name];
                                try
                                {
                                    //InstrumentManager.ExclusiveLockObject.EnterWriteLock();

                                    var val = (int)Math.Round(MathParser.DefaultMathParser.Parse(ipi.Formula.Replace(ipi.Symbol, msbValue.ToString())));

                                    pd.SetValue(ipi.Owner, val > 127 / 2);
                                }
                                finally
                                {
                                    //InstrumentManager.ExclusiveLockObject.ExitWriteLock();
                                }
                                process = true;
                            }
                            else if (ipi.Property.PropertyType.IsEnum)
                            {
                                var vals = Enum.GetValues(ipi.Property.PropertyType);
                                int val = vals.Length * msbValue / 127;

                                var pd = TypeDescriptor.GetProperties(pi.DeclaringType)[pi.Name];
                                try
                                {
                                    //InstrumentManager.ExclusiveLockObject.EnterWriteLock();

                                    pd.SetValue(ipi.Owner, vals.GetValue(val));
                                }
                                finally
                                {
                                    //InstrumentManager.ExclusiveLockObject.ExitWriteLock();
                                }
                                process = true;
                            }
                        }
                    }
                }
            }
            if (process)
            {
                foreach (var t in AllSounds)
                {
                    if (t.ParentModule.UnitNumber != parentModule.UnitNumber)
                        continue;

                    if (t.NoteOnEvent.Channel == channel)
                        t.OnSoundParamsUpdated();
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public int GetLastNoteNumber(int channel)
        {
            if (LastNoteNumbers.ContainsKey(channel))
                return LastNoteNumbers[channel];
            return -1;
        }

        /// <summary>
        /// 
        /// </summary>
        private double processArpeggiatorsForKeyOn(object state)
        {
            var arp = (ArpEngine)state;
            if (arp.ArpAction == null)
                return -1;

            int ch = (int)arp.Channel;
            var timbre = parentModule.GetLastTimbre(ch);

            ArpSettings sds = parentModule.GlobalARP;
            if (timbre.SDS.ARP.Enable)
                sds = timbre.SDS.ARP;

            //end arp
            if (!sds.Enable ||
                sds.ArpMethod != ArpMethod.KeyOn ||
                (!sds.Hold && arp.ResetNextNoteOn) ||
                arp.AddedNotesCount == 0)
            {
                stopArpForKeyOn(arp);
                ArpeggiatorsForKeyOn.Remove(arp.Channel);
                return -1; // continue;
            }

            setupArp(sds, arp);

            var pnn = arp.PeekNextNote();   //do not keyoff/keyon if next note is same with now

            if (arp.GateCounter != -1)
            {
                arp.GateCounter += 1;
                if (arp.GateCounter >= (arp.StepNum * (sds.GateTime + 1)) / 128d)
                {
                    //Gate Time
                    arp.GateCounter = -1;
                    if (arp.LastPassedNote != null && arp.LastPassedNote.NoteNumber != pnn.NoteNumber)
                        keyOffLastPassedCore(arp);
                }
            }

            arp.StepCounter += 1;
            if (arp.StepCounter >= arp.StepNum)
            {
                // on sound
                arp.StepCounter = 0;
                arp.GateCounter = 0;
                var lp = arp.LastPassedNote;
                var nn = arp.NextNote();
                if (lp == null ||
                    (lp != null && lp.NoteNumber != nn.NoteNumber))
                    keyOnCore(nn);
            }

            return arp.Step;
        }


        /// <summary>
        /// 
        /// </summary>
        private double processArpeggiatorsForPitch(object state)
        {
            var arp = (ArpEngine)state;
            if (arp.ArpAction == null)
                return -1;

            int ch = (int)arp.Channel;
            var timbre = parentModule.GetLastTimbre(ch);

            ArpSettings sds = parentModule.GlobalARP;
            if (timbre.SDS.ARP.Enable)
                sds = timbre.SDS.ARP;

            //end arp
            if (!sds.Enable ||
                sds.ArpMethod != ArpMethod.PitchChange ||
                (!sds.Hold && arp.ResetNextNoteOn) ||
                arp.AddedNotesCount == 0)
            {
                //off sound
                stopArpForPitch(arp);
                ArpeggiatorsForPitch.Remove(arp.Channel);
                return -1; // continue;
            }

            setupArp(sds, arp);

            //Gate Time
            var snds = arp.FirstSoundForPitch;
            if (arp.GateCounter != -1)
            {
                arp.GateCounter += 1;
                if (arp.GateCounter >= (arp.StepNum * (sds.GateTime + 1)) / 128d)
                {
                    arp.GateCounter = -1;
                    if (sds.GateTime != 127)
                    {
                        foreach (var snd in snds)
                            snd.ArpeggiateLevel = 0d;
                    }
                }
            }
            arp.StepCounter += 1;
            if (arp.StepCounter >= arp.StepNum)
            {
                // on sound
                arp.StepCounter = 0;
                arp.GateCounter = 0;
                var note = arp.NextNote();
                foreach (var snd in snds)
                {
                    snd.ArpeggiateDeltaNoteNumber = note.NoteNumber - arp.FirstAddedNote.NoteNumber;
                    snd.ArpeggiateLevel = 1d;
                }
            }

            return arp.Step;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="note"></param>
        public virtual void ProcessKeyOn(TaggedNoteOnEvent note)
        {
            if (preProcessArrpegioForKeyOn(note))
                return;

            var snd = keyOnCore(note);

            postProcessArrpegioForKeyOn(note, snd);
        }

        private SoundBase[] keyOnCore(TaggedNoteOnEvent note)
        {
            var snd = SoundOn(note);
            if (snd != null && snd.Length != 0)
            {
                AllSounds.AddRange(snd);
                LastNoteNumbers[note.Channel] = note.NoteNumber;
                return snd;
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="note"></param>
        public virtual SoundBase[] SoundOn(TaggedNoteOnEvent note)
        {
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="note"></param>
        public virtual void ProcessKeyOff(NoteOffEvent note)
        {
            if (preProcessArpeggioForKeyOff(note))
                return;

            keyOffCore(note);
        }

        private void keyOffLastPassedCore(ArpEngine arp)
        {
            var lp = arp.LastPassedNote;
            if (lp != null)
                keyOffCore(new NoteOffEvent(lp.NoteNumber, (SevenBitNumber)0) { Channel = lp.Channel });
            arp.LastPassedNote = null;
        }

        private void keyOffCore(TaggedNoteOnEvent onote)
        {
            if (onote != null)
                keyOffCore(new NoteOffEvent(onote.NoteNumber, (SevenBitNumber)0) { Channel = onote.Channel });
        }

        private void keyOffCore(NoteOffEvent note)
        {
            foreach (SoundBase offsnd in AllSounds)
            {
                if (offsnd.ParentModule.UnitNumber != parentModule.UnitNumber)
                    continue;

                if (offsnd.IsKeyOff)
                    continue;

                if (offsnd.NoteOnEvent.Channel == note.Channel)
                {
                    if (offsnd.NoteOnEvent.NoteNumber == note.NoteNumber)
                    {
                        if (!offsnd.Timbre.IgnoreKeyOff)
                            offsnd.KeyOff();
                        //break;
                    }
                }
            }
        }

        #region アルペジオ関係

        /// <summary>
        /// 
        /// </summary>
        /// <param name="note"></param>
        /// <returns></returns>
        private bool preProcessArrpegioForKeyOn(TaggedNoteOnEvent note)
        {
            FourBitNumber ch = note.Channel;
            if (parentModule.ChannelTypes[ch] != ChannelType.Normal)
                return false;

            ArpSettings sds = parentModule.GetLastTimbre(ch).SDS.ARP;
            if (!sds.Enable)
                sds = parentModule.GlobalARP;
            if (sds.Enable)
            {
                switch (sds.ArpMethod)
                {
                    case ArpMethod.KeyOn:
                        {
                            //create Arpeggiator
                            if (!ArpeggiatorsForKeyOn.ContainsKey(ch))
                                ArpeggiatorsForKeyOn.Add(ch, new ArpEngine(ch));
                            var arp = ArpeggiatorsForKeyOn[ch];

                            setupArp(sds, arp);

                            //hold reset
                            if (arp.ResetNextNoteOn)
                            {
                                arp.ResetNextNoteOn = false;
                                keyOffLastPassedCore(arp);
                                arp.ClearAddedNotes();
                            }
                            arp.AddNote(note);

                            //即音を鳴らすためカウンターを進める
                            if (arp.AddedNotesCount == 1)
                                arp.StepCounter = arp.StepNum - 1;

                            if (arp.ArpAction == null)
                            {
                                arp.ArpAction = new Func<object, double>(processArpeggiatorsForKeyOn);
                                HighPrecisionTimer.SetPeriodicCallback(arp.ArpAction, arp.Step, arp);
                            }
                            return true;
                        }
                    case ArpMethod.PitchChange:
                        {
                            //create Arpeggiator
                            if (ArpeggiatorsForPitch.ContainsKey(ch))
                            {
                                var arp = ArpeggiatorsForPitch[ch];
                                setupArp(sds, arp);
                                if (arp.SkipNextNote == true)
                                    arp.SkipNextNote = false;

                                //hold reset
                                if (arp.ResetNextNoteOn)
                                {
                                    arp.ResetNextNoteOn = false;
                                    stopArpForPitch(arp);
                                    ArpeggiatorsForPitch.Remove(ch);
                                }
                                else
                                {
                                    arp.AddNote(note);
                                    return true;
                                }
                            }
                            break;
                        }
                }
            }
            return false;
        }

        private void postProcessArrpegioForKeyOn(TaggedNoteOnEvent note, SoundBase[] snd)
        {
            FourBitNumber ch = note.Channel;
            if (parentModule.ChannelTypes[ch] != ChannelType.Normal)
                return;

            ArpSettings sds = parentModule.GetLastTimbre(ch).SDS.ARP;
            if (!sds.Enable)
                sds = parentModule.GlobalARP;
            if (snd != null && sds.Enable)
            {
                if (sds.ArpMethod == ArpMethod.PitchChange)
                {
                    //create Arpeggiator
                    if (!ArpeggiatorsForPitch.ContainsKey(ch))
                        ArpeggiatorsForPitch.Add(ch, new ArpEngine(ch));
                    var arp = ArpeggiatorsForPitch[ch];
                    setupArp(sds, arp);
                    arp.FirstSoundForPitch = snd;
                    arp.AddNote(note);
                    //すでに音が鳴っているのでスキップする
                    arp.SkipNextNote = true;
                    if (arp.ArpAction == null)
                    {
                        arp.ArpAction = new Func<object, double>(processArpeggiatorsForPitch);
                        HighPrecisionTimer.SetPeriodicCallback(arp.ArpAction, arp.Step, arp);
                    }
                }
            }
        }

        private bool preProcessArpeggioForKeyOff(NoteOffEvent note)
        {
            int ch = note.Channel;
            if (ArpeggiatorsForKeyOn.ContainsKey(ch))
            {
                var timbre = parentModule.GetLastTimbre(ch);
                var sds = timbre.SDS.ARP;
                ArpEngine arp = ArpeggiatorsForKeyOn[ch];

                if (sds.Enable)
                {
                    //ignore keyoff if hold mode
                    if (sds.Hold && arp.AddedNotesCount != 1)
                    {
                        arp.ResetNextNoteOn = true;
                        return true;
                    }
                }
                if (arp.RemoveNote(note))
                {
                    if (arp.AddedNotesCount == 0)
                    {
                        if (arp.LastPassedNote != null)
                            keyOffLastPassedCore(arp);
                    }
                    //end arp
                    if (arp.AddedNotesCount == 0)
                    {
                        if (arp.ArpAction != null)
                            arp.ArpAction = null;
                        ArpeggiatorsForKeyOn.Remove(ch);
                    }
                    return true;
                }
            }
            else if (ArpeggiatorsForPitch.ContainsKey(ch))
            {
                var timbre = parentModule.GetLastTimbre(ch);
                var sds = timbre.SDS.ARP;
                ArpEngine arp = ArpeggiatorsForPitch[ch];

                if (sds.Enable)
                {
                    //ignore keyoff if hold mode
                    if (sds.Hold && arp.AddedNotesCount != 1)
                    {
                        arp.ResetNextNoteOn = true;
                        return true;
                    }
                }
                if (arp.RemoveNote(note))
                {
                    //end arp
                    if (arp.AddedNotesCount == 0)
                    {
                        if (arp.ArpAction != null)
                            arp.ArpAction = null;
                        keyOffCore(arp.FirstAddedNote);
                        ArpeggiatorsForPitch.Remove(ch);
                    }
                    return true;
                }
            }
            return false;
        }


        private static void setupArp(ArpSettings sds, ArpEngine arp)
        {
            arp.StepStyle = sds.StepStyle;
            arp.Range = sds.OctaveRange;
            arp.RetriggerType = sds.KeySync ? RetriggerType.Note : RetriggerType.Off;
            var steps = (60d * 1000d / sds.Beat) / (double)sds.ArpResolution;
            if (steps < 20)
            {
                arp.Step = steps / 10;
                arp.StepNum = 10;
            }
            else if (arp.StepNum < 50)
            {
                arp.Step = steps / 25;
                arp.StepNum = 25;
            }
            else
            {
                arp.Step = steps / 100;
                arp.StepNum = 100;
            }
        }

        private void stopAllArpForPitch()
        {
            foreach (var ch in ArpeggiatorsForPitch.Keys.ToArray())
                stopArpForPitch(ArpeggiatorsForPitch[ch]);
            ArpeggiatorsForPitch.Clear();
        }

        private void stopAllArpForKeyOn()
        {
            foreach (var ch in ArpeggiatorsForKeyOn.Keys.ToArray())
                stopArpForKeyOn(ArpeggiatorsForKeyOn[ch]);
            ArpeggiatorsForKeyOn.Clear();
        }

        private void stopArpForPitch(ArpEngine arp)
        {
            if (arp.ArpAction != null)
                arp.ArpAction = null;
            keyOffCore(arp.FirstAddedNote);
            arp.ClearAddedNotes();
        }

        private void stopArpForKeyOn(ArpEngine arp)
        {
            if (arp.ArpAction != null)
                arp.ArpAction = null;
            keyOffLastPassedCore(arp);
            arp.ClearAddedNotes();
        }


        #endregion

        /// <summary>
        /// 未使用のスロットを検索する
        /// 空が無い場合は最初に鳴った音を消す
        /// </summary>
        /// <param name="onSounds"></param>
        /// <param name="maxSlot"></param>
        /// <returns></returns>
        protected int SearchEmptySlotAndOff<I, T>(I inst, SoundList<T> onSounds, TaggedNoteOnEvent newNote, int maxSlot) where T : SoundBase where I : InstrumentBase
        {
            return SearchEmptySlotAndOff(inst, onSounds, newNote, maxSlot, -1);
        }

        /// <summary>
        /// 未使用のスロットを検索する
        /// 空が無い場合は最初に鳴った音を消す
        /// </summary>
        /// <param name="onSounds"></param>
        /// <param name="maxSlot"></param>
        /// <param name="slot">強制的に割り当てるスロット。-1なら強制しない</param>
        /// <returns></returns>
        protected int SearchEmptySlotAndOff<I, T>(I inst, SoundList<T> onSounds, TaggedNoteOnEvent newNote, int maxSlot, int slot) where T : SoundBase where I : InstrumentBase
        {
            if (slot < 0)
            {
                List<T> offSnds = new List<T>();
                List<T> sameChSnds = new List<T>();
                for (int i = 0; i < onSounds.Count; i++)
                {
                    var onSnd = onSounds[i];
                    if (onSnd.IsSoundOff)
                    {
                        offSnds.Add(onSnd);
                        continue;
                        /*
                        AllSounds.Remove(onSnd);
                        onSounds.RemoveAt(i);
                        onSnd.Dispose();
                        i--;
                        continue;
                        */
                    }
                    if (newNote.Channel == onSnd.NoteOnEvent.Channel)
                        sameChSnds.Add(onSnd);
                }

                //Process Mono Mode. Remove same ch sounds up to max.
                int mono = inst.MonoMode[newNote.Channel];
                if (mono != 0)
                {
                    for (int i = 0; i < sameChSnds.Count - (mono - 1); i++)
                    {
                        var onSnd = sameChSnds[i];
                        if (onSnd.IsSoundingStarted)
                        {
                            onSnd.SoundOff();
                            offSnds.Add(onSnd);
                            onSounds.Remove(onSnd);
                            /*
                            AllSounds.Remove(onSnd);
                            onSounds.Remove(onSnd);

                            sameChSnds.RemoveAt(i);
                            onSnd.Dispose();
                            i--;
                            */
                        }
                    }
                }

                //Delete same drum sound from same ch
                if (inst.ChannelTypes[newNote.Channel] == ChannelType.Drum)
                {
                    for (int i = 0; i < sameChSnds.Count; i++)
                    {
                        var onSnd = sameChSnds[i];
                        if (onSnd.IsSoundingStarted && onSnd.NoteOnEvent.NoteNumber == newNote.NoteNumber)
                        {
                            onSnd.SoundOff();
                            offSnds.Add(onSnd);
                            onSounds.Remove(onSnd);
                            /*
                            AllSounds.Remove(onSnd);
                            onSounds.Remove(onSnd);

                            sameChSnds.RemoveAt(i);
                            onSnd.Dispose();
                            i--;
                            */
                        }
                    }
                }

                //Search unused slot
                Dictionary<int, bool> usedSlotTable = new Dictionary<int, bool>();
                for (int i = 0; i < onSounds.Count; i++)
                    usedSlotTable.Add(onSounds[i].Slot, true);
                foreach (var offSnd in offSnds)
                    usedSlotTable.Add(offSnd.Slot, true);
                for (int i = 0; i < maxSlot; i++)
                {
                    if (!usedSlotTable.ContainsKey(i))
                        return i;
                }

                List<T> tmpOnSnds = new List<T>(onSounds);

                //Proces Poly mode
                List<byte> polyList = new List<byte>(parentModule.PolyMode);
                for (int i = tmpOnSnds.Count - 1; i >= 0; i--)
                {
                    var onSnd = tmpOnSnds[i];
                    if (polyList[onSnd.NoteOnEvent.Channel] > 0)
                    {
                        tmpOnSnds.RemoveAt(i);
                        polyList[onSnd.NoteOnEvent.Channel]--;
                    }
                }

                //Search last sound off sound
                offSnds.Sort(new Comparison<SoundBase>((x, y) =>
                {
                    if (x.SoundOffTime < y.SoundOffTime)
                        return -1;
                    else if (x.SoundOffTime > y.SoundOffTime)
                        return 1;
                    else
                        return 0;
                }));
                foreach (var snd in offSnds)
                {
                    if (snd.Slot < maxSlot && snd.IsSoundOff)
                    {
                        AllSounds.Remove(snd);
                        onSounds.Remove(snd);
                        snd.Dispose();
                        return snd.Slot;
                    }
                }
                foreach (var snd in tmpOnSnds)
                {
                    if (snd.Slot < maxSlot && snd.IsSoundOff)
                    {
                        AllSounds.Remove(snd);
                        onSounds.Remove(snd);
                        snd.Dispose();
                        return snd.Slot;
                    }
                }

                //Search last key off sound
                foreach (var snd in tmpOnSnds)
                {
                    if (snd.Slot < maxSlot && snd.IsKeyOff)
                    {
                        AllSounds.Remove(snd);
                        onSounds.Remove(snd);
                        snd.Dispose();
                        return snd.Slot;
                    }
                }

                //Search last key on sound
                foreach (var snd in tmpOnSnds)
                {
                    if (snd.Slot < maxSlot)
                    {
                        AllSounds.Remove(snd);
                        onSounds.Remove(snd);
                        snd.Dispose();
                        return snd.Slot;
                    }
                }

                //Search last sounding sound
                foreach (var snd in onSounds)
                {
                    if (snd.Slot < maxSlot)
                    {
                        AllSounds.Remove(snd);
                        onSounds.Remove(snd);
                        snd.Dispose();
                        return snd.Slot;
                    }
                }
            }
            else
            {
                //既存の音を消す
                for (int i = 0; i < onSounds.Count; i++)
                {
                    var snd = onSounds[i];
                    if (snd.Slot == slot)
                    {
                        AllSounds.Remove(snd);
                        onSounds.RemoveAt(i);
                        snd.Dispose();
                        break;
                    }
                }
                return slot;
            }

            return -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="I"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="inst"></param>
        /// <param name="onSounds"></param>
        /// <param name="newNote"></param>
        /// <param name="maxSlot"></param>
        /// <returns></returns>
        protected (I, int) SearchEmptySlotAndOffForLeader<I, T>(I inst, SoundList<T> onSounds, TaggedNoteOnEvent newNote, int maxSlot) where T : SoundBase where I : InstrumentBase
        {
            return SearchEmptySlotAndOffForLeader(inst, onSounds, newNote, maxSlot, -1, 0);
        }

        /// <summary>
        /// 未使用のスロットを検索する
        /// 空が無い場合は最初に鳴った音を消す
        /// </summary>
        /// <param name="onSounds"></param>
        /// <param name="maxSlot"></param>
        /// <param name="slot">強制的に割り当てるスロット。-1なら強制しない</param>
        /// <returns></returns>
        protected (I, int) SearchEmptySlotAndOffForLeader<I, T>(I inst, SoundList<T> onSounds, TaggedNoteOnEvent newNote, int maxSlot, int slot, int offset) where T : SoundBase where I : InstrumentBase
        {
            //gather leader and followers
            Dictionary<uint, InstrumentBase> instskey = new Dictionary<uint, InstrumentBase>();
            Dictionary<uint, Dictionary<int, bool>> usedSlotTable = new Dictionary<uint, Dictionary<int, bool>>();
            foreach (var i in InstrumentManager.GetInstruments(inst.DeviceID))
            {
                instskey.Add(i.UnitNumber, i);
                if (i.UnitNumber == inst.UnitNumber || inst.UnitNumber == (int)i.FollowerMode - 1)
                    usedSlotTable.Add(i.UnitNumber, new Dictionary<int, bool>());
            }

            if (slot < 0)
            {
                List<T> onSnds = new List<T>();
                List<T> offSnds = new List<T>();
                List<T> sameChSnds = new List<T>();
                for (int i = 0; i < onSounds.Count; i++)
                {
                    var onSnd = onSounds[i];
                    if (usedSlotTable.ContainsKey(onSnd.ParentModule.UnitNumber))
                    {
                        if (onSnd.IsSoundOff)
                        {
                            offSnds.Add(onSnd);
                            continue;
                            /*
                            AllSounds.Remove(onSnd);
                            onSounds.RemoveAt(i);
                            onSnd.Dispose();
                            i--;
                            continue;
                            */
                        }
                        onSnds.Add(onSnd);
                        if (newNote.Channel == onSnd.NoteOnEvent.Channel)
                            sameChSnds.Add(onSnd);
                    }
                }

                //Process Mono Mode. Remove same ch sounds up to max.
                int mono = inst.MonoMode[newNote.Channel];
                if (mono != 0)
                {
                    for (int i = 0; i < sameChSnds.Count - (mono - 1); i++)
                    {
                        var onSnd = sameChSnds[i];
                        if (onSnd.IsSoundingStarted)
                        {
                            onSnd.SoundOff();
                            offSnds.Add(onSnd);
                            onSnds.Remove(onSnd);
                            /*
                            AllSounds.Remove(onSnd);
                            onSounds.Remove(onSnd);

                            onSnds.Remove(onSnd);
                            sameChSnds.RemoveAt(i);
                            onSnd.Dispose();
                            i--;
                            */
                        }
                    }
                }

                //Delete same drum sound from same ch
                if (inst.ChannelTypes[newNote.Channel] == ChannelType.Drum)
                {
                    for (int i = 0; i < sameChSnds.Count; i++)
                    {
                        var onSnd = sameChSnds[i];
                        if (onSnd.IsSoundingStarted && onSnd.NoteOnEvent.NoteNumber == newNote.NoteNumber)
                        {
                            onSnd.SoundOff();
                            offSnds.Add(onSnd);
                            onSnds.Remove(onSnd);
                            /*
                            AllSounds.Remove(onSnd);
                            onSounds.Remove(onSnd);

                            onSnds.Remove(onSnd);
                            sameChSnds.RemoveAt(i);
                            onSnd.Dispose();
                            i--;
                            */
                        }
                    }
                }

                //Search completely unused slot
                foreach (var onSnd in onSnds)
                    usedSlotTable[onSnd.ParentModule.UnitNumber].Add(onSnd.Slot, true);
                foreach (var offSnd in offSnds)
                    usedSlotTable[offSnd.ParentModule.UnitNumber].Add(offSnd.Slot, true);
                for (int i = offset; i < maxSlot; i++)
                {
                    foreach (var ist in usedSlotTable.Keys)
                    {
                        if (!usedSlotTable[ist].ContainsKey(i))
                            return ((I)instskey[ist], i);
                    }
                }

                List<T> tmpOnSnds = new List<T>(onSnds);

                //Process Poly mode
                List<byte> polyList = new List<byte>(parentModule.PolyMode);
                for (int i = tmpOnSnds.Count - 1; i >= 0; i--)
                {
                    var onSnd = tmpOnSnds[i];
                    if (polyList[onSnd.NoteOnEvent.Channel] > 0)
                    {
                        tmpOnSnds.RemoveAt(i);
                        polyList[onSnd.NoteOnEvent.Channel]--;
                    }
                }

                //Search last sound off sound
                offSnds.Sort(new Comparison<SoundBase>((x, y) =>
                {
                    if (x.SoundOffTime < y.SoundOffTime)
                        return -1;
                    else if (x.SoundOffTime > y.SoundOffTime)
                        return 1;
                    else
                        return 0;
                }));
                foreach (var snd in offSnds)
                {
                    if (offset <= snd.Slot && snd.Slot < maxSlot)
                    {
                        AllSounds.Remove(snd);
                        onSounds.Remove(snd);
                        snd.Dispose();
                        return ((I)snd.ParentModule, snd.Slot);
                    }
                }
                foreach (var snd in tmpOnSnds)
                {
                    if (offset <= snd.Slot && snd.Slot < maxSlot && snd.IsSoundOff)
                    {
                        AllSounds.Remove(snd);
                        onSounds.Remove(snd);
                        snd.Dispose();
                        return ((I)snd.ParentModule, snd.Slot);
                    }
                }

                //Search last key off sound
                foreach (var snd in tmpOnSnds)
                {
                    if (offset <= snd.Slot && snd.Slot < maxSlot && snd.IsKeyOff)
                    {
                        AllSounds.Remove(snd);
                        onSounds.Remove(snd);
                        snd.Dispose();
                        return ((I)snd.ParentModule, snd.Slot);
                    }
                }

                //Search last key on sound
                foreach (var snd in tmpOnSnds)
                {
                    if (offset <= snd.Slot && snd.Slot < maxSlot)
                    {
                        AllSounds.Remove(snd);
                        onSounds.Remove(snd);
                        snd.Dispose();
                        return ((I)snd.ParentModule, snd.Slot);
                    }
                }

                //Search last sounding sound
                foreach (var snd in onSnds)
                {
                    if (offset <= snd.Slot && snd.Slot < maxSlot)
                    {
                        AllSounds.Remove(snd);
                        onSounds.Remove(snd);
                        snd.Dispose();
                        return ((I)snd.ParentModule, snd.Slot);
                    }
                }
            }
            else
            {
                //既存の音を消す
                for (int i = 0; i < onSounds.Count; i++)
                {
                    var snd = onSounds[i];
                    if (!usedSlotTable.ContainsKey(snd.ParentModule.UnitNumber))
                        continue;

                    if (snd.Slot == slot)
                    {
                        AllSounds.Remove(snd);
                        onSounds.RemoveAt(i);
                        snd.Dispose();
                        break;
                    }
                }
                return (inst, slot);
            }

            return (inst, -1);
        }
    }
}
