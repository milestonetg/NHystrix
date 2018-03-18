using Microsoft.VisualStudio.TestTools.UnitTesting;
using NHystrix.Exceptions;
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
                ExecutionTimeoutInMilliseconds = 5000,
                
            };

            var cmd = new TestCommand(properties);

            HystrixCommandMetrics metrics = HystrixCommandMetrics.GetInstance(cmd.CommandKey, properties);

            string s = await cmd.ExecuteAsync().ConfigureAwait(false);

            long count = metrics.GetCumulativeCount(HystrixEventType.SUCCESS);

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
            
            long count = metrics.GetCumulativeCount(HystrixEventType.TIMEOUT);

            Assert.AreEqual(TestCommand.FALLBACK_VALUE, s);
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        [ExpectedException(typeof(HystrixTimeoutException), AllowDerivedTypes = true)]
        public async Task Timeout_with_no_fallback_should_throw_TimeoutException()
        {
            var properties = new HystrixCommandProperties()
            {
                FallbackEnabled = false,
                TimeoutEnabled = true,
                ExecutionTimeoutInMilliseconds = 5000
            };

            var cmd = new LongRunningTestCommand(properties);

            HystrixCommandMetrics metrics = HystrixCommandMetrics.GetInstance(cmd.CommandKey, properties);

            string s = await cmd.ExecuteAsync().ConfigureAwait(false);
        }

        [TestMethod]
        [ExpectedException(typeof(HystrixFailureException), AllowDerivedTypes = true)]
        public async Task Failure_with_no_fallback_should_throw_FailureException()
        {
            var properties = new HystrixCommandProperties()
            {
                FallbackEnabled = false,
                TimeoutEnabled = true,
                ExecutionTimeoutInMilliseconds = 5000
            };

            var cmd = new FailureCommand(1, properties);

            HystrixCommandMetrics metrics = HystrixCommandMetrics.GetInstance(cmd.CommandKey, properties);

            string s = await cmd.ExecuteAsync().ConfigureAwait(false);
        }

        [TestMethod]
        [ExpectedException(typeof(HystrixBadRequestException), AllowDerivedTypes = true)]
        public async Task BadRequest_with_no_fallback_should_throw_BadRequestException()
        {
            var properties = new HystrixCommandProperties()
            {
                FallbackEnabled = false,
                TimeoutEnabled = true,
                ExecutionTimeoutInMilliseconds = 5000
            };

            var cmd = new BadRequestCommand(1, properties);

            HystrixCommandMetrics metrics = HystrixCommandMetrics.GetInstance(cmd.CommandKey, properties);

            string s = await cmd.ExecuteAsync().ConfigureAwait(false);
        }

        [TestMethod]
        [ExpectedException(typeof(HystrixBadRequestException), AllowDerivedTypes = true)]
        public async Task BadRequest_with_fallback_should_throw_BadRequestException()
        {
            var properties = new HystrixCommandProperties()
            {
                FallbackEnabled = true,
                TimeoutEnabled = true,
                ExecutionTimeoutInMilliseconds = 5000
            };

            var cmd = new BadRequestCommand(1, properties);

            HystrixCommandMetrics metrics = HystrixCommandMetrics.GetInstance(cmd.CommandKey, properties);

            string s = await cmd.ExecuteAsync().ConfigureAwait(false);
        }

        [TestMethod]
        public async Task CircuitBreakerShouldTripAfterFailureThreshold()
        {
            var properties = new HystrixCommandProperties()
            {
                FallbackEnabled = true,
                TimeoutEnabled = true,
                ExecutionTimeoutInMilliseconds = 20000,
                CircuitBreakerOptions = new CircuitBreakerOptions
                {
                    CircuitBreakerSleepWindowInMilliseconds = 60000,
                    CircuitBreakerRequestVolumeThreshold = 100,
                    CircuitBreakerErrorThresholdPercentage = 50,
                }
            };

            long trippedAttempt = 0;
            bool tripped = false;

            var cmd = new FailureCommand(1, properties);
            HystrixCommandMetrics metrics = HystrixCommandMetrics.GetInstance(cmd.CommandKey, properties);
            //metrics.Reset();

            //execute failures to trigger trip
            for (int i=0; i < 1000; i++)
            {
                // test command fails every other request.
                cmd = new FailureCommand(i, properties);
                string s = await cmd.ExecuteAsync().ConfigureAwait(false);

                //Check the circuit breaker
                FieldInfo info = cmd.GetType().BaseType.GetField("circuitBreaker", BindingFlags.NonPublic | BindingFlags.Instance);
                HystrixCircuitBreaker circuitBreaker = info.GetValue(cmd) as HystrixCircuitBreaker;
                if (circuitBreaker.IsOpen)
                {
                    tripped = true;
                    break;
                }
            }

            trippedAttempt = metrics.GetCumulativeCount(HystrixEventType.EMIT);

            Assert.IsTrue(tripped, "Circuit breaker never tripped.");
            //Assert.IsTrue(trippedAttempt >= 100, $"Circuit breaker tripped, but not when expected. Tripped at request {trippedAttempt}");
        }

        [TestMethod]
        public async Task Failure_should_return_fallback_when_enabled()
        {
            var properties = new HystrixCommandProperties()
            {
                FallbackEnabled = true,
                TimeoutEnabled = true,
                ExecutionTimeoutInMilliseconds = 20000,
                CircuitBreakerOptions = new CircuitBreakerOptions
                {
                    CircuitBreakerSleepWindowInMilliseconds = 60000,
                    CircuitBreakerRequestVolumeThreshold = 100,
                    CircuitBreakerErrorThresholdPercentage = 50,
                }
            };

            var cmd = new FailureCommand(1, properties);
            string s = await cmd.ExecuteAsync(null).ConfigureAwait(false);

            Assert.AreEqual(FailureCommand.FALLBACK_VALUE, s);
        }
    }

    class TestCommand : HystrixCommand<string,string>
    {
        public const string RETURN_VALUE = "Hello World";
        public const string FALLBACK_VALUE = "Fallback";

        public TestCommand(HystrixCommandProperties properties)
            : base(new HystrixCommandKey("Test", new HystrixCommandGroup("TestGroup")), properties)
        {

        }

        protected override Task<string> RunAsync(string s)
        {
            return Task.FromResult(RETURN_VALUE);
        }

        protected override Task<string> GetFallback()
        {
            return Task.FromResult(FALLBACK_VALUE);
        }
    }

    class LongRunningTestCommand : TestCommand
    {
        public LongRunningTestCommand(HystrixCommandProperties properties) : base(properties)
        {
        }

        protected override async Task<string> RunAsync(string s)
        {
            await Task.Delay(30000);
            return await base.RunAsync(s);
        }
    }

    class FailureCommand : HystrixCommand<string, string>
    {
        public const string RETURN_VALUE = "Success attempt";
        public const string FALLBACK_VALUE = "Fallback";

        int attempt;

        public FailureCommand(int attempt, HystrixCommandProperties properties)
            : base(new HystrixCommandKey("FailureTest", new HystrixCommandGroup("CircuitBreakerTests")), properties)
        {
            this.attempt = attempt;
        }

        protected override Task<string> RunAsync(string s)
        {
            //if (attempt % 2 == 0)
                throw new Exception("Failed attempt");
            //else
                //return Task.FromResult(RETURN_VALUE);
        }

        protected override Task<string> GetFallback()
        {
            return Task.FromResult(FALLBACK_VALUE);
        }
    }

    class BadRequestCommand : HystrixCommand<string, string>
    {
        public const string RETURN_VALUE = "Success attempt";
        public const string FALLBACK_VALUE = "Fallback";

        int attempt;

        public BadRequestCommand(int attempt, HystrixCommandProperties properties)
            : base(new HystrixCommandKey("FailureTest", new HystrixCommandGroup("CircuitBreakerTests")), properties)
        {
            this.attempt = attempt;
        }

        protected override Task<string> RunAsync(string s)
        {
            //if (attempt % 2 == 0)
            throw new ArgumentException("Bad Request Test");
            //else
            //return Task.FromResult(RETURN_VALUE);
        }

        protected override Task<string> GetFallback()
        {
            return Task.FromResult(FALLBACK_VALUE);
        }
    }
}
