namespace zanac.MAmidiMEmo.Gui.FMEditor
{
    partial class FormDownloadTone
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormDownloadTone));
            this.buttonOK = new MetroFramework.Controls.MetroButton();
            this.buttonCancel = new MetroFramework.Controls.MetroButton();
            this.metroListViewDir = new System.Windows.Forms.ListView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.listBoxTones = new System.Windows.Forms.ListBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.metroLabelDir = new MetroFramework.Controls.MetroLabel();
            this.metroLabel1 = new MetroFramework.Controls.MetroLabel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.metroButton1 = new MetroFramework.Controls.MetroButton();
            this.metroLabelRemaining = new MetroFramework.Controls.MetroLabel();
            this.metroLabel2 = new MetroFramework.Controls.MetroLabel();
            this.metroTextBox1 = new MetroFramework.Controls.MetroTextBox();
            this.metroTextBox2 = new MetroFramework.Controls.MetroTextBox();
            this.metroLink1 = new MetroFramework.Controls.MetroLink();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(479, 61);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 6;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseSelectable = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(560, 61);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 7;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseSelectable = true;
            // 
            // metroListViewDir
            // 
            this.metroListViewDir.Dock = System.Windows.Forms.DockStyle.Fill;
            this.metroListViewDir.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.metroListViewDir.FullRowSelect = true;
            this.metroListViewDir.HideSelection = false;
            this.metroListViewDir.LargeImageList = this.imageList1;
            this.metroListViewDir.Location = new System.Drawing.Point(0, 19);
            this.metroListViewDir.MultiSelect = false;
            this.metroListViewDir.Name = "metroListViewDir";
            this.metroListViewDir.ShowGroups = false;
            this.metroListViewDir.Size = new System.Drawing.Size(317, 306);
            this.metroListViewDir.TabIndex = 1;
            this.metroListViewDir.UseCompatibleStateImageBehavior = false;
            this.metroListViewDir.DoubleClick += new System.EventHandler(this.metroListViewDir_DoubleClick);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "FOLDER");
            this.imageList1.Images.SetKeyName(1, "UP");
            this.imageList1.Images.SetKeyName(2, "SOUND");
            this.imageList1.Images.SetKeyName(3, "TXT");
            // 
            // listBoxTones
            // 
            this.listBoxTones.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxTones.FormattingEnabled = true;
            this.listBoxTones.ItemHeight = 12;
            this.listBoxTones.Location = new System.Drawing.Point(0, 19);
            this.listBoxTones.Name = "listBoxTones";
            this.listBoxTones.Size = new System.Drawing.Size(317, 306);
            this.listBoxTones.TabIndex = 1;
            this.listBoxTones.SelectedIndexChanged += new System.EventHandler(this.listBoxTones_SelectedIndexChanged);
            this.listBoxTones.DoubleClick += new System.EventHandler(this.listBoxTones_DoubleClick);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(5, 60);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.metroListViewDir);
            this.splitContainer1.Panel1.Controls.Add(this.metroLabelDir);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.listBoxTones);
            this.splitContainer1.Panel2.Controls.Add(this.metroLabel1);
            this.splitContainer1.Size = new System.Drawing.Size(638, 325);
            this.splitContainer1.SplitterDistance = 317;
            this.splitContainer1.TabIndex = 8;
            // 
            // metroLabelDir
            // 
            this.metroLabelDir.Dock = System.Windows.Forms.DockStyle.Top;
            this.metroLabelDir.Location = new System.Drawing.Point(0, 0);
            this.metroLabelDir.Name = "metroLabelDir";
            this.metroLabelDir.Size = new System.Drawing.Size(317, 19);
            this.metroLabelDir.TabIndex = 0;
            this.metroLabelDir.Text = "&Directry: (Double click to download)";
            // 
            // metroLabel1
            // 
            this.metroLabel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.metroLabel1.Location = new System.Drawing.Point(0, 0);
            this.metroLabel1.Name = "metroLabel1";
            this.metroLabel1.Size = new System.Drawing.Size(317, 19);
            this.metroLabel1.TabIndex = 0;
            this.metroLabel1.Text = "Tone& list:";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel2.ColumnCount = 4;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this.buttonCancel, 3, 3);
            this.tableLayoutPanel2.Controls.Add(this.buttonOK, 2, 3);
            this.tableLayoutPanel2.Controls.Add(this.metroButton1, 0, 3);
            this.tableLayoutPanel2.Controls.Add(this.metroLabelRemaining, 1, 3);
            this.tableLayoutPanel2.Controls.Add(this.metroLabel2, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.metroTextBox1, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.metroTextBox2, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.metroLink1, 0, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(5, 385);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 4;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(638, 87);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // metroButton1
            // 
            this.metroButton1.Location = new System.Drawing.Point(3, 61);
            this.metroButton1.Name = "metroButton1";
            this.metroButton1.Size = new System.Drawing.Size(75, 23);
            this.metroButton1.TabIndex = 4;
            this.metroButton1.Text = "&Refresh";
            this.metroButton1.UseSelectable = true;
            this.metroButton1.Click += new System.EventHandler(this.metroButtonRefresh_Click);
            // 
            // metroLabelRemaining
            // 
            this.metroLabelRemaining.AutoSize = true;
            this.metroLabelRemaining.Dock = System.Windows.Forms.DockStyle.Fill;
            this.metroLabelRemaining.Location = new System.Drawing.Point(142, 58);
            this.metroLabelRemaining.Name = "metroLabelRemaining";
            this.metroLabelRemaining.Size = new System.Drawing.Size(331, 29);
            this.metroLabelRemaining.TabIndex = 5;
            this.metroLabelRemaining.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // metroLabel2
            // 
            this.metroLabel2.AutoSize = true;
            this.metroLabel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.metroLabel2.Location = new System.Drawing.Point(3, 0);
            this.metroLabel2.Name = "metroLabel2";
            this.metroLabel2.Size = new System.Drawing.Size(133, 29);
            this.metroLabel2.TabIndex = 0;
            this.metroLabel2.Text = "GitHub &User Name:";
            this.metroLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // metroTextBox1
            // 
            this.tableLayoutPanel2.SetColumnSpan(this.metroTextBox1, 3);
            // 
            // 
            // 
            this.metroTextBox1.CustomButton.Image = null;
            this.metroTextBox1.CustomButton.Location = new System.Drawing.Point(471, 1);
            this.metroTextBox1.CustomButton.Name = "";
            this.metroTextBox1.CustomButton.Size = new System.Drawing.Size(21, 21);
            this.metroTextBox1.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.metroTextBox1.CustomButton.TabIndex = 1;
            this.metroTextBox1.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.metroTextBox1.CustomButton.UseSelectable = true;
            this.metroTextBox1.CustomButton.Visible = false;
            this.metroTextBox1.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::zanac.MAmidiMEmo.Properties.Settings.Default, "GitHubUserName", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.metroTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.metroTextBox1.Lines = new string[0];
            this.metroTextBox1.Location = new System.Drawing.Point(142, 3);
            this.metroTextBox1.MaxLength = 32767;
            this.metroTextBox1.Name = "metroTextBox1";
            this.metroTextBox1.PasswordChar = '●';
            this.metroTextBox1.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.metroTextBox1.SelectedText = "";
            this.metroTextBox1.SelectionLength = 0;
            this.metroTextBox1.SelectionStart = 0;
            this.metroTextBox1.ShortcutsEnabled = true;
            this.metroTextBox1.Size = new System.Drawing.Size(493, 23);
            this.metroTextBox1.TabIndex = 1;
            this.metroTextBox1.Text = global::zanac.MAmidiMEmo.Properties.Settings.Default.GitHubUserName;
            this.metroTextBox1.UseSelectable = true;
            this.metroTextBox1.UseSystemPasswordChar = true;
            this.metroTextBox1.WaterMark = "Enter your GitHub account here and token to to get a higher rate limit.";
            this.metroTextBox1.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.metroTextBox1.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            // 
            // metroTextBox2
            // 
            this.tableLayoutPanel2.SetColumnSpan(this.metroTextBox2, 3);
            // 
            // 
            // 
            this.metroTextBox2.CustomButton.Image = null;
            this.metroTextBox2.CustomButton.Location = new System.Drawing.Point(471, 1);
            this.metroTextBox2.CustomButton.Name = "";
            this.metroTextBox2.CustomButton.Size = new System.Drawing.Size(21, 21);
            this.metroTextBox2.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.metroTextBox2.CustomButton.TabIndex = 1;
            this.metroTextBox2.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.metroTextBox2.CustomButton.UseSelectable = true;
            this.metroTextBox2.CustomButton.Visible = false;
            this.metroTextBox2.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::zanac.MAmidiMEmo.Properties.Settings.Default, "GitHubPersonalAccessToken", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.metroTextBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.metroTextBox2.Lines = new string[0];
            this.metroTextBox2.Location = new System.Drawing.Point(142, 32);
            this.metroTextBox2.MaxLength = 32767;
            this.metroTextBox2.Name = "metroTextBox2";
            this.metroTextBox2.PasswordChar = '●';
            this.metroTextBox2.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.metroTextBox2.SelectedText = "";
            this.metroTextBox2.SelectionLength = 0;
            this.metroTextBox2.SelectionStart = 0;
            this.metroTextBox2.ShortcutsEnabled = true;
            this.metroTextBox2.Size = new System.Drawing.Size(493, 23);
            this.metroTextBox2.TabIndex = 3;
            this.metroTextBox2.Text = global::zanac.MAmidiMEmo.Properties.Settings.Default.GitHubPersonalAccessToken;
            this.metroTextBox2.UseSelectable = true;
            this.metroTextBox2.UseSystemPasswordChar = true;
            this.metroTextBox2.WaterMark = "Enter your personal access token here. (Needs \"read:packages\" permission)";
            this.metroTextBox2.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.metroTextBox2.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            // 
            // metroLink1
            // 
            this.metroLink1.AutoSize = true;
            this.metroLink1.Location = new System.Drawing.Point(3, 32);
            this.metroLink1.Name = "metroLink1";
            this.metroLink1.Size = new System.Drawing.Size(133, 23);
            this.metroLink1.TabIndex = 2;
            this.metroLink1.Text = "&Personal access token:";
            this.metroLink1.UseSelectable = true;
            this.metroLink1.Click += new System.EventHandler(this.metroLink1_Click);
            // 
            // FormDownloadTone
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(648, 482);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.tableLayoutPanel2);
            this.KeyPreview = true;
            this.Name = "FormDownloadTone";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Download a tone";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private MetroFramework.Controls.MetroButton buttonOK;
        private MetroFramework.Controls.MetroButton buttonCancel;
        private System.Windows.Forms.ListView metroListViewDir;
        private System.Windows.Forms.ListBox listBoxTones;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private MetroFramework.Controls.MetroLabel metroLabelDir;
        private MetroFramework.Controls.MetroLabel metroLabel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private MetroFramework.Controls.MetroButton metroButton1;
        private MetroFramework.Controls.MetroLabel metroLabelRemaining;
        private MetroFramework.Controls.MetroLabel metroLabel2;
        private MetroFramework.Controls.MetroTextBox metroTextBox1;
        private MetroFramework.Controls.MetroTextBox metroTextBox2;
        private MetroFramework.Controls.MetroLink metroLink1;
        private System.Windows.Forms.ImageList imageList1;
    }
}