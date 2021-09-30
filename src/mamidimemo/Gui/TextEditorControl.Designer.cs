using FastColoredTextBoxNS;

namespace zanac.MAmidiMEmo.Gui
{
    partial class TextEditorControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TextEditorControl));
            this.textBoxWsgDataText = new FastColoredTextBoxNS.FastColoredTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.textBoxWsgDataText)).BeginInit();
            this.SuspendLayout();
            // 
            // textBoxWsgDataText
            // 
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
            this.textBoxWsgDataText.AutoScrollMinSize = new System.Drawing.Size(2, 14);
            this.textBoxWsgDataText.BackBrush = null;
            this.textBoxWsgDataText.CharHeight = 14;
            this.textBoxWsgDataText.CharWidth = 8;
            this.textBoxWsgDataText.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.textBoxWsgDataText.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.textBoxWsgDataText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxWsgDataText.IsReplaceMode = false;
            this.textBoxWsgDataText.Language = FastColoredTextBoxNS.Language.JS;
            this.textBoxWsgDataText.Location = new System.Drawing.Point(0, 0);
            this.textBoxWsgDataText.Name = "textBoxWsgDataText";
            this.textBoxWsgDataText.Paddings = new System.Windows.Forms.Padding(0);
            this.textBoxWsgDataText.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            this.textBoxWsgDataText.Size = new System.Drawing.Size(613, 612);
            this.textBoxWsgDataText.TabIndex = 2;
            this.textBoxWsgDataText.Zoom = 100;
            // 
            // TextEditorControl
            // 
            this.Controls.Add(this.textBoxWsgDataText);
            this.Size = new System.Drawing.Size(613, 612);
            ((System.ComponentModel.ISupportInitialize)(this.textBoxWsgDataText)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private FastColoredTextBox textBoxWsgDataText;
    }
}