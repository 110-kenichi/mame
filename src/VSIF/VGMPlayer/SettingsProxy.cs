using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Remoting.Services;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using zanac.VGMPlayer.Properties;
using System.Dynamic;

namespace zanac.VGMPlayer
{
    class SettingsProxy : DynamicObject
    {
        protected Settings _target;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        public SettingsProxy(Settings target)
        {
            this._target = target;

            target.PropertyChanged += Target_PropertyChanged;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Target_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            cache.Remove(e.PropertyName);
        }

        public Dictionary<String, object> cache = new Dictionary<String, object>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if(cache.TryGetValue(binder.Name, out result))
                return true;

            result = typeof(Settings).GetProperty(binder.Name).GetValue(_target);
            cache.Add(binder.Name, result);
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            typeof(Settings).GetProperty(binder.Name).SetValue(_target, value);
            return true;
        }
    }
}
