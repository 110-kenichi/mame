// copyright-holders:K.Ito
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
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
    public partial class FormMidiTest : FormBase
    {
        private OutputDevice outputDevice;

        public FormMidiTest()
        {
            InitializeComponent();

            ImageUtility.AdjustControlImagesDpiScale(this);

            //Set MIDI I/F
            foreach (var dev in InputDevice.GetAll())
            {
                int idx = comboBoxOutPort.Items.Add(dev.Name);
                if (string.Equals(dev.Name, Settings.Default.OutMidiIF))
                    comboBoxOutPort.SelectedIndex = idx;

                dev.Dispose();
            }

            MidiManager.MidiEventHooked += MidiManager_MidiEventHooked;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Settings.Default.OutMidiIF = comboBoxOutPort.Text;
            base.OnClosing(e);
        }

        private void MidiManager_MidiEventHooked(object sender, CancelMidiEventReceivedEventArgs e)
        {
            if (this.IsDisposed)
                return;

            TimingClockEvent tce = e.Event.Event as TimingClockEvent;
            if (tce != null)
            {
                DateTime now = DateTime.Now;
                var delta = now.Ticks - lastSentTime;
                labelReceiveTime.BeginInvoke(new MethodInvoker(() =>
                {
                    if (this.IsDisposed)
                        return;

                    labelReceiveTime.Text = now.ToString("mm:ss:ffffff");
                    labelSpan.Text = "( +" + TimeSpan.FromTicks(delta).TotalMilliseconds + " ms)";

                }));
            }
        }

        private long lastSentTime;

        private void comboBoxOutPort_DropDown(object sender, EventArgs e)
        {
            comboBoxOutPort.Items.Clear();
            try
            {
                int si = -1;
                foreach (var dev in MidiManager.GetInputMidiDevices())
                {
                    int i = comboBoxOutPort.Items.Add(dev.Name);
                    if (dev.Name.Equals(Settings.Default.MidiIF))
                        si = i;
                    dev.Dispose();
                }
                if (si >= 0)
                    comboBoxOutPort.SelectedIndex = si;

                using (System.Drawing.Graphics graphics = CreateGraphics())
                {
                    int maxWidth = 0;
                    foreach (object obj in comboBoxOutPort.Items)
                    {
                        System.Drawing.SizeF area = graphics.MeasureString(obj.ToString(), comboBoxOutPort.Font);
                        maxWidth = Math.Max((int)area.Width, maxWidth);
                    }
                    comboBoxOutPort.DropDownWidth = maxWidth;
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

        private object playing;

        private async void metroButtonGo_MouseDown(object sender, MouseEventArgs e)
        {
            if (playing != null)
            {
                outputDevice.SendEvent(new NoteOffEvent((SevenBitNumber)69, (SevenBitNumber)0) { Channel = (FourBitNumber)0 });
                playing = null;
            }

            playing = new object();
            object _playing = playing;


            try
            {
                if (outputDevice != null)
                    outputDevice.Dispose();
                outputDevice = OutputDevice.GetByName(comboBoxOutPort.Text);

                var me = new TimingClockEvent();
                lastSentTime = DateTime.Now.Ticks;
                outputDevice.SendEvent(me);
                labelSendTime.Text = DateTime.FromBinary(lastSentTime).ToString("mm:ss:ffffff");
                labelReceiveTime.Text = "-";
                labelSpan.Text = "-";

                outputDevice.SendEvent(new NoteOnEvent((SevenBitNumber)69, (SevenBitNumber)127) { Channel = (FourBitNumber)0 });
                await Task.Delay(500);
                if (playing == _playing)
                {
                    outputDevice.SendEvent(new NoteOffEvent((SevenBitNumber)69, (SevenBitNumber)0) { Channel = (FourBitNumber)0 });
                    playing = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
