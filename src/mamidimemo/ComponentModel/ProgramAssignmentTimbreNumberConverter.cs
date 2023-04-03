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
    public class ProgramAssignmentTimbreNumberConverter : TypeConverter
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
                    ProgramAssignmentTimbreNumber nn = (ProgramAssignmentTimbreNumber)value;
                    try
                    {
                        InstrumentBase inst = instance.Instrument as InstrumentBase;
                        text = nn.ToString();
                        if ((int)nn < inst.BaseTimbres.Length)
                            text += " " + inst.BaseTimbres[(int)nn].TimbreName;
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
            List<ProgramAssignmentTimbreNumber> list = new List<ProgramAssignmentTimbreNumber>();
            list.AddRange(Enum.GetValues(typeof(ProgramAssignmentTimbreNumber)).OfType<ProgramAssignmentTimbreNumber>());
            return new StandardValuesCollection(list);
        }
    }
}
