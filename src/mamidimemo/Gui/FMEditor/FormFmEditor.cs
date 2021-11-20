// copyright-holders:K.Ito
using FM_SoundConvertor;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using MetroFramework.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using zanac.MAmidiMEmo.Instruments;
using zanac.MAmidiMEmo.Midi;
using zanac.MAmidiMEmo.Properties;
using zanac.MAmidiMEmo.Util.FITOM;
using zanac.MAmidiMEmo.Util.Syx;

namespace zanac.MAmidiMEmo.Gui.FMEditor
{
    public partial class FormFmEditor : FormBase
    {
        private Dictionary<String, RegisterContainerBase> controls = new Dictionary<string, RegisterContainerBase>();

        private bool singleSelect;

        public InstrumentBase Instrument
        {
            get;
            private set;
        }

        public TimbreBase Timbre
        {
            get;
            private set;
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

            this.singleSelect = singleSelect;

            for (int nn = 0; nn < 128; nn++)
            {
                toolStripComboBoxNote.Items.Add(MidiManager.GetNoteName((SevenBitNumber)nn) + "(" + nn + ")");
                toolStripComboBoxVelo.Items.Add(nn);
            }

            toolStripComboBoxNote.SelectedIndex = 60;
            toolStripComboBoxVelo.SelectedIndex = 127;
            toolStripComboBoxGate.SelectedIndex = 0;
            toolStripComboBoxCh.SelectedIndex = 0;
            toolStripComboBoxCC.SelectedIndex = 0;

            Settings.Default.SettingsLoaded += Default_SettingsLoaded;
            toolStripButtonPlay.Checked = Settings.Default.FmPlayOnEdit;
            toolStripButtonHook.Checked = Settings.Default.FmHook;
            toolStripComboBoxVelo.SelectedIndex = Settings.Default.FmVelocity;
            toolStripComboBoxGate.SelectedIndex = Settings.Default.FmGateTime;
            toolStripComboBoxNote.SelectedIndex = Settings.Default.FmNote;

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
            }

            setTitle();

            InstrumentManager.InstrumentChanged += InstrumentManager_InstrumentChanged;
            InstrumentManager.InstrumentRemoved += InstrumentManager_InstrumentRemoved;

            pianoControl1.NoteOn += PianoControl1_NoteOn;
            pianoControl1.NoteOff += PianoControl1_NoteOff;
            pianoControl1.EntryDataChanged += PianoControl1_EntryDataChanged;

            Midi.MidiManager.MidiEventHooked += MidiManager_MidiEventHooked;
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
            base.OnClosing(e);
        }

        private void Default_SettingsLoaded(object sender, System.Configuration.SettingsLoadedEventArgs e)
        {
            toolStripButtonPlay.Checked = Settings.Default.FmPlayOnEdit;
            toolStripButtonHook.Checked = Settings.Default.FmHook;
            toolStripComboBoxVelo.SelectedIndex = Settings.Default.FmVelocity;
            toolStripComboBoxGate.SelectedIndex = Settings.Default.FmGateTime;
            toolStripComboBoxNote.SelectedIndex = Settings.Default.FmNote;
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

                            if (rv.IsNullable)
                            {
                                var cv = rv.NullableValue;
                                if (cv.HasValue)
                                    cv += v;
                                else
                                    cv = v;
                                if (cv < rv.Minimum)
                                    cv = null;
                                else if (cv > rv.Maximum)
                                    cv = rv.Maximum;

                                rv.NullableValue = cv;
                            }
                            else
                            {
                                var cv = rv.Value;

                                cv += v;
                                if (cv < rv.Minimum)
                                    cv = rv.Minimum;
                                else if (cv > rv.Maximum)
                                    cv = rv.Maximum;

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

                            if (rv.IsNullable)
                            {
                                var v = rand.Next(rv.Minimum - 1, rv.Maximum + 1);
                                if (v < rv.Minimum)
                                    rv.NullableValue = null;
                                else
                                    rv.NullableValue = v;
                            }
                            else
                            {
                                rv.Value = rand.Next(rv.Minimum, rv.Maximum + 1);
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
            openFileDialogTone.Filter = SupportedExtensionsFilter;
            DialogResult dr = openFileDialogTone.ShowDialog(this);
            if (dr != DialogResult.OK)
                return;

            ImportAndApplyToneFile(openFileDialogTone.FileName, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public virtual IEnumerable<Tone> ImportToneFile(string file)
        {
            string ext = System.IO.Path.GetExtension(file);
            var Option = new Option();
            IEnumerable<Tone> tones = null;
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
            return tones;
        }

        protected virtual void ImportAndApplyToneFile(string file, bool setAll)
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

        private void tryApplyTone(Tone tone)
        {
            if (tone != null)
            {
                try
                {
                    ignorePlayingFlag++;
                    ApplyTone(tone);
                }
                finally
                {
                    ignorePlayingFlag--;
                }
                Control_ValueChanged(this, null);
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
                    if (System.IO.File.Exists(fn) && IsSupportedExtension(ext))
                    {
                        this.BeginInvoke(new MethodInvoker(() =>
                        {
                            if (!this.IsDisposed)
                                ImportAndApplyToneFile(drags[0], false);
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
        public virtual bool IsSupportedExtension(String ext)
        {
            if (ext.Equals(".MUC", StringComparison.OrdinalIgnoreCase) ||
                ext.Equals(".DAT", StringComparison.OrdinalIgnoreCase) ||
                ext.Equals(".MWI", StringComparison.OrdinalIgnoreCase) ||
                ext.Equals(".MML", StringComparison.OrdinalIgnoreCase) ||
                ext.Equals(".FXB", StringComparison.OrdinalIgnoreCase) ||
                ext.Equals(".GWI", StringComparison.OrdinalIgnoreCase)
               )
                return true;
            else
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
                    if (System.IO.File.Exists(fn) && IsSupportedExtension(ext))
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
                    if (System.IO.File.Exists(fn) && IsSupportedExtension(ext))
                    {
                        this.BeginInvoke(new MethodInvoker(() =>
                        {
                            if (!this.IsDisposed)
                                ImportAndApplyToneFile(drags[0], true);
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
                    if (System.IO.File.Exists(fn) && IsSupportedExtension(ext))
                        e.Effect = DragDropEffects.All;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual string SupportedExtensionsFilter
        {
            get
            {
                return openFileDialogTone.Filter;
            }
        }

        private void metroButtonImportAll_Click(object sender, EventArgs e)
        {
            openFileDialogTone.Filter = SupportedExtensionsFilter;
            DialogResult dr = openFileDialogTone.ShowDialog(this);
            if (dr != DialogResult.OK)
                return;

            ImportAndApplyToneFile(openFileDialogTone.FileName, true);
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
    }
}
