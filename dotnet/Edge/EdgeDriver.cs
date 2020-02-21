// <copyright file="EdgeDriver.cs" company="WebDriver Committers">
// Licensed to the Software Freedom Conservancy (SFC) under one
// or more contributor license agreements. See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership. The SFC licenses this file
// to you under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using OpenQA.Selenium.Remote;
using System.Collections.Generic;
using OpenQA.Selenium;

namespace Microsoft.SeleniumTools.Edge
{
    /// <summary>
    /// Provides a mechanism to write tests against Edge Legacy (EdgeHTML) and Edge Chromium
    /// </summary>
    /// <example>
    /// <code>
    /// [TestFixture]
    /// public class Testing
    /// {
    ///     private IWebDriver driver;
    ///     <para></para>
    ///     [SetUp]
    ///     public void SetUp()
    ///     {
    ///         driver = new EdgeDriver();
    ///     }
    ///     <para></para>
    ///     [Test]
    ///     public void TestBing()
    ///     {
    ///         driver.Navigate().GoToUrl("https://www.bing.com");
    ///         /*
    ///         *   Rest of the test
    ///         */
    ///     }
    ///     <para></para>
    ///     [TearDown]
    ///     public void TearDown()
    ///     {
    ///         driver.Quit();
    ///     }
    /// }
    /// </code>
    /// </example>
    public class EdgeDriver : RemoteWebDriver
    {
        /// <summary>
        /// Accept untrusted SSL Certificates
        /// </summary>
        public static readonly bool AcceptUntrustedCertificates = true;

        private const string GetNetworkConditionsCommand = "getNetworkConditions";
        private const string SetNetworkConditionsCommand = "setNetworkConditions";
        private const string DeleteNetworkConditionsCommand = "deleteNetworkConditions";
        private const string SendChromiumCommand = "sendChromeCommand";
        private const string SendChromiumCommandWithResult = "sendChromeCommandWithResult";

        /// <summary>
        /// Initializes a new instance of the <see cref="EdgeDriver"/> class.
        /// </summary>
        public EdgeDriver()
                : this(new EdgeOptions())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdgeDriver"/> class using the specified options.
        /// </summary>
        /// <param name="options">The <see cref="EdgeOptions"/> to be used with the Edge driver.</param>
        public EdgeDriver(EdgeOptions options)
            : this(EdgeDriverService.CreateDefaultServiceFromOptions(options), options, RemoteWebDriver.DefaultCommandTimeout)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdgeDriver"/> class using the specified driver service.
        /// </summary>
        /// <param name="service">The <see cref="EdgeDriverService"/> used to initialize the driver.</param>
        public EdgeDriver(EdgeDriverService service)
            : this(service, new EdgeOptions())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdgeDriver"/> class using the specified path
        /// to the directory containing the WebDriver executable.
        /// </summary>
        /// <param name="edgeDriverDirectory">The full path to the directory containing the WebDriver executable.</param>
        public EdgeDriver(string edgeDriverDirectory)
            : this(edgeDriverDirectory, new EdgeOptions())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdgeDriver"/> class using the specified path
        /// to the directory containing the WebDriver executable and options.
        /// </summary>
        /// <param name="edgeDriverDirectory">The full path to the directory containing the WebDriver executable.</param>
        /// <param name="options">The <see cref="EdgeOptions"/> to be used with the Edge driver.</param>
        public EdgeDriver(string edgeDriverDirectory, EdgeOptions options)
            : this(edgeDriverDirectory, options, RemoteWebDriver.DefaultCommandTimeout)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdgeDriver"/> class using the specified path
        /// to the directory containing the WebDriver executable, options, and command timeout.
        /// </summary>
        /// <param name="edgeDriverDirectory">The full path to the directory containing the WebDriver executable.</param>
        /// <param name="options">The <see cref="EdgeOptions"/> to be used with the Edge driver.</param>
        /// <param name="commandTimeout">The maximum amount of time to wait for each command.</param>
        public EdgeDriver(string edgeDriverDirectory, EdgeOptions options, TimeSpan commandTimeout)
            : this(EdgeDriverService.CreateDefaultServiceFromOptions(edgeDriverDirectory, options), options, commandTimeout)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdgeDriver"/> class using the specified
        /// <see cref="EdgeDriverService"/> and options.
        /// </summary>
        /// <param name="service">The <see cref="EdgeDriverService"/> to use.</param>
        /// <param name="options">The <see cref="EdgeOptions"/> used to initialize the driver.</param>
        public EdgeDriver(EdgeDriverService service, EdgeOptions options)
            : this(service, options, RemoteWebDriver.DefaultCommandTimeout)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdgeDriver"/> class using the specified <see cref="EdgeDriverService"/>.
        /// </summary>
        /// <param name="service">The <see cref="EdgeDriverService"/> to use.</param>
        /// <param name="options">The <see cref="EdgeOptions"/> to be used with the Edge driver.</param>
        /// <param name="commandTimeout">The maximum amount of time to wait for each command.</param>
        public EdgeDriver(EdgeDriverService service, EdgeOptions options, TimeSpan commandTimeout)
            : base(new DriverServiceCommandExecutor(service, commandTimeout), ConvertOptionsToCapabilities(options))
        {
            // Add the custom commands unique to Chromium
            this.AddCustomChromiumCommand(GetNetworkConditionsCommand, CommandInfo.GetCommand, "/session/{sessionId}/chromium/network_conditions");
            this.AddCustomChromiumCommand(SetNetworkConditionsCommand, CommandInfo.PostCommand, "/session/{sessionId}/chromium/network_conditions");
            this.AddCustomChromiumCommand(DeleteNetworkConditionsCommand, CommandInfo.DeleteCommand, "/session/{sessionId}/chromium/network_conditions");
            this.AddCustomChromiumCommand(SendChromiumCommand, CommandInfo.PostCommand, "/session/{sessionId}/chromium/send_command");
            this.AddCustomChromiumCommand(SendChromiumCommandWithResult, CommandInfo.PostCommand, "/session/{sessionId}/chromium/send_command_and_get_result");
        }

