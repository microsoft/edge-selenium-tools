// Portions Copyright Microsoft 2020
// Licensed under the Apache License, Version 2.0
//
// Licensed to the Software Freedom Conservancy (SFC) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The SFC licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.

/**
 * @fileoverview Defines a {@linkplain Driver WebDriver} client for
 * Microsoft's Edge web browser. Before using this module,
 * you must download and install the latest
 * [MicrosoftEdgeDriver](http://go.microsoft.com/fwlink/?LinkId=619687) server.
 * Ensure that the MicrosoftEdgeDriver is on your
 * [PATH](http://en.wikipedia.org/wiki/PATH_%28variable%29).
 *
 * There are three primary classes exported by this module:
 *
 * 1. {@linkplain ServiceBuilder}: configures the
 *     {@link ./remote.DriverService remote.DriverService}
 *     that manages the [MicrosoftEdgeDriver] child process.
 *
 * 2. {@linkplain Options}: defines configuration options for each new
 *     MicrosoftEdgeDriver session, such as which
 *     {@linkplain Options#setProxy proxy} to use when starting the browser.
 *
 * 3. {@linkplain Driver}: the WebDriver client; each new instance will control
 *     a unique browser session.
 *
 * __Customizing the MicrosoftEdgeDriver Server__ <a id="custom-server"></a>
 *
 * By default, every MicrosoftEdge session will use a single driver service,
 * which is started the first time a {@link Driver} instance is created and
 * terminated when this process exits. The default service will inherit its
 * environment from the current process.
 * You may obtain a handle to this default service using
 * {@link #getDefaultService getDefaultService()} and change its configuration
 * with {@link #setDefaultService setDefaultService()}.
 *
 * You may also create a {@link Driver} with its own driver service. This is
 * useful if you need to capture the server's log output for a specific session:
 *
 *     let edge = require('selenium-webdriver/edge');
 *
 *     let service = new edge.ServiceBuilder()
 *         .setPort(55555)
 *         .build();
 *
 *     let options = new edge.Options();
 *     // configure browser options ...
 *
 *     let driver = edge.Driver.createSession(options, service);
 *
 * Users should only instantiate the {@link Driver} class directly when they
 * need a custom driver service configuration (as shown above). For normal
 * operation, users should start MicrosoftEdge using the
 * {@link ./builder.Builder selenium-webdriver.Builder}.
 *
 * [Microsoft EdgeDriver]: https://docs.microsoft.com/en-us/microsoft-edge/webdriver
 */

'use strict';

const http = require('selenium-webdriver/http');
const io = require('selenium-webdriver/io');
const capabilities = require('selenium-webdriver/lib/capabilities');
const promise = require('selenium-webdriver/lib/promise');
const Symbols = require('selenium-webdriver/lib/symbols');
const webdriver = require('selenium-webdriver/lib/webdriver');
const remote = require('selenium-webdriver/remote');
const { Browser, Capabilities, Capability } = require('selenium-webdriver/lib/capabilities');
const command = require('selenium-webdriver/lib/command');
const logging = require('selenium-webdriver/lib/logging');

const EDGE_CHROMIUM_BROWSER_NAME = "msedge";
const WEBVIEW_BROWSER_NAME = "webview2";
const EDGEDRIVER_LEGACY_EXE = 'MicrosoftWebDriver.exe';
const EDGEDRIVER_CHROMIUM_EXE =
  process.platform === 'win32' ? 'msedgedriver.exe' : 'msedgedriver';

function locateSynchronously(browserName) {
  browserName = browserName || Browser.EDGE;
  if (browserName === EDGE_CHROMIUM_BROWSER_NAME) {
    return io.findInPath(EDGEDRIVER_CHROMIUM_EXE, true);
  }

  return process.platform === 'win32'
    ? io.findInPath(EDGEDRIVER_LEGACY_EXE, true) : null;
}

/**
 * Option keys.
 * @enum {string}
 */
