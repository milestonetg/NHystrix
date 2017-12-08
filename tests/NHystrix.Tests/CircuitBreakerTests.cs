using Microsoft.VisualStudio.TestTools.UnitTesting;
using NHystrix;

namespace NHystrix.Tests
{
    [TestClass]
    public class CircuitBreakerTests
    {
        [TestMethod]
        public void FailurePercentageShouldReturnWholeNumber()
        {
            HealthCounts healthCounts = new HealthCounts
            {
                RequestCount = 100,
                FailedRequestCount = 10
            };

            Assert.AreEqual(10, healthCounts.FailurePercentage);
        }

        [TestMethod]
        public void MarkNonSuccess_should_open_circuitbreaker()
        {
            //var group = new HystrixCommandGroup("TestGroup");
            //var command = new HystrixCommandKey("Test", group);

            //IHystrixCircuitBreaker cb = HystrixCircuitBreaker.GetInstance(command, new HystrixCommandProperties(), null);

            //cb.MarkNonSuccess();

            //Assert.IsTrue(cb.IsOpen);
        }

        [TestMethod]
        public void MarkSuccess_should_close_circuitbreaker()
        {
            //var group = new HystrixCommandGroup("TestGroup");
            //var command = new HystrixCommandKey("Test", group);

            //IHystrixCircuitBreaker cb = HystrixCircuitBreaker.GetInstance(command, new HystrixCommandProperties(), null);

            //cb.MarkNonSuccess();

            //Assert.IsTrue(cb.IsOpen);

            //cb.MarkSuccess();

            //Assert.IsFalse(cb.IsOpen);
        }

        [TestMethod]
        public void AllowRequest_should_return_false_when_OPEN()
        {
            //var group = new HystrixCommandGroup("TestGroup");
            //var command = new HystrixCommandKey("Test", group);

            //IHystrixCircuitBreaker cb = HystrixCircuitBreaker.GetInstance(command, new HystrixCommandProperties() { CircuitBreakerSleepWindowInMilliseconds = 600000 }, null);

            //cb.MarkNonSuccess();

            //Assert.IsFalse(cb.ShouldAllowRequest);
        }

        [TestMethod]
        public void AttemptExecution_should_return_false_when_OPEN()
        {
            //var group = new HystrixCommandGroup("TestGroup");
            //var command = new HystrixCommandKey("Test", group);

            //IHystrixCircuitBreaker cb = HystrixCircuitBreaker.GetInstance(command, new HystrixCommandProperties() { CircuitBreakerSleepWindowInMilliseconds = 600000 }, null);

            //cb.MarkNonSuccess();

            //Assert.IsFalse(cb.ShouldAttemptExecution);
        }
    }
}
