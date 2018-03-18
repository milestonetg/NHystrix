﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace NHystrix.Exceptions
{
    /// <summary>
    /// An exception representing an error with provided arguments or state rather than an execution failure.
    /// <para>
    /// Unlike all other exceptions thrown by a <see cref="HystrixCommand{TRequest, TResult}"/> this will not trigger fallback, 
    /// not count against failure metrics and thus not trigger the circuit breaker.
    /// </para>
    /// 
    /// > [!IMPORTANT]
    /// > NOTE: This should **only** be used when an error is due to user input such as <see cref="ArgumentException"/> 
    /// > otherwise it defeats the purpose of fault-tolerance and fallback behavior.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class HystrixBadRequestException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixBadRequestException"/> class.
        /// </summary>
        public HystrixBadRequestException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixBadRequestException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public HystrixBadRequestException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixBadRequestException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public HystrixBadRequestException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixBadRequestException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"></see> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"></see> that contains contextual information about the source or destination.</param>
        protected HystrixBadRequestException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
