# Windows iOS Universal Clipboard

[English](README.md) · [Türkçe](README.tr.md) · [Español](README.es.md) · [简体中文](README.zh-CN.md) · [हिन्दी](README.hi.md) · [العربية](README.ar.md) · [Português](README.pt-BR.md) · [Français](README.fr.md) · [Русский](README.ru.md) · [日本語](README.ja.md)

**iPhone'da kopyala. Windows'ta yapıştır.**

iPhone + Windows arasında Universal Clipboard benzeri bir deneyim sağlayan küçük bir yerel ağ pano köprüsü.

> Resmî olmayan bir projedir. Apple veya Microsoft ile bağlantılı değildir. "Universal Clipboard" ifadesi açıklayıcı amaçla kullanılır.

## Kurulum

PowerShell'i aç ve çalıştır:

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/install.ps1 | iex
```

Kurucu geri kalan her şeyi otomatik yapar:

1. Windows köprüsünü kurar ve yapılandırır;
2. gerekiyorsa Apple Bonjour'u kurar;
3. güvenlik duvarını yalnızca yerel alt ağ erişimine açar;
4. Windows açılışında arka planda otomatik başlamayı etkinleştirir;
5. ortak iPhone Kestirmesi bağlantısını gösterir;
6. ilk Kestirme isteğini bekler ve iPhone'u otomatik eşleştirir.

IP adresi, bilgisayar adı veya token gerekmez.

Ortak Kestirme:

**[Windows iOS Universal Clipboard Kestirmesini Ekle](https://www.icloud.com/shortcuts/e870980e381e4675a27af38c91db1265)**

Kurulum sırasında terminal iPhone tarafını adım adım anlatır:

1. iPhone'da Kestirme bağlantısını aç;
2. **Kestirmeyi Al / Get Shortcut** seçeneğine dokun;
3. **Kestirme Ekle / Add Shortcut** seçeneğine dokun;
4. yeni eklenen Kestirmeyi aç;
5. sağ alttaki **> oynat** düğmesine dokunarak bir kez çalıştır;
6. iPhone izin istediğinde **Her Zaman İzin Ver / Always Allow** seçeneğini seç.

Bunları yaparken kurulum terminalini açık bırak. İlk geçerli Kestirme isteği otomatik eşleştirilir ve terminalde şunları görürsün:

```text
OK - iPhone detected
OK - device paired
```

Ardından terminal Arkaya Dokun ayarını da gösterir:

**Ayarlar -> Erişilebilirlik -> Dokunma -> Arkaya Dokun -> Çift Dokunma -> Windows iOS Universal Clipboard**

Sonrasında normal kullanım:

```text
iPhone'da kopyala -> iPhone'un arkasına iki kez dokun -> Windows'ta Ctrl+V
```

## Windows açılınca otomatik başlıyor mu?

Evet. Kurucu köprüyü mevcut Windows kullanıcısının başlangıç ayarına ekler.

Windows oturumu açıldıktan sonra `WindowsIOSUniversalClipboard.exe` sessizce arka planda çalışır. Terminal penceresinin açık kalması gerekmez.

## Geçici olarak durdurma

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/stop.ps1 | iex
```

Bu komut mevcut arka plan işlemini durdurur. Otomatik başlangıç açık kalır; bir sonraki Windows oturumunda yeniden çalışır.

## Tekrar başlatma

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/start.ps1 | iex
```

## Tamamen kaldırma

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/uninstall.ps1 | iex
```

Kaldırıcı şunları temizler:

- uygulama;
- eşleştirilmiş cihaz verileri;
- yapılandırma ve loglar;
- otomatik başlangıç;
- güvenlik duvarı kuralları;
- eski/legacy bridge görevleri ve dosyaları.

Apple Bonjour bilerek kaldırılmaz; iTunes, iCloud veya başka Apple yazılımları bunu kullanıyor olabilir.

Bonjour'u da kaldırmak için:

```powershell
winget uninstall --id Apple.Bonjour
```

## Eşleştirme nasıl çalışır?

Kurucu terminalde beklerken kısa süreli bir yerel eşleştirme penceresi açar.

Yalnızca bu kurulum penceresi açıkken, bilgisayarın doğrudan bağlı olduğu yerel alt ağdan geçerli bir pano isteği gönderen ilk bilinmeyen cihaz yerel onaylı cihaz listesine otomatik eklenir. İlk eşleştirmeden hemen sonra eşleştirme penceresi kapanır.

Kurulum dışında bilinmeyen cihazlar **sessizce otomatik onaylanmaz**.

Onaylı cihazlar yerel olarak burada saklanır:

```text
%LOCALAPPDATA%\WindowsIOSUniversalClipboard\config.json
```

Kurucu/kaldırıcı gerektiğinde bu durumu temizler.

## Keşif nasıl çalışır?

Windows uygulaması yerel ağda Bonjour/mDNS üzerinden şunu yayınlar:

```text
copybridge.local
```

Kestirme kopyalanan metni her zaman şu adrese gönderir:

```text
http://copybridge.local:8765/copy
```

Bu nedenle Kestirmenin bilgisayarın değişebilen DHCP adresini veya Windows bilgisayar adını bilmesine gerek yoktur.

## Gizlilik ve güvenlik

Pano metni doğrudan yerel ağ üzerinden iletilir. Projede hesap sistemi veya bulut pano aktarımı yoktur.

Mevcut aktarım HTTP'dir; bu nedenle pano trafiği LAN üzerinde **şifreli değildir**. Windows güvenlik duvarı ve uygulama erişimi doğrudan bağlı yerel alt ağlarla sınırlar. Yalnızca güvendiğin ağlarda kullan.

Eşleştirme modeli kriptografik cihaz kimliği yerine yerel ağ kaynak adreslerine dayanan, kullanım kolaylığı odaklı bir modeldir. DHCP iPhone'un adresini değiştirirse yeniden eşleştirme gerekebilir.

TCP `8765` portunu internete açma.

## Derleme

Windows'ta .NET 10 SDK gerekir:

```powershell
dotnet build .\src\WindowsIOSUniversalClipboard\WindowsIOSUniversalClipboard.csproj
```

## Lisans

MIT.
