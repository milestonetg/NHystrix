namespace NHystrix
{
    /// <summary>
    /// Configuration options for a <see cref="HystrixCircuitBreaker"/>.
    /// </summary>
    public class CircuitBreakerOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether [circuit breaker force open]. Default is false.
        /// </summary>
        /// <value><c>true</c> if [circuit breaker force open]; otherwise, <c>false</c>. Default is false.</value>
        public bool CircuitBreakerForceOpen { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [circuit breaker force closed]. Default is false.
        /// </summary>
        /// <value><c>true</c> if [circuit breaker force closed]; otherwise, <c>false</c>. Default is false.</value>
        public bool CircuitBreakerForceClosed { get; set; }

        /// <summary>
        /// Gets or sets the circuit breaker sleep window in milliseconds. Default is 5000.
        /// </summary>
        /// <value>The circuit breaker sleep window in milliseconds. Default is 5000.</value>
        public long CircuitBreakerSleepWindowInMilliseconds { get; set; } = 5000;// default => sleepWindow: 5000 = 5 seconds that we will sleep before trying again after tripping the circuit

        /// <summary>
        /// Gets or sets the circuit breaker error threshold percentage. Default is 50.
        /// </summary>
        /// <value>The circuit breaker error threshold percentage. Default is 50.</value>
        public int CircuitBreakerErrorThresholdPercentage { get; set; } = 50;// default => errorThresholdPercentage = 50 = if 50%+ of requests in 10 seconds are failures or latent then we will trip the circuit

        /// <summary>
        /// Number of requests that must be made within a statisticalWindow before open/close decisions are made using stats. Default is 20.
        /// </summary>
        /// <value>The circuit breaker request volume threshold. Default is 20.</value>
        public int CircuitBreakerRequestVolumeThreshold { get; set; } = 20;// default => statisticalWindowVolumeThreshold: 20 requests in 10 seconds must occur before statistics matter

    }
}