const CAPABILITY_KEY = {
  PAGE_LOAD_STRATEGY: 'pageLoadStrategy',
  USE_EDGE_CHROMIUM: 'ms:edgeChromium'
};

/**
 * @param  {(Options|Capabilities|Object<string, *>)=} o The options object
 * @return {boolean}
 */
function useEdgeChromium(o) {
  if (o instanceof Options) {
    return o.getEdgeChromium();
  }

  if (o instanceof Capabilities) {
    return !!o.get(CAPABILITY_KEY.USE_EDGE_CHROMIUM);
  }

  if (o && typeof o === 'object') {
    return !!o[CAPABILITY_KEY.USE_EDGE_CHROMIUM];
  }

  return false;
}

/**
 * Custom command names supported by EdgeDriver.
 * @enum {string}
 */
const Command = {
  LAUNCH_APP: 'launchApp',
  GET_NETWORK_CONDITIONS: 'getNetworkConditions',
  SET_NETWORK_CONDITIONS: 'setNetworkConditions'
};

/**
 * Creates a command executor with support for EdgeDriver's custom commands.
 * @param {!Promise<string>} url The server's URL.
 * @return {!command.Executor} The new command executor.
 */
function createExecutor(url) {
  let client = url.then(url => new http.HttpClient(url));
  let executor = new http.Executor(client);
  configureExecutor(executor);
  return executor;
}

/**
 * Configures the given executor with Chrome-specific commands.
 * @param {!http.Executor} executor the executor to configure.
 */
function configureExecutor(executor) {
  executor.defineCommand(
    Command.LAUNCH_APP,
    'POST',
    '/session/:sessionId/chromium/launch_app');
  executor.defineCommand(
    Command.GET_NETWORK_CONDITIONS,
    'GET',
    '/session/:sessionId/chromium/network_conditions');
  executor.defineCommand(
    Command.SET_NETWORK_CONDITIONS,
    'POST',
    '/session/:sessionId/chromium/network_conditions');
}

const OPTIONS_CAPABILITY_KEY = 'ms:edgeOptions';

/**
 * Class for managing MicrosoftEdgeDriver specific options.
 */
class Options {
  constructor() {
    /** @private {!Object} */
    this.options_ = {};

    /** @private {!Array<(string|!Buffer)>} */
    this.extensions_ = [];

    /** @private {?logging.Preferences} */
    this.logPrefs_ = null;

    /** @private {?capabilities.ProxyConfig} */
    this.proxy_ = null;

    this.useEdgeChromium_ = false;
    this.useWebView_ = false;
  }

  setEdgeChromium(useEdgeChromium) {
    this.useEdgeChromium_ = !!useEdgeChromium;
    return this;
  }

  getEdgeChromium() {
    return this.useEdgeChromium_;
  }

  setUseWebView(useWebView) {
    this.useWebView_ = useWebView;
  }

  getUseWebView() {
    return this.useWebView_;
  }

  /**
   * Extracts the MicrosoftEdgeDriver specific options from the given
   * capabilities object.
   * @param {!capabilities.Capabilities} caps The capabilities object.
   * @return {!Options} The MicrosoftEdgeDriver options.
   */
  static fromCapabilities(caps) {
    let options = new Options();

    if (caps[CAPABILITY_KEY.USE_EDGE_CHROMIUM]) {// chromium edge
      let o = caps.get(OPTIONS_CAPABILITY_KEY);
      if (o instanceof Options) {
        options = o;
      } else if (o) {
        options.
          addArguments(o.args || []).
          addExtensions(o.extensions || []).
          detachDriver(o.detach).
          excludeSwitches(o.excludeSwitches || []).
          setBinaryPath(o.binary).
          setBrowserLogFile(o.logPath).
          setBrowserMinidumpPath(o.minidumpPath).
          setLocalState(o.localState).
          setMobileEmulation(o.mobileEmulation).
          setUserPreferences(o.prefs).
          setPerfLoggingPrefs(o.perfLoggingPrefs);
      }

      if (caps.has(Capability.LOGGING_PREFS)) {
        options.setLoggingPrefs(
          caps.get(Capability.LOGGING_PREFS));
      }
      options.setEdgeChromium(true);

      if (caps.get(Capability.BROWSER_NAME) == WEBVIEW_BROWSER_NAME) {
        options.setUseWebView(true);
      }

    } else {// legacy edge
      let map = options.options_;

      Object.keys(CAPABILITY_KEY).forEach(function (key) {
        key = CAPABILITY_KEY[key];
        if (caps.has(key)) {
          map[key] = caps.get(key);
        }
      });
    }

    if (caps.has(Capability.PROXY)) {
      options.setProxy(caps.get(Capability.PROXY));
    }

    return options;
  }

