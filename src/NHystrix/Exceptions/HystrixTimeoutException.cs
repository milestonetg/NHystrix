using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace NHystrix.Exceptions
{
    /// <summary>
    /// Thrown when a <see cref="HystrixCommand{TRequest, TResult}"/> or <see cref="HystrixDelegatingHandler"/> times out.
    /// </summary>
    /// <seealso cref="NHystrix.Exceptions.HystrixRuntimeException" />
    public class HystrixTimeoutException : HystrixRuntimeException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixTimeoutException"/> class.
        /// </summary>
        public HystrixTimeoutException() 
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixTimeoutException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public HystrixTimeoutException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixTimeoutException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public HystrixTimeoutException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixTimeoutException"/> class.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="context">The context.</param>
        protected HystrixTimeoutException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
