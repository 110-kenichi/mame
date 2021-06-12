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
    public class PortWriterNes : PortWriter
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serialPort"></param>
        public PortWriterNes(SerialPort serialPort) : base(serialPort)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ftdiPort"></param>
        public PortWriterNes(FTDI ftdiPort, PortId portNo) : base(ftdiPort, portNo)
        {
        }

        public override void Write(byte address, byte data)
        {
            SerialPort?.Write(new byte[] { address, data }, 0, 2);
            if (FtdiPort != null)
            {
                byte[] sd = new byte[2] { address, data };
                sendData(convertToDataPacket(sd));
            }
        }

        public override void Write(byte[] data)
        {
            SerialPort?.Write(data, 0, data.Length);
            if (FtdiPort != null)
            {
                sendData(convertToDataPacket(data));
            }
        }

        [DllImport("msvcrt.dll", EntryPoint = "memset", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        private static extern IntPtr MemSet(IntPtr dest, int c, int count);

        private void sendData(byte[] sendData)
        {
            int wait = (int)(VsifManager.FTDI_BAUDRATE_NES_MUL * 8) / 100;

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
                byte adr = sendData[i + 0];
                byte dat = sendData[i + 1];
                sendData[(i * 2) + 0] = (byte)(0x10 | adr >> 4);
                sendData[(i * 2) + 1] = (byte)(0x00 | adr & 0xf);
                sendData[(i * 2) + 2] = (byte)(0x10 | dat >> 4);
                sendData[(i * 2) + 3] = (byte)(0x00 | dat & 0xf);
            }

            return ret;
        }

    }

}
