using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NHystrix.Tests
{
    [TestClass]
    public class CommandTests
    {
        [TestMethod]
        public void NormalCommandShouldCompleteSuccessfully()
        {
            var properties = new HystrixCommandProperties()
            {
                FallbackEnabled = true,
                TimeoutEnabled = true,
                ExecutionTimeoutInMilliseconds = 5000
            };

            var cmd = new TestCommand(properties);

            HystrixCommandMetrics metrics = HystrixCommandMetrics.GetInstance(cmd.CommandKey, properties);

            string s = cmd.ExecuteAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            long count = metrics.getRollingCount(HystrixEventType.SUCCESS);

            Assert.AreEqual(TestCommand.RETURN_VALUE, s);
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public void CommandRunningLongerThanTimeoutShouldTimeout()
        {
            var properties = new HystrixCommandProperties()
            {
                FallbackEnabled = true,
                TimeoutEnabled = true,
                ExecutionTimeoutInMilliseconds = 5000
            };

            var cmd = new LongRunningTestCommand(properties);

            HystrixCommandMetrics metrics = HystrixCommandMetrics.GetInstance(cmd.CommandKey, properties);

            string s = cmd.ExecuteAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            
            long count = metrics.getRollingCount(HystrixEventType.TIMEOUT);

            Assert.AreEqual(TestCommand.FALLBACK_VALUE, s);
            Assert.AreEqual(1, count);
        }
    }

    class TestCommand : HystrixCommand<string>
    {
        public const string RETURN_VALUE = "Hello World";
        public const string FALLBACK_VALUE = "Fallback";

        public TestCommand(HystrixCommandProperties properties)
            : base(new HystrixCommandKey("Test", new HystrixCommandGroup("TestGroup")), properties)
        {

        }

        protected override Task<string> RunAsync()
        {
            return Task.FromResult(RETURN_VALUE);
        }

        protected override string OnHandleFallback()
        {
            return FALLBACK_VALUE;
        }
    }

    class LongRunningTestCommand : TestCommand
    {
        public LongRunningTestCommand(HystrixCommandProperties properties) : base(properties)
        {
        }

        protected override async Task<string> RunAsync()
        {
            await Task.Delay(30000);
            return await base.RunAsync();
        }
    }
}
