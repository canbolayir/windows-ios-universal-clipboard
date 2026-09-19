# Windows iOS Universal Clipboard

[English](README.md) · [Türkçe](README.tr.md) · [Español](README.es.md) · [简体中文](README.zh-CN.md) · [हिन्दी](README.hi.md) · [العربية](README.ar.md) · [Português](README.pt-BR.md) · [Français](README.fr.md) · [Русский](README.ru.md) · [日本語](README.ja.md)

**iPhone पर कॉपी करें। Windows पर पेस्ट करें।**

एक छोटा लोकल-नेटवर्क क्लिपबोर्ड ब्रिज जो iPhone + Windows पर Universal Clipboard जैसा अनुभव देता है।

> यह अनौपचारिक प्रोजेक्ट है। Apple या Microsoft से संबद्ध नहीं है। "Universal Clipboard" शब्द केवल वर्णन के लिए इस्तेमाल किया गया है।

## इंस्टॉल

PowerShell खोलें और चलाएँ:

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/install.ps1 | iex
```

इंस्टॉलर अपने-आप:

1. Windows ब्रिज इंस्टॉल और कॉन्फ़िगर करता है;
2. ज़रूरत होने पर Apple Bonjour इंस्टॉल करता है;
3. फ़ायरवॉल को केवल लोकल सबनेट तक सीमित करता है;
4. Windows लॉगिन पर बैकग्राउंड ऑटो-स्टार्ट चालू करता है;
5. iPhone Shortcut लिंक दिखाता है;
6. पहली Shortcut रिक्वेस्ट का इंतज़ार कर iPhone को अपने-आप पेयर करता है।

IP, PC hostname या token की ज़रूरत नहीं है।

Shared Shortcut:

**[Windows iOS Universal Clipboard जोड़ें](https://www.icloud.com/shortcuts/e870980e381e4675a27af38c91db1265)**

इंस्टॉल के दौरान iPhone पर:

1. Shortcut लिंक खोलें;
2. **Get Shortcut** टैप करें;
3. **Add Shortcut** टैप करें;
4. नया जोड़ा गया Shortcut खोलें;
5. नीचे-दाएँ **> play** बटन से एक बार चलाएँ;
6. अनुमति माँगे तो **Always Allow** चुनें।

इंस्टॉलर टर्मिनल खुला रखें। पहली वैध रिक्वेस्ट अपने-आप पेयर हो जाएगी:

```text
OK - iPhone detected
OK - device paired
```

फिर Back Tap सेट करें:

**Settings -> Accessibility -> Touch -> Back Tap -> Double Tap -> Windows iOS Universal Clipboard**

सामान्य उपयोग:

```text
iPhone पर कॉपी -> पीछे Double Tap -> Windows पर Ctrl+V
```

## क्या यह अपने-आप शुरू होता है?

हाँ। Windows साइन-इन के बाद `WindowsIOSUniversalClipboard.exe` चुपचाप बैकग्राउंड में शुरू हो जाता है।

## अस्थायी रूप से रोकें

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/stop.ps1 | iex
```

ऑटो-स्टार्ट चालू रहेगा।

## फिर से शुरू करें

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/start.ps1 | iex
```

## पूरी तरह अनइंस्टॉल करें

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/uninstall.ps1 | iex
```

अनइंस्टॉलर ऐप, paired-device डेटा, config/logs, startup entry, firewall rules और पुराने bridge files/tasks हटाता है।

Apple Bonjour को जानबूझकर छोड़ा जाता है क्योंकि iTunes/iCloud जैसे ऐप इसका उपयोग कर सकते हैं। इसे भी हटाने के लिए:

```powershell
winget uninstall --id Apple.Bonjour
```

## Pairing कैसे काम करता है?

इंस्टॉलर थोड़े समय के लिए लोकल pairing window खोलता है। केवल उसी दौरान लोकल सबनेट से आने वाली पहली वैध unknown-device रिक्वेस्ट approved list में जुड़ती है। पहली pairing के बाद window बंद हो जाती है।

इंस्टॉलेशन के बाहर unknown devices **अपने-आप approve नहीं होते**।

```text
%LOCALAPPDATA%\WindowsIOSUniversalClipboard\config.json
```

## Discovery कैसे काम करता है?

ऐप Bonjour/mDNS से `copybridge.local` प्रकाशित करता है और Shortcut टेक्स्ट भेजता है:

```text
http://copybridge.local:8765/copy
```

## Privacy और security

Clipboard text सीधे local network पर जाता है। कोई account system या cloud relay नहीं है।

Transport HTTP है, इसलिए LAN पर traffic **encrypted नहीं है**। केवल भरोसेमंद नेटवर्क पर उपयोग करें। Firewall और ऐप access को directly connected local subnets तक सीमित करते हैं।

Pairing local-network source address पर आधारित है, cryptographic device identity पर नहीं। DHCP से iPhone का address बदलने पर दोबारा pairing करनी पड़ सकती है।

TCP port `8765` को public internet पर expose न करें।

## Build

Windows पर .NET 10 SDK चाहिए:

```powershell
dotnet build .\src\WindowsIOSUniversalClipboard\WindowsIOSUniversalClipboard.csproj
```

## License

MIT.
