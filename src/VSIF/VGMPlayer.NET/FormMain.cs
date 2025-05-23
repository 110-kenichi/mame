﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace zanac.VGMPlayer
{
    public partial class FormMain : Form
    {
        private SongBase currentSong;

        private ListViewItem currentSongItem;

        public static FormMain TopForm
        {
            get;
            private set;
        }

        public FormMain()
        {
            InitializeComponent();

            comboBoxDCSG.SelectedIndex = 0;
            comboBoxOPLL.SelectedIndex = 0;
            comboBoxOPNA2.SelectedIndex = 0;
            comboBoxSCC.SelectedIndex = 0;
            comboBoxSccType.SelectedIndex = 0;
            comboBoxSccSlot.SelectedIndex = 1;
            comboBoxY8910.SelectedIndex = 0;
            comboBoxOPM.SelectedIndex = 0;
            comboBoxOpmSlot.SelectedIndex = 0;
            comboBoxOPL3.SelectedIndex = 0;
            comboBoxOPNA.SelectedIndex = 0;
            comboBoxY8950.SelectedIndex = 0;

            listViewList.Columns[0].Width = -2;
            SetHeight(listViewList, SystemInformation.MenuHeight);
            listViewList.ListViewItemSorter = new ListViewIndexComparer(listViewList);

            TopForm = this;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            checkBoxConnDCSG.Checked = false;
            checkBoxConnOPLL.Checked = false;
            checkBoxConnOPNA2.Checked = false;
            checkBoxConnSCC.Checked = false;
            checkBoxConnY8910.Checked = false;
            checkBoxConnOPM.Checked = false;
            checkBoxConnOPL3.Checked = false;
            checkBoxConnOPNA.Checked = false;
            checkBoxConnY8950.Checked = false;

            //checkBoxConnDCSG_CheckedChanged(null, null);
            //checkBoxConnOPLL_CheckedChanged(null, null);
            //checkBoxConnOPNA2_CheckedChanged(null, null);
            //checkBoxConnSCC_CheckedChanged(null, null);
            //checkBoxConnY8910_CheckedChanged(null, null);

            try
            {
                listViewList.BeginUpdate();
                if (Settings.Default.Files != null)
                {
                    foreach (string fn in Settings.Default.Files)
                        listViewList.Items.Add(fn);

                    int idx = Settings.Default.FocusedItem;
                    if (0 <= idx && idx < listViewList.Items.Count)
                    {
                        listViewList.Items[idx].Focused = true;
                        listViewList.Items[idx].Selected = true;
                        listViewList.Items[idx].EnsureVisible();
                    }
                }
            }
            finally
            {
                listViewList.EndUpdate();
            }
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case NativeConstants.WM_APPCOMMAND:
                    int cmd = (int)((uint)m.LParam >> 16 & ~0xf000);
                    switch ((ApplicationCommand)cmd)
                    {
                        case ApplicationCommand.MediaFastForward:
                            buttonFast.PerformClick();
                            goto default;
                        case ApplicationCommand.MediaRewind:
                            buttonSlow.PerformClick();
                            goto default;
                        case ApplicationCommand.MediaPause:
                            buttonFreeze.PerformClick();
                            goto default;
                        case ApplicationCommand.MediaPlay:
                            buttonPlay.PerformClick();
                            goto default;
                        case ApplicationCommand.MediaPlayPause:
                            buttonPlay.PerformClick();
                            goto default;
                        case ApplicationCommand.MediaNexttrack:
                            buttonNext.PerformClick();
                            goto default;
                        case ApplicationCommand.MediaPrevioustrack:
                            buttonPrev.PerformClick();
                            goto default;
                        case ApplicationCommand.MediaStop:
                            buttonStop.PerformClick();
                            goto default;
                        case ApplicationCommand.Close:
                            Close();
                            goto default;
                        default:
                            /* According to MSDN, when handling
                             * this message, we must return TRUE. */
                            m.Result = new IntPtr(1);
                            base.WndProc(ref m);
                            return;
                    }
            }

            /* Other message handlers here… */

            base.WndProc(ref m);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            stopCurrentSong();

            comPortDCSG?.Dispose();
            comPortOPLL?.Dispose();
            comPortOPN2?.Dispose();
            comPortSCC?.Dispose();
            comPortY8910?.Dispose();
            comPortOPM?.Dispose();
            comPortOPL3?.Dispose();
            comPortOPNA?.Dispose();
            comPortY8950?.Dispose();

            StringCollection sc = new StringCollection();
            foreach (ListViewItem item in listViewList.Items)
                sc.Add(item.Text);
            Settings.Default.Files = sc;

            var fi = listViewList.FocusedItem;
            if (fi != null)
                Settings.Default.FocusedItem = fi.Index;
        }

        private void SetHeight(ListView listView, int height)
        {
            ImageList imgList = new ImageList();
            imgList.ImageSize = new Size(1, height);
            listView.SmallImageList = imgList;
        }

        private void buttonPlay_Click(object sender, EventArgs e)
        {
            if (currentSong == null ||
                listViewList.SelectedItems.Count != 0 &&
                listViewList.SelectedItems[0] != currentSongItem)
            {
                playSelectedItem();
            }
            else
            {
                if (currentSong?.State == SoundState.Paused || currentSong?.State == SoundState.Freezed)
                    currentSong?.Resume();
                else if (currentSong?.State == SoundState.Playing)
                    currentSong?.Pause();
                else
                    currentSong?.Play();
            }
        }


        private void buttonFreeze_Click(object sender, EventArgs e)
        {
            if (currentSong?.State == SoundState.Playing)
                currentSong?.Freeze();
            else
                currentSong?.Resume();
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            stopCurrentSong();
        }

        private void buttonSlow_Click(object sender, EventArgs e)
        {
            if (currentSong != null)
                currentSong.PlaybackSpeed -= 0.1f;
        }

        private void buttonFast_Click(object sender, EventArgs e)
        {
            if (currentSong != null)
                currentSong.PlaybackSpeed += 0.1f;
        }


        private void checkBoxLoop_CheckedChanged(object sender, EventArgs e)
        {
            if (currentSong != null)
                currentSong.LoopByCount = checkBoxLoop.Checked;
        }

        private void listViewList_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                addFilesToList((string[])e.Data.GetData(DataFormats.FileDrop, false));
            }
        }

        private void listView1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
            else
                e.Effect = DragDropEffects.None;
        }

        private void listViewList_ItemDrag(object sender, ItemDragEventArgs e)
        {
        }

        private void listViewList_DragLeave(object sender, EventArgs e)
        {
        }

        private void listViewList_DragOver(object sender, DragEventArgs e)
        {
        }

        private void addFilesToList(string[] files)
        {
            ListViewItem lvi = null;
            try
            {
                listViewList.BeginUpdate();

                listViewList.SelectedItems.Clear();
                lvi = addAllFiles(files, lvi);
            }
            finally
            {
                listViewList.EndUpdate();
                lvi?.EnsureVisible();
            }
        }

        private ListViewItem addAllFiles(IEnumerable<string> files, ListViewItem lvi)
        {
            foreach (var fileName in files)
            {
                var fp = Path.GetFullPath(fileName);
                if (File.Exists(fp))
                {
                    string ext = Path.GetExtension(fp);
                    switch (ext.ToUpper())
                    {
                        case ".VGM":
                        case ".VGZ":
                        case ".XGM":
                        case ".KSS":
                        case ".MGS":
                            lvi = new ListViewItem(fp);
                            listViewList.Items.Add(lvi);
                            lvi.Selected = true;
                            break;
                    }
                }
                if (Directory.Exists(fp))
                {
                    string[] allfiles = Directory.GetFiles(fp, "*.*", SearchOption.AllDirectories);
                    lvi = addAllFiles(allfiles, lvi);
                }
            }

            return lvi;
        }

        private void buttonPrev_Click(object sender, EventArgs e)
        {
            int idx = 0;
            if (currentSongItem != null)
            {
                idx = currentSongItem.Index;
                TimeSpan? ts = labelElapsed.Tag as TimeSpan?;
                if (ts != null && ts.Value.Seconds > 3)
                {
                    playItem(currentSongItem.Index);
                    return;
                }
            }
            if (idx < 0 && listViewList.Items.Count != 0)
            {
                playItem(0);
            }
            else
            {
                idx--;
                if (idx < 0)
                    idx = listViewList.Items.Count - 1;
                playItem(idx);
            }
        }

        private void buttonNext_Click(object sender, EventArgs e)
        {
            int idx = 0;
            if (currentSongItem != null)
                idx = currentSongItem.Index;
            if (idx < 0 && listViewList.Items.Count != 0)
            {
                playItem(0);
            }
            else
            {
                idx++;
                if (idx >= listViewList.Items.Count)
                    idx = 0;
                playItem(idx);
            }
        }

        private void playSelectedItem()
        {
            if (listViewList.SelectedItems.Count != 0)
            {
                currentSongItem = listViewList.SelectedItems[0];
                listViewList.SelectedItems.Clear();

                stopCurrentSong();
                playFile(currentSongItem.Text);
            }
        }

        private void playItem(int idx)
        {
            if (idx >= listViewList.Items.Count)
                idx = listViewList.Items.Count - 1;
            if (idx < 0)
                return;

            currentSongItem = listViewList.Items[idx];
            listViewList.SelectedItems.Clear();

            stopCurrentSong();
            playFile(currentSongItem.Text);
        }

        private void playFile(string fileName)
        {
            string ext = Path.GetExtension(fileName);
            try
            {
                switch (ext.ToUpper())
                {
                    case ".VGM":
                    case ".VGZ":
                        currentSong = new VGMSong(fileName);
                        break;
                    case ".XGM":
                        currentSong = new XGMSong(fileName);
                        break;
                    case ".KSS":
                    case ".MGS":
                        currentSong = new MGSSong(fileName);
                        break;
                }
                currentSong.ConvertChipClock = checkBoxCnvClk.Checked;
                currentSong.LoopByCount = checkBoxLoop.Checked;
                currentSong.LoopedCount = (int)numericUpDownLooped.Value;
                currentSong.LoopByElapsed = checkBoxLoopTimes.Checked;
                currentSong.LoopTimes = new TimeSpan(dateTimePickerLoopTimes.Value.Hour, dateTimePickerLoopTimes.Value.Minute, dateTimePickerLoopTimes.Value.Second);
                currentSong.ProcessLoadOccurred += CurrentSong_ProcessLoadOccurred;
                currentSong.PlayStatusChanged += CurrentSong_PlayStatusChanged;
                currentSong.SpeedChanged += CurrentSong_SpeedChanged;
                currentSong.Finished += CurrentSong_Finished;
                labelSpeed.Text = currentSong.PlaybackSpeed.ToString("0.00") + "x";
                textBoxTitle.Text = currentSong.FileName;
                currentSong.Play();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void CurrentSong_Finished(object sender, EventArgs e)
        {
            if (sender != currentSong)
                return;

            var cs = currentSong;

            await Task.Delay(500);

            if (this.IsDisposed)
                return;

            this.BeginInvoke(new MethodInvoker(() =>
            {
                if (this.IsDisposed)
                    return;
                if (cs != currentSong)
                    return;

                buttonNext_Click(null, null);
            }));
        }

        private void CurrentSong_SpeedChanged(object sender, EventArgs e)
        {
            if (sender != currentSong)
                return;

            labelSpeed.Text = currentSong.PlaybackSpeed.ToString("0.00") + "x";
        }

        private void CurrentSong_PlayStatusChanged(object sender, EventArgs e)
        {
            if (sender != currentSong)
                return;

            if (currentSong.State != SoundState.Playing)
                buttonPlay.ImageIndex = 0;
            else
                buttonPlay.ImageIndex = 1;
        }

        private void CurrentSong_ProcessLoadOccurred(object sender, EventArgs e)
        {
            if (sender != currentSong)
                return;

            if (this.IsDisposed)
                return;

            this.BeginInvoke(new MethodInvoker(() =>
            {
                if (progressBarLoad.IsDisposed)
                    return;
                if (progressBarLoad.Value <= 40)
                    setProgressBarLoadValue(50);
                else if (progressBarLoad.Value <= 90)
                    addProgressBarLoadValue(10);
            }));
        }

        private void stopCurrentSong()
        {
            if (currentSongItem != null)
            {
                currentSongItem.Selected = true;
                currentSongItem.EnsureVisible();
            }
            if (currentSong != null)
            {
                currentSong.Stop();
                currentSong.ProcessLoadOccurred -= CurrentSong_ProcessLoadOccurred;
                currentSong.PlayStatusChanged -= CurrentSong_PlayStatusChanged;
                currentSong.SpeedChanged -= CurrentSong_SpeedChanged;
                currentSong.Finished -= CurrentSong_Finished;
                currentSong.Dispose();
                currentSong = null;
                textBoxTitle.Text = string.Empty;
            }
        }

        private void listViewList_KeyDown(object sender, KeyEventArgs e)
        {
            // Deleteキーが押されたら項目を削除


            if (e.KeyData == Keys.Delete ||
                e.KeyData == Keys.Back ||
                (e.KeyCode == Keys.X && e.Control))
            {
                removeSelectedItem();
            }
            if (e.KeyCode == Keys.Enter)
            {
                playSelectedItem();
            }
            if (e.KeyCode == Keys.A && e.Control)
            {
                try
                {
                    listViewList.BeginUpdate();
                    foreach (ListViewItem item in listViewList.Items)
                        item.Selected = true;
                }
                finally
                {
                    listViewList.EndUpdate();
                }
            }
            if (e.KeyCode == Keys.V && e.Control)
            {
                System.Collections.Specialized.StringCollection files = Clipboard.GetFileDropList();
                if (files != null)
                {
                    ListViewItem lvi = null;
                    try
                    {
                        listViewList.BeginUpdate();

                        listViewList.SelectedItems.Clear();
                        lvi = addAllFiles(files.Cast<string>().ToArray(), lvi);
                    }
                    finally
                    {
                        listViewList.EndUpdate();
                        lvi?.EnsureVisible();
                    }
                }
            }
        }

        private void removeSelectedItem()
        {
            try
            {
                listViewList.BeginUpdate();
                int index = 0;
                for (int i = 0; i < listViewList.SelectedItems.Count; i++)
                {
                    // 現在選択している行のインデックスを取得
                    index = listViewList.SelectedItems[0].Index;
                    if ((0 <= index) && (index < listViewList.Items.Count))
                    {
                        listViewList.Items.RemoveAt(index);
                        i--;
                    }
                }
                if (index < listViewList.Items.Count)
                {
                    listViewList.Items[index].Selected = true;
                    listViewList.Items[index].EnsureVisible();
                }
                else if (listViewList.Items.Count != 0)
                {
                    listViewList.Items[listViewList.Items.Count - 1].Selected = true;
                    listViewList.Items[listViewList.Items.Count - 1].EnsureVisible();
                }
            }
            finally
            {
                listViewList.EndUpdate();
            }
        }

        private void listViewList_DoubleClick(object sender, EventArgs e)
        {
            playSelectedItem();
        }

        private void listViewList_SizeChanged(object sender, EventArgs e)
        {
            listViewList.Columns[0].Width = -2;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (currentSong?.HighLoad == true && currentSong?.State == SoundState.Playing)
            {
                if (progressBarLoad.Value <= 90)
                    addProgressBarLoadValue(10);
            }
            else if (progressBarLoad.Value >= 10)
                addProgressBarLoadValue(-10);
        }

        private void setProgressBarLoadValue(int value)
        {
            progressBarLoad.Value = value;
            progressBarLoad.ForeColor =
                System.Drawing.Color.FromArgb(255, (255 * (100 - progressBarLoad.Value)) / 100, 0);
        }

        private void addProgressBarLoadValue(int value)
        {
            progressBarLoad.Value += value;
            progressBarLoad.ForeColor =
                System.Drawing.Color.FromArgb(255, (255 * (100 - progressBarLoad.Value)) / 100, 0);
        }

        private void FormMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (!e.Control)
            {
                switch (e.KeyCode)
                {
                    //矢印キーが押されたことを表示する
                    case Keys.Z:
                        buttonPrev_Click(null, null);
                        e.Handled = true;
                        break;
                    case Keys.X:
                        buttonPlay_Click(null, null);
                        e.Handled = true;
                        break;
                    case Keys.C:
                        buttonNext_Click(null, null);
                        e.Handled = true;
                        break;
                    case Keys.V:
                        buttonStop_Click(null, null);
                        e.Handled = true;
                        break;
                    case Keys.B:
                        buttonFreeze_Click(null, null);
                        e.Handled = true;
                        break;
                    case Keys.N:
                        buttonSlow_Click(null, null);
                        e.Handled = true;
                        break;
                    case Keys.M:
                        buttonFast_Click(null, null);
                        e.Handled = true;
                        break;
                    case Keys.Oemcomma:
                        checkBoxLoop.Checked = !checkBoxLoop.Checked;
                        e.Handled = true;
                        break;
                }
            }
        }

        private VsifClient comPortDCSG;

        private void checkBoxConnDCSG_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxConnDCSG.Checked)
            {
                switch (Settings.Default.DCSG_IF)
                {
                    case 0:
                        comPortDCSG = VsifManager.TryToConnectVSIF(VsifSoundModuleType.Genesis,
                            (PortId)Settings.Default.DCSG_Port, false);
                        break;
                    case 1:
                        comPortDCSG = VsifManager.TryToConnectVSIF(VsifSoundModuleType.Genesis_FTDI,
                            (PortId)Settings.Default.DCSG_Port, false);
                        break;
                    case 2:
                        comPortDCSG = VsifManager.TryToConnectVSIF(VsifSoundModuleType.SMS,
                            (PortId)Settings.Default.DCSG_Port, false);
                        break;
                    case 3:
                        comPortDCSG = VsifManager.TryToConnectVSIF(VsifSoundModuleType.Genesis_Low,
                            (PortId)Settings.Default.DCSG_Port, false);
                        break;
                    case 4:
                        comPortDCSG = VsifManager.TryToConnectVSIF(VsifSoundModuleType.MSX_FTDI,
                            (PortId)Settings.Default.DCSG_Port, false);
                        break;
                }

                checkBoxConnDCSG.Checked = comPortDCSG != null;
                comboBoxDCSG.Enabled = comPortDCSG == null;
                comboBoxPortSN76489.Enabled = comPortDCSG == null;
            }
            else
            {
                comboBoxDCSG.Enabled = true;
                comboBoxPortSN76489.Enabled = true;
                comPortDCSG?.Dispose();
            }
        }

        private VsifClient comPortOPLL;

        private void checkBoxConnOPLL_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxConnOPLL.Checked)
            {
                switch (Settings.Default.OPLL_IF)
                {
                    case 0:
                        comPortOPLL = VsifManager.TryToConnectVSIF(VsifSoundModuleType.SMS,
                            (PortId)Settings.Default.OPLL_Port, false);
                        break;
                    case 1:
                        comPortOPLL = VsifManager.TryToConnectVSIF(VsifSoundModuleType.MSX_FTDI,
                            (PortId)Settings.Default.OPLL_Port, false);
                        if (comPortOPLL != null)
                            comPortOPLL.Tag["OPLL.Slot"] = comboBoxOpllSlot.SelectedIndex;
                        break;
                }

                checkBoxConnOPLL.Checked = comPortOPLL != null;
                comboBoxOPLL.Enabled = comPortOPLL == null;
                comboBoxPortYm2413.Enabled = comPortOPLL == null;
                comboBoxOpllSlot.Enabled = comPortOPLL == null;
            }
            else
            {
                comboBoxOPLL.Enabled = true;
                comboBoxPortYm2413.Enabled = true;
                comPortOPLL?.Dispose();
                comboBoxOpllSlot.Enabled = true;
            }
        }

        private VsifClient comPortOPN2;

        private void checkBoxConnOPNA2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxConnOPNA2.Checked)
            {
                switch (Settings.Default.OPNA2_IF)
                {
                    case 0:
                        comPortOPN2 = VsifManager.TryToConnectVSIF(VsifSoundModuleType.Genesis,
                            (PortId)Settings.Default.OPNA2_Port, false);
                        break;
                    case 1:
                        comPortOPN2 = VsifManager.TryToConnectVSIF(VsifSoundModuleType.Genesis_FTDI,
                            (PortId)Settings.Default.OPNA2_Port, false);
                        break;
                    case 2:
                        comPortOPN2 = VsifManager.TryToConnectVSIF(VsifSoundModuleType.Genesis_Low,
                            (PortId)Settings.Default.OPNA2_Port, false);
                        break;
                    case 3:
                        comPortOPN2 = VsifManager.TryToConnectVSIF(VsifSoundModuleType.MSX_FTDI,
                            (PortId)Settings.Default.OPNA2_Port, false);
                        break;
                }
                checkBoxConnOPNA2.Checked = comPortOPN2 != null;
                comboBoxOPNA2.Enabled = comPortOPN2 == null;
                comboBoxPortYM2612.Enabled = comPortOPN2 == null;
            }
            else
            {
                comboBoxOPNA2.Enabled = true;
                comboBoxPortYM2612.Enabled = true;
                comPortOPN2?.Dispose();
            }
        }

        private VsifClient comPortSCC;

        private void checkBoxConnSCC_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxConnSCC.Checked)
            {
                switch (Settings.Default.SCC_IF)
                {
                    case 0:
                        comPortSCC = VsifManager.TryToConnectVSIF(VsifSoundModuleType.MSX_FTDI,
                            (PortId)Settings.Default.SCC_Port, false);
                        if (comPortSCC != null)
                        {
                            comPortSCC.Tag["SCC.Type"] = (SCCType)(comboBoxSccType.SelectedIndex + 1);
                            if (comboBoxSccSlot.SelectedIndex < 2)
                                comPortSCC.Tag["SCC.Slot"] = comboBoxSccSlot.SelectedIndex - 2;
                            else
                                comPortSCC.Tag["SCC.Slot"] = SCCSlotNo[comboBoxSccSlot.SelectedIndex - 2];
                        }
                        break;
                }
                checkBoxConnSCC.Checked = comPortSCC != null;
                comboBoxSCC.Enabled = comPortSCC == null;
                comboBoxPortSCC.Enabled = comPortSCC == null;
                comboBoxSccSlot.Enabled = comPortSCC == null;
                comboBoxSccType.Enabled = comPortSCC == null;
            }
            else
            {
                comboBoxSCC.Enabled = true;
                comboBoxPortSCC.Enabled = true;
                comboBoxSccSlot.Enabled = true;
                comboBoxSccType.Enabled = true;

                comPortSCC?.Dispose();
            }
        }

        private byte[] SCCSlotNo = new byte[]
        {
            0b0000_0000,
            0b1000_0000,
            0b1000_0100,
            0b1000_1000,
            0b1000_1100,

            0b0000_0001,
            0b1000_0001,
            0b1000_0101,
            0b1000_1001,
            0b1000_1101,

            0b0000_0010,
            0b1000_0010,
            0b1000_0110,
            0b1000_1010,
            0b1000_1110,

            0b0000_0011,
            0b1000_0011,
            0b1000_0111,
            0b1000_1011,
            0b1000_1111,
        };

        /// <summary>
        /// 
        /// </summary>
        public enum SCCType
        {
            SCC1 = 1,
            SCC1_Compat = 2,
            SCC = 3,
        }

        private VsifClient comPortY8910;

        private void checkBoxConnY8910_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxConnY8910.Checked)
            {
                switch (Settings.Default.Y8910_IF)
                {
                    case 0:
                        comPortY8910 = VsifManager.TryToConnectVSIF(VsifSoundModuleType.MSX_FTDI,
                            (PortId)Settings.Default.Y8910_Port, false);
                        break;
                    case 1:
                        comPortY8910 = VsifManager.TryToConnectVSIF(VsifSoundModuleType.Generic_UART,
                            (PortId)Settings.Default.Y8910_Port, false);
                        break;
                }
                checkBoxConnY8910.Checked = comPortY8910 != null;
                comboBoxY8910.Enabled = comPortY8910 == null;
                comboBoxPortY8910.Enabled = comPortY8910 == null;
            }
            else
            {
                comboBoxY8910.Enabled = true;
                comboBoxPortY8910.Enabled = true;
                comPortY8910?.Dispose();
            }
        }



        private VsifClient comPortOPM;

        private void checkBoxConnOPM_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxConnOPM.Checked)
            {
                switch (Settings.Default.OPM_IF)
                {
                    case 0:
                        comPortOPM = VsifManager.TryToConnectVSIF(VsifSoundModuleType.MSX_FTDI,
                            (PortId)Settings.Default.OPM_Port, false);
                        if (comPortOPM != null)
                        {
                            comPortOPM.Tag["OPM.Slot"] = comboBoxOpmSlot.SelectedIndex;
                            comPortOPM.DeferredWriteData(0xd, 0, (byte)comboBoxOpmSlot.SelectedIndex, (int)Settings.Default.BitBangWaitOPM);
                        }
                        break;
                }
                checkBoxConnOPM.Checked = comPortOPM != null;
                comboBoxOPM.Enabled = comPortOPM == null;
                comboBoxPortOPM.Enabled = comPortOPM == null;
                comboBoxOpmSlot.Enabled = comPortOPM == null;
            }
            else
            {
                comboBoxOPM.Enabled = true;
                comboBoxPortOPM.Enabled = true;
                comboBoxOpmSlot.Enabled = true;
                comPortOPM?.Dispose();
            }
        }

        private VsifClient comPortOPL3;

        private void checkBoxConnOPL3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxConnOPL3.Checked)
            {
                switch (Settings.Default.OPL3_IF)
                {
                    case 0:
                        comPortOPL3 = VsifManager.TryToConnectVSIF(VsifSoundModuleType.MSX_FTDI,
                            (PortId)Settings.Default.OPL3_Port, false);
                        break;
                }
                checkBoxConnOPL3.Checked = comPortOPL3 != null;
                comboBoxOPL3.Enabled = comPortOPL3 == null;
                comboBoxPortOPL3.Enabled = comPortOPL3 == null;
            }
            else
            {
                comboBoxOPL3.Enabled = true;
                comboBoxPortOPL3.Enabled = true;
                comPortOPL3?.Dispose();
            }
        }

        private VsifClient comPortOPNA;

        private void checkBoxOPNA_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxConnOPNA.Checked)
            {
                switch (Settings.Default.OPNA_IF)
                {
                    case 0:
                        comPortOPNA = VsifManager.TryToConnectVSIF(VsifSoundModuleType.MSX_FTDI,
                            (PortId)Settings.Default.OPNA_Port, false);
                        break;
                }
                checkBoxConnOPNA.Checked = comPortOPNA != null;
                comboBoxOPNA.Enabled = comPortOPNA == null;
                comboBoxPortOPNA.Enabled = comPortOPNA == null;
            }
            else
            {
                comboBoxOPNA.Enabled = true;
                comboBoxPortOPNA.Enabled = true;
                comPortOPNA?.Dispose();
            }
        }

        private VsifClient comPortY8950;

        private void checkBoxConnY8950_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxConnY8950.Checked)
            {
                switch (Settings.Default.Y8950_IF)
                {
                    case 0:
                        comPortY8950 = VsifManager.TryToConnectVSIF(VsifSoundModuleType.MSX_FTDI,
                            (PortId)Settings.Default.Y8950_Port, false);
                        if (comPortY8950 != null)
                            comPortY8950.Tag["Y8950.Slot"] = comboBoxY8950Slot.SelectedIndex;
                        break;
                }
                checkBoxConnY8950.Checked = comPortY8950 != null;
                comboBoxY8950.Enabled = comPortY8950 == null;
                comboBoxPortY8950.Enabled = comPortY8950 == null;
                comboBoxY8950Slot.Enabled = comPortY8950 == null;
            }
            else
            {
                comboBoxY8950.Enabled = true;
                comboBoxPortY8950.Enabled = true;
                comboBoxY8950Slot.Enabled = true;
                comPortY8950?.Dispose();
            }
        }

        private void listViewList_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            switch (listViewList.Sorting)
            {
                case SortOrder.Ascending:
                    listViewList.Sorting = SortOrder.Descending;
                    listViewList.Columns[0].Text = "File name ▼";
                    break;
                case SortOrder.Descending:
                    listViewList.Sorting = SortOrder.None;
                    listViewList.Columns[0].Text = "File name";
                    break;
                case SortOrder.None:
                    listViewList.Sorting = SortOrder.Ascending;
                    listViewList.Columns[0].Text = "File name ▲";
                    break;
            }
        }

        private void buttonPlay_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
            else
                e.Effect = DragDropEffects.None;
        }

        private void buttonPlay_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                listViewList.BeginUpdate();

                listViewList.Items.Clear();

                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);
                ListViewItem lvi = null;
                try
                {
                    listViewList.BeginUpdate();

                    listViewList.SelectedItems.Clear();
                    lvi = addAllFiles(files, lvi);
                }
                finally
                {
                    listViewList.EndUpdate();
                    lvi?.EnsureVisible();
                }
            }
            finally
            {
                listViewList.EndUpdate();
            }

            playSelectedItem();
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            try
            {
                listViewList.BeginUpdate();
                listViewList.Items.Clear();
            }
            finally
            {
                listViewList.EndUpdate();
            }
        }

        private void playToolStripMenuItem_Click(object sender, EventArgs e)
        {
            playSelectedItem();
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            removeSelectedItem();
        }

        private void explorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = listViewList.FocusedItem;
            if (item != null)
            {
                Task.Run(new Action(() =>
                {
                    Process.Start("explorer.exe", "/select,\"" + item.Text + "\"");
                }));
            }
        }

        private void buttonEject_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(openFileDialog.InitialDirectory))
            {
                if (String.IsNullOrEmpty(Settings.Default.LastDir) || !Directory.Exists(Settings.Default.LastDir))
                    openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                else
                    openFileDialog.InitialDirectory = Settings.Default.LastDir;
            }

            //ダイアログを表示する
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                addFilesToList(openFileDialog.FileNames);

                openFileDialog.InitialDirectory = Path.GetDirectoryName(openFileDialog.FileName);
                Settings.Default.LastDir = openFileDialog.InitialDirectory;
            }
        }

        private class ListViewIndexComparer : System.Collections.IComparer
        {
            private ListView listView;

            public ListViewIndexComparer(ListView listView)
            {
                this.listView = listView;
            }

            public int Compare(object x, object y)
            {
                if (listView.Sorting == SortOrder.Ascending)
                    return ((ListViewItem)x).Text.CompareTo(((ListViewItem)y).Text);
                else if (listView.Sorting == SortOrder.Descending)
                    return ((ListViewItem)y).Text.CompareTo(((ListViewItem)x).Text);
                else
                    return 0;
            }
        }

        private void aBOUTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormAbout ab = new FormAbout();
            ab.Text = this.Text;
            ab.ShowDialog();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        public void SetStatusText(string text)
        {
            BeginInvoke(new MethodInvoker(() =>
            {
                if (!IsDisposed)
                {
                    toolStripStatusLabel.Text = text;
                }
            }));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        public void SetElapsedTime(TimeSpan timeSpan)
        {
            labelElapsed.BeginInvoke(new MethodInvoker(() =>
            {
                if (!labelElapsed.IsDisposed)
                {
                    labelElapsed.Text = timeSpan.ToString(@"hh\:mm\:ss");
                    labelElapsed.Tag = timeSpan;
                    toolStripStatusLabel.Text = "Playing";
                }
            }));
        }

        private void numericUpDownLooped_ValueChanged(object sender, EventArgs e)
        {
            if (currentSong != null)
                currentSong.LoopedCount = (int)numericUpDownLooped.Value;
        }

        private void dateTimePickerLoopTimes_ValueChanged(object sender, EventArgs e)
        {
            if (currentSong != null)
                currentSong.LoopTimes = new TimeSpan(dateTimePickerLoopTimes.Value.Hour, dateTimePickerLoopTimes.Value.Minute, dateTimePickerLoopTimes.Value.Second);
        }

        private void checkBoxLoopTimes_CheckedChanged(object sender, EventArgs e)
        {
            if (currentSong != null)
                currentSong.LoopByElapsed = checkBoxLoopTimes.Checked;
        }

        private void label3_Click(object sender, EventArgs e)
        {
            int row = tableLayoutPanelPort.GetRow((Control)sender);
            CheckBox cb = (CheckBox)tableLayoutPanelPort.GetControlFromPosition(0, row);
            cb.Checked = !cb.Checked;
        }

        private void label3_DoubleClick(object sender, EventArgs e)
        {
            int row = tableLayoutPanelPort.GetRow((Control)sender);
            CheckBox cb = (CheckBox)tableLayoutPanelPort.GetControlFromPosition(0, row);
            cb.Checked = !cb.Checked;
        }

        private void label8_DoubleClick(object sender, EventArgs e)
        {
            for (int i = 0; i < tableLayoutPanelPort.RowCount; i++)
            {
                CheckBox cb = tableLayoutPanelPort.GetControlFromPosition(0, i) as CheckBox;
                if (cb != null)
                    cb.Checked = !cb.Checked;
            }
        }

        private void checkBoxCnvClk_CheckedChanged(object sender, EventArgs e)
        {
            if (currentSong != null)
                currentSong.ConvertChipClock = checkBoxCnvClk.Checked;
        }
    }

    internal static class NativeConstants
    {
        public const int WM_APPCOMMAND = 0x0319;
    }

    internal enum ApplicationCommand
    {
        VolumeMute = 8,
        VolumeDown = 9,
        VolumeUp = 10,
        MediaNexttrack = 11,
        MediaPrevioustrack = 12,
        MediaStop = 13,
        MediaPlayPause = 14,
        Close = 31,
        MediaPlay = 46,
        MediaPause = 47,
        MediaFastForward = 49,
        MediaRewind = 50
    }
}