  /**
   * Add additional command line arguments to use when launching the Edge
   * browser.  Each argument may be specified with or without the "--" prefix
   * (e.g. "--foo" and "foo"). Arguments with an associated value should be
   * delimited by an "=": "foo=bar".
   *
   * @param {...(string|!Array<string>)} args The arguments to add.
   * @return {!Options} A self reference.
   */
  addArguments(...args) {
    let newArgs = (this.options_.args || []).concat(...args);
    if (newArgs.length) {
      this.options_.args = newArgs;
    }
    return this;
  }

  /**
   * Configures the edgedriver to start Edge in headless mode.
   *
   * @return {!Options} A self reference.
   */
  headless() {
    // TODO(jleyba): Remove `disable-gpu` once head Edge no longer requires
    // that to be set.
    return this.addArguments('headless', 'disable-gpu');
  }

  /**
   * Sets the initial window size.
   *
   * @param {{width: number, height: number}} size The desired window size.
   * @return {!Options} A self reference.
   * @throws {TypeError} if width or height is unspecified, not a number, or
   *     less than or equal to 0.
   */
  windowSize({ width, height }) {
    function checkArg(arg) {
      if (typeof arg !== 'number' || arg <= 0) {
        throw TypeError('Arguments must be {width, height} with numbers > 0');
      }
    }
    checkArg(width);
    checkArg(height);
    return this.addArguments(`window-size=${width},${height}`);
  }

  /**
   * List of Edge command line switches to exclude that EdgeDriver by default
   * passes when starting Edge.  Do not prefix switches with "--".
   *
   * @param {...(string|!Array<string>)} args The switches to exclude.
   * @return {!Options} A self reference.
   */
  excludeSwitches(...args) {
    let switches = (this.options_.excludeSwitches || []).concat(...args);
    if (switches.length) {
      this.options_.excludeSwitches = switches;
    }
    return this;
  }

  /**
   * Add additional extensions to install when launching Edge. Each extension
   * should be specified as the path to the packed CRX file, or a Buffer for an
   * extension.
   * @param {...(string|!Buffer|!Array<(string|!Buffer)>)} args The
   *     extensions to add.
   * @return {!Options} A self reference.
   */
  addExtensions(...args) {
    this.extensions_ = this.extensions_.concat(...args);
    return this;
  }

  /**
   * Sets the path to the Edge binary to use. On Mac OS X, this path should
   * reference the actual Edge executable, not just the application binary
   * (e.g. "/Applications/Microsoft Edge.app/Contents/MacOS/Microsoft Edge").
   *
   * The binary path be absolute or relative to edgedriver server
   * executable, but it must exist on the machine that will launch Edge.
   *
   * @param {string} path The path to the Edge binary to use.
   * @return {!Options} A self reference.
   */
  setBinaryPath(path) {
    this.options_.binary = path;
    return this;
  }

