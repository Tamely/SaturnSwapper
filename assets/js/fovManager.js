window.saturn.fovManager = {
    updateValue: function() {
        OnSetFOV(document.getElementById('FOVCheckBox').checked);
    },
    getConfig: function() {
        fov = OnGetFOV();

        document.getElementById('FOVCheckBox').checked = fov;
    }
}