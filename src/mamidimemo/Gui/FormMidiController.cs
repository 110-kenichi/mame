// copyright-holders:K.Ito
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
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
using zanac.MAmidiMEmo.ComponentModel;
using zanac.MAmidiMEmo.Midi;
using zanac.MAmidiMEmo.Properties;

namespace zanac.MAmidiMEmo.Gui
{
    public partial class FormMidiController : FormBase
    {
        private bool isLoaded = false;

        public FormMidiController()
        {
            InitializeComponent();

            ImageUtility.AdjustControlImagesDpiScale(this);

            global::zanac.MAmidiMEmo.Properties.Settings.Default.PropertyChanged +=
                (s, e) =>
                {
                    if (e.PropertyName.StartsWith("MidiCon"))
                    {
                        string propName = e.PropertyName.Substring("MidiCon".Length);
                        MetroTrackBar tb = tableLayoutPanel.Controls["metroTrackBar" + propName] as MetroTrackBar;
                        if (tb != null)
                        {
                            int val = (int)(decimal)global::zanac.MAmidiMEmo.Properties.Settings.Default[e.PropertyName];
                            if (tb.Value != val)
                                tb.Value = val;

                            MidiPort mp = MidiPort.PortA;
                            switch (comboBoxOutPort.SelectedIndex)
                            {
                                case 0: //PORT A
                                    mp = MidiPort.PortA;
                                    break;
                                case 1: //PORT B
                                    mp = MidiPort.PortB;
                                    break;
                                case 2: //PORT A,B
                                    mp = MidiPort.PortAB;
                                    break;
                            }

                            if (tb.Tag != null)
                            {
                                if (isLoaded)
                                {
                                    int cc = int.Parse((string)tb.Tag);

                                    foreach (MetroCheckBox chc in tableLayoutPanelCh.Controls)
                                    {
                                        if (chc.Checked)
                                        {
                                            FourBitNumber ch = (FourBitNumber)(int.Parse(chc.Name.Substring("metroCheckBoxCh".Length)) - 1);
                                            MidiManager.SendMidiEvent(mp, new ControlChangeEvent((SevenBitNumber)cc, (SevenBitNumber)val) { Channel = (FourBitNumber)ch });
                                        }
                                    }
                                }
                            }
                            else
                            {
                                switch (propName)
                                {
                                    case "Pitch":
                                        if(!isLoaded)
                                            break;
                                        foreach (MetroCheckBox chc in tableLayoutPanelCh.Controls)
                                        {
                                            if (chc.Checked)
                                            {
                                                FourBitNumber ch = (FourBitNumber)(int.Parse(chc.Name.Substring("metroCheckBoxCh".Length)) - 1);
                                                MidiManager.SendMidiEvent(mp, new PitchBendEvent((ushort)val) { Channel = ch });
                                            }
                                        }
                                        break;
                                    case "PitchRange":
                                        if (!isLoaded)
                                            break;
                                        foreach (MetroCheckBox chc in tableLayoutPanelCh.Controls)
                                        {
                                            if (chc.Checked)
                                            {
                                                FourBitNumber ch = (FourBitNumber)(int.Parse(chc.Name.Substring("metroCheckBoxCh".Length)) - 1);
                                                //RPN LSB
                                                MidiManager.SendMidiEvent(mp, new ControlChangeEvent((SevenBitNumber)100, (SevenBitNumber)0) { Channel = ch });
                                                //RPN MSB
                                                MidiManager.SendMidiEvent(mp, new ControlChangeEvent((SevenBitNumber)101, (SevenBitNumber)0) { Channel = ch });
                                                //DATA ENTRY MSB
                                                MidiManager.SendMidiEvent(mp, new ControlChangeEvent((SevenBitNumber)6, (SevenBitNumber)(val & 0x7f)) { Channel = ch });
                                            }
                                        }
                                        break;
                                    case "FineTune":
                                        if (!isLoaded)
                                            break;
                                        foreach (MetroCheckBox chc in tableLayoutPanelCh.Controls)
                                        {
                                            if (chc.Checked)
                                            {
                                                FourBitNumber ch = (FourBitNumber)(int.Parse(chc.Name.Substring("metroCheckBoxCh".Length)) - 1);
                                                //RPN LSB
                                                MidiManager.SendMidiEvent(mp, new ControlChangeEvent((SevenBitNumber)100, (SevenBitNumber)1) { Channel = ch });
                                                //RPN MSB
                                                MidiManager.SendMidiEvent(mp, new ControlChangeEvent((SevenBitNumber)101, (SevenBitNumber)0) { Channel = ch });
                                                //DATA ENTRY MSB
                                                MidiManager.SendMidiEvent(mp, new ControlChangeEvent((SevenBitNumber)6, (SevenBitNumber)(val & 0x7f)) { Channel = ch });
                                                //DATA ENTRY LSB
                                                MidiManager.SendMidiEvent(mp, new ControlChangeEvent((SevenBitNumber)38, (SevenBitNumber)(val >> 7)) { Channel = ch });
                                            }
                                        }
                                        break;
                                    case "ProgNo":
                                        if (!isLoaded)
                                            break;
                                        foreach (MetroCheckBox chc in tableLayoutPanelCh.Controls)
                                        {
                                            if (chc.Checked)
                                            {
                                                FourBitNumber ch = (FourBitNumber)(int.Parse(chc.Name.Substring("metroCheckBoxCh".Length)) - 1);
                                                MidiManager.SendMidiEvent(mp, new ProgramChangeEvent((SevenBitNumber)val) { Channel = ch });
                                            }
                                        }
                                        break;
                                    case "ModDepthRangeNote":
                                        if (!isLoaded)
                                            break;
                                        foreach (MetroCheckBox chc in tableLayoutPanelCh.Controls)
                                        {
                                            if (chc.Checked)
                                            {
                                                FourBitNumber ch = (FourBitNumber)(int.Parse(chc.Name.Substring("metroCheckBoxCh".Length)) - 1);
                                                //RPN LSB
                                                MidiManager.SendMidiEvent(mp, new ControlChangeEvent((SevenBitNumber)100, (SevenBitNumber)5) { Channel = ch });
                                                //RPN MSB
                                                MidiManager.SendMidiEvent(mp, new ControlChangeEvent((SevenBitNumber)101, (SevenBitNumber)0) { Channel = ch });
                                                //DATA ENTRY MSB
                                                MidiManager.SendMidiEvent(mp, new ControlChangeEvent((SevenBitNumber)6, (SevenBitNumber)(val & 0x7f)) { Channel = ch });
                                            }
                                        }
                                        break;
                                    case "ModDepthRangeCent":
                                        if (!isLoaded)
                                            break;
                                        foreach (MetroCheckBox chc in tableLayoutPanelCh.Controls)
                                        {
                                            if (chc.Checked)
                                            {
                                                FourBitNumber ch = (FourBitNumber)(int.Parse(chc.Name.Substring("metroCheckBoxCh".Length)) - 1);
                                                //RPN LSB
                                                MidiManager.SendMidiEvent(mp, new ControlChangeEvent((SevenBitNumber)100, (SevenBitNumber)5) { Channel = ch });
                                                //RPN MSB
                                                MidiManager.SendMidiEvent(mp, new ControlChangeEvent((SevenBitNumber)101, (SevenBitNumber)0) { Channel = ch });
                                                //DATA ENTRY LSB
                                                MidiManager.SendMidiEvent(mp, new ControlChangeEvent((SevenBitNumber)38, (SevenBitNumber)(val & 0x7f)) { Channel = ch });
                                            }
                                        }
                                        break;
                                }
                            }
                        }
                    }
                };

            foreach (Control ctrl in tableLayoutPanel.Controls)
            {
                if (ctrl is MetroTrackBar)
                {
                    string propName = "MidiCon" + ctrl.Name.Substring("metroTrackBar".Length);
                    decimal val = (decimal)global::zanac.MAmidiMEmo.Properties.Settings.Default[propName];
                    ((MetroTrackBar)ctrl).Value = (int)val;
                }
            }
            isLoaded = true;
        }

