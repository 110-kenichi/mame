﻿using FTD2XX_NET;
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
    public class PortWriterMsx : PortWriter
    {
        private int lastSccType = -1;
        private int lastSccSlot = -1;

        private int lastOpllType = -1;
        private int lastOpllSlot = -1;

        private int lastOpmType = -1;
        private int lastOpmSlot = -1;

        private byte lastDataType = 0xff;
        private byte lastWriteAddress;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serialPort"></param>
        public PortWriterMsx(SerialPort serialPort) : base(serialPort)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ftdiPort"></param>
        public PortWriterMsx(FTDI ftdiPort, PortId portNo) : base(ftdiPort, portNo)
        {
        }

        public override void ClearDataCache()
        {
            base.ClearDataCache();

            lastSccType = -1;
            lastSccSlot = -1;

            lastOpllType = -1;
            lastOpllSlot = -1;

            lastOpmType = -1;
            lastOpmSlot = -1;

            lastDataType = 0xff;
            lastWriteAddress = 0;
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
            foreach (var dt in data)
            {
                int lsz = ds.Count;
                if (dt.Type == 0 && dt.Address == 0x07)
                    //https://hra1129.github.io/system/psg_reg7.html
                    dt.Data = (byte)((dt.Data & 0x3f) | 0x80);

                switch (dt.Type)
                {
                    case 2:
                        if (lastOpllType != dt.Address || lastOpllSlot != dt.Data)
                        {
                            byte[] sd = new byte[5] {
                                    (byte)(dt.Type           | 0x20),
                                    (byte)((dt.Address >> 4) | 0x00), (byte)((dt.Address & 0x0f) | 0x10),
                                    (byte)((dt.Data    >> 4) | 0x00), (byte)((dt.Data &    0x0f) | 0x10),
                            };
                            ds.AddRange(sd);

                            //バンク切り替えが必要な分のウエイト
                            ds.AddRange(new byte[2] { 0, 0 });
                            //バンク切り替えが必要な分のウエイト
                            if (lastOpllType < 0)
                                ds.AddRange(new byte[2] { 0, 0 });

                            lastOpllType = dt.Address;
                            lastOpllSlot = dt.Data;

                            lastDataType = dt.Type;
                            lastWriteAddress = dt.Address;

                            lastSccType = -1;
                            lastSccSlot = -1;
                        }
                        break;
                    case 3:
                        if (lastSccType != dt.Address || lastSccSlot != dt.Data)
                        {
                            byte[] sd = new byte[5] {
                                (byte)(dt.Type           | 0x20),
                                (byte)((dt.Address >> 4) | 0x00), (byte)((dt.Address & 0x0f) | 0x10),
                                (byte)((dt.Data    >> 4) | 0x00), (byte)((dt.Data &    0x0f) | 0x10),
                            };
                            ds.AddRange(sd);

                            //バンク切り替えが必要な分のウエイト
                            if (dt.Address < 4)
                                ds.AddRange(new byte[4] { 0, 0, 0, 0 });   //自動選択方式
                            else
                                ds.AddRange(new byte[6] { 0, 0, 0, 0, 0, 0 });  //従来方式
                            //バンク切り替えが必要な分のウエイト
                            if (lastSccType < 0)
                                ds.AddRange(new byte[2] { 0, 0 });

                            lastSccType = dt.Address;
                            lastSccSlot = dt.Data;

                            lastDataType = dt.Type;
                            lastWriteAddress = dt.Address;

                            lastOpllType = -1;
                            lastOpllSlot = -1;
                        }
                        break;
                    case 0xd:
                        if (lastOpmType != dt.Address || lastOpmSlot != dt.Data)
                        {
                            lastOpmType = dt.Address;
                            lastOpmSlot = dt.Data;
                            byte[] sd = new byte[5] {
                                    (byte)(dt.Type           | 0x20),
                                    (byte)((dt.Address >> 4) | 0x00), (byte)((dt.Address & 0x0f) | 0x10),
                                    (byte)((dt.Data    >> 4) | 0x00), (byte)((dt.Data &    0x0f) | 0x10),
                            };
                            ds.AddRange(sd);

                            //バンク切り替えが必要な分のウエイト
                            ds.AddRange(new byte[2] { 0, 0 });   //自動選択方式

                            lastDataType = dt.Type;
                            lastWriteAddress = dt.Address;
                        }
                        break;
                    default:
                        {
                            if ((dt.Type == 0xa || dt.Type == 0xb) && lastWriteAddress == 0xf && //YM8950 ADPCM write
                                lastDataType == dt.Type && (ushort)dt.Address == ((ushort)lastWriteAddress))
                            {
                                byte[] sd = new byte[3] {
                                    (byte)(0x00              | 0x20),
                                    (byte)((dt.Data    >> 4) | 0x00), (byte)((dt.Data &    0x0f) | 0x10),
                                };
                                ds.AddRange(sd);
                            }
                            if (dt.Type == 0x11 && lastWriteAddress == 0x8 && //OPNA ADPCM write
                                lastDataType == dt.Type && (ushort)dt.Address == ((ushort)lastWriteAddress))
                            {
                                byte[] sd = new byte[3] {
                                    (byte)(0x00              | 0x20),
                                    (byte)((dt.Data    >> 4) | 0x00), (byte)((dt.Data &    0x0f) | 0x10),
                                };
                                ds.AddRange(sd);
                            }
                            else if (dt.Type == 0x13) //OPNA Pseudo DAC write
                            {
                                byte[] sd = new byte[3] {
                                    (byte)(dt.Type           | 0x20),
                                    (byte)((dt.Data    >> 4) | 0x00), (byte)((dt.Data &    0x0f) | 0x10),
                                };
                                ds.AddRange(sd);
                            }
                            else if (dt.Type == 0x14) //OPN2 DAC write
                            {
                                byte[] sd = new byte[3] {
                                    (byte)(dt.Type           | 0x20),
                                    (byte)((dt.Data    >> 4) | 0x00), (byte)((dt.Data &    0x0f) | 0x10),
                                };
                                ds.AddRange(sd);
                            }
                            else if (dt.Type == 0x15) //tR DAC write
                            {
                                byte[] sd = new byte[3] {
                                    (byte)(dt.Type           | 0x20),
                                    (byte)((dt.Data    >> 4) | 0x00), (byte)((dt.Data &    0x0f) | 0x10),
                                };
                                ds.AddRange(sd);
                            }
                            else if (dt.Type == 0x16)  //OPNA DAC write
                            {
                                byte[] sd = new byte[3] {
                                    (byte)(dt.Type           | 0x20),
                                    (byte)((dt.Data    >> 4) | 0x00), (byte)((dt.Data &    0x0f) | 0x10),
                                };
                                ds.AddRange(sd);
                            }
                            else if ((dt.Type == 1 || dt.Type == 0xc ||   //OPLL
                                dt.Type == 4 || dt.Type == 5 || //SCC
                                dt.Type == 0xa || dt.Type == 0xb || //OPL3
                                dt.Type == 0xe || //OPM   BUSY WAIT
                                dt.Type == 0x10 || dt.Type == 0x11  //OPN2
                                )
                                && lastDataType == dt.Type && (ushort)dt.Address == ((ushort)lastWriteAddress + 1))
                            {
                                byte[] sd = new byte[3] {
                                    (byte)(0x00              | 0x20),
                                    (byte)((dt.Data    >> 4) | 0x00), (byte)((dt.Data &    0x0f) | 0x10),
                                };
                                ds.AddRange(sd);
                            }
                            else if ((dt.Type == 0x1e  //SAA1099, ...
                                )
                                && lastDataType == dt.Type && (ushort)dt.Address == ((ushort)lastWriteAddress))
                            {
                                byte[] sd = new byte[3] {
                                    (byte)(0x00              | 0x20),
                                    (byte)((dt.Data    >> 4) | 0x00), (byte)((dt.Data &    0x0f) | 0x10),
                                };
                                ds.AddRange(sd);
                            }
                            else if (dt.Type >= 0x20)
                            {
                                byte[] sd = new byte[5] {
                                    (byte)(dt.Type - 0x20    | 0x20),
                                    (byte)((dt.Address >> 4) | 0x00), (byte)((dt.Address & 0x0f) | 0x10),
                                    (byte)((dt.Data    >> 4) | 0x00), (byte)((dt.Data &    0x0f) | 0x10),
                                };
                                ds.AddRange(sd);
                            }
                            else
                            {
                                byte[] sd = new byte[5] {
                                    (byte)(dt.Type           | 0x20),
                                    (byte)((dt.Address >> 4) | 0x00), (byte)((dt.Address & 0x0f) | 0x10),
                                    (byte)((dt.Data    >> 4) | 0x00), (byte)((dt.Data &    0x0f) | 0x10),
                                };
                                ds.AddRange(sd);
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
