// Copyright 2020 Microsoft
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License. 
const assert = require('assert');
const edge = require("../lib/edge");

describe('JS selenium binding tests', function () {
    this.timeout(0);

    it('test default', async function () {
        let driver = await edge.Driver.createSession();

        let cap = await driver.getCapabilities();
        await assert.equal(cap.get('browserName'), 'MicrosoftEdge');

        await driver.quit();
    });

    it('test legacy edge', async function () {
        let options = await new edge.Options().setEdgeChromium(false);
        let driver = await edge.Driver.createSession(options);

        let cap = await driver.getCapabilities();
        await assert.equal(cap.get('browserName'), 'MicrosoftEdge');

        await driver.quit();
    });

    it('test chromium edge', async function () {
        let options = await new edge.Options().setEdgeChromium(true);
        let driver = await edge.Driver.createSession(options);

        let cap = await driver.getCapabilities();
        await assert.equal(cap.get('browserName'), 'msedge');

        await driver.quit();
    });

    it('test legacy options to capabilities', async function () {
        let options = await new edge.Options();
        let cap = await options.toCapabilities();
        await assert.equal(cap.get('browserName'), 'MicrosoftEdge');
        await assert.equal(cap.has('ms:edgeOptions'), false);
        await assert.equal(cap.has('ms:edgeChromium'), true);
        await assert.equal(cap.get('ms:edgeChromium'), false);
    });

    it('test chromium options to capabilities', async function () {
        let options = await new edge
            .Options()
            .setEdgeChromium(true);
        let cap = await options.toCapabilities();
        await assert.equal(cap.get('browserName'), 'MicrosoftEdge');
        await assert.equal(cap.has('ms:edgeOptions'), true);
        await assert.equal(cap.get('ms:edgeOptions').getEdgeChromium(), true);
        await assert.equal(cap.has('ms:edgeChromium'), true);
        await assert.equal(cap.get('ms:edgeChromium'), true);
    });
});

