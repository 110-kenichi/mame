﻿// copyright-holders:K.Ito
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public abstract class SoundBase : IDisposable
    {
        public InstrumentBase ParentModule
        {
            get;
            private set;
        }

        public SoundManagerBase ParentManager
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public TimbreBase Timbre
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public int BaseTimbreIndex
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public DrumTimbre DrumTimbre
        {
            get;
            private set;
        }

        public bool IsDisposed
        {
            get;
            private set;
        }

        public bool IsKeyOff
        {
            get;
            private set;
        }

        public virtual bool IsSoundingStarted
        {
            get;
            private set;
        }

        public virtual bool IsSoundOff
        {
            get;
            private set;
        }

        public virtual bool IsFakeSoundOff
        {
            get;
            private set;
        }

        public static ulong soundOffTimeCounter;

        /// <summary>
        /// for empty sound slot searching.
        /// </summary>
        public ulong SoundOffTime
        {
            get;
            private set;
        }

        /// <summary>
        /// チップ上の物理的なチャンネル(MIDI chと区別するためスロットとする)
        /// </summary>
        public int Slot
        {
            get;
            private set;
        }

        /// <summary>
        /// MIDIイベント
        /// </summary>
        public TaggedNoteOnEvent NoteOnEvent
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slot">チップ上の物理的なチャンネル(MIDI chと区別するためスロットとする)</param>
        protected SoundBase(InstrumentBase parentModule, SoundManagerBase manager, TimbreBase timbre, int baseTimbreIndex, TaggedNoteOnEvent noteOnEvent, int slot)
        {
            NoteOnEvent = noteOnEvent;
            Slot = slot;
            ParentModule = parentModule;
            ParentManager = manager;
            Timbre = timbre;
            BaseTimbreIndex = baseTimbreIndex;
            if (ParentModule.ChannelTypes[NoteOnEvent.Channel] == ChannelType.Drum ||
                ParentModule.ChannelTypes[NoteOnEvent.Channel] == ChannelType.DrumGt)
                DrumTimbre = ParentModule.DrumTimbres[NoteOnEvent.NoteNumber];
        }

        public event EventHandler Disposed;

        /// <summary>
        /// 
        /// </summary>
        public virtual void Dispose()
        {
            if (!IsDisposed)
            {
                if (!IsSoundOff)
                    SoundOff();
            }

            IsDisposed = true;

            Disposed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// サウンドオン
        /// </summary>
        public virtual void FakeKeyOn()
        {
            IsSoundingStarted = true;
            IsKeyOff = false;
            IsSoundOff = false;
        }

        /// <summary>
        /// サウンドオン
        /// </summary>
        public virtual void KeyOn()
        {
            IsSoundingStarted = true;
            IsKeyOff = false;
            IsSoundOff = false;

            if (validateMidiControlValue(ParentModule.ModulationDepthes[NoteOnEvent.Channel] + Timbre.MDS.ModulationDepthShift) > 64 ||
                validateMidiControlValue(ParentModule.Modulations[NoteOnEvent.Channel] + Timbre.MDS.ModulationShift) > 0)
                ModulationEnabled = true;

            int ln = ParentManager.GetLastNoteNumber(NoteOnEvent.Channel);
            if (ParentModule.Portamentos[NoteOnEvent.Channel] >= 64 || ln > 0x80)
            {
                if (ln >= 0)
                {
                    ln &= 0x7f;
                    PortamentoDeltaNoteNumber = ln - NoteOnEvent.NoteNumber;
                    portStartNoteDeltSign = Math.Sign(PortamentoDeltaNoteNumber);

                    PortamentoEnabled = true;
                }
            }

            var adsrs = Timbre.SDS.ADSR;
            ActiveADSR = adsrs.Enable;
            if (ActiveADSR)
            {
                AdsrEngine = new AdsrEngine();
                AdsrEngine.SetAttackRate(Math.Pow(10d * (127d - adsrs.AR) / 127d, 2));
                AdsrEngine.SetDecayRate(Math.Pow(100d * (adsrs.DR / 127d), 2));
                AdsrEngine.SetSustainRate(Math.Pow(60d * (adsrs.SR / 127d), 2));
                AdsrEngine.SetReleaseRate(Math.Pow(60d * (adsrs.RR / 127d), 2));
                AdsrEngine.SetSustainLevel((127d - adsrs.SL) / 127d);
                AdsrEngine.Gate(true);
            }

            var efs = Timbre.SDS.FxS;
            if (efs != null)
                ActiveFx = efs.Enable;

            SoundKeyOn?.Invoke(this, new SoundUpdatedEventArgs(NoteOnEvent.NoteNumber, NoteOnEvent.Velocity, lastPitch));

            if (DrumTimbre != null && DrumTimbre.GateTime != 0 &&
                ParentModule.ChannelTypes[NoteOnEvent.Channel] == ChannelType.Drum)
            {
                HighPrecisionTimer.SetPeriodicCallback
                    (new Func<object, double>(processGateTime), DrumTimbre.GateTime, this, true);
            }
        }

        private double processGateTime(object state)
        {
            if (!IsDisposed && !IsSoundOff)
                KeyOff();
            return -1;
        }

        public static event EventHandler<SoundUpdatedEventArgs> SoundKeyOn;

        /// <summary>
        ///キーオフ
        /// </summary>
        public virtual void KeyOff()
        {
            if (IsKeyOff)
                return;

            IsKeyOff = true;
            AdsrEngine?.Gate(false);

            TrySoundOff();

            SoundKeyOff?.Invoke(this, new SoundUpdatedEventArgs(NoteOnEvent.NoteNumber, NoteOnEvent.Velocity, lastPitch));
        }

        public static event EventHandler<SoundUpdatedEventArgs> SoundKeyOff;

        /// <summary>
        /// サウンドオフ
        /// </summary>
        public virtual bool TrySoundOff()
        {
            if (ActiveADSR || ActiveFx)
                return false;

            SoundOff();

            return true;
        }

        /// <summary>
        /// サウンドオフ
        /// </summary>
        public virtual void SoundOff()
        {
            if (!IsKeyOff)
            {
                IsKeyOff = true;
                AdsrEngine?.Gate(false);

                SoundKeyOff?.Invoke(this, new SoundUpdatedEventArgs(NoteOnEvent.NoteNumber, NoteOnEvent.Velocity, lastPitch));
            }

            IsSoundOff = true;

            SoundOffTime = soundOffTimeCounter++;

            SoundSoundOff?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// サウンドオフ
        /// </summary>
        public virtual void FakeSoundOff()
        {
            if (!IsKeyOff)
            {
                IsKeyOff = true;
                AdsrEngine?.Gate(false);

                SoundKeyOff?.Invoke(this, new SoundUpdatedEventArgs(NoteOnEvent.NoteNumber, NoteOnEvent.Velocity, lastPitch));
            }

            IsSoundOff = true;
            IsFakeSoundOff = true;

            SoundOffTime = soundOffTimeCounter++;

            SoundSoundOff?.Invoke(this, EventArgs.Empty);
        }

        public static event EventHandler SoundSoundOff;

        /// <summary>
        /// 
        /// </summary>
        public virtual void OnVolumeUpdated()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void OnPanpotUpdated()
        {
        }

        public static event EventHandler<SoundUpdatedEventArgs> SoundPitchUpdated;

        /// <summary>
        /// 
        /// </summary>
        public virtual void OnPitchUpdated()
        {
            SoundPitchUpdated?.Invoke(this, new SoundUpdatedEventArgs(NoteOnEvent.NoteNumber, NoteOnEvent.Velocity, lastPitch));
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void OnSoundParamsUpdated()
        {
            OnVolumeUpdated();
            OnPitchUpdated();
            OnPanpotUpdated();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected double CalcCurrentFrequency()
        {
            double d = CalcCurrentPitchDeltaNoteNumber();

            int nn = NoteOnEvent.NoteNumber;
            if (ParentModule.ChannelTypes[NoteOnEvent.Channel] == ChannelType.Drum ||
                ParentModule.ChannelTypes[NoteOnEvent.Channel] == ChannelType.DrumGt)
                nn = (int)ParentModule.DrumTimbres[NoteOnEvent.NoteNumber].BaseNote;

            double noteNum = Math.Pow(2.0, ((double)nn + d - 69.0) / 12.0);
            double freq = 440.0 * noteNum;
            return freq;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected double CalcCurrentPitchDeltaNoteNumber()
        {
            CombinedTimbreSettings parent = ParentModule.TryGetBaseTimbreSettings(NoteOnEvent, Timbre, BaseTimbreIndex);
            int pKeyShift = parent != null ? parent.KeyShift : 0;
            int pPitchShift = parent != null ? parent.PitchShift : 0;

            var pitch = (int)ParentModule.Pitchs[NoteOnEvent.Channel] - 8192;
            var range = (int)ParentModule.PitchBendRanges[NoteOnEvent.Channel];
            var scale = (int)ParentModule.ScaleTunings[NoteOnEvent.Channel].ScalesNums[(int)Math.Abs(NoteOnEvent.NoteNumber + Timbre.MDS.KeyShift + pKeyShift) % 12];
            var tune = (int)ParentModule.FineTunes[NoteOnEvent.Channel] - 8192;

            double d1 = ((double)pitch / 8192d) * range;
            double d = d1 + ModultionDeltaNoteNumber + PortamentoDeltaNoteNumber + ArpeggiateDeltaNoteNumber +
                pKeyShift + Timbre.MDS.KeyShift +
                ((pPitchShift + Timbre.MDS.PitchShift) / 100d) + (scale / 100d) + (tune / 8192d);

            if (FxEngine != null)
                d += FxEngine.DeltaNoteNumber;

            lastPitch = d;
            return d;
        }

        private double lastPitch;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected double CalcCurrentVolume()
        {
            return CalcCurrentVolume(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected double CalcCurrentVolume(bool ignoreVelocity)
        {
            double v = 1;

            if (!Timbre.UseExprForModulator)
                v *= ParentModule.Expressions[NoteOnEvent.Channel] / 127d;
            v *= ParentModule.Volumes[NoteOnEvent.Channel] / 127d;
            if (!ignoreVelocity && !Timbre.MDS.IgnoreVelocity)
                v *= NoteOnEvent.Velocity / 127d;

            CombinedTimbreSettings parent = ParentModule.TryGetBaseTimbreSettings(NoteOnEvent, Timbre, BaseTimbreIndex);

            if (AdsrEngine != null)
                v *= AdsrEngine.OutputLevel;

            if (FxEngine != null)
                v *= FxEngine.OutputLevel;

            v *= ArpeggiateLevel;

            if (parent != null)
                v += parent.VolumeOffest;
            if (v > 1.0)
                v = 1.0;
            else if (v < 0.0)
                v = 0.0;

            return v;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected double CalcModulatorMultiply()
        {
            var v = ParentModule.Expressions[NoteOnEvent.Channel];
            if (v > 64)
                return -(ParentModule.Expressions[NoteOnEvent.Channel] - 64) / 63d;
            else if (v < 63)
                return -(ParentModule.Expressions[NoteOnEvent.Channel] - 63) / 63d;
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected byte CalcCurrentPanpot()
        {
            int pan = ParentModule.Panpots[NoteOnEvent.Channel] - 1;

            if (FxEngine != null)
                pan += FxEngine.PanShift;

            if (ParentModule.ChannelTypes[NoteOnEvent.Channel] == ChannelType.Drum ||
                ParentModule.ChannelTypes[NoteOnEvent.Channel] == ChannelType.DrumGt)
                pan += (int)ParentModule.DrumTimbres[NoteOnEvent.NoteNumber].PanShift;
            else
                pan += Timbre.MDS.PanShift;

            CombinedTimbreSettings parent = ParentModule.TryGetBaseTimbreSettings(NoteOnEvent, Timbre, BaseTimbreIndex);
            if (parent != null)
                pan += parent.PanShift;

            if (pan < 0)
                pan = 0;
            else if (pan > 127)
                pan = 127;

            return (byte)pan;
        }

        private double f_ArpeggiateDeltaNoteNumber;

        /// <summary>
        /// 
        /// </summary>
        internal protected double ArpeggiateDeltaNoteNumber
        {
            get
            {
                return f_ArpeggiateDeltaNoteNumber;
            }
            internal set
            {
                if (f_ArpeggiateDeltaNoteNumber != value)
                {
                    f_ArpeggiateDeltaNoteNumber = value;
                    OnPitchUpdated();
                }
            }
        }

        private double f_ArpeggiateVolume = 1d;

        /// <summary>
        /// 
        /// </summary>
        internal protected double ArpeggiateLevel
        {
            get
            {
                return f_ArpeggiateVolume;
            }
            internal set
            {
                if (f_ArpeggiateVolume != value)
                {
                    f_ArpeggiateVolume = value;
                    OnVolumeUpdated();
                }
            }
        }

        private double processFx(object state)
        {
            if (!IsDisposed && ActiveFx && FxEngine != null)
            {
                if (FxEngine.Process(this, IsKeyOff, IsSoundOff))
                    OnProcessFx();

                ActiveFx = FxEngine.Active;

                if (ActiveFx)
                {
                    var freq = MidiManager.CalcCurrentFrequency(this.NoteOnEvent.NoteNumber);
                    var ksl = FxEngine.Settings.KeyScaleLevel;
                    if (ksl != 0)
                    {
                        if (ksl > 0)
                        {
                            var interval = FxEngine.Settings.EnvelopeInterval * 440d / freq;
                            return interval * ksl;
                        }
                        else
                        {
                            var interval = FxEngine.Settings.EnvelopeInterval * freq / 440d;
                            return interval / (-ksl);
                        }
                    }
                    else
                    {
                        return FxEngine.Settings.EnvelopeInterval;
                    }
                }
                else if (IsKeyOff)
                    TrySoundOff();
            }
            return -1;
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void OnProcessFx()
        {
            OnSoundParamsUpdated();
        }

        private double processAdsr(object state)
        {
            if (!IsDisposed && ActiveADSR && AdsrEngine != null)
            {
                AdsrEngine.Process();

                OnProcessAdsr();

                if (AdsrEngine.AdsrState != AdsrState.SoundOff)
                    return 1;

                ActiveADSR = false;
                TrySoundOff();
            }
            return -1;
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void OnProcessAdsr()
        {
            OnVolumeUpdated();
        }

        private static double[] PortamentSpeedTable ={
            1000,301.995172,181.9700859,128.8249552,91.20108394,69.18309709,53.70317964,42.65795188,35.48133892,30.1995172,26.30267992,22.90867653,
            20.41737945,18.62087137,16.98243652,15.84893192,14.79108388,13.80384265,13.18256739,12.58925412,12.02264435,11.74897555,11.22018454,10.71519305,
            10.47128548,10,9.54992586,9.332543008,9.120108394,8.7096359,8.511380382,8.128305162,7.943282347,7.58577575,7.244359601,6.918309709,6.60693448,6.309573445,
            6.025595861,5.754399373,5.495408739,5.248074602,5.12861384,4.897788194,4.677351413,4.466835922,4.265795188,4.073802778,3.89045145,3.715352291,
            3.548133892,3.388441561,3.235936569,3.090295433,2.951209227,2.818382931,2.691534804,2.570395783,2.454708916,2.398832919,2.290867653,2.187761624,
            2.089296131,1.995262315,1.905460718,1.819700859,1.737800829,1.659586907,1.621810097,1.548816619,1.479108388,1.412537545,1.348962883,1.318256739,
            1.258925412,1.202264435,1.148153621,1.096478196,1.071519305,1.023292992,0.977237221,0.933254301,0.891250938,0.851138038,0.812830516,0.776247117,
            0.72443596,0.691830971,0.660693448,0.630957344,0.602559586,0.575439937,0.549540874,0.52480746,0.489778819,0.467735141,0.446683592,0.426579519,
            0.407380278,0.389045145,0.371535229,0.354813389,0.338844156,0.323593657,0.309029543,0.295120923,0.281838293,0.26915348,0.257039578,0.239883292,
            0.229086765,0.213796209,0.204173794,0.190546072,0.177827941,0.165958691,0.151356125,0.138038426,0.125892541,0.112201845,0.097723722,0.083176377,
            0.069183097,0.054954087,0.042657952,0.031622777,0.020417379,0.01
        };

        private double processPortamento(object state)
        {
            if (!IsDisposed && PortamentoEnabled && PortamentoDeltaNoteNumber != 0)
            {
                double delta = -portStartNoteDeltSign * PortamentSpeedTable[ParentModule.PortamentoTimes[NoteOnEvent.Channel]] / (100 / PortamentInterval);
                PortamentoDeltaNoteNumber += delta;

                if (portStartNoteDeltSign < 0 && PortamentoDeltaNoteNumber >= 0)
                    PortamentoDeltaNoteNumber = 0;
                else if (portStartNoteDeltSign > 0 && PortamentoDeltaNoteNumber <= 0)
                    PortamentoDeltaNoteNumber = 0;

                OnPitchUpdated();

                if (PortamentoDeltaNoteNumber != 0)
                    return PortamentInterval;

                PortamentoEnabled = false;
            }
            return -1;
        }

        private int validateMidiControlValue(int value)
        {
            if (value < 0)
                value = 0;
            else if (value > 127)
                value = 127;
            return value;
        }

        private double processModulation(object state)
        {
            if (!IsDisposed && ModulationEnabled)
            {
                double radian = 2 * Math.PI * (modulationStep / (HighPrecisionTimer.TIMER_BASIC_1KHZ / ModulationInterval));

                double mdepth = 0;

                int mcmd = validateMidiControlValue(ParentModule.ModulationDepthes[NoteOnEvent.Channel] + Timbre.MDS.ModulationDepthShift);

                if (mcmd > 64)
                {
                    if (modulationStartCounter < 10d * (HighPrecisionTimer.TIMER_BASIC_1KHZ / ModulationInterval))
                        modulationStartCounter += 1.0;

                    var md = ParentModule.GetModulationDelaySec(NoteOnEvent.Channel, Timbre.MDS.ModulationDelayShift);
                    md = md * (HighPrecisionTimer.TIMER_BASIC_1KHZ / ModulationInterval);
                    if (modulationStartCounter > md)
                        mdepth = ((double)mcmd - 64d) / 64d;
                }
                //急激な変化を抑制
                var mv = ((double)validateMidiControlValue(ParentModule.Modulations[NoteOnEvent.Channel] + Timbre.MDS.ModulationShift) / 127d) + mdepth;
                if (mv != modulationLevel)
                    modulationLevel += (mv - modulationLevel) / 1.25;

                double modHz = radian * ParentModule.GetModulationRateHz(NoteOnEvent.Channel, Timbre.MDS.ModulationRateShift);
                ModultionDeltaNoteNumber = modulationLevel * Math.Sin(modHz);
                ModultionDeltaNoteNumber *= ((double)validateMidiControlValue(ParentModule.ModulationDepthRangesNote[NoteOnEvent.Channel] + Timbre.MDS.ModulationDepthRangeNoteShift) +
                    ((double)validateMidiControlValue(ParentModule.ModulationDepthRangesCent[NoteOnEvent.Channel] + Timbre.MDS.ModulationDepthRangeCentShift) / 127d));

                OnPitchUpdated();

                if (modulationStep == 0 && IsSoundOff)
                    return -1;

                modulationStep += 1.0;
                if (modHz > 2 * Math.PI)
                    modulationStep = 0;

                return ModulationInterval;
            }
            return -1;
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void OnProcessModulation()
        {
            OnPitchUpdated();
        }

        /// <summary>
        /// 
        /// </summary>
        public AdsrState CurrentAdsrState
        {
            get
            {
                if (AdsrEngine != null)
                {
                    return AdsrEngine.AdsrState;
                }
                else
                {
                    if (!IsKeyOff)
                        return AdsrState.Sustain;
                    else
                        return AdsrState.SoundOff;
                }
            }
        }

        #region modulation

        /// <summary>
        /// モジュレーションの進角
        /// </summary>
        public double modulationStep;

        /// <summary>
        /// モジュレーションの開始カウンタ
        /// </summary>
        public double modulationStartCounter;

        /// <summary>
        /// モジュレーションの効き具合(0-1.0)
        /// </summary>
        public double ModultionDeltaNoteNumber
        {
            get;
            private set;
        }

        /// <summary>
        /// モジュレーションホイール値(0-1.0)
        /// </summary>
        private double modulationLevel;

        private bool f_modulationEnabled;

        /// <summary>
        /// モジュレーションの有効/無効
        /// </summary>
        public bool ModulationEnabled
        {
            get
            {
                return f_modulationEnabled;
            }
            set
            {
                if (value != f_modulationEnabled)
                {
                    if (value)
                    {
                        f_modulationEnabled = value;
                    }
                    else
                    {
                        if (validateMidiControlValue(ParentModule.ModulationDepthes[NoteOnEvent.Channel] + Timbre.MDS.ModulationDepthShift) > 64)
                            return;

                        f_modulationEnabled = value;
                    }
                    if (f_modulationEnabled)
                        HighPrecisionTimer.SetPeriodicCallback(new Func<object, double>(processModulation), ModulationInterval, null);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual double ModulationInterval
        {
            get
            {
                return ParentModule.ProcessingInterval;
            }
        }


        #endregion

        #region portamento

        public double PortamentoDeltaNoteNumber
        {
            get;
            private set;
        }

        private double portStartNoteDeltSign;

        private bool f_portamentoEnabled;

        /// <summary>
        /// ポルタメントの有効無効
        /// </summary>
        public bool PortamentoEnabled
        {
            get
            {
                return f_portamentoEnabled;
            }
            set
            {
                if (value != f_portamentoEnabled)
                {
                    f_portamentoEnabled = value;

                    if (f_portamentoEnabled)
                        HighPrecisionTimer.SetPeriodicCallback(new Func<object, double>(processPortamento), PortamentInterval, null);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual double PortamentInterval
        {
            get
            {
                return ParentModule.ProcessingInterval;
            }
        }

        #endregion

        private bool f_AdsrEnabled;

        /// <summary>
        /// ADSRの有効無効
        /// </summary>
        public bool ActiveADSR
        {
            get
            {
                return f_AdsrEnabled;
            }
            set
            {
                if (value != f_AdsrEnabled)
                {
                    f_AdsrEnabled = value;

                    if (f_AdsrEnabled)
                        HighPrecisionTimer.SetPeriodicCallback(new Func<object, double>(processAdsr), 1, null);
                }
            }
        }

        private bool f_FxEnabled;

        /// <summary>
        /// Fxの有効無効
        /// </summary>
        public bool ActiveFx
        {
            get
            {
                return f_FxEnabled;
            }
            set
            {
                if (value != f_FxEnabled)
                {
                    f_FxEnabled = value;

                    if (f_FxEnabled)
                    {
                        var efs = Timbre.SDS.FxS;
                        if (efs != null)
                        {
                            FxEngine = efs.CreateEngine();
                            HighPrecisionTimer.SetPeriodicCallback(new Func<object, double>(processFx), efs.EnvelopeInterval, null);
                        }
                    }
                }
            }
        }

        protected AdsrEngine AdsrEngine { get; private set; }

        protected AbstractFxEngine FxEngine { get; private set; }

    }

    /// <summary>
    /// 
    /// </summary>
    public class SoundUpdatedEventArgs : EventArgs
    {
        public int NoteNumber
        {
            get;
            private set;
        }

        public int Velocity
        {
            get;
            private set;
        }

        public double Pitch
        {
            get;
            private set;
        }

        public SoundUpdatedEventArgs(int noteNumber, int velocity, double pitch)
        {
            NoteNumber = noteNumber;
            Velocity = velocity;
            Pitch = pitch;
        }

    }
}
