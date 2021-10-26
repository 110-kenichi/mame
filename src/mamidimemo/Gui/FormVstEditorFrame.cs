using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using Jacobi.Vst.Core;
using Jacobi.Vst.Interop.Host;
using MetroFramework.Forms;
using zanac.MAmidiMEmo.Instruments;

namespace zanac.MAmidiMEmo.Gui
{
    /// <summary>
    /// The frame in which a custom plugin editor UI is displayed.
    /// </summary>
    public partial class FormVstEditorFrame : Form
    {

        private List<InstrumentBase> instruments;

        /// <summary>
        /// Default ctor.
        /// </summary>
        public FormVstEditorFrame()
        {
            InitializeComponent();

            InstrumentManager.InstrumentChanged += InstrumentManager_InstrumentChanged;
            InstrumentManager.InstrumentRemoved += InstrumentManager_InstrumentRemoved;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InstrumentManager_InstrumentRemoved(object sender, EventArgs e)
        {
            if (!IsHandleCreated || IsDisposed)
                return;

            List<InstrumentBase> insts = new List<InstrumentBase>();
            foreach (var inst in InstrumentManager.GetAllInstruments())
            {
                foreach (var i in instruments)
                {
                    if (i.DeviceID == inst.DeviceID && i.UnitNumber == inst.UnitNumber)
                        insts.Add(inst);
                }
            }

            instruments = insts;
            if (insts.Count == 0)
                Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InstrumentManager_InstrumentChanged(object sender, EventArgs e)
        {
            if (!IsHandleCreated || IsDisposed)
                return;

            List<InstrumentBase> insts = new List<InstrumentBase>();
            foreach (var inst in InstrumentManager.GetAllInstruments())
            {
                foreach (var i in instruments)
                {
                    if (i.DeviceID == inst.DeviceID && i.UnitNumber == inst.UnitNumber)
                        insts.Add(inst);
                }
            }

            instruments = insts;
            if (insts.Count == 0)
                Close();
        }

        /// <summary>
        /// Gets or sets the Plugin Command Stub.
        /// </summary>
        public Jacobi.Vst.Core.Host.IVstPluginCommandStub PluginCommandStub { get; set; }

        /// <summary>
        /// Shows the custom plugin editor UI.
        /// </summary>
        /// <param name="owner"></param>
        /// <returns></returns>
        public DialogResult ShowDialog(IWin32Window owner, Instruments.InstrumentBase inst)
        {
            Rectangle wndRect = new Rectangle();

            this.Text = PluginCommandStub.GetEffectName();

            if (PluginCommandStub.EditorGetRect(out wndRect))
            {
                this.Size = this.SizeFromClientSize(new Size(wndRect.Width, wndRect.Height));
                PluginCommandStub.EditorOpen(this.Handle);
            }

            instruments = new List<InstrumentBase>();
            instruments.Add(inst);

            return base.ShowDialog(owner);
        }

        /// <summary>
        /// Shows the custom plugin editor UI.
        /// </summary>
        /// <param name="owner"></param>
        /// <returns></returns>
        public void Show(IWin32Window owner, Instruments.InstrumentBase inst)
        {
            Rectangle wndRect = new Rectangle();

            this.Text = PluginCommandStub.GetEffectName();

            if (PluginCommandStub.EditorGetRect(out wndRect))
            {
                this.Size = this.SizeFromClientSize(new Size(wndRect.Width, wndRect.Height));
                PluginCommandStub.EditorOpen(this.Handle);
            }

            instruments = new List<InstrumentBase>();
            instruments.Add(inst);

            base.Show(owner);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            if (e.Cancel == false)
            {
                PluginCommandStub.EditorClose();
            }
        }
    }
}
