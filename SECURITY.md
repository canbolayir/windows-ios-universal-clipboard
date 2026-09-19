# Security

## Supported version

Only the latest release is supported.

## Design

The bridge listens on TCP port `8765` and accepts clipboard writes only when the request includes the locally generated `X-Clipboard-Token`.

The installer creates an inbound Windows Firewall rule for the **Private** network profile only.

The setup page (`/setup`) and configuration endpoint (`/setup.json`) are restricted to localhost.

Do not forward port `8765` from a router and do not expose it to the public internet.

## Reporting a vulnerability

Please open a GitHub Security Advisory for this repository rather than publishing a sensitive issue.
