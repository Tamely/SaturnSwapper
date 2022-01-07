async function OpenFileDialog() {
    var fileSelector = document.createElement('input');
    fileSelector.setAttribute('type', 'file');

    var fileByteArray = [];

    fileSelector.onchange = e => {

        // getting a hold of the file reference
        var file = e.target.files[0];
        
        var reader = new FileReader();
        reader.readAsArrayBuffer(file);
        reader.onloadend = function (evt) {
            if (evt.target.readyState == FileReader.DONE) {
                var arrayBuffer = evt.target.result,
                    array = new Uint8Array(arrayBuffer);
                for (var i = 0; i < array.length; i++) {
                    fileByteArray.push(array[i]);
                }
            }
        }
    }

    fileSelector.click()
    
    while (fileByteArray == []) {
        // wait for the file to be read
        await sleep(200);
    }
    
    await sleep(1000);
    
    return fileByteArray;
}

function sleep(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
}