using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Portfolio.Web.Content;

namespace Portfolio.Web.Tests;

public class StaticSiteExporterTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly string _outputDir = Path.Combine(Path.GetTempPath(), $"portfolio-export-{Guid.NewGuid():N}");

    public void Dispose()
    {
        if (Directory.Exists(_outputDir))
        {
            Directory.Delete(_outputDir, recursive: true);
        }
    }

    [Fact]
    public async Task Writes_the_home_page_404_page_and_assets()
    {
        var files = await Export();

        Assert.Contains("index.html", files);
        Assert.Contains("404.html", files);
        Assert.Contains(Path.Combine("css", "site.css"), files);
        Assert.Contains(Path.Combine("js", "site.js"), files);
        Assert.Contains("favicon.svg", files);
        Assert.Contains(".nojekyll", files);
        Assert.All(files, file => Assert.True(File.Exists(Path.Combine(_outputDir, file)), $"{file} is missing."));

        var name = factory.Services.GetRequiredService<Profile>().Name;
        Assert.Contains($">{name}</h1>", await File.ReadAllTextAsync(Path.Combine(_outputDir, "index.html")));
        Assert.Contains("Page not found", await File.ReadAllTextAsync(Path.Combine(_outputDir, "404.html")));
    }

    [Fact]
    public async Task Running_twice_overwrites_the_previous_export()
    {
        await Export();
        File.WriteAllText(Path.Combine(_outputDir, "index.html"), "stale");

        await Export();

        Assert.NotEqual("stale", File.ReadAllText(Path.Combine(_outputDir, "index.html")));
    }

    private Task<IReadOnlyList<string>> Export()
    {
        var webRoot = factory.Services.GetRequiredService<IWebHostEnvironment>().WebRootPath;
        return StaticSiteExporter.ExportAsync(factory.CreateClient(), webRoot, _outputDir);
    }
}
