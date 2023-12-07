using System.Windows.Forms;

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
            this.components = new System.ComponentModel.Container();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new MetroFramework.Controls.MetroLabel();
            this.textBoxSR = new MetroFramework.Controls.MetroTextBox();
            this.checkBoxFollow = new MetroFramework.Controls.MetroCheckBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.labelName = new zanac.MAmidiMEmo.Gui.FMEditor.VerticalLabel();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.textBoxSR, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.checkBoxFollow, 2, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(18, 102);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(346, 21);
            this.tableLayoutPanel1.TabIndex = 13;
            // 
            // label1
            // 
            this.label1.AllowDrop = true;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(91, 19);
            this.label1.TabIndex = 0;
            this.label1.Text = "Serialize Data:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.toolTip1.SetToolTip(this.label1, "You can Copy/Swap operator values by dragging Serialize Values Label.");
            this.label1.DragDrop += new System.Windows.Forms.DragEventHandler(this.textBoxSR_DragDrop);
            this.label1.DragEnter += new System.Windows.Forms.DragEventHandler(this.textBoxSR_DragEnter);
            this.label1.DragOver += new System.Windows.Forms.DragEventHandler(this.textBoxSR_DragEnter);
            this.label1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.label1_MouseDown);
            // 
            // textBoxSR
            // 
            this.textBoxSR.AllowDrop = true;
            // 
            // 
            // 
            this.textBoxSR.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxSR.Location = new System.Drawing.Point(100, 0);
            this.textBoxSR.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.textBoxSR.MaxLength = 32767;
            this.textBoxSR.Name = "textBoxSR";
            this.textBoxSR.PasswordChar = '\0';
            this.textBoxSR.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.textBoxSR.SelectedText = "";
            this.textBoxSR.Size = new System.Drawing.Size(118, 21);
            this.textBoxSR.TabIndex = 1;
            this.textBoxSR.TextChanged += new System.EventHandler(this.textBoxSR_TextChanged);
            // 
            // checkBoxFollow
            // 
            this.checkBoxFollow.AutoSize = true;
            this.checkBoxFollow.Checked = true;
            this.checkBoxFollow.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxFollow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxFollow.Location = new System.Drawing.Point(224, 3);
            this.checkBoxFollow.Name = "checkBoxFollow";
            this.checkBoxFollow.Size = new System.Drawing.Size(119, 15);
            this.checkBoxFollow.TabIndex = 2;
            this.checkBoxFollow.Text = "Follow";
            // 
            // toolTip1
            // 
            this.toolTip1.IsBalloon = true;
            this.toolTip1.ShowAlways = true;
            this.toolTip1.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            // 
            // labelName
            // 
            this.labelName.BackColor = System.Drawing.SystemColors.ControlDark;
            this.labelName.Dock = System.Windows.Forms.DockStyle.Left;
            this.labelName.Location = new System.Drawing.Point(3, 3);
            this.labelName.Name = "labelName";
            this.labelName.RenderingMode = System.Drawing.Text.TextRenderingHint.SystemDefault;
            this.labelName.Size = new System.Drawing.Size(15, 120);
            this.labelName.TabIndex = 0;
            this.labelName.Text = null;
            this.labelName.TextDrawMode = zanac.MAmidiMEmo.Gui.FMEditor.DrawMode.TopBottom;
            // 
            // RegisterContainerBase
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSize = true;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.labelName);
            this.MinimumSize = new System.Drawing.Size(2, 128);
            this.Name = "RegisterContainerBase";
            this.Padding = new System.Windows.Forms.Padding(3);
            this.Size = new System.Drawing.Size(367, 126);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private MetroFramework.Controls.MetroLabel label1;
        private MetroFramework.Controls.MetroTextBox textBoxSR;
        private zanac.MAmidiMEmo.Gui.FMEditor.VerticalLabel labelName;
        private MetroFramework.Controls.MetroCheckBox checkBoxFollow;
        private ToolTip toolTip1;
    }
}
