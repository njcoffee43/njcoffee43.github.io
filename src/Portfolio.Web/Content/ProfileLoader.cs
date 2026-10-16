using System.Text.Json;
using System.Text.Json.Serialization;

namespace Portfolio.Web.Content;

/// <summary>
/// Reads and checks profile.json at startup, so a typo or a broken link fails
/// the build/tests instead of showing up on the live site.
/// </summary>
public static class ProfileLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        // Catches misspelled keys, which would otherwise be silently ignored.
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    };

    public static Profile Load(string path)
    {
        if (!File.Exists(path))
        {
            throw new InvalidOperationException($"Profile file not found: {path}");
        }

        return Parse(File.ReadAllText(path), path);
    }

    public static Profile Parse(string json, string source = "profile.json")
    {
        Profile? profile;
        try
        {
            profile = JsonSerializer.Deserialize<Profile>(json, JsonOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"{source} is not valid: {ex.Message}", ex);
        }

        if (profile is null)
        {
            throw new InvalidOperationException($"{source} is empty.");
        }

        var errors = Validate(profile);
        if (errors.Count > 0)
        {
            throw new InvalidOperationException(
                $"{source} has {errors.Count} problem(s):{Environment.NewLine}- " +
                string.Join($"{Environment.NewLine}- ", errors));
        }

        return profile;
    }

    public static IReadOnlyList<string> Validate(Profile profile)
    {
        var errors = new List<string>();

        void Required(string? value, string field)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                errors.Add($"{field} is required.");
            }
        }

        void AbsoluteUrl(string? value, string field)
        {
            if (!string.IsNullOrEmpty(value) && !IsHttpUrl(value))
            {
                errors.Add($"{field} must be a full http(s) URL, but was \"{value}\".");
            }
        }

        Required(profile.Name, "name");
        Required(profile.Headline, "headline");
        Required(profile.Summary, "summary");

        Required(profile.Links.Email, "links.email");
        if (!string.IsNullOrWhiteSpace(profile.Links.Email) && !profile.Links.Email.Contains('@'))
        {
            errors.Add($"links.email doesn't look like an email address: \"{profile.Links.Email}\".");
        }

        AbsoluteUrl(profile.Links.GitHub, "links.github");
        AbsoluteUrl(profile.Links.LinkedIn, "links.linkedin");
        if (profile.Links.Resume?.Contains("://") == true && !IsHttpUrl(profile.Links.Resume))
        {
            errors.Add($"links.resume must be a path under wwwroot or a full http(s) URL, but was \"{profile.Links.Resume}\".");
        }

        AbsoluteUrl(profile.Site.Url, "site.url");
        AbsoluteUrl(profile.Site.SourceCode, "site.sourceCode");

        for (var i = 0; i < profile.Experience.Count; i++)
        {
            var job = profile.Experience[i];
            Required(job.Role, $"experience[{i}].role");
            Required(job.Company, $"experience[{i}].company");
            Required(job.Period, $"experience[{i}].period");
        }

        for (var i = 0; i < profile.Projects.Count; i++)
        {
            var project = profile.Projects[i];
            Required(project.Title, $"projects[{i}].title");
            Required(project.Summary, $"projects[{i}].summary");
            AbsoluteUrl(project.RepoUrl, $"projects[{i}].repoUrl");
            AbsoluteUrl(project.LiveUrl, $"projects[{i}].liveUrl");
        }

        for (var i = 0; i < profile.Certifications.Count; i++)
        {
            AbsoluteUrl(profile.Certifications[i].Url, $"certifications[{i}].url");
        }

        return errors;
    }

    private static bool IsHttpUrl(string value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);
}
