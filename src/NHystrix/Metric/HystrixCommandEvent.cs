using System;
using System.Collections.Generic;
using System.Text;

namespace NHystrix.Metric
{
    /// <summary>
    /// Encapsulates and event that occurred as a result of executing a <see cref="HystrixCommand{TRequest, TResult}"/>.
    /// </summary>
    /// <seealso cref="NHystrix.Metric.IHystrixEvent" />
    public class HystrixCommandEvent : IHystrixEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixCommandEvent"/> class.
        /// </summary>
        /// <param name="commandKey">The command key.</param>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="exception">The exception. (optional)</param>
        public HystrixCommandEvent(HystrixCommandKey commandKey, HystrixEventType eventType, Exception exception = null)
        {
            CommandKey = commandKey;
            EventType = eventType;
            Exception = exception;
        }

        /// <summary>
        /// Gets the command key.
        /// </summary>
        /// <value>The command key.</value>
        public HystrixCommandKey CommandKey { get; private set; }

        /// <summary>
        /// Gets the type of the event.
        /// </summary>
        /// <value>The type of the event.</value>
        public HystrixEventType EventType { get; private set; }

        /// <summary>
        /// Gets the exception.
        /// </summary>
        /// <value>The exception. Null if no exception occurred.</value>
        public Exception Exception { get; private set; }
    }
}
