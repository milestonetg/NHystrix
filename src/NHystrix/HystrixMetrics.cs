using NHystrix.Metric;
using System;
using System.Collections.Generic;
using System.Text;

namespace NHystrix
{
    public abstract class HystrixMetrics
    {
        protected readonly HystrixRollingNumber counter;

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
        public long getRollingCount(HystrixEventType eventType) {
            return counter.GetRollingSum(eventType);
        }
    }
}
