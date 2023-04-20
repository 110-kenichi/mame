// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Collections;
using System.Globalization;
using zanac.MAmidiMEmo.Instruments;
using Microsoft.WindowsAPICodePack.Shell;
using System.Xml.Linq;

namespace zanac.MAmidiMEmo.ComponentModel
{
    /// <summary>
    /// </summary>
    public class ExpandableCollectionConverter : CollectionConverter
    {
        /// <summary>
        /// 
        /// </summary>
        public ExpandableCollectionConverter()
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
                        (NumberFormatInfo.InvariantInfo).Length, null));
                    switch (o)
                    {
                        case ProgramAssignmentNumber pan:
                            break;
                        case Object obj:
                            //dynamic dobj = obj;
                            //try
                            //{
                            //    name += " " + dobj.TimbreName;
                            //}
                            //catch
                            //{
                            //    try
                            //    {
                            //        name += " " + dobj.Memo;
                            //    }
                            //    catch
                            //    {
                            //    }
                            //}
                            break;
                    }
                    CollectionPropertyDescriptor cpd = new CollectionPropertyDescriptor(context, type, name, o as IDisplayName, o.GetType(), i);
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

            private IDisplayName dname;

            public override string Name
            {
                get
                {
                    if (dname != null && dname.DisplayName != null)
                        return base.Name + dname.DisplayName;
                    return base.Name;
                }
            }

            public override string DisplayName
            {
                get
                {
                    if (dname != null && dname.DisplayName != null)
                        return base.Name + dname.DisplayName;
                    return base.DisplayName;
                }
            }

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
            public CollectionPropertyDescriptor(ITypeDescriptorContext context, Type componentType, string name, IDisplayName dname, Type elementType, int index)
                : base(componentType, name, elementType)
            {
                this.context = context;
                this.index = index;
                this.dname = dname;

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
                {
                    try
                    {
                        c[index] = value;
                    }
                    catch (Exception ex)
                    {
                        if (ex.GetType() == typeof(Exception))
                            throw;
                        else if (ex.GetType() == typeof(SystemException))
                            throw;

                    }
                }
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
