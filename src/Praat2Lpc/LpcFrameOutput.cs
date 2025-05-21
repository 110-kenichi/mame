using Praat2Lpc;

namespace Praat2Lpc
{
    public abstract class LpcFrameOutput : IDisposable
    {
        public abstract void Dispose();

        public abstract void WriteFrame(LpcFrame lpcFrame);
    }
}
