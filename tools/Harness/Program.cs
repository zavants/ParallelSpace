using System.Diagnostics;
using ParallelSpace.Models;
using ParallelSpace.Services;

// Functional check of the real service layer (no UI).
// 1) Discord channel detection + exe resolution
// 2) Isolated-profile launch of a harmless app (notepad): env redirect,
//    folder creation, process tracking, status, close.

Console.WriteLine("=== Discord channel detection ===");
foreach (var ch in DiscordDetector.Channels)
{
    var installed = DiscordDetector.IsInstalled(ch);
    var exe = DiscordDetector.ResolveExe(ch);
    Console.WriteLine($"  {ch.Display,-16} installed={installed,-5} exe={exe ?? "(none)"}");
    Console.WriteLine($"  {"",16} data ={DiscordDetector.DataPath(ch)}");
}

// Target defaults to notepad; pass an exe to exercise a specific launch pattern
// (e.g. the stub-then-child pattern that job tracking exists to handle).
var target = args.Length > 0 ? args[0] : Path.Combine(Environment.SystemDirectory, "notepad.exe");
var targetName = Path.GetFileNameWithoutExtension(target);

Console.WriteLine();
Console.WriteLine($"=== Isolated-profile launch ({targetName}) ===");

var store = new SpaceStore();
var launcher = new Launcher(store);

var space = new Space
{
    Name = $"{targetName} (isolated test)",
    ExecutablePath = target,
    Isolation = IsolationMode.IsolatedProfile,
    AppendUserDataDir = false, // plain exes choke on unknown args
    Kind = "custom",
};

var dataFolder = store.DataFolder(space);
Console.WriteLine($"  data folder: {dataFolder}");

var result = launcher.Launch(space);
Console.WriteLine($"  launch ok={result.Ok} error={result.Error ?? "-"}");

Thread.Sleep(2000);
var status = launcher.GetStatus(space);
Console.WriteLine($"  status running={status.Running} instances={status.Instances}");

var home = Path.Combine(dataFolder, "Home");
var roaming = Path.Combine(home, "AppData", "Roaming");
Console.WriteLine($"  redirected profile tree created: Home={Directory.Exists(home)} Roaming={Directory.Exists(roaming)}");

var live = Process.GetProcessesByName(targetName).Length;
Console.WriteLine($"  '{targetName}' processes on box: {live}");

launcher.CloseAll(space);
Thread.Sleep(1500);
var after = launcher.GetStatus(space);
var liveAfter = Process.GetProcessesByName(targetName).Length;
Console.WriteLine($"  after close running={after.Running} instances={after.Instances} processes={liveAfter}");

// Leave no test profile behind.
launcher.Dispose();
try { Directory.Delete(dataFolder, recursive: true); } catch { }

Console.WriteLine();
Console.WriteLine("DONE");
