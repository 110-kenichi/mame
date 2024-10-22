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
            this.checkBoxHex = new MetroFramework.Controls.MetroCheckBox();
            this.checkBoxTransparent = new MetroFramework.Controls.MetroCheckBox();
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
            this.graphControl.Size = new System.Drawing.Size(906, 564);
            this.graphControl.TabIndex = 0;
            this.graphControl.WsgBitWide = 4;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(810, 669);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(4);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(100, 23);
            this.buttonCancel.TabIndex = 13;
            this.buttonCancel.Text = "Cancel";
            // 
            // buttonOk
            // 
            this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOk.Location = new System.Drawing.Point(702, 669);
            this.buttonOk.Margin = new System.Windows.Forms.Padding(4);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(100, 23);
            this.buttonOk.TabIndex = 12;
            this.buttonOk.Text = "&OK";
            // 
            // textBoxWsgDataText
            // 
            this.textBoxWsgDataText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.textBoxWsgDataText, 8);
            this.textBoxWsgDataText.Location = new System.Drawing.Point(112, 576);
            this.textBoxWsgDataText.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxWsgDataText.Name = "textBoxWsgDataText";
            this.textBoxWsgDataText.Size = new System.Drawing.Size(798, 23);
            this.textBoxWsgDataText.TabIndex = 2;
            this.textBoxWsgDataText.TextChanged += new System.EventHandler(this.textBoxWsgDataText_TextChanged);
            // 
            // metroButtonRand1
            // 
            this.metroButtonRand1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.metroButtonRand1.Location = new System.Drawing.Point(4, 607);
            this.metroButtonRand1.Margin = new System.Windows.Forms.Padding(4);
            this.metroButtonRand1.Name = "metroButtonRand1";
            this.metroButtonRand1.Size = new System.Drawing.Size(100, 23);
            this.metroButtonRand1.TabIndex = 3;
            this.metroButtonRand1.Text = "&Random1";
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
            this.tableLayoutPanel1.Location = new System.Drawing.Point(5, 60);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 5;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(914, 696);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // metroButtonRandom2
            // 
            this.metroButtonRandom2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.metroButtonRandom2.Location = new System.Drawing.Point(112, 607);
            this.metroButtonRandom2.Margin = new System.Windows.Forms.Padding(4);
            this.metroButtonRandom2.Name = "metroButtonRandom2";
            this.metroButtonRandom2.Size = new System.Drawing.Size(100, 23);
            this.metroButtonRandom2.TabIndex = 4;
            this.metroButtonRandom2.Text = "R&andom2";
            this.metroButtonRandom2.Click += new System.EventHandler(this.metroButtonRandom2_Click);
            // 
            // metroButtonFir1
            // 
            this.metroButtonFir1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.metroButtonFir1.Location = new System.Drawing.Point(4, 638);
            this.metroButtonFir1.Margin = new System.Windows.Forms.Padding(4);
            this.metroButtonFir1.Name = "metroButtonFir1";
            this.metroButtonFir1.Size = new System.Drawing.Size(100, 23);
            this.metroButtonFir1.TabIndex = 10;
            this.metroButtonFir1.Text = "&FIR";
            this.metroButtonFir1.Click += new System.EventHandler(this.metroButtonFir1_Click);
            // 
            // metroTextBoxFirWeight
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.metroTextBoxFirWeight, 8);
            this.metroTextBoxFirWeight.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::zanac.MAmidiMEmo.Properties.Settings.Default, "WsgFirWeight", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.metroTextBoxFirWeight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.metroTextBoxFirWeight.Location = new System.Drawing.Point(112, 638);
            this.metroTextBoxFirWeight.Margin = new System.Windows.Forms.Padding(4);
            this.metroTextBoxFirWeight.Name = "metroTextBoxFirWeight";
            this.metroTextBoxFirWeight.PromptText = "Set FIR weights like \"1,1,1,1,1\"";
            this.metroTextBoxFirWeight.Size = new System.Drawing.Size(798, 23);
            this.metroTextBoxFirWeight.TabIndex = 11;
            this.metroTextBoxFirWeight.Text = global::zanac.MAmidiMEmo.Properties.Settings.Default.WsgFirWeight;
            // 
            // metroButtonMax
            // 
            this.metroButtonMax.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.metroButtonMax.Location = new System.Drawing.Point(810, 607);
            this.metroButtonMax.Margin = new System.Windows.Forms.Padding(4);
            this.metroButtonMax.Name = "metroButtonMax";
            this.metroButtonMax.Size = new System.Drawing.Size(100, 23);
            this.metroButtonMax.TabIndex = 9;
            this.metroButtonMax.Text = "&Maximize";
            this.metroButtonMax.Click += new System.EventHandler(this.metroButtonMax_Click);
            // 
            // metroButtonSin
            // 
            this.metroButtonSin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.metroButtonSin.Location = new System.Drawing.Point(220, 607);
            this.metroButtonSin.Margin = new System.Windows.Forms.Padding(4);
            this.metroButtonSin.Name = "metroButtonSin";
            this.metroButtonSin.Size = new System.Drawing.Size(100, 23);
            this.metroButtonSin.TabIndex = 5;
            this.metroButtonSin.Text = "&Sin";
            this.metroButtonSin.Click += new System.EventHandler(this.metroButtonSin_Click);
            // 
            // metroButtonSaw
            // 
            this.metroButtonSaw.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.metroButtonSaw.Location = new System.Drawing.Point(328, 607);
            this.metroButtonSaw.Margin = new System.Windows.Forms.Padding(4);
            this.metroButtonSaw.Name = "metroButtonSaw";
            this.metroButtonSaw.Size = new System.Drawing.Size(100, 23);
            this.metroButtonSaw.TabIndex = 6;
            this.metroButtonSaw.Text = "Sa&wTooth";
            this.metroButtonSaw.Click += new System.EventHandler(this.metroButtonSaw_Click);
            // 
            // metroButtonSq
            // 
            this.metroButtonSq.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.metroButtonSq.Location = new System.Drawing.Point(436, 607);
            this.metroButtonSq.Margin = new System.Windows.Forms.Padding(4);
            this.metroButtonSq.Name = "metroButtonSq";
            this.metroButtonSq.Size = new System.Drawing.Size(100, 23);
            this.metroButtonSq.TabIndex = 7;
            this.metroButtonSq.Text = "&Square";
            this.metroButtonSq.Click += new System.EventHandler(this.metroButtonSq_Click);
            // 
            // metroButtonTri
            // 
            this.metroButtonTri.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.metroButtonTri.Location = new System.Drawing.Point(544, 607);
            this.metroButtonTri.Margin = new System.Windows.Forms.Padding(4);
            this.metroButtonTri.Name = "metroButtonTri";
            this.metroButtonTri.Size = new System.Drawing.Size(100, 23);
            this.metroButtonTri.TabIndex = 8;
            this.metroButtonTri.Text = "&Triangle";
            this.metroButtonTri.Click += new System.EventHandler(this.metroButtonTri_Click);
            // 
            // checkBoxHex
            // 
            this.checkBoxHex.AutoSize = true;
            this.checkBoxHex.Checked = global::zanac.MAmidiMEmo.Properties.Settings.Default.WsgHex;
            this.checkBoxHex.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::zanac.MAmidiMEmo.Properties.Settings.Default, "WsgHex", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.checkBoxHex.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxHex.Location = new System.Drawing.Point(3, 575);
            this.checkBoxHex.Name = "checkBoxHex";
            this.checkBoxHex.Size = new System.Drawing.Size(102, 25);
            this.checkBoxHex.TabIndex = 1;
            this.checkBoxHex.Text = "&Hex";
            this.checkBoxHex.CheckedChanged += new System.EventHandler(this.checkBoxHex_CheckedChanged);
            // 
            // checkBoxTransparent
            // 
            this.checkBoxTransparent.AutoSize = true;
            this.checkBoxTransparent.Checked = global::zanac.MAmidiMEmo.Properties.Settings.Default.WsgTransparent;
            this.checkBoxTransparent.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::zanac.MAmidiMEmo.Properties.Settings.Default, "WsgTransparent", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.checkBoxTransparent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxTransparent.Location = new System.Drawing.Point(3, 668);
            this.checkBoxTransparent.Name = "checkBoxTransparent";
            this.checkBoxTransparent.Size = new System.Drawing.Size(102, 25);
            this.checkBoxTransparent.TabIndex = 14;
            this.checkBoxTransparent.Text = "Trans&parent";
            this.checkBoxTransparent.CheckedChanged += new System.EventHandler(this.checkBoxTransparent_CheckedChanged);
            // 
            // FormWsgEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(924, 766);
            this.Controls.Add(this.tableLayoutPanel1);
            this.MinimizeBox = false;
            this.Name = "FormWsgEditor";
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
        private MetroFramework.Controls.MetroCheckBox checkBoxHex;
        private MetroFramework.Controls.MetroCheckBox checkBoxTransparent;
    }
}