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
            this.buttonCancel = new MetroFramework.Controls.MetroButton();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.buttonOk = new MetroFramework.Controls.MetroButton();
            this.metroButtonAdd = new MetroFramework.Controls.MetroButton();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.listBoxWsgList = new System.Windows.Forms.ListBox();
            this.metroButtonInterpolate = new MetroFramework.Controls.MetroButton();
            this.metroButtonDelete = new MetroFramework.Controls.MetroButton();
            this.metroButtonInsert = new MetroFramework.Controls.MetroButton();
            this.metroButtonDuplicate = new MetroFramework.Controls.MetroButton();
            this.metroButtonCopy = new MetroFramework.Controls.MetroButton();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonCancel.Location = new System.Drawing.Point(678, 458);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(4);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(100, 29);
            this.buttonCancel.TabIndex = 6;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseSelectable = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 7;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.buttonCancel, 6, 3);
            this.tableLayoutPanel1.Controls.Add(this.buttonOk, 5, 3);
            this.tableLayoutPanel1.Controls.Add(this.metroButtonAdd, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.metroButtonDelete, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.metroButtonInsert, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.metroButtonDuplicate, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.metroButtonInterpolate, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.metroButtonCopy, 5, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(9, 94);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(782, 491);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // buttonOk
            // 
            this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOk.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonOk.Location = new System.Drawing.Point(570, 458);
            this.buttonOk.Margin = new System.Windows.Forms.Padding(4);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(100, 29);
            this.buttonOk.TabIndex = 5;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseSelectable = true;
            // 
            // metroButtonAdd
            // 
            this.metroButtonAdd.Location = new System.Drawing.Point(4, 384);
            this.metroButtonAdd.Margin = new System.Windows.Forms.Padding(4);
            this.metroButtonAdd.Name = "metroButtonAdd";
            this.metroButtonAdd.Size = new System.Drawing.Size(100, 29);
            this.metroButtonAdd.TabIndex = 0;
            this.metroButtonAdd.Text = "&Add new";
            this.metroButtonAdd.UseSelectable = true;
            this.metroButtonAdd.Click += new System.EventHandler(this.metroButtonAdd_Click);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel1.SetColumnSpan(this.tableLayoutPanel2, 7);
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.propertyGrid1, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.listBoxWsgList, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(776, 374);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid1.Location = new System.Drawing.Point(391, 3);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.Size = new System.Drawing.Size(382, 368);
            this.propertyGrid1.TabIndex = 1;
            // 
            // listBoxWsgList
            // 
            this.listBoxWsgList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxWsgList.FormattingEnabled = true;
            this.listBoxWsgList.ItemHeight = 15;
            this.listBoxWsgList.Location = new System.Drawing.Point(3, 3);
            this.listBoxWsgList.Name = "listBoxWsgList";
            this.listBoxWsgList.Size = new System.Drawing.Size(382, 368);
            this.listBoxWsgList.TabIndex = 0;
            this.listBoxWsgList.SelectedValueChanged += new System.EventHandler(this.listBoxWsgList_SelectedValueChanged);
            // 
            // metroButtonInterpolate
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.metroButtonInterpolate, 2);
            this.metroButtonInterpolate.Location = new System.Drawing.Point(4, 421);
            this.metroButtonInterpolate.Margin = new System.Windows.Forms.Padding(4);
            this.metroButtonInterpolate.Name = "metroButtonInterpolate";
            this.metroButtonInterpolate.Size = new System.Drawing.Size(185, 29);
            this.metroButtonInterpolate.TabIndex = 4;
            this.metroButtonInterpolate.Text = "Interpolate &2 WSGs...";
            this.metroButtonInterpolate.UseSelectable = true;
            this.metroButtonInterpolate.Click += new System.EventHandler(this.metroButtonInterpolate_Click);
            // 
            // metroButtonDelete
            // 
            this.metroButtonDelete.Location = new System.Drawing.Point(328, 384);
            this.metroButtonDelete.Margin = new System.Windows.Forms.Padding(4);
            this.metroButtonDelete.Name = "metroButtonDelete";
            this.metroButtonDelete.Size = new System.Drawing.Size(100, 29);
            this.metroButtonDelete.TabIndex = 3;
            this.metroButtonDelete.Text = "&Delete";
            this.metroButtonDelete.UseSelectable = true;
            this.metroButtonDelete.Click += new System.EventHandler(this.metroButtonDelete_Click);
            // 
            // metroButtonInsert
            // 
            this.metroButtonInsert.Location = new System.Drawing.Point(112, 384);
            this.metroButtonInsert.Margin = new System.Windows.Forms.Padding(4);
            this.metroButtonInsert.Name = "metroButtonInsert";
            this.metroButtonInsert.Size = new System.Drawing.Size(100, 29);
            this.metroButtonInsert.TabIndex = 1;
            this.metroButtonInsert.Text = "&Insert new";
            this.metroButtonInsert.UseSelectable = true;
            this.metroButtonInsert.Click += new System.EventHandler(this.metroButtonInsert_Click);
            // 
            // metroButtonDuplicate
            // 
            this.metroButtonDuplicate.Location = new System.Drawing.Point(220, 384);
            this.metroButtonDuplicate.Margin = new System.Windows.Forms.Padding(4);
            this.metroButtonDuplicate.Name = "metroButtonDuplicate";
            this.metroButtonDuplicate.Size = new System.Drawing.Size(100, 29);
            this.metroButtonDuplicate.TabIndex = 2;
            this.metroButtonDuplicate.Text = "D&uplicate";
            this.metroButtonDuplicate.UseSelectable = true;
            this.metroButtonDuplicate.Click += new System.EventHandler(this.metroButtonDuplicate_Click);
            // 
            // metroButtonCopy
            // 
            this.metroButtonCopy.Location = new System.Drawing.Point(570, 384);
            this.metroButtonCopy.Margin = new System.Windows.Forms.Padding(4);
            this.metroButtonCopy.Name = "metroButtonCopy";
            this.metroButtonCopy.Size = new System.Drawing.Size(100, 29);
            this.metroButtonCopy.TabIndex = 0;
            this.metroButtonCopy.Text = "&Copy Hex values";
            this.metroButtonCopy.UseSelectable = true;
            this.metroButtonCopy.Click += new System.EventHandler(this.metroButtonCopy_Click);
            // 
            // FormSccMorphEditor
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.MinimizeBox = false;
            this.Name = "FormSccMorphEditor";
            this.Padding = new System.Windows.Forms.Padding(9, 94, 9, 15);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "SCC WSG Morph Editor";
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
    }
}