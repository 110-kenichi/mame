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
    public abstract class AbstractFxEngine
    {

        public abstract AbstractFxSettingsBase Settings
        {
            get;
        }

        /// <summary>
        /// エフェクトが動作しているかどうか falseなら終了
        /// </summary>
        public abstract bool Active
        {
            get;
            protected set;
        }

        /// <summary>
        /// 
        /// </summary>
        public abstract double OutputLevel
        {
            get;
        }

        /// <summary>
        /// -127:Left
        /// 127:Right
        /// </summary>
        public abstract int PanShift
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        public abstract double DeltaNoteNumber
        {
            get;
        }


        /// <summary>
        /// 
        /// </summary>
        public abstract bool Process(SoundBase sound, bool isKeyOff, bool isSoundOff);
    }
}
