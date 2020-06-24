// <copyright file="EdgeDriver.cs" company="Microsoft">
// Copyright 2020 Microsoft
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.Collections.Generic;
using Microsoft.Edge.SeleniumTools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;

namespace Edge.Test
{
    [TestClass]
    public class EdgeDriverTests
    {
        void UsesLegacy(EdgeDriver driver)
        {
            Assert.AreEqual("MicrosoftEdge", driver.Capabilities.GetCapability("browserName"), "Driver launches Edge Legacy.");
        }

        void UsesChromium(EdgeDriver driver)
        {
            Assert.AreEqual("msedge", driver.Capabilities.GetCapability("browserName"), "Driver launches Edge Chromium.");

            var result = driver.ExecuteChromiumCommandWithResult("Browser.getVersion", new Dictionary<string, object>());
            Assert.IsTrue((result as Dictionary<string, object>).ContainsKey("userAgent"), "Driver can send Chromium-specific commands.");
        }

        [TestMethod]
        [Ignore] // Edge Legacy is not available on Azure hosted environment.
        public void TestDefault()
        {
            var driver = new EdgeDriver();
            try
            {
                UsesLegacy(driver);
            }
            finally
            {
                driver.Quit();
            }
        }

        [TestMethod]
        [Ignore] // Edge Legacy is not available on Azure hosted environment.
        public void TestLegacyOptions()
        {
            var driver = new EdgeDriver(new EdgeOptions() { UseChromium = false });
            try
            {
                UsesLegacy(driver);
            }
            finally
            {
                driver.Quit();
            }
        }

        [TestMethod]
        public void TestChromiumOptions()
        {
            var driver = new EdgeDriver(new EdgeOptions() { UseChromium = true });
            try
            {
                UsesChromium(driver);
            }
            finally
            {
                driver.Quit();
            }
        }

        [TestMethod]
        public void TestChromiumServiceWithLegacyOptions()
        {
            using (var service = EdgeDriverService.CreateChromiumService())
            {
                try
                {
                    var driver = new EdgeDriver(service, new EdgeOptions());
                    Assert.Fail();
                }
                catch (WebDriverException e)
                {
                    Assert.AreEqual("options.UseChromium must be set to true when using an Edge Chromium driver service.", e.Message);
                }
            }
        }

        [TestMethod]
        public void TestChromiumServiceWithChromiumOptions()
        {
            using (var service = EdgeDriverService.CreateChromiumService())
            {
                var driver = new EdgeDriver(service, new EdgeOptions() { UseChromium = true });
                try
                {
                    UsesChromium(driver);
                }
                finally
                {
                    driver.Quit();
                }
            }
        }

        [TestMethod]
        [Ignore] // Edge Legacy is not available on Azure hosted environment.
        public void TestLegacyServiceWithLegacyOptions()
        {
            using (var service = EdgeDriverService.CreateDefaultService())
            {
                var driver = new EdgeDriver(service, new EdgeOptions() { UseChromium = false });
                try
                {
                    UsesLegacy(driver);
                }
                finally
                {
                    driver.Quit();
                }
            }
        }

        [TestMethod]
        [Ignore] // Edge Legacy is not available on Azure hosted environment.
        public void TestLegacyServiceWithChromiumOptions()
        {
            using (var service = EdgeDriverService.CreateDefaultService())
            {
                try
                {
                    var driver = new EdgeDriver(service, new EdgeOptions() { UseChromium = true });
                    Assert.Fail();
                }
                catch (WebDriverException e)
                {
                    Assert.AreEqual("options.UseChromium must be set to false when using an Edge Legacy driver service.", e.Message);
                }
            }
        }

        [TestMethod]
        public void TestChromiumOptionsToCapabilities()
        {
            var options = new EdgeOptions()
            {
                UseChromium = true,
                PageLoadStrategy = PageLoadStrategy.Eager, // Common
                UseInPrivateBrowsing = true, // Legacy only
                DebuggerAddress = "localhost:9222" // Chromium only
            };

            var capabilities = options.ToCapabilities();

            Assert.AreEqual("MicrosoftEdge", capabilities.GetCapability("browserName"));
            Assert.AreEqual(true, capabilities.GetCapability("ms:edgeChromium"));
            Assert.AreEqual("eager", capabilities.GetCapability("pageLoadStrategy"));
            Assert.IsFalse(capabilities.HasCapability("ms:inPrivate"));

            var edgeOptionsDictionary = capabilities.GetCapability("ms:edgeOptions") as Dictionary<string, object>;
            Assert.IsNotNull(edgeOptionsDictionary);
            Assert.AreEqual(1, edgeOptionsDictionary.Count);
            Assert.AreEqual("localhost:9222", edgeOptionsDictionary["debuggerAddress"]);
        }

        [TestMethod]
        public void TestLegacyOptionsToCapabilities()
        {
            var options = new EdgeOptions()
            {
                UseChromium = false,
                PageLoadStrategy = PageLoadStrategy.Eager, // Common
                UseInPrivateBrowsing = true, // Legacy only
                DebuggerAddress = "localhost:9222" // Chromium only
            };

            var capabilities = options.ToCapabilities();

            Assert.AreEqual("MicrosoftEdge", capabilities.GetCapability("browserName"));
            Assert.AreEqual(false, capabilities.GetCapability("ms:edgeChromium"));
            Assert.AreEqual("eager", capabilities.GetCapability("pageLoadStrategy"));
            Assert.AreEqual(true, capabilities.GetCapability("ms:inPrivate"));
            Assert.IsFalse(capabilities.HasCapability("ms:edgeOptions"));
        }

        [TestMethod]
        public void TestWebViewOptions()
        {
            var options = new EdgeOptions();
            options.UseWebView = true;
            Assert.AreEqual("webview2", options.BrowserName);
        }
    }
}
