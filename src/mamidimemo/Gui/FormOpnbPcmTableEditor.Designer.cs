using zanac.MAmidiMEmo.Instruments;

namespace zanac.MAmidiMEmo.Gui
{
    partial class FormOpnbPcmTableEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormPcmTableEditor));
            this.listViewPcmSounds = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.buttonCancel = new MetroFramework.Controls.MetroButton();
            this.buttonOk = new MetroFramework.Controls.MetroButton();
            this.buttonDelete = new MetroFramework.Controls.MetroButton();
            this.buttonAdd = new MetroFramework.Controls.MetroButton();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.label1 = new MetroFramework.Controls.MetroLabel();
            this.label2 = new MetroFramework.Controls.MetroLabel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // listViewPcmSounds
            // 
            this.listViewPcmSounds.AllowDrop = true;
            this.listViewPcmSounds.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.tableLayoutPanel2.SetColumnSpan(this.listViewPcmSounds, 3);
            resources.ApplyResources(this.listViewPcmSounds, "listViewPcmSounds");
            this.listViewPcmSounds.FullRowSelect = true;
            this.listViewPcmSounds.GridLines = true;
            this.listViewPcmSounds.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewPcmSounds.HideSelection = false;
            this.listViewPcmSounds.Name = "listViewPcmSounds";
            this.listViewPcmSounds.UseCompatibleStateImageBehavior = false;
            this.listViewPcmSounds.View = System.Windows.Forms.View.Details;
            this.listViewPcmSounds.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.listViewPcmSounds_ItemSelectionChanged);
            this.listViewPcmSounds.DragDrop += new System.Windows.Forms.DragEventHandler(this.listViewPcmSounds_DragDrop);
            this.listViewPcmSounds.DragEnter += new System.Windows.Forms.DragEventHandler(this.listViewPcmSounds_DragEnter);
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
            // 
            // buttonOk
            // 
            resources.ApplyResources(this.buttonOk, "buttonOk");
            this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOk.Name = "buttonOk";
            // 
            // buttonDelete
            // 
            resources.ApplyResources(this.buttonDelete, "buttonDelete");
            this.buttonDelete.Name = "buttonDelete";
            this.buttonDelete.Click += new System.EventHandler(this.buttonDelete_Click);
            // 
            // buttonAdd
            // 
            resources.ApplyResources(this.buttonAdd, "buttonAdd");
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.SupportMultiDottedExtensions = true;
            resources.ApplyResources(this.openFileDialog1, "openFileDialog1");
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.tableLayoutPanel2.SetColumnSpan(this.label1, 3);
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
            this.tableLayoutPanel1.Controls.Add(this.buttonOk, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.buttonCancel, 2, 1);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // tableLayoutPanel2
            // 
            resources.ApplyResources(this.tableLayoutPanel2, "tableLayoutPanel2");
            this.tableLayoutPanel2.Controls.Add(this.propertyGrid1, 3, 1);
            this.tableLayoutPanel2.Controls.Add(this.buttonAdd, 1, 2);
            this.tableLayoutPanel2.Controls.Add(this.listViewPcmSounds, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.buttonDelete, 2, 2);
            this.tableLayoutPanel2.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.label2, 3, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            // 
            // FormPcmTableEditor
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.buttonCancel;
            this.Controls.Add(this.tableLayoutPanel2);
            this.Controls.Add(this.tableLayoutPanel1);
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormPcmTableEditor";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listViewPcmSounds;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.PropertyGrid propertyGrid1;
        private MetroFramework.Controls.MetroButton buttonCancel;
        private MetroFramework.Controls.MetroButton buttonOk;
        private MetroFramework.Controls.MetroButton buttonDelete;
        private MetroFramework.Controls.MetroButton buttonAdd;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private MetroFramework.Controls.MetroLabel label1;
        private MetroFramework.Controls.MetroLabel label2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
    }
}