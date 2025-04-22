// copyright-holders:K.Ito
using FM_SoundConvertor;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using MetroFramework.Forms;
using Newtonsoft.Json;
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
using System.Windows.Media.Animation;
using zanac.MAmidiMEmo.ComponentModel;
using zanac.MAmidiMEmo.Instruments;
using zanac.MAmidiMEmo.Midi;
using zanac.MAmidiMEmo.Properties;
using zanac.MAmidiMEmo.Util;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using static zanac.MAmidiMEmo.Instruments.Chips.SCC1;
using File = System.IO.File;
using Path = System.IO.Path;

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
        public FormDrumTableEditor(InstrumentBase inst)
        {
            InitializeComponent();

            Size = Settings.Default.DrumEditorWinSize;

            Instrument = inst;

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

            InstrumentManager.InstrumentChanged += InstrumentManager_InstrumentChanged;
            InstrumentManager.InstrumentRemoved += InstrumentManager_InstrumentRemoved;

            pianoControl1.NoteOn += PianoControl1_NoteOn;
            pianoControl1.NoteOff += PianoControl1_NoteOff;
            pianoControl1.EntryDataChanged += PianoControl1_EntryDataChanged;

            Midi.MidiManager.MidiEventHooked += MidiManager_MidiEventHooked;

            System.Windows.Forms.Application.Idle += Application_Idle;
        }

        protected override void OnShown(EventArgs e)
        {
            toolStripComboBoxCh.Focus();

            base.OnShown(e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            Midi.MidiManager.MidiEventHooked -= MidiManager_MidiEventHooked;

            System.Windows.Forms.Application.Idle -= new EventHandler(Application_Idle);

            Settings.Default.SettingsLoaded -= Default_SettingsLoaded;
            InstrumentManager.InstrumentChanged -= InstrumentManager_InstrumentChanged;
            InstrumentManager.InstrumentRemoved -= InstrumentManager_InstrumentRemoved;

            Settings.Default.FmPlayOnEdit = toolStripButtonPlay.Checked;
            Settings.Default.FmHook = toolStripButtonHook.Checked;
            Settings.Default.FmVelocity = toolStripComboBoxVelo.SelectedIndex;
            Settings.Default.FmGateTime = toolStripComboBoxGate.SelectedIndex;
            Settings.Default.FmNote = toolStripComboBoxNote.SelectedIndex;
            Settings.Default.FmCC = toolStripComboBoxCC.SelectedIndex;
            Settings.Default.FmCh = toolStripComboBoxCh.SelectedIndex;

            Settings.Default.DrumEditorWinSize = Size;

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
        /// <param name="value"></param>
        private void updateList()
        {
            int ti = 0;
            if (listViewPcmSounds.TopItem != null)
                ti = listViewPcmSounds.TopItem.Index;

            List<int> sis = new List<int>();
            foreach (var si in listViewPcmSounds.SelectedIndices)
                sis.Add((int)si);
            listViewPcmSounds.BeginUpdate();
            try
            {
                ignorePlayingFlag++;
                listViewPcmSounds.Items.Clear();
                for (int i = 0; i < f_DrumData.Length; i++)
                {
                    DrumTimbre dt = f_DrumData[i];
                    dt.Instrument = Instrument;

                    var item = listViewPcmSounds.Items.Add(i.ToString() + "(" + dt.KeyName + ")");
                    item.Tag = dt;
                    setListViewItemText(item, dt);
                }
                foreach (ColumnHeader c in listViewPcmSounds.Columns)
                    c.Width = -2;
                listViewPcmSounds.Items[listViewPcmSounds.Items.Count - 1].EnsureVisible();
                listViewPcmSounds.Items[ti].EnsureVisible();
                listViewPcmSounds.SelectedIndices.Clear();
                foreach (int si in sis)
                    listViewPcmSounds.SelectedIndices.Add(si);
            }
            finally
            {
                ignorePlayingFlag--;
                listViewPcmSounds.EndUpdate();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="dt"></param>
        private void setListViewItemText(ListViewItem item, DrumTimbre dt)
        {
            string text = string.Empty;
            if (dt.TimbreNumber != null)
            {
                text = dt.TimbreNumber.ToString();
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
            }
            if (item.SubItems.Count == 1)
                item.SubItems.Add(text);
            else
                item.SubItems[1].Text = text;
        }

        private bool listViewPcmSoundsItemSelectionChanged;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="InvalidDataException"></exception>
        private async void Application_Idle(object sender, EventArgs e)
        {
            if (listViewPcmSoundsItemSelectionChanged)
            {
                listViewPcmSoundsItemSelectionChanged = false;

                List<DrumTimbre> insts = new List<DrumTimbre>();
                foreach (ListViewItem item in listViewPcmSounds.SelectedItems)
                    insts.Add((DrumTimbre)item.Tag);
                propertyGrid1.SelectedObjects = insts.ToArray();

                if (listViewPcmSounds.SelectedItems.Count != 0)
                {
                    if (toolStripButtonPlay.Checked && ignorePlayingFlag == 0 && Visible)
                        await testPlay();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listViewPcmSounds_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            listViewPcmSoundsItemSelectionChanged = true;
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

        private void metroButtonImport_Click(object sender, EventArgs e)
        {
            openFileDialogImport.InitialDirectory = Program.GetToneLibLastDir();
            openFileDialogImport.DefaultExt = "*.msds";
            openFileDialogImport.Filter = "MAmi Serialize Data Files(*.msds)|*.msds";
            var dr = openFileDialogImport.ShowDialog(this);
            if (dr == DialogResult.OK)
            {
                try
                {
                    string txt = System.IO.File.ReadAllText(openFileDialogImport.FileName);
                    StringReader rs = new StringReader(txt);

                    string ftname = rs.ReadLine();
                    if (string.Equals(ftname, typeof(DrumTimbre).FullName, StringComparison.Ordinal))
                    {
                        string ver = rs.ReadLine();
                        if (ver != "1.0")
                            throw new InvalidDataException();
                        int num = int.Parse(rs.ReadLine());
                        StringBuilder lines = new StringBuilder();
                        List<DrumTimbre> ts = new List<DrumTimbre>();
                        int progNo = 0;
                        while (true)
                        {
                            string line = rs.ReadLine();
                            if (line == null || line == "-")
                            {
                                if (lines.Length == 0)
                                    break;
                                DrumTimbre tim = JsonConvert.DeserializeObject<DrumTimbre>(lines.ToString(), Program.JsonAutoSettings);

                                tim.NoteNumber = progNo;
                                tim.KeyName = Midi.MidiManager.GetNoteName((SevenBitNumber)progNo);

                                ts.Add(tim);
                                lines.Clear();
                                if (line == null)
                                    break;
                                continue;
                            }
                            lines.AppendLine(line);
                        }
                        for (int i = 0; i < Math.Min(f_DrumData.Length, ts.Count); i++)
                        {
                            f_DrumData[i] = ts[i];
                            setListViewItemText(listViewPcmSounds.Items[i], ts[i]);
                        }
                        Settings.Default.ToneLibLastDir = Path.GetDirectoryName(openFileDialogImport.FileName);
                    }
                }
                catch (Exception ex)
                {
                    if (ex.GetType() == typeof(Exception))
                        throw;
                    else if (ex.GetType() == typeof(SystemException))
                        throw;

                    MessageBox.Show(Resources.FailedLoadFile + "\r\n" + ex.Message);
                }
            }
        }

        private void metroButtonExport_Click(object sender, EventArgs e)
        {
            saveFileDialogExport.InitialDirectory = Program.GetToneLibLastDir();
            saveFileDialogExport.DefaultExt = "*.msds";
            saveFileDialogExport.Filter = "MAmi Serialize Data Files(*.msds)|*.msds";

            string fname = "Drums_" + Instrument.Name;

            foreach (var invalidChar in Path.GetInvalidFileNameChars())
                fname = fname.Replace(invalidChar.ToString(), "");

            fname = Path.ChangeExtension(fname, ".msds");

            saveFileDialogExport.FileName = Utility.MakeUniqueFileName(saveFileDialogExport.InitialDirectory, fname);
            saveFileDialogExport.SupportMultiDottedExtensions = true;

            DialogResult res = saveFileDialogExport.ShowDialog(this);
            if (res == DialogResult.OK)
            {
                fname = saveFileDialogExport.FileName;
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(typeof(DrumTimbre).FullName);
                sb.AppendLine("1.0");
                sb.AppendLine(listViewPcmSounds.Items.Count.ToString());

                for (int i = 0; i < listViewPcmSounds.Items.Count; i++)
                {
                    DrumTimbre ttimi = (DrumTimbre)(listViewPcmSounds.Items[i].Tag);
                    string sd = JsonConvert.SerializeObject(ttimi, Program.JsonAutoSettings);

                    sb.AppendLine(sd);
                    sb.AppendLine("-");
                }

                try
                {
                    File.WriteAllText(fname, sb.ToString());
                }
                catch (Exception ex)
                {
                    if (ex.GetType() == typeof(Exception))
                        throw;
                    else if (ex.GetType() == typeof(SystemException))
                        throw;

                    MessageBox.Show(ex.Message);
                }

                saveFileDialogExport.InitialDirectory = Path.GetDirectoryName(saveFileDialogExport.FileName);
            }
        }

        private void listViewPcmSounds_DragDrop(object sender, DragEventArgs e)
        {
            var items = e.Data.GetData(typeof(ListViewItem[])) as ListViewItem[];
            if (items != null && items.Length != 0)
            {
                if (listViewPcmSounds != items[0].ListView)
                    return;

                Point cp = listViewPcmSounds.PointToClient(new Point(e.X, e.Y));
                ListViewItem dragToItem = listViewPcmSounds.GetItemAt(cp.X, cp.Y);
                if (dragToItem != null)
                {
                    bool exchange = true;
                    if (listViewPcmSounds != items[0].ListView || (e.KeyState & 8) == 8)
                        exchange = false;

                    for (int i = dragToItem.Index; i < Math.Min(dragToItem.Index + items.Length, listViewPcmSounds.Items.Count); i++)
                    {
                        DrumTimbre tim = items[i - dragToItem.Index].Tag as DrumTimbre;
                        if (tim != null)
                        {
                            DrumTimbre ttim = (DrumTimbre)(listViewPcmSounds.Items[i].Tag);
                            if (exchange)
                            {
                                var s = JsonConvert.SerializeObject((DrumTimbre)ttim, Program.JsonAutoSettings);
                                DrumTimbre ttim_ = JsonConvert.DeserializeObject<DrumTimbre>(s, Program.JsonAutoSettings);

                                ttim.TimbreNumber = tim.TimbreNumber;
                                ttim.BaseNote = tim.BaseNote;
                                ttim.GateTime = tim.GateTime;
                                ttim.PanShift = tim.PanShift;
                                ttim.TimbreName = tim.TimbreName;

                                setListViewItemText(listViewPcmSounds.Items[i], ttim);

                                tim.TimbreNumber = ttim_.TimbreNumber;
                                tim.BaseNote = ttim_.BaseNote;
                                tim.GateTime = ttim_.GateTime;
                                tim.PanShift = ttim_.PanShift;
                                tim.TimbreName = ttim_.TimbreName;

                                setListViewItemText(items[i - dragToItem.Index], tim);
                            }
                            else
                            {
                                ttim.TimbreNumber = tim.TimbreNumber;
                                ttim.BaseNote = tim.BaseNote;
                                ttim.GateTime = tim.GateTime;
                                ttim.PanShift = tim.PanShift;
                                ttim.TimbreName = tim.TimbreName;

                                setListViewItemText(listViewPcmSounds.Items[i], ttim);
                            }
                        }
                    }
                }
            }
        }

        private void listViewPcmSounds_DragOver(object sender, DragEventArgs e)
        {
            var items = e.Data.GetData(typeof(ListViewItem[])) as ListViewItem[];
            if (items != null && items.Length != 0)
            {
                if (listViewPcmSounds != items[0].ListView)
                    return;

                if ((e.KeyState & 8) == 8)
                    e.Effect = DragDropEffects.Copy;
                else
                    e.Effect = DragDropEffects.Move;

                try
                {
                    ignorePlayingFlag++;
                    listViewPcmSounds.SelectedItems.Clear();

                    Point cp = listViewPcmSounds.PointToClient(new Point(e.X, e.Y));
                    ListViewItem dragToItem = listViewPcmSounds.GetItemAt(cp.X, cp.Y);
                    if (dragToItem != null)
                    {
                        var index = dragToItem.Index;
                        var maxIndex = listViewPcmSounds.Items.Count - 1;
                        var scrollZoneHeight = listViewPcmSounds.Font.Height;
                        if (index > 0 && cp.Y < scrollZoneHeight)
                            listViewPcmSounds.Items[index - 1].EnsureVisible();
                        else if (index < maxIndex && cp.Y > listViewPcmSounds.Height - scrollZoneHeight)
                            listViewPcmSounds.Items[index + 1].EnsureVisible();

                        for (int i = dragToItem.Index; i < Math.Min(dragToItem.Index + items.Length, listViewPcmSounds.Items.Count); i++)
                            listViewPcmSounds.Items[i].Selected = true;
                    }
                }
                finally
                {
                    ignorePlayingFlag--;
                }
            }
        }

        private void listViewPcmSounds_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ListViewItem[] items = new ListViewItem[listViewPcmSounds.SelectedItems.Count];
                listViewPcmSounds.SelectedItems.CopyTo(items, 0);
                listViewPcmSounds.DoDragDrop(items, DragDropEffects.Copy | DragDropEffects.Move);
            }
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

        private int ignorePlayingFlag;

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
            if (listViewPcmSounds.SelectedIndices.Count != 0)
                ni = (SevenBitNumber)listViewPcmSounds.SelectedIndices[0];
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
                if (Instrument.ChannelTypes[toolStripComboBoxCh.SelectedIndex] == ChannelType.Drum ||
                    Instrument.ChannelTypes[toolStripComboBoxCh.SelectedIndex] == ChannelType.DrumGt)
                {
                    e.Channel = (FourBitNumber)(toolStripComboBoxCh.SelectedIndex & 0xf);
                    Instrument.NotifyMidiEvent(e);
                }
                else
                {
                    //InstrumentManager.ExclusiveLockObject.EnterUpgradeableReadLock();
                    if (listViewPcmSounds.SelectedItems.Count != 0)
                    {
                        DrumTimbre dt = (DrumTimbre)listViewPcmSounds.SelectedItems[0].Tag;
                        TimbreBase tb = null;
                        if (dt != null && dt.TimbreNumber != null)
                        {
                            int pn = (int)dt.TimbreNumber;
                            if ((pn & 0xffff0000) != 0)
                            {
                                int ptidx = pn & 0xffff;
                                if (ptidx >= Instrument.CombinedTimbres.Length)
                                    ptidx = Instrument.CombinedTimbres.Length - 1;
                                tb = Instrument.CombinedTimbres[ptidx];
                            }

                            int btidx = pn & 0xffff;
                            if (btidx >= Instrument.BaseTimbres.Length)
                                btidx = Instrument.BaseTimbres.Length - 1;
                            tb = Instrument.BaseTimbres[btidx];
                        }
                        if (tb != null)
                        {
                            e.Tag = new NoteOnTimbreInfo(tb, 0);
                            e.Channel = (FourBitNumber)(toolStripComboBoxCh.SelectedIndex & 0xf);
                            Instrument.NotifyMidiEvent(e);
                        }
                    }
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
        /// Clear
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void metroButton3_Click(object sender, EventArgs e)
        {
            if (listViewPcmSounds.SelectedItems.Count != 0)
            {
                for (int i = 0; i < listViewPcmSounds.SelectedItems.Count; i++)
                {
                    DrumTimbre ttimi = (DrumTimbre)(listViewPcmSounds.SelectedItems[i].Tag);
                    DrumTimbre tim = JsonConvert.DeserializeObject<DrumTimbre>("{}", Program.JsonAutoSettings);

                    ttimi.TimbreNumber = tim.TimbreNumber;
                    ttimi.BaseNote = tim.BaseNote;
                    ttimi.GateTime = tim.GateTime;
                    ttimi.PanShift = tim.PanShift;
                    ttimi.TimbreName = tim.TimbreName;

                    setListViewItemText(listViewPcmSounds.SelectedItems[i], ttimi);
                }
            }
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            metroButton3_Click(sender, e);
        }

        private void metroButtonCont_Click(object sender, EventArgs e)
        {
            if (listViewPcmSounds.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "Select the item for which the TimbreNumber has been set.", "Error", MessageBoxButtons.OK);
                return;
            }

            var fi = listViewPcmSounds.SelectedItems[0];
            if (fi != null)
            {
                DrumTimbre ttimi = (DrumTimbre)fi.Tag;
                var tn = ttimi.TimbreNumber;
                if (tn != null)
                {
                    using (FormInputNumber f = new FormInputNumber())
                    {
                        f.MaximumValue = 127 - fi.Index;
                        f.MinimumValue = 0;
                        f.TitleText = Resources.ContinuousSet;
                        var r = f.ShowDialog();
                        if (r == DialogResult.OK)
                        {
                            int n = (int)f.InputValue;
                            int stn = (int)tn + 1;
                            for (int i = fi.Index + 1; i < fi.Index + 1 + n; i++)
                            {
                                DrumTimbre sttimi = (DrumTimbre)(listViewPcmSounds.Items[i].Tag);
                                if (Enum.IsDefined(typeof(ProgramAssignmentNumber), stn))
                                {
                                    sttimi.TimbreNumber = (ProgramAssignmentNumber)stn;
                                }
                                else
                                {
                                    break;
                                }
                                stn++;
                            }
                            updateList();
                        }
                    }
                }
                else
                {
                    MessageBox.Show(this, Resources.ContinuousSetError, "Error", MessageBoxButtons.OK);
                }
            }
        }
    }
}
