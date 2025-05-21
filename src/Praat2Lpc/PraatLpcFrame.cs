namespace Praat2Lpc
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="intensity"></param>
    /// <param name="frequency"></param>
    /// <param name="gain"></param>
    /// <param name="predictorCoefficients"></param>
    public class PraatLpcFrame(double intensity, double frequency, double gain, double[]? predictorCoefficients)
    {
        public double Intensity
        {
            get;
            private set;
        } = intensity;
        public double Frequency
        {
            get;
            private set;
        } = frequency;
        public double Gain
        {
            get;
            private set;
        } = gain;
        public double[]? PredictorCoefficients
        {
            get;
            private set;
        } = predictorCoefficients;
    }
}