using zanac.MAmidiMEmo.Instruments;

namespace zanac.MAmidiMEmo.Gui
{
    partial class FormDrumTableEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormDrumTableEditor));
            this.listViewPcmSounds = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.buttonCancel = new MetroFramework.Controls.MetroButton();
            this.buttonOk = new MetroFramework.Controls.MetroButton();
            this.label1 = new MetroFramework.Controls.MetroLabel();
            this.label2 = new MetroFramework.Controls.MetroLabel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.metroButton3 = new MetroFramework.Controls.MetroButton();
            this.metroButtonImport = new MetroFramework.Controls.MetroButton();
            this.metroButtonExport = new MetroFramework.Controls.MetroButton();
            this.openFileDialogImport = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialogExport = new System.Windows.Forms.SaveFileDialog();
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
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.clearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.toolStrip2.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listViewPcmSounds
            // 
            resources.ApplyResources(this.listViewPcmSounds, "listViewPcmSounds");
            this.listViewPcmSounds.AllowDrop = true;
            this.listViewPcmSounds.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.listViewPcmSounds.FullRowSelect = true;
            this.listViewPcmSounds.GridLines = true;
            this.listViewPcmSounds.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewPcmSounds.HideSelection = false;
            this.listViewPcmSounds.Name = "listViewPcmSounds";
            this.listViewPcmSounds.UseCompatibleStateImageBehavior = false;
            this.listViewPcmSounds.View = System.Windows.Forms.View.Details;
            this.listViewPcmSounds.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.listViewPcmSounds_ItemDrag);
            this.listViewPcmSounds.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.listViewPcmSounds_ItemSelectionChanged);
            this.listViewPcmSounds.DragDrop += new System.Windows.Forms.DragEventHandler(this.listViewPcmSounds_DragDrop);
            this.listViewPcmSounds.DragEnter += new System.Windows.Forms.DragEventHandler(this.listViewPcmSounds_DragOver);
            this.listViewPcmSounds.DragOver += new System.Windows.Forms.DragEventHandler(this.listViewPcmSounds_DragOver);
            // 
            // columnHeader1
            // 
            resources.ApplyResources(this.columnHeader1, "columnHeader1");
            // 
            // columnHeader2
            // 
            resources.ApplyResources(this.columnHeader2, "columnHeader2");
            // 
            // propertyGrid1
            // 
            resources.ApplyResources(this.propertyGrid1, "propertyGrid1");
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGrid1_PropertyValueChanged);
            // 
            // buttonCancel
            // 
            resources.ApplyResources(this.buttonCancel, "buttonCancel");
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.UseSelectable = true;
            // 
            // buttonOk
            // 
            resources.ApplyResources(this.buttonOk, "buttonOk");
            this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.UseSelectable = true;
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.buttonOk, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.buttonCancel, 4, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // tableLayoutPanel2
            // 
            resources.ApplyResources(this.tableLayoutPanel2, "tableLayoutPanel2");
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel4, 0, 3);
            this.tableLayoutPanel2.Controls.Add(this.propertyGrid1, 1, 2);
            this.tableLayoutPanel2.Controls.Add(this.listViewPcmSounds, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.label2, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            // 
            // tableLayoutPanel4
            // 
            resources.ApplyResources(this.tableLayoutPanel4, "tableLayoutPanel4");
            this.tableLayoutPanel4.Controls.Add(this.metroButton3, 2, 0);
            this.tableLayoutPanel4.Controls.Add(this.metroButtonImport, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.metroButtonExport, 1, 0);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            // 
            // metroButton3
            // 
            resources.ApplyResources(this.metroButton3, "metroButton3");
            this.metroButton3.Name = "metroButton3";
            this.metroButton3.UseSelectable = true;
            this.metroButton3.Click += new System.EventHandler(this.metroButton3_Click);
            // 
            // metroButtonImport
            // 
            resources.ApplyResources(this.metroButtonImport, "metroButtonImport");
            this.metroButtonImport.Name = "metroButtonImport";
            this.metroButtonImport.UseSelectable = true;
            this.metroButtonImport.Click += new System.EventHandler(this.metroButtonImport_Click);
            // 
            // metroButtonExport
            // 
            resources.ApplyResources(this.metroButtonExport, "metroButtonExport");
            this.metroButtonExport.Name = "metroButtonExport";
            this.metroButtonExport.UseSelectable = true;
            this.metroButtonExport.Click += new System.EventHandler(this.metroButtonExport_Click);
            // 
            // openFileDialogImport
            // 
            resources.ApplyResources(this.openFileDialogImport, "openFileDialogImport");
            // 
            // saveFileDialogExport
            // 
            resources.ApplyResources(this.saveFileDialogExport, "saveFileDialogExport");
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
            // contextMenuStrip1
            // 
            resources.ApplyResources(this.contextMenuStrip1, "contextMenuStrip1");
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.clearToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            // 
            // clearToolStripMenuItem
            // 
            resources.ApplyResources(this.clearToolStripMenuItem, "clearToolStripMenuItem");
            this.clearToolStripMenuItem.Name = "clearToolStripMenuItem";
            this.clearToolStripMenuItem.Click += new System.EventHandler(this.clearToolStripMenuItem_Click);
            // 
            // FormDrumTableEditor
            // 
            this.AcceptButton = this.buttonOk;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.Controls.Add(this.tableLayoutPanel2);
            this.Controls.Add(this.toolStrip2);
            this.Controls.Add(this.pianoControl1);
            this.Controls.Add(this.tableLayoutPanel1);
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormDrumTableEditor";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            this.toolStrip2.ResumeLayout(false);
            this.toolStrip2.PerformLayout();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listViewPcmSounds;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.PropertyGrid propertyGrid1;
        private MetroFramework.Controls.MetroButton buttonCancel;
        private MetroFramework.Controls.MetroButton buttonOk;
        private MetroFramework.Controls.MetroLabel label1;
        private MetroFramework.Controls.MetroLabel label2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.OpenFileDialog openFileDialogImport;
        private System.Windows.Forms.SaveFileDialog saveFileDialogExport;
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
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private MetroFramework.Controls.MetroButton metroButton3;
        private MetroFramework.Controls.MetroButton metroButtonImport;
        private MetroFramework.Controls.MetroButton metroButtonExport;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem clearToolStripMenuItem;
    }
}