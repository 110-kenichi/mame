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
    public class PortWriterNesIndirect : PortWriter
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serialPort"></param>
        public PortWriterNesIndirect(SerialPort serialPort) : base(serialPort)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ftdiPort"></param>
        public PortWriterNesIndirect(FTDI ftdiPort, PortId portNo) : base(ftdiPort, portNo)
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
                ds.Add(dt.Address);
                ds.Add(dt.Data);
            }
            byte[] dsa = ds.ToArray();

            lock (LockObject)
            {
                if (FtdiPort != null)
                    sendData(convertToDataPacket(dsa), data[0].Wait);
            }
        }

        public override void RawWrite(byte[] data, int wait)
        {
            if (FtdiPort != null)
            {
                sendData(data, wait);
            }
        }

        [DllImport("msvcrt.dll", EntryPoint = "memset", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        private static extern IntPtr MemSet(IntPtr dest, int c, int count);

        private void sendData(byte[] sendData, int wait)
        {
            wait = (int)(VsifManager.FTDI_BAUDRATE_NES_MUL * wait) / 100;

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

        private byte[] convertToDataPacket(byte[] sendData)
        {
            byte[] ret = new byte[sendData.Length * 2];

            for (int i = 0; i < sendData.Length; i += 2)
            {
                byte adr = (byte)~(sendData[i + 0] << 1);   //INDIRECT ADDRESS INDEX
                byte dat = (byte)~sendData[i + 1];

                ret[(i * 2) + 0] = (byte)(0x10 | adr >> 4);
                ret[(i * 2) + 1] = (byte)(0x00 | adr & 0xf);
                ret[(i * 2) + 2] = (byte)(0x10 | dat >> 4);
                ret[(i * 2) + 3] = (byte)(0x00 | dat & 0xf);
            }

            return ret;
        }

    }

}
