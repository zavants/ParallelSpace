using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ParallelSpace.Models;

namespace ParallelSpace.Services;

/// <summary>
/// Known apps and the launch recipe that gives each one a separate profile.
///
/// The recipes are per-app on purpose: a Chromium browser takes --user-data-dir,
/// Firefox takes -profile, Telegram takes -many, and Discord takes none of them
/// (hence its separate release channels). Detection is read-only — nothing here
/// starts a program.
/// </summary>
public static class AppCatalog
{
    private static string Local => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    private static string Roaming => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    private static string Programs => Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
    private static string ProgramsX86 => Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

    public static IReadOnlyList<AppPreset> Presets { get; } = BuildPresets();

    private static AppPreset[] BuildPresets() =>
    [
        // ---- Browsers: Chromium's --user-data-dir is the gold standard -------
        new("chrome", "Google Chrome", "Browsers",
            [
                Path.Combine(Programs, "Google", "Chrome", "Application", "chrome.exe"),
                Path.Combine(ProgramsX86, "Google", "Chrome", "Application", "chrome.exe"),
                Path.Combine(Local, "Google", "Chrome", "Application", "chrome.exe"),
            ],
            "--user-data-dir=\"{DATA}\"", MultiTechnique.ProfileFlag, "🌐", "#4285F4",
            "Each copy gets a full private profile — separate logins, cookies and extensions.",
            "https://www.google.com/chrome/"),

        new("edge", "Microsoft Edge", "Browsers",
            [
                Path.Combine(ProgramsX86, "Microsoft", "Edge", "Application", "msedge.exe"),
                Path.Combine(Programs, "Microsoft", "Edge", "Application", "msedge.exe"),
                // Edge 151+ moved to a versioned EdgeCore folder.
                Path.Combine(ProgramsX86, "Microsoft", "EdgeCore", "*", "msedge.exe"),
                Path.Combine(Programs, "Microsoft", "EdgeCore", "*", "msedge.exe"),
            ],
            "--user-data-dir=\"{DATA}\"", MultiTechnique.ProfileFlag, "🌊", "#0F7CC0",
            "Each copy gets a full private profile — separate logins, cookies and extensions.",
            "https://www.microsoft.com/edge"),

        new("brave", "Brave", "Browsers",
            [
                Path.Combine(Programs, "BraveSoftware", "Brave-Browser", "Application", "brave.exe"),
                Path.Combine(ProgramsX86, "BraveSoftware", "Brave-Browser", "Application", "brave.exe"),
                Path.Combine(Local, "BraveSoftware", "Brave-Browser", "Application", "brave.exe"),
            ],
            "--user-data-dir=\"{DATA}\"", MultiTechnique.ProfileFlag, "🦁", "#FB542B",
            "Each copy gets a full private profile — separate logins, cookies and extensions.",
            "https://brave.com/download/"),

        new("firefox", "Firefox", "Browsers",
            [
                Path.Combine(Programs, "Mozilla Firefox", "firefox.exe"),
                Path.Combine(ProgramsX86, "Mozilla Firefox", "firefox.exe"),
            ],
            "-no-remote -profile \"{DATA}\"", MultiTechnique.ProfileFlag, "🦊", "#FF7139",
            "-no-remote lets a second copy start; -profile gives it its own logins.",
            "https://www.mozilla.org/firefox/new/"),

        // ---- Chat / messaging -----------------------------------------------
        new("telegram", "Telegram Desktop", "Chat",
            [
                Path.Combine(Roaming, "Telegram Desktop", "Telegram.exe"),
                Path.Combine(Programs, "Telegram Desktop", "Telegram.exe"),
                Path.Combine(Local, "Programs", "Telegram Desktop", "Telegram.exe"),
            ],
            "-many -workdir \"{DATA}\"", MultiTechnique.MultiInstanceFlag, "✈️", "#2AABEE",
            "-many permits extra instances; -workdir keeps each one's account separate.",
            "https://desktop.telegram.org/"),

        new("signal", "Signal", "Chat",
            [
                Path.Combine(Local, "Programs", "signal-desktop", "Signal.exe"),
            ],
            "--user-data-dir=\"{DATA}\"", MultiTechnique.ProfileFlag, "🔒", "#3A76F0",
            "Signal is Electron and honours --user-data-dir for a separate registration.",
            "https://signal.org/download/"),

        new("element", "Element", "Chat",
            [
                Path.Combine(Local, "Programs", "Element", "Element.exe"),
                Path.Combine(Programs, "Element", "Element.exe"),
            ],
            "--profile-dir=\"{DATA}\"", MultiTechnique.ProfileFlag, "💬", "#0DBD8B",
            "Element Desktop takes --profile-dir, so each copy signs in separately.",
            "https://element.io/download"),

        new("slack", "Slack", "Chat",
            [Path.Combine(Local, "slack", "slack.exe")],
            "--user-data-dir=\"{DATA}\"", MultiTechnique.ProfileFlag, "💼", "#4A154B",
            "Electron honours --user-data-dir, which also re-keys Slack's single-instance lock.",
            "https://slack.com/downloads/windows")
            { SquirrelLayout = true },

        // ---- Editors / tools -------------------------------------------------
        new("vscode", "Visual Studio Code", "Tools",
            [
                Path.Combine(Local, "Programs", "Microsoft VS Code", "Code.exe"),
                Path.Combine(Programs, "Microsoft VS Code", "Code.exe"),
            ],
            @"--user-data-dir ""{DATA}\user"" --extensions-dir ""{DATA}\ext""",
            MultiTechnique.ProfileFlag, "🧑‍💻", "#2F80ED",
            "Separate settings, signed-in account and extension set per copy.",
            "https://code.visualstudio.com/Download"),

        new("obsidian", "Obsidian", "Tools",
            [Path.Combine(Local, "Obsidian", "Obsidian.exe")],
            "--user-data-dir=\"{DATA}\"", MultiTechnique.ProfileFlag, "🗂️", "#7C3AED",
            "Electron honours --user-data-dir, so each copy keeps its own vault list and settings.",
            "https://obsidian.md/download"),

        new("qbittorrent", "qBittorrent", "Tools",
            [
                Path.Combine(Programs, "qBittorrent", "qbittorrent.exe"),
                Path.Combine(ProgramsX86, "qBittorrent", "qbittorrent.exe"),
            ],
            "--profile=\"{DATA}\"", MultiTechnique.ProfileFlag, "⬇️", "#2F67BA",
            "--profile gives each copy its own settings and torrent queue.",
            "https://www.qbittorrent.org/download"),

        new("thunderbird", "Thunderbird", "Mail",
            [
                Path.Combine(Programs, "Mozilla Thunderbird", "thunderbird.exe"),
                Path.Combine(ProgramsX86, "Mozilla Thunderbird", "thunderbird.exe"),
            ],
            "-no-remote -profile \"{DATA}\"", MultiTechnique.ProfileFlag, "📨", "#0A84FF",
            "-no-remote plus -profile runs a second copy with its own mail accounts.",
            "https://www.thunderbird.net/"),
    ];

