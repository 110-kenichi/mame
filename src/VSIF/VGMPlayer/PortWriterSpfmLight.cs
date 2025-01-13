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

namespace zanac.VGMPlayer
{
    public class PortWriterSpfmLight : PortWriter
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serialPort"></param>
        public PortWriterSpfmLight(SerialPort serialPort) : base(serialPort)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ftdiPort"></param>
        public PortWriterSpfmLight(FTDI ftdiPort, PortId portNo) : base(ftdiPort, portNo)
        {
        }

        private bool lastDataisPseudoDac;

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
                        case 0x00:  //OPNA P0
                            ds.Add(0x00);
                            ds.Add(0x00);
                            ds.Add(dt.Address);
                            ds.Add(dt.Data);
                            lastDataisPseudoDac = false;
                            break;
                        case 0x01:  //OPNA P1
                            ds.Add(0x00);
                            ds.Add(0x02);
                            ds.Add(dt.Address);
                            ds.Add(dt.Data);
                            lastDataisPseudoDac = false;
                            break;
                        case 0x02:  //OPNA P1  Pseudo DAC
                            if (lastDataisPseudoDac)
                            {
                                //dummy wait
                                //ds.Add(0x00);
                                //ds.Add(0x00);
                                //ds.Add(0x0e);
                                //ds.Add(0x00);
                            }
                            ds.Add(0x00);
                            ds.Add(0x02);
                            ds.Add(dt.Address);
                            ds.Add(dt.Data);
                            lastDataisPseudoDac = true;
                            break;
                        case 0x10:  //OPM
                            ds.Add(0x01);
                            ds.Add(0x00);
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
