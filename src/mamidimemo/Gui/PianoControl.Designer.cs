using zanac.MAmidiMEmo.Instruments;

namespace zanac.MAmidiMEmo.Gui
{
    partial class PianoControl
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
            if (disposing)
            {
                SoundBase.SoundKeyOn -= SoundBase_SoundKeyOn;
                SoundBase.SoundKeyOff -= SoundBase_SoundKeyOff;
                //SoundBase.SoundSoundOff -= SoundBase_SoundSoundOff;
                SoundBase.SoundPitchUpdated -= SoundBase_SoundPitchUpdated;

                blackBrush?.Dispose();
                whiteBrush?.Dispose();
                //blueBrush?.Dispose();
                blackPen?.Dispose();

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
            this.toolTipSlider = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // toolTipSlider
            // 
            this.toolTipSlider.ShowAlways = true;
            this.toolTipSlider.UseAnimation = false;
            this.toolTipSlider.UseFading = false;
            // 
            // PianoControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "PianoControl";
            this.Size = new System.Drawing.Size(749, 188);
            this.MouseHover += new System.EventHandler(this.PianoControl_MouseHover);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolTip toolTipSlider;
    }
}