        /// <summary>
        /// Gets or sets the <see cref="IFileDetector"/> responsible for detecting
        /// sequences of keystrokes representing file paths and names.
        /// </summary>
        /// <remarks>The Edge driver does not allow a file detector to be set,
        /// as the server component of the Edge driver (msedgedriver.exe) only
        /// allows uploads from the local computer environment. Attempting to set
        /// this property has no effect, but does not throw an exception. If you
        /// are attempting to run the Edge driver remotely, use <see cref="RemoteWebDriver"/>
        /// in conjunction with a standalone WebDriver server.</remarks>
        public override IFileDetector FileDetector
        {
            get { return base.FileDetector; }
            set { }
        }

        /// <summary>
        /// Gets or sets the network condition emulation for Edge.
        /// </summary>
        public EdgeNetworkConditions NetworkConditions
        {
            get
            {
                Response response = this.Execute(GetNetworkConditionsCommand, null);
                return EdgeNetworkConditions.FromDictionary(response.Value as Dictionary<string, object>);
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value", "value must not be null");
                }

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters["network_conditions"] = value.ToDictionary();
                this.Execute(SetNetworkConditionsCommand, parameters);
            }
        }

        /// <summary>
        /// Executes a custom Chromium command.
        /// </summary>
        /// <param name="commandName">Name of the command to execute.</param>
        /// <param name="commandParameters">Parameters of the command to execute.</param>
        public void ExecuteChromiumCommand(string commandName, Dictionary<string, object> commandParameters)
        {
            if (commandName == null)
            {
                throw new ArgumentNullException("commandName", "commandName must not be null");
            }

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters["cmd"] = commandName;
            parameters["params"] = commandParameters;
            this.Execute(SendChromiumCommand, parameters);
        }

        public object ExecuteChromiumCommandWithResult(string commandName, Dictionary<string, object> commandParameters)
        {
            if (commandName == null)
            {
                throw new ArgumentNullException("commandName", "commandName must not be null");
            }

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters["cmd"] = commandName;
            parameters["params"] = commandParameters;
            Response response = this.Execute(SendChromiumCommandWithResult, parameters);
            return response.Value;
        }

        private static ICapabilities ConvertOptionsToCapabilities(EdgeOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options", "options must not be null");
            }

            return options.ToCompatibleCapabilities();
        }

        private void AddCustomChromiumCommand(string commandName, string method, string resourcePath)
        {
            CommandInfo commandInfoToAdd = new CommandInfo(method, resourcePath);
            this.CommandExecutor.CommandInfoRepository.TryAddCommand(commandName, commandInfoToAdd);
        }
    }
}