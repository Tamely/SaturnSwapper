new Promise((resolve) => setTimeout(resolve, 1000)).then(() => {
    if (OnLoadSaturn()) {
        new Promise((resolve) => setTimeout(resolve, 500)).then(() => {
            window.location.pathname = window.location.pathname.replace('/app', '/pages/key');
        });
    }
});
