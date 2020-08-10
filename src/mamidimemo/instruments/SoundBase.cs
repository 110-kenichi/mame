// copyright-holders:K.Ito
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

        public virtual bool IsSoundOff
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

        private uint gateTime;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slot">チップ上の物理的なチャンネル(MIDI chと区別するためスロットとする)</param>
        protected SoundBase(InstrumentBase parentModule, SoundManagerBase manager, TimbreBase timbre, TaggedNoteOnEvent noteOnEvent, int slot)
        {
            NoteOnEvent = noteOnEvent;
            Slot = slot;
            ParentModule = parentModule;
            ParentManager = manager;
            Timbre = timbre;
        }

        public event EventHandler Disposed;

        /// <summary>
        /// 
        /// </summary>
        public virtual void Dispose()
        {
            if (!IsDisposed)
            {
                KeyOff();
                SoundOff();
            }

            IsDisposed = true;

            Disposed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// サウンドオン
        /// </summary>
        public virtual void KeyOn()
        {
            if (ParentModule.ModulationDepthes[NoteOnEvent.Channel] > 64 ||
                ParentModule.Modulations[NoteOnEvent.Channel] > 0)
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
            EnableADSR = adsrs.Enable;
            if (EnableADSR)
            {
                AdsrEngine = new AdsrEngine();
                AdsrEngine.SetAttackRate(Math.Pow(10d * (127d - adsrs.AR) / 127d, 2));
                AdsrEngine.SetDecayRate(Math.Pow(10d * (adsrs.DR / 127d), 2));
                AdsrEngine.SetReleaseRate(Math.Pow(60d * (adsrs.RR / 127d), 2));
                AdsrEngine.SetSustainLevel((127d - adsrs.SL) / 127d);
                AdsrEngine.Gate(true);
            }

            var efs = Timbre.SDS.FxS;
            if (efs != null)
                EnableFx = efs.Enable;

            SoundKeyOn?.Invoke(this, new SoundUpdatedEventArgs(NoteOnEvent.NoteNumber, lastPitch));

            if (ParentModule.ChannelTypes[NoteOnEvent.Channel] == ChannelType.Drum)
            {
                gateTime = ParentModule.DrumTimbres[NoteOnEvent.NoteNumber].GateTime;
                HighPrecisionTimer.SetPeriodicCallback(new Func<object, double>(processGateTime), gateTime, null);
            }
        }

        private double processGateTime(object state)
        {
            if (!IsDisposed && !IsSoundOff)
            {
                if (gateTime > 0)
                    gateTime -= 1 / HighPrecisionTimer.TIMER_BASIC_1MS_COUNT;
                if (gateTime > 0)
                    return gateTime;

                KeyOff();
            }
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

            if (CurrentAdsrState == AdsrState.SoundOff)
                SoundOff();

            SoundKeyOff?.Invoke(this, new SoundUpdatedEventArgs(NoteOnEvent.NoteNumber, lastPitch));
        }

        public static event EventHandler<SoundUpdatedEventArgs> SoundKeyOff;

        /// <summary>
        ///キーオフ
        /// </summary>
        public virtual void SoundOff()
        {
            IsSoundOff = true;

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
            SoundPitchUpdated?.Invoke(this, new SoundUpdatedEventArgs(NoteOnEvent.NoteNumber, lastPitch));
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void OnSoundParamsUpdated()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected double CalcCurrentFrequency()
        {
            double d = CalcCurrentPitch();

            int nn = NoteOnEvent.NoteNumber;
            if (ParentModule.ChannelTypes[NoteOnEvent.Channel] == ChannelType.Drum)
                nn = (int)ParentModule.DrumTimbres[NoteOnEvent.NoteNumber].BaseNote;

            double noteNum = Math.Pow(2.0, ((double)nn + d - 69.0) / 12.0);
            double freq = 440.0 * noteNum;
            return freq;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected double CalcCurrentPitch()
        {
            var pitch = (int)ParentModule.Pitchs[NoteOnEvent.Channel] - 8192;
            var range = (int)ParentModule.PitchBendRanges[NoteOnEvent.Channel];

            double d1 = ((double)pitch / 8192d) * range;
            double d = d1 + ModultionDeltaNoteNumber + PortamentoDeltaNoteNumber + ArpeggiateDeltaNoteNumber + Timbre.KeyShift + (Timbre.PitchShift / 100d);

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
            double v = 1;

            v *= ParentModule.Expressions[NoteOnEvent.Channel] / 127d;
            v *= ParentModule.Volumes[NoteOnEvent.Channel] / 127d;
            v *= NoteOnEvent.Velocity / 127d;

            if (AdsrEngine != null)
                v *= AdsrEngine.OutputLevel;

            if (FxEngine != null)
                v *= FxEngine.OutputLevel;

            v *= ArpeggiateLevel;

            return v;
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
            if (!IsDisposed && !IsSoundOff && EnableFx && FxEngine != null)
            {
                if (FxEngine.Process(this, IsKeyOff, IsSoundOff))
                {
                    OnPitchUpdated();
                    OnVolumeUpdated();
                }

                EnableFx = FxEngine.Active;

                if (EnableFx)
                    return FxEngine.Settings.EnvelopeInterval;
            }
            return -1;
        }

        private double processAdsr(object state)
        {
            if (!IsDisposed && !IsSoundOff && EnableADSR && AdsrEngine != null)
            {
                AdsrEngine.Process();

                OnVolumeUpdated();

                if (AdsrEngine.AdsrState != AdsrState.SoundOff)
                    return 1;

                EnableADSR = false;
                SoundOff();
            }
            return -1;
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
            if (!IsDisposed && !IsSoundOff && PortamentoEnabled && PortamentoDeltaNoteNumber != 0)
            {
                //double delta = -portStartNoteDeltSign * 12d / Math.Pow(((double)ParentModule.PortamentoTimes[NoteOnEvent.Channel] / 2d) + 1d, 1.25);
                double delta = -portStartNoteDeltSign * PortamentSpeedTable[ParentModule.PortamentoTimes[NoteOnEvent.Channel]] * HighPrecisionTimer.TIMER_BASIC_1MS_COUNT / 100d;
                PortamentoDeltaNoteNumber += delta;

                if (portStartNoteDeltSign < 0 && PortamentoDeltaNoteNumber >= 0)
                    PortamentoDeltaNoteNumber = 0;
                else if (portStartNoteDeltSign > 0 && PortamentoDeltaNoteNumber <= 0)
                    PortamentoDeltaNoteNumber = 0;

                OnPitchUpdated();

                if (PortamentoDeltaNoteNumber != 0)
                    return 1;

                PortamentoEnabled = false;
            }
            return -1;
        }

        private double processModulation(object state)
        {
            if (!IsDisposed && !IsSoundOff && ModulationEnabled)
            {
                double radian = 2 * Math.PI * (modulationStep / HighPrecisionTimer.TIMER_BASIC_1KHZ);

                double mdepth = 0;
                if (ParentModule.ModulationDepthes[NoteOnEvent.Channel] > 64)
                {
                    if (modulationStartCounter < 10d * HighPrecisionTimer.TIMER_BASIC_1KHZ)
                        modulationStartCounter += 1.0;

                    if (modulationStartCounter > ParentModule.GetModulationDelaySec(NoteOnEvent.Channel) * HighPrecisionTimer.TIMER_BASIC_1KHZ)
                        mdepth = (double)ParentModule.ModulationDepthes[NoteOnEvent.Channel] / 127d;
                }
                //急激な変化を抑制
                var mv = ((double)ParentModule.Modulations[NoteOnEvent.Channel] / 127d) + mdepth;
                if (mv != modultionLevel)
                    modultionLevel += (mv - modultionLevel) / 1.25;

                double modHz = radian * ParentModule.GetModulationRateHz(NoteOnEvent.Channel);
                ModultionDeltaNoteNumber = modultionLevel * Math.Sin(modHz);
                ModultionDeltaNoteNumber *= ((double)ParentModule.ModulationDepthRangesNote[NoteOnEvent.Channel] +
                    ((double)ParentModule.ModulationDepthRangesCent[NoteOnEvent.Channel] / 127d));

                modulationStep += 1.0;
                if (modHz > 2 * Math.PI)
                    modulationStep = 0;

                OnPitchUpdated();

                return 1;
            }
            return -1;
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
        private double modultionLevel;

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
                        if (ParentModule.ModulationDepthes[NoteOnEvent.Channel] > 64)
                            return;

                        f_modulationEnabled = value;
                    }
                    if (f_modulationEnabled)
                        HighPrecisionTimer.SetPeriodicCallback(new Func<object, double>(processModulation), 1, null);
                }
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
                        HighPrecisionTimer.SetPeriodicCallback(new Func<object, double>(processPortamento), 1, null);
                }
            }
        }

        #endregion

        private bool f_AdsrEnabled;

        /// <summary>
        /// ADSRの有効無効
        /// </summary>
        public bool EnableADSR
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
        public bool EnableFx
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

        public double Pitch
        {
            get;
            private set;
        }

        public SoundUpdatedEventArgs(int noteNumber, double pitch)
        {
            NoteNumber = noteNumber;
            Pitch = pitch;
        }

    }
}
