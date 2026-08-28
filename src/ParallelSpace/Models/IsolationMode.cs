namespace ParallelSpace.Models;

/// <summary>
/// How a space keeps its clone separate from other clones of the same app.
/// </summary>
public enum IsolationMode
{
    /// <summary>
    /// Launch the target as-is. Used when the target is already self-isolating
    /// (e.g. the three Discord release channels, each of which has its own data
    /// folder and login), or when you simply want another window of one app.
    /// </summary>
    None = 0,

    /// <summary>
    /// Give the clone its own private profile: the child process is started with
    /// APPDATA / LOCALAPPDATA / USERPROFILE / TEMP redirected into the space's
    /// data folder, and (optionally) a <c>--user-data-dir</c> argument appended.
    /// Works for portable apps and well-behaved Chromium/Electron apps.
    /// </summary>
    IsolatedProfile = 1,
}
