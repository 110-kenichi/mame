using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Praat2Lpc
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="writer"></param>
    internal class VlmLpcFrameOutputStream(FileStream writer) : LpcFrameOutput
    {
        private readonly FileStream writer = writer;
        private long dataBits;
        private int dataBitCount;

        public override void WriteFrame(LpcFrame lpcFrame)
        {
            dataBits = lpcFrame.ToBits();

            for (dataBitCount += lpcFrame.BitCount; dataBitCount >= 8; dataBitCount -= 8)
            {
                writer.WriteByte((byte)dataBits);
                dataBits >>>= 8;
            }
        }

        public override void Dispose()
        {
            if (dataBitCount > 0)
            {
                writer.WriteByte((byte)(dataBits & (1 << dataBitCount) - 1));
            }

            writer.Dispose();
        }
    }
}