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
    public class PortWriterNesDirect : PortWriter
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serialPort"></param>
        public PortWriterNesDirect(SerialPort serialPort) : base(serialPort)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ftdiPort"></param>
        public PortWriterNesDirect(FTDI ftdiPort, PortId portNo) : base(ftdiPort, portNo)
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="address"></param>
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

        private void sendData(byte[] sendData, int wait)
        {
            wait = (int)(VsifManager.FTDI_BAUDRATE_NES_MUL * wait) / 100;

            SendData(sendData, wait);
        }

        private byte[] convertToDataPacket(byte[] sendData)
        {
            byte[] ret = new byte[sendData.Length * 2];

            for (int i = 0; i < sendData.Length; i += 2)
            {
                byte adr = (byte)~sendData[i + 0];  //DIRECT ADDRESS
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
