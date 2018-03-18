using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace NHystrix.Exceptions
{
    /// <summary>
    /// Thrown when a fallback attempt fails.
    /// </summary>
    /// <seealso cref="NHystrix.Exceptions.HystrixFailureException" />
    public class HystrixFallbackException : HystrixFailureException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixFallbackException"/> class.
        /// </summary>
        /// <param name="failureType">Type of the failure.</param>
        /// <param name="originalException">The original exception.</param>
        public HystrixFallbackException(FailureType failureType, Exception originalException) : base(failureType)
        {
            OriginalException = originalException;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixFallbackException"/> class.
        /// </summary>
        /// <param name="failureType">Type of the failure.</param>
        /// <param name="message">The message.</param>
        /// <param name="originalException">The original exception.</param>
        public HystrixFallbackException(FailureType failureType, string message, Exception originalException) : base(failureType, message)
        {
            OriginalException = originalException;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixFallbackException"/> class.
        /// </summary>
        /// <param name="failureType">Type of the failure.</param>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        /// <param name="originalException">The original exception.</param>
        public HystrixFallbackException(FailureType failureType, string message, Exception innerException, Exception originalException) : base(failureType, message, innerException)
        {
            OriginalException = originalException;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixFallbackException"/> class.
        /// </summary>
        /// <param name="failureType">Type of the failure.</param>
        /// <param name="originalException">The original exception.</param>
        /// <param name="info">The information.</param>
        /// <param name="context">The context.</param>
        protected HystrixFallbackException(FailureType failureType, Exception originalException, SerializationInfo info, StreamingContext context) : base(failureType, info, context)
        {
            OriginalException = originalException;
        }

        /// <summary>
        /// Gets the original exception the caused the fallback to be triggered.
        /// </summary>
        /// <value>The original exception.</value>
        public Exception OriginalException { get;  }
    }
}
