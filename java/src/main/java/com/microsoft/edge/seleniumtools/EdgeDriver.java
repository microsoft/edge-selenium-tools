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

package com.microsoft.edge.seleniumtools;

import com.google.common.collect.ImmutableMap;

import org.openqa.selenium.Capabilities;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.WebDriverException;
import org.openqa.selenium.html5.LocalStorage;
import org.openqa.selenium.html5.Location;
import org.openqa.selenium.html5.LocationContext;
import org.openqa.selenium.html5.SessionStorage;
import org.openqa.selenium.html5.WebStorage;
import org.openqa.selenium.interactions.HasTouchScreen;
import org.openqa.selenium.interactions.TouchScreen;
import org.openqa.selenium.mobile.NetworkConnection;
import org.openqa.selenium.remote.FileDetector;
import org.openqa.selenium.remote.RemoteTouchScreen;
import org.openqa.selenium.remote.RemoteWebDriver;
import org.openqa.selenium.remote.html5.RemoteLocationContext;
import org.openqa.selenium.remote.html5.RemoteWebStorage;
import org.openqa.selenium.remote.mobile.RemoteNetworkConnection;

/**
 * A {@link WebDriver} implementation that controls a Edge browser running on the local machine.
 * This class is provided as a convenience for easily testing the Edge browser. The control server
 * which each instance communicates with will live and die with the instance.
 *
 * To avoid unnecessarily restarting the EdgeDriver server with each instance, use a
 * {@link RemoteWebDriver} coupled with the desired {@link EdgeDriverService}, which is managed
 * separately. For example: <pre>{@code
 *
 * import static org.junit.Assert.assertEquals;
 *
 * import org.junit.*;
 * import org.junit.runner.RunWith;
 * import org.junit.runners.JUnit4;
 * import com.microsoft.edge.seleniumtools.EdgeDriverService;
 * import org.openqa.selenium.remote.DesiredCapabilities;
 * import org.openqa.selenium.remote.RemoteWebDriver;
 *
 * {@literal @RunWith(JUnit4.class)}
 * public class EdgeTest extends TestCase {
 *
 *   private static EdgeDriverService service;
 *   private WebDriver driver;
 *
 *   {@literal @BeforeClass}
 *   public static void createAndStartService() {
 *     service = new EdgeDriverService.Builder()
 *         .usingDriverExecutable(new File("path/to/my/msedgedriver.exe"))
 *         .usingAnyFreePort()
 *         .build();
 *     service.start();
 *   }
 *
 *   {@literal @AfterClass}
 *   public static void createAndStopService() {
 *     service.stop();
 *   }
 *
 *   {@literal @Before}
 *   public void createDriver() {
 *     driver = new RemoteWebDriver(service.getUrl(),
 *         DesiredCapabilities.edge());
 *   }
 *
 *   {@literal @After}
 *   public void quitDriver() {
 *     driver.quit();
 *   }
 *
 *   {@literal @Test}
 *   public void testBingSearch() {
 *     driver.get("https://www.bing.com");
 *     WebElement searchBox = driver.findElement(By.name("q"));
 *     searchBox.sendKeys("webdriver");
 *     searchBox.quit();
 *     assertEquals("webdriver - Bing", driver.getTitle());
 *   }
 * }
 * }</pre>
 *
 * Note that unlike EdgeDriver, RemoteWebDriver doesn't directly implement
 * role interfaces such as {@link LocationContext} and {@link WebStorage}.
 * Therefore, to access that functionality, it needs to be
 * {@link org.openqa.selenium.remote.Augmenter augmented} and then cast
 * to the appropriate interface.
 *
 * @see EdgeDriverService#createDefaultService
 */

/**
 * @deprecated Selenium Tools for Microsoft Edge is deprecated. Please upgrade to Selenium 4 which has built-in support for Microsoft Edge (Chromium): https://docs.microsoft.com/en-us/microsoft-edge/webdriver-chromium/#upgrading-from-selenium-3
 */
