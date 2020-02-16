﻿// copyright-holders:K.Ito
using Melanchall.DryWetMidi.Devices;
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
using zanac.MAmidiMEmo.Mame;
using zanac.MAmidiMEmo.Midi;
using zanac.MAmidiMEmo.Properties;
using zanac.MAmidiMEmo.Instruments;
using zanac.MAmidiMEmo.ComponentModel;
using Newtonsoft.Json;
using System.IO;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Common;
using System.Reflection;

namespace zanac.MAmidiMEmo.Gui
{
    public partial class FormMain : Form
    {

        private static ListView outputListView;

        private static StreamWriter logStream;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="log"></param>
        [Conditional("DEBUG")]
        public static void OutputDebugLogFile(String log)
        {
            if (logStream == null)
                logStream = new StreamWriter("log.txt");
            logStream.WriteLine(log);
            logStream.Flush();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="log"></param>
        [Conditional("DEBUG")]
        public static void OutputDebugLog(String log)
        {
            if (outputListView == null || outputListView.IsDisposed || !outputListView.IsHandleCreated)
                return;

            outputListView?.BeginInvoke(new MethodInvoker(() =>
            {
                if (outputListView.IsDisposed)
                    return;

                var item = outputListView.Items.Add(log);
                outputListView.EnsureVisible(item.Index);
            }), null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="log"></param>
        public static void OutputLog(String log)
        {
            if (outputListView.IsDisposed)
                return;

            outputListView?.BeginInvoke(new MethodInvoker(() =>
            {
                var item = outputListView.Items.Add(log);
                outputListView.EnsureVisible(item.Index);
            }), null);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public FormMain()
        {
            InitializeComponent();

            //Images
            imageList1.Images.Add("YM2612", Resources.YM2612);
            imageList1.Images.Add("YM2151", Resources.YM2151);
            imageList1.Images.Add("SN76496", Resources.SN76496);
            imageList1.Images.Add("NAMCO_CUS30", Resources.NAMCO_CUS30);
            imageList1.Images.Add("GB_APU", Resources.GB_APU);
            imageList1.Images.Add("RP2A03", Resources.RP2A03);
            imageList1.Images.Add("SCC1", Resources.SCC1);
            imageList1.Images.Add("YM3812", Resources.YM3812);
            imageList1.Images.Add("YM2413", Resources.YM2413);
            imageList1.Images.Add("MSM5232", Resources.MSM5232);
            imageList1.Images.Add("AY-3-8910", Resources.AY_3_8910);
            imageList1.Images.Add("MOS8580", Resources.MOS8580);
            imageList1.Images.Add("MOS6581", Resources.MOS6581);
            imageList1.Images.Add("Beep", Resources.Beep);
            imageList1.Images.Add("C140", Resources.C140);
            imageList1.Images.Add("HuC6280", Resources.HuC6280);
            imageList1.Images.Add("SPC700", Resources.SPC700);
            imageList1.Images.Add("POKEY", Resources.POKEY);
            imageList1.Images.Add("YM2610", Resources.YM2612);

            //Set MIDI I/F
            foreach (var dev in InputDevice.GetAll())
            {
                int idx = toolStripComboBoxMidiIf.Items.Add(dev.Name);
                if (string.Equals(dev.Name, Settings.Default.MidiIF))
                    toolStripComboBoxMidiIf.SelectedIndex = idx;
                dev.Dispose();
            }
            if (toolStripComboBoxMidiIf.Items.Count < 1)
            {
                MessageBox.Show("There are no MIDI IN devices.\r\nPlease install at lease one MIDI IN device to use the MAmidiMEmo.\r\nOr, install the loopMIDI to the PC.");
            }
            outputListView = listView1;

            //MIDI Event
            InstrumentManager_InstrumentChanged(null, null);
            InstrumentManager.InstrumentChanged += InstrumentManager_InstrumentChanged;
            InstrumentManager.InstrumentAdded += InstrumentManager_InstrumentAdded;
            InstrumentManager.InstrumentRemoved += InstrumentManager_InstrumentRemoved;

            toolStripComboBox1.SelectedIndex = 0;
            toolStripComboBox2.SelectedIndex = 0;

            pianoControl1.NoteOn += PianoControl1_NoteOn;
            pianoControl1.NoteOff += PianoControl1_NoteOff;
        }

        private void PianoControl1_NoteOn(object sender, NoteOnEvent e)
        {
            if (toolStripComboBox2.SelectedIndex != 0)
            {
                //Program change
                var pe = new ProgramChangeEvent((SevenBitNumber)(toolStripComboBox2.SelectedIndex - 1));
                pe.Channel = (FourBitNumber)(toolStripComboBox1.SelectedIndex);
                MidiManager.SendMidiEvent(pe);
            }
            e.Channel = (FourBitNumber)(toolStripComboBox1.SelectedIndex);
            MidiManager.SendMidiEvent(e);
        }

        private void PianoControl1_NoteOff(object sender, NoteOffEvent e)
        {
            if (toolStripComboBox2.SelectedIndex != 0)
            {
                //Program change
                var pe = new ProgramChangeEvent((SevenBitNumber)(toolStripComboBox2.SelectedIndex - 1));
                pe.Channel = (FourBitNumber)(toolStripComboBox1.SelectedIndex);
                MidiManager.SendMidiEvent(pe);
            }
            e.Channel = (FourBitNumber)(toolStripComboBox1.SelectedIndex);
            MidiManager.SendMidiEvent(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            EnableDoubleBuffering(tabPage1);

            timer1.Start();
        }

        public static void EnableDoubleBuffering(Control control)
        {
            control.GetType().InvokeMember("DoubleBuffered",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null, control, new object[] { true });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InstrumentManager_InstrumentRemoved(object sender, EventArgs e)
        {
            listViewIntruments.Clear();
            foreach (var inst in InstrumentManager.GetAllInstruments())
                addItem(inst);
            listViewIntruments.Sort();
            propertyGrid.SelectedObjects = new object[] { };
            toolStripButtonPopup.Enabled = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InstrumentManager_InstrumentAdded(object sender, EventArgs e)
        {
            listViewIntruments.Clear();
            foreach (var inst in InstrumentManager.GetAllInstruments())
                addItem(inst);
            listViewIntruments.Sort();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InstrumentManager_InstrumentChanged(object sender, EventArgs e)
        {
            listViewIntruments.Clear();
            foreach (var inst in InstrumentManager.GetAllInstruments())
                addItem(inst);
            listViewIntruments.Sort();
            propertyGrid.SelectedObjects = new object[] { };
            toolStripButtonPopup.Enabled = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inst"></param>
        private void addItem(InstrumentBase inst)
        {
            var lvi = new ListViewItem(inst.Name, inst.ImageKey);
            lvi.Group = listViewIntruments.Groups[inst.Group];
            var item = listViewIntruments.Items.Add(lvi);
            item.Tag = inst;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripComboBox1_DropDown(object sender, EventArgs e)
        {
            toolStripComboBoxMidiIf.Items.Clear();
            try
            {
                foreach (var dev in MidiManager.GetInputMidiDevices())
                {
                    toolStripComboBoxMidiIf.Items.Add(dev.Name);
                    dev.Dispose();
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(Exception))
                    throw;
                else if (ex.GetType() == typeof(SystemException))
                    throw;

                MessageBox.Show(ex.ToString());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                MidiManager.SetInputMidiDevice(toolStripComboBoxMidiIf.Text);
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(Exception))
                    throw;
                else if (ex.GetType() == typeof(SystemException))
                    throw;


                MessageBox.Show(ex.ToString());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listViewIntruments_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            List<InstrumentBase> insts = new List<InstrumentBase>();
            foreach (ListViewItem item in listViewIntruments.SelectedItems)
                insts.Add((InstrumentBase)item.Tag);
            propertyGrid.SelectedObjects = insts.ToArray();
            toolStripButtonPopup.Enabled = (listViewIntruments.SelectedItems.Count != 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addYM2612ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstrumentManager.AddInstrument(InstrumentType.YM2612);
        }

        private void addSN76496ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstrumentManager.AddInstrument(InstrumentType.SN76496);
        }

        private void addNAMCOCUS30ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstrumentManager.AddInstrument(InstrumentType.NAMCO_CUS30);
        }

        private void extendGBAPUToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstrumentManager.AddInstrument(InstrumentType.GB_APU);
        }

        private void addYM2151ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstrumentManager.AddInstrument(InstrumentType.YM2151);
        }

        private void extendNESAPUToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstrumentManager.AddInstrument(InstrumentType.RP2A03);
        }

        private void extendSCC1kToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstrumentManager.AddInstrument(InstrumentType.SCC1);
        }

        private void extendYM3812ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstrumentManager.AddInstrument(InstrumentType.YM3812);
        }

        private void extendYM2413ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstrumentManager.AddInstrument(InstrumentType.YM2413);
        }

        private void extendMSM5232ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstrumentManager.AddInstrument(InstrumentType.MSM5232);
        }

        private void extendAY38910ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstrumentManager.AddInstrument(InstrumentType.AY8910);
        }

        private void extendMOS8580ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstrumentManager.AddInstrument(InstrumentType.MOS8580);
        }

