<div align="center">

# Windows iOS Universal Clipboard

### iPhone पर कॉपी करें। Windows पर पेस्ट करें।

एक छोटा और आसान ब्रिज जो iPhone से Windows पर टेक्स्ट कॉपी करना Universal Clipboard जैसा बनाता है।

[English](README.md) · [Türkçe](README.tr.md) · [Español](README.es.md) · [简体中文](README.zh-CN.md) · [हिन्दी](README.hi.md) · [العربية](README.ar.md) · [Português](README.pt-BR.md) · [Français](README.fr.md) · [Русский](README.ru.md) · [日本語](README.ja.md)

</div>

---

## 🎬 सेटअप वीडियो

[पूरा सेटअप देखने के लिए इमेज पर क्लिक करें।](docs/setup.mp4)

[![Setup video](docs/setup-cover.jpg)](docs/setup.mp4)

---

## 1. इंस्टॉल

Windows पर **PowerShell** खोलें और चलाएँ:

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/install.ps1 | iex
```

## 2. iPhone सेट करें

**[iPhone Shortcut जोड़ें](https://www.icloud.com/shortcuts/e870980e381e4675a27af38c91db1265)**

1. नीचे दिया गया Shortcut लिंक iPhone पर खोलें।
2. **Get Shortcut** पर टैप करें।
3. **Add Shortcut** पर टैप करें।
4. नया Shortcut खोलें।
5. नीचे दाईं ओर **▶** बटन को एक बार टैप करें।
6. iPhone अनुमति मांगे तो **Always Allow** चुनें।

> PowerShell विंडो खुली रखें। iPhone अपने-आप डिटेक्ट और पेयर हो जाएगा।

## 3. Double Tap सेट करें

1. iPhone पर **Settings** खोलें।
2. **Accessibility → Touch → Back Tap** पर जाएँ।
3. **Double Tap** खोलें।
4. **Windows iOS Universal Clipboard** चुनें।

## ✅ कैसे इस्तेमाल करें

> **iPhone पर टेक्स्ट कॉपी करें → iPhone के पीछे दो बार टैप करें → Windows पर Ctrl+V दबाएँ।**

यह Windows के साथ अपने-आप शुरू होता है और बैकग्राउंड में चलता है।

---

## Stop / Start / Uninstall

### अस्थायी रूप से रोकें

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/stop.ps1 | iex
```

### फिर से शुरू करें

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/start.ps1 | iex
```

### पूरी तरह हटाएँ

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/uninstall.ps1 | iex
```

---

## ध्यान दें

- iPhone और PC एक ही लोकल नेटवर्क पर होने चाहिए।
- Clipboard टेक्स्ट सीधे लोकल नेटवर्क पर भेजा जाता है; कोई cloud clipboard account नहीं है।
- इसे भरोसेमंद नेटवर्क पर इस्तेमाल करें।
- Uninstall ऐप, pairing data, startup entry और firewall rules हटाता है। Bonjour को रहने दिया जाता है क्योंकि Apple के दूसरे ऐप इसका उपयोग कर सकते हैं।

## लाइसेंस

MIT.
