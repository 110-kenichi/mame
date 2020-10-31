// copyright-holders:K.Ito
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
using zanac.MAmidiMEmo.Instruments;
using zanac.MAmidiMEmo.Midi;
using zanac.MAmidiMEmo.Properties;

namespace zanac.MAmidiMEmo.Gui.FMEditor
{
    public partial class FormFmEditor : FormBase
    {
        private Dictionary<String, RegisterContainerBase> controls = new Dictionary<string, RegisterContainerBase>();

        public InstrumentBase Instrument
        {
            get;
            private set;
        }

        public TimbreBase Timbre
        {
            get;
            private set;
        }

        public int TimbreNo
        {
            get;
            private set;
        } = 0;


        /// <summary>
        /// 
        /// </summary>
        public FormFmEditor()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        public FormFmEditor(InstrumentBase inst, TimbreBase timbre)
        {
            InitializeComponent();

            for (int nn = 0; nn < 128; nn++)
            {
                toolStripComboBoxNote.Items.Add(MidiManager.GetNoteName((SevenBitNumber)nn) + "(" + nn + ")");
                toolStripComboBoxVelo.Items.Add(nn);
            }

            toolStripComboBoxNote.SelectedIndex = 60;
            toolStripComboBoxVelo.SelectedIndex = 127;
            toolStripComboBoxGate.SelectedIndex = 0;
            toolStripComboBoxCh.SelectedIndex = 0;
            toolStripComboBoxCC.SelectedIndex = 0;

            Settings.Default.SettingsLoaded += Default_SettingsLoaded;
            toolStripButtonPlay.Checked = Settings.Default.FmPlayOnEdit;
            toolStripComboBoxVelo.SelectedIndex = Settings.Default.FmVelocity;
            toolStripComboBoxGate.SelectedIndex = Settings.Default.FmGateTime;
            toolStripComboBoxNote.SelectedIndex = Settings.Default.FmNote;

            this.Timbre = timbre;
            this.Instrument = inst;

            for (int i = 0; i < Instrument.BaseTimbres.Length; i++)
            {
                if (Instrument.BaseTimbres[i] == timbre)
                {
                    TimbreNo = i + 1;
                    break;
                }
            }

            setTitle();

            InstrumentManager.InstrumentChanged += InstrumentManager_InstrumentChanged;
            InstrumentManager.InstrumentRemoved += InstrumentManager_InstrumentRemoved;

            pianoControl1.NoteOn += PianoControl1_NoteOn;
            pianoControl1.NoteOff += PianoControl1_NoteOff;
            pianoControl1.EntryDataChanged += PianoControl1_EntryDataChanged;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            Settings.Default.FmPlayOnEdit = toolStripButtonPlay.Checked;
            Settings.Default.FmVelocity = toolStripComboBoxVelo.SelectedIndex;
            Settings.Default.FmGateTime = toolStripComboBoxGate.SelectedIndex;
            Settings.Default.FmNote = toolStripComboBoxNote.SelectedIndex;
            base.OnClosing(e);
        }


        private void Default_SettingsLoaded(object sender, System.Configuration.SettingsLoadedEventArgs e)
        {
            toolStripButtonPlay.Checked = Settings.Default.FmPlayOnEdit;
            toolStripComboBoxVelo.SelectedIndex = Settings.Default.FmVelocity;
            toolStripComboBoxGate.SelectedIndex = Settings.Default.FmGateTime;
            toolStripComboBoxNote.SelectedIndex = Settings.Default.FmNote;
        }

        private void setTitle()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(Instrument.Name + "(" + Instrument.UnitNumber + ")");
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

            foreach (var inst in InstrumentManager.GetAllInstruments())
            {
                if (Instrument.DeviceID == inst.DeviceID && Instrument.UnitNumber == inst.UnitNumber)
                {
                    Close();
                    break;
                }
            }
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

            foreach (var inst in InstrumentManager.GetAllInstruments())
            {
                if (Instrument.DeviceID == inst.DeviceID && Instrument.UnitNumber == inst.UnitNumber)
                {
                    Close();
                    break;
                }
            }
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

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.W | Keys.Control) || keyData == Keys.Escape)
            {
                Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            pianoControl1.SetMouseChannel(toolStripComboBoxCh.SelectedIndex);
            for (int i = 0; i < 16; i++)
                pianoControl1.SetReceiveChannel(i, false);
            pianoControl1.SetReceiveChannel(toolStripComboBoxCh.SelectedIndex, true);
        }

        private void PianoControl1_NoteOn(object sender, TaggedNoteOnEvent e)
        {
            try
            {
                InstrumentManager.ExclusiveLockObject.EnterUpgradeableReadLock();

                e.Tag = new NoteOnTimbreInfo(Timbre, TimbreNo);
                Instrument.NotifyMidiEvent(e);
            }
            finally
            {
                InstrumentManager.ExclusiveLockObject.ExitUpgradeableReadLock();
            }
        }

        private void PianoControl1_NoteOff(object sender, NoteOffEvent e)
        {
            try
            {
                InstrumentManager.ExclusiveLockObject.EnterUpgradeableReadLock();

                Instrument.NotifyMidiEvent(e);
            }
            finally
            {
                InstrumentManager.ExclusiveLockObject.ExitUpgradeableReadLock();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PianoControl1_EntryDataChanged(object sender, EventArgs e)
        {
            try
            {
                InstrumentManager.ExclusiveLockObject.EnterUpgradeableReadLock();

                var cce = new ControlChangeEvent
                    ((SevenBitNumber)toolStripComboBoxCC.SelectedIndex,
                    (SevenBitNumber)pianoControl1.EntryDataValue);
                cce.Channel = (FourBitNumber)(toolStripComboBoxCh.SelectedIndex);
                Instrument.NotifyMidiEvent(cce);
            }
            finally
            {
                InstrumentManager.ExclusiveLockObject.ExitUpgradeableReadLock();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        protected void AddControl(RegisterContainerBase control)
        {
            controls.Add(control.RegisterName, control);

            control.Dock = DockStyle.Top;
            flowLayoutPanel1.Controls.Add(control);

            control.ValueChanged += Control_ValueChanged;
        }

        private object playing;
        private SevenBitNumber ni;
        private SevenBitNumber vi;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Control_ValueChanged(object sender, PropertyChangedEventArgs e)
        {
            if (toolStripButtonPlay.Checked)
            {
                if (playing != null)
                {
                    PianoControl1_NoteOff(null, new NoteOffEvent(ni, vi));
                    playing = null;
                }

                ni = (SevenBitNumber)toolStripComboBoxNote.SelectedIndex;
                vi = (SevenBitNumber)toolStripComboBoxVelo.SelectedIndex;
                PianoControl1_NoteOn(null, new TaggedNoteOnEvent(new NoteOnEvent(ni, vi)) { MonitorEvent = true });
                playing = new object();
                object _playing = playing;

                int wait = 500;
                switch (toolStripComboBoxGate.SelectedIndex)
                {
                    case 0:
                        //500ms
                        wait = 500;
                        break;
                    case 1:
                        //1000ms
                        wait = 1000;
                        break;
                    case 2:
                        //2000ms
                        wait = 2000;
                        break;
                    case 3:
                        //5000ms
                        wait = 5000;
                        break;
                }
                await Task.Delay(wait);

                if (playing == _playing)
                {
                    PianoControl1_NoteOff(null, new NoteOffEvent(ni, vi));
                    playing = null;
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemName"></param>
        /// <returns></returns>
        public RegisterContainerBase GetControl(string itemName)
        {
            return controls[itemName];
        }
    }

}
