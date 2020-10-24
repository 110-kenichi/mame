namespace zanac.MAmidiMEmo.Gui
{
    partial class FormWsgEditor
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
            this.graphControl = new zanac.MAmidiMEmo.Gui.FormWsgEditor.GraphControl();
            this.buttonCancel = new MetroFramework.Controls.MetroButton();
            this.buttonOk = new MetroFramework.Controls.MetroButton();
            this.textBoxWsgDataText = new MetroFramework.Controls.MetroTextBox();
            this.SuspendLayout();
            // 
            // graphControl
            // 
            this.graphControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.graphControl.Location = new System.Drawing.Point(8, 63);
            this.graphControl.Name = "graphControl";
            this.graphControl.ResultOfWsgData = null;
            this.graphControl.Size = new System.Drawing.Size(767, 435);
            this.graphControl.TabIndex = 0;
            this.graphControl.WsgBitWide = 4;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(700, 530);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseSelectable = true;
            // 
            // buttonOk
            // 
            this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOk.Location = new System.Drawing.Point(619, 530);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 2;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseSelectable = true;
            // 
            // textBoxWsgDataText
            // 
            this.textBoxWsgDataText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            // 
            // 
            // 
            this.textBoxWsgDataText.CustomButton.Image = null;
            this.textBoxWsgDataText.CustomButton.Location = new System.Drawing.Point(748, 2);
            this.textBoxWsgDataText.CustomButton.Name = "";
            this.textBoxWsgDataText.CustomButton.Size = new System.Drawing.Size(15, 15);
            this.textBoxWsgDataText.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.textBoxWsgDataText.CustomButton.TabIndex = 1;
            this.textBoxWsgDataText.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.textBoxWsgDataText.CustomButton.UseSelectable = true;
            this.textBoxWsgDataText.CustomButton.Visible = false;
            this.textBoxWsgDataText.Lines = new string[0];
            this.textBoxWsgDataText.Location = new System.Drawing.Point(9, 504);
            this.textBoxWsgDataText.MaxLength = 32767;
            this.textBoxWsgDataText.Name = "textBoxWsgDataText";
            this.textBoxWsgDataText.PasswordChar = '\0';
            this.textBoxWsgDataText.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.textBoxWsgDataText.SelectedText = "";
            this.textBoxWsgDataText.SelectionLength = 0;
            this.textBoxWsgDataText.SelectionStart = 0;
            this.textBoxWsgDataText.ShortcutsEnabled = true;
            this.textBoxWsgDataText.Size = new System.Drawing.Size(766, 20);
            this.textBoxWsgDataText.TabIndex = 1;
            this.textBoxWsgDataText.UseSelectable = true;
            this.textBoxWsgDataText.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.textBoxWsgDataText.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            this.textBoxWsgDataText.TextChanged += new System.EventHandler(this.textBoxWsgDataText_TextChanged);
            // 
            // FormWsgEditor
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.textBoxWsgDataText);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.graphControl);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.MinimizeBox = false;
            this.Name = "FormWsgEditor";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "WSG Editor";
            this.ResumeLayout(false);

        }

        #endregion

        private GraphControl graphControl;
        private MetroFramework.Controls.MetroButton buttonCancel;
        private MetroFramework.Controls.MetroButton buttonOk;
        private MetroFramework.Controls.MetroTextBox textBoxWsgDataText;
    }
}