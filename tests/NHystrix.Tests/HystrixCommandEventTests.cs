using Microsoft.VisualStudio.TestTools.UnitTesting;
using NHystrix.Metric;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace NHystrix.Tests
{
    [TestClass]
    public class HystrixCommandEventTests
    {
        [TestMethod]
        public void PublishedEventShouldBeReceivedBySubscribers()
        {
            var group = new HystrixCommandGroup("TestGroup");
            var commandKey = new HystrixCommandKey("Test", group);

            HystrixCommandEventStream commandEventStream = HystrixCommandEventStream.GetInstance(commandKey);

            HystrixCommandEvent commandEvent = new HystrixCommandEvent(commandKey, HystrixEventType.EMIT);

            commandEventStream.Observe().Subscribe(onNext => {
                Assert.AreEqual(commandEvent, onNext);
            });

            commandEventStream.Write(commandEvent);
        }
    }
}
