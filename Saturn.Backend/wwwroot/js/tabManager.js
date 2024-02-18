window.saturn.tabManager = {
    // variables
    currentTab: "",

    // animations
    tabIntro: async function () {
        const newMenuInAnimation = anime({
            targets: [`.tab-container #${this.currentTab}-page .section`],
            opacity: ['0%', '100%'],
            translateX: [50, 0],
            delay: anime.stagger(100)
        }).finished;
        const newMenuWarningInAnimation = anime({
            targets: [`.tab-container #${this.currentTab}-page .section .icon-overlay i`],
            animationDelay: ".1s",
            duration: 1,
        }).finished;
        const newMenuScaleInAnimation = anime({
            targets: [`.tab-container #${this.currentTab}-page .scale-in`],
            opacity: ['0%', '100%'],
            easing: 'easeInOutQuad',
            duration: 120,
            scale: [0.5, 1]
        }).finished;
        const menuHeaderAnimation = anime({
            targets: [`.tab-container #${this.currentTab}-page .header`],
            opacity: ['0%', '100%'],
            translateY: [-20, 0],
            delay: anime.stagger(100)
        }).finished;
        const menuFooterAnimation = anime({
            targets: [`.tab-container #${this.currentTab}-page .tab-bottom`],
            opacity: ['0%', '30%'],
            translateY: [20, 0],
            delay: anime.stagger(100)
        }).finished;
        const tabContainerAnimation = anime({
            targets: [`.tab-container #${this.currentTab}-page .tab-container`],
            opacity: ['0%', '100%'],
            translateX: [20, 0],
            delay: anime.stagger(100)
        }).finished;

        await Promise.all([newMenuInAnimation, newMenuWarningInAnimation, menuHeaderAnimation, menuFooterAnimation, newMenuScaleInAnimation, tabContainerAnimation]);
    },
    tabOutro: async function () {
        const oldMenuOutAnimation = anime({
            targets: [`.tab-container #${this.currentTab}-page .section`],
            opacity: ['100%', '0%'],
            translateX: [0, 50],
            duration: 150,
            easing: 'easeInQuad',
            delay: anime.stagger(40)
        }).finished;
        const newMenuWarningOutAnimation = anime({
            targets: [`.tab-container #${this.currentTab}-page .section .icon-overlay i`],
            duration: 1,
        }).finished;
        const newMenuScaleOutAnimation = anime({
            targets: [`.tab-container #${this.currentTab}-page .scale-in`],
            opacity: ['100%', '0%'],
            scale: [1, 0.5],
            duration: 150,
            easing: 'easeInQuad'
        }).finished;
        const menuHeaderAnimation = anime({
            targets: [`.tab-container #${this.currentTab}-page .header`],
            opacity: ['100%', '0%'],
            translateY: [0, -20],
            duration: 150,
            easing: 'easeInQuad',
            delay: anime.stagger(100)
        }).finished;
        const menuFooterAnimation = anime({
            targets: [`.tab-container #${this.currentTab}-page .tab-bottom`],
            opacity: ['30%', '0%'],
            translateY: [0, 20],
            duration: 150,
            easing: 'easeInQuad',
            delay: anime.stagger(100)
        }).finished;
        const tabContainerAnimation = anime({
            targets: [`.tab-container #${this.currentTab}-page .tab-container`],
            opacity: ['100%', '0%'],
            translateX: [0, 20],
            duration: 150,
            easing: 'easeInQuad',
            delay: anime.stagger(100)
        }).finished;

        await Promise.all([oldMenuOutAnimation, newMenuWarningOutAnimation, menuHeaderAnimation, menuFooterAnimation, newMenuScaleOutAnimation, tabContainerAnimation]);
    },
    checkTab: function (tab) {
        if (tab === this.currentTab) {
            return true;
        }
        
        return !!document.getElementById(`${tab}-page`);
    },
    tabOut: function (oldTab) {
        if (this.currentTab && document.getElementById(`${this.currentTab}-li`)) {
            document.getElementById(`${this.currentTab}-li`).classList.remove("selected");
        }
        
        this.tabOutro().then(() => {
            if (this.currentTab) {
                document.getElementById(`${this.currentTab}-page`).style = {
                    display: "none"
                };
            }
        })
    },
    tabIn: function (newTab) {
        if (this.currentTab === newTab) {
            return;
        }
        this.currentTab = newTab;
        
        if (document.getElementById(`${this.currentTab}-li`)) {
            document.getElementById(`${this.currentTab}-li`).classList.add("selected");
        }

        document.getElementById(`${this.currentTab}-page`).style = {
            display: "unset"
        };
        
        this.tabIntro();
    },
    tabInNoIntro: function (newTab) {
        if (document.getElementById(`${newTab}-li`)) {
            document.getElementById(`${newTab}-li`).classList.add("selected");
        }
    },
    tabOutNoOutro: function (oldTab) {
        if (document.getElementById(`${oldTab}-li`)) {
            document.getElementById(`${oldTab}-li`).classList.remove("selected");
        }
    }
}