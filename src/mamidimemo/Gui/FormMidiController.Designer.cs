using Melanchall.DryWetMidi.Multimedia;
using zanac.MAmidiMEmo.Midi;

namespace zanac.MAmidiMEmo.Gui
{
    partial class FormMidiController
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMidiController));
            this.button1 = new MetroFramework.Controls.MetroButton();
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBoxOutPort = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tableLayoutPanelCh = new System.Windows.Forms.TableLayoutPanel();
            this.metroCheckBoxCh16 = new MetroFramework.Controls.MetroCheckBox();
            this.metroCheckBoxCh15 = new MetroFramework.Controls.MetroCheckBox();
            this.metroCheckBoxCh14 = new MetroFramework.Controls.MetroCheckBox();
            this.metroCheckBoxCh13 = new MetroFramework.Controls.MetroCheckBox();
            this.metroCheckBoxCh12 = new MetroFramework.Controls.MetroCheckBox();
            this.metroCheckBoxCh11 = new MetroFramework.Controls.MetroCheckBox();
            this.metroCheckBoxCh10 = new MetroFramework.Controls.MetroCheckBox();
            this.metroCheckBoxCh9 = new MetroFramework.Controls.MetroCheckBox();
            this.metroCheckBoxCh8 = new MetroFramework.Controls.MetroCheckBox();
            this.metroCheckBoxCh7 = new MetroFramework.Controls.MetroCheckBox();
            this.metroCheckBoxCh6 = new MetroFramework.Controls.MetroCheckBox();
            this.metroCheckBoxCh5 = new MetroFramework.Controls.MetroCheckBox();
            this.metroCheckBoxCh4 = new MetroFramework.Controls.MetroCheckBox();
            this.metroCheckBoxCh3 = new MetroFramework.Controls.MetroCheckBox();
            this.metroCheckBoxCh2 = new MetroFramework.Controls.MetroCheckBox();
            this.metroCheckBoxCh1 = new MetroFramework.Controls.MetroCheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.metroTrackBarPitch = new MetroFramework.Controls.MetroTrackBar();
            this.numericUpDownPitch = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.metroTrackBarPitchRange = new MetroFramework.Controls.MetroTrackBar();
            this.numericUpDownPitchRange = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.metroTrackBarFineTune = new MetroFramework.Controls.MetroTrackBar();
            this.numericUpDownFineTune = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.metroTrackBarProgNo = new MetroFramework.Controls.MetroTrackBar();
            this.numericUpDownProgNo = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.metroTrackBarVolume = new MetroFramework.Controls.MetroTrackBar();
            this.numericUpDownVolume = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.metroTrackBarExpression = new MetroFramework.Controls.MetroTrackBar();
            this.numericUpDownExpression = new System.Windows.Forms.NumericUpDown();
            this.label9 = new System.Windows.Forms.Label();
            this.metroTrackBarPanpot = new MetroFramework.Controls.MetroTrackBar();
            this.numericUpDownPanpot = new System.Windows.Forms.NumericUpDown();
            this.label10 = new System.Windows.Forms.Label();
            this.metroTrackBarModulation = new MetroFramework.Controls.MetroTrackBar();
            this.numericUpDownModulation = new System.Windows.Forms.NumericUpDown();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.metroTrackBarModRate = new MetroFramework.Controls.MetroTrackBar();
            this.numericUpDownModRate = new System.Windows.Forms.NumericUpDown();
            this.metroTrackBarModDepth = new MetroFramework.Controls.MetroTrackBar();
            this.numericUpDownModDepth = new System.Windows.Forms.NumericUpDown();
            this.metroTrackBarModDelay = new MetroFramework.Controls.MetroTrackBar();
            this.numericUpDownModDelay = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownModDepthRangeNote = new System.Windows.Forms.NumericUpDown();
            this.metroTrackBarModDepthRangeNote = new MetroFramework.Controls.MetroTrackBar();
            this.metroTrackBarModDepthRangeCent = new MetroFramework.Controls.MetroTrackBar();
            this.numericUpDownModDepthRangeCent = new System.Windows.Forms.NumericUpDown();
            this.label16 = new System.Windows.Forms.Label();
            this.metroTrackBarHold = new MetroFramework.Controls.MetroTrackBar();
            this.numericUpDown3 = new System.Windows.Forms.NumericUpDown();
            this.label17 = new System.Windows.Forms.Label();
            this.metroTrackBarPortament = new MetroFramework.Controls.MetroTrackBar();
            this.numericUpDownPortament = new System.Windows.Forms.NumericUpDown();
            this.label18 = new System.Windows.Forms.Label();
            this.metroTrackBarPortamentTime = new MetroFramework.Controls.MetroTrackBar();
            this.numericUpDownPortamentTime = new System.Windows.Forms.NumericUpDown();
            this.label19 = new System.Windows.Forms.Label();
            this.metroTrackBarLegatoFootSw = new MetroFramework.Controls.MetroTrackBar();
            this.numericUpDownLegatoFootSw = new System.Windows.Forms.NumericUpDown();
            this.label20 = new System.Windows.Forms.Label();
            this.metroTrackBarMonoMode = new MetroFramework.Controls.MetroTrackBar();
            this.numericUpDownMonoMode = new System.Windows.Forms.NumericUpDown();
            this.label21 = new System.Windows.Forms.Label();
            this.metroTrackBarPolyMode = new MetroFramework.Controls.MetroTrackBar();
            this.numericUpDownPolyMode = new System.Windows.Forms.NumericUpDown();
            this.tableLayoutPanel.SuspendLayout();
            this.tableLayoutPanelCh.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPitch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPitchRange)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFineTune)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownProgNo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownVolume)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownExpression)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPanpot)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownModulation)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownModRate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownModDepth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownModDelay)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownModDepthRangeNote)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownModDepthRangeCent)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPortament)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPortamentTime)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownLegatoFootSw)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMonoMode)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPolyMode)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            resources.ApplyResources(this.button1, "button1");
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Name = "button1";
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // tableLayoutPanel
            // 
            resources.ApplyResources(this.tableLayoutPanel, "tableLayoutPanel");
            this.tableLayoutPanel.Controls.Add(this.label2, 0, 1);
            this.tableLayoutPanel.Controls.Add(this.comboBoxOutPort, 1, 0);
            this.tableLayoutPanel.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel.Controls.Add(this.tableLayoutPanelCh, 1, 1);
            this.tableLayoutPanel.Controls.Add(this.label3, 0, 3);
            this.tableLayoutPanel.Controls.Add(this.metroTrackBarPitch, 1, 3);
            this.tableLayoutPanel.Controls.Add(this.numericUpDownPitch, 2, 3);
            this.tableLayoutPanel.Controls.Add(this.label4, 0, 4);
            this.tableLayoutPanel.Controls.Add(this.metroTrackBarPitchRange, 1, 4);
            this.tableLayoutPanel.Controls.Add(this.numericUpDownPitchRange, 2, 4);
            this.tableLayoutPanel.Controls.Add(this.label5, 0, 5);
            this.tableLayoutPanel.Controls.Add(this.metroTrackBarFineTune, 1, 5);
            this.tableLayoutPanel.Controls.Add(this.numericUpDownFineTune, 2, 5);
            this.tableLayoutPanel.Controls.Add(this.label6, 0, 6);
            this.tableLayoutPanel.Controls.Add(this.metroTrackBarProgNo, 1, 6);
            this.tableLayoutPanel.Controls.Add(this.numericUpDownProgNo, 2, 6);
            this.tableLayoutPanel.Controls.Add(this.label7, 0, 7);
            this.tableLayoutPanel.Controls.Add(this.metroTrackBarVolume, 1, 7);
            this.tableLayoutPanel.Controls.Add(this.numericUpDownVolume, 2, 7);
            this.tableLayoutPanel.Controls.Add(this.label8, 0, 8);
            this.tableLayoutPanel.Controls.Add(this.metroTrackBarExpression, 1, 8);
            this.tableLayoutPanel.Controls.Add(this.numericUpDownExpression, 2, 8);
            this.tableLayoutPanel.Controls.Add(this.label9, 0, 9);
            this.tableLayoutPanel.Controls.Add(this.metroTrackBarPanpot, 1, 9);
            this.tableLayoutPanel.Controls.Add(this.numericUpDownPanpot, 2, 9);
            this.tableLayoutPanel.Controls.Add(this.label10, 0, 10);
            this.tableLayoutPanel.Controls.Add(this.metroTrackBarModulation, 1, 10);
            this.tableLayoutPanel.Controls.Add(this.numericUpDownModulation, 2, 10);
            this.tableLayoutPanel.Controls.Add(this.label11, 0, 11);
            this.tableLayoutPanel.Controls.Add(this.label12, 0, 12);
            this.tableLayoutPanel.Controls.Add(this.label13, 0, 13);
            this.tableLayoutPanel.Controls.Add(this.label14, 0, 14);
            this.tableLayoutPanel.Controls.Add(this.label15, 0, 15);
            this.tableLayoutPanel.Controls.Add(this.metroTrackBarModRate, 1, 11);
            this.tableLayoutPanel.Controls.Add(this.numericUpDownModRate, 2, 11);
            this.tableLayoutPanel.Controls.Add(this.metroTrackBarModDepth, 1, 12);
            this.tableLayoutPanel.Controls.Add(this.numericUpDownModDepth, 2, 12);
            this.tableLayoutPanel.Controls.Add(this.metroTrackBarModDelay, 1, 13);
            this.tableLayoutPanel.Controls.Add(this.numericUpDownModDelay, 2, 13);
            this.tableLayoutPanel.Controls.Add(this.numericUpDownModDepthRangeNote, 2, 14);
            this.tableLayoutPanel.Controls.Add(this.metroTrackBarModDepthRangeNote, 1, 14);
            this.tableLayoutPanel.Controls.Add(this.metroTrackBarModDepthRangeCent, 1, 15);
            this.tableLayoutPanel.Controls.Add(this.numericUpDownModDepthRangeCent, 2, 15);
            this.tableLayoutPanel.Controls.Add(this.label16, 0, 16);
            this.tableLayoutPanel.Controls.Add(this.metroTrackBarHold, 1, 16);
            this.tableLayoutPanel.Controls.Add(this.numericUpDown3, 2, 16);
            this.tableLayoutPanel.Controls.Add(this.label17, 0, 17);
            this.tableLayoutPanel.Controls.Add(this.metroTrackBarPortament, 1, 17);
            this.tableLayoutPanel.Controls.Add(this.numericUpDownPortament, 2, 17);
            this.tableLayoutPanel.Controls.Add(this.label18, 0, 18);
            this.tableLayoutPanel.Controls.Add(this.metroTrackBarPortamentTime, 1, 18);
            this.tableLayoutPanel.Controls.Add(this.numericUpDownPortamentTime, 2, 18);
            this.tableLayoutPanel.Controls.Add(this.label19, 0, 19);
            this.tableLayoutPanel.Controls.Add(this.metroTrackBarLegatoFootSw, 1, 19);
            this.tableLayoutPanel.Controls.Add(this.numericUpDownLegatoFootSw, 2, 19);
            this.tableLayoutPanel.Controls.Add(this.label20, 0, 20);
            this.tableLayoutPanel.Controls.Add(this.metroTrackBarMonoMode, 1, 20);
            this.tableLayoutPanel.Controls.Add(this.numericUpDownMonoMode, 2, 20);
            this.tableLayoutPanel.Controls.Add(this.label21, 0, 21);
            this.tableLayoutPanel.Controls.Add(this.metroTrackBarPolyMode, 1, 21);
            this.tableLayoutPanel.Controls.Add(this.numericUpDownPolyMode, 2, 21);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // comboBoxOutPort
            // 
            this.tableLayoutPanel.SetColumnSpan(this.comboBoxOutPort, 2);
            this.comboBoxOutPort.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", global::zanac.MAmidiMEmo.Properties.Settings.Default, "MidiConOutPort", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            resources.ApplyResources(this.comboBoxOutPort, "comboBoxOutPort");
            this.comboBoxOutPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxOutPort.FormattingEnabled = true;
            this.comboBoxOutPort.Items.AddRange(new object[] {
            resources.GetString("comboBoxOutPort.Items"),
            resources.GetString("comboBoxOutPort.Items1"),
            resources.GetString("comboBoxOutPort.Items2")});
            this.comboBoxOutPort.Name = "comboBoxOutPort";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // tableLayoutPanelCh
            // 
            resources.ApplyResources(this.tableLayoutPanelCh, "tableLayoutPanelCh");
            this.tableLayoutPanelCh.Controls.Add(this.metroCheckBoxCh16, 7, 1);
            this.tableLayoutPanelCh.Controls.Add(this.metroCheckBoxCh15, 6, 1);
            this.tableLayoutPanelCh.Controls.Add(this.metroCheckBoxCh14, 5, 1);
            this.tableLayoutPanelCh.Controls.Add(this.metroCheckBoxCh13, 4, 1);
            this.tableLayoutPanelCh.Controls.Add(this.metroCheckBoxCh12, 3, 1);
            this.tableLayoutPanelCh.Controls.Add(this.metroCheckBoxCh11, 2, 1);
            this.tableLayoutPanelCh.Controls.Add(this.metroCheckBoxCh10, 1, 1);
            this.tableLayoutPanelCh.Controls.Add(this.metroCheckBoxCh9, 0, 1);
            this.tableLayoutPanelCh.Controls.Add(this.metroCheckBoxCh8, 7, 0);
            this.tableLayoutPanelCh.Controls.Add(this.metroCheckBoxCh7, 6, 0);
            this.tableLayoutPanelCh.Controls.Add(this.metroCheckBoxCh6, 5, 0);
            this.tableLayoutPanelCh.Controls.Add(this.metroCheckBoxCh5, 4, 0);
            this.tableLayoutPanelCh.Controls.Add(this.metroCheckBoxCh4, 3, 0);
            this.tableLayoutPanelCh.Controls.Add(this.metroCheckBoxCh3, 2, 0);
            this.tableLayoutPanelCh.Controls.Add(this.metroCheckBoxCh2, 1, 0);
            this.tableLayoutPanelCh.Controls.Add(this.metroCheckBoxCh1, 0, 0);
            this.tableLayoutPanelCh.Name = "tableLayoutPanelCh";
            // 
            // metroCheckBoxCh16
            // 
            resources.ApplyResources(this.metroCheckBoxCh16, "metroCheckBoxCh16");
            this.metroCheckBoxCh16.Checked = global::zanac.MAmidiMEmo.Properties.Settings.Default.MidiConCh16;
            this.metroCheckBoxCh16.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::zanac.MAmidiMEmo.Properties.Settings.Default, "MidiConCh16", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.metroCheckBoxCh16.Name = "metroCheckBoxCh16";
            this.metroCheckBoxCh16.UseVisualStyleBackColor = true;
            // 
            // metroCheckBoxCh15
            // 
            resources.ApplyResources(this.metroCheckBoxCh15, "metroCheckBoxCh15");
            this.metroCheckBoxCh15.Checked = global::zanac.MAmidiMEmo.Properties.Settings.Default.MidiConCh15;
            this.metroCheckBoxCh15.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::zanac.MAmidiMEmo.Properties.Settings.Default, "MidiConCh15", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.metroCheckBoxCh15.Name = "metroCheckBoxCh15";
            this.metroCheckBoxCh15.UseVisualStyleBackColor = true;
            // 
            // metroCheckBoxCh14
            // 
            resources.ApplyResources(this.metroCheckBoxCh14, "metroCheckBoxCh14");
            this.metroCheckBoxCh14.Checked = global::zanac.MAmidiMEmo.Properties.Settings.Default.MidiConCh14;
            this.metroCheckBoxCh14.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::zanac.MAmidiMEmo.Properties.Settings.Default, "MidiConCh14", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.metroCheckBoxCh14.Name = "metroCheckBoxCh14";
            this.metroCheckBoxCh14.UseVisualStyleBackColor = true;
            // 
            // metroCheckBoxCh13
            // 
            resources.ApplyResources(this.metroCheckBoxCh13, "metroCheckBoxCh13");
            this.metroCheckBoxCh13.Checked = global::zanac.MAmidiMEmo.Properties.Settings.Default.MidiConCh13;
            this.metroCheckBoxCh13.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::zanac.MAmidiMEmo.Properties.Settings.Default, "MidiConCh13", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.metroCheckBoxCh13.Name = "metroCheckBoxCh13";
            this.metroCheckBoxCh13.UseVisualStyleBackColor = true;
            // 
            // metroCheckBoxCh12
            // 
            resources.ApplyResources(this.metroCheckBoxCh12, "metroCheckBoxCh12");
            this.metroCheckBoxCh12.Checked = global::zanac.MAmidiMEmo.Properties.Settings.Default.MidiConCh12;
            this.metroCheckBoxCh12.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::zanac.MAmidiMEmo.Properties.Settings.Default, "MidiConCh12", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.metroCheckBoxCh12.Name = "metroCheckBoxCh12";
            this.metroCheckBoxCh12.UseVisualStyleBackColor = true;
            // 
            // metroCheckBoxCh11
            // 
            resources.ApplyResources(this.metroCheckBoxCh11, "metroCheckBoxCh11");
            this.metroCheckBoxCh11.Checked = global::zanac.MAmidiMEmo.Properties.Settings.Default.MidiConCh11;
            this.metroCheckBoxCh11.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::zanac.MAmidiMEmo.Properties.Settings.Default, "MidiConCh11", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.metroCheckBoxCh11.Name = "metroCheckBoxCh11";
            this.metroCheckBoxCh11.UseVisualStyleBackColor = true;
            // 
            // metroCheckBoxCh10
            // 
            resources.ApplyResources(this.metroCheckBoxCh10, "metroCheckBoxCh10");
            this.metroCheckBoxCh10.Checked = global::zanac.MAmidiMEmo.Properties.Settings.Default.MidiConCh10;
            this.metroCheckBoxCh10.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::zanac.MAmidiMEmo.Properties.Settings.Default, "MidiConCh10", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.metroCheckBoxCh10.Name = "metroCheckBoxCh10";
            this.metroCheckBoxCh10.UseVisualStyleBackColor = true;
            // 
            // metroCheckBoxCh9
            // 
            resources.ApplyResources(this.metroCheckBoxCh9, "metroCheckBoxCh9");
            this.metroCheckBoxCh9.Checked = global::zanac.MAmidiMEmo.Properties.Settings.Default.MidiConCh9;
            this.metroCheckBoxCh9.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::zanac.MAmidiMEmo.Properties.Settings.Default, "MidiConCh9", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.metroCheckBoxCh9.Name = "metroCheckBoxCh9";
            this.metroCheckBoxCh9.UseVisualStyleBackColor = true;
            // 
            // metroCheckBoxCh8
            // 
            resources.ApplyResources(this.metroCheckBoxCh8, "metroCheckBoxCh8");
            this.metroCheckBoxCh8.Checked = global::zanac.MAmidiMEmo.Properties.Settings.Default.MidiConCh8;
            this.metroCheckBoxCh8.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::zanac.MAmidiMEmo.Properties.Settings.Default, "MidiConCh8", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.metroCheckBoxCh8.Name = "metroCheckBoxCh8";
            this.metroCheckBoxCh8.UseVisualStyleBackColor = true;
            // 
            // metroCheckBoxCh7
            // 
            resources.ApplyResources(this.metroCheckBoxCh7, "metroCheckBoxCh7");
            this.metroCheckBoxCh7.Checked = global::zanac.MAmidiMEmo.Properties.Settings.Default.MidiConCh7;
            this.metroCheckBoxCh7.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::zanac.MAmidiMEmo.Properties.Settings.Default, "MidiConCh7", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.metroCheckBoxCh7.Name = "metroCheckBoxCh7";
            this.metroCheckBoxCh7.UseVisualStyleBackColor = true;
            // 
            // metroCheckBoxCh6
            // 
            resources.ApplyResources(this.metroCheckBoxCh6, "metroCheckBoxCh6");
            this.metroCheckBoxCh6.Checked = global::zanac.MAmidiMEmo.Properties.Settings.Default.MidiConCh6;
            this.metroCheckBoxCh6.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::zanac.MAmidiMEmo.Properties.Settings.Default, "MidiConCh6", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.metroCheckBoxCh6.Name = "metroCheckBoxCh6";
            this.metroCheckBoxCh6.UseVisualStyleBackColor = true;
            // 
            // metroCheckBoxCh5
            // 
            resources.ApplyResources(this.metroCheckBoxCh5, "metroCheckBoxCh5");
            this.metroCheckBoxCh5.Checked = global::zanac.MAmidiMEmo.Properties.Settings.Default.MidiConCh5;
            this.metroCheckBoxCh5.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::zanac.MAmidiMEmo.Properties.Settings.Default, "MidiConCh5", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.metroCheckBoxCh5.Name = "metroCheckBoxCh5";
            this.metroCheckBoxCh5.UseVisualStyleBackColor = true;
            // 
            // metroCheckBoxCh4
            // 
            resources.ApplyResources(this.metroCheckBoxCh4, "metroCheckBoxCh4");
            this.metroCheckBoxCh4.Checked = global::zanac.MAmidiMEmo.Properties.Settings.Default.MidiConCh4;
            this.metroCheckBoxCh4.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::zanac.MAmidiMEmo.Properties.Settings.Default, "MidiConCh4", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.metroCheckBoxCh4.Name = "metroCheckBoxCh4";
            this.metroCheckBoxCh4.UseVisualStyleBackColor = true;
            // 
            // metroCheckBoxCh3
            // 
            resources.ApplyResources(this.metroCheckBoxCh3, "metroCheckBoxCh3");
            this.metroCheckBoxCh3.Checked = global::zanac.MAmidiMEmo.Properties.Settings.Default.MidiConCh3;
            this.metroCheckBoxCh3.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::zanac.MAmidiMEmo.Properties.Settings.Default, "MidiConCh3", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.metroCheckBoxCh3.Name = "metroCheckBoxCh3";
            this.metroCheckBoxCh3.UseVisualStyleBackColor = true;
            // 
            // metroCheckBoxCh2
            // 
            resources.ApplyResources(this.metroCheckBoxCh2, "metroCheckBoxCh2");
            this.metroCheckBoxCh2.Checked = global::zanac.MAmidiMEmo.Properties.Settings.Default.MidiConCh2;
            this.metroCheckBoxCh2.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::zanac.MAmidiMEmo.Properties.Settings.Default, "MidiConCh2", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.metroCheckBoxCh2.Name = "metroCheckBoxCh2";
            this.metroCheckBoxCh2.UseVisualStyleBackColor = true;
            // 
            // metroCheckBoxCh1
            // 
            resources.ApplyResources(this.metroCheckBoxCh1, "metroCheckBoxCh1");
            this.metroCheckBoxCh1.Checked = global::zanac.MAmidiMEmo.Properties.Settings.Default.MidiConCh1;
            this.metroCheckBoxCh1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.metroCheckBoxCh1.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::zanac.MAmidiMEmo.Properties.Settings.Default, "MidiConCh1", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.metroCheckBoxCh1.Name = "metroCheckBoxCh1";
            this.metroCheckBoxCh1.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            this.label3.MouseClick += new System.Windows.Forms.MouseEventHandler(this.label3_MouseClick);
            // 
            // metroTrackBarPitch
            // 
            this.metroTrackBarPitch.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.metroTrackBarPitch, "metroTrackBarPitch");
            this.metroTrackBarPitch.LargeChange = 512;
            this.metroTrackBarPitch.Maximum = 16383;
            this.metroTrackBarPitch.MouseWheelBarPartitions = 512;
            this.metroTrackBarPitch.Name = "metroTrackBarPitch";
            this.metroTrackBarPitch.Value = 8192;
            this.metroTrackBarPitch.ValueChanged += new System.EventHandler(this.metroTrackBar_ValueChanged);
            // 
            // numericUpDownPitch
            // 
            resources.ApplyResources(this.numericUpDownPitch, "numericUpDownPitch");
            this.numericUpDownPitch.DataBindings.Add(new System.Windows.Forms.Binding("Value", global::zanac.MAmidiMEmo.Properties.Settings.Default, "MidiConPitch", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.numericUpDownPitch.Maximum = new decimal(new int[] {
            16383,
            0,
            0,
            0});
            this.numericUpDownPitch.Name = "numericUpDownPitch";
            this.numericUpDownPitch.Value = global::zanac.MAmidiMEmo.Properties.Settings.Default.MidiConPitch;
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            this.label4.MouseClick += new System.Windows.Forms.MouseEventHandler(this.label4_MouseClick);
            // 
            // metroTrackBarPitchRange
            // 
            this.metroTrackBarPitchRange.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.metroTrackBarPitchRange, "metroTrackBarPitchRange");
            this.metroTrackBarPitchRange.LargeChange = 8;
            this.metroTrackBarPitchRange.Maximum = 127;
            this.metroTrackBarPitchRange.MouseWheelBarPartitions = 8;
            this.metroTrackBarPitchRange.Name = "metroTrackBarPitchRange";
            this.metroTrackBarPitchRange.Value = 2;
            this.metroTrackBarPitchRange.ValueChanged += new System.EventHandler(this.metroTrackBar_ValueChanged);
            // 
            // numericUpDownPitchRange
            // 
            resources.ApplyResources(this.numericUpDownPitchRange, "numericUpDownPitchRange");
            this.numericUpDownPitchRange.DataBindings.Add(new System.Windows.Forms.Binding("Value", global::zanac.MAmidiMEmo.Properties.Settings.Default, "MidiConPitchRange", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.numericUpDownPitchRange.Maximum = new decimal(new int[] {
            127,
            0,
            0,
            0});
            this.numericUpDownPitchRange.Name = "numericUpDownPitchRange";
            this.numericUpDownPitchRange.Value = global::zanac.MAmidiMEmo.Properties.Settings.Default.MidiConPitchRange;
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            this.label5.Click += new System.EventHandler(this.label5_Click);
            // 
            // metroTrackBarFineTune
            // 
            this.metroTrackBarFineTune.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.metroTrackBarFineTune, "metroTrackBarFineTune");
            this.metroTrackBarFineTune.LargeChange = 512;
            this.metroTrackBarFineTune.Maximum = 16383;
            this.metroTrackBarFineTune.MouseWheelBarPartitions = 512;
            this.metroTrackBarFineTune.Name = "metroTrackBarFineTune";
            this.metroTrackBarFineTune.Value = 8192;
            this.metroTrackBarFineTune.ValueChanged += new System.EventHandler(this.metroTrackBar_ValueChanged);
            // 
            // numericUpDownFineTune
            // 
            resources.ApplyResources(this.numericUpDownFineTune, "numericUpDownFineTune");
            this.numericUpDownFineTune.DataBindings.Add(new System.Windows.Forms.Binding("Value", global::zanac.MAmidiMEmo.Properties.Settings.Default, "MidiConFineTune", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.numericUpDownFineTune.Maximum = new decimal(new int[] {
            16383,
            0,
            0,
            0});
            this.numericUpDownFineTune.Name = "numericUpDownFineTune";
            this.numericUpDownFineTune.Value = global::zanac.MAmidiMEmo.Properties.Settings.Default.MidiConFineTune;
            // 
            // label6
            // 
            resources.ApplyResources(this.label6, "label6");
            this.label6.Name = "label6";
            this.label6.Click += new System.EventHandler(this.label6_Click);
            // 
            // metroTrackBarProgNo
            // 
            this.metroTrackBarProgNo.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.metroTrackBarProgNo, "metroTrackBarProgNo");
            this.metroTrackBarProgNo.LargeChange = 8;
            this.metroTrackBarProgNo.Maximum = 127;
            this.metroTrackBarProgNo.MouseWheelBarPartitions = 8;
            this.metroTrackBarProgNo.Name = "metroTrackBarProgNo";
            this.metroTrackBarProgNo.Value = 0;
            this.metroTrackBarProgNo.ValueChanged += new System.EventHandler(this.metroTrackBar_ValueChanged);
            // 
            // numericUpDownProgNo
            // 
            resources.ApplyResources(this.numericUpDownProgNo, "numericUpDownProgNo");
            this.numericUpDownProgNo.DataBindings.Add(new System.Windows.Forms.Binding("Value", global::zanac.MAmidiMEmo.Properties.Settings.Default, "MidiConProgNo", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.numericUpDownProgNo.Maximum = new decimal(new int[] {
            127,
            0,
            0,
            0});
            this.numericUpDownProgNo.Name = "numericUpDownProgNo";
            this.numericUpDownProgNo.Value = global::zanac.MAmidiMEmo.Properties.Settings.Default.MidiConProgNo;
            // 
            // label7
            // 
            resources.ApplyResources(this.label7, "label7");
            this.label7.Name = "label7";
            this.label7.Click += new System.EventHandler(this.label7_Click);
            // 
            // metroTrackBarVolume
            // 
            this.metroTrackBarVolume.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.metroTrackBarVolume, "metroTrackBarVolume");
            this.metroTrackBarVolume.LargeChange = 8;
            this.metroTrackBarVolume.Maximum = 127;
            this.metroTrackBarVolume.MouseWheelBarPartitions = 8;
            this.metroTrackBarVolume.Name = "metroTrackBarVolume";
            this.metroTrackBarVolume.Tag = "7";
            this.metroTrackBarVolume.Value = 127;
            this.metroTrackBarVolume.ValueChanged += new System.EventHandler(this.metroTrackBar_ValueChanged);
            // 
            // numericUpDownVolume
            // 
            resources.ApplyResources(this.numericUpDownVolume, "numericUpDownVolume");
            this.numericUpDownVolume.DataBindings.Add(new System.Windows.Forms.Binding("Value", global::zanac.MAmidiMEmo.Properties.Settings.Default, "MidiConVolume", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.numericUpDownVolume.Maximum = new decimal(new int[] {
            127,
            0,
            0,
            0});
            this.numericUpDownVolume.Name = "numericUpDownVolume";
            this.numericUpDownVolume.Value = global::zanac.MAmidiMEmo.Properties.Settings.Default.MidiConVolume;
            // 
            // label8
            // 
            resources.ApplyResources(this.label8, "label8");
            this.label8.Name = "label8";
            this.label8.Click += new System.EventHandler(this.label8_Click);
            // 
            // metroTrackBarExpression
            // 
            this.metroTrackBarExpression.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.metroTrackBarExpression, "metroTrackBarExpression");
            this.metroTrackBarExpression.LargeChange = 8;
            this.metroTrackBarExpression.Maximum = 127;
            this.metroTrackBarExpression.MouseWheelBarPartitions = 8;
            this.metroTrackBarExpression.Name = "metroTrackBarExpression";
            this.metroTrackBarExpression.Tag = "11";
            this.metroTrackBarExpression.Value = 127;
            this.metroTrackBarExpression.ValueChanged += new System.EventHandler(this.metroTrackBar_ValueChanged);
            // 
            // numericUpDownExpression
            // 
            resources.ApplyResources(this.numericUpDownExpression, "numericUpDownExpression");
            this.numericUpDownExpression.DataBindings.Add(new System.Windows.Forms.Binding("Value", global::zanac.MAmidiMEmo.Properties.Settings.Default, "MidiConExpression", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.numericUpDownExpression.Maximum = new decimal(new int[] {
            127,
            0,
            0,
            0});
            this.numericUpDownExpression.Name = "numericUpDownExpression";
            this.numericUpDownExpression.Value = global::zanac.MAmidiMEmo.Properties.Settings.Default.MidiConExpression;
            // 
            // label9
            // 
            resources.ApplyResources(this.label9, "label9");
            this.label9.Name = "label9";
            this.label9.Click += new System.EventHandler(this.label9_Click);
            // 
            // metroTrackBarPanpot
            // 
            this.metroTrackBarPanpot.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.metroTrackBarPanpot, "metroTrackBarPanpot");
            this.metroTrackBarPanpot.LargeChange = 8;
            this.metroTrackBarPanpot.Maximum = 127;
            this.metroTrackBarPanpot.MouseWheelBarPartitions = 8;
            this.metroTrackBarPanpot.Name = "metroTrackBarPanpot";
            this.metroTrackBarPanpot.Tag = "10";
            this.metroTrackBarPanpot.Value = 64;
            this.metroTrackBarPanpot.ValueChanged += new System.EventHandler(this.metroTrackBar_ValueChanged);
            // 
            // numericUpDownPanpot
            // 
            resources.ApplyResources(this.numericUpDownPanpot, "numericUpDownPanpot");
            this.numericUpDownPanpot.DataBindings.Add(new System.Windows.Forms.Binding("Value", global::zanac.MAmidiMEmo.Properties.Settings.Default, "MidiConPanpot", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.numericUpDownPanpot.Maximum = new decimal(new int[] {
            127,
            0,
            0,
            0});
            this.numericUpDownPanpot.Name = "numericUpDownPanpot";
            this.numericUpDownPanpot.Value = global::zanac.MAmidiMEmo.Properties.Settings.Default.MidiConPanpot;
            // 
            // label10
            // 
            resources.ApplyResources(this.label10, "label10");
            this.label10.Name = "label10";
            this.label10.Click += new System.EventHandler(this.label10_Click);
            // 
            // metroTrackBarModulation
            // 
            this.metroTrackBarModulation.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.metroTrackBarModulation, "metroTrackBarModulation");
            this.metroTrackBarModulation.LargeChange = 8;
            this.metroTrackBarModulation.Maximum = 127;
            this.metroTrackBarModulation.MouseWheelBarPartitions = 8;
            this.metroTrackBarModulation.Name = "metroTrackBarModulation";
            this.metroTrackBarModulation.Tag = "1";
            this.metroTrackBarModulation.Value = 0;
            this.metroTrackBarModulation.ValueChanged += new System.EventHandler(this.metroTrackBar_ValueChanged);
            // 
            // numericUpDownModulation
            // 
            resources.ApplyResources(this.numericUpDownModulation, "numericUpDownModulation");
            this.numericUpDownModulation.DataBindings.Add(new System.Windows.Forms.Binding("Value", global::zanac.MAmidiMEmo.Properties.Settings.Default, "MidiConModulation", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.numericUpDownModulation.Maximum = new decimal(new int[] {
            127,
            0,
            0,
            0});
            this.numericUpDownModulation.Name = "numericUpDownModulation";
            this.numericUpDownModulation.Value = global::zanac.MAmidiMEmo.Properties.Settings.Default.MidiConModulation;
            // 
            // label11
            // 
            resources.ApplyResources(this.label11, "label11");
            this.label11.Name = "label11";
            this.label11.Click += new System.EventHandler(this.label11_Click);
            // 
            // label12
            // 
            resources.ApplyResources(this.label12, "label12");
            this.label12.Name = "label12";
            this.label12.Click += new System.EventHandler(this.label12_Click);
            // 
            // label13
            // 
            resources.ApplyResources(this.label13, "label13");
            this.label13.Name = "label13";
            this.label13.Click += new System.EventHandler(this.label13_Click);
            // 
            // label14
            // 
            resources.ApplyResources(this.label14, "label14");
            this.label14.Name = "label14";
            this.label14.Click += new System.EventHandler(this.label14_Click);
            // 
            // label15
            // 
            resources.ApplyResources(this.label15, "label15");
            this.label15.Name = "label15";
            this.label15.Click += new System.EventHandler(this.label15_Click);
            // 
            // metroTrackBarModRate
            // 
            this.metroTrackBarModRate.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.metroTrackBarModRate, "metroTrackBarModRate");
            this.metroTrackBarModRate.LargeChange = 8;
            this.metroTrackBarModRate.Maximum = 127;
            this.metroTrackBarModRate.MouseWheelBarPartitions = 8;
            this.metroTrackBarModRate.Name = "metroTrackBarModRate";
            this.metroTrackBarModRate.Tag = "76";
            this.metroTrackBarModRate.Value = 64;
            this.metroTrackBarModRate.ValueChanged += new System.EventHandler(this.metroTrackBar_ValueChanged);
            // 
            // numericUpDownModRate
            // 
            resources.ApplyResources(this.numericUpDownModRate, "numericUpDownModRate");
            this.numericUpDownModRate.DataBindings.Add(new System.Windows.Forms.Binding("Value", global::zanac.MAmidiMEmo.Properties.Settings.Default, "MidiConModRate", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.numericUpDownModRate.Maximum = new decimal(new int[] {
            127,
            0,
            0,
            0});
            this.numericUpDownModRate.Name = "numericUpDownModRate";
            this.numericUpDownModRate.Value = global::zanac.MAmidiMEmo.Properties.Settings.Default.MidiConModRate;
            // 
            // metroTrackBarModDepth
            // 
            this.metroTrackBarModDepth.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.metroTrackBarModDepth, "metroTrackBarModDepth");
            this.metroTrackBarModDepth.LargeChange = 8;
            this.metroTrackBarModDepth.Maximum = 127;
            this.metroTrackBarModDepth.MouseWheelBarPartitions = 8;
            this.metroTrackBarModDepth.Name = "metroTrackBarModDepth";
            this.metroTrackBarModDepth.Tag = "77";
            this.metroTrackBarModDepth.Value = 64;
            this.metroTrackBarModDepth.ValueChanged += new System.EventHandler(this.metroTrackBar_ValueChanged);
            // 
            // numericUpDownModDepth
            // 
            resources.ApplyResources(this.numericUpDownModDepth, "numericUpDownModDepth");
            this.numericUpDownModDepth.DataBindings.Add(new System.Windows.Forms.Binding("Value", global::zanac.MAmidiMEmo.Properties.Settings.Default, "MidiConModDepth", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.numericUpDownModDepth.Maximum = new decimal(new int[] {
            127,
            0,
            0,
            0});
            this.numericUpDownModDepth.Name = "numericUpDownModDepth";
            this.numericUpDownModDepth.Value = global::zanac.MAmidiMEmo.Properties.Settings.Default.MidiConModDepth;
            // 
            // metroTrackBarModDelay
            // 
            this.metroTrackBarModDelay.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.metroTrackBarModDelay, "metroTrackBarModDelay");
            this.metroTrackBarModDelay.LargeChange = 8;
            this.metroTrackBarModDelay.Maximum = 127;
            this.metroTrackBarModDelay.MouseWheelBarPartitions = 8;
            this.metroTrackBarModDelay.Name = "metroTrackBarModDelay";
            this.metroTrackBarModDelay.Tag = "78";
            this.metroTrackBarModDelay.Value = 64;
            this.metroTrackBarModDelay.ValueChanged += new System.EventHandler(this.metroTrackBar_ValueChanged);
            // 
            // numericUpDownModDelay
            // 
            resources.ApplyResources(this.numericUpDownModDelay, "numericUpDownModDelay");
            this.numericUpDownModDelay.DataBindings.Add(new System.Windows.Forms.Binding("Value", global::zanac.MAmidiMEmo.Properties.Settings.Default, "MidiConModDelay", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.numericUpDownModDelay.Maximum = new decimal(new int[] {
            127,
            0,
            0,
            0});
            this.numericUpDownModDelay.Name = "numericUpDownModDelay";
            this.numericUpDownModDelay.Value = global::zanac.MAmidiMEmo.Properties.Settings.Default.MidiConModDelay;
            // 
            // numericUpDownModDepthRangeNote
            // 
            resources.ApplyResources(this.numericUpDownModDepthRangeNote, "numericUpDownModDepthRangeNote");
            this.numericUpDownModDepthRangeNote.DataBindings.Add(new System.Windows.Forms.Binding("Value", global::zanac.MAmidiMEmo.Properties.Settings.Default, "MidiConModDepthRangeNote", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.numericUpDownModDepthRangeNote.Maximum = new decimal(new int[] {
            127,
            0,
            0,
            0});
            this.numericUpDownModDepthRangeNote.Name = "numericUpDownModDepthRangeNote";
            this.numericUpDownModDepthRangeNote.Value = global::zanac.MAmidiMEmo.Properties.Settings.Default.MidiConModDepthRangeNote;
            // 
            // metroTrackBarModDepthRangeNote
            // 
            this.metroTrackBarModDepthRangeNote.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.metroTrackBarModDepthRangeNote, "metroTrackBarModDepthRangeNote");
            this.metroTrackBarModDepthRangeNote.LargeChange = 8;
            this.metroTrackBarModDepthRangeNote.Maximum = 127;
            this.metroTrackBarModDepthRangeNote.MouseWheelBarPartitions = 8;
            this.metroTrackBarModDepthRangeNote.Name = "metroTrackBarModDepthRangeNote";
            this.metroTrackBarModDepthRangeNote.Value = 0;
            this.metroTrackBarModDepthRangeNote.ValueChanged += new System.EventHandler(this.metroTrackBar_ValueChanged);
            // 
            // metroTrackBarModDepthRangeCent
            // 
            this.metroTrackBarModDepthRangeCent.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.metroTrackBarModDepthRangeCent, "metroTrackBarModDepthRangeCent");
            this.metroTrackBarModDepthRangeCent.LargeChange = 8;
            this.metroTrackBarModDepthRangeCent.Maximum = 127;
            this.metroTrackBarModDepthRangeCent.MouseWheelBarPartitions = 8;
            this.metroTrackBarModDepthRangeCent.Name = "metroTrackBarModDepthRangeCent";
            this.metroTrackBarModDepthRangeCent.Value = 64;
            this.metroTrackBarModDepthRangeCent.ValueChanged += new System.EventHandler(this.metroTrackBar_ValueChanged);
            // 
            // numericUpDownModDepthRangeCent
            // 
            resources.ApplyResources(this.numericUpDownModDepthRangeCent, "numericUpDownModDepthRangeCent");
            this.numericUpDownModDepthRangeCent.DataBindings.Add(new System.Windows.Forms.Binding("Value", global::zanac.MAmidiMEmo.Properties.Settings.Default, "MidiConModDepthRangeCent", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.numericUpDownModDepthRangeCent.Maximum = new decimal(new int[] {
            127,
            0,
            0,
            0});
            this.numericUpDownModDepthRangeCent.Name = "numericUpDownModDepthRangeCent";
            this.numericUpDownModDepthRangeCent.Value = global::zanac.MAmidiMEmo.Properties.Settings.Default.MidiConModDepthRangeCent;
            // 
            // label16
            // 
            resources.ApplyResources(this.label16, "label16");
            this.label16.Name = "label16";
            this.label16.Click += new System.EventHandler(this.label16_Click);
            // 
            // metroTrackBarHold
            // 
            this.metroTrackBarHold.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.metroTrackBarHold, "metroTrackBarHold");
            this.metroTrackBarHold.LargeChange = 1;
            this.metroTrackBarHold.Maximum = 1;
            this.metroTrackBarHold.MouseWheelBarPartitions = 1;
            this.metroTrackBarHold.Name = "metroTrackBarHold";
            this.metroTrackBarHold.Tag = "64";
            this.metroTrackBarHold.Value = 0;
            this.metroTrackBarHold.ValueChanged += new System.EventHandler(this.metroTrackBar_ValueChanged);
            // 
            // numericUpDown3
            // 
            resources.ApplyResources(this.numericUpDown3, "numericUpDown3");
            this.numericUpDown3.DataBindings.Add(new System.Windows.Forms.Binding("Value", global::zanac.MAmidiMEmo.Properties.Settings.Default, "MidiConHold", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.numericUpDown3.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown3.Name = "numericUpDown3";
            this.numericUpDown3.Value = global::zanac.MAmidiMEmo.Properties.Settings.Default.MidiConHold;
            // 
            // label17
            // 
            resources.ApplyResources(this.label17, "label17");
            this.label17.Name = "label17";
            this.label17.Click += new System.EventHandler(this.label17_Click);
            // 
            // metroTrackBarPortament
            // 
            this.metroTrackBarPortament.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.metroTrackBarPortament, "metroTrackBarPortament");
            this.metroTrackBarPortament.LargeChange = 1;
            this.metroTrackBarPortament.Maximum = 1;
            this.metroTrackBarPortament.MouseWheelBarPartitions = 1;
            this.metroTrackBarPortament.Name = "metroTrackBarPortament";
            this.metroTrackBarPortament.Tag = "65";
            this.metroTrackBarPortament.Value = 0;
            this.metroTrackBarPortament.ValueChanged += new System.EventHandler(this.metroTrackBar_ValueChanged);
            // 
            // numericUpDownPortament
            // 
            resources.ApplyResources(this.numericUpDownPortament, "numericUpDownPortament");
            this.numericUpDownPortament.DataBindings.Add(new System.Windows.Forms.Binding("Value", global::zanac.MAmidiMEmo.Properties.Settings.Default, "MidiConPortament", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.numericUpDownPortament.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownPortament.Name = "numericUpDownPortament";
            this.numericUpDownPortament.Value = global::zanac.MAmidiMEmo.Properties.Settings.Default.MidiConPortament;
            // 
            // label18
            // 
            resources.ApplyResources(this.label18, "label18");
            this.label18.Name = "label18";
            this.label18.Click += new System.EventHandler(this.label18_Click);
            // 
            // metroTrackBarPortamentTime
            // 
            this.metroTrackBarPortamentTime.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.metroTrackBarPortamentTime, "metroTrackBarPortamentTime");
            this.metroTrackBarPortamentTime.LargeChange = 8;
            this.metroTrackBarPortamentTime.Maximum = 127;
            this.metroTrackBarPortamentTime.MouseWheelBarPartitions = 8;
            this.metroTrackBarPortamentTime.Name = "metroTrackBarPortamentTime";
            this.metroTrackBarPortamentTime.Tag = "5";
            this.metroTrackBarPortamentTime.Value = 0;
            this.metroTrackBarPortamentTime.ValueChanged += new System.EventHandler(this.metroTrackBar_ValueChanged);
            // 
            // numericUpDownPortamentTime
            // 
            resources.ApplyResources(this.numericUpDownPortamentTime, "numericUpDownPortamentTime");
            this.numericUpDownPortamentTime.DataBindings.Add(new System.Windows.Forms.Binding("Value", global::zanac.MAmidiMEmo.Properties.Settings.Default, "MidiConPortamentTime", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.numericUpDownPortamentTime.Maximum = new decimal(new int[] {
            127,
            0,
            0,
            0});
            this.numericUpDownPortamentTime.Name = "numericUpDownPortamentTime";
            this.numericUpDownPortamentTime.Value = global::zanac.MAmidiMEmo.Properties.Settings.Default.MidiConPortamentTime;
            // 
            // label19
            // 
            resources.ApplyResources(this.label19, "label19");
            this.label19.Name = "label19";
            this.label19.Click += new System.EventHandler(this.label19_Click);
            // 
            // metroTrackBarLegatoFootSw
            // 
            this.metroTrackBarLegatoFootSw.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.metroTrackBarLegatoFootSw, "metroTrackBarLegatoFootSw");
            this.metroTrackBarLegatoFootSw.LargeChange = 1;
            this.metroTrackBarLegatoFootSw.Maximum = 1;
            this.metroTrackBarLegatoFootSw.MouseWheelBarPartitions = 1;
            this.metroTrackBarLegatoFootSw.Name = "metroTrackBarLegatoFootSw";
            this.metroTrackBarLegatoFootSw.Tag = "68";
            this.metroTrackBarLegatoFootSw.Value = 0;
            this.metroTrackBarLegatoFootSw.ValueChanged += new System.EventHandler(this.metroTrackBar_ValueChanged);
            // 
            // numericUpDownLegatoFootSw
            // 
            resources.ApplyResources(this.numericUpDownLegatoFootSw, "numericUpDownLegatoFootSw");
            this.numericUpDownLegatoFootSw.DataBindings.Add(new System.Windows.Forms.Binding("Value", global::zanac.MAmidiMEmo.Properties.Settings.Default, "MidiConLegatoFootSw", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.numericUpDownLegatoFootSw.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownLegatoFootSw.Name = "numericUpDownLegatoFootSw";
            this.numericUpDownLegatoFootSw.Value = global::zanac.MAmidiMEmo.Properties.Settings.Default.MidiConLegatoFootSw;
            // 
            // label20
            // 
            resources.ApplyResources(this.label20, "label20");
            this.label20.Name = "label20";
            this.label20.Click += new System.EventHandler(this.label20_Click);
            // 
            // metroTrackBarMonoMode
            // 
            this.metroTrackBarMonoMode.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.metroTrackBarMonoMode, "metroTrackBarMonoMode");
            this.metroTrackBarMonoMode.LargeChange = 8;
            this.metroTrackBarMonoMode.Maximum = 127;
            this.metroTrackBarMonoMode.MouseWheelBarPartitions = 8;
            this.metroTrackBarMonoMode.Name = "metroTrackBarMonoMode";
            this.metroTrackBarMonoMode.Tag = "126";
            this.metroTrackBarMonoMode.Value = 0;
            this.metroTrackBarMonoMode.ValueChanged += new System.EventHandler(this.metroTrackBar_ValueChanged);
            // 
            // numericUpDownMonoMode
            // 
            resources.ApplyResources(this.numericUpDownMonoMode, "numericUpDownMonoMode");
            this.numericUpDownMonoMode.DataBindings.Add(new System.Windows.Forms.Binding("Value", global::zanac.MAmidiMEmo.Properties.Settings.Default, "MidiConMonoMode", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.numericUpDownMonoMode.Maximum = new decimal(new int[] {
            127,
            0,
            0,
            0});
            this.numericUpDownMonoMode.Name = "numericUpDownMonoMode";
            this.numericUpDownMonoMode.Value = global::zanac.MAmidiMEmo.Properties.Settings.Default.MidiConMonoMode;
            // 
            // label21
            // 
            resources.ApplyResources(this.label21, "label21");
            this.label21.Name = "label21";
            this.label21.Click += new System.EventHandler(this.label21_Click);
            // 
            // metroTrackBarPolyMode
            // 
            this.metroTrackBarPolyMode.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.metroTrackBarPolyMode, "metroTrackBarPolyMode");
            this.metroTrackBarPolyMode.LargeChange = 8;
            this.metroTrackBarPolyMode.Maximum = 127;
            this.metroTrackBarPolyMode.MouseWheelBarPartitions = 8;
            this.metroTrackBarPolyMode.Name = "metroTrackBarPolyMode";
            this.metroTrackBarPolyMode.Tag = "127";
            this.metroTrackBarPolyMode.Value = 0;
            this.metroTrackBarPolyMode.ValueChanged += new System.EventHandler(this.metroTrackBar_ValueChanged);
            // 
            // numericUpDownPolyMode
            // 
            resources.ApplyResources(this.numericUpDownPolyMode, "numericUpDownPolyMode");
            this.numericUpDownPolyMode.DataBindings.Add(new System.Windows.Forms.Binding("Value", global::zanac.MAmidiMEmo.Properties.Settings.Default, "MidiConPolyMode", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.numericUpDownPolyMode.Maximum = new decimal(new int[] {
            127,
            0,
            0,
            0});
            this.numericUpDownPolyMode.Name = "numericUpDownPolyMode";
            this.numericUpDownPolyMode.Value = global::zanac.MAmidiMEmo.Properties.Settings.Default.MidiConPolyMode;
            // 
            // FormMidiController
            // 
            this.AcceptButton = this.button1;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.button1;
            this.Controls.Add(this.tableLayoutPanel);
            this.Controls.Add(this.button1);
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormMidiController";
            this.Resizable = false;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.tableLayoutPanel.ResumeLayout(false);
            this.tableLayoutPanel.PerformLayout();
            this.tableLayoutPanelCh.ResumeLayout(false);
            this.tableLayoutPanelCh.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPitch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPitchRange)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFineTune)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownProgNo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownVolume)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownExpression)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPanpot)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownModulation)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownModRate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownModDepth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownModDelay)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownModDepthRangeNote)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownModDepthRangeCent)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPortament)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPortamentTime)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownLegatoFootSw)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMonoMode)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPolyMode)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private MetroFramework.Controls.MetroButton button1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBoxOutPort;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelCh;
        private MetroFramework.Controls.MetroCheckBox metroCheckBoxCh16;
        private MetroFramework.Controls.MetroCheckBox metroCheckBoxCh15;
        private MetroFramework.Controls.MetroCheckBox metroCheckBoxCh14;
        private MetroFramework.Controls.MetroCheckBox metroCheckBoxCh13;
        private MetroFramework.Controls.MetroCheckBox metroCheckBoxCh12;
        private MetroFramework.Controls.MetroCheckBox metroCheckBoxCh11;
        private MetroFramework.Controls.MetroCheckBox metroCheckBoxCh10;
        private MetroFramework.Controls.MetroCheckBox metroCheckBoxCh9;
        private MetroFramework.Controls.MetroCheckBox metroCheckBoxCh8;
        private MetroFramework.Controls.MetroCheckBox metroCheckBoxCh7;
        private MetroFramework.Controls.MetroCheckBox metroCheckBoxCh6;
        private MetroFramework.Controls.MetroCheckBox metroCheckBoxCh5;
        private MetroFramework.Controls.MetroCheckBox metroCheckBoxCh4;
        private MetroFramework.Controls.MetroCheckBox metroCheckBoxCh3;
        private MetroFramework.Controls.MetroCheckBox metroCheckBoxCh2;
        private MetroFramework.Controls.MetroCheckBox metroCheckBoxCh1;
        private System.Windows.Forms.Label label3;
        private MetroFramework.Controls.MetroTrackBar metroTrackBarPitch;
        private System.Windows.Forms.NumericUpDown numericUpDownPitch;
        private System.Windows.Forms.Label label4;
        private MetroFramework.Controls.MetroTrackBar metroTrackBarPitchRange;
        private System.Windows.Forms.NumericUpDown numericUpDownPitchRange;
        private System.Windows.Forms.Label label5;
        private MetroFramework.Controls.MetroTrackBar metroTrackBarFineTune;
        private System.Windows.Forms.NumericUpDown numericUpDownFineTune;
        private System.Windows.Forms.Label label6;
        private MetroFramework.Controls.MetroTrackBar metroTrackBarProgNo;
        private System.Windows.Forms.NumericUpDown numericUpDownProgNo;
        private System.Windows.Forms.Label label7;
        private MetroFramework.Controls.MetroTrackBar metroTrackBarVolume;
        private System.Windows.Forms.NumericUpDown numericUpDownVolume;
        private System.Windows.Forms.Label label8;
        private MetroFramework.Controls.MetroTrackBar metroTrackBarExpression;
        private System.Windows.Forms.NumericUpDown numericUpDownExpression;
        private System.Windows.Forms.Label label9;
        private MetroFramework.Controls.MetroTrackBar metroTrackBarPanpot;
        private System.Windows.Forms.NumericUpDown numericUpDownPanpot;
        private System.Windows.Forms.Label label10;
        private MetroFramework.Controls.MetroTrackBar metroTrackBarModulation;
        private System.Windows.Forms.NumericUpDown numericUpDownModulation;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private MetroFramework.Controls.MetroTrackBar metroTrackBarModRate;
        private System.Windows.Forms.NumericUpDown numericUpDownModRate;
        private MetroFramework.Controls.MetroTrackBar metroTrackBarModDepth;
        private System.Windows.Forms.NumericUpDown numericUpDownModDepth;
        private MetroFramework.Controls.MetroTrackBar metroTrackBarModDelay;
        private System.Windows.Forms.NumericUpDown numericUpDownModDelay;
        private System.Windows.Forms.NumericUpDown numericUpDownModDepthRangeNote;
        private MetroFramework.Controls.MetroTrackBar metroTrackBarModDepthRangeNote;
        private MetroFramework.Controls.MetroTrackBar metroTrackBarModDepthRangeCent;
        private System.Windows.Forms.NumericUpDown numericUpDownModDepthRangeCent;
        private System.Windows.Forms.Label label16;
        private MetroFramework.Controls.MetroTrackBar metroTrackBarHold;
        private System.Windows.Forms.NumericUpDown numericUpDown3;
        private System.Windows.Forms.Label label17;
        private MetroFramework.Controls.MetroTrackBar metroTrackBarPortament;
        private System.Windows.Forms.NumericUpDown numericUpDownPortament;
        private System.Windows.Forms.Label label18;
        private MetroFramework.Controls.MetroTrackBar metroTrackBarPortamentTime;
        private System.Windows.Forms.NumericUpDown numericUpDownPortamentTime;
        private System.Windows.Forms.Label label19;
        private MetroFramework.Controls.MetroTrackBar metroTrackBarLegatoFootSw;
        private System.Windows.Forms.NumericUpDown numericUpDownLegatoFootSw;
        private System.Windows.Forms.Label label20;
        private MetroFramework.Controls.MetroTrackBar metroTrackBarMonoMode;
        private System.Windows.Forms.NumericUpDown numericUpDownMonoMode;
        private System.Windows.Forms.Label label21;
        private MetroFramework.Controls.MetroTrackBar metroTrackBarPolyMode;
        private System.Windows.Forms.NumericUpDown numericUpDownPolyMode;
    }
}