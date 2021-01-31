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
using System.IO.Compression;
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
using Melanchall.DryWetMidi.Interaction;
using zanac.MAmidiMEmo.Util;

namespace zanac.MAmidiMEmo.Gui
{
    public partial class FormMain : FormBase
    {
        /// <summary>
        /// 
        /// </summary>
        public static FormMain AppliactionForm
        {
            get;
            private set;
        }

        private static ListView outputListView;

        private static ToolStripStatusLabel statusLabel = new ToolStripStatusLabel();

        private static StreamWriter logStream;

        private static PrivateFontCollection privateFonts = new PrivateFontCollection();

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
        public static void OutputDebugLog(InstrumentBase inst, String log)
        {
            if (outputListView == null || outputListView.IsDisposed || !outputListView.IsHandleCreated)
                return;

            if(inst != null)
                log = "[" + inst.Name + "(" + inst.UnitNumber + ")]" + log;

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
        public static void OutputLog(InstrumentBase inst, String log)
        {
            if (outputListView == null || outputListView.IsDisposed)
                return;

            if (inst != null)
                log = "[" + inst.Name + "(" + inst.UnitNumber + ")]" + log;

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

            AppliactionForm = this;

            unsafe
            {
                byte[] fontBuf = Properties.Resources.DSEG7ClassicMini_BoldItalic;
                fixed (byte* pFontBuf = fontBuf)
                    privateFonts.AddMemoryFont((IntPtr)pFontBuf, fontBuf.Length);
            }
            unsafe
            {
                byte[] fontBuf = Properties.Resources.DSEG14ClassicMini_BoldItalic;
                fixed (byte* pFontBuf = fontBuf)
                    privateFonts.AddMemoryFont((IntPtr)pFontBuf, fontBuf.Length);
            }
            unsafe
            {
                byte[] fontBuf = Properties.Resources.PixelMplus12_Regular;
                fixed (byte* pFontBuf = fontBuf)
                    privateFonts.AddMemoryFont((IntPtr)pFontBuf, fontBuf.Length);
            }
            labelClock.Font = new Font(privateFonts.Families[1], 18);
            labelCpuLoad.Font = new Font(privateFonts.Families[1], 18);
            labelTitle.Font = new Font(privateFonts.Families[2], 22);

            tabControlBottom.SelectedIndex = Settings.Default.MWinTab;

            Size = Settings.Default.MWinSize;
            splitContainer1.SplitterDistance = Settings.Default.MWinSp1Pos;
            splitContainer2.SplitterDistance = Settings.Default.MWinSp2Pos;

            toolStripComboBoxPort.SelectedIndex = Settings.Default.MWinPort;
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
            imageList1.Images.Add("YM2608", Resources.YM2608);
            imageList1.Images.Add("TMS5220", Resources.TMS5220);
            imageList1.Images.Add("SP0256", Resources.SP0256);
            imageList1.Images.Add("SAM", Resources.SAM);

            if (Program.IsVSTiMode())
            {
                toolStripComboBoxMidiIfA.Enabled = false;
                toolStripComboBoxMidiIfA.Items.Add("VSTi");
                toolStripComboBoxMidiIfA.SelectedIndex = 0;

                toolStripComboBoxMidiIfB.Enabled = false;

                toolStripMenuItemExit.Enabled = false;
                this.ControlBox = false;
            }
            else
            {
                //Set MIDI I/F
                foreach (var dev in InputDevice.GetAll())
                {
                    int idx = toolStripComboBoxMidiIfA.Items.Add(dev.Name);
                    if (string.Equals(dev.Name, Settings.Default.MidiIF))
                        toolStripComboBoxMidiIfA.SelectedIndex = idx;

                    idx = toolStripComboBoxMidiIfB.Items.Add(dev.Name);
                    if (string.Equals(dev.Name, Settings.Default.MidiIF_B))
                        toolStripComboBoxMidiIfB.SelectedIndex = idx;

                    dev.Dispose();
                }

                if (toolStripComboBoxMidiIfA.Items.Count < 1)
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

            InstrumentBase.StaticPropertyChanged += InstrumentBase_StaticPropertyChanged;

            MidiManager.MidiEventReceivedA += MidiManager_MidiEventReceivedA;
            MidiManager.MidiEventReceivedB += MidiManager_MidiEventReceivedB;

            ImageUtility.AdjustControlImagesDpiScale(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            Settings.Default.MWinPort = toolStripComboBoxPort.SelectedIndex;

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
                pe.Channel = (FourBitNumber)(toolStripComboBoxKeyCh.SelectedIndex & 0xf);
                MidiManager.SendMidiEvent((MidiPort)(((toolStripComboBoxKeyCh.SelectedIndex & 0x30) >> 4) + 1), pe);
            }
            e.Channel = (FourBitNumber)(toolStripComboBoxKeyCh.SelectedIndex & 0xf);
            MidiManager.SendMidiEvent((MidiPort)(((toolStripComboBoxKeyCh.SelectedIndex & 0x30) >> 4) + 1), e);
        }

        private void PianoControl1_NoteOff(object sender, NoteOffEvent e)
        {
            if (toolStripComboBoxProgNo.SelectedIndex != 0)
            {
                //Program change
                var pe = new ProgramChangeEvent((SevenBitNumber)(toolStripComboBoxProgNo.SelectedIndex - 1));
                pe.Channel = (FourBitNumber)(toolStripComboBoxKeyCh.SelectedIndex & 0xf);
                MidiManager.SendMidiEvent((MidiPort)(((toolStripComboBoxKeyCh.SelectedIndex & 0x30) >> 4) + 1), pe);
            }
            e.Channel = (FourBitNumber)(toolStripComboBoxKeyCh.SelectedIndex & 0xf);
            MidiManager.SendMidiEvent((MidiPort)(((toolStripComboBoxKeyCh.SelectedIndex & 0x30) >> 4) + 1), e);
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
            cce.Channel = (FourBitNumber)(toolStripComboBoxKeyCh.SelectedIndex & 0xf);
            MidiManager.SendMidiEvent((MidiPort)(((toolStripComboBoxKeyCh.SelectedIndex & 0x30) >> 4) + 1), cce);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            EnableDoubleBuffering(tabPage1);

            timerOsc.Start();
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
            metroLabelDrop.Visible = true;
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
            metroLabelDrop.Visible = true;
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
            metroLabelDrop.Visible = true;
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
            metroLabelDrop.Visible = false;

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
        private void toolStripComboBoxMidiIfA_DropDown(object sender, EventArgs e)
        {
            toolStripComboBoxMidiIfA.Items.Clear();
            try
            {
                int si = -1;
                foreach (var dev in MidiManager.GetInputMidiDevices())
                {
                    int i = toolStripComboBoxMidiIfA.Items.Add(dev.Name);
                    if (dev.Name.Equals(Settings.Default.MidiIF))
                        si = i;
                    dev.Dispose();
                }
                if (si >= 0)
                    toolStripComboBoxMidiIfA.SelectedIndex = si;

            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(Exception))
                    throw;
                else if (ex.GetType() == typeof(SystemException))
                    throw;

                MessageBox.Show(Resources.FailedMidiIf);
            }
        }


        private void toolStripComboBoxMidiIfB_DropDown(object sender, EventArgs e)
        {
            toolStripComboBoxMidiIfB.Items.Clear();
            try
            {
                int si = -1;
                foreach (var dev in MidiManager.GetInputMidiDevices())
                {
                    int i = toolStripComboBoxMidiIfB.Items.Add(dev.Name);
                    if (dev.Name.Equals(Settings.Default.MidiIF_B))
                        si = i;
                    dev.Dispose();
                }
                if (si >= 0)
                    toolStripComboBoxMidiIfB.SelectedIndex = si;

            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(Exception))
                    throw;
                else if (ex.GetType() == typeof(SystemException))
                    throw;

                MessageBox.Show(Resources.FailedMidiIf);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripComboBoxMidiIfA_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Program.IsVSTiMode())
                return;

            try
            {
                MidiManager.SetInputMidiDeviceA(toolStripComboBoxMidiIfA.Text);
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(Exception))
                    throw;
                else if (ex.GetType() == typeof(SystemException))
                    throw;

                MessageBox.Show(Resources.FailedMidiIf);
            }
        }


