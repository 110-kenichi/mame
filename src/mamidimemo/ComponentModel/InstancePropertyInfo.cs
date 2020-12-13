using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace zanac.MAmidiMEmo.ComponentModel
{
    /// <summary>
    /// 
    /// </summary>
    public class InstancePropertyInfo
    {
        public Object Owner
        {
            get;
        }

        public PropertyInfo Property
        {
            get;
        }

        public string Formula
        {
            get;
            set;
        }

        public string Symbol
        {
            get;
            set;
        }

        public InstancePropertyInfo(object ownerObject, PropertyInfo propertyInfo)
        {
            Owner = ownerObject;
            Property = propertyInfo;
        }
    }

}
