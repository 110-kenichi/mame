﻿// copyright-holders:K.Ito
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zanac.MAmidiMEmo.Instruments
{
    //https://cdn.korg.com/jp/support/download/files/6e1e2515cd8d82ff484b6ec38bc91906.pdf?response-content-disposition=inline%3Bfilename*%3DUTF-8''MS2000_OM_J3.pdf&response-content-type=application%2Fpdf%3B
    //https://github.com/wesen/mididuino/blob/develop/hardware/libraries/MidiTools/Arpeggiator.cpp

    public class Arpeggiator
    {
        private static Random random = new Random();

        /// <summary>
        /// added note
        /// </summary>
        private List<NoteOnEvent> orderedNotes = new List<NoteOnEvent>();

        /// <summary>
        /// playing note
        /// </summary>
        private List<NoteOnEvent> arpNotes;

        private int arpOctaveCount;

        private int arpStep;

        private ArpStepStyle f_StepStyle;

        /// <summary>
        /// 
        /// </summary>
        public ArpStepStyle StepStyle
        {
            get
            {
                return f_StepStyle;
            }
            set
            {
                if (f_StepStyle != value)
                {
                    f_StepStyle = value;
                    calculateArp();
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public ArpType ArpType
        {
            get;
            set;
        }


        /// <summary>
        /// 
        /// </summary>
        public CustomArpStepType StaticArpStepType
        {
            get;
            set;
        }


        /// <summary>
        /// 
        /// </summary>
        public RetriggerType RetriggerType
        {
            get;
            set;
        }

        public int f_Range = 1;

        /// <summary>
        /// 
        /// </summary>
        public int Range
        {
            get
            {
                return f_Range;
            }
            set
            {
                if (f_Range != value && value >= 1 && value <= 4)
                {
                    f_Range = value;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public double GateCounter
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public double Counter
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public double StepCounter
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public int AddedNotesCount
        {
            get
            {
                return orderedNotes.Count;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public NoteOnEvent LastPassedNote
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public NoteOnEvent FirstAddedNote
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public SoundBase FirstSoundForPitch
        {
            get;
            internal set;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Arpeggiator()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public void ClearAddedNotes()
        {
            orderedNotes.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        public bool ResetNextNoteOn
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool SkipNextNote
        {
            get;
            set;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="noteOn"></param>
        public void AddNote(NoteOnEvent noteOn)
        {
            for (int i = 0; i < orderedNotes.Count; i++)
            {
                if (orderedNotes[i].NoteNumber == noteOn.NoteNumber)
                {
                    orderedNotes[i].Velocity = noteOn.Velocity;
                    return;
                }
            }

            if (orderedNotes.Count == 0)
            {
                FirstAddedNote = noteOn;
                LastPassedNote = null;
            }

            orderedNotes.Add(noteOn);

            calculateArp();

            if (RetriggerType == RetriggerType.Note)// || orderedNotes.Count == 1)
                Retrigger();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="noteOff"></param>
        public bool RemoveNote(NoteOffEvent noteOff)
        {
            bool result = false;
            for (int i = 0; i < orderedNotes.Count; i++)
            {
                if (orderedNotes[i].NoteNumber == noteOff.NoteNumber)
                {
                    orderedNotes.RemoveAt(i);
                    result = true;
                    break;
                }
            }
            if (result)
                calculateArp();
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public NoteOnEvent NextNote()
        {
            if (SkipNextNote)
            {
                arpStep++;
                SkipNextNote = false;
            }

            if (arpStep >= arpNotes.Count)
            {
                if (ArpType != ArpType.Static)
                {
                    //ステップをループする
                    arpStep = 0;
                    arpOctaveCount++;
                    if (arpOctaveCount >= Range)
                        arpOctaveCount = 0;
                }
                else
                {
                    //相対指定の場合は1つ目の音は2回目以降はならなさない
                    arpStep = StaticArpStepType == CustomArpStepType.Relative ? 1 : 0;
                }
            }

            //次のノートを取得
            NoteOnEvent an = null;
            if (StepStyle != ArpStepStyle.Random)
                an = arpNotes[arpStep];
            else
                an = arpNotes[random.Next(arpNotes.Count)];
            arpStep++;
            var nan = new NoteOnEvent(an.NoteNumber, an.Velocity) { Channel = an.Channel };

            //オクターブを上げる処理
            if (arpOctaveCount > 0)
            {
                int oc = arpOctaveCount;
                if (StepStyle == ArpStepStyle.Random)
                    oc = random.Next(Range - 1);
                oc *= 12;
                if (nan.NoteNumber + oc < 128)
                    nan.NoteNumber += (SevenBitNumber)oc;
            }

            //相対指定の場合のノート番号を動的に計算
            if (ArpType == ArpType.Static && StaticArpStepType == CustomArpStepType.Relative)
            {
                int no = nan.NoteNumber;
                if (LastPassedNote != null)
                {
                    no -= (int)FirstAddedNote.NoteNumber;
                    no = LastPassedNote.NoteNumber + no;
                }
                if (no < 0)
                    no = 0;
                else if (no > 127)
                    no = 127;
                nan.NoteNumber = (SevenBitNumber)no;
            }

            LastPassedNote = nan;
            return nan;
        }

        /// <summary>
        /// 最初のステップから再スタート
        /// </summary>
        public void Retrigger()
        {
            arpStep = 0;
            arpOctaveCount = 0;
            if (StepStyle == ArpStepStyle.RandomOnce)
                calculateArp();
        }

        private void calculateArp()
        {
            //arpStep = 0;
            bool sorted = true;
            switch (StepStyle)
            {
                case ArpStepStyle.Up:
                    sortUp();
                    arpNotes = new List<NoteOnEvent>(orderedNotes);
                    break;

                case ArpStepStyle.Down:
                    sortDown();
                    arpNotes = new List<NoteOnEvent>(orderedNotes);
                    break;

                case ArpStepStyle.Order:
                    arpNotes = new List<NoteOnEvent>(orderedNotes);
                    break;

                case ArpStepStyle.UpDown:
                    {
                        var num = orderedNotes.Count;
                        if (num > 1)
                        {
                            sortUp();
                            arpNotes = new List<NoteOnEvent>(orderedNotes);
                            for (int i = 0; i < num - 2; i++)
                                arpNotes.Add(orderedNotes[num - 2 - i]);
                        }
                        else
                        {
                            arpNotes = new List<NoteOnEvent>(orderedNotes);
                        }
                    }
                    break;

                case ArpStepStyle.DownUp:
                    {
                        var num = orderedNotes.Count;
                        if (num > 1)
                        {
                            sortDown();
                            arpNotes = new List<NoteOnEvent>(orderedNotes);
                            for (int i = 0; i < num - 2; i++)
                                arpNotes.Add(orderedNotes[num - 2 - i]);
                        }
                        else
                        {
                            arpNotes = new List<NoteOnEvent>(orderedNotes);
                        }
                    }
                    break;

                case ArpStepStyle.UpAndDown:
                    {
                        var num = orderedNotes.Count;
                        if (num > 1)
                        {
                            sortUp();
                            arpNotes = new List<NoteOnEvent>(orderedNotes);
                            for (int i = 0; i < num; i++)
                                arpNotes.Add(orderedNotes[num - 1 - i]);
                            arpNotes = new List<NoteOnEvent>(orderedNotes);
                        }
                        else
                        {
                            arpNotes = new List<NoteOnEvent>(orderedNotes);
                        }
                    }
                    break;

                case ArpStepStyle.DownAndUp:
                    {
                        var num = orderedNotes.Count;
                        if (num > 1)
                        {
                            sortDown();
                            arpNotes = new List<NoteOnEvent>(orderedNotes);
                            for (int i = 0; i < num; i++)
                                arpNotes.Add(orderedNotes[num - 1 - i]);
                        }
                        else
                        {
                            arpNotes = new List<NoteOnEvent>(orderedNotes);
                        }
                    }
                    break;

                case ArpStepStyle.Converge:
                    {
                        var num = orderedNotes.Count;
                        sortUp();
                        if (num > 1)
                        {
                            arpNotes.Clear();
                            for (int i = 0; i < (num >> 1); i++)
                            {
                                arpNotes.Add(orderedNotes[i]);
                                arpNotes.Add(orderedNotes[num - i - 1]);
                            }
                            if ((byte)(num & 1) == 1)
                            {
                                arpNotes.Add(orderedNotes[(num >> 1)]);
                            }
                        }
                        else
                        {
                            arpNotes = new List<NoteOnEvent>(orderedNotes);
                        }
                    }
                    break;

                case ArpStepStyle.Diverge:
                    {
                        var num = orderedNotes.Count;
                        sortUp();
                        if (num > 1)
                        {
                            if ((byte)(num & 1) == 1)
                            {
                                arpNotes.Add(orderedNotes[num >> 1]);
                            }
                            for (int i = (num >> 1) - 1; i >= 0; i--)
                            {
                                arpNotes.Add(orderedNotes[i]);
                                arpNotes.Add(orderedNotes[num - i - 1]);
                            }
                        }
                        else
                        {
                            arpNotes = new List<NoteOnEvent>(orderedNotes);
                        }
                    }
                    break;

                case ArpStepStyle.ConAndDiverge:
                    {
                        var num = orderedNotes.Count;
                        sortUp();
                        if (num > 1)
                        {
                            for (int i = 0; i < (num >> 1); i++)
                            {
                                arpNotes.Add(orderedNotes[i]);
                                arpNotes.Add(orderedNotes[num - i - 1]);
                            }
                            if ((byte)(num & 1) == 1)
                            {
                                arpNotes.Add(orderedNotes[(num >> 1)]);
                            }
                            for (int i = (num >> 1) - 1; i >= 0; i--)
                            {
                                arpNotes.Add(orderedNotes[i]);
                                arpNotes.Add(orderedNotes[num - i - 1]);
                            }
                        }
                        else
                        {
                            arpNotes = new List<NoteOnEvent>(orderedNotes);
                        }
                    }
                    break;

                case ArpStepStyle.PinkyUp:
                    {
                        var num = orderedNotes.Count;
                        sortUp();
                        if (num > 1)
                        {
                            for (int i = 0; i < num - 1; i++)
                            {
                                arpNotes.Add(orderedNotes[num - 1]);
                                arpNotes.Add(orderedNotes[i]);
                            }
                        }
                        else
                        {
                            arpNotes = new List<NoteOnEvent>(orderedNotes);
                        }
                    }
                    break;

                case ArpStepStyle.PinkyUpDown:
                    {
                        var num = orderedNotes.Count;
                        sortUp();
                        if (num > 1)
                        {
                            for (int i = 0; i < num - 1; i++)
                            {
                                arpNotes.Add(orderedNotes[num - 1]);
                                arpNotes.Add(orderedNotes[i]);
                            }
                            for (int i = 0; i < num - 1; i++)
                            {
                                arpNotes.Add(orderedNotes[num - 1]);
                                arpNotes.Add(orderedNotes[num - i - 2]);
                            }
                        }
                        else
                        {
                            arpNotes = new List<NoteOnEvent>(orderedNotes);
                        }
                    }
                    break;

                case ArpStepStyle.ThumbUp:
                    {
                        var num = orderedNotes.Count;
                        sortUp();
                        if (num > 1)
                        {
                            for (int i = 1; i < num; i++)
                            {
                                arpNotes.Add(orderedNotes[0]);
                                arpNotes.Add(orderedNotes[i]);
                            }
                        }
                        else
                        {
                            arpNotes = new List<NoteOnEvent>(orderedNotes);
                        }
                    }
                    break;

                case ArpStepStyle.ThumbUpDown:
                    {
                        var num = orderedNotes.Count;
                        sortUp();
                        if (num > 1)
                        {
                            for (int i = 1; i < num; i++)
                            {
                                arpNotes.Add(orderedNotes[0]);
                                arpNotes.Add(orderedNotes[i]);
                            }
                            for (int i = 0; i < num - 1; i++)
                            {
                                arpNotes.Add(orderedNotes[0]);
                                arpNotes.Add(orderedNotes[num - i - 1]);
                            }
                        }
                        else
                        {
                            arpNotes = new List<NoteOnEvent>(orderedNotes);
                        }
                    }
                    break;

                case ArpStepStyle.Random:
                    arpNotes = new List<NoteOnEvent>(orderedNotes);
                    sorted = false;
                    break;

                case ArpStepStyle.RandomOnce:
                    {
                        var num = orderedNotes.Count;
                        arpNotes = new List<NoteOnEvent>(orderedNotes);
                        for (int i = 0; i < num; i++)
                        {
                            byte rand = (byte)(random.Next() % num);
                            var tmp = arpNotes[i];
                            arpNotes[i] = arpNotes[rand];
                            arpNotes[rand] = tmp;
                        }
                    }
                    break;

                default:
                    sorted = false;
                    break;
            }

            //ソート後、同じ音が続けてならないように次の音を別の音にする
            if (sorted)
            {
                if (LastPassedNote != null)
                {
                    for (int i = 0; i < arpNotes.Count; i++)
                    {
                        if (arpNotes[i].NoteNumber == LastPassedNote.NoteNumber)
                        {
                            arpStep = i + 1;
                            break;
                        }
                    }
                }
                else if (FirstSoundForPitch != null)
                {
                    for (int i = 0; i < arpNotes.Count; i++)
                    {
                        if (arpNotes[i].NoteNumber == FirstSoundForPitch.NoteOnEvent.NoteNumber)
                        {
                            arpStep = i + 1;
                            break;
                        }
                    }
                }
            }
        }

        private void sortUp()
        {
            orderedNotes.Sort((a, b) => a.NoteNumber - b.NoteNumber);
        }

        private void sortDown()
        {
            orderedNotes.Sort((a, b) => b.NoteNumber - a.NoteNumber);
        }


    }


    public enum RetriggerType
    {
        Off = 0,
        Note,
        Beat,
        //Cnt
    }

    public enum ArpStepStyle
    {
        Up = 0,
        Down,
        UpDown,
        DownUp,
        UpAndDown,
        DownAndUp,
        Converge,
        Diverge,
        ConAndDiverge,
        PinkyUp,
        PinkyUpDown,
        ThumbUp,
        ThumbUpDown,
        Random,
        RandomOnce,
        Order,
        //Cnt
    }

    public enum ArpType
    {
        Dynamic,
        Static
    }

    public enum ArpMethod
    {
        KeyOn,
        PitchChange
    }

    public enum ArpResolution
    {
        QuarterNote = 1,
        QuarterTriplet = 3,
        EighthNote = 2,
        EighthTriplet = 6,
        SixteenthNote = 4,
        SixteenthTriplet = 12,
        ThirtySecondNote = 8,
    }


    public enum CustomArpStepType
    {
        Absolute,
        Relative,
        Fixed,
    }


}
