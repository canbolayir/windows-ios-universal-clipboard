<div align="center">

# Windows iOS Universal Clipboard

### iPhone’da kopyala. Windows’ta yapıştır.

iPhone’dan Windows’a metin kopyalamayı Universal Clipboard gibi hissettiren küçük ve basit bir köprü.

[English](README.md) · [Türkçe](README.tr.md) · [Español](README.es.md) · [简体中文](README.zh-CN.md) · [हिन्दी](README.hi.md) · [العربية](README.ar.md) · [Português](README.pt-BR.md) · [Français](README.fr.md) · [Русский](README.ru.md) · [日本語](README.ja.md)

</div>

---

## 🎬 Kurulum videosu

[Tam kurulumu izlemek için görsele tıkla.](docs/setup.mp4)

[![Setup video](docs/setup-cover.jpg)](docs/setup.mp4)

---

## 1. Kurulum

Windows’ta **PowerShell** aç ve şunu çalıştır:

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/install.ps1 | iex
```

## 2. iPhone’u hazırla

**[iPhone Kestirmesini ekle](https://www.icloud.com/shortcuts/e870980e381e4675a27af38c91db1265)**

1. Aşağıdaki Kestirme bağlantısını iPhone’da aç.
2. **Get Shortcut** seçeneğine dokun.
3. **Add Shortcut** seçeneğine dokun.
4. Eklenen yeni Kestirme’yi aç.
5. Sağ alttaki **▶ oynat** düğmesine bir kez dokun.
6. iPhone izin istediğinde **Always Allow / Her Zaman İzin Ver** seçeneğini seç.

> Bunları yaparken PowerShell penceresini açık bırak. iPhone otomatik olarak algılanıp eşleştirilecek.

## 3. İki Kez Dokunmayı ayarla

1. iPhone’da **Ayarlar**’ı aç.
2. **Erişilebilirlik → Dokunma → Arkaya Dokun** bölümüne git.
3. **İki Kez Dokun** seçeneğini aç.
4. **Windows iOS Universal Clipboard** Kestirmesini seç.

## ✅ Nasıl kullanılır?

> **iPhone’da metni kopyala → telefonun arkasına iki kez dokun → Windows’ta Ctrl+V yap.**

Windows açıldığında otomatik başlar ve arka planda sessizce çalışır.

---

## Durdur / Başlat / Tamamen Sil

### Geçici olarak durdur

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/stop.ps1 | iex
```

### Tekrar başlat

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/start.ps1 | iex
```

### Tamamen kaldır

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/uninstall.ps1 | iex
```

---

## Bilmen gerekenler

- iPhone ve PC aynı yerel ağda olmalı.
- Pano metni doğrudan yerel ağ üzerinden gider; bulut hesabı kullanılmaz.
- Güvendiğin ağlarda kullan.
- Tam kaldırma uygulamayı, eşleşme verilerini, başlangıç kaydını ve firewall kurallarını siler. Diğer Apple uygulamaları kullanabileceği için Bonjour sistemde bırakılır.

## Lisans

MIT.
