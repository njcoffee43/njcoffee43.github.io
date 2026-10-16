using System.Net;

namespace Portfolio.Web;

/// <summary>
/// Saves the site as plain files (HTML plus everything in wwwroot) so it can be
/// hosted on a static host like GitHub Pages. Run with: dotnet run -- --export &lt;folder&gt;
/// </summary>
public static class StaticSiteExporter
{
    // Each page to save, and the status it should return. GitHub Pages serves 404.html for unknown URLs.
    private static readonly (string Path, string File, HttpStatusCode Status)[] Pages =
    [
        ("/", "index.html", HttpStatusCode.OK),
        ("/not-found", "404.html", HttpStatusCode.NotFound),
    ];

    /// <summary>Starts the app on a free local port, exports it, and stops it.</summary>
    public static async Task RunAsync(WebApplication app, string outputDir)
    {
        app.Urls.Clear();
        app.Urls.Add("http://127.0.0.1:0");
        await app.StartAsync();
        try
        {
            using var client = new HttpClient { BaseAddress = new Uri(app.Urls.First()) };
            var fullPath = Path.GetFullPath(outputDir);
            var files = await ExportAsync(client, app.Environment.WebRootPath, fullPath);
            Console.WriteLine($"Exported {files.Count} files to {fullPath}");
        }
        finally
        {
            await app.StopAsync();
        }
    }

    /// <summary>Writes the pages and static assets into <paramref name="outputDir"/>, overwriting existing files.</summary>
    /// <returns>The paths written, relative to <paramref name="outputDir"/>.</returns>
    public static async Task<IReadOnlyList<string>> ExportAsync(
        HttpClient client, string webRootPath, string outputDir, CancellationToken cancellationToken = default)
    {
        var written = new List<string>();
        Directory.CreateDirectory(outputDir);

        foreach (var source in Directory.EnumerateFiles(webRootPath, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(webRootPath, source);
            var target = Path.Combine(outputDir, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(source, target, overwrite: true);
            written.Add(relative);
        }

        foreach (var (path, file, status) in Pages)
        {
            using var response = await client.GetAsync(path, cancellationToken);
            if (response.StatusCode != status)
            {
                throw new InvalidOperationException(
                    $"GET {path} returned {(int)response.StatusCode}, expected {(int)status}.");
            }

            var html = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            await File.WriteAllBytesAsync(Path.Combine(outputDir, file), html, cancellationToken);
            written.Add(file);
        }

        // Tells GitHub Pages to serve the files as-is instead of running them through Jekyll.
        await File.WriteAllTextAsync(Path.Combine(outputDir, ".nojekyll"), "", cancellationToken);
        written.Add(".nojekyll");

        return written;
    }
}
