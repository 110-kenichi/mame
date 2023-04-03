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
        /// <summary>
        /// 
        /// </summary>
        public InstrumentBase Instrument
        {
            get;
            set;
        }

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
                updateList();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        private void updateList()
        {
            int ti = 0;
            if(listViewPcmSounds.TopItem != null)
                ti = listViewPcmSounds.TopItem.Index;

            listViewPcmSounds.BeginUpdate();
            try
            {
                listViewPcmSounds.Items.Clear();
                for (int i = 0; i < f_DrumData.Length; i++)
                {
                    DrumTimbre dt = f_DrumData[i];
                    dt.Instrument = Instrument;

                    var item = listViewPcmSounds.Items.Add(i.ToString() + "(" + dt.KeyName + ")");
                    item.Tag = dt;
                    if (dt.TimbreNumber != null)
                    {
                        string text = dt.TimbreNumber.ToString();
                        if (!string.IsNullOrWhiteSpace(dt.TimbreName))
                        {
                            text += " " + dt.TimbreName;
                        }
                        else if (dt.TimbreNumber < (ProgramAssignmentNumber)0x10000)
                        {
                            if ((int)dt.TimbreNumber.Value < Instrument.BaseTimbres.Length)
                                text += " " + Instrument.BaseTimbres[(int)dt.TimbreNumber.Value].TimbreName;
                        }
                        else
                        {
                            if ((int)(dt.TimbreNumber.Value - 0x10000) < Instrument.CombinedTimbres.Length)
                                text += " " + Instrument.CombinedTimbres[(int)dt.TimbreNumber.Value - 0x10000].TimbreName;
                        }
                        item.SubItems.Add(text);
                    }
                }
                foreach (ColumnHeader c in listViewPcmSounds.Columns)
                    c.Width = -2;
                listViewPcmSounds.Items[listViewPcmSounds.Items.Count - 1].EnsureVisible();
                listViewPcmSounds.Items[ti].EnsureVisible();
            }
            finally
            {
                listViewPcmSounds.EndUpdate();
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
            updateList();
        }

    }
}
