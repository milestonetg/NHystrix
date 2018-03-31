using System;
using System.Threading.Tasks;
using System.Threading;
using NHystrix.Metric;
using NHystrix.Exceptions;

namespace NHystrix
{
    /// <summary>
    /// Base class for all HystrixCommands. Providing functionality for:
    /// - Bulkhead Pattern
    /// - Timeout
    /// - Circuit Breaker Pattern
    /// - Fallback responses
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <remarks>
    /// #### Fallback
    /// 
    /// You can support graceful degradation in a Hystrix command by adding a fallback method that Hystrix will call 
    /// to obtain a default value or values in case the main command fails.  You will want to implement a fallback 
    /// for most Hystrix commands that might conceivably fail, with a couple of exceptions:
    ///
    /// 1. a command that performs a write operation
    ///    * If your Hystrix command is designed to do a write operation rather than to return a value, there isn't 
    ///    much point in implementing a fallback. If the write fails, you probably want the failure to propagate back 
    ///    to the caller.
    /// 2. batch systems/offline compute
    ///    * If your Hystrix command is filling up a cache, or generating a report, or doing any sort of offline 
    ///    computation, it's usually more appropriate to propagate the error back to the caller who can then retry the 
    ///    command later, rather than to send the caller a silently-degraded response.
    ///
    /// Whether or not your command has a fallback, all of the usual Hystrix state and circuit-breaker state/metrics are 
    /// updated to indicate the command failure.
    ///
    /// In an ordinary `HystrixCommand` you implement a fallback by means of a 
    /// <see cref="HystrixCommand{TRequest, TResult}.GetFallback"/>() implementation. Hystrix will execute this fallback 
    /// for all types of failure such as `RunAsync()` failure, timeout, thread pool or semaphore rejection, and circuit
    /// breaker short-circuiting.
    /// 
    /// #### Error Propagation
    ///
    /// All exceptions thrown from the `RunAsync()` method except for <see cref="HystrixBadRequestException"/> and 
    /// <see cref="ArgumentException"/> count as failures and trigger `GetFallback()` and circuit-breaker logic.
    ///
    /// You can wrap the exception that you would like to throw in `HystrixBadRequestException`. The 
    /// `HystrixBadRequestException` is intended for use cases such as reporting illegal arguments or non-system 
    /// failures that should not count against the failure metrics and should not trigger fallback logic.
    /// </remarks>
    /// <seealso cref="NHystrix.IHystrixCommand{TRequest, TResult}" />
    public abstract class HystrixCommand<TRequest, TResult> : IHystrixCommand<TRequest, TResult>, IDisposable
    {
        IHystrixCircuitBreaker circuitBreaker;
        HystrixBulkhead bulkhead;
        HystrixCommandProperties properties;
        HystrixCommandEventStream commandEventStream;

        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixCommand{TRequest, TResult}"/> class.
        /// </summary>
        /// <param name="commandKey">The command key.</param>
        /// <param name="properties">The properties.</param>
        protected HystrixCommand(HystrixCommandKey commandKey, HystrixCommandProperties properties)
        {
            CommandKey = commandKey;
            commandEventStream = HystrixCommandEventStream.GetInstance(CommandKey);

            this.properties = properties ?? throw new ArgumentNullException(nameof(properties));

            if (properties.CircuitBreakerEnabled)
                circuitBreaker = HystrixCircuitBreaker.GetInstance(
                    commandKey, properties.CircuitBreakerOptions, HystrixCommandMetrics.GetInstance(commandKey, properties));

            if (properties.BulkheadingEnabled)
                bulkhead = HystrixBulkhead.GetInstance(commandKey, properties);
        }

