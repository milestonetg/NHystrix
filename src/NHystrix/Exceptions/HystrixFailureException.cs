using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace NHystrix.Exceptions
{
    /// <summary>
    /// Class HystrixFailureException.
    /// </summary>
    /// <seealso cref="NHystrix.Exceptions.HystrixRuntimeException" />
    public class HystrixFailureException : HystrixRuntimeException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixFailureException"/> class.
        /// </summary>
        /// <param name="failureType">Type of the failure.</param>
        public HystrixFailureException(FailureType failureType) : base()
        {
            FailureType = failureType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixFailureException"/> class.
        /// </summary>
        /// <param name="failureType">Type of the failure.</param>
        /// <param name="message">The message.</param>
        public HystrixFailureException(FailureType failureType, string message) : base(message)
        {
            FailureType = failureType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixFailureException"/> class.
        /// </summary>
        /// <param name="failureType">Type of the failure.</param>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public HystrixFailureException(FailureType failureType, string message, Exception innerException) : base(message, innerException)
        {
            FailureType = failureType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixFailureException"/> class.
        /// </summary>
        /// <param name="failureType">Type of the failure.</param>
        /// <param name="info">The information.</param>
        /// <param name="context">The context.</param>
        protected HystrixFailureException(FailureType failureType, SerializationInfo info, StreamingContext context) : base(info, context)
        {
            FailureType = failureType;
        }

        /// <summary>
        /// Gets the type of the failure. Can be used to determine if the failure was due to a short-circuit or semaphore rejection.
        /// </summary>
        /// <value>The type of the failure.</value>
        public FailureType FailureType { get; }

    }
}
