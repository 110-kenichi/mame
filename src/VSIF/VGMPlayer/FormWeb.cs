using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using zanac.VGMPlayer.Properties;

namespace zanac.VGMPlayer
{
    public partial class FormWeb : Form
    {
        private FormMain formMain;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="formMain"></param>
        public FormWeb(FormMain formMain)
        {
            InitializeComponent();

            webView.Source = new Uri("https://vgmrips.net/packs/");
            this.formMain = formMain;
            this.formMain.StopRequested += FormMain_StopRequested;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this.formMain.StopRequested -= FormMain_StopRequested;
            base.OnClosing(e);
        }

        private void toolStripButtonBack_Click(object sender, EventArgs e)
        {
            webView.GoBack();
        }

        private void toolStripButtonForward_Click(object sender, EventArgs e)
        {
            webView.GoForward();
        }

        private void toolStripButtonHome_Click(object sender, EventArgs e)
        {
            webView.Source = new Uri("https://vgmrips.net/packs/");
        }

        private void toolStripButtonStopRefresh_Click(object sender, EventArgs e)
        {
            if (String.Equals(toolStripButtonStopRefresh.Text, "○"))
                webView.Reload();
            else
                webView.Stop();
        }

        //private String lastCoverArt;

        private async void webView_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            toolStripButtonForward.Enabled = webView.CanGoForward;
            toolStripButtonBack.Enabled = webView.CanGoBack;
            toolStripButtonStopRefresh.Text = "○";
            toolStripButtonStopRefresh.Image = Resources.Reload;
            toolStripStatusLabel.Text = "";

            var imgName = await webView.ExecuteScriptAsync("# playBtn > span");
            await webView.ExecuteScriptAsync("document.querySelector('# playBtn').click();");
        }

        private async void FormMain_StopRequested(object sender, EventArgs e)
        {
            var imgName = await webView.ExecuteScriptAsync("document.querySelector('#playBtn > span').getAttribute('class')");
            if (imgName == "\"icon icon-pause\"")
            {
                await webView.ExecuteScriptAsync("document.querySelector('#playBtn').click();");
            }
        }

        private async void StartRequested(object sender, EventArgs e)
        {
            var imgName = await webView.ExecuteScriptAsync("document.querySelector('#playBtn > span').getAttribute('class')");
            if (imgName == "\"icon icon-play\"")
            {
                await webView.ExecuteScriptAsync("document.querySelector('#playBtn').click();");
            }
        }

        private void toolStripButtonAdd_Click(object sender, EventArgs e)
        {
            toolStripButtonAdd.Checked = !toolStripButtonAdd.Checked;
        }

        private void webView_NavigationStarting(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
        {
            toolStripButtonStopRefresh.Text = "×";
            toolStripButtonStopRefresh.Image = Resources.Cancel;
            toolStripStatusLabel.Text = "Loading...";
        }

        private CoreWebView2Environment environment;

        private void webView_CoreWebView2InitializationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
        {
            var task = CoreWebView2Environment.CreateAsync();
            task.Wait();
            environment = task.Result;

            webView.CoreWebView2.DownloadStarting += CoreWebView2_DownloadStarting;
            webView.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
            webView.CoreWebView2.DownloadStarting += CoreWebView2_DownloadStarting;
            webView.CoreWebView2.AddWebResourceRequestedFilter("https://vgmrips.net/packs/*", CoreWebView2WebResourceContext.All);
            webView.CoreWebView2.WebResourceRequested += CoreWebView2_WebResourceRequested;
            webView.CoreWebView2.IsMuted = true;
        }

        private void CoreWebView2_WebResourceRequested(object sender, CoreWebView2WebResourceRequestedEventArgs e)
        {
            var file = e.Request.Uri;
            switch (Path.GetExtension(file).ToUpper(CultureInfo.InvariantCulture))
            {
                case ".VGZ":
                case ".VGM":
                    //e.Response = environment.CreateWebResourceResponse(null, 404, "Not found", "");

                    formMain.BeginInvoke(new MethodInvoker(() =>
                        {
                            //FormMain_StopRequested(null, EventArgs.Empty);

                            if (formMain.IsDisposed)
                                return;
                            if (toolStripButtonAdd.Checked)
                                formMain.AddFilesToList(new[] { file }, true);
                            else
                                formMain.PlayFileDirect(file);

                            //StartRequested(null, EventArgs.Empty);
                        }));
                    break;
                case ".PNG":
                    break;
            }
        }

        private void CoreWebView2_NewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            //var file = e.Uri;
            //switch (Path.GetExtension(file).ToUpper(CultureInfo.InvariantCulture))
            //{
            //    case ".VGZ":
            //    case ".VGM":
            //        formMain.AddFilesToList(new[] { file }, true);
            //        break;
            //}
            //e.Handled = true;
        }

        private CoreWebView2DownloadOperation downloadOperation;

        private void CoreWebView2_DownloadStarting(object sender, CoreWebView2DownloadStartingEventArgs e)
        {
            downloadOperation = e.DownloadOperation; // Store the 'DownloadOperation' for later use in events
            //downloadOperation.BytesReceivedChanged += DownloadOperation_BytesReceivedChanged; // Subscribe to BytesReceivedChanged event
            //downloadOperation.EstimatedEndTimeChanged += DownloadOperation_EstimatedEndTimeChanged; // Subsribe to EstimatedEndTimeChanged event
        }

        //private void DownloadOperation_EstimatedEndTimeChanged(object sender, object e)
        //{
        //    //label1.Text = downloadOperation.EstimatedEndTime.ToString(); // Show the progress
        //}

        //private void DownloadOperation_BytesReceivedChanged(object sender, object e)
        //{
        //    toolStripProgressBar.Value = (int)(100 * (1 / (1 + Math.Pow(Math.E, downloadOperation.TotalBytesToReceive.Value))));
        //    //label2.Text = downloadOperation.BytesReceived.ToString(); // Show the progress
        //}

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        private const int WM_PARENTNOTIFY = 0x210;
        private const int WM_LBUTTONDOWN = 0x201;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_PARENTNOTIFY)
            {
                if (m.WParam.ToInt32() == WM_LBUTTONDOWN && ActiveForm != this)
                {
                    Point p = PointToClient(Cursor.Position);
                    if (GetChildAtPoint(p) is ToolStrip)
                        mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, (uint)p.X, (uint)p.Y, 0, 0);
                }
            }
            base.WndProc(ref m);
        }

    }
}
