using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;
using System.Windows.Forms;

namespace zanac.MAmidiMEmo.ComponentModel
{
    public class RuntimeServiceProvider : IServiceProvider, ITypeDescriptorContext
    {
        #region IServiceProvider Members

        object IServiceProvider.GetService(Type serviceType)
        {
            if (serviceType == typeof(IWindowsFormsEditorService))
            {
                return new WindowsFormsEditorService();
            }

            return null;
        }

        class WindowsFormsEditorService : IWindowsFormsEditorService
        {
            #region IWindowsFormsEditorService Members

            public void DropDownControl(Control control)
            {
            }

            public void CloseDropDown()
            {
            }

            public System.Windows.Forms.DialogResult ShowDialog(Form dialog)
            {
                return dialog.ShowDialog();
            }

            #endregion
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="container"></param>
        /// <param name="instance"></param>
        /// <param name="propertyDescriptor"></param>
        public RuntimeServiceProvider(IContainer container, object instance, PropertyDescriptor propertyDescriptor)
        {
            this.container = container;
            this.instance = instance;
            this.propertyDescriptor = propertyDescriptor;
        }

        #region ITypeDescriptorContext Members

        public void OnComponentChanged()
        {
        }

        IContainer container;
        object instance;
        PropertyDescriptor propertyDescriptor;

        public IContainer Container
        {
            get { return container; }
        }

        public bool OnComponentChanging()
        {
            return true; // true to keep changes, otherwise false
        }

        public object Instance
        {
            get { return instance; }
        }

        public PropertyDescriptor PropertyDescriptor
        {
            get { return propertyDescriptor; }
        }

        #endregion
    }
}
