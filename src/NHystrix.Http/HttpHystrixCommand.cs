using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace NHystrix.Http
{
    internal class HttpHystrixCommand : HystrixCommand<HttpRequestMessage, HttpResponseMessage>
    {
        readonly Func<Task<HttpResponseMessage>> fallback;
        readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> runDelegate;

        public HttpHystrixCommand(HystrixCommandKey commandKey, HystrixCommandProperties properties, 
                                  Func<HttpRequestMessage, Task<HttpResponseMessage>> runDelegate, 
                                  Func<Task<HttpResponseMessage>> fallback = null) 
            : base(commandKey, properties)
        {
            this.runDelegate = runDelegate ?? throw new ArgumentNullException(nameof(runDelegate));
            this.fallback = fallback;

            IsTimeoutEnabled = properties.TimeoutEnabled;
            properties.TimeoutEnabled = false;
        }

        public bool IsTimeoutEnabled { get; }

        protected override Task<HttpResponseMessage> RunAsync(HttpRequestMessage request)
        {
            return runDelegate(request);
        }


        protected override Task<HttpResponseMessage> GetFallback()
        {
            if (fallback == null)
                return base.GetFallback();
            else
                return fallback();
        }
    }
}
