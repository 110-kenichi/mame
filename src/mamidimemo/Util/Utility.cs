// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zanac.MAmidiMEmo.Util
{
    public static class Utility
    {
        [System.Runtime.InteropServices.DllImport("msvcrt.dll",
          CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern int memcmp(byte[] b1, byte[] b2, UIntPtr count);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool IsEquals(byte[] a, byte[] b)
        {
            if (object.ReferenceEquals(a, b))
                return true;

            if (a == null || b == null || a.Length != b.Length)
                return false;

            return memcmp(a, b, new UIntPtr((uint)a.Length)) == 0;
        }
    }
}
