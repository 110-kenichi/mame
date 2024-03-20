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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormTimbreManager));
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
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.propertyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.checkBoxSetDrumKeyNo = new System.Windows.Forms.CheckBox();
            this.comboBoxDrumKeyNo = new System.Windows.Forms.ComboBox();
            this.checkBoxSetDrumTimbreName = new System.Windows.Forms.CheckBox();
            this.betterFolderBrowser1 = new WK.Libraries.BetterFolderBrowserNS.BetterFolderBrowser(this.components);
            this.toolStrip2.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            this.SuspendLayout();
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
            this.toolStrip2.Name = "toolStrip2";
            // 
            // toolStripComboBoxCh
            // 
            resources.ApplyResources(this.toolStripComboBoxCh, "toolStripComboBoxCh");
            this.toolStripComboBoxCh.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripComboBoxCh.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBoxCh.DropDownWidth = 32;
            this.toolStripComboBoxCh.Items.AddRange(new object[] {
            resources.GetString("toolStripComboBoxCh.Items"),
            resources.GetString("toolStripComboBoxCh.Items1"),
            resources.GetString("toolStripComboBoxCh.Items2"),
            resources.GetString("toolStripComboBoxCh.Items3"),
            resources.GetString("toolStripComboBoxCh.Items4"),
            resources.GetString("toolStripComboBoxCh.Items5"),
            resources.GetString("toolStripComboBoxCh.Items6"),
            resources.GetString("toolStripComboBoxCh.Items7"),
            resources.GetString("toolStripComboBoxCh.Items8"),
            resources.GetString("toolStripComboBoxCh.Items9"),
            resources.GetString("toolStripComboBoxCh.Items10"),
            resources.GetString("toolStripComboBoxCh.Items11"),
            resources.GetString("toolStripComboBoxCh.Items12"),
            resources.GetString("toolStripComboBoxCh.Items13"),
            resources.GetString("toolStripComboBoxCh.Items14"),
            resources.GetString("toolStripComboBoxCh.Items15")});
            this.toolStripComboBoxCh.Name = "toolStripComboBoxCh";
            this.toolStripComboBoxCh.SelectedIndexChanged += new System.EventHandler(this.toolStripComboBox1_SelectedIndexChanged);
            // 
            // toolStripLabel2
            // 
            resources.ApplyResources(this.toolStripLabel2, "toolStripLabel2");
            this.toolStripLabel2.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripLabel2.Name = "toolStripLabel2";
            // 
            // toolStripButtonPlay
            // 
            resources.ApplyResources(this.toolStripButtonPlay, "toolStripButtonPlay");
            this.toolStripButtonPlay.CheckOnClick = true;
            this.toolStripButtonPlay.Image = global::zanac.MAmidiMEmo.Properties.Resources.Inst;
            this.toolStripButtonPlay.Name = "toolStripButtonPlay";
            // 
            // toolStripButtonHook
            // 
            resources.ApplyResources(this.toolStripButtonHook, "toolStripButtonHook");
            this.toolStripButtonHook.CheckOnClick = true;
            this.toolStripButtonHook.Name = "toolStripButtonHook";
            // 
            // toolStripButton1
            // 
            resources.ApplyResources(this.toolStripButton1, "toolStripButton1");
            this.toolStripButton1.Image = global::zanac.MAmidiMEmo.Properties.Resources.Panic;
            this.toolStripButton1.Name = "toolStripButton1";
            // 
            // toolStripSeparator1
            // 
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            // 
            // toolStripLabel1
            // 
            resources.ApplyResources(this.toolStripLabel1, "toolStripLabel1");
            this.toolStripLabel1.Name = "toolStripLabel1";
            // 
            // toolStripComboBoxNote
            // 
            resources.ApplyResources(this.toolStripComboBoxNote, "toolStripComboBoxNote");
            this.toolStripComboBoxNote.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBoxNote.Name = "toolStripComboBoxNote";
            // 
            // toolStripLabel4
            // 
            resources.ApplyResources(this.toolStripLabel4, "toolStripLabel4");
            this.toolStripLabel4.Name = "toolStripLabel4";
            // 
            // toolStripComboBoxVelo
            // 
            resources.ApplyResources(this.toolStripComboBoxVelo, "toolStripComboBoxVelo");
            this.toolStripComboBoxVelo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBoxVelo.Name = "toolStripComboBoxVelo";
            // 
            // toolStripLabel3
            // 
            resources.ApplyResources(this.toolStripLabel3, "toolStripLabel3");
            this.toolStripLabel3.Name = "toolStripLabel3";
            // 
            // toolStripComboBoxGate
            // 
            resources.ApplyResources(this.toolStripComboBoxGate, "toolStripComboBoxGate");
            this.toolStripComboBoxGate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBoxGate.DropDownWidth = 100;
            this.toolStripComboBoxGate.Items.AddRange(new object[] {
            resources.GetString("toolStripComboBoxGate.Items"),
            resources.GetString("toolStripComboBoxGate.Items1"),
            resources.GetString("toolStripComboBoxGate.Items2"),
            resources.GetString("toolStripComboBoxGate.Items3")});
            this.toolStripComboBoxGate.Name = "toolStripComboBoxGate";
            // 
            // toolStripSeparator2
            // 
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            // 
            // toolStripLabel5
            // 
            resources.ApplyResources(this.toolStripLabel5, "toolStripLabel5");
            this.toolStripLabel5.Name = "toolStripLabel5";
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
            // listViewCurrentTimbres
            // 
            resources.ApplyResources(this.listViewCurrentTimbres, "listViewCurrentTimbres");
            this.listViewCurrentTimbres.AllowDrop = true;
            this.listViewCurrentTimbres.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.listViewCurrentTimbres.ContextMenuStrip = this.contextMenuStrip1;
            this.listViewCurrentTimbres.FullRowSelect = true;
            this.listViewCurrentTimbres.GridLines = true;
            this.listViewCurrentTimbres.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            ((System.Windows.Forms.ListViewGroup)(resources.GetObject("listViewCurrentTimbres.Groups")))});
            this.listViewCurrentTimbres.HideSelection = false;
            this.listViewCurrentTimbres.Name = "listViewCurrentTimbres";
            this.listViewCurrentTimbres.ShowGroups = false;
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
            resources.ApplyResources(this.columnHeader1, "columnHeader1");
            // 
            // columnHeader2
            // 
            resources.ApplyResources(this.columnHeader2, "columnHeader2");
            // 
            // columnHeader3
            // 
            resources.ApplyResources(this.columnHeader3, "columnHeader3");
            // 
            // contextMenuStrip1
            // 
            resources.ApplyResources(this.contextMenuStrip1, "contextMenuStrip1");
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.editToolStripMenuItem,
            this.propertyToolStripMenuItem,
            this.renameToolStripMenuItem,
            this.clearToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // editToolStripMenuItem
            // 
            resources.ApplyResources(this.editToolStripMenuItem, "editToolStripMenuItem");
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Click += new System.EventHandler(this.editToolStripMenuItem_Click);
            // 
            // propertyToolStripMenuItem
            // 
            resources.ApplyResources(this.propertyToolStripMenuItem, "propertyToolStripMenuItem");
            this.propertyToolStripMenuItem.Name = "propertyToolStripMenuItem";
            this.propertyToolStripMenuItem.Click += new System.EventHandler(this.propertyToolStripMenuItem_Click);
            // 
            // renameToolStripMenuItem
            // 
            resources.ApplyResources(this.renameToolStripMenuItem, "renameToolStripMenuItem");
            this.renameToolStripMenuItem.Name = "renameToolStripMenuItem";
            this.renameToolStripMenuItem.Click += new System.EventHandler(this.renameToolStripMenuItem_Click);
            // 
            // clearToolStripMenuItem
            // 
            resources.ApplyResources(this.clearToolStripMenuItem, "clearToolStripMenuItem");
            this.clearToolStripMenuItem.Name = "clearToolStripMenuItem";
            this.clearToolStripMenuItem.Click += new System.EventHandler(this.clearToolStripMenuItem_Click);
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.buttonOK, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.buttonCancel, 2, 1);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // buttonOK
            // 
            resources.ApplyResources(this.buttonOK, "buttonOK");
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            resources.ApplyResources(this.buttonCancel, "buttonCancel");
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // fileFolderList1
            // 
            resources.ApplyResources(this.fileFolderList1, "fileFolderList1");
            this.fileFolderList1.Activation = System.Windows.Forms.ItemActivation.TwoClick;
            this.fileFolderList1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4});
            this.tableLayoutPanel2.SetColumnSpan(this.fileFolderList1, 3);
            this.fileFolderList1.CurrentDirectory = "C:\\";
            this.fileFolderList1.FileValidator = null;
            this.fileFolderList1.FilterExts = null;
            this.fileFolderList1.FullRowSelect = true;
            this.fileFolderList1.GridLines = true;
            this.fileFolderList1.HideSelection = false;
            this.fileFolderList1.isSoloBrowser = true;
            this.fileFolderList1.Name = "fileFolderList1";
            this.fileFolderList1.UseCompatibleStateImageBehavior = false;
            this.fileFolderList1.View = System.Windows.Forms.View.Details;
            this.fileFolderList1.CurrentDirectoryChanged += new System.EventHandler(this.fileFolderList1_CurrentDirectoryChanged);
            // 
            // columnHeader4
            // 
            resources.ApplyResources(this.columnHeader4, "columnHeader4");
            // 
            // metroTextBox1
            // 
            resources.ApplyResources(this.metroTextBox1, "metroTextBox1");
            this.metroTextBox1.Name = "metroTextBox1";
            this.metroTextBox1.TextChanged += new System.EventHandler(this.metroTextBox1_TextChanged);
            // 
            // metroLabelDir
            // 
            resources.ApplyResources(this.metroLabelDir, "metroLabelDir");
            this.metroLabelDir.Name = "metroLabelDir";
            // 
            // metroLabel1
            // 
            resources.ApplyResources(this.metroLabel1, "metroLabel1");
            this.metroLabel1.Name = "metroLabel1";
            // 
            // tableLayoutPanel2
            // 
            resources.ApplyResources(this.tableLayoutPanel2, "tableLayoutPanel2");
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
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel5, 3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            // 
            // metroLabel2
            // 
            resources.ApplyResources(this.metroLabel2, "metroLabel2");
            this.metroLabel2.Name = "metroLabel2";
            // 
            // listViewFilesTimbres
            // 
            resources.ApplyResources(this.listViewFilesTimbres, "listViewFilesTimbres");
            this.listViewFilesTimbres.AllowDrop = true;
            this.listViewFilesTimbres.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader7});
            this.listViewFilesTimbres.FullRowSelect = true;
            this.listViewFilesTimbres.GridLines = true;
            this.listViewFilesTimbres.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            ((System.Windows.Forms.ListViewGroup)(resources.GetObject("listViewFilesTimbres.Groups")))});
            this.listViewFilesTimbres.HideSelection = false;
            this.listViewFilesTimbres.Name = "listViewFilesTimbres";
            this.listViewFilesTimbres.ShowGroups = false;
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
            resources.ApplyResources(this.columnHeader5, "columnHeader5");
            // 
            // columnHeader6
            // 
            resources.ApplyResources(this.columnHeader6, "columnHeader6");
            // 
            // columnHeader7
            // 
            resources.ApplyResources(this.columnHeader7, "columnHeader7");
            // 
            // metroLabel3
            // 
            resources.ApplyResources(this.metroLabel3, "metroLabel3");
            this.metroLabel3.Name = "metroLabel3";
            // 
            // tableLayoutPanel3
            // 
            resources.ApplyResources(this.tableLayoutPanel3, "tableLayoutPanel3");
            this.tableLayoutPanel2.SetColumnSpan(this.tableLayoutPanel3, 3);
            this.tableLayoutPanel3.Controls.Add(this.metroButtonNewDir, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.metroButtonDelete, 1, 1);
            this.tableLayoutPanel3.Controls.Add(this.metroButtonRefresh, 4, 1);
            this.tableLayoutPanel3.Controls.Add(this.metroButtonExplorer, 3, 1);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            // 
            // metroButtonNewDir
            // 
            resources.ApplyResources(this.metroButtonNewDir, "metroButtonNewDir");
            this.metroButtonNewDir.Name = "metroButtonNewDir";
            this.metroButtonNewDir.Click += new System.EventHandler(this.metroButtonNewDir_Click);
            // 
            // metroButtonDelete
            // 
            resources.ApplyResources(this.metroButtonDelete, "metroButtonDelete");
            this.metroButtonDelete.Name = "metroButtonDelete";
            this.metroButtonDelete.Click += new System.EventHandler(this.metroButtonDelete_Click);
            // 
            // metroButtonRefresh
            // 
            resources.ApplyResources(this.metroButtonRefresh, "metroButtonRefresh");
            this.metroButtonRefresh.Name = "metroButtonRefresh";
            this.metroButtonRefresh.Click += new System.EventHandler(this.metroButtonRefresh_Click);
            // 
            // metroButtonExplorer
            // 
            resources.ApplyResources(this.metroButtonExplorer, "metroButtonExplorer");
            this.metroButtonExplorer.Name = "metroButtonExplorer";
            this.metroButtonExplorer.Click += new System.EventHandler(this.metroButtonExplorer_Click);
            // 
            // metroButton1
            // 
            resources.ApplyResources(this.metroButton1, "metroButton1");
            this.metroButton1.Name = "metroButton1";
            this.metroButton1.Click += new System.EventHandler(this.metroButton1_Click);
            // 
            // tableLayoutPanel4
            // 
            resources.ApplyResources(this.tableLayoutPanel4, "tableLayoutPanel4");
            this.tableLayoutPanel4.Controls.Add(this.metroButtonExport, 0, 1);
            this.tableLayoutPanel4.Controls.Add(this.metroButton2, 1, 1);
            this.tableLayoutPanel4.Controls.Add(this.metroButton3, 3, 1);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            // 
            // metroButtonExport
            // 
            resources.ApplyResources(this.metroButtonExport, "metroButtonExport");
            this.metroButtonExport.Name = "metroButtonExport";
            this.metroButtonExport.Click += new System.EventHandler(this.metroButtonExport_Click);
            // 
            // metroButton2
            // 
            resources.ApplyResources(this.metroButton2, "metroButton2");
            this.metroButton2.Name = "metroButton2";
            this.metroButton2.Click += new System.EventHandler(this.metroButton2_Click);
            // 
            // metroButton3
            // 
            resources.ApplyResources(this.metroButton3, "metroButton3");
            this.metroButton3.Name = "metroButton3";
            this.metroButton3.Click += new System.EventHandler(this.metroButtonClear_Click);
            // 
            // tableLayoutPanel5
            // 
            resources.ApplyResources(this.tableLayoutPanel5, "tableLayoutPanel5");
            this.tableLayoutPanel5.Controls.Add(this.checkBoxSetDrumKeyNo, 0, 0);
            this.tableLayoutPanel5.Controls.Add(this.comboBoxDrumKeyNo, 1, 0);
            this.tableLayoutPanel5.Controls.Add(this.checkBoxSetDrumTimbreName, 0, 1);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            // 
            // checkBoxSetDrumKeyNo
            // 
            resources.ApplyResources(this.checkBoxSetDrumKeyNo, "checkBoxSetDrumKeyNo");
            this.checkBoxSetDrumKeyNo.Checked = global::zanac.MAmidiMEmo.Properties.Settings.Default.RegisterDrumTimbre;
            this.checkBoxSetDrumKeyNo.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::zanac.MAmidiMEmo.Properties.Settings.Default, "RegisterDrumTimbre", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.checkBoxSetDrumKeyNo.Name = "checkBoxSetDrumKeyNo";
            this.checkBoxSetDrumKeyNo.UseVisualStyleBackColor = true;
            // 
            // comboBoxDrumKeyNo
            // 
            resources.ApplyResources(this.comboBoxDrumKeyNo, "comboBoxDrumKeyNo");
            this.comboBoxDrumKeyNo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDrumKeyNo.FormattingEnabled = true;
            this.comboBoxDrumKeyNo.Name = "comboBoxDrumKeyNo";
            // 
            // checkBoxSetDrumTimbreName
            // 
            resources.ApplyResources(this.checkBoxSetDrumTimbreName, "checkBoxSetDrumTimbreName");
            this.checkBoxSetDrumTimbreName.Checked = global::zanac.MAmidiMEmo.Properties.Settings.Default.SetDrumTimbreName;
            this.checkBoxSetDrumTimbreName.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::zanac.MAmidiMEmo.Properties.Settings.Default, "SetDrumTimbreName", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.checkBoxSetDrumTimbreName.Name = "checkBoxSetDrumTimbreName";
            this.checkBoxSetDrumTimbreName.UseVisualStyleBackColor = true;
            // 
            // betterFolderBrowser1
            // 
            this.betterFolderBrowser1.Multiselect = false;
            this.betterFolderBrowser1.RootFolder = "C:\\Users\\zanac2\\Desktop";
            this.betterFolderBrowser1.Title = "Please select a folder...";
            // 
            // FormTimbreManager
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.tableLayoutPanel2);
            this.Controls.Add(this.toolStrip2);
            this.Controls.Add(this.pianoControl1);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "FormTimbreManager";
            this.toolStrip2.ResumeLayout(false);
            this.toolStrip2.PerformLayout();
            this.contextMenuStrip1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel5.ResumeLayout(false);
            this.tableLayoutPanel5.PerformLayout();
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
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem renameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem propertyToolStripMenuItem;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.CheckBox checkBoxSetDrumKeyNo;
        private System.Windows.Forms.CheckBox checkBoxSetDrumTimbreName;
        private System.Windows.Forms.ComboBox comboBoxDrumKeyNo;
    }
}