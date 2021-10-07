// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using zanac.MAmidiMEmo.Instruments.Chips;
using zanac.MAmidiMEmo.Instruments;

namespace zanac.MAmidiMEmo.Gui.FMEditor
{
    public partial class YM2414OperatorContainer : RegisterContainerBase
    {
        private YM2414.YM2414Operator op;

        private RegisterValue d2r;
        private RegisterValue egsf;
        private RegisterValue dt2;
        private RegisterValue fix;
        private RegisterValue oscf;
        private RegisterValue mul;
        private RegisterValue dt1;

        /// <summary>
        /// 
        /// </summary>
        public override string SerializeData
        {
            get
            {
                return SerializeProps(this,
                        nameof(op.EN),
                        nameof(op.AR),
                        nameof(op.D1R),
                        nameof(op.D2R),
                        nameof(op.RR),
                        nameof(op.SL),
                        nameof(op.TL),
                        nameof(op.RS),
                        nameof(op.FIX),
                        nameof(op.OSCF),
                        nameof(op.MUL),
                        nameof(op.DT1),
                        nameof(op.AM),
                        nameof(op.EGSF),
                        nameof(op.DT2));
            }
            set
            {
                DeserializeProps(this, value,
                    nameof(op.EN),
                    nameof(op.AR),
                    nameof(op.D1R),
                    nameof(op.D2R),
                    nameof(op.RR),
                    nameof(op.SL),
                    nameof(op.TL),
                    nameof(op.RS),
                    nameof(op.FIX),
                    nameof(op.OSCF),
                    nameof(op.MUL),
                    nameof(op.DT1),
                    nameof(op.AM),
                    nameof(op.EGSF),
                    nameof(op.DT2));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public YM2414OperatorContainer(YM2414.YM2414Operator op, string name) : base(op, name)
        {
            InitializeComponent();

            this.op = op;

            AddControl(new RegisterFlag("EN", op.EN != 0 ? true : false));
            AddControl(new RegisterValue("AR", op.AR, 0, 31));
            AddControl(new RegisterValue("D1R", op.D1R, 0, 31));
            d2r = new RegisterValue("D2R", "D2R", op.D2R, 0, 31, false);
            AddControl(d2r);
            AddControl(new RegisterValue("RR", op.RR, 0, 15));
            AddControl(new RegisterValue("SL", op.SL, 0, 15));
            AddControl(new RegisterValue("TL", op.TL, 0, 127));
            AddControl(new RegisterValue("RS", op.RS, 0, 3));
            fix = new RegisterValue("FIX", op.FIX, 0, 1);
            AddControl(fix);
            oscf = new RegisterValue("OSCF", op.OSCF, 0, 1);
            AddControl(oscf);
            mul = new RegisterValue("MUL/FXF/FINE", "MUL", op.MUL, 0, 15, false);
            AddControl(mul);
            dt1 = new RegisterValue("DT1/FXR/OW", "DT1", op.DT1, 0, 7, false);
            AddControl(dt1);
            AddControl(new RegisterValue("AM", op.AM, 0, 1));
            egsf = new RegisterValue("EGSF", op.EGSF, 0, 1);
            AddControl(egsf);
            dt2 = new RegisterValue("DT2/EGS", "DT2", op.DT2, 0, 3, false);
            AddControl(dt2);

            AddControl(new RegisterSpace("spc") { Dock = DockStyle.Right });
            AddControl(new RegisterOpzWaveForm((RegisterValue)GetControl("OSCF"), (RegisterValue)GetControl("DT1")));
            AddControl(new RegisterEnvForm(
                (RegisterValue)GetControl("AR"),
                (RegisterValue)GetControl("TL"),
                (RegisterValue)GetControl("D1R"), false,
                (RegisterValue)GetControl("SL"),
                (RegisterValue)GetControl("D2R"),
                (RegisterValue)GetControl("RR")
                ));

            egsf.ValueChanged += YM2414OperatorContainer_EGSF_ValueChanged;
            YM2414OperatorContainer_EGSF_ValueChanged(egsf, null);
            oscf.ValueChanged += YM2414OperatorContainer_OSCF_ValueChanged;
            fix.ValueChanged += YM2414OperatorContainer_OSCF_ValueChanged;
            YM2414OperatorContainer_OSCF_ValueChanged(oscf, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void YM2414OperatorContainer_EGSF_ValueChanged(object sender, PropertyChangedEventArgs e)
        {
            if (egsf.Value == 0)
            {
                d2r.Label = "D2R";
                dt2.Label = "DT2";
            }
            else
            {
                d2r.Label = "REV";
                dt2.Label = "EGS";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void YM2414OperatorContainer_OSCF_ValueChanged(object sender, PropertyChangedEventArgs e)
        {
            if (oscf.Value == 0)
            {
                if (fix.Value == 0)
                {
                    dt1.Label = "DT1";
                    mul.Label = "MUL";
                }
                else
                {
                    dt1.Label = "FXR";
                    mul.Label = "FXF";
                }
            }
            else
            {
                dt1.Label = "OW";
                mul.Label = "FINE";
            }
        }
    }
}
