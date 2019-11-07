using NHystrix.Exceptions;
using NHystrix.Metric;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace NHystrix.Http
{
    /// <summary>
    /// A <see cref="DelegatingHandler"/> that wraps the request in the Circuit Breaker Pattern and Bulkhead Pattern.
    /// Timeouts are handled by <see cref="System.Net.Http.HttpClient"/> directly, rather than the underlying
    /// <see cref="HystrixCommand{TRequest, TResult}"/>.
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
        Func<Task<HttpResponseMessage>> fallbackDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixDelegatingHandler"/> class.
        /// </summary>
        /// <param name="commandKey">The command key.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="fallbackDelegate">
        /// Optional. The function to execute as the fallback if the circuit-breaker is open.
        /// If no fallback function is provided, an HttpResponseMessage with a status code 
        /// of [204 No Content] will be returned.
        /// </param>
        /// <exception cref="ArgumentNullException">properties</exception>
        public HystrixDelegatingHandler(HystrixCommandKey commandKey, 
                                        HystrixCommandProperties properties, 
                                        Func<Task<HttpResponseMessage>> fallbackDelegate = null)
        {
            if (commandKey.Equals(default(HystrixCommandKey)))
                throw new ArgumentException("A HystrixCommandKey is required.", nameof(commandKey));

            this.properties = properties ?? throw new ArgumentNullException(nameof(properties));
            
            this.fallbackDelegate = fallbackDelegate;

            CommandKey = commandKey;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixDelegatingHandler"/> class.
        /// </summary>
        /// <param name="commandKey">The command key.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="innerHandler">The inner handler.</param>
        /// <param name="fallbackDelegate">
        /// Optional. The function to execute as the fallback if the circuit-breaker is open.
        /// If no fallback function is provided, an HttpResponseMessage with a status code 
        /// of [204 No Content] will be returned.
        /// </param>
        /// <exception cref="ArgumentNullException">properties</exception>
        public HystrixDelegatingHandler(HystrixCommandKey commandKey, 
                                        HystrixCommandProperties properties, 
                                        HttpMessageHandler innerHandler, 
                                        Func<Task<HttpResponseMessage>> fallbackDelegate = null)
            : this(commandKey, properties, fallbackDelegate)
        {
            InnerHandler = innerHandler ?? throw new ArgumentNullException(nameof(innerHandler));
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
        /// Send an HTTP request message.
        /// </summary>
        /// <param name="request">The HTTP request message to send to the server.</param>
        /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            using (var command = new HttpHystrixCommand(CommandKey, properties,
                async (r) =>
                {
                    HystrixCommandEventStream commandEventStream = HystrixCommandEventStream.GetInstance(CommandKey);
                    
                    try
                    {
                        HttpResponseMessage responseMessage = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

                        
                        if (!responseMessage.IsSuccessStatusCode)
                        {
                            switch (responseMessage.StatusCode)
                            {
                                case HttpStatusCode.BadRequest:
                                    commandEventStream.Write(new HystrixCommandEvent(CommandKey, HystrixEventType.BAD_REQUEST));
                                    break;
                                case HttpStatusCode.RequestTimeout:
                                case HttpStatusCode.GatewayTimeout:
                                    commandEventStream.Write(new HystrixCommandEvent(CommandKey, HystrixEventType.TIMEOUT));
                                    break;
                                case HttpStatusCode.Forbidden: // some apis return 403 when a rate limit is reached.
                                case (HttpStatusCode)429: // rate-limited/throttled. see https://tools.ietf.org/html/rfc6585
                                    commandEventStream.Write(new HystrixCommandEvent(CommandKey, HystrixEventType.FAILURE));
                                    break;
                                default:
                                    if (responseMessage.StatusCode >= HttpStatusCode.InternalServerError)
                                    {
                                        commandEventStream.Write(new HystrixCommandEvent(CommandKey, HystrixEventType.FAILURE));
                                        break;
                                    }
                                    break;
                            }
                        }

                        return responseMessage;
                    }
                    catch (TaskCanceledException ex)
                    {
                        commandEventStream.Write(new HystrixCommandEvent(CommandKey, HystrixEventType.TIMEOUT));
                        throw new HystrixTimeoutException($"CancelationToken cancelled calling {request.RequestUri}", ex);
                    }
                }, fallbackDelegate))
            {
                return await command.ExecuteAsync(request).ConfigureAwait(false);
            }
        }
    }
}
