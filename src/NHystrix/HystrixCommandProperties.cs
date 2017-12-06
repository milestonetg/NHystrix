namespace NHystrix
{
    /// <summary>
    /// Configuration properties for <see cref="HystrixCommand{TResult}"/> and related components.
    /// </summary>
    public class HystrixCommandProperties
    {
        /// <summary>
        /// Gets or sets a value indicating whether [circuit breaker force open].
        /// </summary>
        /// <value><c>true</c> if [circuit breaker force open]; otherwise, <c>false</c>. Default is false.</value>
        public bool CircuitBreakerForceOpen { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [circuit breaker force closed].
        /// </summary>
        /// <value><c>true</c> if [circuit breaker force closed]; otherwise, <c>false</c>. Default is false.</value>
        public bool CircuitBreakerForceClosed { get; set; }

        /// <summary>
        /// Gets or sets the circuit breaker sleep window in milliseconds.
        /// </summary>
        /// <value>The circuit breaker sleep window in milliseconds. Default is 5000.</value>
        public long CircuitBreakerSleepWindowInMilliseconds { get; set; } = 5000;// default => sleepWindow: 5000 = 5 seconds that we will sleep before trying again after tripping the circuit

        /// <summary>
        /// Gets or sets the circuit breaker error threshold percentage.
        /// </summary>
        /// <value>The circuit breaker error threshold percentage. Default is 50.</value>
        public int CircuitBreakerErrorThresholdPercentage { get; set; } = 50;// default => errorThresholdPercentage = 50 = if 50%+ of requests in 10 seconds are failures or latent then we will trip the circuit

        /// <summary>
        /// Number of requests that must be made within a statisticalWindow before open/close decisions are made using stats
        /// </summary>
        /// <value>The circuit breaker request volume threshold. Default is 20.</value>
        public int CircuitBreakerRequestVolumeThreshold { get; set; } = 20;// default => statisticalWindowVolumeThreshold: 20 requests in 10 seconds must occur before statistics matter

        /// <summary>
        /// Gets or sets the execution timeout in milliseconds.
        /// </summary>
        /// <value>The execution timeout in milliseconds. Default is 1000.</value>
        public int ExecutionTimeoutInMilliseconds { get; set; } = 1000; // default => executionTimeoutInMilliseconds: 1000 = 1 second

        /// <summary>
        /// Gets or sets the maximum concurrent requests.
        /// </summary>
        /// <value>The maximum concurrent requests. Default is 10.</value>
        public int MaxConcurrentRequests { get; set; } = 10;

        /// <summary>
        /// Gets or sets a value indicating whether [fallback enabled].
        /// </summary>
        /// <value><c>true</c> if [fallback enabled]; otherwise, <c>false</c>. Default is false.</value>
        public bool FallbackEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [circuit breaker enabled].
        /// </summary>
        /// <value><c>true</c> if [circuit breaker enabled]; otherwise, <c>false</c>. Default is true.</value>
        public bool CircuitBreakerEnabled { get; set; } = true;

        /// <summary>
        /// Whether timeout should be triggered
        /// </summary>
        /// <value><c>true</c> if [timeout enabled]; otherwise, <c>false</c>. Default is true.</value>
        public bool TimeoutEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether [bulkheading enabled].
        /// </summary>
        /// <value><c>true</c> if [bulkheading enabled]; otherwise, <c>false</c>. Default is true.</value>
        public bool BulkheadingEnabled { get; set; } = true;

        /// <summary>
        /// The amount of time bulkheading should wait to acquire a semaphore, in milliseconds, before
        /// returning any fallback responses.
        /// </summary>
        /// <value>The bulkhead semaphore acquire timeout in milliseconds. Default is 1.</value>
        public int BulkheadSemaphoreAcquireTimeoutInMilliseconds { get; set; } = 1;

        /// <summary>
        /// Gets or sets the metrics rolling statistical window in milliseconds.
        /// </summary>
        /// <value>The metrics rolling statistical window in milliseconds. Default is 1000.</value>
        public int MetricsRollingStatisticalWindowInMilliseconds { get; set; } = 10000; // default => statisticalWindow: 10000 = 10 seconds (and default of 10 buckets so each bucket is 1 second)

        /// <summary>
        /// Gets or sets the metrics rolling statistical window buckets.
        /// </summary>
        /// <value>The metrics rolling statistical window buckets. Default is 10.</value>
        public int MetricsRollingStatisticalWindowBuckets { get; set; } = 10;// default => statisticalWindowBuckets: 10 = 10 buckets in a 10 second window so each bucket is 1 second
    }
}
