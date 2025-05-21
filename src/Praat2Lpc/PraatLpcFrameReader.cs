using System;
using System.IO;

namespace Praat2Lpc
{
    internal class PraatLpcFrameReader : IDisposable
    {
        private readonly PraatPitchReader pitchReader;
        private readonly PraatLpcReader lpcReader;

        public double Xmin
        {
            get
            {
                return this.pitchReader.Xmin;
            }
        }

        public double Xmax
        {
            get
            {
                return this.pitchReader.Xmax;
            }
        }

        public int FrameCount
        {
            get
            {
                return this.pitchReader.FrameCount;
            }
        }

        public double DX
        {
            get
            {
                return this.pitchReader.DX;
            }
        }

        public double X1
        {
            get
            {
                return this.pitchReader.X1;
            }
        }

        public double Ceiling
        {
            get
            {
                return this.pitchReader.Ceiling;
            }
        }

        public int MaxNCandidates
        {
            get
            {
                return this.pitchReader.MaxNCandidates;
            }
        }

        public double SamplingPeriod
        {
            get
            {
                return this.lpcReader.SamplingPeriod;
            }
        }

        public int MaxNCoefficients
        {
            get
            {
                return this.lpcReader.MaxNCoefficients;
            }
        }
        /// <summary>  
        /// Initializes a new instance of the <see cref="PraatLpcFrameReader"/> class.  
        /// </summary>  
        /// <param name="pitchFile">The pitch file stream.</param>  
        /// <param name="lpcFile">The LPC file stream.</param>  
        /// <exception cref="IOException">Thrown when there are mismatches in the file properties.</exception>  
        public PraatLpcFrameReader(Stream pitchFile, Stream lpcFile)
        {
            pitchReader = new PraatPitchReader(pitchFile);
            lpcReader = new PraatLpcReader(lpcFile);

            if (pitchReader.Xmin == lpcReader.Xmin && pitchReader.Xmax == lpcReader.Xmax)
            {
                if (Math.Abs(pitchReader.FrameCount - lpcReader.FrameCount) > 10)
                {
                    throw new IOException($"Very different frame counts for pitches [{pitchReader.FrameCount}] and for LPC coefficients [{lpcReader.FrameCount}]");
                }
                else if (pitchReader.DX != lpcReader.DX)
                {
                    throw new IOException($"Different time steps for pitches [{pitchReader.DX}] and for LPC coefficients [{lpcReader.DX}]");
                }
                else
                {
                    int frameOffset = (int)Math.Round((lpcReader.X1 - pitchReader.X1) / DX);
                    if (frameOffset > 0)
                    {
                        pitchReader.SkipFrames(frameOffset);
                    }
                    else
                    {
                        lpcReader.SkipFrames(frameOffset);
                    }
                }
            }
            else
            {
                throw new IOException($"Different time ranges for pitches [{pitchReader.Xmin},{pitchReader.Xmax}] and for LPC coefficients [{lpcReader.Xmin},{lpcReader.Xmax}]");
            }
        }


        public PraatLpcFrame? ReadFrame()
        {
            PraatLpcFrame? pitchFrame = pitchReader.ReadFrame();
            if (pitchFrame == null)
            {
                return null;
            }

            PraatLpcFrame? lpcFrame = lpcReader.ReadFrame();
            return lpcFrame == null ? null : new PraatLpcFrame(pitchFrame.Intensity, pitchFrame.Frequency, lpcFrame.Gain, lpcFrame.PredictorCoefficients);
        }

        public void Dispose()
        {
            pitchReader?.Dispose();
            lpcReader?.Dispose();
        }
    }
}