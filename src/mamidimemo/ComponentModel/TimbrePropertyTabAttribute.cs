// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;

namespace zanac.MAmidiMEmo.ComponentModel
{
    public class TimbrePropertyTabAttribute : PropertyTabAttribute
    {
        public TimbrePropertyTabAttribute()
        {
            InitializeArrays(
                new Type[] {
                    typeof(TimbreMidiTab),
                    typeof(TimbreSoundTab),
                    typeof(TimbreGeneralTab),
                    typeof(TimbreChipTab)},
                new PropertyTabScope[] {
                    PropertyTabScope.Component,
                    PropertyTabScope.Component,
                    PropertyTabScope.Component,
                    PropertyTabScope.Component
                });
        }
    }
}
