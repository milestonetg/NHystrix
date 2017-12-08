using System;
using System.Collections.Generic;
using System.Text;

namespace NHystrix
{
    /// <summary>
    /// Encapsulates health related statistics sent to the HealthStream of a HystrixCommandMetric.
    /// </summary>
    public class HealthCounts
    {
        /// <summary>
        /// The total number of attempted requests within the reporting window.
        /// </summary>
        /// <value>The request count.</value>
        /// <remarks>
        /// In the <see cref="HystrixCircuitBreaker"/>, this is the value
        /// compared to the <see cref="HystrixCommandProperties.CircuitBreakerRequestVolumeThreshold"/>
        /// to ensure we've met the minimum number of requests before evaluating
        /// the failure percentage.
        /// </remarks>
        public long RequestCount { get; set; }

        /// <summary>
        /// The number of failed requests within the reporting window.
        /// </summary>
        /// <value>The failed request count.</value>
        public long FailedRequestCount { get; set; }

        /// <summary>
        /// The failure percentage as a whole number.
        /// </summary>
        /// <value>The failure percentage as a whole number.</value>
        /// <remarks>
        /// In the <see cref="HystrixCircuitBreaker"/>, this is the value
        /// compared to the <see cref="HystrixCommandProperties.CircuitBreakerErrorThresholdPercentage"/>
        /// to determine if the circuit breaker should be tripped.
        /// </remarks>
        public int FailurePercentage
        {
            get => (int)(((double)FailedRequestCount / RequestCount) * 100);
        }
    }
}
