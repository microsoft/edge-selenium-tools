const assert = require('assert');
const edge = require("../edge");

describe('JS selenium binding tests', function() {
    this.timeout(10000);

    it('test default', async function(){
        let service = await new edge.ServiceBuilder().build();
        let options = await new edge.Options();
        let driver = await edge.Driver.createSession(options, service);

        let cap = await driver.getCapabilities();
        await assert.equal(cap.get('browserName'), 'MicrosoftEdge');
        
        await driver.quit();
    });

    it('test legacy edge', async function(){
        let service = await new edge.ServiceBuilder().build();
        let options = await new edge.Options().setEdgeChromium(false);
        let driver = await edge.Driver.createSession(options, service);

        let cap = await driver.getCapabilities();
        await assert.equal(cap.get('browserName'), 'MicrosoftEdge');
        
        await driver.quit();
    });

    it('test chromium edge2', async function(){
        let service = await new edge.ServiceBuilder('msedgedriver.exe').build();
        let options = await new edge.Options().setEdgeChromium(true);
        let driver = await edge.Driver.createSession(options, service);

        let cap = await driver.getCapabilities();
        await assert.equal(cap.get('browserName'), 'msedge');

        await driver.quit();
    });

    it('test legacy options to capabilities', async function(){
        let options = await new edge.Options();
        let cap = await options.toCapabilities();
        await assert.equal(cap.get('browserName'), 'MicrosoftEdge');
        await assert.equal(cap.has('ms:edgeOptions'), false);
        await assert.equal(cap.has('ms:edgeChromium'), true);
        await assert.equal(cap.get('ms:edgeChromium'), false);
    });

    it('test chromium options to capabilities', async function(){
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

