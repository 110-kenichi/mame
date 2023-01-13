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

        private bool abortRequested;

        public void Abort()
        {
            abortRequested = true;
        }

        [DllImport("msvcrt.dll", EntryPoint = "memset", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        private static extern IntPtr MemSet(IntPtr dest, int c, int count);

        /*
         *  long before, after;
            byte[] osd = new byte[1024];
            for (int i = 0; i < osd.Length; i++)
                osd[i] = (byte)(i & 0xff);
            int wait = 25;

            QueryPerformanceCounter(out before);
            {
                byte[] sd = new byte[1024 * (int)wait];
                unsafe
                {
                    fixed (byte* bp2 = sd)
                    {
                        byte* bp = bp2;
                        for (int i = 0; i < osd.Length; i++)
                        {
                            var dt = osd[i];
                            for (int j = 0; j < wait; j++)
                                *bp++ = dt;
                        }
                    }
                }
            }
            QueryPerformanceCounter(out after);
            Console.WriteLine(after - before);

            QueryPerformanceCounter(out before);
            {
                byte[] sd = new byte[1024 * (int)25];
                unsafe
                {
                    fixed (byte* bp2 = sd)
                    {
                        byte* bp = bp2;
                        for (int i = 0; i < osd.Length; i++)
                        {
                            var dt = osd[i];
                            MemSet((IntPtr)bp, dt, (int)wait);
                            bp += (int)wait;
                        }
                    }
                }
            }
            QueryPerformanceCounter(out after);
            Console.WriteLine(after - before);
         */

        protected void SendData(byte[] sendData, int wait)
        {
            var rawSendData = new byte[sendData.Length * (int)wait];
            unsafe
            {
                fixed (byte* sdp = rawSendData)
                {
                    byte* tsdp = sdp;
                    foreach (var data in sendData)
                    {
                        for (int j = 0; j < wait; j++)
                            *tsdp++ = data;
                        //MemSet((IntPtr)bp, dt, (int)wait);
                        //tsdp += (int)wait;
                    }
                }
            }

            var sendBuffer = new Span<byte>(rawSendData);
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
