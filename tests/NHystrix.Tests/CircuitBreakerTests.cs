using Microsoft.VisualStudio.TestTools.UnitTesting;
using NHystrix;
using NHystrix.Metric;
using System;
using System.Threading;

namespace NHystrix.Tests
{
    [TestClass]
    public class CircuitBreakerTests
    {
        [TestMethod]
        public void CircuitBreaker_should_trip_when_request_volume_and_failure_percentage_is_met()
        {
            var group = new HystrixCommandGroup("TestGroup");
            var command = new HystrixCommandKey("Test_failure_rate", group);

            var properties = new CircuitBreakerOptions
            {
                CircuitBreakerErrorThresholdPercentage = 50,
                CircuitBreakerRequestVolumeThreshold = 20
            };

            var metricsProperties = new HystrixCommandProperties
            {
                MetricsRollingStatisticalWindowBuckets = 10,
                MetricsRollingStatisticalWindowInMilliseconds = 60000
            };

            var commandEventStream = HystrixCommandEventStream.GetInstance(command);
            var metrics = HystrixCommandMetrics.GetInstance(command, metricsProperties);
            
            IHystrixCircuitBreaker cb = HystrixCircuitBreaker.GetInstance(command, properties, metrics);

            int iteration = 0;

            //We need to emit more than 20 requests, but should never reach more than 20
            for (int i = 0; i < 40; i++)
            {
                commandEventStream.Write(new HystrixCommandEvent(command, HystrixEventType.EMIT));
                commandEventStream.Write(new HystrixCommandEvent(command, HystrixEventType.FAILURE));

                //Let the observer threads do stuff
                Thread.Sleep(0);

                if (cb.IsOpen)
                {
                    iteration = i + 1;
                    break;
                }
            }
            
            // The circuit breaker should have tripped
            Assert.IsTrue(cb.IsOpen, "The circuit-breaker did not trip.");

            // It should have happened on the 20th iteration with the configured parameters.
            Assert.AreEqual(20, iteration, "The circuit breaker didn't trip at the configured failure threshold.");
        }

        [TestMethod]
        public void FailurePercentageShouldReturnWholeNumber()
        {
            HealthCounts healthCounts = new HealthCounts
            {
                RequestCount = 100,
                FailedRequestCount = 33
            };

            Assert.AreEqual(33, healthCounts.FailurePercentage);
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
