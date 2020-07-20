using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using zanac.MAmidiMEmo.Instruments;
using zanac.MAmidiMEmo.Midi;

namespace zanac.MAmidiMEmo.Gui
{
    public partial class FormProp : Form
    {
        private List<InstrumentBase> instruments;

        private List<TimbreBase> timbres;

        public int TimbreNo
        {
            get;
            private set;
        } = 0;

        /// <summary>
        /// 
        /// </summary>
        public FormProp(InstrumentBase[] insts) : this(insts, null)
        {

        }


        /// <summary>
        /// 
        /// </summary>
        public FormProp(InstrumentBase[] insts, TimbreBase[] timbres)
        {
            InitializeComponent();

            instruments = new List<InstrumentBase>(insts);
            if (timbres != null)
            {
                this.timbres = new List<TimbreBase>(timbres);
                propertyGrid.SelectedObjects = this.timbres.ToArray();

                for (int i = 0; i < instruments[0].BaseTimbres.Length; i++)
                {
                    if (instruments[0].BaseTimbres[i] == timbres[0])
                    {
                        TimbreNo = i + 1;
                        break;
                    }
                }
            }
            else
            {
                propertyGrid.SelectedObjects = instruments.ToArray();
            }

            setTitle();

            toolStripComboBox1.SelectedIndex = 0;
            toolStripComboBox2.SelectedIndex = 0;
            if (timbres != null)
            {
                toolStripComboBox2.Enabled = false;
                toolStripButtonPopup.Enabled = false;
            }

            InstrumentManager.InstrumentChanged += InstrumentManager_InstrumentChanged;
            InstrumentManager.InstrumentRemoved += InstrumentManager_InstrumentRemoved;

            pianoControl1.NoteOn += PianoControl1_NoteOn;
            pianoControl1.NoteOff += PianoControl1_NoteOff;
        }

        private void setTitle()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var i in instruments)
            {
                if (sb.Length != 0)
                    sb.Append(", ");
                sb.Append(i.Name + "(" + i.UnitNumber + ")");
            }
            if (timbres != null)
                sb.Append(" - Instrument " + TimbreNo);

            this.Text = sb.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InstrumentManager_InstrumentRemoved(object sender, EventArgs e)
        {
            if (!IsHandleCreated || IsDisposed)
                return;

            List<InstrumentBase> insts = new List<InstrumentBase>();
            foreach (var inst in InstrumentManager.GetAllInstruments())
            {
                foreach (var i in instruments)
                {
                    if (i.DeviceID == inst.DeviceID && i.UnitNumber == inst.UnitNumber)
                        insts.Add(inst);
                }
            }

            instruments = insts;
            propertyGrid.SelectedObjects = insts.ToArray();
            setTitle();
            if (insts.Count == 0)
                Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InstrumentManager_InstrumentChanged(object sender, EventArgs e)
        {
            if (!IsHandleCreated || IsDisposed)
                return;

            List<InstrumentBase> insts = new List<InstrumentBase>();
            foreach (var inst in InstrumentManager.GetAllInstruments())
            {
                foreach (var i in instruments)
                {
                    if (i.DeviceID == inst.DeviceID && i.UnitNumber == inst.UnitNumber)
                        insts.Add(inst);
                }
            }

            instruments = insts;
            propertyGrid.SelectedObjects = insts.ToArray();
            setTitle();
            if (insts.Count == 0)
                Close();
        }

        private void toolStripButtonCat_Click(object sender, EventArgs e)
        {
            propertyGrid.PropertySort = PropertySort.Categorized;
            toolStripButtonCat.Checked = true;
            toolStripButtonA2Z.Checked = false;
        }

        private void toolStripButtonA2Z_Click(object sender, EventArgs e)
        {
            propertyGrid.PropertySort = PropertySort.Alphabetical;
            toolStripButtonCat.Checked = false;
            toolStripButtonA2Z.Checked = true;
        }

        private TimbreBase[] findTimbre(GridItem item)
        {
            List<TimbreBase> il = new List<TimbreBase>();
            if (item == null)
                return il.ToArray();

            var instance = item.GetType().GetProperty("Instance").GetValue(item, null);
            if (instance.GetType() == typeof(object[]))
            {
                var objs = instance as object[];
                foreach (var o in objs)
                {
                    var inst = o as TimbreBase;
                    if (inst != null)
                        il.Add(inst);
                }
            }
            {
                var inst = instance as TimbreBase;
                if (inst != null)
                    il.Add(inst);
            }
            if (il.Count != 0)
                return il.ToArray();

            return findTimbre(item.Parent);
        }

        private void toolStripButtonPopup_Click(object sender, EventArgs e)
        {
            TimbreBase[] timbres = findTimbre(propertyGrid.SelectedGridItem);

            if (timbres == null || timbres.Length == 0)
            {
                //FormProp fp = new FormProp(instruments.ToArray(), null);
                //fp.Show(Owner);
            }
            else
            {
                FormProp fp = new FormProp(instruments.ToArray(), timbres);
                fp.Show(this);
            }
        }

        private void contextMenuStripProp_Click(object sender, EventArgs e)
        {
            propertyGrid.ResetSelectedProperty();
            propertyGrid.Refresh();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.W | Keys.Control))
            {
                Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        //protected override void OnClosing(CancelEventArgs e)
        //{
        //    var r = MessageBox.Show(this, "Are you sure you want to close " + Text + " ?", "Qeuestion", MessageBoxButtons.OKCancel);
        //    if (r == DialogResult.Cancel)
        //        e.Cancel = true;
        //    base.OnClosing(e);
        //}

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            pianoControl1.SetMouseChannel(toolStripComboBox1.SelectedIndex);
            for (int i = 0; i < 16; i++)
                pianoControl1.SetReceiveChannel(i, false);
            pianoControl1.SetReceiveChannel(toolStripComboBox1.SelectedIndex, true);
        }

        private void PianoControl1_NoteOn(object sender, TaggedNoteOnEvent e)
        {
            if (toolStripComboBox2.SelectedIndex != 0)
            {
                //Program change
                var pe = new ProgramChangeEvent((SevenBitNumber)(toolStripComboBox2.SelectedIndex - 1));
                foreach (var i in instruments)
                    i.NotifyMidiEvent(pe);
                foreach (var i in instruments)
                    i.NotifyMidiEvent(e);
            }
            else
            {
                if (timbres != null)
                {
                    for (int i = 0; i < instruments.Count; i++)
                    {
                        e.Tag = new NoteOnTimbreInfo(timbres[i], TimbreNo);
                        instruments[i].NotifyMidiEvent(e);
                    }
                }
                else
                {
                    foreach (var i in instruments)
                        i.NotifyMidiEvent(e);
                }
            }
        }

        private void PianoControl1_NoteOff(object sender, NoteOffEvent e)
        {
            foreach (var i in instruments)
                i.NotifyMidiEvent(e);
        }

        private void propertyGrid_SelectedGridItemChanged(object sender, SelectedGridItemChangedEventArgs e)
        {
            if (timbres == null)
            {
                TimbreBase[] timbres = findTimbre(propertyGrid.SelectedGridItem);
                toolStripButtonPopup.Enabled = !(timbres != null && timbres.Length == 0);
            }
        }
    }
}
