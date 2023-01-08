namespace BitBangTest
{
    partial class Form1
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.numericUpDownBps = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.numericUpDownPort = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.numericUpDownNum = new System.Windows.Forms.NumericUpDown();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownBps)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPort)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownNum)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 55);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(98, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Baudrate[bps]:";
            // 
            // numericUpDownBps
            // 
            this.numericUpDownBps.DataBindings.Add(new System.Windows.Forms.Binding("Value", global::BitBangTest.Properties.Settings.Default, "Baudrate", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.numericUpDownBps.Location = new System.Drawing.Point(116, 55);
            this.numericUpDownBps.Maximum = new decimal(new int[] {
            3000000,
            0,
            0,
            0});
            this.numericUpDownBps.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownBps.Name = "numericUpDownBps";
            this.numericUpDownBps.Size = new System.Drawing.Size(120, 22);
            this.numericUpDownBps.TabIndex = 1;
            this.numericUpDownBps.Value = global::BitBangTest.Properties.Settings.Default.Baudrate;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(60, 15);
            this.label2.TabIndex = 0;
            this.label2.Text = "FTDI ID:";
            // 
            // numericUpDownPort
            // 
            this.numericUpDownPort.DataBindings.Add(new System.Windows.Forms.Binding("Value", global::BitBangTest.Properties.Settings.Default, "Port", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.numericUpDownPort.Location = new System.Drawing.Point(116, 7);
            this.numericUpDownPort.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericUpDownPort.Name = "numericUpDownPort";
            this.numericUpDownPort.Size = new System.Drawing.Size(120, 22);
            this.numericUpDownPort.TabIndex = 1;
            this.numericUpDownPort.Value = global::BitBangTest.Properties.Settings.Default.Port;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 97);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(69, 15);
            this.label3.TabIndex = 0;
            this.label3.Text = "Puls num:";
            // 
            // numericUpDownNum
            // 
            this.numericUpDownNum.DataBindings.Add(new System.Windows.Forms.Binding("Value", global::BitBangTest.Properties.Settings.Default, "Num", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.numericUpDownNum.Location = new System.Drawing.Point(116, 95);
            this.numericUpDownNum.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownNum.Name = "numericUpDownNum";
            this.numericUpDownNum.Size = new System.Drawing.Size(120, 22);
            this.numericUpDownNum.TabIndex = 1;
            this.numericUpDownNum.Value = global::BitBangTest.Properties.Settings.Default.Num;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(15, 138);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "TEST";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(342, 217);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.numericUpDownPort);
            this.Controls.Add(this.numericUpDownNum);
            this.Controls.Add(this.numericUpDownBps);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownBps)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPort)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownNum)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numericUpDownBps;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numericUpDownPort;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numericUpDownNum;
        private System.Windows.Forms.Button button1;
    }
}

