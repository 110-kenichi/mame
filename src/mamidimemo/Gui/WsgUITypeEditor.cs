﻿// copyright-holders:K.Ito
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using zanac.MAmidiMEmo.Instruments;

namespace zanac.MAmidiMEmo.Gui
{
    /// <summary>
    /// 
    /// </summary>
    public class WsgUITypeEditor : ArrayEditor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        public WsgUITypeEditor(Type type) : base(type)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="provider"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            IWindowsFormsEditorService editorService = null;

            if (provider != null)
                editorService = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

            if (editorService == null)
                return value;

            WsgBitWideAttribute watt = (WsgBitWideAttribute)context.PropertyDescriptor.Attributes[typeof(WsgBitWideAttribute)];

            using (FormWsgEditor frm = new FormWsgEditor())
            {
                frm.WsgBitWide = watt.BitWide;
                frm.Tag = context;
                byte[] orgByteValue = null;
                sbyte[] orgSbyteValue = null;

                if (value.GetType() == typeof(byte[]))
                {
                    frm.ByteWsgData = (byte[])value;
                    frm.ValueChanged += Frm_ValueChangedByte;
                    orgByteValue = new byte[frm.ByteWsgData.Length];
                    for (int i = 0; i < frm.ByteWsgData.Length; i++)
                        orgByteValue[i] = frm.ByteWsgData[i];
                }
                else if (value.GetType() == typeof(sbyte[]))
                {
                    frm.WsgSigned = true;
                    frm.SbyteWsgData = (sbyte[])value;
                    frm.ValueChanged += Frm_ValueChangedSbyte;
                    orgSbyteValue = new sbyte[frm.SbyteWsgData.Length];
                    for (int i = 0; i < frm.SbyteWsgData.Length; i++)
                        orgSbyteValue[i] = frm.SbyteWsgData[i];
                }

                DialogResult dr = frm.ShowDialog();
                if (dr != DialogResult.OK)
                {
                    if (value.GetType() == typeof(byte[]))
                    {
                        try
                        {
                            //InstrumentManager.ExclusiveLockObject.EnterWriteLock();

                            context.PropertyDescriptor.SetValue(context.Instance, orgByteValue);
                        }
                        finally
                        {
                            //InstrumentManager.ExclusiveLockObject.ExitWriteLock();
                        }
                    }
                    else if (value.GetType() == typeof(sbyte[]))
                    {
                        try
                        {
                            //InstrumentManager.ExclusiveLockObject.EnterWriteLock();

                            context.PropertyDescriptor.SetValue(context.Instance, orgSbyteValue);
                        }
                        finally
                        {
                            //InstrumentManager.ExclusiveLockObject.ExitWriteLock();
                        }
                    }
                }
            }
            return value;
        }


        private void Frm_ValueChangedByte(object sender, EventArgs e)
        {
            FormWsgEditor editor = (FormWsgEditor)sender;
            ITypeDescriptorContext ctx = (ITypeDescriptorContext)editor.Tag;
            if (ctx != null)
            {
                var value = editor.ByteWsgData;
                try
                {
                    //InstrumentManager.ExclusiveLockObject.EnterWriteLock();

                    ctx.PropertyDescriptor.SetValue(ctx.Instance, value);
                }
                finally
                {
                    //InstrumentManager.ExclusiveLockObject.ExitWriteLock();
                }
            }

        }

        private void Frm_ValueChangedSbyte(object sender, EventArgs e)
        {
            FormWsgEditor editor = (FormWsgEditor)sender;
            ITypeDescriptorContext ctx = (ITypeDescriptorContext)editor.Tag;
            if (ctx != null)
            {
                var value = editor.SbyteWsgData;
                try
                {
                    //InstrumentManager.ExclusiveLockObject.EnterWriteLock();

                    ctx.PropertyDescriptor.SetValue(ctx.Instance, value);
                }
                finally
                {
                    //InstrumentManager.ExclusiveLockObject.ExitWriteLock();
                }
            }

        }

    }
}
