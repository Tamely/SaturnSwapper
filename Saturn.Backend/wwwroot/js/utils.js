window.saturn.utils = {
    mouse: {
        mouseX: 0,
        mouseY: 0,
        internalMouseCallback: function(event) {
            this.mouseX = event.clientX;
            this.mouseY = event.clientY;
        }
    },
    delay: function(n){
        return new Promise(function(resolve){
            setTimeout(resolve,n*1000);
        });
    },
    randomInRange: function(min, max) {
        return Math.random() * (max - min) + min;
    },
    addConsoleCommand: function(name) {
        window[name] = async function() {
            await DotNet.invokeMethodAsync("Saturn", name);
        }
    }
}

window.onmousemove = function(event) {
    saturn.utils.mouse.internalMouseCallback(event);
}