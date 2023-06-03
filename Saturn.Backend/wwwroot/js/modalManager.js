window.saturn.modalManager = {
    showModal: function(modalId) {
        document.getElementById(`${modalId}-modal`).classList.remove("minimized");
    },
    hideModal: function(modalId) {
        document.getElementById(`${modalId}-modal`).classList.add("minimized");
    }
}