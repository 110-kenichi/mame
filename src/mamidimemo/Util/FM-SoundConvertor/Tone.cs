


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

        public int KSR;
        public int VIB;
        public int EG;
        public int WS;
        public int SSG;


        public Op()
        {
        }


        public Op(Op op)
        {
            AR = op.AR;
            DR = op.DR;
            SR = op.SR;
            RR = op.RR;
            SL = op.SL;
            TL = op.TL;
            KS = op.KS;
            ML = op.ML;
            DT = op.DT;
            DT2 = op.DT2;
            AM = op.AM;

            KSR = op.KSR;
            VIB = op.VIB;
            EG = op.EG;
            WS = op.WS;
            SSG = op.SSG;
        }
    }



    public class Tone
    {
        public string Name;
        public int Number;
        public int FB;
        public int AL;
        public int CNT = -1;
        public Op[] aOp;



        public Tone()
        {
            Name = "";
            Number = -1;
            aOp = new Op[4];
            aOp[0] = new Op();
            aOp[1] = new Op();
            aOp[2] = new Op();
            aOp[3] = new Op();
        }

        public Tone(Tone tone)
        {
            Name = tone.Name;
            Number = tone.Number;
            FB = tone.FB;
            AL = tone.AL;
            CNT = tone.CNT;
            aOp = new Op[4];
            aOp[0] = new Op(tone.aOp[0]);
            aOp[1] = new Op(tone.aOp[1]);
            aOp[2] = new Op(tone.aOp[2]);
            aOp[3] = new Op(tone.aOp[3]);
        }

        public bool IsValid()
        {
            return (Number >= 0x00 && Number <= 0xff && aOp[0].AR > 0 && aOp[1].AR > 0 && aOp[2].AR > 0 && aOp[3].AR > 0);
        }

        public bool IsValid2Op()
        {
            return (Number >= 0x00 && Number <= 0xff && aOp[0].AR > 0 && aOp[1].AR > 0);
        }

        public override string ToString()
        {
            return "(" + Number + ")" + Name;
        }
    }
}
