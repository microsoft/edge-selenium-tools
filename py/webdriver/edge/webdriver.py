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
import warnings

from selenium.webdriver.common import utils
from selenium.webdriver.remote.webdriver import WebDriver as RemoteWebDriver
from selenium.webdriver.remote.remote_connection import RemoteConnection
from .remote_connection import EdgeRemoteConnection
from .service import Service
from .options import Options


class WebDriver(RemoteWebDriver):

    def __init__(self, executable_path='MicrosoftWebDriver.exe',
                 capabilities=None, port=0, verbose=False, service_log_path=None,
                 log_path=None, keep_alive=False,
                 desired_capabilities=None, service_args=None,
                 options=None, edge_options=None,  use_chromium=False):
        """
        Creates a new instance of the edge driver.

        Starts the service and then creates new instance of edge driver.

        :Args:
         - executable_path - path to the executable. If the default is used it assumes the executable is in the $PATH
         - capabilities - Dictionary object with non-browser specific
           capabilities only, such as "proxy" or "loggingPref".
         - port - port you would like the service to run, if left as 0, a free port will be found.
         - verbose - whether to set verbose logging in the service
         - service_log_path - Where to log information from the driver.
         - log_path: Deprecated argument for service_log_path
         - keep_alive - Whether to configure EdgeRemoteConnection to use HTTP keep-alive.
         - desired_capabilities - Dictionary object with non-browser specific
           capabilities only, such as "proxy" or "loggingPref".
         - service_args - List of args to pass to the driver service
         - options - this takes an instance of EdgeOptions
         - edge_options - Deprecated argument for options
         - use_chromium - Whether to use chromium edge driver, default to use legacy edge driver
         """
        self.use_chromium = use_chromium

        if edge_options:
            warnings.warn('use options instead of edge_options',
                          DeprecationWarning, stacklevel=2)
            options = edge_options

        # options use_chromium flag validation
        if self.use_chromium and options and not options.use_chromium:
            raise Exception(
                'options.use_chromium must be set to true when using an Edge Chromium driver service.')
        if not self.use_chromium and options and options.use_chromium:
            raise Exception(
                'options.use_chromium must be set to false when using an Edge Legacy driver service.')

        if self.use_chromium:
            if options is None:
                # desired_capabilities stays as passed in
                if desired_capabilities is None:
                    desired_capabilities = self.create_options().to_capabilities()
            else:
                if desired_capabilities is None:
                    desired_capabilities = options.to_capabilities()
                else:
                    desired_capabilities.update(options.to_capabilities())

        if log_path:
            warnings.warn('use service_log_path instead of log_path',
                          DeprecationWarning, stacklevel=2)
            service_log_path = log_path

        self.port = port
        if self.port == 0:
            self.port = utils.free_port()

        if self.use_chromium:
            self.service = Service(
                executable_path,
                port=port,
                verbose=verbose,
                service_args=service_args,
                log_path=service_log_path)
            self.service.start()
        else:
            self.edge_service = Service(
                executable_path, 
                port=self.port, 
                verbose=verbose, 
                log_path=service_log_path)
            self.edge_service.start()

        try:
            if self.use_chromium:
                RemoteWebDriver.__init__(
                    self,
                    command_executor=EdgeRemoteConnection(
                        remote_server_addr=self.service.service_url,
                        keep_alive=keep_alive),
                    desired_capabilities=desired_capabilities)
            else:
                RemoteWebDriver.__init__(
                    self,
                    command_executor=RemoteConnection(
                        remote_server_addr='http://localhost:%d' % self.port,
                        resolve_ip=False,
                        keep_alive=keep_alive),
                    desired_capabilities=capabilities)
        except Exception:
            self.quit()
            raise
        self._is_remote = False

    def launch_app(self, id):
        """Launches Edge app specified by id."""
        return self.execute("launchApp", {'id': id})

    def get_network_conditions(self):
        """
        Gets Edge network emulation settings.

        :Returns:
            A dict. For example:

            {'latency': 4, 'download_throughput': 2, 'upload_throughput': 2,
            'offline': False}

        """
        return self.execute("getNetworkConditions")['value']

    def set_network_conditions(self, **network_conditions):
        """
        Sets Edge network emulation settings.

        :Args:
         - network_conditions: A dict with conditions specification.

        :Usage:
            driver.set_network_conditions(
                offline=False,
                latency=5,  # additional latency (ms)
                download_throughput=500 * 1024,  # maximal throughput
                upload_throughput=500 * 1024)  # maximal throughput

            Note: 'throughput' can be used to set both (for download and upload).
        """
        self.execute("setNetworkConditions", {
            'network_conditions': network_conditions
        })

    def execute_cdp_cmd(self, cmd, cmd_args):
        """
        Execute Edge Devtools Protocol command and get returned result

        The command and command args should follow Edge devtools protocol domains/commands, refer to link
        https://chromedevtools.github.io/devtools-protocol/

        :Args:
         - cmd: A str, command name
         - cmd_args: A dict, command args. empty dict {} if there is no command args

        :Usage:
            driver.execute_cdp_cmd('Network.getResponseBody', {'requestId': requestId})

        :Returns:
            A dict, empty dict {} if there is no result to return.
            For example to getResponseBody:

            {'base64Encoded': False, 'body': 'response body string'}

        """
        return self.execute("executeCdpCommand", {'cmd': cmd, 'params': cmd_args})['value']

    def quit(self):
        """
        Closes the browser and shuts down the EdgeDriver executable
        that is started when starting the EdgeDriver
        """
        try:
            RemoteWebDriver.quit(self)
        except Exception:
            # We don't care about the message because something probably has gone wrong
            pass
        if self.use_chromium:
            self.service.stop()
        else:
            self.edge_service.stop()

    def create_options(self):
        return Options(self.use_chromium)
