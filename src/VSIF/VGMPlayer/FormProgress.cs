using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using zanac.VGMPlayer;

namespace zanac.VGMPlayer
{
    public partial class FormProgress : Form
    {
        /// <summary>
        /// Thread Safe
        /// </summary>
        public string Message
        {
            get
            {
                string rv = null;
                if (labelMessage.IsHandleCreated)
                {
                    labelMessage.Invoke(new MethodInvoker(() =>
                    {
                        if (!labelMessage.IsDisposed)
                            rv = labelMessage.Text;
                    }));
                }
                else
                {
                    rv = labelMessage.Text;
                }
                return rv;
            }
            set
            {
                if (labelMessage.IsHandleCreated)
                {
                    labelMessage.Invoke(new MethodInvoker(() =>
                    {
                        if (!labelMessage.IsDisposed)
                            labelMessage.Text = value;
                    }));
                }
                else
                {
                    labelMessage.Text = value;
                }
            }
        }

        private int percentage;

        /// <summary>
        /// Thread Safe
        /// </summary>
        public int Percentage
        {
            get
            {
                return percentage;
            }
            set
            {
                if (percentage != value)
                {
                    percentage = value;

                    if (metroProgressBar1.IsDisposed)
                        return;

                    metroProgressBar1.BeginInvoke(new MethodInvoker(() =>
                    {
                        if (!metroProgressBar1.IsDisposed)
                        {
                            if (value < 0)
                                metroProgressBar1.Style = ProgressBarStyle.Marquee;
                            else
                            {
                                metroProgressBar1.Style = ProgressBarStyle.Continuous;
                                metroProgressBar1.Value = value;
                            }
                        }
                    }));
                }
            }
        }

        public FormProgress()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="initialMessage"></param>
        /// <param name="taskAction"></param>
        public static void RunDialog(string initialMessage, Action<FormProgress> taskAction)
        {
            RunDialog(initialMessage, taskAction, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="initialMessage"></param>
        /// <param name="taskAction"></param>
        public static void RunDialog(string initialMessage, Action<FormProgress> taskAction, Action cancelHandler)
        {
            runDialogCore(FormMain.TopForm, initialMessage, taskAction, cancelHandler);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="initialMessage"></param>
        /// <param name="action"></param>
        private static void runDialogCore(Form parent, string initialMessage, Action<FormProgress> action, Action cancelHandler)
        {
            using (FormProgress f = new FormProgress())
            {
                if (parent == null)
                    f.StartPosition = FormStartPosition.CenterScreen;

                if (cancelHandler != null)
                {
                    f.metroButtonCancel.Enabled = true;
                    f.metroButtonCancel.Click += (s, e) =>
                    {
                        cancelHandler.Invoke();
                    };
                }

                f.CreateControl();

                f.Message = initialMessage;

                f.Shown += (s, e) =>
                {
                    var t = Task.Run(new Action(() =>
                    {
                        var now = DateTime.Now;

                        action(f);

                        // Dummy wait for elegant UI
                        var span = DateTime.Now - now;
                        if (span.TotalMilliseconds < 500)
                            Thread.Sleep((int)(500 - span.TotalMilliseconds));

                        f.BeginInvoke(new MethodInvoker(() => { f.Close(); }));
                    }));
                };
                f.ShowDialog(parent);
            }
        }

    }
}
