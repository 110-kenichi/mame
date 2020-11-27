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
using static zanac.MAmidiMEmo.Instruments.Chips.YM2413;

namespace zanac.MAmidiMEmo.Gui.FMEditor
{
    public partial class FormYM2413Editor : FormFmEditor
    {
        private YM2413Timbre timbre;

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

                var mod = timbre.Modulator;
                string modt = GetControl("Modulator").SerializeData;
                list.Add(modt);

                var ca = timbre.Career;
                string cat = GetControl("Career").SerializeData;
                list.Add(cat);

                return list.ToArray();
            }
            set
            {
                var mod = timbre.Modulator;
                GetControl("Modulator").SerializeData = value[0];

                var ca = timbre.Career;
                GetControl("Career").SerializeData = value[1];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public FormYM2413Editor()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        public FormYM2413Editor(YM2413 inst, YM2413Timbre timbre, bool singleSelect) : base(inst, timbre, singleSelect)
        {
            this.timbre = timbre;
            InitializeComponent();

            Size = Settings.Default.YM2413EdSize;

            AddControl(new YM2413GeneralContainer(inst, timbre, "General"));

            AddControl(new YM2413OperatorContainer(timbre.Modulator, "Modulator"));
            AddControl(new YM2413OperatorContainer(timbre.Career, "Career"));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            Settings.Default.YM2413EdSize = Size;
        }


        protected override void ApplyTone(Tone tone)
        {
            ((RegisterValue)this["General"]["FB"]).Value = tone.FB;
            ((RegisterValue)this["General"]["SUS"]).Value = 0;

            ((RegisterValue)this["Modulator"]["AR"]).Value = tone.aOp[0].AR / 2;
            ((RegisterValue)this["Modulator"]["DR"]).Value = tone.aOp[0].DR / 2;
            ((RegisterValue)this["Modulator"]["RR"]).Value = tone.aOp[0].RR;
            ((RegisterValue)this["Modulator"]["SL"]).Value = tone.aOp[0].SL;
            ((RegisterValue)this["Modulator"]["SR"]).Value = tone.aOp[0].SR / 2;
            ((RegisterValue)this["Modulator"]["TL"]).Value = tone.aOp[0].TL / 2;
            ((RegisterValue)this["Modulator"]["KSL"]).Value = tone.aOp[0].KS;
            ((RegisterValue)this["Modulator"]["KSR"]).Value = 0;
            ((RegisterValue)this["Modulator"]["MUL"]).Value = tone.aOp[0].ML;
            ((RegisterValue)this["Modulator"]["AM"]).Value = tone.aOp[0].AM;
            ((RegisterValue)this["Modulator"]["VIB"]).Value = 0;
            ((RegisterValue)this["Modulator"]["EG"]).Value = 0;
            ((RegisterValue)this["Modulator"]["DIST"]).Value = 0;

            ((RegisterValue)this["Career"]["AR"]).Value = tone.aOp[1].AR / 2;
            ((RegisterValue)this["Career"]["DR"]).Value = tone.aOp[1].DR / 2;
            ((RegisterValue)this["Career"]["RR"]).Value = tone.aOp[1].RR;
            ((RegisterValue)this["Career"]["SL"]).Value = tone.aOp[1].SL;
            ((RegisterValue)this["Career"]["SR"]).Value = tone.aOp[1].SR / 2;
            ((RegisterValue)this["Career"]["KSL"]).Value = tone.aOp[1].KS;
            ((RegisterValue)this["Career"]["KSR"]).Value = 0;
            ((RegisterValue)this["Career"]["MUL"]).Value = tone.aOp[1].ML;
            ((RegisterValue)this["Career"]["AM"]).Value = tone.aOp[1].AM;
            ((RegisterValue)this["Career"]["VIB"]).Value = 0;
            ((RegisterValue)this["Career"]["EG"]).Value = 0;
            ((RegisterValue)this["Career"]["DIST"]).Value = 0;

            if (!string.IsNullOrWhiteSpace(tone.Name))
                timbre.Memo = tone.Name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tone"></param>
        protected override void ApplyTimbre(TimbreBase timbre)
        {
            YM2413Timbre tim = (YM2413Timbre)timbre;
            this.timbre = tim;

            this["General"].Target = tim;
            ((RegisterValue)this["General"]["FB"]).Value = tim.FB;
            ((RegisterValue)this["General"]["SUS"]).Value = tim.SUS;

            this["Modulator"].Target = tim.Modulator;
            ((RegisterValue)this["Modulator"]["AR"]).Value = tim.Modulator.AR;
            ((RegisterValue)this["Modulator"]["DR"]).Value = tim.Modulator.DR;
            ((RegisterValue)this["Modulator"]["RR"]).Value = tim.Modulator.RR;
            ((RegisterValue)this["Modulator"]["SL"]).Value = tim.Modulator.SL;
            ((RegisterValue)this["Modulator"]["SR"]).NullableValue = tim.Modulator.SR;
            ((RegisterValue)this["Modulator"]["TL"]).Value = tim.Modulator.TL;
            ((RegisterValue)this["Modulator"]["KSL"]).Value = tim.Modulator.KSL;
            ((RegisterValue)this["Modulator"]["KSR"]).Value = tim.Modulator.KSR;
            ((RegisterValue)this["Modulator"]["MUL"]).Value = tim.Modulator.MUL;
            ((RegisterValue)this["Modulator"]["AM"]).Value = tim.Modulator.AM;
            ((RegisterValue)this["Modulator"]["VIB"]).Value = tim.Modulator.VIB;
            ((RegisterValue)this["Modulator"]["EG"]).Value = tim.Modulator.EG;
            ((RegisterValue)this["Modulator"]["DIST"]).Value = tim.Modulator.DIST;

            this["Career"].Target = tim.Career;
            ((RegisterValue)this["Career"]["AR"]).Value = tim.Career.AR;
            ((RegisterValue)this["Career"]["DR"]).Value = tim.Career.DR;
            ((RegisterValue)this["Career"]["RR"]).Value = tim.Career.RR;
            ((RegisterValue)this["Career"]["SL"]).Value = tim.Career.SL;
            ((RegisterValue)this["Career"]["SR"]).NullableValue = tim.Career.SR;
            ((RegisterValue)this["Career"]["KSL"]).Value = tim.Career.KSL;
            ((RegisterValue)this["Career"]["KSR"]).Value = tim.Career.KSR;
            ((RegisterValue)this["Career"]["MUL"]).Value = tim.Career.MUL;
            ((RegisterValue)this["Career"]["AM"]).Value = tim.Career.AM;
            ((RegisterValue)this["Career"]["VIB"]).Value = tim.Career.VIB;
            ((RegisterValue)this["Career"]["EG"]).Value = tim.Career.EG;
            ((RegisterValue)this["Career"]["DIST"]).Value = tim.Career.DIST;
        }
    }

}
