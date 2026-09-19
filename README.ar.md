# Windows iOS Universal Clipboard

[English](README.md) · [Türkçe](README.tr.md) · [Español](README.es.md) · [简体中文](README.zh-CN.md) · [हिन्दी](README.hi.md) · [العربية](README.ar.md) · [Português](README.pt-BR.md) · [Français](README.fr.md) · [Русский](README.ru.md) · [日本語](README.ja.md)

**انسخ على iPhone والصق على Windows.**

جسر صغير للحافظة عبر الشبكة المحلية يمنح iPhone + Windows تجربة شبيهة بـ Universal Clipboard.

> مشروع غير رسمي وغير تابع لـ Apple أو Microsoft. يُستخدم اسم "Universal Clipboard" للوصف فقط.

## التثبيت

افتح PowerShell وشغّل:

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/install.ps1 | iex
```

يقوم المثبّت تلقائيًا بـ:

1. تثبيت جسر Windows وإعداده؛
2. تثبيت Apple Bonjour عند الحاجة؛
3. تقييد جدار الحماية بالشبكة الفرعية المحلية فقط؛
4. تفعيل التشغيل التلقائي في الخلفية بعد تسجيل الدخول إلى Windows؛
5. عرض رابط اختصار iPhone؛
6. انتظار أول طلب من الاختصار وإقران iPhone تلقائيًا.

لا تحتاج إلى عنوان IP أو اسم الكمبيوتر أو token.

الاختصار المشترك:

**[إضافة Windows iOS Universal Clipboard](https://www.icloud.com/shortcuts/e870980e381e4675a27af38c91db1265)**

أثناء التثبيت على iPhone:

1. افتح رابط الاختصار؛
2. اضغط **Get Shortcut**؛
3. اضغط **Add Shortcut**؛
4. افتح الاختصار الذي أُضيف؛
5. اضغط زر **> التشغيل** أسفل اليمين لتشغيله مرة واحدة؛
6. عند طلب الإذن اختر **Always Allow**.

اترك نافذة الطرفية مفتوحة. أول طلب صالح سيُقرن تلقائيًا:

```text
OK - iPhone detected
OK - device paired
```

ثم اضبط Back Tap:

**Settings -> Accessibility -> Touch -> Back Tap -> Double Tap -> Windows iOS Universal Clipboard**

الاستخدام اليومي:

```text
انسخ على iPhone -> انقر مرتين على ظهر iPhone -> Ctrl+V على Windows
```

## هل يبدأ تلقائيًا؟

نعم. بعد تسجيل الدخول إلى Windows يبدأ `WindowsIOSUniversalClipboard.exe` بصمت في الخلفية.

## إيقاف مؤقت

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/stop.ps1 | iex
```

سيبقى التشغيل التلقائي مفعّلًا.

## التشغيل من جديد

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/start.ps1 | iex
```

## الإزالة الكاملة

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/uninstall.ps1 | iex
```

يزيل برنامج الإزالة التطبيق وبيانات الأجهزة المقترنة والإعدادات والسجلات والتشغيل التلقائي وقواعد جدار الحماية وملفات/مهام الإصدارات القديمة.

يُترك Apple Bonjour مثبتًا لأن برامج Apple الأخرى قد تستخدمه. لإزالته أيضًا:

```powershell
winget uninstall --id Apple.Bonjour
```

## كيف يعمل الاقتران؟

يفتح المثبّت نافذة اقتران محلية قصيرة. أثناء تفعيلها فقط، يُضاف أول جهاز غير معروف من الشبكة الفرعية المحلية يرسل طلب حافظة صالحًا إلى قائمة الأجهزة المسموح بها. تُغلق النافذة مباشرة بعد أول اقتران.

خارج عملية التثبيت، الأجهزة غير المعروفة **لا تُعتمد تلقائيًا**.

```text
%LOCALAPPDATA%\WindowsIOSUniversalClipboard\config.json
```

## كيف يعمل الاكتشاف؟

ينشر التطبيق `copybridge.local` عبر Bonjour/mDNS ويرسل الاختصار النص إلى:

```text
http://copybridge.local:8765/copy
```

## الخصوصية والأمان

ينتقل نص الحافظة مباشرة عبر الشبكة المحلية. لا توجد حسابات ولا خدمة وسيطة سحابية.

النقل الحالي يستخدم HTTP، لذلك البيانات على LAN **غير مشفرة**. استخدمه فقط على شبكات تثق بها. يقيّد جدار حماية Windows والتطبيق الوصول إلى الشبكات الفرعية المحلية المتصلة مباشرة.

يعتمد الاقتران على عنوان المصدر في الشبكة المحلية وليس على هوية جهاز مشفرة. إذا غيّر DHCP عنوان iPhone فقد تحتاج إلى الاقتران من جديد.

لا تعرض منفذ TCP `8765` للإنترنت العام.

## البناء

يتطلب .NET 10 SDK على Windows:

```powershell
dotnet build .\src\WindowsIOSUniversalClipboard\WindowsIOSUniversalClipboard.csproj
```

## الترخيص

MIT.
