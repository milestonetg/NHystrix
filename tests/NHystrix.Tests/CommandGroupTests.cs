using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace NHystrix.Tests
{
    [TestClass]
    public class CommandGroupTests
    {
        [TestMethod]
        public void AddCommandKey_should_add_mulitple_keys()
        {
            var group = new HystrixCommandGroup("service");
            group.AddCommandKey("get");
            group.AddCommandKey("add");

            Assert.AreEqual(2, group.CommandKeys.Count);
        }

        [TestMethod]
        public void AddCommandKey_should_not_allow_duplicate_keys()
        {
            var group = new HystrixCommandGroup("service");
            group.AddCommandKey("get");
            group.AddCommandKey("add");
            group.AddCommandKey("get");

            Assert.AreEqual(2, group.CommandKeys.Count);
        }

    }
}
