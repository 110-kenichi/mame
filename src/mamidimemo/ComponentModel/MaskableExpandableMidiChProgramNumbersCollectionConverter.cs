// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Collections;
using System.Globalization;
using zanac.MAmidiMEmo.Instruments;
using System.Runtime.Remoting.Channels;
using System.Xml.Linq;
using System.Drawing.Design;
using System.Windows.Forms.Design;
using zanac.MAmidiMEmo.Gui;
using Melanchall.DryWetMidi.Common;
using System.Windows.Forms;
using zanac.MAmidiMEmo.Midi;

namespace zanac.MAmidiMEmo.ComponentModel
{
    /// <summary>
    /// </summary>
    public class MaskableExpandableMidiChProgramNumbersCollectionConverter : CollectionConverter
    {
        /// <summary>
        /// 
        /// </summary>
        public MaskableExpandableMidiChProgramNumbersCollectionConverter()
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
                        "[{0}]({1}ch)", i, (i + 1).ToString("d" + list.Count.ToString
                        (NumberFormatInfo.InvariantInfo).Length, null));
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

            private int index;

            private uint maskValue;

            private object defaultValue;

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

            public override object GetEditor(Type editorBaseType)
            {
                if (maskValue != 0)
                    return new ProgramNumberUITypeEditor(context);

                return base.GetEditor(editorBaseType);
            }

            private TypeConverter converter;

            public override TypeConverter Converter
            {
                get
                {
                    return converter;
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
                this.converter = new ProgramAssignmentNumberTypeConverter((InstrumentBase)context.Instance);

                MaskAttribute att = (MaskAttribute)context.PropertyDescriptor.Attributes[typeof(MaskAttribute)];
                maskValue = att.MaskValue;

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
                    switch (value)
                    {
                        case byte n:
                            {
                                c[index] = (byte)(n & maskValue);
                                break;
                            }
                        case short n:
                            {
                                c[index] = (short)(n & maskValue);
                                break;
                            }
                        case ushort n:
                            {
                                c[index] = (ushort)(n & maskValue);
                                break;
                            }
                        case uint n:
                            {
                                c[index] = (uint)(n & maskValue);
                                break;
                            }
                        case int n:
                            {
                                c[index] = (int)(n & maskValue);
                                break;
                            }
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
                    return byte.Parse(((string)value).Split(' ')[0]);
                }

                public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
                {
                    if (destinationType == typeof(string))
                    {
                        int nn = 0;
                        string name = null;
                        if (value is string)
                        {
                            int.TryParse(((string)value).Split(' ')[0], out nn);
                            name = nn.ToString();
                        }
                        else if (value is byte)
                        {
                            nn = (byte)value;
                            name = nn.ToString();
                        }

                        int pn = (int)inst.ProgramAssignments[nn];

                        if ((pn & 0xffff0000) != 0)
                        {
                            int ptidx = pn & 0xffff;
                            if (ptidx >= inst.CombinedTimbres.Length)
                                ptidx = inst.CombinedTimbres.Length - 1;
                            var pts = inst.CombinedTimbres[ptidx];
                            return name += " " + pts.DisplayName;
                        }

                        int btidx = pn & 0xffff;
                        if (btidx >= inst.BaseTimbres.Length)
                            btidx = inst.BaseTimbres.Length - 1;
                        return name += " " + inst.BaseTimbres[btidx].DisplayName;
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

                    for (int i = 0; i < 128; i++)
                    {
                        String name = i.ToString();

                        int pn = (int)inst.ProgramAssignments[i];

                        if ((pn & 0xffff0000) != 0)
                        {
                            int ptidx = pn & 0xffff;
                            if (ptidx >= inst.CombinedTimbres.Length)
                                ptidx = inst.CombinedTimbres.Length - 1;
                            var pts = inst.CombinedTimbres[ptidx];
                            names.Add(name += " " + pts.DisplayName);
                        }
                        else
                        {
                            int btidx = pn & 0xffff;
                            if (btidx >= inst.BaseTimbres.Length)
                                btidx = inst.BaseTimbres.Length - 1;
                            names.Add(name += " " + inst.BaseTimbres[btidx].DisplayName);
                        }
                    }
                    return new StandardValuesCollection(names);
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public class ProgramNumberUITypeEditor : UITypeEditor
        {
            private InstrumentBase inst;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="type"></param>
            public ProgramNumberUITypeEditor(ITypeDescriptorContext context)
            {
                this.inst = (InstrumentBase)context.Instance;
            }

            private IWindowsFormsEditorService _editorService;
            private bool _cancel;

            public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
            {
                return UITypeEditorEditStyle.DropDown;
            }

            public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                _editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
                ListBox listBox = new ListBox();
                listBox.DisplayMember = "Name"; // EnumItem 'Name' property
                listBox.IntegralHeight = true;
                listBox.SelectionMode = SelectionMode.One;
                listBox.MouseClick += OnListBoxMouseClick;
                listBox.KeyDown += OnListBoxKeyDown;
                listBox.PreviewKeyDown += OnListBoxPreviewKeyDown;

                var converter = context.PropertyDescriptor.Converter;
                if (converter != null && converter.GetStandardValuesSupported())
                {
                    var svs = converter.GetStandardValues();
                    foreach (var sv in svs)
                    {
                        int index = listBox.Items.Add(sv);

                        if(index == (byte)value)
                            listBox.SetSelected(index, true);
                    }
                }

                _cancel = false;
                listBox.Height = 200;
                _editorService.DropDownControl(listBox);
                if (_cancel || listBox.SelectedIndices.Count == 0)
                    return value;

                return (byte)listBox.SelectedIndex;
            }

            private void OnListBoxPreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
            {
                if (e.KeyCode == Keys.Escape)
                {
                    _cancel = true;
                    _editorService.CloseDropDown();
                }
            }

            private void OnListBoxMouseClick(object sender, MouseEventArgs e)
            {
                int index = ((ListBox)sender).IndexFromPoint(e.Location);
                if (index >= 0)
                {
                    _editorService.CloseDropDown();
                }
            }

            private void OnListBoxKeyDown(object sender, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    _editorService.CloseDropDown();
                }
            }

        }
    }

}
