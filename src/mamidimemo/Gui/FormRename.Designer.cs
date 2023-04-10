namespace zanac.MAmidiMEmo.Gui
{
    partial class FormRename
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
            this.metroLabelName = new MetroFramework.Controls.MetroLabel();
            this.metroTextBoxText = new MetroFramework.Controls.MetroTextBox();
            this.buttonOK = new MetroFramework.Controls.MetroButton();
            this.buttonCancel = new MetroFramework.Controls.MetroButton();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // metroLabelName
            // 
            this.metroLabelName.AutoSize = true;
            this.metroLabelName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.metroLabelName.Location = new System.Drawing.Point(3, 0);
            this.metroLabelName.Name = "metroLabelName";
            this.metroLabelName.Size = new System.Drawing.Size(50, 31);
            this.metroLabelName.TabIndex = 0;
            this.metroLabelName.Text = "&Name:";
            this.metroLabelName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // metroTextBoxText
            // 
            // 
            // 
            // 
            this.metroTextBoxText.CustomButton.Image = null;
            this.metroTextBoxText.CustomButton.Location = new System.Drawing.Point(426, 1);
            this.metroTextBoxText.CustomButton.Name = "";
            this.metroTextBoxText.CustomButton.Size = new System.Drawing.Size(21, 21);
            this.metroTextBoxText.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.metroTextBoxText.CustomButton.TabIndex = 1;
            this.metroTextBoxText.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.metroTextBoxText.CustomButton.UseSelectable = true;
            this.metroTextBoxText.CustomButton.Visible = false;
            this.metroTextBoxText.Lines = new string[0];
            this.metroTextBoxText.Location = new System.Drawing.Point(59, 3);
            this.metroTextBoxText.MaxLength = 32767;
            this.metroTextBoxText.Name = "metroTextBoxText";
            this.metroTextBoxText.PasswordChar = '\0';
            this.metroTextBoxText.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.metroTextBoxText.SelectedText = "";
            this.metroTextBoxText.SelectionLength = 0;
            this.metroTextBoxText.SelectionStart = 0;
            this.metroTextBoxText.ShortcutsEnabled = true;
            this.metroTextBoxText.Size = new System.Drawing.Size(448, 23);
            this.metroTextBoxText.TabIndex = 1;
            this.metroTextBoxText.UseSelectable = true;
            this.metroTextBoxText.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.metroTextBoxText.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(312, 135);
            this.buttonOK.Margin = new System.Windows.Forms.Padding(4);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(100, 29);
            this.buttonOK.TabIndex = 10;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseSelectable = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(420, 135);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(4);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(100, 29);
            this.buttonCancel.TabIndex = 11;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseSelectable = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.metroLabelName, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.metroTextBoxText, 1, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(10, 78);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(510, 31);
            this.tableLayoutPanel1.TabIndex = 12;
            // 
            // FormRename
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(531, 180);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormRename";
            this.Resizable = false;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Set a name";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private MetroFramework.Controls.MetroLabel metroLabelName;
        private MetroFramework.Controls.MetroTextBox metroTextBoxText;
        private MetroFramework.Controls.MetroButton buttonOK;
        private MetroFramework.Controls.MetroButton buttonCancel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}