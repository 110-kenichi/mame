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
using static zanac.MAmidiMEmo.Instruments.Chips.YMF262;

namespace zanac.MAmidiMEmo.Gui.FMEditor
{
    public partial class FormYMF262Editor : FormFmEditor
    {
        private YMF262Timbre timbre;

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
                    var op = timbre.Ops[i];
                    string opt = GetControl("Operator " + (i + 1)).SerializeData;
                    list.Add(opt);
                }
                return list.ToArray();
            }
            set
            {
                for (int i = 0; i < timbre.Ops.Length; i++)
                {
                    var op = timbre.Ops[i];
                    GetControl("Operator " + (i + 1)).SerializeData = value[i];
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public FormYMF262Editor()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        public FormYMF262Editor(YMF262 inst, YMF262Timbre timbre, bool singleSelect) : base(inst, timbre, singleSelect)
        {
            this.timbre = timbre;
            InitializeComponent();

            Size = Settings.Default.YMF262EdSize;

            var gen = new YMF262GeneralContainer(inst, timbre, "General");
            AddControl(gen);

            AddControl(new YMF262OperatorContainer(timbre.Ops[0], null, "Operator 1"));
            AddControl(new YMF262OperatorContainer(timbre.Ops[1], null, "Operator 2"));
            AddControl(new YMF262OperatorContainer(timbre.Ops[2], gen, "Operator 3"));
            AddControl(new YMF262OperatorContainer(timbre.Ops[3], gen, "Operator 4"));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            Settings.Default.YMF262EdSize = Size;
        }

        protected override void ApplyTone(Tone tone)
        {
            int alg = 0;
            if (tone.CNT == -1)
            {
                switch (tone.AL)
                {
                    case 0:
                        alg = 2;
                        break;
                    case 1:
                        alg = 4;    //?
                        break;
                    case 2:
                        alg = 4;    //?
                        break;
                    case 3:
                        alg = 3;    //?
                        break;
                    case 4:
                        alg = 3;
                        break;
                    case 5:
                        alg = 5;    //?
                        break;
                    case 6:
                        alg = 5;    //?
                        break;
                    case 7:
                        alg = 5;
                        break;
                }
            }
            else
            {
                alg = tone.CNT;
            }
            ((RegisterValue)this["General"]["ALG"]).Value = alg;
            ((RegisterValue)this["General"]["FB"]).Value = tone.FB;
            ((RegisterValue)this["General"]["FB2"]).Value = tone.FB;
            ((RegisterFlag)this["General"]["GlobalSettings.EN"]).Value = false;
            ((RegisterValue)this["General"]["GlobalSettings.DAM"]).NullableValue = null;
            ((RegisterValue)this["General"]["GlobalSettings.DVB"]).NullableValue = null;

            for (int i = 0; i < 4; i++)
            {
                ((RegisterValue)this["Operator " + (i + 1)]["AR"]).Value = tone.aOp[i].AR / 2;
                ((RegisterValue)this["Operator " + (i + 1)]["DR"]).Value = tone.aOp[i].DR / 2;
                ((RegisterValue)this["Operator " + (i + 1)]["RR"]).Value = tone.aOp[i].RR;
                ((RegisterValue)this["Operator " + (i + 1)]["SL"]).Value = tone.aOp[i].SL;
                ((RegisterValue)this["Operator " + (i + 1)]["SR"]).Value = tone.aOp[i].SR / 2;
                ((RegisterValue)this["Operator " + (i + 1)]["TL"]).Value = tone.aOp[i].TL / 2;
                ((RegisterValue)this["Operator " + (i + 1)]["KSL"]).Value = tone.aOp[i].KS;
                ((RegisterValue)this["Operator " + (i + 1)]["KSR"]).Value = tone.aOp[i].KSR;
                ((RegisterValue)this["Operator " + (i + 1)]["MFM"]).Value = tone.aOp[i].ML;
                ((RegisterValue)this["Operator " + (i + 1)]["AM"]).Value = tone.aOp[i].AM;
                ((RegisterValue)this["Operator " + (i + 1)]["VIB"]).Value = tone.aOp[i].VIB;
                ((RegisterValue)this["Operator " + (i + 1)]["EG"]).Value = tone.aOp[i].EG;
                ((RegisterValue)this["Operator " + (i + 1)]["WS"]).Value = tone.aOp[i].WS;
            }
            timbre.Memo = tone.Name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tone"></param>
        protected override void ApplyTone(TimbreBase timbre, Tone tone)
        {
            YMF262Timbre tim = (YMF262Timbre)timbre;

            tim.ALG = (byte)tone.AL;
            tim.FB = (byte)tone.FB;
            tim.FB2 = (byte)tone.FB;
            tim.GlobalSettings.Enable = false;
            tim.GlobalSettings.DAM = null;
            tim.GlobalSettings.DVB = null;

            for (int i = 0; i < 3; i++)
            {
                tim.Ops[i].AR = (byte)(tone.aOp[i].AR / 2);
                tim.Ops[i].DR = (byte)(tone.aOp[i].DR / 2);
                tim.Ops[i].RR = (byte)tone.aOp[i].RR;
                tim.Ops[i].SL = (byte)tone.aOp[i].SL;
                tim.Ops[i].SR = (byte)(tone.aOp[i].SR / 2);
                tim.Ops[i].TL = (byte)(tone.aOp[i].TL / 2);
                tim.Ops[i].KSL = (byte)tone.aOp[i].KS;
                tim.Ops[i].KSR = 0;
                tim.Ops[i].MFM = (byte)tone.aOp[i].ML;
                tim.Ops[i].AM = (byte)tone.aOp[i].AM;
                tim.Ops[i].VIB = 0;
                tim.Ops[i].EG = 0;
                tim.Ops[i].WS = 0;
            }
            timbre.Memo = tone.Name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tone"></param>
        protected override void ApplyTimbre(TimbreBase timbre)
        {
            YMF262Timbre tim = (YMF262Timbre)timbre;
            this.timbre = tim;

            this["General"].Target = tim;
            ((RegisterValue)this["General"]["ALG"]).Value = tim.ALG;
            ((RegisterValue)this["General"]["FB"]).Value = tim.FB;
            ((RegisterFlag)this["General"]["GlobalSettings.EN"]).Value = tim.GlobalSettings.Enable;
            ((RegisterValue)this["General"]["GlobalSettings.DAM"]).NullableValue = tim.GlobalSettings.DAM;
            ((RegisterValue)this["General"]["GlobalSettings.DVB"]).NullableValue = tim.GlobalSettings.DVB;
            for (int i = 0; i < 4; i++)
            {
                this["Operator " + (i + 1)].Target = tim.Ops[i];
                ((RegisterValue)this["Operator " + (i + 1)]["AR"]).Value = tim.Ops[i].AR;
                ((RegisterValue)this["Operator " + (i + 1)]["DR"]).Value = tim.Ops[i].DR;
                ((RegisterValue)this["Operator " + (i + 1)]["RR"]).Value = tim.Ops[i].RR;
                ((RegisterValue)this["Operator " + (i + 1)]["SL"]).Value = tim.Ops[i].SL;
                ((RegisterValue)this["Operator " + (i + 1)]["SR"]).NullableValue = tim.Ops[i].SR;
                ((RegisterValue)this["Operator " + (i + 1)]["TL"]).Value = tim.Ops[i].TL;
                ((RegisterValue)this["Operator " + (i + 1)]["KSL"]).Value = tim.Ops[i].KSL;
                ((RegisterValue)this["Operator " + (i + 1)]["KSR"]).Value = tim.Ops[i].KSR;
                ((RegisterValue)this["Operator " + (i + 1)]["MFM"]).Value = tim.Ops[i].MFM;
                ((RegisterValue)this["Operator " + (i + 1)]["VIB"]).Value = tim.Ops[i].VIB;
                ((RegisterValue)this["Operator " + (i + 1)]["EG"]).Value = tim.Ops[i].EG;
                ((RegisterValue)this["Operator " + (i + 1)]["WS"]).Value = tim.Ops[i].WS;
            }
        }
    }

}
