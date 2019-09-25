﻿// copyright-holders:K.Ito
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using zanac.MAmidiMEmo.ComponentModel;

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
        protected SoundList<SoundBase> AllSounds
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
                if (t.NoteOnEvent.Channel == midiEvent.Channel)
                    t.UpdatePitch();
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
                        if (t.NoteOnEvent.Channel == midiEvent.Channel)
                            t.UpdateVolume();
                    }
                    break;
                case 10:    //Panpot
                    foreach (var t in AllSounds)
                    {
                        if (t.NoteOnEvent.Channel == midiEvent.Channel)
                            t.UpdatePanpot();
                    }
                    break;
                case 11:    //Expression
                    foreach (var t in AllSounds)
                    {
                        if (t.NoteOnEvent.Channel == midiEvent.Channel)
                            t.UpdateVolume();
                    }
                    break;
                case 120:   //All Sounds Off
                case 123:   //All Note Off
                    {
                        //clear all arps
                        stopAllArpForKeyOn();
                        stopAllArpForPitch();
                        foreach (var snd in AllSounds)
                            KeyOff(new NoteOffEvent(snd.NoteOnEvent.NoteNumber, (SevenBitNumber)0) { Channel = snd.NoteOnEvent.Channel });
                        break;
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
            var timbre = parentModule.GetTimbre(ch);
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

            if (arp.GateCounter != -1)
            {
                arp.GateCounter += 1;
                if (arp.GateCounter >= (arp.StepNum * (sds.GateTime + 1)) / 128d)
                {
                    //Gate Time
                    arp.GateCounter = -1;
                    keyOffCore(arp.LastPassedNote);
                }
            }

            arp.StepCounter += 1;
            if (arp.StepCounter == arp.StepNum)
            {
                // on sound
                arp.StepCounter = 0;
                arp.GateCounter = 0;
                keyOnCore(arp.NextNote());
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
            var timbre = parentModule.GetTimbre(ch);
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
            var snd = arp.FirstSoundForPitch;
            if (arp.GateCounter != -1)
            {
                arp.GateCounter += 1;
                if (arp.GateCounter >= (arp.StepNum * (sds.GateTime + 1)) / 128d)
                {
                    arp.GateCounter = -1;
                    if (sds.GateTime != 127)
                        snd.ArpeggiateLevel = 0d;
                }
            }
            arp.StepCounter += 1;
            if (arp.StepCounter == arp.StepNum)
            {
                // on sound
                arp.StepCounter = 0;
                arp.GateCounter = 0;
                var note = arp.NextNote();
                snd.ArpeggiateDeltaNoteNumber = note.NoteNumber - arp.FirstAddedNote.NoteNumber;
                snd.ArpeggiateLevel = 1d;
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

        private SoundBase keyOnCore(NoteOnEvent note)
        {
            var snd = SoundOn(note);
            if (snd != null)
            {
                AllSounds.Add(snd);
                LastNoteNumbers[note.Channel] = note.NoteNumber;
                return snd;
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="note"></param>
        public virtual SoundBase SoundOn(NoteOnEvent note)
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

        private void keyOffCore(NoteOnEvent onote)
        {
            if (onote != null)
                keyOffCore(new NoteOffEvent(onote.NoteNumber, (SevenBitNumber)0) { Channel = onote.Channel });
        }

        private SoundBase keyOffCore(NoteOffEvent note)
        {
            SoundBase offsnd = null;

            for (int i = 0; i < AllSounds.Count; i++)
            {
                if (AllSounds[i].IsKeyOff)
                    continue;

                if (AllSounds[i].NoteOnEvent.Channel == note.Channel)
                {
                    if (AllSounds[i].NoteOnEvent.NoteNumber == note.NoteNumber)
                    {
                        offsnd = AllSounds[i];
                        offsnd.KeyOff();
                        break;
                    }
                }
            }

            return offsnd;
        }

        #region アルペジオ関係


        private bool preProcessArrpegioForKeyOn(NoteOnEvent note)
        {
            FourBitNumber ch = note.Channel;
            ArpSettings sds = parentModule.GetTimbre(ch).SDS.ARP;
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
                                keyOffCore(arp.LastPassedNote);
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


        private void postProcessArrpegioForKeyOn(NoteOnEvent note, SoundBase snd)
        {
            FourBitNumber ch = note.Channel;
            ArpSettings sds = parentModule.GetTimbre(ch).SDS.ARP;
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
                var timbre = parentModule.GetTimbre(ch);
                var sds = timbre.SDS.ARP;
                ArpEngine arp = ArpeggiatorsForKeyOn[ch];

                if (sds.Enable)
                {
                    //ignore keyoff if hold mode
                    if (sds.Hold)
                    {
                        arp.ResetNextNoteOn = true;
                        return true;
                    }
                }
                if (arp.RemoveNote(note))
                {
                    if (arp.LastPassedNote != null)
                        keyOffCore(arp.LastPassedNote);

                    //全アルペジオ終了
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
                var timbre = parentModule.GetTimbre(ch);
                var sds = timbre.SDS.ARP;
                ArpEngine arp = ArpeggiatorsForPitch[ch];

                if (sds.Enable)
                {
                    //ignore keyoff if hold mode
                    if (sds.Hold)
                    {
                        arp.ResetNextNoteOn = true;
                        return true;
                    }
                }
                if (arp.RemoveNote(note) && arp.AddedNotesCount == 0)
                {
                    if (arp.ArpAction != null)
                        arp.ArpAction = null;
                    keyOffCore(arp.FirstAddedNote);
                    ArpeggiatorsForPitch.Remove(ch);
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
            keyOffCore(arp.LastPassedNote);
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
        protected virtual int SearchEmptySlotAndOff<T>(List<T> onSounds, NoteOnEvent newNote, int maxSlot) where T : SoundBase
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
        protected virtual int SearchEmptySlotAndOff<T>(List<T> onSounds, NoteOnEvent newNote, int maxSlot, int slot) where T : SoundBase
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

    }
}
