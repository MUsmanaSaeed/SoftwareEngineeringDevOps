export function afterWebStarted(blazor) {
    blazor.addEventListener('enhancedload', applyTheme);
    applyTheme();
}

function applyTheme() {
    var storageKey = 'app-theme';
    var allowedThemes = ['light', 'dark', 'system'];
    var storedTheme;

    try {
        storedTheme = localStorage.getItem(storageKey);
    } catch (e) {
        storedTheme = null;
    }

    var theme = allowedThemes.indexOf(storedTheme) >= 0 ? storedTheme : 'system';
    var resolved = theme === 'dark' || (theme === 'system' && window.matchMedia('(prefers-color-scheme: dark)').matches)
        ? 'dark'
        : 'light';

    document.documentElement.setAttribute('data-theme', resolved);
    document.documentElement.style.colorScheme = resolved;
}