        /// <summary>
        /// Gets the command key.
        /// </summary>
        /// <value>The command key.</value>
        public HystrixCommandKey CommandKey { get; private set; }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <returns>TResult.</returns>
        /// <exception cref="HystrixBadRequestException"></exception>
        /// <exception cref="HystrixFailureException"></exception>
        /// <exception cref="HystrixTimeoutException"></exception>
        public TResult Execute(TRequest request = default(TRequest))
        {
            return Task.Run(() => ExecuteAsync(request))
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <returns>Task&lt;TResult&gt;.</returns>
        /// <exception cref="HystrixBadRequestException"></exception>
        /// <exception cref="HystrixFailureException"></exception>
        /// <exception cref="HystrixTimeoutException"></exception>
        public Task<TResult> ExecuteAsync(TRequest request = default(TRequest))
        {
            commandEventStream.Write(new HystrixCommandEvent(CommandKey, HystrixEventType.EMIT));

            if (properties.CircuitBreakerEnabled && !circuitBreaker.ShouldAllowRequest)
            {
                commandEventStream.Write(new HystrixCommandEvent(CommandKey, HystrixEventType.SHORT_CIRCUITED));
                return HandleFallback(HystrixEventType.SHORT_CIRCUITED);
            }

            if (properties.CircuitBreakerEnabled && !circuitBreaker.ShouldAttemptExecution)
            {
                commandEventStream.Write(new HystrixCommandEvent(CommandKey, HystrixEventType.SHORT_CIRCUITED));
                return HandleFallback(HystrixEventType.SHORT_CIRCUITED);
            }

            if (properties.BulkheadingEnabled && !bulkhead.Try())
            {
                commandEventStream.Write(new HystrixCommandEvent(CommandKey, HystrixEventType.SEMAPHORE_REJECTED));
                return HandleFallback(HystrixEventType.SEMAPHORE_REJECTED);
            }

            bool isBulkheadSemaphoreReleased = false;

            // Releases the semaphore if it hasn't been already.
            void releaseBulkHeadSemaphore()
            {
                if (properties.BulkheadingEnabled && !isBulkheadSemaphoreReleased)
                {
                    isBulkheadSemaphoreReleased = true;

                    try
                    {
                        bulkhead.Release();
                    }
                    catch
                    {
                        // Don't care. An exception just means we've released more than we should have.
                        // It would mean there is a bug in that we are releasing too many times in some scenarios,
                        // but we don't want that to abend or prevent execution because of it.
                    }
                }
            }

            try
            {
                int timeout = 
                    properties.TimeoutEnabled 
                    ? properties.ExecutionTimeoutInMilliseconds 
                    : int.MaxValue;
                               
                return RunAsync(request)
                    .WithTimeout(timeout)
                    .ContinueWith((t) => 
                    {
                        releaseBulkHeadSemaphore();

                        HystrixEventType eventType = null;
                        TResult result = default(TResult);

                        //Timeouts generally result as a faulted task.
                        if (t.IsFaulted)
                        {
                            circuitBreaker.MarkNonSuccess();

                            Exception ex = t.Exception?.GetBaseException();

                            if (ex is TimeoutException)
                                eventType = HystrixEventType.TIMEOUT;
                            else
                                eventType = HystrixEventType.FAILURE;

                            commandEventStream.Write(new HystrixCommandEvent(CommandKey, eventType, ex));
                                
                            if (properties.FallbackEnabled)
                                result = HandleFallback(eventType, ex).ConfigureAwait(false).GetAwaiter().GetResult();
                        }
                        else
                        {
                            result = t.Result;
                            commandEventStream.Write(new HystrixCommandEvent(CommandKey, HystrixEventType.SUCCESS));
                            circuitBreaker.MarkSuccess();
                        }

                        if (properties.FallbackEnabled || !t.IsFaulted)
                            return result;
                        else
                        {
                            string msg = t.Exception?.Message ?? "Command faulted, but no Exception was provided by the runtime.";
                            if (eventType == HystrixEventType.TIMEOUT)
                                throw new HystrixTimeoutException(msg, t.Exception);
                            else
                                throw new HystrixFailureException(FailureType.COMMAND_EXCEPTION, msg, t.Exception);
                        }
                    });
            }
            // Exceptions thrown directly from the implementation of RunAsyc() are caught by the catch blocks here
            catch(HystrixRuntimeException)
            {
                // If a RuntimeException has already been thrown, then it has already been emitted.
                // Just bubble it up.

                // try and release just in case the user explicitly threw a HystrixRuntimeException from RunAsync()
                releaseBulkHeadSemaphore(); 
                throw;
            }
            catch(HystrixBadRequestException ex)
            {
                // Bad Request Exceptions don't count against failures.
                releaseBulkHeadSemaphore();
                commandEventStream.Write(new HystrixCommandEvent(CommandKey, HystrixEventType.BAD_REQUEST, ex));
                throw;
            }
            catch(ArgumentException ex)
            {
                releaseBulkHeadSemaphore();
                commandEventStream.Write(new HystrixCommandEvent(CommandKey, HystrixEventType.BAD_REQUEST, ex));

                // Any ArgumentException is a BadRequestException for Hystrix.
                throw new HystrixBadRequestException(ex.Message, ex);
            }
            catch (Exception ex)
            {
                releaseBulkHeadSemaphore();
                circuitBreaker.MarkNonSuccess();

                HystrixEventType eventType = null;
                if (ex is TimeoutException)
                    eventType = HystrixEventType.TIMEOUT;
                else
                    eventType = HystrixEventType.FAILURE;

                commandEventStream.Write(new HystrixCommandEvent(CommandKey, eventType, ex));

                if (properties.FallbackEnabled)
                    return HandleFallback(eventType, ex);
                else
                {
                    if (eventType == HystrixEventType.TIMEOUT)
                        throw new HystrixTimeoutException(ex.Message, ex);
                    else
                        throw new HystrixFailureException(FailureType.COMMAND_EXCEPTION, ex.Message, ex);
                }
            }
        }

        /// <summary>
        /// Queues the command for async.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="callback">Optional callback action.</param>
        /// <example>
        /// <code>
        /// var cmd = new HelloCommand(properties);
        /// cmd.Queue(string.Empty, t => {
        ///     //Callback logic.
        /// });
        /// </code>
        /// </example>
        public void Queue(TRequest request = default(TRequest), Action<Task<TResult>> callback = null)
        {
            HystrixWorker<TRequest, TResult> worker = HystrixWorker<TRequest, TResult>.GetInstance(CommandKey, properties);

            worker?.Enqueue(this, callback);
        }

        /// <summary>
        /// Implements the logic to perform when <see cref="ExecuteAsync"/> is called.
        /// </summary>
        /// <returns>Task&lt;TResult&gt;.</returns>
        protected abstract Task<TResult> RunAsync(TRequest request);

        /// <summary>
        /// When overridden in a derived class, returns the fallback response when fallback is enabled.
        /// </summary>
        /// <returns>The fallback response for the command.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        protected virtual Task<TResult> GetFallback()
        {
            commandEventStream.Write(new HystrixCommandEvent(CommandKey, HystrixEventType.FALLBACK_MISSING));

            throw new NotImplementedException($"Fallback is enabled, but no fallback provided for command {CommandKey}");
        }

        private async Task<TResult> HandleFallback(HystrixEventType eventType, Exception exception = null)
        {
            if (properties.FallbackEnabled)
            {
                commandEventStream.Write(new HystrixCommandEvent(CommandKey, HystrixEventType.FALLBACK_EMIT));

                try
                {
                    TResult result = await GetFallback();
                    commandEventStream.Write(new HystrixCommandEvent(CommandKey, HystrixEventType.FALLBACK_SUCCESS));
                    return result;
                }
                catch (Exception ex)
                {
                    commandEventStream.Write(new HystrixCommandEvent(CommandKey, HystrixEventType.FALLBACK_FAILURE, ex));
                    throw new HystrixFallbackException(
                        FailureType.COMMAND_EXCEPTION, exception?.Message ?? "Fallback failure", ex, exception);
                }
            }
            else
            {
                commandEventStream.Write(new HystrixCommandEvent(CommandKey, HystrixEventType.FALLBACK_DISABLED));

                if (exception is TimeoutException)
                    throw new HystrixTimeoutException(exception?.Message, exception);
                else
                {
                    FailureType failureType;
                    if (eventType == HystrixEventType.SHORT_CIRCUITED)
                        failureType = FailureType.SHORTCIRCUIT;
                    else if (eventType == HystrixEventType.SEMAPHORE_REJECTED)
                        failureType = FailureType.REJECTED_SEMAPHORE_EXECUTION;
                    else
                        failureType = FailureType.COMMAND_EXCEPTION;

                    throw new HystrixFailureException(failureType, exception?.Message ?? "Fallback disabled", exception);
                }
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">
        /// <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    bulkhead.Dispose();
                }

                circuitBreaker = null;
                commandEventStream = null;
                properties = null;
                bulkhead = null;

                disposedValue = true;
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="HystrixCommand{TRequest, TResult}"/> class.
        /// </summary>
        ~HystrixCommand()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