        private void toolStripComboBoxMidiIfB_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Program.IsVSTiMode())
                return;

            try
            {
                MidiManager.SetInputMidiDeviceB(toolStripComboBoxMidiIfB.Text);
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(Exception))
                    throw;
                else if (ex.GetType() == typeof(SystemException))
                    throw;

                MessageBox.Show(Resources.FailedMidiIf);
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

        private void extendYM2608ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstrumentManager.AddInstrument(InstrumentType.YM2608);
        }

        private void tms5220ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstrumentManager.AddInstrument(InstrumentType.TMS5220);
        }

        private void extendSP0256AL2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstrumentManager.AddInstrument(InstrumentType.SP0256);
        }

        private void extendSAMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstrumentManager.AddInstrument(InstrumentType.SAM);
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

                MessageBox.Show(Resources.FailedSaveEnv + "\r\n" + ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dr = saveFileDialogMami.ShowDialog(this);
            if (dr == DialogResult.OK)
            {
                try
                {
                    var es = Program.SaveEnvironmentSettings();
                    string data = JsonConvert.SerializeObject(es, Formatting.Indented, Program.JsonAutoSettings);
                    File.WriteAllText(saveFileDialogMami.FileName, StringCompressionUtility.Compress(data));
                }
                catch (Exception ex)
                {
                    if (ex.GetType() == typeof(Exception))
                        throw;
                    else if (ex.GetType() == typeof(SystemException))
                        throw;

                    MessageBox.Show(Resources.FailedSaveEnv + "\r\n" + ex.Message);
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
            DialogResult dr = openFileDialogMami.ShowDialog(this);
            if (dr == DialogResult.OK)
            {
                string file = openFileDialogMami.FileName;
                loadMAmidiFile(file);
            }
        }

        private void loadMAmidiFile(string file)
        {
            string ext = Path.GetExtension(file);
            if (ext.Equals(".MAmi", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    readMAmiCore(File.ReadAllText(file));
                }
                catch (Exception ex)
                {
                    if (ex.GetType() == typeof(Exception))
                        throw;
                    else if (ex.GetType() == typeof(SystemException))
                        throw;

                    MessageBox.Show(Resources.FailedLoadMAmi + "\r\n" + ex.Message);
                }
            }
            else if (ext.Equals(".MAmidi", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    using (ZipArchive archive = new ZipArchive(new FileStream(file, FileMode.Open), ZipArchiveMode.Read))
                    {
                        var allTextFiles = archive.Entries;

                        ZipArchiveEntry mamiEntry = archive.GetEntry("mami.MAmi");
                        ZipArchiveEntry midiEntry = archive.GetEntry("midi.midi");
                        if (mamiEntry == null || midiEntry == null)
                        {
                            MessageBox.Show(Resources.FailedLoadMAmidi);
                            return;
                        }
                        using (var sr = new StreamReader(mamiEntry.Open()))
                            readMAmiCore(sr.ReadToEnd());

                        string midiFile = Path.GetTempFileName();
                        midiEntry.ExtractToFile(midiFile, true);
                        loadMidiFile(midiFile);
                    }
                }
                catch (Exception ex)
                {
                    if (ex.GetType() == typeof(Exception))
                        throw;
                    else if (ex.GetType() == typeof(SystemException))
                        throw;

                    MessageBox.Show(Resources.FailedLoadMAmidi + "\r\n" + ex.Message);
                }
            }
        }

        private static void readMAmiCore(string data)
        {
            try
            {
                string text = StringCompressionUtility.Decompress(data);
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

                MessageBox.Show(Resources.FailedLoadMAmi + "\r\n" + ex.Message);
            }
        }

        private void toolStripMenuItemAbout_Click(object sender, EventArgs e)
        {
            FormAbout fa = new FormAbout();
            fa.ShowDialog(this);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            InstrumentManager.Panic();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            var me = new ControlChangeEvent((SevenBitNumber)121, (SevenBitNumber)0);
            MidiManager.SendMidiEvent(MidiPort.PortAB, me);
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

            OscUtility.DrawOsc(e, inst, target, Color.DarkGreen);
        }

        private static Color oscLineColor = Color.FromArgb(115, 63, 0);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void panelOsc2_Paint(object sender, PaintEventArgs e)
        {
            OscUtility.DrawOsc(e, null, panelOsc2, oscLineColor);
        }

        private int clockCounter;

        private int loadCounter;

        private CpuUsage cpuUsage = new CpuUsage();

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (tabControlBottom.SelectedTab == tabPage1)
                tabPage1.Invalidate();
            else if (tabControlBottom.SelectedTab == tabPage4)
            {
                panelOsc2.Invalidate();
                panelChDisp.Invalidate();
            }

            clockCounter++;
            if (clockCounter > 100 / timerOsc.Interval)
            {
                clockCounter = 0;
                MetricTimeSpan playTime = (MetricTimeSpan)midiPlayback?.GetCurrentTime(Melanchall.DryWetMidi.Interaction.TimeSpanType.Metric);
                if (playTime != null)
                {
                    TimeSpan ts = new TimeSpan(0, playTime.Hours, playTime.Minutes, playTime.Seconds, playTime.Milliseconds);
                    labelClock.Text = ((int)ts.TotalMinutes).ToString("00") + ":" + ts.Seconds.ToString("00");
                }
                else
                {
                    labelClock.Text = "00:00";
                }
            }
            loadCounter++;
            if (loadCounter > 1000 / timerOsc.Interval)
            {
                loadCounter = 0;
                labelCpuLoad.Text = cpuUsage.GetUsage().ToString("000");
            }
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
                MidiManager.SendMidiEvent((MidiPort)(((toolStripComboBoxKeyCh.SelectedIndex & 0x30) >> 4) + 1), pe);
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
                        loadMAmidiFile(drags[0]);
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

        private Playback midiPlayback;

        private void toolStripButtonPlay_Click(object sender, EventArgs e)
        {
            if (midiPlayback == null)
                return;

            midiPlayback.Stop();
            InstrumentManager.Panic();
            midiPlayback.MoveToStart();
            midiPlayback.Start();
            this.labelStat.Image = global::zanac.MAmidiMEmo.Properties.Resources.Play;
        }

        private void toolStripButtonPause_Click(object sender, EventArgs e)
        {
            if (midiPlayback == null)
                return;

            if (this.labelStat.Image != null)
                this.labelStat.Image.Dispose();
            if (midiPlayback.IsRunning)
            {
                midiPlayback.Stop();
                this.labelStat.Image = global::zanac.MAmidiMEmo.Properties.Resources.Pause;
            }
            else
            {
                midiPlayback.Start();
                this.labelStat.Image = global::zanac.MAmidiMEmo.Properties.Resources.Play;
            }
        }

        private void toolStripButtonStop_Click(object sender, EventArgs e)
        {
            if (midiPlayback == null)
                return;

            midiPlayback.Stop();
            InstrumentManager.Panic();
            midiPlayback.MoveToStart();
            if (this.labelStat.Image != null)
                this.labelStat.Image.Dispose();
            this.labelStat.Image = global::zanac.MAmidiMEmo.Properties.Resources.Stop;
        }

        private void toolStripButtonOpen_Click(object sender, EventArgs e)
        {
            DialogResult dr = openFileDialogMidi.ShowDialog(this);
            if (dr == DialogResult.OK)
            {
                string ext = Path.GetExtension(openFileDialogMidi.FileName);
                switch (ext.ToUpperInvariant())
                {
                    case ".MIDI":
                    case ".MID":
                    case ".SMF":
                        loadMidiFile(openFileDialogMidi.FileName);
                        break;
                    case ".MAMIDI":
                        loadMAmidiFile(openFileDialogMidi.FileName);
                        break;
                }
            }
        }

        private string loadedMidiFile;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fn"></param>
        private void loadMidiFile(string fn)
        {
            try
            {
                toolStripButtonStop_Click(null, EventArgs.Empty);

                var midiFile = MidiFile.Read(fn);

                loadedMidiFile = fn;

                try
                {
                    fileSystemWatcherMidi.Path = Path.GetDirectoryName(fn);
                    fileSystemWatcherMidi.Filter = Path.GetFileName(fn);
                    fileSystemWatcherMidi.EnableRaisingEvents = toolStripButtonReload.Checked;
                }
                catch (Exception ex)
                {
                    if (ex.GetType() == typeof(Exception))
                        throw;
                    else if (ex.GetType() == typeof(SystemException))
                        throw;
                }

                labelTitle.SetText("(Loaded)");
                labelTitle.Tag = new object();

                midiPlayback?.Dispose();
                midiPlayback = midiFile.GetPlayback(new InternalMidiPlayerDevice());
                midiPlayback.EventPlayed += MidiPlayback_EventPlayed;

                InstrumentBase.MasterGain = (float)metroTrackBarVol.Value / 100f;

                toolStripButtonPlay_Click(null, null);
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(Exception))
                    throw;
                else if (ex.GetType() == typeof(SystemException))
                    throw;

                MessageBox.Show(Resources.FailedLoadMidi + "\r\n" + ex.Message);
            }
        }

        private void InstrumentBase_StaticPropertyChanged(object sender, PropertyChangingEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(InstrumentBase.MasterGain):
                    metroTrackBarVol.Value = (int)(InstrumentBase.MasterGain * 100f);
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MidiPlayback_EventPlayed(object sender, MidiEventPlayedEventArgs e)
        {
            switch (e.Event.EventType)
            {
                case MidiEventType.Text:
                    {
                        setTitle(((TextEvent)e.Event).Text);
                        break;
                    }
                case MidiEventType.CopyrightNotice:
                    {
                        setTitle(((CopyrightNoticeEvent)e.Event).Text);
                        break;
                    }
                case MidiEventType.SequenceTrackName:
                    {
                        setTitle(((SequenceTrackNameEvent)e.Event).Text);
                        break;
                    }
            }
        }

        private void toolStripButtonReload_CheckStateChanged(object sender, EventArgs e)
        {
            if (midiPlayback != null)
                fileSystemWatcherMidi.EnableRaisingEvents = toolStripButtonReload.Checked;
        }

        private void fileSystemWatcher1_Changed(object sender, FileSystemEventArgs e)
        {
            timerReload.Enabled = true;
        }

        private void timerReload_Tick(object sender, EventArgs e)
        {
            loadMidiFile(loadedMidiFile);
            timerReload.Enabled = false;
        }

        private void metroTrackBar1_ValueChanged(object sender, EventArgs e)
        {
            InstrumentBase.MasterGain = (float)metroTrackBarVol.Value / 100f;
        }

        private void labelTitle_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] drags = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (drags.Length == 1)
                {
                    if (File.Exists(drags[0]))
                    {
                        string ext = Path.GetExtension(drags[0]);
                        switch (ext.ToUpperInvariant())
                        {
                            case ".MIDI":
                            case ".MID":
                            case ".SMF":
                                loadMidiFile(drags[0]);
                                break;
                            case ".MAMIDI":
                                loadMAmidiFile(drags[0]);
                                break;
                        }
                    }
                }
            }
        }

        private void labelTitle_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] drags = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (drags.Length == 1)
                {
                    if (File.Exists(drags[0]))
                    {
                        string ext = Path.GetExtension(drags[0]);
                        switch (ext.ToUpperInvariant())
                        {
                            case ".MIDI":
                            case ".MID":
                            case ".SMF":
                            case ".MAMIDI":
                                e.Effect = DragDropEffects.All;
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exportMAmidiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loadedMidiFile == null)
            {
                MessageBox.Show(Resources.LoadMidiFile);
                return;
            }

            DialogResult dr = saveFileDialogMAmidi.ShowDialog(this);
            if (dr == DialogResult.OK)
            {
                try
                {
                    using (FileStream zipToOpen = new FileStream(saveFileDialogMAmidi.FileName, FileMode.OpenOrCreate))
                    {
                        using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
                        {
                            {
                                ZipArchiveEntry mamiEntry = archive.CreateEntry("mami.MAmi");

                                var es = Program.SaveEnvironmentSettings();
                                string data = JsonConvert.SerializeObject(es, Formatting.Indented, Program.JsonAutoSettings);
                                string mami = StringCompressionUtility.Compress(data);

                                using (var writer = new StreamWriter(mamiEntry.Open()))
                                {
                                    writer.Write(mami);
                                }
                            }
                            {
                                ZipArchiveEntry midiEntry = archive.CreateEntry("midi.midi");

                                using (var writer = new BinaryWriter(midiEntry.Open()))
                                {
                                    foreach (var data in File.ReadAllBytes(loadedMidiFile))
                                        writer.Write(data);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (ex.GetType() == typeof(Exception))
                        throw;
                    else if (ex.GetType() == typeof(SystemException))
                        throw;

                    MessageBox.Show(Resources.FailedSaveMAmidi + "\r\n" + ex.Message);
                }
            }
        }

        private void MidiManager_MidiEventReceivedA(object sender, MidiEvent e)
        {
            if (e.EventType == MidiEventType.NoteOn)
            {
                NoteOnEvent noe = e as NoteOnEvent;
                if (noe != null && noe.Velocity != 0)
                {
                    lock (chNoteOnDataA)
                        chNoteOnDataA[noe.Channel] = noe.Velocity;
                }
                TaggedNoteOnEvent tnoe = e as TaggedNoteOnEvent;
                if (tnoe != null && tnoe.Velocity != 0)
                {
                    lock (chNoteOnDataA)
                        chNoteOnDataA[tnoe.Channel] = tnoe.Velocity;
                }
            }
        }

        private void MidiManager_MidiEventReceivedB(object sender, MidiEvent e)
        {
            if (e.EventType == MidiEventType.NoteOn)
            {
                NoteOnEvent noe = e as NoteOnEvent;
                if (noe != null && noe.Velocity != 0)
                {
                    lock (chNoteOnDataB)
                        chNoteOnDataB[noe.Channel] = noe.Velocity;
                }
                TaggedNoteOnEvent tnoe = e as TaggedNoteOnEvent;
                if (tnoe != null && tnoe.Velocity != 0)
                {
                    lock (chNoteOnDataB)
                        chNoteOnDataB[tnoe.Channel] = tnoe.Velocity;
                }
            }
        }

        private void setTitle(string text)
        {
            labelTitle.BeginInvoke(new MethodInvoker(() =>
            {
                if (!labelTitle.IsDisposed && labelTitle.Tag != null)
                {
                    List<byte> data = new List<byte>();
                    foreach (char ch in text)
                    {
                        if (ch == 0)
                            break;
                        data.Add((byte)ch);
                    }
                    text = Encoding.Default.GetString(data.ToArray());

                    labelTitle.SetText(text);
                    labelTitle.Tag = null;
                }
            }));
        }

        private int[] chNoteOnDataA = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        private int[] chNoteOnDataB = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        private Brush chDispBarBrush = new SolidBrush(Color.FromArgb(115, 63, 0));

        private Brush chDispBackBrush = new SolidBrush(Color.FromArgb(229, 126, 0));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void panelChDisp_Paint(object sender, PaintEventArgs e)
        {
            int w = (panelChDisp.Width / 16);
            int bw = w - (2 * 2);
            int h = panelChDisp.Height;
            int bh = h - (2 * 2);
            var g = e.Graphics;

            for (int i = 0; i < 16; i++)
            {
                var vel = chNoteOnDataA[i];

                int x = i * w + 2;
                int y = bh * (127 - vel) / 127;
                g.FillRectangle(chDispBackBrush, x, 2, bw, y);
                g.FillRectangle(chDispBarBrush, x, 2 + y, bw, bh - y);

                vel -= 8;
                if (vel < 0)
                    vel = 0;
                chNoteOnDataA[i] = vel;
            }
        }

        private void helpToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/110-kenichi/mame/blob/master/docs/MAmidiMEmo/Manual.pdf");
        }

    }
}
