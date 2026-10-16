namespace Portfolio.Web.Content;

// Shape of Content/profile.json. Everything the site shows comes from here.

public sealed record Profile
{
    public required string Name { get; init; }
    public required string Headline { get; init; }
    public required string Summary { get; init; }
    public string Location { get; init; } = "";
    public string Availability { get; init; } = "";
    public IReadOnlyList<string> About { get; init; } = [];
    public required ProfileLinks Links { get; init; }
    public SiteInfo Site { get; init; } = new();
    public IReadOnlyList<Job> Experience { get; init; } = [];
    public IReadOnlyList<Project> Projects { get; init; } = [];
    public IReadOnlyList<SkillGroup> Skills { get; init; } = [];
    public IReadOnlyList<Education> Education { get; init; } = [];
    public IReadOnlyList<Certification> Certifications { get; init; } = [];
}

public sealed record ProfileLinks
{
    public required string Email { get; init; }
    public string GitHub { get; init; } = "";
    public string LinkedIn { get; init; } = "";

    /// <summary>A path under wwwroot (e.g. "resume.pdf") or an absolute URL.</summary>
    public string Resume { get; init; } = "";
}

public sealed record SiteInfo
{
    /// <summary>Public URL of the deployed site, used for canonical and Open Graph tags.</summary>
    public string Url { get; init; } = "";

    /// <summary>Link to this site's source code, shown in the footer.</summary>
    public string SourceCode { get; init; } = "";
}

public sealed record Job
{
    public required string Role { get; init; }
    public required string Company { get; init; }
    public string Location { get; init; } = "";
    public required string Period { get; init; }
    public string Summary { get; init; } = "";
    public IReadOnlyList<string> Highlights { get; init; } = [];
    public IReadOnlyList<string> Tags { get; init; } = [];
}

public sealed record Project
{
    public required string Title { get; init; }
    public string Kind { get; init; } = "";
    public string Period { get; init; } = "";
    public required string Summary { get; init; }
    public IReadOnlyList<string> Highlights { get; init; } = [];
    public IReadOnlyList<string> Tags { get; init; } = [];
    public string RepoUrl { get; init; } = "";
    public string LiveUrl { get; init; } = "";
}

public sealed record SkillGroup
{
    public required string Name { get; init; }
    public required IReadOnlyList<string> Items { get; init; }
}

public sealed record Education
{
    public required string School { get; init; }
    public required string Degree { get; init; }
    public string Period { get; init; } = "";
    public IReadOnlyList<string> Details { get; init; } = [];
}

public sealed record Certification
{
    public required string Name { get; init; }
    public string Issued { get; init; } = "";
    public string Url { get; init; } = "";
}
