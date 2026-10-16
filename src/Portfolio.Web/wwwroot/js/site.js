// Light/dark toggle. The saved choice is applied in <head> (see App.razor) to avoid a flash.
const toggle = document.querySelector("[data-theme-toggle]");
toggle?.addEventListener("click", () => {
    const root = document.documentElement;
    const current = root.dataset.theme
        ?? (matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light");
    const next = current === "dark" ? "light" : "dark";
    root.dataset.theme = next;
    try { localStorage.setItem("theme", next); } catch { /* storage blocked; still works for this visit */ }
});

// Highlight the nav link for the section being read.
const navLinks = new Map(
    [...document.querySelectorAll("[data-nav]")].map(link => [link.dataset.nav, link]));
const sections = [...navLinks.keys()]
    .map(id => document.getElementById(id))
    .filter(Boolean);

if (sections.length > 0 && "IntersectionObserver" in window) {
    const observer = new IntersectionObserver(entries => {
        for (const entry of entries) {
            if (!entry.isIntersecting) continue;
            navLinks.forEach((link, id) => {
                if (id === entry.target.id) link.setAttribute("aria-current", "true");
                else link.removeAttribute("aria-current");
            });
        }
    }, { rootMargin: "-45% 0px -50% 0px" });
    sections.forEach(section => observer.observe(section));
}
