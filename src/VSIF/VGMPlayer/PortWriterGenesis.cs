﻿using FTD2XX_NET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace zanac.VGMPlayer
{
    public class PortWriterGenesis : PortWriter
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serialPort"></param>
        public PortWriterGenesis(SerialPort serialPort) : base(serialPort)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ftdiPort"></param>
        public PortWriterGenesis(FTDI ftdiPort, PortId portNo) : base(ftdiPort, portNo)
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
            //Remove PCM Wait
            List<PortWriteData> pdata = new List<PortWriteData>(data);
            for (int i = 1; i < pdata.Count; i++)
            {
                if (pdata[i].Type == 0x0 && pdata[i - 1].Type == 0xff)
                {
                    pdata.RemoveAt(i - 1);
                    i--;
                }
            }

            List<byte> ds = new List<byte>();
            List<int> dsw = new List<int>();
            foreach (var dt in pdata)
            {
                ds.Add(dt.Address);
                ds.Add(dt.Data);
                dsw.Add(dt.Wait);
                dsw.Add(dt.Wait);
            }
            byte[] dsa = ds.ToArray();
            int[] dsaw = dsw.ToArray();

            lock (LockObject)
            {
                if (SerialPort != null)
                {
                    for (int i = 0; i < dsa.Length; i += 2)
                        SerialPort?.Write(dsa, i, 2);
                }
                if (FtdiPort != null)
                {
                    convertToDataPacket(dsa);
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
                if (SerialPort != null)
                {
                    for (int i = 0; i < data.Length; i += 2)
                        SerialPort?.Write(data, i, 2);
                }
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

        private void convertToDataPacket(byte[] sendData)
        {
            for (int i = 0; i < sendData.Length; i += 2)
            {
                byte adr = sendData[i + 0];
                byte dat = sendData[i + 1];
                sendData[i + 0] = (byte)(0x40 | (((dat & 0xc0) | adr) >> 2));
                sendData[i + 1] = (byte)(0x00 | (dat & 0x3f));
            }
        }

    }

}
