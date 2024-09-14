window.saturn.modalManager = {
    showModal: function(modalId) {
        if (document.getElementById(`${modalId}-modal`)) {
            document.getElementById(`${modalId}-modal`).classList.remove('minimized');
        }
    },
    hideModal: function(modalId) {
        if (document.getElementById(`${modalId}-modal`)) {
            document.getElementById(`${modalId}-modal`).classList.add('minimized');
        }
    }
}