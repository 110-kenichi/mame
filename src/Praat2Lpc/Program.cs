namespace Praat2Lpc
{
    internal class Program
    {
        /// <summary>
        ///  file name>
        /// </summary>
        /// <param name="args"></param>
        /// <exception cref="IOException"></exception>
        static void Main(string[] args)
        {
            int argc;

            LpcQuantizer? quantizer = null;
            double period = 0.025D;
            double samplePeriod = 1.25E-4D;

            bool endSwitch = false;
            for (argc = 0; argc < args.Length; argc++)
            {
                switch (args[argc])
                {
                    case "-tms5220":
                        quantizer = LpcQuantizer.TMS5220;
                        period = 0.025d;
                        break;
                    case "-vlm5030":
                        quantizer = LpcQuantizer.VLM5030;
                        period = 0.02d;
                        PraatPitchReader.FrameDX = period;
                        break;
                    default:
                        endSwitch = true;
                        break;
                }
                if (endSwitch)
                    break;
            }
            if (quantizer == null)
                throw new IOException("Unknown quantizer [" + quantizer + "]");

            String pitchFile = args[argc++];
            String energyFile = args[argc++];
            double unvoiced_energy_factor = Double.Parse(args[argc++]);
            double voiced_energy_factor = Double.Parse(args[argc++]);
            String outputLpcFile = args[argc++];

            PraatLpcFrameReader lpcFrameReader = new (
                new BufferedStream(File.OpenRead(pitchFile)),
                new BufferedStream(File.OpenRead(energyFile)));

            PraatPitchReader.FrameDX = period;

            double dx = lpcFrameReader.DX;
            if (dx != period)
            {
                throw new IOException("Unexpected frame time [" + dx + "] instead of [" + period + "]");
            }

            double peri = lpcFrameReader.SamplingPeriod;
            if (peri != samplePeriod)
            {
                throw new IOException("Unexpected sampling period [" + peri + "] instead of [" + samplePeriod + "]");
            }

            int var20 = lpcFrameReader.MaxNCoefficients;
            if (var20 < 10)
            {
                throw new IOException("Unexpected maximum number of coefficients [" + var20 + "] instead of [10]");
            }

            LpcFrameOutput? output = null;
            try
            {
                if (quantizer == LpcQuantizer.TMS5220)
                    output = new TsmLpcFrameOutputStream(File.OpenWrite(outputLpcFile));
                else if (quantizer == LpcQuantizer.VLM5030)
                    output = new VlmLpcFrameOutputStream(File.OpenWrite(outputLpcFile));
                if (output == null)
                    throw new IOException("Unknown quantizer [" + quantizer + "]");

                while (true)
                {
                    PraatLpcFrame? praatFrame = lpcFrameReader.ReadFrame();
                    if (praatFrame != null)
                    {
                        LpcFrame frame = CreateLpcFrame(quantizer, unvoiced_energy_factor, voiced_energy_factor, praatFrame.Intensity, praatFrame.Frequency, praatFrame.Gain, praatFrame.PredictorCoefficients);
                        output.WriteFrame(frame);
                        continue;
                    }

                    if (quantizer == LpcQuantizer.TMS5220)
                        output.WriteFrame(new LpcStopFrame());
                    else if (quantizer == LpcQuantizer.VLM5030)
                        output.WriteFrame(new VlmLpcStopFrame());

                    break;
                }
            }
            finally
            {
                output?.Dispose();
            }
        }


        private static LpcFrame CreateLpcFrame(LpcQuantizer quantizer, double unvoiced_energy_factor, double voiced_energy_factor, double intensity, double frequency, double gain, double[]? predictorCoefficients)
        {
            double[] coeffs = GetReflectionCoefficientsFromPredictorCoefficients(predictorCoefficients);
            int energy;
            if (quantizer == LpcQuantizer.VLM5030)
            {
                if (frequency <= 0.0D)
                {
                    energy = quantizer.EncodeEnergy(unvoiced_energy_factor * intensity);
                    return (LpcFrame)(energy == 0 ? new VlmLpcSilenceFrame() : new VlmLpcUnvoicedFrame(energy, quantizer.EncodeLpcCoefficients(coeffs, false)));
                }
                else
                {
                    energy = quantizer.EncodeEnergy(voiced_energy_factor * GetEnergyFromGain(gain));
                    return (LpcFrame)(energy == 0 ? new VlmLpcSilenceFrame() : new VlmLpcVoicedFrame(energy, quantizer.EncodePitch(frequency), quantizer.EncodeLpcCoefficients(coeffs, true)));
                }
            }
            else if (quantizer == LpcQuantizer.TMS5220)
            {
                if (frequency <= 0.0D)
                {
                    energy = quantizer.EncodeEnergy(unvoiced_energy_factor * intensity);
                    return (LpcFrame)(energy == 0 ? new LpcSilenceFrame() : new LpcUnvoicedFrame(energy, quantizer.EncodeLpcCoefficients(coeffs, false)));
                }
                else
                {
                    energy = quantizer.EncodeEnergy(voiced_energy_factor * GetEnergyFromGain(gain));
                    return (LpcFrame)(energy == 0 ? new LpcSilenceFrame() : new LpcVoicedFrame(energy, quantizer.EncodePitch(frequency), quantizer.EncodeLpcCoefficients(coeffs, true)));
                }
            }
            throw new IOException("Unknown quantizer [" + quantizer + "]");
        }

        private static double[] GetReflectionCoefficientsFromPredictorCoefficients(double[]? predictorCoefficients)
        {
            if(predictorCoefficients == null)
                return [];

            int predictorCoeffCount = predictorCoefficients.Length;
            double[] var2 = new double[predictorCoeffCount];
            double[] var3 = new double[predictorCoeffCount];
            double[] var4 = new double[predictorCoeffCount];
            Array.Copy(predictorCoefficients, 0, var3, 0, predictorCoeffCount);

            for (int i = predictorCoeffCount - 1; i >= 0; --i)
            {
                var2[i] = var3[i];
                double var6 = 1.0D - var2[i] * var2[i];
                Array.Copy(var3, 0, var4, 0, i);

                for (int var8 = 0; var8 < i; ++var8)
                {
                    var3[var8] = (var4[var8] - var2[i] * var4[i - var8 - 1]) / var6;
                }
            }

            return var2;
        }

        private static double GetEnergyFromIntensity(double var0)
        {
            return Math.Sqrt(var0);
        }

        private static double GetEnergyFromGain(double var0)
        {
            return Math.Sqrt(var0);
        }
    }
}
