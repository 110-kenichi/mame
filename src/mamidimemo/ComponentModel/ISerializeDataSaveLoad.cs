using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using zanac.MAmidiMEmo.Gui;

namespace zanac.MAmidiMEmo.ComponentModel
{
    internal interface ISerializeDataSaveLoad
    {

        string SerializeDataSave
        {
            get;
            set;
        }

        string SerializeDataLoad
        {
            get;
            set;
        }

        string SerializeData
        {
            get;
            set;
        }
    }
}
