// copyright-holders:K.Ito
using MetroFramework.Components;
using MetroFramework.Controls;
using MetroFramework.Forms;
using MetroFramework.Interfaces;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using zanac.MAmidiMEmo.Properties;
using Point = System.Drawing.Point;

namespace zanac.MAmidiMEmo.Gui
{
    public partial class FormBase : MetroForm
    {

        public FormBase()
        {
            InitializeComponent();

            ApplyFontSize();
        }

        private const int WM_NCHITTEST = 0x0084;

        protected override void WndProc(ref Message m)
        {
            if (DesignMode)
            {
                base.WndProc(ref m);
                return;
            }

            switch (m.Msg)
            {
                case WM_NCHITTEST:

                    base.WndProc(ref m);
                    if (m.Result == (IntPtr)1)  //HTCLIENT
                    {
                        Point pc = PointToClient(new Point((int)m.LParam));
                        var cc = GetChildAtPoint(pc);
                        if (cc == null && Resizable)
                        {
                            int direct = 0;
                            Rectangle rtop = new Rectangle(0, 0, Size.Width, Padding.Top);
                            if (rtop.Contains(pc))
                                direct |= 0x8;
                            Rectangle rleft = new Rectangle(0, 0, Padding.Left, Size.Height);
                            if (rleft.Contains(pc))
                                direct |= 0x2;
                            Rectangle rright = new Rectangle(Size.Width - Padding.Right, 0, Padding.Right, Size.Height);
                            if (rright.Contains(pc))
                                direct |= 0x4;
                            Rectangle rbottom = new Rectangle(0, Size.Height - Padding.Bottom, Size.Width, Padding.Bottom);
                            if (rbottom.Contains(pc))
                                direct |= 0x1;

                            switch (direct)
                            {
                                //HTBOTTOM 15
                                case 1:
                                    m.Result = (IntPtr)15;
                                    break;
                                //HTLEFT 10
                                case 2:
                                    m.Result = (IntPtr)10;
                                    break;
                                //HTBOTTOMLEFT 16
                                case 3:
                                    m.Result = (IntPtr)16;
                                    break;
                                //HTRIGHT 11
                                case 4:
                                    m.Result = (IntPtr)11;
                                    break;
                                //HTBOTTOMRIGHT 17
                                case 5:
                                    m.Result = (IntPtr)17;
                                    break;
                                //HTTOP 12
                                case 8:
                                    m.Result = (IntPtr)12;
                                    break;
                                //HTTOPLEFT 13
                                case 10:
                                    m.Result = (IntPtr)13;
                                    break;
                                //HTTOPRIGHT 14
                                case 12:
                                    m.Result = (IntPtr)14;
                                    break;
                            }
                        }
                    }
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        private static MethodInfo scaleFont = typeof(Control).GetMethod("ScaleFont", BindingFlags.Instance | BindingFlags.NonPublic);

        private static MethodInfo scaleControl = typeof(ComboBox).GetMethod("ScaleControl", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(SizeF), typeof(BoundsSpecified)}, null);

        private bool canSetFontSize(Control c)
        {
            if (c is ToolStrip)
                return false;
            else if (c is NumericUpDown)
                return false;
            else if (c is ComboBox)
                return false;

            return true;
        }

        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, Int32 wParam, Int32 lParam);
        private const Int32 CB_SETITEMHEIGHT = 0x153;

        private void SetComboBoxHeight(IntPtr comboBoxHandle, Int32 comboBoxDesiredHeight)
        {
            //https://stackoverflow.com/questions/3158004/how-do-i-set-the-height-of-a-combobox
            SendMessage(comboBoxHandle, CB_SETITEMHEIGHT, -1, comboBoxDesiredHeight);
            SendMessage(comboBoxHandle, CB_SETITEMHEIGHT, 0, comboBoxDesiredHeight);
        }

        private void setDefaultFontSize(Control c)
        {
            if (c is NumericUpDown)
            {
                c.Font = new Font("Yu Gothic UI", System.Drawing.SystemFonts.DefaultFont.Size * (1f + Program.GuiScale));
                c.Controls[1].Font = c.Font;
                c.Margin = new Padding(1);
            }
            else if (c is ComboBox)
            {
                c.Font = new Font("Yu Gothic UI", System.Drawing.SystemFonts.DefaultFont.Size * (1f + Program.GuiScale));
                SetComboBoxHeight(c.Handle,c.Font.Height);
            }
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            if (Program.GuiScale != 0)
            {
                SetAllControlsFontSize(e.Control, Program.GuiScale);
                if (canSetFontSize(e.Control))
                    scaleFont.Invoke(e.Control, new object[] { 1f + Program.GuiScale });
                else
                    setDefaultFontSize(e.Control);
            }
            if (e.Control.GetType().Name.Equals("MetroFormButton"))
            {
                switch (e.Control.Text)
                {
                    case "r":   //Close;
                    case "0":   //Minimize;
                    case "1":   //Maximize Normal;
                    case "2":   //Maximize Maximized;
                        var sf = 1f + Program.GuiScale;
                        e.Control.Size = new System.Drawing.Size
                            ((int)Math.Round(e.Control.Size.Width * sf),
                            (int)Math.Round(e.Control.Size.Height * sf));
                        e.Control.Location = new Point(0, 0);
                        break;
                }
            }

            base.OnControlAdded(e);
        }

        protected void SetAllControlsFontSize(Control target, float amount)
        {
            foreach (Control child in target.Controls)
            {
                // recursive
                if (child.Controls != null)
                    SetAllControlsFontSize(child, amount);
                if (canSetFontSize(child))
                    scaleFont.Invoke(child, new object[] { 1f + amount });
                else
                    setDefaultFontSize(child);
            };
        }

        private bool ignoreFontChanged;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnFontChanged(EventArgs e)
        {
            if (ignoreFontChanged)
                return;

            ApplyFontSize();

            base.OnFontChanged(e);
        }

        /// <summary>
        /// 
        /// </summary>
        protected void ApplyFontSize()
        {
            if (Program.GuiScale != 0)
            {
                try
                {
                    ignoreFontChanged = true;
                    SetAllControlsFontSize(this, Program.GuiScale);
                    if (canSetFontSize(this))
                        scaleFont.Invoke(this, new object[] { 1f + Program.GuiScale });
                    else
                        setDefaultFontSize(this);
                }
                finally
                {
                    ignoreFontChanged = false;
                }
            }
        }
    }
}
