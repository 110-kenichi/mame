


using System;



namespace FM_SoundConvertor
{
	class Program
	{
		static int Main(string[] aArg)
		{
			var Option = new Option();

			int oArg = 0;
			foreach (var Arg in aArg)
			{
				if (Arg[0] == '-')
				{
					switch (Arg)
					{
						case "-muc":
							{
								Option.bMuc = true;
								break;
							}
						case "-dat":
							{
								Option.bDat = true;
								break;
							}
						case "-fmp":
							{
								Option.bFmp = true;
								break;
							}
						case "-pmd":
							{
								Option.bPmd = true;
								break;
							}
						case "-vopm":
							{
								Option.bVopm = true;
								break;
							}
					}
					aArg[oArg] = String.Empty;
				}
				++oArg;
			}

			Muc.Reader(aArg, Option);
			Dat.Reader(aArg, Option);
			Fmp.Reader(aArg, Option);
			Pmd.Reader(aArg, Option);
			Vopm.Reader(aArg, Option);
			return 0;
		}
	}
}
