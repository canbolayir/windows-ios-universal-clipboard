# iPhone Shortcut

Create a Shortcut named **Windows Clipboard**.

Use exactly these actions:

1. **Get Clipboard**
2. **Get Contents of URL**

Configure **Get Contents of URL**:

- URL: `http://copybridge.local:8765/copy`
- Method: `POST`
- Request Body: `JSON`
- key: `text`
- value: **Clipboard**

There are no setup questions, tokens, IP addresses, or PC-specific values.

On first use, approve the pairing prompt shown on Windows.

Recommended trigger:

**Settings → Accessibility → Touch → Back Tap → Double Tap → Windows Clipboard**
