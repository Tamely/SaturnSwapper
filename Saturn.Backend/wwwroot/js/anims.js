window.saturn.anims = {
    animations: {
        installerIn: async function() {
            const logoAnimation = anime(
                {
                    targets: [`#installer-welcome .image img`],
                    opacity: ['0%', '100%'],
                    scale: [0, 1],
                    delay: anime.stagger(100, { start: 200 })
                }
            ).finished;
            
            const textAnimation = anime(
                {
                    targets: [`#installer-welcome .text h1`, `#installer-welcome .text h3`],
                    opacity: ['0%', '100%'],
                    translateY: [20, 0],
                    delay: anime.stagger(1000, { start: 200 })
                }
            ).finished;
            
            await Promise.all([logoAnimation, textAnimation]);
        },
        installerOut: async function() {
            const logoAnimation = anime(
                {
                    targets: [`#installer-welcome .image img`],
                    opacity: ['100%', '0%'],
                    scale: [1, 0],
                    delay: anime.stagger(100, { start: 200 })
                }
            ).finished;
            
            const textAnimation = anime(
                {
                    targets: [`#installer-welcome .text h1`, `#installer-welcome .text h3`],
                    opacity: ['100%', '0%'],
                    translateY: [0, 20],
                    delay: anime.stagger(1000, { start: 200 })
                }
            ).finished;
            
            await Promise.all([logoAnimation, textAnimation]);
        },
        installerTextIn: async function() {
            const textAnimation = anime(
                {
                    targets: [`#installer-welcome .text h3`],
                    opacity: ['0%', '100%'],
                    translateY: [20, 0],
                    delay: anime.stagger(1000, { start: 200 })
                }
            ).finished;

            await Promise.all([textAnimation]);
        },
        installerTextOut: async function() {
            const textAnimation = anime(
                {
                    targets: [`#installer-welcome .text h3`],
                    opacity: ['100%', '0%'],
                    translateY: [0, 20],
                    delay: anime.stagger(1000, { start: 200 })
                }
            ).finished;

            await Promise.all([textAnimation]);
        },
    },
    installerWelcomeIn: async function() {
        let welcomeTab = document.getElementById(`installer-welcome`);
        welcomeTab.style.display = "flex";
        await saturn.anims.animations.installerIn()
    },
    installerWelcomeOut: async function() {
        let welcomeTab = document.getElementById(`installer-welcome`);
        await saturn.anims.animations.installerOut()
        welcomeTab.style.display = "none";
        
        let swapperTab = document.getElementById('swapper-page');
        swapperTab.style.display = "flex";
    },
    installerChangeText: async function(text) {
        let depText = document.getElementById(`installer-dep`);
        await saturn.anims.animations.installerTextOut();
        depText.innerText = text;
        await saturn.anims.animations.installerTextIn();
    },
    waitForId: function (id) {
        return !!document.getElementById(`${id}`);
    }
}