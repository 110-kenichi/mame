using System;
using ZeroFormatter;

namespace LegacyWrapper.Common.Serialization
{
    /// <summary>
    /// Class to transmit info to the server. The server will execute an appropriate call and eventually return the results.
    /// </summary>
    [ZeroFormattable]
    [Serializable]
    public class CallData
    {
        /// <summary>
        /// Name of the procedure to call.
        /// </summary>
        [Index(0)]
        public virtual string ProcedureName { get; set; }

        /// <summary>
        /// Array of parameters to pass to the function call.
        /// </summary>
        [Index(1)]
        public virtual uint[] Parameters { get; set; }

        /// <summary>
        /// Delegate type to use for the call.
        /// </summary>
        [Index(2)]
        public virtual string DelegateType { get; set; }

        /// <summary>
        /// Status indicating if the wrapper executable should close the connection and terminate itself
        /// </summary>
        [Index(3)]
        public virtual KeepAliveStatus Status { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum KeepAliveStatus
    {
        /// <summary>
        /// 
        /// </summary>
        KeepAlive,
        /// <summary>
        /// 
        /// </summary>
        Close
    }
}
