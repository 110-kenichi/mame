


namespace FM_SoundConvertor
{
    public class Op
	{
		public int AR;
		public int DR;
		public int SR;
		public int RR;
		public int SL;
		public int TL;
		public int KS;
		public int ML;
		public int DT;
		public int DT2;
		public int AM;



		public Op()
		{
			AR = 0;
			DR = 0;
			SR = 0;
			RR = 0;
			SL = 0;
			TL = 0;
			KS = 0;
			ML = 0;
			DT = 0;
			DT2 = 0;
			AM = 0;
		}
	}



	public class Tone
	{
		public string Name;
		public int Number;
		public int FB;
		public int AL;
		public Op[] aOp;



		public Tone()
		{
			Name = "";
			Number = -1;
			FB = 0;
			AL = 0;
			aOp = new Op[4];
			aOp[0] = new Op();
			aOp[1] = new Op();
			aOp[2] = new Op();
			aOp[3] = new Op();
		}



		public bool IsValid()
		{
			return (Number >= 0x00 && Number <= 0xff && aOp[0].AR > 0 && aOp[1].AR > 0 && aOp[2].AR > 0 && aOp[3].AR > 0);
		}
	}
}
