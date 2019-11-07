using Microsoft.VisualStudio.TestTools.UnitTesting;
using NHystrix.Http;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NHystrix.Tests
{
    [TestClass]
    public class HystrixDelegatingHandlerTests
    {
        HystrixCommandGroup commandGroup = new HystrixCommandGroup("HystrixDelegatingHandlerTests");

        [TestMethod]
        public async Task Timeout_in_HttpClient_should_increment_timeout_metric()
        {
            HystrixCommandKey commandKey = new HystrixCommandKey("test_timeout", commandGroup);
            HystrixCommandProperties properties = new HystrixCommandProperties
            {
                CircuitBreakerOptions = new CircuitBreakerOptions
                {
                    CircuitBreakerRequestVolumeThreshold = 20,
                    CircuitBreakerErrorThresholdPercentage = 50
                },
                MetricsRollingStatisticalWindowInMilliseconds = 10000
            };

            TestingDelegatingHandler mock = new TestingDelegatingHandler(200);
            HystrixDelegatingHandler hystrix = new HystrixDelegatingHandler(commandKey, properties, mock);

            using (HttpClient client = new HttpClient(hystrix))
            {
                client.Timeout = TimeSpan.FromMilliseconds(1);

                HttpResponseMessage response = null;
                {
                    try
                    {
                        response = await client.GetAsync("http://test");
                    }
                    catch { }
                }
                var metrics = HystrixCommandMetrics.GetInstance(commandKey, properties);
                Assert.AreEqual(1, metrics.GetCumulativeCount(HystrixEventType.TIMEOUT));
            }
        }

        [TestMethod]
        public async Task Http200_should_not_trip_breaker()
        {
            HystrixCommandKey commandKey = new HystrixCommandKey("test200", commandGroup);
            HystrixCommandProperties properties = new HystrixCommandProperties
            {
                CircuitBreakerOptions = new CircuitBreakerOptions
                {
                    CircuitBreakerRequestVolumeThreshold = 20,
                    CircuitBreakerErrorThresholdPercentage = 50
                },
                MetricsRollingStatisticalWindowInMilliseconds = 10000
            };

            TestingDelegatingHandler mock = new TestingDelegatingHandler(200);
            HystrixDelegatingHandler hystrix = new HystrixDelegatingHandler(commandKey, properties);
            hystrix.InnerHandler = mock;

            var cb = HystrixCircuitBreaker.GetInstance(commandKey, properties.CircuitBreakerOptions, HystrixCommandMetrics.GetInstance(commandKey, properties));
            using (HttpClient client = new HttpClient(hystrix))
            {
                HttpResponseMessage response = null;
                for (int i = 0; i < 30; i++)
                    response = await client.GetAsync("http://test");

                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                Assert.IsFalse(cb.IsOpen, "CircuitBreaker was open at end of test but should have been closed.");
            }
        }

        [TestMethod]
        public async Task Http500_should_trip_breaker()
        {
            HystrixCommandKey commandKey = new HystrixCommandKey("test500", commandGroup);
            HystrixCommandProperties properties = new HystrixCommandProperties
            {
                CircuitBreakerOptions = new CircuitBreakerOptions
                {
                    CircuitBreakerRequestVolumeThreshold = 20,
                    CircuitBreakerErrorThresholdPercentage = 50
                },
                MetricsRollingStatisticalWindowInMilliseconds = 10000
                
            };

            TestingDelegatingHandler mock = new TestingDelegatingHandler(500);
            HystrixDelegatingHandler hystrix = new HystrixDelegatingHandler(commandKey, properties, mock);
            var cb = HystrixCircuitBreaker.GetInstance(commandKey, properties.CircuitBreakerOptions, HystrixCommandMetrics.GetInstance(commandKey, properties));

            using (HttpClient client = new HttpClient(hystrix))
            {
                HttpResponseMessage response = null;
                for (int i=0;i<30;i++)
                {
                    try
                    {
                        response = await client.GetAsync("http://test");
                    }
                    catch { }
                }

                Assert.IsTrue(cb.IsOpen, "CircuitBreaker was closed at end of test but should have been open.");
            }
        }

        [TestMethod]
        public async Task Http429_should_trip_breaker()
        {
            HystrixCommandKey commandKey = new HystrixCommandKey("test429", commandGroup);
            HystrixCommandProperties properties = new HystrixCommandProperties
            {
                CircuitBreakerOptions = new CircuitBreakerOptions
                {
                    CircuitBreakerRequestVolumeThreshold = 20,
                    CircuitBreakerErrorThresholdPercentage = 50
                },
                MetricsRollingStatisticalWindowInMilliseconds = 10000

            };

            TestingDelegatingHandler mock = new TestingDelegatingHandler(429);
            HystrixDelegatingHandler hystrix = new HystrixDelegatingHandler(commandKey, properties, mock);
            var cb = HystrixCircuitBreaker.GetInstance(commandKey, properties.CircuitBreakerOptions, HystrixCommandMetrics.GetInstance(commandKey, properties));

            using (HttpClient client = new HttpClient(hystrix))
            {
                HttpResponseMessage response = null;
                for (int i = 0; i < 30; i++)
                {
                    try
                    {
                        response = await client.GetAsync("http://test");
                    }
                    catch { }
                }

                Assert.IsTrue(cb.IsOpen, "CircuitBreaker was closed at end of test but should have been open.");
            }
        }

        [TestMethod]
        public async Task Http403_should_trip_breaker()
        {
            HystrixCommandKey commandKey = new HystrixCommandKey("test403", commandGroup);
            HystrixCommandProperties properties = new HystrixCommandProperties
            {
                CircuitBreakerOptions = new CircuitBreakerOptions
                {
                    CircuitBreakerRequestVolumeThreshold = 20,
                    CircuitBreakerErrorThresholdPercentage = 50
                },
                MetricsRollingStatisticalWindowInMilliseconds = 10000

            };

            TestingDelegatingHandler mock = new TestingDelegatingHandler(403);
            HystrixDelegatingHandler hystrix = new HystrixDelegatingHandler(commandKey, properties, mock);
            var cb = HystrixCircuitBreaker.GetInstance(commandKey, properties.CircuitBreakerOptions, HystrixCommandMetrics.GetInstance(commandKey, properties));

            using (HttpClient client = new HttpClient(hystrix))
            {
                HttpResponseMessage response = null;
                for (int i = 0; i < 30; i++)
                {
                    try
                    {
                        response = await client.GetAsync("http://test");
                    }
                    catch { }
                }
                Assert.IsTrue(cb.IsOpen, "CircuitBreaker was closed at end of test but should have been open.");
            }
        }

        [TestMethod]
        public async Task Http408_should_trip_breaker()
        {
            HystrixCommandKey commandKey = new HystrixCommandKey("test408", commandGroup);
            HystrixCommandProperties properties = new HystrixCommandProperties
            {
                CircuitBreakerOptions = new CircuitBreakerOptions
                {
                    CircuitBreakerRequestVolumeThreshold = 20,
                    CircuitBreakerErrorThresholdPercentage = 50
                },
                MetricsRollingStatisticalWindowInMilliseconds = 10000

            };

            TestingDelegatingHandler mock = new TestingDelegatingHandler(HttpStatusCode.RequestTimeout);
            HystrixDelegatingHandler hystrix = new HystrixDelegatingHandler(commandKey, properties, mock);
            var cb = HystrixCircuitBreaker.GetInstance(commandKey, properties.CircuitBreakerOptions, HystrixCommandMetrics.GetInstance(commandKey, properties));

            using (HttpClient client = new HttpClient(hystrix))
            {
                HttpResponseMessage response = null;
                for (int i = 0; i < 30; i++)
                {
                    try
                    {
                        response = await client.GetAsync("http://test");
                    }
                    catch { }
                }

                Assert.IsTrue(cb.IsOpen, "CircuitBreaker was closed at end of test but should have been open.");
            }
        }

        [TestMethod]
        public async Task Http504_should_trip_breaker()
        {
            HystrixCommandKey commandKey = new HystrixCommandKey("test504", commandGroup);
            HystrixCommandProperties properties = new HystrixCommandProperties
            {
                CircuitBreakerOptions = new CircuitBreakerOptions
                {
                    CircuitBreakerRequestVolumeThreshold = 20,
                    CircuitBreakerErrorThresholdPercentage = 50
                },
                MetricsRollingStatisticalWindowInMilliseconds = 10000

            };

            TestingDelegatingHandler mock = new TestingDelegatingHandler(HttpStatusCode.GatewayTimeout);
            HystrixDelegatingHandler hystrix = new HystrixDelegatingHandler(commandKey, properties, mock);
            var cb = HystrixCircuitBreaker.GetInstance(commandKey, properties.CircuitBreakerOptions, HystrixCommandMetrics.GetInstance(commandKey, properties));

            using (HttpClient client = new HttpClient(hystrix))
            {
                HttpResponseMessage response = null;
                for (int i = 0; i < 30; i++)
                {
                    try
                    {
                        response = await client.GetAsync("http://test");
                    }
                    catch { }
                }

                Assert.IsTrue(cb.IsOpen, "CircuitBreaker was closed at end of test but should have been open.");
            }
        }

        [TestMethod]
        public async Task Http502_should_trip_breaker()
        {
            HystrixCommandKey commandKey = new HystrixCommandKey("test502", commandGroup);
            HystrixCommandProperties properties = new HystrixCommandProperties
            {
                CircuitBreakerOptions = new CircuitBreakerOptions
                {
                    CircuitBreakerRequestVolumeThreshold = 20,
                    CircuitBreakerErrorThresholdPercentage = 50
                },
                MetricsRollingStatisticalWindowInMilliseconds = 10000

            };

            TestingDelegatingHandler mock = new TestingDelegatingHandler(HttpStatusCode.BadGateway);
            HystrixDelegatingHandler hystrix = new HystrixDelegatingHandler(commandKey, properties, mock);
            var cb = HystrixCircuitBreaker.GetInstance(commandKey, properties.CircuitBreakerOptions, HystrixCommandMetrics.GetInstance(commandKey, properties));

            using (HttpClient client = new HttpClient(hystrix))
            {
                HttpResponseMessage response = null;
                for (int i = 0; i < 30; i++)
                {
                    try
                    {
                        response = await client.GetAsync("http://test");
                    }
                    catch { }
                }

                Assert.IsTrue(cb.IsOpen, "CircuitBreaker was closed at end of test but should have been open.");
            }
        }

        [TestMethod]
        public async Task Http503_should_trip_breaker()
        {
            HystrixCommandKey commandKey = new HystrixCommandKey("test503", commandGroup);
            HystrixCommandProperties properties = new HystrixCommandProperties
            {
                CircuitBreakerOptions = new CircuitBreakerOptions
                {
                    CircuitBreakerRequestVolumeThreshold = 20,
                    CircuitBreakerErrorThresholdPercentage = 50
                },
                MetricsRollingStatisticalWindowInMilliseconds = 10000

            };

            TestingDelegatingHandler mock = new TestingDelegatingHandler(HttpStatusCode.ServiceUnavailable);
            HystrixDelegatingHandler hystrix = new HystrixDelegatingHandler(commandKey, properties, mock);
            var cb = HystrixCircuitBreaker.GetInstance(commandKey, properties.CircuitBreakerOptions, HystrixCommandMetrics.GetInstance(commandKey, properties));

            using (HttpClient client = new HttpClient(hystrix))
            {
                HttpResponseMessage response = null;
                for (int i = 0; i < 30; i++)
                {
                    try
                    {
                        response = await client.GetAsync("http://test");
                    }
                    catch { }
                }

                Assert.IsTrue(cb.IsOpen, "CircuitBreaker was closed at end of test but should have been open.");
            }
        }

        [TestMethod]
        public async Task Http400_should_not_trip_breaker()
        {
            HystrixCommandKey commandKey = new HystrixCommandKey("test400", commandGroup);
            HystrixCommandProperties properties = new HystrixCommandProperties
            {
                CircuitBreakerOptions = new CircuitBreakerOptions
                {
                    CircuitBreakerRequestVolumeThreshold = 20,
                    CircuitBreakerErrorThresholdPercentage = 50
                },
                MetricsRollingStatisticalWindowInMilliseconds = 10000

            };

            TestingDelegatingHandler mock = new TestingDelegatingHandler(HttpStatusCode.BadRequest);
            HystrixDelegatingHandler hystrix = new HystrixDelegatingHandler(commandKey, properties, mock);
            var cb = HystrixCircuitBreaker.GetInstance(commandKey, properties.CircuitBreakerOptions, HystrixCommandMetrics.GetInstance(commandKey, properties));

            using (HttpClient client = new HttpClient(hystrix))
            {
                HttpResponseMessage response = null;
                for (int i = 0; i < 30; i++)
                {
                    try
                    {
                        response = await client.GetAsync("http://test");
                    }
                    catch { }
                }

                Assert.IsFalse(cb.IsOpen, "CircuitBreaker was open at end of test but should have been closed.");
            }
        }

        [TestMethod]
        public async Task Exception_should_trip_breaker()
        {
            HystrixCommandKey commandKey = new HystrixCommandKey("test_exception", commandGroup);
            HystrixCommandProperties properties = new HystrixCommandProperties
            {
                CircuitBreakerOptions = new CircuitBreakerOptions
                {
                    CircuitBreakerRequestVolumeThreshold = 20,
                    CircuitBreakerErrorThresholdPercentage = 50
                },
                MetricsRollingStatisticalWindowInMilliseconds = 10000,
                FallbackEnabled = true
                
            };

            TestingDelegatingHandler mock = new TestingDelegatingHandler(new Exception("foobar"));
            HystrixDelegatingHandler hystrix = new HystrixDelegatingHandler(commandKey, properties, mock,
                fallbackDelegate: () => {
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
                });
            var cb = HystrixCircuitBreaker.GetInstance(commandKey, properties.CircuitBreakerOptions, HystrixCommandMetrics.GetInstance(commandKey, properties));

            using (HttpClient client = new HttpClient(hystrix))
            {
                HttpResponseMessage response = null;
                for (int i = 0; i < 30; i++)
                {
                    try
                    {
                        response = await client.GetAsync("http://test");
                    }
                    catch { }
                }

                Assert.IsTrue(cb.IsOpen, "CircuitBreaker was closed at end of test but should have been open.");
            }
        }
    }
}
