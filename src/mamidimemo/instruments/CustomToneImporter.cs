using FM_SoundConvertor;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zanac.MAmidiMEmo.Instruments
{
    public abstract class CustomToneImporter
    {
        /// <summary>
        /// 
        /// </summary>
        public abstract string ExtensionsFilterExt
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public abstract IEnumerable<Tone> ImportTone(string text);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public abstract IEnumerable<Tone> ImportToneFile(string file);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tones"></param>
        /// <returns></returns>
        public abstract IEnumerable<TimbreBase> ImportToneFileAsTimbre(string file);
    }
}
