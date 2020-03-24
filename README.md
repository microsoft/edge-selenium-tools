# Selenium Tools for Microsoft Edge

Selenium Tools for Microsoft Edge extends [Selenium 3](https://www.selenium.dev/) with a unified driver to help you write automated tests for both the Microsoft Edge (EdgeHTML) and new Microsoft Edge (Chromium) browsers.

The ``EdgeDriver`` and ``EdgeDriverService`` classes included in this package are 100% backward-compatible with Selenium's built-in [EdgeDriver](https://www.selenium.dev/selenium/docs/api/dotnet/?topic=html/T_OpenQA_Selenium_Edge_EdgeDriver.htm), and will run Microsoft Edge (EdgeHTML) by default so you can use our classes as a seamless drop-in replacement. In addition to being compatible with your existing Selenium tests, Selenium Tools for Microsoft Edge gives you the ability to drive the new Microsoft Edge (Chromium) browser and unlock all of the latest functionality!

The classes in this package are based on the existing ``Edge.*`` and ``Chrome.*`` driver classes included in the [Selenium](https://github.com/SeleniumHQ/selenium) project.

## Before you Begin

We provide this package as a solution for users who prefer to remain on Selenium 3 since it is the current stable release, and users who have existing browser tests and want to add coverage for the new Microsoft Edge (Chromium) without changing their Selenium version.

The very same ``Edge`` driver classes we provide here will be included in Selenium 4 once it is officially released, and are already available today in the latest Selenium 4 Alpha release. If you are able to upgrade to Selenium 4 Alpha, there is no need to use this package as Selenium should already have everything you need built in!

## Getting Starting

### Requirements

* [Selenium.WebDriver 3.141.0](https://www.nuget.org/packages/Selenium.WebDriver/3.141.0)
* [MicrosoftWebDriver and/or MSEdgeDriver](https://developer.microsoft.com/en-us/microsoft-edge/tools/webdriver/)

Note: You will need the correct WebDriver executable for the version of Microsoft Edge you are attempting to drive. The executables are not included with this package. WebDriver executables for all supported versions of Microsoft Edge are available for download [here](https://developer.microsoft.com/en-us/microsoft-edge/tools/webdriver/).

For more information, and instructions on selecting the correct driver for your browser, check out the Microsoft Edge WebDriver [documentation](https://docs.microsoft.com/en-us/microsoft-edge/webdriver-chromium).

### Installation

Add a reference to the [Microsoft.Edge.SeleniumTools](https://www.nuget.org/packages/Microsoft.Edge.SeleniumTools) package to your .NET project.

## Basic Usage

To use with Microsoft Edge (EdgeHTML), simple create a default instance of ``EdgeDriver``. All of the same methods and properties that are currently available in Selenium 3 will continue to work:

```csharp
var driver = new EdgeDriver();
```

To use with Microsoft Edge (Chromium) instead, create a new ``EdgeDriver`` and pass in an ``EdgeOptions`` instance with the ``UseChromium`` property set to true:

```csharp
var options = new EdgeOptions();
options.UseChromium = true;

var driver = new EdgeDriver(options);
```

You can also use ``EdgeOptions`` to choose a specific binary. This is useful for testing [Microsoft Edge Insider](https://www.microsoftedgeinsider.com/) channels like the Beta:

```csharp
var options = new EdgeOptions();
options.UseChromium = true;
options.BinaryLocation = @"C:\Program Files (x86)\Microsoft\Edge Beta\Application\msedge.exe";

var driver = new EdgeDriver(options);
```

When an ``EdgeDriver`` instance is created using ``EdgeOptions``, it will automatically create and launch the appropriate ``EdgeDriverService`` for either Microsoft Edge (EdgeHTML) or Microsoft Edge (Chromium).

If you would like to create an ``EdgeDriverService`` yourself, you can create one configured for Chromium using the ``CreateChromiumService()`` method. This is useful for customizations like enabling verbose log output:

```csharp
using (var service = EdgeDriverService.CreateChromiumService())
{
    service.UseVerboseLogging = true;

    var driver = new EdgeDriver(service);
}
```

Note that it is not necessary to provide ``EdgeOptions`` when creating an ``EdgeDriver`` with your own ``EdgeDriverService`` this way. ``EdgeDriver`` will use the default options for either Microsoft Edge (EdgeHTML) or Microsoft Edge (Chromium) depending on what kind of service you provide. However, if you want to provide both an ``EdgeDriverService`` and ``EdgeOptions``, you must ensure that they are both configured for the same version of Microsoft Edge. For example, it is not possible to use a default (EdgeHTML) ``EdgeDriverService`` and Chromium ``EdgeOptions``. Attempting to do so will throw an error.

## Using Chromium-Specific Options

Using ``EdgeOptions`` with ``UseChromium`` set to true gives you access to all of the same methods and properties that are available on Selenium's [ChromeOptions](https://www.selenium.dev/selenium/docs/api/dotnet/?topic=html/T_OpenQA_Selenium_Chrome_ChromeOptions.htm) class. For example, just like with other Chromium browsers, you can use the ``EdgeOptions.AddArguments()`` method to run Microsoft Edge (Chromium) in headless mode:

```csharp
var options = new EdgeOptions();
options.UseChromium = true;
options.AddArgument("headless");
options.AddArgument("disable-gpu");
```

Note that these Chromium-specific properties and methods are always available but will have no effect if ``UseChromium`` is not enabled. Similarly, existing properties and methods meant for Microsoft Edge (EdgeHTML) will have no effect if ``UseChromium`` is enabled.

# Contributing

We are glad you are interested in automating the latest Microsoft Edge browser and improving the automation experience for the rest of the community!

Before you begin, please read & follow our [Contributor's Guide](CONTRIBUTING.md). Consider also contributing your feature or bug fix directly to [Selenium](https://github.com/SeleniumHQ/selenium) so that it will be included in future Selenium releases.

# Code of Conduct

This project has adopted the [Microsoft Open Source Code of Conduct][conduct-code].
For more information see the [Code of Conduct FAQ][conduct-FAQ] or contact [opencode@microsoft.com][conduct-email] with any additional questions or comments.

[conduct-code]: https://opensource.microsoft.com/codeofconduct/
[conduct-FAQ]: https://opensource.microsoft.com/codeofconduct/faq/
[conduct-email]: mailto:opencode@microsoft.com
