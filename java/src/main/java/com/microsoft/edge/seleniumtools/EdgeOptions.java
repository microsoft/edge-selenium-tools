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

import static com.google.common.base.Preconditions.checkArgument;
import static com.google.common.base.Preconditions.checkNotNull;
import static org.openqa.selenium.remote.CapabilityType.ACCEPT_INSECURE_CERTS;
import static org.openqa.selenium.remote.CapabilityType.PAGE_LOAD_STRATEGY;
import static org.openqa.selenium.remote.CapabilityType.UNEXPECTED_ALERT_BEHAVIOUR;
import static org.openqa.selenium.remote.CapabilityType.UNHANDLED_PROMPT_BEHAVIOUR;

import com.google.common.collect.ImmutableList;
import com.google.common.io.Files;

import org.openqa.selenium.Capabilities;
import org.openqa.selenium.MutableCapabilities;
import org.openqa.selenium.PageLoadStrategy;
import org.openqa.selenium.Proxy;
import org.openqa.selenium.SessionNotCreatedException;
import org.openqa.selenium.UnexpectedAlertBehaviour;
import org.openqa.selenium.remote.BrowserType;
import org.openqa.selenium.remote.CapabilityType;

import java.io.File;
import java.io.IOException;
import java.util.ArrayList;
import java.util.Base64;
import java.util.Collections;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.Objects;
import java.util.TreeMap;
import java.util.stream.Stream;

/**
 * Class to manage options specific to {@link EdgeDriver}.
 *
 * <p>Example usage:
 * <pre><code>
 * EdgeOptions options = new EdgeOptions()
 * options.addExtensions(new File("/path/to/extension.crx"))
 * options.setBinary(new File("/path/to/msedge"));
 *
 * // For use with EdgeDriver:
 * EdgeDriver driver = new EdgeDriver(options);
 *
 * // For use with RemoteWebDriver:
 * RemoteWebDriver driver = new RemoteWebDriver(
 *     new URL("http://localhost:4444/wd/hub"),
 *     new EdgeOptions());
 * </code></pre>
 *
 * @since Since msedgedriver v17.0.963.0
 */
public class EdgeOptions extends MutableCapabilities {

    /**
     * Key used to store a set of EdgeOptions in a {@link Capabilities}
     * object.
     */
    public static final String CAPABILITY = "ms:edgeOptions";
    public static final String USE_CHROMIUM = "ms:edgeChromium";

    private static final String WEBVIEW_BROWSER_NAME = "webview2";

    private String binary;
    private List<String> args = new ArrayList<>();
    private List<File> extensionFiles = new ArrayList<>();
    private List<String> extensions = new ArrayList<>();
    private Map<String, Object> experimentalOptions = new HashMap<>();

    public EdgeOptions() {
        setCapability(CapabilityType.BROWSER_NAME, BrowserType.EDGE);
        setCapability(USE_CHROMIUM, true);
    }

    @Override
    public EdgeOptions merge(Capabilities extraCapabilities) {
        super.merge(extraCapabilities);
        return this;
    }

    /**
     * Sets whether to launch an Edge (Chromium) WebView executable instead of
     * launching the Edge browser.
     *
     * @param useWebView Whether to launch a WebView executable.
     */
    public EdgeOptions setUseWebView(boolean useWebView) {
        setCapability(CapabilityType.BROWSER_NAME, useWebView ? WEBVIEW_BROWSER_NAME : BrowserType.EDGE);
        return this;
    }

    /**
     * Sets the path to the Edge executable. This path should exist on the
     * machine which will launch Edge. The path should either be absolute or
     * relative to the location of running EdgeDriver server.
     *
     * @param path Path to Edge executable.
     */
    public EdgeOptions setBinary(File path) {
        binary = checkNotNull(path).getPath();
        return this;
    }

    /**
     * Sets the path to the Edge executable. This path should exist on the
     * machine which will launch Edge. The path should either be absolute or
     * relative to the location of running EdgeDriver server.
     *
     * @param path Path to Edge executable.
     */
    public EdgeOptions setBinary(String path) {
        binary = checkNotNull(path);
        return this;
    }

    /**
     * @param arguments The arguments to use when starting Edge.
     * @see #addArguments(java.util.List)
     */
    public EdgeOptions addArguments(String... arguments) {
        addArguments(ImmutableList.copyOf(arguments));
        return this;
    }

    /**
     * Adds additional command line arguments to be used when starting Edge.
     * For example:
     * <pre><code>
     *   options.setArguments(
     *       "load-extension=/path/to/unpacked_extension",
     *       "allow-outdated-plugins");
     * </code></pre>
     *
     * <p>Each argument may contain an option "--" prefix: "--foo" or "foo".
     * Arguments with an associated value should be delimitted with an "=":
     * "foo=bar".
     *
     * @param arguments The arguments to use when starting Edge.
     */
    public EdgeOptions addArguments(List<String> arguments) {
        args.addAll(arguments);
        return this;
    }

