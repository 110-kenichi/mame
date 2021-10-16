using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using MetroFramework.Forms;
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
using System.Windows.Forms.PropertyGridInternal;
using zanac.MAmidiMEmo.ComponentModel;
using zanac.MAmidiMEmo.Instruments;
using zanac.MAmidiMEmo.Midi;
using zanac.MAmidiMEmo.Properties;

namespace zanac.MAmidiMEmo.Gui
{
    public partial class FormProp : FormBase
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
            ClientSize = Settings.Default.PWinSize;
        }

        protected override void OnClosed(EventArgs e)
        {
            Settings.Default.PWinSize = ClientSize;
            base.OnClosed(e);
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
            propertyGrid.RefreshTabs(PropertyTabScope.Component);

            //HACK:
            var toolStrip = propertyGrid.Controls.OfType<ToolStrip>().FirstOrDefault();
            toolStrip.Items.Add(toolStripButtonPopup);

            setTitle();

            toolStripComboBoxCh.SelectedIndex = 0;
            toolStripComboBoxProg.SelectedIndex = 0;
            if (timbres != null)
            {
                toolStripComboBoxProg.Enabled = false;
                toolStripButtonPopup.Enabled = false;
            }
            toolStripComboBoxCC.SelectedIndex = 0;

            InstrumentManager.InstrumentChanged += InstrumentManager_InstrumentChanged;
            InstrumentManager.InstrumentRemoved += InstrumentManager_InstrumentRemoved;

            pianoControl1.NoteOn += PianoControl1_NoteOn;
            pianoControl1.NoteOff += PianoControl1_NoteOff;
            pianoControl1.EntryDataChanged += PianoControl1_EntryDataChanged;
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
            for (int i = 0; i < 16; i++)
                pianoControl1.SetReceiveChannel(i, false);
            pianoControl1.SetReceiveChannel(toolStripComboBoxCh.SelectedIndex, true);
        }

        private void PianoControl1_NoteOn(object sender, TaggedNoteOnEvent e)
        {
            if (toolStripComboBoxProg.SelectedIndex != 0)
            {
                //Program change
                var pe = new ProgramChangeEvent((SevenBitNumber)(toolStripComboBoxProg.SelectedIndex - 1));
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


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PianoControl1_EntryDataChanged(object sender, EventArgs e)
        {
            var cce = new ControlChangeEvent
                ((SevenBitNumber)toolStripComboBoxCC.SelectedIndex,
                (SevenBitNumber)pianoControl1.EntryDataValue);
            cce.Channel = (FourBitNumber)(toolStripComboBoxCh.SelectedIndex);

            foreach (var i in instruments)
                i.NotifyMidiEvent(cce);
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
