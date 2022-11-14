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
                if (dt.Type == 3)
                {
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

                        //dummy wait
                        sd = new byte[5] { 0, 0, 0, 0, 0 };
                        for (int i = 0; i < 8; i++)
                            ds.AddRange(sd);
                    }
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
            uint writtenBytes = 0;
            var stat = FtdiPort.Write(sd, sd.Length, ref writtenBytes);
            if (stat != FTDI.FT_STATUS.FT_OK)
                Debug.WriteLine(stat);
        }

    }

}
