using FTD2XX_NET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;

namespace zanac.VGMPlayer
{
    public class PortWriterMsxPi : PortWriter
    {
        private int lastSccType = -1;
        private int lastSccSlot = -1;

        private int lastOpllType = -1;
        private int lastOpllSlot = -1;

        private int lastOpmType = -1;
        private int lastOpmSlot = -1;

        private byte lastDataType = 0xff;
        private byte lastWriteAddress;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serialPort"></param>
        public PortWriterMsxPi(SerialPort serialPort) : base(serialPort)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ftdiPort"></param>
        public PortWriterMsxPi(FTDI ftdiPort, PortId portNo) : base(ftdiPort, portNo)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Abort()
        {
            lastOpllType = -1;
            lastOpllSlot = -1;

            lastSccType = -1;
            lastSccSlot = -1;

            lastOpmType = -1;
            lastOpmSlot = -1;

            lastDataType = 0xff;
            lastWriteAddress = 0;

            base.Abort();
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
                if (dt.Type == 0x17 && dt.Address == 0x07)
                    //https://hra1129.github.io/system/psg_reg7.html
                    dt.Data = (byte)((dt.Data & 0x3f) | 0x80);
                if (lastDataType == dt.Type && dt.Wait < 0)
                {
                    //dummy wait
                    for (int i = 0; i < -dt.Wait; i++)
                        ds.Add(0x1F);
                }
                switch (dt.Type)
                {
                    case 0x1F:
                        //dummy wait
                        ds.Add(dt.Type);
                        break;
                    case 2:
                        if (lastOpllType != dt.Address || lastOpllSlot != dt.Data)
                        {
                            byte[] sd = new byte[] {
                                    dt.Type,
                                    dt.Address,
                                    dt.Data,
                            };
                            ds.AddRange(sd);

                            lastOpllType = dt.Address;
                            lastOpllSlot = dt.Data;

                            lastSccType = -1;
                            lastSccSlot = -1;
                        }
                        break;
                    case 3:
                        if (lastSccType != dt.Address || lastSccSlot != dt.Data)
                        {
                            lastSccType = dt.Address;
                            lastSccSlot = dt.Data;

                            byte[] sd = new byte[] {
                                dt.Type,
                                dt.Address,
                                dt.Data,
                            };
                            ds.AddRange(sd);

                            lastSccType = dt.Address;
                            lastSccSlot = dt.Data;

                            lastOpllType = -1;
                            lastOpllSlot = -1;
                        }
                        break;
                    case 0x18:  // NEOTRON SLOT
                        if (lastSccType != dt.Address || lastSccSlot != dt.Data)
                        {
                            lastSccType = dt.Address;
                            lastSccSlot = dt.Data;

                            byte[] sd = new byte[] {
                                dt.Type,
                                dt.Address,
                                dt.Data,
                            };
                            ds.AddRange(sd);

                            lastSccType = dt.Address;
                            lastSccSlot = dt.Data;

                            lastOpllType = -1;
                            lastOpllSlot = -1;
                        }
                        break;
                    case 0xd:
                        if (lastOpmType != dt.Address || lastOpmSlot != dt.Data)
                        {
                            lastOpmType = dt.Address;
                            lastOpmSlot = dt.Data;
                            byte[] sd = new byte[] {
                                dt.Type,
                                dt.Address,
                                dt.Data,
                            };
                            ds.AddRange(sd);
                        }
                        break;
                    default:
                        {
                            if ((dt.Type == 0xa || dt.Type == 0xb) && lastWriteAddress == 0xf && //YM8950 ADPCM write
                                lastDataType == dt.Type && (ushort)dt.Address == ((ushort)lastWriteAddress))
                            {
                                byte[] sd = new byte[] {
                                    0x00,
                                    dt.Data,
                                };
                                ds.AddRange(sd);
                            }
                            else if (dt.Type == 0x11 && lastWriteAddress == 0x8 && //OPNA ADPCM write
                                lastDataType == dt.Type && (ushort)dt.Address == ((ushort)lastWriteAddress))
                            {
                                byte[] sd = new byte[] {
                                    0x00,
                                    dt.Data,
                                };
                                ds.AddRange(sd);
                            }
                            else if (dt.Type == 0x13) //OPNA Pseudo DAC write
                            {
                                byte[] sd = new byte[] {
                                    dt.Type,
                                    dt.Data,
                                };
                                ds.AddRange(sd);
                            }
                            else if (dt.Type == 0x14) //OPN2 DAC write
                            {
                                byte[] sd = new byte[] {
                                    dt.Type,
                                    dt.Data,
                                };
                                ds.AddRange(sd);
                            }
                            else if (dt.Type == 0x15) //tR DAC write
                            {
                                byte[] sd = new byte[] {
                                    dt.Type,
                                    dt.Data,
                                };
                                ds.AddRange(sd);
                            }
                            else if (dt.Type == 0x16)  //OPNA DAC write
                            {
                                byte[] sd = new byte[] {
                                    dt.Type,
                                    dt.Data,
                                };
                                ds.AddRange(sd);
                            }
                            else if ((dt.Type == 1 || dt.Type == 0xc ||   //OPLL
                                                                          //dt.Type == 4 || dt.Type == 5 || //SCC
                                dt.Type == 0xa || dt.Type == 0xb || //OPL3
                                dt.Type == 0xe || //OPM  BUSY WAIT
                                dt.Type == 0x10 || dt.Type == 0x11 || //OPN2 or OPNA
                                dt.Type == 0x12 || //OPN
                                dt.Type == 0x1A || dt.Type == 0x1B //OPNB
                                )
                                && lastDataType == dt.Type && (ushort)dt.Address == ((ushort)lastWriteAddress + 1))
                            {
                                //連続書き込み
                                byte[] sd = new byte[] {
                                    0,
                                    dt.Data,
                                };
                                ds.AddRange(sd);
                            }
                            else if ((dt.Type == 0x1e  //SAA1099, ...
                                )
                                && lastDataType == dt.Type && (ushort)dt.Address == ((ushort)lastWriteAddress))
                            {
                                //連続書き込み
                                byte[] sd = new byte[] {
                                    0,
                                    dt.Data,
                                };
                                ds.AddRange(sd);
                            }
                            else if (dt.Type >= 0x20)
                            {
                                byte[] sd = new byte[] {
                                    (byte)(dt.Type - 0x20),
                                    dt.Address,
                                    dt.Data,
                                };
                                ds.AddRange(sd);
                            }
                            else
                            {
                                byte[] sd = new byte[] {
                                    dt.Type,
                                    dt.Address,
                                    dt.Data,
                                };
                                ds.AddRange(sd);
                            }
                            lastDataType = dt.Type;
                            lastWriteAddress = dt.Address;
                            break;
                        }
                }
            }
            sendData(ds.ToArray());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="wait"></param>
        public override void RawWrite(byte[] data, int[] wait)
        {
            sendData(data);
        }

        private void sendData(byte[] sendData)
        {
            lock (LockObject)
            {
                try
                {
                    SerialPort?.Write(sendData, 0, sendData.Length);
                }
                catch (Exception ex)
                {
                    SerialPort.Close();
                    if (ex.GetType() == typeof(Exception))
                        throw;
                    else if (ex.GetType() == typeof(SystemException))
                        throw;
                }
            }
        }

    }

}
