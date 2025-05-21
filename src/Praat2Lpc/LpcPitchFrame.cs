namespace Praat2Lpc
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="lpcEnergy"></param>
    /// <param name="pitch"></param>
    public abstract class LpcPitchFrame(int lpcEnergy, int pitch) : LpcEnergyFrame(lpcEnergy)
    {
        public int Pitch
        {
            get;
            private set;
        } = pitch;

        public override LpcFrame Clone()
        {
            return (LpcPitchFrame)MemberwiseClone();
        }
    }
}