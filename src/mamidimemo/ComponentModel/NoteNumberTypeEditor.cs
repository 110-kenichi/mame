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
using zanac.MAmidiMEmo.Midi;

namespace zanac.MAmidiMEmo.ComponentModel
{
    public class NoteNumberTypeEditor : UITypeEditor
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

            Type enumType = value.GetType();
            if (!enumType.IsEnum)
                throw new InvalidOperationException();

            var converter = context.PropertyDescriptor.Converter;
            List<object> vs = null;
            if (converter != null && converter.GetStandardValuesSupported())
            {
                vs = new List<object>();
                var svs = converter.GetStandardValues();
                foreach (var sv in svs)
                    vs.Add(sv);
            }

            foreach (var sv in vs)
            {
                NoteNames nn = (NoteNames)sv;

                EnumItem item = new EnumItem();
                item.Value = sv;

                item.Name = MidiManager.GetNoteName((SevenBitNumber)((byte)nn));
                int index = listBox.Items.Add(item);

                if (nn == (NoteNames)value)
                {
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
