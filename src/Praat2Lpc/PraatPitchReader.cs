using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace Praat2Lpc
{
    internal class PraatPitchReader : IDisposable
    {
        public static double FrameDX = 0.025D;
        private const double SamplingPeriod = 1.25E-4D;
        private const int MaxNCoefficients = 10;
        private readonly LineNumberReader reader;

        public double Xmin { get; private set; }
        public double Xmax { get; private set; }
        public int FrameCount { get; private set; }
        public double DX { get; private set; }
        public double X1 { get; private set; }
        public double Ceiling { get; private set; }
        public int MaxNCandidates { get; private set; }
        public int FrameCounter { get; private set; }

        public PraatPitchReader(Stream var1)
        {
            this.reader = new LineNumberReader(var1);
            string lineText = this.ReadString();
            if (!lineText.Equals("File type = \"ooTextFile\""))
            {
                throw new IOException("Unexpected header [" + lineText + "] instead of [File type = \"ooTextFile\"] on line " + (this.reader.LineNumber - 1));
            }
            else
            {
                lineText = this.ReadString();
                if (!lineText.Equals("Object class = \"Pitch 1\""))
                {
                    throw new IOException("Unexpected header [" + lineText + "] instead of [Object class = \"Pitch 1\"] on line " + (reader.LineNumber - 1));
                }
                else
                {
                    ReadString();
                    Xmin = ReadDouble();
                    Xmax = ReadDouble();
                    FrameCount = ReadInt();
                    DX = ReadDouble();
                    X1 = ReadDouble();
                    Ceiling = ReadDouble();
                    MaxNCandidates = ReadInt();
                    if (DX != FrameDX)
                    {
                        double dx = DX;
                        throw new IOException("Unexpected frame time [" + dx + "] instead of [" + FrameDX + "] on line " + (reader.LineNumber - 1));
                    }
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
                double var1 = this.ReadDouble();
                int candidateCount = ReadInt();
                if (candidateCount < 1)
                {
                    throw new IOException("Unexpected number of candidates [" + candidateCount + "] in frame " + FrameCount + " on line " + (reader.LineNumber - 1));
                }
                else
                {
                    double frequency = this.ReadDouble();
                    ReadString();

                    for (int i = 1; i < candidateCount; ++i)
                    {
                        this.ReadString();
                        this.ReadString();
                    }

                    ++FrameCounter;
                    return new PraatLpcFrame(var1, frequency, 0.0D, null);
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
                this.SkipFrame();
            }
        }

        private string ReadString()
        {
            return this.reader.ReadLine();
        }

        private int ReadInt() => int.Parse(this.reader.ReadLine());

        private double ReadDouble() => double.Parse(this.reader.ReadLine());

        public void Dispose()
        {
            this.reader?.Dispose();
        }
    }
}
