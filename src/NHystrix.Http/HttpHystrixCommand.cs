using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace NHystrix.Http
{
    internal class HttpHystrixCommand : HystrixCommand<HttpRequestMessage, HttpResponseMessage>
    {
        Func<HttpResponseMessage> fallback;
        
        public HttpHystrixCommand(HystrixCommandKey commandKey, HystrixCommandProperties properties, 
                                  Func<HttpRequestMessage, Task<HttpResponseMessage>> runDelegate, 
                                  Func<HttpResponseMessage> fallback = null) 
            : base(commandKey, properties, runDelegate)
        {
            this.fallback = fallback;

            IsTimeoutEnabled = properties.TimeoutEnabled;
            properties.TimeoutEnabled = false;
            properties.FallbackEnabled = true;
        }

        public bool IsTimeoutEnabled { get; }

        protected override Task<HttpResponseMessage> GetFallback()
        {
            if (fallback == null)
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.NoContent));
            else
                return Task.FromResult(fallback());
        }
    }
}
