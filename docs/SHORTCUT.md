# iPhone Shortcut

The Windows bridge accepts text using a very small HTTP request.

Create a Shortcut named **Windows Clipboard** with these actions:

1. **Get Clipboard**
2. **Get Contents of URL**

Configure **Get Contents of URL**:

- URL: your Windows setup page's **Endpoint**
- Method: `POST`
- Header:
  - `X-Clipboard-Token` = your Windows setup page's **Token**
- Request Body: `JSON`
  - key: `text`
  - value: **Clipboard**

## Before publishing the shared Shortcut

The project owner should add two **Import Questions** so the same iCloud Shortcut works for everyone:

### Import Question 1

Attach it to the URL field in **Get Contents of URL**.

Question:

```text
Paste the Endpoint shown by the Windows setup page
```

### Import Question 2

Attach it to the value of the `X-Clipboard-Token` header.

Question:

```text
Paste the Token shown by the Windows setup page
```

Then share the Shortcut using **Copy iCloud Link** and place the resulting link in the repository README.

Apple's Import Questions replace those fields with each user's own values when they add the shared Shortcut, so the repository never needs to contain a user's hostname or token.

## Recommended trigger

**Settings → Accessibility → Touch → Back Tap → Double Tap → Windows Clipboard**
