(function () {
    var svg = "";

    function rand(min, max) {
        min = Math.ceil(min);
        max = Math.floor(max);
        return Math.floor(Math.random() * (max - min + 1)) + min;
    }

    function HSLToRGB(h, s, l, a) {
        // Must be fractions of 1
        s /= 100;
        l /= 100;

        let c = (1 - Math.abs(2 * l - 1)) * s,
            x = c * (1 - Math.abs(((h / 60) % 2) - 1)),
            m = l - c / 2,
            r = 0,
            g = 0,
            b = 0;

        if (0 <= h && h < 60) {
            r = c;
            g = x;
            b = 0;
        } else if (60 <= h && h < 120) {
            r = x;
            g = c;
            b = 0;
        } else if (120 <= h && h < 180) {
            r = 0;
            g = c;
            b = x;
        } else if (180 <= h && h < 240) {
            r = 0;
            g = x;
            b = c;
        } else if (240 <= h && h < 300) {
            r = x;
            g = 0;
            b = c;
        } else if (300 <= h && h < 360) {
            r = c;
            g = 0;
            b = x;
        }
        r = Math.round((r + m) * 255);
        g = Math.round((g + m) * 255);
        b = Math.round((b + m) * 255);

        return "rgba(" + r + "," + g + "," + b + "," + a + ")";
    }

    // GENERATE GRADIENT
    function generateGrad(bgColor, layers, display) {
        var grad = [];
        var svgGrad = [];
        var svgRect = [];

        for (i = 0; i < layers; i++) {
            var xPos = rand(0, 100);
            var yPos = rand(0, 100);
            var hue = rand(0, 360);
            var saturation = rand(85, 95);
            var lightness = rand(50, 70);
            var rotation = rand(0, 365);
            var newGrad = `radial-gradient(at ${xPos}% ${yPos}%, hsla(${hue}, ${saturation}%, ${lightness}%, 1) 0, hsla(${hue}, ${saturation}%, ${lightness}%, 0) 50%)`;
            grad.push(newGrad);

            var newSvgGrad = `<radialGradient id="grad${i}" cx="${xPos}%" cy="${yPos}%" r="100%" fx="${xPos}%" fy="${yPos}%" gradientUnits="objectBoundingBox">
                              <stop offset="0" stop-color="${HSLToRGB(
                hue,
                saturation,
                lightness,
                1
            )}" stop-opacity="1" />
                              <stop offset="0.5" stop-color="${HSLToRGB(
                hue,
                saturation,
                lightness,
                0
            )}" stop-opacity="0" />
                          </radialGradient>`;
            svgGrad.push(newSvgGrad);
            var newSvgRect = `<rect x="0" y="0" width="3000" height="2000" fill="url(#grad${i})" />`;
            svgRect.push(newSvgRect);
        }

        var svgTop = `<svg viewBox="0 0 3000 2000" fill="none" xmlns="http://www.w3.org/2000/svg">
                      <defs>`;
        var svgBg = `<rect x="0" y="0" width="3000" height="2000" fill="#${bgColor}" />`;
        svg =
            svgTop + [...svgGrad] + "</defs>" + svgBg + [...svgRect] + "</svg>";
        display.innerHTML = svg;

        var copyCss =
            "background-color: #" +
            bgColor +
            ";<br/>background-image: " +
            grad.join(",<br/>") +
            ";";
        // document.getElementById("css-code").innerHTML = copyCss;

    }

    function saveSvg(svgEl, name) {
        var svgBlob = new Blob([svgEl], {
            type: "image/svg+xml;charset=utf-8",
        });
        var svgUrl = URL.createObjectURL(svgBlob);
        var downloadLink = document.createElement("a");
        downloadLink.href = svgUrl;
        downloadLink.download = name;
        document.body.appendChild(downloadLink);
        downloadLink.click();
        document.body.removeChild(downloadLink);
    }

    // DOWNLOAD IMAGE
    function downloadImage() {
        saveSvg(svg, "color-morph.svg");

    }

    // INIT
    generateGrad('#26004d', 7);
}());