# Security

Windows iOS Universal Clipboard is designed for **trusted local networks**.

## Trust model

- Clipboard traffic stays on the local network.
- There is no cloud relay operated by this project.
- Unknown source IPs require an explicit Windows **Yes / No** approval.
- Approved source IPs are stored locally.
- Rejected sources are temporarily rate-limited to avoid repeated approval prompts.

## Important limitations

Clipboard text is sent over plain HTTP on the LAN. It is not end-to-end encrypted.

Approval is IP-based convenience pairing, not cryptographic device identity. On a hostile LAN, IP spoofing or local traffic interception may be possible.

Do not use this project on untrusted networks if the clipboard may contain passwords, tokens, private keys, or other sensitive data.

## Reporting a security issue

Please use GitHub's private vulnerability reporting feature rather than opening a public issue.
