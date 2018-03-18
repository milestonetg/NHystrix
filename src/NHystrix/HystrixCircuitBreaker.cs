using System;
using System.Collections.Concurrent;
using System.Threading;

namespace NHystrix
{
    /// <summary>
    /// The default production implementation of <see cref="IHystrixCircuitBreaker"/>.
    /// </summary>
    /// <seealso cref="NHystrix.IHystrixCircuitBreaker" />
    public class HystrixCircuitBreaker : IHystrixCircuitBreaker
    {
        readonly static ConcurrentDictionary<HystrixCommandKey, IHystrixCircuitBreaker> intern = 
            new ConcurrentDictionary<HystrixCommandKey, IHystrixCircuitBreaker>();

        readonly CircuitBreakerOptions properties;
        readonly HystrixCommandMetrics metrics;

        class Status
        {
            internal const long CLOSED = 1;
            internal const long OPEN = 0;
            internal const long HALF_OPEN = 3;
        }

        // The current state of the circuit breaker.
        long status = Status.CLOSED;

        // The time the circuit was opened in Ticks.
        long circuitOpened = -1L;

        private HystrixCircuitBreaker(CircuitBreakerOptions properties, HystrixCommandMetrics metrics)
        {
            this.properties = properties;
            this.metrics = metrics;

            metrics.HealthStream.Subscribe(
                onNext => 
                {
                    // If we're not already open, evaluate the error rate...
                    if (Interlocked.Read(ref circuitOpened) == -1 &&
                        onNext.RequestCount >= properties.CircuitBreakerRequestVolumeThreshold &&
                        onNext.FailurePercentage >= properties.CircuitBreakerErrorThresholdPercentage)
                    {
                        Trip();
                    }
                });
        }

        /// <summary>
        /// Get the <see cref="IHystrixCircuitBreaker" /> instance for a given key.
        /// </summary>
        /// <param name="commandKey">The <see cref="HystrixCommandKey"/> for the command using this circuit breaker.</param>
        /// <param name="properties">The <see cref="HystrixCommandProperties"/> for the command using this circuit breaker.</param>
        /// <param name="metrics">The <see cref="HystrixCommandMetrics"/> for the command using this circuit breaker.</param>
        /// <returns>A singleton instance of IHystrixCircuitBreaker for the given <see cref="HystrixCommandKey"/></returns>
        public static IHystrixCircuitBreaker GetInstance(HystrixCommandKey commandKey, CircuitBreakerOptions properties, HystrixCommandMetrics metrics)
        {
            IHystrixCircuitBreaker cb = null;
            if (intern.TryGetValue(commandKey, out cb))
            {
                //the key existed, so we'll return the existing instance
                return cb;
            }
            else
            {
                //we need to add a new one...
                cb = new HystrixCircuitBreaker(properties, metrics);

                //try to add the key
                if (intern.TryAdd(commandKey, cb))
                {
                    return cb;
                }
                else
                {
                    //another thread beat us to it, so we'll get that instance instead.
                    intern.TryGetValue(commandKey, out cb);
                    return cb;
                }
            }
        }

        /// <summary>
        /// Every <see cref="HystrixCommand{TRequest, TResult}" /> requests asks this if it is allowed to proceed or not.  It is idempotent and does
        /// not modify any internal state, and takes into account the half-open logic which allows some requests through
        /// after the circuit has been opened.
        /// </summary>
        /// <returns><c>true</c> if the request should be allowed, <c>false</c> otherwise.</returns>
        public bool ShouldAllowRequest
        {
            get
            {
                if (properties.CircuitBreakerForceOpen)
                {
                    return false;
                }
                if (properties.CircuitBreakerForceClosed)
                {
                    return true;
                }
                if (status.Equals(Status.CLOSED))
                {
                    return true;
                }
                else
                {
                    if (status.Equals(Status.HALF_OPEN))
                    {
                        return false;
                    }
                    else
                    {
                        return IsAfterSleepWindow();
                    }
                }
            }
        }

        /// <summary>
        /// Invoked at start of command execution to attempt an execution.  This is non-idempotent - it may modify internal
        /// state.
        /// </summary>
        /// <returns><c>true</c> if execution can be attempted, <c>false</c> otherwise.</returns>
        public bool ShouldAttemptExecution
        {
            get
            {
                if (properties.CircuitBreakerForceOpen)
                {
                    return false;
                }
                if (properties.CircuitBreakerForceClosed)
                {
                    return true;
                }
                if (Interlocked.Read(ref circuitOpened) == -1L)
                {
                    return true;
                }
                else
                {
                    if (IsAfterSleepWindow())
                    {
                        //only the first request after sleep window should execute
                        //if the executing command succeeds, the status will transition to CLOSED
                        //if the executing command fails, the status will transition to OPEN
                        Interlocked.CompareExchange(ref status, Status.HALF_OPEN, Status.OPEN);

                        if (status == Status.HALF_OPEN)
                            return true;
                        else
                            return false;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// Whether the circuit is currently open (tripped).
        /// </summary>
        /// <returns><c>true</c> if the circuit breaker is open, <c>false</c> otherwise.</returns>
        public bool IsOpen
        {
            get
            {
                if (properties.CircuitBreakerForceOpen)
                {
                    return true;
                }
                if (properties.CircuitBreakerForceClosed)
                {
                    return false;
                }
                return Interlocked.Read(ref circuitOpened) >= 0;
            }
        }

        /// <summary>
        /// Trips the circuit breaker.
        /// </summary>
        public void Trip()
        {
            // The circuit is closed and we've reached the error rate threshold, so trip the circuit breaker...
            Interlocked.Exchange(ref status, Status.OPEN);

            if (status == Status.OPEN)
            {
                //This thread wins the race to re-open the circuit - it resets the start time for the sleep window
                Interlocked.Exchange(ref circuitOpened, DateTime.UtcNow.Ticks);
            }
        }

        /// <summary>
        /// Invoked on successful executions from <see cref="HystrixCommand{TRequest, TResult}" /> as part of feedback mechanism when in a half-open state.
        /// </summary>
        public void MarkSuccess()
        {
            Interlocked.Exchange(ref status, Status.CLOSED);

            if (status == Status.CLOSED)
            {
                Interlocked.Exchange(ref circuitOpened, -1L);
            }
        }

        /// <summary>
        /// Invoked on unsuccessful executions from <see cref="HystrixCommand{TRequest, TResult}" /> as part of feedback mechanism when in a half-open state.
        /// </summary>
        public void MarkNonSuccess()
        {
            Interlocked.CompareExchange(ref status, Status.OPEN, Status.HALF_OPEN);

            if (status == Status.OPEN)
            {
                //This thread wins the race to re-open the circuit - it resets the start time for the sleep window
                Interlocked.Exchange(ref circuitOpened, DateTime.UtcNow.Ticks);
            }
        }

        /// <summary>
        /// Determines whether the current time is after sleep window.
        /// </summary>
        /// <returns><c>true</c> if the current time is after sleep window; otherwise, <c>false</c>.</returns>
        private bool IsAfterSleepWindow()
        {
            return Interlocked.Read(ref circuitOpened) + TimeSpan.FromMilliseconds(properties.CircuitBreakerSleepWindowInMilliseconds).Ticks < DateTime.UtcNow.Ticks;
        }
    }
}
