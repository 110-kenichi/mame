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
            this.toolStripButtonHook = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripComboBoxNote = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripLabel4 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripComboBoxVelo = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripLabel3 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripComboBoxGate = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel5 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripComboBoxCC = new System.Windows.Forms.ToolStripComboBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.metroButtonRand1 = new MetroFramework.Controls.MetroButton();
            this.metroButtonParams = new MetroFramework.Controls.MetroButton();
            this.metroTextBoxTarget = new MetroFramework.Controls.MetroTextBox();
            this.buttonOK = new MetroFramework.Controls.MetroButton();
            this.buttonCancel = new MetroFramework.Controls.MetroButton();
            this.metroButtonImport = new MetroFramework.Controls.MetroButton();
            this.metroButtonImportGit = new MetroFramework.Controls.MetroButton();
            this.metroButtonAbort = new MetroFramework.Controls.MetroButton();
            this.metroButtonPaste = new MetroFramework.Controls.MetroButton();
            this.metroButtonCopy = new MetroFramework.Controls.MetroButton();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.panel1 = new System.Windows.Forms.Panel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.openFileDialogTone = new System.Windows.Forms.OpenFileDialog();
            this.metroLabel1 = new MetroFramework.Controls.MetroLabel();
            this.metroComboBoxTimbres = new MetroFramework.Controls.MetroComboBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.metroButtonImportAll = new MetroFramework.Controls.MetroButton();
            this.metroButtonImportAllGit = new MetroFramework.Controls.MetroButton();
            this.metroToolTip1 = new MetroFramework.Components.MetroToolTip();
            this.panelPiano.SuspendLayout();
            this.toolStrip2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelPiano
            // 
            this.panelPiano.Controls.Add(this.pianoControl1);
            this.panelPiano.Controls.Add(this.toolStrip2);
            this.panelPiano.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelPiano.Location = new System.Drawing.Point(9, 626);
            this.panelPiano.Margin = new System.Windows.Forms.Padding(4);
            this.panelPiano.Name = "panelPiano";
            this.panelPiano.Size = new System.Drawing.Size(1053, 100);
            this.panelPiano.TabIndex = 4;
            // 
            // pianoControl1
            // 
            this.pianoControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pianoControl1.Location = new System.Drawing.Point(0, 28);
            this.pianoControl1.Margin = new System.Windows.Forms.Padding(5);
            this.pianoControl1.Name = "pianoControl1";
            this.pianoControl1.Size = new System.Drawing.Size(1053, 72);
            this.pianoControl1.TabIndex = 0;
            this.pianoControl1.TargetTimbres = null;
            // 
            // toolStrip2
            // 
            this.toolStrip2.ClickThrough = false;
            this.toolStrip2.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripComboBoxCh,
            this.toolStripLabel2,
            this.toolStripButtonPlay,
            this.toolStripButtonHook,
            this.toolStripButton1,
            this.toolStripSeparator1,
            this.toolStripLabel1,
            this.toolStripComboBoxNote,
            this.toolStripLabel4,
            this.toolStripComboBoxVelo,
            this.toolStripLabel3,
            this.toolStripComboBoxGate,
            this.toolStripSeparator2,
            this.toolStripLabel5,
            this.toolStripComboBoxCC});
            this.toolStrip2.Location = new System.Drawing.Point(0, 0);
            this.toolStrip2.Name = "toolStrip2";
            this.toolStrip2.Size = new System.Drawing.Size(1053, 28);
            this.toolStrip2.TabIndex = 2;
            // 
            // toolStripComboBoxCh
            // 
            this.toolStripComboBoxCh.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripComboBoxCh.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBoxCh.DropDownWidth = 32;
            this.toolStripComboBoxCh.Items.AddRange(new object[] {
            "1ch",
            "2ch",
            "3ch",
            "4ch",
            "5ch",
            "6ch",
            "7ch",
            "8ch",
            "9ch",
            "10ch",
            "11ch",
            "12ch",
            "13ch",
            "14ch",
            "15ch",
            "16ch"});
            this.toolStripComboBoxCh.Name = "toolStripComboBoxCh";
            this.toolStripComboBoxCh.Size = new System.Drawing.Size(99, 28);
            this.toolStripComboBoxCh.SelectedIndexChanged += new System.EventHandler(this.toolStripComboBox1_SelectedIndexChanged);
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Size = new System.Drawing.Size(54, 25);
            this.toolStripLabel2.Text = "Key Ch";
            // 
            // toolStripButtonPlay
            // 
            this.toolStripButtonPlay.CheckOnClick = true;
            this.toolStripButtonPlay.Image = global::zanac.MAmidiMEmo.Properties.Resources.Inst;
            this.toolStripButtonPlay.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonPlay.Name = "toolStripButtonPlay";
            this.toolStripButtonPlay.Size = new System.Drawing.Size(105, 25);
            this.toolStripButtonPlay.Text = "PlayOnEdit";
            // 
            // toolStripButtonHook
            // 
            this.toolStripButtonHook.CheckOnClick = true;
            this.toolStripButtonHook.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonHook.Name = "toolStripButtonHook";
            this.toolStripButtonHook.Size = new System.Drawing.Size(79, 25);
            this.toolStripButtonHook.Text = "HookMidi";
            this.toolStripButtonHook.ToolTipText = "Hook all MIDI event for editor.";
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.Image = global::zanac.MAmidiMEmo.Properties.Resources.Panic;
            this.toolStripButton1.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(68, 25);
            this.toolStripButton1.Text = "Panic!";
            this.toolStripButton1.Click += new System.EventHandler(this.toolStripButton1_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 28);
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(42, 25);
            this.toolStripLabel1.Text = "Note";
            // 
            // toolStripComboBoxNote
            // 
            this.toolStripComboBoxNote.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBoxNote.Name = "toolStripComboBoxNote";
            this.toolStripComboBoxNote.Size = new System.Drawing.Size(99, 28);
            // 
            // toolStripLabel4
            // 
            this.toolStripLabel4.Name = "toolStripLabel4";
            this.toolStripLabel4.Size = new System.Drawing.Size(61, 25);
            this.toolStripLabel4.Text = "Velocity";
            // 
            // toolStripComboBoxVelo
            // 
            this.toolStripComboBoxVelo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBoxVelo.Name = "toolStripComboBoxVelo";
            this.toolStripComboBoxVelo.Size = new System.Drawing.Size(99, 28);
            // 
            // toolStripLabel3
            // 
            this.toolStripLabel3.Name = "toolStripLabel3";
            this.toolStripLabel3.Size = new System.Drawing.Size(40, 25);
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
            this.toolStripComboBoxGate.Size = new System.Drawing.Size(75, 28);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 28);
            // 
            // toolStripLabel5
            // 
            this.toolStripLabel5.Name = "toolStripLabel5";
            this.toolStripLabel5.Size = new System.Drawing.Size(27, 25);
            this.toolStripLabel5.Text = "CC";
            // 
            // toolStripComboBoxCC
            // 
            this.toolStripComboBoxCC.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBoxCC.DropDownWidth = 32;
            this.toolStripComboBoxCC.Items.AddRange(new object[] {
            "0",
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
            "15",
            "16",
            "17",
            "18",
            "19",
            "20",
            "21",
            "22",
            "23",
            "24",
            "25",
            "26",
            "27",
            "28",
            "29",
            "30",
            "31",
            "32",
            "33",
            "34",
            "35",
            "36",
            "37",
            "38",
            "39",
            "40",
            "41",
            "42",
            "43",
            "44",
            "45",
            "46",
            "47",
            "48",
            "49",
            "50",
            "51",
            "52",
            "53",
            "54",
            "55",
            "56",
            "57",
            "58",
            "59",
            "60",
            "61",
            "62",
            "63",
            "64",
            "65",
            "66",
            "67",
            "68",
            "69",
            "70",
            "71",
            "72",
            "73",
            "74",
            "75",
            "76",
            "77",
            "78",
            "79",
            "80",
            "81",
            "82",
            "83",
            "84",
            "85",
            "86",
            "87",
            "88",
            "89",
            "90",
            "91",
            "92",
            "93",
            "94",
            "95",
            "96",
            "97",
            "98",
            "99",
            "100",
            "101",
            "102",
            "103",
            "104",
            "105",
            "106",
            "107",
            "108",
            "109",
            "110",
            "111",
            "112",
            "113",
            "114",
            "115",
            "116",
            "117",
            "118",
            "119",
            "120",
            "121",
            "122",
            "123",
            "124",
            "125",
            "126",
            "127"});
            this.toolStripComboBoxCC.Name = "toolStripComboBoxCC";
            this.toolStripComboBoxCC.Size = new System.Drawing.Size(75, 28);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 6;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.metroButtonRand1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.metroButtonParams, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.metroTextBoxTarget, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.buttonOK, 3, 2);
            this.tableLayoutPanel1.Controls.Add(this.buttonCancel, 4, 2);
            this.tableLayoutPanel1.Controls.Add(this.metroButtonImport, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.metroButtonImportGit, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.metroButtonAbort, 5, 2);
            this.tableLayoutPanel1.Controls.Add(this.metroButtonPaste, 5, 0);
            this.tableLayoutPanel1.Controls.Add(this.metroButtonCopy, 4, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(9, 726);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1053, 89);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // metroButtonRand1
            // 
            this.metroButtonRand1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.metroButtonRand1.Location = new System.Drawing.Point(7, 7);
            this.metroButtonRand1.Margin = new System.Windows.Forms.Padding(4);
            this.metroButtonRand1.Name = "metroButtonRand1";
            this.metroButtonRand1.Size = new System.Drawing.Size(100, 29);
            this.metroButtonRand1.TabIndex = 0;
            this.metroButtonRand1.Text = "&Rand All";
            this.metroButtonRand1.UseSelectable = true;
            this.metroButtonRand1.Click += new System.EventHandler(this.metroButtonRandAll_Click);
            // 
            // metroButtonParams
            // 
            this.metroButtonParams.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.metroButtonParams.Location = new System.Drawing.Point(115, 7);
            this.metroButtonParams.Margin = new System.Windows.Forms.Padding(4);
            this.metroButtonParams.Name = "metroButtonParams";
            this.metroButtonParams.Size = new System.Drawing.Size(100, 29);
            this.metroButtonParams.TabIndex = 1;
            this.metroButtonParams.Text = "Rand &Fine";
            this.metroButtonParams.UseSelectable = true;
            this.metroButtonParams.Click += new System.EventHandler(this.metroButtonRandParams_Click);
            // 
            // metroTextBoxTarget
            // 
            // 
            // 
            // 
            this.metroTextBoxTarget.CustomButton.Image = null;
            this.metroTextBoxTarget.CustomButton.Location = new System.Drawing.Point(469, 2);
            this.metroTextBoxTarget.CustomButton.Margin = new System.Windows.Forms.Padding(4);
            this.metroTextBoxTarget.CustomButton.Name = "";
            this.metroTextBoxTarget.CustomButton.Size = new System.Drawing.Size(27, 27);
            this.metroTextBoxTarget.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.metroTextBoxTarget.CustomButton.TabIndex = 1;
            this.metroTextBoxTarget.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.metroTextBoxTarget.CustomButton.UseSelectable = true;
            this.metroTextBoxTarget.CustomButton.Visible = false;
            this.metroTextBoxTarget.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::zanac.MAmidiMEmo.Properties.Settings.Default, "FmTarget", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.metroTextBoxTarget.Dock = System.Windows.Forms.DockStyle.Fill;
            this.metroTextBoxTarget.Lines = new string[] {
        "!ALG"};
            this.metroTextBoxTarget.Location = new System.Drawing.Point(223, 6);
            this.metroTextBoxTarget.Margin = new System.Windows.Forms.Padding(4);
            this.metroTextBoxTarget.MaxLength = 32767;
            this.metroTextBoxTarget.Name = "metroTextBoxTarget";
            this.metroTextBoxTarget.PasswordChar = '\0';
            this.metroTextBoxTarget.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.metroTextBoxTarget.SelectedText = "";
            this.metroTextBoxTarget.SelectionLength = 0;
            this.metroTextBoxTarget.SelectionStart = 0;
            this.metroTextBoxTarget.ShortcutsEnabled = true;
            this.metroTextBoxTarget.Size = new System.Drawing.Size(499, 32);
            this.metroTextBoxTarget.TabIndex = 2;
            this.metroTextBoxTarget.Text = global::zanac.MAmidiMEmo.Properties.Settings.Default.FmTarget;
            this.metroTextBoxTarget.UseSelectable = true;
            this.metroTextBoxTarget.WaterMark = "(Write randomize target register and untarget( prefix \"!\" ) names here. Separated" +
    " with comma. )";
            this.metroTextBoxTarget.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.metroTextBoxTarget.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(730, 54);
            this.buttonOK.Margin = new System.Windows.Forms.Padding(4);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(100, 29);
            this.buttonOK.TabIndex = 7;
            this.buttonOK.Text = "&OK";
            this.metroToolTip1.SetToolTip(this.buttonOK, "Close and apply editor data.");
            this.buttonOK.UseSelectable = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(838, 54);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(4);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(100, 29);
            this.buttonCancel.TabIndex = 8;
            this.buttonCancel.Text = "&Cancel";
            this.metroToolTip1.SetToolTip(this.buttonCancel, "Close and undo data to initial data.");
            this.buttonCancel.UseSelectable = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // metroButtonImport
            // 
            this.metroButtonImport.AllowDrop = true;
            this.metroButtonImport.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.metroButtonImport.Location = new System.Drawing.Point(7, 54);
            this.metroButtonImport.Margin = new System.Windows.Forms.Padding(4);
            this.metroButtonImport.Name = "metroButtonImport";
            this.metroButtonImport.Size = new System.Drawing.Size(100, 29);
            this.metroButtonImport.TabIndex = 5;
            this.metroButtonImport.Text = "&Import...";
            this.metroButtonImport.UseSelectable = true;
            this.metroButtonImport.Click += new System.EventHandler(this.metroButtonImport_Click);
            this.metroButtonImport.DragDrop += new System.Windows.Forms.DragEventHandler(this.metroButtonImport_DragDrop);
            this.metroButtonImport.DragEnter += new System.Windows.Forms.DragEventHandler(this.metroButtonImport_DragEnter);
            // 
            // metroButtonImportGit
            // 
            this.metroButtonImportGit.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.metroButtonImportGit.Location = new System.Drawing.Point(115, 54);
            this.metroButtonImportGit.Margin = new System.Windows.Forms.Padding(4);
            this.metroButtonImportGit.Name = "metroButtonImportGit";
            this.metroButtonImportGit.Size = new System.Drawing.Size(100, 29);
            this.metroButtonImportGit.TabIndex = 6;
            this.metroButtonImportGit.Text = "Import &Git...";
            this.metroButtonImportGit.UseSelectable = true;
            this.metroButtonImportGit.Click += new System.EventHandler(this.metroButtonImportGit_Click);
            // 
            // metroButtonAbort
            // 
            this.metroButtonAbort.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.metroButtonAbort.DialogResult = System.Windows.Forms.DialogResult.Abort;
            this.metroButtonAbort.Location = new System.Drawing.Point(946, 54);
            this.metroButtonAbort.Margin = new System.Windows.Forms.Padding(4);
            this.metroButtonAbort.Name = "metroButtonAbort";
            this.metroButtonAbort.Size = new System.Drawing.Size(100, 29);
            this.metroButtonAbort.TabIndex = 9;
            this.metroButtonAbort.Text = "&Abort";
            this.metroToolTip1.SetToolTip(this.metroButtonAbort, "Close and does not undo data to initial data.");
            this.metroButtonAbort.UseSelectable = true;
            this.metroButtonAbort.Click += new System.EventHandler(this.metroButtonAbort_Click);
            // 
            // metroButtonPaste
            // 
            this.metroButtonPaste.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.metroButtonPaste.Location = new System.Drawing.Point(946, 7);
            this.metroButtonPaste.Margin = new System.Windows.Forms.Padding(4);
            this.metroButtonPaste.Name = "metroButtonPaste";
            this.metroButtonPaste.Size = new System.Drawing.Size(100, 29);
            this.metroButtonPaste.TabIndex = 4;
            this.metroButtonPaste.Text = "&Paste data";
            this.metroButtonPaste.UseSelectable = true;
            this.metroButtonPaste.Click += new System.EventHandler(this.metroButtonPaste_Click);
            // 
            // metroButtonCopy
            // 
            this.metroButtonCopy.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.metroButtonCopy.Location = new System.Drawing.Point(838, 7);
            this.metroButtonCopy.Margin = new System.Windows.Forms.Padding(4);
            this.metroButtonCopy.Name = "metroButtonCopy";
            this.metroButtonCopy.Size = new System.Drawing.Size(100, 29);
            this.metroButtonCopy.TabIndex = 3;
            this.metroButtonCopy.Text = "Cop&y data";
            this.metroButtonCopy.UseSelectable = true;
            this.metroButtonCopy.Click += new System.EventHandler(this.metroButtonCopy_Click);
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.Controls.Add(this.flowLayoutPanel1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(9, 133);
            this.panel1.Margin = new System.Windows.Forms.Padding(4);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(4);
            this.panel1.Size = new System.Drawing.Size(1053, 493);
            this.panel1.TabIndex = 5;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(8, 8);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.flowLayoutPanel1.MinimumSize = new System.Drawing.Size(67, 62);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(67, 62);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // openFileDialogTone
            // 
            this.openFileDialogTone.DefaultExt = "*.muc";
            this.openFileDialogTone.Filter = "Tone file(MUCOM88, FMP, PMD, VOPM, GWI, FITOM, SYX, FF, FFOPM)|*.muc;*.dat;*.mwi;*.mml;*.fxb" +
    ";*.gwi;*.bnk;*.syx;*.ff;*.ffopm";
            this.openFileDialogTone.SupportMultiDottedExtensions = true;
            this.openFileDialogTone.Title = "Load tone file";
            // 
            // metroLabel1
            // 
            this.metroLabel1.AutoSize = true;
            this.metroLabel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.metroLabel1.Location = new System.Drawing.Point(4, 0);
            this.metroLabel1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.metroLabel1.Name = "metroLabel1";
            this.metroLabel1.Size = new System.Drawing.Size(56, 39);
            this.metroLabel1.TabIndex = 2;
            this.metroLabel1.Text = "&Timbre:";
            this.metroLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // metroComboBoxTimbres
            // 
            this.metroComboBoxTimbres.Dock = System.Windows.Forms.DockStyle.Fill;
            this.metroComboBoxTimbres.FormattingEnabled = true;
            this.metroComboBoxTimbres.ItemHeight = 24;
            this.metroComboBoxTimbres.Location = new System.Drawing.Point(68, 4);
            this.metroComboBoxTimbres.Margin = new System.Windows.Forms.Padding(4);
            this.metroComboBoxTimbres.Name = "metroComboBoxTimbres";
            this.metroComboBoxTimbres.Size = new System.Drawing.Size(700, 30);
            this.metroComboBoxTimbres.TabIndex = 3;
            this.metroComboBoxTimbres.UseSelectable = true;
            this.metroComboBoxTimbres.SelectedIndexChanged += new System.EventHandler(this.metroComboBoxTimbres_SelectedIndexChanged);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel2.ColumnCount = 4;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this.metroComboBoxTimbres, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.metroLabel1, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.metroButtonImportAll, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.metroButtonImportAllGit, 3, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(9, 94);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1053, 39);
            this.tableLayoutPanel2.TabIndex = 4;
            // 
            // metroButtonImportAll
            // 
            this.metroButtonImportAll.AllowDrop = true;
            this.metroButtonImportAll.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.metroButtonImportAll.AutoSize = true;
            this.metroButtonImportAll.Location = new System.Drawing.Point(776, 4);
            this.metroButtonImportAll.Margin = new System.Windows.Forms.Padding(4);
            this.metroButtonImportAll.Name = "metroButtonImportAll";
            this.metroButtonImportAll.Size = new System.Drawing.Size(112, 31);
            this.metroButtonImportAll.TabIndex = 3;
            this.metroButtonImportAll.Text = "Import &all...";
            this.metroButtonImportAll.UseSelectable = true;
            this.metroButtonImportAll.Click += new System.EventHandler(this.metroButtonImportAll_Click);
            this.metroButtonImportAll.DragDrop += new System.Windows.Forms.DragEventHandler(this.metroButtonImportAll_DragDrop);
            this.metroButtonImportAll.DragEnter += new System.Windows.Forms.DragEventHandler(this.metroButtonImportAll_DragEnter);
            // 
            // metroButtonImportAllGit
            // 
            this.metroButtonImportAllGit.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.metroButtonImportAllGit.AutoSize = true;
            this.metroButtonImportAllGit.Location = new System.Drawing.Point(896, 4);
            this.metroButtonImportAllGit.Margin = new System.Windows.Forms.Padding(4);
            this.metroButtonImportAllGit.Name = "metroButtonImportAllGit";
            this.metroButtonImportAllGit.Size = new System.Drawing.Size(153, 31);
            this.metroButtonImportAllGit.TabIndex = 3;
            this.metroButtonImportAllGit.Text = "Import all Gi&t...";
            this.metroButtonImportAllGit.UseSelectable = true;
            this.metroButtonImportAllGit.Click += new System.EventHandler(this.metroButtonImportAllGit_Click);
            // 
            // metroToolTip1
            // 
            this.metroToolTip1.Style = MetroFramework.MetroColorStyle.Blue;
            this.metroToolTip1.StyleManager = null;
            this.metroToolTip1.Theme = MetroFramework.MetroThemeStyle.Light;
            // 
            // FormFmEditor
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(1071, 830);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panelPiano);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.tableLayoutPanel2);
            this.Margin = new System.Windows.Forms.Padding(5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormFmEditor";
            this.Padding = new System.Windows.Forms.Padding(9, 94, 9, 15);
            this.ShowIcon = false;
            this.ShowInTaskbar = true;
            this.Text = "FM Synthesis Editor";
            this.panelPiano.ResumeLayout(false);
            this.panelPiano.PerformLayout();
            this.toolStrip2.ResumeLayout(false);
            this.toolStrip2.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

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
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxCC;
        private System.Windows.Forms.ToolStripLabel toolStripLabel5;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private MetroFramework.Controls.MetroButton metroButtonRand1;
        private MetroFramework.Controls.MetroButton metroButtonParams;
        private MetroFramework.Controls.MetroTextBox metroTextBoxTarget;
        private MetroFramework.Controls.MetroButton metroButtonImport;
        private System.Windows.Forms.OpenFileDialog openFileDialogTone;
        private MetroFramework.Controls.MetroLabel metroLabel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private MetroFramework.Controls.MetroComboBox metroComboBoxTimbres;
        private MetroFramework.Controls.MetroButton metroButtonImportAll;
        private MetroFramework.Controls.MetroButton metroButtonImportAllGit;
        private MetroFramework.Controls.MetroButton metroButtonImportGit;
        private System.Windows.Forms.ToolStripButton toolStripButtonHook;
        private MetroFramework.Components.MetroToolTip metroToolTip1;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private MetroFramework.Controls.MetroButton metroButtonAbort;
        private MetroFramework.Controls.MetroButton metroButtonCopy;
        private MetroFramework.Controls.MetroButton metroButtonPaste;
    }
}