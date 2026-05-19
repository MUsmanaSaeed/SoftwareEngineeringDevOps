window.appTheme = {
    cookieName: 'app_theme',

    init: function () {
        const theme = this._getCookie(this.cookieName) || 'light';
        document.documentElement.setAttribute('data-bs-theme', theme);
    },

    set: function (theme) {
        document.documentElement.setAttribute('data-bs-theme', theme);
        document.cookie = this.cookieName + '=' + theme + '; path=/; max-age=31536000; SameSite=Strict';
    },

    isDark: function () {
        const theme = this._getCookie(this.cookieName);
        return theme === 'dark';
    },

    _getCookie: function (name) {
        const match = document.cookie.match(new RegExp('(^| )' + name + '=([^;]+)'));
        return match ? match[2] : null;
    }
};

window.appTheme.init();
