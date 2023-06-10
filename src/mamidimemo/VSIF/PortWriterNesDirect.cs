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
            lock (LockObject)
            {
                if (FtdiPort != null)
                    sendData(convertToDataPacket(data), data[0].Wait);
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
            SendDataByFtdi(sendData, wait);
        }

        private byte[] convertToDataPacket(PortWriteData[] sendData)
        {
            List<byte> ret = new List<byte>();

            for (int i = 0; i < sendData.Length; i ++)
            {
                byte adr = (byte)~sendData[i].Address;  //DIRECT ADDRESS
                byte dat = (byte)~sendData[i].Data;
                if (sendData[i].Type == 1)
                {
                    ret.Add((byte)(0x00 | ((dat >> 3) & 0xe) | 1));
                    ret.Add((byte)(0x10 | ((dat << 0) & 0xf) | 0));
                }
                else
                {
                    ret.Add((byte)(0x00 | ((adr >> 4) & 0xe) | 0));
                    ret.Add((byte)(0x10 | ((adr >> 1) & 0xe) | 1));
                    ret.Add((byte)(0x10 | ((adr << 1) & 0x6) | 0));

                    ret.Add((byte)(0x10 | ((dat >> 4) & 0xe) | 1));
                    ret.Add((byte)(0x10 | ((dat >> 1) & 0xe) | 0));
                    ret.Add((byte)(0x10 | ((dat << 1) & 0x6) | 1));
                }
            }

            return ret.ToArray();
        }

    }

}
