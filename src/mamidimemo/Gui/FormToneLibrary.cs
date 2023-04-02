using FM_SoundConvertor;
using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using zanac.MAmidiMEmo.Properties;

namespace zanac.MAmidiMEmo.Gui.FMEditor
{
    public partial class FormToneLibrary : FormBase
    {
        public event EventHandler SelectedToneChanged;

        /// <summary>
        /// 
        /// </summary>
        public FormToneLibrary()
        {
            InitializeComponent();
        }

        protected override void OnShown(EventArgs e)
        {
            string dir = Settings.Default.ToneLibLastDir;
            if (string.IsNullOrWhiteSpace(dir))
            {
                dir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                dir = Path.Combine(dir, "MAmi");
            }
            try
            {
                Directory.Create(dir);
            }
            catch(Exception ex)
            {
                if (ex.GetType() == typeof(Exception))
                    throw;
                else if (ex.GetType() == typeof(SystemException))
                    throw;

                dir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }

            explorerBrowser1.Navigate(ShellFileSystemFolder.FromFolderPath(dir));

            base.OnShown(e);
        }

        private void listBoxTones_SelectedIndexChanged(object sender, EventArgs e)
        {
           // buttonOK.Enabled = listBoxTones.SelectedItem != null;
           // SelectedToneChanged?.Invoke(this, e);
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void explorerBrowser1_SelectionChanged(object sender, EventArgs e)
        {
            var sis = explorerBrowser1.SelectedItems;
            if (sis.Count != 0)
            {
                CurrentSelectedItem = sis[0].ParsingName;
            }
        }

        private void explorerBrowser1_NavigationComplete(object sender, Microsoft.WindowsAPICodePack.Controls.NavigationCompleteEventArgs e)
        {
            CurrentLocation = e.NewLocation.ParsingName;
        }

        /// <summary>
        /// 
        /// </summary>
        public string CurrentSelectedItem
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public string CurrentLocation
        {
            get;
            private set;
        }
    }
}
