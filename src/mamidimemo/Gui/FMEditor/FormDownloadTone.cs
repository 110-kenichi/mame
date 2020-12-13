using FM_SoundConvertor;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using zanac.MAmidiMEmo.Properties;

namespace zanac.MAmidiMEmo.Gui.FMEditor
{
    public partial class FormDownloadTone : FormBase
    {

        private Stack<string> accessedContentsUrlStack = new Stack<string>();

        private Stack<string> accessedContentsDirNameStack = new Stack<string>();

        private string lastAccessedContentsUrl;

        public event EventHandler SelectedToneChanged;

        /// <summary>
        /// 
        /// </summary>
        public Tone SelectedTone
        {
            get
            {
                return (Tone)listBoxTones.SelectedItem;
            }
        }

        private IEnumerable<Tone> selectedTones;

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<Tone> SelectedTones
        {
            get
            {
                return selectedTones;
            }
            private set
            {
                selectedTones = value;
                if (selectAll)
                    buttonOK.Enabled = selectedTones != null && selectedTones.Count() != 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public FormDownloadTone()
        {
            InitializeComponent();

            buttonOK.Enabled = false;
        }

        private bool selectAll;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="selectAll"></param>
        public FormDownloadTone(bool selectAll)
        {
            InitializeComponent();

            buttonOK.Enabled = false;

            this.selectAll = selectAll;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);

            var contentsUrl = $"https://api.github.com/repos/110-kenichi/ToneLibrary/contents";
            accessedContentsUrlStack.Push(contentsUrl);
            accessedContentsDirNameStack.Push("./");
            await updateDirList(contentsUrl);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private WebClient createWebClient()
        {
            WebClient client = new WebClient();
            client.Headers.Add("user-agent", "MAmidiMEmo/" + Program.FILE_VERSION);


            var userName = Settings.Default.GitHubUserName;
            var token = Settings.Default.GitHubPersonalAccessToken;
            if (!string.IsNullOrWhiteSpace(userName) && !string.IsNullOrWhiteSpace(token))
            {
                userName = userName.Trim();
                token = token.Trim();
                client.Headers[HttpRequestHeader.Accept] = "application/json";
                client.Headers[HttpRequestHeader.Authorization] = "token " + token;
            }

            return client;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentsUrl"></param>
        /// <returns></returns>
        private async Task updateDirList(string contentsUrl)
        {
            metroListViewDir.Items.Clear();

            if (String.IsNullOrWhiteSpace(contentsUrl))
                return;

            lastAccessedContentsUrl = contentsUrl;

            using (var webClient = createWebClient())
            {
                //Upper folder
                if (accessedContentsUrlStack.Count != 1)
                {
                    var item = new ListViewItem((string)"../", "UP");
                    item.Tag = "../";
                    metroListViewDir.Items.Add(item);
                }

                try
                {
                    metroListViewDir.BeginUpdate();

                    //Download list
                    var contentsJson = await webClient.DownloadStringTaskAsync(contentsUrl);
                    if (this.IsDisposed)
                        return;

                    updateRemainingCount(webClient);

                    var contents = (JArray)JsonConvert.DeserializeObject(contentsJson);
                    foreach (var file in contents)
                    {
                        switch ((string)file["type"])
                        {
                            case "dir":
                                {
                                    var item = new ListViewItem((string)file["name"], "FOLDER");
                                    item.Tag = file;
                                    metroListViewDir.Items.Add(item);
                                }
                                break;
                            case "file":
                                {
                                    var durl = (string)file["download_url"];
                                    if (durl == null)
                                    {
                                        //submodule
                                        goto case "dir";
                                    }
                                    else
                                    {
                                        var fn = (string)file["name"];
                                        var ext = System.IO.Path.GetExtension(fn);
                                        if (FormFmEditor.IsSupportedExtension(ext))
                                        {
                                            var item = new ListViewItem(fn, "SOUND");
                                            item.Tag = file;
                                            metroListViewDir.Items.Add(item);
                                        }
                                        else if (ext.Equals(".txt", StringComparison.OrdinalIgnoreCase)
                                            || ext.Equals(".md", StringComparison.OrdinalIgnoreCase))
                                        {
                                            var item = new ListViewItem(fn, "TXT");
                                            item.Tag = file;
                                            metroListViewDir.Items.Add(item);
                                        }
                                    }
                                }
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (ex.GetType() == typeof(Exception))
                        throw;
                    else if (ex.GetType() == typeof(SystemException))
                        throw;

                    MessageBox.Show(Resources.FailedLoadFile + "\r\n" + ex.Message);
                }
                finally
                {
                    metroListViewDir.EndUpdate();
                }
            }
        }

        private void updateRemainingCount(WebClient webClient)
        {
            WebHeaderCollection myWebHeaderCollection = webClient.ResponseHeaders;
            metroLabelRemaining.Text = "Remaining access count: " + myWebHeaderCollection["X-RateLimit-Remaining"];
        }

        private async void metroListViewDir_DoubleClick(object sender, EventArgs e)
        {
            if (metroListViewDir.SelectedItems.Count != 0)
            {
                switch (metroListViewDir.SelectedItems[0].Tag)
                {
                    case JToken file:
                        try
                        {
                            switch ((string)file["type"])
                            {
                                case "dir":
                                    accessedContentsUrlStack.Push((string)file["url"]);
                                    accessedContentsDirNameStack.Push((string)file["name"] + "/");
                                    await updateDirList((string)file["url"]);
                                    if (this.IsDisposed)
                                        return;
                                    break;
                                case "file":
                                    var durl = (string)file["download_url"];
                                    if (durl == null)
                                    {
                                        //submod
                                        using (var webClient = createWebClient())
                                        {
                                            //Download submodule
                                            var contentsJson = await webClient.DownloadStringTaskAsync((string)file["url"]);
                                            if (this.IsDisposed)
                                                return;

                                            updateRemainingCount(webClient);

                                            var contents = (JObject)JsonConvert.DeserializeObject(contentsJson);
                                            Uri uri = new Uri((string)contents["submodule_git_url"]);
                                            var urib = new UriBuilder(uri.Scheme, "api.github.com", uri.Port, "repos" + uri.LocalPath + "/contents");
                                            accessedContentsUrlStack.Push(urib.Uri.OriginalString);
                                            accessedContentsDirNameStack.Push((string)file["name"] + "/");
                                            await updateDirList(urib.Uri.OriginalString);
                                            if (this.IsDisposed)
                                                return;
                                        }
                                        break;
                                    }
                                    else
                                    {
                                        //file
                                        var fn = (string)file["path"];
                                        var ext = System.IO.Path.GetExtension(fn);

                                        //download dest dir
                                        var dlDestDir = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MAmidiMEmo");
                                        StringBuilder relPath = new StringBuilder();
                                        foreach (string dn in accessedContentsDirNameStack.ToArray())
                                            relPath.Append(dn + "/");
                                        dlDestDir = System.IO.Path.Combine(dlDestDir, relPath.ToString());
                                        System.IO.Directory.CreateDirectory(dlDestDir);
                                        //download dest file path
                                        var dlDestFilePath = System.IO.Path.Combine(dlDestDir, (string)file["name"]);

                                        //Download from Git
                                        bool download = true;
                                        //if (System.IO.File.Exists(dlDestFilePath))
                                        //{
                                        //    using (var fs = new FileStream(dlDestFilePath, FileMode.Open, FileAccess.Read))
                                        //    {
                                        //        string localSha1 = BitConverter.ToString(SHA1.Create().ComputeHash(fs)).Replace("-", "").ToLower();
                                        //        using (var webClient = createWebClient())
                                        //        {
                                        //            //Download submodule
                                        //            var contentsJson = await webClient.DownloadStringTaskAsync((string)file["url"]);
                                        //            var contents = (JObject)JsonConvert.DeserializeObject(contentsJson);
                                        //            string remoteSha1 = (string)contents["sha"];
                                        //            if (localSha1.Equals(remoteSha1, StringComparison.OrdinalIgnoreCase))
                                        //                download = false;
                                        //        }
                                        //    }
                                        //}
                                        if (download)
                                        {
                                            //Download new file
                                            using (var wc = createWebClient())
                                                await wc.DownloadFileTaskAsync(new Uri(durl), dlDestFilePath);
                                            if (this.IsDisposed)
                                                return;
                                        }
                                        if (FormFmEditor.IsSupportedExtension(ext))
                                        {
                                            listBoxTones.Items.Clear();

                                            //Import tone file
                                            IEnumerable<Tone> tones = null;
                                            var Option = new Option();
                                            string[] importFile = { dlDestFilePath.ToLower(CultureInfo.InvariantCulture) };
                                            switch (ext.ToUpper(CultureInfo.InvariantCulture))
                                            {
                                                case ".MUC":
                                                    tones = Muc.Reader(importFile, Option);
                                                    break;
                                                case ".DAT":
                                                    tones = Dat.Reader(importFile, Option);
                                                    break;
                                                case ".MWI":
                                                    tones = Fmp.Reader(importFile, Option);
                                                    break;
                                                case ".MML":
                                                    tones = Pmd.Reader(importFile, Option);
                                                    break;
                                                case ".FXB":
                                                    tones = Vopm.Reader(importFile, Option);
                                                    break;
                                            }

                                            //Listing tones
                                            if (tones != null && tones.Count() > 0)
                                            {
                                                foreach (var tone in tones)
                                                    listBoxTones.Items.Add(tone);

                                                listBoxTones.SelectedItem = null;
                                            }
                                            SelectedTones = tones;
                                        }
                                        else if (ext.Equals(".txt", StringComparison.OrdinalIgnoreCase)
                                            || ext.Equals(".md", StringComparison.OrdinalIgnoreCase))
                                        {
                                            Process.Start(dlDestFilePath);
                                        }
                                    }
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            if (ex.GetType() == typeof(Exception))
                                throw;
                            else if (ex.GetType() == typeof(SystemException))
                                throw;

                            MessageBox.Show(Resources.FailedLoadFile + "\r\n" + ex.Message);
                        }
                        break;
                    case string url:
                        if (url.Equals("../", StringComparison.Ordinal))
                        {
                            accessedContentsUrlStack.Pop();
                            accessedContentsDirNameStack.Pop();
                            string upurl = accessedContentsUrlStack.Pop();
                            string updn = accessedContentsDirNameStack.Pop();
                            accessedContentsUrlStack.Push(upurl);
                            accessedContentsDirNameStack.Push(updn);
                            await updateDirList(upurl);
                            if (this.IsDisposed)
                                return;
                        }
                        else
                        {
                            try
                            {
                                accessedContentsUrlStack.Push(url);
                                await updateDirList(url);
                                if (this.IsDisposed)
                                    return;
                            }
                            catch (Exception ex)
                            {
                                if (ex.GetType() == typeof(Exception))
                                    throw;
                                else if (ex.GetType() == typeof(SystemException))
                                    throw;

                                MessageBox.Show(Resources.FailedLoadFile + "\r\n" + ex.Message);
                            }
                        }
                        break;
                }
            }
        }

        private void listBoxTones_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!selectAll)
                buttonOK.Enabled = listBoxTones.SelectedItem != null;

            SelectedToneChanged?.Invoke(this, e);
        }

        private void listBoxTones_DoubleClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private async void metroButtonRefresh_Click(object sender, EventArgs e)
        {
            await updateDirList(lastAccessedContentsUrl);
        }

        private void metroLink1_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("https://github.com/settings/tokens");
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(Exception))
                    throw;
                else if (ex.GetType() == typeof(SystemException))
                    throw;

                MessageBox.Show(Resources.FailedLaunch + "\r\n" + ex.Message);
            }
        }
    }
}
