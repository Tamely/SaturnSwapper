window.saturn.pluginManager = {
    addPlugin: function(id, name, image) {
        const templateString = `<div class="container-item" onclick="OnSelectPlugin('${id}')"> <h2>${name}</h2> <img src="${image}" /></div>`
        const container = document.getElementById('container');

        container.innerHTML += templateString;
    },
    clearPlugins: function() {
        var container = document.getElementById('container');
        container.innerHTML = "";
    },
    onSearch: function(event) {
        if (event.key == "Enter") {
            const search = document.getElementById('search').value;
            OnSearch(search);
            OnDisplayItems();
        }
    },
    onLoadPlugins() {
        OnLoadPluginTab();
        PR.prettyPrint();
    },
    onSetPlugin(name, author, message, backgroundImage, plugin) {
        const pluginContainer = document.getElementById(`plugin`);
        const headerName = document.getElementById(`header-name`);
        const headerAuthor = document.getElementById(`header-author`);
        const pluginHeader = document.getElementById(`header`);

        pluginHeader.style.backgroundImage = `url('${backgroundImage}')`

        pluginContainer.innerHTML = `<pre class="prettyprint">${atob(plugin)}</pre>`;
        headerName.innerHTML = name;
        headerAuthor.innerHTML = `Author: ` + author;

        if (message) {
            const messageText = document.getElementById(`message`);

            messageText.innerHTML = message;
            saturn.modalManager.showModal('message');
        }
    },
    onDownloadPlugin() {
        const text = document.getElementById(`download-text`);
        if (text.innerHTML.startsWith("Download")) {
            if (OnDownloadPlugin()) {
                text.innerHTML = `In Library <i class="fas fa-check"></i>`;
            } else {
                text.innerHTML = `Error <i class="fas fa-x"></i>`;
            }
        }
    }
}