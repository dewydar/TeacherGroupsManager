(function () {
    const root = document.documentElement;
    const body = document.body;
    const themeButton = document.querySelector('[data-theme-toggle]');
    const sidebarToggle = document.getElementById('togglemenu');
    const sidebarOverlay = document.querySelector('[data-sidebar-overlay]');
    const loader = document.querySelector('[data-page-loader]');
    const typewriter = document.getElementById('typewriter');
    const utcTime = document.getElementById('utcTime');
    const utcDate = document.getElementById('utcDate');

    function applyTheme(theme) {
        root.dataset.theme = theme;
        root.setAttribute('data-bs-theme', theme);
        localStorage.setItem('tgm-theme', theme);
    }

    function hideLoader() {
        loader?.classList.add('hide');
    }

    function setSidebarDefault() {
        if (window.innerWidth >= 1200) {
            body.classList.remove('sidebar-open');
            body.dataset.sidebarSize = localStorage.getItem('tgm-sidebar-size') || 'default';
            return;
        }

        body.dataset.sidebarSize = 'default';
    }

    function toggleSidebar() {
        if (window.innerWidth >= 1200) {
            const nextSize = body.dataset.sidebarSize === 'collapsed' ? 'default' : 'collapsed';
            body.dataset.sidebarSize = nextSize;
            localStorage.setItem('tgm-sidebar-size', nextSize);
            return;
        }

        body.classList.toggle('sidebar-open');
    }

    function closeMobileSidebar() {
        body.classList.remove('sidebar-open');
    }

    function hydrateAvatars() {
        document.querySelectorAll('.avatar[data-name]').forEach((avatar) => {
            const parts = avatar.dataset.name.trim().split(/\s+/).filter(Boolean);
            const first = parts[0]?.[0] || 'T';
            const last = parts.length > 1 ? parts[parts.length - 1][0] : 'G';
            avatar.textContent = (first + last).toUpperCase();
        });
    }

    function updateUtcClock() {
        if (!utcTime || !utcDate) {
            return;
        }

        const now = new Date();
        utcTime.textContent = [
            String(now.getUTCHours()).padStart(2, '0'),
            String(now.getUTCMinutes()).padStart(2, '0'),
            String(now.getUTCSeconds()).padStart(2, '0')
        ].join(':');
        utcDate.textContent = [
            now.getUTCFullYear(),
            String(now.getUTCMonth() + 1).padStart(2, '0'),
            String(now.getUTCDate()).padStart(2, '0')
        ].join('-');
    }

    function startTypewriter() {
        if (!typewriter) {
            return;
        }

        const messages = [
            'Smart groups. Clear lessons.',
            'Teachers, students, payments, all in rhythm.',
            'Learning schedules that stay organized.'
        ];
        let textIndex = 0;
        let charIndex = 0;
        let deleting = false;

        function tick() {
            const text = messages[textIndex];
            typewriter.textContent = text.slice(0, charIndex);

            if (!deleting && charIndex < text.length) {
                charIndex += 1;
                window.setTimeout(tick, 85);
                return;
            }

            if (!deleting) {
                deleting = true;
                window.setTimeout(tick, 1800);
                return;
            }

            if (charIndex > 0) {
                charIndex -= 1;
                window.setTimeout(tick, 35);
                return;
            }

            deleting = false;
            textIndex = (textIndex + 1) % messages.length;
            window.setTimeout(tick, 300);
        }

        tick();
    }

    function decorateTables() {
        document.querySelectorAll('.table-responsive').forEach((shell) => {
            shell.classList.add('app-table-shell');
        });

        document.querySelectorAll('table.table').forEach((table) => {
            table.classList.add('app-table');
            table.querySelectorAll('tbody tr').forEach((row) => {
                row.classList.add('app-table-row');
                row.querySelectorAll('td').forEach((cell) => cell.classList.add('app-table-cell'));
            });
        });
    }

    applyTheme(localStorage.getItem('tgm-theme') || 'light');
    setSidebarDefault();
    hydrateAvatars();
    updateUtcClock();
    startTypewriter();
    decorateTables();

    if (document.readyState === 'complete') {
        hideLoader();
        decorateTables();
    } else {
        document.addEventListener('DOMContentLoaded', function () {
            hideLoader();
            decorateTables();
        });
        window.addEventListener('load', hideLoader);
    }

    window.setTimeout(hideLoader, 1200);
    window.setInterval(updateUtcClock, 1000);

    themeButton?.addEventListener('click', function () {
        applyTheme(root.dataset.theme === 'dark' ? 'light' : 'dark');
    });

    sidebarToggle?.addEventListener('click', toggleSidebar);
    sidebarOverlay?.addEventListener('click', closeMobileSidebar);

    document.querySelectorAll('.sidebar .nav-link').forEach((link) => {
        link.addEventListener('click', closeMobileSidebar);
    });

    window.addEventListener('resize', setSidebarDefault);
})();
