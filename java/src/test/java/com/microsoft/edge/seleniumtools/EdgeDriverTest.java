package com.microsoft.edge.seleniumtools;

import org.junit.jupiter.api.Test;
import org.openqa.selenium.Capabilities;

import static org.junit.jupiter.api.Assertions.*;

class EdgeDriverTest {

    @Test
    void testDriver() {
        EdgeDriver driver = new EdgeDriver();
        try {
            assertEquals("msedge", driver.getCapabilities().getBrowserName());
        } finally {
            driver.quit();
        }
    }

    @Test
    void testDefaultOptions() {
        EdgeOptions options = new EdgeOptions();
        assertEquals("MicrosoftEdge", options.getBrowserName());
        assertTrue((Boolean)options.getCapability(EdgeOptions.USE_CHROMIUM));
    }

    @Test
    void testUseWebView() {
        EdgeOptions options = new EdgeOptions();
        options.setUseWebView(true);
        assertEquals("webview2", options.getBrowserName());
        assertTrue((Boolean)options.getCapability(EdgeOptions.USE_CHROMIUM));
    }
}