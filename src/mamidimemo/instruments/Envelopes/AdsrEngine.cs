using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace zanac.MAmidiMEmo.Instruments.Envelopes
{

    //https://www.earlevel.com/main/2013/06/03/envelope-generators-adsr-code/
    /// <summary>
    /// 
    /// </summary>
    public class AdsrEngine
    {
        AdsrState adsrState;
        double outputLevel;
        double attackRate;
        double decayRate;
        double sustainRate;
        double releaseRate;
        double attackCoef;
        double decayCoef;
        double sustainCoef;
        double releaseCoef;
        double sustainLevel;
        double targetRatioA;
        double targetRatioDR;
        double attackBase;
        double decayBase;
        double sustainBase;
        double releaseBase;


        [Browsable(false)]
        [IgnoreDataMember]
        [JsonIgnore]
        public AdsrState AdsrState
        {
            get => adsrState;
        }

        [Browsable(false)]
        [IgnoreDataMember]
        [JsonIgnore]
        public double OutputLevel
        {
            get => outputLevel;
        }

        public AdsrEngine()
        {
            SetAttackRate(0);
            SetDecayRate(0);
            SetSustainRate(30);
            SetReleaseRate(30);
            SetSustainLevel(1.0);
            SetTargetRatioA(0.3);
            SetTargetRatioDR(0.01);
        }

        /// <summary>
        /// set attack rate
        /// </summary>
        /// <param name="rate">[sec]</param>
        public void SetAttackRate(double rate)
        {
            attackRate = rate * HighPrecisionTimer.TIMER_BASIC_1KHZ;
            attackCoef = calcCoef(rate, targetRatioA);
            attackBase = (1.0 + targetRatioA) * (1.0 - attackCoef);
        }

        /// <summary>
        /// set decay rate
        /// </summary>
        /// <param name="rate">[sec]</param>
        public void SetDecayRate(double rate)
        {
            decayRate = rate * HighPrecisionTimer.TIMER_BASIC_1KHZ;
            decayCoef = calcCoef(rate, targetRatioDR);
            decayBase = (sustainLevel - targetRatioDR) * (1.0 - decayCoef);
        }

        /// <summary>
        /// set sustain rate
        /// </summary>
        /// <param name="rate">[sec]</param>
        public void SetSustainRate(double rate)
        {
            sustainRate = rate * HighPrecisionTimer.TIMER_BASIC_1KHZ;
            sustainCoef = calcCoef(rate, targetRatioDR);
            sustainBase = -targetRatioDR * (1.0 - sustainCoef);
        }

        /// <summary>
        /// set release rate
        /// </summary>
        /// <param name="rate">[sec]</param>
        public void SetReleaseRate(double rate)
        {
            releaseRate = rate * HighPrecisionTimer.TIMER_BASIC_1KHZ;
            releaseCoef = calcCoef(rate, targetRatioDR);
            releaseBase = -targetRatioDR * (1.0 - releaseCoef);
        }

        private double calcCoef(double rate, double targetRatio)
        {
            return (rate <= 0) ? 0.0 : Math.Exp(-Math.Log((1.0 + targetRatio) / targetRatio) / rate);
        }

        /// <summary>
        /// set sustain level
        /// </summary>
        /// <param name="level"></param>
        public void SetSustainLevel(double level)
        {
            sustainLevel = level;
            decayBase = (sustainLevel - targetRatioDR) * (1.0 - decayCoef);
        }

        /// <summary>
        /// Adjust the curves of the Attack, or Decay and Release segments,
        /// from the initial default values (small number such as 0.0001 to 0.01 for mostly-exponential,
        /// large numbers like 100 for virtually linear):
        /// </summary>
        /// <param name="targetRatio">0.0001 to 0.01</param>
        public void SetTargetRatioA(double targetRatio)
        {
            if (targetRatio < 0.000000001)
                targetRatio = 0.000000001;  // -180 dB
            targetRatioA = targetRatio;
            attackCoef = calcCoef(attackRate, targetRatioA);
            attackBase = (1.0 + targetRatioA) * (1.0 - attackCoef);
        }

        /// <summary>
        /// Adjust the curves of the Attack, or Decay and Release segments,
        /// from the initial default values (small number such as 0.0001 to 0.01 for mostly-exponential,
        /// large numbers like 100 for virtually linear):
        /// </summary>
        /// <param name="targetRatio">0.0001 to 0.01</param>
        public void SetTargetRatioDR(double targetRatio)
        {
            if (targetRatio < 0.000000001)
                targetRatio = 0.000000001;  // -180 dB
            targetRatioDR = targetRatio;
            decayCoef = calcCoef(decayRate, targetRatioDR);
            sustainCoef = calcCoef(sustainRate, targetRatioDR);
            releaseCoef = calcCoef(releaseRate, targetRatioDR);
            decayBase = (sustainLevel - targetRatioDR) * (1.0 - decayCoef);
            releaseBase = -targetRatioDR * (1.0 - releaseCoef);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double Process()
        {
            switch (adsrState)
            {
                case AdsrState.SoundOff:
                    break;
                case AdsrState.Attack:
                    outputLevel = attackBase + outputLevel * attackCoef;
                    if (outputLevel >= 1.0)
                    {
                        outputLevel = 1.0;
                        adsrState = AdsrState.Decay;
                    }
                    break;
                case AdsrState.Decay:
                    outputLevel = decayBase + outputLevel * decayCoef;
                    if (outputLevel <= sustainLevel)
                    {
                        outputLevel = sustainLevel;
                        adsrState = AdsrState.Sustain;
                    }
                    break;
                case AdsrState.Sustain:
                    if (sustainRate != 3600000)
                    {
                        outputLevel = sustainBase + outputLevel * sustainCoef;
                        if (outputLevel <= 0.0)
                        {
                            outputLevel = 0.0;
                            adsrState = AdsrState.SoundOff;
                        }
                    }
                    break;
                case AdsrState.Release:
                    outputLevel = releaseBase + outputLevel * releaseCoef;
                    if (outputLevel <= 0.0)
                    {
                        outputLevel = 0.0;
                        adsrState = AdsrState.SoundOff;
                    }
                    break;
            }
            return outputLevel;
        }


        public void Gate(bool keyOn)
        {
            if (keyOn)
                adsrState = AdsrState.Attack;
            else if (adsrState != AdsrState.SoundOff)
                adsrState = AdsrState.Release;
        }

        public void Reset()
        {
            adsrState = AdsrState.SoundOff;
            outputLevel = 0.0;
        }

    }

    /// <summary>
    /// 
    /// </summary>
    public enum AdsrState
    {
        SoundOff = 0,
        Attack,
        Decay,
        Sustain,
        Release
    };
}
