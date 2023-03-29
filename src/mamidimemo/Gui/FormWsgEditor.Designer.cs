namespace zanac.MAmidiMEmo.Gui
{
    partial class FormWsgEditor
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
            this.graphControl = new zanac.MAmidiMEmo.Gui.FormWsgEditor.GraphControl();
            this.buttonCancel = new MetroFramework.Controls.MetroButton();
            this.buttonOk = new MetroFramework.Controls.MetroButton();
            this.textBoxWsgDataText = new MetroFramework.Controls.MetroTextBox();
            this.metroButtonRand1 = new MetroFramework.Controls.MetroButton();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.metroButtonRandom2 = new MetroFramework.Controls.MetroButton();
            this.metroButtonFir1 = new MetroFramework.Controls.MetroButton();
            this.metroTextBoxFirWeight = new MetroFramework.Controls.MetroTextBox();
            this.metroButtonMax = new MetroFramework.Controls.MetroButton();
            this.metroButtonSin = new MetroFramework.Controls.MetroButton();
            this.metroButtonSaw = new MetroFramework.Controls.MetroButton();
            this.metroButtonSq = new MetroFramework.Controls.MetroButton();
            this.metroButtonTri = new MetroFramework.Controls.MetroButton();
            this.checkBoxHex = new System.Windows.Forms.CheckBox();
            this.checkBoxTransparent = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // graphControl
            // 
            this.graphControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.graphControl, 9);
            this.graphControl.Editor = null;
            this.graphControl.Location = new System.Drawing.Point(4, 4);
            this.graphControl.Margin = new System.Windows.Forms.Padding(4);
            this.graphControl.Name = "graphControl";
            this.graphControl.ResultOfWsgData = null;
            this.graphControl.Size = new System.Drawing.Size(898, 505);
            this.graphControl.TabIndex = 0;
            this.graphControl.WsgBitWide = 4;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(802, 624);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(4);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(100, 29);
            this.buttonCancel.TabIndex = 13;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseSelectable = true;
            // 
            // buttonOk
            // 
            this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOk.Location = new System.Drawing.Point(694, 624);
            this.buttonOk.Margin = new System.Windows.Forms.Padding(4);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(100, 29);
            this.buttonOk.TabIndex = 12;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseSelectable = true;
            // 
            // textBoxWsgDataText
            // 
            this.textBoxWsgDataText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.textBoxWsgDataText, 8);
            // 
            // 
            // 
            this.textBoxWsgDataText.CustomButton.Image = null;
            this.textBoxWsgDataText.CustomButton.Location = new System.Drawing.Point(763, 1);
            this.textBoxWsgDataText.CustomButton.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxWsgDataText.CustomButton.Name = "";
            this.textBoxWsgDataText.CustomButton.Size = new System.Drawing.Size(23, 23);
            this.textBoxWsgDataText.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.textBoxWsgDataText.CustomButton.TabIndex = 1;
            this.textBoxWsgDataText.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.textBoxWsgDataText.CustomButton.UseSelectable = true;
            this.textBoxWsgDataText.CustomButton.Visible = false;
            this.textBoxWsgDataText.Lines = new string[0];
            this.textBoxWsgDataText.Location = new System.Drawing.Point(115, 517);
            this.textBoxWsgDataText.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxWsgDataText.MaxLength = 32767;
            this.textBoxWsgDataText.Name = "textBoxWsgDataText";
            this.textBoxWsgDataText.PasswordChar = '\0';
            this.textBoxWsgDataText.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.textBoxWsgDataText.SelectedText = "";
            this.textBoxWsgDataText.SelectionLength = 0;
            this.textBoxWsgDataText.SelectionStart = 0;
            this.textBoxWsgDataText.ShortcutsEnabled = true;
            this.textBoxWsgDataText.Size = new System.Drawing.Size(787, 25);
            this.textBoxWsgDataText.TabIndex = 2;
            this.textBoxWsgDataText.UseSelectable = true;
            this.textBoxWsgDataText.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.textBoxWsgDataText.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            this.textBoxWsgDataText.TextChanged += new System.EventHandler(this.textBoxWsgDataText_TextChanged);
            // 
            // metroButtonRand1
            // 
            this.metroButtonRand1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.metroButtonRand1.Location = new System.Drawing.Point(4, 550);
            this.metroButtonRand1.Margin = new System.Windows.Forms.Padding(4);
            this.metroButtonRand1.Name = "metroButtonRand1";
            this.metroButtonRand1.Size = new System.Drawing.Size(100, 29);
            this.metroButtonRand1.TabIndex = 3;
            this.metroButtonRand1.Text = "&Random1";
            this.metroButtonRand1.UseSelectable = true;
            this.metroButtonRand1.Click += new System.EventHandler(this.metroButtonRand1_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 9;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.buttonCancel, 8, 4);
            this.tableLayoutPanel1.Controls.Add(this.graphControl, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.metroButtonRand1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.buttonOk, 7, 4);
            this.tableLayoutPanel1.Controls.Add(this.textBoxWsgDataText, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.metroButtonRandom2, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.metroButtonFir1, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.metroTextBoxFirWeight, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.metroButtonMax, 8, 2);
            this.tableLayoutPanel1.Controls.Add(this.metroButtonSin, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.metroButtonSaw, 3, 2);
            this.tableLayoutPanel1.Controls.Add(this.metroButtonSq, 4, 2);
            this.tableLayoutPanel1.Controls.Add(this.metroButtonTri, 5, 2);
            this.tableLayoutPanel1.Controls.Add(this.checkBoxHex, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.checkBoxTransparent, 0, 4);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(9, 94);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 5;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(906, 657);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // metroButtonRandom2
            // 
            this.metroButtonRandom2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.metroButtonRandom2.Location = new System.Drawing.Point(115, 550);
            this.metroButtonRandom2.Margin = new System.Windows.Forms.Padding(4);
            this.metroButtonRandom2.Name = "metroButtonRandom2";
            this.metroButtonRandom2.Size = new System.Drawing.Size(100, 29);
            this.metroButtonRandom2.TabIndex = 4;
            this.metroButtonRandom2.Text = "R&andom2";
            this.metroButtonRandom2.UseSelectable = true;
            this.metroButtonRandom2.Click += new System.EventHandler(this.metroButtonRandom2_Click);
            // 
            // metroButtonFir1
            // 
            this.metroButtonFir1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.metroButtonFir1.Location = new System.Drawing.Point(4, 587);
            this.metroButtonFir1.Margin = new System.Windows.Forms.Padding(4);
            this.metroButtonFir1.Name = "metroButtonFir1";
            this.metroButtonFir1.Size = new System.Drawing.Size(100, 29);
            this.metroButtonFir1.TabIndex = 10;
            this.metroButtonFir1.Text = "&FIR";
            this.metroButtonFir1.UseSelectable = true;
            this.metroButtonFir1.Click += new System.EventHandler(this.metroButtonFir1_Click);
            // 
            // metroTextBoxFirWeight
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.metroTextBoxFirWeight, 8);
            // 
            // 
            // 
            this.metroTextBoxFirWeight.CustomButton.Image = null;
            this.metroTextBoxFirWeight.CustomButton.Location = new System.Drawing.Point(759, 1);
            this.metroTextBoxFirWeight.CustomButton.Margin = new System.Windows.Forms.Padding(4);
            this.metroTextBoxFirWeight.CustomButton.Name = "";
            this.metroTextBoxFirWeight.CustomButton.Size = new System.Drawing.Size(27, 27);
            this.metroTextBoxFirWeight.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.metroTextBoxFirWeight.CustomButton.TabIndex = 1;
            this.metroTextBoxFirWeight.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.metroTextBoxFirWeight.CustomButton.UseSelectable = true;
            this.metroTextBoxFirWeight.CustomButton.Visible = false;
            this.metroTextBoxFirWeight.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::zanac.MAmidiMEmo.Properties.Settings.Default, "WsgFirWeight", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.metroTextBoxFirWeight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.metroTextBoxFirWeight.Lines = new string[] {
        " 1, 1, 1, 1, 1 "};
            this.metroTextBoxFirWeight.Location = new System.Drawing.Point(115, 587);
            this.metroTextBoxFirWeight.Margin = new System.Windows.Forms.Padding(4);
            this.metroTextBoxFirWeight.MaxLength = 32767;
            this.metroTextBoxFirWeight.Name = "metroTextBoxFirWeight";
            this.metroTextBoxFirWeight.PasswordChar = '\0';
            this.metroTextBoxFirWeight.PromptText = "Set FIR weights like \"1,1,1,1,1\"";
            this.metroTextBoxFirWeight.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.metroTextBoxFirWeight.SelectedText = "";
            this.metroTextBoxFirWeight.SelectionLength = 0;
            this.metroTextBoxFirWeight.SelectionStart = 0;
            this.metroTextBoxFirWeight.ShortcutsEnabled = true;
            this.metroTextBoxFirWeight.Size = new System.Drawing.Size(787, 29);
            this.metroTextBoxFirWeight.TabIndex = 11;
            this.metroTextBoxFirWeight.Text = global::zanac.MAmidiMEmo.Properties.Settings.Default.WsgFirWeight;
            this.metroTextBoxFirWeight.UseSelectable = true;
            this.metroTextBoxFirWeight.WaterMark = "Set FIR weights like \"1,1,1,1,1\"";
            this.metroTextBoxFirWeight.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.metroTextBoxFirWeight.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            // 
            // metroButtonMax
            // 
            this.metroButtonMax.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.metroButtonMax.Location = new System.Drawing.Point(802, 550);
            this.metroButtonMax.Margin = new System.Windows.Forms.Padding(4);
            this.metroButtonMax.Name = "metroButtonMax";
            this.metroButtonMax.Size = new System.Drawing.Size(100, 29);
            this.metroButtonMax.TabIndex = 9;
            this.metroButtonMax.Text = "&Maximize";
            this.metroButtonMax.UseSelectable = true;
            this.metroButtonMax.Click += new System.EventHandler(this.metroButtonMax_Click);
            // 
            // metroButtonSin
            // 
            this.metroButtonSin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.metroButtonSin.Location = new System.Drawing.Point(223, 550);
            this.metroButtonSin.Margin = new System.Windows.Forms.Padding(4);
            this.metroButtonSin.Name = "metroButtonSin";
            this.metroButtonSin.Size = new System.Drawing.Size(100, 29);
            this.metroButtonSin.TabIndex = 5;
            this.metroButtonSin.Text = "&Sin";
            this.metroButtonSin.UseSelectable = true;
            this.metroButtonSin.Click += new System.EventHandler(this.metroButtonSin_Click);
            // 
            // metroButtonSaw
            // 
            this.metroButtonSaw.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.metroButtonSaw.Location = new System.Drawing.Point(331, 550);
            this.metroButtonSaw.Margin = new System.Windows.Forms.Padding(4);
            this.metroButtonSaw.Name = "metroButtonSaw";
            this.metroButtonSaw.Size = new System.Drawing.Size(100, 29);
            this.metroButtonSaw.TabIndex = 6;
            this.metroButtonSaw.Text = "Sa&wTooth";
            this.metroButtonSaw.UseSelectable = true;
            this.metroButtonSaw.Click += new System.EventHandler(this.metroButtonSaw_Click);
            // 
            // metroButtonSq
            // 
            this.metroButtonSq.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.metroButtonSq.Location = new System.Drawing.Point(439, 550);
            this.metroButtonSq.Margin = new System.Windows.Forms.Padding(4);
            this.metroButtonSq.Name = "metroButtonSq";
            this.metroButtonSq.Size = new System.Drawing.Size(100, 29);
            this.metroButtonSq.TabIndex = 7;
            this.metroButtonSq.Text = "&Square";
            this.metroButtonSq.UseSelectable = true;
            this.metroButtonSq.Click += new System.EventHandler(this.metroButtonSq_Click);
            // 
            // metroButtonTri
            // 
            this.metroButtonTri.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.metroButtonTri.Location = new System.Drawing.Point(547, 550);
            this.metroButtonTri.Margin = new System.Windows.Forms.Padding(4);
            this.metroButtonTri.Name = "metroButtonTri";
            this.metroButtonTri.Size = new System.Drawing.Size(100, 29);
            this.metroButtonTri.TabIndex = 8;
            this.metroButtonTri.Text = "&Triangle";
            this.metroButtonTri.UseSelectable = true;
            this.metroButtonTri.Click += new System.EventHandler(this.metroButtonTri_Click);
            // 
            // checkBoxHex
            // 
            this.checkBoxHex.AutoSize = true;
            this.checkBoxHex.Checked = global::zanac.MAmidiMEmo.Properties.Settings.Default.WsgHex;
            this.checkBoxHex.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::zanac.MAmidiMEmo.Properties.Settings.Default, "WsgHex", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.checkBoxHex.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxHex.Location = new System.Drawing.Point(3, 516);
            this.checkBoxHex.Name = "checkBoxHex";
            this.checkBoxHex.Size = new System.Drawing.Size(105, 27);
            this.checkBoxHex.TabIndex = 1;
            this.checkBoxHex.Text = "&Hex";
            this.checkBoxHex.UseVisualStyleBackColor = true;
            this.checkBoxHex.CheckedChanged += new System.EventHandler(this.checkBoxHex_CheckedChanged);
            // 
            // checkBoxTransparent
            // 
            this.checkBoxTransparent.AutoSize = true;
            this.checkBoxTransparent.Checked = global::zanac.MAmidiMEmo.Properties.Settings.Default.WsgTransparent;
            this.checkBoxTransparent.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::zanac.MAmidiMEmo.Properties.Settings.Default, "WsgTransparent", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.checkBoxTransparent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxTransparent.Location = new System.Drawing.Point(3, 623);
            this.checkBoxTransparent.Name = "checkBoxTransparent";
            this.checkBoxTransparent.Size = new System.Drawing.Size(105, 31);
            this.checkBoxTransparent.TabIndex = 14;
            this.checkBoxTransparent.Text = "Trans&parent";
            this.checkBoxTransparent.UseVisualStyleBackColor = true;
            this.checkBoxTransparent.CheckedChanged += new System.EventHandler(this.checkBoxTransparent_CheckedChanged);
            // 
            // FormWsgEditor
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(924, 766);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.MinimizeBox = false;
            this.Name = "FormWsgEditor";
            this.Padding = new System.Windows.Forms.Padding(9, 94, 9, 15);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "WSG Editor";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private GraphControl graphControl;
        private MetroFramework.Controls.MetroButton buttonCancel;
        private MetroFramework.Controls.MetroButton buttonOk;
        private MetroFramework.Controls.MetroTextBox textBoxWsgDataText;
        private MetroFramework.Controls.MetroButton metroButtonRand1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private MetroFramework.Controls.MetroButton metroButtonRandom2;
        private MetroFramework.Controls.MetroButton metroButtonFir1;
        private MetroFramework.Controls.MetroTextBox metroTextBoxFirWeight;
        private MetroFramework.Controls.MetroButton metroButtonMax;
        private MetroFramework.Controls.MetroButton metroButtonSin;
        private MetroFramework.Controls.MetroButton metroButtonSaw;
        private MetroFramework.Controls.MetroButton metroButtonSq;
        private MetroFramework.Controls.MetroButton metroButtonTri;
        private System.Windows.Forms.CheckBox checkBoxHex;
        private System.Windows.Forms.CheckBox checkBoxTransparent;
    }
}