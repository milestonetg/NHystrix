using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace NHystrix.Exceptions
{
    /// <summary>
    /// Base class for all NHystrix exceptions that occur as part of command execution.
    /// You can catch this exception to catch all exceptions thrown my Hystrix that are not bad requests.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class HystrixRuntimeException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixRuntimeException"/> class.
        /// </summary>
        public HystrixRuntimeException() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixRuntimeException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public HystrixRuntimeException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixRuntimeException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public HystrixRuntimeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixRuntimeException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"></see> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"></see> that contains contextual information about the source or destination.</param>
        protected HystrixRuntimeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
