window.saturn.loadoutManager = {
    addLoadout: function(skinId, pickaxeId, backblingId, coverId) {
        const track = document.getElementById('image-track');
        const templateString = `<div class="image" onclick="saturn.loadoutManager.applyLoadout('${skinId}', '${pickaxeId}', '${backblingId}')"> <img src="https://fortnite-api.com/images/cosmetics/br/${coverId}/icon.png"/> </div>`;
        
        track.innerHTML += templateString;
    },
    applyLoadout: function(skinId, pickaxeId, backblingId) {
        saturn.modalManager.showModal('apply');
        OnApplyLoadout(skinId, pickaxeId, backblingId);
    },
    clearLoadouts: function() {
        var track = document.getElementById('image-track');
        track.innerHTML = "";
    }
}