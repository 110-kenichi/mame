using System;
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
using zanac.VGMPlayer.Properties;

namespace zanac.VGMPlayer
{
    public partial class FormMain : Form
    {
        private SongBase currentSong;

        private ListViewItem currentSongItem;

        public FormMain()
        {
            InitializeComponent();

            comboBoxOPLL.SelectedIndex = 0;
            comboBoxOPNA2.SelectedIndex = 0;

            listViewList.Columns[0].Width = -2;
            SetHeight(listViewList, SystemInformation.MenuHeight);

            if (Settings.Default.Files != null)
            {
                foreach (string fn in Settings.Default.Files)
                    listViewList.Items.Add(fn);

                int idx = Settings.Default.FocusedItem;
                if (0 <= idx && idx < listViewList.Items.Count)
                {
                    listViewList.Items[idx].Focused = true;
                    listViewList.Items[idx].Selected = true;
                }
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            checkBoxConnDCSG_CheckedChanged(null, null);
            checkBoxConnOPLL_CheckedChanged(null, null);
            checkBoxConnOPNA2_CheckedChanged(null, null);
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
                if (currentSong?.State == SoundState.Paused)
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
                currentSong.Looped = checkBoxLoop.Checked;
        }

        private void listView1_DragDrop(object sender, DragEventArgs e)
        {
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

        private ListViewItem addAllFiles(IEnumerable<string> files, ListViewItem lvi)
        {
            foreach (var fileName in files)
            {
                if (File.Exists(fileName))
                {
                    string ext = Path.GetExtension(fileName);
                    switch (ext.ToUpper())
                    {
                        case ".VGM":
                        case ".VGZ":
                        case ".XGM":
                            lvi = new ListViewItem(fileName);
                            listViewList.Items.Add(lvi);
                            lvi.Selected = true;
                            break;
                    }
                }
                if (Directory.Exists(fileName))
                {
                    string[] allfiles = Directory.GetFiles(fileName, "*.*", SearchOption.AllDirectories);
                    lvi = addAllFiles(allfiles, lvi);
                }
            }

            return lvi;
        }

        private void listView1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
            else
                e.Effect = DragDropEffects.None;
        }

        private void buttonPrev_Click(object sender, EventArgs e)
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
                }
                currentSong.Looped = checkBoxLoop.Checked;
                currentSong.LoopCount = (int)numericUpDown1.Value;
                currentSong.ProcessLoadOccurred += CurrentSong_ProcessLoadOccurred;
                currentSong.PlayStatusChanged += CurrentSong_PlayStatusChanged;
                currentSong.SpeedChanged += CurrentSong_SpeedChanged;
                currentSong.Finished += CurrentSong_Finished;
                labelSpeed.Text = currentSong.PlaybackSpeed.ToString("0.00");
                textBoxTitle.Text = currentSong.FileName;
                currentSong.Play();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void CurrentSong_Finished(object sender, EventArgs e)
        {
            if (sender != currentSong)
                return;

            if (this.IsDisposed)
                return;

            this.BeginInvoke(new MethodInvoker(() =>
            {
                buttonNext_Click(null, null);
            }));
        }

        private void CurrentSong_SpeedChanged(object sender, EventArgs e)
        {
            if (sender != currentSong)
                return;

            labelSpeed.Text = currentSong.PlaybackSpeed.ToString("0.00");
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
                            (ComPort)Settings.Default.DCSG_Port, false);
                        break;
                    case 1:
                        comPortDCSG = VsifManager.TryToConnectVSIF(VsifSoundModuleType.Genesis_FTDI,
                            (ComPort)Settings.Default.DCSG_Port, false);
                        break;
                    case 2:
                        comPortDCSG = VsifManager.TryToConnectVSIF(VsifSoundModuleType.SMS,
                            (ComPort)Settings.Default.DCSG_Port, false);
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
                comPortOPLL = VsifManager.TryToConnectVSIF(VsifSoundModuleType.SMS,
                    (ComPort)Settings.Default.OPLL_Port, false);

                checkBoxConnOPLL.Checked = comPortOPLL != null;
                comboBoxOPLL.Enabled = comPortOPLL == null;
                comboBoxPortYm2413.Enabled = comPortOPLL == null;
            }
            else
            {
                comboBoxOPLL.Enabled = true;
                comboBoxPortYm2413.Enabled = true;
                comPortOPLL?.Dispose();
            }
        }

        private VsifClient comPortOPNA2;

        private void checkBoxConnOPNA2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxConnOPNA2.Checked)
            {
                switch (Settings.Default.OPNA2_IF)
                {
                    case 0:
                        comPortOPNA2 = VsifManager.TryToConnectVSIF(VsifSoundModuleType.Genesis,
                            (ComPort)Settings.Default.OPNA2_Port, false);
                        break;
                    case 1:
                        comPortOPNA2 = VsifManager.TryToConnectVSIF(VsifSoundModuleType.Genesis_FTDI,
                            (ComPort)Settings.Default.OPNA2_Port, false);
                        break;
                }
                checkBoxConnOPNA2.Checked = comPortOPNA2 != null;
                comboBoxOPNA2.Enabled = comPortOPNA2 == null;
                comboBoxPortYM2612.Enabled = comPortOPNA2 == null;
            }
            else
            {
                comboBoxOPNA2.Enabled = true;
                comboBoxPortYM2612.Enabled = true;
                comPortOPNA2?.Dispose();
            }
        }

        private void listViewList_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (listViewList.Sorting != SortOrder.Ascending)
                listViewList.Sorting = SortOrder.Ascending;
            else
                listViewList.Sorting = SortOrder.Descending;
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

            playSelectedItem();
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            listViewList.Items.Clear();
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
