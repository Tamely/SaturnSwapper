window.saturn.keyManager = {
    checkKey: function() {
        if (OnCheckKey(document.getElementById('key').value)) {
            return;
        }

        saturn.modalManager.showModal('key');
    },
    getConfig: function() {
        key = OnGetKey();

        if (key !== null && key.trim() !== "") {
            document.getElementById('key').value = key;
        }
    }
}