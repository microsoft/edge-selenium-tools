const assert = require('assert');
const edge = require("../edge");
const {Builder, By, Key, until} = require('..');

describe('JS selenium binding tests', function() {
    this.timeout(10000);

    it('test default', async function(){
        try{
        let driver = await new Builder()
            .forBrowser('chrome')
            .build();

        let cap = await driver.getCapabilities();
        await assert.equal(cap.get('browserName'), 'chrome');
        
        driver.quit();
        }catch(e){
            console.log(e);
        }
    });

    it('test default', async function(){
        let driver = await new Builder()
            .forBrowser('MicrosoftEdge')
            .build();

        let cap = await driver.getCapabilities();
        await assert.equal(cap.get('browserName'), 'MicrosoftEdge');
        
        driver.quit();
    });

    it('test legacy edge', async function(){
        try{
        let options = await new edge.Options();
        await options.setEdgeChromium(false);

        let driver = await new Builder()
            .forBrowser('MicrosoftEdge')
            .setEdgeOptions(options)
            .build();

        let cap = await driver.getCapabilities();
        await assert.equal(cap.get('browserName'), 'MicrosoftEdge');
        
        driver.quit();
        }catch(e){
            console.log(e);
        }
    });

    it('test chromium edge', async function(){
        try{
        let options = await new edge
            .Options()
            .setEdgeChromium(true);

        let driver = await new Builder()
            .forBrowser('MicrosoftEdge')
            .setEdgeOptions(options)
            .build();

        let cap = await driver.getCapabilities();
        await assert.equal(cap.get('browserName'), 'msedge');

        await driver.quit();
        }catch(e){
            console.log(e);
        }
    });
});