        private void metroTrackBar_ValueChanged(object sender, EventArgs e)
        {
            string propName = "MidiCon" + ((MetroTrackBar)sender).Name.Substring("metroTrackBar".Length);
            decimal val = (decimal)global::zanac.MAmidiMEmo.Properties.Settings.Default[propName];
            if (val != (decimal)((MetroTrackBar)sender).Value)
                global::zanac.MAmidiMEmo.Properties.Settings.Default[propName] = (decimal)((MetroTrackBar)sender).Value;
        }

        private void label3_MouseClick(object sender, MouseEventArgs e)
        {
            global::zanac.MAmidiMEmo.Properties.Settings.Default["MidiConPitch"] = (decimal)8192;
        }

        private void label4_MouseClick(object sender, MouseEventArgs e)
        {
            global::zanac.MAmidiMEmo.Properties.Settings.Default["MidiConPitchRange"] = (decimal)2;
        }

        private void label5_Click(object sender, EventArgs e)
        {
            global::zanac.MAmidiMEmo.Properties.Settings.Default["MidiConFineTune"] = (decimal)8192;
        }

        private void label6_Click(object sender, EventArgs e)
        {
            global::zanac.MAmidiMEmo.Properties.Settings.Default["MidiConProgNo"] = (decimal)0;
        }

