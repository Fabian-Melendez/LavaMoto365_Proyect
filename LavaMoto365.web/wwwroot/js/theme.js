export function getTheme() {
    const theme = localStorage.getItem("lavaMoto365-theme") || "light";

    document.documentElement.setAttribute("data-theme", theme);

    return theme;
}

export function setTheme(theme) {
    localStorage.setItem("lavaMoto365-theme", theme);

    document.documentElement.setAttribute("data-theme", theme);
}