// copyright-holders:K.Ito
using MetroFramework.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace zanac.MAmidiMEmo.Gui
{
    public partial class FormBase : MetroForm
    {
        public FormBase()
        {
            InitializeComponent();
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
                        if (cc == null)
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
    }
}
