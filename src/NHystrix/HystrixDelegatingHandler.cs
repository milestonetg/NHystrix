using NHystrix.Exceptions;
using NHystrix.Metric;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace NHystrix
{
    /// <summary>
    /// A <see cref="DelegatingHandler"/> that wraps the request in the Circuit Breaker Pattern and Bulkhead Pattern.
    /// Similar functionality to the <see cref="HystrixCommand{TRequest, TResult}"/>, but timeouts are handled by <see cref="System.Net.Http.HttpClient"/> directly.
    /// </summary>
    /// <remarks>
    /// Should be used before the RetryDelegatingHandler in the pipeline.
    /// 
    /// ``` cs
    /// var hystrix = new HystrixDelegatingHandler(commandKey, properties, new RetryDelegatingHandler());
    /// ```
    /// 
    /// #### HTTP Status Codes
    /// 
    /// Failures are counted and emitted for:
    /// 
    /// Status | Reason            | HystrixEvent             | Comments
    /// -------|-------------------|--------------------------|----------
    /// 408    | Request Timeout   | HystrixEventType.TIMEOUT |
    /// 504    | Gateway Timeout   | HystrixEventType.TIMEOUT |
    /// 403    | Forbidden         | HystrixEventType.FAILURE | Some APIs, such as GitHub, return a 403 when a rate limit is reached
    /// 429    | Too Many Requests | HystrixEventType.FAILURE | Proposed rate-limit status code. [See RFC 6585](https://tools.ietf.org/html/rfc6585)
    /// >=500  | Server Errors     | HystrixEventType.FAILURE | All server side errors
    /// 
    /// Http 400 Bad Requests are not counted against failures but do emit a `HystrixEventType.BAD_REQUEST`.
    /// 
    /// All other Http Status codes are ignored by NHystrix.
    /// 
    /// #### Exceptions and Fallback
    /// 
    /// Like the <see cref="HystrixCommand{TRequest, TResult}"/>, the <see cref="HystrixDelegatingHandler"/> does not
    /// throw an exception on error nor does is allow exceptions to propagate up the stack. Rather, it relies
    /// on the fallback implementation to handle error situations.
    /// 
    /// By default, when fallback is enabled, an HttpResponseMessage with a status code of [204 No Content] 
    /// will be returned and <see cref="HystrixEventType.FALLBACK_MISSING"/> emitted if no fallback function is provided.
    /// 
    /// #### Metrics
    /// 
    /// The handler emits:
    /// - Request Counts (<see cref="HystrixEventType.EMIT"/>) for all requests, short-circuited or not.
    /// - Bad Request (<see cref="HystrixEventType.BAD_REQUEST"/>) for HTTP 400.
    /// - Timeout (<see cref="HystrixEventType.TIMEOUT"/>)
    /// - Failure (<see cref="HystrixEventType.FAILURE"/>)
    /// - Exception Thrown (<see cref="HystrixEventType.EXCEPTION_THROWN"/>)
    /// - Fallback Called (<see cref="HystrixEventType.FALLBACK_EMIT"/>)
    /// - Fallback Success (<see cref="HystrixEventType.FALLBACK_SUCCESS"/>)
    /// - Fallback Failure (<see cref="HystrixEventType.FALLBACK_FAILURE"/>)
    /// - Fallback Disabled (<see cref="HystrixEventType.FALLBACK_DISABLED"/>)
    /// - Fallback Missing (<see cref="HystrixEventType.FALLBACK_MISSING"/>)
    /// - Short Circuited because the circuit breaker is open (<see cref="HystrixEventType.SHORT_CIRCUITED"/>)
    /// - Semaphore rejected if a bulkhead semaphore could not be obtained before the <see cref="HystrixCommandProperties.BulkheadSemaphoreAcquireTimeoutInMilliseconds"/> expires. (<see cref="HystrixEventType.SHORT_CIRCUITED"/>)
    /// </remarks>
    /// <seealso cref="System.Net.Http.DelegatingHandler" />
    public class HystrixDelegatingHandler : DelegatingHandler
    {
        HystrixCommandProperties properties;
        IHystrixCircuitBreaker circuitBreaker;
        HystrixBulkhead bulkhead;
        HystrixCommandEventStream commandEventStream;
        Func<HttpResponseMessage> fallback;

        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixDelegatingHandler"/> class.
        /// </summary>
        /// <param name="commandKey">The command key.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="innerHandler">The inner handler.</param>
        /// <param name="fallback">
        /// Optional. The function to execute as the fallback if the circuit-breaker is open.
        /// If no fallback function is provided, an HttpResponseMessage with a status code 
        /// of [204 No Content] will be returned.
        /// </param>
        /// <exception cref="ArgumentNullException">properties</exception>
        public HystrixDelegatingHandler(HystrixCommandKey commandKey, HystrixCommandProperties properties, HttpMessageHandler innerHandler, Func<HttpResponseMessage> fallback = null)
        {
            if (commandKey.Equals(default(HystrixCommandKey)))
                throw new ArgumentException("A HystrixCommandKey is required.", nameof(commandKey));

            this.properties = properties ?? throw new ArgumentNullException(nameof(properties));
            InnerHandler = innerHandler ?? throw new ArgumentNullException(nameof(innerHandler));
            this.fallback = fallback;

            CommandKey = commandKey;
            commandEventStream = HystrixCommandEventStream.GetInstance(CommandKey);

            if (properties.CircuitBreakerEnabled)
                circuitBreaker = HystrixCircuitBreaker.GetInstance(commandKey, properties.CircuitBreakerOptions, HystrixCommandMetrics.GetInstance(commandKey, properties));

            if (properties.BulkheadingEnabled)
                bulkhead = HystrixBulkhead.GetInstance(commandKey, properties);
        }


        /// <summary>
        /// Gets the command key.
        /// </summary>
        /// <value>The command key.</value>
        public HystrixCommandKey CommandKey { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is circuit breaker enabled.
        /// </summary>
        /// <value><c>true</c> if this instance is circuit breaker enabled; otherwise, <c>false</c>.</value>
        public bool IsCircuitBreakerEnabled => properties.CircuitBreakerEnabled;

        /// <summary>
        /// Gets a value indicating whether this instance is circuit breaker open.
        /// </summary>
        /// <value><c>true</c> if this instance is circuit breaker open; otherwise, <c>false</c>.</value>
        public bool IsCircuitBreakerOpen => circuitBreaker?.IsOpen ?? false;

        /// <summary>
        /// Send an HTTP request message.
        /// </summary>
        /// <param name="request">The HTTP request message to send to the server.</param>
        /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            commandEventStream.Write(new HystrixCommandEvent(CommandKey, HystrixEventType.EMIT));

            if (properties.CircuitBreakerEnabled 
                && !circuitBreaker.ShouldAllowRequest
                && !circuitBreaker.ShouldAttemptExecution)
            {
                commandEventStream.Write(new HystrixCommandEvent(CommandKey, HystrixEventType.SHORT_CIRCUITED));
                return HandleFallback(HystrixEventType.SHORT_CIRCUITED);
            }

            if (properties.BulkheadingEnabled && !bulkhead.Try())
            {
                commandEventStream.Write(new HystrixCommandEvent(CommandKey, HystrixEventType.SEMAPHORE_REJECTED));
                return HandleFallback(HystrixEventType.SEMAPHORE_REJECTED);
            }

            try
            {
                HttpResponseMessage responseMessage = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

                if (!responseMessage.IsSuccessStatusCode)
                {
                    switch(responseMessage.StatusCode)
                    {
                        case HttpStatusCode.BadRequest:
                            commandEventStream.Write(new HystrixCommandEvent(CommandKey, HystrixEventType.BAD_REQUEST));
                            break;

                        case HttpStatusCode.RequestTimeout:
                        case HttpStatusCode.GatewayTimeout:
                            commandEventStream.Write(new HystrixCommandEvent(CommandKey, HystrixEventType.TIMEOUT));
                            circuitBreaker?.MarkNonSuccess();
                            break;

                        case HttpStatusCode.Forbidden: // some apis return 403 when a rate limit is reached.
                        case (HttpStatusCode)429: // rate-limited/throttled. see https://tools.ietf.org/html/rfc6585
                            commandEventStream.Write(new HystrixCommandEvent(CommandKey, HystrixEventType.FAILURE));
                            circuitBreaker?.MarkNonSuccess();
                            break;

                        default:
                            if (responseMessage.StatusCode >= HttpStatusCode.InternalServerError)
                            {
                                commandEventStream.Write(new HystrixCommandEvent(CommandKey, HystrixEventType.FAILURE));
                                circuitBreaker?.MarkNonSuccess();
                            }
                            break;
                    }
                }

                return responseMessage;
            }
            catch (HystrixRuntimeException)
            {
                throw;
            }
            catch (TaskCanceledException ex)
            {
                // HttpClient cancels the underlying task to handle timeouts.
                // We'll assume that, under default circumstances, that this exception is the result of a timeout
                // and not a user cancellation. https://github.com/dotnet/corefx/issues/20296

                commandEventStream.Write(new HystrixCommandEvent(CommandKey, HystrixEventType.TIMEOUT, ex));
                circuitBreaker?.MarkNonSuccess();
                return HandleFallback(HystrixEventType.TIMEOUT, ex);
            }
            catch (Exception ex)
            {
                commandEventStream.Write(new HystrixCommandEvent(CommandKey, HystrixEventType.FAILURE, ex));
                commandEventStream.Write(new HystrixCommandEvent(CommandKey, HystrixEventType.EXCEPTION_THROWN, ex));
                circuitBreaker?.MarkNonSuccess();
                return HandleFallback(HystrixEventType.FAILURE, ex);
            }
            finally
            {
                if (properties.BulkheadingEnabled)
                    bulkhead.Release();
            }
        }

        private HttpResponseMessage HandleFallback(HystrixEventType eventType, Exception originalException = null)
        {
            if (properties.FallbackEnabled)
            {
                if (fallback != null)
                {
                    commandEventStream.Write(new HystrixCommandEvent(CommandKey, HystrixEventType.FALLBACK_EMIT));

                    try
                    {
                        HttpResponseMessage fallbackResponse = fallback();
                        commandEventStream.Write(new HystrixCommandEvent(CommandKey, HystrixEventType.FALLBACK_SUCCESS));
                        return fallbackResponse;
                    }
                    catch (Exception ex)
                    {
                        commandEventStream.Write(new HystrixCommandEvent(CommandKey, HystrixEventType.FALLBACK_FAILURE, ex));
                        throw new HystrixFallbackException(FailureType.COMMAND_EXCEPTION, ex.Message, ex, originalException);
                    }
                }
                else
                {
                    commandEventStream.Write(new HystrixCommandEvent(CommandKey, HystrixEventType.FALLBACK_MISSING));
                }
            }
            else
            {
                commandEventStream.Write(new HystrixCommandEvent(CommandKey, HystrixEventType.FALLBACK_DISABLED));
            }

            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="T:System.Net.Http.DelegatingHandler"></see>, and optionally disposes of the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to releases only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                bulkhead?.Dispose();
            }

            circuitBreaker = null;
            commandEventStream = null;
            properties = null;
            bulkhead = null;
            fallback = null;
        }
    }
}
