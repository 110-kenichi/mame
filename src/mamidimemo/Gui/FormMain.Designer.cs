namespace zanac.MAmidiMEmo.Gui
{
    partial class FormMain
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            if (disposing)
            {
                Instruments.InstrumentManager.InstrumentAdded -= InstrumentManager_InstrumentAdded;
                Instruments.InstrumentManager.InstrumentChanged += InstrumentManager_InstrumentChanged;
                Instruments.InstrumentManager.InstrumentRemoved += InstrumentManager_InstrumentRemoved;
            }

            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.listViewIntruments = new System.Windows.Forms.ListView();
            this.contextMenuStripInst = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.decreaseThisKindOfChipToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cloneSelectedChipToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemSep = new System.Windows.Forms.ToolStripSeparator();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.propertyGrid = new System.Windows.Forms.PropertyGrid();
            this.contextMenuStripProp = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.resetToDefaultThisPropertyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip3 = new zanac.MAmidiMEmo.ComponentModel.ToolStripBase();
            this.toolStripButtonCat = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonA2Z = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonPopup = new System.Windows.Forms.ToolStripButton();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.pianoControl1 = new zanac.MAmidiMEmo.Gui.PianoControl();
            this.toolStrip2 = new zanac.MAmidiMEmo.ComponentModel.ToolStripBase();
            this.toolStripLabel3 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripButton19 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton18 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton17 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton16 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton15 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton14 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton13 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton12 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton11 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton10 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton9 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton8 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton7 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton6 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton5 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton4 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton3 = new System.Windows.Forms.ToolStripButton();
            this.toolStripComboBox2 = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripLabel4 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripComboBox1 = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemExit = new System.Windows.Forms.ToolStripMenuItem();
            this.toolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.toolStrip1 = new zanac.MAmidiMEmo.ComponentModel.ToolStripBase();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripComboBoxMidiIf = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.lAToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mT32ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extendCM32PToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pCMToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extendC140ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extendSPC700ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fMSynthesisToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extendYM2610BToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addYM2151ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addYM2612ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extendYM3812ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extendYM2413ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wSGToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extendSCC1kToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addNAMCOCUS30ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extendHuC6230ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pSGToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extendMOS8580ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extendMOS6581ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extendNESAPUToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extendGBAPUToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extendPOKEYToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addSN76496ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extendAY38910ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extendMSM5232ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extendBeepToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton20 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton21 = new System.Windows.Forms.ToolStripButton();
            this.multiMediaTimerComponent1 = new zanac.MAmidiMEmo.ComponentModel.MultiMediaTimerComponent(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.contextMenuStripInst.SuspendLayout();
            this.contextMenuStripProp.SuspendLayout();
            this.toolStrip3.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.toolStrip2.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            resources.ApplyResources(this.splitContainer1, "splitContainer1");
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            resources.ApplyResources(this.splitContainer1.Panel1, "splitContainer1.Panel1");
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabControl1);
            resources.ApplyResources(this.splitContainer1.Panel2, "splitContainer1.Panel2");
            // 
            // splitContainer2
            // 
            resources.ApplyResources(this.splitContainer2, "splitContainer2");
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.listViewIntruments);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.propertyGrid);
            this.splitContainer2.Panel2.Controls.Add(this.toolStrip3);
            // 
            // listViewIntruments
            // 
            this.listViewIntruments.AllowDrop = true;
            this.listViewIntruments.ContextMenuStrip = this.contextMenuStripInst;
            resources.ApplyResources(this.listViewIntruments, "listViewIntruments");
            this.listViewIntruments.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            ((System.Windows.Forms.ListViewGroup)(resources.GetObject("listViewIntruments.Groups"))),
            ((System.Windows.Forms.ListViewGroup)(resources.GetObject("listViewIntruments.Groups1"))),
            ((System.Windows.Forms.ListViewGroup)(resources.GetObject("listViewIntruments.Groups2"))),
            ((System.Windows.Forms.ListViewGroup)(resources.GetObject("listViewIntruments.Groups3")))});
            this.listViewIntruments.HideSelection = false;
            this.listViewIntruments.LargeImageList = this.imageList1;
            this.listViewIntruments.Name = "listViewIntruments";
            this.listViewIntruments.ShowItemToolTips = true;
            this.listViewIntruments.SmallImageList = this.imageList1;
            this.listViewIntruments.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.listViewIntruments.UseCompatibleStateImageBehavior = false;
            this.listViewIntruments.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.listViewIntruments_ItemSelectionChanged);
            this.listViewIntruments.DragDrop += new System.Windows.Forms.DragEventHandler(this.listViewIntruments_DragDrop);
            this.listViewIntruments.DragEnter += new System.Windows.Forms.DragEventHandler(this.listViewIntruments_DragEnter);
            // 
            // contextMenuStripInst
            // 
            this.contextMenuStripInst.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.contextMenuStripInst.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.decreaseThisKindOfChipToolStripMenuItem,
            this.cloneSelectedChipToolStripMenuItem,
            this.toolStripMenuItemSep});
            this.contextMenuStripInst.Name = "contextMenuStrip1";
            resources.ApplyResources(this.contextMenuStripInst, "contextMenuStripInst");
            this.contextMenuStripInst.Closed += new System.Windows.Forms.ToolStripDropDownClosedEventHandler(this.contextMenuStripInst_Closed);
            this.contextMenuStripInst.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStripInst_Opening);
            // 
            // decreaseThisKindOfChipToolStripMenuItem
            // 
            this.decreaseThisKindOfChipToolStripMenuItem.Name = "decreaseThisKindOfChipToolStripMenuItem";
            resources.ApplyResources(this.decreaseThisKindOfChipToolStripMenuItem, "decreaseThisKindOfChipToolStripMenuItem");
            this.decreaseThisKindOfChipToolStripMenuItem.Click += new System.EventHandler(this.decreaseThisKindOfChipToolStripMenuItem_Click);
            // 
            // cloneSelectedChipToolStripMenuItem
            // 
            this.cloneSelectedChipToolStripMenuItem.Name = "cloneSelectedChipToolStripMenuItem";
            resources.ApplyResources(this.cloneSelectedChipToolStripMenuItem, "cloneSelectedChipToolStripMenuItem");
            this.cloneSelectedChipToolStripMenuItem.Click += new System.EventHandler(this.cloneSelectedChipToolStripMenuItem_Click);
            // 
            // toolStripMenuItemSep
            // 
            this.toolStripMenuItemSep.Name = "toolStripMenuItemSep";
            resources.ApplyResources(this.toolStripMenuItemSep, "toolStripMenuItemSep");
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            resources.ApplyResources(this.imageList1, "imageList1");
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // propertyGrid
            // 
            this.propertyGrid.ContextMenuStrip = this.contextMenuStripProp;
            resources.ApplyResources(this.propertyGrid, "propertyGrid");
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.ToolbarVisible = false;
            // 
            // contextMenuStripProp
            // 
            this.contextMenuStripProp.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.contextMenuStripProp.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.resetToDefaultThisPropertyToolStripMenuItem});
            this.contextMenuStripProp.Name = "contextMenuStripProp";
            resources.ApplyResources(this.contextMenuStripProp, "contextMenuStripProp");
            // 
            // resetToDefaultThisPropertyToolStripMenuItem
            // 
            this.resetToDefaultThisPropertyToolStripMenuItem.Name = "resetToDefaultThisPropertyToolStripMenuItem";
            resources.ApplyResources(this.resetToDefaultThisPropertyToolStripMenuItem, "resetToDefaultThisPropertyToolStripMenuItem");
            this.resetToDefaultThisPropertyToolStripMenuItem.Click += new System.EventHandler(this.resetToDefaultThisPropertyToolStripMenuItem_Click);
            // 
            // toolStrip3
            // 
            resources.ApplyResources(this.toolStrip3, "toolStrip3");
            this.toolStrip3.ClickThrough = false;
            this.toolStrip3.ImageScalingSize = new System.Drawing.Size(609, 609);
            this.toolStrip3.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonCat,
            this.toolStripButtonA2Z,
            this.toolStripButtonPopup});
            this.toolStrip3.Name = "toolStrip3";
            // 
            // toolStripButtonCat
            // 
            this.toolStripButtonCat.Checked = true;
            this.toolStripButtonCat.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripButtonCat.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonCat.Image = global::zanac.MAmidiMEmo.Properties.Resources.Cat;
            resources.ApplyResources(this.toolStripButtonCat, "toolStripButtonCat");
            this.toolStripButtonCat.Name = "toolStripButtonCat";
            this.toolStripButtonCat.Click += new System.EventHandler(this.toolStripButtonCat_Click);
            // 
            // toolStripButtonA2Z
            // 
            this.toolStripButtonA2Z.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonA2Z.Image = global::zanac.MAmidiMEmo.Properties.Resources.AtoZ;
            resources.ApplyResources(this.toolStripButtonA2Z, "toolStripButtonA2Z");
            this.toolStripButtonA2Z.Name = "toolStripButtonA2Z";
            this.toolStripButtonA2Z.Click += new System.EventHandler(this.toolStripButtonA2Z_Click);
            // 
            // toolStripButtonPopup
            // 
            this.toolStripButtonPopup.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButtonPopup.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonPopup.Image = global::zanac.MAmidiMEmo.Properties.Resources.Popup;
            resources.ApplyResources(this.toolStripButtonPopup, "toolStripButtonPopup");
            this.toolStripButtonPopup.Name = "toolStripButtonPopup";
            this.toolStripButtonPopup.Click += new System.EventHandler(this.toolStripButtonPopup_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage2);
            resources.ApplyResources(this.tabControl1, "tabControl1");
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.tabPage1, "tabPage1");
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Paint += new System.Windows.Forms.PaintEventHandler(this.tabPage1_Paint);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.pianoControl1);
            this.tabPage3.Controls.Add(this.toolStrip2);
            resources.ApplyResources(this.tabPage3, "tabPage3");
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // pianoControl1
            // 
            resources.ApplyResources(this.pianoControl1, "pianoControl1");
            this.pianoControl1.Name = "pianoControl1";
            // 
            // toolStrip2
            // 
            resources.ApplyResources(this.toolStrip2, "toolStrip2");
            this.toolStrip2.ClickThrough = false;
            this.toolStrip2.ImageScalingSize = new System.Drawing.Size(1369, 1369);
            this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel3,
            this.toolStripButton19,
            this.toolStripButton18,
            this.toolStripButton17,
            this.toolStripButton16,
            this.toolStripButton15,
            this.toolStripButton14,
            this.toolStripButton13,
            this.toolStripButton12,
            this.toolStripButton11,
            this.toolStripButton10,
            this.toolStripButton9,
            this.toolStripButton8,
            this.toolStripButton7,
            this.toolStripButton6,
            this.toolStripButton5,
            this.toolStripButton4,
            this.toolStripButton3,
            this.toolStripComboBox2,
            this.toolStripLabel4,
            this.toolStripComboBox1,
            this.toolStripLabel2});
            this.toolStrip2.Name = "toolStrip2";
            // 
            // toolStripLabel3
            // 
            this.toolStripLabel3.Name = "toolStripLabel3";
            resources.ApplyResources(this.toolStripLabel3, "toolStripLabel3");
            // 
            // toolStripButton19
            // 
            this.toolStripButton19.Checked = true;
            this.toolStripButton19.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripButton19.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            resources.ApplyResources(this.toolStripButton19, "toolStripButton19");
            this.toolStripButton19.Name = "toolStripButton19";
            this.toolStripButton19.Click += new System.EventHandler(this.toolStripButton19_Click);
            // 
            // toolStripButton18
            // 
            this.toolStripButton18.Checked = true;
            this.toolStripButton18.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripButton18.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            resources.ApplyResources(this.toolStripButton18, "toolStripButton18");
            this.toolStripButton18.Name = "toolStripButton18";
            this.toolStripButton18.CheckedChanged += new System.EventHandler(this.toolStripButton18_CheckedChanged);
            this.toolStripButton18.Click += new System.EventHandler(this.toolStripButton18_Click);
            // 
            // toolStripButton17
            // 
            this.toolStripButton17.Checked = true;
            this.toolStripButton17.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripButton17.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            resources.ApplyResources(this.toolStripButton17, "toolStripButton17");
            this.toolStripButton17.Name = "toolStripButton17";
            this.toolStripButton17.CheckedChanged += new System.EventHandler(this.toolStripButton18_CheckedChanged);
            this.toolStripButton17.Click += new System.EventHandler(this.toolStripButton18_Click);
            // 
            // toolStripButton16
            // 
            this.toolStripButton16.Checked = true;
            this.toolStripButton16.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripButton16.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            resources.ApplyResources(this.toolStripButton16, "toolStripButton16");
            this.toolStripButton16.Name = "toolStripButton16";
            this.toolStripButton16.CheckedChanged += new System.EventHandler(this.toolStripButton18_CheckedChanged);
            this.toolStripButton16.Click += new System.EventHandler(this.toolStripButton18_Click);
            // 
            // toolStripButton15
            // 
            this.toolStripButton15.Checked = true;
            this.toolStripButton15.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripButton15.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            resources.ApplyResources(this.toolStripButton15, "toolStripButton15");
            this.toolStripButton15.Name = "toolStripButton15";
            this.toolStripButton15.CheckedChanged += new System.EventHandler(this.toolStripButton18_CheckedChanged);
            this.toolStripButton15.Click += new System.EventHandler(this.toolStripButton18_Click);
            // 
            // toolStripButton14
            // 
            this.toolStripButton14.Checked = true;
            this.toolStripButton14.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripButton14.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            resources.ApplyResources(this.toolStripButton14, "toolStripButton14");
            this.toolStripButton14.Name = "toolStripButton14";
            this.toolStripButton14.CheckedChanged += new System.EventHandler(this.toolStripButton18_CheckedChanged);
            this.toolStripButton14.Click += new System.EventHandler(this.toolStripButton18_Click);
            // 
            // toolStripButton13
            // 
            this.toolStripButton13.Checked = true;
            this.toolStripButton13.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripButton13.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            resources.ApplyResources(this.toolStripButton13, "toolStripButton13");
            this.toolStripButton13.Name = "toolStripButton13";
            this.toolStripButton13.CheckedChanged += new System.EventHandler(this.toolStripButton18_CheckedChanged);
            this.toolStripButton13.Click += new System.EventHandler(this.toolStripButton18_Click);
            // 
            // toolStripButton12
            // 
            this.toolStripButton12.Checked = true;
            this.toolStripButton12.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripButton12.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            resources.ApplyResources(this.toolStripButton12, "toolStripButton12");
            this.toolStripButton12.Name = "toolStripButton12";
            this.toolStripButton12.CheckedChanged += new System.EventHandler(this.toolStripButton18_CheckedChanged);
            this.toolStripButton12.Click += new System.EventHandler(this.toolStripButton18_Click);
            // 
            // toolStripButton11
            // 
            this.toolStripButton11.Checked = true;
            this.toolStripButton11.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripButton11.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            resources.ApplyResources(this.toolStripButton11, "toolStripButton11");
            this.toolStripButton11.Name = "toolStripButton11";
            this.toolStripButton11.CheckedChanged += new System.EventHandler(this.toolStripButton18_CheckedChanged);
            this.toolStripButton11.Click += new System.EventHandler(this.toolStripButton18_Click);
            // 
            // toolStripButton10
            // 
            this.toolStripButton10.Checked = true;
            this.toolStripButton10.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripButton10.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            resources.ApplyResources(this.toolStripButton10, "toolStripButton10");
            this.toolStripButton10.Name = "toolStripButton10";
            this.toolStripButton10.CheckedChanged += new System.EventHandler(this.toolStripButton18_CheckedChanged);
            this.toolStripButton10.Click += new System.EventHandler(this.toolStripButton18_Click);
            // 
            // toolStripButton9
            // 
            this.toolStripButton9.Checked = true;
            this.toolStripButton9.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripButton9.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            resources.ApplyResources(this.toolStripButton9, "toolStripButton9");
            this.toolStripButton9.Name = "toolStripButton9";
            this.toolStripButton9.CheckedChanged += new System.EventHandler(this.toolStripButton18_CheckedChanged);
            this.toolStripButton9.Click += new System.EventHandler(this.toolStripButton18_Click);
            // 
            // toolStripButton8
            // 
            this.toolStripButton8.Checked = true;
            this.toolStripButton8.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripButton8.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            resources.ApplyResources(this.toolStripButton8, "toolStripButton8");
            this.toolStripButton8.Name = "toolStripButton8";
            this.toolStripButton8.CheckedChanged += new System.EventHandler(this.toolStripButton18_CheckedChanged);
            this.toolStripButton8.Click += new System.EventHandler(this.toolStripButton18_Click);
            // 
            // toolStripButton7
            // 
            this.toolStripButton7.Checked = true;
            this.toolStripButton7.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripButton7.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            resources.ApplyResources(this.toolStripButton7, "toolStripButton7");
            this.toolStripButton7.Name = "toolStripButton7";
            this.toolStripButton7.CheckedChanged += new System.EventHandler(this.toolStripButton18_CheckedChanged);
            this.toolStripButton7.Click += new System.EventHandler(this.toolStripButton18_Click);
            // 
            // toolStripButton6
            // 
            this.toolStripButton6.Checked = true;
            this.toolStripButton6.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripButton6.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            resources.ApplyResources(this.toolStripButton6, "toolStripButton6");
            this.toolStripButton6.Name = "toolStripButton6";
            this.toolStripButton6.CheckedChanged += new System.EventHandler(this.toolStripButton18_CheckedChanged);
            this.toolStripButton6.Click += new System.EventHandler(this.toolStripButton18_Click);
            // 
            // toolStripButton5
            // 
            this.toolStripButton5.Checked = true;
            this.toolStripButton5.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripButton5.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            resources.ApplyResources(this.toolStripButton5, "toolStripButton5");
            this.toolStripButton5.Name = "toolStripButton5";
            this.toolStripButton5.CheckedChanged += new System.EventHandler(this.toolStripButton18_CheckedChanged);
            this.toolStripButton5.Click += new System.EventHandler(this.toolStripButton18_Click);
            // 
            // toolStripButton4
            // 
            this.toolStripButton4.Checked = true;
            this.toolStripButton4.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripButton4.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            resources.ApplyResources(this.toolStripButton4, "toolStripButton4");
            this.toolStripButton4.Name = "toolStripButton4";
            this.toolStripButton4.CheckedChanged += new System.EventHandler(this.toolStripButton18_CheckedChanged);
            this.toolStripButton4.Click += new System.EventHandler(this.toolStripButton18_Click);
            // 
            // toolStripButton3
            // 
            this.toolStripButton3.Checked = true;
            this.toolStripButton3.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripButton3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            resources.ApplyResources(this.toolStripButton3, "toolStripButton3");
            this.toolStripButton3.Name = "toolStripButton3";
            this.toolStripButton3.CheckedChanged += new System.EventHandler(this.toolStripButton18_CheckedChanged);
            this.toolStripButton3.Click += new System.EventHandler(this.toolStripButton18_Click);
            // 
            // toolStripComboBox2
            // 
            this.toolStripComboBox2.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripComboBox2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBox2.DropDownWidth = 32;
            this.toolStripComboBox2.Items.AddRange(new object[] {
            resources.GetString("toolStripComboBox2.Items"),
            resources.GetString("toolStripComboBox2.Items1"),
            resources.GetString("toolStripComboBox2.Items2"),
            resources.GetString("toolStripComboBox2.Items3"),
            resources.GetString("toolStripComboBox2.Items4"),
            resources.GetString("toolStripComboBox2.Items5"),
            resources.GetString("toolStripComboBox2.Items6"),
            resources.GetString("toolStripComboBox2.Items7"),
            resources.GetString("toolStripComboBox2.Items8"),
            resources.GetString("toolStripComboBox2.Items9"),
            resources.GetString("toolStripComboBox2.Items10"),
            resources.GetString("toolStripComboBox2.Items11"),
            resources.GetString("toolStripComboBox2.Items12"),
            resources.GetString("toolStripComboBox2.Items13"),
            resources.GetString("toolStripComboBox2.Items14"),
            resources.GetString("toolStripComboBox2.Items15"),
            resources.GetString("toolStripComboBox2.Items16"),
            resources.GetString("toolStripComboBox2.Items17"),
            resources.GetString("toolStripComboBox2.Items18"),
            resources.GetString("toolStripComboBox2.Items19"),
            resources.GetString("toolStripComboBox2.Items20"),
            resources.GetString("toolStripComboBox2.Items21"),
            resources.GetString("toolStripComboBox2.Items22"),
            resources.GetString("toolStripComboBox2.Items23"),
            resources.GetString("toolStripComboBox2.Items24"),
            resources.GetString("toolStripComboBox2.Items25"),
            resources.GetString("toolStripComboBox2.Items26"),
            resources.GetString("toolStripComboBox2.Items27"),
            resources.GetString("toolStripComboBox2.Items28"),
            resources.GetString("toolStripComboBox2.Items29"),
            resources.GetString("toolStripComboBox2.Items30"),
            resources.GetString("toolStripComboBox2.Items31"),
            resources.GetString("toolStripComboBox2.Items32"),
            resources.GetString("toolStripComboBox2.Items33"),
            resources.GetString("toolStripComboBox2.Items34"),
            resources.GetString("toolStripComboBox2.Items35"),
            resources.GetString("toolStripComboBox2.Items36"),
            resources.GetString("toolStripComboBox2.Items37"),
            resources.GetString("toolStripComboBox2.Items38"),
            resources.GetString("toolStripComboBox2.Items39"),
            resources.GetString("toolStripComboBox2.Items40"),
            resources.GetString("toolStripComboBox2.Items41"),
            resources.GetString("toolStripComboBox2.Items42"),
            resources.GetString("toolStripComboBox2.Items43"),
            resources.GetString("toolStripComboBox2.Items44"),
            resources.GetString("toolStripComboBox2.Items45"),
            resources.GetString("toolStripComboBox2.Items46"),
            resources.GetString("toolStripComboBox2.Items47"),
            resources.GetString("toolStripComboBox2.Items48"),
            resources.GetString("toolStripComboBox2.Items49"),
            resources.GetString("toolStripComboBox2.Items50"),
            resources.GetString("toolStripComboBox2.Items51"),
            resources.GetString("toolStripComboBox2.Items52"),
            resources.GetString("toolStripComboBox2.Items53"),
            resources.GetString("toolStripComboBox2.Items54"),
            resources.GetString("toolStripComboBox2.Items55"),
            resources.GetString("toolStripComboBox2.Items56"),
            resources.GetString("toolStripComboBox2.Items57"),
            resources.GetString("toolStripComboBox2.Items58"),
            resources.GetString("toolStripComboBox2.Items59"),
            resources.GetString("toolStripComboBox2.Items60"),
            resources.GetString("toolStripComboBox2.Items61"),
            resources.GetString("toolStripComboBox2.Items62"),
            resources.GetString("toolStripComboBox2.Items63"),
            resources.GetString("toolStripComboBox2.Items64"),
            resources.GetString("toolStripComboBox2.Items65"),
            resources.GetString("toolStripComboBox2.Items66"),
            resources.GetString("toolStripComboBox2.Items67"),
            resources.GetString("toolStripComboBox2.Items68"),
            resources.GetString("toolStripComboBox2.Items69"),
            resources.GetString("toolStripComboBox2.Items70"),
            resources.GetString("toolStripComboBox2.Items71"),
            resources.GetString("toolStripComboBox2.Items72"),
            resources.GetString("toolStripComboBox2.Items73"),
            resources.GetString("toolStripComboBox2.Items74"),
            resources.GetString("toolStripComboBox2.Items75"),
            resources.GetString("toolStripComboBox2.Items76"),
            resources.GetString("toolStripComboBox2.Items77"),
            resources.GetString("toolStripComboBox2.Items78"),
            resources.GetString("toolStripComboBox2.Items79"),
            resources.GetString("toolStripComboBox2.Items80"),
            resources.GetString("toolStripComboBox2.Items81"),
            resources.GetString("toolStripComboBox2.Items82"),
            resources.GetString("toolStripComboBox2.Items83"),
            resources.GetString("toolStripComboBox2.Items84"),
            resources.GetString("toolStripComboBox2.Items85"),
            resources.GetString("toolStripComboBox2.Items86"),
            resources.GetString("toolStripComboBox2.Items87"),
            resources.GetString("toolStripComboBox2.Items88"),
            resources.GetString("toolStripComboBox2.Items89"),
            resources.GetString("toolStripComboBox2.Items90"),
            resources.GetString("toolStripComboBox2.Items91"),
            resources.GetString("toolStripComboBox2.Items92"),
            resources.GetString("toolStripComboBox2.Items93"),
            resources.GetString("toolStripComboBox2.Items94"),
            resources.GetString("toolStripComboBox2.Items95"),
            resources.GetString("toolStripComboBox2.Items96"),
            resources.GetString("toolStripComboBox2.Items97"),
            resources.GetString("toolStripComboBox2.Items98"),
            resources.GetString("toolStripComboBox2.Items99"),
            resources.GetString("toolStripComboBox2.Items100"),
            resources.GetString("toolStripComboBox2.Items101"),
            resources.GetString("toolStripComboBox2.Items102"),
            resources.GetString("toolStripComboBox2.Items103"),
            resources.GetString("toolStripComboBox2.Items104"),
            resources.GetString("toolStripComboBox2.Items105"),
            resources.GetString("toolStripComboBox2.Items106"),
            resources.GetString("toolStripComboBox2.Items107"),
            resources.GetString("toolStripComboBox2.Items108"),
            resources.GetString("toolStripComboBox2.Items109"),
            resources.GetString("toolStripComboBox2.Items110"),
            resources.GetString("toolStripComboBox2.Items111"),
            resources.GetString("toolStripComboBox2.Items112"),
            resources.GetString("toolStripComboBox2.Items113"),
            resources.GetString("toolStripComboBox2.Items114"),
            resources.GetString("toolStripComboBox2.Items115"),
            resources.GetString("toolStripComboBox2.Items116"),
            resources.GetString("toolStripComboBox2.Items117"),
            resources.GetString("toolStripComboBox2.Items118"),
            resources.GetString("toolStripComboBox2.Items119"),
            resources.GetString("toolStripComboBox2.Items120"),
            resources.GetString("toolStripComboBox2.Items121"),
            resources.GetString("toolStripComboBox2.Items122"),
            resources.GetString("toolStripComboBox2.Items123"),
            resources.GetString("toolStripComboBox2.Items124"),
            resources.GetString("toolStripComboBox2.Items125"),
            resources.GetString("toolStripComboBox2.Items126"),
            resources.GetString("toolStripComboBox2.Items127"),
            resources.GetString("toolStripComboBox2.Items128")});
            this.toolStripComboBox2.Name = "toolStripComboBox2";
            resources.ApplyResources(this.toolStripComboBox2, "toolStripComboBox2");
            this.toolStripComboBox2.SelectedIndexChanged += new System.EventHandler(this.toolStripComboBox2_SelectedIndexChanged);
            // 
            // toolStripLabel4
            // 
            this.toolStripLabel4.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripLabel4.Name = "toolStripLabel4";
            resources.ApplyResources(this.toolStripLabel4, "toolStripLabel4");
            // 
            // toolStripComboBox1
            // 
            this.toolStripComboBox1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripComboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBox1.DropDownWidth = 32;
            this.toolStripComboBox1.Items.AddRange(new object[] {
            resources.GetString("toolStripComboBox1.Items"),
            resources.GetString("toolStripComboBox1.Items1"),
            resources.GetString("toolStripComboBox1.Items2"),
            resources.GetString("toolStripComboBox1.Items3"),
            resources.GetString("toolStripComboBox1.Items4"),
            resources.GetString("toolStripComboBox1.Items5"),
            resources.GetString("toolStripComboBox1.Items6"),
            resources.GetString("toolStripComboBox1.Items7"),
            resources.GetString("toolStripComboBox1.Items8"),
            resources.GetString("toolStripComboBox1.Items9"),
            resources.GetString("toolStripComboBox1.Items10"),
            resources.GetString("toolStripComboBox1.Items11"),
            resources.GetString("toolStripComboBox1.Items12"),
            resources.GetString("toolStripComboBox1.Items13"),
            resources.GetString("toolStripComboBox1.Items14")});
            this.toolStripComboBox1.Name = "toolStripComboBox1";
            resources.ApplyResources(this.toolStripComboBox1, "toolStripComboBox1");
            this.toolStripComboBox1.SelectedIndexChanged += new System.EventHandler(this.toolStripComboBox1_SelectedIndexChanged_1);
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripLabel2.Name = "toolStripLabel2";
            resources.ApplyResources(this.toolStripLabel2, "toolStripLabel2");
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.listView1);
            resources.ApplyResources(this.tabPage2, "tabPage2");
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            resources.ApplyResources(this.listView1, "listView1");
            this.listView1.FullRowSelect = true;
            this.listView1.GridLines = true;
            this.listView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listView1.Name = "listView1";
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            resources.ApplyResources(this.columnHeader1, "columnHeader1");
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripStatusLabel2});
            resources.ApplyResources(this.statusStrip1, "statusStrip1");
            this.statusStrip1.Name = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            resources.ApplyResources(this.toolStripStatusLabel1, "toolStripStatusLabel1");
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            resources.ApplyResources(this.toolStripStatusLabel2, "toolStripStatusLabel2");
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.toolToolStripMenuItem,
            this.helpToolStripMenuItem});
            resources.ApplyResources(this.menuStrip1, "menuStrip1");
            this.menuStrip1.Name = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.toolStripMenuItemExit});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            resources.ApplyResources(this.fileToolStripMenuItem, "fileToolStripMenuItem");
            // 
            // loadToolStripMenuItem
            // 
            this.loadToolStripMenuItem.Name = "loadToolStripMenuItem";
            resources.ApplyResources(this.loadToolStripMenuItem, "loadToolStripMenuItem");
            this.loadToolStripMenuItem.Click += new System.EventHandler(this.loadToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            resources.ApplyResources(this.saveToolStripMenuItem, "saveToolStripMenuItem");
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            resources.ApplyResources(this.saveAsToolStripMenuItem, "saveAsToolStripMenuItem");
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // toolStripMenuItemExit
            // 
            this.toolStripMenuItemExit.Name = "toolStripMenuItemExit";
            resources.ApplyResources(this.toolStripMenuItemExit, "toolStripMenuItemExit");
            this.toolStripMenuItemExit.Click += new System.EventHandler(this.toolStripMenuItemExit_Click);
            // 
            // toolToolStripMenuItem
            // 
            this.toolToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingsToolStripMenuItem});
            this.toolToolStripMenuItem.Name = "toolToolStripMenuItem";
            resources.ApplyResources(this.toolToolStripMenuItem, "toolToolStripMenuItem");
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            resources.ApplyResources(this.settingsToolStripMenuItem, "settingsToolStripMenuItem");
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.settingsToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemAbout});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            resources.ApplyResources(this.helpToolStripMenuItem, "helpToolStripMenuItem");
            // 
            // toolStripMenuItemAbout
            // 
            this.toolStripMenuItemAbout.Name = "toolStripMenuItemAbout";
            resources.ApplyResources(this.toolStripMenuItemAbout, "toolStripMenuItemAbout");
            this.toolStripMenuItemAbout.Click += new System.EventHandler(this.toolStripMenuItemAbout_Click);
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.DefaultExt = "MAmi";
            this.saveFileDialog1.FileName = "MyEnvironment";
            resources.ApplyResources(this.saveFileDialog1, "saveFileDialog1");
            this.saveFileDialog1.SupportMultiDottedExtensions = true;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.DefaultExt = "*.MAmi";
            this.openFileDialog1.FileName = "MyEnvironment.MAmi";
            resources.ApplyResources(this.openFileDialog1, "openFileDialog1");
            this.openFileDialog1.SupportMultiDottedExtensions = true;
            // 
            // timer1
            // 
            this.timer1.Interval = 50;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // toolStrip1
            // 
            this.toolStrip1.ClickThrough = false;
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(609, 609);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.toolStripComboBoxMidiIf,
            this.toolStripDropDownButton1,
            this.toolStripButton1,
            this.toolStripButton2,
            this.toolStripSeparator1,
            this.toolStripButton20,
            this.toolStripButton21});
            resources.ApplyResources(this.toolStrip1, "toolStrip1");
            this.toolStrip1.Name = "toolStrip1";
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            resources.ApplyResources(this.toolStripLabel1, "toolStripLabel1");
            // 
            // toolStripComboBoxMidiIf
            // 
            this.toolStripComboBoxMidiIf.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBoxMidiIf.Name = "toolStripComboBoxMidiIf";
            resources.ApplyResources(this.toolStripComboBoxMidiIf, "toolStripComboBoxMidiIf");
            this.toolStripComboBoxMidiIf.DropDown += new System.EventHandler(this.toolStripComboBox1_DropDown);
            this.toolStripComboBoxMidiIf.SelectedIndexChanged += new System.EventHandler(this.toolStripComboBox1_SelectedIndexChanged);
            // 
            // toolStripDropDownButton1
            // 
            this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lAToolStripMenuItem,
            this.pCMToolStripMenuItem,
            this.fMSynthesisToolStripMenuItem,
            this.wSGToolStripMenuItem,
            this.pSGToolStripMenuItem});
            resources.ApplyResources(this.toolStripDropDownButton1, "toolStripDropDownButton1");
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            // 
            // lAToolStripMenuItem
            // 
            this.lAToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mT32ToolStripMenuItem,
            this.extendCM32PToolStripMenuItem});
            this.lAToolStripMenuItem.Name = "lAToolStripMenuItem";
            resources.ApplyResources(this.lAToolStripMenuItem, "lAToolStripMenuItem");
            // 
            // mT32ToolStripMenuItem
            // 
            this.mT32ToolStripMenuItem.Name = "mT32ToolStripMenuItem";
            resources.ApplyResources(this.mT32ToolStripMenuItem, "mT32ToolStripMenuItem");
            this.mT32ToolStripMenuItem.Click += new System.EventHandler(this.mT32ToolStripMenuItem_Click);
            // 
            // extendCM32PToolStripMenuItem
            // 
            this.extendCM32PToolStripMenuItem.Name = "extendCM32PToolStripMenuItem";
            resources.ApplyResources(this.extendCM32PToolStripMenuItem, "extendCM32PToolStripMenuItem");
            this.extendCM32PToolStripMenuItem.Click += new System.EventHandler(this.extendCM32PToolStripMenuItem_Click);
            // 
            // pCMToolStripMenuItem
            // 
            this.pCMToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.extendC140ToolStripMenuItem,
            this.extendSPC700ToolStripMenuItem});
            this.pCMToolStripMenuItem.Name = "pCMToolStripMenuItem";
            resources.ApplyResources(this.pCMToolStripMenuItem, "pCMToolStripMenuItem");
            // 
            // extendC140ToolStripMenuItem
            // 
            this.extendC140ToolStripMenuItem.Name = "extendC140ToolStripMenuItem";
            resources.ApplyResources(this.extendC140ToolStripMenuItem, "extendC140ToolStripMenuItem");
            this.extendC140ToolStripMenuItem.Click += new System.EventHandler(this.extendC140ToolStripMenuItem_Click);
            // 
            // extendSPC700ToolStripMenuItem
            // 
            this.extendSPC700ToolStripMenuItem.Name = "extendSPC700ToolStripMenuItem";
            resources.ApplyResources(this.extendSPC700ToolStripMenuItem, "extendSPC700ToolStripMenuItem");
            this.extendSPC700ToolStripMenuItem.Click += new System.EventHandler(this.extendSPC700ToolStripMenuItem_Click);
            // 
            // fMSynthesisToolStripMenuItem
            // 
            this.fMSynthesisToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.extendYM2610BToolStripMenuItem,
            this.addYM2151ToolStripMenuItem,
            this.addYM2612ToolStripMenuItem,
            this.extendYM3812ToolStripMenuItem,
            this.extendYM2413ToolStripMenuItem});
            this.fMSynthesisToolStripMenuItem.Name = "fMSynthesisToolStripMenuItem";
            resources.ApplyResources(this.fMSynthesisToolStripMenuItem, "fMSynthesisToolStripMenuItem");
            // 
            // extendYM2610BToolStripMenuItem
            // 
            this.extendYM2610BToolStripMenuItem.Name = "extendYM2610BToolStripMenuItem";
            resources.ApplyResources(this.extendYM2610BToolStripMenuItem, "extendYM2610BToolStripMenuItem");
            this.extendYM2610BToolStripMenuItem.Click += new System.EventHandler(this.extendYM2610BToolStripMenuItem_Click);
            // 
            // addYM2151ToolStripMenuItem
            // 
            this.addYM2151ToolStripMenuItem.Name = "addYM2151ToolStripMenuItem";
            resources.ApplyResources(this.addYM2151ToolStripMenuItem, "addYM2151ToolStripMenuItem");
            this.addYM2151ToolStripMenuItem.Click += new System.EventHandler(this.addYM2151ToolStripMenuItem_Click);
            // 
            // addYM2612ToolStripMenuItem
            // 
            this.addYM2612ToolStripMenuItem.Name = "addYM2612ToolStripMenuItem";
            resources.ApplyResources(this.addYM2612ToolStripMenuItem, "addYM2612ToolStripMenuItem");
            this.addYM2612ToolStripMenuItem.Click += new System.EventHandler(this.addYM2612ToolStripMenuItem_Click);
            // 
            // extendYM3812ToolStripMenuItem
            // 
            this.extendYM3812ToolStripMenuItem.Name = "extendYM3812ToolStripMenuItem";
            resources.ApplyResources(this.extendYM3812ToolStripMenuItem, "extendYM3812ToolStripMenuItem");
            this.extendYM3812ToolStripMenuItem.Click += new System.EventHandler(this.extendYM3812ToolStripMenuItem_Click);
            // 
            // extendYM2413ToolStripMenuItem
            // 
            this.extendYM2413ToolStripMenuItem.Name = "extendYM2413ToolStripMenuItem";
            resources.ApplyResources(this.extendYM2413ToolStripMenuItem, "extendYM2413ToolStripMenuItem");
            this.extendYM2413ToolStripMenuItem.Click += new System.EventHandler(this.extendYM2413ToolStripMenuItem_Click);
            // 
            // wSGToolStripMenuItem
            // 
            this.wSGToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.extendSCC1kToolStripMenuItem,
            this.addNAMCOCUS30ToolStripMenuItem,
            this.extendHuC6230ToolStripMenuItem});
            this.wSGToolStripMenuItem.Name = "wSGToolStripMenuItem";
            resources.ApplyResources(this.wSGToolStripMenuItem, "wSGToolStripMenuItem");
            // 
            // extendSCC1kToolStripMenuItem
            // 
            this.extendSCC1kToolStripMenuItem.Name = "extendSCC1kToolStripMenuItem";
            resources.ApplyResources(this.extendSCC1kToolStripMenuItem, "extendSCC1kToolStripMenuItem");
            this.extendSCC1kToolStripMenuItem.Click += new System.EventHandler(this.extendSCC1kToolStripMenuItem_Click);
            // 
            // addNAMCOCUS30ToolStripMenuItem
            // 
            this.addNAMCOCUS30ToolStripMenuItem.Name = "addNAMCOCUS30ToolStripMenuItem";
            resources.ApplyResources(this.addNAMCOCUS30ToolStripMenuItem, "addNAMCOCUS30ToolStripMenuItem");
            this.addNAMCOCUS30ToolStripMenuItem.Click += new System.EventHandler(this.addNAMCOCUS30ToolStripMenuItem_Click);
            // 
            // extendHuC6230ToolStripMenuItem
            // 
            this.extendHuC6230ToolStripMenuItem.Name = "extendHuC6230ToolStripMenuItem";
            resources.ApplyResources(this.extendHuC6230ToolStripMenuItem, "extendHuC6230ToolStripMenuItem");
            this.extendHuC6230ToolStripMenuItem.Click += new System.EventHandler(this.extendHuC6230ToolStripMenuItem_Click);
            // 
            // pSGToolStripMenuItem
            // 
            this.pSGToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.extendMOS8580ToolStripMenuItem,
            this.extendMOS6581ToolStripMenuItem,
            this.extendNESAPUToolStripMenuItem,
            this.extendGBAPUToolStripMenuItem,
            this.extendPOKEYToolStripMenuItem,
            this.addSN76496ToolStripMenuItem,
            this.extendAY38910ToolStripMenuItem,
            this.extendMSM5232ToolStripMenuItem,
            this.extendBeepToolStripMenuItem});
            this.pSGToolStripMenuItem.Name = "pSGToolStripMenuItem";
            resources.ApplyResources(this.pSGToolStripMenuItem, "pSGToolStripMenuItem");
            // 
            // extendMOS8580ToolStripMenuItem
            // 
            this.extendMOS8580ToolStripMenuItem.Name = "extendMOS8580ToolStripMenuItem";
            resources.ApplyResources(this.extendMOS8580ToolStripMenuItem, "extendMOS8580ToolStripMenuItem");
            this.extendMOS8580ToolStripMenuItem.Click += new System.EventHandler(this.extendMOS8580ToolStripMenuItem_Click);
            // 
            // extendMOS6581ToolStripMenuItem
            // 
            this.extendMOS6581ToolStripMenuItem.Name = "extendMOS6581ToolStripMenuItem";
            resources.ApplyResources(this.extendMOS6581ToolStripMenuItem, "extendMOS6581ToolStripMenuItem");
            this.extendMOS6581ToolStripMenuItem.Click += new System.EventHandler(this.extendMOS6581ToolStripMenuItem_Click);
            // 
            // extendNESAPUToolStripMenuItem
            // 
            this.extendNESAPUToolStripMenuItem.Name = "extendNESAPUToolStripMenuItem";
            resources.ApplyResources(this.extendNESAPUToolStripMenuItem, "extendNESAPUToolStripMenuItem");
            this.extendNESAPUToolStripMenuItem.Click += new System.EventHandler(this.extendNESAPUToolStripMenuItem_Click);
            // 
            // extendGBAPUToolStripMenuItem
            // 
            this.extendGBAPUToolStripMenuItem.Name = "extendGBAPUToolStripMenuItem";
            resources.ApplyResources(this.extendGBAPUToolStripMenuItem, "extendGBAPUToolStripMenuItem");
            this.extendGBAPUToolStripMenuItem.Click += new System.EventHandler(this.extendGBAPUToolStripMenuItem_Click);
            // 
            // extendPOKEYToolStripMenuItem
            // 
            this.extendPOKEYToolStripMenuItem.Name = "extendPOKEYToolStripMenuItem";
            resources.ApplyResources(this.extendPOKEYToolStripMenuItem, "extendPOKEYToolStripMenuItem");
            this.extendPOKEYToolStripMenuItem.Click += new System.EventHandler(this.extendPOKEYToolStripMenuItem_Click);
            // 
            // addSN76496ToolStripMenuItem
            // 
            this.addSN76496ToolStripMenuItem.Name = "addSN76496ToolStripMenuItem";
            resources.ApplyResources(this.addSN76496ToolStripMenuItem, "addSN76496ToolStripMenuItem");
            this.addSN76496ToolStripMenuItem.Click += new System.EventHandler(this.addSN76496ToolStripMenuItem_Click);
            // 
            // extendAY38910ToolStripMenuItem
            // 
            this.extendAY38910ToolStripMenuItem.Name = "extendAY38910ToolStripMenuItem";
            resources.ApplyResources(this.extendAY38910ToolStripMenuItem, "extendAY38910ToolStripMenuItem");
            this.extendAY38910ToolStripMenuItem.Click += new System.EventHandler(this.extendAY38910ToolStripMenuItem_Click);
            // 
            // extendMSM5232ToolStripMenuItem
            // 
            this.extendMSM5232ToolStripMenuItem.Name = "extendMSM5232ToolStripMenuItem";
            resources.ApplyResources(this.extendMSM5232ToolStripMenuItem, "extendMSM5232ToolStripMenuItem");
            this.extendMSM5232ToolStripMenuItem.Click += new System.EventHandler(this.extendMSM5232ToolStripMenuItem_Click);
            // 
            // extendBeepToolStripMenuItem
            // 
            this.extendBeepToolStripMenuItem.Name = "extendBeepToolStripMenuItem";
            resources.ApplyResources(this.extendBeepToolStripMenuItem, "extendBeepToolStripMenuItem");
            this.extendBeepToolStripMenuItem.Click += new System.EventHandler(this.extendBeepToolStripMenuItem_Click);
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton1.Image = global::zanac.MAmidiMEmo.Properties.Resources.Panic;
            resources.ApplyResources(this.toolStripButton1, "toolStripButton1");
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Click += new System.EventHandler(this.toolStripButton1_Click);
            // 
            // toolStripButton2
            // 
            this.toolStripButton2.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton2.Image = global::zanac.MAmidiMEmo.Properties.Resources.Rst;
            resources.ApplyResources(this.toolStripButton2, "toolStripButton2");
            this.toolStripButton2.Name = "toolStripButton2";
            this.toolStripButton2.Click += new System.EventHandler(this.toolStripButton2_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // toolStripButton20
            // 
            resources.ApplyResources(this.toolStripButton20, "toolStripButton20");
            this.toolStripButton20.Name = "toolStripButton20";
            this.toolStripButton20.CheckedChanged += new System.EventHandler(this.toolStripButton20_CheckedChanged);
            this.toolStripButton20.Click += new System.EventHandler(this.toolStripButton20_Click);
            // 
            // toolStripButton21
            // 
            resources.ApplyResources(this.toolStripButton21, "toolStripButton21");
            this.toolStripButton21.Name = "toolStripButton21";
            this.toolStripButton21.CheckedChanged += new System.EventHandler(this.toolStripButton21_CheckedChanged);
            this.toolStripButton21.Click += new System.EventHandler(this.toolStripButton21_Click);
            // 
            // multiMediaTimerComponent1
            // 
            this.multiMediaTimerComponent1.Enabled = false;
            this.multiMediaTimerComponent1.Interval = ((uint)(1000u));
            this.multiMediaTimerComponent1.Resolution = ((uint)(1000u));
            // 
            // FormMain
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "FormMain";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.contextMenuStripInst.ResumeLayout(false);
            this.contextMenuStripProp.ResumeLayout(false);
            this.toolStrip3.ResumeLayout(false);
            this.toolStrip3.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.toolStrip2.ResumeLayout(false);
            this.toolStrip2.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemExit;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemAbout;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.ListView listViewIntruments;
        private ComponentModel.ToolStripBase toolStrip1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxMidiIf;
        private System.Windows.Forms.PropertyGrid propertyGrid;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripInst;
        private System.Windows.Forms.ToolStripMenuItem decreaseThisKindOfChipToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripMenuItem toolToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton toolStripButton2;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripProp;
        private System.Windows.Forms.ToolStripMenuItem resetToDefaultThisPropertyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fMSynthesisToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addYM2151ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addYM2612ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extendYM3812ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extendYM2413ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem wSGToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extendSCC1kToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addNAMCOCUS30ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pSGToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extendNESAPUToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addSN76496ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extendMSM5232ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extendAY38910ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extendMOS8580ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extendMOS6581ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extendGBAPUToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pCMToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extendC140ToolStripMenuItem;
        private System.Windows.Forms.TabPage tabPage3;
        private PianoControl pianoControl1;
        private System.Windows.Forms.ToolStripMenuItem extendBeepToolStripMenuItem;
        private ComponentModel.ToolStripBase toolStrip2;
        private System.Windows.Forms.ToolStripButton toolStripButton19;
        private System.Windows.Forms.ToolStripButton toolStripButton18;
        private System.Windows.Forms.ToolStripButton toolStripButton17;
        private System.Windows.Forms.ToolStripButton toolStripButton16;
        private System.Windows.Forms.ToolStripButton toolStripButton15;
        private System.Windows.Forms.ToolStripButton toolStripButton14;
        private System.Windows.Forms.ToolStripButton toolStripButton13;
        private System.Windows.Forms.ToolStripButton toolStripButton12;
        private System.Windows.Forms.ToolStripButton toolStripButton11;
        private System.Windows.Forms.ToolStripButton toolStripButton10;
        private System.Windows.Forms.ToolStripButton toolStripButton9;
        private System.Windows.Forms.ToolStripButton toolStripButton8;
        private System.Windows.Forms.ToolStripButton toolStripButton7;
        private System.Windows.Forms.ToolStripButton toolStripButton6;
        private System.Windows.Forms.ToolStripButton toolStripButton5;
        private System.Windows.Forms.ToolStripButton toolStripButton4;
        private System.Windows.Forms.ToolStripButton toolStripButton3;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBox1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ToolStripLabel toolStripLabel3;
        private ComponentModel.MultiMediaTimerComponent multiMediaTimerComponent1;
        private ComponentModel.ToolStripBase toolStrip3;
        private System.Windows.Forms.ToolStripButton toolStripButtonCat;
        private System.Windows.Forms.ToolStripButton toolStripButtonA2Z;
        private System.Windows.Forms.ToolStripButton toolStripButtonPopup;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBox2;
        private System.Windows.Forms.ToolStripLabel toolStripLabel4;
        private System.Windows.Forms.ToolStripMenuItem extendHuC6230ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extendSPC700ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extendPOKEYToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extendYM2610BToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem lAToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mT32ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extendCM32PToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButton20;
        private System.Windows.Forms.ToolStripButton toolStripButton21;
        private System.Windows.Forms.ToolStripMenuItem cloneSelectedChipToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItemSep;
    }
}

