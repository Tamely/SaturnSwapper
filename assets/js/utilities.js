function SendToURL(url) {
    window.location.pathname = url;
}

function SendToDash() {
    window.location.pathname = '/pages/home.html';
}

function SendToHome() {
    OnResetTab();
    window.location.pathname = '/pages/cosmetics.html';
}

function SendToSkins() {
    OnGenerateSkins();
    window.location.pathname = '/pages/items.html';
}

function SendToBackblings() {
    OnGenerateBackblings();
    window.location.pathname = '/pages/items.html';
}

function SendToPickaxes() {
    OnGeneratePickaxes();
    window.location.pathname = '/pages/items.html';
}

function SendToEmotes() {
    OnGenerateEmotes();
    window.location.pathname = '/pages/items.html';
}