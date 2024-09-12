(function () {
    var system = require('system');
    var fs = require('fs');
    var config = {
        JQUERY: 'jquery-3.6.3.min.js',
        ECHARTS: 'thumbnail-bg-gen.js',
        DEFAULT_WIDTH: '300',
        DEFAULT_HEIGHT: '200'
    }, parseParams, render, pick, usage, params;
    usage = function () {
        console.log("\nUsage: phantomjs echarts-convert.js -options options -outfile filename -width width -height height"
            + "OR"
            + "Usage: phantomjs echarts-convert.js -infile URL -outfile filename -width width -height height\n");
    };

    pick = function () {
        var args = arguments, i, arg, length = args.length;
        for (i = 0; i < length; i += 1) {
            arg = args[i];
            if (arg !== undefined && arg !== null && arg !== 'null' && arg != '0') {
                return arg;
            }
        }
    };

    parseParams = function () {
        var map = {}, i, key;
        console.log("--logs:\n")
        console.log(system.args)
        if (system.args.length < 2) {
            usage();
            phantom.exit();
        }
        for (i = 0; i < system.args.length; i += 1) {
            if (system.args[i].charAt(0) === '-') {
                key = system.args[i].substr(1, i.length);
                map[key] = system.args[i + 1].replace(/^\s+/, '');

            }
        }
        params = map;
    };

    render = function (params) {
        var page = require('webpage').create(), createChart;

        var bodyMale = config.SVG_MALE;
        page.onConsoleMessage = function (msg) {
            console.log(msg);
        };

        page.onAlert = function (msg) {
            console.log(msg);
        };

        createChart = function (title, width, height, config) {
            var counter = 0;
            function decrementImgCounter() {
                counter -= 1;
                if (counter < 1) {
                    console.log(messages.imagesLoaded);
                }
            }


            function loadImages() {
                var images = $('image'), i, img;
                if (images.length > 0) {
                    counter = images.length;
                    for (i = 0; i < images.length; i += 1) {
                        img = new Image();
                        img.onload = img.onerror = decrementImgCounter;
                        img.src = images[i].getAttribute('href');
                    }
                } else {
                    console.log('The images have been loaded');
                }
            }


            var newDiv = document.createElement("div");
            newDiv.id = "display";
            newDiv.style.width = width + "px";
            newDiv.style.height = height + "px";
            newDiv.style.background = "black";

            const p = document.createElement('p');

            p.innerText = title;

            p.style.position = 'absolute';
            p.style.top = '50%';
            p.style.left = '50%';
            p.style.transform = 'translate(-50%, -50%)';
            p.style.textAlign = 'center';
            p.style.color = 'white';




            document.body.appendChild(newDiv);

            console.log(document.body.innerHTML);

            this.generateGrad("#000000", 7, newDiv);

            newDiv.appendChild(p);

        };

        // parse the params
        page.open("about:blank", function (status) {
            //page.injectJs(config.ESL);
            page.injectJs(config.JQUERY);
            page.injectJs(config.ECHARTS);


            var width = pick(params.width, config.DEFAULT_WIDTH);
            var height = pick(params.height, config.DEFAULT_HEIGHT);
            console.log('start rendering:' + params.outfile);

            var base64 = page.evaluate(createChart, params.title, width, height, config);
            //fs.write("base64.txt", base64);
            // define the clip-rectangle
            console.log('rendering.. :' + params.outfile);
            console.log('start rendering page:' + params.outfile);

            var clipRect = page.evaluate(function () {
                var element = document.getElementById('display');  // 目标元素
                var rect = element.getBoundingClientRect();  // 获取元素的边界
                return {
                    top: rect.top,
                    left: rect.left,
                    width: rect.width,
                    height: rect.height
                };
            });
            page.clipRect = clipRect;
            page.render(params.outfile);
            console.log('render complete:' + params.outfile);
            phantom.exit();
        });
    };


    parseParams()
    console.warn(params);
    if (params.outfile === undefined) {
        var tmpDir = fs.workingDirectory + '/tmp';

        if (!fs.exists(tmpDir)) {
            try {
                fs.makeDirectory(tmpDir);
            } catch (e) {
                console.log('ERROR: Cannot make tmp directory');
            }
        }
        params.outfile = tmpDir + "/" + new Date().getTime() + ".png";
    }
    else {
        params.outfile = params.outfile + "/" + new Date().getTime() + ".png";
    }



    render(params);


}());