using FM_SoundConvertor;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Animation;
using zanac.MAmidiMEmo.ComponentModel;
using zanac.MAmidiMEmo.Instruments;
using zanac.MAmidiMEmo.Midi;
using zanac.MAmidiMEmo.Properties;
using zanac.MAmidiMEmo.Util.FITOM;
using zanac.MAmidiMEmo.Util.Syx;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using Directory = System.IO.Directory;
using File = System.IO.File;
using Path = System.IO.Path;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;

namespace zanac.MAmidiMEmo.Gui
{
    public partial class FormTimbreList : FormBase
    {
        public InstrumentBase Instrument
        {
            get;
            private set;
        }

        private Type timbreType;

        private Type drumTimbreType;

        public static Dictionary<InstrumentBase, FormTimbreList> TimbreManagers = new Dictionary<InstrumentBase, FormTimbreList>();

        /// <summary>
        /// 
        /// </summary>
        public FormTimbreList()
        {
            InitializeComponent();
        }


        /// <summary>
        /// 
        /// </summary>
        public FormTimbreList(InstrumentBase inst)
        {
            InitializeComponent();

            Size = Settings.Default.TimbreListWinSize;

            this.Instrument = inst;

            for (int nn = 0; nn < 128; nn++)
            {
                toolStripComboBoxNote.Items.Add(MidiManager.GetNoteName((SevenBitNumber)nn) + "(" + nn + ")");
                toolStripComboBoxVelo.Items.Add(nn);
            }

            Settings.Default.SettingsLoaded += Default_SettingsLoaded;
            toolStripButtonPlay.Checked = Settings.Default.FmPlayOnEdit;
            toolStripButtonHook.Checked = Settings.Default.FmHook;
            toolStripComboBoxVelo.SelectedIndex = Settings.Default.FmVelocity;
            toolStripComboBoxGate.SelectedIndex = Settings.Default.FmGateTime;
            toolStripComboBoxNote.SelectedIndex = Settings.Default.FmNote;
            toolStripComboBoxCC.SelectedIndex = Settings.Default.FmCC;
            toolStripComboBoxCh.SelectedIndex = Settings.Default.FmCh;
            if (inst.Channels[toolStripComboBoxCh.SelectedIndex] == false)
            {
                for (int i = 0; i < inst.Channels.Length; i++)
                {
                    if (inst.Channels[i])
                    {
                        toolStripComboBoxCh.SelectedIndex = i;
                        break;
                    }
                }
            }

            //pianoControl1.TargetTimbres = new TimbreBase[] { timbre };

            timbreType = Instrument.BaseTimbres[0].GetType();
            drumTimbreType = Instrument.DrumTimbres[0].GetType();

            for (int i = 0; i < Instrument.BaseTimbres.Length; i++)
            {
                var tim = new TimbreItem(inst.BaseTimbres[i], i);
                var lvi = new ListViewItem(new string[] { i.ToString(), tim.Timbre.TimbreName, tim.Timbre.Memo });
                lvi.Tag = tim;
                listViewCurrentTimbres.Items.Add(lvi);
            }
            listViewCurrentTimbres.Items[0].Selected = true;

            setTitle();

            InstrumentManager.InstrumentChanged += InstrumentManager_InstrumentChanged;
            InstrumentManager.InstrumentRemoved += InstrumentManager_InstrumentRemoved;

            pianoControl1.NoteOn += PianoControl1_NoteOn;
            pianoControl1.NoteOff += PianoControl1_NoteOff;
            pianoControl1.EntryDataChanged += PianoControl1_EntryDataChanged;

            Midi.MidiManager.MidiEventHooked += MidiManager_MidiEventHooked;

            List<string> extList = new List<string>(new string[] { ".MSD", ".MSDS" });
            if (inst.CanImportToneFile)
            {
                var texts = Tone.SupportedExts.Split(';');
                for (int i = 0; i < texts.Length; i++)
                    texts[i] = texts[i].Replace("*", "");
                extList.AddRange(texts);
            }
            if (inst.CanImportBinFile)
            {
                var texts = inst.SupportedBinExts.Split(';');
                for (int i = 0; i < texts.Length; i++)
                    texts[i] = texts[i].Replace("*", "");
                extList.AddRange(texts);
            }

            TimbreManagers.Add(inst, this);
        }

