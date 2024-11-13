using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using zanac.VGMPlayer.Properties;

namespace zanac.VGMPlayer
{
    /// <summary>
    /// 
    /// </summary>
    public class MGSSong : VGMSong
    {
        private string tmpVgmFile;

        private string ext;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        public MGSSong(string fileName) : base(fileName)
        {
            ext = Path.GetExtension(fileName).ToUpper();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (File.Exists(tmpVgmFile))
                File.Delete(tmpVgmFile);
        }

        //public override int LoopCount {
        //    get
        //    {
        //        return base.LoopCount;
        //    }
        //    set
        //    {
        //        string ext = Path.GetExtension(FileName);
        //        switch (ext.ToUpper())
        //        {
        //            case ".KSS":
        //                base.LoopCount = value;
        //                break;
        //            case ".MGS":
        //                base.LoopCount = 0;
        //                break;
        //        }
        //    }
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        protected override void OpenFile(string fileName)
        {
            string tmpFile = Path.GetTempFileName();
            tmpVgmFile = Path.ChangeExtension(tmpFile, ".VGM");
            File.Move(tmpFile, tmpVgmFile);

            Process p = new Process();

            string loopCount = "";
            if (Program.Default.Loop && Program.Default.LoopCount > 0)
                loopCount = $"-l{(int)Program.Default.LoopCount}";
            string loopTime = "";
            if (Program.Default.LoopTime)
            {
                var ts = new TimeSpan(Program.Default.LoopTimes.Hour, Program.Default.LoopTimes.Minute, Program.Default.LoopTimes.Second);
                loopTime = String.Format($"-p{ts.TotalSeconds}");
            }
            p.StartInfo.FileName = "kss2vgm";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = false;
            p.StartInfo.RedirectStandardInput = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.Arguments = string.Format($"{loopCount} {loopTime} \"-o{tmpVgmFile}\" \"{fileName}\"");
            p.Start();
            p.WaitForExit(1000 * 60);

            base.OpenFile(tmpVgmFile);
        }

    }
}