        private void label7_Click(object sender, EventArgs e)
        {
            global::zanac.MAmidiMEmo.Properties.Settings.Default["MidiConVolume"] = (decimal)127;
        }

        private void label8_Click(object sender, EventArgs e)
        {
            global::zanac.MAmidiMEmo.Properties.Settings.Default["MidiConExpression"] = (decimal)127;
        }

        private void label9_Click(object sender, EventArgs e)
        {
            global::zanac.MAmidiMEmo.Properties.Settings.Default["MidiConPanpot"] = (decimal)64;
        }

        private void label10_Click(object sender, EventArgs e)
        {
            global::zanac.MAmidiMEmo.Properties.Settings.Default["MidiConModulation"] = (decimal)0;
        }

        private void label11_Click(object sender, EventArgs e)
        {
            global::zanac.MAmidiMEmo.Properties.Settings.Default["MidiConModRate"] = (decimal)64;
        }

        private void label12_Click(object sender, EventArgs e)
        {
            global::zanac.MAmidiMEmo.Properties.Settings.Default["MidiConModDepth"] = (decimal)64;
        }

        private void label13_Click(object sender, EventArgs e)
        {
            global::zanac.MAmidiMEmo.Properties.Settings.Default["MidiConModDelay"] = (decimal)64;
        }

        private void label14_Click(object sender, EventArgs e)
        {
            global::zanac.MAmidiMEmo.Properties.Settings.Default["MidiConModDepthRangeNote"] = (decimal)0;
        }

        private void label15_Click(object sender, EventArgs e)
        {
            global::zanac.MAmidiMEmo.Properties.Settings.Default["MidiConModDepthRangeCent"] = (decimal)64;
        }

        private void label16_Click(object sender, EventArgs e)
        {
            global::zanac.MAmidiMEmo.Properties.Settings.Default["MidiConHold"] = (decimal)0;
        }

        private void label17_Click(object sender, EventArgs e)
        {
            global::zanac.MAmidiMEmo.Properties.Settings.Default["MidiConPortament"] = (decimal)0;
        }

        private void label18_Click(object sender, EventArgs e)
        {
            global::zanac.MAmidiMEmo.Properties.Settings.Default["MidiConPortamentTime"] = (decimal)0;
        }

        private void label19_Click(object sender, EventArgs e)
        {
            global::zanac.MAmidiMEmo.Properties.Settings.Default["MidiConLegatoFootSw"] = (decimal)0;
        }

        private void label20_Click(object sender, EventArgs e)
        {
            global::zanac.MAmidiMEmo.Properties.Settings.Default["MidiConMonoMode"] = (decimal)0;
        }

        private void label21_Click(object sender, EventArgs e)
        {
            global::zanac.MAmidiMEmo.Properties.Settings.Default["MidiConPolyMode"] = (decimal)0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
