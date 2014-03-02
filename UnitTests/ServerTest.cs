using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    /// <summary>
    /// Unit tests for the Server class. Note that Initialize and Cleanup is called for each test method.
    /// </summary>
    [TestClass]
    public class ServerTest
    {
        [TestInitialize]
        public void Initialize()
        {
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestMethod1()
        {
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestMethod2()
        {
            Assert.IsTrue(true);
        }

        [TestCleanup]
        public void Cleanup()
        {
            Assert.IsTrue(true);
        }
    }
}
