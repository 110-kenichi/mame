// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Collections;
using System.Globalization;

namespace zanac.MAmidiMEmo.ComponentModel
{
    /// <summary>
    /// </summary>
    public class OpenEditorTypeConverter : TypeConverter
    {
        /// <summary>
        /// 
        /// </summary>
        public OpenEditorTypeConverter()
        {
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return "(Click […] to open the editor)";
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
