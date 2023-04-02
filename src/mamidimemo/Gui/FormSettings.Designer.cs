namespace zanac.MAmidiMEmo.Gui
{
    partial class FormSettings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSettings));
            this.buttonOk = new MetroFramework.Controls.MetroButton();
            this.buttonCancel = new MetroFramework.Controls.MetroButton();
            this.labelRate = new MetroFramework.Controls.MetroLabel();
            this.label2 = new MetroFramework.Controls.MetroLabel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label3 = new MetroFramework.Controls.MetroLabel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.label6 = new MetroFramework.Controls.MetroLabel();
            this.label5 = new MetroFramework.Controls.MetroLabel();
            this.label4 = new MetroFramework.Controls.MetroLabel();
            this.label7 = new MetroFramework.Controls.MetroLabel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.button1 = new MetroFramework.Controls.MetroButton();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.metroLabel1 = new MetroFramework.Controls.MetroLabel();
            this.metroComboBox1 = new MetroFramework.Controls.MetroComboBox();
            this.comboBoxAudioLatency = new MetroFramework.Controls.MetroComboBox();
            this.comboBoxSoundType = new MetroFramework.Controls.MetroComboBox();
            this.comboBoxSampleRate = new System.Windows.Forms.ComboBox();
            this.textBoxPaLatency = new MetroFramework.Controls.MetroTextBox();
            this.textBoxPaDevice = new MetroFramework.Controls.MetroTextBox();
            this.textBoxPaApi = new MetroFramework.Controls.MetroTextBox();
            this.textBox1 = new MetroFramework.Controls.MetroTextBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOk
            // 
            resources.ApplyResources(this.buttonOk, "buttonOk");
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.UseSelectable = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // buttonCancel
            // 
            resources.ApplyResources(this.buttonCancel, "buttonCancel");
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.UseSelectable = true;
            // 
            // labelRate
            // 
            resources.ApplyResources(this.labelRate, "labelRate");
            this.labelRate.Name = "labelRate";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.comboBoxAudioLatency, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.comboBoxSoundType, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.labelRate, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.comboBoxSampleRate, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.label7, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.metroLabel1, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.metroComboBox1, 1, 5);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // groupBox1
            // 
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.tableLayoutPanel1.SetColumnSpan(this.groupBox1, 2);
            this.groupBox1.Controls.Add(this.tableLayoutPanel2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // tableLayoutPanel2
            // 
            resources.ApplyResources(this.tableLayoutPanel2, "tableLayoutPanel2");
            this.tableLayoutPanel2.Controls.Add(this.textBoxPaLatency, 1, 2);
            this.tableLayoutPanel2.Controls.Add(this.label6, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.textBoxPaDevice, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.label5, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.label4, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.textBoxPaApi, 1, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            // 
            // label6
            // 
            resources.ApplyResources(this.label6, "label6");
            this.label6.Name = "label6";
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // label7
            // 
            resources.ApplyResources(this.label7, "label7");
            this.label7.Name = "label7";
            // 
            // tableLayoutPanel3
            // 
            resources.ApplyResources(this.tableLayoutPanel3, "tableLayoutPanel3");
            this.tableLayoutPanel3.Controls.Add(this.button1, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.textBox1, 0, 0);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            // 
            // button1
            // 
            resources.ApplyResources(this.button1, "button1");
            this.button1.Name = "button1";
            this.button1.UseSelectable = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // folderBrowserDialog1
            // 
            resources.ApplyResources(this.folderBrowserDialog1, "folderBrowserDialog1");
            // 
            // metroLabel1
            // 
            resources.ApplyResources(this.metroLabel1, "metroLabel1");
            this.metroLabel1.Name = "metroLabel1";
            // 
            // metroComboBox1
            // 
            this.metroComboBox1.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", global::zanac.MAmidiMEmo.Properties.Settings.Default, "OctaveDisplay", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            resources.ApplyResources(this.metroComboBox1, "metroComboBox1");
            this.metroComboBox1.FormattingEnabled = true;
            this.metroComboBox1.Items.AddRange(new object[] {
            resources.GetString("metroComboBox1.Items"),
            resources.GetString("metroComboBox1.Items1")});
            this.metroComboBox1.Name = "metroComboBox1";
            this.metroComboBox1.TabIndex = global::zanac.MAmidiMEmo.Properties.Settings.Default.OctaveDisplay;
            this.metroComboBox1.UseSelectable = true;
            // 
            // comboBoxAudioLatency
            // 
            this.comboBoxAudioLatency.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", global::zanac.MAmidiMEmo.Properties.Settings.Default, "AudioLatency", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            resources.ApplyResources(this.comboBoxAudioLatency, "comboBoxAudioLatency");
            this.comboBoxAudioLatency.FormattingEnabled = true;
            this.comboBoxAudioLatency.Items.AddRange(new object[] {
            resources.GetString("comboBoxAudioLatency.Items"),
            resources.GetString("comboBoxAudioLatency.Items1"),
            resources.GetString("comboBoxAudioLatency.Items2"),
            resources.GetString("comboBoxAudioLatency.Items3")});
            this.comboBoxAudioLatency.Name = "comboBoxAudioLatency";
            this.comboBoxAudioLatency.TabIndex = global::zanac.MAmidiMEmo.Properties.Settings.Default.AudioLatency;
            this.comboBoxAudioLatency.UseSelectable = true;
            // 
            // comboBoxSoundType
            // 
            this.comboBoxSoundType.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", global::zanac.MAmidiMEmo.Properties.Settings.Default, "SoundType", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            resources.ApplyResources(this.comboBoxSoundType, "comboBoxSoundType");
            this.comboBoxSoundType.FormattingEnabled = true;
            this.comboBoxSoundType.Items.AddRange(new object[] {
            resources.GetString("comboBoxSoundType.Items"),
            resources.GetString("comboBoxSoundType.Items1"),
            resources.GetString("comboBoxSoundType.Items2"),
            resources.GetString("comboBoxSoundType.Items3")});
            this.comboBoxSoundType.Name = "comboBoxSoundType";
            this.comboBoxSoundType.TabIndex = global::zanac.MAmidiMEmo.Properties.Settings.Default.SoundType;
            this.comboBoxSoundType.UseSelectable = true;
            // 
            // comboBoxSampleRate
            // 
            this.comboBoxSampleRate.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::zanac.MAmidiMEmo.Properties.Settings.Default, "SampleRate", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            resources.ApplyResources(this.comboBoxSampleRate, "comboBoxSampleRate");
            this.comboBoxSampleRate.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.comboBoxSampleRate.FormattingEnabled = true;
            this.comboBoxSampleRate.Items.AddRange(new object[] {
            resources.GetString("comboBoxSampleRate.Items"),
            resources.GetString("comboBoxSampleRate.Items1"),
            resources.GetString("comboBoxSampleRate.Items2"),
            resources.GetString("comboBoxSampleRate.Items3"),
            resources.GetString("comboBoxSampleRate.Items4")});
            this.comboBoxSampleRate.Name = "comboBoxSampleRate";
            this.comboBoxSampleRate.Text = global::zanac.MAmidiMEmo.Properties.Settings.Default.SampleRate;
            this.comboBoxSampleRate.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.comboBoxSampleRate_DrawItem);
            this.comboBoxSampleRate.Validating += new System.ComponentModel.CancelEventHandler(this.comboBoxText_Validating);
            // 
            // textBoxPaLatency
            // 
            // 
            // 
            // 
            this.textBoxPaLatency.CustomButton.Image = ((System.Drawing.Image)(resources.GetObject("resource.Image")));
            this.textBoxPaLatency.CustomButton.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("resource.ImeMode")));
            this.textBoxPaLatency.CustomButton.Location = ((System.Drawing.Point)(resources.GetObject("resource.Location")));
            this.textBoxPaLatency.CustomButton.Name = "";
            this.textBoxPaLatency.CustomButton.Size = ((System.Drawing.Size)(resources.GetObject("resource.Size")));
            this.textBoxPaLatency.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.textBoxPaLatency.CustomButton.TabIndex = ((int)(resources.GetObject("resource.TabIndex")));
            this.textBoxPaLatency.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.textBoxPaLatency.CustomButton.UseSelectable = true;
            this.textBoxPaLatency.CustomButton.Visible = ((bool)(resources.GetObject("resource.Visible")));
            this.textBoxPaLatency.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::zanac.MAmidiMEmo.Properties.Settings.Default, "PaLatency", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            resources.ApplyResources(this.textBoxPaLatency, "textBoxPaLatency");
            this.textBoxPaLatency.FontSize = MetroFramework.MetroTextBoxSize.Medium;
            this.textBoxPaLatency.Lines = new string[] {
        "0.003334"};
            this.textBoxPaLatency.MaxLength = 32767;
            this.textBoxPaLatency.Name = "textBoxPaLatency";
            this.textBoxPaLatency.PasswordChar = '\0';
            this.textBoxPaLatency.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.textBoxPaLatency.SelectedText = "";
            this.textBoxPaLatency.SelectionLength = 0;
            this.textBoxPaLatency.SelectionStart = 0;
            this.textBoxPaLatency.ShortcutsEnabled = true;
            this.textBoxPaLatency.Text = global::zanac.MAmidiMEmo.Properties.Settings.Default.PaLatency;
            this.textBoxPaLatency.UseSelectable = true;
            this.textBoxPaLatency.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.textBoxPaLatency.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            this.textBoxPaLatency.Validating += new System.ComponentModel.CancelEventHandler(this.textBox_Validating);
            // 
            // textBoxPaDevice
            // 
            // 
            // 
            // 
            this.textBoxPaDevice.CustomButton.Image = ((System.Drawing.Image)(resources.GetObject("resource.Image1")));
            this.textBoxPaDevice.CustomButton.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("resource.ImeMode1")));
            this.textBoxPaDevice.CustomButton.Location = ((System.Drawing.Point)(resources.GetObject("resource.Location1")));
            this.textBoxPaDevice.CustomButton.Name = "";
            this.textBoxPaDevice.CustomButton.Size = ((System.Drawing.Size)(resources.GetObject("resource.Size1")));
            this.textBoxPaDevice.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.textBoxPaDevice.CustomButton.TabIndex = ((int)(resources.GetObject("resource.TabIndex1")));
            this.textBoxPaDevice.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.textBoxPaDevice.CustomButton.UseSelectable = true;
            this.textBoxPaDevice.CustomButton.Visible = ((bool)(resources.GetObject("resource.Visible1")));
            this.textBoxPaDevice.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::zanac.MAmidiMEmo.Properties.Settings.Default, "PaDevice", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            resources.ApplyResources(this.textBoxPaDevice, "textBoxPaDevice");
            this.textBoxPaDevice.FontSize = MetroFramework.MetroTextBoxSize.Medium;
            this.textBoxPaDevice.FontWeight = MetroFramework.MetroTextBoxWeight.Light;
            this.textBoxPaDevice.Lines = new string[] {
        "default"};
            this.textBoxPaDevice.MaxLength = 32767;
            this.textBoxPaDevice.Name = "textBoxPaDevice";
            this.textBoxPaDevice.PasswordChar = '\0';
            this.textBoxPaDevice.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.textBoxPaDevice.SelectedText = "";
            this.textBoxPaDevice.SelectionLength = 0;
            this.textBoxPaDevice.SelectionStart = 0;
            this.textBoxPaDevice.ShortcutsEnabled = true;
            this.textBoxPaDevice.Text = global::zanac.MAmidiMEmo.Properties.Settings.Default.PaDevice;
            this.textBoxPaDevice.UseSelectable = true;
            this.textBoxPaDevice.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.textBoxPaDevice.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            this.textBoxPaDevice.Validating += new System.ComponentModel.CancelEventHandler(this.textBox_Validating);
            // 
            // textBoxPaApi
            // 
            // 
            // 
            // 
            this.textBoxPaApi.CustomButton.Image = ((System.Drawing.Image)(resources.GetObject("resource.Image2")));
            this.textBoxPaApi.CustomButton.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("resource.ImeMode2")));
            this.textBoxPaApi.CustomButton.Location = ((System.Drawing.Point)(resources.GetObject("resource.Location2")));
            this.textBoxPaApi.CustomButton.Name = "";
            this.textBoxPaApi.CustomButton.Size = ((System.Drawing.Size)(resources.GetObject("resource.Size2")));
            this.textBoxPaApi.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.textBoxPaApi.CustomButton.TabIndex = ((int)(resources.GetObject("resource.TabIndex2")));
            this.textBoxPaApi.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.textBoxPaApi.CustomButton.UseSelectable = true;
            this.textBoxPaApi.CustomButton.Visible = ((bool)(resources.GetObject("resource.Visible2")));
            this.textBoxPaApi.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::zanac.MAmidiMEmo.Properties.Settings.Default, "PaApi", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            resources.ApplyResources(this.textBoxPaApi, "textBoxPaApi");
            this.textBoxPaApi.FontSize = MetroFramework.MetroTextBoxSize.Medium;
            this.textBoxPaApi.Lines = new string[] {
        "\"Windows WASAPI\""};
            this.textBoxPaApi.MaxLength = 32767;
            this.textBoxPaApi.Name = "textBoxPaApi";
            this.textBoxPaApi.PasswordChar = '\0';
            this.textBoxPaApi.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.textBoxPaApi.SelectedText = "";
            this.textBoxPaApi.SelectionLength = 0;
            this.textBoxPaApi.SelectionStart = 0;
            this.textBoxPaApi.ShortcutsEnabled = true;
            this.textBoxPaApi.Text = global::zanac.MAmidiMEmo.Properties.Settings.Default.PaApi;
            this.textBoxPaApi.UseSelectable = true;
            this.textBoxPaApi.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.textBoxPaApi.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            this.textBoxPaApi.Validating += new System.ComponentModel.CancelEventHandler(this.textBox_Validating);
            // 
            // textBox1
            // 
            // 
            // 
            // 
            this.textBox1.CustomButton.Image = ((System.Drawing.Image)(resources.GetObject("resource.Image3")));
            this.textBox1.CustomButton.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("resource.ImeMode3")));
            this.textBox1.CustomButton.Location = ((System.Drawing.Point)(resources.GetObject("resource.Location3")));
            this.textBox1.CustomButton.Name = "";
            this.textBox1.CustomButton.Size = ((System.Drawing.Size)(resources.GetObject("resource.Size3")));
            this.textBox1.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.textBox1.CustomButton.TabIndex = ((int)(resources.GetObject("resource.TabIndex3")));
            this.textBox1.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.textBox1.CustomButton.UseSelectable = true;
            this.textBox1.CustomButton.Visible = ((bool)(resources.GetObject("resource.Visible3")));
            this.textBox1.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::zanac.MAmidiMEmo.Properties.Settings.Default, "OutputDir", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            resources.ApplyResources(this.textBox1, "textBox1");
            this.textBox1.FontSize = MetroFramework.MetroTextBoxSize.Medium;
            this.textBox1.Lines = new string[0];
            this.textBox1.MaxLength = 32767;
            this.textBox1.Name = "textBox1";
            this.textBox1.PasswordChar = '\0';
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.textBox1.SelectedText = "";
            this.textBox1.SelectionLength = 0;
            this.textBox1.SelectionStart = 0;
            this.textBox1.ShortcutsEnabled = true;
            this.textBox1.Text = global::zanac.MAmidiMEmo.Properties.Settings.Default.OutputDir;
            this.textBox1.UseSelectable = true;
            this.textBox1.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.textBox1.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            // 
            // FormSettings
            // 
            this.AcceptButton = this.buttonOk;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.buttonCancel);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormSettings";
            this.Resizable = false;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private MetroFramework.Controls.MetroButton buttonOk;
        private MetroFramework.Controls.MetroButton buttonCancel;
        private MetroFramework.Controls.MetroLabel labelRate;
        private MetroFramework.Controls.MetroLabel label2;
        private MetroFramework.Controls.MetroComboBox comboBoxSoundType;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ComboBox comboBoxSampleRate;
        private MetroFramework.Controls.MetroLabel label3;
        private MetroFramework.Controls.MetroComboBox comboBoxAudioLatency;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private MetroFramework.Controls.MetroLabel label4;
        private MetroFramework.Controls.MetroTextBox textBoxPaApi;
        private MetroFramework.Controls.MetroTextBox textBoxPaDevice;
        private MetroFramework.Controls.MetroLabel label5;
        private MetroFramework.Controls.MetroLabel label6;
        private MetroFramework.Controls.MetroTextBox textBoxPaLatency;
        private MetroFramework.Controls.MetroLabel label7;
        private MetroFramework.Controls.MetroTextBox textBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private MetroFramework.Controls.MetroButton button1;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private MetroFramework.Controls.MetroLabel metroLabel1;
        private MetroFramework.Controls.MetroComboBox metroComboBox1;
    }
}