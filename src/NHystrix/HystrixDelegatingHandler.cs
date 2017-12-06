using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NHystrix
{
    public class HystrixDelegatingHandler : DelegatingHandler
    {
        HystrixCommandProperties properties;

        public HystrixDelegatingHandler(HystrixCommandProperties properties, HttpMessageHandler innerHandler)
        {
            this.properties = properties;
            InnerHandler = innerHandler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return base.SendAsync(request, cancellationToken);
        }
    }
}
