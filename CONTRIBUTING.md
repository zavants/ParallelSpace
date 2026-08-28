# Contributing

Thanks for taking a look. The most useful contribution by far is **a new app recipe**
— see below, it's a handful of lines.

## Building

You need the **.NET 9 SDK** and Windows 10/11 (x64).

```powershell
git clone https://github.com/dboudreau00/ParallelSpace.git
cd ParallelSpace

powershell -ExecutionPolicy Bypass -File tools/run.ps1       # build + run
powershell -ExecutionPolicy Bypass -File tools/build.ps1     # build Release
powershell -ExecutionPolicy Bypass -File tools/package.ps1   # zip a release into dist/
```

`tools/Harness` is a small console app that exercises the service layer without the
UI — app detection, an isolated launch, status tracking and close:

```powershell
dotnet run --project tools/Harness -c Release            # uses notepad
dotnet run --project tools/Harness -c Release -- "C:\path\to\some.exe"
```

## Adding an app to the catalog

Every app needs a different trick to run twice, so each entry in
[`AppCatalog.cs`](src/ParallelSpace/Services/AppCatalog.cs) carries the recipe that
works for it. Add one entry:

```csharp
new("obsidian", "Obsidian", "Tools",
    [Path.Combine(Local, "Obsidian", "Obsidian.exe")],   // where to look for it
    "",                                                  // launch arguments
    MultiTechnique.IsolatedProfile,                      // how it separates
    "🗂️", "#7C3AED",                                     // icon + accent
    "Launched with a redirected profile so each copy keeps its own vault list.",
    "https://obsidian.md/download"),
```

- `{DATA}` in the arguments is replaced at launch with that space's private folder.
  That is how a profile flag is pointed somewhere isolated, e.g.
  `--user-data-dir="{DATA}"`.
- A path may contain one `*` directory segment; the highest version wins. Useful for
  installers that use a versioned folder (Edge's `EdgeCore\<version>\msedge.exe`).
- Set `SquirrelLayout = true` for installers that keep the real exe under `app-<version>`.
- Pick the technique honestly:
  | Technique | When |
  |---|---|
  | `ProfileFlag` | the app takes a flag that relocates its whole profile — most reliable |
  | `MultiInstanceFlag` | a flag permits a second instance |
  | `SeparateBuild` | separate installs/channels each keep their own account |
  | `IsolatedProfile` | no flag; we redirect the environment instead |

**Please only add a recipe you have actually run.** A wrong recipe is worse than a
missing one — it looks supported and then silently shares one account. Say in the PR
what you tested it on.

## Style

Match the surrounding code: standard .NET conventions, terse comments that explain
*why* rather than restating the code, and no new dependencies without a good reason.

## Reporting a bug

Include your Windows version, the app you were cloning (and its version), and what
the space card showed versus what actually happened.
