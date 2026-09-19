# Windows iOS Universal Clipboard

**Copy on iPhone. Paste on Windows.**

A small local-network clipboard bridge that gives iPhone + Windows a Universal Clipboard-style workflow.

> Unofficial project. Not affiliated with Apple or Microsoft. "Universal Clipboard" is used descriptively.

## Install

Open PowerShell and run:

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/install.ps1 | iex
```

The installer does the rest:

1. installs/configures the Windows bridge;
2. installs Apple Bonjour when needed;
3. configures local-subnet-only firewall access;
4. enables automatic background startup with Windows;
5. shows the shared iPhone Shortcut link;
6. waits for the first Shortcut request and pairs that iPhone automatically.

No IP address, PC hostname, or token is required.

Shared Shortcut:

**[Add Windows iOS Universal Clipboard](https://www.icloud.com/shortcuts/e870980e381e4675a27af38c91db1265)**

During installation, copy any text on the iPhone and run the Shortcut once. The terminal should finish with:

```text
OK - iPhone detected
OK - device paired
Setup complete.
```

After that the normal workflow is:

```text
Copy on iPhone -> run Shortcut -> Ctrl+V on Windows
```

For a more Mac-like gesture, assign the Shortcut to iPhone Back Tap:

**Settings -> Accessibility -> Touch -> Back Tap**

## Does it start automatically?

Yes. The installer adds the bridge to the current Windows user's startup configuration.

After Windows sign-in, `WindowsIOSUniversalClipboard.exe` starts silently in the background. No terminal window needs to remain open.

## Stop it temporarily

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/stop.ps1 | iex
```

This stops the current background process. Automatic startup remains enabled, so it will run again after the next Windows sign-in.

## Start it again

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/start.ps1 | iex
```

## Completely uninstall it

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/uninstall.ps1 | iex
```

The uninstaller removes:

- the app;
- paired-device data;
- config and logs;
- automatic startup;
- firewall rules;
- old/legacy bridge tasks and files.

Apple Bonjour is deliberately left installed because iTunes, iCloud, or other Apple software may use it.

To remove Bonjour too:

```powershell
winget uninstall --id Apple.Bonjour
```

## How pairing works

The installer creates a short-lived local pairing window while it waits in the terminal.

Only while that setup window is active, the first unknown device on the PC's directly connected local subnet that sends a valid clipboard request is automatically added to the local approved-device list. The pairing window closes immediately after that first pairing.

Outside installation, unknown devices are **not** silently auto-approved.

Approved devices are stored locally in:

```text
%LOCALAPPDATA%\WindowsIOSUniversalClipboard\config.json
```

The installer/uninstaller removes this state when appropriate.

## How discovery works

The Windows app publishes:

```text
copybridge.local
```

on the local network using Bonjour/mDNS. The Shortcut always sends copied text to:

```text
http://copybridge.local:8765/copy
```

So the Shortcut does not need to know the PC's changing DHCP address or Windows computer name.

## Privacy and security

Clipboard text travels directly across the local network. This project has no account system and no cloud clipboard relay.

The current transport is HTTP, so clipboard traffic is **not encrypted** on the LAN. The Windows firewall and the app both restrict access to directly connected local subnets. Use it on networks you trust.

The pairing model is intentionally a convenience model based on local-network source addresses, not cryptographic device identity. If DHCP changes the iPhone's address, pairing may need to be repeated.

Do not expose TCP port `8765` to the public internet.

## Build

Requires the .NET 10 SDK on Windows:

```powershell
dotnet build .\src\WindowsIOSUniversalClipboard\WindowsIOSUniversalClipboard.csproj
```

## License

MIT.
