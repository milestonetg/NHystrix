﻿using System;
using System.Threading.Tasks;
using NHystrix;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var properties = new HystrixCommandProperties() { FallbackEnabled = true, TimeoutEnabled = true, ExecutionTimeoutInMilliseconds = 5000 };
            var cmd = new TestCommand(properties);

            HystrixCommandMetrics metrics = HystrixCommandMetrics.GetInstance(cmd.CommandKey, properties);

            string s = cmd.ExecuteAsync().ConfigureAwait(false).GetAwaiter().GetResult();


            long count = metrics.getRollingCount(HystrixEventType.TIMEOUT);

            Console.WriteLine("{0} : {1}", s, count);
            Console.ReadLine();
        }
    }

    class TestCommand : HystrixCommand<string>
    {
        public TestCommand(HystrixCommandProperties properties)
            : base(new HystrixCommandKey("Test", new HystrixCommandGroup("TestGroup")), properties)
        {

        }

        protected override async Task<string> RunAsync()
        {
            await Task.Delay(10000);
            return "Hello World";
        }

        protected override string OnHandleFallback()
        {
            return "Fallback";
        }
    }
}