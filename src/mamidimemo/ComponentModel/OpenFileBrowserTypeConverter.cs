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
    public class OpenFileBrowserTypeConverter : TypeConverter
    {
        /// <summary>
        /// 
        /// </summary>
        public OpenFileBrowserTypeConverter()
        {
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return "(Click […] to open the file browser)";
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
