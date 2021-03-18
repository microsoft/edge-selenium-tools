# Selenium Tools for Microsoft Edge

[![Build Status](https://dev.azure.com/ms/edge-selenium-tools/_apis/build/status/microsoft.edge-selenium-tools?branchName=master)](https://dev.azure.com/ms/edge-selenium-tools/_build/latest?definitionId=345&branchName=master)

Selenium Tools for Microsoft Edge extends [Selenium 3](https://www.selenium.dev/) with a unified driver to help you write automated tests for both the Microsoft Edge (EdgeHTML) and new Microsoft Edge (Chromium) browsers.

The libraries included in this project are fully compatible with Selenium's built-in Edge libraries, and run Microsoft Edge (EdgeHTML) by default so you can use our project as a seamless drop-in replacement. In addition to being compatible with your existing Selenium tests, Selenium Tools for Microsoft Edge gives you the ability to drive the new Microsoft Edge (Chromium) browser and unlock all of the latest functionality!

The classes in this package are based on the existing ``Edge`` and ``Chrome`` driver classes included in the [Selenium](https://github.com/SeleniumHQ/selenium) project.

## Before you Begin

The Selenium Tools for Microsoft Edge is a solution for developers who prefer to remain on Selenium 3 which is the current stable release and developers who have existing browser tests and want to add coverage for the new Microsoft Edge (Chromium) browser without changing the Selenium version.

The very same ``Edge`` driver classes provided in this package are included in Selenium 4 and are already available today in the latest Selenium 4 Alpha release. If you are able to upgrade to Selenium 4 Alpha, there is no need to use this package as Selenium should already have everything you need built in!

## Getting Started

### Downloading Driver Executables

You will need the correct [WebDriver executable][webdriver-download] for the version of Microsoft Edge you want to drive. The executables are not included with this package. WebDriver executables for all supported versions of Microsoft Edge are available for download [here][webdriver-download]. For more information, and instructions on downloading the correct driver for your browser, see the [Microsoft Edge WebDriver documentation][webdriver-chromium-docs].

### Installation

Selenium Tools for Microsoft Edge depends on the official Selenium 3 package to run. You will need to ensure that both Selenium 3 and the Tools and included in your project.

#### C#

Add the [Microsoft.Edge.SeleniumTools](https://www.nuget.org/packages/Microsoft.Edge.SeleniumTools) and [Selenium.WebDriver](https://www.nuget.org/packages/Selenium.WebDriver/3.141.0) packages to your .NET project using the NuGet CLI or Visual Studio.

#### JavaScript

```
npm install @microsoft/edge-selenium-tools
```

#### Java

Add [msedge-selenium-tools-java](https://search.maven.org/search?q=a:msedge-selenium-tools-java) to your project using Maven:

```xml
<dependencies>
    <dependency>
        <groupId>com.microsoft.edge</groupId>
        <artifactId>msedge-selenium-tools-java</artifactId>
        <version>3.141.0</version>
    </dependency>
</dependencies>
```

The Java package is also available for download on the [Releases](https://github.com/microsoft/edge-selenium-tools/releases/latest) page.

#### Python

Use pip to install the [msedge-selenium-tools](https://pypi.org/project/msedge-selenium-tools/) and [selenium](https://pypi.org/project/selenium/3.141.0/) packages:

```
pip install msedge-selenium-tools selenium==3.141
```

## Example Code

See the [Microsoft Edge WebDriver documentation][webdriver-chromium-docs] for lots more information on using Microsoft Edge (Chromium) with WebDriver.

### C#

```csharp
using Microsoft.Edge.SeleniumTools;

// Launch Microsoft Edge (EdgeHTML)
var driver = new EdgeDriver();

// Launch Microsoft Edge (Chromium)
var options = new EdgeOptions();
options.UseChromium = true;
var driver = new EdgeDriver(options);
```

### Java

Import `EdgeDriver` from the `com.microsoft.edge.seleniumtools` package to launch Microsoft Edge (Chromium). The `com.microsoft.edge.seleniumtools` package supports **Chromium** only. Use the official Selenium 3 package `org.openqa.selenium.edge` to launch **EdgeHTML**.

```java
import com.microsoft.edge.seleniumtools.EdgeDriver;

// Launch Microsoft Edge (Chromium)
EdgeDriver driver = new EdgeDriver();
```

### JavaScript

```js
const edge = require("@microsoft/edge-selenium-tools");

// Launch Microsoft Edge (EdgeHTML)
let driver = edge.Driver.createSession();

// Launch Microsoft Edge (Chromium)
let options = new edge.Options().setEdgeChromium(true);
let driver = edge.Driver.createSession(options);
```

### Python

```python
from msedge.selenium_tools import Edge, EdgeOptions

# Launch Microsoft Edge (EdgeHTML)
driver = Edge()

# Launch Microsoft Edge (Chromium)
options = EdgeOptions()
options.use_chromium = True
driver = Edge(options = options)
```

## Contributing

We are glad you are interested in automating the latest Microsoft Edge browser and improving the automation experience for the rest of the community!

Before you begin, please read & follow our [Contributor's Guide](CONTRIBUTING.md). Consider also contributing your feature or bug fix directly to [Selenium](https://github.com/SeleniumHQ/selenium) so that it will be included in future Selenium releases.

## Code of Conduct

This project has adopted the [Microsoft Open Source Code of Conduct][conduct-code].
For more information see the [Code of Conduct FAQ][conduct-FAQ] or contact [opencode@microsoft.com][conduct-email] with any additional questions or comments.

[webdriver-download]: https://developer.microsoft.com/en-us/microsoft-edge/tools/webdriver/
[webdriver-chromium-docs]: https://docs.microsoft.com/en-us/microsoft-edge/webdriver-chromium
[conduct-code]: https://opensource.microsoft.com/codeofconduct/
[conduct-FAQ]: https://opensource.microsoft.com/codeofconduct/faq/
[conduct-email]: mailto:opencode@microsoft.com
