using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using zanac.MAmidiMEmo.ComponentModel;

namespace zanac.MAmidiMEmo.Instruments.Envelopes
{

    /// <summary>
    /// 
    /// </summary>
    public class BasicFxEngine : AbstractFxEngine
    {
        private BasicFxSettings settings;

        public override AbstractFxSettingsBase Settings
        {
            get
            {
                return settings;
            }
        }

        private double f_OutputLevel;

        /// <summary>
        /// 
        /// </summary>
        public override double OutputLevel
        {
            get => f_OutputLevel;
        }


        private int f_PanShift;

        /// <summary>
        /// -127:Left
        /// 127:Right
        /// </summary>
        public override int PanShift
        {
            get => f_PanShift;
        }

        private int lastArpNoteNumber;

        private double f_DeltaNoteNumber;

        private double lastPitchValue;

        /// <summary>
        /// 
        /// </summary>
        public override double DeltaNoteNumber
        {
            get => f_DeltaNoteNumber;
        }

        private bool f_Active;

        /// <summary>
        /// エフェクトが動作しているかどうか falseなら終了
        /// </summary>
        public override bool Active
        {
            get
            {
                return f_Active;
            }
            protected set
            {
                if (f_Active != value)
                    f_Active = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        public BasicFxEngine(BasicFxSettings settings)
        {
            this.settings = settings;

            f_OutputLevel = 1d;
            f_DeltaNoteNumber = 0d;
        }

        private uint volumeCounter;

        private uint pitchCounter;

        private uint panCounter;

        private uint arpCounter;

        /// <summary>
        /// 
        /// </summary>
        protected virtual bool ProcessCore(SoundBase sound, bool isKeyOff, bool isSoundOff)
        {
            bool process = false;
            //volume
            if (settings.VolumeEnvelopesNums.Length > 0)
            {
                if (!isKeyOff)
                {
                    var vm = settings.VolumeEnvelopesNums.Length;
                    if (settings.VolumeEnvelopesReleasePoint >= 0)
                        vm = settings.VolumeEnvelopesReleasePoint;
                    if (volumeCounter >= vm)
                    {
                        if (settings.VolumeEnvelopesRepeatPoint >= 0)
                            volumeCounter = (uint)settings.VolumeEnvelopesRepeatPoint;
                        else
                            volumeCounter = (uint)vm - 1;
                    }
                }
                else
                {
                    //if (settings.VolumeEnvelopesReleasePoint < 0)
                    //    volumeCounter = (uint)settings.VolumeEnvelopesNums.Length;

                    if (volumeCounter < settings.VolumeEnvelopesNums.Length)
                    {
                        if (settings.VolumeEnvelopesReleasePoint >= 0 && volumeCounter < (uint)settings.VolumeEnvelopesReleasePoint)
                            volumeCounter = (uint)settings.VolumeEnvelopesReleasePoint;
                        else
                            volumeCounter = (uint)settings.VolumeEnvelopesNums.Length;
                    }
                }

                if (volumeCounter < settings.VolumeEnvelopesNums.Length)
                {
                    int vol = settings.VolumeEnvelopesNums[volumeCounter++];

                    f_OutputLevel = vol / 127d;
                    process = true;
                }
            }

            //pitch
            if (settings.PitchEnvelopesNums.Length > 0)
            {
                if (!isKeyOff)
                {
                    var vm = settings.PitchEnvelopesNums.Length;
                    if (settings.PitchEnvelopesReleasePoint >= 0)
                        vm = settings.PitchEnvelopesReleasePoint;
                    if (pitchCounter >= vm)
                    {
                        if (settings.PitchEnvelopesRepeatPoint >= 0)
                            pitchCounter = (uint)settings.PitchEnvelopesRepeatPoint;
                        else
                            pitchCounter = (uint)vm;
                    }
                }
                else
                {
                    //if (settings.PitchEnvelopesReleasePoint < 0)
                    //    pitchCounter = (uint)settings.PitchEnvelopesNums.Length;

                    if (pitchCounter < settings.PitchEnvelopesNums.Length)
                    {
                        if (settings.PitchEnvelopesReleasePoint >= 0 && pitchCounter < (uint)settings.PitchEnvelopesReleasePoint)
                            pitchCounter = (uint)settings.PitchEnvelopesReleasePoint;
                        else
                            pitchCounter = (uint)settings.PitchEnvelopesNums.Length;
                    }
                }
                if (pitchCounter < settings.PitchEnvelopesNums.Length)
                {
                    double pitch = settings.PitchEnvelopesNums[pitchCounter++];
                    double range = settings.PitchEnvelopeRange;

                    switch (settings.PitchStepType)
                    {
                        case PitchStepType.Absolute:
                            f_DeltaNoteNumber += ((double)(pitch - lastPitchValue) / 8192d) * range;
                            break;
                        case PitchStepType.Relative:
                            f_DeltaNoteNumber += ((double)pitch / 8192d) * range;
                            break;
                    }

                    lastPitchValue = pitch;
                    process = true;
                }
            }

            //pan
            if (settings.PanShiftEnvelopesNums.Length > 0)
            {
                if (!isKeyOff)
                {
                    var vm = settings.PanShiftEnvelopesNums.Length;
                    if (settings.PanShiftEnvelopesReleasePoint >= 0)
                        vm = settings.PanShiftEnvelopesReleasePoint;
                    if (panCounter >= vm)
                    {
                        if (settings.PanShiftEnvelopesRepeatPoint >= 0)
                            panCounter = (uint)settings.PanShiftEnvelopesRepeatPoint;
                        else
                            panCounter = (uint)vm - 1;
                    }
                }
                else
                {
                    //if (settings.PanShiftEnvelopesReleasePoint < 0)
                    //    panCounter = (uint)settings.PanShiftEnvelopesNums.Length;

                    if (panCounter < settings.PanShiftEnvelopesNums.Length)
                    {
                        if (settings.PanShiftEnvelopesReleasePoint >= 0 && panCounter < (uint)settings.PanShiftEnvelopesReleasePoint)
                            panCounter = (uint)settings.PanShiftEnvelopesReleasePoint;
                        else
                            panCounter = (uint)settings.PanShiftEnvelopesNums.Length;
                    }
                }

                if (panCounter < settings.PanShiftEnvelopesNums.Length)
                {
                    int pan = settings.PanShiftEnvelopesNums[panCounter++];

                    f_PanShift = pan;
                    process = true;
                }
            }

            //arpeggio
            if (settings.ArpEnvelopesNums.Length > 0)
            {
                if (!isKeyOff)
                {
                    var vm = settings.ArpEnvelopesNums.Length;
                    if (settings.ArpEnvelopesReleasePoint >= 0)
                        vm = settings.ArpEnvelopesReleasePoint;
                    if (arpCounter >= vm)
                    {
                        if (settings.ArpEnvelopesRepeatPoint >= 0)
                            arpCounter = (uint)settings.ArpEnvelopesRepeatPoint;
                        else
                            arpCounter = (uint)vm;
                    }
                }
                else
                {
                    //if (settings.ArpEnvelopesReleasePoint < 0)
                    //    arpCounter = (uint)settings.ArpEnvelopesNums.Length;

                    if (arpCounter < settings.ArpEnvelopesNums.Length)
                    {
                        if (settings.ArpEnvelopesReleasePoint >= 0 && arpCounter < (uint)settings.ArpEnvelopesReleasePoint)
                            arpCounter = (uint)settings.ArpEnvelopesReleasePoint;
                        else
                            arpCounter = (uint)settings.ArpEnvelopesNums.Length;
                    }
                }
                if (arpCounter < settings.ArpEnvelopesNums.Length)
                {
                    int? dnote = settings.ArpEnvelopesNums[arpCounter++];
                    if (dnote.HasValue)
                    {
                        switch (settings.ArpStepType)
                        {
                            case ArpStepType.Absolute:
                                f_DeltaNoteNumber += -lastArpNoteNumber + dnote.Value;
                                break;
                            case ArpStepType.Relative:
                                f_DeltaNoteNumber += dnote.Value;
                                break;
                            case ArpStepType.Fixed:
                                f_DeltaNoteNumber = -sound.NoteOnEvent.NoteNumber + dnote.Value;
                                break;
                        }
                        lastArpNoteNumber = dnote.Value;

                        if (settings.ArpMethod == ArpMethod.KeyOn)
                        {
                            sound.SoundOff();
                            sound.KeyOn();
                        }

                        process = true;
                    }
                }
            }

            return process;
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool Process(SoundBase sound, bool isKeyOff, bool isSoundOff)
        {
            Active = true;

            if (!settings.Enable || isSoundOff)
            {
                Active = false;
                return false;
            }

            Active = ProcessCore(sound, isKeyOff, isSoundOff);
            return Active;
        }
    }
}
