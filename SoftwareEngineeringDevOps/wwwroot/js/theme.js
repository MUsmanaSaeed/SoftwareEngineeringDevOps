(function () {
    var storageKey = 'app-theme';
    var defaultTheme = 'system';
    var allowedThemes = ['light', 'dark', 'system'];
    var colorSchemeQuery = window.matchMedia('(prefers-color-scheme: dark)');

    function getStoredTheme() {
        try {
            var storedTheme = localStorage.getItem(storageKey);
            return allowedThemes.indexOf(storedTheme) >= 0 ? storedTheme : defaultTheme;
        } catch (e) {
            return defaultTheme;
        }
    }

    function resolveTheme(theme) {
        return theme === 'dark' || (theme === 'system' && colorSchemeQuery.matches)
            ? 'dark'
            : 'light';
    }

    function applyTheme(theme) {
        var selectedTheme = allowedThemes.indexOf(theme) >= 0 ? theme : defaultTheme;
        var resolvedTheme = resolveTheme(selectedTheme);

        document.documentElement.setAttribute('data-theme', resolvedTheme);
        document.documentElement.style.colorScheme = resolvedTheme;
    }

    function applySavedTheme() {
        applyTheme(getStoredTheme());
    }

    window.themeManager = {
        getTheme: function () {
            return getStoredTheme();
        },
        setTheme: function (theme) {
            var selectedTheme = allowedThemes.indexOf(theme) >= 0 ? theme : defaultTheme;

            try {
                localStorage.setItem(storageKey, selectedTheme);
            } catch (e) {
            }

            applyTheme(selectedTheme);
        }
    };

    applySavedTheme();

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', applySavedTheme, { once: true });
    }

    window.addEventListener('pageshow', applySavedTheme);

    if (typeof colorSchemeQuery.addEventListener === 'function') {
        colorSchemeQuery.addEventListener('change', function () {
            if (getStoredTheme() === 'system') {
                applyTheme(defaultTheme);
            }
        });
    }
})();
