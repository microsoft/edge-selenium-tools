// <copyright file="EdgeDriverService.cs" company="WebDriver Committers">
//
// Portions Copyright Microsoft 2020
// Licensed under the Apache License, Version 2.0
//
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
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.Internal;

namespace Microsoft.Edge.SeleniumTools
{
    /// <summary>
    /// Exposes the service provided by the native MicrosoftWebDriver or msedgedriver executable.
    /// </summary>
    public sealed class EdgeDriverService : DriverService
    {
        private const string MicrosoftWebDriverServiceExecutableName = "MicrosoftWebDriver.exe";
        private const string ChromiumEdgeDriverServiceExecutableName = "msedgedriver";

        private static readonly Uri MicrosoftWebDriverDownloadUrl = new Uri("https://developer.microsoft.com/en-us/microsoft-edge/tools/webdriver/");

        private readonly bool useChromium;

        // Chromium properties
        private string logPath = string.Empty;
        private string urlPathPrefix = string.Empty;
        private string portServerAddress = string.Empty;
        private string whitelistedIpAddresses = string.Empty;
        private int adbPort = -1;
        private bool enableVerboseLogging;

        // Legacy properties
        private string host;
        private string package;
        private bool? useSpecCompliantProtocol;

        /// <summary>
        /// Initializes a new instance of the <see cref="EdgeDriverService"/> class.
        /// </summary>
        /// <param name="executablePath">The full path to the WebDriver executable.</param>
        /// <param name="executableFileName">The file name of the WebDriver executable.</param>
        /// <param name="port">The port on which the WebDriver executable should listen.</param>
        private EdgeDriverService(string executablePath, string executableFileName, int port, bool useChromium)
            : base(executablePath, port, executableFileName, MicrosoftWebDriverDownloadUrl)
        {
            this.useChromium = useChromium;
        }

        /// <summary>
        /// Gets a value indicating whether the driver service is using Edge Chromium.
        /// </summary>
        public bool UsingChromium
        {
            get { return this.useChromium; }
        }

        /// <summary>
        /// Gets or sets the value of the host adapter on which the Edge driver service should listen for connections.
        /// </summary>
        public string Host
        {
            get { return this.host; }
            set { this.host = value; }
        }

        /// <summary>
        /// Gets or sets the value of the package the Edge driver service will launch and automate.
        /// </summary>
        public string Package
        {
            get { return this.package; }
            set { this.package = value; }
        }

        /// <summary>
        /// Gets or sets the location of the log file written to by the WebDriver executable.
        /// </summary>
        public string LogPath
        {
            get { return this.logPath; }
            set { this.logPath = value; }
        }

        /// <summary>
        /// Gets or sets the base URL path prefix for commands (e.g., "wd/url").
        /// </summary>
        public string UrlPathPrefix
        {
            get { return this.urlPathPrefix; }
            set { this.urlPathPrefix = value; }
        }

        /// <summary>
        /// Gets or sets the address of a server to contact for reserving a port.
        /// </summary>
        public string PortServerAddress
        {
            get { return this.portServerAddress; }
            set { this.portServerAddress = value; }
        }

