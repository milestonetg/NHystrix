using Microsoft.Extensions.DependencyInjection;
using NHystrix.Http;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace NHystrix.Extensions.Http
{
    public static class IHttpClientBuilderExtensions
    {
        public static IHttpClientBuilder AddNHystrixHandler(this IHttpClientBuilder builder, HystrixCommandGroup group, string commandName, HystrixCommandProperties properties, Func<Task<HttpResponseMessage>> fallbackDelegate = null)
        {
            if (string.IsNullOrWhiteSpace(commandName))
                throw new ArgumentException("A command name must be provided.", nameof(commandName));

            return builder.AddNHystrixHandler(new HystrixCommandKey(commandName, group), properties, fallbackDelegate);
        }

        public static IHttpClientBuilder AddNHystrixHandler(this IHttpClientBuilder builder, HystrixCommandKey commandKey, HystrixCommandProperties properties, Func<Task<HttpResponseMessage>> fallbackDelegate = null)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (properties == null )
            {
                throw new ArgumentNullException(nameof(properties));
            }
                                    
            builder.AddHttpMessageHandler(() => new HystrixDelegatingHandler(commandKey, properties, fallbackDelegate));

            return builder;
        }
    }
}
