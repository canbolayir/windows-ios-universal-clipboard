# Windows iOS Universal Clipboard

Copy text on your iPhone. Paste it on Windows.

A small, local-network bridge that gives **iPhone + Windows** a Universal Clipboard-style workflow.

> Unofficial project. Not affiliated with Apple or Microsoft. “Universal Clipboard” is used descriptively.

## Install

Open **PowerShell** on Windows and run:

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/install.ps1 | iex
```

That's it.

The installer:

- downloads the latest self-contained build
- installs Apple Bonjour when needed
- creates a Private-network-only firewall rule
- starts automatically when you sign in
- publishes one universal local address:

```text
http://copybridge.local:8765/copy
```

No IP address. No PC hostname. No token to copy.

## iPhone Shortcut

The shared Shortcut always uses the same endpoint:

```text
http://copybridge.local:8765/copy
```

Actions:

1. **Get Clipboard**
2. **Get Contents of URL**
   - URL: `http://copybridge.local:8765/copy`
   - Method: `POST`
   - Request Body: `JSON`
   - `text` = Clipboard

On the first request from a new device, Windows shows an **Allow / Deny** pairing prompt. Approve your iPhone once.

After that:

**Copy on iPhone → Back Tap → Ctrl+V on Windows**

The public iCloud Shortcut link will be added here after publishing.

## Back Tap

On iPhone:

**Settings → Accessibility → Touch → Back Tap → Double Tap → Windows Clipboard**

## Security model

CopyBridge is designed for a **trusted Private local network**.

- The Windows Firewall rule only applies to the **Private** profile.
- A new LAN device must be approved once on Windows.
- Approved device IPs are remembered locally.
- Clipboard content is sent directly over your LAN.
- There is no hosted clipboard service or account.

This is intentionally optimized for simple home/private Wi‑Fi use. Device approval is LAN-level protection, not cryptographic device authentication.

Do **not** expose port `8765` to the public internet.

## Multiple PCs

`copybridge.local` is intentionally optimized for a single CopyBridge PC on a LAN. Running multiple CopyBridge PCs on the same network can cause a hostname conflict.

## Uninstall

Run `uninstall.ps1` from this repository.

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