        private void extendMOS6581ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstrumentManager.AddInstrument(InstrumentType.MOS6581);
        }


        private void extendBeepToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstrumentManager.AddInstrument(InstrumentType.Beep);
        }


        private void extendC140ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstrumentManager.AddInstrument(InstrumentType.C140);
        }


        private void extendHuC6230ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstrumentManager.AddInstrument(InstrumentType.HuC6280);
        }


        private void extendSPC700ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstrumentManager.AddInstrument(InstrumentType.SPC700);
        }

        private void extendPOKEYToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstrumentManager.AddInstrument(InstrumentType.POKEY);
        }

        private void extendYM2610BToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstrumentManager.AddInstrument(InstrumentType.YM2610B);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void decreaseThisKindOfChipToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Dictionary<InstrumentType, object> insts = new Dictionary<InstrumentType, object>();
            foreach (ListViewItem item in listViewIntruments.SelectedItems)
            {
                var tp = ((InstrumentBase)item.Tag).InstrumentType;
                if (!insts.ContainsKey(tp))
                    insts.Add(tp, null);
            }
            foreach (var tp in insts.Keys)
                InstrumentManager.RemoveInstrument(tp);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Settings.Default.EnvironmentSettings = StringCompressionUtility.Compress(
                    JsonConvert.SerializeObject(Program.SaveEnvironmentSettings(), Formatting.Indented, Program.JsonAutoSettings));
                Settings.Default.Save();
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(Exception))
                    throw;
                else if (ex.GetType() == typeof(SystemException))
                    throw;

                MessageBox.Show(ex.ToString());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dr = saveFileDialog1.ShowDialog(this);
            if (dr == DialogResult.OK)
            {
                try
                {
                    var es = Program.SaveEnvironmentSettings();
                    string data = JsonConvert.SerializeObject(es, Formatting.Indented, Program.JsonAutoSettings);
                    File.WriteAllText(saveFileDialog1.FileName, StringCompressionUtility.Compress(data));
                }
                catch (Exception ex)
                {
                    if (ex.GetType() == typeof(Exception))
                        throw;
                    else if (ex.GetType() == typeof(SystemException))
                        throw;

                    MessageBox.Show(ex.ToString());
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dr = openFileDialog1.ShowDialog(this);
            if (dr == DialogResult.OK)
            {
                try
                {
                    string text = StringCompressionUtility.Decompress(File.ReadAllText(openFileDialog1.FileName));
                    var settings = JsonConvert.DeserializeObject<EnvironmentSettings>(text, Program.JsonAutoSettings);
                    InstrumentManager.RestoreSettings(settings);
                }
                catch (Exception ex)
                {
                    if (ex.GetType() == typeof(Exception))
                        throw;
                    else if (ex.GetType() == typeof(SystemException))
                        throw;

                    MessageBox.Show(ex.ToString());
                }
            }
        }

        private void toolStripMenuItemAbout_Click(object sender, EventArgs e)
        {
            FormAbout fa = new FormAbout();
            fa.ShowDialog(this);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            //All Sounds Off
            var me = new ControlChangeEvent((SevenBitNumber)120, (SevenBitNumber)0);
            MidiManager.SendMidiEvent(me);
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            var me = new ControlChangeEvent((SevenBitNumber)121, (SevenBitNumber)0);
            MidiManager.SendMidiEvent(me);
        }

        private static FormSettings f_formSetting;

        private static FormSettings FormSetting
        {
            get
            {
                if (f_formSetting == null)
                    f_formSetting = new FormSettings();

                return f_formSetting;
            }
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dr = FormSetting.ShowDialog(this);
            if (dr == DialogResult.OK)
            {
                Settings.Default.Save();
                var rdr = MessageBox.Show(this, "Do you restart to apply new settings?", "Message", MessageBoxButtons.YesNo);
                if (rdr == DialogResult.Yes)
                {
                    Close();
                    Program.RestartRequiredApplication = Application.ExecutablePath;
                }
            }
            else
                Settings.Default.Reload();
        }

        private void tabPage1_Paint(object sender, PaintEventArgs e)
        {
            var fis = listViewIntruments.SelectedItems;
            int[][] data;
            InstrumentBase inst = null;
            if (fis != null && fis.Count != 0)
                inst = (InstrumentBase)fis[0].Tag;
            data = InstrumentManager.GetLastOutputBuffer(inst);

            e.Graphics.Clear(tabPage1.BackColor);

            int w = tabPage1.ClientSize.Width;
            int h = tabPage1.ClientSize.Height;
            if (data != null)
            {
                using (Pen p = new Pen(Color.DarkGreen))
                {
                    int max = h * 4;
                    //int? ldt = null;
                    int zero = 0;
                    for (int ch = 0; ch < 2; ch++)
                    {
                        if (data[ch] != null)
                        {
                            for (int i = 0; i < data[ch].Length; i++)
                            {
                                int dt = data[ch][i];
                                //int sn = Math.Sign(dt);
                                //if (zero == 0 && ldt != null && sn > 0 && sn != Math.Sign(ldt.Value))
                                //    zero = i;
                                max = Math.Max(Math.Abs(dt), max);
                                //ldt = dt;
                            }
                        }
                    }
                    max += max / 10;

                    if (data[0] != null)
                        draw(e, p, data[0], zero, w / 2 - 1, h / 2, 0, max);
                    if (data[1] != null)
                        draw(e, p, data[1], zero, w / 2 - 1, h / 2, w / 2 + 1, max);
                }
            }
            e.Graphics.DrawLine(SystemPens.Control, w / 2 - 1, 0, w / 2 - 1, h);
            e.Graphics.DrawLine(SystemPens.Control, w / 2, 0, w / 2, h);
        }

        private static void draw(PaintEventArgs e, Pen p, int[] data, int zero, int w, int h, int ox, int max)
        {
            int len = data.Length - zero;
            List<Point> pts = new List<Point>();
            for (int i = 0; i < w; i++)
            {
                int y = data[zero + (int)(((double)i / w) * len)];
                y = -(int)((y / (double)max) * h);
                y += h;
                int lx = i - 1;
                if (lx < 0)
                    lx = 0;
                pts.Add(new Point(ox + i, y));
            }
            e.Graphics.DrawLines(p, pts.ToArray());
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == tabPage1)
                tabPage1.Invalidate();
            //else if (tabControl1.SelectedTab == tabPage3)
            //    tabPage3.Invalidate();
        }

        private void resetToDefaultThisPropertyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            propertyGrid.ResetSelectedProperty();
            propertyGrid.Refresh();
        }

        private void toolStripButton19_Click(object sender, EventArgs e)
        {
            toolStripButton19.Checked = !toolStripButton19.Checked;
            bool chk = toolStripButton19.Checked;

            toolStripButton18.Checked = chk;
            toolStripButton17.Checked = chk;
            toolStripButton16.Checked = chk;
            toolStripButton15.Checked = chk;
            toolStripButton14.Checked = chk;
            toolStripButton13.Checked = chk;
            toolStripButton12.Checked = chk;
            toolStripButton11.Checked = chk;
            toolStripButton10.Checked = chk;
            toolStripButton9.Checked = chk;
            toolStripButton8.Checked = chk;
            toolStripButton7.Checked = chk;
            toolStripButton6.Checked = chk;
            toolStripButton5.Checked = chk;
            toolStripButton4.Checked = chk;
            toolStripButton3.Checked = chk;
        }

        private void toolStripButton18_Click(object sender, EventArgs e)
        {
            var tb = (ToolStripButton)sender;
            tb.Checked = !tb.Checked;
        }

        private void toolStripButton18_CheckedChanged(object sender, EventArgs e)
        {
            var tb = (ToolStripButton)sender;
            pianoControl1.SetReceiveChannel(int.Parse(tb.Text) - 1, tb.Checked);
        }

        private void toolStripComboBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            pianoControl1.SetMouseChannel(toolStripComboBox1.SelectedIndex);
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

        private void toolStripButtonPopup_Click(object sender, EventArgs e)
        {
            if (listViewIntruments.SelectedItems.Count != 0)
            {
                List<InstrumentBase> insts = new List<InstrumentBase>();
                foreach (ListViewItem item in listViewIntruments.SelectedItems)
                    insts.Add((InstrumentBase)item.Tag);
                FormProp fp = new FormProp(insts.ToArray());
                fp.StartPosition = FormStartPosition.Manual;
                fp.Location = this.Location;

                fp.Show(this);
            }
        }

    }
}
