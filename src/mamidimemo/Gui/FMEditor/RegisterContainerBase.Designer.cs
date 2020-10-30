namespace zanac.MAmidiMEmo.Gui.FMEditor
{
    partial class RegisterContainerBase
    {
        /// <summary> 
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region コンポーネント デザイナーで生成されたコード

        /// <summary> 
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を 
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new MetroFramework.Controls.MetroLabel();
            this.textBoxSR = new MetroFramework.Controls.MetroTextBox();
            this.labelName = new zanac.MAmidiMEmo.Gui.FMEditor.VerticalLabel();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.textBoxSR, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(18, 152);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(237, 25);
            this.tableLayoutPanel1.TabIndex = 13;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(91, 25);
            this.label1.TabIndex = 0;
            this.label1.Text = "Serialize Data:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBoxSR
            // 
            // 
            // 
            // 
            this.textBoxSR.CustomButton.Image = null;
            this.textBoxSR.CustomButton.Location = new System.Drawing.Point(664, 1);
            this.textBoxSR.CustomButton.Name = "";
            this.textBoxSR.CustomButton.Size = new System.Drawing.Size(17, 17);
            this.textBoxSR.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.textBoxSR.CustomButton.TabIndex = 1;
            this.textBoxSR.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.textBoxSR.CustomButton.UseSelectable = true;
            this.textBoxSR.CustomButton.Visible = false;
            this.textBoxSR.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxSR.Lines = new string[0];
            this.textBoxSR.Location = new System.Drawing.Point(100, 3);
            this.textBoxSR.MaxLength = 32767;
            this.textBoxSR.Name = "textBoxSR";
            this.textBoxSR.PasswordChar = '\0';
            this.textBoxSR.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.textBoxSR.SelectedText = "";
            this.textBoxSR.SelectionLength = 0;
            this.textBoxSR.SelectionStart = 0;
            this.textBoxSR.ShortcutsEnabled = true;
            this.textBoxSR.Size = new System.Drawing.Size(134, 19);
            this.textBoxSR.TabIndex = 1;
            this.textBoxSR.UseSelectable = true;
            this.textBoxSR.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.textBoxSR.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            this.textBoxSR.TextChanged += new System.EventHandler(this.textBoxSR_TextChanged);
            // 
            // labelName
            // 
            this.labelName.BackColor = System.Drawing.SystemColors.ControlDark;
            this.labelName.Dock = System.Windows.Forms.DockStyle.Left;
            this.labelName.Location = new System.Drawing.Point(3, 3);
            this.labelName.Name = "labelName";
            this.labelName.RenderingMode = System.Drawing.Text.TextRenderingHint.SystemDefault;
            this.labelName.Size = new System.Drawing.Size(15, 174);
            this.labelName.TabIndex = 0;
            this.labelName.Text = null;
            this.labelName.TextDrawMode = zanac.MAmidiMEmo.Gui.FMEditor.DrawMode.TopBottom;
            this.labelName.TransparentBackground = false;
            // 
            // RegisterContainerBase
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.labelName);
            this.MinimumSize = new System.Drawing.Size(0, 180);
            this.Name = "RegisterContainerBase";
            this.Padding = new System.Windows.Forms.Padding(3);
            this.Size = new System.Drawing.Size(258, 180);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private MetroFramework.Controls.MetroLabel label1;
        private MetroFramework.Controls.MetroTextBox textBoxSR;
        private zanac.MAmidiMEmo.Gui.FMEditor.VerticalLabel labelName;
    }
}