  /**
   * Sets whether to leave the started Edge browser running if the controlling
   * EdgeDriver service is killed before {@link webdriver.WebDriver#quit()} is
   * called.
   * @param {boolean} detach Whether to leave the browser running if the
   *     edgedriver service is killed before the session.
   * @return {!Options} A self reference.
   */
  detachDriver(detach) {
    this.options_.detach = detach;
    return this;
  }

  /**
   * Sets the page load strategy for Edge.
   * Supported values are "normal", "eager", and "none";
   *
   * @param {string} pageLoadStrategy The page load strategy to use.
   * @return {!Options} A self reference.
   */
  setPageLoadStrategy(pageLoadStrategy) {
    this.options_[CAPABILITY_KEY.PAGE_LOAD_STRATEGY] =
      pageLoadStrategy.toLowerCase();
    return this;
  }

  /**
   * Sets the user preferences for Edge's user profile. See the "Preferences"
   * file in Edge's user data directory for examples.
   * @param {!Object} prefs Dictionary of user preferences to use.
   * @return {!Options} A self reference.
   */
  setUserPreferences(prefs) {
    this.options_.prefs = prefs;
    return this;
  }

  /**
     * Sets the logging preferences for the new session.
     * @param {!logging.Preferences} prefs The logging preferences.
     * @return {!Options} A self reference.
     */
  setLoggingPrefs(prefs) {
    this.logPrefs_ = prefs;
    return this;
  }

  /**
   * Sets the performance logging preferences. Options include:
   *
   * - `enableNetwork`: Whether or not to collect events from Network domain.
   * - `enablePage`: Whether or not to collect events from Page domain.
   * - `enableTimeline`: Whether or not to collect events from Timeline domain.
   *     Note: when tracing is enabled, Timeline domain is implicitly disabled,
   *     unless `enableTimeline` is explicitly set to true.
   * - `tracingCategories`: A comma-separated string of Edge tracing
   *     categories for which trace events should be collected. An unspecified
   *     or empty string disables tracing.
   * - `bufferUsageReportingInterval`: The requested number of milliseconds
   *     between DevTools trace buffer usage events. For example, if 1000, then
   *     once per second, DevTools will report how full the trace buffer is. If
   *     a report indicates the buffer usage is 100%, a warning will be issued.
   *
   * @param {{enableNetwork: boolean,
    *          enablePage: boolean,
    *          enableTimeline: boolean,
    *          tracingCategories: string,
    *          bufferUsageReportingInterval: number}} prefs The performance
    *     logging preferences.
    * @return {!Options} A self reference.
    */
  setPerfLoggingPrefs(prefs) {
    this.options_.perfLoggingPrefs = prefs;
    return this;
  }

  /**
   * Sets preferences for the "Local State" file in Edge's user data
   * directory.
   * @param {!Object} state Dictionary of local state preferences.
   * @return {!Options} A self reference.
   */
  setLocalState(state) {
    this.options_.localState = state;
    return this;
  }

  /**
   * Sets the name of the activity hosting a Edge-based Android WebView. This
   * option must be set to connect to an [Android WebView]
   * 
   * @param {string} name The activity name.
   * @return {!Options} A self reference.
   */
  androidActivity(name) {
    this.options_.androidActivity = name;
    return this;
  }

  /**
   * Sets the device serial number to connect to via ADB. If not specified, the
   * EdgeDriver will select an unused device at random. An error will be
   * returned if all devices already have active sessions.
   *
   * @param {string} serial The device serial number to connect to.
   * @return {!Options} A self reference.
   */
  androidDeviceSerial(serial) {
    this.options_.androidDeviceSerial = serial;
    return this;
  }

  /**
 * Sets the package name of the Edge or WebView app.
 *
 * @param {?string} pkg The package to connect to, or `null` to disable Android
 *     and switch back to using desktop Edge.
 * @return {!Options} A self reference.
 */
  androidPackage(pkg) {
    this.options_.androidPackage = pkg;
    return this;
  }

