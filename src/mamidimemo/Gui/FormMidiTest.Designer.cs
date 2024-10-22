using Melanchall.DryWetMidi.Multimedia;
using zanac.MAmidiMEmo.Midi;

namespace zanac.MAmidiMEmo.Gui
{
    partial class FormMidiTest
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
            if (disposing)
            {
                if (outputDevice != null)
                {
                    outputDevice.Dispose();
                }
                MidiManager.MidiEventHooked -= MidiManager_MidiEventHooked;
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMidiTest));
            this.button1 = new MetroFramework.Controls.MetroButton();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBoxOutPort = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.labelSendTime = new System.Windows.Forms.Label();
            this.labelReceiveTime = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.metroButtonGo = new MetroFramework.Controls.MetroButton();
            this.labelSpan = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            resources.ApplyResources(this.button1, "button1");
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Name = "button1";
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.comboBoxOutPort, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.label4, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.labelSendTime, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.labelReceiveTime, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.pictureBox1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.metroButtonGo, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.labelSpan, 2, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // comboBoxOutPort
            // 
            this.comboBoxOutPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxOutPort.FormattingEnabled = true;
            resources.ApplyResources(this.comboBoxOutPort, "comboBoxOutPort");
            this.comboBoxOutPort.Name = "comboBoxOutPort";
            this.comboBoxOutPort.DropDown += new System.EventHandler(this.comboBoxOutPort_DropDown);
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // labelSendTime
            // 
            resources.ApplyResources(this.labelSendTime, "labelSendTime");
            this.labelSendTime.Name = "labelSendTime";
            // 
            // labelReceiveTime
            // 
            resources.ApplyResources(this.labelReceiveTime, "labelReceiveTime");
            this.labelReceiveTime.Name = "labelReceiveTime";
            // 
            // pictureBox1
            // 
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.tableLayoutPanel1.SetColumnSpan(this.pictureBox1, 3);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            // 
            // metroButtonGo
            // 
            resources.ApplyResources(this.metroButtonGo, "metroButtonGo");
            this.metroButtonGo.Name = "metroButtonGo";
            this.metroButtonGo.MouseDown += new System.Windows.Forms.MouseEventHandler(this.metroButtonGo_MouseDown);
            // 
            // labelSpan
            // 
            resources.ApplyResources(this.labelSpan, "labelSpan");
            this.labelSpan.Name = "labelSpan";
            // 
            // FormMidiTest
            // 
            this.AcceptButton = this.button1;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.button1;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.button1);
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormMidiTest";
            this.Resizable = false;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private MetroFramework.Controls.MetroButton button1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBoxOutPort;
        private System.Windows.Forms.PictureBox pictureBox1;
        private MetroFramework.Controls.MetroButton metroButtonGo;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label labelSendTime;
        private System.Windows.Forms.Label labelReceiveTime;
        private System.Windows.Forms.Label labelSpan;
    }
}