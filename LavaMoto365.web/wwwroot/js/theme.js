/*
 * LavaMoto365 - Gestion del tema claro/oscuro.
 *
 * Script clasico (no modulo) cargado de forma sincrona en <head> desde App.razor.
 * No depende de Blazor ni de JS interop, por lo que funciona igual en:
 *   - paginas con renderizado estatico (Login, Register, Account/*),
 *   - paginas interactivas (InteractiveServer),
 *   - navegacion mejorada (enhanced navigation) y recargas completas.
 *
 * El tema se guarda en localStorage y se aplica como <html data-theme="..."> antes
 * del primer pintado para evitar parpadeos.
 */
(function () {
    "use strict";

    var STORAGE_KEY = "lavaMoto365-theme";
    var DEFAULT_THEME = "light";
    var root = document.documentElement;

    function normalize(theme) {
        return theme === "dark" ? "dark" : "light";
    }

    function read() {
        try {
            return normalize(localStorage.getItem(STORAGE_KEY) || DEFAULT_THEME);
        } catch (e) {
            return DEFAULT_THEME; // almacenamiento no disponible (modo privado, etc.)
        }
    }

    var current = read();

    function apply(theme) {
        current = normalize(theme);
        if (root.getAttribute("data-theme") !== current) {
            root.setAttribute("data-theme", current);
        }
        // Bootstrap 5.3 adapta sus componentes con este atributo.
        if (root.getAttribute("data-bs-theme") !== current) {
            root.setAttribute("data-bs-theme", current);
        }
        syncToggles();
    }

    function save(theme) {
        try {
            localStorage.setItem(STORAGE_KEY, normalize(theme));
        } catch (e) { /* se ignora: el tema sigue aplicado en esta pagina */ }
    }

    function syncToggles() {
        var buttons = document.querySelectorAll("[data-theme-toggle]");
        for (var i = 0; i < buttons.length; i++) {
            buttons[i].setAttribute("aria-pressed", current === "dark" ? "true" : "false");
        }
    }

    function set(theme) {
        save(theme);
        apply(theme);
    }

    function toggle() {
        set(current === "dark" ? "light" : "dark");
    }

    // 1) Aplicar de inmediato (antes del primer pintado).
    apply(current);

    // 2) Boton de cambio de tema: delegacion de eventos en document, de modo que
    //    sigue funcionando aunque Blazor reemplace el DOM durante la navegacion.
    document.addEventListener("click", function (event) {
        var target = event.target;
        var button = target && target.closest ? target.closest("[data-theme-toggle]") : null;
        if (button) {
            event.preventDefault();
            toggle();
        }
    });

    // 3) Mantener sincronizados varias pestañas/ventanas.
    window.addEventListener("storage", function (event) {
        if (event.key === STORAGE_KEY) {
            apply(read());
        }
    });

    // 4) Si la navegacion mejorada de Blazor parchea <html> y pierde los atributos, se restauran.
    if (window.MutationObserver) {
        new MutationObserver(function () {
            if (root.getAttribute("data-theme") !== current || root.getAttribute("data-bs-theme") !== current) {
                apply(current);
            }
        }).observe(root, { attributes: true, attributeFilter: ["data-theme", "data-bs-theme"] });
    }

    // 5) Actualizar estado de los botones cuando el DOM este listo y tras cada navegacion mejorada.
    document.addEventListener("DOMContentLoaded", function () {
        syncToggles();
        if (window.Blazor && typeof window.Blazor.addEventListener === "function") {
            window.Blazor.addEventListener("enhancedload", syncToggles);
        }
    });

    // API publica opcional (por si algun componente necesita leer/cambiar el tema).
    window.lavaMotoTheme = {
        get: function () { return current; },
        set: set,
        toggle: toggle
    };
})();
