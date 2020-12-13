// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Remoting.Services;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using zanac.MAmidiMEmo.Instruments;

namespace zanac.MAmidiMEmo.ComponentModel
{
    public class MidiHookProxy : RealProxy
    {
        private MarshalByRefObject f_Target;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="t"></param>
        public MidiHookProxy(MarshalByRefObject target, Type t) : base(t)
        {
            this.f_Target = target;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public override IMessage Invoke(IMessage msg)
        {
            IConstructionCallMessage ccm = msg as IConstructionCallMessage;
            if (ccm != null)
            {
                return this.InitializeServerObject(ccm);

            }

            IMethodCallMessage mcm = (IMethodCallMessage)msg;
            object[] args = mcm.Args;
            try
            {
                var mb = mcm.MethodBase;
                if (mb.Name.StartsWith("set_"))
                {
                    try
                    {
                        InstrumentManager.ExclusiveLockObject.EnterWriteLock();

                        return RemotingServices.ExecuteMessage(this.f_Target, mcm) as ReturnMessage;
                    }
                    finally
                    {
                        InstrumentManager.ExclusiveLockObject.ExitWriteLock();
                    }
                }
                else
                {
                    return RemotingServices.ExecuteMessage(this.f_Target, mcm) as ReturnMessage;
                }
            }
            catch (TargetInvocationException ex)
            {
                return new ReturnMessage(ex.InnerException, mcm);
            }
        }

    }
}
