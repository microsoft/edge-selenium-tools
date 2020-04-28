# Licensed to the Software Freedom Conservancy (SFC) under one
# or more contributor license agreements.  See the NOTICE file
# distributed with this work for additional information
# regarding copyright ownership.  The SFC licenses this file
# to you under the Apache License, Version 2.0 (the
# "License"); you may not use this file except in compliance
# with the License.  You may obtain a copy of the License at
#
#   http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing,
# software distributed under the License is distributed on an
# "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
# KIND, either express or implied.  See the License for the
# specific language governing permissions and limitations
# under the License.

import unittest
import sys
import webdriver

class EdgeDriverTest(unittest.TestCase):
    def use_legacy_edge(self, driver=None):
        if not driver: return
        options = driver.create_options()
        cap = options.to_capabilities
        self.assertEqual('MicrosoftEdge', cap['browserName'], 'Driver launches Edge Legacy.')
    
    def use_chromium_edge(self, driver=None):
        if not driver: return
        options = driver.create_options()
        cap = options.to_capabilities()
        self.assertEqual('msedge', cap['browserName'], 'Driver launches Edge Chromium.')

        result = driver.execute_cdp_cmd('Browser.getVersion', {})
        self.assertTrue('userAgent' in result, 'Driver can send Chromium-specific commands.')
    
    def test_default(self):
        driver = webdriver.Edge()
        try:
            self.use_legacy_edge(driver)
        except:
            pass
        else:
            driver.quit()
    
    def test_lagacy_options(self):
        driver = webdriver.Edge(use_chromium=False)
        try:
            self.use_legacy_edge(driver)
        except:
            pass
        else:
            driver.quit()

    def test_chromium_options(self):
        driver = webdriver.Edge(use_chromium=True)
        try:
            self.use_chromium_edge(driver)
        except:
            pass
        else:
            driver.quit()

    def test_chromium_driver_with_legacy_options(self):
        options = webdriver.EdgeOptions()
        try:
            driver = webdriver.Edge(options=options)
            self.assertRaises(Exception)
        except Exception as e:
            self.assertEqual('options.use_chromium must be set to true when using an Edge Chromium driver service.', e.args[0])
        else:
            driver.quit()
    
    def test_chromium_driver_with_chromium_options(self):
        options = webdriver.EdgeOptions(use_chromium=True)
        try:
            driver = webdriver.Edge(options=options)
        except:
            pass
        else:
            driver.quit()
    
    def test_legacy_driver_with_legacy_options(self):
        options = webdriver.EdgeOptions()
        try:
            driver =  webdriver.Edge(options=options)
        except:
            pass
        else:
            driver.quit()

    def test_legacy_driver_with_chromium_options(self):
        options = webdriver.EdgeOptions(use_chromium=True)
        try:
            driver = webdriver.Edge(options=options)
            self.assertRaises(Exception)
        except Exception as e:
            self.assertEqual('options.use_chromium must be set to false when using an Edge Legacy driver service.', e.args[0])
        else:
            driver.quit()

    def test_chromium_options_to_capabilities(self):
        options = webdriver.EdgeOptions(use_chromium = True)
        options._page_load_strategy = 'eager'            # common
        options._debugger_address = 'localhost:9222'     # chromium only

        cap = options.to_capabilities()
        self.assertEqual('MicrosoftEdge', cap['browserName'])
        self.assertIn('ms:edgeOptions', cap)

        edge_options_dict = cap['ms:edgeOptions']
        self.assertIsNotNone(edge_options_dict)
        self.assertEqual('localhost:9222', edge_options_dict['debuggerAddress'])

    def test_legacy_options_to_capabilities(self):
        options = webdriver.EdgeOptions(use_chromium = False)
        options._page_load_strategy = 'eager'            # common
        options._debugger_address = 'localhost:9222'     # chromium only

        cap = options.to_capabilities()
        self.assertEqual('MicrosoftEdge', cap['browserName'])
        self.assertEqual('eager', cap['pageLoadStrategy'])
        self.assertFalse('ms:edgeOptions' in cap)

if __name__=='__main__':
    unittest.main()