using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace zanac.MAmidiMEmo.ComponentModel
{
    public class EnumConverter<TEnum> : TypeConverter where TEnum : struct
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string svalue = string.Format(culture, "{0}", value);
            TEnum e;
            if (Enum.TryParse(svalue, out e))
                return e;

            foreach (FieldInfo fi in typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                object[] atts = fi.GetCustomAttributes(typeof(DescriptionAttribute), true);
                if (atts != null && atts.Length > 0)
                {
                    if (string.Compare(((DescriptionAttribute)atts[0]).Description, svalue, StringComparison.OrdinalIgnoreCase) == 0)
                        return fi.GetValue(null);
                }
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                string svalue = string.Format(culture, "{0}", value);
                foreach (FieldInfo fi in typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static))
                {
                    object[] atts = fi.GetCustomAttributes(typeof(DescriptionAttribute), true);
                    if (atts != null && atts.Length > 0)
                    {
                        if (string.Compare(fi.Name, svalue, StringComparison.OrdinalIgnoreCase) == 0)
                            return ((DescriptionAttribute)atts[0]).Description;
                    }
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(Enum.GetValues(typeof(TEnum)));
        }
    }
}
