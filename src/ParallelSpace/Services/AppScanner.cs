using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ParallelSpace.Services;

public sealed record DetectedApp(string Name, string ExePath);

/// <summary>
/// Enumerates installed apps by resolving Start Menu shortcuts (read-only).
/// This never launches anything — it only reads .lnk targets.
/// </summary>
public static class AppScanner
{
    private static readonly string[] SkipWords =
    {
        "uninstall", "update", "setup", "installer", "repair", "readme",
        "documentation", "help", "website", "web site", "report a", "crash",
        "adapter", "administrative", "command prompt", "powershell",
    };

    private static readonly string WindowsDir =
        Environment.GetFolderPath(Environment.SpecialFolder.Windows);

    public static List<DetectedApp> Scan()
    {
        var roots = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Microsoft", "Windows", "Start Menu", "Programs"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "Microsoft", "Windows", "Start Menu", "Programs"),
        };

        var byPath = new Dictionary<string, DetectedApp>(StringComparer.OrdinalIgnoreCase);

        object? shell = null;
        Type? shellType = Type.GetTypeFromProgID("WScript.Shell");
        if (shellType is not null)
            shell = Activator.CreateInstance(shellType);

        try
        {
            foreach (var root in roots)
            {
                if (!Directory.Exists(root))
                    continue;

                // Enumeration is lazy, so a try around this call would not catch a
                // mid-walk access error — skip inaccessible entries instead.
                IEnumerable<string> links;
                try
                {
                    links = Directory.EnumerateFiles(root, "*.lnk", new EnumerationOptions
                    {
                        RecurseSubdirectories = true,
                        IgnoreInaccessible = true,
                        AttributesToSkip = 0,
                    });
                }
                catch { continue; }

                // A throw mid-walk ends this root only, never the whole scan.
                try
                {
                    foreach (var lnk in links)
                    {
                        try
                        {
                            var name = Path.GetFileNameWithoutExtension(lnk);
                            if (ShouldSkip(name))
                                continue;

                            var target = ResolveTarget(shellType, shell, lnk);
                            if (string.IsNullOrEmpty(target))
                                continue;
                            if (!target.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                                continue;
                            if (!File.Exists(target))
                                continue;
                            if (ShouldSkip(Path.GetFileNameWithoutExtension(target)))
                                continue;
                            // Skip Windows built-ins (admin tools, accessories, applets) — noise.
                            if (!string.IsNullOrEmpty(WindowsDir) &&
                                target.StartsWith(WindowsDir, StringComparison.OrdinalIgnoreCase))
                                continue;

                            byPath.TryAdd(target, new DetectedApp(name, target));
                        }
                        catch { }
                    }
                }
                catch { }
            }
        }
        finally
        {
            if (shell is not null)
            {
                try { System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shell); }
                catch { }
            }
        }

        return byPath.Values
            .OrderBy(a => a.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    private static bool ShouldSkip(string name)
    {
        var lower = name.ToLowerInvariant();
        return SkipWords.Any(w => lower.Contains(w));
    }

    private static string? ResolveTarget(Type? shellType, object? shell, string lnkPath)
    {
        if (shellType is null || shell is null)
            return null;
        try
        {
            var shortcut = shellType.InvokeMember("CreateShortcut",
                BindingFlags.InvokeMethod, null, shell, new object[] { lnkPath });
            if (shortcut is null)
                return null;

            var target = shortcut.GetType().InvokeMember("TargetPath",
                BindingFlags.GetProperty, null, shortcut, null) as string;

            try { System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shortcut); }
            catch { }

            return target;
        }
        catch
        {
            return null;
        }
    }
}
