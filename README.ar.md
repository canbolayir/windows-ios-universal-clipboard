<div align="center">

# Windows iOS Universal Clipboard

### انسخ على iPhone. الصق على Windows.

أداة صغيرة وبسيطة تجعل نسخ النص من iPhone إلى Windows قريباً من تجربة Universal Clipboard.

[English](README.md) · [Türkçe](README.tr.md) · [Español](README.es.md) · [简体中文](README.zh-CN.md) · [हिन्दी](README.hi.md) · [العربية](README.ar.md) · [Português](README.pt-BR.md) · [Français](README.fr.md) · [Русский](README.ru.md) · [日本語](README.ja.md)

</div>

---

## 🎬 فيديو الإعداد

[اضغط على الصورة لمشاهدة الإعداد الكامل.](docs/setup.mp4)

[![Setup video](docs/setup-cover.jpg)](docs/setup.mp4)

---

## 1. التثبيت

افتح **PowerShell** على Windows وشغّل:

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/install.ps1 | iex
```

## 2. إعداد iPhone

**[إضافة اختصار iPhone](https://www.icloud.com/shortcuts/e870980e381e4675a27af38c91db1265)**

1. افتح رابط الاختصار أدناه على iPhone.
2. اضغط **Get Shortcut**.
3. اضغط **Add Shortcut**.
4. افتح الاختصار الجديد.
5. اضغط مرة واحدة على زر **▶** أسفل اليمين.
6. عندما يطلب iPhone الإذن، اختر **Always Allow**.

> اترك نافذة PowerShell مفتوحة. سيتم اكتشاف iPhone وإقرانه تلقائياً.

## 3. تفعيل النقر مرتين

1. افتح **Settings** على iPhone.
2. اذهب إلى **Accessibility → Touch → Back Tap**.
3. اختر **Double Tap**.
4. اختر **Windows iOS Universal Clipboard**.

## ✅ طريقة الاستخدام

> **انسخ النص على iPhone → انقر مرتين على ظهر iPhone → اضغط Ctrl+V على Windows.**

يبدأ تلقائياً مع Windows ويعمل بصمت في الخلفية.

---

## إيقاف / تشغيل / إزالة

### إيقاف مؤقت

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/stop.ps1 | iex
```

### تشغيل مرة أخرى

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/start.ps1 | iex
```

### إزالة كاملة

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/uninstall.ps1 | iex
```

---

## ملاحظات

- يجب أن يكون iPhone والكمبيوتر على نفس الشبكة المحلية.
- نص الحافظة ينتقل مباشرة عبر الشبكة المحلية ولا توجد خدمة حافظة سحابية.
- استخدمه على شبكات تثق بها.
- الإزالة تحذف التطبيق وبيانات الاقتران وبدء التشغيل وقواعد الجدار الناري. يتم إبقاء Bonjour لأنه قد يُستخدم من تطبيقات Apple أخرى.

## الترخيص

MIT.