        protected override void OnActivated(EventArgs e)
        {
            listViewCurrentTimbres.BeginUpdate();
            for (int i = 0; i < Instrument.BaseTimbres.Length; i++)
            {
                var tim = Instrument.BaseTimbres[i];
                listViewCurrentTimbres.Items[i].SubItems[1].Text = tim.TimbreName;
                listViewCurrentTimbres.Items[i].SubItems[2].Text = tim.Memo;
            }
            listViewCurrentTimbres.EndUpdate();
            base.OnActivated(e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnShown(EventArgs e)
        {
            listViewCurrentTimbres.ItemSelectionChanged += new ListViewItemSelectionChangedEventHandler(listViewCurrentTimbres_ItemSelectionChanged);
            foreach (ColumnHeader c in listViewCurrentTimbres.Columns)
                c.Width = -1;

            toolStripComboBoxCh.Focus();
            base.OnShown(e);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            TimbreManagers.Remove(Instrument);

            Midi.MidiManager.MidiEventHooked -= MidiManager_MidiEventHooked;

            Settings.Default.FmPlayOnEdit = toolStripButtonPlay.Checked;
            Settings.Default.FmHook = toolStripButtonHook.Checked;
            Settings.Default.FmVelocity = toolStripComboBoxVelo.SelectedIndex;
            Settings.Default.FmGateTime = toolStripComboBoxGate.SelectedIndex;
            Settings.Default.FmNote = toolStripComboBoxNote.SelectedIndex;
            Settings.Default.FmCC = toolStripComboBoxCC.SelectedIndex;
            Settings.Default.FmCh = toolStripComboBoxCh.SelectedIndex;

            Settings.Default.TimbreListWinSize = Size;

            base.OnClosing(e);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.W | Keys.Control) || keyData == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void Default_SettingsLoaded(object sender, System.Configuration.SettingsLoadedEventArgs e)
        {
            toolStripButtonPlay.Checked = Settings.Default.FmPlayOnEdit;
            toolStripButtonHook.Checked = Settings.Default.FmHook;
            toolStripComboBoxVelo.SelectedIndex = Settings.Default.FmVelocity;
            toolStripComboBoxGate.SelectedIndex = Settings.Default.FmGateTime;
            toolStripComboBoxNote.SelectedIndex = Settings.Default.FmNote;
            toolStripComboBoxCC.SelectedIndex = Settings.Default.FmCC;
            toolStripComboBoxCh.SelectedIndex = Settings.Default.FmCh;
        }

        private void setTitle()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(Instrument.Name + "(" + Instrument.UnitNumber + ")");
            sb.Append(" - " + timbreType.Name);

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
                    return;
            }
            DialogResult = DialogResult.Cancel;
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
                    return;
            }
            DialogResult = DialogResult.Cancel;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MidiManager_MidiEventHooked(object sender, CancelMidiEventReceivedEventArgs e)
        {
            if (!this.IsHandleCreated || ActiveForm != this)
                return;

            if (this.IsDisposed)
                return;

            this.Invoke(new MethodInvoker(() =>
            {
                if (this.IsDisposed)
                    return;

                if (toolStripButtonHook.Checked)
                {
                    switch (e.Event.Event)
                    {
                        case NoteOnEvent non:
                            if (non.Velocity != 0)
                                PianoControl1_NoteOn(null, new TaggedNoteOnEvent(non) { MonitorEvent = true });
                            else
                                PianoControl1_NoteOff(null, new NoteOffEvent(non.NoteNumber, (SevenBitNumber)0) { Channel = non.Channel, DeltaTime = non.DeltaTime });
                            break;
                        case NoteOffEvent noff:
                            PianoControl1_NoteOff(null, noff);
                            break;
                        default:
                            Instrument.NotifyMidiEvent(e.Event.Event);
                            break;
                    }
                    e.Cancel = true;
                }
            }));
        }

        private object playing;
        private SevenBitNumber ni;
        private SevenBitNumber vi;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private void testStop()
        {
            if (playing != null)
            {
                PianoControl1_NoteOff(null, new NoteOffEvent(ni, vi));
                playing = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task testPlay()
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
                if (IsDisposed)
                    return;

                PianoControl1_NoteOff(null, new NoteOffEvent(ni, vi));
                playing = null;
            }
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < 16; i++)
                pianoControl1.SetReceiveChannel(i, false);
            pianoControl1.SetReceiveChannel(toolStripComboBoxCh.SelectedIndex, true);
        }

        private void PianoControl1_NoteOn(object sender, TaggedNoteOnEvent e)
        {
            try
            {
                //InstrumentManager.ExclusiveLockObject.EnterUpgradeableReadLock();
                if (listViewCurrentTimbres.SelectedItems.Count != 0)
                {
                    TimbreItem tim = (TimbreItem)listViewCurrentTimbres.SelectedItems[0].Tag;
                    TimbreItem timNo = (TimbreItem)listViewCurrentTimbres.Items[0].Tag;
                    if (listViewCurrentTimbres.SelectedItems.Count != 0)
                        timNo = (TimbreItem)listViewCurrentTimbres.SelectedItems[0].Tag;

                    e.Tag = new NoteOnTimbreInfo(tim.Timbre, timNo.Number);
                    e.Channel = (FourBitNumber)(toolStripComboBoxCh.SelectedIndex & 0xf);
                    Instrument.NotifyMidiEvent(e);
                }
            }
            finally
            {
                //InstrumentManager.ExclusiveLockObject.ExitUpgradeableReadLock();
            }
        }

