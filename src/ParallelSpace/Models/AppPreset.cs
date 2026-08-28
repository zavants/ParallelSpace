namespace ParallelSpace.Models;

/// <summary>
/// How a given app can be made to run more than once with separate accounts.
/// Different apps need genuinely different tricks, so each preset says which.
/// </summary>
public enum MultiTechnique
{
    /// <summary>App takes a flag pointing its whole profile at a folder we choose
    /// (Chromium's <c>--user-data-dir</c>, Firefox's <c>-profile</c>, …). Most reliable.</summary>
    ProfileFlag = 0,

    /// <summary>App ships separate builds/channels, each with its own data folder
    /// and therefore its own account (Discord Stable / PTB / Canary).</summary>
    SeparateBuild = 1,

    /// <summary>App has a flag that merely permits a second instance.</summary>
    MultiInstanceFlag = 2,

    /// <summary>No flag: we redirect the environment so the copy sees a private
    /// profile. Works for portable and well-behaved apps; see the note on each.</summary>
    IsolatedProfile = 3,
}

/// <summary>
/// A known app plus the launch recipe that gives it a separate profile.
/// <see cref="Arguments"/> may contain <c>{DATA}</c>, replaced at launch with the
/// space's private data folder.
/// </summary>
public sealed record AppPreset(
    string Key,
    string Display,
    string Category,
    string[] ExeCandidates,
    string Arguments,
    MultiTechnique Technique,
    string Glyph,
    string Accent,
    string Note,
    string DownloadUrl = "")
{
    /// <summary>Squirrel-style installs keep the real exe in a versioned app-* folder.</summary>
    public bool SquirrelLayout { get; init; }

    public string TechniqueLabel => Technique switch
    {
        MultiTechnique.ProfileFlag => "Profile flag",
        MultiTechnique.SeparateBuild => "Separate builds",
        MultiTechnique.MultiInstanceFlag => "Multi-instance flag",
        _ => "Isolated profile",
    };
}