    /**
     * @param paths Paths to the extensions to install.
     * @see #addExtensions(java.util.List)
     */
    public EdgeOptions addExtensions(File... paths) {
        addExtensions(ImmutableList.copyOf(paths));
        return this;
    }

    /**
     * Adds a new Edge extension to install on browser startup. Each path should
     * specify a packed Edge extension (CRX file).
     *
     * @param paths Paths to the extensions to install.
     */
    public EdgeOptions addExtensions(List<File> paths) {
        for (File path : paths) {
            checkNotNull(path);
            checkArgument(path.exists(), "%s does not exist", path.getAbsolutePath());
            checkArgument(!path.isDirectory(), "%s is a directory",
                    path.getAbsolutePath());
        }
        extensionFiles.addAll(paths);
        return this;
    }

    /**
     * @param encoded Base64 encoded data of the extensions to install.
     * @see #addEncodedExtensions(java.util.List)
     */
    public EdgeOptions addEncodedExtensions(String... encoded) {
        addEncodedExtensions(ImmutableList.copyOf(encoded));
        return this;
    }

    /**
     * Adds a new Edge extension to install on browser startup. Each string data should
     * specify a Base64 encoded string of packed Edge extension (CRX file).
     *
     * @param encoded Base64 encoded data of the extensions to install.
     */
    public EdgeOptions addEncodedExtensions(List<String> encoded) {
        for (String extension : encoded) {
            checkNotNull(extension);
        }
        extensions.addAll(encoded);
        return this;
    }

    /**
     * Sets an experimental option. Useful for new EdgeDriver options not yet
     * exposed through the {@link EdgeOptions} API.
     *
     * @param name Name of the experimental option.
     * @param value Value of the experimental option, which must be convertible
     *     to JSON.
     */
    public EdgeOptions setExperimentalOption(String name, Object value) {
        experimentalOptions.put(checkNotNull(name), value);
        return this;
    }

    /**
     * Returns the value of an experimental option.
     *
     * @param name The option name.
     * @return The option value, or {@code null} if not set.
     * @deprecated Getters are not needed in browser Options classes.
     */
    @Deprecated
    public Object getExperimentalOption(String name) {
        return experimentalOptions.get(checkNotNull(name));
    }

    public EdgeOptions setPageLoadStrategy(PageLoadStrategy strategy) {
        setCapability(PAGE_LOAD_STRATEGY, strategy);
        return this;
    }

    public EdgeOptions setUnhandledPromptBehaviour(UnexpectedAlertBehaviour behaviour) {
        setCapability(UNHANDLED_PROMPT_BEHAVIOUR, behaviour);
        setCapability(UNEXPECTED_ALERT_BEHAVIOUR, behaviour);
        return this;
    }

    /**
     * Returns EdgeOptions with the capability ACCEPT_INSECURE_CERTS set.
     * @param acceptInsecureCerts
     * @return EdgeOptions
     */
    public EdgeOptions setAcceptInsecureCerts(boolean acceptInsecureCerts) {
        setCapability(ACCEPT_INSECURE_CERTS, acceptInsecureCerts);
        return this;
    }

    public EdgeOptions setHeadless(boolean headless) {
        args.remove("--headless");
        if (headless) {
            args.add("--headless");
            args.add("--disable-gpu");
        }
        return this;
    }

    public EdgeOptions setProxy(Proxy proxy) {
        setCapability(CapabilityType.PROXY, proxy);
        return this;
    }

    @Override
    protected int amendHashCode() {
        return Objects.hash(
                args,
                binary,
                experimentalOptions,
                extensionFiles,
                extensions);
    }

    @Override
    public Map<String, Object> asMap() {
        Map<String, Object> toReturn = new TreeMap<>(super.asMap());

        Map<String, Object> options = new TreeMap<>();
        experimentalOptions.forEach(options::put);

        if (binary != null) {
            options.put("binary", binary);
        }

        options.put("args", ImmutableList.copyOf(args));

        options.put(
                "extensions",
                Stream.concat(
                        extensionFiles.stream()
                                .map(file -> {
                                    try {
                                        return Base64.getEncoder().encodeToString(Files.toByteArray(file));
                                    } catch (IOException e) {
                                        throw new SessionNotCreatedException(e.getMessage(), e);
                                    }
                                }),
                        extensions.stream()
                ).collect(ImmutableList.toImmutableList()));

        toReturn.put(CAPABILITY, options);
        toReturn.put(USE_CHROMIUM, true);

        return Collections.unmodifiableMap(toReturn);
    }
}
