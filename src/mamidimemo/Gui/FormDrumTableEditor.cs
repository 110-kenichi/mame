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
    public partial class FormDrumTableEditor : FormBase
    {
        public DrumTimbre[] f_DrumData;

        /// <summary>
        /// 
        /// </summary>
        public DrumTimbre[] DrumData
        {
            get
            {
                return f_DrumData;
            }
            set
            {
                f_DrumData = value;

                propertyGrid1.SelectedObjects = null;
                listViewPcmSounds.BeginUpdate();
                try
                {
                    listViewPcmSounds.Items.Clear();
                    for (int i = 0; i < value.Length; i++)
                    {
                        DrumTimbre dt = value[i];

                        var item = listViewPcmSounds.Items.Add(i.ToString() + "(" + dt.KeyName + ")");
                        item.Tag = dt;
                        item.SubItems.Add(dt.TimbreNumber.ToString());
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

        /// <summary>
        /// 
        /// </summary>
        public FormDrumTableEditor()
        {
            InitializeComponent();

        }

        private void listViewPcmSounds_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            List<DrumTimbre> insts = new List<DrumTimbre>();
            foreach (ListViewItem item in listViewPcmSounds.SelectedItems)
                insts.Add((DrumTimbre)item.Tag);
            propertyGrid1.SelectedObjects = insts.ToArray();
         
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            DrumTimbre[] insts = (DrumTimbre[])propertyGrid1.SelectedObjects;
            foreach (var pcm in insts)
            {
                foreach (ListViewItem item in listViewPcmSounds.SelectedItems)
                {
                    if (item.Tag == pcm)
                    {
                        item.SubItems[1].Text = pcm.TimbreNumber.ToString();
                    }
                }
            }
            foreach (ColumnHeader c in listViewPcmSounds.Columns)
                c.Width = -2;
        }

    }
}
