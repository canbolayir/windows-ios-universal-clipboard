# Security

Windows iOS Universal Clipboard is designed for **trusted local networks**.

## Trust model

- Clipboard submissions are accepted only from the local network path that can reach the Windows listener.
- An unknown source IP is not trusted automatically.
- Windows displays an explicit **Allow / Deny** dialog on first contact.
- Approved IP addresses are stored locally.
- There is no cloud relay and no project-operated server.

## Important limitation

Clipboard text is sent over plain HTTP on the LAN. It is not end-to-end encrypted.

Do not use this project on hostile or untrusted networks if the clipboard may contain sensitive information.

## Reporting a security issue

Please use GitHub's private vulnerability reporting feature for this repository rather than opening a public issue.
