<div align="center">

# Windows iOS Universal Clipboard

### Copy on iPhone. Paste on Windows.

A tiny bridge that makes copying text from iPhone to Windows feel almost like Universal Clipboard.

[English](README.md) · [Türkçe](README.tr.md) · [Español](README.es.md) · [简体中文](README.zh-CN.md) · [हिन्दी](README.hi.md) · [العربية](README.ar.md) · [Português](README.pt-BR.md) · [Français](README.fr.md) · [Русский](README.ru.md) · [日本語](README.ja.md)

</div>

---

## 🎬 Setup video

[Click the image to watch the full setup video.](docs/setup.mp4)

[![Setup video](docs/setup-cover.jpg)](docs/setup.mp4)

---

## 1. Install

Open **PowerShell** on Windows and run:

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/install.ps1 | iex
```

## 2. Set up your iPhone

**[Add the iPhone Shortcut](https://www.icloud.com/shortcuts/e870980e381e4675a27af38c91db1265)**

1. Open the Shortcut link below on your iPhone.
2. Tap **Get Shortcut**.
3. Tap **Add Shortcut**.
4. Open the new Shortcut.
5. Tap the **▶ play** button in the bottom-right corner once.
6. When iPhone asks for permission, tap **Always Allow**.

> Keep the PowerShell window open while doing this. It will detect and pair your iPhone automatically.

## 3. Turn on Double Tap

1. Open **Settings** on iPhone.
2. Go to **Accessibility → Touch → Back Tap**.
3. Open **Double Tap**.
4. Choose **Windows iOS Universal Clipboard**.

## ✅ How to use

> **Copy text on iPhone → double-tap the back of your iPhone → press Ctrl+V on Windows.**

It starts automatically with Windows and runs silently in the background.

---

## Stop / Start / Uninstall

### Stop temporarily

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/stop.ps1 | iex
```

### Start again

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/start.ps1 | iex
```

### Remove completely

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/uninstall.ps1 | iex
```

---

## Good to know

- Your iPhone and PC should be on the same local network.
- Clipboard text is sent directly over your local network; there is no cloud clipboard account.
- Use it on networks you trust.
- Uninstall removes the app, pairing data, startup entry and firewall rules. Apple Bonjour is left installed because other Apple software may use it.

## License

MIT.
