# Selenium Tools for Microsoft Edge

Selenium Tools for Microsoft Edge extends [Selenium 3](https://www.selenium.dev/) with a unified driver to help you write automated tests for both the Microsoft Edge (EdgeHTML) and new Microsoft Edge (Chromium) browsers.

The libraries included in this package are 100% backward-compatible with Selenium's built-in Edge libraries, and will run Microsoft Edge (EdgeHTML) by default so you can use our classes as a seamless drop-in replacement. In addition to being compatible with your existing Selenium tests, Selenium Tools for Microsoft Edge gives you the ability to drive the new Microsoft Edge (Chromium) browser and unlock all of the latest functionality!

The classes in this package are based on the existing ``Edge`` and ``Chrome`` driver classes included in the [Selenium](https://github.com/SeleniumHQ/selenium) project.

## Before you Begin

The Selenium Tools for Microsoft Edge is a solution for developers who prefer to remain on Selenium 3 which is the current stable release and developers who have existing browser tests and want to add coverage for the new Microsoft Edge (Chromium) browser without changing the Selenium version.

The very same ``Edge`` driver classes provided in this package will be included in Selenium 4 once it is officially released, and are already available today in the latest Selenium 4 Alpha release. If you are able to upgrade to Selenium 4 Alpha, there is no need to use this package as Selenium should already have everything you need built in!

# Getting Started

## Requirements

* [Microsoft Edge Driver](https://developer.microsoft.com/en-us/microsoft-edge/tools/webdriver/)
* Selenium WebDriver 3.141.0
    * [Python](https://pypi.org/project/selenium/3.141.0/)
    * [.NET](https://www.nuget.org/packages/Selenium.WebDriver/3.141.0)

## Installation

Note: You will need the correct [WebDriver executable](https://developer.microsoft.com/en-us/microsoft-edge/tools/webdriver/) for the version of Microsoft Edge you are attempting to drive. The executables are not included with this package. WebDriver executables for all supported versions of Microsoft Edge are available for download [here](https://developer.microsoft.com/en-us/microsoft-edge/tools/webdriver/).

For more information, and instructions on selecting the correct driver for your browser, check out the Microsoft Edge WebDriver [documentation](https://docs.microsoft.com/en-us/microsoft-edge/webdriver-chromium).


### C#

Add the [Microsoft.Edge.SeleniumTools](https://www.nuget.org/packages/Microsoft.Edge.SeleniumTools) package to your .NET project using the NuGet CLI or Visual Studio.

### Python

Use pip to install the [msedge-selenium-tools](https://pypi.org/project/msedge-selenium-tools/) package:

```
pip install msedge-selenium-tools
```

# Examples

## Basic Usage

To use with Microsoft Edge (EdgeHTML), simply create a default instance of the Edge driver class in your language of choice. All of the same methods and properties that are currently available in Selenium 3 will continue to work:

### C#
```csharp
var driver = new EdgeDriver();
```

### Python
```python
driver = Edge()
```

## Driving Microsoft Edge (Chromium)

To use with Microsoft Edge (Chromium) instead, create a new ``Edge`` driver and pass in an ``EdgeOptions`` instance with the ``UseChromium`` property set to true:

### C#
```csharp
var options = new EdgeOptions();
options.UseChromium = true;

var driver = new EdgeDriver(options);
```

### Python
```python
options = EdgeOptions()
options.use_chromium = True

driver = Edge(options)
```

## Choosing Specific Browser Binaries (Chromium-Only)

Use ``EdgeOptions`` to choose a specific binary. This is useful for testing [Microsoft Edge preview channels](https://www.microsoftedgeinsider.com/) such as Microsoft Edge Beta.

### C#
```csharp
var options = new EdgeOptions();
options.UseChromium = true;
options.BinaryLocation = @"C:\Program Files (x86)\Microsoft\Edge Beta\Application\msedge.exe";

var driver = new EdgeDriver(options);
```

### Python
```python
options = EdgeOptions()
options.use_chromium = True
options.binary_location = r"C:\Program Files (x86)\Microsoft\Edge Beta\Application\msedge.exe"

driver = Edge(options)
```

## Customizing the Edge Driver Service

### C#

When an ``EdgeDriver`` instance is created using ``EdgeOptions``, it automatically creates and launches the appropriate ``EdgeDriverService`` for either Microsoft Edge (EdgeHTML) or Microsoft Edge (Chromium).

If you want to create an `EdgeDriverService`, create one configured for Microsoft Edge \(Chromium\) using the `CreateChromiumService()` method. You may find it useful for additional customizations like enabling verbose log output in the following code.

```csharp
using (var service = EdgeDriverService.CreateChromiumService())
{
    service.UseVerboseLogging = true;

    var driver = new EdgeDriver(service);
}
```

> [!NOTE]
> You do not need to provide the `EdgeOptions` object when passing the `EdgeDriver` instance the `EdgeDriverService`.  `EdgeDriver` uses the default options for either Microsoft Edge \(EdgeHTML\) or Microsoft Edge \(Chromium\) depending on what kind of service you provide.  
> 
> However, if you want to provide both an `EdgeDriverService` and `EdgeOptions`, you must ensure that both are configured for the same version of Microsoft Edge.  For example, it is not possible to use a default Microsoft Edge \(EdgeHTML\) `EdgeDriverService` and Chromium properties in `EdgeOptions`.  The `EdgeDriver ` will throw an error to prevent this.

### Python

When using Python, the ``Edge`` object creates and manages the ``EdgeService`` for you. To configure the ``EdgeService``, you may pass additional arguments to the ``Edge`` object:

```python
service_args = ['--verbose']
driver = Edge(service_args = service_args)
```

## Using Chromium-Specific Options

Using `EdgeOptions` with the `UseChromium` property set to `true` gives you access to all of the same methods and properties that are available in the [ChromeOptions][SeleniumWebDriverChromeoptionsClass] class in Selenium.  For example, just like with other Chromium browsers, use the `EdgeOptions.AddArguments()` method to run Microsoft Edge \(Chromium\) in [headless mode][WikiHeadlessBrowser] in the following code.  

### C#
```csharp
var options = new EdgeOptions();
options.UseChromium = true;
options.AddArgument("headless");
options.AddArgument("disable-gpu");
```

### Python
```python
options = EdgeOptions()
options.use_chromium = True
options.add_argument('headless')
options.add_argument('disable-gpu')
```

> [!NOTE]
> These [Chromium-specific properties and methods][SeleniumWebDriverChromeoptionsClass] are always available but have no effect if the `UseChromium` property is not set to `true`.  Similarly, existing properties and methods meant for Microsoft Edge \(EdgeHTML\) have no effect if `UseChromium` is set to `true`.  

# Contributing

We are glad you are interested in automating the latest Microsoft Edge browser and improving the automation experience for the rest of the community!

Before you begin, please read & follow our [Contributor's Guide](CONTRIBUTING.md). Consider also contributing your feature or bug fix directly to [Selenium](https://github.com/SeleniumHQ/selenium) so that it will be included in future Selenium releases.

# Code of Conduct

This project has adopted the [Microsoft Open Source Code of Conduct][conduct-code].
For more information see the [Code of Conduct FAQ][conduct-FAQ] or contact [opencode@microsoft.com][conduct-email] with any additional questions or comments.

[conduct-code]: https://opensource.microsoft.com/codeofconduct/
[conduct-FAQ]: https://opensource.microsoft.com/codeofconduct/faq/
[conduct-email]: mailto:opencode@microsoft.com
