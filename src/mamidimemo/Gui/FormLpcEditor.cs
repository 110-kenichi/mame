using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using MetroFramework.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using zanac.MAmidiMEmo.Instruments.Chips;
using zanac.MAmidiMEmo.Instruments.Chips.TMS5220_Data;
using zanac.MAmidiMEmo.Midi;
using zanac.MAmidiMEmo.Properties;
using static zanac.MAmidiMEmo.Instruments.Chips.TMS5220;

namespace zanac.MAmidiMEmo.Gui
{
    public partial class FormLpcEditor : FormBase
    {
        private TMS5220Timbre timbre;
        private int timbreNo;
        private TMS5220 inst;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler ValueChanged;

        private byte[] lpcData;

        /// <summary>
        /// 
        /// </summary>
        public byte[] LpcData
        {
            get
            {
                return lpcData;
            }
            set
            {
                lpcData = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public FormLpcEditor()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        public FormLpcEditor(TMS5220 inst, TMS5220Timbre timbre, bool singleSelect)
        {
            this.inst = inst;
            this.timbre = timbre;
            for (int i = 0; i < inst.BaseTimbres.Length; i++)
            {
                if (inst.BaseTimbres[i] == timbre)
                    timbreNo = i;
            }

            InitializeComponent();

            Size = Settings.Default.LpcEdSize;

            setPreset(typeof(Vocab_AstroBlaster));
            setPreset(typeof(Vocab_Soundbites));
            setPreset(typeof(Vocab_Special));
            setPreset(typeof(Vocab_US_Acorn));
            setPreset(typeof(Vocab_US_Clock));
            setPreset(typeof(Vocab_US_Large));
            setPreset(typeof(Vocab_US_TI99));
        }

        private void setPreset(Type t)
        {
            var fis = t.GetFields();
            foreach (var fi in fis)
            {
                listBox1.Items.
                    Add(new PresetVoice(fi.DeclaringType.Name + ":" + fi.Name, (byte[])fi.GetValue(null)));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            Settings.Default.LpcEdSize = Size;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            PresetVoice pv = (PresetVoice)listBox1.SelectedItem;
            LpcData = pv.LpcData;

            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private TaggedNoteOnEvent noteOnEvent;

        private void listBox1_MouseDown(object sender, MouseEventArgs e)
        {
            int idx = listBox1.IndexFromPoint(e.Location);
            PresetVoice pv = (PresetVoice)listBox1.Items[idx];
            LpcData = pv.LpcData;

            ValueChanged?.Invoke(this, EventArgs.Empty);

            noteOnEvent = new TaggedNoteOnEvent((SevenBitNumber)69, (SevenBitNumber)127);
            noteOnEvent.Tag = new NoteOnTimbreInfo(timbre, timbreNo);
            inst.NotifyMidiEvent(noteOnEvent);
        }

        private void listBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (noteOnEvent != null)
            {
                NoteOffEvent noe = new NoteOffEvent(noteOnEvent.NoteNumber, noteOnEvent.Velocity);
                inst.NotifyMidiEvent(noe);

                noteOnEvent = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private class PresetVoice
        {
            public string Name
            {
                get;
                private set;
            }

            public byte[] LpcData
            {
                get;
                private set;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="name"></param>
            /// <param name="lpcData"></param>
            public PresetVoice(string name, byte[] lpcData)
            {
                Name = name;
                LpcData = lpcData;
            }

            public override string ToString()
            {
                return Name;
            }
        }

    }
}
