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
    public class PortWriterMsx : PortWriter
    {
        private int lastSccType = -1;
        private int lastSccSlot = -1;

        private int lastOpllType = -1;
        private int lastOpllSlot = -1;

        private int lastOpmType = -1;
        private int lastOpmSlot = -1;

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

        private byte lastDataType = 0xff;
        private byte lastWriteSccAddress;

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
            foreach (var dt in data)
            {
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
                            lastWriteSccAddress = dt.Address;

                            lastSccType = -1;
                            lastSccSlot = -1;
                        }
                        break;
                    case 3:
                        if (lastSccType != dt.Address || lastSccSlot != dt.Data)
                        {
                            lastSccType = dt.Address;
                            lastSccSlot = dt.Data;

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
                            lastWriteSccAddress = dt.Address;

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
                            lastWriteSccAddress = dt.Address;
                        }
                        break;
                    default:
                        {
                            if ((dt.Type == 1 || dt.Type == 0xc ||   //OPLL
                                dt.Type == 4 || dt.Type == 5 || //SCC
                                dt.Type == 0xa || dt.Type == 0xb || //OPL3
                                dt.Type == 0xe || //OPM
                                dt.Type == 0x10 || dt.Type == 0x11  //OPN2 or OPNA
                                )
                                && lastDataType == dt.Type && (ushort)dt.Address == ((ushort)lastWriteSccAddress + 1))
                            {
                                byte[] sd = new byte[3] {
                                    (byte)(0x1f              | 0x20),
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
                            lastWriteSccAddress = dt.Address;
                            break;
                        }
                }
            }
            byte[] dsa = ds.ToArray();

            lock (LockObject)
            {
                if (FtdiPort != null)
                {
                    sendData(dsa, data[0].Wait);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="wait"></param>
        public override void RawWrite(byte[] data, int wait)
        {
            lock (LockObject)
            {
                if (FtdiPort != null)
                {
                    sendData(data, wait);
                }
            }
        }

        [DllImport("msvcrt.dll", EntryPoint = "memset", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        private static extern IntPtr MemSet(IntPtr dest, int c, int count);

        private void sendData(byte[] sendData, int wait)
        {
            wait = (int)(VsifManager.FTDI_BAUDRATE_MSX_MUL * wait) / 100;

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
            SendData(sd);
        }

    }

}
