using System;
using System.IO;
using System.Linq;

namespace ParallelSpace.Services;

/// <summary>
/// Locates the installed Discord release channels. Each channel keeps its data
/// in a separate <c>%APPDATA%\discord*</c> folder, so signing three different
/// accounts into Stable / PTB / Canary is fully supported with no trickery.
/// </summary>
public static class DiscordDetector
{
    public sealed record Channel(
        string Key,        // "stable" | "ptb" | "canary"
        string Display,    // "Discord", "Discord PTB", "Discord Canary"
        string FolderName, // "Discord", "DiscordPTB", "DiscordCanary"
        string ExeName,    // "Discord.exe", "DiscordPTB.exe", "DiscordCanary.exe"
        string ProcessName,// "Discord", "DiscordPTB", "DiscordCanary"
        string DataFolder, // "discord", "discordptb", "discordcanary"
        string DownloadUrl,
        string Glyph,
        string Accent);

    public static readonly Channel[] Channels =
    {
        new("stable", "Discord",        "Discord",       "Discord.exe",       "Discord",
            "discord",       "https://discord.com/download",                 "🎮", "#5865F2"),
        new("ptb",    "Discord PTB",    "DiscordPTB",    "DiscordPTB.exe",    "DiscordPTB",
            "discordptb",    "https://discord.com/api/download/ptb?platform=win",    "🧪", "#3BA55D"),
        new("canary", "Discord Canary", "DiscordCanary", "DiscordCanary.exe", "DiscordCanary",
            "discordcanary", "https://discord.com/api/download/canary?platform=win", "🐤", "#FBB848"),
    };

    public static Channel? Get(string key) =>
        Channels.FirstOrDefault(c => string.Equals(c.Key, key, StringComparison.OrdinalIgnoreCase));

    private static string LocalAppData =>
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    private static string InstallRoot(Channel c) => Path.Combine(LocalAppData, c.FolderName);

    public static bool IsInstalled(Channel c) => ResolveExe(c) is not null;

    /// <summary>
    /// Returns the best executable to launch for a channel, or null if not installed.
    /// Prefers the newest versioned <c>app-*\{Exe}</c>, falling back to Update.exe.
    /// </summary>
    public static string? ResolveExe(Channel c)
    {
        var root = InstallRoot(c);
        if (!Directory.Exists(root))
            return null;

        try
        {
            var newest = Directory.GetDirectories(root, "app-*")
                .Select(d => new
                {
                    Dir = d,
                    Exe = Path.Combine(d, c.ExeName),
                    Ver = ParseVersion(Path.GetFileName(d))
                })
                .Where(x => File.Exists(x.Exe))
                .OrderByDescending(x => x.Ver)
                .FirstOrDefault();

            if (newest is not null)
                return newest.Exe;
        }
        catch
        {
            // fall through to Update.exe
        }

        var update = Path.Combine(root, "Update.exe");
        return File.Exists(update) ? update : null;
    }

    /// <summary>The channel's own data folder under Roaming AppData (its account/login).</summary>
    public static string DataPath(Channel c) =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), c.DataFolder);

    private static Version ParseVersion(string appDirName)
    {
        // "app-1.0.9251" -> 1.0.9251
        var raw = appDirName.StartsWith("app-", StringComparison.OrdinalIgnoreCase)
            ? appDirName[4..]
            : appDirName;
        return Version.TryParse(raw, out var v) ? v : new Version(0, 0);
    }
}
