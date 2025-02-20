using FTD2XX_NET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using zanac.MAmidiMEmo.Gimic;
using zanac.MAmidiMEmo.TurboLink;
using static zanac.MAmidiMEmo.Gimic.GimicManager;

namespace zanac.MAmidiMEmo.VSIF
{
    public class PortWriterNanoDrive : PortWriter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serialPort"></param>
        public PortWriterNanoDrive(SerialPort serialPort) : base(serialPort)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ftdiPort"></param>
        public PortWriterNanoDrive(FTDI ftdiPort, PortId portNo) : base(ftdiPort, portNo)
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
            try
            {
                List<byte> ds = new List<byte>();
                foreach (var dt in data)
                {
                    switch (dt.Type)
                    {
                        //PSG (SN76489/SN76496) write value dd
                        case 0x50:
                            ds.Add(dt.Type);
                            ds.Add(dt.Data);
                            break;
                        //YM2612 port 0, write value dd to register aa
                        case 0x52:
                        //YM2612 port 1, write value dd to register aa
                        case 0x53:
                            ds.Add(dt.Type);
                            ds.Add(dt.Address);
                            ds.Add(dt.Data);
                            break;
                    }
                }

                byte[] dsa = ds.ToArray();
                lock (LockObject)
                    SerialPort?.Write(dsa, 0, dsa.Length);
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(Exception))
                    throw;
                else if (ex.GetType() == typeof(SystemException))
                    throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="wait"></param>
        public override void RawWrite(byte[] data, int[] wait)
        {
            try
            {
                lock (LockObject)
                    SerialPort?.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(Exception))
                    throw;
                else if (ex.GetType() == typeof(SystemException))
                    throw;
            }
        }

    }

}
