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
            this.label2 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.labelStatus = new System.Windows.Forms.Label();
            this.textBoxBaudrate = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.numericUpDownPort = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownWidth = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownNum = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownDiv1 = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownDiv2 = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPort)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownNum)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDiv1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDiv2)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 43);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(98, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Baudrate[bps]:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 14);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(60, 15);
            this.label2.TabIndex = 0;
            this.label2.Text = "FTDI ID:";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(15, 201);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "TEST";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 166);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(69, 15);
            this.label3.TabIndex = 0;
            this.label3.Text = "Puls num:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 138);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(68, 15);
            this.label4.TabIndex = 0;
            this.label4.Text = "Clk width:";
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(113, 205);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(58, 15);
            this.labelStatus.TabIndex = 3;
            this.labelStatus.Text = "(Status)";
            // 
            // textBoxBaudrate
            // 
            this.textBoxBaudrate.Location = new System.Drawing.Point(116, 40);
            this.textBoxBaudrate.Name = "textBoxBaudrate";
            this.textBoxBaudrate.ReadOnly = true;
            this.textBoxBaudrate.Size = new System.Drawing.Size(122, 22);
            this.textBoxBaudrate.TabIndex = 4;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(14, 71);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 15);
            this.label5.TabIndex = 0;
            this.label5.Text = "Divider:";
            // 
            // numericUpDownPort
            // 
            this.numericUpDownPort.DataBindings.Add(new System.Windows.Forms.Binding("Value", global::BitBangTest.Properties.Settings.Default, "Port", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.numericUpDownPort.Location = new System.Drawing.Point(116, 12);
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
            // numericUpDownWidth
            // 
            this.numericUpDownWidth.DataBindings.Add(new System.Windows.Forms.Binding("Value", global::BitBangTest.Properties.Settings.Default, "ClkWidth", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.numericUpDownWidth.Location = new System.Drawing.Point(116, 136);
            this.numericUpDownWidth.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.numericUpDownWidth.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownWidth.Name = "numericUpDownWidth";
            this.numericUpDownWidth.Size = new System.Drawing.Size(120, 22);
            this.numericUpDownWidth.TabIndex = 1;
            this.numericUpDownWidth.Value = global::BitBangTest.Properties.Settings.Default.ClkWidth;
            // 
            // numericUpDownNum
            // 
            this.numericUpDownNum.DataBindings.Add(new System.Windows.Forms.Binding("Value", global::BitBangTest.Properties.Settings.Default, "Num", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.numericUpDownNum.Location = new System.Drawing.Point(116, 164);
            this.numericUpDownNum.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
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
            // numericUpDownDiv1
            // 
            this.numericUpDownDiv1.DataBindings.Add(new System.Windows.Forms.Binding("Value", global::BitBangTest.Properties.Settings.Default, "Divider1", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.numericUpDownDiv1.Location = new System.Drawing.Point(118, 68);
            this.numericUpDownDiv1.Maximum = new decimal(new int[] {
            16384,
            0,
            0,
            0});
            this.numericUpDownDiv1.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownDiv1.Name = "numericUpDownDiv1";
            this.numericUpDownDiv1.Size = new System.Drawing.Size(120, 22);
            this.numericUpDownDiv1.TabIndex = 1;
            this.numericUpDownDiv1.Value = global::BitBangTest.Properties.Settings.Default.Divider1;
            this.numericUpDownDiv1.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
            // 
            // numericUpDownDiv2
            // 
            this.numericUpDownDiv2.DataBindings.Add(new System.Windows.Forms.Binding("Value", global::BitBangTest.Properties.Settings.Default, "Divider2", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.numericUpDownDiv2.DecimalPlaces = 3;
            this.numericUpDownDiv2.Increment = new decimal(new int[] {
            125,
            0,
            0,
            196608});
            this.numericUpDownDiv2.Location = new System.Drawing.Point(118, 96);
            this.numericUpDownDiv2.Maximum = new decimal(new int[] {
            875,
            0,
            0,
            196608});
            this.numericUpDownDiv2.Name = "numericUpDownDiv2";
            this.numericUpDownDiv2.Size = new System.Drawing.Size(120, 22);
            this.numericUpDownDiv2.TabIndex = 1;
            this.numericUpDownDiv2.Value = global::BitBangTest.Properties.Settings.Default.Divider2;
            this.numericUpDownDiv2.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(275, 248);
            this.Controls.Add(this.textBoxBaudrate);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.numericUpDownPort);
            this.Controls.Add(this.numericUpDownWidth);
            this.Controls.Add(this.numericUpDownNum);
            this.Controls.Add(this.numericUpDownDiv1);
            this.Controls.Add(this.numericUpDownDiv2);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "BitBangTest";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPort)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownNum)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDiv1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDiv2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numericUpDownDiv2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numericUpDownPort;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numericUpDownNum;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numericUpDownWidth;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.NumericUpDown numericUpDownDiv1;
        private System.Windows.Forms.TextBox textBoxBaudrate;
        private System.Windows.Forms.Label label5;
    }
}

