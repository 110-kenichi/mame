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
            byte[] dsa = convertToDataPacket(data);
            int[] dsaw = convertToWaitPacket(data);

            lock (LockObject)
            {
                if (FtdiPort != null)
                    sendData(dsa, dsaw);
            }
        }

        private static int[] convertToWaitPacket(PortWriteData[] data)
        {
            List<int> dsw = new List<int>();
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].Type == 1)
                {
                    dsw.Add(data[i].Wait);
                    dsw.Add(data[i].Wait);
                }
                else
                {
                    dsw.Add(data[i].Wait);
                    dsw.Add(data[i].Wait);
                    dsw.Add(data[i].Wait);

                    dsw.Add(data[i].Wait);
                    dsw.Add(data[i].Wait);
                    dsw.Add(data[i].Wait);
                }
            }
            int[] dsaw = dsw.ToArray();
            return dsaw;
        }

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

        private byte[] convertToDataPacket(PortWriteData[] sendData)
        {
            List<byte> ret = new List<byte>();

            for (int i = 0; i < sendData.Length; i++)
            {
                byte adr = (byte)~(sendData[i].Address << 1);     //INDIRECT ADDRESS INDEX
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
