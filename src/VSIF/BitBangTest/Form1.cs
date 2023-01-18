using FTD2XX_NET;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
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

            numericUpDown1_ValueChanged(null, null);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            labelStatus.Text = "送出中...";
            Application.DoEvents();

            byte[] data = new byte[2]
                {
                    0b01010101,
                    0b10101010
                };
            var ftdi = new FTD2XX_NET.FTDI();
            var stat = ftdi.OpenByIndex((uint)numericUpDownPort.Value);
            if (stat == FTDI.FT_STATUS.FT_OK)
            {
                ftdi.SetBitMode(0x00, FTDI.FT_BIT_MODES.FT_BIT_MODE_RESET);
                ftdi.SetBitMode(0xff, FTDI.FT_BIT_MODES.FT_BIT_MODE_ASYNC_BITBANG);

                double rate = Math.Round(3000000d / ((double)numericUpDownDiv1.Value + (double)numericUpDownDiv2.Value));
                ftdi.SetBaudRate((uint)rate);

                ftdi.SetTimeouts(500, 500);
                ftdi.SetLatency(0);
                List<byte> sd = new List<byte>();
                for (int i = 0; i < numericUpDownNum.Value; i++)
                {
                    sd.AddRange(data);
                }
                sendData(ftdi, sd.ToArray(), (int)numericUpDownWidth.Value);
                labelStatus.Text = "送出完了";
            }
            else
            {
                labelStatus.Text = "接続失敗";
            }
            ftdi.Close();
        }

        [DllImport("msvcrt.dll", EntryPoint = "memset", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        private static extern IntPtr MemSet(IntPtr dest, int c, int count);

        protected void SendData(FTDI ftdi, byte[] sendData, int wait)
        {
            var rawSendData = new byte[sendData.Length * (int)wait];
            unsafe
            {
                fixed (byte* sdp = rawSendData)
                {
                    byte* tsdp = sdp;
                    foreach (var data in sendData)
                    {
                        for (int j = 0; j < wait; j++)
                            *tsdp++ = data;
                        //MemSet((IntPtr)bp, dt, (int)wait);
                        //tsdp += (int)wait;
                    }
                }
            }

            var sendBuffer = new Span<byte>(rawSendData);
            while (true)
            {
                uint writtenBytes = 0;
                var stat = ftdi.Write(sendBuffer.ToArray(), sendBuffer.Length, ref writtenBytes);
                if (stat != FTDI.FT_STATUS.FT_OK)
                    break;
                if (sendBuffer.Length == writtenBytes)
                    break;

                sendBuffer = sendBuffer.Slice((int)writtenBytes, (int)(sendBuffer.Length - writtenBytes));
            }
        }

        private void sendData(FTDI ftdi, byte[] sendData, int wait)
        {
            var osd = sendData.ToArray();
            byte[] sd = new byte[osd.Length * (int)wait];
            unsafe
            {
                for (int i = 0; i < osd.Length; i++)
                {
                    fixed (byte* bp = &sd[i * (int)wait])
                        MemSet(new IntPtr(bp), osd[i], (int)wait);
                }
            }
            sendDataCore2(ftdi, sd);
        }

        private void sendDataCore2(FTDI ftdi, byte[] sd)
        {
            while (true)
            {
                uint writtenBytes = 0;
                var stat = ftdi.Write(sd, sd.Length, ref writtenBytes);
                if (stat != FTDI.FT_STATUS.FT_OK)
                {
                    Debug.WriteLine(stat);
                    break;
                }
                if (sd.Length == writtenBytes)
                    break;

                byte[] nsd = new byte[sd.Length - writtenBytes];
                Buffer.BlockCopy(sd, (int)writtenBytes, nsd, 0, nsd.Length);
                sd = nsd;
            }
        }

        private void sendDataCore(FTDI ftdi, byte[] sd2)
        {
            for (int i = 0; i < sd2.Length; i += 62)
            {
                byte[] sd = new byte[62];
                if (i + 62 < sd2.Length)
                    Buffer.BlockCopy(sd2, i, sd, 0, 62);
                else
                {
                    Buffer.BlockCopy(sd2, i, sd, 0, sd2.Length - i);
                    for (int j = sd2.Length - i; j < sd2.Length; j++)
                        sd2[j] = sd2[sd2.Length - i - 1];
                }

                while (true)
                {
                    uint writtenBytes = 0;
                    var stat = ftdi.Write(sd, sd.Length, ref writtenBytes);
                    if (stat != FTDI.FT_STATUS.FT_OK)
                    {
                        Debug.WriteLine(stat);
                        break;
                    }
                    if (sd.Length == writtenBytes)
                        break;

                    byte[] nsd = new byte[sd.Length - writtenBytes];
                    Buffer.BlockCopy(sd, (int)writtenBytes, nsd, 0, nsd.Length);
                    sd = nsd;
                }
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            double rate = Math.Round(3000000d / ((double)numericUpDownDiv1.Value + (double)numericUpDownDiv2.Value));
            textBoxBaudrate.Text = ((uint)rate).ToString();
        }
    }
}
