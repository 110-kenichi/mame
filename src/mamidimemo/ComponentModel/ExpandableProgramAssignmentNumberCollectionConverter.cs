// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Collections;
using System.Globalization;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Common;
using zanac.MAmidiMEmo.Instruments;
using System.Xml.Linq;
using zanac.MAmidiMEmo.Instruments.Chips;
using zanac.MAmidiMEmo.Midi;

namespace zanac.MAmidiMEmo.ComponentModel
{
    /// <summary>
    /// </summary>
    public class ExpandableProgramAssignmentNumberCollectionConverter : CollectionConverter
    {
        /// <summary>
        /// 
        /// </summary>
        public ExpandableProgramAssignmentNumberCollectionConverter()
        {
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                ICollection c = value as ICollection;
                if (c != null)
                    return context.PropertyDescriptor.DisplayName + "[" + c.Count + "]";
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            InstrumentBase inst = (InstrumentBase)context.Instance;
            PropertyDescriptor[] array = null;
            ICollection list = value as ICollection;
            if (list != null)
            {
                array = new PropertyDescriptor[list.Count];
                Type type = typeof(ICollection);
                int i = 0;
                foreach (ProgramAssignmentNumber o in list)
                {
                    string name = string.Format(CultureInfo.InvariantCulture,
                        "[{0}]", i.ToString("d" + list.Count.ToString
                        (NumberFormatInfo.InvariantInfo).Length, null));
                    if ((int)o < inst.BaseTimbres.Length)
                        name += " " + inst.BaseTimbres[(int)o].DisplayName;
                    else if ((int)o >= (int)ProgramAssignmentNumber.CombinedTimbre0 && ((int)o - (int)ProgramAssignmentNumber.CombinedTimbre0) < inst.CombinedTimbres.Length)
                        name += " " + inst.CombinedTimbres[(int)o - (int)ProgramAssignmentNumber.CombinedTimbre0].DisplayName;
                    CollectionPropertyDescriptor cpd = new CollectionPropertyDescriptor(context, type, name, o.GetType(), i, o);
                    array[i] = cpd;
                    i++;
                }
            }
            return new PropertyDescriptorCollection(array);
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        private class CollectionPropertyDescriptor : SimplePropertyDescriptor
        {
            private object value;

            private ITypeDescriptorContext context;

            private object defaultValue;

            private int index;

            private ProgramAssignmentNumberTypeConverter converter;

            public override bool IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            public override AttributeCollection Attributes
            {
                get
                {
                    var attrs = base.Attributes;
                    List<Attribute> list = new List<Attribute>();
                    foreach (Attribute attr in attrs)
                        list.Add(attr);

                    list.Add(new DefaultValueAttribute(defaultValue));
                    AttributeCollection ac = new AttributeCollection(list.ToArray());
                    return ac;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="context"></param>
            /// <param name="componentType"></param>
            /// <param name="name"></param>
            /// <param name="elementType"></param>
            /// <param name="index"></param>
            public CollectionPropertyDescriptor(ITypeDescriptorContext context, Type componentType, string name, Type elementType, int index, object o)
                : base(componentType, name, elementType)
            {
                this.context = context;
                this.index = index;
                this.value = o;
                InstrumentBase inst = (InstrumentBase)context.Instance;
                converter = new ProgramAssignmentNumberTypeConverter(inst);

                CollectionDefaultValueAttribute datt = (CollectionDefaultValueAttribute)context.PropertyDescriptor.Attributes[typeof(CollectionDefaultValueAttribute)];
                if (datt != null)
                    defaultValue = datt.DefaultValue;
            }

            public override object GetValue(object component)
            {
                ICollection c = component as ICollection;
                if (c != null)
                {
                    if (c.Count > index)
                    {
                        int i = 0;
                        foreach (object o in c)
                        {
                            if (i == index)
                            {
                                return o;
                            }
                            i++;
                        }
                    }
                }
                return null;
            }

            public override TypeConverter Converter
            {
                get
                {
                    return converter;
                }
            }

            public override void SetValue(object component, object value)
            {
                IList c = component as IList;
                if (c != null)
                    c[index] = value;
            }

            public override string Description
            {
                get
                {
                    return context.PropertyDescriptor.Description;
                }
            }

        }


        private class ProgramAssignmentNumberTypeConverter : TypeConverter
        {
            private InstrumentBase inst;

            public ProgramAssignmentNumberTypeConverter(InstrumentBase inst)
            {
                this.inst = inst;
            }

            public override bool CanConvertFrom(ITypeDescriptorContext context, Type srcType)
            {
                return srcType == typeof(string);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                return Enum.Parse(typeof(ProgramAssignmentNumber), ((string)value).Split(' ')[0]);
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                if (destinationType == typeof(string))
                {
                    if (value is string)
                    {
                        ProgramAssignmentNumber nn = (ProgramAssignmentNumber)Enum.Parse(typeof(ProgramAssignmentNumber), ((string)value).Split(' ')[0]);
                        string name = nn.ToString();
                        if ((int)nn < inst.BaseTimbres.Length)
                        {
                            name += " " + inst.BaseTimbres[(int)nn].DisplayName;
                        }else if ((int)nn >= (int)ProgramAssignmentNumber.CombinedTimbre0 && ((int)nn - (int)ProgramAssignmentNumber.CombinedTimbre0) < inst.CombinedTimbres.Length)
                        {
                            name += " " + inst.CombinedTimbres[(int)nn - (int)ProgramAssignmentNumber.CombinedTimbre0].DisplayName;
                        }
                        return name;
                    }else if (value is ProgramAssignmentNumber)
                    {
                        ProgramAssignmentNumber nn = (ProgramAssignmentNumber)value;
                        string name = nn.ToString();
                        if ((int)nn < inst.BaseTimbres.Length)
                        {
                            name += " " + inst.BaseTimbres[(int)nn].DisplayName;
                        }
                        else if ((int)nn >= (int)ProgramAssignmentNumber.CombinedTimbre0 && ((int)nn - (int)ProgramAssignmentNumber.CombinedTimbre0) < inst.CombinedTimbres.Length)
                        {
                            name += " " + inst.CombinedTimbres[(int)nn - (int)ProgramAssignmentNumber.CombinedTimbre0].DisplayName;
                        }
                        return name;
                    }
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }

            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
            {
                return true;
            }

            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                List<string> names = new List<string>();
                for (int ci = 0; ci < inst.BaseTimbres.Length; ci++)
                {
                    string name = Enum.GetName(typeof(ProgramAssignmentNumber), ci);
                    if (ci < inst.BaseTimbres.Length)
                        name += " " + inst.BaseTimbres[ci].DisplayName;
                    names.Add(name);
                }
                for (int ci = 0; ci < inst.CombinedTimbres.Length; ci++)
                {
                    string name = Enum.GetName(typeof(ProgramAssignmentNumber), (int)(ProgramAssignmentNumber.CombinedTimbre0) + ci);
                    if (ci < inst.CombinedTimbres.Length)
                        name += " " + inst.CombinedTimbres[ci].DisplayName;
                    names.Add(name);
                }
                return new StandardValuesCollection(names);
            }
        }

    }
}
