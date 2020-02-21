using System;
using Microsoft.SeleniumTools.Edge;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Edge.Test
{
    [TestClass]
    public class EdgeDriverTests
    {
        [TestMethod]
        public void TestChromiumOptions()
        {
            var driver = new EdgeDriver(new EdgeOptions() { UseChromium = true });
            driver.Url = "https://bing.com";
            Assert.IsTrue(driver.Title.Contains("Bing"), "Edge Chromium navigates successfully.");
            driver.Quit();
        }

        [TestMethod]
        public void TestChromiumServiceWithLegacyOptions()
        {

        }

        [TestMethod]
        public void TestChromiumServiceWithChromiumOptions()
        {

        }

        [TestMethod]
        public void TestLegacyOptions()
        {
        }

        [TestMethod]
        public void TestLegacyServiceWithLegacyOptions()
        {

        }

        [TestMethod]
        public void TestLegacyServiceWithChromiumOptions()
        {

        }
    }
}
