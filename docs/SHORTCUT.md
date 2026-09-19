# iPhone Shortcut

The public Shortcut requires **zero configuration**.

Actions, top to bottom:

1. **Get Clipboard**
2. **Get Contents of URL**
   - URL: `http://copybridge.local:8765/copy`
   - Method: `POST`
   - Request Body: `JSON`
   - `text` = Clipboard

Do not add an authentication header.

On first use, Windows displays an approval dialog for the iPhone's current local IP. After approval, future clipboard sends from that IP are accepted automatically.

Recommended trigger:

**Settings → Accessibility → Touch → Back Tap → Double Tap → the Shortcut**
