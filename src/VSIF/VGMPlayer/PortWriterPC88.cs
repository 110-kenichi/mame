using FTD2XX_NET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace zanac.VGMPlayer
{
    public class PortWriterPC88 : PortWriter
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serialPort"></param>
        public PortWriterPC88(SerialPort serialPort) : base(serialPort)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ftdiPort"></param>
        public PortWriterPC88(FTDI ftdiPort, PortId portNo) : base(ftdiPort, portNo)
        {
        }

        private byte lastDataType = 0xff;
        private byte lastWriteAddress;

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
            foreach (var dt in data)
            {
                int lsz = ds.Count;
                switch (dt.Type)
                {
                    default:
                        {
                            if ((dt.Type == 0x3 || dt.Type == 0xb ||  //OPNA Pseudo DAC write
                                dt.Type == 0x4 || dt.Type == 0xc ||     //OPNA ADPCM data write
                                dt.Type == 0x5 || dt.Type == 0xd)     //OPNA DAC  write
                                && lastDataType == dt.Type && (ushort)dt.Address == ((ushort)lastWriteAddress))
                            {
                                byte[] sd = new byte[] {
                                    (byte)(0x0f              | 0x20),
                                    (byte)((dt.Data    >> 4) | 0x10), (byte)((dt.Data &    0x0f) | 0x00),
                                };
                                ds.AddRange(sd);
                            }
                            else
                            if ((dt.Type == 0x2 || dt.Type == 0xa)   // Rhythm
                                && lastDataType == dt.Type && (ushort)dt.Address == ((ushort)lastWriteAddress + 1))
                            {
                                byte[] sd = new byte[] {
                                    (byte)(0x0f              | 0x20),
                                    (byte)((dt.Data    >> 4) | 0x10), (byte)((dt.Data &    0x0f) | 0x00),
                                    0x10, //FM Wait
                                };
                                ds.AddRange(sd);
                                ds.AddRange(new byte[] { 0x10, 0x10, 0x10, 0x10, 0x10, 0x10 });
                            }
                            else
                            if ((dt.Type == 0x0 || dt.Type == 0x1 ||  //OPNA
                                dt.Type == 0x8 || dt.Type == 0xa //OPNA SB2
                                )
                                && lastDataType == dt.Type && (ushort)dt.Address == ((ushort)lastWriteAddress + 1))
                            {
                                byte[] sd = new byte[] {
                                    (byte)(0x0f              | 0x20),
                                    (byte)((dt.Data    >> 4) | 0x10), (byte)((dt.Data &    0x0f) | 0x00),
                                    0x10, //FM Wait
                                };
                                ds.AddRange(sd);
                            }
                            else
                            {
                                if (dt.Type == 0x3 || dt.Type == 0xb ||  //OPNA Pseudo DAC write
                                    dt.Type == 0x4 || dt.Type == 0xc ||    //OPNA ADPCM data write
                                    dt.Type == 0x5 || dt.Type == 0xd)    //OPNA DAC write
                                {
                                    byte[] sd = new byte[] {
                                        (byte)(dt.Type           | 0x20),
                                        (byte)((dt.Data    >> 4) | 0x10), (byte)((dt.Data &    0x0f) | 0x00),
                                    };
                                    ds.AddRange(sd);
                                }
                                else if (dt.Type == 2 || dt.Type == 0xa)   // Rhythm
                                {
                                    byte[] sd = new byte[] {
                                        (byte)(dt.Type           | 0x20),
                                        (byte)((dt.Address >> 4) | 0x10), (byte)((dt.Address & 0x0f) | 0x00),
                                        (byte)((dt.Data    >> 4) | 0x10), (byte)((dt.Data &    0x0f) | 0x00),
                                        0x10,   //FM Wait
                                    };
                                    ds.AddRange(sd);
                                    ds.AddRange(new byte[] { 0x10, 0x10, 0x10, 0x10, 0x10, 0x10 });
                                }
                                else
                                {
                                    byte[] sd = new byte[] {
                                        (byte)(dt.Type           | 0x20),
                                        (byte)((dt.Address >> 4) | 0x10), (byte)((dt.Address & 0x0f) | 0x00),
                                        (byte)((dt.Data    >> 4) | 0x10), (byte)((dt.Data &    0x0f) | 0x00),
                                        0x10,   //FM Wait
                                    };
                                    ds.AddRange(sd);
                                }
                            }
                            lastDataType = dt.Type;
                            lastWriteAddress = dt.Address;
                            break;
                        }
                }
                for (int i = 0; i < ds.Count - lsz; i++)
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
