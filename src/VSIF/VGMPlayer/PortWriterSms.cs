using FTD2XX_NET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using zanac.VGMPlayer;

namespace zanac.MAmidiMEmo.VSIF
{
    public class PortWriterSms : PortWriter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serialPort"></param>
        public PortWriterSms(SerialPort serialPort) : base(serialPort)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ftdiPort"></param>
        public PortWriterSms(FTDI ftdiPort, PortId portNo) : base(ftdiPort, portNo)
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
                if (dt.Address == 0)
                {
                }
                if (dt.Address == 0x10)
                {
                }
                switch (dt.Type)
                {
                    default:
                        {
                            byte[] sd = new byte[]
                            {
                                (byte)(((dt.Address & 0xf0) >> 2) | 0x03), (byte)(((dt.Address & 0x0f) << 2) | 0x00),
                                (byte)(((dt.Data    & 0xf0) >> 2) | 0x01), (byte)(((dt.Data    & 0x0f) << 2) | 0x00),
                            };
                            ds.AddRange(sd);
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

        private void sendData(byte[] sendData, int wait)
        {
            SendDataByFtdi(sendData, wait);
        }

    }

}
