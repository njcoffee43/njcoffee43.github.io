# Portfolio

My personal site: who I am, where I've worked, and what I've built. It's a server-rendered ASP.NET Core 8 app using Blazor components.

## Run it

- **Visual Studio 2022:** open `Portfolio.sln` and press **F5**. `Portfolio.Web` is the startup project.
- **Command line:** `dotnet run --project src/Portfolio.Web`, then open http://localhost:5180. Use `dotnet watch --project src/Portfolio.Web` to reload on save.
- **Tests:** `dotnet test`, or **Test → Run All Tests** in Visual Studio.

## Editing the content

All of the text on the site lives in [`src/Portfolio.Web/Content/profile.json`](src/Portfolio.Web/Content/profile.json). Edit it and restart the app; no code changes needed.

- **Links:** fill in `links.github`, `links.linkedin` and `site.sourceCode` with full `https://` URLs. Any link left as `""` is hidden, and so is any section with nothing in it.
- **Resume download:** put the PDF in `src/Portfolio.Web/wwwroot/` and set `links.resume` to its file name (e.g. `"resume.pdf"`), or to a full URL.
- **Project links:** each project has `repoUrl` and `liveUrl` for **Source** and **Live** links.
- **Site URL:** once the site is deployed, set `site.url` so search engines and link previews get the right canonical address.

The file is checked when the app starts. A misspelled key, a missing required field, or a link that isn't a full URL stops startup with a list of every problem. The tests catch the same mistakes, so CI fails before a broken page goes live.

## Deploying (GitHub Pages)

The site is hosted for free on GitHub Pages at `https://<username>.github.io`. Every push to `master` runs [`.github/workflows/deploy.yml`](.github/workflows/deploy.yml). It runs the tests, saves the site as static HTML with `--export`, and publishes it.

One-time setup:

1. Create a **public** GitHub repo named exactly `<username>.github.io`, and push this folder to its `master` branch. The site links to `/css/...`, so it must be served from the root of the domain; a repo with any other name would be served under `/<repo>/` and lose its styles.
2. In the repo, go to **Settings → Pages → Build and deployment** and set **Source** to **GitHub Actions**.
3. Go to **Actions → Deploy to GitHub Pages → Run workflow**, or push a commit. The site is live about two minutes later.

To export locally (e.g. to check the output): `dotnet run --project src/Portfolio.Web -- --export C:\temp\site`.

## Projects

| Project | What it is |
|---|---|
| `src/Portfolio.Web` | The site. `Content/` holds the profile model, loader/validator and JSON; `Components/` holds the layout, pages and shared Blazor components; `wwwroot/` holds the CSS, JS and favicon; `StaticSiteExporter` saves the site as static files for GitHub Pages. |
| `tests/Portfolio.Web.Tests` | xUnit tests. They render the real pages through `WebApplicationFactory` and check the content, 404 handling, health check, static files and security headers, plus unit tests for the profile validator and the JSON-LD output. |

## How it works

- **Static server-side rendering:** pages are rendered to plain HTML on the server, with no Blazor runtime in the browser. The only JavaScript is about 30 lines for the light/dark toggle and nav highlighting.
- **Light and dark themes:** the site follows the visitor's system setting, and the toggle saves their choice. A tiny inline script applies the saved theme before the page draws, so it doesn't flash.
- **Search and link previews:** the page has a meta description, Open Graph tags, and schema.org `Person` data (JSON-LD).
- **Server mode:** run as a live ASP.NET Core server (e.g. on Azure App Service), the app also sends `X-Content-Type-Options`, `X-Frame-Options` and `Referrer-Policy` headers, and has a health check at `/healthz`. GitHub Pages serves the exported files, so it doesn't send those headers.
- **CI/CD:** [`.github/workflows/ci.yml`](.github/workflows/ci.yml) builds and tests on every push and pull request; [`deploy.yml`](.github/workflows/deploy.yml) publishes `master` to GitHub Pages.
