# Parallel Space v1.0.0

**Run two or three signed-in copies of any app, side by side — on Windows.**
Each launch gets its own isolated profile, so you can be signed into multiple
accounts at once. Multiple Discord accounts are a first-class case.

## ✨ Highlights

- **Spaces** — add any program; each runs as an isolated copy with its own data.
- **Discord multi-account** via the official Stable / PTB / Canary channels — no
  injection, no patching, nothing an anti-cheat or EDR would flag.
- **Detected-apps** quick-add strip, built from your Start Menu shortcuts.
- **Add/edit dialog** — name, executable, arguments, working folder, isolation
  mode, plus a custom icon and colour per space.
- **Live status** with Launch / Close / open-data-folder on every card.
- **Self-contained** single `.exe` — no .NET runtime, no admin rights.

## 📦 Install

1. Download **`ParallelSpace-v1.0.0-win-x64.zip`** below.
2. Unzip and run **`ParallelSpace.exe`**.

Windows 10/11 (x64). The build isn't code-signed, so on first run choose
**More info → Run anyway** if SmartScreen prompts.

## 🚀 Two Discord accounts in 30 seconds

1. Open the **Discord** tab → **Add space** on *Discord*.
2. **Download ↗** *Discord PTB*, install it, then **Add space**.
3. Back on **Spaces**, **Launch** each and sign into a different account.

## ⚙️ How isolation works

- **Discord** uses its three official release channels — each is a separate
  install with its own data folder, so each holds a different account.
- **Everything else** launches with `USERPROFILE` / `APPDATA` / `LOCALAPPDATA` /
  `TEMP` redirected into a private per-space folder (plus an optional
  `--user-data-dir`), giving a fresh profile for portable and Chromium/Electron
  apps.

> Apps that read their data folder from the Windows known-folder API (Discord,
> some Squirrel/Electron wallets) can't be split within one Windows user by the
> generic engine — which is why Discord uses channels.

## Notes

- Windows only (x64), current-user (no admin).
- Spaces are saved in `%APPDATA%\ParallelSpace`.

**Full details:** see the [README](../README.md) · **License:** MIT
