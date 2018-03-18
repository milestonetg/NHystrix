using NHystrix.Metric;
using System;
using System.Collections.Generic;
using System.Text;

namespace NHystrix
{
    /// <summary>
    /// Base class for implementing a metric within NHystrix.
    /// </summary>
    public abstract class HystrixMetrics
    {
        private readonly HystrixRollingNumber counter;

        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixMetrics"/> class.
        /// </summary>
        /// <param name="counter">The counter.</param>
        protected HystrixMetrics(HystrixRollingNumber counter)
        {
            this.counter = counter;
        }

        /// <summary>
        /// Get the cumulative count since the start of the application for the given <see cref="HystrixEventType"/>
        /// </summary>
        /// <param name="eventType">Type of the event.</param>
        /// <returns>long cumulative count</returns>
        public long GetCumulativeCount(HystrixEventType eventType) {
            return counter.GetCumulativeSum(eventType);
        }

        /// <summary>
        /// Get the rolling count for the given <see cref="HystrixEventType"/>.
        /// </summary>
        /// <param name="eventType">Type of the event.</param>
        /// <returns>long rolling count</returns>
        public long GetRollingCount(HystrixEventType eventType) {
            return counter.GetRollingSum(eventType);
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public void Reset()
        {
            counter.Reset();
        }
    }
}
