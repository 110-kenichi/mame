﻿namespace zanac.MAmidiMEmo.Gui
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
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOk = new System.Windows.Forms.Button();
            this.textBoxWsgDataText = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // graphControl
            // 
            this.graphControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.graphControl.ByteInstance = null;
            this.graphControl.Location = new System.Drawing.Point(12, 12);
            this.graphControl.Name = "graphControl";
            this.graphControl.ResultOfWsgData = new byte[] {
        ((byte)(0)),
        ((byte)(1)),
        ((byte)(2)),
        ((byte)(3)),
        ((byte)(4)),
        ((byte)(5)),
        ((byte)(6)),
        ((byte)(7)),
        ((byte)(8)),
        ((byte)(9)),
        ((byte)(10)),
        ((byte)(11)),
        ((byte)(12)),
        ((byte)(13)),
        ((byte)(14)),
        ((byte)(15))};
            this.graphControl.SbyteInstance = null;
            this.graphControl.Size = new System.Drawing.Size(485, 357);
            this.graphControl.TabIndex = 0;
            this.graphControl.WsgMaxValue = 15;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(422, 400);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOk
            // 
            this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOk.Location = new System.Drawing.Point(341, 400);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 1;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            // 
            // textBoxWsgDataText
            // 
            this.textBoxWsgDataText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxWsgDataText.Location = new System.Drawing.Point(12, 375);
            this.textBoxWsgDataText.Name = "textBoxWsgDataText";
            this.textBoxWsgDataText.Size = new System.Drawing.Size(485, 19);
            this.textBoxWsgDataText.TabIndex = 2;
            this.textBoxWsgDataText.TextChanged += new System.EventHandler(this.textBoxWsgDataText_TextChanged);
            // 
            // FormWsgEditor
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(509, 435);
            this.Controls.Add(this.textBoxWsgDataText);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.graphControl);
            this.MinimizeBox = false;
            this.Name = "FormWsgEditor";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "WSG Editor";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private GraphControl graphControl;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.TextBox textBoxWsgDataText;
    }
}