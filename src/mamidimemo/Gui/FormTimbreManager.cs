using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using zanac.MAmidiMEmo.Instruments;
using zanac.MAmidiMEmo.Midi;
using zanac.MAmidiMEmo.Properties;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using Directory = System.IO.Directory;
using File = System.IO.File;
using Path = System.IO.Path;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;

namespace zanac.MAmidiMEmo.Gui
{
    public partial class FormTimbreManager : FormBase
    {
        public InstrumentBase Instrument
        {
            get;
            private set;
        }

        private Type timbreType;

        private string originalSerializedData;


        /// <summary>
        /// 
        /// </summary>
        public FormTimbreManager()
        {
            InitializeComponent();
        }


        /// <summary>
        /// 
        /// </summary>
        public FormTimbreManager(InstrumentBase inst)
        {
            InitializeComponent();

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
            this.Instrument = inst;
            timbreType = Instrument.BaseTimbres[0].GetType();

            originalSerializedData = JsonConvert.SerializeObject(Instrument.BaseTimbres, Formatting.Indented);

            for (int i = 0; i < Instrument.BaseTimbres.Length; i++)
            {
                TimbreBase t = (TimbreBase)JsonConvert.DeserializeObject(inst.BaseTimbres[i].SerializeData, timbreType);
                var tim = new TimbreItem(t, i);
                var lvi = new ListViewItem(new string[] { i.ToString(), tim.Timbre.TimbreName, tim.Timbre.Memo });
                lvi.Tag = tim;
                listViewCurrentTimbres.Items.Add(lvi);
            }
            listViewCurrentTimbres.Items[0].Selected = true;
            lastFocusedListView = listViewCurrentTimbres;

            setTitle();

            InstrumentManager.InstrumentChanged += InstrumentManager_InstrumentChanged;
            InstrumentManager.InstrumentRemoved += InstrumentManager_InstrumentRemoved;

            pianoControl1.NoteOn += PianoControl1_NoteOn;
            pianoControl1.NoteOff += PianoControl1_NoteOff;
            pianoControl1.EntryDataChanged += PianoControl1_EntryDataChanged;

            Midi.MidiManager.MidiEventHooked += MidiManager_MidiEventHooked;

            fileFolderList1.FilterExts = new string[] { ".MSD", ".MSDS" };
            fileFolderList1.FilterFunction = new Func<FileSystemInfo, bool>((fi) =>
            {
                FileInfo ffi = fi as FileInfo;
                if (ffi != null)
                {
                    try
                    {
                        string fullTypeName = timbreType.FullName;
                        string ftname = System.IO.File.OpenText(ffi.FullName).ReadLine();
                        if (string.Equals(fullTypeName, ftname, StringComparison.Ordinal))
                            return true;
                    }
                    catch (Exception ex)
                    {
                        if (ex.GetType() == typeof(Exception))
                            throw;
                        else if (ex.GetType() == typeof(SystemException))
                            throw;

                    }
                    return false;
                }
                else
                    return true;
            });
            fileFolderList1.ItemSelectionChanged += fileFolderList1_SelectedIndexChanged;

            fileFolderList1.CurrentDirectory = Settings.Default.ToneLibLastDir;
            metroTextBox1.Text = Settings.Default.ToneLibLastDir;
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

            listViewFilesTimbres.ItemSelectionChanged += new ListViewItemSelectionChangedEventHandler(listViewFilesTimbres_ItemSelectionChanged);

            fileFolderList1.Browse(fileFolderList1.CurrentDirectory);

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

            Settings.Default.FmPlayOnEdit = toolStripButtonPlay.Checked;
            Settings.Default.FmHook = toolStripButtonHook.Checked;
            Settings.Default.FmVelocity = toolStripComboBoxVelo.SelectedIndex;
            Settings.Default.FmGateTime = toolStripComboBoxGate.SelectedIndex;
            Settings.Default.FmNote = toolStripComboBoxNote.SelectedIndex;
            Settings.Default.FmCC = toolStripComboBoxCC.SelectedIndex;
            Settings.Default.FmCh = toolStripComboBoxCh.SelectedIndex;

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonOK_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < Instrument.BaseTimbres.Length; i++)
            {
                TimbreItem ttim = (TimbreItem)(listViewCurrentTimbres.Items[i].Tag);
                Instrument.BaseTimbres[i] = ttim.Timbre;
            }

