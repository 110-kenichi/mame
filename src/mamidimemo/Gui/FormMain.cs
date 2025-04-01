// copyright-holders:K.Ito
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
using System.Security.Permissions;
using System.Runtime.CompilerServices;
using zanac.MAmidiMEmo.VSIF;
using zanac.MAmidiMEmo.Instruments.Chips;
using System.Xml.Linq;
using Melanchall.DryWetMidi.Multimedia;

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

            try
            {
                unsafe
                {
                    byte[] fontBuf = Properties.Resources.DSEG7ClassicMini_BoldItalic;
                    fixed (byte* pFontBuf = fontBuf)
                        privateFonts.AddMemoryFont((IntPtr)pFontBuf, fontBuf.Length);
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(Exception))
                    throw;
                else if (ex.GetType() == typeof(SystemException))
                    throw;
            }
            try
            {
                unsafe
                {
                    byte[] fontBuf = Properties.Resources.DSEG14ClassicMini_BoldItalic;
                    fixed (byte* pFontBuf = fontBuf)
                        privateFonts.AddMemoryFont((IntPtr)pFontBuf, fontBuf.Length);
                }
                labelClock.Font = new Font(privateFonts.Families[1], 18);
                labelCpuLoad.Font = new Font(privateFonts.Families[1], 18);
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(Exception))
                    throw;
                else if (ex.GetType() == typeof(SystemException))
                    throw;
            }
            try
            {
                unsafe
                {
                    byte[] fontBuf = Properties.Resources.PixelMplus12_Regular;
                    fixed (byte* pFontBuf = fontBuf)
                        privateFonts.AddMemoryFont((IntPtr)pFontBuf, fontBuf.Length);
                }
                labelTitle.Font = new Font(privateFonts.Families[2], 22);
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(Exception))
                    throw;
                else if (ex.GetType() == typeof(SystemException))
                    throw;
            }

            tabControlBottom.SelectedIndex = Settings.Default.MWinTab;

            Size = Settings.Default.MWinSize;
            try
            {
                splitContainer1.SplitterDistance = Settings.Default.MWinSp1Pos;
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(Exception))
                    throw;
                else if (ex.GetType() == typeof(SystemException))
                    throw;
            }
            try
            {
                splitContainer2.SplitterDistance = Settings.Default.MWinSp2Pos;
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(Exception))
                    throw;
                else if (ex.GetType() == typeof(SystemException))
                    throw;
            }

            toolStripComboBoxPort.SelectedIndex = Settings.Default.MWinPort;
            toolStripComboBoxKeyCh.SelectedIndex = Settings.Default.MWinKeyCh;
            toolStripComboBoxProgNo.SelectedIndex = Settings.Default.MWinProgNo;
            toolStripComboBoxCC.SelectedIndex = Settings.Default.MWinCC;

            try
            {
                draggableListViewMediaList.BeginUpdate();
                foreach (string fn in Settings.Default.MediaList)
                    draggableListViewMediaList.Items.Add(new ListViewItem(fn));
                draggableListViewMediaList.Columns[0].Width = -2;
            }
            catch { }
            finally
            {
                draggableListViewMediaList.EndUpdate();
            }

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
            imageList1.Images.Add("SN76477", Resources.SN76477);
            imageList1.Images.Add("uPD1771C", Resources.uPD1771C);
            imageList1.Images.Add("YM2414", Resources.YM2414);
            imageList1.Images.Add("YM3806", Resources.YM3806);
            imageList1.Images.Add("MIDITHRU", Resources.MIDITHRU);
            imageList1.Images.Add("MultiPCM", Resources.MultiPCM);
            imageList1.Images.Add("RF5C164", Resources.RF5C164);
            imageList1.Images.Add("SAA1099", Resources.SAA1099);
            imageList1.Images.Add("PAULA_8364", Resources.PAULA_8364);

            if (Program.IsVSTiMode())
            {
                toolStripComboBoxMidiIfA.Enabled = false;
                toolStripComboBoxMidiIfA.Items.Add("VSTi");
                toolStripComboBoxMidiIfA.SelectedIndex = 0;

                toolStripComboBoxMidiIfB.Enabled = false;

                mIDIDelayCheckerToolStripMenuItem.Enabled = false;
                this.ControlBox = false;

                toolStripMenuItemExit.Enabled = false;
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
                    MessageBox.Show(Resources.NoMidiPort, "Warning", MessageBoxButtons.OK);
                }
            }
            outputListView = listViewOutput;

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

            XGMWriter.RecodingStarted += XGMWriter_RecodingStarted;
            XGMWriter.RecodingStopped += XGMWriter_RecodingStopped;
            XGM2Writer.RecodingStarted += XGM2Writer_RecodingStarted;
            XGM2Writer.RecodingStopped += XGM2Writer_RecodingStopped;

            ImageUtility.AdjustControlImagesDpiScale(this);
        }

        private bool forceCloseForm;

        public void ForceClose()
        {
            forceCloseForm = true;
            Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            if (Program.IsVSTiMode() && !forceCloseForm)
            {
                e.Cancel = true;
                return;
            }

            SaveWindowStatus();
            base.OnClosing(e);
        }

        public void SaveWindowStatus()
        {
            Settings.Default.MWinPort = toolStripComboBoxPort.SelectedIndex;

            Settings.Default.MWinProgNo = toolStripComboBoxProgNo.SelectedIndex;
            Settings.Default.MWinKeyCh = toolStripComboBoxKeyCh.SelectedIndex;
            Settings.Default.MWinCC = toolStripComboBoxCC.SelectedIndex;

            Settings.Default.MWinSize = Size;

            Settings.Default.MWinSp1Pos = splitContainer1.SplitterDistance;
            Settings.Default.MWinSp2Pos = splitContainer2.SplitterDistance;

            Settings.Default.MWinTab = tabControlBottom.SelectedIndex;

            Settings.Default.MediaList = new System.Collections.Specialized.StringCollection();
            Settings.Default.MediaList.AddRange(draggableListViewMediaList.Items.Cast<ListViewItem>().Select(item => item.Text).ToArray());
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
            setSelectedObject(new InstrumentBase[] { });
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
            setSelectedObject(new InstrumentBase[] { });
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

                using (System.Drawing.Graphics graphics = CreateGraphics())
                {
                    int maxWidth = 0;
                    foreach (object obj in toolStripComboBoxMidiIfA.Items)
                    {
                        System.Drawing.SizeF area = graphics.MeasureString(obj.ToString(), toolStripComboBoxMidiIfA.Font);
                        maxWidth = Math.Max((int)area.Width, maxWidth);
                    }
                    toolStripComboBoxMidiIfA.DropDownWidth = maxWidth;
                }
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

                using (System.Drawing.Graphics graphics = CreateGraphics())
                {
                    int maxWidth = 0;
                    foreach (object obj in toolStripComboBoxMidiIfB.Items)
                    {
                        System.Drawing.SizeF area = graphics.MeasureString(obj.ToString(), toolStripComboBoxMidiIfB.Font);
                        maxWidth = Math.Max((int)area.Width, maxWidth);
                    }
                    toolStripComboBoxMidiIfB.DropDownWidth = maxWidth;
                }
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
            setSelectedObject(insts.ToArray());
            toolStripButtonPopup.Enabled = (listViewIntruments.SelectedItems.Count != 0);
        }

        private string lastCheckedLabel = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="insts"></param>
        private void setSelectedObject(InstrumentBase[] insts)
        {
            var toolStrip = propertyGrid.Controls.OfType<ToolStrip>().FirstOrDefault();

            //Retreive last selected button name
            bool foundSeparator = false;
            foreach (ToolStripItem item in toolStrip.Items)
            {
                if (!foundSeparator)
                {
                    if (item is ToolStripSeparator)
                        foundSeparator = true;
                    continue;
                }
                var menu = item as ToolStripButton;
                if (menu != null && menu.Enabled && menu.Visible && menu.Checked)
                {
                    lastCheckedLabel = menu.Text;
                    break;
                }
            }

            propertyGrid.SelectedObjects = insts;
            //HACK:
            toolStrip.Items.Add(toolStripButtonPopup);

            //Re-select last selected button
            if (lastCheckedLabel != null)
            {
                foundSeparator = false;
                foreach (ToolStripItem item in toolStrip.Items)
                {
                    if (!foundSeparator)
                    {
                        if (item is ToolStripSeparator)
                            foundSeparator = true;
                        continue;
                    }
                    var menu = item as ToolStripButton;
                    if (menu != null && menu.Enabled && menu.Visible)
                    {
                        if (string.Equals(menu.Text, lastCheckedLabel, StringComparison.Ordinal))
                        {
                            menu.PerformClick();
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

        private void yM2414ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstrumentManager.AddInstrument(InstrumentType.YM2414);
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

        private void mIDITHRUToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstrumentManager.AddMidiThru();
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

        private void sN76477ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstrumentManager.AddInstrument(InstrumentType.SN76477);
        }

        private void uPD1771ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstrumentManager.AddInstrument(InstrumentType.uPD1771C);
        }

        private void yM3806OPQToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstrumentManager.AddInstrument(InstrumentType.YM3806);
        }

        private void multiPCMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstrumentManager.AddInstrument(InstrumentType.MultiPCM);
        }

        private void rF5C164ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstrumentManager.AddInstrument(InstrumentType.RF5C164);
        }

        private void sA1099ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstrumentManager.AddInstrument(InstrumentType.SAA1099);
        }

        private void pAULA8364ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstrumentManager.AddInstrument(InstrumentType.PAULA_8364);
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
                var inst = (InstrumentBase)item.Tag;
                if (inst is MIDITHRU)
                {
                    InstrumentManager.RemoveMidiThru();
                }
                else
                {
                    var tp = inst.InstrumentType;
                    if (!insts.ContainsKey(tp))
                        insts.Add(tp, null);
                }
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
                var oinst = (InstrumentBase)item.Tag;
                if (oinst is MIDITHRU)
                {
                    var inst = InstrumentManager.AddMidiThru();
                    if (inst != null)
                        inst.SerializeData = oinst.SerializeData;
                }
                else
                {
                    var tp = oinst.InstrumentType;
                    var inst = InstrumentManager.AddInstrument(tp);
                    if (inst != null)
                        inst.SerializeData = oinst.SerializeData;
                }
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
                        loadMidiFile(file, midiFile);
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
            for (int i = 0; i < 16; i++)
            {
                var me = new ControlChangeEvent((SevenBitNumber)121, (SevenBitNumber)0);
                me.Channel = (FourBitNumber)i;
                MidiManager.SendMidiEvent(MidiPort.PortAB, me);
            }
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
                        MessageBox.Show(this, Resources.SampleRate, "Information", MessageBoxButtons.OK);
                    }
                    else
                    {
                        var rdr = MessageBox.Show(this, Resources.RestartConfirmation, "Message", MessageBoxButtons.YesNo);
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
            if (item != null && item.Value is ISerializeDataSaveLoad)
            {
                string sd = ((ISerializeDataSaveLoad)Activator.CreateInstance(item.Value.GetType())).SerializeData;
                ((ISerializeDataSaveLoad)item.Value).SerializeData = sd;
                SoftRefreshPropertyGrid();
            }
            bool enabled = false;
            try
            {
                enabled = item != null &&
                    item.GridItemType == GridItemType.Property &&
                    context != null && context.Instance != null && item.PropertyDescriptor.CanResetValue(context.Instance);
                if (enabled)
                {
                    propertyGrid.ResetSelectedProperty();
                    SoftRefreshPropertyGrid();
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(Exception))
                    throw;
                else if (ex.GetType() == typeof(SystemException))
                    throw;
            }


            //try
            //{
            //    enabled = item != null &&
            //        item.GridItemType == GridItemType.Property &&
            //        context != null && context.Instance != null && item.PropertyDescriptor.CanResetValue(context.Instance);
            //}
            //catch (Exception ex)
            //{
            //    if (ex.GetType() == typeof(Exception))
            //        throw;
            //    else if (ex.GetType() == typeof(SystemException))
            //        throw;
            //}
            //if (enabled)
            //{
            //    propertyGrid.ResetSelectedProperty();
            //    propertyGrid.Refresh();
            //}
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

                TimbreBase[] timbres = findTimbre(propertyGrid.SelectedGridItem);
                FormProp fp = null;
                if (timbres != null && timbres.Length == 0)
                    fp = new FormProp(insts.ToArray());
                else
                    fp = new FormProp(insts.ToArray().ToArray(), timbres);
                fp.StartPosition = FormStartPosition.Manual;
                fp.Location = this.Location;

                fp.Show(this);
            }
        }


        private TimbreBase[] findTimbre(GridItem item)
        {
            List<TimbreBase> il = new List<TimbreBase>();
            if (item == null)
                return il.ToArray();

            if (item.Value != null && item.Value.GetType() == typeof(object[]))
            {
                var objs = item.Value as object[];
                foreach (var o in objs)
                {
                    var inst = o as TimbreBase;
                    if (inst != null)
                        il.Add(inst);
                }
            }
            {
                var inst = item.Value as TimbreBase;
                if (inst != null)
                    il.Add(inst);
            }
            {
                var array = item.Value as Array;
                if (array != null && array.GetValue(0) is TimbreBase)
                    il.Add((TimbreBase)array.GetValue(0));
            }

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
            {
                var array = instance as Array;
                if (array != null && array.GetValue(0) is TimbreBase)
                    il.Add((TimbreBase)array.GetValue(0));
            }
            if (il.Count != 0)
                return il.ToArray();

            return findTimbre(item.Parent);
        }

        private void toolStripButton20_Click(object sender, EventArgs e)
        {
            toolStripButtonWAV.Checked = !toolStripButtonWAV.Checked;
        }

        private void toolStripButton20_CheckedChanged(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            string fname = (Path.GetFileNameWithoutExtension(loadedOrgMidiFile) ?? "MAmi") + "_" + now.ToShortDateString().Replace('/', '-') + "_" + now.ToLongTimeString().Replace(':', '-');

            if (string.IsNullOrWhiteSpace(Settings.Default.OutputDir))
                Settings.Default.OutputDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            try
            {
                Program.SoundUpdating();
                if (toolStripButtonWAV.Checked)
                {
                    if (string.IsNullOrWhiteSpace(Settings.Default.OutputDir))
                        Settings.Default.OutputDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                    string op = Path.Combine(Settings.Default.OutputDir, fname + ".wav");

                    MameIF.StartRecordingTo(op);
                }
                else
                {
                    MameIF.StopRecording();
                }
            }
            finally
            {
                Program.SoundUpdated();
            }
        }



        private void toolStripButton21_Click(object sender, EventArgs e)
        {
            toolStripButtonVGM.Checked = !toolStripButtonVGM.Checked;
        }

        private void toolStripButton21_CheckedChanged(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            string fname = (Path.GetFileNameWithoutExtension(loadedOrgMidiFile) ?? "MAmi_VGM") + "_" + now.ToShortDateString().Replace('/', '-') + "_" + now.ToLongTimeString().Replace(':', '-');


            try
            {
                Program.SoundUpdating();
                if (toolStripButtonVGM.Checked)
                {
                    if (string.IsNullOrWhiteSpace(Settings.Default.OutputDir))
                        Settings.Default.OutputDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                    string op = Path.Combine(Settings.Default.OutputDir, fname);
                    Directory.CreateDirectory(Settings.Default.OutputDir);

                    InstrumentManager.StartVgmRecordingTo(op);
                }
                else
                {
                    InstrumentManager.StopVgmRecording();
                    //Process.Start(InstrumentManager.LastVgmOutputDir);
                }
            }
            finally
            {
                Program.SoundUpdated();
            }
        }

        private void toolStripComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (toolStripComboBoxProgNo.SelectedIndex != 0)
            {
                //Program change
                var pe = new ProgramChangeEvent((SevenBitNumber)(toolStripComboBoxProgNo.SelectedIndex - 1));
                pe.Channel = (FourBitNumber)(toolStripComboBoxKeyCh.SelectedIndex & 0xf);
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
            var rdr = MessageBox.Show(this, Resources.ClearAllInsts, "Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (rdr == DialogResult.Yes)
            {
                InstrumentManager.ClearAllInstruments();
            }
        }

        private Playback midiPlayback;

        private void toolStripButtonPlay_Click(object sender, EventArgs e)
        {
            if (midiPlayback == null)
            {
                playSelectedItem();
                return;
            }

            playCore();
        }

        private void playCore()
        {
            midiPlayback.Stop();

            if (toolStripButtonAutoWav.Checked)
            {
                toolStripButtonWAV.Checked = false;
                toolStripButtonWAV.Checked = true;
            }
            if (toolStripButtonAutoVGM.Checked)
            {
                toolStripButtonVGM.Checked = false;
                toolStripButtonVGM.Checked = true;
            }

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

            if (toolStripButtonAutoWav.Checked)
                toolStripButtonWAV.Checked = false;
            if (toolStripButtonAutoVGM.Checked)
                toolStripButtonVGM.Checked = false;

            midiPlayback.Stop();
            InstrumentManager.Stop();
            midiPlayback.MoveToStart();
            if (this.labelStat.Image != null)
                this.labelStat.Image.Dispose();
            this.labelStat.Image = global::zanac.MAmidiMEmo.Properties.Resources.Stop;

            if (toolStripButtonAutoWav.Checked)
                toolStripButtonWAV.Checked = false;
            if (toolStripButtonAutoVGM.Checked)
                toolStripButtonVGM.Checked = false;
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
                        loadMidiFile(openFileDialogMidi.FileName, openFileDialogMidi.FileName);
                        break;
                    case ".MAMIDI":
                        loadMAmidiFile(openFileDialogMidi.FileName);
                        break;
                }
            }
        }

        private string loadedMidiFile;

        private string loadedOrgMidiFile;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fn"></param>
        private void loadMidiFile(string orgFileName, string fn)
        {
            try
            {
                toolStripButtonStop_Click(null, EventArgs.Empty);

                var midiFile = MidiFile.Read(fn);

                loadedMidiFile = fn;
                loadedOrgMidiFile = orgFileName;

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
                labelTitle.SetText(Path.GetFileName(fn));
                labelTitle.Tag = new object();

                midiPlayback?.Dispose();
                PlaybackSettings ps = new PlaybackSettings();
                midiPlayback = midiFile.GetPlayback(new InternalMidiPlayerDevice(), ps);
                midiPlayback.EventPlayed += MidiPlayback_EventPlayed;
                midiPlayback.Finished += MidiPlayback_Finished;

                InstrumentBase.MasterGain = (float)metroTrackBarVol.Value / 100f;

                playCore();
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MidiPlayback_Finished(object sender, EventArgs e)
        {
            this.BeginInvoke(new MethodInvoker(() =>
            {
                if(labelStat.IsDisposed)
                    return;

                this.labelStat.Image = global::zanac.MAmidiMEmo.Properties.Resources.Stop;

                int idx = 0;
                if (currentSongItem != null)
                    idx = currentSongItem.Index;
                if (idx < 0 && draggableListViewMediaList.Items.Count != 0)
                {
                    playItem(0);
                }
                else
                {
                    idx++;
                    if (idx < draggableListViewMediaList.Items.Count)
                        playItem(idx);
                }
            }));
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
            loadMidiFile(loadedMidiFile, loadedMidiFile);
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
                                loadMidiFile(drags[0], drags[0]);
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

        private void MidiManager_MidiEventReceivedA(object sender, MidiEvent[] es)
        {
            foreach (MidiEvent e in es)
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
        }

        private void MidiManager_MidiEventReceivedB(object sender, MidiEvent[] es)
        {
            foreach (MidiEvent e in es)
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

        private void openSampleFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(Path.Combine(Program.MAmiDir, @"Samples"));
        }

        private void copyMAmiVSTiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string lastDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (!string.IsNullOrWhiteSpace(Settings.Default.LastVSTiFolder))
                lastDir = Settings.Default.LastVSTiFolder;

            betterFolderBrowserVSTi.RootFolder = lastDir;
            betterFolderBrowserVSTi.Title = Resources.SelectDAWFolder;
            var result = betterFolderBrowserVSTi.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                try
                {
                    string pluginDir = Path.Combine(betterFolderBrowserVSTi.SelectedPath, "MAmi");

                    //create dir tor copy
                    if (!Directory.Exists(pluginDir))
                        Directory.CreateDirectory(pluginDir);

                    //copy dll
                    string dllFilePath = Path.Combine(Program.MAmiDir, @"VST\MAmiVSTi.dll");
                    File.Copy(dllFilePath, Path.Combine(pluginDir, @"MAmiVSTi.dll"), true);
                    //create ini
                    using (var sw = File.CreateText(Path.Combine(pluginDir, "MAmiVSTi.ini")))
                    {
                        sw.WriteLine("[MAmi]");
                        sw.WriteLine($"MAmiDir = {Path.Combine(Program.MAmiDir, @"MAmidiMEmo.exe")}");
                    }

                    Settings.Default.LastVSTiFolder = Path.GetDirectoryName(pluginDir);

                    MessageBox.Show(Resources.CopiedVSTi);
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

        private void propertyGrid_PropertyTabChanged(object sender, PropertyTabChangedEventArgs e)
        {
            selectTopItem(sender);
        }

        private void propertyGrid_SelectedObjectsChanged(object sender, EventArgs e)
        {
            selectTopItem(sender);
        }

        private static void selectTopItem(object sender)
        {
            PropertyGrid propertyGrid = (PropertyGrid)sender;
            if (!propertyGrid.IsHandleCreated)
                return;
            propertyGrid.BeginInvoke(new MethodInvoker(() =>
            {
                if (propertyGrid.IsDisposed)
                    return;

                // get selected item
                GridItem gi = propertyGrid.SelectedGridItem;
                // get category for selected item
                if (gi != null)
                {
                    try
                    {
                        GridItem pgi = gi.Parent;
                        if (pgi != null && gi.Parent.Parent != null)
                            pgi = gi.Parent.Parent;
                        if (pgi != null)
                        {
                            //sort categories
                            List<GridItem> sortedCats = new List<GridItem>(pgi.GridItems.Cast<GridItem>());
                            sortedCats.Sort(delegate (GridItem gi1, GridItem gi2) { return gi1.Label.CompareTo(gi2.Label); });

                            // loop to first category
                            for (int i = 0; i < pgi.GridItems.Count; i++)
                            {
                                if (pgi.GridItems[i] == gi)
                                    break; // in case full circle done
                                           // select if first category
                                if (pgi.GridItems[i].Label == sortedCats[0].Label)
                                {
                                    pgi.GridItems[i].Select();
                                    break;
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
                    }
                }
            }));
        }

        private void draggableListView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            switch (draggableListViewMediaList.Sorting)
            {
                case SortOrder.Ascending:
                    draggableListViewMediaList.Sorting = SortOrder.Descending;
                    draggableListViewMediaList.Columns[0].Text = "File name ▼";
                    break;
                case SortOrder.Descending:
                    draggableListViewMediaList.Sorting = SortOrder.None;
                    draggableListViewMediaList.Columns[0].Text = "File name";
                    break;
                case SortOrder.None:
                    draggableListViewMediaList.Sorting = SortOrder.Ascending;
                    draggableListViewMediaList.Columns[0].Text = "File name ▲";
                    break;
            }
        }

        private ListViewItem currentSongItem;

        private void playSelectedItem()
        {
            if (draggableListViewMediaList.SelectedItems.Count != 0)
            {
                currentSongItem = draggableListViewMediaList.SelectedItems[0];
                draggableListViewMediaList.SelectedItems.Clear();

                toolStripButtonStop_Click(null, null);

                string ext = Path.GetExtension(currentSongItem.Text);
                switch (ext.ToUpperInvariant())
                {
                    case ".MIDI":
                    case ".MID":
                    case ".SMF":
                        loadMidiFile(currentSongItem.Text, currentSongItem.Text);
                        break;
                    case ".MAMIDI":
                        loadMAmidiFile(currentSongItem.Text);
                        break;
                }
            }
            else
            {
                playItem(0);
            }
        }

        private void draggableListView1_DoubleClick(object sender, EventArgs e)
        {
            playSelectedItem();
        }

        private void draggableListView1_ItemDrag(object sender, ItemDragEventArgs e)
        {

        }

        private void draggableListView1_KeyDown(object sender, KeyEventArgs e)
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
                    draggableListViewMediaList.BeginUpdate();
                    foreach (ListViewItem item in draggableListViewMediaList.Items)
                        item.Selected = true;
                }
                finally
                {
                    draggableListViewMediaList.EndUpdate();
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
                        draggableListViewMediaList.BeginUpdate();

                        draggableListViewMediaList.SelectedItems.Clear();
                        lvi = addAllFiles(files.Cast<string>().ToArray(), lvi);
                    }
                    finally
                    {
                        draggableListViewMediaList.EndUpdate();
                        lvi?.EnsureVisible();
                    }
                }
            }
        }


        private void removeSelectedItem()
        {
            try
            {
                draggableListViewMediaList.BeginUpdate();
                int index = 0;
                for (int i = 0; i < draggableListViewMediaList.SelectedItems.Count; i++)
                {
                    // 現在選択している行のインデックスを取得
                    index = draggableListViewMediaList.SelectedItems[0].Index;
                    if ((0 <= index) && (index < draggableListViewMediaList.Items.Count))
                    {
                        draggableListViewMediaList.Items.RemoveAt(index);
                        i--;
                    }
                }
                if (index < draggableListViewMediaList.Items.Count)
                {
                    draggableListViewMediaList.Items[index].Selected = true;
                    draggableListViewMediaList.Items[index].EnsureVisible();
                }
                else if (draggableListViewMediaList.Items.Count != 0)
                {
                    draggableListViewMediaList.Items[draggableListViewMediaList.Items.Count - 1].Selected = true;
                    draggableListViewMediaList.Items[draggableListViewMediaList.Items.Count - 1].EnsureVisible();
                }
            }
            finally
            {
                draggableListViewMediaList.EndUpdate();
            }
        }

        private void draggableListView1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                addFilesToList((string[])e.Data.GetData(DataFormats.FileDrop, false));
            }
        }

        private void addFilesToList(string[] files)
        {
            ListViewItem lvi = null;
            try
            {
                draggableListViewMediaList.BeginUpdate();

                draggableListViewMediaList.SelectedItems.Clear();
                lvi = addAllFiles(files, lvi);
                draggableListViewMediaList.Columns[0].Width = -2;
            }
            finally
            {
                draggableListViewMediaList.EndUpdate();
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
                        case ".MIDI":
                        case ".MID":
                        case ".SMF":
                        case ".MAMIDI":
                            lvi = new ListViewItem(fp);
                            draggableListViewMediaList.Items.Add(lvi);
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

        private void draggableListView1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
            else
                e.Effect = DragDropEffects.None;
        }

        private void draggableListView1_DragLeave(object sender, EventArgs e)
        {

        }

        private void draggableListView1_DragOver(object sender, DragEventArgs e)
        {

        }

        private void draggableListView1_SizeChanged(object sender, EventArgs e)
        {
            //draggableListView1.Columns[0].Width = -2;
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
            var item = draggableListViewMediaList.FocusedItem;
            if (item != null)
            {
                Task.Run(new Action(() =>
                {
                    Process.Start("explorer.exe", "/select,\"" + item.Text + "\"");
                }));
            }
        }

        private void playFile(string fileName)
        {
            //currentSongItem = draggableListView1.SelectedItems[0];
            draggableListViewMediaList.SelectedItems.Clear();

            toolStripButtonStop_Click(null, null);

            string ext = Path.GetExtension(fileName);
            switch (ext.ToUpperInvariant())
            {
                case ".MIDI":
                case ".MID":
                case ".SMF":
                    loadMidiFile(currentSongItem.Text, currentSongItem.Text);
                    break;
                case ".MAMIDI":
                    loadMAmidiFile(currentSongItem.Text);
                    break;
            }
        }

        private void stopCurrentSong()
        {
            if (currentSongItem != null)
            {
                currentSongItem.Selected = true;
                currentSongItem.EnsureVisible();
            }
            toolStripButtonStop_Click(null, null);
        }

        private void playItem(int idx)
        {
            if (idx >= draggableListViewMediaList.Items.Count)
                idx = draggableListViewMediaList.Items.Count - 1;
            if (idx < 0)
                return;

            currentSongItem = draggableListViewMediaList.Items[idx];
            draggableListViewMediaList.SelectedItems.Clear();

            stopCurrentSong();
            playFile(currentSongItem.Text);

            draggableListViewMediaList.SelectedItem = currentSongItem;
            draggableListViewMediaList.FocusedItem = currentSongItem;
        }

        private void toolStripButton23_Click(object sender, EventArgs e)
        {
            int idx = 0;
            if (currentSongItem != null)
                idx = currentSongItem.Index;
            if (idx < 0 && draggableListViewMediaList.Items.Count != 0)
            {
                playItem(0);
            }
            else
            {
                idx++;
                if (idx >= draggableListViewMediaList.Items.Count)
                    idx = 0;
                playItem(idx);
            }
        }

        private void toolStripButton22_Click(object sender, EventArgs e)
        {
            int idx = 0;
            if (currentSongItem != null)
                idx = currentSongItem.Index;
            if (idx < 0 && draggableListViewMediaList.Items.Count != 0)
            {
                playItem(0);
            }
            else
            {
                idx--;
                if (idx < 0)
                    idx = draggableListViewMediaList.Items.Count - 1;
                playItem(idx);
            }
        }

        private void draggableListView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            draggableListViewMediaList.Items.Cast<ListViewItem>()
                .ToList().ForEach(item =>
                {
                    item.BackColor = SystemColors.Window;
                    item.ForeColor = SystemColors.WindowText;
                });
            draggableListViewMediaList.SelectedItems.Cast<ListViewItem>()
                .ToList().ForEach(item =>
                {
                    item.BackColor = SystemColors.Highlight;
                    item.ForeColor = SystemColors.HighlightText;
                });
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

        private static int GET_APPCOMMAND_LPARAM(IntPtr lParam)
        {
            return (int)(lParam.ToInt64() >> 16) & 0xFFF;
        }

        protected override void WndProc(ref Message m)
        {
            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();

            switch (m.Msg)
            {
                case NativeConstants.WM_APPCOMMAND:
                    switch ((ApplicationCommand)(GET_APPCOMMAND_LPARAM(m.LParam)))
                    {
                        case ApplicationCommand.MediaFastForward:
                            goto default;
                        case ApplicationCommand.MediaRewind:
                            goto default;
                        case ApplicationCommand.MediaPause:
                            toolStripButtonPause.PerformClick();
                            goto default;
                        case ApplicationCommand.MediaPlay:
                            toolStripButtonPlay.PerformClick();
                            goto default;
                        case ApplicationCommand.MediaPlayPause:
                            if (midiPlayback == null)
                                toolStripButtonPlay.PerformClick();
                            else
                                toolStripButtonPause.PerformClick();
                            goto default;
                        case ApplicationCommand.MediaNexttrack:
                            toolStripButtonNext.PerformClick();
                            goto default;
                        case ApplicationCommand.MediaPrevioustrack:
                            toolStripButtonPrev.PerformClick();
                            goto default;
                        case ApplicationCommand.MediaStop:
                            toolStripButtonStop.PerformClick();
                            goto default;
                        case ApplicationCommand.VolumeDown:
                            goto default;
                        case ApplicationCommand.VolumeUp:
                            goto default;
                        case ApplicationCommand.VolumeMute:
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

        private void mIDIDelayCheckerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var mt = new FormMidiTest())
            {
                mt.ShowDialog();
            }
        }

        private void toolStripButton22_Click_1(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Settings.Default.OutputDir))
                Settings.Default.OutputDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            Directory.CreateDirectory(Settings.Default.OutputDir);

            var opn2s = InstrumentManager.GetInstruments((int)(InstrumentType.YM2612 + 1)).ToArray();
            var dcsgs = InstrumentManager.GetInstruments((int)(InstrumentType.SN76496 + 1)).ToArray();
            try
            {
                Program.SoundUpdating();

                if (!toolStripButtonXGM.Checked)
                {
                    for (int i = 0; i < Math.Max(opn2s.Length, dcsgs.Length); i++)
                    {
                        var xgmw = new XGMWriter();
                        xgmw.RecordStart(Settings.Default.OutputDir, (uint)i);
                    }
                }
                else
                {
                    for (int i = 0; i < Math.Max(opn2s.Length, dcsgs.Length); i++)
                    {
                        if (i < opn2s.Length)
                            ((YM2612)opn2s[i]).XgmWriter?.RecordStop(true);
                        else if (i < dcsgs.Length)
                            ((SN76496)dcsgs[i]).XgmWriter?.RecordStop(true);
                    }
                }
            }
            finally
            {
                Program.SoundUpdated();
            }
        }

        private int xgmRecordingCount;

        private void XGMWriter_RecodingStarted(object sender, EventArgs e)
        {
            xgmRecordingCount++;
            if (xgmRecordingCount > 0)
                toolStripButtonXGM.Checked = true;
        }

        private void XGMWriter_RecodingStopped(object sender, EventArgs e)
        {
            xgmRecordingCount--;
            if (xgmRecordingCount == 0)
                toolStripButtonXGM.Checked = false;
        }


        private void toolStripButtonXGM2_Click_1(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Settings.Default.OutputDir))
                Settings.Default.OutputDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            Directory.CreateDirectory(Settings.Default.OutputDir);

            var opn2s = InstrumentManager.GetInstruments((int)(InstrumentType.YM2612 + 1)).ToArray();
            var dcsgs = InstrumentManager.GetInstruments((int)(InstrumentType.SN76496 + 1)).ToArray();
            try
            {
                Program.SoundUpdating();

                if (!toolStripButtonXGM2.Checked)
                {
                    for (int i = 0; i < Math.Max(opn2s.Length, dcsgs.Length); i++)
                    {
                        var xgmw = new XGM2Writer();
                        xgmw.RecordStart(Settings.Default.OutputDir, (uint)i);
                    }
                }
                else
                {
                    for (int i = 0; i < Math.Max(opn2s.Length, dcsgs.Length); i++)
                    {
                        if (i < opn2s.Length)
                            ((YM2612)opn2s[i]).Xgm2Writer?.RecordStop(true);
                        else if (i < dcsgs.Length)
                            ((SN76496)dcsgs[i]).Xgm2Writer?.RecordStop(true);
                    }
                }
            }
            finally
            {
                Program.SoundUpdated();
            }
        }

        private int xgm2RecordingCount;

        private void XGM2Writer_RecodingStarted(object sender, EventArgs e)
        {
            xgm2RecordingCount++;
            if (xgm2RecordingCount > 0)
                toolStripButtonXGM2.Checked = true;
        }

        private void XGM2Writer_RecodingStopped(object sender, EventArgs e)
        {
            xgm2RecordingCount--;
            if (xgm2RecordingCount == 0)
                toolStripButtonXGM2.Checked = false;
        }

        private string copiedValue;

        private object copiedValueInstance;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void copySerializeDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = propertyGrid.SelectedGridItem;
            ITypeDescriptorContext context = item as ITypeDescriptorContext;
            bool enabled = false;
            try
            {
                enabled = item != null && (item.PropertyDescriptor.Name == "SerializeDataSave" || item.PropertyDescriptor.Name == "SerializeDataLoad");
                enabled |= item != null && item.Value is ISerializeDataSaveLoad;
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
                if (item != null && (item.PropertyDescriptor.Name == "SerializeDataSave" || item.PropertyDescriptor.Name == "SerializeDataLoad"))
                {
                    copiedValue = item.PropertyDescriptor.GetValue(context.Instance) as string;
                    copiedValueInstance = context.Instance;
                }
                else if (item != null && item.Value is ISerializeDataSaveLoad)
                {
                    copiedValue = ((ISerializeDataSaveLoad)item.Value).SerializeData;
                    copiedValueInstance = item.Value;
                }
            }
        }

        private void pasteSerializeDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = propertyGrid.SelectedGridItem;
            ITypeDescriptorContext context = item as ITypeDescriptorContext;
            bool enabled = false;
            try
            {
                enabled = item != null && (item.PropertyDescriptor.Name == "SerializeDataSave" || item.PropertyDescriptor.Name == "SerializeDataLoad");
                enabled |= item != null && item.Value is ISerializeDataSaveLoad;
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(Exception))
                    throw;
                else if (ex.GetType() == typeof(SystemException))
                    throw;
            }
            if (enabled && copiedValue != null && copiedValueInstance != null)
            {
                if (item != null &&
                    (item.PropertyDescriptor.Name == "SerializeDataSave" || item.PropertyDescriptor.Name == "SerializeDataLoad"))
                {
                    if (copiedValueInstance.GetType() == context.Instance.GetType())
                    {
                        item.PropertyDescriptor.SetValue(context.Instance, copiedValue);
                        SoftRefreshPropertyGrid();
                    }
                }
                else if (item != null && item.Value is ISerializeDataSaveLoad)
                {
                    if (copiedValueInstance.GetType() == item.Value.GetType())
                    {
                        ((ISerializeDataSaveLoad)item.Value).SerializeData = copiedValue;
                        SoftRefreshPropertyGrid();
                    }
                }
            }
        }

        private void contextMenuStripProp_Opening(object sender, CancelEventArgs e)
        {
            var item = propertyGrid.SelectedGridItem;
            ITypeDescriptorContext context = item as ITypeDescriptorContext;

            {
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
                enabled |= item != null && item.Value is ISerializeDataSaveLoad;
                resetToDefaultThisPropertyToolStripMenuItem.Enabled = enabled;
            }

            try
            {
                bool enabled = item != null && (item.PropertyDescriptor.Name == "SerializeDataSave" || item.PropertyDescriptor.Name == "SerializeDataLoad");
                enabled |= item != null && item.Value is ISerializeDataSaveLoad;

                copySerializeDataToolStripMenuItem.Enabled = enabled;
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(Exception))
                    throw;
                else if (ex.GetType() == typeof(SystemException))
                    throw;
            }


            try
            {
                bool enabled = item != null && (item.PropertyDescriptor.Name == "SerializeDataSave" || item.PropertyDescriptor.Name == "SerializeDataLoad");
                enabled |= item != null && item.Value is ISerializeDataSaveLoad;

                if (enabled && copiedValue != null && copiedValueInstance != null)
                {
                    if (item != null && (item.PropertyDescriptor.Name == "SerializeDataSave" || item.PropertyDescriptor.Name == "SerializeDataLoad"))
                    {
                        pasteSerializeDataToolStripMenuItem.Enabled = (copiedValueInstance.GetType() == context.Instance.GetType());
                    }
                    else if (item != null && item.Value is ISerializeDataSaveLoad)
                    {
                        pasteSerializeDataToolStripMenuItem.Enabled = (copiedValueInstance.GetType() == item.Value.GetType());
                    }
                    else
                        pasteSerializeDataToolStripMenuItem.Enabled = false;
                }
                else
                    pasteSerializeDataToolStripMenuItem.Enabled = false;
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(Exception))
                    throw;
                else if (ex.GetType() == typeof(SystemException))
                    throw;
            }
        }



        public void SoftRefreshPropertyGrid()
        {
            timer1.Start();
            /*
            try
            {
                foreach (Control c in propertyGrid.Controls)
                    PropertyGridUtility.BeginUpdate(c);

                //var sgi = propertyGrid.SelectedGridItem;
                //string sname = null;
                //string cname = null;
                //if (sgi != null)
                //{
                //    cname = sgi.Label;
                //    if (sgi.PropertyDescriptor != null)
                //        sname = sgi.PropertyDescriptor.Name;
                //}

                ScrollBar sb = PropertyGridUtility.GetPropertyGridViewScrollBar(propertyGrid);
                int lsv = -1;
                if (sb != null)
                    lsv = sb.Value;
                bool vis = PropertyGridUtility.GridViewEditVisible(propertyGrid);

                propertyGrid.Refresh();

                if (sb != null)
                    PropertyGridUtility.SetPropertyGridViewScrollOffset(propertyGrid, lsv);
                if (!vis)
                    PropertyGridUtility.CommonEditorHide(propertyGrid);

                //PropertyGridUtility.SelectGridItem(propertyGrid, cname, sname);
            }
            finally
            {
                foreach (Control c in propertyGrid.Controls)
                {
                    PropertyGridUtility.EndUpdate(c);
                    c.Invalidate(true);
                    c.Update();
                }
            }
            */
        }

        private void timer1_Tick_1(object sender, EventArgs e)
        {
            if (!propertyGrid.IsDisposed)
                propertyGrid.Refresh();
            timer1.Stop();
        }

    }
}
