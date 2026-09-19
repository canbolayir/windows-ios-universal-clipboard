# Security

## Security model

CopyBridge is intended for trusted home/private LANs.

The bridge listens on TCP port `8765`, while the installer creates a Windows Firewall rule limited to the **Private** network profile.

The first request from a previously unseen LAN IP triggers an interactive Windows **Allow / Deny** prompt. Approved device IPs are stored locally in:

```text
%LOCALAPPDATA%\WindowsIOSUniversalClipboard\config.json
```

This provides simple local-network pairing without requiring users to copy tokens into the iPhone Shortcut.

### Important limitations

- Device identity is based on LAN IP, not a cryptographic device key.
- A sufficiently capable attacker already present on the same trusted LAN may be able to spoof network identity.
- `copybridge.local` assumes one CopyBridge PC per LAN.
- Do not expose TCP port `8765` to the public internet.

## Reporting a vulnerability

Please use GitHub Security Advisories for this repository rather than publishing sensitive details in a public issue.
