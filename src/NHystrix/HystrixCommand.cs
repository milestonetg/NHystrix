using System;
using System.Threading.Tasks;
using System.Threading;
using NHystrix.Metric;

namespace NHystrix
{
    /// <summary>
    /// Base class for all HystrixCommands. Providing functionality for:
    /// - Bulkheading
    /// - Timeout
    /// - Circuit Breaker
    /// - Request Caching
    /// - Request Logging
    /// - Fallback responses
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <seealso cref="NHystrix.IHystrixCommand{TResult}" />
    public abstract class HystrixCommand<TResult> : IHystrixCommand<TResult>
    {
        IHystrixCircuitBreaker circuitBreaker;
        HystrixCommandProperties properties;
        HystrixCommandEventStream commandEventStream;

        Semaphore bulkhead;

        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixCommand{TResult}"/> class.
        /// </summary>
        /// <param name="commandKey">The command key.</param>
        /// <param name="properties">The properties.</param>
        protected HystrixCommand(HystrixCommandKey commandKey, HystrixCommandProperties properties)
        {
            CommandKey = commandKey;
            commandEventStream = HystrixCommandEventStream.GetInstance(CommandKey);

            this.properties = properties ?? throw new ArgumentNullException(nameof(properties));

            if (properties.CircuitBreakerEnabled)
                circuitBreaker = HystrixCircuitBreaker.GetInstance(commandKey, properties, HystrixCommandMetrics.GetInstance(commandKey, properties));

            if (properties.BulkheadingEnabled)
                bulkhead = new Semaphore(properties.MaxConcurrentRequests, properties.MaxConcurrentRequests);
        }

        /// <summary>
        /// Gets the command key.
        /// </summary>
        /// <value>The command key.</value>
        public HystrixCommandKey CommandKey { get; private set; }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <returns>TResult.</returns>
        public TResult Execute()
        {
            return Task.Run(() => ExecuteAsync())
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <returns>Task&lt;TResult&gt;.</returns>
        public Task<TResult> ExecuteAsync()
        {
            commandEventStream.Write(new HystrixCommandEvent(CommandKey, HystrixEventType.EMIT));

            if (!circuitBreaker.ShouldAllowRequest)
            {
                commandEventStream.Write(new HystrixCommandEvent(CommandKey, HystrixEventType.SHORT_CIRCUITED));
                return Task.FromResult(HandleFallback());
            }

            if (!circuitBreaker.ShouldAttemptExecution)
            {
                commandEventStream.Write(new HystrixCommandEvent(CommandKey, HystrixEventType.SHORT_CIRCUITED));
                return Task.FromResult(HandleFallback());
            }

            if (properties.BulkheadingEnabled && !bulkhead.WaitOne(properties.BulkheadSemaphoreAcquireTimeoutInMilliseconds))
            {
                commandEventStream.Write(new HystrixCommandEvent(CommandKey, HystrixEventType.SEMAPHORE_REJECTED));
                return Task.FromResult(HandleFallback());
            }

            try
            {
                int timeout = 
                    properties.TimeoutEnabled 
                    ? properties.ExecutionTimeoutInMilliseconds 
                    : int.MaxValue;

                return RunAsync()
                    .WithTimeout(timeout)
                    .ContinueWith((t) => 
                        {
                            TResult result = default(TResult);
                            if (t.IsFaulted)
                            {
                                Exception ex = t.Exception?.GetBaseException(); 
                                if (ex is TimeoutException)
                                    commandEventStream.Write(new HystrixCommandEvent(CommandKey, HystrixEventType.TIMEOUT, ex));
                                else
                                    commandEventStream.Write(new HystrixCommandEvent(CommandKey, HystrixEventType.FAILURE, ex));

                                circuitBreaker.MarkNonSuccess();

                                result = HandleFallback();
                            }
                            else
                            {
                                result = t.Result;
                                commandEventStream.Write(new HystrixCommandEvent(CommandKey, HystrixEventType.SUCCESS));
                                circuitBreaker.MarkSuccess();
                            }
                            bulkhead?.Release();
                            return result;
                        });
            }
            catch(Exception ex)
            {
                bulkhead?.Release();
                commandEventStream.Write(new HystrixCommandEvent(CommandKey, HystrixEventType.FAILURE, ex));
                circuitBreaker.MarkNonSuccess();
                return Task.FromResult(HandleFallback());
            }
        }

        /// <summary>
        /// Implements the logic to perform when <see cref="ExecuteAsync"/> is called.
        /// </summary>
        /// <returns>Task&lt;TResult&gt;.</returns>
        protected abstract Task<TResult> RunAsync();

        /// <summary>
        /// When overridden in a derived class, returns the fallback response when fallback is enabled.
        /// </summary>
        /// <returns>The fallback response for the command.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        protected virtual TResult OnHandleFallback()
        {
            commandEventStream.Write(new HystrixCommandEvent(CommandKey, HystrixEventType.FALLBACK_MISSING));
            throw new NotImplementedException($"Fallback is enabled, but no fallback provided for command {CommandKey}");
        }

        private TResult HandleFallback()
        {
            if (properties.FallbackEnabled)
            {
                commandEventStream.Write(new HystrixCommandEvent(CommandKey, HystrixEventType.FALLBACK_EMIT));

                try
                {
                    TResult result = OnHandleFallback();
                    commandEventStream.Write(new HystrixCommandEvent(CommandKey, HystrixEventType.FALLBACK_SUCCESS));
                    return result;
                }
                catch (Exception ex)
                {
                    commandEventStream.Write(new HystrixCommandEvent(CommandKey, HystrixEventType.FALLBACK_FAILURE, ex));
                    throw;
                }
            }
            else
            {
                commandEventStream.Write(new HystrixCommandEvent(CommandKey, HystrixEventType.FALLBACK_DISABLED));
                return default(TResult);
            }
        }
    }
}
