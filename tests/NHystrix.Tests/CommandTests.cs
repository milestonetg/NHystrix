using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NHystrix.Tests
{
    [TestClass]
    public class CommandTests
    {
        [TestMethod]
        public async Task NormalCommandShouldCompleteSuccessfully()
        {
            var properties = new HystrixCommandProperties()
            {
                FallbackEnabled = true,
                TimeoutEnabled = true,
                ExecutionTimeoutInMilliseconds = 5000
            };

            var cmd = new TestCommand(properties);

            HystrixCommandMetrics metrics = HystrixCommandMetrics.GetInstance(cmd.CommandKey, properties);

            string s = await cmd.ExecuteAsync().ConfigureAwait(false);

            long count = metrics.getRollingCount(HystrixEventType.SUCCESS);

            Assert.AreEqual(TestCommand.RETURN_VALUE, s);
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public async Task CommandRunningLongerThanTimeoutShouldTimeout()
        {
            var properties = new HystrixCommandProperties()
            {
                FallbackEnabled = true,
                TimeoutEnabled = true,
                ExecutionTimeoutInMilliseconds = 5000
            };

            var cmd = new LongRunningTestCommand(properties);

            HystrixCommandMetrics metrics = HystrixCommandMetrics.GetInstance(cmd.CommandKey, properties);

            string s = await cmd.ExecuteAsync().ConfigureAwait(false);
            
            long count = metrics.getRollingCount(HystrixEventType.TIMEOUT);

            Assert.AreEqual(TestCommand.FALLBACK_VALUE, s);
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public async Task CircuitBreakerShouldTripAfterFailureThreshold()
        {
            var properties = new HystrixCommandProperties()
            {
                FallbackEnabled = true,
                TimeoutEnabled = true,
                ExecutionTimeoutInMilliseconds = 20000,
                CircuitBreakerSleepWindowInMilliseconds = 60000,
                CircuitBreakerRequestVolumeThreshold = 100,
                CircuitBreakerErrorThresholdPercentage = 50,
            };

            int trippedAttempt = 0;
            bool tripped = false;

            //execute failures to trigger trip
            for(int i=0; i < 1000; i++)
            {
                // test command fails every other request.
                var cmd = new FailureCommand(i, properties);
                string s = await cmd.ExecuteAsync().ConfigureAwait(false);

                //Check the circuit breaker
                FieldInfo info = cmd.GetType().BaseType.GetField("circuitBreaker", BindingFlags.NonPublic | BindingFlags.Instance);
                HystrixCircuitBreaker circuitBreaker = info.GetValue(cmd) as HystrixCircuitBreaker;
                if (circuitBreaker.IsOpen)
                {
                    trippedAttempt = i;
                    tripped = true;
                    break;
                }
            }

            Assert.IsTrue(tripped, "Circuit breaker never tripped.");
            Assert.IsTrue(trippedAttempt >= 50 && trippedAttempt <= 100, $"Circuit breaker tripped, but not when expected. Tripped at requrest {trippedAttempt}");
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

    class FailureCommand : HystrixCommand<string>
    {
        public const string RETURN_VALUE = "Success attempt";
        public const string FALLBACK_VALUE = "Fallback";

        int attempt;

        public FailureCommand(int attempt, HystrixCommandProperties properties)
            : base(new HystrixCommandKey("FailureTest", new HystrixCommandGroup("CircuitBreakerTests")), properties)
        {
            this.attempt = attempt;
        }

        protected override Task<string> RunAsync()
        {
            if (attempt % 2 == 0)
                throw new Exception("Failed attempt");
            else
                return Task.FromResult(RETURN_VALUE);
        }

        protected override string OnHandleFallback()
        {
            return FALLBACK_VALUE;
        }
    }
}
