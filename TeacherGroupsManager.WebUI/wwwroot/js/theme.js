(function () {
    const root = document.documentElement;
    const button = document.querySelector('[data-theme-toggle]');

    function applyTheme(theme) {
        root.dataset.theme = theme;
        localStorage.setItem('tgm-theme', theme);
    }

    applyTheme(localStorage.getItem('tgm-theme') || 'light');

    if (button) {
        button.addEventListener('click', function () {
            applyTheme(root.dataset.theme === 'dark' ? 'light' : 'dark');
        });
    }
})();