    /// <summary>Resolves a preset's executable, or null when it isn't installed.</summary>
    public static string? ResolveExe(AppPreset preset)
    {
        foreach (var candidate in preset.ExeCandidates)
        {
            if (string.IsNullOrWhiteSpace(candidate))
                continue;

            if (candidate.Contains('*', StringComparison.Ordinal))
            {
                var resolved = ResolveVersioned(candidate);
                if (resolved is not null)
                    return resolved;
                continue;
            }

            if (preset.SquirrelLayout)
            {
                var resolved = ResolveSquirrel(candidate);
                if (resolved is not null)
                    return resolved;
                continue;
            }

            try
            {
                if (File.Exists(candidate))
                    return candidate;
            }
            catch { }
        }
        return null;
    }

    public static bool IsInstalled(AppPreset preset) => ResolveExe(preset) is not null;

    /// <summary>
    /// Resolves a path with one wildcard directory segment (e.g. EdgeCore\*\msedge.exe),
    /// preferring the highest version number.
    /// </summary>
    private static string? ResolveVersioned(string pattern)
    {
        try
        {
            var starIndex = pattern.IndexOf('*');
            var head = pattern[..starIndex].TrimEnd(Path.DirectorySeparatorChar);
            var afterStar = pattern[starIndex..];
            var sep = afterStar.IndexOf(Path.DirectorySeparatorChar);
            if (sep < 0 || !Directory.Exists(head))
                return null;

            var mask = afterStar[..sep];
            var tail = afterStar[(sep + 1)..];

            return Directory.GetDirectories(head, mask)
                .Select(d => new { Exe = Path.Combine(d, tail), Ver = ParseVersion(Path.GetFileName(d)) })
                .Where(x => File.Exists(x.Exe))
                .OrderByDescending(x => x.Ver)
                .Select(x => x.Exe)
                .FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Squirrel installs keep a stub at the root and the real exe under app-&lt;version&gt;.
    /// Prefer the newest versioned copy, else the stub.
    /// </summary>
    private static string? ResolveSquirrel(string stubPath)
    {
        try
        {
            var root = Path.GetDirectoryName(stubPath);
            var exeName = Path.GetFileName(stubPath);
            if (root is null || !Directory.Exists(root))
                return null;

            var newest = Directory.GetDirectories(root, "app-*")
                .Select(d => new { Exe = Path.Combine(d, exeName), Ver = ParseVersion(Path.GetFileName(d)) })
                .Where(x => File.Exists(x.Exe))
                .OrderByDescending(x => x.Ver)
                .FirstOrDefault();

            if (newest is not null)
                return newest.Exe;

            return File.Exists(stubPath) ? stubPath : null;
        }
        catch
        {
            return null;
        }
    }

    internal static Version ParseVersion(string appDirName)
    {
        var raw = appDirName.StartsWith("app-", StringComparison.OrdinalIgnoreCase)
            ? appDirName[4..]
            : appDirName;
        return Version.TryParse(raw, out var v) ? v : new Version(0, 0);
    }
}