            Settings.Default.ToneLibLastDir = fileFolderList1.CurrentDirectory;

            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void metroButtonAbort_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Abort;
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
                if (lastFocusedListView != null && lastFocusedListView.SelectedItems.Count != 0)
                {
                    TimbreItem tim = (TimbreItem)lastFocusedListView.SelectedItems[0].Tag;
                    TimbreItem timNo = (TimbreItem)listViewCurrentTimbres.Items[0].Tag;
                    if(listViewCurrentTimbres.SelectedItems.Count != 0)
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

        private bool ignoreMetroComboBoxTimbres_SelectedIndexChanged;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void listViewCurrentTimbres_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (!e.IsSelected)
                return;
            if (ignoreMetroComboBoxTimbres_SelectedIndexChanged)
                return;
            if (listViewCurrentTimbres.SelectedItems.Count == 0)
                return;

            TimbreItem ti = (TimbreItem)listViewCurrentTimbres.SelectedItems[0].Tag;
            if (ti == null)
                return;

            if (toolStripButtonPlay.Checked && ignorePlayingFlag == 0 && Visible)
                await testPlay();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void listViewFilesTimbres_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (!e.IsSelected)
                return;
            if (ignoreMetroComboBoxTimbres_SelectedIndexChanged)
                return;
            if (listViewCurrentTimbres.SelectedItems.Count == 0)
                return;

            TimbreItem ti = (TimbreItem)listViewFilesTimbres.SelectedItems[0].Tag;
            if (ti == null)
                return;

            if (toolStripButtonPlay.Checked && ignorePlayingFlag == 0 && Visible)
                await testPlay();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fileFolderList1_SelectedIndexChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (!e.IsSelected)
                return;

            var dlDestFilePath = fileFolderList1.SelectedPaths;
            if (dlDestFilePath == null)
                return;

            int no = 0;
            try
            {
                listViewFilesTimbres.BeginUpdate();
                listViewFilesTimbres.Items.Clear();

                for (int i = 0; i < dlDestFilePath.Length; i++)
                {
                    string file = dlDestFilePath[i];
                    if (!System.IO.File.Exists(file))
                        return;

                    switch (Path.GetExtension(file).ToUpper())
                    {
                        case ".MSD":
                            try
                            {
                                string txt = System.IO.File.ReadAllText(file);
                                StringReader rs = new StringReader(txt);

                                string ftname = rs.ReadLine();
                                if (!string.Equals(timbreType.FullName, ftname, StringComparison.Ordinal))
                                    return;
                                string ver = rs.ReadLine();
                                if (ver != "1.0")
                                    return;

                                string serializeData = rs.ReadToEnd();

                                TimbreBase t = (TimbreBase)JsonConvert.DeserializeObject(serializeData, timbreType);
                                TimbreItem tim = new TimbreItem(t, i);

                                var lvi = new ListViewItem(new string[] { i.ToString(), tim.Timbre.TimbreName, tim.Timbre.Memo });
                                lvi.Tag = tim;
                                listViewFilesTimbres.Items.Add(lvi);
                            }
                            catch (Exception ex)
                            {
                                if (ex.GetType() == typeof(Exception))
                                    throw;
                                else if (ex.GetType() == typeof(SystemException))
                                    throw;

                            }
                            break;
                        case ".MSDS":
                            {
                                string txt = System.IO.File.ReadAllText(file);
                                StringReader rs = new StringReader(txt);

                                string ftname = rs.ReadLine();
                                if (ftname == timbreType.FullName)
                                {
                                    string ver = rs.ReadLine();
                                    if (ver != "1.0")
                                        throw new InvalidDataException();
                                    int num = int.Parse(rs.ReadLine());
                                    StringBuilder lines = new StringBuilder();
                                    while (true)
                                    {
                                        try
                                        {
                                            string line = rs.ReadLine();
                                            if (line == null || line == "-")
                                            {
                                                if (lines.Length == 0)
                                                    break;

                                                TimbreBase t = (TimbreBase)JsonConvert.DeserializeObject(lines.ToString(), timbreType);
                                                TimbreItem tim = new TimbreItem(t, i);

                                                var lvi = new ListViewItem(new string[] { no.ToString(), tim.Timbre.TimbreName, tim.Timbre.Memo });
                                                no++;
                                                lvi.Tag = tim;
                                                listViewFilesTimbres.Items.Add(lvi);

                                                lines.Clear();
                                                if (line == null)
                                                    break;
                                                continue;
                                            }
                                            lines.AppendLine(line);
                                        }
                                        catch (Exception ex)
                                        {
                                            if (ex.GetType() == typeof(Exception))
                                                throw;
                                            else if (ex.GetType() == typeof(SystemException))
                                                throw;

                                        }
                                    }
                                }
                                break;
                            }
                    }
                }
            }
            finally
            {
                foreach (ColumnHeader c in listViewFilesTimbres.Columns)
                    c.Width = -1;

                listViewFilesTimbres.EndUpdate();
            }
            //fileFolderList1.SelectedPath
        }

