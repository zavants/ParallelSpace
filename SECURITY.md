# Security

## What this app does on your machine

Parallel Space is a launcher. It is worth being clear about its reach, because it
starts other programs on your behalf:

- **It runs as you.** No elevation, no service, no driver. The manifest requests
  `asInvoker`.
- **It starts programs you configure**, with arguments you can see and edit, and puts
  each launch in a Windows job object so it can report status and close that copy's
  process tree — and nothing outside it. The one exception is a Discord channel
  space, which is closed by process image name and therefore ends every running
  copy of that channel, including one started outside Parallel Space.
- **It redirects environment variables** (`USERPROFILE`, `APPDATA`, `LOCALAPPDATA`,
  `TEMP`) for spaces set to "isolated profile", so the copy reads and writes inside
  `%APPDATA%\ParallelSpace\Spaces\<id>`.
- **It reads Start Menu shortcuts** to list detected apps. This is read-only: it
  resolves `.lnk` targets and never launches anything while scanning.
- **It does not patch, inject into, or modify any other application**, and it makes no
  network requests. The only outbound action is opening a vendor's official download
  page in your browser when you click "Get it".

Your data stays in `%APPDATA%\ParallelSpace`. Nothing is sent anywhere.

## Reporting a vulnerability

Please open a [security advisory](https://github.com/dboudreau00/ParallelSpace/security/advisories/new)
rather than a public issue, and include the version and steps to reproduce.

## Note on multi-accounting

Running several copies of an app is a normal thing to do with your own accounts.
Some services restrict multiple accounts in their terms — that is between you and
them; this tool takes no position and does nothing to hide or falsify anything.
