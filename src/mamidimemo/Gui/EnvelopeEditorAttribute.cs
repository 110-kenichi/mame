// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zanac.MAmidiMEmo.Gui
{
    /// <summary>
    /// 
    /// </summary>
    public class EnvelopeEditorAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public int Min
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public int Max
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileDialogFilter"></param>
        /// <param name="maxSize"></param>
        public EnvelopeEditorAttribute(int min, int max)
        {
            Min = min;
            Max = max;
        }
    }
}
