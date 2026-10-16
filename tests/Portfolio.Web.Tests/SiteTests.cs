using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Portfolio.Web.Content;

namespace Portfolio.Web.Tests;

public class SiteTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private Profile ShippedProfile => factory.Services.GetRequiredService<Profile>();

    [Fact]
    public void Shipped_profile_is_valid()
    {
        Assert.Empty(ProfileLoader.Validate(ShippedProfile));
    }

    [Fact]
    public async Task Home_page_shows_everything_in_the_profile()
    {
        var html = await GetDecodedHtml(factory, "/");
        var profile = ShippedProfile;

        Assert.Contains($">{profile.Name}</h1>", html);
        Assert.Contains(profile.Headline, html);
        Assert.Contains(profile.Summary, html);
        Assert.All(profile.About, paragraph => Assert.Contains(paragraph, html));
        Assert.All(profile.Experience, job =>
        {
            Assert.Contains(job.Role, html);
            Assert.Contains(job.Company, html);
            Assert.All(job.Highlights, item => Assert.Contains(item, html));
        });
        Assert.All(profile.Projects, project =>
        {
            Assert.Contains(project.Title, html);
            Assert.Contains(project.Summary, html);
        });
        Assert.All(profile.Skills, group => Assert.All(group.Items, item => Assert.Contains(item, html)));
        Assert.All(profile.Education, school => Assert.Contains(school.Degree, html));
        Assert.All(profile.Certifications, cert => Assert.Contains(cert.Name, html));
        Assert.Contains($"href=\"mailto:{profile.Links.Email}\"", html);
    }

    [Fact]
    public async Task Home_page_has_title_description_and_structured_data()
    {
        var html = await GetDecodedHtml(factory, "/");

        Assert.Contains($"<title>{ShippedProfile.Name} · {ShippedProfile.Headline}</title>", html);
        Assert.Contains("<meta name=\"description\"", html);

        const string open = "<script type=\"application/ld+json\">";
        var start = html.IndexOf(open, StringComparison.Ordinal);
        Assert.True(start >= 0, "JSON-LD script is missing.");
        start += open.Length;
        var json = html[start..html.IndexOf("</script>", start, StringComparison.Ordinal)];

        using var doc = JsonDocument.Parse(json);
        Assert.Equal("Person", doc.RootElement.GetProperty("@type").GetString());
        Assert.Equal(ShippedProfile.Name, doc.RootElement.GetProperty("name").GetString());
    }

    [Fact]
    public async Task Links_left_empty_are_not_rendered()
    {
        var profile = ShippedProfile with
        {
            Links = ShippedProfile.Links with
            {
                GitHub = "",
                LinkedIn = "https://www.linkedin.com/in/example",
                Resume = "resume.pdf",
            },
        };
        using var customized = WithProfile(profile);

        var html = await GetDecodedHtml(customized, "/");

        Assert.DoesNotContain("GitHub</a>", html);
        Assert.Contains("href=\"https://www.linkedin.com/in/example\"", html);
        Assert.Contains("href=\"/resume.pdf\"", html);
    }

    [Fact]
    public async Task Nav_skips_sections_with_no_content()
    {
        using var customized = WithProfile(ShippedProfile with { Projects = [], Skills = [] });

        var html = await GetDecodedHtml(customized, "/");

        Assert.DoesNotContain("data-nav=\"projects\"", html);
        Assert.DoesNotContain("id=\"projects\"", html);
        Assert.DoesNotContain("data-nav=\"skills\"", html);
        Assert.Contains("data-nav=\"experience\"", html);
    }

    [Theory]
    [InlineData("/does-not-exist")]
    [InlineData("/not-found")]
    public async Task Unknown_pages_return_404_with_a_friendly_page(string path)
    {
        var response = await factory.CreateClient().GetAsync(path);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("Page not found", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Health_check_reports_healthy()
    {
        var response = await factory.CreateClient().GetAsync("/healthz");

        response.EnsureSuccessStatusCode();
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    [Theory]
    [InlineData("/css/site.css", "text/css")]
    [InlineData("/js/site.js", "text/javascript")]
    [InlineData("/favicon.svg", "image/svg+xml")]
    public async Task Static_assets_are_served(string path, string contentType)
    {
        var response = await factory.CreateClient().GetAsync(path);

        response.EnsureSuccessStatusCode();
        Assert.Equal(contentType, response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Responses_include_security_headers()
    {
        var response = await factory.CreateClient().GetAsync("/");

        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("DENY", response.Headers.GetValues("X-Frame-Options").Single());
        Assert.Equal("strict-origin-when-cross-origin", response.Headers.GetValues("Referrer-Policy").Single());
    }

    private WebApplicationFactory<Program> WithProfile(Profile profile) =>
        factory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services => services.AddSingleton(profile)));

    // Razor encodes characters like "–" and "&" as entities; decode so assertions can use plain text.
    private static async Task<string> GetDecodedHtml(WebApplicationFactory<Program> app, string path)
    {
        var response = await app.CreateClient().GetAsync(path);
        response.EnsureSuccessStatusCode();
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
        return WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
    }
}
