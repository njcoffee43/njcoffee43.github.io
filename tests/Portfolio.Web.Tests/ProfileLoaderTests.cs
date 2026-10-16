using Portfolio.Web.Content;

namespace Portfolio.Web.Tests;

public class ProfileLoaderTests
{
    private const string MinimalJson = """
        {
          "name": "Ada Lovelace",
          "headline": "Software Engineer",
          "summary": "Writes programs.",
          "links": { "email": "ada@example.com" }
        }
        """;

    private static readonly Profile Minimal = ProfileLoader.Parse(MinimalJson);

    [Fact]
    public void Parses_a_minimal_profile_with_defaults()
    {
        Assert.Equal("Ada Lovelace", Minimal.Name);
        Assert.Equal("", Minimal.Links.GitHub);
        Assert.Empty(Minimal.Projects);
        Assert.Empty(ProfileLoader.Validate(Minimal));
    }

    [Fact]
    public void Accepts_comments_and_trailing_commas()
    {
        var profile = ProfileLoader.Parse("""
            {
              // hand-edited file
              "name": "Ada Lovelace",
              "headline": "Software Engineer",
              "summary": "Writes programs.",
              "links": { "email": "ada@example.com", },
            }
            """);

        Assert.Equal("Ada Lovelace", profile.Name);
    }

    [Fact]
    public void Rejects_misspelled_keys()
    {
        var json = MinimalJson.Replace("\"summary\"", "\"sumary\"");

        var ex = Assert.Throws<InvalidOperationException>(() => ProfileLoader.Parse(json));

        Assert.Contains("sumary", ex.Message);
    }

    [Fact]
    public void Rejects_missing_required_fields()
    {
        var json = MinimalJson.Replace("\"headline\": \"Software Engineer\",", "");

        var ex = Assert.Throws<InvalidOperationException>(() => ProfileLoader.Parse(json));

        Assert.Contains("headline", ex.Message);
    }

    [Fact]
    public void Reports_every_problem_at_once()
    {
        var json = MinimalJson
            .Replace("\"Ada Lovelace\"", "\"  \"")
            .Replace("\"ada@example.com\"", "\"ada.example.com\", \"github\": \"github.com/ada\"");

        var ex = Assert.Throws<InvalidOperationException>(() => ProfileLoader.Parse(json));

        Assert.Contains("3 problem(s)", ex.Message);
        Assert.Contains("name is required", ex.Message);
        Assert.Contains("links.email", ex.Message);
        Assert.Contains("links.github", ex.Message);
    }

    [Theory]
    [InlineData("resume.pdf", true)]
    [InlineData("/files/resume.pdf", true)]
    [InlineData("https://example.com/resume.pdf", true)]
    [InlineData("ftp://example.com/resume.pdf", false)]
    public void Resume_can_be_a_local_path_or_web_url(string resume, bool valid)
    {
        var profile = Minimal with { Links = Minimal.Links with { Resume = resume } };

        Assert.Equal(valid, ProfileLoader.Validate(profile).Count == 0);
    }

    [Theory]
    [InlineData("https://github.com/ada", true)]
    [InlineData("github.com/ada", false)]
    [InlineData("javascript:alert(1)", false)]
    public void Project_links_must_be_web_urls(string url, bool valid)
    {
        var project = new Project { Title = "Engine", Summary = "Computes things.", RepoUrl = url };
        var profile = Minimal with { Projects = [project] };

        Assert.Equal(valid, ProfileLoader.Validate(profile).Count == 0);
    }

    [Fact]
    public void Load_reports_a_missing_file()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => ProfileLoader.Load("no-such-file.json"));

        Assert.Contains("not found", ex.Message);
    }
}
