using Melanchall.DryWetMidi.Common;
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
using zanac.MAmidiMEmo.Instruments;
using zanac.MAmidiMEmo.Midi;

namespace zanac.MAmidiMEmo.ComponentModel
{
    public class ProgramAssignmentNumberTypeEditor : UITypeEditor
    {
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
            List<object> vs = null;
            if (converter != null && converter.GetStandardValuesSupported())
            {
                vs = new List<object>();
                var svs = converter.GetStandardValues();
                foreach (var sv in svs)
                    vs.Add(sv);
            }
            dynamic instance = context.Instance;

            foreach (var sv in vs)
            {
                string text = "";
                InstrumentBase inst = null;
                try
                {
                    if (instance is Array)
                        inst = ((dynamic)((Array)instance).GetValue(0)).Instrument as InstrumentBase;
                    else
                        inst = instance.Instrument as InstrumentBase;
                }
                catch
                {
                    break;
                }
                if (inst == null)
                    break;

                if (context.PropertyDescriptor.PropertyType == typeof(ProgramAssignmentNumber?))
                {
                    if (sv != null)
                    {
                        ProgramAssignmentNumber nn = (ProgramAssignmentNumber)sv;
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

                        EnumItem item = new EnumItem();
                        item.Value = sv;

                        item.Name = text;
                        int index = listBox.Items.Add(item);

                        if (nn == (ProgramAssignmentNumber?)value)
                            listBox.SetSelected(index, true);
                    }
                }
                else if (context.PropertyDescriptor.PropertyType == typeof(ProgramAssignmentNumber))
                {
                    ProgramAssignmentNumber nn = (ProgramAssignmentNumber)sv;
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

                    EnumItem item = new EnumItem();
                    item.Value = sv;

                    item.Name = text;
                    int index = listBox.Items.Add(item);

                    if (nn == (ProgramAssignmentNumber)value)
                        listBox.SetSelected(index, true);
                }
                else if (context.PropertyDescriptor.PropertyType == typeof(ProgramAssignmentTimbreNumber))
                {
                    ProgramAssignmentTimbreNumber nn = (ProgramAssignmentTimbreNumber)sv;
                    text = nn.ToString();
                    if (inst != null)
                    {
                        if ((int)nn < inst.BaseTimbres.Length)
                            text += " " + inst.BaseTimbres[(int)nn].TimbreName;
                    }
                    EnumItem item = new EnumItem();
                    item.Value = sv;

                    item.Name = text;
                    int index = listBox.Items.Add(item);

                    if (nn == (ProgramAssignmentTimbreNumber)value)
                        listBox.SetSelected(index, true);
                }
            }

            _cancel = false;
            listBox.Height = 200;
            _editorService.DropDownControl(listBox);
            if (_cancel || listBox.SelectedIndices.Count == 0)
                return value;

            return ((EnumItem)listBox.SelectedItem).Value;
        }

        private class EnumItem
        {
            public object Value { get; set; }
            public string Name { get; set; }
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
