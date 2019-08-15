﻿// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Proxies;
using System.Text;
using System.Threading.Tasks;

namespace zanac.MAmidiMEmo.ComponentModel
{
    /// <summary>
    /// 
    /// </summary>
    public class MidiHookAttribute : ProxyAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serverType"></param>
        /// <returns></returns>
        public override MarshalByRefObject CreateInstance(Type serverType)
        {
            MarshalByRefObject target = base.CreateInstance(serverType);
            RealProxy rp;
            rp = new MidiHookProxy(target, serverType);
            return rp.GetTransparentProxy() as MarshalByRefObject;
        }
    }
}
