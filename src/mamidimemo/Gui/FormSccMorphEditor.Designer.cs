namespace zanac.MAmidiMEmo.Gui
{
    partial class FormSccMorphEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSccMorphEditor));
            this.buttonCancel = new MetroFramework.Controls.MetroButton();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.buttonOk = new MetroFramework.Controls.MetroButton();
            this.metroButtonAdd = new MetroFramework.Controls.MetroButton();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.listBoxWsgList = new System.Windows.Forms.ListBox();
            this.metroButtonDelete = new MetroFramework.Controls.MetroButton();
            this.metroButtonInsert = new MetroFramework.Controls.MetroButton();
            this.metroButtonDuplicate = new MetroFramework.Controls.MetroButton();
            this.metroButtonInterpolate = new MetroFramework.Controls.MetroButton();
            this.metroButtonCopy = new MetroFramework.Controls.MetroButton();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this.buttonCancel, "buttonCancel");
            this.buttonCancel.Name = "buttonCancel";
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.buttonCancel, 6, 3);
            this.tableLayoutPanel1.Controls.Add(this.buttonOk, 5, 3);
            this.tableLayoutPanel1.Controls.Add(this.metroButtonAdd, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.metroButtonDelete, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.metroButtonInsert, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.metroButtonDuplicate, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.metroButtonInterpolate, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.metroButtonCopy, 5, 1);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // buttonOk
            // 
            this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            resources.ApplyResources(this.buttonOk, "buttonOk");
            this.buttonOk.Name = "buttonOk";
            // 
            // metroButtonAdd
            // 
            resources.ApplyResources(this.metroButtonAdd, "metroButtonAdd");
            this.metroButtonAdd.Name = "metroButtonAdd";
            this.metroButtonAdd.Click += new System.EventHandler(this.metroButtonAdd_Click);
            // 
            // tableLayoutPanel2
            // 
            resources.ApplyResources(this.tableLayoutPanel2, "tableLayoutPanel2");
            this.tableLayoutPanel1.SetColumnSpan(this.tableLayoutPanel2, 7);
            this.tableLayoutPanel2.Controls.Add(this.propertyGrid1, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.listBoxWsgList, 0, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            // 
            // propertyGrid1
            // 
            resources.ApplyResources(this.propertyGrid1, "propertyGrid1");
            this.propertyGrid1.Name = "propertyGrid1";
            // 
            // listBoxWsgList
            // 
            resources.ApplyResources(this.listBoxWsgList, "listBoxWsgList");
            this.listBoxWsgList.FormattingEnabled = true;
            this.listBoxWsgList.Name = "listBoxWsgList";
            this.listBoxWsgList.SelectedValueChanged += new System.EventHandler(this.listBoxWsgList_SelectedValueChanged);
            // 
            // metroButtonDelete
            // 
            resources.ApplyResources(this.metroButtonDelete, "metroButtonDelete");
            this.metroButtonDelete.Name = "metroButtonDelete";
            this.metroButtonDelete.Click += new System.EventHandler(this.metroButtonDelete_Click);
            // 
            // metroButtonInsert
            // 
            resources.ApplyResources(this.metroButtonInsert, "metroButtonInsert");
            this.metroButtonInsert.Name = "metroButtonInsert";
            this.metroButtonInsert.Click += new System.EventHandler(this.metroButtonInsert_Click);
            // 
            // metroButtonDuplicate
            // 
            resources.ApplyResources(this.metroButtonDuplicate, "metroButtonDuplicate");
            this.metroButtonDuplicate.Name = "metroButtonDuplicate";
            this.metroButtonDuplicate.Click += new System.EventHandler(this.metroButtonDuplicate_Click);
            // 
            // metroButtonInterpolate
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.metroButtonInterpolate, 2);
            resources.ApplyResources(this.metroButtonInterpolate, "metroButtonInterpolate");
            this.metroButtonInterpolate.Name = "metroButtonInterpolate";
            this.toolTip1.SetToolTip(this.metroButtonInterpolate, resources.GetString("metroButtonInterpolate.ToolTip"));
            this.metroButtonInterpolate.Click += new System.EventHandler(this.metroButtonInterpolate_Click);
            // 
            // metroButtonCopy
            // 
            resources.ApplyResources(this.metroButtonCopy, "metroButtonCopy");
            this.metroButtonCopy.Name = "metroButtonCopy";
            this.metroButtonCopy.Click += new System.EventHandler(this.metroButtonCopy_Click);
            // 
            // FormSccMorphEditor
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.buttonCancel;
            this.Controls.Add(this.tableLayoutPanel1);
            this.MinimizeBox = false;
            this.Name = "FormSccMorphEditor";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private MetroFramework.Controls.MetroButton buttonCancel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private MetroFramework.Controls.MetroButton buttonOk;
        private System.Windows.Forms.ListBox listBoxWsgList;
        private System.Windows.Forms.PropertyGrid propertyGrid1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private MetroFramework.Controls.MetroButton metroButtonAdd;
        private MetroFramework.Controls.MetroButton metroButtonDelete;
        private MetroFramework.Controls.MetroButton metroButtonInterpolate;
        private MetroFramework.Controls.MetroButton metroButtonInsert;
        private MetroFramework.Controls.MetroButton metroButtonDuplicate;
        private MetroFramework.Controls.MetroButton metroButtonCopy;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}