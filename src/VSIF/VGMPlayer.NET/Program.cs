using System.Runtime.InteropServices;
using zanac.VGMPlayer;

namespace zanac.VGMPlayer
{
    internal static class Program
    {
        [DllImport("ntdll.dll", SetLastError = true)]
        static extern int NtQueryTimerResolution(out int MaximumResolution, out int MinimumResolution, out int CurrentResolution);

        [DllImport("ntdll.dll", SetLastError = true)]
        static extern int NtSetTimerResolution(int DesiredResolution, bool SetResolution, out int CurrentResolution);

        public static int MaximumResolution;
        public static int MinimumResolution;
        public static int CurrentResolution;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            NtQueryTimerResolution(out MaximumResolution, out MinimumResolution, out CurrentResolution);
            NtSetTimerResolution(MinimumResolution, true, out CurrentResolution);

            Settings.Default.Reload();

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            //ApplicationConfiguration.Initialize();
            Application.Run(new FormMain());

            Settings.Default.Save();
        }
    }
}