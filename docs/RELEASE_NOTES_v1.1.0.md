# Parallel Space v1.1.0

**Run isolated, parallel copies of any Windows app** — so you can be signed into
several accounts at once. This release turns Parallel Space from a Discord-shaped
tool into a general one.

## ✨ What's new

- **App catalog.** 15 ready-made recipes — Chrome, Edge, Brave, Firefox, Telegram,
  Signal, Element, Slack, VS Code, Obsidian, qBittorrent, Thunderbird, and the three
  Discord channels. Installed apps are detected and added in one click.
- **Per-app techniques.** Every app needs a different trick to run twice, so each
  recipe carries the one that works for it — a profile flag, a multi-instance flag,
  separate builds, or a redirected profile — and the UI tells you which.
- **`{DATA}`** in a space's arguments expands to that space's private folder, so any
  app with its own profile flag can be pointed somewhere isolated:
  `--user-data-dir="{DATA}"`.
- **Six themes** on a new Appearance page — Midnight, Nord, Carbon, Ember, Daylight
  (light) and Matrix — switched live. Every palette is contrast-checked: primary text
  ≥ 7:1 on every surface, secondary ≥ 4.5:1, white on accent ≥ 4.5:1.
- **Versioned install paths**, so apps that live in a versioned folder are found
  (Edge 151's `EdgeCore\<version>\msedge.exe`).
- CI, a contributor guide, and issue/PR templates.

## 🔧 Changed

- Discord moved off the front page into the catalog; the generic **Spaces** page is
  the home screen.
- Space cards state their technique ("Profile flag · own profile") instead of an exe
  name.
- No hardcoded colours remain in the UI — everything follows the theme.

## 📦 Install

1. Download **`ParallelSpace-v1.1.0-win-x64.zip`** below.
2. Unzip and run **`ParallelSpace.exe`**.

Windows 10/11 (x64), no .NET runtime and no admin rights needed. Not code-signed, so
choose **More info → Run anyway** if SmartScreen prompts.

## 🚀 Two of something in 30 seconds

1. **Catalog** → **Add space** on an app marked *Installed*.
2. **Spaces** → **Launch**. Launch it again for a second copy.
3. Sign each copy into a different account.

For several Discord accounts, add *Discord*, *Discord PTB* and *Discord Canary* —
separate installs, separate accounts, no patching.

## ⚠️ Known limitation

The isolated-profile fallback can't separate accounts for apps that resolve their
data folder through the Windows known-folder API instead of the environment (Discord
being the notable one — hence its separate builds). Catalog recipes name their
technique so you always know what you're getting.

**Changelog:** [CHANGELOG.md](../CHANGELOG.md) · **License:** MIT
