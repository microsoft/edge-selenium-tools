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

import unittest
import sys
import os

sys.path.insert(1, os.path.join(sys.path[0], '..'))
from msedge.selenium_tools import Edge, EdgeOptions, EdgeService

class EdgeDriverTest(unittest.TestCase):

    @unittest.skip(reason="Edge Legacy is not available on Azure hosted environment.")
    def test_default(self):
        try:
            driver = Edge()
            cap = driver.capabilities
            self.assertEqual('MicrosoftEdge', cap['browserName'], 'Driver launches Edge Legacy.')
        except:
            self.assertTrue(False, 'Test default options failed.')
        else:
            driver.quit()

    @unittest.skip(reason="Edge Legacy is not available on Azure hosted environment.")
    def test_legacy_options(self):
        try:
            options = EdgeOptions()
            options.use_chromium = False
            driver = Edge(options = options)
            cap = driver.capabilities
            self.assertEqual('MicrosoftEdge', cap['browserName'], 'Driver launches Edge Legacy.')
        except:
            self.assertTrue(False, 'Test legacy options failed.')
        else:
            driver.quit()

    def test_chromium_options(self):
        try:
            options = EdgeOptions()
            options.use_chromium = True
            driver = Edge(options = options)
            cap = driver.capabilities
            self.assertEqual('msedge', cap['browserName'], 'Driver launches Edge Chromium.')

            result = driver.execute_cdp_cmd('Browser.getVersion', {})
            self.assertTrue('userAgent' in result, 'Driver can send Chromium-specific commands.')
        except:
            self.assertTrue(False, 'Test chromium options failed.')
        else:
            driver.quit()

    def test_chromium_driver_with_chromium_options(self):
        options = EdgeOptions()
        options.use_chromium = True
        try:
            driver = Edge('msedgedriver.exe', options=options)
        except:
            self.assertTrue(False, 'Test chromium driver with chromium options failed.')
        else:
            driver.quit()

    @unittest.skip(reason="Edge Legacy is not available on Azure hosted environment.")
    def test_legacy_driver_with_legacy_options(self):
        options = EdgeOptions()
        try:
            driver =  Edge('MicrosoftWebDriver.exe', options=options)
        except Exception as e:
            self.assertTrue(False, 'Test legacy driver with legacy options failed.')
        else:
            driver.quit()

    def test_chromium_options_to_capabilities(self):
        options = EdgeOptions()
        options.use_chromium = True
        options._page_load_strategy = 'eager'            # common
        options._debugger_address = 'localhost:9222'     # chromium only

        cap = options.to_capabilities()
        self.assertEqual('MicrosoftEdge', cap['browserName'])
        self.assertIn('ms:edgeOptions', cap)
        self.assertTrue(cap['ms:edgeChromium'])

        edge_options_dict = cap['ms:edgeOptions']
        self.assertIsNotNone(edge_options_dict)
        self.assertEqual('localhost:9222', edge_options_dict['debuggerAddress'])

    def test_legacy_options_to_capabilities(self):
        options = EdgeOptions()
        options._page_load_strategy = 'eager'            # common
        options._debugger_address = 'localhost:9222'     # chromium only

        cap = options.to_capabilities()
        self.assertEqual('MicrosoftEdge', cap['browserName'])
        self.assertEqual('eager', cap['pageLoadStrategy'])
        self.assertFalse('ms:edgeOptions' in cap)
        self.assertFalse(cap['ms:edgeChromium'])

    def test_webview_options_to_capabilities(self):
        options = EdgeOptions()
        options.use_chromium = True
        options.use_webview = True

        cap = options.to_capabilities()
        self.assertEqual('webview2', cap['browserName'])

if __name__=='__main__':
    unittest.main()