using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ParallelSpace.Models;

namespace ParallelSpace.Services;

/// <summary>
/// Launches spaces, applies per-space isolation, and tracks running clones.
/// </summary>
public sealed class Launcher : IDisposable
{
    private readonly object _gate = new();
    // One job per launch, so a space can hold several independent copies.
    private readonly Dictionary<string, List<JobObject>> _jobs = new();
    private readonly SpaceStore _store;

    /// <summary>Raised whenever the running set changes (launch / exit / close).</summary>
    public event Action? InstancesChanged;

    public Launcher(SpaceStore? store = null)
    {
        _store = store ?? new SpaceStore();
    }

    public readonly record struct LaunchResult(bool Ok, string? Error)
    {
        public static LaunchResult Success => new(true, null);
        public static LaunchResult Fail(string msg) => new(false, msg);
    }

    public LaunchResult Launch(Space space)
    {
        // 1. Resolve the executable and base arguments.
        string exe;
        string args = space.Arguments?.Trim() ?? "";

        if (!string.IsNullOrEmpty(space.DiscordChannel))
        {
            var channel = DiscordDetector.Get(space.DiscordChannel);
            if (channel is null)
                return LaunchResult.Fail($"Unknown Discord channel '{space.DiscordChannel}'.");

            var resolved = DiscordDetector.ResolveExe(channel);
            if (resolved is null)
                return LaunchResult.Fail($"{channel.Display} is not installed.");

            exe = resolved;

            // If we fell back to Update.exe, it needs --processStart to launch the app.
            if (string.Equals(Path.GetFileName(exe), "Update.exe", StringComparison.OrdinalIgnoreCase))
                args = $"--processStart {channel.ExeName} " + args;
        }
        else if (!string.IsNullOrEmpty(space.PresetKey) &&
                 AppCatalog.Presets.FirstOrDefault(p => p.Key == space.PresetKey) is { } preset &&
                 AppCatalog.ResolveExe(preset) is { } presetExe)
        {
            // Re-resolve each launch so an app update (new versioned folder) still works.
            exe = presetExe;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(space.ExecutablePath) || !File.Exists(space.ExecutablePath))
                return LaunchResult.Fail("Executable not found. Edit the space and pick a valid .exe.");
            exe = space.ExecutablePath;
        }

        var workDir = !string.IsNullOrWhiteSpace(space.WorkingDirectory) && Directory.Exists(space.WorkingDirectory)
            ? space.WorkingDirectory
            : Path.GetDirectoryName(exe) ?? Environment.CurrentDirectory;

        // 2. Isolation.
        Dictionary<string, string>? env = null;
        if (space.Isolation == IsolationMode.IsolatedProfile)
        {
            var dataFolder = _store.DataFolder(space);
            var home = Path.Combine(dataFolder, "Home");
            var roaming = Path.Combine(home, "AppData", "Roaming");
            var local = Path.Combine(home, "AppData", "Local");
            var temp = Path.Combine(local, "Temp");
            foreach (var d in new[] { home, roaming, local, temp })
                Directory.CreateDirectory(d);

            env = new Dictionary<string, string>
            {
                ["USERPROFILE"] = home,
                ["APPDATA"] = roaming,
                ["LOCALAPPDATA"] = local,
                ["TEMP"] = temp,
                ["TMP"] = temp,
            };

            if (space.AppendUserDataDir)
                args = ($"--user-data-dir=\"{Path.Combine(dataFolder, "Chromium")}\" " + args).Trim();
        }

        // {DATA} in the arguments means "this space's private folder" — that is how
        // presets point an app's own profile flag at somewhere isolated.
        if (args.Contains("{DATA}", StringComparison.Ordinal))
        {
            var dir = _store.DataFolder(space);
            Directory.CreateDirectory(dir);
            args = args.Replace("{DATA}", dir, StringComparison.Ordinal);
        }

        // 3. Launch into its own job, so stubs that hand off to a real process
        //    (Squirrel/Electron updaters) stay tracked and closable.
        try
        {
            var job = new JobObject();
            var started = ProcessStarter.StartInJob(exe, args.Trim(), workDir, env, job);
            if (!started.Ok)
            {
                job.Dispose();
                return LaunchResult.Fail(started.Error ?? "The process could not be started.");
            }

            lock (_gate)
            {
                if (!_jobs.TryGetValue(space.Id, out var list))
                    _jobs[space.Id] = list = new List<JobObject>();
                list.Add(job);
            }

            InstancesChanged?.Invoke();
            return LaunchResult.Success;
        }
        catch (Exception ex)
        {
            return LaunchResult.Fail(ex.Message);
        }
    }

    /// <summary>Drops jobs whose processes have all exited. Caller holds _gate.</summary>
    private int PruneAndCountLocked(string spaceId)
    {
        if (!_jobs.TryGetValue(spaceId, out var list))
            return 0;

        int live = 0;
        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (list[i].GetProcessIds().Count > 0)
                live++;
            else
            {
                list[i].Dispose();
                list.RemoveAt(i);
            }
        }
        if (list.Count == 0)
            _jobs.Remove(spaceId);
        return live;
    }

    public readonly record struct Status(bool Running, int Instances);

    public Status GetStatus(Space space)
    {
        // Discord channels self-isolate; detect them by process name so status is
        // accurate even for instances started outside this launcher.
        if (!string.IsNullOrEmpty(space.DiscordChannel))
        {
            var channel = DiscordDetector.Get(space.DiscordChannel);
            if (channel is not null)
            {
                var running = SafeProcessCount(channel.ProcessName) > 0;
                return new Status(running, running ? 1 : 0);
            }
        }

        lock (_gate)
        {
            var count = PruneAndCountLocked(space.Id);
            return new Status(count > 0, count);
        }
    }

    public void CloseAll(Space space)
    {
        if (!string.IsNullOrEmpty(space.DiscordChannel))
        {
            var channel = DiscordDetector.Get(space.DiscordChannel);
            if (channel is not null)
                TaskKillByImage(channel.ExeName);
        }

        List<JobObject> toKill;
        lock (_gate)
        {
            toKill = _jobs.TryGetValue(space.Id, out var list) ? new List<JobObject>(list) : new();
            _jobs.Remove(space.Id);
        }

        // Terminating the job kills that copy's whole tree — and nothing outside it.
        foreach (var job in toKill)
        {
            try { job.TerminateAll(); } catch { }
            job.Dispose();
        }

        InstancesChanged?.Invoke();
    }

    private static int SafeProcessCount(string name)
    {
        try { return Process.GetProcessesByName(name).Length; }
        catch { return 0; }
    }

    private static void TaskKillByImage(string image) => RunHidden("taskkill", $"/IM \"{image}\" /T /F");

    private static void RunHidden(string file, string args)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = file,
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            Process.Start(psi)?.WaitForExit(5000);
        }
        catch { }
    }

    public void Dispose()
    {
        // Intentionally do NOT kill launched clones on exit — the user usually
        // wants their apps to keep running after closing the manager. The jobs
        // carry no kill-on-close limit, so releasing the handles is safe.
        lock (_gate)
        {
            foreach (var list in _jobs.Values)
                foreach (var job in list)
                    job.Dispose();
            _jobs.Clear();
        }
    }
}
