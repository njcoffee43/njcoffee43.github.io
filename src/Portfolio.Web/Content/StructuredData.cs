using System.Text.Json;

namespace Portfolio.Web.Content;

/// <summary>
/// schema.org Person data for search engines, embedded in the home page as JSON-LD.
/// </summary>
public static class StructuredData
{
    public static string PersonJsonLd(Profile profile)
    {
        var person = new Dictionary<string, object>
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "Person",
            ["name"] = profile.Name,
            ["jobTitle"] = profile.Headline,
            ["email"] = $"mailto:{profile.Links.Email}",
        };

        if (profile.Site.Url.Length > 0)
        {
            person["url"] = profile.Site.Url;
        }

        var sameAs = new[] { profile.Links.GitHub, profile.Links.LinkedIn }.Where(url => url.Length > 0).ToArray();
        if (sameAs.Length > 0)
        {
            person["sameAs"] = sameAs;
        }

        if (profile.Education.Count > 0)
        {
            person["alumniOf"] = profile.Education
                .Select(e => new Dictionary<string, string> { ["@type"] = "CollegeOrUniversity", ["name"] = e.School })
                .ToArray();
        }

        // The default encoder escapes < and >, so the output is safe inside a <script> element.
        return JsonSerializer.Serialize(person);
    }
}
