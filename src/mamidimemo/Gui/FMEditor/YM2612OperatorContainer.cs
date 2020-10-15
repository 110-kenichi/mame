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
    public partial class YM2612OperatorContainer : RegisterContainerBase
    {
        private YM2612.YM2612Operator op;

        /// <summary>
        /// 
        /// </summary>
        public YM2612OperatorContainer(YM2612.YM2612Operator op, string name) : base(op, name)
        {
            InitializeComponent();

            this.op = op;

            AddControl(new RegisterFlag("EN", op.EN != 0 ? true : false));
            AddControl(new RegisterValue("AR", op.AR, 0, 31));
            AddControl(new RegisterValue("D1R", op.D1R, 0, 31));
            AddControl(new RegisterValue("D2R", op.D2R, 0, 31));
            AddControl(new RegisterValue("RR", op.RR, 0, 15));
            AddControl(new RegisterValue("SL", op.SL, 0, 15));
            AddControl(new RegisterValue("TL", op.TL, 0, 127));
            AddControl(new RegisterValue("RS", op.RS, 0, 3));
            AddControl(new RegisterValue("MUL", op.MUL, 0, 15));
            AddControl(new RegisterValue("DT1", op.DT1, 0, 7));
            AddControl(new RegisterValue("AM", op.AM, 0, 1));
            AddControl(new RegisterValue("SSG", op.SSG, 0, 15));

            AddControl(new RegisterEnvForm(
                (RegisterValue)GetControl("AR"),
                (RegisterValue)GetControl("TL"),
                (RegisterValue)GetControl("D1R"), false,
                (RegisterValue)GetControl("SL"),
                (RegisterValue)GetControl("D2R"),
                (RegisterValue)GetControl("RR")
                ));
        }

    }
}
