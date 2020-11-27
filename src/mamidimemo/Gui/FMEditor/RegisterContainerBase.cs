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
using zanac.MAmidiMEmo.Instruments;
using zanac.MAmidiMEmo.ComponentModel;
using System.Reflection;
using System.Collections;

namespace zanac.MAmidiMEmo.Gui.FMEditor
{
    public partial class RegisterContainerBase : UserControl
    {
        public object Target
        {
            get;
            set;
        }

        private Dictionary<String, RegisterBase> controls = new Dictionary<string, RegisterBase>();

        /// <summary>
        /// 
        /// </summary>
        public RegisterBase[] RegisterControls
        {
            get
            {
                return controls.Values.ToArray();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string RegisterName
        {
            get;
            set;
        }


        /// <summary>
        /// 
        /// </summary>
        public virtual string SerializeData
        {
            get
            {
                return string.Empty;
            }
            set
            {
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public RegisterContainerBase()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        public RegisterContainerBase(object target, string registerName)
        {
            InitializeComponent();

            RegisterName = registerName;
            this.Target = target;
            labelName.Text = registerName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        protected void AddControl(RegisterBase control)
        {
            controls.Add(control.ItemName, control);

            this.Controls.Add(control);
            if (control.Dock == DockStyle.Left)
                this.Controls.SetChildIndex(control, 0);
            else
                this.Controls.SetChildIndex(control, this.Controls.Count - 3);

            switch (control)
            {
                case RegisterFlag fc:
                    fc.ValueChanged += (s, e) => { OnValueChanged(s, e); };
                    break;
                case RegisterValue rc:
                    rc.ValueChanged += (s, e) => { OnValueChanged(s, e); };
                    break;
            }

            textBoxSR.Text = SerializeData;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public RegisterBase GetControl(string name)
        {
            return controls[name];
        }


        public RegisterBase this[string name]
        {
            get
            {
                return controls[name];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<PropertyChangedEventArgs> ValueChanged;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnValueChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (sender)
            {
                case RegisterFlag rf:
                    {
                        SimpleSerializer.DeserializeProps(Target, rf.Value ? "1" : "0", rf.ItemName);
                        break;
                    }
                case RegisterValue rc:
                    if (rc.IsNullable)
                    {
                        if (rc.NullableValue == null)
                            SimpleSerializer.DeserializeProps(Target, "", rc.ItemName);
                        else
                            SimpleSerializer.DeserializeProps(Target, rc.Value.ToString((IFormatProvider)null), rc.ItemName);
                    }
                    else
                    {
                        SimpleSerializer.DeserializeProps(Target, rc.Value.ToString((IFormatProvider)null), rc.ItemName);
                    }
                    break;
            }

            ValueChanged?.Invoke(this, e);

            ignoreTextChange = true;
            textBoxSR.Text = SerializeData;
            ignoreTextChange = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="props"></param>
        /// <returns></returns>
        public static string SerializeProps<T>(T obj, params string[] props) where T : RegisterContainerBase
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                foreach (string m in props)
                {
                    if (sb.Length != 0)
                        sb.Append(",");
                    var f = getRegisterControl(obj, m);
                    switch (f)
                    {
                        case RegisterFlag rf:
                            {
                                var val = rf.Value;
                                sb.Append(val ? "1" : "0");
                                break;
                            }
                        case RegisterValue rv:
                            {
                                if (rv.IsNullable)
                                {
                                    var val = rv.NullableValue;
                                    if (val != null)
                                        sb.Append(val.ToString());
                                }
                                else
                                {
                                    var val = rv.Value;
                                    sb.Append(val.ToString());
                                }
                                break;
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(Exception))
                    throw;
                else if (ex.GetType() == typeof(SystemException))
                    throw;

            }

            return sb.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="props"></param>
        /// <returns></returns>
        public static void DeserializeProps<T>(T obj, string serializeData, params string[] props) where T : RegisterContainerBase
        {
            try
            {
                serializeData = serializeData.Replace("\r", "").Replace("\n", "");
                IEnumerator vals = null;
                if (serializeData.Contains(','))
                    vals = serializeData.Split(new char[] { ',' }).GetEnumerator();
                else
                    vals = serializeData.Split(new char[] { ' ', '\t' }).GetEnumerator();
                foreach (string m in props)
                {
                    if (!vals.MoveNext())
                        break;
                    var v = (string)vals.Current;
                    v = v.Trim();

                    var f = getRegisterControl(obj, m);
                    try
                    {
                        if (string.IsNullOrEmpty(v))
                        {
                            switch (f)
                            {
                                case RegisterValue rv:
                                    {
                                        if (rv.IsNullable)
                                            rv.NullableValue = null;
                                        break;
                                    }
                            }
                        }
                        else
                        {
                            var val = int.Parse(v);
                            switch (f)
                            {
                                case RegisterValue rv:
                                    {
                                        rv.Value = val;
                                        break;
                                    }
                                case RegisterFlag rf:
                                    {
                                        rf.Value = val != 0 ? true : false;
                                        break;
                                    }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex.GetType() == typeof(Exception))
                            throw;
                        else if (ex.GetType() == typeof(SystemException))
                            throw;

                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(Exception))
                    throw;
                else if (ex.GetType() == typeof(SystemException))
                    throw;

            }
        }

        private static Control getRegisterControl(RegisterContainerBase cnt, string propertyName)
        {
            foreach (Control c in cnt.Controls)
            {
                switch (c)
                {
                    case RegisterValue rv:
                        {
                            if (rv.ItemName.Equals(propertyName, StringComparison.Ordinal))
                                return rv;
                            break;
                        }
                    case RegisterFlag rf:
                        {
                            if (rf.ItemName.Equals(propertyName, StringComparison.Ordinal))
                                return rf;
                            break;
                        }
                }
            }
            return null;
        }

        private bool ignoreTextChange;

        private void textBoxSR_TextChanged(object sender, EventArgs e)
        {
            if (ignoreTextChange)
                return;

            try
            {
                SerializeData = textBoxSR.Text;
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(Exception))
                    throw;
                else if (ex.GetType() == typeof(SystemException))
                    throw;

            }
        }


        protected override void OnMouseWheel(MouseEventArgs e)
        {

            if (((Control.ModifierKeys & Keys.Alt) != Keys.Alt) && ((Control.ModifierKeys & Keys.Shift) != Keys.Shift))
            {
                ((HandledMouseEventArgs)e).Handled = true;
            }
            else
            {
                // do other staff
            }
        }
    }
}
