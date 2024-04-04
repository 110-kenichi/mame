// copyright-holders:K.Ito
using FM_SoundConvertor;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using MetroFramework.Forms;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using NAudio.SoundFont;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using zanac.MAmidiMEmo.Instruments;
using zanac.MAmidiMEmo.Instruments.Chips;
using zanac.MAmidiMEmo.Midi;
using zanac.MAmidiMEmo.Properties;
using zanac.MAmidiMEmo.Util;
using zanac.MAmidiMEmo.Util.FITOM;
using zanac.MAmidiMEmo.Util.Syx;
using static System.Net.WebRequestMethods;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace zanac.MAmidiMEmo.Gui.FMEditor
{
    public partial class FormFmEditor : FormBase
    {
        private Dictionary<String, RegisterContainerBase> controls = new Dictionary<string, RegisterContainerBase>();

        private bool singleSelect;

        private static PropertySort propAlphabetical;

        public InstrumentBase Instrument
        {
            get;
            private set;
        }

        private TimbreBase timbre;

        public TimbreBase Timbre
        {
            get
            {
                return timbre;
            }
            private set
            {
                timbre = value;
                propertyGrid.SelectedObject = value;
                propertyGrid.RefreshTabs(PropertyTabScope.Component);
            }
        }

        public int TimbreNo
        {
            get;
            private set;
        } = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemName"></param>
        /// <returns></returns>
        public RegisterContainerBase GetControl(string itemName)
        {
            return controls[itemName];
        }

        public RegisterContainerBase this[string itemName]
        {
            get
            {
                return controls[itemName];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public FormFmEditor()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        public FormFmEditor(InstrumentBase inst, TimbreBase timbre) : this(inst, timbre, true)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public FormFmEditor(InstrumentBase inst, TimbreBase timbre, bool singleSelect)
        {
            InitializeComponent();

            propertyGrid.PropertySort = propAlphabetical;

            this.singleSelect = singleSelect;

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
            try
            {
                splitContainer1.SplitterDistance = Settings.Default.FmSp1Pos;
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(Exception))
                    throw;
                else if (ex.GetType() == typeof(SystemException))
                    throw;
            }

            if (singleSelect)
                pianoControl1.TargetTimbres = new TimbreBase[] { timbre };
            this.Timbre = timbre;
            this.Instrument = inst;

            ignoreMetroComboBoxTimbres_SelectedIndexChanged = true;
            for (int i = 0; i < Instrument.BaseTimbres.Length; i++)
            {
                metroComboBoxTimbres.Items.Add(new TimbreItem(inst.BaseTimbres[i], i));
                if (inst.BaseTimbres[i] == timbre)
                    TimbreNo = i;
            }
            metroComboBoxTimbres.SelectedIndex = TimbreNo;
            ignoreMetroComboBoxTimbres_SelectedIndexChanged = false;

            if (singleSelect)
            {
                metroComboBoxTimbres.Enabled = false;
                metroButtonImportAll.Enabled = false;
                metroButtonImportAllGit.Enabled = false;
                metroButtonCopy.Enabled = false;
                metroButtonPaste.Enabled = false;
                metroButtonExportAll.Enabled = false;
                metroTextBoxPatchFile.Text = ((IFmTimbre)Timbre).PatchInfo;
            }

            setTitle();

            InstrumentManager.InstrumentChanged += InstrumentManager_InstrumentChanged;
            InstrumentManager.InstrumentRemoved += InstrumentManager_InstrumentRemoved;

            pianoControl1.NoteOn += PianoControl1_NoteOn;
            pianoControl1.NoteOff += PianoControl1_NoteOff;
            pianoControl1.EntryDataChanged += PianoControl1_EntryDataChanged;

            Midi.MidiManager.MidiEventHooked += MidiManager_MidiEventHooked;
        }

        protected override void OnShown(EventArgs e)
        {
            toolStripComboBoxCh.Focus();
            base.OnShown(e);
        }

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
                return "No " + Number.ToString() + " " + Timbre.TimbreName;
            }
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

            Settings.Default.MWinSp1Pos = splitContainer1.SplitterDistance;

            propAlphabetical = propertyGrid.PropertySort;

            base.OnClosing(e);
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

            try
            {
                splitContainer1.SplitterDistance = Settings.Default.FmSp1Pos;
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(Exception))
                    throw;
                else if (ex.GetType() == typeof(SystemException))
                    throw;
            }
        }

        private void setTitle()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(Instrument.Name + "(" + Instrument.UnitNumber + ")");
            sb.Append(" - Instrument");

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
            Close();
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
            Close();
        }

        private TimbreBase[] findTimbre(GridItem item)
        {
            List<TimbreBase> il = new List<TimbreBase>();
            if (item == null)
                return il.ToArray();

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
            if (il.Count != 0)
                return il.ToArray();

            return findTimbre(item.Parent);
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

        private bool ignoreMetroComboBoxTimbres_SelectedIndexChanged;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void metroComboBoxTimbres_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ignoreMetroComboBoxTimbres_SelectedIndexChanged)
                return;

            TimbreItem ti = (TimbreItem)metroComboBoxTimbres.Items[metroComboBoxTimbres.SelectedIndex];
            this.Timbre = ti.Timbre;
            this.TimbreNo = ti.Number;

            try
            {
                ignorePlayingFlag++;
                ApplyTimbre(ti.Timbre);
                metroTextBoxPatchFile.Text = ((IFmTimbre)Timbre).PatchInfo;
            }
            finally
            {
                ignorePlayingFlag--;
            }
            Control_ValueChanged(this, null);
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

                e.Tag = new NoteOnTimbreInfo(Timbre, TimbreNo);
                e.Channel = (FourBitNumber)toolStripComboBoxCh.SelectedIndex;
                Instrument.NotifyMidiEvent(e);
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
        /// <param name="control"></param>
        protected void AddControl(RegisterContainerBase control)
        {
            controls.Add(control.RegisterName, control);

            control.Dock = DockStyle.Top;
            flowLayoutPanel1.Controls.Add(control);

            control.ParentEditor = this;

            control.ValueChanged += Control_ValueChanged;
        }

        private object playing;
        private SevenBitNumber ni;
        private SevenBitNumber vi;

        private int ignorePlayingFlag;

        /// <summary>
        /// 
        /// </summary>
        public bool IgnoreControlValueChanged
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Control_ValueChanged(object sender, PropertyChangedEventArgs e)
        {
            if (IgnoreControlValueChanged)
                return;

            if (ModifierKeys == Keys.Shift)
            {
                IgnoreControlValueChanged = true;
                try
                {
                    RegisterBase reg = (RegisterBase)sender;
                    foreach (RegisterContainerBase container in controls.Values)
                    {
                        if (reg.Parent == container || !container.Follow || !container.Enabled)
                            continue;
                        foreach (var r in container.RegisterControls)
                        {
                            if (reg.ItemName.Equals(r.ItemName, StringComparison.Ordinal))
                            {
                                try
                                {
                                    switch (reg)
                                    {
                                        case RegisterFlag rf:
                                            {
                                                var rf2 = r as RegisterFlag;
                                                if (rf2 != null)
                                                    rf2.Value = rf.Value;
                                                break;
                                            }
                                        case RegisterValue rc:
                                            {
                                                var rc2 = r as RegisterValue;
                                                if (rc2 != null)
                                                {
                                                    rc2.Value = rc.Value;
                                                }
                                            }
                                            break;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    if (ex.GetType() == typeof(Exception))
                                        throw;
                                    else if (ex.GetType() == typeof(SystemException))
                                        throw;
                                }
                                break;
                            }
                        }
                    }
                }
                finally
                {
                    IgnoreControlValueChanged = false;
                }
            }
            if (ModifierKeys == Keys.Control)
            {
                IgnoreControlValueChanged = true;
                try
                {
                    RegisterBase reg = (RegisterBase)sender;
                    foreach (RegisterContainerBase container in controls.Values)
                    {
                        if (reg.Parent == container || !container.Follow || !container.Enabled)
                            continue;
                        foreach (var r in container.RegisterControls)
                        {
                            if (reg.ItemName.Equals(r.ItemName, StringComparison.Ordinal))
                            {
                                try
                                {
                                    switch (reg)
                                    {
                                        case RegisterFlag rf:
                                            {
                                                var rf2 = r as RegisterFlag;
                                                if (rf2 != null)
                                                    rf2.Value = rf.Value;
                                                break;
                                            }
                                        case RegisterValue rc:
                                            {
                                                var rc2 = r as RegisterValue;
                                                if (rc2 != null)
                                                {
                                                    rc2.Value += rc.Value - rc.PreviousValue;
                                                }
                                            }
                                            break;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    if (ex.GetType() == typeof(Exception))
                                        throw;
                                    else if (ex.GetType() == typeof(SystemException))
                                        throw;
                                }
                                break;
                            }
                        }
                    }
                }
                finally
                {
                    IgnoreControlValueChanged = false;
                }
            }
            if (toolStripButtonPlay.Checked && ignorePlayingFlag == 0 && Visible)
            {
                await testPlay();
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

        private void metroButtonRandParams_Click(object sender, EventArgs e)
        {
            var names = metroTextBoxTarget.Text.Trim().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> targetName = new List<string>();
            List<string> untargetName = new List<string>();
            for (int i = 0; i < names.Length; i++)
            {
                if (!names[i].StartsWith("!"))
                    targetName.Add(names[i].Trim());
                else if (names[i].Length > 1)
                    untargetName.Add(names[i].Substring(1).Trim());
            }
            var minmaxnames = metroTextBoxTargetMinMax.Text.Trim().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string, (int min, int max)> targetNameMinMax = new Dictionary<string, (int, int)>();
            for (int i = 0; i < minmaxnames.Length; i++)
            {
                string[] nmm = minmaxnames[i].Trim().Split(new char[] { '=', '-' }, StringSplitOptions.RemoveEmptyEntries);
                if (nmm.Length == 3)
                {
                    try
                    {
                        targetNameMinMax.Add(nmm[0], (int.Parse(nmm[1]), int.Parse(nmm[2])));
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

            try
            {
                ignorePlayingFlag++;

                var rand = new Random(DateTime.Now.GetHashCode());
                foreach (var rcb in controls.Values)
                {
                    foreach (var rc in rcb.RegisterControls)
                    {
                        RegisterValue rv = rc as RegisterValue;
                        if (rv != null)
                        {
                            if (targetName.Count != 0)
                            {
                                bool match = false;
                                foreach (var rn in targetName)
                                {
                                    if (rn.Equals(rv.ItemName, StringComparison.Ordinal))
                                    {
                                        match = true;
                                        break;
                                    }
                                }
                                if (!match)
                                    continue;
                            }
                            if (untargetName.Count != 0)
                            {
                                bool match = false;
                                foreach (var rn in untargetName)
                                {
                                    if (rn.Equals(rv.ItemName, StringComparison.Ordinal))
                                    {
                                        match = true;
                                        break;
                                    }
                                }
                                if (match)
                                    continue;
                            }

                            var v = rand.Next(-1, 2);

                            int min = rv.Minimum;
                            int max = rv.Maximum;
                            if (targetNameMinMax.ContainsKey(rv.ItemName))
                            {
                                var minmax = targetNameMinMax[rv.ItemName];
                                if (min <= minmax.min && minmax.min <= max)
                                    min = minmax.min;
                                if (min <= minmax.max && minmax.max <= max)
                                    max = minmax.max;
                            }

                            if (rv.IsNullable)
                            {
                                var cv = rv.NullableValue;
                                if (cv.HasValue)
                                    cv += v;
                                else
                                    cv = v;
                                if (cv < rv.Minimum)
                                    cv = null;
                                else if (cv < min)
                                    cv = min;
                                else if (cv > max)
                                    cv = max;

                                rv.NullableValue = cv;
                            }
                            else
                            {
                                var cv = rv.Value;

                                cv += v;
                                if (cv < min)
                                    cv = min;
                                else if (cv > max)
                                    cv = max;

                                rv.Value = cv;
                            }
                        }
                    }
                }
            }
            finally
            {
                ignorePlayingFlag--;
            }
            Control_ValueChanged(this, null);
        }

        private void metroButtonRandAll_Click(object sender, EventArgs e)
        {
            var names = metroTextBoxTarget.Text.Trim().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> targetName = new List<string>();
            List<string> untargetName = new List<string>();
            for (int i = 0; i < names.Length; i++)
            {
                if (!names[i].StartsWith("!"))
                    targetName.Add(names[i].Trim());
                else if (names[i].Length > 1)
                    untargetName.Add(names[i].Substring(1).Trim());
            }

            var minmaxnames = metroTextBoxTargetMinMax.Text.Trim().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string, (int min, int max)> targetNameMinMax = new Dictionary<string, (int, int)>();
            for (int i = 0; i < minmaxnames.Length; i++)
            {
                string[] nmm = minmaxnames[i].Trim().Split(new char[] { '=', '-' }, StringSplitOptions.RemoveEmptyEntries);
                if (nmm.Length == 3)
                {
                    try
                    {
                        targetNameMinMax.Add(nmm[0], (int.Parse(nmm[1]), int.Parse(nmm[2])));
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

            try
            {
                ignorePlayingFlag++;

                var rand = new Random(DateTime.Now.GetHashCode());
                foreach (var rcb in controls.Values)
                {
                    foreach (var rc in rcb.RegisterControls)
                    {
                        RegisterValue rv = rc as RegisterValue;
                        if (rv != null)
                        {
                            if (targetName.Count != 0)
                            {
                                bool match = false;
                                foreach (var rn in targetName)
                                {
                                    if (rn.Equals(rv.ItemName, StringComparison.Ordinal))
                                    {
                                        match = true;
                                        break;
                                    }
                                }
                                if (!match)
                                    continue;
                            }
                            if (untargetName.Count != 0)
                            {
                                bool match = false;
                                foreach (var rn in untargetName)
                                {
                                    if (rn.Equals(rv.ItemName, StringComparison.Ordinal))
                                    {
                                        match = true;
                                        break;
                                    }
                                }
                                if (match)
                                    continue;
                            }
                            int min = rv.Minimum;
                            int max = rv.Maximum;
                            if (targetNameMinMax.ContainsKey(rv.ItemName))
                            {
                                var minmax = targetNameMinMax[rv.ItemName];
                                if (min <= minmax.min && minmax.min <= max)
                                    min = minmax.min;
                                if (min <= minmax.max && minmax.max <= max)
                                    max = minmax.max;
                            }

                            if (rv.IsNullable)
                            {
                                var v = rand.Next(min - 1, max + 1);
                                if (v < rv.Minimum)
                                    rv.NullableValue = null;
                                else
                                    rv.NullableValue = v;
                            }
                            else
                            {
                                rv.Value = rand.Next(min, max + 1);
                            }
                        }
                    }
                }
            }
            finally
            {
                ignorePlayingFlag--;
            }
            Control_ValueChanged(this, null);
        }

        private void metroButtonImport_Click(object sender, EventArgs e)
        {
            using (CommonOpenFileDialog openFileDialogTone = new CommonOpenFileDialog())
            {
                //TODO: openFileDialogTone.SelectionChanged += OpenFileDialogTone_SelectionChanged;
                openFileDialogTone.Filters.Clear();

                String allext = ExtensionsFilterExt;
                if (SupportedExtensionsCustomFilterExt != null)
                    allext += ";" + ExtensionsFilterExt;
                if (SupportedExtensionsExtFilterExt != null)
                    allext += ";" + SupportedExtensionsExtFilterExt;
                openFileDialogTone.Filters.Add(new CommonFileDialogFilter("All Supported Files", allext));

                openFileDialogTone.Filters.Add(new CommonFileDialogFilter(ExtensionsFilterLabel, ExtensionsFilterExt));

                if (SupportedExtensionsCustomFilterExt != null)
                    openFileDialogTone.Filters.Add(new CommonFileDialogFilter(SupportedExtensionsCustomFilterLabel, SupportedExtensionsCustomFilterExt));

                if (SupportedExtensionsExtFilterExt != null)
                    openFileDialogTone.Filters.Add(new CommonFileDialogFilter(SupportedExtensionsExtFilterLabel, SupportedExtensionsExtFilterExt));

                string dir = Settings.Default.ToneLibLastDir;
                if (string.IsNullOrWhiteSpace(dir))
                {
                    dir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    dir = System.IO.Path.Combine(dir, "MAmi");
                }
                openFileDialogTone.InitialDirectory = dir;

                var dr = openFileDialogTone.ShowDialog(this.Handle);
                if (dr != CommonFileDialogResult.Ok)
                    return;

                Settings.Default.ToneLibLastDir = System.IO.Path.GetDirectoryName(openFileDialogTone.FileName);
                string importFileName = openFileDialogTone.FileName;

                importAndApplyToneFile(importFileName, false);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void updateTimbreNames()
        {
            metroComboBoxTimbres.Items[TimbreNo] = ((TimbreItem)metroComboBoxTimbres.Items[TimbreNo]);

            for (int i = 0; i < Instrument.BaseTimbres.Length; i++)
            {
                metroComboBoxTimbres.Items[i] = new TimbreItem(Instrument.BaseTimbres[i], i);
                //if (i == TimbreNo)
                //    Timbre = Instrument.BaseTimbres[i];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public virtual IEnumerable<Tone> ImportToneFile(string file)
        {
            string ext = System.IO.Path.GetExtension(file);
            IEnumerable<Tone> tones = null;

            var exts = ExtensionsFilterExt.Split(new char[] { ';' });
            string mext = System.IO.Path.GetExtension(exts[0]).ToUpper(CultureInfo.InvariantCulture);
            if (ext.ToUpper(CultureInfo.InvariantCulture).Equals(mext))
            {
                try
                {
                    string txt = System.IO.File.ReadAllText(file);
                    StringReader rs = new StringReader(txt);

                    string ftname = rs.ReadLine().ToUpper(CultureInfo.InvariantCulture);
                    if (exts[0].ToUpper(CultureInfo.InvariantCulture).Equals(ftname))
                    {
                        string ver = rs.ReadLine();
                        if (ver != "1.0")
                            throw new InvalidDataException();
                        int num = int.Parse(rs.ReadLine());
                        List<string> lines = new List<string>();
                        List<Tone> ts = new List<Tone>();
                        int progNo = 0;
                        while (true)
                        {
                            string line = rs.ReadLine();
                            if (line == null || line == "-")
                            {
                                if (lines.Count == 0)
                                    break;
                                Tone t = new Tone();
                                t.MML = lines.ToArray();
                                t.Name = t.MML[0];
                                t.Number = progNo++;
                                ts.Add(t);
                                lines.Clear();
                                if (line == null)
                                    break;
                                continue;
                            }
                            lines.Add(line);
                        }
                        tones = ts;
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
            else
            {
                var Option = new Option();
                try
                {
                    string[] importFile = { file.ToLower(CultureInfo.InvariantCulture) };
                    switch (ext.ToUpper(CultureInfo.InvariantCulture))
                    {
                        case ".MUC":
                            tones = Muc.Reader(importFile, Option);
                            break;
                        case ".DAT":
                            tones = Dat.Reader(importFile, Option);
                            break;
                        case ".MWI":
                            tones = Fmp.Reader(importFile, Option);
                            break;
                        case ".MML":
                            tones = Pmd.Reader(importFile, Option);
                            break;
                        case ".FXB":
                            tones = Vopm.Reader(importFile, Option);
                            break;
                        case ".GWI":
                            tones = Gwi.Reader(importFile, Option);
                            break;
                        case ".BNK":
                            tones = BankReader.Read(file);
                            break;
                        case ".SYX":
                            tones = SyxReaderTX81Z.Read(file);
                            break;
                        case ".FF":
                            tones = FF.Reader(file, Option);
                            break;
                        case ".FFOPM":
                            tones = FF.Reader(file, Option);
                            break;
                        default:

                            break;
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
            return tones;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="setAll"></param>
        protected virtual void importAndApplyToneFile(string file, bool setAll)
        {
            string[] importFile = { file.ToLower(CultureInfo.InvariantCulture) };
            try
            {
                IEnumerable<Tone> tones = ImportToneFile(file);

                if (tones != null && tones.Count() > 0)
                {
                    if (tones.Count() == 1)
                    {
                        try
                        {
                            ignorePlayingFlag++;
                            ApplyTone(tones.ToArray()[0]);
                        }
                        finally
                        {
                            ignorePlayingFlag--;
                        }
                        ((IFmTimbre)Timbre).PatchInfo = file;
                        metroTextBoxPatchFile.Text = file;
                        Control_ValueChanged(this, null);
                    }
                    else
                    {
                        List<string> ses = new List<string>();

                        foreach (var c in controls.Values)
                            ses.Add(c.SerializeData);

                        if (!setAll)
                        {
                            using (var f = new FormToneSelector(tones))
                            {
                                f.SelectedToneChanged += (s, e) => { tryApplyTone(f.SelectedTone); };
                                DialogResult dr = f.ShowDialog(this);
                                try
                                {
                                    ignorePlayingFlag++;
                                    if (dr == DialogResult.OK)
                                    {
                                        ApplyTone(f.SelectedTone);
                                    }
                                    else
                                    {
                                        //restore
                                        int idx = 0;
                                        foreach (var c in controls.Values)
                                            c.SerializeData = ses[idx++];
                                    }
                                }
                                finally
                                {
                                    ignorePlayingFlag--;
                                }
                            }
                        }
                        else
                        {
                            tryApplyTones(tones);
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

                MessageBox.Show(Resources.FailedLoadFile + "\r\n" + ex.Message);
            }
            finally
            {
                updateTimbreNames();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tone"></param>
        protected virtual void ApplyTone(Tone tone)
        {
            //nothing
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tone"></param>
        protected virtual void ApplyTone(TimbreBase timbre, Tone tone)
        {
            //nothing
        }


        private void tryApplyTones(IEnumerable<Tone> tones)
        {
            if (tones != null && tones.Count() != 0)
            {
                try
                {
                    ignorePlayingFlag++;
                    try
                    {
                        ignoreMetroComboBoxTimbres_SelectedIndexChanged = true;

                        for (int i = 0; i < tones.Count(); i++)
                        {
                            if (i >= metroComboBoxTimbres.Items.Count)
                                break;
                            TimbreItem ti = (TimbreItem)metroComboBoxTimbres.Items[i];
                            this.Timbre = ti.Timbre;
                            this.TimbreNo = ti.Number;
                            ApplyTone(ti.Timbre, tones.ElementAt(i));
                            metroComboBoxTimbres.Items[i] = new TimbreItem(ti.Timbre, i);
                        }

                        metroComboBoxTimbres.SelectedIndex = 0;
                    }
                    finally
                    {
                        ignoreMetroComboBoxTimbres_SelectedIndexChanged = false;
                    }
                    metroComboBoxTimbres_SelectedIndexChanged(null, null);
                    metroComboBoxTimbres.Invalidate();
                }
                finally
                {
                    ignorePlayingFlag--;
                }
            }
        }

        private async void tryApplyTone(Tone tone)
        {
            if (tone != null)
            {
                try
                {
                    ignorePlayingFlag++;
                    ApplyTone(tone);
                    Control_ValueChanged(this, null);
                }
                finally
                {
                    ignorePlayingFlag--;
                }
                if (ignorePlayingFlag == 0 && Visible)
                {
                    await testPlay();
                }
            }
        }


        private void metroButtonImport_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] drags = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (drags.Length == 1)
                {
                    var fn = drags[0];
                    var ext = System.IO.Path.GetExtension(fn);
                    if (System.IO.File.Exists(fn) && IsSupportedToneExtension(ext))
                    {
                        this.BeginInvoke(new MethodInvoker(() =>
                        {
                            if (!this.IsDisposed)
                                importAndApplyToneFile(drags[0], false);
                        }));
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ext"></param>
        /// <returns></returns>
        public virtual bool IsSupportedToneExtension(String ext)
        {
            var exts = ExtensionsFilterExt.Split(new char[] { ';' });
            foreach (string e in exts)
            {
                if (e.Replace("*", "").Equals(ext, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            if (SupportedExtensionsExtFilterExt != null)
            {
                exts = SupportedExtensionsExtFilterExt.Split(new char[] { ';' });
                foreach (string e in exts)
                {
                    if (e.Replace("*", "").Equals(ext, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }

            if (SupportedExtensionsCustomFilterExt != null)
            {
                exts = SupportedExtensionsCustomFilterExt.Split(new char[] { ';' });
                foreach (string e in exts)
                {
                    if (e.Replace("*", "").Equals(ext, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }

            return false;
        }

        private void metroButtonImport_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] drags = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (drags.Length == 1)
                {
                    var fn = drags[0];
                    var ext = System.IO.Path.GetExtension(fn);
                    if (System.IO.File.Exists(fn) && IsSupportedToneExtension(ext))
                        e.Effect = DragDropEffects.All;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tone"></param>
        protected virtual void ApplyTimbre(TimbreBase timbre)
        {
            //nothing
        }

        private void metroButtonImportAll_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] drags = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (drags.Length == 1)
                {
                    var fn = drags[0];
                    var ext = System.IO.Path.GetExtension(fn);
                    if (System.IO.File.Exists(fn) && IsSupportedToneExtension(ext))
                    {
                        this.BeginInvoke(new MethodInvoker(() =>
                        {
                            if (!this.IsDisposed)
                                importAndApplyToneFile(drags[0], true);
                        }));
                    }
                }
            }
        }

        private void metroButtonImportAll_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] drags = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (drags.Length == 1)
                {
                    var fn = drags[0];
                    var ext = System.IO.Path.GetExtension(fn);
                    if (System.IO.File.Exists(fn) && IsSupportedToneExtension(ext))
                        e.Effect = DragDropEffects.All;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual string ExtensionsFilterLabel
        {
            get
            {
                return "Tone file(MAmi FM)";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual string ExtensionsFilterExt
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual string SupportedExtensionsExtFilterLabel
        {
            get
            {
                return "Tone file(MUCOM88, FMP, PMD, VOPM, GWI, FITOM, SYX, FF, FFOPM)";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual string SupportedExtensionsExtFilterExt
        {
            get
            {
                return Tone.SupportedExts;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual string SupportedExtensionsCustomFilterLabel
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual string SupportedExtensionsCustomFilterExt
        {
            get
            {
                return null;
            }
        }

        private void metroButtonImportAll_Click(object sender, EventArgs e)
        {
            using (CommonOpenFileDialog openFileDialogTone = new CommonOpenFileDialog())
            {
                //TODO: openFileDialogTone.SelectionChanged += OpenFileDialogTone_SelectionChanged;
                openFileDialogTone.Filters.Clear();

                String allext = ExtensionsFilterExt;
                if (SupportedExtensionsCustomFilterExt != null)
                    allext += ";" + ExtensionsFilterExt;
                if (SupportedExtensionsExtFilterExt != null)
                    allext += ";" + SupportedExtensionsExtFilterExt;
                openFileDialogTone.Filters.Add(new CommonFileDialogFilter("All Supported Files", allext));

                if (ExtensionsFilterExt != null)
                    openFileDialogTone.Filters.Add(new CommonFileDialogFilter(ExtensionsFilterLabel, ExtensionsFilterExt));
                if (SupportedExtensionsCustomFilterExt != null)
                    openFileDialogTone.Filters.Add(new CommonFileDialogFilter(SupportedExtensionsCustomFilterLabel, SupportedExtensionsCustomFilterExt));
                if (SupportedExtensionsExtFilterExt != null)
                    openFileDialogTone.Filters.Add(new CommonFileDialogFilter(SupportedExtensionsExtFilterLabel, SupportedExtensionsExtFilterExt));
                string dir = Settings.Default.ToneLibLastDir;
                if (string.IsNullOrWhiteSpace(dir))
                {
                    dir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    dir = System.IO.Path.Combine(dir, "MAmi");
                }
                openFileDialogTone.InitialDirectory = dir;

                var dr = openFileDialogTone.ShowDialog(this.Handle);
                if (dr != CommonFileDialogResult.Ok)
                    return;

                Settings.Default.ToneLibLastDir = System.IO.Path.GetDirectoryName(openFileDialogTone.FileName);

                importAndApplyToneFile(openFileDialogTone.FileName, true);
            }
        }

        private void metroButtonImportGit_Click(object sender, EventArgs ea)
        {
            //backup
            List<string> ses = new List<string>();
            foreach (var c in controls.Values)
                ses.Add(c.SerializeData);

            using (var f = new FormDownloadTone(this, false))
            {
                f.SelectedToneChanged += (s, e) => { tryApplyTone(f.SelectedTone); };

                var dr = f.ShowDialog(this);
                try
                {
                    ignorePlayingFlag++;
                    if (dr == DialogResult.OK && f.SelectedTone != null)
                    {
                        ApplyTone(f.SelectedTone);
                    }
                    else
                    {
                        //restore
                        int idx = 0;
                        foreach (var c in controls.Values)
                            c.SerializeData = ses[idx++];
                    }
                }
                finally
                {
                    ignorePlayingFlag--;
                }
            }
        }

        private void metroButtonImportAllGit_Click(object sender, EventArgs ea)
        {
            //backup
            List<string> ses = new List<string>();
            foreach (var c in controls.Values)
                ses.Add(c.SerializeData);

            using (var f = new FormDownloadTone(this, true))
            {
                f.SelectedToneChanged += (s, e) => { tryApplyTone(f.SelectedTone); };

                var dr = f.ShowDialog(this);
                try
                {
                    ignorePlayingFlag++;
                    if (dr == DialogResult.OK)
                    {
                        var tones = f.SelectedTones;
                        tryApplyTones(tones);
                    }
                    else
                    {
                        //restore
                        int idx = 0;
                        foreach (var c in controls.Values)
                            c.SerializeData = ses[idx++];
                    }
                }
                finally
                {
                    ignorePlayingFlag--;
                }
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            InstrumentManager.Panic();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void metroButtonAbort_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Abort;
            Close();
        }

        public event EventHandler CopyRequested;

        private void metroButtonCopy_Click(object sender, EventArgs e)
        {
            CopyRequested?.Invoke(sender, e);
        }

        public event EventHandler PasteRequested;

        private void metroButtonPaste_Click(object sender, EventArgs e)
        {
            PasteRequested?.Invoke(sender, e);
        }

        /// <summary>
        /// Export
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void metroButtonExport_Click(object sender, EventArgs e)
        {
            using (CommonSaveFileDialog saveFileDialog = new CommonSaveFileDialog())
            {
                string dir = Settings.Default.ToneLibLastDir;
                if (string.IsNullOrWhiteSpace(dir))
                {
                    dir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    dir = System.IO.Path.Combine(dir, "MAmi");
                }
                saveFileDialog.InitialDirectory = dir;

                var exts = ExtensionsFilterExt.Split(new char[] { ';' });
                saveFileDialog.DefaultExtension = exts[0].Replace("*", "");
                saveFileDialog.Filters.Add(new CommonFileDialogFilter(ExtensionsFilterLabel, exts[0]));

                if (!string.IsNullOrWhiteSpace(((IFmTimbre)Timbre).PatchInfo))
                    saveFileDialog.DefaultFileName = Utility.MakeUniqueFileName(dir,
                        System.IO.Path.ChangeExtension(((IFmTimbre)Timbre).PatchInfo, saveFileDialog.DefaultExtension));
                else if (string.IsNullOrWhiteSpace(Timbre.TimbreName))
                    saveFileDialog.DefaultFileName = Utility.MakeUniqueFileName(dir, $"Timbre[{TimbreNo}]" + saveFileDialog.DefaultExtension);
                else
                    saveFileDialog.DefaultFileName = Utility.MakeUniqueFileName(dir, Timbre.TimbreName + saveFileDialog.DefaultExtension);

                var dr = saveFileDialog.ShowDialog(this.Handle);
                if (dr == CommonFileDialogResult.Ok)
                {
                    StringBuilder sb = new StringBuilder();
                    string fullTypeName = exts[0];

                    sb.AppendLine(fullTypeName);
                    sb.AppendLine("1.0");
                    sb.AppendLine("1");

                    string[] vals = GetMMlValues();
                    foreach (string val in vals)
                        sb.AppendLine(val);

                    System.IO.File.WriteAllText(saveFileDialog.FileName, sb.ToString());

                    ((IFmTimbre)Timbre).PatchInfo = saveFileDialog.FileName;
                    metroTextBoxPatchFile.Text = saveFileDialog.FileName;

                    Settings.Default.ToneLibLastDir = System.IO.Path.GetDirectoryName(saveFileDialog.FileName);
                }
            }
        }

        private void metroButtonExportAll_Click(object sender, EventArgs e)
        {
            using (CommonSaveFileDialog saveFileDialog = new CommonSaveFileDialog())
            {
                saveFileDialog.InitialDirectory = Program.GetToneLibLastDir();

                saveFileDialog.DefaultExtension = ExtensionsFilterExt.Replace("*", "");
                saveFileDialog.Filters.Add(new CommonFileDialogFilter(ExtensionsFilterLabel, ExtensionsFilterExt));

                saveFileDialog.DefaultFileName = Utility.MakeUniqueFileName(saveFileDialog.InitialDirectory, $"Instrument_{Instrument.Name}" + saveFileDialog.DefaultExtension);

                var dr = saveFileDialog.ShowDialog(this.Handle);
                if (dr == CommonFileDialogResult.Ok)
                {
                    StringBuilder sb = new StringBuilder();
                    string fullTypeName = ExtensionsFilterExt;

                    sb.AppendLine(fullTypeName);
                    sb.AppendLine("1.0");
                    sb.AppendLine(metroComboBoxTimbres.Items.Count.ToString());

                    try
                    {
                        ignorePlayingFlag++;
                        ignoreMetroComboBoxTimbres_SelectedIndexChanged = true;
                        int slectedIndex = metroComboBoxTimbres.SelectedIndex;

                        for (int i = 0; i < metroComboBoxTimbres.Items.Count; i++)
                        {
                            TimbreItem ti = (TimbreItem)metroComboBoxTimbres.Items[i];
                            this.Timbre = ti.Timbre;
                            this.TimbreNo = ti.Number;
                            ApplyTimbre(ti.Timbre);

                            string[] vals = GetMMlValues();
                            foreach (string val in vals)
                                sb.AppendLine(val);
                            sb.AppendLine("-");
                        }
                        metroComboBoxTimbres.SelectedIndex = slectedIndex;
                        ignoreMetroComboBoxTimbres_SelectedIndexChanged = false;
                    }
                    finally
                    {
                        ignorePlayingFlag--;
                    }
                    metroComboBoxTimbres.Invalidate();

                    System.IO.File.WriteAllText(saveFileDialog.FileName, sb.ToString());

                    Settings.Default.ToneLibLastDir = System.IO.Path.GetDirectoryName(saveFileDialog.FileName);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual string[] GetMMlValues()
        {
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenFileDialogTone_SelectionChanged(object sender, EventArgs e)
        {
            CommonOpenFileDialog cofd = (CommonOpenFileDialog)sender;

            IEnumerable<Tone> tones = ImportToneFile(cofd.FileName);

            MethodInfo mi = typeof(CommonOpenFileDialog).GetMethod("GetNativeFileDialog", BindingFlags.Instance | BindingFlags.NonPublic);
            if (tones != null && tones.Count() > 0)
            {
                if (tones.Count() == 1)
                {
                    try
                    {
                        ignorePlayingFlag++;
                        ApplyTone(tones.ToArray()[0]);
                    }
                    finally
                    {
                        ignorePlayingFlag--;
                    }
                    Control_ValueChanged(this, null);
                }
            }
        }

        private void metroButtonTimbre_Click(object sender, EventArgs e)
        {
            using (FormRename f = new FormRename())
            {
                f.InputText = Timbre.TimbreName;
                var result = f.ShowDialog();
                if (result == DialogResult.OK)
                {
                    Timbre.TimbreName = f.InputText;

                    metroComboBoxTimbres.Items[TimbreNo] = ((TimbreItem)metroComboBoxTimbres.Items[TimbreNo]);
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

        private static void selectTopItem(object s)
        {
            PropertyGrid propertyGrid = (PropertyGrid)s;
            if (!propertyGrid.IsHandleCreated)
                return;
            propertyGrid.BeginInvoke(new MethodInvoker(() =>
            {
                if (propertyGrid.IsDisposed)
                    return;

                // get selected item
                GridItem gi = propertyGrid.SelectedGridItem;
                if (gi != null)
                {
                    // get category for selected item
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
            }));
        }

        private void contextMenuStripProp_Click(object sender, EventArgs e)
        {
            propertyGrid.ResetSelectedProperty();
            propertyGrid.Refresh();
        }

        private void FormFmEditor_Activated(object sender, EventArgs e)
        {
            ignoreMetroComboBoxTimbres_SelectedIndexChanged = true;
            metroComboBoxTimbres.Items[TimbreNo] = ((TimbreItem)metroComboBoxTimbres.Items[TimbreNo]);
            ignoreMetroComboBoxTimbres_SelectedIndexChanged = false;
        }

        private void propertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            ApplyTimbre(timbre);
        }
    }
}
