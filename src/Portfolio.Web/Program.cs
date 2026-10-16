using Portfolio.Web;
using Portfolio.Web.Components;
using Portfolio.Web.Content;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents();
builder.Services.AddHealthChecks();

// Loaded once at startup; an invalid profile.json stops the app with a list of what's wrong.
var profilePath = Path.Combine(builder.Environment.ContentRootPath, "Content", "profile.json");
builder.Services.AddSingleton(_ => ProfileLoader.Load(profilePath));

var app = builder.Build();

// Resolve now so a bad profile fails at startup, not on the first request.
app.Services.GetRequiredService<Profile>();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error", createScopeForErrors: true);
    app.UseHsts();
}

app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers.XContentTypeOptions = "nosniff";
    headers.XFrameOptions = "DENY";
    headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    await next();
});

app.UseStatusCodePagesWithReExecute("/not-found");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapHealthChecks("/healthz");
app.MapRazorComponents<App>();

// "--export <folder>" saves the site as static files instead of serving it (used by the GitHub Pages deploy).
var exportIndex = Array.IndexOf(args, "--export");
if (exportIndex >= 0)
{
    if (exportIndex + 1 >= args.Length)
    {
        throw new ArgumentException("--export needs a folder, e.g. --export ./site");
    }

    await StaticSiteExporter.RunAsync(app, args[exportIndex + 1]);
    return;
}

app.Run();

// Exposes Program to WebApplicationFactory in the test project.
public partial class Program { }
