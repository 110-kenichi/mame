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
        public FormWeb()
        {
            InitializeComponent();

            webView.Source = new Uri("https://vgmrips.net/packs/");
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

        //private String lastCoverArt;

        private void webView_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            toolStripButtonForward.Enabled = webView.CanGoForward;
            toolStripButtonBack.Enabled = webView.CanGoBack;
            toolStripButtonStopRefresh.Text = "○";
            toolStripButtonStopRefresh.Image = Resources.Reload;
            toolStripStatusLabel.Text = "";

            //https://vgmrips.net/packs/vgm/Arcade/Battle_Garegga_%28Toaplan_2%29/14%20Marginal%20Consciousness%20%5BStage%207%20Airport%5D.vgz
            //https://vgmrips.net/packs/images/large/Arcade/Battle_Garegga_%28Toaplan_2%29.png
            //lastCoverArt = await webView.ExecuteScriptAsync("document.querySelector('#imageContainer > div > a').getAttribute('href');");
        }

        private void toolStripButtonStopRefresh_Click(object sender, EventArgs e)
        {
            if (String.Equals(toolStripButtonStopRefresh.Text, "○"))
                webView.Reload();
            else
                webView.Stop();
        }

        private void webView_NavigationStarting(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
        {
            toolStripButtonStopRefresh.Text = "×";
            toolStripButtonStopRefresh.Image = Resources.Cancel;
            toolStripStatusLabel.Text = "Loading...";
        }

        private void webView_ContentLoading(object sender, Microsoft.Web.WebView2.Core.CoreWebView2ContentLoadingEventArgs e)
        {

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

                    FormMain.TopForm.BeginInvoke(new MethodInvoker(() =>
                        {
                            if (FormMain.TopForm.IsDisposed)
                                return;
                            if (toolStripButtonAdd.Checked)
                                FormMain.TopForm.AddFilesToList(new[] { file }, true);
                            else
                                FormMain.TopForm.PlayFileDirect(file);
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
            //        FormMain.TopForm.AddFilesToList(new[] { file }, true);
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

        private void toolStripButtonAdd_Click(object sender, EventArgs e)
        {
            toolStripButtonAdd.Checked = !toolStripButtonAdd.Checked;
        }
    }
}
