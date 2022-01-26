# DEPRECATED: Selenium Tools for Microsoft Edge

<a name="deprecation"></a>
:warning: **This project is no longer maintained. Please uninstall Selenium Tools for Microsoft Edge and upgrade to [Selenium 4](https://www.selenium.dev/) which has built-in support for Microsoft Edge (Chromium). For help upgrading your Selenium 3 browser tests to Selenium 4, see Selenium's guide [here](https://www.selenium.dev/documentation/webdriver/getting_started/upgrade_to_selenium_4/).** :warning:

This repository will remain available as an example, and for users that have not yet had a chance to upgrade. However, there will be no further activity on issues or pull requests. The [@EdgeDevTools](https://twitter.com/EdgeDevTools) team will continue to work with the Selenium project to contribute future Microsoft Edge Driver features and bug fixes directly to Selenium 4.

* * * 

[![Build Status](https://dev.azure.com/ms/edge-selenium-tools/_apis/build/status/microsoft.edge-selenium-tools?branchName=master)](https://dev.azure.com/ms/edge-selenium-tools/_build/latest?definitionId=345&branchName=master)

Selenium Tools for Microsoft Edge extends [Selenium 3](https://github.com/SeleniumHQ/selenium/releases/tag/selenium-3.141.59) with a unified driver to help you write automated tests for both the Microsoft Edge (EdgeHTML) and new Microsoft Edge (Chromium) browsers.

The libraries included in this project are fully compatible with Selenium's built-in Edge libraries, and run Microsoft Edge (EdgeHTML) by default so you can use our project as a seamless drop-in replacement. In addition to being compatible with your existing Selenium tests, Selenium Tools for Microsoft Edge gives you the ability to drive the new Microsoft Edge (Chromium) browser and unlock all of the latest functionality!

The classes in this package are based on the existing ``Edge`` and ``Chrome`` driver classes included in the [Selenium](https://github.com/SeleniumHQ/selenium) project.

## Before you Begin

Selenium Tools for Microsoft Edge was created as a compatiblity solution for developers who have existing Selenium 3 browser tests and want to add coverage for the latest Microsoft Edge (Chromium) browser. The [Microsoft Edge Developer Tools Team](https://twitter.com/EdgeDevTools) recommends using Selenium 4 instead because Selenium 4 has built-in support for Microsoft Edge (Chromium). If you are able to upgrade your existing tests, or write new tests using Selenium 4, then there is no need to use this package as Selenium should already have everything you need built in!

See Selenium's upgrade [guide](https://www.selenium.dev/documentation/webdriver/getting_started/upgrade_to_selenium_4/) for help with upgrading from Selenium 3 to Selenium 4. If you are unable to upgrade due to a compatibility issues, please consider opening an issue in the official Selenium GitHub repo [here](https://github.com/SeleniumHQ/selenium/issues). If you have determined that you cannot upgrade from Selenium 3 at this time, and would still like to add test coverage for Microsoft Edge (Chromium) to your project, see the steps in the section below.

## Getting Started

### Downloading Driver Executables

You will need the correct [WebDriver executable][webdriver-download] for the version of Microsoft Edge you want to drive. The executables are not included with this package. WebDriver executables for all supported versions of Microsoft Edge are available for download [here][webdriver-download]. For more information, and instructions on downloading the correct driver for your browser, see the [Microsoft Edge WebDriver documentation][webdriver-chromium-docs].

### Installation

Selenium Tools for Microsoft Edge depends on the official Selenium 3 package to run. You will need to ensure that both Selenium 3 and the Tools and included in your project.

Use pip to install the [msedge-selenium-tools](https://pypi.org/project/msedge-selenium-tools/) and [selenium](https://pypi.org/project/selenium/3.141.0/) packages:

```
pip install msedge-selenium-tools selenium==3.141
```

## Example Code

See the [Microsoft Edge WebDriver documentation][webdriver-chromium-docs] for lots more information on using Microsoft Edge (Chromium) with WebDriver.

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
