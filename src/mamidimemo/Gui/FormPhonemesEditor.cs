using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using MetroFramework.Controls;
using MetroFramework.Forms;
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
using zanac.MAmidiMEmo.Instruments.Chips;
using zanac.MAmidiMEmo.Midi;
using zanac.MAmidiMEmo.Properties;
using static zanac.MAmidiMEmo.Instruments.Chips.SAM;

namespace zanac.MAmidiMEmo.Gui
{
    public partial class FormPhonemesEditor : FormBase
    {
        private SAMTimbre timbre;
        private int timbreNo;
        private SAM inst;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler ValueChanged;

        /// <summary>
        /// 
        /// </summary>
        public string Allophones
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public FormPhonemesEditor()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        public FormPhonemesEditor(SAM inst, SAMTimbre timbre, bool singleSelect)
        {
            this.inst = inst;
            this.timbre = timbre;
            for (int i = 0; i < inst.BaseTimbres.Length; i++)
            {
                if (inst.BaseTimbres[i] == timbre)
                    timbreNo = i;
            }

            InitializeComponent();

            metroTextBoxAllophones.Text = timbre.Phonemes;

            Size = Settings.Default.PhonemesEdSize;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            Settings.Default.PhonemesEdSize = Size;
        }

        private TaggedNoteOnEvent noteOnEvent;



        private void metroButtonPlay_MouseDown(object sender, MouseEventArgs e)
        {
            Allophones = metroTextBoxAllophones.Text;
            ValueChanged?.Invoke(this, EventArgs.Empty);

            noteOnEvent = new TaggedNoteOnEvent((SevenBitNumber)69, (SevenBitNumber)127);
            noteOnEvent.Tag = new NoteOnTimbreInfo(timbre, timbreNo);
            inst.NotifyMidiEvent(noteOnEvent);
        }

        private void metroButtonPlay_MouseUp(object sender, MouseEventArgs e)
        {
            if (noteOnEvent != null)
            {
                NoteOffEvent noe = new NoteOffEvent(noteOnEvent.NoteNumber, noteOnEvent.Velocity);
                inst.NotifyMidiEvent(noe);

                noteOnEvent = null;
            }
        }

        private void metroButton2_Click(object sender, EventArgs e)
        {
            var mb = (MetroButton)sender;

            var insertText = mb.Text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)[0];

            var selectionIndex = metroTextBoxAllophones.SelectionStart;
            metroTextBoxAllophones.Text = metroTextBoxAllophones.Text.Insert(selectionIndex, insertText).Trim();
            metroTextBoxAllophones.SelectionStart = selectionIndex + insertText.Length;
        }

        private void metroButton2_MouseDown(object sender, MouseEventArgs e)
        {
            var mb = (MetroButton)sender;

            string allophone = mb.Text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)[0];

            noteOnEvent = new TaggedNoteOnEvent((SevenBitNumber)69, (SevenBitNumber)127);
            noteOnEvent.Tag = new NoteOnTimbreInfo(timbre, timbreNo) { Tag = allophone };
            inst.NotifyMidiEvent(noteOnEvent);
        }

        private void metroButton2_MouseUp(object sender, MouseEventArgs e)
        {
            if (noteOnEvent != null)
            {
                NoteOffEvent noe = new NoteOffEvent(noteOnEvent.NoteNumber, noteOnEvent.Velocity);
                inst.NotifyMidiEvent(noe);

                noteOnEvent = null;
            }
        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/110-kenichi/mame/blob/master/docs/MAmidiMEmo/Chips/sam.pdf");
        }
    }
}
