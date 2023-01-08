using FTD2XX_NET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BitBangTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            byte[] data = new byte[64];
            for (int i = 0; i < 64; i += 2)
            {
                data[i + 0] = 0b01010101;
                data[i + 1] = 0b10101010;
            }
            var ftdi = new FTD2XX_NET.FTDI();
            var stat = ftdi.OpenByIndex((uint)numericUpDownPort.Value);
            if (stat == FTDI.FT_STATUS.FT_OK)
            {
                ftdi.SetBitMode(0x00, FTDI.FT_BIT_MODES.FT_BIT_MODE_RESET);
                ftdi.SetBitMode(0xff, FTDI.FT_BIT_MODES.FT_BIT_MODE_ASYNC_BITBANG);
                ftdi.SetBaudRate((uint)numericUpDownBps.Value);
                ftdi.SetTimeouts(500, 500);
                ftdi.SetLatency(0);
                for (int i = 0; i < numericUpDownNum.Value; i++)
                {
                    for (int j = 0; j < 64; j++)
                    {
                        uint dummy = 0;
                        ftdi.Write(data, 64, ref dummy);
                    }
                    i += 64;
                }
                MessageBox.Show("送出完了");
            }
            else
            {
                MessageBox.Show("接続失敗");
            }
            ftdi.Close();
        }
    }
}
