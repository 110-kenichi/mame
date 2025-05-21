namespace Praat2Lpc
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="energy"></param>
    public abstract class LpcEnergyFrame(int energy) : LpcFrame
    {
        public int Energy
        {
            get;
            private set;
        } = energy;

        public override LpcFrame Clone()
        {
            return (LpcEnergyFrame)MemberwiseClone();
        }
    }
}