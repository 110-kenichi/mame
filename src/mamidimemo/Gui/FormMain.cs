// copyright-holders:K.Ito
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
using Melanchall.DryWetMidi.Common;
using System.Reflection;
using Melanchall.DryWetMidi.Core;
using System.Runtime.InteropServices;
using System.Drawing.Text;
using MetroFramework.Forms;

namespace zanac.MAmidiMEmo.Gui
{
    public partial class FormMain : FormBase
    {
        private static ListView outputListView;

        private static ToolStripStatusLabel statusLabel = new ToolStripStatusLabel();

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
                if (outputListView.Items.Count > 10000)
                    outputListView.Items.RemoveAt(0);
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
                if (outputListView.IsDisposed)
                    return;

                var item = outputListView.Items.Add(log);
                outputListView.EnsureVisible(item.Index);
                if (outputListView.Items.Count > 10000)
                    outputListView.Items.RemoveAt(0);
            }), null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="log"></param>
        public static void SetStatusText(String text)
        {
            if (statusLabel.IsDisposed)
                return;

            statusLabel.GetCurrentParent()?.BeginInvoke(new MethodInvoker(() =>
            {
                if (statusLabel.IsDisposed)
                    return;

                statusLabel.Text = text;
            }), null);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public FormMain()
        {
            InitializeComponent();
            //this.Font = new Font(PrivateFonts.Families[0], 13f);

            tabControlBottom.SelectedIndex = Settings.Default.MWinTab;

            Size = Settings.Default.MWinSize;
            splitContainer1.SplitterDistance = Settings.Default.MWinSp1Pos;
            splitContainer2.SplitterDistance = Settings.Default.MWinSp2Pos;

            toolStripComboBoxKeyCh.SelectedIndex = Settings.Default.MWinKeyCh;
            toolStripComboBoxProgNo.SelectedIndex = Settings.Default.MWinProgNo;
            toolStripComboBoxCC.SelectedIndex = Settings.Default.MWinCC;

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
            imageList1.Images.Add("YM2610B", Resources.YM2610B);
            imageList1.Images.Add("MT32", Resources.MT32);
            imageList1.Images.Add("CM32P", Resources.CM32P);
            imageList1.Images.Add("YMF262", Resources.YMF262);

            if (Program.IsVSTiMode())
            {
                toolStripComboBoxMidiIf.Enabled = false;
                toolStripComboBoxMidiIf.Items.Add("VSTi");
                toolStripComboBoxMidiIf.SelectedIndex = 0;

                toolStripMenuItemExit.Enabled = false;
                this.ControlBox = false;
            }
            else
            {
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
                    MessageBox.Show(
                        "There are no MIDI IN devices.\r\n" +
                        "Please install at least one MIDI IN device to use the MAmidiMEmo.\r\n" +
                        "Or, install the loopMIDI to the PC.");
                }
            }
            outputListView = listView1;

            //statusLabel = toolStripStatusLabel1;

            //MIDI Event
            InstrumentManager_InstrumentChanged(null, null);
            InstrumentManager.InstrumentChanged += InstrumentManager_InstrumentChanged;
            InstrumentManager.InstrumentAdded += InstrumentManager_InstrumentAdded;
            InstrumentManager.InstrumentRemoved += InstrumentManager_InstrumentRemoved;

            pianoControl1.NoteOn += PianoControl1_NoteOn;
            pianoControl1.NoteOff += PianoControl1_NoteOff;
            pianoControl1.EntryDataChanged += PianoControl1_EntryDataChanged;

            ImageUtility.AdjustControlImagesDpiScale(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            Settings.Default.MWinProgNo = toolStripComboBoxProgNo.SelectedIndex;
            Settings.Default.MWinKeyCh = toolStripComboBoxKeyCh.SelectedIndex;
            Settings.Default.MWinCC = toolStripComboBoxCC.SelectedIndex;

            Settings.Default.MWinSize = Size;

            Settings.Default.MWinSp1Pos = splitContainer1.SplitterDistance;
            Settings.Default.MWinSp2Pos = splitContainer2.SplitterDistance;

            Settings.Default.MWinTab = tabControlBottom.SelectedIndex;

            base.OnClosing(e);
        }

        private void PianoControl1_NoteOn(object sender, TaggedNoteOnEvent e)
        {
            if (toolStripComboBoxProgNo.SelectedIndex != 0)
            {
                //Program change
                var pe = new ProgramChangeEvent((SevenBitNumber)(toolStripComboBoxProgNo.SelectedIndex - 1));
                pe.Channel = (FourBitNumber)(toolStripComboBoxKeyCh.SelectedIndex);
                MidiManager.SendMidiEvent(pe);
            }
            e.Channel = (FourBitNumber)(toolStripComboBoxKeyCh.SelectedIndex);
            MidiManager.SendMidiEvent(e);
        }

        private void PianoControl1_NoteOff(object sender, NoteOffEvent e)
        {
            if (toolStripComboBoxProgNo.SelectedIndex != 0)
            {
                //Program change
                var pe = new ProgramChangeEvent((SevenBitNumber)(toolStripComboBoxProgNo.SelectedIndex - 1));
                pe.Channel = (FourBitNumber)(toolStripComboBoxKeyCh.SelectedIndex);
                MidiManager.SendMidiEvent(pe);
            }
            e.Channel = (FourBitNumber)(toolStripComboBoxKeyCh.SelectedIndex);
            MidiManager.SendMidiEvent(e);
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
            cce.Channel = (FourBitNumber)(toolStripComboBoxKeyCh.SelectedIndex);
            MidiManager.SendMidiEvent(cce);
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
                int si = -1;
                foreach (var dev in MidiManager.GetInputMidiDevices())
                {
                    int i = toolStripComboBoxMidiIf.Items.Add(dev.Name);
                    if (dev.Name.Equals(Settings.Default.MidiIF))
                        si = i;
                    dev.Dispose();
                }
                if (si >= 0)
                    toolStripComboBoxMidiIf.SelectedIndex = si;

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
            if (Program.IsVSTiMode())
                return;

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

        private void mT32ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstrumentManager.AddInstrument(InstrumentType.MT32);
        }

        private void extendCM32PToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstrumentManager.AddInstrument(InstrumentType.CM32P);
        }

        private void extendYMF262ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstrumentManager.AddInstrument(InstrumentType.YMF262);
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
        private void cloneSelectedChipToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = listViewIntruments.FocusedItem;
            if (item != null)
            {
                var tp = ((InstrumentBase)item.Tag).InstrumentType;
                var inst = InstrumentManager.AddInstrument(tp);
                if (inst != null)
                    inst.SerializeData = ((InstrumentBase)item.Tag).SerializeData;
            }
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
                string file = openFileDialog1.FileName;
                loadMAmi(file);
            }
        }

        private static void loadMAmi(string file)
        {
            try
            {
                string text = StringCompressionUtility.Decompress(File.ReadAllText(file));
                InstrumentManager.ClearAllInstruments();
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

        private void toolStripMenuItemAbout_Click(object sender, EventArgs e)
        {
            FormAbout fa = new FormAbout();
            fa.ShowDialog(this);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            //All Note Off
            var me = new ControlChangeEvent((SevenBitNumber)123, (SevenBitNumber)0);
            MidiManager.SendMidiEvent(me);

            //All Sounds Off
            me = new ControlChangeEvent((SevenBitNumber)120, (SevenBitNumber)0);
            MidiManager.SendMidiEvent(me);

            foreach (var inst in InstrumentManager.GetAllInstruments())
                inst.AllSoundOff();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            var me = new ControlChangeEvent((SevenBitNumber)121, (SevenBitNumber)0);
            MidiManager.SendMidiEvent(me);
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var f_formSetting = new FormSettings())
            {
                DialogResult dr = f_formSetting.ShowDialog(this);
                if (dr == DialogResult.OK)
                {
                    Settings.Default.Save();
                    if (Program.IsVSTiMode())
                    {
                        MessageBox.Show(this, "Please restart host DAW application.", "Information", MessageBoxButtons.OK);
                    }
                    else
                    {
                        var rdr = MessageBox.Show(this, "Do you restart to apply new settings?", "Message", MessageBoxButtons.YesNo);
                        if (rdr == DialogResult.Yes)
                        {
                            Close();
                            Program.RestartRequiredApplication = Application.ExecutablePath;
                        }
                    }
                }
                else
                    Settings.Default.Reload();
            }
        }

        private void tabPage1_Paint(object sender, PaintEventArgs e)
        {
            var fis = listViewIntruments.SelectedItems;
            InstrumentBase inst = null;
            if (fis != null && fis.Count != 0)
                inst = (InstrumentBase)fis[0].Tag;
            Control target = tabPage1;

            OscUtility.DrawOsc(e, inst, target);
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            if (tabControlBottom.SelectedTab == tabPage1)
                tabPage1.Invalidate();
        }

        private void resetToDefaultThisPropertyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = propertyGrid.SelectedGridItem;
            ITypeDescriptorContext context = item as ITypeDescriptorContext;
            bool enabled = false;
            try
            {
                enabled = item != null &&
                    item.GridItemType == GridItemType.Property &&
                    context != null && context.Instance != null && item.PropertyDescriptor.CanResetValue(context.Instance);
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(Exception))
                    throw;
                else if (ex.GetType() == typeof(SystemException))
                    throw;
            }
            if (enabled)
            {
                propertyGrid.ResetSelectedProperty();
                propertyGrid.Refresh();
            }
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
            pianoControl1.SetMouseChannel(toolStripComboBoxKeyCh.SelectedIndex);
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


        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delg_start_recording_to(string wavfile);

        private static delg_start_recording_to start_recording_to;

        /// <summary>
        /// 
        /// </summary>
        private static delg_start_recording_to StartRecordingTo
        {
            get
            {
                if (start_recording_to == null)
                {
                    IntPtr funcPtr = MameIF.GetProcAddress("start_recording_to");
                    if (funcPtr != IntPtr.Zero)
                        start_recording_to = (delg_start_recording_to)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delg_start_recording_to));
                }
                return start_recording_to;
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delg_stop_recording();

        private static delg_stop_recording stop_recording;

        /// <summary>
        /// 
        /// </summary>
        private static delg_stop_recording StopRecording
        {
            get
            {
                if (stop_recording == null)
                {
                    IntPtr funcPtr = MameIF.GetProcAddress("stop_recording");
                    if (funcPtr != IntPtr.Zero)
                        stop_recording = (delg_stop_recording)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delg_stop_recording));
                }
                return stop_recording;
            }
        }

        private void toolStripButton20_Click(object sender, EventArgs e)
        {
            toolStripButton20.Checked = !toolStripButton20.Checked;
        }

        private void toolStripButton20_CheckedChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Settings.Default.OutputDir))
                Settings.Default.OutputDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            var now = DateTime.Now;
            string op = Path.Combine(Settings.Default.OutputDir, "MAmi_" + now.ToShortDateString().Replace('/', '-') + "_" + now.ToLongTimeString().Replace(':', '-') + ".wav");

            try
            {
                Program.SoundUpdating();
                if (toolStripButton20.Checked)
                {
                    StartRecordingTo(op);
                }
                else
                {
                    StopRecording();
                }
            }
            finally
            {
                Program.SoundUpdated();
            }
        }



        private void toolStripButton21_Click(object sender, EventArgs e)
        {
            toolStripButton21.Checked = !toolStripButton21.Checked;
        }

        private void toolStripButton21_CheckedChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Settings.Default.OutputDir))
                Settings.Default.OutputDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            if (toolStripButton21.Checked)
            {
                var now = DateTime.Now;
                string op = Path.Combine(Settings.Default.OutputDir, "MAmi_VGM_" +
                    now.ToShortDateString().Replace('/', '-') + "_" + now.ToLongTimeString().Replace(':', '-'));
                Directory.CreateDirectory(op);

                InstrumentManager.StartVgmRecordingTo(op);
            }
            else
            {
                InstrumentManager.StopVgmRecording();
                Process.Start(InstrumentManager.LastVgmOutputDir);
            }
        }

        private void toolStripComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (toolStripComboBoxProgNo.SelectedIndex != 0)
            {
                //Program change
                var pe = new ProgramChangeEvent((SevenBitNumber)(toolStripComboBoxProgNo.SelectedIndex - 1));
                pe.Channel = (FourBitNumber)(toolStripComboBoxKeyCh.SelectedIndex);
                MidiManager.SendMidiEvent(pe);
            }
        }

        private void listViewIntruments_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] drags = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (drags.Length == 1)
                {
                    if (File.Exists(drags[0]) && Path.GetExtension(drags[0]).Equals(".MAmi", StringComparison.OrdinalIgnoreCase))
                    {
                        loadMAmi(drags[0]);
                    }
                }
            }
        }

        private void listViewIntruments_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] drags = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (drags.Length == 1)
                {
                    if (File.Exists(drags[0]) && Path.GetExtension(drags[0]).Equals(".MAmi", StringComparison.OrdinalIgnoreCase))
                    {
                        e.Effect = DragDropEffects.All;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void contextMenuStripInst_Opening(object sender, CancelEventArgs e)
        {
            ListViewItem item = null;
            foreach (ListViewItem li in listViewIntruments.SelectedItems)
                item = li;
            if (item != null)
            {
                var tp = (InstrumentBase)item.Tag;
                var mns = tp.GetInstrumentMenus();
                if (mns != null)
                {
                    foreach (var mi in mns)
                        contextMenuStripInst.Items.Add(mi);
                }
            }
            for (int i = 0; i < contextMenuStripInst.Items.IndexOf(toolStripSepInst) + 1; i++)
                contextMenuStripInst.Items[i].Enabled = item != null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void contextMenuStripInst_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            for (int i = contextMenuStripInst.Items.IndexOf(toolStripMenuItemSep) + 1; i < contextMenuStripInst.Items.Count; i++)
            {
                contextMenuStripInst.Items.RemoveAt(i);
                i--;
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var rdr = MessageBox.Show(this, "Do you clear all instruments?", "Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (rdr == DialogResult.Yes)
            {
                InstrumentManager.ClearAllInstruments();
            }
        }

    }
}
