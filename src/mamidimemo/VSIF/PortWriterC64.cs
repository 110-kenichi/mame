using FTD2XX_NET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace zanac.MAmidiMEmo.VSIF
{
    public class PortWriterC64 : PortWriter
    {
        //https://www.c64-wiki.com/wiki/Control_Port
        //D0 TXD    0
        //D1 RXD    1
        //D2 RTS    2
        //D3 CTS    3
        //D4 DTR    Clk
        //
        //D6 DCD    Y = 2 byte Data
        //D7 RI     X = 1 byte Data

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serialPort"></param>
        public PortWriterC64(SerialPort serialPort) : base(serialPort)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ftdiPort"></param>
        public PortWriterC64(FTDI ftdiPort, PortId portNo) : base(ftdiPort, portNo)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="address"></param>
        /// <param name="data"></param>
        /// <param name="wait"></param>
        public override void Write(PortWriteData[] data)
        {
            List<byte> ds = new List<byte>();
            List<int> dsw = new List<int>();
            for (int i = 0; i < data.Length; i++)
            {
                int lsz = ds.Count;
                var dt = data[i];
                switch (dt.Type)
                {
                    case 0:
                        {
                            byte[] sd = new byte[] {
                                (byte)(((dt.Address >> 0) & 0x07) | 0x00 ),
                                (byte)(((dt.Address >> 3) & 0x07) | 0x18 ),
                                (byte)(((dt.Address >> 6) & 0x03) | 0x10 ),
                                (byte)(((dt.Data    >> 5) & 0x07) | 0x18 ),
                                (byte)(((dt.Data    >> 2) & 0x07) | 0x10 ),
                                (byte)(((dt.Data    >> 0) & 0x03) | 0x18 ),
                            };
                            ds.AddRange(sd);
                        }
                        break;
                    case 1:
                        {
                            var dt2 = data[i+1];
                            byte[] sd = new byte[] {
                                (byte)(((dt.Address >> 0) & 0x07) | 0x00 ),
                                (byte)(((dt.Address >> 3) & 0x07) | 0x18 ),
                                (byte)(((dt.Address >> 6) & 0x03) | 0x10 ),
                                (byte)(((dt.Data    >> 5) & 0x07) | 0x18 ),
                                (byte)(((dt.Data    >> 2) & 0x07) | 0x10 ),
                                (byte)(((dt.Data    >> 0) & 0x03) | 0x18 | 0x4),
                                (byte)(((dt2.Data   >> 5) & 0x07) | 0x10 ),
                                (byte)(((dt2.Data   >> 2) & 0x07) | 0x18 ),
                                (byte)(((dt2.Data   >> 0) & 0x03) | 0x10 ),
                            };
                            i++;
                            ds.AddRange(sd);
                        }
                        break;
                }
                for (int wi = 0; wi < ds.Count - lsz; wi++)
                    dsw.Add(dt.Wait);
            }
            byte[] dsa = ds.ToArray();
            int[] dsaw = dsw.ToArray();

            lock (LockObject)
            {
                if (FtdiPort != null)
                {
                    sendData(dsa, dsaw);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="wait"></param>
        public override void RawWrite(byte[] data, int[] wait)
        {
            lock (LockObject)
            {
                if (FtdiPort != null)
                {
                    sendData(data, wait);
                }
            }
        }

        private void sendData(byte[] sendData, int[] wait)
        {
            SendDataByFtdi(sendData, wait);
        }

    }

}
