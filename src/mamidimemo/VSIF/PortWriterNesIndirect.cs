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

        private void sendData(byte[] sendData, int wait)
        {
            wait = (int)(VsifManager.FTDI_BAUDRATE_NES_MUL * wait) / 100;

            SendDataByFtdi(sendData, wait);
        }

        private byte[] convertToDataPacket(byte[] sendData)
        {
            List<byte> ret = new List<byte>();

            for (int i = 0; i < sendData.Length; i += 2)
            {
                byte adr = (byte)~(sendData[i + 0] << 1);   //INDIRECT ADDRESS INDEX
                byte dat = (byte)~sendData[i + 1];

                ret.Add((byte)(0x00 | ((adr >> 4) & 0xe) | 0));
                ret.Add((byte)(0x10 | ((adr >> 1) & 0xe) | 1));
                ret.Add((byte)(0x10 | ((adr << 1) & 0x6) | 0));

                ret.Add((byte)(0x10 | ((dat >> 4) & 0xe) | 1));
                ret.Add((byte)(0x10 | ((dat >> 1) & 0xe) | 0));
                ret.Add((byte)(0x10 | ((dat << 1) & 0x6) | 1));
            }

            return ret.ToArray();
        }

    }

}