        /// <summary>
        /// Gets or sets the port on which the Android Debug Bridge is listening for commands.
        /// </summary>
        public int AndroidDebugBridgePort
        {
            get { return this.adbPort; }
            set { this.adbPort = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to enable verbose logging for the WebDriver executable.
        /// Defaults to <see langword="false"/>.
        /// </summary>
        public bool UseVerboseLogging
        {
            get { return this.enableVerboseLogging; }
            set { this.enableVerboseLogging = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="EdgeDriverService"/> instance
        /// should use the a protocol dialect compliant with the W3C WebDriver Specification.
        /// </summary>
        /// <remarks>
        /// Setting this property to a non-<see langword="null"/> value for driver
        /// executables matched to versions of Windows before the 2018 Fall Creators
        /// Update will result in a the driver executable shutting down without
        /// execution, and all commands will fail. Do not set this property unless
        /// you are certain your version of the MicrosoftWebDriver.exe supports the
        /// --w3c and --jwp command-line arguments.
        /// </remarks>
        public bool? UseSpecCompliantProtocol
        {
            get { return this.useSpecCompliantProtocol; }
            set { this.useSpecCompliantProtocol = value; }
        }

        /// <summary>
        /// Gets or sets the comma-delimited list of IP addresses that are approved to
        /// connect to this instance of the Edge driver. Defaults to an empty string,
        /// which means only the local loopback address can connect.
        /// </summary>
        public string WhitelistedIPAddresses
        {
            get { return this.whitelistedIpAddresses; }
            set { this.whitelistedIpAddresses = value; }
        }

        /// <summary>
        /// Gets a value indicating whether the service has a shutdown API that can be called to terminate
        /// it gracefully before forcing a termination.
        /// </summary>
        protected override bool HasShutdown
        {
            get
            {
                if (this.useSpecCompliantProtocol.HasValue && !this.useSpecCompliantProtocol.Value)
                {
                    return base.HasShutdown;
                }

                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating the time to wait for the service to terminate before forcing it to terminate.
        /// </summary>
        protected override TimeSpan TerminationTimeout
        {
            // Use a very small timeout for terminating the Edge driver,
            // because the executable does not have a clean shutdown command,
            // which means we have to kill the process. Using a short timeout
            // gets us to the termination point much faster.
            get
            {
                if (this.useSpecCompliantProtocol.HasValue && !this.useSpecCompliantProtocol.Value)
                {
                    return base.TerminationTimeout;
                }

                return TimeSpan.FromMilliseconds(100);
            }
        }

        /// <summary>
        /// Gets the command-line arguments for the driver service.
        /// </summary>
        protected override string CommandLineArguments
        {
            get
            {
                StringBuilder argsBuilder = new StringBuilder(base.CommandLineArguments);
                if (this.useChromium)
                {
                    AddChromiumCommandLineArguments(argsBuilder);
                }
                else
                {
                    AddLegacyCommandLineArguments(argsBuilder);
                }
                return argsBuilder.ToString();
            }
        }

        private void AddChromiumCommandLineArguments(StringBuilder argsBuilder)
        {
            if (this.adbPort > 0)
            {
                argsBuilder.AppendFormat(CultureInfo.InvariantCulture, " --adb-port={0}", this.adbPort);
            }

            if (this.SuppressInitialDiagnosticInformation)
            {
                argsBuilder.Append(" --silent");
            }

            if (this.enableVerboseLogging)
            {
                argsBuilder.Append(" --verbose");
            }

            if (!string.IsNullOrEmpty(this.logPath))
            {
                argsBuilder.AppendFormat(CultureInfo.InvariantCulture, " --log-path={0}", this.logPath);
            }

            if (!string.IsNullOrEmpty(this.urlPathPrefix))
            {
                argsBuilder.AppendFormat(CultureInfo.InvariantCulture, " --url-base={0}", this.urlPathPrefix);
            }

            if (!string.IsNullOrEmpty(this.portServerAddress))
            {
                argsBuilder.AppendFormat(CultureInfo.InvariantCulture, " --port-server={0}", this.portServerAddress);
            }

            if (!string.IsNullOrEmpty(this.whitelistedIpAddresses))
            {
                argsBuilder.Append(string.Format(CultureInfo.InvariantCulture, " -whitelisted-ips={0}", this.whitelistedIpAddresses));
            }
        }

        private void AddLegacyCommandLineArguments(StringBuilder argsBuilder)
        {
            if (!string.IsNullOrEmpty(this.host))
            {
                argsBuilder.Append(string.Format(CultureInfo.InvariantCulture, " --host={0}", this.host));
            }

            if (!string.IsNullOrEmpty(this.package))
            {
                argsBuilder.Append(string.Format(CultureInfo.InvariantCulture, " --package={0}", this.package));
            }

            if (this.enableVerboseLogging)
            {
                argsBuilder.Append(" --verbose");
            }

            if (this.SuppressInitialDiagnosticInformation)
            {
                argsBuilder.Append(" --silent");
            }

            if (this.useSpecCompliantProtocol.HasValue)
            {
                if (this.useSpecCompliantProtocol.Value)
                {
                    argsBuilder.Append(" --w3c");
                }
                else
                {
                    argsBuilder.Append(" --jwp");
                }
            }
        }

        /// <summary>
        /// Creates an instance of the EdgeDriverService for Edge Chromium.
        /// </summary>
        /// <returns>A EdgeDriverService that implements default settings.</returns>
        public static EdgeDriverService CreateChromiumService()
        {
            return CreateDefaultServiceFromOptions(new EdgeOptions() { UseChromium = true });
        }

        /// <summary>
        /// Creates an instance of the EdgeDriverService for Edge Chromium using a specified path to the WebDriver executable.
        /// </summary>
        /// <param name="driverPath">The directory containing the WebDriver executable.</param>
        /// <returns>A EdgeDriverService using a random port.</returns>
        public static EdgeDriverService CreateChromiumService(string driverPath)
        {
            return CreateDefaultServiceFromOptions(driverPath, EdgeDriverServiceFileName(true), new EdgeOptions() { UseChromium = true });
        }

        /// <summary>
        /// Creates an instance of the EdgeDriverService for Edge Chromium using a specified path to the WebDriver executable with the given name.
        /// </summary>
        /// <param name="driverPath">The directory containing the WebDriver executable.</param>
        /// <param name="driverExecutableFileName">The name of the WebDriver executable file.</param>
        /// <returns>A EdgeDriverService using a random port.</returns>
        public static EdgeDriverService CreateChromiumService(string driverPath, string driverExecutableFileName)
        {
            return CreateDefaultServiceFromOptions(driverPath, driverExecutableFileName, new EdgeOptions() { UseChromium = true });
        }

        /// <summary>
        /// Creates an instance of the EdgeDriverService for Edge Chromium using a specified path to the WebDriver executable with the given name and listening port.
        /// </summary>
        /// <param name="driverPath">The directory containing the WebDriver executable.</param>
        /// <param name="driverExecutableFileName">The name of the WebDriver executable file</param>
        /// <param name="port">The port number on which the driver will listen</param>
        /// <returns>A EdgeDriverService using the specified port.</returns>
        public static EdgeDriverService CreateChromiumService(string driverPath, string driverExecutableFileName, int port)
        {
            return CreateDefaultServiceFromOptions(driverPath, driverExecutableFileName, port, new EdgeOptions() { UseChromium = true });
        }

        /// <summary>
        /// Creates a default instance of the EdgeDriverService.
        /// </summary>
        /// <returns>A EdgeDriverService that implements default settings.</returns>
        public static EdgeDriverService CreateDefaultService()
        {
            return CreateDefaultServiceFromOptions(new EdgeOptions());
        }

        /// <summary>
        /// Creates a default instance of the EdgeDriverService using a specified path to the WebDriver executable.
        /// </summary>
        /// <param name="driverPath">The directory containing the WebDriver executable.</param>
        /// <returns>A EdgeDriverService using a random port.</returns>
        public static EdgeDriverService CreateDefaultService(string driverPath)
        {
            return CreateDefaultServiceFromOptions(driverPath, EdgeDriverServiceFileName(false), new EdgeOptions());
        }

        /// <summary>
        /// Creates a default instance of the EdgeDriverService using a specified path to the WebDriver executable with the given name.
        /// </summary>
        /// <param name="driverPath">The directory containing the WebDriver executable.</param>
        /// <param name="driverExecutableFileName">The name of the WebDriver executable file.</param>
        /// <returns>A EdgeDriverService using a random port.</returns>
        public static EdgeDriverService CreateDefaultService(string driverPath, string driverExecutableFileName)
        {
            return CreateDefaultServiceFromOptions(driverPath, driverExecutableFileName, new EdgeOptions());
        }

        /// <summary>
        /// Creates a default instance of the EdgeDriverService using a specified path to the WebDriver executable with the given name and listening port.
        /// </summary>
        /// <param name="driverPath">The directory containing the WebDriver executable.</param>
        /// <param name="driverExecutableFileName">The name of the WebDriver executable file</param>
        /// <param name="port">The port number on which the driver will listen</param>
        /// <returns>A EdgeDriverService using the specified port.</returns>
        public static EdgeDriverService CreateDefaultService(string driverPath, string driverExecutableFileName, int port)
        {
            return CreateDefaultServiceFromOptions(driverPath, driverExecutableFileName, port, new EdgeOptions());
        }

        /// <summary>
        /// Creates a default instance of the EdgeDriverService.
        /// </summary>
        /// <returns>A EdgeDriverService that implements default settings.</returns>
        public static EdgeDriverService CreateDefaultServiceFromOptions(EdgeOptions options)
        {
            string serviceDirectory = DriverService.FindDriverServiceExecutable(EdgeDriverServiceFileName(options.UseChromium), MicrosoftWebDriverDownloadUrl);
            return CreateDefaultServiceFromOptions(serviceDirectory, options);
        }

        /// <summary>
        /// Creates a default instance of the EdgeDriverService using a specified path to the WebDriver executable.
        /// </summary>
        /// <param name="driverPath">The directory containing the WebDriver executable.</param>
        /// <returns>A EdgeDriverService using a random port.</returns>
        public static EdgeDriverService CreateDefaultServiceFromOptions(string driverPath, EdgeOptions options)
        {
            return CreateDefaultServiceFromOptions(driverPath, EdgeDriverServiceFileName(options.UseChromium), options);
        }

        /// <summary>
        /// Creates a default instance of the EdgeDriverService using a specified path to the WebDriver executable with the given name.
        /// </summary>
        /// <param name="driverPath">The directory containing the WebDriver executable.</param>
        /// <param name="driverExecutableFileName">The name of the WebDriver executable file.</param>
        /// <returns>A EdgeDriverService using a random port.</returns>
        public static EdgeDriverService CreateDefaultServiceFromOptions(string driverPath, string driverExecutableFileName, EdgeOptions options)
        {
            // Locate a free port on the local machine by binding a socket to
            // an IPEndPoint using IPAddress.Any and port 0. The socket will
            // select a free port.
            int listeningPort = 0;
            Socket portSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                IPEndPoint socketEndPoint = new IPEndPoint(IPAddress.Any, 0);
                portSocket.Bind(socketEndPoint);
                socketEndPoint = (IPEndPoint)portSocket.LocalEndPoint;
                listeningPort = socketEndPoint.Port;
            }
            finally
            {
                portSocket.Close();
            }

            return CreateDefaultServiceFromOptions(driverPath, driverExecutableFileName, listeningPort, options);
        }

        /// <summary>
        /// Creates a default instance of the EdgeDriverService using a specified path to the WebDriver executable with the given name and listening port.
        /// </summary>
        /// <param name="driverPath">The directory containing the WebDriver executable.</param>
        /// <param name="driverExecutableFileName">The name of the WebDriver executable file</param>
        /// <param name="port">The port number on which the driver will listen</param>
        /// <returns>A EdgeDriverService using the specified port.</returns>
        public static EdgeDriverService CreateDefaultServiceFromOptions(string driverPath, string driverExecutableFileName, int port, EdgeOptions options)
        {
            return new EdgeDriverService(driverPath, driverExecutableFileName, port, options.UseChromium);
        }

        /// <summary>
        /// Returns the Edge driver filename for the currently running platform
        /// </summary>
        /// <returns>The file name of the Edge driver service executable.</returns>
        private static string EdgeDriverServiceFileName(bool useChromium)
        {
            string fileName = useChromium ? ChromiumEdgeDriverServiceExecutableName : MicrosoftWebDriverServiceExecutableName;

            if (useChromium)
            {
                // Unfortunately, detecting the currently running platform isn't as
                // straightforward as you might hope.
                // See: http://mono.wikia.com/wiki/Detecting_the_execution_platform
                // and https://msdn.microsoft.com/en-us/library/3a8hyw88(v=vs.110).aspx
                const int PlatformMonoUnixValue = 128;

                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Win32NT:
                    case PlatformID.Win32S:
                    case PlatformID.Win32Windows:
                    case PlatformID.WinCE:
                        fileName += ".exe";
                        break;

                    case PlatformID.MacOSX:
                    case PlatformID.Unix:
                        break;

                    // Don't handle the Xbox case. Let default handle it.
                    // case PlatformID.Xbox:
                    //     break;
                    default:
                        if ((int)Environment.OSVersion.Platform == PlatformMonoUnixValue)
                        {
                            break;
                        }

                        throw new WebDriverException("Unsupported platform: " + Environment.OSVersion.Platform);
                }
            }

            return fileName;
        }
    }
}