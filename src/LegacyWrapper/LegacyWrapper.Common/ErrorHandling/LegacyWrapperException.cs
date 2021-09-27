using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LegacyWrapper.ErrorHandling
{
    [Serializable]
    public class LegacyWrapperException : Exception
    {

        /// <summary>
        /// Creates a new instance of <see cref="LegacyWrapperException"/>.
        /// </summary>
        public LegacyWrapperException()
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="LegacyWrapperException"/> with the specified error message.
        /// </summary>
        /// <param name="message">Error message.</param>
        public LegacyWrapperException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="LegacyWrapperException"/> with the specified error message and inner exception.
        /// </summary>
        /// <param name="message">Error message.</param>
        /// <param name="innerException">Inner exception.</param>
        public LegacyWrapperException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="LegacyWrapperException"/> with the specified serialization info and streaming context.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected LegacyWrapperException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
