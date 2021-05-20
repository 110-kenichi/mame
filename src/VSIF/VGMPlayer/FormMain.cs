using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
        }

        private bool isShown;

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            isShown = true;

            checkBoxConnDCSG_CheckedChanged(null, null);
            checkBoxConnOPLL_CheckedChanged(null, null);
            checkBoxConnOPNA2_CheckedChanged(null, null);
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
                    return addAllFiles(allfiles, lvi);
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
            int idx = currentSongItem.Index;
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
            int idx = currentSongItem.Index;
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
                buttonPlay.ImageIndex = 1;
            else
                buttonPlay.ImageIndex = 2;
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
            currentSongItem.Selected = true;
            currentSongItem.EnsureVisible();
            currentSong?.Stop();
            if (currentSong != null)
            {
                currentSong.ProcessLoadOccurred -= CurrentSong_ProcessLoadOccurred;
                currentSong.PlayStatusChanged -= CurrentSong_PlayStatusChanged;
                currentSong.SpeedChanged -= CurrentSong_SpeedChanged;
                currentSong.Finished -= CurrentSong_Finished;
                textBoxTitle.Text = string.Empty;
            }
            currentSong?.Dispose();
            currentSong = null;
        }

        private void listViewList_KeyDown(object sender, KeyEventArgs e)
        {
            // Deleteキーが押されたら項目を削除


            if (e.KeyData == Keys.Delete ||
                e.KeyData == Keys.Back ||
                (e.KeyCode == Keys.X && e.Control))
            {
                try
                {
                    //listViewList.BeginUpdate();
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
                    //listViewList.EndUpdate();
                }
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
            if (currentSong?.Wait < 0 && currentSong?.State == SoundState.Playing)
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
            }
            else
            {
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
            }
            else
            {
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
            }
            else
            {
                comPortOPNA2?.Dispose();
            }
        }

        private void comboBoxOPNA2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!isShown)
                return;
            checkBoxConnOPNA2.Checked = false;
        }

        private void comboBoxOPLL_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!isShown)
                return;
            checkBoxConnOPLL.Checked = false;
        }

        private void comboBoxDCSG_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!isShown)
                return;
            checkBoxConnDCSG.Checked = false;
        }

        private void comboBoxPortYM2612_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!isShown)
                return;
            checkBoxConnOPNA2.Checked = false;
        }

        private void comboBoxPortYm2413_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!isShown)
                return;
            checkBoxConnOPLL.Checked = false;
        }

        private void comboBoxPortSN76489_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!isShown)
                return;
            checkBoxConnDCSG.Checked = false;
        }
    }
}
