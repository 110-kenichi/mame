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
            this.groupBoxOp = new System.Windows.Forms.GroupBox();
            this.SuspendLayout();
            // 
            // groupBoxOp
            // 
            this.groupBoxOp.AutoSize = true;
            this.groupBoxOp.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBoxOp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxOp.Location = new System.Drawing.Point(0, 0);
            this.groupBoxOp.Name = "groupBoxOp";
            this.groupBoxOp.Padding = new System.Windows.Forms.Padding(6);
            this.groupBoxOp.Size = new System.Drawing.Size(808, 182);
            this.groupBoxOp.TabIndex = 12;
            this.groupBoxOp.TabStop = false;
            this.groupBoxOp.Text = "{0}";
            // 
            // RegisterContainerBase
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.groupBoxOp);
            this.Name = "RegisterContainerBase";
            this.Size = new System.Drawing.Size(808, 182);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxOp;
    }
}
