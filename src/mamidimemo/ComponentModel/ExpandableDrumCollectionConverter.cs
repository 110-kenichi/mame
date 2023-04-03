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

namespace zanac.MAmidiMEmo.ComponentModel
{
    /// <summary>
    /// </summary>
    public class ExpandableDrumCollectionConverter : CollectionConverter
    {
        /// <summary>
        /// 
        /// </summary>
        public ExpandableDrumCollectionConverter()
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
            PropertyDescriptor[] array = null;
            ICollection list = value as ICollection;
            if (list != null)
            {
                array = new PropertyDescriptor[list.Count];
                Type type = typeof(ICollection);
                int i = 0;
                foreach (object o in list)
                {
                    string name = string.Format(CultureInfo.InvariantCulture,
                        "[{0}]", i.ToString("d" + list.Count.ToString
                        (NumberFormatInfo.InvariantInfo).Length, null) +
                        ":" + Midi.MidiManager.GetNoteName((SevenBitNumber)i));
                    switch (o)
                    {
                        case TimbreBase tim:
                            name += " " + tim.TimbreName;
                            break;
                        case DrumTimbre dtim:
                            name += " " + dtim.TimbreName;
                            break;
                    }
                    CollectionPropertyDescriptor cpd = new CollectionPropertyDescriptor(context, type, name, o.GetType(), i);
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

            private ITypeDescriptorContext context;

            private object defaultValue;

            private int index;

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
            public CollectionPropertyDescriptor(ITypeDescriptorContext context, Type componentType, string name, Type elementType, int index)
                : base(componentType, name, elementType)
            {
                this.context = context;
                this.index = index;

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
                                return o;
                            i++;
                        }
                    }
                }
                return null;
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
    }
}
