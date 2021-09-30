namespace zanac.MAmidiMEmo.Gui
{
    partial class FormEnvelopeEditor
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series3 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.buttonCancel = new MetroFramework.Controls.MetroButton();
            this.buttonOk = new MetroFramework.Controls.MetroButton();
            this.textBoxEnvText = new MetroFramework.Controls.MetroTextBox();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.metroToggleRepeat = new MetroFramework.Controls.MetroToggle();
            this.metroLabel1 = new MetroFramework.Controls.MetroLabel();
            this.metroLabel2 = new MetroFramework.Controls.MetroLabel();
            this.metroToggleRelease = new MetroFramework.Controls.MetroToggle();
            this.numericUpDownNum = new System.Windows.Forms.NumericUpDown();
            this.metroLabel3 = new MetroFramework.Controls.MetroLabel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.metroButtonRand1 = new MetroFramework.Controls.MetroButton();
            this.metroButtonRandom2 = new MetroFramework.Controls.MetroButton();
            this.metroButtonMax = new MetroFramework.Controls.MetroButton();
            this.metroButtonFir1 = new MetroFramework.Controls.MetroButton();
            this.metroPanel1 = new MetroFramework.Controls.MetroPanel();
            this.metroButton1 = new MetroFramework.Controls.MetroButton();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.metroButton2 = new MetroFramework.Controls.MetroButton();
            this.metroButton3 = new MetroFramework.Controls.MetroButton();
            this.metroButton4 = new MetroFramework.Controls.MetroButton();
            this.metroButton5 = new MetroFramework.Controls.MetroButton();
            this.metroButton6 = new MetroFramework.Controls.MetroButton();
            this.metroTextBoxFirWeight = new MetroFramework.Controls.MetroTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownNum)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.metroPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(550, 89);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 18);
            this.buttonCancel.TabIndex = 8;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseSelectable = true;
            // 
            // buttonOk
            // 
            this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOk.Location = new System.Drawing.Point(469, 89);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 18);
            this.buttonOk.TabIndex = 9;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseSelectable = true;
            // 
            // textBoxEnvText
            // 
            this.textBoxEnvText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.textBoxEnvText, 6);
            // 
            // 
            // 
            this.textBoxEnvText.CustomButton.Image = null;
            this.textBoxEnvText.CustomButton.Location = new System.Drawing.Point(602, 2);
            this.textBoxEnvText.CustomButton.Name = "";
            this.textBoxEnvText.CustomButton.Size = new System.Drawing.Size(15, 15);
            this.textBoxEnvText.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.textBoxEnvText.CustomButton.TabIndex = 1;
            this.textBoxEnvText.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.textBoxEnvText.CustomButton.UseSelectable = true;
            this.textBoxEnvText.CustomButton.Visible = false;
            this.textBoxEnvText.Lines = new string[0];
            this.textBoxEnvText.Location = new System.Drawing.Point(5, 5);
            this.textBoxEnvText.MaxLength = 32767;
            this.textBoxEnvText.Name = "textBoxEnvText";
            this.textBoxEnvText.PasswordChar = '\0';
            this.textBoxEnvText.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.textBoxEnvText.SelectedText = "";
            this.textBoxEnvText.SelectionLength = 0;
            this.textBoxEnvText.SelectionStart = 0;
            this.textBoxEnvText.ShortcutsEnabled = true;
            this.textBoxEnvText.Size = new System.Drawing.Size(620, 20);
            this.textBoxEnvText.TabIndex = 7;
            this.textBoxEnvText.UseSelectable = true;
            this.textBoxEnvText.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.textBoxEnvText.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            this.textBoxEnvText.TextChanged += new System.EventHandler(this.textBoxText_TextChanged);
            // 
            // chart1
            // 
            this.chart1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            chartArea1.AxisX.Crossing = 0D;
            chartArea1.AxisX.LineColor = System.Drawing.Color.Gray;
            chartArea1.AxisX.MajorGrid.LineColor = System.Drawing.Color.LightGray;
            chartArea1.AxisX.MajorTickMark.LineColor = System.Drawing.Color.LightGray;
            chartArea1.AxisX.MinorGrid.LineColor = System.Drawing.Color.Gray;
            chartArea1.AxisX2.LineColor = System.Drawing.Color.Gray;
            chartArea1.AxisY.Crossing = 0D;
            chartArea1.AxisY.LineColor = System.Drawing.Color.Gray;
            chartArea1.AxisY.MajorGrid.LineColor = System.Drawing.Color.LightGray;
            chartArea1.AxisY.MajorTickMark.LineColor = System.Drawing.Color.LightGray;
            chartArea1.AxisY.Maximum = 8192D;
            chartArea1.AxisY.Minimum = -8193D;
            chartArea1.AxisY.MinorGrid.LineColor = System.Drawing.Color.Gray;
            chartArea1.AxisY2.LineColor = System.Drawing.Color.Gray;
            chartArea1.BackColor = System.Drawing.Color.White;
            chartArea1.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            this.chart1.Legends.Add(legend1);
            this.chart1.Location = new System.Drawing.Point(4, 3);
            this.chart1.Name = "chart1";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.RangeColumn;
            series1.Color = System.Drawing.Color.CornflowerBlue;
            series1.Legend = "Legend1";
            series1.LegendText = "Repeat Start\\n(Set w/ Shift key)";
            series1.MarkerSize = 10;
            series1.Name = "SeriesLoop";
            series1.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Int32;
            series1.YValuesPerPoint = 4;
            series1.YValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Int32;
            series2.ChartArea = "ChartArea1";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.RangeColumn;
            series2.Color = System.Drawing.Color.DarkOrange;
            series2.Legend = "Legend1";
            series2.LegendText = "Release Start\\n(Set w/ Ctrl key)";
            series2.MarkerSize = 10;
            series2.Name = "SeriesRelease";
            series2.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Int32;
            series2.YValuesPerPoint = 2;
            series2.YValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Int32;
            series3.ChartArea = "ChartArea1";
            series3.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series3.Color = System.Drawing.Color.Red;
            series3.IsValueShownAsLabel = true;
            series3.Legend = "Legend1";
            series3.LegendText = "Values";
            series3.MarkerSize = 10;
            series3.MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Circle;
            series3.Name = "SeriesValues";
            series3.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Int32;
            series3.YValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Int32;
            this.chart1.Series.Add(series1);
            this.chart1.Series.Add(series2);
            this.chart1.Series.Add(series3);
            this.chart1.Size = new System.Drawing.Size(622, 267);
            this.chart1.TabIndex = 0;
            this.chart1.Text = "chart1";
            this.chart1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.chart1_MouseDown);
            this.chart1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.chart1_MouseMove);
            // 
            // metroToggleRepeat
            // 
            this.metroToggleRepeat.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.metroToggleRepeat.AutoSize = true;
            this.metroToggleRepeat.Location = new System.Drawing.Point(139, 279);
            this.metroToggleRepeat.Name = "metroToggleRepeat";
            this.metroToggleRepeat.Size = new System.Drawing.Size(80, 16);
            this.metroToggleRepeat.TabIndex = 2;
            this.metroToggleRepeat.Text = "Off";
            this.metroToggleRepeat.UseSelectable = true;
            this.metroToggleRepeat.CheckedChanged += new System.EventHandler(this.metroToggleRepeat_CheckedChanged);
            // 
            // metroLabel1
            // 
            this.metroLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.metroLabel1.AutoSize = true;
            this.metroLabel1.Location = new System.Drawing.Point(4, 276);
            this.metroLabel1.Name = "metroLabel1";
            this.metroLabel1.Size = new System.Drawing.Size(129, 19);
            this.metroLabel1.TabIndex = 1;
            this.metroLabel1.Text = "Enable &Repeat Point:";
            // 
            // metroLabel2
            // 
            this.metroLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.metroLabel2.AutoSize = true;
            this.metroLabel2.Location = new System.Drawing.Point(225, 276);
            this.metroLabel2.Name = "metroLabel2";
            this.metroLabel2.Size = new System.Drawing.Size(132, 19);
            this.metroLabel2.TabIndex = 3;
            this.metroLabel2.Text = "Enable &Release Point:";
            // 
            // metroToggleRelease
            // 
            this.metroToggleRelease.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.metroToggleRelease.AutoSize = true;
            this.metroToggleRelease.Location = new System.Drawing.Point(363, 279);
            this.metroToggleRelease.Name = "metroToggleRelease";
            this.metroToggleRelease.Size = new System.Drawing.Size(80, 16);
            this.metroToggleRelease.TabIndex = 4;
            this.metroToggleRelease.Text = "Off";
            this.metroToggleRelease.UseSelectable = true;
            this.metroToggleRelease.CheckedChanged += new System.EventHandler(this.metroToggleRelease_CheckedChanged);
            // 
            // numericUpDownNum
            // 
            this.numericUpDownNum.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.numericUpDownNum.Location = new System.Drawing.Point(557, 276);
            this.numericUpDownNum.Name = "numericUpDownNum";
            this.numericUpDownNum.Size = new System.Drawing.Size(80, 19);
            this.numericUpDownNum.TabIndex = 6;
            this.numericUpDownNum.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
            // 
            // metroLabel3
            // 
            this.metroLabel3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.metroLabel3.AutoSize = true;
            this.metroLabel3.Location = new System.Drawing.Point(456, 276);
            this.metroLabel3.Name = "metroLabel3";
            this.metroLabel3.Size = new System.Drawing.Size(95, 19);
            this.metroLabel3.TabIndex = 5;
            this.metroLabel3.Text = "Point Number:";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 6;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.buttonOk, 4, 3);
            this.tableLayoutPanel1.Controls.Add(this.buttonCancel, 5, 3);
            this.tableLayoutPanel1.Controls.Add(this.metroButtonRand1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.textBoxEnvText, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.metroButtonRandom2, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.metroButtonMax, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.metroButtonFir1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.metroTextBoxFirWeight, 1, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(5, 358);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(630, 112);
            this.tableLayoutPanel1.TabIndex = 11;
            // 
            // metroButtonRand1
            // 
            this.metroButtonRand1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.metroButtonRand1.Location = new System.Drawing.Point(5, 31);
            this.metroButtonRand1.Name = "metroButtonRand1";
            this.metroButtonRand1.Size = new System.Drawing.Size(75, 23);
            this.metroButtonRand1.TabIndex = 2;
            this.metroButtonRand1.Text = "&Random1";
            this.metroButtonRand1.UseSelectable = true;
            this.metroButtonRand1.Click += new System.EventHandler(this.metroButtonRand1_Click);
            // 
            // metroButtonRandom2
            // 
            this.metroButtonRandom2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.metroButtonRandom2.Location = new System.Drawing.Point(86, 31);
            this.metroButtonRandom2.Name = "metroButtonRandom2";
            this.metroButtonRandom2.Size = new System.Drawing.Size(75, 23);
            this.metroButtonRandom2.TabIndex = 3;
            this.metroButtonRandom2.Text = "R&andom2";
            this.metroButtonRandom2.UseSelectable = true;
            this.metroButtonRandom2.Click += new System.EventHandler(this.metroButtonRandom2_Click);
            // 
            // metroButtonMax
            // 
            this.metroButtonMax.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.metroButtonMax.Location = new System.Drawing.Point(167, 31);
            this.metroButtonMax.Name = "metroButtonMax";
            this.metroButtonMax.Size = new System.Drawing.Size(75, 23);
            this.metroButtonMax.TabIndex = 4;
            this.metroButtonMax.Text = "&Maximize";
            this.metroButtonMax.UseSelectable = true;
            this.metroButtonMax.Click += new System.EventHandler(this.metroButtonMax_Click);
            // 
            // metroButtonFir1
            // 
            this.metroButtonFir1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.metroButtonFir1.Location = new System.Drawing.Point(5, 60);
            this.metroButtonFir1.Name = "metroButtonFir1";
            this.metroButtonFir1.Size = new System.Drawing.Size(75, 23);
            this.metroButtonFir1.TabIndex = 5;
            this.metroButtonFir1.Text = "&FIR";
            this.metroButtonFir1.UseSelectable = true;
            this.metroButtonFir1.Click += new System.EventHandler(this.metroButtonFir1_Click);
            // 
            // metroPanel1
            // 
            this.metroPanel1.Controls.Add(this.chart1);
            this.metroPanel1.Controls.Add(this.metroToggleRepeat);
            this.metroPanel1.Controls.Add(this.numericUpDownNum);
            this.metroPanel1.Controls.Add(this.metroToggleRelease);
            this.metroPanel1.Controls.Add(this.metroLabel3);
            this.metroPanel1.Controls.Add(this.metroLabel1);
            this.metroPanel1.Controls.Add(this.metroLabel2);
            this.metroPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.metroPanel1.HorizontalScrollbarBarColor = true;
            this.metroPanel1.HorizontalScrollbarHighlightOnWheel = false;
            this.metroPanel1.HorizontalScrollbarSize = 10;
            this.metroPanel1.Location = new System.Drawing.Point(5, 60);
            this.metroPanel1.Name = "metroPanel1";
            this.metroPanel1.Size = new System.Drawing.Size(630, 298);
            this.metroPanel1.TabIndex = 10;
            this.metroPanel1.VerticalScrollbarBarColor = true;
            this.metroPanel1.VerticalScrollbarHighlightOnWheel = false;
            this.metroPanel1.VerticalScrollbarSize = 10;
            // 
            // metroButton1
            // 
            this.metroButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.metroButton1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.metroButton1.Location = new System.Drawing.Point(419, 517);
            this.metroButton1.Name = "metroButton1";
            this.metroButton1.Size = new System.Drawing.Size(75, 23);
            this.metroButton1.TabIndex = 7;
            this.metroButton1.Text = "OK";
            this.metroButton1.UseSelectable = true;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 6;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 84F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this.metroButton2, 5, 4);
            this.tableLayoutPanel2.Controls.Add(this.metroButton3, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.metroButton1, 4, 4);
            this.tableLayoutPanel2.Controls.Add(this.metroButton4, 1, 2);
            this.tableLayoutPanel2.Controls.Add(this.metroButton5, 0, 3);
            this.tableLayoutPanel2.Controls.Add(this.metroButton6, 2, 2);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(5, 60);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 5;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(578, 543);
            this.tableLayoutPanel2.TabIndex = 5;
            // 
            // metroButton2
            // 
            this.metroButton2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.metroButton2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.metroButton2.Location = new System.Drawing.Point(500, 517);
            this.metroButton2.Name = "metroButton2";
            this.metroButton2.Size = new System.Drawing.Size(75, 23);
            this.metroButton2.TabIndex = 8;
            this.metroButton2.Text = "Cancel";
            this.metroButton2.UseSelectable = true;
            // 
            // metroButton3
            // 
            this.metroButton3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.metroButton3.Location = new System.Drawing.Point(3, 459);
            this.metroButton3.Name = "metroButton3";
            this.metroButton3.Size = new System.Drawing.Size(75, 23);
            this.metroButton3.TabIndex = 2;
            this.metroButton3.Text = "&Random1";
            this.metroButton3.UseSelectable = true;
            this.metroButton3.Click += new System.EventHandler(this.metroButtonRand1_Click);
            // 
            // metroButton4
            // 
            this.metroButton4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.metroButton4.Location = new System.Drawing.Point(84, 459);
            this.metroButton4.Name = "metroButton4";
            this.metroButton4.Size = new System.Drawing.Size(75, 23);
            this.metroButton4.TabIndex = 3;
            this.metroButton4.Text = "R&andom2";
            this.metroButton4.UseSelectable = true;
            this.metroButton4.Click += new System.EventHandler(this.metroButtonRandom2_Click);
            // 
            // metroButton5
            // 
            this.metroButton5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.metroButton5.Location = new System.Drawing.Point(3, 488);
            this.metroButton5.Name = "metroButton5";
            this.metroButton5.Size = new System.Drawing.Size(75, 23);
            this.metroButton5.TabIndex = 5;
            this.metroButton5.Text = "&FIR";
            this.metroButton5.UseSelectable = true;
            this.metroButton5.Click += new System.EventHandler(this.metroButtonFir1_Click);
            // 
            // metroButton6
            // 
            this.metroButton6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.metroButton6.Location = new System.Drawing.Point(165, 459);
            this.metroButton6.Name = "metroButton6";
            this.metroButton6.Size = new System.Drawing.Size(75, 23);
            this.metroButton6.TabIndex = 4;
            this.metroButton6.Text = "&Maximize";
            this.metroButton6.UseSelectable = true;
            this.metroButton6.Click += new System.EventHandler(this.metroButtonMax_Click);
            // 
            // metroTextBoxFirWeight
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.metroTextBoxFirWeight, 5);
            // 
            // 
            // 
            this.metroTextBoxFirWeight.CustomButton.Image = null;
            this.metroTextBoxFirWeight.CustomButton.Location = new System.Drawing.Point(517, 1);
            this.metroTextBoxFirWeight.CustomButton.Name = "";
            this.metroTextBoxFirWeight.CustomButton.Size = new System.Drawing.Size(21, 21);
            this.metroTextBoxFirWeight.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.metroTextBoxFirWeight.CustomButton.TabIndex = 1;
            this.metroTextBoxFirWeight.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.metroTextBoxFirWeight.CustomButton.UseSelectable = true;
            this.metroTextBoxFirWeight.CustomButton.Visible = false;
            this.metroTextBoxFirWeight.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::zanac.MAmidiMEmo.Properties.Settings.Default, "EnvFirWeight", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.metroTextBoxFirWeight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.metroTextBoxFirWeight.Lines = new string[] {
        " 1, 1, 1, 1, 1 "};
            this.metroTextBoxFirWeight.Location = new System.Drawing.Point(86, 60);
            this.metroTextBoxFirWeight.MaxLength = 32767;
            this.metroTextBoxFirWeight.Name = "metroTextBoxFirWeight";
            this.metroTextBoxFirWeight.PasswordChar = '\0';
            this.metroTextBoxFirWeight.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.metroTextBoxFirWeight.SelectedText = "";
            this.metroTextBoxFirWeight.SelectionLength = 0;
            this.metroTextBoxFirWeight.SelectionStart = 0;
            this.metroTextBoxFirWeight.ShortcutsEnabled = true;
            this.metroTextBoxFirWeight.Size = new System.Drawing.Size(539, 23);
            this.metroTextBoxFirWeight.TabIndex = 6;
            this.metroTextBoxFirWeight.Text = global::zanac.MAmidiMEmo.Properties.Settings.Default.EnvFirWeight;
            this.metroTextBoxFirWeight.UseSelectable = true;
            this.metroTextBoxFirWeight.WaterMark = "Set FIR weights like \"1,1,1,1,1\"";
            this.metroTextBoxFirWeight.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.metroTextBoxFirWeight.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            // 
            // FormEnvelopeEditor
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(640, 480);
            this.Controls.Add(this.metroPanel1);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.MinimizeBox = false;
            this.Name = "FormEnvelopeEditor";
            this.Padding = new System.Windows.Forms.Padding(6, 60, 6, 12);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Envelope Editor";
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownNum)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.metroPanel1.ResumeLayout(false);
            this.metroPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private MetroFramework.Controls.MetroButton buttonCancel;
        private MetroFramework.Controls.MetroButton buttonOk;
        private MetroFramework.Controls.MetroTextBox textBoxEnvText;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private MetroFramework.Controls.MetroToggle metroToggleRepeat;
        private MetroFramework.Controls.MetroLabel metroLabel1;
        private MetroFramework.Controls.MetroLabel metroLabel2;
        private MetroFramework.Controls.MetroToggle metroToggleRelease;
        private System.Windows.Forms.NumericUpDown numericUpDownNum;
        private MetroFramework.Controls.MetroLabel metroLabel3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private MetroFramework.Controls.MetroPanel metroPanel1;
        private MetroFramework.Controls.MetroButton metroButtonRand1;
        private MetroFramework.Controls.MetroButton metroButtonRandom2;
        private MetroFramework.Controls.MetroButton metroButtonMax;
        private MetroFramework.Controls.MetroButton metroButtonFir1;
        private MetroFramework.Controls.MetroTextBox metroTextBoxFirWeight;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private MetroFramework.Controls.MetroButton metroButton2;
        private MetroFramework.Controls.MetroButton metroButton3;
        private MetroFramework.Controls.MetroButton metroButton1;
        private MetroFramework.Controls.MetroButton metroButton4;
        private MetroFramework.Controls.MetroButton metroButton5;
        private MetroFramework.Controls.MetroButton metroButton6;
    }
}