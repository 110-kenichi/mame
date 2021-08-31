using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zanac.VGMPlayer
{
    /// <summary>
    /// 
    /// </summary>
    public class KSSSong : VGMSong
    {
        private string tmpVgmFile;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        public KSSSong(string fileName) : base(fileName)
        {
            string tmpFile = Path.GetTempFileName();
            tmpVgmFile = Path.ChangeExtension(tmpFile, ".VGM");
            File.Move(tmpFile, tmpVgmFile);

            Process p = new Process();

            p.StartInfo.FileName = "kss2vgm";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = false;
            p.StartInfo.RedirectStandardInput = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.Arguments = string.Format($"\"-o{tmpVgmFile}\" \"{fileName}\"");
            p.Start();
            p.WaitForExit();

            OpenVGMFile(tmpVgmFile);
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
    }
}
