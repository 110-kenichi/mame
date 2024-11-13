using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Globalization;
using System.Windows.Forms;
using zanac.VGMPlayer.Properties;
using System.Runtime.InteropServices;

namespace zanac.VGMPlayer
{
    class Program
    {
        [DllImport("ntdll.dll", SetLastError = true)]
        static extern int NtQueryTimerResolution(out int MaximumResolution, out int MinimumResolution, out int CurrentResolution);

        [DllImport("ntdll.dll", SetLastError = true)]
        static extern int NtSetTimerResolution(int DesiredResolution, bool SetResolution, out int CurrentResolution);

        public static int MaximumResolution;
        public static int MinimumResolution;
        public static int CurrentResolution;

        /// <summary>
        /// 
        /// </summary>
        public static dynamic Default
        {
            get;
            private set;
        }

        [STAThread]
        static void Main(string[] args)
        {
            //Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Settings.Default.Reload();

            Default = new SettingsProxy(Settings.Default);

            var w = Default.BitBangWaitAY8910;
            w = Default.BitBangWaitAY8910;

            NtQueryTimerResolution(out MaximumResolution, out MinimumResolution, out CurrentResolution);
            NtSetTimerResolution(MinimumResolution, true, out CurrentResolution);

            Application.Run(new FormMain());

            Settings.Default.Save();
        }
    }
}
