namespace zanac.MAmidiMEmo.Gui.FMEditor
{
    partial class FormFmEditor
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
            if (disposing)
            {
                Properties.Settings.Default.SettingsLoaded -= Default_SettingsLoaded;
                MAmidiMEmo.Instruments.InstrumentManager.InstrumentChanged -= InstrumentManager_InstrumentChanged;
                MAmidiMEmo.Instruments.InstrumentManager.InstrumentRemoved -= InstrumentManager_InstrumentRemoved;
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
            this.panelPiano = new System.Windows.Forms.Panel();
            this.pianoControl1 = new zanac.MAmidiMEmo.Gui.PianoControl();
            this.toolStrip2 = new zanac.MAmidiMEmo.ComponentModel.ToolStripBase();
            this.toolStripComboBoxCh = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripButtonPlay = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripComboBoxNote = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripLabel4 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripComboBoxVelo = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripLabel3 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripComboBoxGate = new System.Windows.Forms.ToolStripComboBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.buttonCancel = new MetroFramework.Controls.MetroButton();
            this.buttonOK = new MetroFramework.Controls.MetroButton();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.panel1 = new System.Windows.Forms.Panel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.panelPiano.SuspendLayout();
            this.toolStrip2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelPiano
            // 
            this.panelPiano.Controls.Add(this.pianoControl1);
            this.panelPiano.Controls.Add(this.toolStrip2);
            this.panelPiano.Controls.Add(this.tableLayoutPanel1);
            this.panelPiano.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelPiano.Location = new System.Drawing.Point(5, 511);
            this.panelPiano.Name = "panelPiano";
            this.panelPiano.Size = new System.Drawing.Size(793, 143);
            this.panelPiano.TabIndex = 4;
            // 
            // pianoControl1
            // 
            this.pianoControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pianoControl1.Location = new System.Drawing.Point(0, 25);
            this.pianoControl1.Name = "pianoControl1";
            this.pianoControl1.Size = new System.Drawing.Size(793, 82);
            this.pianoControl1.TabIndex = 0;
            // 
            // toolStrip2
            // 
            this.toolStrip2.ClickThrough = false;
            this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripComboBoxCh,
            this.toolStripLabel2,
            this.toolStripButtonPlay,
            this.toolStripSeparator1,
            this.toolStripLabel1,
            this.toolStripComboBoxNote,
            this.toolStripLabel4,
            this.toolStripComboBoxVelo,
            this.toolStripLabel3,
            this.toolStripComboBoxGate});
            this.toolStrip2.Location = new System.Drawing.Point(0, 0);
            this.toolStrip2.Name = "toolStrip2";
            this.toolStrip2.Size = new System.Drawing.Size(793, 25);
            this.toolStrip2.TabIndex = 2;
            // 
            // toolStripComboBoxCh
            // 
            this.toolStripComboBoxCh.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripComboBoxCh.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBoxCh.DropDownWidth = 32;
            this.toolStripComboBoxCh.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "11",
            "12",
            "13",
            "14",
            "15"});
            this.toolStripComboBoxCh.Name = "toolStripComboBoxCh";
            this.toolStripComboBoxCh.Size = new System.Drawing.Size(75, 25);
            this.toolStripComboBoxCh.SelectedIndexChanged += new System.EventHandler(this.toolStripComboBox1_SelectedIndexChanged);
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Size = new System.Drawing.Size(43, 22);
            this.toolStripLabel2.Text = "Key Ch";
            // 
            // toolStripButtonPlay
            // 
            this.toolStripButtonPlay.CheckOnClick = true;
            this.toolStripButtonPlay.Image = global::zanac.MAmidiMEmo.Properties.Resources.Inst;
            this.toolStripButtonPlay.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonPlay.Name = "toolStripButtonPlay";
            this.toolStripButtonPlay.Size = new System.Drawing.Size(89, 22);
            this.toolStripButtonPlay.Text = "Play on edit";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(33, 22);
            this.toolStripLabel1.Text = "Note";
            // 
            // toolStripComboBoxNote
            // 
            this.toolStripComboBoxNote.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBoxNote.Name = "toolStripComboBoxNote";
            this.toolStripComboBoxNote.Size = new System.Drawing.Size(75, 25);
            // 
            // toolStripLabel4
            // 
            this.toolStripLabel4.Name = "toolStripLabel4";
            this.toolStripLabel4.Size = new System.Drawing.Size(48, 22);
            this.toolStripLabel4.Text = "Velocity";
            // 
            // toolStripComboBoxVelo
            // 
            this.toolStripComboBoxVelo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBoxVelo.Name = "toolStripComboBoxVelo";
            this.toolStripComboBoxVelo.Size = new System.Drawing.Size(75, 25);
            // 
            // toolStripLabel3
            // 
            this.toolStripLabel3.Name = "toolStripLabel3";
            this.toolStripLabel3.Size = new System.Drawing.Size(31, 22);
            this.toolStripLabel3.Text = "Gate";
            // 
            // toolStripComboBoxGate
            // 
            this.toolStripComboBoxGate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBoxGate.DropDownWidth = 100;
            this.toolStripComboBoxGate.Items.AddRange(new object[] {
            "500ms",
            "1000ms",
            "2000ms",
            "5000ms"});
            this.toolStripComboBoxGate.Name = "toolStripComboBoxGate";
            this.toolStripComboBoxGate.Size = new System.Drawing.Size(75, 25);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.buttonCancel, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.buttonOK, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 107);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(793, 36);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(713, 5);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 0;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseSelectable = true;
            // 
            // buttonOK
            // 
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(632, 5);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 0;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseSelectable = true;
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.Controls.Add(this.flowLayoutPanel1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(5, 60);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(3);
            this.panel1.Size = new System.Drawing.Size(793, 451);
            this.panel1.TabIndex = 5;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(6, 6);
            this.flowLayoutPanel1.MinimumSize = new System.Drawing.Size(50, 50);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(50, 50);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // FormFmEditor
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(803, 664);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panelPiano);
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormFmEditor";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "FM Synthesis Editor";
            this.panelPiano.ResumeLayout(false);
            this.panelPiano.PerformLayout();
            this.toolStrip2.ResumeLayout(false);
            this.toolStrip2.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panelPiano;
        private PianoControl pianoControl1;
        private ComponentModel.ToolStripBase toolStrip2;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxCh;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private MetroFramework.Controls.MetroButton buttonCancel;
        private MetroFramework.Controls.MetroButton buttonOK;
        private System.Windows.Forms.ToolStripButton toolStripButtonPlay;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxNote;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel3;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxGate;
        private System.Windows.Forms.ToolStripLabel toolStripLabel4;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxVelo;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}