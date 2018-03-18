using System;
using System.Collections.Generic;
using System.Text;

namespace NHystrix.Exceptions
{
    /// <summary>
    /// Enum FailureType
    /// </summary>
    public enum FailureType
    {
        /// <summary>
        /// The bad request exception
        /// </summary>
        BAD_REQUEST_EXCEPTION,
        /// <summary>
        /// The command exception
        /// </summary>
        COMMAND_EXCEPTION,
        /// <summary>
        /// The timeout
        /// </summary>
        TIMEOUT,
        /// <summary>
        /// The failure was the result of the circuit breaker being open. 
        /// </summary>
        SHORTCIRCUIT,
        /// <summary>
        /// The rejected thread execution
        /// </summary>
        REJECTED_THREAD_EXECUTION,
        /// <summary>
        /// The failure was the result of a bulkhead semaphore not being obtained before the <see cref="HystrixCommandProperties.BulkheadSemaphoreAcquireTimeoutInMilliseconds"/>.
        /// </summary>
        REJECTED_SEMAPHORE_EXECUTION,
        /// <summary>
        /// The rejected semaphore fallback
        /// </summary>
        REJECTED_SEMAPHORE_FALLBACK
    }
}
