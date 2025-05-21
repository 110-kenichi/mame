using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Praat2Lpc
{
    internal class TsmLpcFrameOutputStream(FileStream writer) : LpcFrameOutput
    {
        private readonly FileStream writer = writer;
        private long dataBits;
        private int dataBitCount;
        private LpcFrame? previousFrame;

        public override void WriteFrame(LpcFrame lpcFrame)
        {
            if (lpcFrame is not LpcRepeatFrame)
            {
                if (previousFrame != null && ((LpcFrame)lpcFrame).GetType() == previousFrame.GetType())
                {
                    if (lpcFrame is LpcUnvoicedFrame uFrame && uFrame.k == ((LpcUnvoicedFrame)previousFrame).k)
                    {
                        lpcFrame = new LpcRepeatFrame(((LpcEnergyFrame)lpcFrame).Energy, 0);
                    }
                    else if (lpcFrame is LpcVoicedFrame vFrame && vFrame.k == ((LpcVoicedFrame)previousFrame).k)
                    {
                        lpcFrame = new LpcRepeatFrame(((LpcEnergyFrame)lpcFrame).Energy, vFrame.Pitch);
                    }
                    else
                    {
                        previousFrame = (LpcFrame)lpcFrame;
                    }
                }
                else
                {
                    previousFrame = (LpcFrame)lpcFrame;
                }
            }

            dataBits |= lpcFrame.ToReversedBits() << dataBitCount;

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

            writer?.Dispose();
        }
    }
}
