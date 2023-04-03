using Melanchall.DryWetMidi.Common;
using System;
using System.Collections;
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
using zanac.MAmidiMEmo.Instruments;
using zanac.MAmidiMEmo.Midi;

namespace zanac.MAmidiMEmo.ComponentModel
{
    public class ProgramAssignmentNumberConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            dynamic instance = context.Instance;

            if (value is string)
            {
                if (!string.IsNullOrWhiteSpace((string)value))
                {
                    string text = (string)value;
                    string[] tn = text.Split(' ');
                    return base.ConvertFrom(context, culture, tn[0]);
                }
                else
                {
                    return null;
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
            if (destinationType == typeof(string) && context != null)
            {
                dynamic instance = context.Instance;

                string text = value?.ToString();
                if (value != null)
                {
                    ProgramAssignmentNumber nn = (ProgramAssignmentNumber)value;
                    try
                    {
                        InstrumentBase inst = instance.Instrument as InstrumentBase;
                        text = nn.ToString();
                        if (nn < (ProgramAssignmentNumber)0x10000)
                        {
                            if ((int)nn < inst.BaseTimbres.Length)
                                text += " " + inst.BaseTimbres[(int)nn].TimbreName;
                        }
                        else
                        {
                            if ((int)(nn - 0x10000) < inst.CombinedTimbres.Length)
                                text += " " + inst.CombinedTimbres[(int)nn - 0x10000].TimbreName;
                        }
                    }
                    catch { }
                }
                return text;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            List<ProgramAssignmentNumber> list = new List<ProgramAssignmentNumber>();
            list.AddRange(Enum.GetValues(typeof(ProgramAssignmentNumber)).OfType<ProgramAssignmentNumber>());
            return new StandardValuesCollection(list);
        }
    }
}
