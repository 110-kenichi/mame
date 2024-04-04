using Microsoft.WindowsAPICodePack.Dialogs;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormFmEditor));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.metroButtonRand1 = new MetroFramework.Controls.MetroButton();
            this.metroButtonParams = new MetroFramework.Controls.MetroButton();
            this.metroTextBoxTarget = new MetroFramework.Controls.MetroTextBox();
            this.metroButtonImport = new MetroFramework.Controls.MetroButton();
            this.metroButtonImportGit = new MetroFramework.Controls.MetroButton();
            this.metroButtonPaste = new MetroFramework.Controls.MetroButton();
            this.metroButtonCopy = new MetroFramework.Controls.MetroButton();
            this.metroButtonExport = new MetroFramework.Controls.MetroButton();
            this.metroLabel1 = new MetroFramework.Controls.MetroLabel();
            this.metroTextBoxTargetMinMax = new MetroFramework.Controls.MetroTextBox();
            this.metroTextBoxPatchFile = new MetroFramework.Controls.MetroTextBox();
            this.propertyGrid = new System.Windows.Forms.PropertyGrid();
            this.contextMenuStripProp = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.resetToDefaultThisPropertyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            this.buttonOK = new MetroFramework.Controls.MetroButton();
            this.buttonCancel = new MetroFramework.Controls.MetroButton();
            this.metroButtonAbort = new MetroFramework.Controls.MetroButton();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.panel1 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.metroComboBoxTimbres = new MetroFramework.Controls.MetroComboBox();
            this.metroButtonImportAll = new MetroFramework.Controls.MetroButton();
            this.metroButtonImportAllGit = new MetroFramework.Controls.MetroButton();
            this.metroButtonExportAll = new MetroFramework.Controls.MetroButton();
            this.metroButtonTimbre = new MetroFramework.Controls.MetroButton();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.checkBoxHidePropAre = new System.Windows.Forms.CheckBox();
            this.metroToolTip1 = new MetroFramework.Components.MetroToolTip();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.contextMenuStripProp.SuspendLayout();
            this.panelPiano.SuspendLayout();
            this.toolStrip2.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            resources.ApplyResources(this.splitContainer1, "splitContainer1");
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer1.DataBindings.Add(new System.Windows.Forms.Binding("Panel2Collapsed", global::zanac.MAmidiMEmo.Properties.Settings.Default, "FmHideProp", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            resources.ApplyResources(this.splitContainer1.Panel1, "splitContainer1.Panel1");
            this.splitContainer1.Panel1.Controls.Add(this.flowLayoutPanel1);
            this.splitContainer1.Panel1.Controls.Add(this.tableLayoutPanel1);
            this.metroToolTip1.SetToolTip(this.splitContainer1.Panel1, resources.GetString("splitContainer1.Panel1.ToolTip"));
            this.toolTip1.SetToolTip(this.splitContainer1.Panel1, resources.GetString("splitContainer1.Panel1.ToolTip1"));
            // 
            // splitContainer1.Panel2
            // 
            resources.ApplyResources(this.splitContainer1.Panel2, "splitContainer1.Panel2");
            this.splitContainer1.Panel2.Controls.Add(this.propertyGrid);
            this.metroToolTip1.SetToolTip(this.splitContainer1.Panel2, resources.GetString("splitContainer1.Panel2.ToolTip"));
            this.toolTip1.SetToolTip(this.splitContainer1.Panel2, resources.GetString("splitContainer1.Panel2.ToolTip1"));
            this.splitContainer1.Panel2Collapsed = global::zanac.MAmidiMEmo.Properties.Settings.Default.FmHideProp;
            this.toolTip1.SetToolTip(this.splitContainer1, resources.GetString("splitContainer1.ToolTip"));
            this.metroToolTip1.SetToolTip(this.splitContainer1, resources.GetString("splitContainer1.ToolTip1"));
            // 
            // flowLayoutPanel1
            // 
            resources.ApplyResources(this.flowLayoutPanel1, "flowLayoutPanel1");
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.toolTip1.SetToolTip(this.flowLayoutPanel1, resources.GetString("flowLayoutPanel1.ToolTip"));
            this.metroToolTip1.SetToolTip(this.flowLayoutPanel1, resources.GetString("flowLayoutPanel1.ToolTip1"));
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.metroButtonRand1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.metroButtonParams, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.metroTextBoxTarget, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.metroButtonImport, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.metroButtonImportGit, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.metroButtonPaste, 9, 0);
            this.tableLayoutPanel1.Controls.Add(this.metroButtonCopy, 8, 0);
            this.tableLayoutPanel1.Controls.Add(this.metroButtonExport, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.metroLabel1, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.metroTextBoxTargetMinMax, 5, 0);
            this.tableLayoutPanel1.Controls.Add(this.metroTextBoxPatchFile, 4, 1);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.toolTip1.SetToolTip(this.tableLayoutPanel1, resources.GetString("tableLayoutPanel1.ToolTip"));
            this.metroToolTip1.SetToolTip(this.tableLayoutPanel1, resources.GetString("tableLayoutPanel1.ToolTip1"));
            // 
            // metroButtonRand1
            // 
            resources.ApplyResources(this.metroButtonRand1, "metroButtonRand1");
            this.metroButtonRand1.Name = "metroButtonRand1";
            this.metroToolTip1.SetToolTip(this.metroButtonRand1, resources.GetString("metroButtonRand1.ToolTip"));
            this.toolTip1.SetToolTip(this.metroButtonRand1, resources.GetString("metroButtonRand1.ToolTip1"));
            this.metroButtonRand1.Click += new System.EventHandler(this.metroButtonRandAll_Click);
            // 
            // metroButtonParams
            // 
            resources.ApplyResources(this.metroButtonParams, "metroButtonParams");
            this.metroButtonParams.Name = "metroButtonParams";
            this.metroToolTip1.SetToolTip(this.metroButtonParams, resources.GetString("metroButtonParams.ToolTip"));
            this.toolTip1.SetToolTip(this.metroButtonParams, resources.GetString("metroButtonParams.ToolTip1"));
            this.metroButtonParams.Click += new System.EventHandler(this.metroButtonRandParams_Click);
            // 
            // metroTextBoxTarget
            // 
            resources.ApplyResources(this.metroTextBoxTarget, "metroTextBoxTarget");
            this.tableLayoutPanel1.SetColumnSpan(this.metroTextBoxTarget, 3);
            this.metroTextBoxTarget.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::zanac.MAmidiMEmo.Properties.Settings.Default, "FmTarget", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.metroTextBoxTarget.Name = "metroTextBoxTarget";
            this.metroTextBoxTarget.PromptText = "(Write randomize target register and untarget( prefix \"!\" ) names here. Separated" +
    " with comma. )";
            this.metroTextBoxTarget.Text = global::zanac.MAmidiMEmo.Properties.Settings.Default.FmTarget;
            this.toolTip1.SetToolTip(this.metroTextBoxTarget, resources.GetString("metroTextBoxTarget.ToolTip"));
            this.metroToolTip1.SetToolTip(this.metroTextBoxTarget, resources.GetString("metroTextBoxTarget.ToolTip1"));
            // 
            // metroButtonImport
            // 
            resources.ApplyResources(this.metroButtonImport, "metroButtonImport");
            this.metroButtonImport.AllowDrop = true;
            this.metroButtonImport.Name = "metroButtonImport";
            this.metroToolTip1.SetToolTip(this.metroButtonImport, resources.GetString("metroButtonImport.ToolTip"));
            this.toolTip1.SetToolTip(this.metroButtonImport, resources.GetString("metroButtonImport.ToolTip1"));
            this.metroButtonImport.Click += new System.EventHandler(this.metroButtonImport_Click);
            this.metroButtonImport.DragDrop += new System.Windows.Forms.DragEventHandler(this.metroButtonImport_DragDrop);
            this.metroButtonImport.DragEnter += new System.Windows.Forms.DragEventHandler(this.metroButtonImport_DragEnter);
            // 
            // metroButtonImportGit
            // 
            resources.ApplyResources(this.metroButtonImportGit, "metroButtonImportGit");
            this.metroButtonImportGit.Name = "metroButtonImportGit";
            this.metroToolTip1.SetToolTip(this.metroButtonImportGit, resources.GetString("metroButtonImportGit.ToolTip"));
            this.toolTip1.SetToolTip(this.metroButtonImportGit, resources.GetString("metroButtonImportGit.ToolTip1"));
            this.metroButtonImportGit.Click += new System.EventHandler(this.metroButtonImportGit_Click);
            // 
            // metroButtonPaste
            // 
            resources.ApplyResources(this.metroButtonPaste, "metroButtonPaste");
            this.metroButtonPaste.Name = "metroButtonPaste";
            this.metroToolTip1.SetToolTip(this.metroButtonPaste, resources.GetString("metroButtonPaste.ToolTip"));
            this.toolTip1.SetToolTip(this.metroButtonPaste, resources.GetString("metroButtonPaste.ToolTip1"));
            this.metroButtonPaste.Click += new System.EventHandler(this.metroButtonPaste_Click);
            // 
            // metroButtonCopy
            // 
            resources.ApplyResources(this.metroButtonCopy, "metroButtonCopy");
            this.metroButtonCopy.Name = "metroButtonCopy";
            this.metroToolTip1.SetToolTip(this.metroButtonCopy, resources.GetString("metroButtonCopy.ToolTip"));
            this.toolTip1.SetToolTip(this.metroButtonCopy, resources.GetString("metroButtonCopy.ToolTip1"));
            this.metroButtonCopy.Click += new System.EventHandler(this.metroButtonCopy_Click);
            // 
            // metroButtonExport
            // 
            resources.ApplyResources(this.metroButtonExport, "metroButtonExport");
            this.metroButtonExport.Name = "metroButtonExport";
            this.metroToolTip1.SetToolTip(this.metroButtonExport, resources.GetString("metroButtonExport.ToolTip"));
            this.toolTip1.SetToolTip(this.metroButtonExport, resources.GetString("metroButtonExport.ToolTip1"));
            this.metroButtonExport.Click += new System.EventHandler(this.metroButtonExport_Click);
            // 
            // metroLabel1
            // 
            resources.ApplyResources(this.metroLabel1, "metroLabel1");
            this.metroLabel1.Name = "metroLabel1";
            this.toolTip1.SetToolTip(this.metroLabel1, resources.GetString("metroLabel1.ToolTip"));
            this.metroToolTip1.SetToolTip(this.metroLabel1, resources.GetString("metroLabel1.ToolTip1"));
            // 
            // metroTextBoxTargetMinMax
            // 
            resources.ApplyResources(this.metroTextBoxTargetMinMax, "metroTextBoxTargetMinMax");
            this.tableLayoutPanel1.SetColumnSpan(this.metroTextBoxTargetMinMax, 3);
            this.metroTextBoxTargetMinMax.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::zanac.MAmidiMEmo.Properties.Settings.Default, "FmTargetMinMax", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.metroTextBoxTargetMinMax.Name = "metroTextBoxTargetMinMax";
            this.metroTextBoxTargetMinMax.PromptText = "(Write randomize target register min max value here. Separated with comma. )";
            this.metroTextBoxTargetMinMax.Text = global::zanac.MAmidiMEmo.Properties.Settings.Default.FmTargetMinMax;
            this.toolTip1.SetToolTip(this.metroTextBoxTargetMinMax, resources.GetString("metroTextBoxTargetMinMax.ToolTip"));
            this.metroToolTip1.SetToolTip(this.metroTextBoxTargetMinMax, resources.GetString("metroTextBoxTargetMinMax.ToolTip1"));
            // 
            // metroTextBoxPatchFile
            // 
            resources.ApplyResources(this.metroTextBoxPatchFile, "metroTextBoxPatchFile");
            this.tableLayoutPanel1.SetColumnSpan(this.metroTextBoxPatchFile, 6);
            this.metroTextBoxPatchFile.Name = "metroTextBoxPatchFile";
            this.metroTextBoxPatchFile.ReadOnly = true;
            this.toolTip1.SetToolTip(this.metroTextBoxPatchFile, resources.GetString("metroTextBoxPatchFile.ToolTip"));
            this.metroToolTip1.SetToolTip(this.metroTextBoxPatchFile, resources.GetString("metroTextBoxPatchFile.ToolTip1"));
            // 
            // propertyGrid
            // 
            resources.ApplyResources(this.propertyGrid, "propertyGrid");
            this.propertyGrid.ContextMenuStrip = this.contextMenuStripProp;
            this.propertyGrid.HelpBackColor = System.Drawing.SystemColors.Info;
            this.propertyGrid.Name = "propertyGrid";
            this.toolTip1.SetToolTip(this.propertyGrid, resources.GetString("propertyGrid.ToolTip"));
            this.metroToolTip1.SetToolTip(this.propertyGrid, resources.GetString("propertyGrid.ToolTip1"));
            this.propertyGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGrid_PropertyValueChanged);
            this.propertyGrid.PropertyTabChanged += new System.Windows.Forms.PropertyTabChangedEventHandler(this.propertyGrid_PropertyTabChanged);
            this.propertyGrid.SelectedObjectsChanged += new System.EventHandler(this.propertyGrid_SelectedObjectsChanged);
            // 
            // contextMenuStripProp
            // 
            resources.ApplyResources(this.contextMenuStripProp, "contextMenuStripProp");
            this.contextMenuStripProp.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.resetToDefaultThisPropertyToolStripMenuItem});
            this.contextMenuStripProp.Name = "contextMenuStripProp";
            this.metroToolTip1.SetToolTip(this.contextMenuStripProp, resources.GetString("contextMenuStripProp.ToolTip"));
            this.toolTip1.SetToolTip(this.contextMenuStripProp, resources.GetString("contextMenuStripProp.ToolTip1"));
            this.contextMenuStripProp.Click += new System.EventHandler(this.contextMenuStripProp_Click);
            // 
            // resetToDefaultThisPropertyToolStripMenuItem
            // 
            resources.ApplyResources(this.resetToDefaultThisPropertyToolStripMenuItem, "resetToDefaultThisPropertyToolStripMenuItem");
            this.resetToDefaultThisPropertyToolStripMenuItem.Name = "resetToDefaultThisPropertyToolStripMenuItem";
            // 
            // panelPiano
            // 
            resources.ApplyResources(this.panelPiano, "panelPiano");
            this.panelPiano.Controls.Add(this.pianoControl1);
            this.panelPiano.Controls.Add(this.toolStrip2);
            this.panelPiano.Name = "panelPiano";
            this.metroToolTip1.SetToolTip(this.panelPiano, resources.GetString("panelPiano.ToolTip"));
            this.toolTip1.SetToolTip(this.panelPiano, resources.GetString("panelPiano.ToolTip1"));
            // 
            // pianoControl1
            // 
            resources.ApplyResources(this.pianoControl1, "pianoControl1");
            this.pianoControl1.Name = "pianoControl1";
            this.pianoControl1.TargetTimbres = null;
            this.metroToolTip1.SetToolTip(this.pianoControl1, resources.GetString("pianoControl1.ToolTip"));
            this.toolTip1.SetToolTip(this.pianoControl1, resources.GetString("pianoControl1.ToolTip1"));
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
            this.toolTip1.SetToolTip(this.toolStrip2, resources.GetString("toolStrip2.ToolTip"));
            this.metroToolTip1.SetToolTip(this.toolStrip2, resources.GetString("toolStrip2.ToolTip1"));
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
            this.toolStripButton1.Click += new System.EventHandler(this.toolStripButton1_Click);
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
            // buttonOK
            // 
            resources.ApplyResources(this.buttonOK, "buttonOK");
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Name = "buttonOK";
            this.metroToolTip1.SetToolTip(this.buttonOK, resources.GetString("buttonOK.ToolTip"));
            this.toolTip1.SetToolTip(this.buttonOK, resources.GetString("buttonOK.ToolTip1"));
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            resources.ApplyResources(this.buttonCancel, "buttonCancel");
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Name = "buttonCancel";
            this.metroToolTip1.SetToolTip(this.buttonCancel, resources.GetString("buttonCancel.ToolTip"));
            this.toolTip1.SetToolTip(this.buttonCancel, resources.GetString("buttonCancel.ToolTip1"));
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // metroButtonAbort
            // 
            resources.ApplyResources(this.metroButtonAbort, "metroButtonAbort");
            this.metroButtonAbort.DialogResult = System.Windows.Forms.DialogResult.Abort;
            this.metroButtonAbort.Name = "metroButtonAbort";
            this.metroToolTip1.SetToolTip(this.metroButtonAbort, resources.GetString("metroButtonAbort.ToolTip"));
            this.toolTip1.SetToolTip(this.metroButtonAbort, resources.GetString("metroButtonAbort.ToolTip1"));
            this.metroButtonAbort.Click += new System.EventHandler(this.metroButtonAbort_Click);
            // 
            // panel1
            // 
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            this.metroToolTip1.SetToolTip(this.panel1, resources.GetString("panel1.ToolTip"));
            this.toolTip1.SetToolTip(this.panel1, resources.GetString("panel1.ToolTip1"));
            // 
            // tableLayoutPanel2
            // 
            resources.ApplyResources(this.tableLayoutPanel2, "tableLayoutPanel2");
            this.tableLayoutPanel2.Controls.Add(this.metroComboBoxTimbres, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.metroButtonImportAll, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.metroButtonImportAllGit, 3, 0);
            this.tableLayoutPanel2.Controls.Add(this.metroButtonExportAll, 4, 0);
            this.tableLayoutPanel2.Controls.Add(this.metroButtonTimbre, 0, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.toolTip1.SetToolTip(this.tableLayoutPanel2, resources.GetString("tableLayoutPanel2.ToolTip"));
            this.metroToolTip1.SetToolTip(this.tableLayoutPanel2, resources.GetString("tableLayoutPanel2.ToolTip1"));
            // 
            // metroComboBoxTimbres
            // 
            resources.ApplyResources(this.metroComboBoxTimbres, "metroComboBoxTimbres");
            this.metroComboBoxTimbres.FormattingEnabled = true;
            this.metroComboBoxTimbres.Name = "metroComboBoxTimbres";
            this.toolTip1.SetToolTip(this.metroComboBoxTimbres, resources.GetString("metroComboBoxTimbres.ToolTip"));
            this.metroToolTip1.SetToolTip(this.metroComboBoxTimbres, resources.GetString("metroComboBoxTimbres.ToolTip1"));
            this.metroComboBoxTimbres.SelectedIndexChanged += new System.EventHandler(this.metroComboBoxTimbres_SelectedIndexChanged);
            // 
            // metroButtonImportAll
            // 
            resources.ApplyResources(this.metroButtonImportAll, "metroButtonImportAll");
            this.metroButtonImportAll.AllowDrop = true;
            this.metroButtonImportAll.Name = "metroButtonImportAll";
            this.metroToolTip1.SetToolTip(this.metroButtonImportAll, resources.GetString("metroButtonImportAll.ToolTip"));
            this.toolTip1.SetToolTip(this.metroButtonImportAll, resources.GetString("metroButtonImportAll.ToolTip1"));
            this.metroButtonImportAll.Click += new System.EventHandler(this.metroButtonImportAll_Click);
            this.metroButtonImportAll.DragDrop += new System.Windows.Forms.DragEventHandler(this.metroButtonImportAll_DragDrop);
            this.metroButtonImportAll.DragEnter += new System.Windows.Forms.DragEventHandler(this.metroButtonImportAll_DragEnter);
            // 
            // metroButtonImportAllGit
            // 
            resources.ApplyResources(this.metroButtonImportAllGit, "metroButtonImportAllGit");
            this.metroButtonImportAllGit.Name = "metroButtonImportAllGit";
            this.metroToolTip1.SetToolTip(this.metroButtonImportAllGit, resources.GetString("metroButtonImportAllGit.ToolTip"));
            this.toolTip1.SetToolTip(this.metroButtonImportAllGit, resources.GetString("metroButtonImportAllGit.ToolTip1"));
            this.metroButtonImportAllGit.Click += new System.EventHandler(this.metroButtonImportAllGit_Click);
            // 
            // metroButtonExportAll
            // 
            resources.ApplyResources(this.metroButtonExportAll, "metroButtonExportAll");
            this.metroButtonExportAll.Name = "metroButtonExportAll";
            this.metroToolTip1.SetToolTip(this.metroButtonExportAll, resources.GetString("metroButtonExportAll.ToolTip"));
            this.toolTip1.SetToolTip(this.metroButtonExportAll, resources.GetString("metroButtonExportAll.ToolTip1"));
            this.metroButtonExportAll.Click += new System.EventHandler(this.metroButtonExportAll_Click);
            // 
            // metroButtonTimbre
            // 
            resources.ApplyResources(this.metroButtonTimbre, "metroButtonTimbre");
            this.metroButtonTimbre.Name = "metroButtonTimbre";
            this.metroToolTip1.SetToolTip(this.metroButtonTimbre, resources.GetString("metroButtonTimbre.ToolTip"));
            this.toolTip1.SetToolTip(this.metroButtonTimbre, resources.GetString("metroButtonTimbre.ToolTip1"));
            this.metroButtonTimbre.Click += new System.EventHandler(this.metroButtonTimbre_Click);
            // 
            // tableLayoutPanel3
            // 
            resources.ApplyResources(this.tableLayoutPanel3, "tableLayoutPanel3");
            this.tableLayoutPanel3.Controls.Add(this.metroButtonAbort, 4, 1);
            this.tableLayoutPanel3.Controls.Add(this.buttonCancel, 3, 1);
            this.tableLayoutPanel3.Controls.Add(this.buttonOK, 2, 1);
            this.tableLayoutPanel3.Controls.Add(this.checkBoxHidePropAre, 0, 1);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.toolTip1.SetToolTip(this.tableLayoutPanel3, resources.GetString("tableLayoutPanel3.ToolTip"));
            this.metroToolTip1.SetToolTip(this.tableLayoutPanel3, resources.GetString("tableLayoutPanel3.ToolTip1"));
            // 
            // checkBoxHidePropAre
            // 
            resources.ApplyResources(this.checkBoxHidePropAre, "checkBoxHidePropAre");
            this.checkBoxHidePropAre.Checked = global::zanac.MAmidiMEmo.Properties.Settings.Default.FmHideProp;
            this.checkBoxHidePropAre.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::zanac.MAmidiMEmo.Properties.Settings.Default, "FmHideProp", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.checkBoxHidePropAre.Name = "checkBoxHidePropAre";
            this.toolTip1.SetToolTip(this.checkBoxHidePropAre, resources.GetString("checkBoxHidePropAre.ToolTip"));
            this.metroToolTip1.SetToolTip(this.checkBoxHidePropAre, resources.GetString("checkBoxHidePropAre.ToolTip1"));
            this.checkBoxHidePropAre.UseVisualStyleBackColor = true;
            // 
            // metroToolTip1
            // 
            this.metroToolTip1.Style = MetroFramework.MetroColorStyle.Blue;
            this.metroToolTip1.Theme = MetroFramework.MetroThemeStyle.Light;
            // 
            // FormFmEditor
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.buttonCancel;
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.panelPiano);
            this.Controls.Add(this.tableLayoutPanel2);
            this.Controls.Add(this.tableLayoutPanel3);
            this.Controls.Add(this.panel1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormFmEditor";
            this.ShowIcon = false;
            this.metroToolTip1.SetToolTip(this, resources.GetString("$this.ToolTip"));
            this.toolTip1.SetToolTip(this, resources.GetString("$this.ToolTip1"));
            this.Activated += new System.EventHandler(this.FormFmEditor_Activated);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.contextMenuStripProp.ResumeLayout(false);
            this.panelPiano.ResumeLayout(false);
            this.panelPiano.PerformLayout();
            this.toolStrip2.ResumeLayout(false);
            this.toolStrip2.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
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
        private MetroFramework.Controls.MetroButton metroButtonImportGit;
        private System.Windows.Forms.ToolStripButton toolStripButtonHook;
        private MetroFramework.Components.MetroToolTip metroToolTip1;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private MetroFramework.Controls.MetroButton metroButtonAbort;
        private MetroFramework.Controls.MetroButton metroButtonCopy;
        private MetroFramework.Controls.MetroButton metroButtonPaste;
        private MetroFramework.Controls.MetroButton metroButtonExport;
        private MetroFramework.Controls.MetroTextBox metroTextBoxTargetMinMax;
        private MetroFramework.Controls.MetroLabel metroLabel1;
        private MetroFramework.Controls.MetroTextBox metroTextBoxPatchFile;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private MetroFramework.Controls.MetroComboBox metroComboBoxTimbres;
        private MetroFramework.Controls.MetroButton metroButtonImportAll;
        private MetroFramework.Controls.MetroButton metroButtonImportAllGit;
        private MetroFramework.Controls.MetroButton metroButtonExportAll;
        private MetroFramework.Controls.MetroButton metroButtonTimbre;
        private System.Windows.Forms.PropertyGrid propertyGrid;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripProp;
        private System.Windows.Forms.ToolStripMenuItem resetToDefaultThisPropertyToolStripMenuItem;
        private System.Windows.Forms.CheckBox checkBoxHidePropAre;
    }
}