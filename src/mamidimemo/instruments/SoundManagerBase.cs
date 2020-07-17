// copyright-holders:K.Ito
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
        public virtual void PitchBend(PitchBendEvent midiEvent)
        {
            foreach (var t in AllSounds)
            {
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
        public virtual void ControlChange(ControlChangeEvent midiEvent)
        {
            switch (midiEvent.ControlNumber)
            {
                case 1:    //Modulation
                    foreach (var t in AllSounds)
                    {
                        if (t.ParentModule.UnitNumber != parentModule.UnitNumber)
                            continue;

                        if (t.NoteOnEvent.Channel == midiEvent.Channel)
                            t.ModulationEnabled = midiEvent.ControlValue != 0;
                    }
                    break;
                case 6:    //Data Entry
                           //nothing
                    break;
                case 7:    //Volume
                    foreach (var t in AllSounds)
                    {
                        if (t.ParentModule.UnitNumber != parentModule.UnitNumber)
                            continue;

                        if (t.NoteOnEvent.Channel == midiEvent.Channel)
                            t.OnVolumeUpdated();
                    }
                    break;
                case 10:    //Panpot
                    foreach (var t in AllSounds)
                    {
                        if (t.ParentModule.UnitNumber != parentModule.UnitNumber)
                            continue;

                        if (t.NoteOnEvent.Channel == midiEvent.Channel)
                            t.OnPanpotUpdated();
                    }
                    break;
                case 11:    //Expression
                    foreach (var t in AllSounds)
                    {
                        if (t.ParentModule.UnitNumber != parentModule.UnitNumber)
                            continue;

                        if (t.NoteOnEvent.Channel == midiEvent.Channel)
                            t.OnVolumeUpdated();
                    }
                    break;

                //GPCS
                case 16:
                case 17:
                case 18:
                case 19:
                    {
                        int no = midiEvent.ControlNumber - 16 + 1;
                        ProcessGPCS(midiEvent, no);
                    }
                    break;
                //GPCS
                case 80:
                case 81:
                case 82:
                case 83:
                    {
                        int no = midiEvent.ControlNumber - 80 + 1;
                        ProcessGPCS(midiEvent, no);
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
                    processSCCS(midiEvent);
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
                        foreach (var snd in AllSounds)
                        {
                            if (snd.ParentModule.UnitNumber != parentModule.UnitNumber)
                                continue;

                            KeyOff(new NoteOffEvent(snd.NoteOnEvent.NoteNumber, (SevenBitNumber)0) { Channel = snd.NoteOnEvent.Channel });
                        }
                        break;
                    }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        abstract internal void AllSoundOff();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        private void processSCCS(ControlChangeEvent midiEvent)
        {
            var tim = parentModule.GetFinalTimbre(midiEvent.Channel);
            bool process = false;
            foreach (var ipi in tim.SCCS.GetPropertyInfo(tim, midiEvent.ControlNumber - 70 + 1))
            {
                if (ipi != null)
                {
                    var pi = ipi.Property;

                    SlideParametersAttribute attribute =
                        Attribute.GetCustomAttribute(pi, typeof(SlideParametersAttribute)) as SlideParametersAttribute;
                    if (attribute != null)
                    {
                        int len = (attribute.SliderMax - attribute.SliderMin) + 1;
                        int val = len * midiEvent.ControlValue / 128;

                        var pd = TypeDescriptor.GetProperties(pi.DeclaringType)[pi.Name];
                        pd.SetValue(ipi.Owner, pd.Converter.ConvertFromString(val.ToString()));
                        process = true;
                    }
                    else
                    {
                        DoubleSlideParametersAttribute dattribute =
                            Attribute.GetCustomAttribute(pi, typeof(DoubleSlideParametersAttribute)) as DoubleSlideParametersAttribute;
                        if (dattribute != null)
                        {
                            double len = dattribute.SliderMax - dattribute.SliderMin;
                            double val = len * (double)midiEvent.ControlValue / (double)128;

                            var pd = TypeDescriptor.GetProperties(pi.DeclaringType)[pi.Name];
                            pd.SetValue(ipi.Owner, pd.Converter.ConvertFromString(val.ToString()));
                            process = true;
                        }
                        else
                        {
                            if (ipi.Property.PropertyType == typeof(bool))
                            {
                                var pd = TypeDescriptor.GetProperties(pi.DeclaringType)[pi.Name];
                                pd.SetValue(ipi.Owner, midiEvent.ControlValue > 63);
                                process = true;
                            }
                            else if (ipi.Property.PropertyType.IsEnum)
                            {
                                var vals = Enum.GetValues(ipi.Property.PropertyType);
                                int val = vals.Length * midiEvent.ControlValue / 128;

                                var pd = TypeDescriptor.GetProperties(pi.DeclaringType)[pi.Name];
                                pd.SetValue(ipi.Owner, vals.GetValue(val));
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

                    if (t.NoteOnEvent.Channel == midiEvent.Channel && t.Timbre == tim)
                        t.OnSoundParamsUpdated();
                }
            }
        }

        private void ProcessGPCS(ControlChangeEvent midiEvent, int no)
        {
            bool process = false;
            foreach (var ipi in parentModule.GPCS[midiEvent.Channel].GetPropertyInfo(parentModule, no))
            {
                if (ipi != null)
                {
                    var pi = ipi.Property;

                    SlideParametersAttribute attribute =
                        Attribute.GetCustomAttribute(pi, typeof(SlideParametersAttribute)) as SlideParametersAttribute;
                    if (attribute != null)
                    {
                        int len = (attribute.SliderMax - attribute.SliderMin) + 1;
                        int val = len * midiEvent.ControlValue / 128;

                        var pd = TypeDescriptor.GetProperties(pi.DeclaringType)[pi.Name];
                        pd.SetValue(ipi.Owner, pd.Converter.ConvertFromString(val.ToString()));
                        process = true;
                    }
                    else
                    {
                        DoubleSlideParametersAttribute dattribute =
                            Attribute.GetCustomAttribute(pi, typeof(DoubleSlideParametersAttribute)) as DoubleSlideParametersAttribute;
                        if (dattribute != null)
                        {
                            double len = dattribute.SliderMax - dattribute.SliderMin;
                            double val = len * (double)midiEvent.ControlValue / (double)128;

                            var pd = TypeDescriptor.GetProperties(pi.DeclaringType)[pi.Name];
                            pd.SetValue(ipi.Owner, pd.Converter.ConvertFromString(val.ToString()));
                            process = true;
                        }
                        else
                        {
                            if (ipi.Property.PropertyType == typeof(bool))
                            {
                                var pd = TypeDescriptor.GetProperties(pi.DeclaringType)[pi.Name];
                                pd.SetValue(ipi.Owner, midiEvent.ControlValue > 63);
                                process = true;
                            }
                            else if (ipi.Property.PropertyType.IsEnum)
                            {
                                var vals = Enum.GetValues(ipi.Property.PropertyType);
                                int val = vals.Length * midiEvent.ControlValue / 128;

                                var pd = TypeDescriptor.GetProperties(pi.DeclaringType)[pi.Name];
                                pd.SetValue(ipi.Owner, vals.GetValue(val));
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

                    if (t.NoteOnEvent.Channel == midiEvent.Channel)
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
            var timbre = parentModule.GetFinalTimbre(ch);
            var sds = timbre.SDS.ARP;

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
            if (arp.StepCounter == arp.StepNum)
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
            var timbre = parentModule.GetFinalTimbre(ch);
            var sds = timbre.SDS.ARP;
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
            if (arp.StepCounter == arp.StepNum)
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
        public virtual void KeyOn(NoteOnEvent note)
        {
            if (preProcessArrpegioForKeyOn(note))
                return;

            var snd = keyOnCore(note);

            postProcessArrpegioForKeyOn(note, snd);
        }

        private SoundBase[] keyOnCore(NoteOnEvent note)
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
        public virtual SoundBase[] SoundOn(NoteOnEvent note)
        {
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="note"></param>
        public virtual void KeyOff(NoteOffEvent note)
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

        private void keyOffCore(NoteOnEvent onote)
        {
            if (onote != null)
                keyOffCore(new NoteOffEvent(onote.NoteNumber, (SevenBitNumber)0) { Channel = onote.Channel });
        }

        private void keyOffCore(NoteOffEvent note)
        {
            for (int i = 0; i < AllSounds.Count; i++)
            {
                SoundBase offsnd = AllSounds[i];
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
        private bool preProcessArrpegioForKeyOn(NoteOnEvent note)
        {
            FourBitNumber ch = note.Channel;
            ArpSettings sds = parentModule.GetFinalTimbre(ch).SDS.ARP;
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


        private void postProcessArrpegioForKeyOn(NoteOnEvent note, SoundBase[] snd)
        {
            FourBitNumber ch = note.Channel;
            ArpSettings sds = parentModule.GetFinalTimbre(ch).SDS.ARP;
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
                var timbre = parentModule.GetFinalTimbre(ch);
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
                var timbre = parentModule.GetFinalTimbre(ch);
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
                arp.Step = steps / 2;
                arp.StepNum = 2;
            }
            else if (arp.StepNum < 50)
            {
                arp.Step = steps / 5;
                arp.StepNum = 5;
            }
            else
            {
                arp.Step = steps / 10;
                arp.StepNum = 10;
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
        protected int SearchEmptySlotAndOff<T>(List<T> onSounds, NoteOnEvent newNote, int maxSlot) where T : SoundBase
        {
            return SearchEmptySlotAndOff(onSounds, newNote, maxSlot, -1);
        }

        /// <summary>
        /// 未使用のスロットを検索する
        /// 空が無い場合は最初に鳴った音を消す
        /// </summary>
        /// <param name="onSounds"></param>
        /// <param name="maxSlot"></param>
        /// <param name="slot">強制的に割り当てるスロット。-1なら強制しない</param>
        /// <returns></returns>
        protected int SearchEmptySlotAndOff<T>(List<T> onSounds, NoteOnEvent newNote, int maxSlot, int slot) where T : SoundBase
        {
            if (slot < 0)
            {
                Dictionary<int, bool> usedTable = new Dictionary<int, bool>();
                for (int i = 0; i < onSounds.Count; i++)
                    usedTable.Add(onSounds[i].Slot, true);

                //使っていないスロットがあればそれを返す
                for (int i = 0; i < maxSlot; i++)
                {
                    if (!usedTable.ContainsKey(i))
                        return i;
                }

                //一番古いキーオフされたスロットを探す
                for (int i = 0; i < onSounds.Count; i++)
                {
                    var snd = onSounds[i];
                    if (snd.Slot < maxSlot && snd.IsKeyOff)
                    {
                        AllSounds.Remove(snd);
                        onSounds.RemoveAt(i);
                        snd.Dispose();
                        return snd.Slot;
                    }
                }
                //一番古いキーオンされたスロットを探す
                for (int i = 0; i < onSounds.Count; i++)
                {
                    var snd = onSounds[i];
                    if (snd.Slot < maxSlot)
                    {
                        AllSounds.Remove(snd);
                        onSounds.RemoveAt(i);
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
        protected (I, int) SearchEmptySlotAndOffForLeader<I, T>(I inst, List<T> onSounds, NoteOnEvent newNote, int maxSlot) where T : SoundBase where I : InstrumentBase
        {
            return SearchEmptySlotAndOffForLeader(inst, onSounds, newNote, maxSlot, -1);
        }

        /// <summary>
        /// 未使用のスロットを検索する
        /// 空が無い場合は最初に鳴った音を消す
        /// </summary>
        /// <param name="onSounds"></param>
        /// <param name="maxSlot"></param>
        /// <param name="slot">強制的に割り当てるスロット。-1なら強制しない</param>
        /// <returns></returns>
        protected (I, int) SearchEmptySlotAndOffForLeader<I, T>(I inst, List<T> onSounds, NoteOnEvent newNote, int maxSlot, int slot) where T : SoundBase where I : InstrumentBase
        {
            //gather leader and followers
            Dictionary<uint, InstrumentBase> instskey = new Dictionary<uint, InstrumentBase>();
            foreach (var i in InstrumentManager.GetInstruments(inst.DeviceID))
                instskey.Add(i.UnitNumber, i);

            Dictionary<uint, Dictionary<int, bool>> insts = new Dictionary<uint, Dictionary<int, bool>>();
            foreach (var i in InstrumentManager.GetInstruments(inst.DeviceID))
            {
                if (i.UnitNumber == inst.UnitNumber)
                    insts.Add(i.UnitNumber, new Dictionary<int, bool>());
                else if (inst.UnitNumber == (int)i.FollowerMode - 1)
                    insts.Add(i.UnitNumber, new Dictionary<int, bool>());
            }


            if (slot < 0)
            {
                //search empty slot
                foreach (var onSnd in onSounds)
                {
                    if (insts.ContainsKey(onSnd.ParentModule.UnitNumber))
                        insts[onSnd.ParentModule.UnitNumber].Add(onSnd.Slot, true);
                }

                //使っていないスロットがあればそれを返す
                for (int i = 0; i < maxSlot; i++)
                {
                    foreach (var ist in insts.Keys)
                    {
                        if (!insts[ist].ContainsKey(i))
                            return ((I)instskey[ist], i);
                    }
                }

                //一番古いキーオフされたスロットを探す
                for (int i = 0; i < onSounds.Count; i++)
                {
                    var snd = onSounds[i];
                    if (!insts.ContainsKey(snd.ParentModule.UnitNumber))
                        continue;

                    if (snd.Slot < maxSlot && snd.IsKeyOff)
                    {
                        AllSounds.Remove(snd);
                        onSounds.RemoveAt(i);
                        snd.Dispose();
                        return ((I)snd.ParentModule, snd.Slot);
                    }
                }

                //一番古いキーオンされたスロットを探す
                for (int i = 0; i < onSounds.Count; i++)
                {
                    var snd = onSounds[i];
                    if (!insts.ContainsKey(snd.ParentModule.UnitNumber))
                        continue;

                    if (snd.Slot < maxSlot)
                    {
                        AllSounds.Remove(snd);
                        onSounds.RemoveAt(i);
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
                    if (!insts.ContainsKey(snd.ParentModule.UnitNumber))
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
