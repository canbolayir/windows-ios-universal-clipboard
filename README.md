# Windows iOS Universal Clipboard

**Copy on iPhone. Paste on Windows.**

A local-network clipboard bridge for iPhone and Windows.

- No account
- No cloud relay
- No IP address setup
- No hostname setup
- No token to copy
- One-time Windows approval
- Works with iPhone Back Tap
- Starts automatically with Windows

> Not affiliated with Apple. "Universal Clipboard" is used descriptively.

## Install on Windows

Open PowerShell and run:

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/install.ps1 | iex
```

Accept the Windows UAC prompt.

The installer:

- installs Apple Bonjour automatically if local discovery is missing;
- downloads the latest self-contained Windows release;
- adds inbound firewall rules scoped to your **local subnet only** (works whether Windows labels the network Public or Private);
- starts the clipboard bridge;
- configures automatic startup.

No .NET installation is required.

Current release: **Windows x64**.

## Install on iPhone

Add the shared Shortcut:

**Shortcut:** [Add Windows iOS Universal Clipboard](https://www.icloud.com/shortcuts/e870980e381e4675a27af38c91db1265)

The Shortcut is identical for every user. It always sends copied text to:

```text
http://copybridge.local:8765/copy
```

There is no per-user hostname, IP, or token.

### First use

1. Put the iPhone and Windows PC on the same trusted local network.
2. Copy text on iPhone.
3. Run the Shortcut.
4. Windows asks whether to allow that local device.
5. Click **Yes** once.
6. Paste anywhere on Windows with `Ctrl+V`.

For a macOS-like gesture, assign the Shortcut to:

**Settings â†’ Accessibility â†’ Touch â†’ Back Tap**

Then the normal flow is:

**Copy on iPhone â†’ Back Tap â†’ Ctrl+V on Windows**

## How local discovery works

The Windows app publishes this local mDNS record through Apple's Bonjour DNS-SD client:

```text
copybridge.local â†’ your active Windows LAN address
```

The alias is refreshed when the PC's active network interfaces or IP addresses change.

The iPhone Shortcut therefore never needs to know the Windows computer name or DHCP address.

## Privacy and security

Clipboard text is transferred directly over the local network using HTTP. It is **not encrypted** and is not sent through this project or a cloud relay.

Requests from outside the PC's directly connected local subnets are rejected. An unknown local source IP cannot write to the Windows clipboard until the user explicitly approves it on the Windows PC.

Approved source IPs are stored locally in:

```text
%LOCALAPPDATA%\WindowsIOSUniversalClipboard\config.json
```

This is intentionally a trusted-LAN convenience model, not cryptographic device identity. DHCP changes may cause Windows to ask for approval again. Do not use it on hostile or untrusted networks if your clipboard may contain sensitive information.

## Uninstall

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/uninstall.ps1 | iex
```

Bonjour is intentionally left installed because iTunes or other Apple software may also use it.

## Build

Requires the .NET 10 SDK on Windows:

```powershell
dotnet build .\src\WindowsIOSUniversalClipboard\WindowsIOSUniversalClipboard.csproj
```

## License

MIT.
