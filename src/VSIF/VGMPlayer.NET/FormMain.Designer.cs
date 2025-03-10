﻿
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
            zanac.VGMPlayer.Settings settings1 = new zanac.VGMPlayer.Settings();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fILEToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.eXITToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aBOUTToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.tableLayoutPanelPort = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.label10 = new System.Windows.Forms.Label();
            this.numericUpDown3 = new System.Windows.Forms.NumericUpDown();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.comboBoxSCC = new System.Windows.Forms.ComboBox();
            this.comboBoxSccSlot = new System.Windows.Forms.ComboBox();
            this.numericUpDownClkWidthDCSG = new System.Windows.Forms.NumericUpDown();
            this.label9 = new System.Windows.Forms.Label();
            this.comboBoxPortYm2413 = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.comboBoxDCSG = new System.Windows.Forms.ComboBox();
            this.comboBoxPortSN76489 = new System.Windows.Forms.ComboBox();
            this.comboBoxPortSCC = new System.Windows.Forms.ComboBox();
            this.numericUpDownOPLL = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownSCC = new System.Windows.Forms.NumericUpDown();
            this.label12 = new System.Windows.Forms.Label();
            this.comboBoxY8910 = new System.Windows.Forms.ComboBox();
            this.numericUpDownY8910 = new System.Windows.Forms.NumericUpDown();
            this.comboBoxPortY8910 = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.checkBoxConnDCSG = new System.Windows.Forms.CheckBox();
            this.checkBoxConnOPLL = new System.Windows.Forms.CheckBox();
            this.checkBoxConnSCC = new System.Windows.Forms.CheckBox();
            this.checkBoxConnY8910 = new System.Windows.Forms.CheckBox();
            this.checkBoxConnOPNA2 = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.comboBoxOPNA2 = new System.Windows.Forms.ComboBox();
            this.numericUpDownOPNA2 = new System.Windows.Forms.NumericUpDown();
            this.comboBoxPortYM2612 = new System.Windows.Forms.ComboBox();
            this.comboBoxSccType = new System.Windows.Forms.ComboBox();
            this.checkBoxConnOPM = new System.Windows.Forms.CheckBox();
            this.label11 = new System.Windows.Forms.Label();
            this.numericUpDownOPM = new System.Windows.Forms.NumericUpDown();
            this.comboBoxPortOPM = new System.Windows.Forms.ComboBox();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.comboBoxOPM = new System.Windows.Forms.ComboBox();
            this.comboBoxOpmSlot = new System.Windows.Forms.ComboBox();
            this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
            this.comboBoxOPLL = new System.Windows.Forms.ComboBox();
            this.comboBoxOpllSlot = new System.Windows.Forms.ComboBox();
            this.checkBoxConnOPL3 = new System.Windows.Forms.CheckBox();
            this.label14 = new System.Windows.Forms.Label();
            this.comboBoxOPL3 = new System.Windows.Forms.ComboBox();
            this.numericUpDownOPL3 = new System.Windows.Forms.NumericUpDown();
            this.comboBoxPortOPL3 = new System.Windows.Forms.ComboBox();
            this.checkBoxConnOPNA = new System.Windows.Forms.CheckBox();
            this.label15 = new System.Windows.Forms.Label();
            this.comboBoxOPNA = new System.Windows.Forms.ComboBox();
            this.numericUpDownOPNA = new System.Windows.Forms.NumericUpDown();
            this.comboBoxPortOPNA = new System.Windows.Forms.ComboBox();
            this.checkBoxConnY8950 = new System.Windows.Forms.CheckBox();
            this.label16 = new System.Windows.Forms.Label();
            this.numericUpDownY8950 = new System.Windows.Forms.NumericUpDown();
            this.comboBoxPortY8950 = new System.Windows.Forms.ComboBox();
            this.tableLayoutPanel7 = new System.Windows.Forms.TableLayoutPanel();
            this.comboBoxY8950 = new System.Windows.Forms.ComboBox();
            this.comboBoxY8950Slot = new System.Windows.Forms.ComboBox();
            this.checkBoxCnvClk = new System.Windows.Forms.CheckBox();
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
            this.numericUpDownLooped = new System.Windows.Forms.NumericUpDown();
            this.buttonStop = new System.Windows.Forms.Button();
            this.buttonNext = new System.Windows.Forms.Button();
            this.buttonClear = new System.Windows.Forms.Button();
            this.buttonEject = new System.Windows.Forms.Button();
            this.checkBoxLoopTimes = new System.Windows.Forms.CheckBox();
            this.dateTimePickerLoopTimes = new System.Windows.Forms.DateTimePicker();
            this.labelElapsed = new System.Windows.Forms.Label();
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
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.listViewList = new ListViewInsertionDrag.DraggableListView();
            this.columnHeaderFile = new System.Windows.Forms.ColumnHeader();
            this.menuStrip1.SuspendLayout();
            this.tableLayoutPanelPort.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown3)).BeginInit();
            this.tableLayoutPanel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownClkWidthDCSG)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownOPLL)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSCC)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownY8910)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownOPNA2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownOPM)).BeginInit();
            this.tableLayoutPanel5.SuspendLayout();
            this.tableLayoutPanel6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownOPL3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownOPNA)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownY8950)).BeginInit();
            this.tableLayoutPanel7.SuspendLayout();
            this.tableLayoutPanelButton.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownLooped)).BeginInit();
            this.contextMenuStripList.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fILEToolStripMenuItem,
            this.aBOUTToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(5, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(787, 24);
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
            // aBOUTToolStripMenuItem
            // 
            this.aBOUTToolStripMenuItem.Name = "aBOUTToolStripMenuItem";
            this.aBOUTToolStripMenuItem.Size = new System.Drawing.Size(57, 20);
            this.aBOUTToolStripMenuItem.Text = "&ABOUT";
            this.aBOUTToolStripMenuItem.Click += new System.EventHandler(this.aBOUTToolStripMenuItem_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(51, 8);
            this.label1.Margin = new System.Windows.Forms.Padding(4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(131, 15);
            this.label1.TabIndex = 1;
            this.label1.Text = "Chip";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(190, 8);
            this.label2.Margin = new System.Windows.Forms.Padding(4);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(169, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "VSIF Type, Slot type (MSX only)";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Location = new System.Drawing.Point(51, 62);
            this.label3.Margin = new System.Windows.Forms.Padding(4);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(131, 23);
            this.label3.TabIndex = 11;
            this.label3.Text = "SN&76489(DCSG)";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label3.Click += new System.EventHandler(this.label3_Click);
            this.label3.DoubleClick += new System.EventHandler(this.label3_DoubleClick);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label4.Location = new System.Drawing.Point(51, 93);
            this.label4.Margin = new System.Windows.Forms.Padding(4);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(131, 23);
            this.label4.TabIndex = 16;
            this.label4.Text = "YM2&413(OPLL)";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label4.Click += new System.EventHandler(this.label3_Click);
            this.label4.DoubleClick += new System.EventHandler(this.label3_DoubleClick);
            // 
            // tableLayoutPanelPort
            // 
            this.tableLayoutPanelPort.AutoScroll = true;
            this.tableLayoutPanelPort.ColumnCount = 5;
            this.tableLayoutPanelPort.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelPort.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelPort.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelPort.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 116F));
            this.tableLayoutPanelPort.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 147F));
            this.tableLayoutPanelPort.Controls.Add(this.tableLayoutPanel3, 2, 10);
            this.tableLayoutPanelPort.Controls.Add(this.tableLayoutPanel4, 2, 4);
            this.tableLayoutPanelPort.Controls.Add(this.numericUpDownClkWidthDCSG, 3, 2);
            this.tableLayoutPanelPort.Controls.Add(this.label9, 3, 0);
            this.tableLayoutPanelPort.Controls.Add(this.comboBoxPortYm2413, 4, 3);
            this.tableLayoutPanelPort.Controls.Add(this.label4, 1, 3);
            this.tableLayoutPanelPort.Controls.Add(this.label3, 1, 2);
            this.tableLayoutPanelPort.Controls.Add(this.label2, 2, 0);
            this.tableLayoutPanelPort.Controls.Add(this.label1, 1, 0);
            this.tableLayoutPanelPort.Controls.Add(this.label6, 4, 0);
            this.tableLayoutPanelPort.Controls.Add(this.comboBoxDCSG, 2, 2);
            this.tableLayoutPanelPort.Controls.Add(this.comboBoxPortSN76489, 4, 2);
            this.tableLayoutPanelPort.Controls.Add(this.comboBoxPortSCC, 4, 4);
            this.tableLayoutPanelPort.Controls.Add(this.numericUpDownOPLL, 3, 3);
            this.tableLayoutPanelPort.Controls.Add(this.numericUpDownSCC, 3, 4);
            this.tableLayoutPanelPort.Controls.Add(this.label12, 1, 5);
            this.tableLayoutPanelPort.Controls.Add(this.comboBoxY8910, 2, 5);
            this.tableLayoutPanelPort.Controls.Add(this.numericUpDownY8910, 3, 5);
            this.tableLayoutPanelPort.Controls.Add(this.comboBoxPortY8910, 4, 5);
            this.tableLayoutPanelPort.Controls.Add(this.label8, 0, 0);
            this.tableLayoutPanelPort.Controls.Add(this.checkBoxConnDCSG, 0, 2);
            this.tableLayoutPanelPort.Controls.Add(this.checkBoxConnOPLL, 0, 3);
            this.tableLayoutPanelPort.Controls.Add(this.checkBoxConnSCC, 0, 4);
            this.tableLayoutPanelPort.Controls.Add(this.checkBoxConnY8910, 0, 5);
            this.tableLayoutPanelPort.Controls.Add(this.checkBoxConnOPNA2, 0, 1);
            this.tableLayoutPanelPort.Controls.Add(this.label5, 1, 1);
            this.tableLayoutPanelPort.Controls.Add(this.comboBoxOPNA2, 2, 1);
            this.tableLayoutPanelPort.Controls.Add(this.numericUpDownOPNA2, 3, 1);
            this.tableLayoutPanelPort.Controls.Add(this.comboBoxPortYM2612, 4, 1);
            this.tableLayoutPanelPort.Controls.Add(this.comboBoxSccType, 1, 4);
            this.tableLayoutPanelPort.Controls.Add(this.checkBoxConnOPM, 0, 6);
            this.tableLayoutPanelPort.Controls.Add(this.label11, 1, 6);
            this.tableLayoutPanelPort.Controls.Add(this.numericUpDownOPM, 3, 6);
            this.tableLayoutPanelPort.Controls.Add(this.comboBoxPortOPM, 4, 6);
            this.tableLayoutPanelPort.Controls.Add(this.tableLayoutPanel5, 2, 6);
            this.tableLayoutPanelPort.Controls.Add(this.tableLayoutPanel6, 2, 3);
            this.tableLayoutPanelPort.Controls.Add(this.checkBoxConnOPL3, 0, 7);
            this.tableLayoutPanelPort.Controls.Add(this.label14, 1, 7);
            this.tableLayoutPanelPort.Controls.Add(this.comboBoxOPL3, 2, 7);
            this.tableLayoutPanelPort.Controls.Add(this.numericUpDownOPL3, 3, 7);
            this.tableLayoutPanelPort.Controls.Add(this.comboBoxPortOPL3, 4, 7);
            this.tableLayoutPanelPort.Controls.Add(this.checkBoxConnOPNA, 0, 8);
            this.tableLayoutPanelPort.Controls.Add(this.label15, 1, 8);
            this.tableLayoutPanelPort.Controls.Add(this.comboBoxOPNA, 2, 8);
            this.tableLayoutPanelPort.Controls.Add(this.numericUpDownOPNA, 3, 8);
            this.tableLayoutPanelPort.Controls.Add(this.comboBoxPortOPNA, 4, 8);
            this.tableLayoutPanelPort.Controls.Add(this.checkBoxConnY8950, 0, 9);
            this.tableLayoutPanelPort.Controls.Add(this.label16, 1, 9);
            this.tableLayoutPanelPort.Controls.Add(this.numericUpDownY8950, 3, 9);
            this.tableLayoutPanelPort.Controls.Add(this.comboBoxPortY8950, 4, 9);
            this.tableLayoutPanelPort.Controls.Add(this.tableLayoutPanel7, 2, 9);
            this.tableLayoutPanelPort.Controls.Add(this.checkBoxCnvClk, 1, 10);
            this.tableLayoutPanelPort.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanelPort.Location = new System.Drawing.Point(0, 24);
            this.tableLayoutPanelPort.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanelPort.Name = "tableLayoutPanelPort";
            this.tableLayoutPanelPort.Padding = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanelPort.RowCount = 12;
            this.tableLayoutPanelPort.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelPort.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelPort.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelPort.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelPort.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelPort.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelPort.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelPort.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelPort.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelPort.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelPort.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelPort.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelPort.Size = new System.Drawing.Size(787, 276);
            this.tableLayoutPanelPort.TabIndex = 1;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.AutoSize = true;
            this.tableLayoutPanel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel3.ColumnCount = 5;
            this.tableLayoutPanelPort.SetColumnSpan(this.tableLayoutPanel3, 3);
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.Controls.Add(this.label10, 2, 0);
            this.tableLayoutPanel3.Controls.Add(this.numericUpDown3, 3, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(190, 310);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.Size = new System.Drawing.Size(572, 31);
            this.tableLayoutPanel3.TabIndex = 50;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label10.Location = new System.Drawing.Point(4, 0);
            this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(115, 31);
            this.label10.TabIndex = 1;
            this.label10.Text = "Cmd Wait [Samples]:";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // numericUpDown3
            // 
            settings1.BitBangWaitAY8910 = new decimal(new int[] {
            21,
            0,
            0,
            0});
            settings1.BitBangWaitDCSG = new decimal(new int[] {
            8,
            0,
            0,
            0});
            settings1.BitBangWaitOPL3 = new decimal(new int[] {
            21,
            0,
            0,
            0});
            settings1.BitBangWaitOPLL = new decimal(new int[] {
            21,
            0,
            0,
            0});
            settings1.BitBangWaitOPM = new decimal(new int[] {
            21,
            0,
            0,
            0});
            settings1.BitBangWaitOPNA = new decimal(new int[] {
            21,
            0,
            0,
            0});
            settings1.BitBangWaitOPNA2 = new decimal(new int[] {
            8,
            0,
            0,
            0});
            settings1.BitBangWaitSCC = new decimal(new int[] {
            21,
            0,
            0,
            0});
            settings1.BitBangWaitY8950 = new decimal(new int[] {
            21,
            0,
            0,
            0});
            settings1.ConvertClock = false;
            settings1.DCSG_Enable = false;
            settings1.DCSG_IF = 0;
            settings1.DCSG_Port = 0;
            settings1.Files = null;
            settings1.FocusedItem = -1;
            settings1.KSS_Max = new decimal(new int[] {
            300,
            0,
            0,
            0});
            settings1.LastDir = "";
            settings1.Loop = true;
            settings1.LoopCount = new decimal(new int[] {
            1,
            0,
            0,
            0});
            settings1.LoopTime = false;
            settings1.LoopTimes = new System.DateTime(1753, 1, 1, 0, 5, 0, 0);
            settings1.OPL3_Enable = false;
            settings1.OPL3_IF = 0;
            settings1.OPL3_Port = 0;
            settings1.OPL3_Slot = 0;
            settings1.OPLL_Enable = false;
            settings1.OPLL_IF = 0;
            settings1.OPLL_Port = 0;
            settings1.OPLL_Slot = 0;
            settings1.OPM_Enable = false;
            settings1.OPM_IF = 0;
            settings1.OPM_Port = 0;
            settings1.OPM_Slot = 0;
            settings1.OPNA_Enable = false;
            settings1.OPNA_IF = 0;
            settings1.OPNA_Port = 0;
            settings1.OPNA_Slot = 0;
            settings1.OPNA2_Enable = false;
            settings1.OPNA2_IF = 0;
            settings1.OPNA2_Port = 0;
            settings1.SCC_Enable = false;
            settings1.SCC_IF = 0;
            settings1.SCC_Port = 0;
            settings1.SCC_Slot = 0;
            settings1.SCC_Type = 0;
            settings1.SettingsKey = "";
            settings1.VGMWait = new decimal(new int[] {
            0,
            0,
            0,
            0});
            settings1.Y8910_Enable = false;
            settings1.Y8910_IF = 0;
            settings1.Y8910_Port = 0;
            settings1.Y8950_Enable = false;
            settings1.Y8950_IF = 0;
            settings1.Y8950_Port = 0;
            settings1.Y8950_Slot = 0;
            settings1.Y8950_Type = 0;
            this.numericUpDown3.DataBindings.Add(new System.Windows.Forms.Binding("Value", settings1, "VGMWait", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.numericUpDown3.Location = new System.Drawing.Point(127, 4);
            this.numericUpDown3.Margin = new System.Windows.Forms.Padding(4);
            this.numericUpDown3.Name = "numericUpDown3";
            this.numericUpDown3.Size = new System.Drawing.Size(140, 23);
            this.numericUpDown3.TabIndex = 2;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.AutoSize = true;
            this.tableLayoutPanel4.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel4.ColumnCount = 2;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 61.45552F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 38.54448F));
            this.tableLayoutPanel4.Controls.Add(this.comboBoxSCC, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.comboBoxSccSlot, 1, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(186, 120);
            this.tableLayoutPanel4.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 1;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel4.Size = new System.Drawing.Size(317, 31);
            this.tableLayoutPanel4.TabIndex = 22;
            // 
            // comboBoxSCC
            // 
            this.comboBoxSCC.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", settings1, "SCC_IF", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.comboBoxSCC.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxSCC.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSCC.FormattingEnabled = true;
            this.comboBoxSCC.Items.AddRange(new object[] {
            "VSIF - MSX(FTDI2XX)"});
            this.comboBoxSCC.Location = new System.Drawing.Point(4, 4);
            this.comboBoxSCC.Margin = new System.Windows.Forms.Padding(4);
            this.comboBoxSCC.Name = "comboBoxSCC";
            this.comboBoxSCC.Size = new System.Drawing.Size(186, 23);
            this.comboBoxSCC.TabIndex = 0;
            // 
            // comboBoxSccSlot
            // 
            this.comboBoxSccSlot.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", settings1, "SCC_Slot", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.comboBoxSccSlot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxSccSlot.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSccSlot.FormattingEnabled = true;
            this.comboBoxSccSlot.Items.AddRange(new object[] {
            "ID1",
            "ID0",
            "Slot0_Basic",
            "Slot0_Ext0",
            "Slot0_Ext1",
            "Slot0_Ext2",
            "Slot0_Ext3",
            "Slot1_Basic",
            "Slot1_Ext0",
            "Slot1_Ext1",
            "Slot1_Ext2",
            "Slot1_Ext3",
            "Slot2_Basic",
            "Slot2_Ext0",
            "Slot2_Ext1",
            "Slot2_Ext2",
            "Slot2_Ext3",
            "Slot3_Basic",
            "Slot3_Ext0",
            "Slot3_Ext1",
            "Slot3_Ext2",
            "Slot3_Ext3"});
            this.comboBoxSccSlot.Location = new System.Drawing.Point(198, 4);
            this.comboBoxSccSlot.Margin = new System.Windows.Forms.Padding(4);
            this.comboBoxSccSlot.Name = "comboBoxSccSlot";
            this.comboBoxSccSlot.Size = new System.Drawing.Size(115, 23);
            this.comboBoxSccSlot.TabIndex = 1;
            // 
            // numericUpDownClkWidthDCSG
            // 
            this.numericUpDownClkWidthDCSG.DataBindings.Add(new System.Windows.Forms.Binding("Value", settings1, "BitBangWaitDCSG", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.numericUpDownClkWidthDCSG.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numericUpDownClkWidthDCSG.Location = new System.Drawing.Point(507, 62);
            this.numericUpDownClkWidthDCSG.Margin = new System.Windows.Forms.Padding(4);
            this.numericUpDownClkWidthDCSG.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownClkWidthDCSG.Name = "numericUpDownClkWidthDCSG";
            this.numericUpDownClkWidthDCSG.Size = new System.Drawing.Size(108, 23);
            this.numericUpDownClkWidthDCSG.TabIndex = 13;
            this.numericUpDownClkWidthDCSG.Value = new decimal(new int[] {
            8,
            0,
            0,
            0});
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label9.Location = new System.Drawing.Point(507, 4);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(108, 23);
            this.label9.TabIndex = 3;
            this.label9.Text = "FTDI CLK [%]:";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // comboBoxPortYm2413
            // 
            this.comboBoxPortYm2413.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", settings1, "OPLL_Port", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
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
            this.comboBoxPortYm2413.Location = new System.Drawing.Point(623, 93);
            this.comboBoxPortYm2413.Margin = new System.Windows.Forms.Padding(4);
            this.comboBoxPortYm2413.Name = "comboBoxPortYm2413";
            this.comboBoxPortYm2413.Size = new System.Drawing.Size(139, 23);
            this.comboBoxPortYm2413.TabIndex = 19;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label6.Location = new System.Drawing.Point(623, 8);
            this.label6.Margin = new System.Windows.Forms.Padding(4);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(139, 15);
            this.label6.TabIndex = 4;
            this.label6.Text = "COM Port/FDTI ID";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // comboBoxDCSG
            // 
            this.comboBoxDCSG.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", settings1, "DCSG_IF", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.comboBoxDCSG.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxDCSG.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDCSG.FormattingEnabled = true;
            this.comboBoxDCSG.Items.AddRange(new object[] {
            "VSIF - Genesis(UART 163Kbps)",
            "VSIF - Genesis(FTDI2XX)",
            "VSIF - SMS",
            "VSIF - Genesis(UART 115Kbps)",
            "VSIF - MSX(FTDI2XX)"});
            this.comboBoxDCSG.Location = new System.Drawing.Point(190, 62);
            this.comboBoxDCSG.Margin = new System.Windows.Forms.Padding(4);
            this.comboBoxDCSG.Name = "comboBoxDCSG";
            this.comboBoxDCSG.Size = new System.Drawing.Size(309, 23);
            this.comboBoxDCSG.TabIndex = 12;
            // 
            // comboBoxPortSN76489
            // 
            this.comboBoxPortSN76489.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", settings1, "DCSG_Port", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
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
            this.comboBoxPortSN76489.Location = new System.Drawing.Point(623, 62);
            this.comboBoxPortSN76489.Margin = new System.Windows.Forms.Padding(4);
            this.comboBoxPortSN76489.Name = "comboBoxPortSN76489";
            this.comboBoxPortSN76489.Size = new System.Drawing.Size(139, 23);
            this.comboBoxPortSN76489.TabIndex = 14;
            // 
            // comboBoxPortSCC
            // 
            this.comboBoxPortSCC.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", settings1, "SCC_Port", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.comboBoxPortSCC.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxPortSCC.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxPortSCC.FormattingEnabled = true;
            this.comboBoxPortSCC.Items.AddRange(new object[] {
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
            this.comboBoxPortSCC.Location = new System.Drawing.Point(623, 124);
            this.comboBoxPortSCC.Margin = new System.Windows.Forms.Padding(4);
            this.comboBoxPortSCC.Name = "comboBoxPortSCC";
            this.comboBoxPortSCC.Size = new System.Drawing.Size(139, 23);
            this.comboBoxPortSCC.TabIndex = 24;
            // 
            // numericUpDownOPLL
            // 
            this.numericUpDownOPLL.DataBindings.Add(new System.Windows.Forms.Binding("Value", settings1, "BitBangWaitOPLL", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.numericUpDownOPLL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numericUpDownOPLL.Location = new System.Drawing.Point(507, 93);
            this.numericUpDownOPLL.Margin = new System.Windows.Forms.Padding(4);
            this.numericUpDownOPLL.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownOPLL.Name = "numericUpDownOPLL";
            this.numericUpDownOPLL.Size = new System.Drawing.Size(108, 23);
            this.numericUpDownOPLL.TabIndex = 18;
            this.numericUpDownOPLL.Value = new decimal(new int[] {
            21,
            0,
            0,
            0});
            // 
            // numericUpDownSCC
            // 
            this.numericUpDownSCC.DataBindings.Add(new System.Windows.Forms.Binding("Value", settings1, "BitBangWaitSCC", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.numericUpDownSCC.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numericUpDownSCC.Location = new System.Drawing.Point(507, 124);
            this.numericUpDownSCC.Margin = new System.Windows.Forms.Padding(4);
            this.numericUpDownSCC.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownSCC.Name = "numericUpDownSCC";
            this.numericUpDownSCC.Size = new System.Drawing.Size(108, 23);
            this.numericUpDownSCC.TabIndex = 23;
            this.numericUpDownSCC.Value = new decimal(new int[] {
            21,
            0,
            0,
            0});
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label12.Location = new System.Drawing.Point(51, 155);
            this.label12.Margin = new System.Windows.Forms.Padding(4);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(131, 23);
            this.label12.TabIndex = 26;
            this.label12.Text = "&AY-3-8910(PSG)";
            this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.toolTip1.SetToolTip(this.label12, "SSG of OPN2 fallbacks to this.\r\n");
            this.label12.Click += new System.EventHandler(this.label3_Click);
            this.label12.DoubleClick += new System.EventHandler(this.label3_DoubleClick);
            // 
            // comboBoxY8910
            // 
            this.comboBoxY8910.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", settings1, "Y8910_IF", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.comboBoxY8910.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxY8910.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxY8910.FormattingEnabled = true;
            this.comboBoxY8910.Items.AddRange(new object[] {
            "VSIF - MSX(FTDI2XX)",
            "VSIF - Generic(UART 115Kbps)"});
            this.comboBoxY8910.Location = new System.Drawing.Point(190, 155);
            this.comboBoxY8910.Margin = new System.Windows.Forms.Padding(4);
            this.comboBoxY8910.Name = "comboBoxY8910";
            this.comboBoxY8910.Size = new System.Drawing.Size(309, 23);
            this.comboBoxY8910.TabIndex = 27;
            // 
            // numericUpDownY8910
            // 
            this.numericUpDownY8910.DataBindings.Add(new System.Windows.Forms.Binding("Value", settings1, "BitBangWaitAY8910", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.numericUpDownY8910.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numericUpDownY8910.Location = new System.Drawing.Point(507, 155);
            this.numericUpDownY8910.Margin = new System.Windows.Forms.Padding(4);
            this.numericUpDownY8910.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownY8910.Name = "numericUpDownY8910";
            this.numericUpDownY8910.Size = new System.Drawing.Size(108, 23);
            this.numericUpDownY8910.TabIndex = 28;
            this.numericUpDownY8910.Value = new decimal(new int[] {
            21,
            0,
            0,
            0});
            // 
            // comboBoxPortY8910
            // 
            this.comboBoxPortY8910.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", settings1, "Y8910_Port", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.comboBoxPortY8910.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxPortY8910.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxPortY8910.FormattingEnabled = true;
            this.comboBoxPortY8910.Items.AddRange(new object[] {
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
            this.comboBoxPortY8910.Location = new System.Drawing.Point(623, 155);
            this.comboBoxPortY8910.Margin = new System.Windows.Forms.Padding(4);
            this.comboBoxPortY8910.Name = "comboBoxPortY8910";
            this.comboBoxPortY8910.Size = new System.Drawing.Size(139, 23);
            this.comboBoxPortY8910.TabIndex = 0;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label8.Location = new System.Drawing.Point(8, 8);
            this.label8.Margin = new System.Windows.Forms.Padding(4);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(35, 15);
            this.label8.TabIndex = 0;
            this.label8.Text = "Conn";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label8.Click += new System.EventHandler(this.label8_DoubleClick);
            this.label8.DoubleClick += new System.EventHandler(this.label8_DoubleClick);
            // 
            // checkBoxConnDCSG
            // 
            this.checkBoxConnDCSG.AutoSize = true;
            this.checkBoxConnDCSG.DataBindings.Add(new System.Windows.Forms.Binding("Checked", settings1, "DCSG_Enable", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.checkBoxConnDCSG.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxConnDCSG.Location = new System.Drawing.Point(8, 62);
            this.checkBoxConnDCSG.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxConnDCSG.Name = "checkBoxConnDCSG";
            this.checkBoxConnDCSG.Size = new System.Drawing.Size(35, 23);
            this.checkBoxConnDCSG.TabIndex = 10;
            this.checkBoxConnDCSG.UseVisualStyleBackColor = true;
            this.checkBoxConnDCSG.CheckedChanged += new System.EventHandler(this.checkBoxConnDCSG_CheckedChanged);
            // 
            // checkBoxConnOPLL
            // 
            this.checkBoxConnOPLL.AutoSize = true;
            this.checkBoxConnOPLL.DataBindings.Add(new System.Windows.Forms.Binding("Checked", settings1, "OPLL_Enable", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.checkBoxConnOPLL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxConnOPLL.Location = new System.Drawing.Point(8, 93);
            this.checkBoxConnOPLL.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxConnOPLL.Name = "checkBoxConnOPLL";
            this.checkBoxConnOPLL.Size = new System.Drawing.Size(35, 23);
            this.checkBoxConnOPLL.TabIndex = 15;
            this.checkBoxConnOPLL.UseVisualStyleBackColor = true;
            this.checkBoxConnOPLL.CheckedChanged += new System.EventHandler(this.checkBoxConnOPLL_CheckedChanged);
            // 
            // checkBoxConnSCC
            // 
            this.checkBoxConnSCC.AutoSize = true;
            this.checkBoxConnSCC.DataBindings.Add(new System.Windows.Forms.Binding("Checked", settings1, "SCC_Enable", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.checkBoxConnSCC.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxConnSCC.Location = new System.Drawing.Point(8, 124);
            this.checkBoxConnSCC.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxConnSCC.Name = "checkBoxConnSCC";
            this.checkBoxConnSCC.Size = new System.Drawing.Size(35, 23);
            this.checkBoxConnSCC.TabIndex = 20;
            this.checkBoxConnSCC.UseVisualStyleBackColor = true;
            this.checkBoxConnSCC.CheckedChanged += new System.EventHandler(this.checkBoxConnSCC_CheckedChanged);
            // 
            // checkBoxConnY8910
            // 
            this.checkBoxConnY8910.AutoSize = true;
            this.checkBoxConnY8910.DataBindings.Add(new System.Windows.Forms.Binding("Checked", settings1, "Y8910_Enable", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.checkBoxConnY8910.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxConnY8910.Location = new System.Drawing.Point(8, 155);
            this.checkBoxConnY8910.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxConnY8910.Name = "checkBoxConnY8910";
            this.checkBoxConnY8910.Size = new System.Drawing.Size(35, 23);
            this.checkBoxConnY8910.TabIndex = 25;
            this.checkBoxConnY8910.UseVisualStyleBackColor = true;
            this.checkBoxConnY8910.CheckedChanged += new System.EventHandler(this.checkBoxConnY8910_CheckedChanged);
            // 
            // checkBoxConnOPNA2
            // 
            this.checkBoxConnOPNA2.AutoSize = true;
            this.checkBoxConnOPNA2.DataBindings.Add(new System.Windows.Forms.Binding("Checked", settings1, "OPNA2_Enable", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.checkBoxConnOPNA2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxConnOPNA2.Location = new System.Drawing.Point(8, 31);
            this.checkBoxConnOPNA2.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxConnOPNA2.Name = "checkBoxConnOPNA2";
            this.checkBoxConnOPNA2.Size = new System.Drawing.Size(35, 23);
            this.checkBoxConnOPNA2.TabIndex = 5;
            this.checkBoxConnOPNA2.UseVisualStyleBackColor = true;
            this.checkBoxConnOPNA2.CheckedChanged += new System.EventHandler(this.checkBoxConnOPNA2_CheckedChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label5.Location = new System.Drawing.Point(51, 31);
            this.label5.Margin = new System.Windows.Forms.Padding(4);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(131, 23);
            this.label5.TabIndex = 6;
            this.label5.Text = "YM2&612(OPN2)";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.toolTip1.SetToolTip(this.label5, "OPN fallbacks to this.");
            this.label5.Click += new System.EventHandler(this.label3_Click);
            this.label5.DoubleClick += new System.EventHandler(this.label3_DoubleClick);
            // 
            // comboBoxOPNA2
            // 
            this.comboBoxOPNA2.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", settings1, "OPNA2_IF", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.comboBoxOPNA2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxOPNA2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxOPNA2.FormattingEnabled = true;
            this.comboBoxOPNA2.Items.AddRange(new object[] {
            "VSIF - Genesis(UART 163Kbps)",
            "VSIF - Genesis(FTDI2XX)",
            "VSIF - Genesis(UART 115Kbps)",
            "VSIF - MSX(FTDI2XX)"});
            this.comboBoxOPNA2.Location = new System.Drawing.Point(190, 31);
            this.comboBoxOPNA2.Margin = new System.Windows.Forms.Padding(4);
            this.comboBoxOPNA2.Name = "comboBoxOPNA2";
            this.comboBoxOPNA2.Size = new System.Drawing.Size(309, 23);
            this.comboBoxOPNA2.TabIndex = 7;
            // 
            // numericUpDownOPNA2
            // 
            this.numericUpDownOPNA2.DataBindings.Add(new System.Windows.Forms.Binding("Value", settings1, "BitBangWaitOPNA2", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.numericUpDownOPNA2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numericUpDownOPNA2.Location = new System.Drawing.Point(507, 31);
            this.numericUpDownOPNA2.Margin = new System.Windows.Forms.Padding(4);
            this.numericUpDownOPNA2.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownOPNA2.Name = "numericUpDownOPNA2";
            this.numericUpDownOPNA2.Size = new System.Drawing.Size(108, 23);
            this.numericUpDownOPNA2.TabIndex = 8;
            this.numericUpDownOPNA2.Value = new decimal(new int[] {
            8,
            0,
            0,
            0});
            // 
            // comboBoxPortYM2612
            // 
            this.comboBoxPortYM2612.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", settings1, "OPNA2_Port", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
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
            this.comboBoxPortYM2612.Location = new System.Drawing.Point(623, 31);
            this.comboBoxPortYM2612.Margin = new System.Windows.Forms.Padding(4);
            this.comboBoxPortYM2612.Name = "comboBoxPortYM2612";
            this.comboBoxPortYM2612.Size = new System.Drawing.Size(139, 23);
            this.comboBoxPortYM2612.TabIndex = 9;
            // 
            // comboBoxSccType
            // 
            this.comboBoxSccType.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", settings1, "SCC_Type", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.comboBoxSccType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSccType.FormattingEnabled = true;
            this.comboBoxSccType.Items.AddRange(new object[] {
            "SCC1",
            "SCC1(SCC mode)",
            "SCC"});
            this.comboBoxSccType.Location = new System.Drawing.Point(50, 123);
            this.comboBoxSccType.Name = "comboBoxSccType";
            this.comboBoxSccType.Size = new System.Drawing.Size(133, 23);
            this.comboBoxSccType.TabIndex = 21;
            // 
            // checkBoxConnOPM
            // 
            this.checkBoxConnOPM.AutoSize = true;
            this.checkBoxConnOPM.DataBindings.Add(new System.Windows.Forms.Binding("Checked", settings1, "OPM_Enable", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.checkBoxConnOPM.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxConnOPM.Location = new System.Drawing.Point(8, 186);
            this.checkBoxConnOPM.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxConnOPM.Name = "checkBoxConnOPM";
            this.checkBoxConnOPM.Size = new System.Drawing.Size(35, 23);
            this.checkBoxConnOPM.TabIndex = 30;
            this.checkBoxConnOPM.UseVisualStyleBackColor = true;
            this.checkBoxConnOPM.CheckedChanged += new System.EventHandler(this.checkBoxConnOPM_CheckedChanged);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label11.Location = new System.Drawing.Point(51, 186);
            this.label11.Margin = new System.Windows.Forms.Padding(4);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(131, 23);
            this.label11.TabIndex = 31;
            this.label11.Text = "YM21&51(OPM)";
            this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label11.Click += new System.EventHandler(this.label3_Click);
            this.label11.DoubleClick += new System.EventHandler(this.label3_DoubleClick);
            // 
            // numericUpDownOPM
            // 
            this.numericUpDownOPM.DataBindings.Add(new System.Windows.Forms.Binding("Value", settings1, "BitBangWaitOPM", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.numericUpDownOPM.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numericUpDownOPM.Location = new System.Drawing.Point(507, 186);
            this.numericUpDownOPM.Margin = new System.Windows.Forms.Padding(4);
            this.numericUpDownOPM.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownOPM.Name = "numericUpDownOPM";
            this.numericUpDownOPM.Size = new System.Drawing.Size(108, 23);
            this.numericUpDownOPM.TabIndex = 33;
            this.numericUpDownOPM.Value = new decimal(new int[] {
            21,
            0,
            0,
            0});
            // 
            // comboBoxPortOPM
            // 
            this.comboBoxPortOPM.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", settings1, "OPM_Port", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.comboBoxPortOPM.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxPortOPM.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxPortOPM.FormattingEnabled = true;
            this.comboBoxPortOPM.Items.AddRange(new object[] {
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
            this.comboBoxPortOPM.Location = new System.Drawing.Point(623, 186);
            this.comboBoxPortOPM.Margin = new System.Windows.Forms.Padding(4);
            this.comboBoxPortOPM.Name = "comboBoxPortOPM";
            this.comboBoxPortOPM.Size = new System.Drawing.Size(139, 23);
            this.comboBoxPortOPM.TabIndex = 34;
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.AutoSize = true;
            this.tableLayoutPanel5.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel5.ColumnCount = 2;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 61.71429F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 38.28571F));
            this.tableLayoutPanel5.Controls.Add(this.comboBoxOPM, 0, 0);
            this.tableLayoutPanel5.Controls.Add(this.comboBoxOpmSlot, 1, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(186, 182);
            this.tableLayoutPanel5.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 1;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(317, 31);
            this.tableLayoutPanel5.TabIndex = 32;
            // 
            // comboBoxOPM
            // 
            this.comboBoxOPM.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", settings1, "OPM_IF", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.comboBoxOPM.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxOPM.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxOPM.FormattingEnabled = true;
            this.comboBoxOPM.Items.AddRange(new object[] {
            "VSIF - MSX(FTDI2XX)"});
            this.comboBoxOPM.Location = new System.Drawing.Point(4, 4);
            this.comboBoxOPM.Margin = new System.Windows.Forms.Padding(4);
            this.comboBoxOPM.Name = "comboBoxOPM";
            this.comboBoxOPM.Size = new System.Drawing.Size(187, 23);
            this.comboBoxOPM.TabIndex = 0;
            // 
            // comboBoxOpmSlot
            // 
            this.comboBoxOpmSlot.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", settings1, "OPM_Slot", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.comboBoxOpmSlot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxOpmSlot.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxOpmSlot.FormattingEnabled = true;
            this.comboBoxOpmSlot.Items.AddRange(new object[] {
            "ID0",
            "ID1"});
            this.comboBoxOpmSlot.Location = new System.Drawing.Point(199, 4);
            this.comboBoxOpmSlot.Margin = new System.Windows.Forms.Padding(4);
            this.comboBoxOpmSlot.Name = "comboBoxOpmSlot";
            this.comboBoxOpmSlot.Size = new System.Drawing.Size(114, 23);
            this.comboBoxOpmSlot.TabIndex = 1;
            // 
            // tableLayoutPanel6
            // 
            this.tableLayoutPanel6.AutoSize = true;
            this.tableLayoutPanel6.ColumnCount = 2;
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 61.64773F));
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 38.35227F));
            this.tableLayoutPanel6.Controls.Add(this.comboBoxOPLL, 0, 0);
            this.tableLayoutPanel6.Controls.Add(this.comboBoxOpllSlot, 1, 0);
            this.tableLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel6.Location = new System.Drawing.Point(186, 89);
            this.tableLayoutPanel6.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.tableLayoutPanel6.RowCount = 1;
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel6.Size = new System.Drawing.Size(317, 31);
            this.tableLayoutPanel6.TabIndex = 17;
            // 
            // comboBoxOPLL
            // 
            this.comboBoxOPLL.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", settings1, "OPLL_IF", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.comboBoxOPLL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxOPLL.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxOPLL.FormattingEnabled = true;
            this.comboBoxOPLL.Items.AddRange(new object[] {
            "VSIF - SMS(UART 115Kbps)",
            "VSIF - MSX(FTDI2XX)"});
            this.comboBoxOPLL.Location = new System.Drawing.Point(4, 4);
            this.comboBoxOPLL.Margin = new System.Windows.Forms.Padding(4);
            this.comboBoxOPLL.Name = "comboBoxOPLL";
            this.comboBoxOPLL.Size = new System.Drawing.Size(187, 23);
            this.comboBoxOPLL.TabIndex = 0;
            // 
            // comboBoxOpllSlot
            // 
            this.comboBoxOpllSlot.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", settings1, "OPLL_Slot", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.comboBoxOpllSlot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxOpllSlot.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxOpllSlot.FormattingEnabled = true;
            this.comboBoxOpllSlot.Items.AddRange(new object[] {
            "IO",
            "MMIO_1",
            "MMIO_2"});
            this.comboBoxOpllSlot.Location = new System.Drawing.Point(199, 4);
            this.comboBoxOpllSlot.Margin = new System.Windows.Forms.Padding(4);
            this.comboBoxOpllSlot.Name = "comboBoxOpllSlot";
            this.comboBoxOpllSlot.Size = new System.Drawing.Size(114, 23);
            this.comboBoxOpllSlot.TabIndex = 1;
            // 
            // checkBoxConnOPL3
            // 
            this.checkBoxConnOPL3.AutoSize = true;
            this.checkBoxConnOPL3.DataBindings.Add(new System.Windows.Forms.Binding("Checked", settings1, "OPL3_Enable", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.checkBoxConnOPL3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxConnOPL3.Location = new System.Drawing.Point(8, 217);
            this.checkBoxConnOPL3.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxConnOPL3.Name = "checkBoxConnOPL3";
            this.checkBoxConnOPL3.Size = new System.Drawing.Size(35, 23);
            this.checkBoxConnOPL3.TabIndex = 35;
            this.checkBoxConnOPL3.UseVisualStyleBackColor = true;
            this.checkBoxConnOPL3.CheckedChanged += new System.EventHandler(this.checkBoxConnOPL3_CheckedChanged);
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label14.Location = new System.Drawing.Point(51, 217);
            this.label14.Margin = new System.Windows.Forms.Padding(4);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(131, 23);
            this.label14.TabIndex = 36;
            this.label14.Text = "YM&F262(OPL3)";
            this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.toolTip1.SetToolTip(this.label14, "OPL fallbacks to this.");
            this.label14.Click += new System.EventHandler(this.label3_Click);
            this.label14.DoubleClick += new System.EventHandler(this.label3_DoubleClick);
            // 
            // comboBoxOPL3
            // 
            this.comboBoxOPL3.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", settings1, "OPL3_IF", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.comboBoxOPL3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxOPL3.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxOPL3.FormattingEnabled = true;
            this.comboBoxOPL3.Items.AddRange(new object[] {
            "VSIF - MSX(FTDI2XX)"});
            this.comboBoxOPL3.Location = new System.Drawing.Point(190, 217);
            this.comboBoxOPL3.Margin = new System.Windows.Forms.Padding(4);
            this.comboBoxOPL3.Name = "comboBoxOPL3";
            this.comboBoxOPL3.Size = new System.Drawing.Size(309, 23);
            this.comboBoxOPL3.TabIndex = 37;
            // 
            // numericUpDownOPL3
            // 
            this.numericUpDownOPL3.DataBindings.Add(new System.Windows.Forms.Binding("Value", settings1, "BitBangWaitOPL3", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.numericUpDownOPL3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numericUpDownOPL3.Location = new System.Drawing.Point(507, 217);
            this.numericUpDownOPL3.Margin = new System.Windows.Forms.Padding(4);
            this.numericUpDownOPL3.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownOPL3.Name = "numericUpDownOPL3";
            this.numericUpDownOPL3.Size = new System.Drawing.Size(108, 23);
            this.numericUpDownOPL3.TabIndex = 38;
            this.numericUpDownOPL3.Value = new decimal(new int[] {
            21,
            0,
            0,
            0});
            // 
            // comboBoxPortOPL3
            // 
            this.comboBoxPortOPL3.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", settings1, "OPL3_Port", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.comboBoxPortOPL3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxPortOPL3.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxPortOPL3.FormattingEnabled = true;
            this.comboBoxPortOPL3.Items.AddRange(new object[] {
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
            this.comboBoxPortOPL3.Location = new System.Drawing.Point(623, 217);
            this.comboBoxPortOPL3.Margin = new System.Windows.Forms.Padding(4);
            this.comboBoxPortOPL3.Name = "comboBoxPortOPL3";
            this.comboBoxPortOPL3.Size = new System.Drawing.Size(139, 23);
            this.comboBoxPortOPL3.TabIndex = 39;
            // 
            // checkBoxConnOPNA
            // 
            this.checkBoxConnOPNA.AutoSize = true;
            this.checkBoxConnOPNA.DataBindings.Add(new System.Windows.Forms.Binding("Checked", settings1, "OPNA_Enable", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.checkBoxConnOPNA.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxConnOPNA.Location = new System.Drawing.Point(8, 248);
            this.checkBoxConnOPNA.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxConnOPNA.Name = "checkBoxConnOPNA";
            this.checkBoxConnOPNA.Size = new System.Drawing.Size(35, 23);
            this.checkBoxConnOPNA.TabIndex = 40;
            this.checkBoxConnOPNA.UseVisualStyleBackColor = true;
            this.checkBoxConnOPNA.CheckedChanged += new System.EventHandler(this.checkBoxOPNA_CheckedChanged);
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label15.Location = new System.Drawing.Point(51, 248);
            this.label15.Margin = new System.Windows.Forms.Padding(4);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(131, 23);
            this.label15.TabIndex = 41;
            this.label15.Text = "YM260&8(OPNA)";
            this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.toolTip1.SetToolTip(this.label15, "OPN fallbacks to this.");
            this.label15.Click += new System.EventHandler(this.label3_Click);
            this.label15.DoubleClick += new System.EventHandler(this.label3_DoubleClick);
            // 
            // comboBoxOPNA
            // 
            this.comboBoxOPNA.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", settings1, "OPNA_IF", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.comboBoxOPNA.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxOPNA.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxOPNA.FormattingEnabled = true;
            this.comboBoxOPNA.Items.AddRange(new object[] {
            "VSIF - MSX(FTDI2XX)"});
            this.comboBoxOPNA.Location = new System.Drawing.Point(190, 248);
            this.comboBoxOPNA.Margin = new System.Windows.Forms.Padding(4);
            this.comboBoxOPNA.Name = "comboBoxOPNA";
            this.comboBoxOPNA.Size = new System.Drawing.Size(309, 23);
            this.comboBoxOPNA.TabIndex = 42;
            // 
            // numericUpDownOPNA
            // 
            this.numericUpDownOPNA.DataBindings.Add(new System.Windows.Forms.Binding("Value", settings1, "BitBangWaitOPNA", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.numericUpDownOPNA.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numericUpDownOPNA.Location = new System.Drawing.Point(507, 248);
            this.numericUpDownOPNA.Margin = new System.Windows.Forms.Padding(4);
            this.numericUpDownOPNA.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownOPNA.Name = "numericUpDownOPNA";
            this.numericUpDownOPNA.Size = new System.Drawing.Size(108, 23);
            this.numericUpDownOPNA.TabIndex = 43;
            this.numericUpDownOPNA.Value = new decimal(new int[] {
            21,
            0,
            0,
            0});
            // 
            // comboBoxPortOPNA
            // 
            this.comboBoxPortOPNA.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", settings1, "OPNA_Port", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.comboBoxPortOPNA.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxPortOPNA.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxPortOPNA.FormattingEnabled = true;
            this.comboBoxPortOPNA.Items.AddRange(new object[] {
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
            this.comboBoxPortOPNA.Location = new System.Drawing.Point(623, 248);
            this.comboBoxPortOPNA.Margin = new System.Windows.Forms.Padding(4);
            this.comboBoxPortOPNA.Name = "comboBoxPortOPNA";
            this.comboBoxPortOPNA.Size = new System.Drawing.Size(139, 23);
            this.comboBoxPortOPNA.TabIndex = 44;
            // 
            // checkBoxConnY8950
            // 
            this.checkBoxConnY8950.AutoSize = true;
            this.checkBoxConnY8950.DataBindings.Add(new System.Windows.Forms.Binding("Checked", settings1, "Y8950_Enable", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.checkBoxConnY8950.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxConnY8950.Location = new System.Drawing.Point(8, 279);
            this.checkBoxConnY8950.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxConnY8950.Name = "checkBoxConnY8950";
            this.checkBoxConnY8950.Size = new System.Drawing.Size(35, 23);
            this.checkBoxConnY8950.TabIndex = 45;
            this.checkBoxConnY8950.UseVisualStyleBackColor = true;
            this.checkBoxConnY8950.CheckedChanged += new System.EventHandler(this.checkBoxConnY8950_CheckedChanged);
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label16.Location = new System.Drawing.Point(51, 279);
            this.label16.Margin = new System.Windows.Forms.Padding(4);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(131, 23);
            this.label16.TabIndex = 46;
            this.label16.Text = "&Y8950(MSX-AUDIO)";
            this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.toolTip1.SetToolTip(this.label16, "OPL fallbacks to this.");
            this.label16.Click += new System.EventHandler(this.label3_Click);
            this.label16.DoubleClick += new System.EventHandler(this.label3_DoubleClick);
            // 
            // numericUpDownY8950
            // 
            this.numericUpDownY8950.DataBindings.Add(new System.Windows.Forms.Binding("Value", settings1, "BitBangWaitY8950", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.numericUpDownY8950.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numericUpDownY8950.Location = new System.Drawing.Point(507, 279);
            this.numericUpDownY8950.Margin = new System.Windows.Forms.Padding(4);
            this.numericUpDownY8950.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownY8950.Name = "numericUpDownY8950";
            this.numericUpDownY8950.Size = new System.Drawing.Size(108, 23);
            this.numericUpDownY8950.TabIndex = 48;
            this.numericUpDownY8950.Value = new decimal(new int[] {
            21,
            0,
            0,
            0});
            // 
            // comboBoxPortY8950
            // 
            this.comboBoxPortY8950.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", settings1, "Y8950_Port", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.comboBoxPortY8950.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxPortY8950.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxPortY8950.FormattingEnabled = true;
            this.comboBoxPortY8950.Items.AddRange(new object[] {
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
            this.comboBoxPortY8950.Location = new System.Drawing.Point(623, 279);
            this.comboBoxPortY8950.Margin = new System.Windows.Forms.Padding(4);
            this.comboBoxPortY8950.Name = "comboBoxPortY8950";
            this.comboBoxPortY8950.Size = new System.Drawing.Size(139, 23);
            this.comboBoxPortY8950.TabIndex = 49;
            // 
            // tableLayoutPanel7
            // 
            this.tableLayoutPanel7.AutoSize = true;
            this.tableLayoutPanel7.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel7.ColumnCount = 2;
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 61.93182F));
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 38.06818F));
            this.tableLayoutPanel7.Controls.Add(this.comboBoxY8950, 0, 0);
            this.tableLayoutPanel7.Controls.Add(this.comboBoxY8950Slot, 1, 0);
            this.tableLayoutPanel7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel7.Location = new System.Drawing.Point(186, 275);
            this.tableLayoutPanel7.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel7.Name = "tableLayoutPanel7";
            this.tableLayoutPanel7.RowCount = 1;
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel7.Size = new System.Drawing.Size(317, 31);
            this.tableLayoutPanel7.TabIndex = 47;
            // 
            // comboBoxY8950
            // 
            this.comboBoxY8950.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", settings1, "Y8950_IF", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.comboBoxY8950.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxY8950.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxY8950.FormattingEnabled = true;
            this.comboBoxY8950.Items.AddRange(new object[] {
            "VSIF - MSX(FTDI2XX)"});
            this.comboBoxY8950.Location = new System.Drawing.Point(4, 4);
            this.comboBoxY8950.Margin = new System.Windows.Forms.Padding(4);
            this.comboBoxY8950.Name = "comboBoxY8950";
            this.comboBoxY8950.Size = new System.Drawing.Size(188, 23);
            this.comboBoxY8950.TabIndex = 0;
            // 
            // comboBoxY8950Slot
            // 
            this.comboBoxY8950Slot.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", settings1, "Y8950_Slot", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.comboBoxY8950Slot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxY8950Slot.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxY8950Slot.FormattingEnabled = true;
            this.comboBoxY8950Slot.Items.AddRange(new object[] {
            "ID0 256K RAM",
            "ID1 256K RAM"});
            this.comboBoxY8950Slot.Location = new System.Drawing.Point(200, 4);
            this.comboBoxY8950Slot.Margin = new System.Windows.Forms.Padding(4);
            this.comboBoxY8950Slot.Name = "comboBoxY8950Slot";
            this.comboBoxY8950Slot.Size = new System.Drawing.Size(113, 23);
            this.comboBoxY8950Slot.TabIndex = 1;
            // 
            // checkBoxCnvClk
            // 
            this.checkBoxCnvClk.AutoSize = true;
            this.checkBoxCnvClk.DataBindings.Add(new System.Windows.Forms.Binding("Checked", settings1, "ConvertClock", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.checkBoxCnvClk.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxCnvClk.Location = new System.Drawing.Point(50, 309);
            this.checkBoxCnvClk.Name = "checkBoxCnvClk";
            this.checkBoxCnvClk.Size = new System.Drawing.Size(133, 33);
            this.checkBoxCnvClk.TabIndex = 51;
            this.checkBoxCnvClk.Text = "Convert chip clk";
            this.checkBoxCnvClk.UseVisualStyleBackColor = true;
            this.checkBoxCnvClk.CheckedChanged += new System.EventHandler(this.checkBoxCnvClk_CheckedChanged);
            // 
            // tableLayoutPanelButton
            // 
            this.tableLayoutPanelButton.ColumnCount = 12;
            this.tableLayoutPanelButton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelButton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelButton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelButton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelButton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelButton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelButton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelButton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 69F));
            this.tableLayoutPanelButton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelButton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelButton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelButton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelButton.Controls.Add(this.buttonPrev, 0, 0);
            this.tableLayoutPanelButton.Controls.Add(this.checkBoxLoop, 7, 0);
            this.tableLayoutPanelButton.Controls.Add(this.buttonFast, 6, 0);
            this.tableLayoutPanelButton.Controls.Add(this.buttonSlow, 5, 0);
            this.tableLayoutPanelButton.Controls.Add(this.buttonFreeze, 4, 0);
            this.tableLayoutPanelButton.Controls.Add(this.buttonPlay, 1, 0);
            this.tableLayoutPanelButton.Controls.Add(this.labelSpeed, 5, 1);
            this.tableLayoutPanelButton.Controls.Add(this.numericUpDownLooped, 7, 1);
            this.tableLayoutPanelButton.Controls.Add(this.buttonStop, 3, 0);
            this.tableLayoutPanelButton.Controls.Add(this.buttonNext, 2, 0);
            this.tableLayoutPanelButton.Controls.Add(this.buttonClear, 11, 0);
            this.tableLayoutPanelButton.Controls.Add(this.buttonEject, 10, 0);
            this.tableLayoutPanelButton.Controls.Add(this.checkBoxLoopTimes, 8, 0);
            this.tableLayoutPanelButton.Controls.Add(this.dateTimePickerLoopTimes, 8, 1);
            this.tableLayoutPanelButton.Controls.Add(this.labelElapsed, 2, 1);
            this.tableLayoutPanelButton.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanelButton.Location = new System.Drawing.Point(0, 691);
            this.tableLayoutPanelButton.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanelButton.Name = "tableLayoutPanelButton";
            this.tableLayoutPanelButton.RowCount = 3;
            this.tableLayoutPanelButton.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanelButton.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanelButton.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelButton.Size = new System.Drawing.Size(787, 76);
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
            this.buttonPrev.Location = new System.Drawing.Point(4, 4);
            this.buttonPrev.Margin = new System.Windows.Forms.Padding(4);
            this.buttonPrev.Name = "buttonPrev";
            this.buttonPrev.Size = new System.Drawing.Size(38, 42);
            this.buttonPrev.TabIndex = 0;
            this.buttonPrev.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.toolTip1.SetToolTip(this.buttonPrev, "Previous Track");
            this.buttonPrev.UseVisualStyleBackColor = true;
            this.buttonPrev.Click += new System.EventHandler(this.buttonPrev_Click);
            // 
            // imageListSmall
            // 
            this.imageListSmall.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
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
            this.imageListSmall.Images.SetKeyName(9, "Loop_Time.png");
            // 
            // checkBoxLoop
            // 
            this.checkBoxLoop.Checked = true;
            this.checkBoxLoop.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxLoop.DataBindings.Add(new System.Windows.Forms.Binding("Checked", settings1, "Loop", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.checkBoxLoop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxLoop.FlatAppearance.BorderSize = 0;
            this.checkBoxLoop.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.checkBoxLoop.ImageIndex = 6;
            this.checkBoxLoop.ImageList = this.imageListSmall;
            this.checkBoxLoop.Location = new System.Drawing.Point(396, 4);
            this.checkBoxLoop.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxLoop.Name = "checkBoxLoop";
            this.checkBoxLoop.Size = new System.Drawing.Size(61, 42);
            this.checkBoxLoop.TabIndex = 8;
            this.checkBoxLoop.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.toolTip1.SetToolTip(this.checkBoxLoop, "Loop Count");
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
            this.buttonFast.Location = new System.Drawing.Point(350, 4);
            this.buttonFast.Margin = new System.Windows.Forms.Padding(4);
            this.buttonFast.Name = "buttonFast";
            this.buttonFast.Size = new System.Drawing.Size(38, 42);
            this.buttonFast.TabIndex = 7;
            this.buttonFast.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.toolTip1.SetToolTip(this.buttonFast, "Fast");
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
            this.buttonSlow.Location = new System.Drawing.Point(304, 4);
            this.buttonSlow.Margin = new System.Windows.Forms.Padding(4);
            this.buttonSlow.Name = "buttonSlow";
            this.buttonSlow.Size = new System.Drawing.Size(38, 42);
            this.buttonSlow.TabIndex = 5;
            this.buttonSlow.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.toolTip1.SetToolTip(this.buttonSlow, "Slow");
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
            this.buttonFreeze.Location = new System.Drawing.Point(258, 4);
            this.buttonFreeze.Margin = new System.Windows.Forms.Padding(4);
            this.buttonFreeze.Name = "buttonFreeze";
            this.buttonFreeze.Size = new System.Drawing.Size(38, 42);
            this.buttonFreeze.TabIndex = 4;
            this.buttonFreeze.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.toolTip1.SetToolTip(this.buttonFreeze, "Freeze");
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
            this.buttonPlay.Location = new System.Drawing.Point(50, 4);
            this.buttonPlay.Margin = new System.Windows.Forms.Padding(4);
            this.buttonPlay.Name = "buttonPlay";
            this.tableLayoutPanelButton.SetRowSpan(this.buttonPlay, 2);
            this.buttonPlay.Size = new System.Drawing.Size(108, 68);
            this.buttonPlay.TabIndex = 1;
            this.buttonPlay.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.toolTip1.SetToolTip(this.buttonPlay, "Play/Pause");
            this.buttonPlay.UseVisualStyleBackColor = true;
            this.buttonPlay.Click += new System.EventHandler(this.buttonPlay_Click);
            this.buttonPlay.DragDrop += new System.Windows.Forms.DragEventHandler(this.buttonPlay_DragDrop);
            this.buttonPlay.DragEnter += new System.Windows.Forms.DragEventHandler(this.buttonPlay_DragEnter);
            // 
            // imageListBig
            // 
            this.imageListBig.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.imageListBig.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListBig.ImageStream")));
            this.imageListBig.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListBig.Images.SetKeyName(0, "Play.png");
            this.imageListBig.Images.SetKeyName(1, "Pause.png");
            // 
            // labelSpeed
            // 
            this.labelSpeed.AutoSize = true;
            this.labelSpeed.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.tableLayoutPanelButton.SetColumnSpan(this.labelSpeed, 2);
            this.labelSpeed.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelSpeed.Location = new System.Drawing.Point(304, 50);
            this.labelSpeed.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelSpeed.Name = "labelSpeed";
            this.labelSpeed.Size = new System.Drawing.Size(84, 26);
            this.labelSpeed.TabIndex = 6;
            this.labelSpeed.Text = "1.00x";
            this.labelSpeed.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // numericUpDownLooped
            // 
            this.numericUpDownLooped.AutoSize = true;
            this.numericUpDownLooped.DataBindings.Add(new System.Windows.Forms.Binding("Value", settings1, "LoopCount", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.numericUpDownLooped.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numericUpDownLooped.Location = new System.Drawing.Point(396, 54);
            this.numericUpDownLooped.Margin = new System.Windows.Forms.Padding(4);
            this.numericUpDownLooped.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.numericUpDownLooped.Name = "numericUpDownLooped";
            this.numericUpDownLooped.Size = new System.Drawing.Size(61, 23);
            this.numericUpDownLooped.TabIndex = 9;
            this.numericUpDownLooped.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownLooped.ValueChanged += new System.EventHandler(this.numericUpDownLooped_ValueChanged);
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
            this.buttonStop.Location = new System.Drawing.Point(212, 4);
            this.buttonStop.Margin = new System.Windows.Forms.Padding(4);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(38, 42);
            this.buttonStop.TabIndex = 3;
            this.buttonStop.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.toolTip1.SetToolTip(this.buttonStop, "Stop");
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
            this.buttonNext.Location = new System.Drawing.Point(166, 4);
            this.buttonNext.Margin = new System.Windows.Forms.Padding(4);
            this.buttonNext.Name = "buttonNext";
            this.buttonNext.Size = new System.Drawing.Size(38, 42);
            this.buttonNext.TabIndex = 2;
            this.buttonNext.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.toolTip1.SetToolTip(this.buttonNext, "Next Track");
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
            this.buttonClear.Location = new System.Drawing.Point(745, 4);
            this.buttonClear.Margin = new System.Windows.Forms.Padding(4);
            this.buttonClear.Name = "buttonClear";
            this.buttonClear.Size = new System.Drawing.Size(38, 42);
            this.buttonClear.TabIndex = 13;
            this.buttonClear.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.toolTip1.SetToolTip(this.buttonClear, "Clear List");
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
            this.buttonEject.Location = new System.Drawing.Point(699, 4);
            this.buttonEject.Margin = new System.Windows.Forms.Padding(4);
            this.buttonEject.Name = "buttonEject";
            this.buttonEject.Size = new System.Drawing.Size(38, 42);
            this.buttonEject.TabIndex = 12;
            this.buttonEject.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.toolTip1.SetToolTip(this.buttonEject, "Load Musics");
            this.buttonEject.UseVisualStyleBackColor = true;
            this.buttonEject.Click += new System.EventHandler(this.buttonEject_Click);
            // 
            // checkBoxLoopTimes
            // 
            this.checkBoxLoopTimes.AutoSize = true;
            this.checkBoxLoopTimes.DataBindings.Add(new System.Windows.Forms.Binding("Checked", settings1, "LoopTime", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.checkBoxLoopTimes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxLoopTimes.FlatAppearance.BorderSize = 0;
            this.checkBoxLoopTimes.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.checkBoxLoopTimes.ImageIndex = 9;
            this.checkBoxLoopTimes.ImageList = this.imageListSmall;
            this.checkBoxLoopTimes.Location = new System.Drawing.Point(465, 4);
            this.checkBoxLoopTimes.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxLoopTimes.Name = "checkBoxLoopTimes";
            this.checkBoxLoopTimes.Size = new System.Drawing.Size(82, 42);
            this.checkBoxLoopTimes.TabIndex = 10;
            this.checkBoxLoopTimes.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.toolTip1.SetToolTip(this.checkBoxLoopTimes, "Loop Time");
            this.checkBoxLoopTimes.UseVisualStyleBackColor = true;
            this.checkBoxLoopTimes.CheckedChanged += new System.EventHandler(this.checkBoxLoopTimes_CheckedChanged);
            // 
            // dateTimePickerLoopTimes
            // 
            this.dateTimePickerLoopTimes.CustomFormat = "hh:mm:ss";
            this.dateTimePickerLoopTimes.DataBindings.Add(new System.Windows.Forms.Binding("Value", settings1, "LoopTimes", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.dateTimePickerLoopTimes.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.dateTimePickerLoopTimes.Location = new System.Drawing.Point(464, 53);
            this.dateTimePickerLoopTimes.MaxDate = new System.DateTime(1753, 1, 1, 23, 59, 59, 0);
            this.dateTimePickerLoopTimes.Name = "dateTimePickerLoopTimes";
            this.dateTimePickerLoopTimes.ShowUpDown = true;
            this.dateTimePickerLoopTimes.Size = new System.Drawing.Size(84, 23);
            this.dateTimePickerLoopTimes.TabIndex = 11;
            this.dateTimePickerLoopTimes.Value = new System.DateTime(1753, 1, 1, 0, 5, 0, 0);
            this.dateTimePickerLoopTimes.ValueChanged += new System.EventHandler(this.dateTimePickerLoopTimes_ValueChanged);
            // 
            // labelElapsed
            // 
            this.labelElapsed.AutoSize = true;
            this.labelElapsed.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.tableLayoutPanelButton.SetColumnSpan(this.labelElapsed, 3);
            this.labelElapsed.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelElapsed.Location = new System.Drawing.Point(165, 50);
            this.labelElapsed.Name = "labelElapsed";
            this.labelElapsed.Size = new System.Drawing.Size(132, 26);
            this.labelElapsed.TabIndex = 14;
            this.labelElapsed.Text = "00:00:00";
            this.labelElapsed.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // contextMenuStripList
            // 
            this.contextMenuStripList.ImageScalingSize = new System.Drawing.Size(20, 20);
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
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 767);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(787, 24);
            this.tableLayoutPanel1.TabIndex = 5;
            // 
            // labelLoad
            // 
            this.labelLoad.AutoSize = true;
            this.labelLoad.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelLoad.Location = new System.Drawing.Point(4, 0);
            this.labelLoad.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelLoad.Name = "labelLoad";
            this.labelLoad.Size = new System.Drawing.Size(33, 24);
            this.labelLoad.TabIndex = 0;
            this.labelLoad.Text = "Load";
            this.labelLoad.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // progressBarLoad
            // 
            this.progressBarLoad.Dock = System.Windows.Forms.DockStyle.Fill;
            this.progressBarLoad.Location = new System.Drawing.Point(45, 4);
            this.progressBarLoad.Margin = new System.Windows.Forms.Padding(4);
            this.progressBarLoad.Name = "progressBarLoad";
            this.progressBarLoad.Size = new System.Drawing.Size(738, 16);
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
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 659);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(787, 32);
            this.tableLayoutPanel2.TabIndex = 3;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label7.Location = new System.Drawing.Point(4, 0);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(46, 32);
            this.label7.TabIndex = 0;
            this.label7.Text = "&Current";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBoxTitle
            // 
            this.textBoxTitle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxTitle.Location = new System.Drawing.Point(58, 4);
            this.textBoxTitle.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxTitle.Name = "textBoxTitle";
            this.textBoxTitle.ReadOnly = true;
            this.textBoxTitle.ShortcutsEnabled = false;
            this.textBoxTitle.Size = new System.Drawing.Size(725, 23);
            this.textBoxTitle.TabIndex = 1;
            // 
            // openFileDialog
            // 
            this.openFileDialog.Filter = "Video Game Music Files(*.vgm;*.vgz;*.xgm;*.mgs;*.kss)|*.vgm;*.vgz;*.xgm;*.mgs;*.k" +
    "ss";
            this.openFileDialog.Multiselect = true;
            this.openFileDialog.Title = "Select VGM/XGM files";
            // 
            // splitter1
            // 
            this.splitter1.BackColor = System.Drawing.SystemColors.ControlDark;
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Top;
            this.splitter1.Location = new System.Drawing.Point(0, 300);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(787, 8);
            this.splitter1.TabIndex = 6;
            this.splitter1.TabStop = false;
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 791);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 12, 0);
            this.statusStrip1.Size = new System.Drawing.Size(787, 22);
            this.statusStrip1.TabIndex = 7;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(71, 17);
            this.toolStripStatusLabel.Text = "(Status Text)";
            this.toolStripStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
            this.listViewList.LabelWrap = false;
            this.listViewList.Location = new System.Drawing.Point(0, 308);
            this.listViewList.Margin = new System.Windows.Forms.Padding(4);
            this.listViewList.Name = "listViewList";
            this.listViewList.Size = new System.Drawing.Size(787, 351);
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
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(787, 813);
            this.Controls.Add(this.listViewList);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.tableLayoutPanel2);
            this.Controls.Add(this.tableLayoutPanelButton);
            this.Controls.Add(this.tableLayoutPanelPort);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.statusStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "FormMain";
            this.Text = "VSIF Checker (VGM/XGM/MGS Player) V1.32";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormMain_KeyDown);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.tableLayoutPanelPort.ResumeLayout(false);
            this.tableLayoutPanelPort.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown3)).EndInit();
            this.tableLayoutPanel4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownClkWidthDCSG)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownOPLL)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSCC)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownY8910)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownOPNA2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownOPM)).EndInit();
            this.tableLayoutPanel5.ResumeLayout(false);
            this.tableLayoutPanel6.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownOPL3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownOPNA)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownY8950)).EndInit();
            this.tableLayoutPanel7.ResumeLayout(false);
            this.tableLayoutPanelButton.ResumeLayout(false);
            this.tableLayoutPanelButton.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownLooped)).EndInit();
            this.contextMenuStripList.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
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
        private System.Windows.Forms.NumericUpDown numericUpDownLooped;
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
        private System.Windows.Forms.NumericUpDown numericUpDownClkWidthDCSG;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.NumericUpDown numericUpDown3;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripList;
        private System.Windows.Forms.ToolStripMenuItem playToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem explorerToolStripMenuItem;
        private System.Windows.Forms.Button buttonEject;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.ComboBox comboBoxSCC;
        private System.Windows.Forms.ComboBox comboBoxPortSCC;
        private System.Windows.Forms.CheckBox checkBoxConnSCC;
        private System.Windows.Forms.NumericUpDown numericUpDownOPLL;
        private System.Windows.Forms.NumericUpDown numericUpDownOPNA2;
        private System.Windows.Forms.NumericUpDown numericUpDownSCC;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.ComboBox comboBoxY8910;
        private System.Windows.Forms.NumericUpDown numericUpDownY8910;
        private System.Windows.Forms.ComboBox comboBoxPortY8910;
        private System.Windows.Forms.CheckBox checkBoxConnY8910;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.ComboBox comboBoxSccSlot;
        private System.Windows.Forms.ToolStripMenuItem aBOUTToolStripMenuItem;
        private System.Windows.Forms.ComboBox comboBoxSccType;
        private System.Windows.Forms.CheckBox checkBoxConnOPM;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.ComboBox comboBoxOPM;
        private System.Windows.Forms.NumericUpDown numericUpDownOPM;
        private System.Windows.Forms.ComboBox comboBoxPortOPM;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.ComboBox comboBoxOpmSlot;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel6;
        private System.Windows.Forms.ComboBox comboBoxOpllSlot;
        private System.Windows.Forms.CheckBox checkBoxConnOPL3;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.ComboBox comboBoxOPL3;
        private System.Windows.Forms.NumericUpDown numericUpDownOPL3;
        private System.Windows.Forms.ComboBox comboBoxPortOPL3;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.CheckBox checkBoxConnOPNA;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.ComboBox comboBoxOPNA;
        private System.Windows.Forms.NumericUpDown numericUpDownOPNA;
        private System.Windows.Forms.ComboBox comboBoxPortOPNA;
        private System.Windows.Forms.CheckBox checkBoxConnY8950;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.NumericUpDown numericUpDownY8950;
        private System.Windows.Forms.ComboBox comboBoxPortY8950;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel7;
        private System.Windows.Forms.ComboBox comboBoxY8950;
        private System.Windows.Forms.ComboBox comboBoxY8950Slot;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.CheckBox checkBoxLoopTimes;
        private System.Windows.Forms.DateTimePicker dateTimePickerLoopTimes;
        private System.Windows.Forms.Label labelElapsed;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.CheckBox checkBoxCnvClk;
    }
}