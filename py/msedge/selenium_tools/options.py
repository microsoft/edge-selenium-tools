# Portions Copyright Microsoft 2020
# Licensed under the Apache License, Version 2.0
#
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

import base64
import os
import platform
import warnings

from selenium.webdriver.common.desired_capabilities import DesiredCapabilities


class Options(object):
    KEY = "ms:edgeOptions"

    def __init__(self):
        self._page_load_strategy = "normal"
        self._binary_location = ''
        self._arguments = []
        self._extension_files = []
        self._extensions = []
        self._experimental_options = {}
        self._debugger_address = None
        self._caps = DesiredCapabilities.EDGE.copy()
        self._use_chromium = False
        self._use_webview = False
    
    @property
    def use_chromium(self):
        return self._use_chromium

    @use_chromium.setter
    def use_chromium(self, value):
        self._use_chromium = bool(value)

    @property
    def use_webview(self):
        return self._use_webview

    @use_webview.setter
    def use_webview(self, value):
        self._use_webview = bool(value)

    @property
    def page_load_strategy(self):
        return self._page_load_strategy

    @page_load_strategy.setter
    def page_load_strategy(self, value):
        if value not in ['normal', 'eager', 'none']:
            raise ValueError("Page Load Strategy should be 'normal', 'eager' or 'none'.")
        self._page_load_strategy = value

    @property
    def capabilities(self):
        return self._caps

    def set_capability(self, name, value):
        """Sets a capability."""
        self._caps[name] = value
    
    def to_capabilities(self):
        """
            Creates a capabilities with all the options that have been set and

            returns a dictionary with everything
        """
        if self.use_chromium:
            caps = self._caps
            if self._use_webview:
                caps['browserName'] = 'webview2'
            edge_options = self.experimental_options.copy()
            edge_options["extensions"] = self.extensions
            if self.binary_location:
                edge_options["binary"] = self.binary_location
            edge_options["args"] = self.arguments
            if self.debugger_address:
                edge_options["debuggerAddress"] = self.debugger_address

            caps[self.KEY] = edge_options
        else:
            caps = self._caps
            caps['pageLoadStrategy'] = self._page_load_strategy
        caps['ms:edgeChromium'] = self.use_chromium
        return caps

    @property
    def binary_location(self):
        """
        Returns the location of the binary otherwise an empty string
        """
        return self._binary_location

    @binary_location.setter
    def binary_location(self, value):
        """
        Allows you to set where the edge binary lives

        :Args:
         - value: path to the edge binary
        """
        self._binary_location = value

    @property
    def debugger_address(self):
        """
        Returns the address of the remote devtools instance
        """
        return self._debugger_address

    @debugger_address.setter
    def debugger_address(self, value):
        """
        Allows you to set the address of the remote devtools instance
        that the EdgeDriver instance will try to connect to during an
        active wait.

        :Args:
         - value: address of remote devtools instance if any (hostname[:port])
        """
        self._debugger_address = value

    @property
    def arguments(self):
        """
        Returns a list of arguments needed for the browser
        """
        return self._arguments

    def add_argument(self, argument):
        """
        Adds an argument to the list

        :Args:
         - Sets the arguments
        """
        if argument:
            self._arguments.append(argument)
        else:
            raise ValueError("argument can not be null")

    @property
    def extensions(self):
        """
        Returns a list of encoded extensions that will be loaded into edge

        """
        encoded_extensions = []
        for ext in self._extension_files:
            file_ = open(ext, 'rb')
            # Should not use base64.encodestring() which inserts newlines every
            # 76 characters (per RFC 1521).  Edgedriver has to remove those
            # unnecessary newlines before decoding, causing performance hit.
            encoded_extensions.append(base64.b64encode(file_.read()).decode('UTF-8'))

            file_.close()
        return encoded_extensions + self._extensions

    def add_extension(self, extension):
        """
        Adds the path to the extension to a list that will be used to extract it
        to the EdgeDriver

        :Args:
         - extension: path to the \*.crx file
        """
        if extension:
            extension_to_add = os.path.abspath(os.path.expanduser(extension))
            if os.path.exists(extension_to_add):
                self._extension_files.append(extension_to_add)
            else:
                raise IOError("Path to the extension doesn't exist")
        else:
            raise ValueError("argument can not be null")

    def add_encoded_extension(self, extension):
        """
        Adds Base64 encoded string with extension data to a list that will be used to extract it
        to the EdgeDriver

        :Args:
         - extension: Base64 encoded string with extension data
        """
        if extension:
            self._extensions.append(extension)
        else:
            raise ValueError("argument can not be null")

    @property
    def experimental_options(self):
        """
        Returns a dictionary of experimental options for edge.
        """
        return self._experimental_options

    def add_experimental_option(self, name, value):
        """
        Adds an experimental option which is passed to edge.

        Args:
          name: The experimental option name.
          value: The option value.
        """
        self._experimental_options[name] = value

    @property
    def headless(self):
        """
        Returns whether or not the headless argument is set
        """
        return '--headless' in self._arguments

    @headless.setter
    def headless(self, value):
        """
        Sets the headless argument

        Args:
          value: boolean value indicating to set the headless option
        """
        args = {'--headless'}
        if platform.system().lower() == 'windows':
            args.add('--disable-gpu')
        if value is True:
            self._arguments.extend(args)
        else:
            self._arguments = list(set(self._arguments) - args)

    def set_headless(self, headless=True):
        """ Deprecated, options.headless = True """
        warnings.warn('use setter for headless property instead of set_headless',
                      DeprecationWarning, stacklevel=2)
        self.headless = headless