@Deprecated
public class EdgeDriver extends RemoteWebDriver
        implements LocationContext, WebStorage, HasTouchScreen, NetworkConnection {

    private RemoteLocationContext locationContext;
    private RemoteWebStorage webStorage;
    private TouchScreen touchScreen;
    private RemoteNetworkConnection networkConnection;

    /**
     * Creates a new EdgeDriver using the {@link EdgeDriverService#createDefaultService default}
     * server configuration.
     *
     * @see #EdgeDriver(EdgeDriverService, EdgeOptions)
     */
    public EdgeDriver() {
        this(EdgeDriverService.createDefaultService(), new EdgeOptions());
    }

    /**
     * Creates a new EdgeDriver instance. The {@code service} will be started along with the driver,
     * and shutdown upon calling {@link #quit()}.
     *
     * @param service The service to use.
     * @see RemoteWebDriver#RemoteWebDriver(org.openqa.selenium.remote.CommandExecutor, Capabilities)
     */
    public EdgeDriver(EdgeDriverService service) {
        this(service, new EdgeOptions());
    }

    /**
     * Creates a new EdgeDriver instance. The {@code capabilities} will be passed to the
     * EdgeDriver service.
     *
     * @param capabilities The capabilities required from the EdgeDriver.
     * @see #EdgeDriver(EdgeDriverService, Capabilities)
     * @deprecated Use {@link EdgeDriver(EdgeOptions)} instead.
     */
    @Deprecated
    public EdgeDriver(Capabilities capabilities) {
        this(EdgeDriverService.createDefaultService(), capabilities);
    }

    /**
     * Creates a new EdgeDriver instance with the specified options.
     *
     * @param options The options to use.
     * @see #EdgeDriver(EdgeDriverService, EdgeOptions)
     */
    public EdgeDriver(EdgeOptions options) {
        this(EdgeDriverService.createDefaultService(), options);
    }

    /**
     * Creates a new EdgeDriver instance with the specified options. The {@code service} will be
     * started along with the driver, and shutdown upon calling {@link #quit()}.
     *
     * @param service The service to use.
     * @param options The options to use.
     */
    public EdgeDriver(EdgeDriverService service, EdgeOptions options) {
        this(service, (Capabilities) options);
    }

    /**
     * Creates a new EdgeDriver instance. The {@code service} will be started along with the
     * driver, and shutdown upon calling {@link #quit()}.
     *
     * @param service The service to use.
     * @param capabilities The capabilities required from the EdgeDriver.
     * @deprecated Use {@link EdgeDriver(EdgeDriverService, EdgeOptions)} instead.
     */
    @Deprecated
    public EdgeDriver(EdgeDriverService service, Capabilities capabilities) {
        super(new EdgeDriverCommandExecutor(service), capabilities);
        locationContext = new RemoteLocationContext(getExecuteMethod());
        webStorage = new RemoteWebStorage(getExecuteMethod());
        touchScreen = new RemoteTouchScreen(getExecuteMethod());
        networkConnection = new RemoteNetworkConnection(getExecuteMethod());
    }

    @Override
    public void setFileDetector(FileDetector detector) {
        throw new WebDriverException(
                "Setting the file detector only works on remote webdriver instances obtained " +
                        "via RemoteWebDriver");
    }

    @Override
    public LocalStorage getLocalStorage() {
        return webStorage.getLocalStorage();
    }

    @Override
    public SessionStorage getSessionStorage() {
        return webStorage.getSessionStorage();
    }

    @Override
    public Location location() {
        return locationContext.location();
    }

    @Override
    public void setLocation(Location location) {
        locationContext.setLocation(location);
    }

    @Override
    public TouchScreen getTouch() {
        return touchScreen;
    }

    @Override
    public ConnectionType getNetworkConnection() {
        return networkConnection.getNetworkConnection();
    }

    @Override
    public ConnectionType setNetworkConnection(ConnectionType type) {
        return networkConnection.setNetworkConnection(type);
    }

    /**
     * Launches Edge app specified by id.
     *
     * @param id Edge app id.
     */
    public void launchApp(String id) {
        execute(EdgeDriverCommand.LAUNCH_APP, ImmutableMap.of("id", id));
    }

}
