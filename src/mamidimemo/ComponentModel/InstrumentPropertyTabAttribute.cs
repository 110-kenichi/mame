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
    public class InstrumentPropertyTabAttribute : PropertyTabAttribute
    {
        public InstrumentPropertyTabAttribute()
        {
            InitializeArrays(
                new Type[] {
                    typeof(InstMidiTab),
                    typeof(InstTimbresTab),
                    typeof(InstGeneralTab),
                    typeof(InstFilterTab),
                    typeof(InstChipTab) },
                new PropertyTabScope[] {
                    PropertyTabScope.Component,
                    PropertyTabScope.Component,
                    PropertyTabScope.Component,
                    PropertyTabScope.Component,
                    PropertyTabScope.Component
                });
        }
    }
}
