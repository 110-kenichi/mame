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

namespace zanac.MAmidiMEmo.Gui.FMEditor
{
    public partial class RegisterValue : RegisterBase
    {
        /// <summary>
        /// 
        /// </summary>
        public string Label
        {
            get
            {
                return labelName.Text;
            }
            set
            {
                labelName.Text = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int Value
        {
            get
            {
                return trackBar.Value;
            }
            set
            {
                if (value < Minimum)
                    value = Minimum;
                else if (value > Maximum)
                    value = Maximum;
                if (trackBar.Value != value)
                    trackBar.Value = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int? NullableValue
        {
            get
            {
                if (trackBar.Value == trackBar.Minimum)
                    return null;
                else
                    return trackBar.Value;
            }
            set
            {
                if (value == null)
                {
                    trackBar.Value = trackBar.Minimum;
                }
                else
                {
                    if (value < Minimum)
                        value = Minimum;
                    else if (value > Maximum)
                        value = Maximum;
                    trackBar.Value = value.Value;
                }

            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsNullable
        {
            get;
            private set;
        }


        /// <summary>
        /// 
        /// </summary>
        public int Minimum
        {
            get
            {
                return trackBar.Minimum;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int Maximum
        {
            get
            {
                return trackBar.Maximum;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public RegisterValue() : base(null)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="registerName"></param>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public RegisterValue(string registerName, int value, int min, int max) : this(registerName, value, min, max, false)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public RegisterValue(string registerName, int value, int min, int max, bool isNullable) : this(null, registerName, value, min, max, isNullable)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public RegisterValue(string labelName, string registerName, int value, int min, int max, bool isNullable) : base(registerName)
        {
            InitializeComponent();
            Dock = DockStyle.Left;

            this.labelName.Text = labelName != null ? labelName : registerName;

            ItemName = registerName;
            IsNullable = isNullable;

            numericUpDown.Minimum = isNullable ? min - 1 : min;
            numericUpDown.Maximum = max;
            numericUpDown.Value = value;

            trackBar.Minimum = isNullable ? min - 1 : min;
            trackBar.Maximum = max;
            trackBar.Value = value;

            numericUpDown.ValueChanged += (s, e) =>
            {
                var v = (int)numericUpDown.Value;
                if (trackBar.Value != v)
                    trackBar.Value = v;

                OnValueChanged(new PropertyChangedEventArgs(registerName));
            };
            trackBar.ValueChanged += (s, e) =>
            {
                var v = (int)trackBar.Value;
                if ((int)numericUpDown.Value != v)
                    numericUpDown.Value = v;

                OnValueChanged(new PropertyChangedEventArgs(registerName));
            };
        }
    }
}
