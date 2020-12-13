using System;
using System.Linq;

namespace Kermalis.SoundFont2
{
    static class SF2Utils
    {
        public static void TruncateOrNot(string str, int length, ref char[] toArray)
        {
            toArray = new char[length];
            var strAsChars = str.ToCharArray().Take(length).ToArray();
            Buffer.BlockCopy(strAsChars, 0, toArray, 0, strAsChars.Length * 2);
        }
    }
}
