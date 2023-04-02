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
using static zanac.MAmidiMEmo.Instruments.Chips.YM3806;

namespace zanac.MAmidiMEmo.Gui.FMEditor
{
    public partial class FormYM3806Editor : FormFmEditor
    {
        private YM3806Timbre timbre;

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
        public FormYM3806Editor()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        public FormYM3806Editor(YM3806 inst, YM3806Timbre timbre, bool singleSelect) : base(inst, timbre, singleSelect)
        {
            this.timbre = timbre;
            InitializeComponent();

            Size = Settings.Default.YM3806EdSize;

            AddControl(new YM3806GeneralContainer(inst, timbre, "General"));

            AddControl(new YM3806OperatorContainer(timbre.Ops[0], "Operator 1"));
            AddControl(new YM3806OperatorContainer(timbre.Ops[1], "Operator 2"));
            AddControl(new YM3806OperatorContainer(timbre.Ops[2], "Operator 3"));
            AddControl(new YM3806OperatorContainer(timbre.Ops[3], "Operator 4"));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            Settings.Default.YM3806EdSize = Size;
        }

        protected override void ApplyTone(Tone tone)
        {
            ((RegisterValue)this["General"]["ALG"]).Value = tone.AL;
            ((RegisterValue)this["General"]["FB"]).Value = tone.FB;
            ((RegisterValue)this["General"]["REV"]).Value = tone.REV;
            ((RegisterValue)this["General"]["AMS"]).Value = tone.AMS;
            ((RegisterValue)this["General"]["PMS"]).Value = tone.PMS;
            ((RegisterValue)this["General"]["PitchShift13"]).Value = tone.PitchShift;
            ((RegisterValue)this["General"]["PitchShift24"]).Value = tone.PitchShift2;
            ((RegisterFlag)this["General"]["GlobalSettings.EN"]).Value = false;
            ((RegisterValue)this["General"]["GlobalSettings.LFOE"]).NullableValue = tone.LFOE;
            ((RegisterValue)this["General"]["GlobalSettings.LFRQ"]).NullableValue = tone.LFRQ;

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
                ((RegisterValue)this["Operator " + (i + 1)]["MUL"]).Value = tone.aOp[i].ML;
                ((RegisterValue)this["Operator " + (i + 1)]["DT1"]).Value = tone.aOp[i].DT;
                ((RegisterValue)this["Operator " + (i + 1)]["AM"]).Value = tone.aOp[i].AM;
                ((RegisterValue)this["Operator " + (i + 1)]["OSCW"]).Value = tone.aOp[i].OSCW;
                ((RegisterValue)this["Operator " + (i + 1)]["LS"]).Value = tone.aOp[i].LS;
                ((RegisterValue)this["Operator " + (i + 1)]["KVS"]).Value = tone.aOp[i].KVS;
            }
            timbre.TimbreName = tone.Name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tone"></param>
        protected override void ApplyTone(TimbreBase timbre, Tone tone)
        {
            if (tone.MML != null)
            {
                ApplyTimbre(timbre);
                ApplyTone(tone);
            }
            else
            {
                YM3806Timbre tim = (YM3806Timbre)timbre;

                tim.ALG = (byte)tone.AL;
                tim.FB = (byte)tone.FB;
                tim.REV = (byte)tone.REV;
                tim.AMS = (byte)tone.AMS;
                tim.PMS = (byte)tone.PMS;
                tim.PitchShift13 = (byte)tone.PitchShift;
                tim.PitchShift24 = (byte)tone.PitchShift2;
                tim.GlobalSettings.Enable = false;
                tim.GlobalSettings.LFOE = (byte?)tone.LFOE;
                tim.GlobalSettings.LFRQ = (byte?)tone.LFRQ;
                if (tim.GlobalSettings.LFRQ > 0 ||
                    tim.GlobalSettings.LFOE > 0
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
                    tim.Ops[i].MUL = (byte)tone.aOp[i].ML;
                    tim.Ops[i].DT1 = (byte)tone.aOp[i].DT;
                    tim.Ops[i].AM = (byte)tone.aOp[i].AM;
                    tim.Ops[i].OSCW = (byte)tone.aOp[i].OSCW;
                    tim.Ops[i].LS = (byte)tone.aOp[i].LS;
                    tim.Ops[i].KVS = (byte)tone.aOp[i].KVS;
                }
                timbre.TimbreName = tone.Name;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tone"></param>
        protected override void ApplyTimbre(TimbreBase timbre)
        {
            YM3806Timbre tim = (YM3806Timbre)timbre;
            this.timbre = tim;

            this["General"].Target = tim;
            ((RegisterValue)this["General"]["ALG"]).Value = tim.ALG;
            ((RegisterValue)this["General"]["FB"]).Value = tim.FB;
            ((RegisterValue)this["General"]["REV"]).Value = tim.REV;
            ((RegisterValue)this["General"]["AMS"]).Value = tim.AMS;
            ((RegisterValue)this["General"]["PMS"]).Value = tim.PMS;
            ((RegisterValue)this["General"]["PitchShift13"]).Value = tim.PitchShift13;
            ((RegisterValue)this["General"]["PitchShift24"]).Value = tim.PitchShift24;
            ((RegisterFlag)this["General"]["GlobalSettings.EN"]).Value = tim.GlobalSettings.Enable;
            ((RegisterValue)this["General"]["GlobalSettings.LFOE"]).NullableValue = tim.GlobalSettings.LFOE;
            ((RegisterValue)this["General"]["GlobalSettings.LFRQ"]).NullableValue = tim.GlobalSettings.LFRQ;
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
                ((RegisterValue)this["Operator " + (i + 1)]["MUL"]).Value = tim.Ops[i].MUL;
                ((RegisterValue)this["Operator " + (i + 1)]["DT1"]).Value = tim.Ops[i].DT1;
                ((RegisterValue)this["Operator " + (i + 1)]["AM"]).Value = tim.Ops[i].AM;
                ((RegisterValue)this["Operator " + (i + 1)]["OSCW"]).Value = tim.Ops[i].OSCW;
                ((RegisterValue)this["Operator " + (i + 1)]["LS"]).Value = tim.Ops[i].LS;
                ((RegisterValue)this["Operator " + (i + 1)]["KVS"]).Value = tim.Ops[i].KVS;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override string ExtensionsFilterExt
        {
            get
            {
                return "*.mopq";
            }
        }

        protected override string[] GetMMlValues()
        {
            return new string[] { Timbre.TimbreName, MmlValueGeneral, MmlValueOps[0], MmlValueOps[1], MmlValueOps[2], MmlValueOps[3] };

        }

    }

}