        private void PianoControl1_NoteOff(object sender, NoteOffEvent e)
        {
            try
            {
                //InstrumentManager.ExclusiveLockObject.EnterUpgradeableReadLock();
                e.Channel = (FourBitNumber)(toolStripComboBoxCh.SelectedIndex & 0xf);
                Instrument.NotifyMidiEvent(e);
            }
            finally
            {
                //InstrumentManager.ExclusiveLockObject.ExitUpgradeableReadLock();
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
                //InstrumentManager.ExclusiveLockObject.EnterUpgradeableReadLock();

                var cce = new ControlChangeEvent
                    ((SevenBitNumber)toolStripComboBoxCC.SelectedIndex,
                    (SevenBitNumber)pianoControl1.EntryDataValue);
                cce.Channel = (FourBitNumber)(toolStripComboBoxCh.SelectedIndex);
                Instrument.NotifyMidiEvent(cce);
            }
            finally
            {
                //InstrumentManager.ExclusiveLockObject.ExitUpgradeableReadLock();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void listViewCurrentTimbres_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (!e.IsSelected)
                return;
            if (listViewCurrentTimbres.SelectedItems.Count == 0)
                return;

            TimbreItem ti = (TimbreItem)listViewCurrentTimbres.SelectedItems[0].Tag;
            if (ti == null)
                return;

            if (e.IsSelected && toolStripButtonPlay.Checked && Visible)
                await testPlay();
        }

        /// <summary>
        /// 
        /// </summary>
        private class TimbreItem
        {
            public TimbreBase Timbre
            {
                get;
                private set;
            }

            public int Number
            {
                get;
                private set;
            }

            public TimbreItem(TimbreBase timbre, int number)
            {
                Timbre = timbre;
                Number = number;
            }

            public override string ToString()
            {
                return "No " + Number.ToString() + " " + Timbre.TimbreName + " " + Timbre.Memo;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private class DrumTimbreItem
        {
            public DrumTimbre Timbre
            {
                get;
                private set;
            }

            public int Number
            {
                get;
                private set;
            }

            public DrumTimbreItem(DrumTimbre timbre, int number)
            {
                Timbre = timbre;
                Number = number;
            }

            public override string ToString()
            {
                return "No " + Number.ToString() + " " + Timbre.TimbreName + " " + Timbre.KeyName;
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            ListViewHitTestInfo info = listViewCurrentTimbres.HitTest(listViewCurrentTimbres.PointToClient(contextMenuStrip1.Bounds.Location));

            TimbreItem ttim = (TimbreItem)info.Item.Tag;

            editToolStripMenuItem.Enabled = ttim.Timbre.CanOpenTimbreEditor(Instrument);
        }

        private void listViewCurrentTimbres_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            /*
            ListViewHitTestInfo info = listViewCurrentTimbres.HitTest(e.Location);
            if (info.Item != null)
            {
                TimbreItem ttim = (TimbreItem)info.Item.Tag;

                testStop();

                ttim.Timbre.OpenTimbreEditor(Instrument);
            }
            */
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListViewHitTestInfo info = listViewCurrentTimbres.HitTest(listViewCurrentTimbres.PointToClient(contextMenuStrip1.Bounds.Location));
            if (info.SubItem == null)
                return;
            if (info.Item.SubItems[0] == info.SubItem)
                return;

            TimbreItem ttim = (TimbreItem)info.Item.Tag;
            using (var f = new FormRename())
            {
                if (info.Item.SubItems[1] == info.SubItem)
                {
                    f.InputText = ttim.Timbre.TimbreName;
                    f.TitleText = "Specify the Timbre name";
                }
                else if (info.Item.SubItems[2] == info.SubItem)
                {
                    f.InputText = ttim.Timbre.Memo;
                    f.TitleText = "Specify the Timbre memo";
                }

                var dr = f.ShowDialog(this);
                if (dr == DialogResult.OK)
                {
                    info.SubItem.Text = f.InputText;
                    if (info.Item.SubItems[1] == info.SubItem)
                        ttim.Timbre.TimbreName = f.InputText;
                    else if (info.Item.SubItems[2] == info.SubItem)
                        ttim.Timbre.Memo = f.InputText;
                }
            }
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListViewHitTestInfo info = listViewCurrentTimbres.HitTest(listViewCurrentTimbres.PointToClient(contextMenuStrip1.Bounds.Location));
            if (info.Item != null)
            {
                TimbreItem ttim = (TimbreItem)info.Item.Tag;

                testStop();

                ttim.Timbre.OpenTimbreEditor(Instrument);
            }
        }

        private void propertyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListViewHitTestInfo info = listViewCurrentTimbres.HitTest(listViewCurrentTimbres.PointToClient(contextMenuStrip1.Bounds.Location));
            if (info.Item != null)
            {
                TimbreItem ttim = (TimbreItem)info.Item.Tag;

                testStop();

                ttim.Timbre.OpenPropEditor(Instrument);
            }
        }
    }
}
