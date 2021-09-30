using FastColoredTextBoxNS;

namespace zanac.MAmidiMEmo.Gui
{
    partial class FormTextEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormTextEditor));
            this.buttonCancel = new MetroFramework.Controls.MetroButton();
            this.buttonOk = new MetroFramework.Controls.MetroButton();
            this.textBoxWsgDataText = new FastColoredTextBoxNS.FastColoredTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.textBoxWsgDataText)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(761, 614);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(4);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 2;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseSelectable = true;
            // 
            // buttonOk
            // 
            this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOk.Location = new System.Drawing.Point(678, 614);
            this.buttonOk.Margin = new System.Windows.Forms.Padding(4);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 1;
            this.buttonOk.Text = "&OK";
            this.buttonOk.UseSelectable = true;
            // 
            // textBoxWsgDataText
            // 
            this.textBoxWsgDataText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxWsgDataText.AutoCompleteBracketsList = new char[] {
        '(',
        ')',
        '{',
        '}',
        '[',
        ']',
        '\"',
        '\"',
        '\'',
        '\''};
            this.textBoxWsgDataText.AutoIndentCharsPatterns = "\r\n^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;]+);\r\n";
            this.textBoxWsgDataText.AutoScrollMinSize = new System.Drawing.Size(27, 14);
            this.textBoxWsgDataText.BackBrush = null;
            this.textBoxWsgDataText.BracketsHighlightStrategy = FastColoredTextBoxNS.BracketsHighlightStrategy.Strategy2;
            this.textBoxWsgDataText.CharHeight = 14;
            this.textBoxWsgDataText.CharWidth = 8;
            this.textBoxWsgDataText.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.textBoxWsgDataText.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.textBoxWsgDataText.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.textBoxWsgDataText.IsReplaceMode = false;
            this.textBoxWsgDataText.Language = FastColoredTextBoxNS.Language.JS;
            this.textBoxWsgDataText.LeftBracket = '(';
            this.textBoxWsgDataText.LeftBracket2 = '{';
            this.textBoxWsgDataText.Location = new System.Drawing.Point(9, 64);
            this.textBoxWsgDataText.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxWsgDataText.Name = "textBoxWsgDataText";
            this.textBoxWsgDataText.Paddings = new System.Windows.Forms.Padding(0);
            this.textBoxWsgDataText.RightBracket = ')';
            this.textBoxWsgDataText.RightBracket2 = '}';
            this.textBoxWsgDataText.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            this.textBoxWsgDataText.ServiceColors = ((FastColoredTextBoxNS.ServiceColors)(resources.GetObject("textBoxWsgDataText.ServiceColors")));
            this.textBoxWsgDataText.ShowFoldingLines = true;
            this.textBoxWsgDataText.Size = new System.Drawing.Size(827, 542);
            this.textBoxWsgDataText.TabIndex = 0;
            this.textBoxWsgDataText.Zoom = 100;
            // 
            // FormTextEditor
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(846, 650);
            this.Controls.Add(this.textBoxWsgDataText);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.buttonCancel);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.MinimizeBox = false;
            this.Name = "FormTextEditor";
            this.Padding = new System.Windows.Forms.Padding(6, 60, 6, 12);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Text Editor";
            ((System.ComponentModel.ISupportInitialize)(this.textBoxWsgDataText)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private MetroFramework.Controls.MetroButton buttonCancel;
        private MetroFramework.Controls.MetroButton buttonOk;
        private FastColoredTextBoxNS.FastColoredTextBox textBoxWsgDataText;
    }
}