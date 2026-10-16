using System.Text.Json;
using Portfolio.Web.Content;

namespace Portfolio.Web.Tests;

public class StructuredDataTests
{
    private static readonly Profile Profile = new()
    {
        Name = "Ada Lovelace",
        Headline = "Software Engineer",
        Summary = "Writes programs.",
        Links = new ProfileLinks { Email = "ada@example.com", GitHub = "https://github.com/ada" },
        Education = [new Education { School = "University of London", Degree = "Mathematics" }],
    };

    [Fact]
    public void Describes_the_person_with_only_the_links_that_are_set()
    {
        using var doc = JsonDocument.Parse(StructuredData.PersonJsonLd(Profile));
        var root = doc.RootElement;

        Assert.Equal("https://schema.org", root.GetProperty("@context").GetString());
        Assert.Equal("Ada Lovelace", root.GetProperty("name").GetString());
        Assert.Equal(new[] { "https://github.com/ada" }, root.GetProperty("sameAs").EnumerateArray().Select(e => e.GetString()));
        Assert.Equal("University of London", root.GetProperty("alumniOf")[0].GetProperty("name").GetString());
        Assert.False(root.TryGetProperty("url", out _));
    }

    [Fact]
    public void Output_cannot_close_its_script_tag()
    {
        var json = StructuredData.PersonJsonLd(Profile with { Name = "</script><script>alert(1)</script>" });

        Assert.DoesNotContain("</script>", json, StringComparison.OrdinalIgnoreCase);
        using var doc = JsonDocument.Parse(json);
        Assert.Equal("</script><script>alert(1)</script>", doc.RootElement.GetProperty("name").GetString());
    }
}
