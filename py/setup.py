from setuptools import setup

setup(
    name = 'edgeseleniumtools',
    version = '0.1',
    description = 'The package is to extend selenium 3 for both legacy and chromium edge',
    url = 'https://github.com/microsoft/edge-selenium-tools',
    author = 'Microsoft',
    author_email = 'guangyue.xu@microsoft.com',
    license = 'Apache 2.0',
    packages = [
        'edgeseleniumtools', 
        'edgeseleniumtools.webdriver', 
        'edgeseleniumtools.webdriver.edge',
        'edgeseleniumtools.test'
        ],
    install_requires = [
        "selenium==3.141"
    ],
    zip_safe = False
)