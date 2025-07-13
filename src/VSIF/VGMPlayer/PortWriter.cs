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
        public PortWriter(string portName)
        {
            PortName = portName;
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

        public abstract void RawWrite(byte[] data, int[] wait);

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

        public void Purge()
        {
            ftdiPort?.Purge(FTD2XX_NET.FTDI.FT_PURGE.FT_PURGE_TX);
        }

        private bool abortRequested;

        public virtual void Abort()
        {
            abortRequested = true;
        }

        [DllImport("msvcrt.dll", EntryPoint = "memset", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        private static extern IntPtr MemSet(IntPtr dest, int c, int count);

        protected void SendDataByFtdi(byte[] sendData, int[] wait)
        {
            List<byte> rawSendData = new List<byte>();
            for (int i = 0; i < sendData.Length; i++)
            {
                var dt = sendData[i];
                for (int j = 0; j < wait[i]; j++)
                    rawSendData.Add(dt);
            }

            var sendBuffer = new Span<byte>(rawSendData.ToArray());
            while (true)
            {
                uint writtenBytes = 0;
                var stat = FtdiPort.Write(sendBuffer.ToArray(), sendBuffer.Length, ref writtenBytes);
                if (stat != FTDI.FT_STATUS.FT_OK)
                    break;
                if (sendBuffer.Length == writtenBytes)
                    break;

                if (abortRequested)
                {
                    abortRequested = false;
                    break;
                }
                sendBuffer = sendBuffer.Slice((int)writtenBytes, (int)(sendBuffer.Length - writtenBytes));
            }
        }

        protected void SendDataBySerial(byte[] sendData, int[] wait)
        {
            List<byte> rawSendData = new List<byte>();
            for (int i = 0; i < sendData.Length; i++)
            {
                var dt = sendData[i];
                for (int j = 0; j < wait[i]; j++)
                    rawSendData.Add(dt);
            }

            var sendBuffer = new Span<byte>(rawSendData.ToArray());
            while (true)
            {
                uint writtenBytes = 0;
                var stat = FtdiPort.Write(sendBuffer.ToArray(), sendBuffer.Length, ref writtenBytes);
                if (stat != FTDI.FT_STATUS.FT_OK)
                    break;
                if (sendBuffer.Length == writtenBytes)
                    break;

                if (abortRequested)
                {
                    abortRequested = false;
                    break;
                }
                sendBuffer = sendBuffer.Slice((int)writtenBytes, (int)(sendBuffer.Length - writtenBytes));
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
            abortRequested = true;
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

}
