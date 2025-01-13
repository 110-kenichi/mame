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

namespace zanac.VGMPlayer
{
    public class PortWriterTurboEverDrive : PortWriter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serialPort"></param>
        public PortWriterTurboEverDrive(SerialPort serialPort) : base(serialPort)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ftdiPort"></param>
        public PortWriterTurboEverDrive(FTDI ftdiPort, PortId portNo) : base(ftdiPort, portNo)
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
                    ds.Add(dt.Address);
                    ds.Add(dt.Data);
                }

                List<byte> ds2 = new List<byte>();
                ds2.AddRange(Edio.getTxCMD(SerialPort, Edio.CMD_MEM_WR));
                ds2.AddRange(Edio.getTx32(SerialPort, 0x1810000));
                ds2.AddRange(Edio.getTx32(SerialPort, ds.Count));
                ds2.AddRange(Edio.getTx8(SerialPort, 0));//exec

                ds2.AddRange(ds);

                byte[] dsa = ds2.ToArray();
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
