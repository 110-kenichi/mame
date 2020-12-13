using FM_SoundConvertor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace zanac.MAmidiMEmo.Gui.FMEditor
{
    public partial class FormToneSelector : FormBase
    {
        public event EventHandler SelectedToneChanged;

        public Tone SelectedTone
        {
            get
            {
                return (Tone)listBoxTones.SelectedItem;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tones"></param>
        public FormToneSelector(IEnumerable<Tone> tones)
        {
            InitializeComponent();

            foreach (var tone in tones)
                listBoxTones.Items.Add(tone);

            listBoxTones.SelectedItem = null;
            buttonOK.Enabled = false;
        }

        private void listBoxTones_SelectedIndexChanged(object sender, EventArgs e)
        {
            buttonOK.Enabled = listBoxTones.SelectedItem != null;
            SelectedToneChanged?.Invoke(this, e);
        }

        private void listBoxTones_DoubleClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }


    }
}
