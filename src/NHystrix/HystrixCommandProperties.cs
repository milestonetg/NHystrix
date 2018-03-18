namespace NHystrix
{
    /// <summary>
    /// Configuration properties for <see cref="HystrixCommand{TRequest, TResult}"/> and related components.
    /// </summary>
    public class HystrixCommandProperties
    {
        /// <summary>
        /// Gets or sets the circuit breaker options.
        /// </summary>
        /// <value>The circuit breaker options.</value>
        public CircuitBreakerOptions CircuitBreakerOptions { get; set; } = new CircuitBreakerOptions();

        /// <summary>
        /// Gets or sets the execution timeout in milliseconds. Default is 1000.
        /// </summary>
        /// <value>The execution timeout in milliseconds. Default is 1000.</value>
        public int ExecutionTimeoutInMilliseconds { get; set; } = 1000; // default => executionTimeoutInMilliseconds: 1000 = 1 second

        /// <summary>
        /// Gets or sets the maximum concurrent requests. Default is 10.
        /// </summary>
        /// <value>The maximum concurrent requests. Default is 10.</value>
        public int MaxConcurrentRequests { get; set; } = 10;

        /// <summary>
        /// Gets or sets a value indicating whether [fallback enabled]. Default is false.
        /// </summary>
        /// <value><c>true</c> if [fallback enabled]; otherwise, <c>false</c>. Default is false.</value>
        public bool FallbackEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [circuit breaker enabled]. Default is true.
        /// </summary>
        /// <value><c>true</c> if [circuit breaker enabled]; otherwise, <c>false</c>. Default is true.</value>
        public bool CircuitBreakerEnabled { get; set; } = true;

        /// <summary>
        /// Whether timeout should be triggered. Default is true.
        /// </summary>
        /// <value><c>true</c> if [timeout enabled]; otherwise, <c>false</c>. Default is true.</value>
        public bool TimeoutEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether [bulkheading enabled]. Default is true.
        /// </summary>
        /// <value><c>true</c> if [bulkheading enabled]; otherwise, <c>false</c>. Default is true.</value>
        public bool BulkheadingEnabled { get; set; } = true;

        /// <summary>
        /// The amount of time bulkheading should wait to acquire a semaphore, in milliseconds, before
        /// returning any fallback responses. Default is 1.
        /// </summary>
        /// <value>The bulkhead semaphore acquire timeout in milliseconds. Default is 1.</value>
        public int BulkheadSemaphoreAcquireTimeoutInMilliseconds { get; set; } = 1;

        /// <summary>
        /// Gets or sets the metrics rolling statistical window in milliseconds. Default is 1000.
        /// </summary>
        /// <value>The metrics rolling statistical window in milliseconds. Default is 1000.</value>
        public int MetricsRollingStatisticalWindowInMilliseconds { get; set; } = 10000; // default => statisticalWindow: 10000 = 10 seconds (and default of 10 buckets so each bucket is 1 second)

        /// <summary>
        /// Gets or sets the metrics rolling statistical window buckets. Default is 10.
        /// </summary>
        /// <value>The metrics rolling statistical window buckets. Default is 10.</value>
        public int MetricsRollingStatisticalWindowBuckets { get; set; } = 10;// default => statisticalWindowBuckets: 10 = 10 buckets in a 10 second window so each bucket is 1 second
    }
}
