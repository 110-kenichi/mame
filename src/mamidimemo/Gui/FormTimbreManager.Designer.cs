namespace zanac.MAmidiMEmo.Gui
{
    partial class FormTimbreManager
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
            System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("Default", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup2 = new System.Windows.Forms.ListViewGroup("Default", System.Windows.Forms.HorizontalAlignment.Left);
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
            this.listViewCurrentTimbres = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.clearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.buttonOK = new MetroFramework.Controls.MetroButton();
            this.buttonCancel = new MetroFramework.Controls.MetroButton();
            this.fileFolderList1 = new zanac.MAmidiMEmo.ComponentModel.FileFolderList();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.metroTextBox1 = new MetroFramework.Controls.MetroTextBox();
            this.metroLabelDir = new MetroFramework.Controls.MetroLabel();
            this.metroLabel1 = new MetroFramework.Controls.MetroLabel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.metroLabel2 = new MetroFramework.Controls.MetroLabel();
            this.listViewFilesTimbres = new System.Windows.Forms.ListView();
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.metroLabel3 = new MetroFramework.Controls.MetroLabel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.metroButtonNewDir = new MetroFramework.Controls.MetroButton();
            this.metroButtonDelete = new MetroFramework.Controls.MetroButton();
            this.metroButtonRefresh = new MetroFramework.Controls.MetroButton();
            this.metroButtonExplorer = new MetroFramework.Controls.MetroButton();
            this.metroButton1 = new MetroFramework.Controls.MetroButton();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.metroButtonExport = new MetroFramework.Controls.MetroButton();
            this.metroButton2 = new MetroFramework.Controls.MetroButton();
            this.metroButton3 = new MetroFramework.Controls.MetroButton();
            this.betterFolderBrowser1 = new WK.Libraries.BetterFolderBrowserNS.BetterFolderBrowser(this.components);
            this.toolStrip2.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // pianoControl1
            // 
            this.pianoControl1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pianoControl1.Location = new System.Drawing.Point(7, 687);
            this.pianoControl1.Margin = new System.Windows.Forms.Padding(5);
            this.pianoControl1.Name = "pianoControl1";
            this.pianoControl1.Size = new System.Drawing.Size(1042, 72);
            this.pianoControl1.TabIndex = 2;
            this.pianoControl1.TargetTimbres = null;
            // 
            // toolStrip2
            // 
            this.toolStrip2.Dock = System.Windows.Forms.DockStyle.Bottom;
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
            this.toolStrip2.Location = new System.Drawing.Point(7, 659);
            this.toolStrip2.Name = "toolStrip2";
            this.toolStrip2.Size = new System.Drawing.Size(1042, 28);
            this.toolStrip2.TabIndex = 1;
            // 
            // toolStripComboBoxCh
            // 
            this.toolStripComboBoxCh.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripComboBoxCh.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBoxCh.DropDownWidth = 32;
            this.toolStripComboBoxCh.Items.AddRange(new object[] {
            "A0(1ch)",
            "A1(2ch)",
            "A2(3ch)",
            "A3(4ch)",
            "A4(5ch)",
            "A5(6ch)",
            "A6(7ch)",
            "A7(8ch)",
            "A8(9ch)",
            "A9(10ch)",
            "A10(11ch)",
            "A11(12ch)",
            "A12(13ch)",
            "A13(14ch)",
            "A14(15ch)",
            "A15(16ch)"});
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
            // listViewCurrentTimbres
            // 
            this.listViewCurrentTimbres.AllowDrop = true;
            this.listViewCurrentTimbres.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.listViewCurrentTimbres.ContextMenuStrip = this.contextMenuStrip1;
            this.listViewCurrentTimbres.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewCurrentTimbres.FullRowSelect = true;
            this.listViewCurrentTimbres.GridLines = true;
            listViewGroup1.Header = "Default";
            listViewGroup1.Name = "listViewGroup1";
            this.listViewCurrentTimbres.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup1});
            this.listViewCurrentTimbres.HideSelection = false;
            this.listViewCurrentTimbres.Location = new System.Drawing.Point(745, 34);
            this.listViewCurrentTimbres.Name = "listViewCurrentTimbres";
            this.listViewCurrentTimbres.ShowGroups = false;
            this.listViewCurrentTimbres.Size = new System.Drawing.Size(291, 509);
            this.listViewCurrentTimbres.TabIndex = 8;
            this.listViewCurrentTimbres.UseCompatibleStateImageBehavior = false;
            this.listViewCurrentTimbres.View = System.Windows.Forms.View.Details;
            this.listViewCurrentTimbres.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.listViewCurrentTimbres_ItemDrag);
            this.listViewCurrentTimbres.DragDrop += new System.Windows.Forms.DragEventHandler(this.listViewCurrentTimbres_DragDrop);
            this.listViewCurrentTimbres.DragEnter += new System.Windows.Forms.DragEventHandler(this.listViewCurrentTimbres_DragOver);
            this.listViewCurrentTimbres.DragOver += new System.Windows.Forms.DragEventHandler(this.listViewCurrentTimbres_DragOver);
            this.listViewCurrentTimbres.Enter += new System.EventHandler(this.listViewCurrentTimbres_Enter);
            this.listViewCurrentTimbres.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listViewCurrentTimbres_MouseDoubleClick);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "No";
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Name";
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Memo";
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.clearToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(121, 28);
            // 
            // clearToolStripMenuItem
            // 
            this.clearToolStripMenuItem.Name = "clearToolStripMenuItem";
            this.clearToolStripMenuItem.Size = new System.Drawing.Size(120, 24);
            this.clearToolStripMenuItem.Text = "&Clear...";
            this.clearToolStripMenuItem.Click += new System.EventHandler(this.clearToolStripMenuItem_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Controls.Add(this.buttonOK, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.buttonCancel, 2, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(7, 759);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1042, 46);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(827, 8);
            this.buttonOK.Margin = new System.Windows.Forms.Padding(4);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(100, 29);
            this.buttonOK.TabIndex = 0;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseSelectable = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(935, 8);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(4);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(100, 29);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseSelectable = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // fileFolderList1
            // 
            this.fileFolderList1.Activation = System.Windows.Forms.ItemActivation.TwoClick;
            this.fileFolderList1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4});
            this.tableLayoutPanel2.SetColumnSpan(this.fileFolderList1, 3);
            this.fileFolderList1.CurrentDirectory = "C:\\";
            this.fileFolderList1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fileFolderList1.FileValidator = null;
            this.fileFolderList1.FilterExts = null;
            this.fileFolderList1.FullRowSelect = true;
            this.fileFolderList1.GridLines = true;
            this.fileFolderList1.HideSelection = false;
            this.fileFolderList1.isSoloBrowser = true;
            this.fileFolderList1.Location = new System.Drawing.Point(3, 34);
            this.fileFolderList1.Name = "fileFolderList1";
            this.fileFolderList1.Size = new System.Drawing.Size(392, 509);
            this.fileFolderList1.TabIndex = 3;
            this.fileFolderList1.UseCompatibleStateImageBehavior = false;
            this.fileFolderList1.View = System.Windows.Forms.View.Details;
            this.fileFolderList1.CurrentDirectoryChanged += new System.EventHandler(this.fileFolderList1_CurrentDirectoryChanged);
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "File name";
            // 
            // metroTextBox1
            // 
            // 
            // 
            // 
            this.metroTextBox1.CustomButton.Image = null;
            this.metroTextBox1.CustomButton.Location = new System.Drawing.Point(266, 1);
            this.metroTextBox1.CustomButton.Name = "";
            this.metroTextBox1.CustomButton.Size = new System.Drawing.Size(23, 23);
            this.metroTextBox1.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.metroTextBox1.CustomButton.TabIndex = 1;
            this.metroTextBox1.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.metroTextBox1.CustomButton.UseSelectable = true;
            this.metroTextBox1.CustomButton.Visible = false;
            this.metroTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.metroTextBox1.Lines = new string[] {
        "c:\\"};
            this.metroTextBox1.Location = new System.Drawing.Point(73, 3);
            this.metroTextBox1.MaxLength = 32767;
            this.metroTextBox1.Name = "metroTextBox1";
            this.metroTextBox1.PasswordChar = '\0';
            this.metroTextBox1.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.metroTextBox1.SelectedText = "";
            this.metroTextBox1.SelectionLength = 0;
            this.metroTextBox1.SelectionStart = 0;
            this.metroTextBox1.ShortcutsEnabled = true;
            this.metroTextBox1.Size = new System.Drawing.Size(290, 25);
            this.metroTextBox1.TabIndex = 1;
            this.metroTextBox1.Text = "c:\\";
            this.metroTextBox1.UseSelectable = true;
            this.metroTextBox1.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.metroTextBox1.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            this.metroTextBox1.TextChanged += new System.EventHandler(this.metroTextBox1_TextChanged);
            // 
            // metroLabelDir
            // 
            this.metroLabelDir.AutoSize = true;
            this.metroLabelDir.Dock = System.Windows.Forms.DockStyle.Fill;
            this.metroLabelDir.Location = new System.Drawing.Point(4, 0);
            this.metroLabelDir.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.metroLabelDir.Name = "metroLabelDir";
            this.metroLabelDir.Size = new System.Drawing.Size(62, 31);
            this.metroLabelDir.TabIndex = 0;
            this.metroLabelDir.Text = "&Directry:";
            // 
            // metroLabel1
            // 
            this.metroLabel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.metroLabel1.Location = new System.Drawing.Point(402, 0);
            this.metroLabel1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.metroLabel1.Name = "metroLabel1";
            this.metroLabel1.Size = new System.Drawing.Size(288, 31);
            this.metroLabel1.TabIndex = 4;
            this.metroLabel1.Text = "Loaded &Timbres: (Drag to right pane)";
            this.metroLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel2.ColumnCount = 6;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 48F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.Controls.Add(this.metroLabelDir, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.metroLabel1, 3, 0);
            this.tableLayoutPanel2.Controls.Add(this.fileFolderList1, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.metroLabel2, 5, 0);
            this.tableLayoutPanel2.Controls.Add(this.metroTextBox1, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.listViewFilesTimbres, 3, 1);
            this.tableLayoutPanel2.Controls.Add(this.metroLabel3, 4, 1);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel3, 0, 3);
            this.tableLayoutPanel2.Controls.Add(this.metroButton1, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel4, 5, 3);
            this.tableLayoutPanel2.Controls.Add(this.listViewCurrentTimbres, 5, 1);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(10, 79);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 4;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1039, 577);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // metroLabel2
            // 
            this.metroLabel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.metroLabel2.Location = new System.Drawing.Point(746, 0);
            this.metroLabel2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.metroLabel2.Name = "metroLabel2";
            this.metroLabel2.Size = new System.Drawing.Size(289, 31);
            this.metroLabel2.TabIndex = 7;
            this.metroLabel2.Text = "C&urrent Timbres: (Drag(+shift) to copy(exchg) order)";
            this.metroLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // listViewFilesTimbres
            // 
            this.listViewFilesTimbres.AllowDrop = true;
            this.listViewFilesTimbres.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewFilesTimbres.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader7});
            this.listViewFilesTimbres.FullRowSelect = true;
            this.listViewFilesTimbres.GridLines = true;
            listViewGroup2.Header = "Default";
            listViewGroup2.Name = "listViewGroup1";
            this.listViewFilesTimbres.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup2});
            this.listViewFilesTimbres.HideSelection = false;
            this.listViewFilesTimbres.Location = new System.Drawing.Point(401, 34);
            this.listViewFilesTimbres.Name = "listViewFilesTimbres";
            this.listViewFilesTimbres.ShowGroups = false;
            this.listViewFilesTimbres.Size = new System.Drawing.Size(290, 509);
            this.listViewFilesTimbres.TabIndex = 5;
            this.listViewFilesTimbres.UseCompatibleStateImageBehavior = false;
            this.listViewFilesTimbres.View = System.Windows.Forms.View.Details;
            this.listViewFilesTimbres.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.listViewFilesTimbres_ItemDrag);
            this.listViewFilesTimbres.DragDrop += new System.Windows.Forms.DragEventHandler(this.listViewFilesTimbres_DragDrop);
            this.listViewFilesTimbres.DragEnter += new System.Windows.Forms.DragEventHandler(this.listViewFilesTimbres_DragEnter);
            this.listViewFilesTimbres.DragOver += new System.Windows.Forms.DragEventHandler(this.listViewFilesTimbres_DragOver);
            this.listViewFilesTimbres.Enter += new System.EventHandler(this.listViewFilesTimbres_Enter);
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "No";
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "Name";
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "Memo";
            // 
            // metroLabel3
            // 
            this.metroLabel3.AutoSize = true;
            this.metroLabel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.metroLabel3.Location = new System.Drawing.Point(697, 31);
            this.metroLabel3.Name = "metroLabel3";
            this.metroLabel3.Size = new System.Drawing.Size(42, 515);
            this.metroLabel3.TabIndex = 6;
            this.metroLabel3.Text = "⇒";
            this.metroLabel3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.AutoSize = true;
            this.tableLayoutPanel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel3.ColumnCount = 5;
            this.tableLayoutPanel2.SetColumnSpan(this.tableLayoutPanel3, 3);
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.Controls.Add(this.metroButtonNewDir, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.metroButtonDelete, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.metroButtonRefresh, 4, 0);
            this.tableLayoutPanel3.Controls.Add(this.metroButtonExplorer, 3, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 546);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.Size = new System.Drawing.Size(398, 31);
            this.tableLayoutPanel3.TabIndex = 9;
            // 
            // metroButtonNewDir
            // 
            this.metroButtonNewDir.AutoSize = true;
            this.metroButtonNewDir.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.metroButtonNewDir.Dock = System.Windows.Forms.DockStyle.Fill;
            this.metroButtonNewDir.Location = new System.Drawing.Point(3, 3);
            this.metroButtonNewDir.Name = "metroButtonNewDir";
            this.metroButtonNewDir.Size = new System.Drawing.Size(110, 25);
            this.metroButtonNewDir.TabIndex = 0;
            this.metroButtonNewDir.Text = "Create &folder...";
            this.metroButtonNewDir.UseSelectable = true;
            this.metroButtonNewDir.Click += new System.EventHandler(this.metroButtonNewDir_Click);
            // 
            // metroButtonDelete
            // 
            this.metroButtonDelete.Dock = System.Windows.Forms.DockStyle.Fill;
            this.metroButtonDelete.Location = new System.Drawing.Point(119, 3);
            this.metroButtonDelete.Name = "metroButtonDelete";
            this.metroButtonDelete.Size = new System.Drawing.Size(75, 25);
            this.metroButtonDelete.TabIndex = 1;
            this.metroButtonDelete.Text = "D&elete...";
            this.metroButtonDelete.UseSelectable = true;
            this.metroButtonDelete.Click += new System.EventHandler(this.metroButtonDelete_Click);
            // 
            // metroButtonRefresh
            // 
            this.metroButtonRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.metroButtonRefresh.AutoSize = true;
            this.metroButtonRefresh.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.metroButtonRefresh.Location = new System.Drawing.Point(328, 3);
            this.metroButtonRefresh.Name = "metroButtonRefresh";
            this.metroButtonRefresh.Size = new System.Drawing.Size(67, 25);
            this.metroButtonRefresh.TabIndex = 3;
            this.metroButtonRefresh.Text = "&Refresh";
            this.metroButtonRefresh.UseSelectable = true;
            this.metroButtonRefresh.Click += new System.EventHandler(this.metroButtonRefresh_Click);
            // 
            // metroButtonExplorer
            // 
            this.metroButtonExplorer.AutoSize = true;
            this.metroButtonExplorer.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.metroButtonExplorer.Location = new System.Drawing.Point(254, 3);
            this.metroButtonExplorer.Name = "metroButtonExplorer";
            this.metroButtonExplorer.Size = new System.Drawing.Size(68, 25);
            this.metroButtonExplorer.TabIndex = 2;
            this.metroButtonExplorer.Text = "E&xplorer";
            this.metroButtonExplorer.UseSelectable = true;
            this.metroButtonExplorer.Click += new System.EventHandler(this.metroButtonExplorer_Click);
            // 
            // metroButton1
            // 
            this.metroButton1.AutoSize = true;
            this.metroButton1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.metroButton1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.metroButton1.Location = new System.Drawing.Point(369, 3);
            this.metroButton1.Name = "metroButton1";
            this.metroButton1.Size = new System.Drawing.Size(26, 25);
            this.metroButton1.TabIndex = 2;
            this.metroButton1.Text = "...";
            this.metroButton1.UseSelectable = true;
            this.metroButton1.Click += new System.EventHandler(this.metroButton1_Click);
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.AutoSize = true;
            this.tableLayoutPanel4.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel4.ColumnCount = 3;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Controls.Add(this.metroButtonExport, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.metroButton2, 1, 0);
            this.tableLayoutPanel4.Controls.Add(this.metroButton3, 2, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(742, 546);
            this.tableLayoutPanel4.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 1;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel4.Size = new System.Drawing.Size(297, 31);
            this.tableLayoutPanel4.TabIndex = 10;
            // 
            // metroButtonExport
            // 
            this.metroButtonExport.AutoSize = true;
            this.metroButtonExport.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.metroButtonExport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.metroButtonExport.Location = new System.Drawing.Point(3, 3);
            this.metroButtonExport.Name = "metroButtonExport";
            this.metroButtonExport.Size = new System.Drawing.Size(117, 25);
            this.metroButtonExport.TabIndex = 0;
            this.metroButtonExport.Text = "Ex&port slected...";
            this.metroButtonExport.UseSelectable = true;
            this.metroButtonExport.Click += new System.EventHandler(this.metroButtonExport_Click);
            // 
            // metroButton2
            // 
            this.metroButton2.AutoSize = true;
            this.metroButton2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.metroButton2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.metroButton2.Location = new System.Drawing.Point(126, 3);
            this.metroButton2.Name = "metroButton2";
            this.metroButton2.Size = new System.Drawing.Size(84, 25);
            this.metroButton2.TabIndex = 1;
            this.metroButton2.Text = "Export &all...";
            this.metroButton2.UseSelectable = true;
            this.metroButton2.Click += new System.EventHandler(this.metroButton2_Click);
            // 
            // metroButton3
            // 
            this.metroButton3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.metroButton3.AutoSize = true;
            this.metroButton3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.metroButton3.Location = new System.Drawing.Point(235, 3);
            this.metroButton3.Name = "metroButton3";
            this.metroButton3.Size = new System.Drawing.Size(59, 25);
            this.metroButton3.TabIndex = 2;
            this.metroButton3.Text = "&Clear...";
            this.metroButton3.UseSelectable = true;
            this.metroButton3.Click += new System.EventHandler(this.metroButtonClear_Click);
            // 
            // betterFolderBrowser1
            // 
            this.betterFolderBrowser1.Multiselect = false;
            this.betterFolderBrowser1.RootFolder = "C:\\Users\\zanac2\\Desktop";
            this.betterFolderBrowser1.Title = "Please select a folder...";
            // 
            // FormTimbreManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1056, 817);
            this.Controls.Add(this.tableLayoutPanel2);
            this.Controls.Add(this.toolStrip2);
            this.Controls.Add(this.pianoControl1);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "FormTimbreManager";
            this.Text = "Timbre Manager";
            this.toolStrip2.ResumeLayout(false);
            this.toolStrip2.PerformLayout();
            this.contextMenuStrip1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private PianoControl pianoControl1;
        private ComponentModel.ToolStripBase toolStrip2;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxCh;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ToolStripButton toolStripButtonPlay;
        private System.Windows.Forms.ToolStripButton toolStripButtonHook;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxNote;
        private System.Windows.Forms.ToolStripLabel toolStripLabel4;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxVelo;
        private System.Windows.Forms.ToolStripLabel toolStripLabel3;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxGate;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripLabel toolStripLabel5;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxCC;
        private System.Windows.Forms.ListView listViewCurrentTimbres;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private MetroFramework.Controls.MetroButton buttonOK;
        private MetroFramework.Controls.MetroButton buttonCancel;
        private ComponentModel.FileFolderList fileFolderList1;
        private MetroFramework.Controls.MetroTextBox metroTextBox1;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private MetroFramework.Controls.MetroLabel metroLabelDir;
        private MetroFramework.Controls.MetroLabel metroLabel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private MetroFramework.Controls.MetroLabel metroLabel2;
        private System.Windows.Forms.ListView listViewFilesTimbres;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private MetroFramework.Controls.MetroButton metroButtonNewDir;
        private MetroFramework.Controls.MetroButton metroButtonDelete;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private MetroFramework.Controls.MetroButton metroButtonExport;
        private MetroFramework.Controls.MetroLabel metroLabel3;
        private MetroFramework.Controls.MetroButton metroButton1;
        private WK.Libraries.BetterFolderBrowserNS.BetterFolderBrowser betterFolderBrowser1;
        private MetroFramework.Controls.MetroButton metroButton2;
        private MetroFramework.Controls.MetroButton metroButtonRefresh;
        private MetroFramework.Controls.MetroButton metroButtonExplorer;
        private MetroFramework.Controls.MetroButton metroButton3;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem clearToolStripMenuItem;
    }
}