using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZeroFormatter;

namespace LegacyWrapper.Common.Serialization
{
    /// <summary>
    /// Class used to return the call result back to the client. 
    /// Includes return value, eventually changed ref params, and if thrown in the wrapper, an exception.
    /// </summary>
    [ZeroFormattable]
    [Serializable]
    public class CallResult
    {
        /// <summary>
        /// Result object of the call.
        /// </summary>
        [Index(0)]
        public virtual uint? Result { get; set; }

        /// <summary>
        /// Array of parameters passed to the function call.
        /// The original parameters may have changed due to ref parameters used in the dll function.
        /// </summary>
        [Index(1)]
        public virtual uint[] Parameters { get; set; }

        /// <summary>
        /// If the library call in the wrapper throws an exception, it will be delivered here.
        /// </summary>
        [Index(2)]
        public virtual string Exception { get; set; }
    }
}