  /**
   * Sets the process name of the Activity hosting the WebView (as given by
   * `ps`). If not specified, the process name is assumed to be the same as
   * {@link #androidPackage}.
   *
   * @param {string} processName The main activity name.
   * @return {!Options} A self reference.
   */
  androidProcess(processName) {
    this.options_.androidProcess = processName;
    return this;
  }

  /**
   * Sets whether to connect to an already-running instead of the specified
   * {@linkplain #androidProcess app} instead of launching the app with a clean
   * data directory.
   *
   * @param {boolean} useRunning Whether to connect to a running instance.
   * @return {!Options} A self reference.
   */
  androidUseRunningApp(useRunning) {
    this.options_.androidUseRunningApp = useRunning;
    return this;
  }

  /**
 * Sets the path to Edge's log file. This path should exist on the machine
 * that will launch Edge.
 * @param {string} path Path to the log file to use.
 * @return {!Options} A self reference.
 */
  setBrowserLogFile(path) {
    this.options_.logPath = path;
    return this;
  }

  /**
   * Sets the directory to store Edge minidumps in. This option is only
   * supported when EdgeDriver is running on Linux.
   * @param {string} path The directory path.
   * @return {!Options} A self reference.
   */
  setBrowserMinidumpPath(path) {
    this.options_.minidumpPath = path;
    return this;
  }

  /**
   * Configures Edge to emulate a mobile device. For more information, refer
   * to the EdgeDriver project page on [mobile emulation][em]. Configuration
   * options include:
   *
   * - `deviceName`: The name of a pre-configured [emulated device][devem]
   * - `width`: screen width, in pixels
   * - `height`: screen height, in pixels
   * - `pixelRatio`: screen pixel ratio
   *
   * __Example 1: Using a Pre-configured Device__
   *
   *     let options = new edge.Options().setMobileEmulation(
   *         {deviceName: 'Google Nexus 5'});
   *
   *     let driver = edge.Driver.createSession(options);
   *
   * __Example 2: Using Custom Screen Configuration__
   *
   *     let options = new edge.Options().setMobileEmulation({
   *         width: 360,
   *         height: 640,
   *         pixelRatio: 3.0
   *     });
   *
   *     let driver = edge.Driver.createSession(options);
   *
   *
   * @param {?({deviceName: string}|
    *           {width: number, height: number, pixelRatio: number})} config The
    *     mobile emulation configuration, or `null` to disable emulation.
    * @return {!Options} A self reference.
    */
  setMobileEmulation(config) {
    this.options_.mobileEmulation = config;
    return this;
  }

  /**
   * Sets the proxy settings for the new session.
   * @param {capabilities.ProxyConfig} proxy The proxy configuration to use.
   * @return {!Options} A self reference.
   */
  setProxy(proxy) {
    this.proxy_ = proxy;
    return this;
  }


  /**
   * Converts this options instance to a {@link capabilities.Capabilities}
   * object.
   * @param {capabilities.Capabilities=} opt_capabilities The capabilities to
   *     merge these options into, if any.
   * @return {!capabilities.Capabilities} The capabilities.
   */
  toCapabilities(opt_capabilities) {
    let caps = opt_capabilities || Capabilities.edge();
    if (this.getEdgeChromium()) {// chromium edge
      caps.
        set(Capability.PROXY, this.proxy_).
        set(Capability.LOGGING_PREFS, this.logPrefs_).
        set(OPTIONS_CAPABILITY_KEY, this).
        set(CAPABILITY_KEY.USE_EDGE_CHROMIUM, true);
      if (this.getUseWebView()) {
        caps.set(Capability.BROWSER_NAME, WEBVIEW_BROWSER_NAME);
      }
    } else {// legacy edge
      if (this.proxy_) {
        caps.set(Capability.PROXY, this.proxy_);
      }
      Object.keys(this.options_).forEach(function (key) {
        caps.set(key, this.options_[key]);
      }, this);
      caps.set(CAPABILITY_KEY.USE_EDGE_CHROMIUM, false);
    }
    return caps;
  }

