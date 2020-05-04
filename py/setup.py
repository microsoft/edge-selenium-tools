# Copyright 2020 Microsoft
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#   http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

from setuptools import setup

setup(
    name = 'msedge-selenium-tools',
    version = '3.141.0',
    description = 'An updated EdgeDriver implementation for Selenium 3 with newly-added support for Microsoft Edge (Chromium).',
    url = 'https://github.com/microsoft/edge-selenium-tools',
    author = 'Microsoft Corporation',
    author_email = 'EdgeDevToolsOSS@microsoft.com',
    license = 'Apache 2.0',
    packages = [
        'msedge', 
        'msedge.selenium_tools'
        ],
    install_requires = [
        "selenium==3.141"
    ],
    zip_safe = False
)