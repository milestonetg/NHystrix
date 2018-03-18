using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NHystrix.Tests
{
    public class TestingDelegatingHandler : DelegatingHandler
    {
        HttpStatusCode statusCode;

        Exception exception;

        public TestingDelegatingHandler(Exception exception)
        {
            this.exception = exception;
        }

        public TestingDelegatingHandler(int statusCode)
        {
            this.statusCode = (HttpStatusCode)statusCode;
        }

        public TestingDelegatingHandler(HttpStatusCode statusCode)
        {
            this.statusCode = statusCode;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (exception != null)
                throw exception;

            await Task.Delay(50);

            if (cancellationToken.IsCancellationRequested)
                throw new TaskCanceledException();

            return new HttpResponseMessage(statusCode);
        }
    }
}
