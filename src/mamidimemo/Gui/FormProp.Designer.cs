namespace zanac.MAmidiMEmo.Gui
{
    partial class FormProp
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormProp));
            this.propertyGrid = new System.Windows.Forms.PropertyGrid();
            this.contextMenuStripProp = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.resetToDefaultThisPropertyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panel1 = new System.Windows.Forms.Panel();
            this.pianoControl1 = new zanac.MAmidiMEmo.Gui.PianoControl();
            this.toolStrip2 = new zanac.MAmidiMEmo.ComponentModel.ToolStripBase();
            this.toolStripComboBoxProg = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripLabel4 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripComboBoxCh = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripLabel5 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripComboBoxCC = new System.Windows.Forms.ToolStripComboBox();
            this.toolStrip3 = new zanac.MAmidiMEmo.ComponentModel.ToolStripBase();
            this.toolStripButtonCat = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonA2Z = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonPopup = new System.Windows.Forms.ToolStripButton();
            this.contextMenuStripProp.SuspendLayout();
            this.panel1.SuspendLayout();
            this.toolStrip2.SuspendLayout();
            this.toolStrip3.SuspendLayout();
            this.SuspendLayout();
            // 
            // propertyGrid
            // 
            resources.ApplyResources(this.propertyGrid, "propertyGrid");
            this.propertyGrid.ContextMenuStrip = this.contextMenuStripProp;
            this.propertyGrid.HelpBackColor = System.Drawing.SystemColors.Info;
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.PropertyTabChanged += new System.Windows.Forms.PropertyTabChangedEventHandler(this.propertyGrid_PropertyTabChanged);
            this.propertyGrid.SelectedGridItemChanged += new System.Windows.Forms.SelectedGridItemChangedEventHandler(this.propertyGrid_SelectedGridItemChanged);
            this.propertyGrid.SelectedObjectsChanged += new System.EventHandler(this.propertyGrid_SelectedObjectsChanged);
            // 
            // contextMenuStripProp
            // 
            resources.ApplyResources(this.contextMenuStripProp, "contextMenuStripProp");
            this.contextMenuStripProp.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.resetToDefaultThisPropertyToolStripMenuItem});
            this.contextMenuStripProp.Name = "contextMenuStripProp";
            this.contextMenuStripProp.Click += new System.EventHandler(this.contextMenuStripProp_Click);
            // 
            // resetToDefaultThisPropertyToolStripMenuItem
            // 
            resources.ApplyResources(this.resetToDefaultThisPropertyToolStripMenuItem, "resetToDefaultThisPropertyToolStripMenuItem");
            this.resetToDefaultThisPropertyToolStripMenuItem.Name = "resetToDefaultThisPropertyToolStripMenuItem";
            // 
            // panel1
            // 
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Controls.Add(this.pianoControl1);
            this.panel1.Controls.Add(this.toolStrip2);
            this.panel1.Name = "panel1";
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
            this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripComboBoxProg,
            this.toolStripLabel4,
            this.toolStripComboBoxCh,
            this.toolStripLabel2,
            this.toolStripLabel5,
            this.toolStripComboBoxCC});
            this.toolStrip2.Name = "toolStrip2";
            // 
            // toolStripComboBoxProg
            // 
            resources.ApplyResources(this.toolStripComboBoxProg, "toolStripComboBoxProg");
            this.toolStripComboBoxProg.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripComboBoxProg.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBoxProg.DropDownWidth = 32;
            this.toolStripComboBoxProg.Items.AddRange(new object[] {
            resources.GetString("toolStripComboBoxProg.Items"),
            resources.GetString("toolStripComboBoxProg.Items1"),
            resources.GetString("toolStripComboBoxProg.Items2"),
            resources.GetString("toolStripComboBoxProg.Items3"),
            resources.GetString("toolStripComboBoxProg.Items4"),
            resources.GetString("toolStripComboBoxProg.Items5"),
            resources.GetString("toolStripComboBoxProg.Items6"),
            resources.GetString("toolStripComboBoxProg.Items7"),
            resources.GetString("toolStripComboBoxProg.Items8"),
            resources.GetString("toolStripComboBoxProg.Items9"),
            resources.GetString("toolStripComboBoxProg.Items10"),
            resources.GetString("toolStripComboBoxProg.Items11"),
            resources.GetString("toolStripComboBoxProg.Items12"),
            resources.GetString("toolStripComboBoxProg.Items13"),
            resources.GetString("toolStripComboBoxProg.Items14"),
            resources.GetString("toolStripComboBoxProg.Items15"),
            resources.GetString("toolStripComboBoxProg.Items16"),
            resources.GetString("toolStripComboBoxProg.Items17"),
            resources.GetString("toolStripComboBoxProg.Items18"),
            resources.GetString("toolStripComboBoxProg.Items19"),
            resources.GetString("toolStripComboBoxProg.Items20"),
            resources.GetString("toolStripComboBoxProg.Items21"),
            resources.GetString("toolStripComboBoxProg.Items22"),
            resources.GetString("toolStripComboBoxProg.Items23"),
            resources.GetString("toolStripComboBoxProg.Items24"),
            resources.GetString("toolStripComboBoxProg.Items25"),
            resources.GetString("toolStripComboBoxProg.Items26"),
            resources.GetString("toolStripComboBoxProg.Items27"),
            resources.GetString("toolStripComboBoxProg.Items28"),
            resources.GetString("toolStripComboBoxProg.Items29"),
            resources.GetString("toolStripComboBoxProg.Items30"),
            resources.GetString("toolStripComboBoxProg.Items31"),
            resources.GetString("toolStripComboBoxProg.Items32"),
            resources.GetString("toolStripComboBoxProg.Items33"),
            resources.GetString("toolStripComboBoxProg.Items34"),
            resources.GetString("toolStripComboBoxProg.Items35"),
            resources.GetString("toolStripComboBoxProg.Items36"),
            resources.GetString("toolStripComboBoxProg.Items37"),
            resources.GetString("toolStripComboBoxProg.Items38"),
            resources.GetString("toolStripComboBoxProg.Items39"),
            resources.GetString("toolStripComboBoxProg.Items40"),
            resources.GetString("toolStripComboBoxProg.Items41"),
            resources.GetString("toolStripComboBoxProg.Items42"),
            resources.GetString("toolStripComboBoxProg.Items43"),
            resources.GetString("toolStripComboBoxProg.Items44"),
            resources.GetString("toolStripComboBoxProg.Items45"),
            resources.GetString("toolStripComboBoxProg.Items46"),
            resources.GetString("toolStripComboBoxProg.Items47"),
            resources.GetString("toolStripComboBoxProg.Items48"),
            resources.GetString("toolStripComboBoxProg.Items49"),
            resources.GetString("toolStripComboBoxProg.Items50"),
            resources.GetString("toolStripComboBoxProg.Items51"),
            resources.GetString("toolStripComboBoxProg.Items52"),
            resources.GetString("toolStripComboBoxProg.Items53"),
            resources.GetString("toolStripComboBoxProg.Items54"),
            resources.GetString("toolStripComboBoxProg.Items55"),
            resources.GetString("toolStripComboBoxProg.Items56"),
            resources.GetString("toolStripComboBoxProg.Items57"),
            resources.GetString("toolStripComboBoxProg.Items58"),
            resources.GetString("toolStripComboBoxProg.Items59"),
            resources.GetString("toolStripComboBoxProg.Items60"),
            resources.GetString("toolStripComboBoxProg.Items61"),
            resources.GetString("toolStripComboBoxProg.Items62"),
            resources.GetString("toolStripComboBoxProg.Items63"),
            resources.GetString("toolStripComboBoxProg.Items64"),
            resources.GetString("toolStripComboBoxProg.Items65"),
            resources.GetString("toolStripComboBoxProg.Items66"),
            resources.GetString("toolStripComboBoxProg.Items67"),
            resources.GetString("toolStripComboBoxProg.Items68"),
            resources.GetString("toolStripComboBoxProg.Items69"),
            resources.GetString("toolStripComboBoxProg.Items70"),
            resources.GetString("toolStripComboBoxProg.Items71"),
            resources.GetString("toolStripComboBoxProg.Items72"),
            resources.GetString("toolStripComboBoxProg.Items73"),
            resources.GetString("toolStripComboBoxProg.Items74"),
            resources.GetString("toolStripComboBoxProg.Items75"),
            resources.GetString("toolStripComboBoxProg.Items76"),
            resources.GetString("toolStripComboBoxProg.Items77"),
            resources.GetString("toolStripComboBoxProg.Items78"),
            resources.GetString("toolStripComboBoxProg.Items79"),
            resources.GetString("toolStripComboBoxProg.Items80"),
            resources.GetString("toolStripComboBoxProg.Items81"),
            resources.GetString("toolStripComboBoxProg.Items82"),
            resources.GetString("toolStripComboBoxProg.Items83"),
            resources.GetString("toolStripComboBoxProg.Items84"),
            resources.GetString("toolStripComboBoxProg.Items85"),
            resources.GetString("toolStripComboBoxProg.Items86"),
            resources.GetString("toolStripComboBoxProg.Items87"),
            resources.GetString("toolStripComboBoxProg.Items88"),
            resources.GetString("toolStripComboBoxProg.Items89"),
            resources.GetString("toolStripComboBoxProg.Items90"),
            resources.GetString("toolStripComboBoxProg.Items91"),
            resources.GetString("toolStripComboBoxProg.Items92"),
            resources.GetString("toolStripComboBoxProg.Items93"),
            resources.GetString("toolStripComboBoxProg.Items94"),
            resources.GetString("toolStripComboBoxProg.Items95"),
            resources.GetString("toolStripComboBoxProg.Items96"),
            resources.GetString("toolStripComboBoxProg.Items97"),
            resources.GetString("toolStripComboBoxProg.Items98"),
            resources.GetString("toolStripComboBoxProg.Items99"),
            resources.GetString("toolStripComboBoxProg.Items100"),
            resources.GetString("toolStripComboBoxProg.Items101"),
            resources.GetString("toolStripComboBoxProg.Items102"),
            resources.GetString("toolStripComboBoxProg.Items103"),
            resources.GetString("toolStripComboBoxProg.Items104"),
            resources.GetString("toolStripComboBoxProg.Items105"),
            resources.GetString("toolStripComboBoxProg.Items106"),
            resources.GetString("toolStripComboBoxProg.Items107"),
            resources.GetString("toolStripComboBoxProg.Items108"),
            resources.GetString("toolStripComboBoxProg.Items109"),
            resources.GetString("toolStripComboBoxProg.Items110"),
            resources.GetString("toolStripComboBoxProg.Items111"),
            resources.GetString("toolStripComboBoxProg.Items112"),
            resources.GetString("toolStripComboBoxProg.Items113"),
            resources.GetString("toolStripComboBoxProg.Items114"),
            resources.GetString("toolStripComboBoxProg.Items115"),
            resources.GetString("toolStripComboBoxProg.Items116"),
            resources.GetString("toolStripComboBoxProg.Items117"),
            resources.GetString("toolStripComboBoxProg.Items118"),
            resources.GetString("toolStripComboBoxProg.Items119"),
            resources.GetString("toolStripComboBoxProg.Items120"),
            resources.GetString("toolStripComboBoxProg.Items121"),
            resources.GetString("toolStripComboBoxProg.Items122"),
            resources.GetString("toolStripComboBoxProg.Items123"),
            resources.GetString("toolStripComboBoxProg.Items124"),
            resources.GetString("toolStripComboBoxProg.Items125"),
            resources.GetString("toolStripComboBoxProg.Items126"),
            resources.GetString("toolStripComboBoxProg.Items127"),
            resources.GetString("toolStripComboBoxProg.Items128")});
            this.toolStripComboBoxProg.Name = "toolStripComboBoxProg";
            // 
            // toolStripLabel4
            // 
            resources.ApplyResources(this.toolStripLabel4, "toolStripLabel4");
            this.toolStripLabel4.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripLabel4.Name = "toolStripLabel4";
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
            // toolStrip3
            // 
            resources.ApplyResources(this.toolStrip3, "toolStrip3");
            this.toolStrip3.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonCat,
            this.toolStripButtonA2Z,
            this.toolStripButtonPopup});
            this.toolStrip3.Name = "toolStrip3";
            // 
            // toolStripButtonCat
            // 
            resources.ApplyResources(this.toolStripButtonCat, "toolStripButtonCat");
            this.toolStripButtonCat.Checked = true;
            this.toolStripButtonCat.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripButtonCat.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonCat.Image = global::zanac.MAmidiMEmo.Properties.Resources.Cat;
            this.toolStripButtonCat.Name = "toolStripButtonCat";
            this.toolStripButtonCat.Click += new System.EventHandler(this.toolStripButtonCat_Click);
            // 
            // toolStripButtonA2Z
            // 
            resources.ApplyResources(this.toolStripButtonA2Z, "toolStripButtonA2Z");
            this.toolStripButtonA2Z.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonA2Z.Image = global::zanac.MAmidiMEmo.Properties.Resources.AtoZ;
            this.toolStripButtonA2Z.Name = "toolStripButtonA2Z";
            this.toolStripButtonA2Z.Click += new System.EventHandler(this.toolStripButtonA2Z_Click);
            // 
            // toolStripButtonPopup
            // 
            resources.ApplyResources(this.toolStripButtonPopup, "toolStripButtonPopup");
            this.toolStripButtonPopup.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButtonPopup.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonPopup.Image = global::zanac.MAmidiMEmo.Properties.Resources.Popup;
            this.toolStripButtonPopup.Name = "toolStripButtonPopup";
            this.toolStripButtonPopup.Click += new System.EventHandler(this.toolStripButtonPopup_Click);
            // 
            // FormProp
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.propertyGrid);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.toolStrip3);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormProp";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.contextMenuStripProp.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.toolStrip2.ResumeLayout(false);
            this.toolStrip2.PerformLayout();
            this.toolStrip3.ResumeLayout(false);
            this.toolStrip3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PropertyGrid propertyGrid;
        private ComponentModel.ToolStripBase toolStrip3;
        private System.Windows.Forms.ToolStripButton toolStripButtonCat;
        private System.Windows.Forms.ToolStripButton toolStripButtonA2Z;
        private System.Windows.Forms.ToolStripButton toolStripButtonPopup;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripProp;
        private System.Windows.Forms.ToolStripMenuItem resetToDefaultThisPropertyToolStripMenuItem;
        private System.Windows.Forms.Panel panel1;
        private PianoControl pianoControl1;
        private ComponentModel.ToolStripBase toolStrip2;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxProg;
        private System.Windows.Forms.ToolStripLabel toolStripLabel4;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxCh;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxCC;
        private System.Windows.Forms.ToolStripLabel toolStripLabel5;
    }
}