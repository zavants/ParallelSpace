using System.Diagnostics;

// Test fixture for the launcher's process tracking.
//
// It imitates a Squirrel/Electron style launcher: with no arguments it spawns the
// "real" app (itself, with "child") and exits immediately. That is the pattern that
// used to make a running space report "Stopped" and leave nothing to close, because
// assigning the job *after* Process.Start loses the race against the stub.
//
// Run it through the harness to check the behaviour end to end:
//   dotnet run --project tools/Harness -c Release -- "<path to StubApp.exe>"
// Expect: status running=True, and processes=0 after close.

if (args.Length > 0 && args[0] == "child")
{
    Thread.Sleep(TimeSpan.FromMinutes(5));
    return;
}

var exe = Environment.ProcessPath!;
Process.Start(new ProcessStartInfo { FileName = exe, Arguments = "child", UseShellExecute = false });
// The stub exits right away — that is the whole point.
