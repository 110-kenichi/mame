// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;
using System.Windows.Forms.PropertyGridInternal;
using zanac.MAmidiMEmo.Properties;

namespace zanac.MAmidiMEmo.ComponentModel
{
    public class TimbreChipTab : PropertiesTab
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="component"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object component, Attribute[] attributes)
        {
            var props = base.GetProperties(context, component, attributes);
            if (!(component is Instruments.TimbreBase))
                return props;

            List<PropertyDescriptor> pdlist = new List<PropertyDescriptor>();
            foreach (PropertyDescriptor p in props)
            {
                var atr = p.Attributes[typeof(CategoryAttribute)] as CategoryAttribute;
                if (atr != null && atr.Category.StartsWith(TabName, StringComparison.Ordinal))
                {
                    pdlist.Add(p);
                }
            }
            return new PropertyDescriptorCollection(pdlist.ToArray());
        }

        /// <summary>
        /// 
        /// </summary>
        public override string TabName
        {
            get
            {
                return "Chip";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override System.Drawing.Bitmap Bitmap
        {
            get
            {
                return Resources.Chip;
            }
        }
    }
}
