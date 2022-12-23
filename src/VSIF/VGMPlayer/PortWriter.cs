using FTD2XX_NET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace zanac.VGMPlayer
{
    public abstract class PortWriter : IDisposable
    {
        private bool disposedValue;

        private SerialPort serialPort;

        protected object LockObject = new object();

        /// <summary>
        /// Need lock by LockObject to use this object
        /// </summary>
        protected SerialPort SerialPort
        {
            get
            {
                return serialPort;
            }
        }

        private FTDI ftdiPort;

        /// <summary>
        /// Need lock by LockObject to use this object
        /// </summary>
        protected FTDI FtdiPort
        {
            get
            {
                return ftdiPort;
            }
        }

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ftdiPort"></param>
        public PortWriter(FTDI ftdiPort, PortId portNo)
        {
            this.ftdiPort = ftdiPort;
            PortName = "FTDI_COM" + (int)portNo;
        }

        public abstract void Write(PortWriteData[] data);

        public abstract void RawWrite(byte[] data, int wait);

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージド状態を破棄します (マネージド オブジェクト)
                }
                lock (LockObject)
                {
                    serialPort?.Dispose();
                    //uint dummy = 0;
                    //ftdiPort?.Write(new byte[] { 0xFF }, 1, ref dummy);
                    ftdiPort?.SetBitMode(0x00, FTDI.FT_BIT_MODES.FT_BIT_MODE_RESET);
                    ftdiPort?.Close();
                }
                // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
                // TODO: 大きなフィールドを null に設定します
                disposedValue = true;
            }
        }

        protected void SendData(byte[] sd2)
        {
            for (int i = 0; i < sd2.Length; i += 64)
            {
                byte[] sd = new byte[64];
                if (i + 64 < sd2.Length)
                    Buffer.BlockCopy(sd2, i, sd, 0, 64);
                else
                {
                    Buffer.BlockCopy(sd2, i, sd, 0, sd2.Length - i);
                    for (int j = sd2.Length - i; j < sd2.Length; j++)
                        sd2[j] = sd2[sd2.Length - i - 1];
                }

                while (true)
                {
                    uint writtenBytes = 0;
                    var stat = FtdiPort.Write(sd, sd.Length, ref writtenBytes);
                    if (stat != FTDI.FT_STATUS.FT_OK)
                    {
                        Debug.WriteLine(stat);
                        break;
                    }
                    if (sd.Length == writtenBytes)
                        break;

                    byte[] nsd = new byte[sd.Length - writtenBytes];
                    Buffer.BlockCopy(sd, (int)writtenBytes, nsd, 0, nsd.Length);
                    sd = nsd;
                }
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