  /**
   * Converts this instance to its JSON wire protocol representation. Note this
   * function is an implementation not intended for general use.
   * @return {!Object}
   *   The JSON wire protocol representation of this instance.
   */
  [Symbols.serialize]() {
    let json = {};
    for (let key in this.options_) {
      if (this.options_[key] != null) {
        json[key] = this.options_[key];
      }
    }
    // chromium edge
    if (this.getEdgeChromium() && this.extensions_.length) {
      json.extensions = this.extensions_.map(function (extension) {
        if (Buffer.isBuffer(extension)) {
          return extension.toString('base64');
        }
        return io.read(/** @type {string} */(extension))
          .then(buffer => buffer.toString('base64'));
      });
    }
    return json;
  }
}// class Options

/**
 * Creates {@link remote.DriverService} instances that manage a
 * MicrosoftEdgeDriver server in a child process.
 */
class ServiceBuilder extends remote.DriverService.Builder {
  /**
   * @param {string=} opt_exe Path to the server executable to use. If omitted,
   *   the builder will attempt to locate the MicrosoftEdgeDriver on the current
   *   PATH.
   * @throws {Error} If provided executable does not exist, or the
   *   MicrosoftEdgeDriver cannot be found on the PATH.
   */
  constructor(opt_exe) {
    let exe = opt_exe || locateSynchronously();
    if (!exe) {
      throw Error(
        'The WebDriver for Edge could not be found on the current PATH. ' +
        'Please download the latest version of the MicrosoftEdgeDriver from ' +
        'https://developer.microsoft.com/en-us/microsoft-edge/tools/webdriver/ and ' +
        'ensure it can be found on your PATH.');
    }

    super(exe);
    this.setLoopback(true);  // Required for chromium edge
    this.options_.args.push('--jwp'); // Required for legacy edge
  }

  /**
   * Sets which port adb is listening to. _The EdgeDriver will connect to adb
   * if an {@linkplain Options#androidPackage Android session} is requested, but
   * adb **must** be started beforehand._
   *
   * @param {number} port Which port adb is running on.
   * @return {!ServiceBuilder} A self reference.
   */
  setAdbPort(port) {
    return this.addArguments('--adb-port=' + port);
  }

  /**
   * Sets the path of the log file the driver should log to. If a log file is
   * not specified, the driver will log to stderr.
   * @param {string} path Path of the log file to use.
   * @return {!ServiceBuilder} A self reference.
   */
  loggingTo(path) {
    return this.addArguments('--log-path=' + path);
  }

  /**
   * Enables verbose logging.
   * @return {!ServiceBuilder} A self reference.
   */
  enableVerboseLogging() {
    return this.addArguments('--verbose');
  }

  /**
   * Sets the number of threads the driver should use to manage HTTP requests.
   * By default, the driver will use 4 threads.
   * @param {number} n The number of threads to use.
   * @return {!ServiceBuilder} A self reference.
   */
  setNumHttpThreads(n) {
    return this.addArguments('--http-threads=' + n);
  }

  /**
   * @override
   */
  setPath(path) {
    super.setPath(path);
    return this.addArguments('--url-base=' + path);
  }
}


/** @type {remote.DriverService} */
let defaultService = null;


/**
 * Sets the default service to use for new MicrosoftEdgeDriver instances.
 * @param {!remote.DriverService} service The service to use.
 * @throws {Error} If the default service is currently running.
 */
function setDefaultService(service) {
  if (defaultService && defaultService.isRunning()) {
    throw Error(
      'The previously configured EdgeDriver service is still running. ' +
      'You must shut it down before you may adjust its configuration.');
  }
  defaultService = service;
}


/**
 * Returns the default MicrosoftEdgeDriver service. If such a service has
 * not been configured, one will be constructed using the default configuration
 * for an MicrosoftEdgeDriver executable found on the system PATH.
 * @return {!remote.DriverService} The default MicrosoftEdgeDriver service.
 */
