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
using static zanac.MAmidiMEmo.Instruments.Chips.YM3812;

namespace zanac.MAmidiMEmo.Gui.FMEditor
{
    public partial class FormYM3812Editor : FormFmEditor
    {
        private YM3812Timbre timbre;

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
        public FormYM3812Editor()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        public FormYM3812Editor(YM3812 inst, YM3812Timbre timbre, bool singleSelect) : base(inst, timbre, singleSelect)
        {
            this.timbre = timbre;
            InitializeComponent();

            Size = Settings.Default.YM3812EdSize;

            AddControl(new YM3812GeneralContainer(inst, timbre, "General"));

            AddControl(new YM3812OperatorContainer(timbre.Ops[0], "Operator 1"));
            AddControl(new YM3812OperatorContainer(timbre.Ops[1], "Operator 2"));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            Settings.Default.YM3812EdSize = Size;
        }


        protected override void ApplyTone(Tone tone)
        {
            ((RegisterValue)this["General"]["ALG"]).Value = tone.AL;
            ((RegisterValue)this["General"]["FB"]).Value = tone.FB;
            ((RegisterFlag)this["General"]["GlobalSettings.EN"]).Value = false;
            ((RegisterValue)this["General"]["GlobalSettings.AMD"]).NullableValue = null;
            ((RegisterValue)this["General"]["GlobalSettings.VIB"]).NullableValue = null;

            for (int i = 0; i < 2; i++)
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
                ((RegisterValue)this["Operator " + (i + 1)]["VR"]).Value = tone.aOp[i].VIB;
                ((RegisterValue)this["Operator " + (i + 1)]["EG"]).Value = tone.aOp[i].EG;
                ((RegisterValue)this["Operator " + (i + 1)]["WS"]).Value = tone.aOp[i].WS;
            }
            timbre.TimbreName = tone.Name;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="tone"></param>
        protected override void ApplyTone(TimbreBase timbre, Tone tone)
        {
            YM3812Timbre tim = (YM3812Timbre)timbre;

            tim.ALG = (byte)tone.AL;
            tim.FB = (byte)tone.FB;
            tim.GlobalSettings.Enable = false;
            tim.GlobalSettings.AMD = null;
            tim.GlobalSettings.VIB = null;

            for (int i = 0; i < 2; i++)
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
                tim.Ops[i].VR = 0;
                tim.Ops[i].EG = 0;
                tim.Ops[i].WS = 0;
            }
            timbre.TimbreName = tone.Name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tone"></param>
        protected override void ApplyTimbre(TimbreBase timbre)
        {
            YM3812Timbre tim = (YM3812Timbre)timbre;
            this.timbre = tim;

            this["General"].Target = tim;
            ((RegisterValue)this["General"]["ALG"]).Value = tim.ALG;
            ((RegisterValue)this["General"]["FB"]).Value = tim.FB;
            ((RegisterFlag)this["General"]["GlobalSettings.EN"]).Value = tim.GlobalSettings.Enable;
            ((RegisterValue)this["General"]["GlobalSettings.AMD"]).NullableValue = tim.GlobalSettings.AMD;
            ((RegisterValue)this["General"]["GlobalSettings.VIB"]).NullableValue = tim.GlobalSettings.VIB;
            for (int i = 0; i < 2; i++)
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
                ((RegisterValue)this["Operator " + (i + 1)]["AM"]).Value = tim.Ops[i].AM;
                ((RegisterValue)this["Operator " + (i + 1)]["VR"]).Value = tim.Ops[i].VR;
                ((RegisterValue)this["Operator " + (i + 1)]["EG"]).Value = tim.Ops[i].EG;
                ((RegisterValue)this["Operator " + (i + 1)]["WS"]).Value = tim.Ops[i].WS;
            }
        }
    }

}
