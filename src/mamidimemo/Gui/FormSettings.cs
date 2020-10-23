using MetroFramework.Controls;
using MetroFramework.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using zanac.MAmidiMEmo.Properties;

namespace zanac.MAmidiMEmo.Gui
{
    public partial class FormSettings : FormBase
    {
        public FormSettings()
        {
            InitializeComponent();
        }


        private static string[] SoundTypes = new string[] { "auto", "dsound", "xaudio2", "portaudio" };

        private static string[] AudioLatency = new string[] { "1", "2", "3", "4" };

        private void buttonOk_Click(object sender, EventArgs e)
        {
            try
            {
                using (var t = new StreamWriter(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "mame.ini"), false, new UTF8Encoding(true)))
                {
                    t.WriteLine("sound " + SoundTypes[comboBoxSoundType.SelectedIndex]);
                    t.WriteLine("samplerate " + comboBoxSampleRate.Text);
                    t.WriteLine("audio_latency " + AudioLatency[comboBoxAudioLatency.SelectedIndex]);
                    t.WriteLine("volume 0");
                    t.WriteLine("pa_api " + textBoxPaApi.Text);
                    t.WriteLine("pa_device " + textBoxPaDevice.Text);
                    t.WriteLine("pa_latency " + textBoxPaLatency.Text);
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(Exception))
                    throw;
                else if (ex.GetType() == typeof(SystemException))
                    throw;

                MessageBox.Show(ex.ToString());
                return;
            }
            DialogResult = DialogResult.OK;
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int MessageBeep(uint n);

        private void comboBoxText_Validating(object sender, CancelEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(((ComboBox)sender).Text))
            {
                MessageBeep(0);
                e.Cancel = true;
            }
        }

        private void textBox_Validating(object sender, CancelEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(((MetroTextBox)sender).Text))
            {
                MessageBeep(0);
                e.Cancel = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var r = folderBrowserDialog1.ShowDialog(this);
            if(r == DialogResult.OK)
                textBox1.Text = folderBrowserDialog1.SelectedPath;
        }

        private void comboBoxSampleRate_DrawItem(object sender, DrawItemEventArgs e)
        {
            //背景を描画する
            //項目が選択されている時は強調表示される
            e.DrawBackground();

            ComboBox cmb = (ComboBox)sender;
            //項目に表示する文字列
            string txt = e.Index > -1 ?
                cmb.Items[e.Index].ToString() : cmb.Text;
            //使用するブラシ
            Brush b = new SolidBrush(e.ForeColor);
            //文字列を描画する
            float ym =
                (e.Bounds.Height - e.Graphics.MeasureString(
                    txt, cmb.Font).Height) / 2;
            e.Graphics.DrawString(
                txt, cmb.Font, b, e.Bounds.X, e.Bounds.Y + ym);
            b.Dispose();

            //フォーカスを示す四角形を描画
            e.DrawFocusRectangle();
        }
    }
}
