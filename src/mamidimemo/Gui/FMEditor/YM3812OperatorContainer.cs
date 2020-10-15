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
    public partial class YM3812OperatorContainer : RegisterContainerBase
    {
        private YM3812.YM3812Operator op;

        /// <summary>
        /// 
        /// </summary>
        public override string SerializeData
        {
            get
            {
                return SerializeProps(this,
                    nameof(op.AR),
                    nameof(op.DR),
                    nameof(op.RR),
                    nameof(op.SL),
                    nameof(op.SR),
                    nameof(op.TL),
                    nameof(op.KSL),
                    nameof(op.KSR),
                    nameof(op.MFM),
                    nameof(op.AM),
                    nameof(op.VR),
                    nameof(op.EG),
                    nameof(op.WS));
            }
            set
            {
                DeserializeProps(this, value,
                    nameof(op.AR),
                    nameof(op.DR),
                    nameof(op.RR),
                    nameof(op.SL),
                    nameof(op.SR),
                    nameof(op.TL),
                    nameof(op.KSL),
                    nameof(op.KSR),
                    nameof(op.MFM),
                    nameof(op.AM),
                    nameof(op.VR),
                    nameof(op.EG),
                    nameof(op.WS));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public YM3812OperatorContainer(YM3812.YM3812Operator op, string name) : base(op, name)
        {
            InitializeComponent();

            this.op = op;

            AddControl(new RegisterValue("AR", op.AR, 0, 15));
            AddControl(new RegisterValue("DR", op.DR, 0, 15));
            AddControl(new RegisterValue("RR", op.RR, 0, 15));
            AddControl(new RegisterValue("SL", op.SL, 0, 15));
            AddControl(new RegisterValue("SR", op.SR == null ? -1 : op.SR.Value, 0, 15, true));
            AddControl(new RegisterValue("TL", op.TL, 0, 63));
            AddControl(new RegisterValue("KSL", op.KSL, 0, 3));
            AddControl(new RegisterValue("KSR", op.KSR, 0, 1));
            AddControl(new RegisterValue("MFM", op.MFM, 0, 15));
            AddControl(new RegisterValue("AM", op.AM, 0, 1));
            AddControl(new RegisterValue("VR", op.VR, 0, 1));
            AddControl(new RegisterValue("EG", op.EG, 0, 1));
            AddControl(new RegisterValue("WS", op.WS, 0, 3));
            AddControl(new RegisterOplWaveForm(
                (RegisterValue)GetControl("WS")
                ));

            AddControl(new RegisterEnvForm(
                (RegisterValue)GetControl("AR"),
                (RegisterValue)GetControl("TL"),
                (RegisterValue)GetControl("DR"), true,
                (RegisterValue)GetControl("SL"),
                (RegisterValue)GetControl("SR"),
                (RegisterValue)GetControl("RR")
                ));
        }

    }
}
