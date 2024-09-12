(function () {
    var system = require('system');
    var fs = require('fs');
    var config = {
        MARKED: 'marked.js',
    }, render, params;


    parseParams = function () {
        var map = {}, i, key;
        for (i = 0; i < system.args.length; i += 1) {
            if (system.args[i].charAt(0) === '-') {
                key = system.args[i].substr(1, i.length);
                map[key] = system.args[i + 1];
            }
        }
        params = map;
    };
    render = function (p) {
        var markdown = p['markdown'];

        var page = require('webpage').create(), createChart;

        var bodyMale = config.SVG_MALE;
        page.onConsoleMessage = function (msg) {
            console.log('render console message:' + msg);
        };

        page.onAlert = function (msg) {
            console.log('render alert message:' + msg);
        };

        var renderedContent = "";
        createChart = function (markdown) {
            var r = marked.parse(markdown);
            return r
        };

        page.injectJs(config.MARKED);
        renderedContent = page.evaluate(createChart, markdown);
        console.log('^^\n' + renderedContent + '\n$$');
        phantom.exit();

    };
    parseParams()


    render(params);


}());