function getDefaultService() {
  if (!defaultService) {
    defaultService = new ServiceBuilder().build();
  }
  return defaultService;
}

function createServiceFromCapabilities(options) {
  if (useEdgeChromium(options)) {
    let exe = locateSynchronously(EDGE_CHROMIUM_BROWSER_NAME);
    return new ServiceBuilder(exe).build();
  }
  return getDefaultService();
}

/**
 * Creates a new WebDriver client for Microsoft's Edge.
 */
class Driver extends webdriver.WebDriver {
  /**
   * Creates a new browser session for Microsoft's Edge browser.
   *
   * @param {(capabilities.Capabilities|Options)=} opt_config The configuration
   *     options.
   * @param {remote.DriverService=} opt_service The session to use; will use
   *     the {@linkplain #getDefaultService default service} by default.
   * @param {promise.ControlFlow=} opt_flow The control flow to use, or
   *     {@code null} to use the currently active flow.
   * @return {!Driver} A new driver instance.
   */
  static createSession(opt_config, opt_service, opt_flow) {
    let caps = opt_config instanceof Options ? opt_config.toCapabilities() :
    (opt_config || Capabilities.edge());

    let executor, service, client;
    if (useEdgeChromium(opt_config)) {// chromium edge
      if (opt_service instanceof http.Executor) {
        executor = opt_service;
        configureExecutor(executor);
      } else {
        service = opt_service || createServiceFromCapabilities(opt_config);
        executor = createExecutor(service.start());
      }
    } else { // legacy edge
      service = opt_service || createServiceFromCapabilities(opt_config);
      client = service.start().then(url => new http.HttpClient(url));
      executor = new http.Executor(client);
    }

    return /** @type {!Driver} */(
      super.createSession(executor, caps, opt_flow, () => service.kill()));
  }

  /**
   * This function is a no-op as file detectors are not supported by this
   * implementation.
   * @override
   */
  setFileDetector() { }

  /**
   * Schedules a command to launch Edge App with given ID.
   * @param {string} id ID of the App to launch.
   * @return {!promise.Thenable<void>} A promise that will be resolved
   *     when app is launched.
   */
  launchApp(id) {
    return this.schedule(
      new command.Command(Command.LAUNCH_APP).setParameter('id', id),
      'Driver.launchApp()');
  }

  /**
   * Schedules a command to get Edge network emulation settings.
   * @return {!promise.Thenable<T>} A promise that will be resolved
   *     when network emulation settings are retrievied.
   */
  getNetworkConditions() {
    return this.schedule(
      new command.Command(Command.GET_NETWORK_CONDITIONS),
      'Driver.getNetworkConditions()');
  }

  /**
   * Schedules a command to set Edge network emulation settings.
   *
   * __Sample Usage:__
   *
   *  driver.setNetworkConditions({
   *    offline: false,
   *    latency: 5, // Additional latency (ms).
   *    download_throughput: 500 * 1024, // Maximal aggregated download throughput.
   *    upload_throughput: 500 * 1024 // Maximal aggregated upload throughput.
   * });
   *
   * @param {Object} spec Defines the network conditions to set
   * @return {!promise.Thenable<void>} A promise that will be resolved
   *     when network emulation settings are set.
   */
  setNetworkConditions(spec) {
    if (!spec || typeof spec !== 'object') {
      throw TypeError('setNetworkConditions called with non-network-conditions parameter');
    }

    return this.schedule(
      new command.Command(Command.SET_NETWORK_CONDITIONS).setParameter('network_conditions', spec),
      'Driver.setNetworkConditions(' + JSON.stringify(spec) + ')');
  }
}


// PUBLIC API


exports.Driver = Driver;
exports.Options = Options;
exports.ServiceBuilder = ServiceBuilder;
exports.getDefaultService = getDefaultService;
exports.setDefaultService = setDefaultService;
