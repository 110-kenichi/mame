// copyright-holders:K.Ito
using AspectInjector.Broker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using zanac.MAmidiMEmo.Instruments;

namespace zanac.MAmidiMEmo.ComponentModel
{

    [Aspect(Scope.PerInstance)]
    [Injection(typeof(InstLockAttribute), Inherited = true)]
    public class InstLockAttribute : Attribute
    {
        [Advice(Kind.Before, Targets = Target.Setter)] // you can have also After (async-aware), and Around(Wrap/Instead) kinds
        public void Invoking([Argument(Source.Name)] string name, [Argument(Source.Metadata)] MethodBase method)
        {
            //if (method.Name.StartsWith("set_"))
            {
                InstrumentManager.InstExclusiveLockObject.EnterWriteLock();
            }
        }

        [Advice(Kind.After, Targets = Target.Setter)] // you can have also After (async-aware), and Around(Wrap/Instead) kinds
        public void Invoked([Argument(Source.Name)] string name, [Argument(Source.Metadata)] MethodBase method)
        {
            //if (method.Name.StartsWith("set_"))
            {
                InstrumentManager.InstExclusiveLockObject.ExitWriteLock();
            }
        }

    }
}
