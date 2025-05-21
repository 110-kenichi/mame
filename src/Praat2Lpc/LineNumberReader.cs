using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Praat2Lpc
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="reader"></param>
    public class LineNumberReader(Stream reader) : IDisposable
    {
        private readonly StreamReader _reader = new(reader);
        private bool _disposed;

        public int LineNumber { get; private set; } = 0;

        public string ReadLine()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(LineNumberReader));

            var line = _reader.ReadLine();
            if (line != null)
                LineNumber++;
            return line ?? "";
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _reader.Dispose();
                }
                _disposed = true;
            }
        }

        ~LineNumberReader()
        {
            Dispose(false);
        }
    }
}
