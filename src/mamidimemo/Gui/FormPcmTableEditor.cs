// copyright-holders:K.Ito
using MetroFramework.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using zanac.MAmidiMEmo.Instruments;

namespace zanac.MAmidiMEmo.Gui
{
    public partial class FormPcmTableEditor : FormBase
    {
        public PcmTimbreBase[] f_PcmData;

        /// <summary>
        /// 
        /// </summary>
        public PcmTimbreBase[] PcmData
        {
            get
            {
                return f_PcmData;
            }
            set
            {
                f_PcmData = value;

                propertyGrid1.SelectedObjects = null;
                listViewPcmSounds.BeginUpdate();
                try
                {
                    listViewPcmSounds.Items.Clear();
                    for (int i = 0; i < value.Length; i++)
                    {
                        PcmTimbreBase pcm = value[i];

                        var item = listViewPcmSounds.Items.Add(i.ToString() + "(" + pcm.KeyName + ")");
                        item.Tag = pcm;
                        item.SubItems.Add(pcm.TimbreName);
                    }
                    foreach (ColumnHeader c in listViewPcmSounds.Columns)
                        c.Width = -2;
                }
                finally
                {
                    listViewPcmSounds.EndUpdate();
                }
            }
        }

        private string f_FileDialogFilter;

        /// <summary>
        /// 
        /// </summary>
        public string FileDialogFilter
        {
            get
            {
                return f_FileDialogFilter;
            }
            set
            {
                f_FileDialogFilter = value;

                supportedExtensions.Clear();

                var regex = new Regex(@"(?<Name>[^|]*)\|(?<Extension>[^|]*)\|?");
                var matches = regex.Matches(value);
                foreach (Match match in matches)
                {
                    //match.Groups["Name"].Value, match.Groups["Extension"].Value
                    foreach (string ext in match.Groups["Extension"].Value.Split(';'))
                        supportedExtensions.Add(Path.GetExtension(ext));
                }
            }
        }

        private List<string> supportedExtensions = new List<string>();

        /// <summary>
        /// 
        /// </summary>
        public FormPcmTableEditor()
        {
            InitializeComponent();

            System.Windows.Forms.Application.Idle += Application_Idle;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            System.Windows.Forms.Application.Idle -= Application_Idle;

            base.OnClosing(e);
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            var fi = listViewPcmSounds.FocusedItem;
            if (fi == null)
                return;

            openFileDialog1.Filter = FileDialogFilter;
            var result = openFileDialog1.ShowDialog();
            if (result != DialogResult.OK)
                return;

            var fn = openFileDialog1.FileName;
            loadPcmData(fi, fn);
        }

        private void loadPcmData(ListViewItem fi, string fn)
        {
            byte[] data = null;
            try
            {
                data = File.ReadAllBytes(fn);
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(Exception))
                    throw;
                else if (ex.GetType() == typeof(SystemException))
                    throw;


                MessageBox.Show(ex.ToString());
            }
            if (data != null)
            {
                PcmTimbreBase pcm = (PcmTimbreBase)fi.Tag;
                pcm.PcmData = data;
                pcm.TimbreName = Path.GetFileNameWithoutExtension(fn);

                fi.SubItems[1].Text = pcm.TimbreName;
            }
            propertyGrid1.Refresh();
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < listViewPcmSounds.SelectedItems.Count; i++)
            {
                PcmTimbreBase snd = (PcmTimbreBase)listViewPcmSounds.SelectedItems[i].Tag;
                snd.PcmData = null;
                snd.TimbreName = null;

                listViewPcmSounds.SelectedItems[i].SubItems[1].Text = null;
            }
            propertyGrid1.Refresh();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="InvalidDataException"></exception>
        private void Application_Idle(object sender, EventArgs e)
        {
            if (listViewPcmSoundsItemSelectionChanged)
            {
                listViewPcmSoundsItemSelectionChanged = false;

                List<PcmTimbreBase> insts = new List<PcmTimbreBase>();
                foreach (ListViewItem item in listViewPcmSounds.SelectedItems)
                    insts.Add((PcmTimbreBase)item.Tag);
                propertyGrid1.SelectedObjects = insts.ToArray();
            }
        }

        private bool listViewPcmSoundsItemSelectionChanged;

        private void listViewPcmSounds_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            buttonAdd.Enabled = listViewPcmSounds.FocusedItem != null ? true : false;
            buttonDelete.Enabled = listViewPcmSounds.SelectedItems.Count != 0 ? true : false;

            listViewPcmSoundsItemSelectionChanged = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            PcmTimbreBase[] insts = (PcmTimbreBase[])propertyGrid1.SelectedObjects;
            foreach (var pcm in insts)
            {
                foreach (ListViewItem item in listViewPcmSounds.SelectedItems)
                {
                    if (item.Tag == pcm)
                    {
                        item.SubItems[1].Text = pcm.TimbreName;
                    }
                }
            }
            foreach (ColumnHeader c in listViewPcmSounds.Columns)
                c.Width = -2;
        }

        private void listViewPcmSounds_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] fileNames = (string[])e.Data.GetData(DataFormats.FileDrop, false);
                foreach (string fn in fileNames)
                {
                    foreach (string sext in supportedExtensions)
                    {
                        if (string.Equals(Path.GetExtension(fn), sext, StringComparison.OrdinalIgnoreCase))
                        {
                            e.Effect = DragDropEffects.Copy;
                            return;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listViewPcmSounds_DragDrop(object sender, DragEventArgs e)
        {
            var pt = listViewPcmSounds.PointToClient(new Point(e.X, e.Y));
            var item = listViewPcmSounds.GetItemAt(pt.X, pt.Y);
            if (item != null)
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    string[] fileNames = (string[])e.Data.GetData(DataFormats.FileDrop, false);
                    foreach (string fn in fileNames)
                    {
                        foreach (string sext in supportedExtensions)
                        {
                            if (string.Equals(Path.GetExtension(fn), sext, StringComparison.OrdinalIgnoreCase))
                            {
                                loadPcmData(item, fn);
                                if (item.Index >= listViewPcmSounds.Items.Count - 1)
                                    return;

                                item = listViewPcmSounds.Items[item.Index + 1];
                            }
                        }
                    }
                }
            }
            propertyGrid1.Refresh();
        }
    }
}
