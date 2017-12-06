using Microsoft.VisualStudio.TestTools.UnitTesting;
using NHystrix.Metric;
using System;
using System.Collections.Generic;
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

            HystrixCommandEventStream foo = HystrixCommandEventStream.GetInstance(commandKey);

            HystrixCommandEvent commandEvent = new HystrixCommandEvent(commandKey, HystrixEventType.EMIT);

            foo.Observe().Subscribe(onNext => {
                Assert.AreEqual(commandEvent, onNext);
            });

            foo.Write(commandEvent);
        }
    }
}
