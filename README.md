# Windows iOS Universal Clipboard

Copy text on your iPhone. Paste it on Windows.

A small, local-network bridge that brings a Universal Clipboard-style workflow to **iPhone + Windows**.

> Unofficial project. Not affiliated with Apple or Microsoft. “Universal Clipboard” is used descriptively.

## How it works

1. Copy text on your iPhone.
2. Trigger the iPhone Shortcut (Back Tap works well).
3. Press **Ctrl+V** on Windows.

No account. No cloud clipboard. Clipboard text is sent directly from the iPhone to your PC over your local network.

## Install on Windows

Open **PowerShell** and run:

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/install.ps1 | iex
```

Accept the Windows UAC prompt once.

The installer will:

- download the latest self-contained Windows build
- install it under your user profile
- generate a random local access token
- add a **Private network only** firewall rule
- start the bridge automatically when you sign in
- install Apple Bonjour through `winget` when available, for a stable `PC-NAME.local` address
- open a local setup page containing your Shortcut endpoint and token

The app does **not** need the .NET runtime installed separately.

## Add the iPhone Shortcut

The public iCloud Shortcut link will be added here after the shared Shortcut is published.

For now, see [docs/SHORTCUT.md](docs/SHORTCUT.md).

The shared Shortcut is designed to use two Apple Shortcuts **Import Questions**:

1. **Endpoint** — shown by the Windows setup page
2. **Token** — shown by the Windows setup page

That means every user can install the same iCloud Shortcut and enter their own PC details during setup.

## Back Tap

On iPhone:

**Settings → Accessibility → Touch → Back Tap → Double Tap → Windows Clipboard**

Then:

**Copy on iPhone → Double Tap the back of the iPhone → Ctrl+V on Windows**

## Reopen the setup page

Open:

```text
http://127.0.0.1:8765/setup
```

on the Windows PC.

## Privacy and security

- Text is transferred directly over the local network.
- There is no hosted clipboard service and no account.
- Incoming clipboard writes require a randomly generated token.
- The Windows firewall rule is limited to the **Private** network profile.
- The setup page and token are only exposed to localhost.
- The token is stored in `%LOCALAPPDATA%\WindowsIOSUniversalClipboard\config.json`.

Do not expose port `8765` to the public internet.

## Uninstall

Download `uninstall.ps1` from this repository and run it in PowerShell.

The uninstaller deliberately leaves Apple Bonjour installed because other applications may depend on it.

## Build from source

Requirements: .NET 10 SDK.

```powershell
dotnet publish .\src\WindowsIOSUniversalClipboard\WindowsIOSUniversalClipboard.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true
```

## License

MIT
