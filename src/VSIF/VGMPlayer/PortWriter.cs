using FTD2XX_NET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zanac.VGMPlayer
{
    public class PortWriter : IDisposable
    {
        private bool disposedValue;

        private SerialPort serialPort;

        private object lockObject = new object();

        /// <summary>
        /// 
        /// </summary>
        public string PortName
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serialPort"></param>
        public PortWriter(SerialPort serialPort)
        {
            this.serialPort = serialPort;
            PortName = serialPort.PortName;
        }

        private FTDI ftdiPort;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ftdiPort"></param>
        public PortWriter(FTDI ftdiPort, ComPort portNo)
        {
            this.ftdiPort = ftdiPort;
            PortName = "FTDI_COM" + (int)portNo;
        }

        public void Write(byte address, byte data)
        {
            lock (lockObject)
            {
                serialPort?.Write(new byte[] { address, data }, 0, 2);
                if (ftdiPort != null)
                {
                    List<byte> sendData = new List<byte>();
                    {
                        sendData.Add((byte)(((address << 1) & 0xe) | 0));
                        sendData.Add((byte)(((address >> 2) & 0xe) | 1));
                        sendData.Add((byte)(((address >> 5) & 0xe) | 0));
                        sendData.Add(1);
                    }
                    {
                        sendData.Add((byte)(((data << 1) & 0xe) | 0));
                        sendData.Add((byte)(((data >> 2) & 0xe) | 1));
                        sendData.Add((byte)(((data >> 5) & 0xe) | 0));
                        sendData.Add(1);
                    }
                    var sd = sendData.ToArray();
                    uint writtenBytes = 0;
                    var stat = ftdiPort.Write(sd, sd.Length, ref writtenBytes);
                    if (stat != FTDI.FT_STATUS.FT_OK)
                        Debug.WriteLine(stat);
                }
            }
        }

        public void Write(byte[] data)
        {
            lock (lockObject)
            {
                serialPort?.Write(data, 0, data.Length);
                if (ftdiPort != null)
                {
                    List<byte> sendData = new List<byte>();
                    foreach (var dt in data)
                    {
                        sendData.Add((byte)(((dt << 1) & 0xe) | 0));
                        sendData.Add((byte)(((dt >> 2) & 0xe) | 1));
                        sendData.Add((byte)(((dt >> 5) & 0xe) | 0));
                        sendData.Add(1);
                    }
                    var sd = sendData.ToArray();
                    uint writtenBytes = 0;
                    var stat = ftdiPort.Write(sd, sd.Length, ref writtenBytes);
                    if (stat != FTDI.FT_STATUS.FT_OK)
                        Debug.WriteLine(stat);
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージド状態を破棄します (マネージド オブジェクト)
                }
                lock (lockObject)
                {
                    serialPort?.Dispose();
                    serialPort = null;
                    //uint dummy = 0;
                    //ftdiPort?.Write(new byte[] { 0xFF }, 1, ref dummy);
                    ftdiPort?.SetBitMode(0x00, FTDI.FT_BIT_MODES.FT_BIT_MODE_RESET);
                    ftdiPort?.Close();
                    ftdiPort = null;
                }
                // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
                // TODO: 大きなフィールドを null に設定します
                disposedValue = true;
            }
        }

        // // TODO: 'Dispose(bool disposing)' にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします
        // ~PortWriter()
        // {
        //     // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