        private bool ignore_metroTextBox1_TextChanged;

        private void metroTextBox1_TextChanged(object sender, EventArgs e)
        {
            if (ignore_metroTextBox1_TextChanged)
                return;

            try
            {
                if (System.IO.Directory.Exists(metroTextBox1.Text))
                    fileFolderList1.Browse(metroTextBox1.Text);
            }
            catch { }
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


        private void listViewFilesTimbres_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ListViewItem[] items = new ListViewItem[listViewFilesTimbres.SelectedItems.Count];
                listViewFilesTimbres.SelectedItems.CopyTo(items, 0);
                listViewFilesTimbres.DoDragDrop(items, DragDropEffects.Copy | DragDropEffects.Move);
            }
        }

        private void listViewFilesTimbres_DragDrop(object sender, DragEventArgs e)
        {

        }

        private void listViewFilesTimbres_DragOver(object sender, DragEventArgs e)
        {

        }

        private void listViewFilesTimbres_DragEnter(object sender, DragEventArgs e)
        {

        }

        private void listViewCurrentTimbres_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ListViewItem[] items = new ListViewItem[listViewCurrentTimbres.SelectedItems.Count];
                listViewCurrentTimbres.SelectedItems.CopyTo(items, 0);
                listViewCurrentTimbres.DoDragDrop(items, DragDropEffects.Copy | DragDropEffects.Move);
            }
        }

        private void listViewCurrentTimbres_DragDrop(object sender, DragEventArgs e)
        {
            var items = e.Data.GetData(typeof(ListViewItem[])) as ListViewItem[];
            if (items != null && items.Length != 0)
            {
                Point cp = listViewCurrentTimbres.PointToClient(new Point(e.X, e.Y));
                ListViewItem dragToItem = listViewCurrentTimbres.GetItemAt(cp.X, cp.Y);
                if (dragToItem != null)
                {
                    bool exchange = false;
                    if (listViewCurrentTimbres == items[0].ListView && (e.KeyState & 4) == 4)
                        exchange = true;

                    for (int i = dragToItem.Index; i < Math.Min(dragToItem.Index + items.Length, listViewCurrentTimbres.Items.Count); i++)
                    {
                        TimbreItem tim = items[i - dragToItem.Index].Tag as TimbreItem;
                        if (tim != null)
                        {
                            string ss = tim.Timbre.SerializeData;
                            TimbreItem ttim = (TimbreItem)(listViewCurrentTimbres.Items[i].Tag);
                            if (exchange)
                            {
                                string tss = ttim.Timbre.SerializeData;

                                ttim.Timbre.SerializeData = ss;
                                tim.Timbre.SerializeData = tss;

                                listViewCurrentTimbres.Items[tim.Number].SubItems[1].Text = tim.Timbre.TimbreName;
                                listViewCurrentTimbres.Items[tim.Number].SubItems[2].Text = tim.Timbre.Memo;
                            }
                            else
                            {
                                ttim.Timbre.SerializeData = ss;

                            }
                            listViewCurrentTimbres.Items[i].SubItems[1].Text = ttim.Timbre.TimbreName;
                            listViewCurrentTimbres.Items[i].SubItems[2].Text = ttim.Timbre.Memo;
                        }
                    }
                }
            }
        }

        private void listViewCurrentTimbres_DragOver(object sender, DragEventArgs e)
        {
            var items = e.Data.GetData(typeof(ListViewItem[])) as ListViewItem[];
            if (items != null && items.Length != 0)
            {
                if (listViewCurrentTimbres == items[0].ListView && (e.KeyState & 4) == 4)
                    e.Effect = DragDropEffects.Move;
                else
                    e.Effect = DragDropEffects.Copy;
                try
                {
                    ignoreMetroComboBoxTimbres_SelectedIndexChanged = true;
                    listViewCurrentTimbres.SelectedItems.Clear();

                    Point cp = listViewCurrentTimbres.PointToClient(new Point(e.X, e.Y));
                    ListViewItem dragToItem = listViewCurrentTimbres.GetItemAt(cp.X, cp.Y);
                    if (dragToItem != null)
                    {
                        var index = dragToItem.Index;
                        var maxIndex = listViewCurrentTimbres.Items.Count - 1;
                        var scrollZoneHeight = listViewCurrentTimbres.Font.Height;
                        if (index > 0 && cp.Y < scrollZoneHeight)
                            listViewCurrentTimbres.Items[index - 1].EnsureVisible();
                        else if (index < maxIndex && cp.Y > listViewCurrentTimbres.Height - scrollZoneHeight)
                            listViewCurrentTimbres.Items[index + 1].EnsureVisible();

                        for (int i = dragToItem.Index; i < Math.Min(dragToItem.Index + items.Length, listViewCurrentTimbres.Items.Count); i++)
                            listViewCurrentTimbres.Items[i].Selected = true;
                    }
                }
                finally
                {
                    ignoreMetroComboBoxTimbres_SelectedIndexChanged = false;
                }
            }
        }

        private void metroButtonNewDir_Click(object sender, EventArgs e)
        {
            using (var f = new FormRename())
            {
                var result = f.ShowDialog();
                if (result == DialogResult.OK)
                {
                    if (!string.IsNullOrEmpty(f.InputText))
                    {
                        try
                        {
                            Directory.CreateDirectory(Path.Combine(fileFolderList1.CurrentDirectory, f.InputText));
                        }
                        catch (Exception ex)
                        {
                            if (ex.GetType() == typeof(Exception))
                                throw;
                            else if (ex.GetType() == typeof(SystemException))
                                throw;

                            MessageBox.Show(ex.Message);
                        }
                        fileFolderList1.Browse(fileFolderList1.CurrentDirectory);
                    }
                }
            }
        }

        private void metroButtonDelete_Click(object sender, EventArgs e)
        {
            if (fileFolderList1.SelectedPaths.Length == 0)
                return;

            var result = MessageBox.Show(this, "Are you sure you want to delete selected files/directories?",
                "Qeuestion", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

            if (result == DialogResult.OK)
            {
                for (int i = 0; i < fileFolderList1.SelectedPaths.Length; i++)
                {
                    try
                    {
                        string path = Path.Combine(fileFolderList1.CurrentDirectory, fileFolderList1.SelectedPaths[i]);
                        if (Directory.Exists(path))
                        {
                            Directory.Delete(path);
                        }
                        else if (File.Exists(path))
                        {
                            File.Delete(path);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex.GetType() == typeof(Exception))
                            throw;
                        else if (ex.GetType() == typeof(SystemException))
                            throw;

                        MessageBox.Show(ex.Message);
                    }
                }
                fileFolderList1.Browse(fileFolderList1.CurrentDirectory);
            }
        }

        private void metroButtonExport_Click(object sender, EventArgs e)
        {
            if (listViewCurrentTimbres.SelectedItems.Count == 0)
                return;

            if (listViewCurrentTimbres.SelectedItems.Count == 1)
            {
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    TimbreItem ttim = (TimbreItem)(listViewCurrentTimbres.SelectedItems[0].Tag);

                    string dir = fileFolderList1.CurrentDirectory;
                    if (string.IsNullOrWhiteSpace(dir))
                    {
                        dir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        dir = Path.Combine(dir, "MAmi");
                    }
                    saveFileDialog.InitialDirectory = dir;

                    saveFileDialog.DefaultExt = "*.msd";
                    saveFileDialog.Filter = "MAmi Serialize Data File(*.msd)|*.msd";
                    string fname = null;
                    try
                    {
                        fname = ttim.Timbre.TimbreName;
                    }
                    catch (Exception ex1)
                    {
                        if (ex1.GetType() == typeof(Exception))
                            throw;
                        else if (ex1.GetType() == typeof(SystemException))
                            throw;

                        try
                        {
                            if (ttim.Timbre.Memo?.Trim() != null)
                            {
                                StringReader rs = new StringReader(ttim.Timbre.Memo);
                                while (rs.Peek() > -1)
                                {
                                    fname = rs.ReadLine();
                                    break;
                                }
                            }
                        }
                        catch (Exception ex2)
                        {
                            if (ex2.GetType() == typeof(Exception))
                                throw;
                            else if (ex2.GetType() == typeof(SystemException))
                                throw;
                        }
                    }

                    fname = fname?.Trim();
                    if (string.IsNullOrWhiteSpace(fname))
                        fname = timbreType.FullName;

                    foreach (var invalidChar in Path.GetInvalidFileNameChars())
                        fname = fname.Replace(invalidChar.ToString(), "");

                    fname = Path.ChangeExtension(fname, ".msd");

                    saveFileDialog.FileName = fname;
                    saveFileDialog.SupportMultiDottedExtensions = true;

                    DialogResult res = saveFileDialog.ShowDialog();
                    if (res == DialogResult.OK)
                    {
                        fname = saveFileDialog.FileName;
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine(timbreType.FullName);
                        sb.AppendLine("1.0");

                        TimbreItem ttimi = (TimbreItem)(listViewCurrentTimbres.SelectedItems[0].Tag);
                        string sd = ttimi.Timbre.SerializeData;
                        sb.AppendLine(sd);

                        File.WriteAllText(fname, sb.ToString());
                    }
                }
            }
            else
            {
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    TimbreItem ttim = (TimbreItem)(listViewCurrentTimbres.SelectedItems[0].Tag);

                    string dir = fileFolderList1.CurrentDirectory;
                    if (string.IsNullOrWhiteSpace(dir))
                    {
                        dir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        dir = Path.Combine(dir, "MAmi");
                    }
                    saveFileDialog.InitialDirectory = dir;

                    saveFileDialog.DefaultExt = "*.msds";
                    saveFileDialog.Filter = "MAmi Serialize Data Files(*.msds)|*.msds";
                    string fname = null;
                    try
                    {
                        fname = ttim.Timbre.TimbreName;
                    }
                    catch (Exception ex1)
                    {
                        if (ex1.GetType() == typeof(Exception))
                            throw;
                        else if (ex1.GetType() == typeof(SystemException))
                            throw;

                        try
                        {
                            if (ttim.Timbre.Memo?.Trim() != null)
                            {
                                StringReader rs = new StringReader(ttim.Timbre.Memo);
                                while (rs.Peek() > -1)
                                {
                                    fname = rs.ReadLine();
                                    break;
                                }
                            }
                        }
                        catch (Exception ex2)
                        {
                            if (ex2.GetType() == typeof(Exception))
                                throw;
                            else if (ex2.GetType() == typeof(SystemException))
                                throw;
                        }
                    }

                    fname = fname?.Trim();
                    if (string.IsNullOrWhiteSpace(fname))
                        fname = timbreType.FullName;

                    foreach (var invalidChar in Path.GetInvalidFileNameChars())
                        fname = fname.Replace(invalidChar.ToString(), "");

                    fname = Path.ChangeExtension(fname, ".msds");

                    saveFileDialog.FileName = fname;
                    saveFileDialog.SupportMultiDottedExtensions = true;

                    DialogResult res = saveFileDialog.ShowDialog();
                    if (res == DialogResult.OK)
                    {
                        fname = saveFileDialog.FileName;
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine(timbreType.FullName);
                        sb.AppendLine("1.0");
                        sb.AppendLine(listViewCurrentTimbres.SelectedItems.Count.ToString());

                        for (int i = 0; i < listViewCurrentTimbres.SelectedItems.Count; i++)
                        {
                            TimbreItem ttimi = (TimbreItem)(listViewCurrentTimbres.SelectedItems[i].Tag);
                            string sd = ttimi.Timbre.SerializeData;
                            sb.AppendLine(sd);
                            sb.AppendLine("-");
                        }

                        File.WriteAllText(fname, sb.ToString());
                    }
                }

                /*
                bool? overwrite = null;
                for (int i = 0; i < listViewCurrentTimbres.SelectedItems.Count; i++)
                {
                    TimbreItem ttim = (TimbreItem)(listViewCurrentTimbres.SelectedItems[i].Tag);
                    string sd = ttim.Timbre.SerializeData;

                    StringBuilder serializeData = new StringBuilder();
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine(timbreType.FullName);
                    sb.AppendLine("1.0");
                    sb.AppendLine(sd);

                    string fname = ttim.Timbre.TimbreName;
                    if (string.IsNullOrEmpty(fname))
                    {
                        if (ttim.Timbre.Memo?.Trim() != null)
                        {
                            StringReader rs = new StringReader(ttim.Timbre.Memo);
                            while (rs.Peek() > -1)
                            {
                                fname = rs.ReadLine();
                                break;
                            }
                        }
                    }

                    fname = fname?.Trim();
                    if (string.IsNullOrWhiteSpace(fname))
                        fname = timbreType.FullName;

                    foreach (var invalidChar in Path.GetInvalidFileNameChars())
                        fname = fname.Replace(invalidChar.ToString(), "");

                    fname = Path.ChangeExtension(fname, ".msd");

                    fname = Path.Combine(fileFolderList1.CurrentDirectory, fname);
                    if (File.Exists(fname))
                    {
                        if (overwrite == null)
                        {
                            var result = MessageBox.Show(this, "Are you sure you want to overrite files?", "Qeuestion", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                            if (result == DialogResult.OK)
                                overwrite = true;
                            else
                                overwrite = false;
                        }
                        if (!overwrite.HasValue || !overwrite.Value)
                            continue;
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
                }*/
            }
            fileFolderList1.Browse(fileFolderList1.CurrentDirectory);
        }

        private ListView lastFocusedListView;

        private void listViewFilesTimbres_Enter(object sender, EventArgs e)
        {
            lastFocusedListView = listViewFilesTimbres;
        }

        private void listViewCurrentTimbres_Enter(object sender, EventArgs e)
        {
            lastFocusedListView = listViewCurrentTimbres;
        }

        private void fileFolderList1_CurrentDirectoryChanged(object sender, EventArgs e)
        {
            try
            {
                ignore_metroTextBox1_TextChanged = true;
                metroTextBox1.Text = fileFolderList1.CurrentDirectory;
            }
            finally
            {
                ignore_metroTextBox1_TextChanged = false;
            }
        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            betterFolderBrowser1.RootFolder = Settings.Default.ToneLibLastDir;

            var dr = betterFolderBrowser1.ShowDialog(this);
            if (dr == DialogResult.OK)
            {
                metroTextBox1.Text = betterFolderBrowser1.SelectedFolder;
            }
        }
    }
}
