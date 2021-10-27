// copyright-holders:K.Ito
using FM_SoundConvertor;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using zanac.MAmidiMEmo.Instruments;
using zanac.MAmidiMEmo.Instruments.Chips;
using zanac.MAmidiMEmo.Midi;
using zanac.MAmidiMEmo.Properties;
using static zanac.MAmidiMEmo.Instruments.Chips.YM2414;

namespace zanac.MAmidiMEmo.Gui.FMEditor
{
    public partial class FormYM2414Editor : FormFmEditor
    {
        private YM2414Timbre timbre;

        /// <summary>
        /// 
        /// </summary>
        public string MmlValueGeneral
        {
            get
            {
                return GetControl("General").SerializeData;
            }
            set
            {
                GetControl("General").SerializeData = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string[] MmlValueOps
        {
            get
            {
                List<string> list = new List<string>();
                for (int i = 0; i < timbre.Ops.Length; i++)
                {
                    string opt = GetControl("Operator " + (i + 1)).SerializeData;
                    list.Add(opt);
                }
                return list.ToArray();
            }
            set
            {
                for (int i = 0; i < timbre.Ops.Length; i++)
                {
                    GetControl("Operator " + (i + 1)).SerializeData = value[i];
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public FormYM2414Editor()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        public FormYM2414Editor(YM2414 inst, YM2414Timbre timbre, bool singleSelect) : base(inst, timbre, singleSelect)
        {
            this.timbre = timbre;
            InitializeComponent();

            Size = Settings.Default.YM2414EdSize;

            AddControl(new YM2414GeneralContainer(inst, timbre, "General"));

            AddControl(new YM2414OperatorContainer(timbre.Ops[0], "Operator 1"));
            AddControl(new YM2414OperatorContainer(timbre.Ops[1], "Operator 2"));
            AddControl(new YM2414OperatorContainer(timbre.Ops[2], "Operator 3"));
            AddControl(new YM2414OperatorContainer(timbre.Ops[3], "Operator 4"));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            Settings.Default.YM2414EdSize = Size;
        }

        protected override void ApplyTone(Tone tone)
        {
            ((RegisterValue)this["General"]["ALG"]).Value = tone.AL;
            ((RegisterValue)this["General"]["FB"]).Value = tone.FB;
            ((RegisterValue)this["General"]["AMSF"]).Value = tone.AMSF;
            ((RegisterValue)this["General"]["AMS"]).Value = tone.AMS;
            ((RegisterValue)this["General"]["PMSF"]).Value = tone.PMSF;
            ((RegisterValue)this["General"]["PMS"]).Value = tone.PMS;

            ((RegisterValue)this["General"]["GlobalSettings.LFRQ"]).NullableValue = tone.LFRQ;
            ((RegisterValue)this["General"]["GlobalSettings.LFRQ2"]).NullableValue = tone.LFRQ2;
            ((RegisterValue)this["General"]["GlobalSettings.LFOF"]).NullableValue = tone.LFOF;
            ((RegisterValue)this["General"]["GlobalSettings.LFOD"]).NullableValue = tone.LFOD;
            ((RegisterValue)this["General"]["GlobalSettings.LFOF2"]).NullableValue = tone.LFOF2;
            ((RegisterValue)this["General"]["GlobalSettings.LFOD2"]).NullableValue = tone.LFOD2;
            ((RegisterValue)this["General"]["GlobalSettings.LFOW"]).NullableValue = tone.LFOW;
            ((RegisterValue)this["General"]["GlobalSettings.LFOW2"]).NullableValue = tone.LFOW2;

            ((RegisterValue)this["General"]["GlobalSettings.SYNC"]).NullableValue = tone.SY;
            ((RegisterValue)this["General"]["GlobalSettings.SYNC2"]).NullableValue = tone.SY2;

            ((RegisterValue)this["General"]["GlobalSettings.NE"]).NullableValue = tone.NE;
            ((RegisterValue)this["General"]["GlobalSettings.NFRQ"]).NullableValue = tone.NF;

            if (tone.NE > 0 ||
               tone.LFRQ > 0 ||
               tone.LFRQ2 > 0 ||
               tone.LFOW > 0 ||
               tone.LFOW2 > 0 ||
               tone.LFOD > 0 ||
               tone.LFOD2 > 0
               )
                ((RegisterFlag)this["General"]["GlobalSettings.EN"]).Value = true;

            for (int i = 0; i < 4; i++)
            {
                ((RegisterFlag)this["Operator " + (i + 1)]["EN"]).Value = true;
                ((RegisterValue)this["Operator " + (i + 1)]["AR"]).Value = tone.aOp[i].AR;
                ((RegisterValue)this["Operator " + (i + 1)]["D1R"]).Value = tone.aOp[i].DR;
                ((RegisterValue)this["Operator " + (i + 1)]["D2R"]).Value = tone.aOp[i].SR < 0 ? (byte)0 : (byte)tone.aOp[i].SR;
                ((RegisterValue)this["Operator " + (i + 1)]["RR"]).Value = tone.aOp[i].RR;
                ((RegisterValue)this["Operator " + (i + 1)]["SL"]).Value = tone.aOp[i].SL;
                ((RegisterValue)this["Operator " + (i + 1)]["TL"]).Value = tone.aOp[i].TL;
                ((RegisterValue)this["Operator " + (i + 1)]["RS"]).Value = tone.aOp[i].KS;
                ((RegisterValue)this["Operator " + (i + 1)]["FIX"]).Value = tone.aOp[i].FIX;
                ((RegisterValue)this["Operator " + (i + 1)]["OSCF"]).Value = tone.aOp[i].OSCF;
                ((RegisterValue)this["Operator " + (i + 1)]["MUL"]).Value = tone.aOp[i].ML;
                ((RegisterValue)this["Operator " + (i + 1)]["DT1"]).Value = tone.aOp[i].DT;
                ((RegisterValue)this["Operator " + (i + 1)]["AM"]).Value = tone.aOp[i].AM;
                ((RegisterValue)this["Operator " + (i + 1)]["EGSF"]).Value = tone.aOp[i].EGSF;
                ((RegisterValue)this["Operator " + (i + 1)]["DT2"]).Value = tone.aOp[i].DT2;
            }
            timbre.TimbreName = tone.Name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tone"></param>
        protected override void ApplyTone(TimbreBase timbre, Tone tone)
        {
            YM2414Timbre tim = (YM2414Timbre)timbre;

            tim.ALG = (byte)tone.AL;
            tim.FB = (byte)tone.FB;
            tim.AMSF = (byte)tone.AMSF;
            tim.AMS = (byte)tone.AMS;
            tim.PMSF = (byte)tone.PMSF;
            tim.PMS = (byte)tone.PMS;

            tim.GlobalSettings.NE = (byte?)tone.NE;
            tim.GlobalSettings.NFRQ = (byte?)tone.NF;

            tim.GlobalSettings.LFRQ = (byte?)tone.LFRQ;
            tim.GlobalSettings.LFRQ2 = (byte?)tone.LFRQ2;
            tim.GlobalSettings.LFOF = (byte?)tone.LFOF;
            tim.GlobalSettings.LFOD = (byte?)tone.LFOD;
            tim.GlobalSettings.LFOF2 = (byte?)tone.LFOF2;
            tim.GlobalSettings.LFOD2 = (byte?)tone.LFOD2;
            tim.GlobalSettings.LFOW = (byte?)tone.LFOW;
            tim.GlobalSettings.LFOW2 = (byte?)tone.LFOW2;

            tim.GlobalSettings.SYNC = (byte?)tone.SY;
            tim.GlobalSettings.SYNC2 = (byte?)tone.SY2;

            if (tim.GlobalSettings.NE > 0 ||
                tim.GlobalSettings.LFRQ > 0 ||
                tim.GlobalSettings.LFRQ2 > 0 ||
                tim.GlobalSettings.LFOW > 0 ||
                tim.GlobalSettings.LFOW2 > 0 ||
                tim.GlobalSettings.LFOD > 0 ||
                tim.GlobalSettings.LFOD2 > 0
                )
                tim.GlobalSettings.Enable = true;

            for (int i = 0; i < 4; i++)
            {
                tim.Ops[i].Enable = 1;
                tim.Ops[i].AR = (byte)tone.aOp[i].AR;
                tim.Ops[i].D1R = (byte)tone.aOp[i].DR;
                tim.Ops[i].D2R = tone.aOp[i].SR < 0 ? (byte)0 : (byte)tone.aOp[i].SR;
                tim.Ops[i].RR = (byte)tone.aOp[i].RR;
                tim.Ops[i].SL = (byte)tone.aOp[i].SL;
                tim.Ops[i].TL = (byte)tone.aOp[i].TL;
                tim.Ops[i].RS = (byte)tone.aOp[i].KS;
                tim.Ops[i].FIX = (byte)tone.aOp[i].FIX;
                tim.Ops[i].OSCF = (byte)tone.aOp[i].OSCF;
                tim.Ops[i].MUL = (byte)tone.aOp[i].ML;
                tim.Ops[i].DT1 = (byte)tone.aOp[i].DT;
                tim.Ops[i].AM = (byte)tone.aOp[i].AM;
                tim.Ops[i].EGSF = (byte)tone.aOp[i].EGSF;
                tim.Ops[i].DT2 = (byte)tone.aOp[i].DT2;
            }
            timbre.TimbreName = tone.Name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tone"></param>
        protected override void ApplyTimbre(TimbreBase timbre)
        {
            YM2414Timbre tim = (YM2414Timbre)timbre;
            this.timbre = tim;

            this["General"].Target = tim;
            ((RegisterValue)this["General"]["ALG"]).Value = tim.ALG;
            ((RegisterValue)this["General"]["FB"]).Value = tim.FB;
            ((RegisterValue)this["General"]["AMSF"]).Value = tim.AMSF == 0 ? 0 : 1;
            ((RegisterValue)this["General"]["AMS"]).Value = tim.AMS;
            ((RegisterValue)this["General"]["PMSF"]).Value = tim.PMSF == 0 ? 0 : 1;
            ((RegisterValue)this["General"]["PMS"]).Value = tim.PMS;
            ((RegisterFlag)this["General"]["GlobalSettings.EN"]).Value = tim.GlobalSettings.Enable;
            ((RegisterValue)this["General"]["GlobalSettings.LFRQ"]).NullableValue = tim.GlobalSettings.LFRQ;
            ((RegisterValue)this["General"]["GlobalSettings.LFRQ2"]).NullableValue = tim.GlobalSettings.LFRQ2;

            ((RegisterValue)this["General"]["GlobalSettings.LFOF"]).NullableValue = tim.GlobalSettings.LFOF;
            ((RegisterValue)this["General"]["GlobalSettings.LFOD"]).NullableValue = tim.GlobalSettings.LFOD;
            ((RegisterValue)this["General"]["GlobalSettings.LFOF2"]).NullableValue = tim.GlobalSettings.LFOF2;
            ((RegisterValue)this["General"]["GlobalSettings.LFOD2"]).NullableValue = tim.GlobalSettings.LFOD2;

            ((RegisterValue)this["General"]["GlobalSettings.LFOW"]).NullableValue = tim.GlobalSettings.LFOW;
            ((RegisterValue)this["General"]["GlobalSettings.LFOW2"]).NullableValue = tim.GlobalSettings.LFOW2;

            ((RegisterValue)this["General"]["GlobalSettings.SYNC"]).NullableValue = tim.GlobalSettings.SYNC;
            ((RegisterValue)this["General"]["GlobalSettings.SYNC2"]).NullableValue = tim.GlobalSettings.SYNC2;

            for (int i = 0; i < 4; i++)
            {
                this["Operator " + (i + 1)].Target = tim.Ops[i];
                ((RegisterFlag)this["Operator " + (i + 1)]["EN"]).Value = tim.Ops[i].Enable == 0 ? false : true;
                ((RegisterValue)this["Operator " + (i + 1)]["AR"]).Value = tim.Ops[i].AR;
                ((RegisterValue)this["Operator " + (i + 1)]["D1R"]).Value = tim.Ops[i].D1R;
                ((RegisterValue)this["Operator " + (i + 1)]["D2R"]).Value = tim.Ops[i].D2R;
                ((RegisterValue)this["Operator " + (i + 1)]["RR"]).Value = tim.Ops[i].RR;
                ((RegisterValue)this["Operator " + (i + 1)]["SL"]).Value = tim.Ops[i].SL;
                ((RegisterValue)this["Operator " + (i + 1)]["TL"]).Value = tim.Ops[i].TL;
                ((RegisterValue)this["Operator " + (i + 1)]["RS"]).Value = tim.Ops[i].RS;
                ((RegisterValue)this["Operator " + (i + 1)]["FIX"]).Value = tim.Ops[i].FIX;
                ((RegisterValue)this["Operator " + (i + 1)]["OSCF"]).Value = tim.Ops[i].OSCF;
                ((RegisterValue)this["Operator " + (i + 1)]["MUL"]).Value = tim.Ops[i].MUL;
                ((RegisterValue)this["Operator " + (i + 1)]["DT1"]).Value = tim.Ops[i].DT1;
                ((RegisterValue)this["Operator " + (i + 1)]["AM"]).Value = tim.Ops[i].AM;
                ((RegisterValue)this["Operator " + (i + 1)]["EGSF"]).Value = tim.Ops[i].EGSF;
                ((RegisterValue)this["Operator " + (i + 1)]["DT2"]).Value = tim.Ops[i].DT2;
            }
        }
    }

}
