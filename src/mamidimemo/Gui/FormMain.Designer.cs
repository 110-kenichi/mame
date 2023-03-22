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
            this.metroLabelDrop = new MetroFramework.Controls.MetroLabel();
            this.listViewIntruments = new System.Windows.Forms.ListView();
            this.contextMenuStripInst = new MetroFramework.Controls.MetroContextMenu(this.components);
            this.decreaseThisKindOfChipToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cloneSelectedChipToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSepInst = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemSep = new System.Windows.Forms.ToolStripSeparator();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.propertyGrid = new System.Windows.Forms.PropertyGrid();
            this.contextMenuStripProp = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.resetToDefaultThisPropertyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip3 = new zanac.MAmidiMEmo.ComponentModel.ToolStripBase();
            this.toolStripButtonCat = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonA2Z = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonPopup = new System.Windows.Forms.ToolStripButton();
            this.tabControlBottom = new MetroFramework.Controls.MetroTabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.pianoControl1 = new zanac.MAmidiMEmo.Gui.PianoControl();
            this.toolStrip2 = new zanac.MAmidiMEmo.ComponentModel.ToolStripBase();
            this.toolStripLabel3 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripComboBoxPort = new System.Windows.Forms.ToolStripComboBox();
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
            this.toolStripComboBoxProgNo = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripLabel4 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripComboBoxKeyCh = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripLabel5 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripComboBoxCC = new System.Windows.Forms.ToolStripComboBox();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.metroTrackBarVol = new MetroFramework.Controls.MetroTrackBar();
            this.label1 = new System.Windows.Forms.Label();
            this.labelClock = new System.Windows.Forms.Label();
            this.panelOsc2 = new System.Windows.Forms.Panel();
            this.labelTitle = new zanac.MAmidiMEmo.Gui.WrapLabel();
            this.labelStat = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.labelCpuLoad = new System.Windows.Forms.Label();
            this.panelChDisp = new System.Windows.Forms.Panel();
            this.toolStrip4 = new zanac.MAmidiMEmo.Gui.ClickThroughToolStrip();
            this.toolStripButtonPlay = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonPause = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonStop = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonPrev = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonNext = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonOpen = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonAutoVGM = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonAutoWav = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonReload = new System.Windows.Forms.ToolStripButton();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.draggableListViewMediaList = new ListViewInsertionDrag.DraggableListView();
            this.columnHeaderFname = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStripList = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.playToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.explorerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.listViewOutput = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openSampleFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportMAmidiToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemExit = new System.Windows.Forms.ToolStripMenuItem();
            this.toolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyMAmiVSTiToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mIDIDelayCheckerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.saveFileDialogMami = new System.Windows.Forms.SaveFileDialog();
            this.openFileDialogMami = new System.Windows.Forms.OpenFileDialog();
            this.timerOsc = new System.Windows.Forms.Timer(this.components);
            this.toolStrip1 = new zanac.MAmidiMEmo.ComponentModel.ToolStripBase();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripComboBoxMidiIfA = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripLabel6 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripComboBoxMidiIfB = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.lAToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mT32ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extendCM32PToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pCMToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extendC140ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extendSPC700ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fMSynthesisToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extendYM2610BToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2608 = new System.Windows.Forms.ToolStripMenuItem();
            this.addYM2151ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addYM2612ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extendYM3812ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extendYM2413ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extendYMF262ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.yM2414ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.yM3806OPQToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            this.uPD1771ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extendMSM5232ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extendBeepToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.eTCToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sP0256ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extendSP0256AL2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extendSAMToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dISCRETEToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sN76477ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton20 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton21 = new System.Windows.Forms.ToolStripButton();
            this.openFileDialogMidi = new System.Windows.Forms.OpenFileDialog();
            this.fileSystemWatcherMidi = new System.IO.FileSystemWatcher();
            this.timerReload = new System.Windows.Forms.Timer(this.components);
            this.saveFileDialogMAmidi = new System.Windows.Forms.SaveFileDialog();
            this.betterFolderBrowserVSTi = new WK.Libraries.BetterFolderBrowserNS.BetterFolderBrowser(this.components);
            this.columnHeaderFile = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
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
            this.tabControlBottom.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.toolStrip2.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.toolStrip4.SuspendLayout();
            this.tabPage5.SuspendLayout();
            this.contextMenuStripList.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fileSystemWatcherMidi)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            resources.ApplyResources(this.splitContainer1, "splitContainer1");
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            resources.ApplyResources(this.splitContainer1.Panel1, "splitContainer1.Panel1");
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabControlBottom);
            resources.ApplyResources(this.splitContainer1.Panel2, "splitContainer1.Panel2");
            // 
            // splitContainer2
            // 
            resources.ApplyResources(this.splitContainer2, "splitContainer2");
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.metroLabelDrop);
            this.splitContainer2.Panel1.Controls.Add(this.listViewIntruments);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.propertyGrid);
            this.splitContainer2.Panel2.Controls.Add(this.toolStrip3);
            // 
            // metroLabelDrop
            // 
            resources.ApplyResources(this.metroLabelDrop, "metroLabelDrop");
            this.metroLabelDrop.Name = "metroLabelDrop";
            this.metroLabelDrop.WrapToLine = true;
            this.metroLabelDrop.DragDrop += new System.Windows.Forms.DragEventHandler(this.listViewIntruments_DragDrop);
            this.metroLabelDrop.DragEnter += new System.Windows.Forms.DragEventHandler(this.listViewIntruments_DragEnter);
            // 
            // listViewIntruments
            // 
            this.listViewIntruments.AllowDrop = true;
            this.listViewIntruments.ContextMenuStrip = this.contextMenuStripInst;
            resources.ApplyResources(this.listViewIntruments, "listViewIntruments");
            this.listViewIntruments.FullRowSelect = true;
            this.listViewIntruments.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            ((System.Windows.Forms.ListViewGroup)(resources.GetObject("listViewIntruments.Groups"))),
            ((System.Windows.Forms.ListViewGroup)(resources.GetObject("listViewIntruments.Groups1"))),
            ((System.Windows.Forms.ListViewGroup)(resources.GetObject("listViewIntruments.Groups2"))),
            ((System.Windows.Forms.ListViewGroup)(resources.GetObject("listViewIntruments.Groups3"))),
            ((System.Windows.Forms.ListViewGroup)(resources.GetObject("listViewIntruments.Groups4"))),
            ((System.Windows.Forms.ListViewGroup)(resources.GetObject("listViewIntruments.Groups5")))});
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
            this.toolStripSepInst,
            this.toolStripMenuItem1,
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
            // toolStripSepInst
            // 
            this.toolStripSepInst.Name = "toolStripSepInst";
            resources.ApplyResources(this.toolStripSepInst, "toolStripSepInst");
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            resources.ApplyResources(this.toolStripMenuItem1, "toolStripMenuItem1");
            this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
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
            this.propertyGrid.HelpBackColor = System.Drawing.SystemColors.Info;
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.PropertyTabChanged += new System.Windows.Forms.PropertyTabChangedEventHandler(this.propertyGrid_PropertyTabChanged);
            this.propertyGrid.SelectedObjectsChanged += new System.EventHandler(this.propertyGrid_SelectedObjectsChanged);
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
            // tabControlBottom
            // 
            this.tabControlBottom.Controls.Add(this.tabPage1);
            this.tabControlBottom.Controls.Add(this.tabPage3);
            this.tabControlBottom.Controls.Add(this.tabPage4);
            this.tabControlBottom.Controls.Add(this.tabPage5);
            this.tabControlBottom.Controls.Add(this.tabPage2);
            this.tabControlBottom.DataBindings.Add(new System.Windows.Forms.Binding("TabIndex", global::zanac.MAmidiMEmo.Properties.Settings.Default, "MWinTab", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            resources.ApplyResources(this.tabControlBottom, "tabControlBottom");
            this.tabControlBottom.Name = "tabControlBottom";
            this.tabControlBottom.SelectedIndex = 0;
            this.tabControlBottom.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabControlBottom.TabIndex = global::zanac.MAmidiMEmo.Properties.Settings.Default.MWinTab;
            this.tabControlBottom.UseSelectable = true;
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
            this.pianoControl1.TargetTimbres = null;
            // 
            // toolStrip2
            // 
            resources.ApplyResources(this.toolStrip2, "toolStrip2");
            this.toolStrip2.ClickThrough = false;
            this.toolStrip2.ImageScalingSize = new System.Drawing.Size(1369, 1369);
            this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel3,
            this.toolStripComboBoxPort,
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
            this.toolStripComboBoxProgNo,
            this.toolStripLabel4,
            this.toolStripComboBoxKeyCh,
            this.toolStripLabel5,
            this.toolStripSeparator2,
            this.toolStripLabel2,
            this.toolStripComboBoxCC});
            this.toolStrip2.Name = "toolStrip2";
            // 
            // toolStripLabel3
            // 
            this.toolStripLabel3.Name = "toolStripLabel3";
            resources.ApplyResources(this.toolStripLabel3, "toolStripLabel3");
            // 
            // toolStripComboBoxPort
            // 
            resources.ApplyResources(this.toolStripComboBoxPort, "toolStripComboBoxPort");
            this.toolStripComboBoxPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBoxPort.DropDownWidth = 32;
            this.toolStripComboBoxPort.Items.AddRange(new object[] {
            resources.GetString("toolStripComboBoxPort.Items"),
            resources.GetString("toolStripComboBoxPort.Items1"),
            resources.GetString("toolStripComboBoxPort.Items2")});
            this.toolStripComboBoxPort.Name = "toolStripComboBoxPort";
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
            // toolStripComboBoxProgNo
            // 
            this.toolStripComboBoxProgNo.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            resources.ApplyResources(this.toolStripComboBoxProgNo, "toolStripComboBoxProgNo");
            this.toolStripComboBoxProgNo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBoxProgNo.DropDownWidth = 32;
            this.toolStripComboBoxProgNo.Items.AddRange(new object[] {
            resources.GetString("toolStripComboBoxProgNo.Items"),
            resources.GetString("toolStripComboBoxProgNo.Items1"),
            resources.GetString("toolStripComboBoxProgNo.Items2"),
            resources.GetString("toolStripComboBoxProgNo.Items3"),
            resources.GetString("toolStripComboBoxProgNo.Items4"),
            resources.GetString("toolStripComboBoxProgNo.Items5"),
            resources.GetString("toolStripComboBoxProgNo.Items6"),
            resources.GetString("toolStripComboBoxProgNo.Items7"),
            resources.GetString("toolStripComboBoxProgNo.Items8"),
            resources.GetString("toolStripComboBoxProgNo.Items9"),
            resources.GetString("toolStripComboBoxProgNo.Items10"),
            resources.GetString("toolStripComboBoxProgNo.Items11"),
            resources.GetString("toolStripComboBoxProgNo.Items12"),
            resources.GetString("toolStripComboBoxProgNo.Items13"),
            resources.GetString("toolStripComboBoxProgNo.Items14"),
            resources.GetString("toolStripComboBoxProgNo.Items15"),
            resources.GetString("toolStripComboBoxProgNo.Items16"),
            resources.GetString("toolStripComboBoxProgNo.Items17"),
            resources.GetString("toolStripComboBoxProgNo.Items18"),
            resources.GetString("toolStripComboBoxProgNo.Items19"),
            resources.GetString("toolStripComboBoxProgNo.Items20"),
            resources.GetString("toolStripComboBoxProgNo.Items21"),
            resources.GetString("toolStripComboBoxProgNo.Items22"),
            resources.GetString("toolStripComboBoxProgNo.Items23"),
            resources.GetString("toolStripComboBoxProgNo.Items24"),
            resources.GetString("toolStripComboBoxProgNo.Items25"),
            resources.GetString("toolStripComboBoxProgNo.Items26"),
            resources.GetString("toolStripComboBoxProgNo.Items27"),
            resources.GetString("toolStripComboBoxProgNo.Items28"),
            resources.GetString("toolStripComboBoxProgNo.Items29"),
            resources.GetString("toolStripComboBoxProgNo.Items30"),
            resources.GetString("toolStripComboBoxProgNo.Items31"),
            resources.GetString("toolStripComboBoxProgNo.Items32"),
            resources.GetString("toolStripComboBoxProgNo.Items33"),
            resources.GetString("toolStripComboBoxProgNo.Items34"),
            resources.GetString("toolStripComboBoxProgNo.Items35"),
            resources.GetString("toolStripComboBoxProgNo.Items36"),
            resources.GetString("toolStripComboBoxProgNo.Items37"),
            resources.GetString("toolStripComboBoxProgNo.Items38"),
            resources.GetString("toolStripComboBoxProgNo.Items39"),
            resources.GetString("toolStripComboBoxProgNo.Items40"),
            resources.GetString("toolStripComboBoxProgNo.Items41"),
            resources.GetString("toolStripComboBoxProgNo.Items42"),
            resources.GetString("toolStripComboBoxProgNo.Items43"),
            resources.GetString("toolStripComboBoxProgNo.Items44"),
            resources.GetString("toolStripComboBoxProgNo.Items45"),
            resources.GetString("toolStripComboBoxProgNo.Items46"),
            resources.GetString("toolStripComboBoxProgNo.Items47"),
            resources.GetString("toolStripComboBoxProgNo.Items48"),
            resources.GetString("toolStripComboBoxProgNo.Items49"),
            resources.GetString("toolStripComboBoxProgNo.Items50"),
            resources.GetString("toolStripComboBoxProgNo.Items51"),
            resources.GetString("toolStripComboBoxProgNo.Items52"),
            resources.GetString("toolStripComboBoxProgNo.Items53"),
            resources.GetString("toolStripComboBoxProgNo.Items54"),
            resources.GetString("toolStripComboBoxProgNo.Items55"),
            resources.GetString("toolStripComboBoxProgNo.Items56"),
            resources.GetString("toolStripComboBoxProgNo.Items57"),
            resources.GetString("toolStripComboBoxProgNo.Items58"),
            resources.GetString("toolStripComboBoxProgNo.Items59"),
            resources.GetString("toolStripComboBoxProgNo.Items60"),
            resources.GetString("toolStripComboBoxProgNo.Items61"),
            resources.GetString("toolStripComboBoxProgNo.Items62"),
            resources.GetString("toolStripComboBoxProgNo.Items63"),
            resources.GetString("toolStripComboBoxProgNo.Items64"),
            resources.GetString("toolStripComboBoxProgNo.Items65"),
            resources.GetString("toolStripComboBoxProgNo.Items66"),
            resources.GetString("toolStripComboBoxProgNo.Items67"),
            resources.GetString("toolStripComboBoxProgNo.Items68"),
            resources.GetString("toolStripComboBoxProgNo.Items69"),
            resources.GetString("toolStripComboBoxProgNo.Items70"),
            resources.GetString("toolStripComboBoxProgNo.Items71"),
            resources.GetString("toolStripComboBoxProgNo.Items72"),
            resources.GetString("toolStripComboBoxProgNo.Items73"),
            resources.GetString("toolStripComboBoxProgNo.Items74"),
            resources.GetString("toolStripComboBoxProgNo.Items75"),
            resources.GetString("toolStripComboBoxProgNo.Items76"),
            resources.GetString("toolStripComboBoxProgNo.Items77"),
            resources.GetString("toolStripComboBoxProgNo.Items78"),
            resources.GetString("toolStripComboBoxProgNo.Items79"),
            resources.GetString("toolStripComboBoxProgNo.Items80"),
            resources.GetString("toolStripComboBoxProgNo.Items81"),
            resources.GetString("toolStripComboBoxProgNo.Items82"),
            resources.GetString("toolStripComboBoxProgNo.Items83"),
            resources.GetString("toolStripComboBoxProgNo.Items84"),
            resources.GetString("toolStripComboBoxProgNo.Items85"),
            resources.GetString("toolStripComboBoxProgNo.Items86"),
            resources.GetString("toolStripComboBoxProgNo.Items87"),
            resources.GetString("toolStripComboBoxProgNo.Items88"),
            resources.GetString("toolStripComboBoxProgNo.Items89"),
            resources.GetString("toolStripComboBoxProgNo.Items90"),
            resources.GetString("toolStripComboBoxProgNo.Items91"),
            resources.GetString("toolStripComboBoxProgNo.Items92"),
            resources.GetString("toolStripComboBoxProgNo.Items93"),
            resources.GetString("toolStripComboBoxProgNo.Items94"),
            resources.GetString("toolStripComboBoxProgNo.Items95"),
            resources.GetString("toolStripComboBoxProgNo.Items96"),
            resources.GetString("toolStripComboBoxProgNo.Items97"),
            resources.GetString("toolStripComboBoxProgNo.Items98"),
            resources.GetString("toolStripComboBoxProgNo.Items99"),
            resources.GetString("toolStripComboBoxProgNo.Items100"),
            resources.GetString("toolStripComboBoxProgNo.Items101"),
            resources.GetString("toolStripComboBoxProgNo.Items102"),
            resources.GetString("toolStripComboBoxProgNo.Items103"),
            resources.GetString("toolStripComboBoxProgNo.Items104"),
            resources.GetString("toolStripComboBoxProgNo.Items105"),
            resources.GetString("toolStripComboBoxProgNo.Items106"),
            resources.GetString("toolStripComboBoxProgNo.Items107"),
            resources.GetString("toolStripComboBoxProgNo.Items108"),
            resources.GetString("toolStripComboBoxProgNo.Items109"),
            resources.GetString("toolStripComboBoxProgNo.Items110"),
            resources.GetString("toolStripComboBoxProgNo.Items111"),
            resources.GetString("toolStripComboBoxProgNo.Items112"),
            resources.GetString("toolStripComboBoxProgNo.Items113"),
            resources.GetString("toolStripComboBoxProgNo.Items114"),
            resources.GetString("toolStripComboBoxProgNo.Items115"),
            resources.GetString("toolStripComboBoxProgNo.Items116"),
            resources.GetString("toolStripComboBoxProgNo.Items117"),
            resources.GetString("toolStripComboBoxProgNo.Items118"),
            resources.GetString("toolStripComboBoxProgNo.Items119"),
            resources.GetString("toolStripComboBoxProgNo.Items120"),
            resources.GetString("toolStripComboBoxProgNo.Items121"),
            resources.GetString("toolStripComboBoxProgNo.Items122"),
            resources.GetString("toolStripComboBoxProgNo.Items123"),
            resources.GetString("toolStripComboBoxProgNo.Items124"),
            resources.GetString("toolStripComboBoxProgNo.Items125"),
            resources.GetString("toolStripComboBoxProgNo.Items126"),
            resources.GetString("toolStripComboBoxProgNo.Items127"),
            resources.GetString("toolStripComboBoxProgNo.Items128")});
            this.toolStripComboBoxProgNo.Name = "toolStripComboBoxProgNo";
            this.toolStripComboBoxProgNo.SelectedIndexChanged += new System.EventHandler(this.toolStripComboBox2_SelectedIndexChanged);
            // 
            // toolStripLabel4
            // 
            this.toolStripLabel4.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripLabel4.Name = "toolStripLabel4";
            resources.ApplyResources(this.toolStripLabel4, "toolStripLabel4");
            // 
            // toolStripComboBoxKeyCh
            // 
            this.toolStripComboBoxKeyCh.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            resources.ApplyResources(this.toolStripComboBoxKeyCh, "toolStripComboBoxKeyCh");
            this.toolStripComboBoxKeyCh.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBoxKeyCh.DropDownWidth = 32;
            this.toolStripComboBoxKeyCh.Items.AddRange(new object[] {
            resources.GetString("toolStripComboBoxKeyCh.Items"),
            resources.GetString("toolStripComboBoxKeyCh.Items1"),
            resources.GetString("toolStripComboBoxKeyCh.Items2"),
            resources.GetString("toolStripComboBoxKeyCh.Items3"),
            resources.GetString("toolStripComboBoxKeyCh.Items4"),
            resources.GetString("toolStripComboBoxKeyCh.Items5"),
            resources.GetString("toolStripComboBoxKeyCh.Items6"),
            resources.GetString("toolStripComboBoxKeyCh.Items7"),
            resources.GetString("toolStripComboBoxKeyCh.Items8"),
            resources.GetString("toolStripComboBoxKeyCh.Items9"),
            resources.GetString("toolStripComboBoxKeyCh.Items10"),
            resources.GetString("toolStripComboBoxKeyCh.Items11"),
            resources.GetString("toolStripComboBoxKeyCh.Items12"),
            resources.GetString("toolStripComboBoxKeyCh.Items13"),
            resources.GetString("toolStripComboBoxKeyCh.Items14"),
            resources.GetString("toolStripComboBoxKeyCh.Items15"),
            resources.GetString("toolStripComboBoxKeyCh.Items16"),
            resources.GetString("toolStripComboBoxKeyCh.Items17"),
            resources.GetString("toolStripComboBoxKeyCh.Items18"),
            resources.GetString("toolStripComboBoxKeyCh.Items19"),
            resources.GetString("toolStripComboBoxKeyCh.Items20"),
            resources.GetString("toolStripComboBoxKeyCh.Items21"),
            resources.GetString("toolStripComboBoxKeyCh.Items22"),
            resources.GetString("toolStripComboBoxKeyCh.Items23"),
            resources.GetString("toolStripComboBoxKeyCh.Items24"),
            resources.GetString("toolStripComboBoxKeyCh.Items25"),
            resources.GetString("toolStripComboBoxKeyCh.Items26"),
            resources.GetString("toolStripComboBoxKeyCh.Items27"),
            resources.GetString("toolStripComboBoxKeyCh.Items28"),
            resources.GetString("toolStripComboBoxKeyCh.Items29"),
            resources.GetString("toolStripComboBoxKeyCh.Items30"),
            resources.GetString("toolStripComboBoxKeyCh.Items31")});
            this.toolStripComboBoxKeyCh.Name = "toolStripComboBoxKeyCh";
            // 
            // toolStripLabel5
            // 
            this.toolStripLabel5.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripLabel5.Name = "toolStripLabel5";
            resources.ApplyResources(this.toolStripLabel5, "toolStripLabel5");
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.Name = "toolStripLabel2";
            resources.ApplyResources(this.toolStripLabel2, "toolStripLabel2");
            // 
            // toolStripComboBoxCC
            // 
            resources.ApplyResources(this.toolStripComboBoxCC, "toolStripComboBoxCC");
            this.toolStripComboBoxCC.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBoxCC.DropDownWidth = 32;
            this.toolStripComboBoxCC.Items.AddRange(new object[] {
            resources.GetString("toolStripComboBoxCC.Items"),
            resources.GetString("toolStripComboBoxCC.Items1"),
            resources.GetString("toolStripComboBoxCC.Items2"),
            resources.GetString("toolStripComboBoxCC.Items3"),
            resources.GetString("toolStripComboBoxCC.Items4"),
            resources.GetString("toolStripComboBoxCC.Items5"),
            resources.GetString("toolStripComboBoxCC.Items6"),
            resources.GetString("toolStripComboBoxCC.Items7"),
            resources.GetString("toolStripComboBoxCC.Items8"),
            resources.GetString("toolStripComboBoxCC.Items9"),
            resources.GetString("toolStripComboBoxCC.Items10"),
            resources.GetString("toolStripComboBoxCC.Items11"),
            resources.GetString("toolStripComboBoxCC.Items12"),
            resources.GetString("toolStripComboBoxCC.Items13"),
            resources.GetString("toolStripComboBoxCC.Items14"),
            resources.GetString("toolStripComboBoxCC.Items15"),
            resources.GetString("toolStripComboBoxCC.Items16"),
            resources.GetString("toolStripComboBoxCC.Items17"),
            resources.GetString("toolStripComboBoxCC.Items18"),
            resources.GetString("toolStripComboBoxCC.Items19"),
            resources.GetString("toolStripComboBoxCC.Items20"),
            resources.GetString("toolStripComboBoxCC.Items21"),
            resources.GetString("toolStripComboBoxCC.Items22"),
            resources.GetString("toolStripComboBoxCC.Items23"),
            resources.GetString("toolStripComboBoxCC.Items24"),
            resources.GetString("toolStripComboBoxCC.Items25"),
            resources.GetString("toolStripComboBoxCC.Items26"),
            resources.GetString("toolStripComboBoxCC.Items27"),
            resources.GetString("toolStripComboBoxCC.Items28"),
            resources.GetString("toolStripComboBoxCC.Items29"),
            resources.GetString("toolStripComboBoxCC.Items30"),
            resources.GetString("toolStripComboBoxCC.Items31"),
            resources.GetString("toolStripComboBoxCC.Items32"),
            resources.GetString("toolStripComboBoxCC.Items33"),
            resources.GetString("toolStripComboBoxCC.Items34"),
            resources.GetString("toolStripComboBoxCC.Items35"),
            resources.GetString("toolStripComboBoxCC.Items36"),
            resources.GetString("toolStripComboBoxCC.Items37"),
            resources.GetString("toolStripComboBoxCC.Items38"),
            resources.GetString("toolStripComboBoxCC.Items39"),
            resources.GetString("toolStripComboBoxCC.Items40"),
            resources.GetString("toolStripComboBoxCC.Items41"),
            resources.GetString("toolStripComboBoxCC.Items42"),
            resources.GetString("toolStripComboBoxCC.Items43"),
            resources.GetString("toolStripComboBoxCC.Items44"),
            resources.GetString("toolStripComboBoxCC.Items45"),
            resources.GetString("toolStripComboBoxCC.Items46"),
            resources.GetString("toolStripComboBoxCC.Items47"),
            resources.GetString("toolStripComboBoxCC.Items48"),
            resources.GetString("toolStripComboBoxCC.Items49"),
            resources.GetString("toolStripComboBoxCC.Items50"),
            resources.GetString("toolStripComboBoxCC.Items51"),
            resources.GetString("toolStripComboBoxCC.Items52"),
            resources.GetString("toolStripComboBoxCC.Items53"),
            resources.GetString("toolStripComboBoxCC.Items54"),
            resources.GetString("toolStripComboBoxCC.Items55"),
            resources.GetString("toolStripComboBoxCC.Items56"),
            resources.GetString("toolStripComboBoxCC.Items57"),
            resources.GetString("toolStripComboBoxCC.Items58"),
            resources.GetString("toolStripComboBoxCC.Items59"),
            resources.GetString("toolStripComboBoxCC.Items60"),
            resources.GetString("toolStripComboBoxCC.Items61"),
            resources.GetString("toolStripComboBoxCC.Items62"),
            resources.GetString("toolStripComboBoxCC.Items63"),
            resources.GetString("toolStripComboBoxCC.Items64"),
            resources.GetString("toolStripComboBoxCC.Items65"),
            resources.GetString("toolStripComboBoxCC.Items66"),
            resources.GetString("toolStripComboBoxCC.Items67"),
            resources.GetString("toolStripComboBoxCC.Items68"),
            resources.GetString("toolStripComboBoxCC.Items69"),
            resources.GetString("toolStripComboBoxCC.Items70"),
            resources.GetString("toolStripComboBoxCC.Items71"),
            resources.GetString("toolStripComboBoxCC.Items72"),
            resources.GetString("toolStripComboBoxCC.Items73"),
            resources.GetString("toolStripComboBoxCC.Items74"),
            resources.GetString("toolStripComboBoxCC.Items75"),
            resources.GetString("toolStripComboBoxCC.Items76"),
            resources.GetString("toolStripComboBoxCC.Items77"),
            resources.GetString("toolStripComboBoxCC.Items78"),
            resources.GetString("toolStripComboBoxCC.Items79"),
            resources.GetString("toolStripComboBoxCC.Items80"),
            resources.GetString("toolStripComboBoxCC.Items81"),
            resources.GetString("toolStripComboBoxCC.Items82"),
            resources.GetString("toolStripComboBoxCC.Items83"),
            resources.GetString("toolStripComboBoxCC.Items84"),
            resources.GetString("toolStripComboBoxCC.Items85"),
            resources.GetString("toolStripComboBoxCC.Items86"),
            resources.GetString("toolStripComboBoxCC.Items87"),
            resources.GetString("toolStripComboBoxCC.Items88"),
            resources.GetString("toolStripComboBoxCC.Items89"),
            resources.GetString("toolStripComboBoxCC.Items90"),
            resources.GetString("toolStripComboBoxCC.Items91"),
            resources.GetString("toolStripComboBoxCC.Items92"),
            resources.GetString("toolStripComboBoxCC.Items93"),
            resources.GetString("toolStripComboBoxCC.Items94"),
            resources.GetString("toolStripComboBoxCC.Items95"),
            resources.GetString("toolStripComboBoxCC.Items96"),
            resources.GetString("toolStripComboBoxCC.Items97"),
            resources.GetString("toolStripComboBoxCC.Items98"),
            resources.GetString("toolStripComboBoxCC.Items99"),
            resources.GetString("toolStripComboBoxCC.Items100"),
            resources.GetString("toolStripComboBoxCC.Items101"),
            resources.GetString("toolStripComboBoxCC.Items102"),
            resources.GetString("toolStripComboBoxCC.Items103"),
            resources.GetString("toolStripComboBoxCC.Items104"),
            resources.GetString("toolStripComboBoxCC.Items105"),
            resources.GetString("toolStripComboBoxCC.Items106"),
            resources.GetString("toolStripComboBoxCC.Items107"),
            resources.GetString("toolStripComboBoxCC.Items108"),
            resources.GetString("toolStripComboBoxCC.Items109"),
            resources.GetString("toolStripComboBoxCC.Items110"),
            resources.GetString("toolStripComboBoxCC.Items111"),
            resources.GetString("toolStripComboBoxCC.Items112"),
            resources.GetString("toolStripComboBoxCC.Items113"),
            resources.GetString("toolStripComboBoxCC.Items114"),
            resources.GetString("toolStripComboBoxCC.Items115"),
            resources.GetString("toolStripComboBoxCC.Items116"),
            resources.GetString("toolStripComboBoxCC.Items117"),
            resources.GetString("toolStripComboBoxCC.Items118"),
            resources.GetString("toolStripComboBoxCC.Items119"),
            resources.GetString("toolStripComboBoxCC.Items120"),
            resources.GetString("toolStripComboBoxCC.Items121"),
            resources.GetString("toolStripComboBoxCC.Items122"),
            resources.GetString("toolStripComboBoxCC.Items123"),
            resources.GetString("toolStripComboBoxCC.Items124"),
            resources.GetString("toolStripComboBoxCC.Items125"),
            resources.GetString("toolStripComboBoxCC.Items126"),
            resources.GetString("toolStripComboBoxCC.Items127")});
            this.toolStripComboBoxCC.Name = "toolStripComboBoxCC";
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.panel1);
            this.tabPage4.Controls.Add(this.toolStrip4);
            resources.ApplyResources(this.tabPage4, "tabPage4");
            this.tabPage4.Name = "tabPage4";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(115)))), ((int)(((byte)(63)))), ((int)(((byte)(0)))));
            this.panel1.Controls.Add(this.tableLayoutPanel1);
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(249)))), ((int)(((byte)(146)))), ((int)(((byte)(0)))));
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.metroTrackBarVol, 4, 1);
            this.tableLayoutPanel1.Controls.Add(this.label1, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.labelClock, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.panelOsc2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.labelTitle, 4, 0);
            this.tableLayoutPanel1.Controls.Add(this.labelStat, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.label2, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.label3, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.label4, 6, 1);
            this.tableLayoutPanel1.Controls.Add(this.labelCpuLoad, 7, 1);
            this.tableLayoutPanel1.Controls.Add(this.panelChDisp, 5, 1);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // metroTrackBarVol
            // 
            resources.ApplyResources(this.metroTrackBarVol, "metroTrackBarVol");
            this.metroTrackBarVol.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(229)))), ((int)(((byte)(126)))), ((int)(((byte)(0)))));
            this.metroTrackBarVol.DataBindings.Add(new System.Windows.Forms.Binding("Value", global::zanac.MAmidiMEmo.Properties.Settings.Default, "MasterVolume", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.metroTrackBarVol.Name = "metroTrackBarVol";
            this.metroTrackBarVol.UseCustomBackColor = true;
            this.metroTrackBarVol.Value = global::zanac.MAmidiMEmo.Properties.Settings.Default.MasterVolume;
            this.metroTrackBarVol.ValueChanged += new System.EventHandler(this.metroTrackBar1_ValueChanged);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(115)))), ((int)(((byte)(63)))), ((int)(((byte)(0)))));
            this.label1.Name = "label1";
            // 
            // labelClock
            // 
            resources.ApplyResources(this.labelClock, "labelClock");
            this.labelClock.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(229)))), ((int)(((byte)(126)))), ((int)(((byte)(0)))));
            this.labelClock.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(115)))), ((int)(((byte)(63)))), ((int)(((byte)(0)))));
            this.labelClock.Name = "labelClock";
            this.labelClock.UseCompatibleTextRendering = true;
            // 
            // panelOsc2
            // 
            resources.ApplyResources(this.panelOsc2, "panelOsc2");
            this.panelOsc2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(229)))), ((int)(((byte)(126)))), ((int)(((byte)(0)))));
            this.tableLayoutPanel1.SetColumnSpan(this.panelOsc2, 3);
            this.panelOsc2.Name = "panelOsc2";
            this.panelOsc2.Paint += new System.Windows.Forms.PaintEventHandler(this.panelOsc2_Paint);
            // 
            // labelTitle
            // 
            this.labelTitle.AllowDrop = true;
            this.labelTitle.AutoEllipsis = true;
            this.labelTitle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(229)))), ((int)(((byte)(126)))), ((int)(((byte)(0)))));
            this.tableLayoutPanel1.SetColumnSpan(this.labelTitle, 4);
            resources.ApplyResources(this.labelTitle, "labelTitle");
            this.labelTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(115)))), ((int)(((byte)(63)))), ((int)(((byte)(0)))));
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.UseMnemonic = false;
            this.labelTitle.DragDrop += new System.Windows.Forms.DragEventHandler(this.labelTitle_DragDrop);
            this.labelTitle.DragEnter += new System.Windows.Forms.DragEventHandler(this.labelTitle_DragEnter);
            // 
            // labelStat
            // 
            resources.ApplyResources(this.labelStat, "labelStat");
            this.labelStat.Image = global::zanac.MAmidiMEmo.Properties.Resources.Stop;
            this.labelStat.Name = "labelStat";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(115)))), ((int)(((byte)(63)))), ((int)(((byte)(0)))));
            this.label2.Name = "label2";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(115)))), ((int)(((byte)(63)))), ((int)(((byte)(0)))));
            this.label3.Name = "label3";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(115)))), ((int)(((byte)(63)))), ((int)(((byte)(0)))));
            this.label4.Name = "label4";
            // 
            // labelCpuLoad
            // 
            resources.ApplyResources(this.labelCpuLoad, "labelCpuLoad");
            this.labelCpuLoad.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(229)))), ((int)(((byte)(126)))), ((int)(((byte)(0)))));
            this.labelCpuLoad.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(115)))), ((int)(((byte)(63)))), ((int)(((byte)(0)))));
            this.labelCpuLoad.Name = "labelCpuLoad";
            this.labelCpuLoad.UseCompatibleTextRendering = true;
            // 
            // panelChDisp
            // 
            this.panelChDisp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(229)))), ((int)(((byte)(126)))), ((int)(((byte)(0)))));
            resources.ApplyResources(this.panelChDisp, "panelChDisp");
            this.panelChDisp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(115)))), ((int)(((byte)(63)))), ((int)(((byte)(0)))));
            this.panelChDisp.Name = "panelChDisp";
            this.panelChDisp.Paint += new System.Windows.Forms.PaintEventHandler(this.panelChDisp_Paint);
            // 
            // toolStrip4
            // 
            this.toolStrip4.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip4.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonPlay,
            this.toolStripButtonPause,
            this.toolStripButtonStop,
            this.toolStripButtonPrev,
            this.toolStripButtonNext,
            this.toolStripButtonOpen,
            this.toolStripButtonAutoVGM,
            this.toolStripButtonAutoWav,
            this.toolStripButtonReload});
            resources.ApplyResources(this.toolStrip4, "toolStrip4");
            this.toolStrip4.Name = "toolStrip4";
            // 
            // toolStripButtonPlay
            // 
            this.toolStripButtonPlay.Image = global::zanac.MAmidiMEmo.Properties.Resources.Play;
            resources.ApplyResources(this.toolStripButtonPlay, "toolStripButtonPlay");
            this.toolStripButtonPlay.Name = "toolStripButtonPlay";
            this.toolStripButtonPlay.Click += new System.EventHandler(this.toolStripButtonPlay_Click);
            // 
            // toolStripButtonPause
            // 
            this.toolStripButtonPause.Image = global::zanac.MAmidiMEmo.Properties.Resources.Pause;
            resources.ApplyResources(this.toolStripButtonPause, "toolStripButtonPause");
            this.toolStripButtonPause.Name = "toolStripButtonPause";
            this.toolStripButtonPause.Click += new System.EventHandler(this.toolStripButtonPause_Click);
            // 
            // toolStripButtonStop
            // 
            this.toolStripButtonStop.Image = global::zanac.MAmidiMEmo.Properties.Resources.Stop;
            resources.ApplyResources(this.toolStripButtonStop, "toolStripButtonStop");
            this.toolStripButtonStop.Name = "toolStripButtonStop";
            this.toolStripButtonStop.Click += new System.EventHandler(this.toolStripButtonStop_Click);
            // 
            // toolStripButtonPrev
            // 
            this.toolStripButtonPrev.Image = global::zanac.MAmidiMEmo.Properties.Resources.Prev;
            resources.ApplyResources(this.toolStripButtonPrev, "toolStripButtonPrev");
            this.toolStripButtonPrev.Name = "toolStripButtonPrev";
            this.toolStripButtonPrev.Click += new System.EventHandler(this.toolStripButton22_Click);
            // 
            // toolStripButtonNext
            // 
            this.toolStripButtonNext.Image = global::zanac.MAmidiMEmo.Properties.Resources.Next;
            resources.ApplyResources(this.toolStripButtonNext, "toolStripButtonNext");
            this.toolStripButtonNext.Name = "toolStripButtonNext";
            this.toolStripButtonNext.Click += new System.EventHandler(this.toolStripButton23_Click);
            // 
            // toolStripButtonOpen
            // 
            this.toolStripButtonOpen.Image = global::zanac.MAmidiMEmo.Properties.Resources.Open;
            resources.ApplyResources(this.toolStripButtonOpen, "toolStripButtonOpen");
            this.toolStripButtonOpen.Name = "toolStripButtonOpen";
            this.toolStripButtonOpen.Click += new System.EventHandler(this.toolStripButtonOpen_Click);
            // 
            // toolStripButtonAutoVGM
            // 
            this.toolStripButtonAutoVGM.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButtonAutoVGM.Checked = global::zanac.MAmidiMEmo.Properties.Settings.Default.AutoReload;
            this.toolStripButtonAutoVGM.CheckOnClick = true;
            this.toolStripButtonAutoVGM.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButtonAutoVGM.Name = "toolStripButtonAutoVGM";
            resources.ApplyResources(this.toolStripButtonAutoVGM, "toolStripButtonAutoVGM");
            // 
            // toolStripButtonAutoWav
            // 
            this.toolStripButtonAutoWav.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButtonAutoWav.Checked = global::zanac.MAmidiMEmo.Properties.Settings.Default.AutoReload;
            this.toolStripButtonAutoWav.CheckOnClick = true;
            this.toolStripButtonAutoWav.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButtonAutoWav.Name = "toolStripButtonAutoWav";
            resources.ApplyResources(this.toolStripButtonAutoWav, "toolStripButtonAutoWav");
            // 
            // toolStripButtonReload
            // 
            this.toolStripButtonReload.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButtonReload.Checked = global::zanac.MAmidiMEmo.Properties.Settings.Default.AutoReload;
            this.toolStripButtonReload.CheckOnClick = true;
            this.toolStripButtonReload.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            resources.ApplyResources(this.toolStripButtonReload, "toolStripButtonReload");
            this.toolStripButtonReload.Name = "toolStripButtonReload";
            this.toolStripButtonReload.CheckStateChanged += new System.EventHandler(this.toolStripButtonReload_CheckStateChanged);
            // 
            // tabPage5
            // 
            this.tabPage5.Controls.Add(this.draggableListViewMediaList);
            resources.ApplyResources(this.tabPage5, "tabPage5");
            this.tabPage5.Name = "tabPage5";
            // 
            // draggableListViewMediaList
            // 
            this.draggableListViewMediaList.AllowDrop = true;
            this.draggableListViewMediaList.AllowItemDrag = true;
            this.draggableListViewMediaList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderFname});
            this.draggableListViewMediaList.ContextMenuStrip = this.contextMenuStripList;
            resources.ApplyResources(this.draggableListViewMediaList, "draggableListViewMediaList");
            this.draggableListViewMediaList.FullRowSelect = true;
            this.draggableListViewMediaList.GridLines = true;
            this.draggableListViewMediaList.HideSelection = false;
            this.draggableListViewMediaList.Name = "draggableListViewMediaList";
            this.draggableListViewMediaList.UseCompatibleStateImageBehavior = false;
            this.draggableListViewMediaList.View = System.Windows.Forms.View.Details;
            this.draggableListViewMediaList.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.draggableListView1_ColumnClick);
            this.draggableListViewMediaList.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.draggableListView1_ItemDrag);
            this.draggableListViewMediaList.SelectedIndexChanged += new System.EventHandler(this.draggableListView1_SelectedIndexChanged);
            this.draggableListViewMediaList.SizeChanged += new System.EventHandler(this.draggableListView1_SizeChanged);
            this.draggableListViewMediaList.DragDrop += new System.Windows.Forms.DragEventHandler(this.draggableListView1_DragDrop);
            this.draggableListViewMediaList.DragEnter += new System.Windows.Forms.DragEventHandler(this.draggableListView1_DragEnter);
            this.draggableListViewMediaList.DragOver += new System.Windows.Forms.DragEventHandler(this.draggableListView1_DragOver);
            this.draggableListViewMediaList.DragLeave += new System.EventHandler(this.draggableListView1_DragLeave);
            this.draggableListViewMediaList.DoubleClick += new System.EventHandler(this.draggableListView1_DoubleClick);
            this.draggableListViewMediaList.KeyDown += new System.Windows.Forms.KeyEventHandler(this.draggableListView1_KeyDown);
            // 
            // columnHeaderFname
            // 
            resources.ApplyResources(this.columnHeaderFname, "columnHeaderFname");
            // 
            // contextMenuStripList
            // 
            this.contextMenuStripList.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStripList.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.playToolStripMenuItem,
            this.removeToolStripMenuItem,
            this.explorerToolStripMenuItem});
            this.contextMenuStripList.Name = "contextMenuStrip1";
            resources.ApplyResources(this.contextMenuStripList, "contextMenuStripList");
            // 
            // playToolStripMenuItem
            // 
            this.playToolStripMenuItem.Name = "playToolStripMenuItem";
            resources.ApplyResources(this.playToolStripMenuItem, "playToolStripMenuItem");
            this.playToolStripMenuItem.Click += new System.EventHandler(this.playToolStripMenuItem_Click);
            // 
            // removeToolStripMenuItem
            // 
            this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            resources.ApplyResources(this.removeToolStripMenuItem, "removeToolStripMenuItem");
            this.removeToolStripMenuItem.Click += new System.EventHandler(this.removeToolStripMenuItem_Click);
            // 
            // explorerToolStripMenuItem
            // 
            this.explorerToolStripMenuItem.Name = "explorerToolStripMenuItem";
            resources.ApplyResources(this.explorerToolStripMenuItem, "explorerToolStripMenuItem");
            this.explorerToolStripMenuItem.Click += new System.EventHandler(this.explorerToolStripMenuItem_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.listViewOutput);
            resources.ApplyResources(this.tabPage2, "tabPage2");
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // listViewOutput
            // 
            this.listViewOutput.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            resources.ApplyResources(this.listViewOutput, "listViewOutput");
            this.listViewOutput.FullRowSelect = true;
            this.listViewOutput.GridLines = true;
            this.listViewOutput.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listViewOutput.HideSelection = false;
            this.listViewOutput.Name = "listViewOutput";
            this.listViewOutput.UseCompatibleStateImageBehavior = false;
            this.listViewOutput.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            resources.ApplyResources(this.columnHeader1, "columnHeader1");
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
            this.openSampleFolderToolStripMenuItem,
            this.toolStripSeparator4,
            this.loadToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.toolStripSeparator3,
            this.saveAsToolStripMenuItem,
            this.exportMAmidiToolStripMenuItem,
            this.toolStripSeparator5,
            this.toolStripMenuItemExit});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            resources.ApplyResources(this.fileToolStripMenuItem, "fileToolStripMenuItem");
            // 
            // openSampleFolderToolStripMenuItem
            // 
            this.openSampleFolderToolStripMenuItem.Name = "openSampleFolderToolStripMenuItem";
            resources.ApplyResources(this.openSampleFolderToolStripMenuItem, "openSampleFolderToolStripMenuItem");
            this.openSampleFolderToolStripMenuItem.Click += new System.EventHandler(this.openSampleFolderToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            resources.ApplyResources(this.toolStripSeparator4, "toolStripSeparator4");
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
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            resources.ApplyResources(this.toolStripSeparator3, "toolStripSeparator3");
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            resources.ApplyResources(this.saveAsToolStripMenuItem, "saveAsToolStripMenuItem");
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // exportMAmidiToolStripMenuItem
            // 
            this.exportMAmidiToolStripMenuItem.Name = "exportMAmidiToolStripMenuItem";
            resources.ApplyResources(this.exportMAmidiToolStripMenuItem, "exportMAmidiToolStripMenuItem");
            this.exportMAmidiToolStripMenuItem.Click += new System.EventHandler(this.exportMAmidiToolStripMenuItem_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            resources.ApplyResources(this.toolStripSeparator5, "toolStripSeparator5");
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
            this.copyMAmiVSTiToolStripMenuItem,
            this.mIDIDelayCheckerToolStripMenuItem,
            this.toolStripSeparator6,
            this.settingsToolStripMenuItem});
            this.toolToolStripMenuItem.Name = "toolToolStripMenuItem";
            resources.ApplyResources(this.toolToolStripMenuItem, "toolToolStripMenuItem");
            // 
            // copyMAmiVSTiToolStripMenuItem
            // 
            this.copyMAmiVSTiToolStripMenuItem.Name = "copyMAmiVSTiToolStripMenuItem";
            resources.ApplyResources(this.copyMAmiVSTiToolStripMenuItem, "copyMAmiVSTiToolStripMenuItem");
            this.copyMAmiVSTiToolStripMenuItem.Click += new System.EventHandler(this.copyMAmiVSTiToolStripMenuItem_Click);
            // 
            // mIDIDelayCheckerToolStripMenuItem
            // 
            this.mIDIDelayCheckerToolStripMenuItem.Name = "mIDIDelayCheckerToolStripMenuItem";
            resources.ApplyResources(this.mIDIDelayCheckerToolStripMenuItem, "mIDIDelayCheckerToolStripMenuItem");
            this.mIDIDelayCheckerToolStripMenuItem.Click += new System.EventHandler(this.mIDIDelayCheckerToolStripMenuItem_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            resources.ApplyResources(this.toolStripSeparator6, "toolStripSeparator6");
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
            this.helpToolStripMenuItem1,
            this.toolStripMenuItemAbout});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            resources.ApplyResources(this.helpToolStripMenuItem, "helpToolStripMenuItem");
            // 
            // helpToolStripMenuItem1
            // 
            this.helpToolStripMenuItem1.Name = "helpToolStripMenuItem1";
            resources.ApplyResources(this.helpToolStripMenuItem1, "helpToolStripMenuItem1");
            this.helpToolStripMenuItem1.Click += new System.EventHandler(this.helpToolStripMenuItem1_Click);
            // 
            // toolStripMenuItemAbout
            // 
            this.toolStripMenuItemAbout.Name = "toolStripMenuItemAbout";
            resources.ApplyResources(this.toolStripMenuItemAbout, "toolStripMenuItemAbout");
            this.toolStripMenuItemAbout.Click += new System.EventHandler(this.toolStripMenuItemAbout_Click);
            // 
            // saveFileDialogMami
            // 
            this.saveFileDialogMami.DefaultExt = "MAmi";
            this.saveFileDialogMami.FileName = "MyEnvironment";
            resources.ApplyResources(this.saveFileDialogMami, "saveFileDialogMami");
            this.saveFileDialogMami.SupportMultiDottedExtensions = true;
            // 
            // openFileDialogMami
            // 
            this.openFileDialogMami.DefaultExt = "*.MAmi";
            this.openFileDialogMami.FileName = "MyEnvironment.MAmi";
            resources.ApplyResources(this.openFileDialogMami, "openFileDialogMami");
            this.openFileDialogMami.SupportMultiDottedExtensions = true;
            // 
            // timerOsc
            // 
            this.timerOsc.Interval = 50;
            this.timerOsc.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // toolStrip1
            // 
            this.toolStrip1.ClickThrough = false;
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(609, 609);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.toolStripComboBoxMidiIfA,
            this.toolStripLabel6,
            this.toolStripComboBoxMidiIfB,
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
            // toolStripComboBoxMidiIfA
            // 
            this.toolStripComboBoxMidiIfA.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBoxMidiIfA.Name = "toolStripComboBoxMidiIfA";
            resources.ApplyResources(this.toolStripComboBoxMidiIfA, "toolStripComboBoxMidiIfA");
            this.toolStripComboBoxMidiIfA.DropDown += new System.EventHandler(this.toolStripComboBoxMidiIfA_DropDown);
            this.toolStripComboBoxMidiIfA.SelectedIndexChanged += new System.EventHandler(this.toolStripComboBoxMidiIfA_SelectedIndexChanged);
            // 
            // toolStripLabel6
            // 
            this.toolStripLabel6.Name = "toolStripLabel6";
            resources.ApplyResources(this.toolStripLabel6, "toolStripLabel6");
            // 
            // toolStripComboBoxMidiIfB
            // 
            this.toolStripComboBoxMidiIfB.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBoxMidiIfB.Name = "toolStripComboBoxMidiIfB";
            resources.ApplyResources(this.toolStripComboBoxMidiIfB, "toolStripComboBoxMidiIfB");
            this.toolStripComboBoxMidiIfB.DropDown += new System.EventHandler(this.toolStripComboBoxMidiIfB_DropDown);
            this.toolStripComboBoxMidiIfB.SelectedIndexChanged += new System.EventHandler(this.toolStripComboBoxMidiIfB_SelectedIndexChanged);
            // 
            // toolStripDropDownButton1
            // 
            this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lAToolStripMenuItem,
            this.pCMToolStripMenuItem,
            this.fMSynthesisToolStripMenuItem,
            this.wSGToolStripMenuItem,
            this.pSGToolStripMenuItem,
            this.eTCToolStripMenuItem,
            this.dISCRETEToolStripMenuItem});
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
            this.toolStripMenuItem2608,
            this.addYM2151ToolStripMenuItem,
            this.addYM2612ToolStripMenuItem,
            this.extendYM3812ToolStripMenuItem,
            this.extendYM2413ToolStripMenuItem,
            this.extendYMF262ToolStripMenuItem,
            this.yM2414ToolStripMenuItem,
            this.yM3806OPQToolStripMenuItem});
            this.fMSynthesisToolStripMenuItem.Name = "fMSynthesisToolStripMenuItem";
            resources.ApplyResources(this.fMSynthesisToolStripMenuItem, "fMSynthesisToolStripMenuItem");
            // 
            // extendYM2610BToolStripMenuItem
            // 
            this.extendYM2610BToolStripMenuItem.Name = "extendYM2610BToolStripMenuItem";
            resources.ApplyResources(this.extendYM2610BToolStripMenuItem, "extendYM2610BToolStripMenuItem");
            this.extendYM2610BToolStripMenuItem.Click += new System.EventHandler(this.extendYM2610BToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2608
            // 
            this.toolStripMenuItem2608.Name = "toolStripMenuItem2608";
            resources.ApplyResources(this.toolStripMenuItem2608, "toolStripMenuItem2608");
            this.toolStripMenuItem2608.Click += new System.EventHandler(this.extendYM2608ToolStripMenuItem_Click);
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
            // extendYMF262ToolStripMenuItem
            // 
            this.extendYMF262ToolStripMenuItem.Name = "extendYMF262ToolStripMenuItem";
            resources.ApplyResources(this.extendYMF262ToolStripMenuItem, "extendYMF262ToolStripMenuItem");
            this.extendYMF262ToolStripMenuItem.Click += new System.EventHandler(this.extendYMF262ToolStripMenuItem_Click);
            // 
            // yM2414ToolStripMenuItem
            // 
            this.yM2414ToolStripMenuItem.Name = "yM2414ToolStripMenuItem";
            resources.ApplyResources(this.yM2414ToolStripMenuItem, "yM2414ToolStripMenuItem");
            this.yM2414ToolStripMenuItem.Click += new System.EventHandler(this.yM2414ToolStripMenuItem_Click);
            // 
            // yM3806OPQToolStripMenuItem
            // 
            this.yM3806OPQToolStripMenuItem.Name = "yM3806OPQToolStripMenuItem";
            resources.ApplyResources(this.yM3806OPQToolStripMenuItem, "yM3806OPQToolStripMenuItem");
            this.yM3806OPQToolStripMenuItem.Click += new System.EventHandler(this.yM3806OPQToolStripMenuItem_Click);
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
            this.uPD1771ToolStripMenuItem,
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
            // uPD1771ToolStripMenuItem
            // 
            this.uPD1771ToolStripMenuItem.Name = "uPD1771ToolStripMenuItem";
            resources.ApplyResources(this.uPD1771ToolStripMenuItem, "uPD1771ToolStripMenuItem");
            this.uPD1771ToolStripMenuItem.Click += new System.EventHandler(this.uPD1771ToolStripMenuItem_Click);
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
            // eTCToolStripMenuItem
            // 
            this.eTCToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sP0256ToolStripMenuItem,
            this.extendSP0256AL2ToolStripMenuItem,
            this.extendSAMToolStripMenuItem});
            this.eTCToolStripMenuItem.Name = "eTCToolStripMenuItem";
            resources.ApplyResources(this.eTCToolStripMenuItem, "eTCToolStripMenuItem");
            // 
            // sP0256ToolStripMenuItem
            // 
            this.sP0256ToolStripMenuItem.Name = "sP0256ToolStripMenuItem";
            resources.ApplyResources(this.sP0256ToolStripMenuItem, "sP0256ToolStripMenuItem");
            this.sP0256ToolStripMenuItem.Click += new System.EventHandler(this.tms5220ToolStripMenuItem_Click);
            // 
            // extendSP0256AL2ToolStripMenuItem
            // 
            this.extendSP0256AL2ToolStripMenuItem.Name = "extendSP0256AL2ToolStripMenuItem";
            resources.ApplyResources(this.extendSP0256AL2ToolStripMenuItem, "extendSP0256AL2ToolStripMenuItem");
            this.extendSP0256AL2ToolStripMenuItem.Click += new System.EventHandler(this.extendSP0256AL2ToolStripMenuItem_Click);
            // 
            // extendSAMToolStripMenuItem
            // 
            this.extendSAMToolStripMenuItem.Name = "extendSAMToolStripMenuItem";
            resources.ApplyResources(this.extendSAMToolStripMenuItem, "extendSAMToolStripMenuItem");
            this.extendSAMToolStripMenuItem.Click += new System.EventHandler(this.extendSAMToolStripMenuItem_Click);
            // 
            // dISCRETEToolStripMenuItem
            // 
            this.dISCRETEToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sN76477ToolStripMenuItem});
            this.dISCRETEToolStripMenuItem.Name = "dISCRETEToolStripMenuItem";
            resources.ApplyResources(this.dISCRETEToolStripMenuItem, "dISCRETEToolStripMenuItem");
            // 
            // sN76477ToolStripMenuItem
            // 
            this.sN76477ToolStripMenuItem.Name = "sN76477ToolStripMenuItem";
            resources.ApplyResources(this.sN76477ToolStripMenuItem, "sN76477ToolStripMenuItem");
            this.sN76477ToolStripMenuItem.Click += new System.EventHandler(this.sN76477ToolStripMenuItem_Click);
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
            // openFileDialogMidi
            // 
            this.openFileDialogMidi.DefaultExt = "*.mid";
            resources.ApplyResources(this.openFileDialogMidi, "openFileDialogMidi");
            this.openFileDialogMidi.SupportMultiDottedExtensions = true;
            // 
            // fileSystemWatcherMidi
            // 
            this.fileSystemWatcherMidi.EnableRaisingEvents = true;
            this.fileSystemWatcherMidi.SynchronizingObject = this;
            this.fileSystemWatcherMidi.Changed += new System.IO.FileSystemEventHandler(this.fileSystemWatcher1_Changed);
            // 
            // timerReload
            // 
            this.timerReload.Tick += new System.EventHandler(this.timerReload_Tick);
            // 
            // saveFileDialogMAmidi
            // 
            this.saveFileDialogMAmidi.DefaultExt = "MAmi";
            this.saveFileDialogMAmidi.FileName = "MyEnvAndMidi";
            resources.ApplyResources(this.saveFileDialogMAmidi, "saveFileDialogMAmidi");
            this.saveFileDialogMAmidi.SupportMultiDottedExtensions = true;
            // 
            // betterFolderBrowserVSTi
            // 
            this.betterFolderBrowserVSTi.Multiselect = false;
            this.betterFolderBrowserVSTi.RootFolder = ".\\";
            this.betterFolderBrowserVSTi.Title = "";
            // 
            // FormMain
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.toolStrip1);
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
            this.tabControlBottom.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.toolStrip2.ResumeLayout(false);
            this.toolStrip2.PerformLayout();
            this.tabPage4.ResumeLayout(false);
            this.tabPage4.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.toolStrip4.ResumeLayout(false);
            this.toolStrip4.PerformLayout();
            this.tabPage5.ResumeLayout(false);
            this.contextMenuStripList.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fileSystemWatcherMidi)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ListView listViewOutput;
        private System.Windows.Forms.ColumnHeader columnHeader1;
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
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxMidiIfA;
        private System.Windows.Forms.PropertyGrid propertyGrid;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ImageList imageList1;
        private MetroFramework.Controls.MetroContextMenu contextMenuStripInst;
        private System.Windows.Forms.ToolStripMenuItem decreaseThisKindOfChipToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog saveFileDialogMami;
        private System.Windows.Forms.OpenFileDialog openFileDialogMami;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripMenuItem toolToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton toolStripButton2;
        private MetroFramework.Controls.MetroTabControl tabControlBottom;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Timer timerOsc;
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
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxKeyCh;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ToolStripLabel toolStripLabel3;
        private ComponentModel.ToolStripBase toolStrip3;
        private System.Windows.Forms.ToolStripButton toolStripButtonCat;
        private System.Windows.Forms.ToolStripButton toolStripButtonA2Z;
        private System.Windows.Forms.ToolStripButton toolStripButtonPopup;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxProgNo;
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
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItemSep;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSepInst;
        private System.Windows.Forms.ToolStripMenuItem extendYMF262ToolStripMenuItem;
        private System.Windows.Forms.ToolStripLabel toolStripLabel5;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxCC;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.OpenFileDialog openFileDialogMidi;
        private MetroFramework.Controls.MetroLabel metroLabelDrop;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelClock;
        private System.Windows.Forms.Panel panelOsc2;
        private System.Windows.Forms.Label labelStat;
        private System.IO.FileSystemWatcher fileSystemWatcherMidi;
        private System.Windows.Forms.Timer timerReload;
        private Gui.WrapLabel labelTitle;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private MetroFramework.Controls.MetroTrackBar metroTrackBarVol;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label labelCpuLoad;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ToolStripMenuItem exportMAmidiToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog saveFileDialogMAmidi;
        private System.Windows.Forms.Panel panelChDisp;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxMidiIfB;
        private System.Windows.Forms.ToolStripLabel toolStripLabel6;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxPort;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2608;
        private System.Windows.Forms.ToolStripMenuItem eTCToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sP0256ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extendSP0256AL2ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem extendSAMToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dISCRETEToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sN76477ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem uPD1771ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem yM2414ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openSampleFolderToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem copyMAmiVSTiToolStripMenuItem;
        private WK.Libraries.BetterFolderBrowserNS.BetterFolderBrowser betterFolderBrowserVSTi;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private ClickThroughToolStrip toolStrip4;
        private System.Windows.Forms.ToolStripButton toolStripButtonPlay;
        private System.Windows.Forms.ToolStripButton toolStripButtonPause;
        private System.Windows.Forms.ToolStripButton toolStripButtonStop;
        private System.Windows.Forms.ToolStripButton toolStripButtonOpen;
        private System.Windows.Forms.ToolStripButton toolStripButtonReload;
        private System.Windows.Forms.ToolStripButton toolStripButtonAutoVGM;
        private System.Windows.Forms.ToolStripButton toolStripButtonAutoWav;
        private System.Windows.Forms.TabPage tabPage5;
        private System.Windows.Forms.ColumnHeader columnHeaderFile;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private ListViewInsertionDrag.DraggableListView draggableListViewMediaList;
        private System.Windows.Forms.ColumnHeader columnHeaderFname;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripList;
        private System.Windows.Forms.ToolStripMenuItem playToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem explorerToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton toolStripButtonPrev;
        private System.Windows.Forms.ToolStripButton toolStripButtonNext;
        private System.Windows.Forms.ToolStripMenuItem yM3806OPQToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mIDIDelayCheckerToolStripMenuItem;
    }
}

