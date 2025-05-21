
using System;
using System.IO;

namespace Praat2Lpc
{
    public class PraatLpcReader : IDisposable
    {
        private readonly LineNumberReader reader;
        private bool disposed = false;

        public double Xmin { get; private set; }
        public double Xmax { get; private set; }
        public int FrameCount { get; private set; }
        public double DX { get; private set; }
        public double X1 { get; private set; }
        public double SamplingPeriod { get; private set; }
        public int MaxNCoefficients { get; private set; }
        public int FrameCounter { get; private set; }

        public PraatLpcReader(Stream lpcFile)
        {
            reader = new LineNumberReader(lpcFile);
            String lineText = ReadString();
            if (!lineText.Equals("File type = \"ooTextFile\""))
            {
                throw new IOException("Unexpected header [" + lineText + "] instead of [File type = \"ooTextFile\"] on line " + (reader.LineNumber - 1));
            }
            else
            {
                lineText = ReadString();
                if (!lineText.Equals("Object class = \"LPC 1\""))
                {
                    throw new IOException("Unexpected header [" + lineText + "] instead of [Object class = \"LPC 1\"] on line " + (reader.LineNumber - 1));
                }
                else
                {
                    ReadString();
                    Xmin = ReadDouble();
                    Xmax = ReadDouble();
                    FrameCount = ReadInt();
                    DX = ReadDouble();
                    X1 = ReadDouble();
                    SamplingPeriod = ReadDouble();
                    MaxNCoefficients = ReadInt();
                }
            }
        }

        public PraatLpcFrame? ReadFrame()
        {
            if (this.FrameCounter >= this.FrameCount)
            {
                return null;
            }
            else
            {
                int coefficientCount = this.ReadInt();
                if (coefficientCount >= 0 && coefficientCount <= MaxNCoefficients)
                {
                    double[] lpcCoefficients = new double[coefficientCount];

                    for (int i = 0; i < coefficientCount; ++i)
                    {
                        lpcCoefficients[i] = ReadDouble();
                    }

                    double gain = this.ReadDouble();
                    ++FrameCounter;
                    return new PraatLpcFrame(0.0D, 0.0D, gain, lpcCoefficients);
                }
                else
                {
                    throw new IOException("Unexpected number of coefficients [" + coefficientCount + "] in frame " + this.FrameCount + " on line " + (this.reader.LineNumber - 1));
                }
            }
        }

        public void SkipFrame()
        {
            ReadFrame();
        }

        public void SkipFrames(int frameSkipCount)
        {
            for (int i = 0; i < frameSkipCount; ++i)
            {
                SkipFrame();
            }
        }

        private String ReadString()
        {
            return reader.ReadLine();
        }

        private int ReadInt() => int.Parse(reader.ReadLine());

        private double ReadDouble() => double.Parse(reader.ReadLine());

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    reader?.Dispose();
                }
                disposed = true;
            }
        }
    }
}