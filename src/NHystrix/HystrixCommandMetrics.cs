using System;
using System.Collections.Generic;
using System.Text;
using NHystrix.Metric;
using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace NHystrix
{
    public class HystrixCommandMetrics : HystrixMetrics
    {
        static ConcurrentDictionary<HystrixCommandKey, HystrixCommandMetrics> intern = new ConcurrentDictionary<HystrixCommandKey, HystrixCommandMetrics>();

        HystrixCommandEventStream commandEventStream;

        ISubject<HealthCounts> healthStream;

        private HystrixCommandMetrics(HystrixRollingNumber counter, HystrixCommandKey commandKey, HystrixCommandProperties properties) : base(counter)
        {
            CommandKey = commandKey;
            CommandGroup = commandKey.Group;

            healthStream = new Subject<HealthCounts>();
            commandEventStream = HystrixCommandEventStream.GetInstance(commandKey);
            commandEventStream.Observe().Subscribe(
                onNext =>
                {
                    counter.Add(onNext.EventType, 1);

                    HealthCounts health = new HealthCounts
                    {
                        RequestCount = counter.GetRollingSum(HystrixEventType.EMIT),
                        FailedRequestCount =
                            counter.GetRollingSum(HystrixEventType.FAILURE)
                                + counter.GetRollingSum(HystrixEventType.TIMEOUT)
                    };

                    healthStream.OnNext(health);
                });
        }

        public static HystrixCommandMetrics GetInstance(HystrixCommandKey commandKey, HystrixCommandProperties properties)
        {
            HystrixCommandMetrics metrics = null;
            if (intern.TryGetValue(commandKey, out metrics))
            {
                //the key existed, so we'll return the existing instance
                return metrics;
            }
            else
            {
                //we need to add a new one...
                metrics = 
                    new HystrixCommandMetrics(
                        new HystrixRollingNumber(properties.MetricsRollingStatisticalWindowInMilliseconds,
                                                 properties.MetricsRollingStatisticalWindowBuckets),
                        commandKey, properties);

                //try to add the key
                if (intern.TryAdd(commandKey, metrics))
                {
                    return metrics;
                }
                else
                {
                    //another thread beat us to it, so we'll get that instance instead.
                    intern.TryGetValue(commandKey, out metrics);
                    return metrics;
                }
            }
        }

        public HystrixCommandKey CommandKey { get; private set; }

        public HystrixCommandGroup CommandGroup { get; private set; }

        public IObservable<HealthCounts> HealthStream { get => healthStream; }
    }
}
