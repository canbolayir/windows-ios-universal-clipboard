# Windows iOS Universal Clipboard

**Copy on iPhone. Paste on Windows.**

A tiny local-network clipboard bridge for iPhone and Windows.

- No account
- No cloud relay
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

Accept the Windows UAC prompt. The installer downloads the latest self-contained release, adds Private-network firewall rules, starts the app, and configures automatic startup.

No .NET installation is required.

## Install on iPhone

Add the shared Shortcut from the link below:

**Shortcut link: coming next**

The Shortcut is the same for every user. It sends copied text to:

```text
http://copybridge.local:8765/copy
```

There is no per-user hostname or token.

### First use

1. Make sure the iPhone and Windows PC are on the same local network.
2. Copy text on the iPhone.
3. Run the Shortcut.
4. Windows asks whether to allow that device.
5. Click **Yes** once.
6. Paste anywhere on Windows with `Ctrl+V`.

For a macOS-like gesture, assign the Shortcut to:

**Settings → Accessibility → Touch → Back Tap**

Then the normal flow is:

**Copy on iPhone → Back Tap → Ctrl+V on Windows**

## How it works

The Windows app:

- listens only for clipboard submissions on TCP `8765`;
- announces `copybridge.local` over local mDNS;
- prompts on Windows before trusting an unknown local IP;
- remembers approved local IPs;
- writes accepted text into the Windows clipboard.

The iPhone Shortcut sends a small JSON request:

```json
{
  "text": "the current iPhone clipboard text"
}
```

## Privacy and security

Clipboard text is transferred directly over the local network using HTTP. It is **not encrypted** and is not sent through this project or a cloud service.

Use it on networks you trust. Unknown local IPs require an explicit Windows approval before they can write to the clipboard.

Approved devices are stored locally in:

```text
%LOCALAPPDATA%\WindowsIOSUniversalClipboard\config.json
```

## Uninstall

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/uninstall.ps1 | iex
```

## Build

Requires the .NET 10 SDK on Windows:

```powershell
dotnet build .\src\WindowsIOSUniversalClipboard\WindowsIOSUniversalClipboard.csproj
```

## License

MIT.
