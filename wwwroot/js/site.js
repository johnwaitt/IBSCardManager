(() => {
    const root = document.documentElement;
    const shell = document.getElementById('appShell');
    const mobileMenu = document.getElementById('mobileMenu');
    const sidebarClose = document.getElementById('sidebarClose');
    const sidebarOverlay = document.getElementById('sidebarOverlay');
    const themeToggle = document.getElementById('themeToggle');
    const themeIcon = document.getElementById('themeIcon');
    const themeLabel = document.getElementById('themeLabel');

    const setSidebar = (open) => shell?.classList.toggle('sidebar-open', open);
    mobileMenu?.addEventListener('click', () => setSidebar(true));
    sidebarClose?.addEventListener('click', () => setSidebar(false));
    sidebarOverlay?.addEventListener('click', () => setSidebar(false));

    const applyTheme = (theme) => {
        root.dataset.theme = theme;
        if (themeIcon) themeIcon.textContent = theme === 'dark' ? '☀' : '☾';
        if (themeLabel) themeLabel.textContent = theme === 'dark' ? 'Light mode' : 'Dark mode';
    };

    const savedTheme = localStorage.getItem('cmp-theme');
    const preferredTheme = window.matchMedia?.('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
    applyTheme(savedTheme || preferredTheme);

    themeToggle?.addEventListener('click', () => {
        const nextTheme = root.dataset.theme === 'dark' ? 'light' : 'dark';
        localStorage.setItem('cmp-theme', nextTheme);
        applyTheme(nextTheme);
    });

    document.addEventListener('keydown', (event) => {
        if (event.key === 'Escape') setSidebar(false);
    });
})();
