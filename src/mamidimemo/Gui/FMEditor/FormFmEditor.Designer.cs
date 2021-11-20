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
            this.panelPiano.Location = new System.Drawing.Point(5, 503);
            this.panelPiano.Name = "panelPiano";
            this.panelPiano.Size = new System.Drawing.Size(793, 80);
            this.panelPiano.TabIndex = 4;
            // 
            // pianoControl1
            // 
            this.pianoControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pianoControl1.Location = new System.Drawing.Point(0, 25);
            this.pianoControl1.Name = "pianoControl1";
            this.pianoControl1.Size = new System.Drawing.Size(793, 55);
            this.pianoControl1.TabIndex = 0;
            this.pianoControl1.TargetTimbres = null;
            // 
            // toolStrip2
            // 
            this.toolStrip2.ClickThrough = false;
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
            this.toolStrip2.Size = new System.Drawing.Size(793, 25);
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
            this.toolStripButtonPlay.Size = new System.Drawing.Size(85, 22);
            this.toolStripButtonPlay.Text = "PlayOnEdit";
            // 
            // toolStripButtonHook
            // 
            this.toolStripButtonHook.CheckOnClick = true;
            this.toolStripButtonHook.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonHook.Name = "toolStripButtonHook";
            this.toolStripButtonHook.Size = new System.Drawing.Size(64, 22);
            this.toolStripButtonHook.Text = "HookMidi";
            this.toolStripButtonHook.ToolTipText = "Hook all MIDI event for editor.";
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.Image = global::zanac.MAmidiMEmo.Properties.Resources.Panic;
            this.toolStripButton1.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(59, 22);
            this.toolStripButton1.Text = "Panic!";
            this.toolStripButton1.Click += new System.EventHandler(this.toolStripButton1_Click);
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
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel5
            // 
            this.toolStripLabel5.Name = "toolStripLabel5";
            this.toolStripLabel5.Size = new System.Drawing.Size(21, 22);
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
            this.toolStripComboBoxCC.Size = new System.Drawing.Size(75, 25);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 5;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.metroButtonRand1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.metroButtonParams, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.metroTextBoxTarget, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.buttonOK, 3, 2);
            this.tableLayoutPanel1.Controls.Add(this.buttonCancel, 4, 2);
            this.tableLayoutPanel1.Controls.Add(this.metroButtonImport, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.metroButtonImportGit, 1, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(5, 583);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(793, 71);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // metroButtonRand1
            // 
            this.metroButtonRand1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.metroButtonRand1.Location = new System.Drawing.Point(5, 6);
            this.metroButtonRand1.Name = "metroButtonRand1";
            this.metroButtonRand1.Size = new System.Drawing.Size(75, 23);
            this.metroButtonRand1.TabIndex = 0;
            this.metroButtonRand1.Text = "&Rand All";
            this.metroButtonRand1.UseSelectable = true;
            this.metroButtonRand1.Click += new System.EventHandler(this.metroButtonRandAll_Click);
            // 
            // metroButtonParams
            // 
            this.metroButtonParams.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.metroButtonParams.Location = new System.Drawing.Point(86, 6);
            this.metroButtonParams.Name = "metroButtonParams";
            this.metroButtonParams.Size = new System.Drawing.Size(75, 23);
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
            this.metroTextBoxTarget.CustomButton.Location = new System.Drawing.Point(435, 2);
            this.metroTextBoxTarget.CustomButton.Name = "";
            this.metroTextBoxTarget.CustomButton.Size = new System.Drawing.Size(21, 21);
            this.metroTextBoxTarget.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.metroTextBoxTarget.CustomButton.TabIndex = 1;
            this.metroTextBoxTarget.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.metroTextBoxTarget.CustomButton.UseSelectable = true;
            this.metroTextBoxTarget.CustomButton.Visible = false;
            this.metroTextBoxTarget.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::zanac.MAmidiMEmo.Properties.Settings.Default, "FmTarget", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.metroTextBoxTarget.Dock = System.Windows.Forms.DockStyle.Fill;
            this.metroTextBoxTarget.Lines = new string[] {
        "!ALG"};
            this.metroTextBoxTarget.Location = new System.Drawing.Point(167, 5);
            this.metroTextBoxTarget.MaxLength = 32767;
            this.metroTextBoxTarget.Name = "metroTextBoxTarget";
            this.metroTextBoxTarget.PasswordChar = '\0';
            this.metroTextBoxTarget.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.metroTextBoxTarget.SelectedText = "";
            this.metroTextBoxTarget.SelectionLength = 0;
            this.metroTextBoxTarget.SelectionStart = 0;
            this.metroTextBoxTarget.ShortcutsEnabled = true;
            this.metroTextBoxTarget.Size = new System.Drawing.Size(459, 26);
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
            this.buttonOK.Location = new System.Drawing.Point(632, 43);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 4;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseSelectable = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(713, 43);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseSelectable = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // metroButtonImport
            // 
            this.metroButtonImport.AllowDrop = true;
            this.metroButtonImport.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.metroButtonImport.Location = new System.Drawing.Point(5, 43);
            this.metroButtonImport.Name = "metroButtonImport";
            this.metroButtonImport.Size = new System.Drawing.Size(75, 23);
            this.metroButtonImport.TabIndex = 3;
            this.metroButtonImport.Text = "&Import...";
            this.metroButtonImport.UseSelectable = true;
            this.metroButtonImport.Click += new System.EventHandler(this.metroButtonImport_Click);
            this.metroButtonImport.DragDrop += new System.Windows.Forms.DragEventHandler(this.metroButtonImport_DragDrop);
            this.metroButtonImport.DragEnter += new System.Windows.Forms.DragEventHandler(this.metroButtonImport_DragEnter);
            // 
            // metroButtonImportGit
            // 
            this.metroButtonImportGit.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.metroButtonImportGit.Location = new System.Drawing.Point(86, 43);
            this.metroButtonImportGit.Name = "metroButtonImportGit";
            this.metroButtonImportGit.Size = new System.Drawing.Size(75, 23);
            this.metroButtonImportGit.TabIndex = 3;
            this.metroButtonImportGit.Text = "Import &Git...";
            this.metroButtonImportGit.UseSelectable = true;
            this.metroButtonImportGit.Click += new System.EventHandler(this.metroButtonImportGit_Click);
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.Controls.Add(this.flowLayoutPanel1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(5, 95);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(3);
            this.panel1.Size = new System.Drawing.Size(793, 408);
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
            // openFileDialogTone
            // 
            this.openFileDialogTone.DefaultExt = "*.muc";
            this.openFileDialogTone.Filter = "Tone file(MUCOM88, FMP, PMD, VOPM, GWI, FITOM, SYX)|*.muc;*.dat;*.mwi;*.mml;*.fxb" +
    ";*.gwi;*.bnk;*.syx";
            this.openFileDialogTone.SupportMultiDottedExtensions = true;
            this.openFileDialogTone.Title = "Load tone file";
            // 
            // metroLabel1
            // 
            this.metroLabel1.AutoSize = true;
            this.metroLabel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.metroLabel1.Location = new System.Drawing.Point(3, 0);
            this.metroLabel1.Name = "metroLabel1";
            this.metroLabel1.Size = new System.Drawing.Size(54, 35);
            this.metroLabel1.TabIndex = 2;
            this.metroLabel1.Text = "&Timbre:";
            this.metroLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // metroComboBoxTimbres
            // 
            this.metroComboBoxTimbres.Dock = System.Windows.Forms.DockStyle.Fill;
            this.metroComboBoxTimbres.FormattingEnabled = true;
            this.metroComboBoxTimbres.ItemHeight = 23;
            this.metroComboBoxTimbres.Location = new System.Drawing.Point(63, 3);
            this.metroComboBoxTimbres.Name = "metroComboBoxTimbres";
            this.metroComboBoxTimbres.Size = new System.Drawing.Size(525, 29);
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
            this.tableLayoutPanel2.Location = new System.Drawing.Point(5, 60);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(793, 35);
            this.tableLayoutPanel2.TabIndex = 4;
            // 
            // metroButtonImportAll
            // 
            this.metroButtonImportAll.AllowDrop = true;
            this.metroButtonImportAll.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.metroButtonImportAll.AutoSize = true;
            this.metroButtonImportAll.Location = new System.Drawing.Point(594, 6);
            this.metroButtonImportAll.Name = "metroButtonImportAll";
            this.metroButtonImportAll.Size = new System.Drawing.Size(75, 23);
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
            this.metroButtonImportAllGit.Location = new System.Drawing.Point(675, 6);
            this.metroButtonImportAllGit.Name = "metroButtonImportAllGit";
            this.metroButtonImportAllGit.Size = new System.Drawing.Size(115, 23);
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
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(803, 664);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panelPiano);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.tableLayoutPanel2);
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
    }
}