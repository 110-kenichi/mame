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

        protected override void OnControlAdded(ControlEventArgs e)
        {
            if (Program.GuiScale != 0)
            {
                SetAllControlsFontSize(e.Control, Program.GuiScale);
                scaleFont.Invoke(e.Control, new object[] { 1f + Program.GuiScale });
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
                scaleFont.Invoke(child, new object[] { 1f + amount });
            };
            //scaleFont.Invoke(target, new object[] { 1f + amount });
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
                    scaleFont.Invoke(this, new object[] { 1f + Program.GuiScale });
                }
                finally
                {
                    ignoreFontChanged = false;
                }
            }
        }
    }
}
