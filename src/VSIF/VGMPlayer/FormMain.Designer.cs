
namespace zanac.VGMPlayer
{
    partial class FormMain
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
            if (disposing && (currentSong != null))
            {
                currentSong.Dispose();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fILEToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.eXITToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.tableLayoutPanelPort = new System.Windows.Forms.TableLayoutPanel();
            this.checkBoxConnOPNA2 = new System.Windows.Forms.CheckBox();
            this.checkBoxConnOPLL = new System.Windows.Forms.CheckBox();
            this.comboBoxPortYM2612 = new System.Windows.Forms.ComboBox();
            this.comboBoxPortYm2413 = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.comboBoxOPNA2 = new System.Windows.Forms.ComboBox();
            this.comboBoxOPLL = new System.Windows.Forms.ComboBox();
            this.comboBoxDCSG = new System.Windows.Forms.ComboBox();
            this.comboBoxPortSN76489 = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.checkBoxConnDCSG = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.label9 = new System.Windows.Forms.Label();
            this.numericUpDown2 = new System.Windows.Forms.NumericUpDown();
            this.label10 = new System.Windows.Forms.Label();
            this.numericUpDown3 = new System.Windows.Forms.NumericUpDown();
            this.tableLayoutPanelButton = new System.Windows.Forms.TableLayoutPanel();
            this.buttonPrev = new System.Windows.Forms.Button();
            this.imageListSmall = new System.Windows.Forms.ImageList(this.components);
            this.checkBoxLoop = new System.Windows.Forms.CheckBox();
            this.buttonFast = new System.Windows.Forms.Button();
            this.buttonSlow = new System.Windows.Forms.Button();
            this.buttonFreeze = new System.Windows.Forms.Button();
            this.buttonPlay = new System.Windows.Forms.Button();
            this.imageListBig = new System.Windows.Forms.ImageList(this.components);
            this.labelSpeed = new System.Windows.Forms.Label();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.buttonStop = new System.Windows.Forms.Button();
            this.buttonNext = new System.Windows.Forms.Button();
            this.buttonClear = new System.Windows.Forms.Button();
            this.buttonEject = new System.Windows.Forms.Button();
            this.contextMenuStripList = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.playToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.explorerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.labelLoad = new System.Windows.Forms.Label();
            this.progressBarLoad = new System.Windows.Forms.ProgressBar();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.label7 = new System.Windows.Forms.Label();
            this.textBoxTitle = new System.Windows.Forms.TextBox();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.listViewList = new ListViewInsertionDrag.DraggableListView();
            this.columnHeaderFile = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.menuStrip1.SuspendLayout();
            this.tableLayoutPanelPort.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown3)).BeginInit();
            this.tableLayoutPanelButton.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.contextMenuStripList.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fILEToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(788, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fILEToolStripMenuItem
            // 
            this.fILEToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.eXITToolStripMenuItem});
            this.fILEToolStripMenuItem.Name = "fILEToolStripMenuItem";
            this.fILEToolStripMenuItem.Size = new System.Drawing.Size(40, 20);
            this.fILEToolStripMenuItem.Text = "&FILE";
            // 
            // eXITToolStripMenuItem
            // 
            this.eXITToolStripMenuItem.Name = "eXITToolStripMenuItem";
            this.eXITToolStripMenuItem.Size = new System.Drawing.Size(96, 22);
            this.eXITToolStripMenuItem.Text = "&EXIT";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(6, 6);
            this.label1.Margin = new System.Windows.Forms.Padding(3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "Chip";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(64, 6);
            this.label2.Margin = new System.Windows.Forms.Padding(3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "VSIF Type";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Location = new System.Drawing.Point(6, 24);
            this.label3.Margin = new System.Windows.Forms.Padding(3);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(52, 20);
            this.label3.TabIndex = 0;
            this.label3.Text = "SN&76489:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label4.Location = new System.Drawing.Point(6, 50);
            this.label4.Margin = new System.Windows.Forms.Padding(3);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(52, 20);
            this.label4.TabIndex = 4;
            this.label4.Text = "YM2&413:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableLayoutPanelPort
            // 
            this.tableLayoutPanelPort.AutoScroll = true;
            this.tableLayoutPanelPort.ColumnCount = 4;
            this.tableLayoutPanelPort.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelPort.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelPort.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 192F));
            this.tableLayoutPanelPort.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelPort.Controls.Add(this.checkBoxConnOPNA2, 3, 3);
            this.tableLayoutPanelPort.Controls.Add(this.checkBoxConnOPLL, 3, 2);
            this.tableLayoutPanelPort.Controls.Add(this.comboBoxPortYM2612, 2, 3);
            this.tableLayoutPanelPort.Controls.Add(this.comboBoxPortYm2413, 2, 2);
            this.tableLayoutPanelPort.Controls.Add(this.label4, 0, 2);
            this.tableLayoutPanelPort.Controls.Add(this.label3, 0, 1);
            this.tableLayoutPanelPort.Controls.Add(this.label2, 1, 0);
            this.tableLayoutPanelPort.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanelPort.Controls.Add(this.label5, 0, 3);
            this.tableLayoutPanelPort.Controls.Add(this.label6, 2, 0);
            this.tableLayoutPanelPort.Controls.Add(this.comboBoxOPNA2, 1, 3);
            this.tableLayoutPanelPort.Controls.Add(this.comboBoxOPLL, 1, 2);
            this.tableLayoutPanelPort.Controls.Add(this.comboBoxDCSG, 1, 1);
            this.tableLayoutPanelPort.Controls.Add(this.comboBoxPortSN76489, 2, 1);
            this.tableLayoutPanelPort.Controls.Add(this.label8, 3, 0);
            this.tableLayoutPanelPort.Controls.Add(this.checkBoxConnDCSG, 3, 1);
            this.tableLayoutPanelPort.Controls.Add(this.tableLayoutPanel3, 1, 5);
            this.tableLayoutPanelPort.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanelPort.Location = new System.Drawing.Point(0, 24);
            this.tableLayoutPanelPort.Name = "tableLayoutPanelPort";
            this.tableLayoutPanelPort.Padding = new System.Windows.Forms.Padding(3);
            this.tableLayoutPanelPort.RowCount = 6;
            this.tableLayoutPanelPort.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelPort.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelPort.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelPort.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelPort.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelPort.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelPort.Size = new System.Drawing.Size(788, 162);
            this.tableLayoutPanelPort.TabIndex = 1;
            // 
            // checkBoxConnOPNA2
            // 
            this.checkBoxConnOPNA2.AutoSize = true;
            this.checkBoxConnOPNA2.Checked = global::zanac.VGMPlayer.Properties.Settings.Default.OPNA2_Enable;
            this.checkBoxConnOPNA2.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::zanac.VGMPlayer.Properties.Settings.Default, "OPNA2_Enable", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.checkBoxConnOPNA2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxConnOPNA2.Location = new System.Drawing.Point(716, 76);
            this.checkBoxConnOPNA2.Name = "checkBoxConnOPNA2";
            this.checkBoxConnOPNA2.Size = new System.Drawing.Size(66, 20);
            this.checkBoxConnOPNA2.TabIndex = 11;
            this.checkBoxConnOPNA2.Text = "Connect";
            this.checkBoxConnOPNA2.UseVisualStyleBackColor = true;
            this.checkBoxConnOPNA2.CheckedChanged += new System.EventHandler(this.checkBoxConnOPNA2_CheckedChanged);
            // 
            // checkBoxConnOPLL
            // 
            this.checkBoxConnOPLL.AutoSize = true;
            this.checkBoxConnOPLL.Checked = global::zanac.VGMPlayer.Properties.Settings.Default.OPLL_Enable;
            this.checkBoxConnOPLL.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::zanac.VGMPlayer.Properties.Settings.Default, "OPLL_Enable", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.checkBoxConnOPLL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxConnOPLL.Location = new System.Drawing.Point(716, 50);
            this.checkBoxConnOPLL.Name = "checkBoxConnOPLL";
            this.checkBoxConnOPLL.Size = new System.Drawing.Size(66, 20);
            this.checkBoxConnOPLL.TabIndex = 7;
            this.checkBoxConnOPLL.Text = "Connect";
            this.checkBoxConnOPLL.UseVisualStyleBackColor = true;
            this.checkBoxConnOPLL.CheckedChanged += new System.EventHandler(this.checkBoxConnOPLL_CheckedChanged);
            // 
            // comboBoxPortYM2612
            // 
            this.comboBoxPortYM2612.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", global::zanac.VGMPlayer.Properties.Settings.Default, "OPNA2_Port", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.comboBoxPortYM2612.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxPortYM2612.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxPortYM2612.FormattingEnabled = true;
            this.comboBoxPortYM2612.Items.AddRange(new object[] {
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
            "127",
            "128",
            "129",
            "130",
            "131",
            "132",
            "133",
            "134",
            "135",
            "136",
            "137",
            "138",
            "139",
            "140",
            "141",
            "142",
            "143",
            "144",
            "145",
            "146",
            "147",
            "148",
            "149",
            "150",
            "151",
            "152",
            "153",
            "154",
            "155",
            "156",
            "157",
            "158",
            "159",
            "160",
            "161",
            "162",
            "163",
            "164",
            "165",
            "166",
            "167",
            "168",
            "169",
            "170",
            "171",
            "172",
            "173",
            "174",
            "175",
            "176",
            "177",
            "178",
            "179",
            "180",
            "181",
            "182",
            "183",
            "184",
            "185",
            "186",
            "187",
            "188",
            "189",
            "190",
            "191",
            "192",
            "193",
            "194",
            "195",
            "196",
            "197",
            "198",
            "199",
            "200",
            "201",
            "202",
            "203",
            "204",
            "205",
            "206",
            "207",
            "208",
            "209",
            "210",
            "211",
            "212",
            "213",
            "214",
            "215",
            "216",
            "217",
            "218",
            "219",
            "220",
            "221",
            "222",
            "223",
            "224",
            "225",
            "226",
            "227",
            "228",
            "229",
            "230",
            "231",
            "232",
            "233",
            "234",
            "235",
            "236",
            "237",
            "238",
            "239",
            "240",
            "241",
            "242",
            "243",
            "244",
            "245",
            "246",
            "247",
            "248",
            "249",
            "250",
            "251",
            "252",
            "253",
            "254",
            "255",
            "256"});
            this.comboBoxPortYM2612.Location = new System.Drawing.Point(524, 76);
            this.comboBoxPortYM2612.Name = "comboBoxPortYM2612";
            this.comboBoxPortYM2612.Size = new System.Drawing.Size(186, 20);
            this.comboBoxPortYM2612.TabIndex = global::zanac.VGMPlayer.Properties.Settings.Default.OPNA2_Port;
            // 
            // comboBoxPortYm2413
            // 
            this.comboBoxPortYm2413.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", global::zanac.VGMPlayer.Properties.Settings.Default, "OPLL_Port", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.comboBoxPortYm2413.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxPortYm2413.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxPortYm2413.FormattingEnabled = true;
            this.comboBoxPortYm2413.Items.AddRange(new object[] {
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
            "127",
            "128",
            "129",
            "130",
            "131",
            "132",
            "133",
            "134",
            "135",
            "136",
            "137",
            "138",
            "139",
            "140",
            "141",
            "142",
            "143",
            "144",
            "145",
            "146",
            "147",
            "148",
            "149",
            "150",
            "151",
            "152",
            "153",
            "154",
            "155",
            "156",
            "157",
            "158",
            "159",
            "160",
            "161",
            "162",
            "163",
            "164",
            "165",
            "166",
            "167",
            "168",
            "169",
            "170",
            "171",
            "172",
            "173",
            "174",
            "175",
            "176",
            "177",
            "178",
            "179",
            "180",
            "181",
            "182",
            "183",
            "184",
            "185",
            "186",
            "187",
            "188",
            "189",
            "190",
            "191",
            "192",
            "193",
            "194",
            "195",
            "196",
            "197",
            "198",
            "199",
            "200",
            "201",
            "202",
            "203",
            "204",
            "205",
            "206",
            "207",
            "208",
            "209",
            "210",
            "211",
            "212",
            "213",
            "214",
            "215",
            "216",
            "217",
            "218",
            "219",
            "220",
            "221",
            "222",
            "223",
            "224",
            "225",
            "226",
            "227",
            "228",
            "229",
            "230",
            "231",
            "232",
            "233",
            "234",
            "235",
            "236",
            "237",
            "238",
            "239",
            "240",
            "241",
            "242",
            "243",
            "244",
            "245",
            "246",
            "247",
            "248",
            "249",
            "250",
            "251",
            "252",
            "253",
            "254",
            "255",
            "256"});
            this.comboBoxPortYm2413.Location = new System.Drawing.Point(524, 50);
            this.comboBoxPortYm2413.Name = "comboBoxPortYm2413";
            this.comboBoxPortYm2413.Size = new System.Drawing.Size(186, 20);
            this.comboBoxPortYm2413.TabIndex = global::zanac.VGMPlayer.Properties.Settings.Default.OPLL_Port;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label5.Location = new System.Drawing.Point(6, 76);
            this.label5.Margin = new System.Windows.Forms.Padding(3);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(52, 20);
            this.label5.TabIndex = 8;
            this.label5.Text = "YM2&612:";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(524, 6);
            this.label6.Margin = new System.Windows.Forms.Padding(3);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(101, 12);
            this.label6.TabIndex = 1;
            this.label6.Text = "COM Port/FDTI ID";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // comboBoxOPNA2
            // 
            this.comboBoxOPNA2.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", global::zanac.VGMPlayer.Properties.Settings.Default, "OPNA2_IF", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.comboBoxOPNA2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxOPNA2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxOPNA2.FormattingEnabled = true;
            this.comboBoxOPNA2.Items.AddRange(new object[] {
            "VSIF - Genesis(UART 163Kbps)",
            "VSIF - Genesis(FTDI2XX) (for Expert)",
            "VSIF - Genesis(UART 115Kbps)"});
            this.comboBoxOPNA2.Location = new System.Drawing.Point(64, 76);
            this.comboBoxOPNA2.Name = "comboBoxOPNA2";
            this.comboBoxOPNA2.Size = new System.Drawing.Size(454, 20);
            this.comboBoxOPNA2.TabIndex = global::zanac.VGMPlayer.Properties.Settings.Default.OPNA2_IF;
            // 
            // comboBoxOPLL
            // 
            this.comboBoxOPLL.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", global::zanac.VGMPlayer.Properties.Settings.Default, "OPLL_IF", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.comboBoxOPLL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxOPLL.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxOPLL.FormattingEnabled = true;
            this.comboBoxOPLL.Items.AddRange(new object[] {
            "VSIF - SMS(UART 115Kbps)"});
            this.comboBoxOPLL.Location = new System.Drawing.Point(64, 50);
            this.comboBoxOPLL.Name = "comboBoxOPLL";
            this.comboBoxOPLL.Size = new System.Drawing.Size(454, 20);
            this.comboBoxOPLL.TabIndex = global::zanac.VGMPlayer.Properties.Settings.Default.OPLL_IF;
            // 
            // comboBoxDCSG
            // 
            this.comboBoxDCSG.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", global::zanac.VGMPlayer.Properties.Settings.Default, "DCSG_IF", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.comboBoxDCSG.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxDCSG.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDCSG.FormattingEnabled = true;
            this.comboBoxDCSG.Items.AddRange(new object[] {
            "VSIF - Genesis(UART 163Kbps)",
            "VSIF - Genesis(FTDI2XX)  (for Expert)",
            "VSIF - SMS",
            "VSIF - Genesis(UART 115Kbps)"});
            this.comboBoxDCSG.Location = new System.Drawing.Point(64, 24);
            this.comboBoxDCSG.Name = "comboBoxDCSG";
            this.comboBoxDCSG.Size = new System.Drawing.Size(454, 20);
            this.comboBoxDCSG.TabIndex = global::zanac.VGMPlayer.Properties.Settings.Default.DCSG_IF;
            // 
            // comboBoxPortSN76489
            // 
            this.comboBoxPortSN76489.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", global::zanac.VGMPlayer.Properties.Settings.Default, "DCSG_Port", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.comboBoxPortSN76489.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxPortSN76489.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxPortSN76489.FormattingEnabled = true;
            this.comboBoxPortSN76489.Items.AddRange(new object[] {
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
            "127",
            "128",
            "129",
            "130",
            "131",
            "132",
            "133",
            "134",
            "135",
            "136",
            "137",
            "138",
            "139",
            "140",
            "141",
            "142",
            "143",
            "144",
            "145",
            "146",
            "147",
            "148",
            "149",
            "150",
            "151",
            "152",
            "153",
            "154",
            "155",
            "156",
            "157",
            "158",
            "159",
            "160",
            "161",
            "162",
            "163",
            "164",
            "165",
            "166",
            "167",
            "168",
            "169",
            "170",
            "171",
            "172",
            "173",
            "174",
            "175",
            "176",
            "177",
            "178",
            "179",
            "180",
            "181",
            "182",
            "183",
            "184",
            "185",
            "186",
            "187",
            "188",
            "189",
            "190",
            "191",
            "192",
            "193",
            "194",
            "195",
            "196",
            "197",
            "198",
            "199",
            "200",
            "201",
            "202",
            "203",
            "204",
            "205",
            "206",
            "207",
            "208",
            "209",
            "210",
            "211",
            "212",
            "213",
            "214",
            "215",
            "216",
            "217",
            "218",
            "219",
            "220",
            "221",
            "222",
            "223",
            "224",
            "225",
            "226",
            "227",
            "228",
            "229",
            "230",
            "231",
            "232",
            "233",
            "234",
            "235",
            "236",
            "237",
            "238",
            "239",
            "240",
            "241",
            "242",
            "243",
            "244",
            "245",
            "246",
            "247",
            "248",
            "249",
            "250",
            "251",
            "252",
            "253",
            "254",
            "255",
            "256"});
            this.comboBoxPortSN76489.Location = new System.Drawing.Point(524, 24);
            this.comboBoxPortSN76489.Name = "comboBoxPortSN76489";
            this.comboBoxPortSN76489.Size = new System.Drawing.Size(186, 20);
            this.comboBoxPortSN76489.TabIndex = global::zanac.VGMPlayer.Properties.Settings.Default.DCSG_Port;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(716, 6);
            this.label8.Margin = new System.Windows.Forms.Padding(3);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(47, 12);
            this.label8.TabIndex = 2;
            this.label8.Text = "Connect";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // checkBoxConnDCSG
            // 
            this.checkBoxConnDCSG.AutoSize = true;
            this.checkBoxConnDCSG.Checked = global::zanac.VGMPlayer.Properties.Settings.Default.DCSG_Enable;
            this.checkBoxConnDCSG.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::zanac.VGMPlayer.Properties.Settings.Default, "DCSG_Enable", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.checkBoxConnDCSG.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxConnDCSG.Location = new System.Drawing.Point(716, 24);
            this.checkBoxConnDCSG.Name = "checkBoxConnDCSG";
            this.checkBoxConnDCSG.Size = new System.Drawing.Size(66, 20);
            this.checkBoxConnDCSG.TabIndex = 3;
            this.checkBoxConnDCSG.Text = "Connect";
            this.checkBoxConnDCSG.UseVisualStyleBackColor = true;
            this.checkBoxConnDCSG.CheckedChanged += new System.EventHandler(this.checkBoxConnDCSG_CheckedChanged);
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.AutoSize = true;
            this.tableLayoutPanel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel3.ColumnCount = 5;
            this.tableLayoutPanelPort.SetColumnSpan(this.tableLayoutPanel3, 2);
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.label9, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.numericUpDown2, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.label10, 2, 0);
            this.tableLayoutPanel3.Controls.Add(this.numericUpDown3, 3, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(64, 131);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.Size = new System.Drawing.Size(646, 25);
            this.tableLayoutPanel3.TabIndex = 12;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label9.Location = new System.Drawing.Point(3, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(123, 25);
            this.label9.TabIndex = 0;
            this.label9.Text = "BitBang CLK Width [%]:";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // numericUpDown2
            // 
            this.numericUpDown2.DataBindings.Add(new System.Windows.Forms.Binding("Value", global::zanac.VGMPlayer.Properties.Settings.Default, "BitBangWait", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.numericUpDown2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numericUpDown2.Location = new System.Drawing.Point(132, 3);
            this.numericUpDown2.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown2.Name = "numericUpDown2";
            this.numericUpDown2.Size = new System.Drawing.Size(100, 19);
            this.numericUpDown2.TabIndex = 1;
            this.numericUpDown2.Value = global::zanac.VGMPlayer.Properties.Settings.Default.BitBangWait;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label10.Location = new System.Drawing.Point(238, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(117, 25);
            this.label10.TabIndex = 0;
            this.label10.Text = "Cmd Buffer[Samples]:";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // numericUpDown3
            // 
            this.numericUpDown3.DataBindings.Add(new System.Windows.Forms.Binding("Value", global::zanac.VGMPlayer.Properties.Settings.Default, "VGMWait", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.numericUpDown3.Location = new System.Drawing.Point(361, 3);
            this.numericUpDown3.Name = "numericUpDown3";
            this.numericUpDown3.Size = new System.Drawing.Size(120, 19);
            this.numericUpDown3.TabIndex = 2;
            this.numericUpDown3.Value = global::zanac.VGMPlayer.Properties.Settings.Default.VGMWait;
            // 
            // tableLayoutPanelButton
            // 
            this.tableLayoutPanelButton.ColumnCount = 11;
            this.tableLayoutPanelButton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelButton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelButton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelButton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelButton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelButton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelButton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelButton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelButton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelButton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelButton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelButton.Controls.Add(this.buttonPrev, 0, 0);
            this.tableLayoutPanelButton.Controls.Add(this.checkBoxLoop, 7, 0);
            this.tableLayoutPanelButton.Controls.Add(this.buttonFast, 6, 0);
            this.tableLayoutPanelButton.Controls.Add(this.buttonSlow, 5, 0);
            this.tableLayoutPanelButton.Controls.Add(this.buttonFreeze, 4, 0);
            this.tableLayoutPanelButton.Controls.Add(this.buttonPlay, 1, 0);
            this.tableLayoutPanelButton.Controls.Add(this.labelSpeed, 5, 1);
            this.tableLayoutPanelButton.Controls.Add(this.numericUpDown1, 7, 1);
            this.tableLayoutPanelButton.Controls.Add(this.buttonStop, 3, 0);
            this.tableLayoutPanelButton.Controls.Add(this.buttonNext, 2, 0);
            this.tableLayoutPanelButton.Controls.Add(this.buttonClear, 10, 0);
            this.tableLayoutPanelButton.Controls.Add(this.buttonEject, 9, 0);
            this.tableLayoutPanelButton.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanelButton.Location = new System.Drawing.Point(0, 399);
            this.tableLayoutPanelButton.Name = "tableLayoutPanelButton";
            this.tableLayoutPanelButton.RowCount = 2;
            this.tableLayoutPanelButton.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelButton.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelButton.Size = new System.Drawing.Size(788, 85);
            this.tableLayoutPanelButton.TabIndex = 4;
            // 
            // buttonPrev
            // 
            this.buttonPrev.AutoSize = true;
            this.buttonPrev.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.buttonPrev.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonPrev.FlatAppearance.BorderSize = 0;
            this.buttonPrev.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonPrev.ImageIndex = 0;
            this.buttonPrev.ImageList = this.imageListSmall;
            this.buttonPrev.Location = new System.Drawing.Point(3, 3);
            this.buttonPrev.Name = "buttonPrev";
            this.buttonPrev.Size = new System.Drawing.Size(38, 54);
            this.buttonPrev.TabIndex = 0;
            this.buttonPrev.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.buttonPrev.UseVisualStyleBackColor = true;
            this.buttonPrev.Click += new System.EventHandler(this.buttonPrev_Click);
            // 
            // imageListSmall
            // 
            this.imageListSmall.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListSmall.ImageStream")));
            this.imageListSmall.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListSmall.Images.SetKeyName(0, "Prev.png");
            this.imageListSmall.Images.SetKeyName(1, "Next.png");
            this.imageListSmall.Images.SetKeyName(2, "Stop.png");
            this.imageListSmall.Images.SetKeyName(3, "Freeze.png");
            this.imageListSmall.Images.SetKeyName(4, "Slow.png");
            this.imageListSmall.Images.SetKeyName(5, "Fast.png");
            this.imageListSmall.Images.SetKeyName(6, "Loop.png");
            this.imageListSmall.Images.SetKeyName(7, "Clear.png");
            this.imageListSmall.Images.SetKeyName(8, "Eject.png");
            // 
            // checkBoxLoop
            // 
            this.checkBoxLoop.AutoSize = true;
            this.checkBoxLoop.Checked = global::zanac.VGMPlayer.Properties.Settings.Default.Loop;
            this.checkBoxLoop.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxLoop.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::zanac.VGMPlayer.Properties.Settings.Default, "Loop", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.checkBoxLoop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxLoop.FlatAppearance.BorderSize = 0;
            this.checkBoxLoop.ImageIndex = 6;
            this.checkBoxLoop.ImageList = this.imageListSmall;
            this.checkBoxLoop.Location = new System.Drawing.Point(365, 3);
            this.checkBoxLoop.Name = "checkBoxLoop";
            this.checkBoxLoop.Size = new System.Drawing.Size(53, 54);
            this.checkBoxLoop.TabIndex = 8;
            this.checkBoxLoop.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.checkBoxLoop.UseVisualStyleBackColor = true;
            this.checkBoxLoop.CheckedChanged += new System.EventHandler(this.checkBoxLoop_CheckedChanged);
            // 
            // buttonFast
            // 
            this.buttonFast.AutoSize = true;
            this.buttonFast.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.buttonFast.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonFast.FlatAppearance.BorderSize = 0;
            this.buttonFast.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonFast.ImageIndex = 5;
            this.buttonFast.ImageList = this.imageListSmall;
            this.buttonFast.Location = new System.Drawing.Point(321, 3);
            this.buttonFast.Name = "buttonFast";
            this.buttonFast.Size = new System.Drawing.Size(38, 54);
            this.buttonFast.TabIndex = 7;
            this.buttonFast.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.buttonFast.UseVisualStyleBackColor = true;
            this.buttonFast.Click += new System.EventHandler(this.buttonFast_Click);
            // 
            // buttonSlow
            // 
            this.buttonSlow.AutoSize = true;
            this.buttonSlow.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.buttonSlow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonSlow.FlatAppearance.BorderSize = 0;
            this.buttonSlow.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonSlow.ImageIndex = 4;
            this.buttonSlow.ImageList = this.imageListSmall;
            this.buttonSlow.Location = new System.Drawing.Point(277, 3);
            this.buttonSlow.Name = "buttonSlow";
            this.buttonSlow.Size = new System.Drawing.Size(38, 54);
            this.buttonSlow.TabIndex = 5;
            this.buttonSlow.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.buttonSlow.UseVisualStyleBackColor = true;
            this.buttonSlow.Click += new System.EventHandler(this.buttonSlow_Click);
            // 
            // buttonFreeze
            // 
            this.buttonFreeze.AutoSize = true;
            this.buttonFreeze.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.buttonFreeze.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonFreeze.FlatAppearance.BorderSize = 0;
            this.buttonFreeze.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonFreeze.ImageIndex = 3;
            this.buttonFreeze.ImageList = this.imageListSmall;
            this.buttonFreeze.Location = new System.Drawing.Point(233, 3);
            this.buttonFreeze.Name = "buttonFreeze";
            this.buttonFreeze.Size = new System.Drawing.Size(38, 54);
            this.buttonFreeze.TabIndex = 4;
            this.buttonFreeze.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.buttonFreeze.UseVisualStyleBackColor = true;
            this.buttonFreeze.Click += new System.EventHandler(this.buttonFreeze_Click);
            // 
            // buttonPlay
            // 
            this.buttonPlay.AllowDrop = true;
            this.buttonPlay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonPlay.FlatAppearance.BorderSize = 0;
            this.buttonPlay.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonPlay.ImageIndex = 0;
            this.buttonPlay.ImageList = this.imageListBig;
            this.buttonPlay.Location = new System.Drawing.Point(47, 3);
            this.buttonPlay.Name = "buttonPlay";
            this.tableLayoutPanelButton.SetRowSpan(this.buttonPlay, 2);
            this.buttonPlay.Size = new System.Drawing.Size(92, 79);
            this.buttonPlay.TabIndex = 1;
            this.buttonPlay.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.buttonPlay.UseVisualStyleBackColor = true;
            this.buttonPlay.Click += new System.EventHandler(this.buttonPlay_Click);
            this.buttonPlay.DragDrop += new System.Windows.Forms.DragEventHandler(this.buttonPlay_DragDrop);
            this.buttonPlay.DragEnter += new System.Windows.Forms.DragEventHandler(this.buttonPlay_DragEnter);
            // 
            // imageListBig
            // 
            this.imageListBig.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListBig.ImageStream")));
            this.imageListBig.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListBig.Images.SetKeyName(0, "Play.png");
            this.imageListBig.Images.SetKeyName(1, "Pause.png");
            // 
            // labelSpeed
            // 
            this.labelSpeed.AutoSize = true;
            this.tableLayoutPanelButton.SetColumnSpan(this.labelSpeed, 2);
            this.labelSpeed.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelSpeed.Location = new System.Drawing.Point(277, 60);
            this.labelSpeed.Name = "labelSpeed";
            this.labelSpeed.Size = new System.Drawing.Size(82, 25);
            this.labelSpeed.TabIndex = 6;
            this.labelSpeed.Text = "1.00";
            this.labelSpeed.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(365, 63);
            this.numericUpDown1.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(53, 19);
            this.numericUpDown1.TabIndex = 9;
            this.numericUpDown1.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // buttonStop
            // 
            this.buttonStop.AutoSize = true;
            this.buttonStop.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.buttonStop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonStop.FlatAppearance.BorderSize = 0;
            this.buttonStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonStop.ImageIndex = 2;
            this.buttonStop.ImageList = this.imageListSmall;
            this.buttonStop.Location = new System.Drawing.Point(189, 3);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(38, 54);
            this.buttonStop.TabIndex = 3;
            this.buttonStop.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.buttonStop.UseVisualStyleBackColor = true;
            this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
            // 
            // buttonNext
            // 
            this.buttonNext.AutoSize = true;
            this.buttonNext.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.buttonNext.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonNext.FlatAppearance.BorderSize = 0;
            this.buttonNext.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonNext.ImageIndex = 1;
            this.buttonNext.ImageList = this.imageListSmall;
            this.buttonNext.Location = new System.Drawing.Point(145, 3);
            this.buttonNext.Name = "buttonNext";
            this.buttonNext.Size = new System.Drawing.Size(38, 54);
            this.buttonNext.TabIndex = 2;
            this.buttonNext.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.buttonNext.UseVisualStyleBackColor = true;
            this.buttonNext.Click += new System.EventHandler(this.buttonNext_Click);
            // 
            // buttonClear
            // 
            this.buttonClear.AutoSize = true;
            this.buttonClear.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.buttonClear.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonClear.FlatAppearance.BorderSize = 0;
            this.buttonClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonClear.ImageIndex = 7;
            this.buttonClear.ImageList = this.imageListSmall;
            this.buttonClear.Location = new System.Drawing.Point(747, 3);
            this.buttonClear.Name = "buttonClear";
            this.buttonClear.Size = new System.Drawing.Size(38, 54);
            this.buttonClear.TabIndex = 10;
            this.buttonClear.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.buttonClear.UseVisualStyleBackColor = true;
            this.buttonClear.Click += new System.EventHandler(this.buttonClear_Click);
            // 
            // buttonEject
            // 
            this.buttonEject.AutoSize = true;
            this.buttonEject.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.buttonEject.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonEject.FlatAppearance.BorderSize = 0;
            this.buttonEject.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonEject.ImageIndex = 8;
            this.buttonEject.ImageList = this.imageListSmall;
            this.buttonEject.Location = new System.Drawing.Point(703, 3);
            this.buttonEject.Name = "buttonEject";
            this.buttonEject.Size = new System.Drawing.Size(38, 54);
            this.buttonEject.TabIndex = 10;
            this.buttonEject.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.buttonEject.UseVisualStyleBackColor = true;
            this.buttonEject.Click += new System.EventHandler(this.buttonEject_Click);
            // 
            // contextMenuStripList
            // 
            this.contextMenuStripList.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.playToolStripMenuItem,
            this.removeToolStripMenuItem,
            this.explorerToolStripMenuItem});
            this.contextMenuStripList.Name = "contextMenuStrip1";
            this.contextMenuStripList.Size = new System.Drawing.Size(118, 70);
            // 
            // playToolStripMenuItem
            // 
            this.playToolStripMenuItem.Name = "playToolStripMenuItem";
            this.playToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
            this.playToolStripMenuItem.Text = "&Play";
            this.playToolStripMenuItem.Click += new System.EventHandler(this.playToolStripMenuItem_Click);
            // 
            // removeToolStripMenuItem
            // 
            this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            this.removeToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
            this.removeToolStripMenuItem.Text = "&Remove";
            this.removeToolStripMenuItem.Click += new System.EventHandler(this.removeToolStripMenuItem_Click);
            // 
            // explorerToolStripMenuItem
            // 
            this.explorerToolStripMenuItem.Name = "explorerToolStripMenuItem";
            this.explorerToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
            this.explorerToolStripMenuItem.Text = "&Explorer";
            this.explorerToolStripMenuItem.Click += new System.EventHandler(this.explorerToolStripMenuItem_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.labelLoad, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.progressBarLoad, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 484);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(788, 32);
            this.tableLayoutPanel1.TabIndex = 5;
            // 
            // labelLoad
            // 
            this.labelLoad.AutoSize = true;
            this.labelLoad.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelLoad.Location = new System.Drawing.Point(3, 0);
            this.labelLoad.Name = "labelLoad";
            this.labelLoad.Size = new System.Drawing.Size(29, 32);
            this.labelLoad.TabIndex = 0;
            this.labelLoad.Text = "Load";
            this.labelLoad.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // progressBarLoad
            // 
            this.progressBarLoad.Dock = System.Windows.Forms.DockStyle.Fill;
            this.progressBarLoad.Location = new System.Drawing.Point(38, 3);
            this.progressBarLoad.Name = "progressBarLoad";
            this.progressBarLoad.Size = new System.Drawing.Size(747, 26);
            this.progressBarLoad.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBarLoad.TabIndex = 1;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.label7, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.textBoxTitle, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 373);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(788, 26);
            this.tableLayoutPanel2.TabIndex = 3;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label7.Location = new System.Drawing.Point(3, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(43, 26);
            this.label7.TabIndex = 0;
            this.label7.Text = "&Current";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBoxTitle
            // 
            this.textBoxTitle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxTitle.Location = new System.Drawing.Point(52, 3);
            this.textBoxTitle.Name = "textBoxTitle";
            this.textBoxTitle.ReadOnly = true;
            this.textBoxTitle.ShortcutsEnabled = false;
            this.textBoxTitle.Size = new System.Drawing.Size(733, 19);
            this.textBoxTitle.TabIndex = 1;
            // 
            // openFileDialog
            // 
            this.openFileDialog.Filter = "Video Game Music Files(*.vgm;*.vgz;*.xgm)|*.vgm;*.vgz;*.xgm";
            this.openFileDialog.Multiselect = true;
            this.openFileDialog.Title = "Select VGM/XGM files";
            // 
            // listViewList
            // 
            this.listViewList.AllowDrop = true;
            this.listViewList.AllowItemDrag = true;
            this.listViewList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderFile});
            this.listViewList.ContextMenuStrip = this.contextMenuStripList;
            this.listViewList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewList.FullRowSelect = true;
            this.listViewList.GridLines = true;
            this.listViewList.HideSelection = false;
            this.listViewList.LabelWrap = false;
            this.listViewList.Location = new System.Drawing.Point(0, 186);
            this.listViewList.Name = "listViewList";
            this.listViewList.Size = new System.Drawing.Size(788, 187);
            this.listViewList.TabIndex = 2;
            this.listViewList.UseCompatibleStateImageBehavior = false;
            this.listViewList.View = System.Windows.Forms.View.Details;
            this.listViewList.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listViewList_ColumnClick);
            this.listViewList.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.listViewList_ItemDrag);
            this.listViewList.SizeChanged += new System.EventHandler(this.listViewList_SizeChanged);
            this.listViewList.DragDrop += new System.Windows.Forms.DragEventHandler(this.listViewList_DragDrop);
            this.listViewList.DragEnter += new System.Windows.Forms.DragEventHandler(this.listView1_DragEnter);
            this.listViewList.DragOver += new System.Windows.Forms.DragEventHandler(this.listViewList_DragOver);
            this.listViewList.DragLeave += new System.EventHandler(this.listViewList_DragLeave);
            this.listViewList.DoubleClick += new System.EventHandler(this.listViewList_DoubleClick);
            this.listViewList.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listViewList_KeyDown);
            // 
            // columnHeaderFile
            // 
            this.columnHeaderFile.Text = "File name";
            this.columnHeaderFile.Width = 325;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(788, 516);
            this.Controls.Add(this.listViewList);
            this.Controls.Add(this.tableLayoutPanel2);
            this.Controls.Add(this.tableLayoutPanelButton);
            this.Controls.Add(this.tableLayoutPanelPort);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "FormMain";
            this.Text = "VSIF Checker (VGM/XGM Player) V1.08";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormMain_KeyDown);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.tableLayoutPanelPort.ResumeLayout(false);
            this.tableLayoutPanelPort.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown3)).EndInit();
            this.tableLayoutPanelButton.ResumeLayout(false);
            this.tableLayoutPanelButton.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.contextMenuStripList.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fILEToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem eXITToolStripMenuItem;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelPort;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelButton;
        private System.Windows.Forms.Button buttonPlay;
        private System.Windows.Forms.Button buttonSlow;
        private System.Windows.Forms.Button buttonStop;
        private System.Windows.Forms.Button buttonFast;
        private System.Windows.Forms.Button buttonFreeze;
        private System.Windows.Forms.CheckBox checkBoxLoop;
        private ListViewInsertionDrag.DraggableListView listViewList;
        private System.Windows.Forms.ColumnHeader columnHeaderFile;
        private System.Windows.Forms.Button buttonPrev;
        private System.Windows.Forms.Button buttonNext;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox comboBoxOPNA2;
        private System.Windows.Forms.ComboBox comboBoxOPLL;
        private System.Windows.Forms.ComboBox comboBoxDCSG;
        private System.Windows.Forms.ComboBox comboBoxPortSN76489;
        private System.Windows.Forms.ComboBox comboBoxPortYM2612;
        private System.Windows.Forms.ComboBox comboBoxPortYm2413;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label labelLoad;
        private System.Windows.Forms.ProgressBar progressBarLoad;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ImageList imageListBig;
        private System.Windows.Forms.ImageList imageListSmall;
        private System.Windows.Forms.Label labelSpeed;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBoxTitle;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.CheckBox checkBoxConnOPNA2;
        private System.Windows.Forms.CheckBox checkBoxConnOPLL;
        private System.Windows.Forms.CheckBox checkBoxConnDCSG;
        private System.Windows.Forms.Button buttonClear;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.NumericUpDown numericUpDown2;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.NumericUpDown numericUpDown3;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripList;
        private System.Windows.Forms.ToolStripMenuItem playToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem explorerToolStripMenuItem;
        private System.Windows.Forms.Button buttonEject;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
    }
}