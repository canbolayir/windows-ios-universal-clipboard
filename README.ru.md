# Windows iOS Universal Clipboard

[English](README.md) · [Türkçe](README.tr.md) · [Español](README.es.md) · [简体中文](README.zh-CN.md) · [हिन्दी](README.hi.md) · [العربية](README.ar.md) · [Português](README.pt-BR.md) · [Français](README.fr.md) · [Русский](README.ru.md) · [日本語](README.ja.md)

**Копируйте на iPhone. Вставляйте в Windows.**

Небольшой мост буфера обмена по локальной сети, создающий между iPhone и Windows сценарий, похожий на Universal Clipboard.

> Неофициальный проект. Не связан с Apple или Microsoft. Название "Universal Clipboard" используется только для описания.

## Установка

Откройте PowerShell и выполните:

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/install.ps1 | iex
```

Установщик автоматически:

1. устанавливает и настраивает Windows-bridge;
2. при необходимости устанавливает Apple Bonjour;
3. ограничивает правило брандмауэра локальной подсетью;
4. включает автоматический фоновый запуск;
5. показывает ссылку на iPhone Shortcut;
6. ждёт первый запрос Shortcut и автоматически привязывает iPhone.

IP-адрес, имя ПК и token не нужны.

Общий Shortcut:

**[Добавить Windows iOS Universal Clipboard](https://www.icloud.com/shortcuts/e870980e381e4675a27af38c91db1265)**

Во время установки на iPhone:

1. откройте ссылку Shortcut;
2. нажмите **Get Shortcut**;
3. нажмите **Add Shortcut**;
4. откройте добавленный Shortcut;
5. нажмите кнопку **> play** внизу справа и запустите его один раз;
6. при запросе разрешения выберите **Always Allow**.

Не закрывайте терминал. Первый корректный запрос будет автоматически привязан:

```text
OK - iPhone detected
OK - device paired
```

Затем настройте Back Tap:

**Settings -> Accessibility -> Touch -> Back Tap -> Double Tap -> Windows iOS Universal Clipboard**

Обычное использование:

```text
Копировать на iPhone -> двойное касание задней панели -> Ctrl+V в Windows
```

## Запускается автоматически?

Да. После входа в Windows `WindowsIOSUniversalClipboard.exe` тихо запускается в фоне.

## Временно остановить

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/stop.ps1 | iex
```

Автозапуск останется включён.

## Запустить снова

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/start.ps1 | iex
```

## Полностью удалить

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/uninstall.ps1 | iex
```

Удаляются приложение, данные привязанных устройств, конфигурация и логи, автозапуск, правила брандмауэра и старые файлы/задачи bridge.

Apple Bonjour остаётся установленным, поскольку его могут использовать другие программы Apple. Чтобы удалить и его:

```powershell
winget uninstall --id Apple.Bonjour
```

## Как работает привязка

Установщик на короткое время открывает локальное окно привязки. Только пока оно активно, первое неизвестное устройство из локальной подсети, отправившее корректный запрос, автоматически добавляется в список разрешённых. После первой привязки окно закрывается.

Вне установки неизвестные устройства **не подтверждаются автоматически**.

```text
%LOCALAPPDATA%\WindowsIOSUniversalClipboard\config.json
```

## Как работает обнаружение

Приложение публикует `copybridge.local` через Bonjour/mDNS, а Shortcut отправляет текст на:

```text
http://copybridge.local:8765/copy
```

## Конфиденциальность и безопасность

Текст буфера обмена передаётся напрямую по локальной сети. Аккаунтов и облачного relay нет.

Сейчас используется HTTP, поэтому трафик в LAN **не шифруется**. Используйте только доверенные сети. Брандмауэр Windows и приложение ограничивают доступ напрямую подключёнными локальными подсетями.

Привязка основана на сетевом адресе источника, а не на криптографической идентичности устройства. Если DHCP изменит адрес iPhone, может потребоваться повторная привязка.

Не открывайте TCP-порт `8765` в публичный интернет.

## Сборка

Требуется .NET 10 SDK для Windows:

```powershell
dotnet build .\src\WindowsIOSUniversalClipboard\WindowsIOSUniversalClipboard.csproj
```

## Лицензия

MIT.
