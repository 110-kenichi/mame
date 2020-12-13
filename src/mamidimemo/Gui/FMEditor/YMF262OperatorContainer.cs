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
    public partial class YMF262OperatorContainer : RegisterContainerBase
    {
        private YMF262.YMF262Operator op;
        private YMF262GeneralContainer gen;

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
                    nameof(op.VIB),
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
                    nameof(op.VIB),
                    nameof(op.EG),
                    nameof(op.WS));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public YMF262OperatorContainer(YMF262.YMF262Operator op, YMF262GeneralContainer gen, string name) : base(op, name)
        {
            InitializeComponent();

            this.op = op;
            if (gen != null)
            {
                this.gen = gen;
                gen.ValueChanged += Gen_ValueChanged;
                Gen_ValueChanged(null, new PropertyChangedEventArgs("ALG"));
            }

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
            AddControl(new RegisterValue("VIB", op.VIB, 0, 1));
            AddControl(new RegisterValue("EG", op.EG, 0, 1));
            AddControl(new RegisterValue("WS", op.WS, 0, 7));
            AddControl(new RegisterOplWaveForm(
                (RegisterValue)GetControl("WS")
                ));

            AddControl(new RegisterSpace("spc") { Dock = DockStyle.Right });
            AddControl(new RegisterEnvForm(
                (RegisterValue)GetControl("AR"),
                (RegisterValue)GetControl("TL"),
                (RegisterValue)GetControl("DR"), true,
                (RegisterValue)GetControl("SL"),
                (RegisterValue)GetControl("SR"),
                (RegisterValue)GetControl("RR")
                ));
        }

        private void Gen_ValueChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "ALG":
                    RegisterValue rv = (RegisterValue)gen.GetControl("ALG");
                    Enabled = (rv.Value >= 2);
                    break;
            }
        }
    }
}
