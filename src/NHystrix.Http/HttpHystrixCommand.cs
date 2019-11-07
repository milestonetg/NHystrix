using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace NHystrix.Http
{
    internal class HttpHystrixCommand : HystrixCommand<HttpRequestMessage, HttpResponseMessage>
    {
        Func<Task<HttpResponseMessage>> fallback;
        
        public HttpHystrixCommand(HystrixCommandKey commandKey, HystrixCommandProperties properties, 
                                  Func<HttpRequestMessage, Task<HttpResponseMessage>> runDelegate, 
                                  Func<Task<HttpResponseMessage>> fallback = null) 
            : base(commandKey, properties, runDelegate)
        {
            this.fallback = fallback;

            IsTimeoutEnabled = properties.TimeoutEnabled;
            properties.TimeoutEnabled = false;
            //properties.FallbackEnabled = true;
        }

        public bool IsTimeoutEnabled { get; }

        protected override Task<HttpResponseMessage> GetFallback()
        {
            if (fallback == null)
                return base.GetFallback();
            else
                return fallback();
        }
    }
}
