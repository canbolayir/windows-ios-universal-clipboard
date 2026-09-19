<div align="center">

# Windows iOS Universal Clipboard

### Копируйте на iPhone. Вставляйте в Windows.

Небольшой простой мост для копирования текста с iPhone в Windows почти как Universal Clipboard.

[English](README.md) · [Türkçe](README.tr.md) · [Español](README.es.md) · [简体中文](README.zh-CN.md) · [हिन्दी](README.hi.md) · [العربية](README.ar.md) · [Português](README.pt-BR.md) · [Français](README.fr.md) · [Русский](README.ru.md) · [日本語](README.ja.md)

</div>

---

## 🎬 Видео установки

[Нажмите на изображение, чтобы посмотреть полную установку.](docs/setup.mp4)

[![Setup video](docs/setup-cover.jpg)](docs/setup.mp4)

---

## 1. Установка

Откройте **PowerShell** в Windows и выполните:

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/install.ps1 | iex
```

## 2. Настройте iPhone

**[Добавить Команду на iPhone](https://www.icloud.com/shortcuts/e870980e381e4675a27af38c91db1265)**

1. Откройте на iPhone ссылку на Команду ниже.
2. Нажмите **Get Shortcut**.
3. Нажмите **Add Shortcut**.
4. Откройте новую Команду.
5. Один раз нажмите кнопку **▶** в правом нижнем углу.
6. Когда iPhone запросит разрешение, выберите **Always Allow**.

> Не закрывайте PowerShell. iPhone будет обнаружен и сопряжён автоматически.

## 3. Настройте двойное касание

1. Откройте **Настройки** на iPhone.
2. Перейдите в **Универсальный доступ → Касание → Касание задней панели**.
3. Откройте **Двойное касание**.
4. Выберите **Windows iOS Universal Clipboard**.

## ✅ Как пользоваться

> **Скопируйте текст на iPhone → дважды коснитесь задней панели iPhone → нажмите Ctrl+V в Windows.**

Приложение автоматически запускается вместе с Windows и работает в фоне.

---

## Остановить / Запустить / Удалить

### Временно остановить

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/stop.ps1 | iex
```

### Запустить снова

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/start.ps1 | iex
```

### Удалить полностью

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/uninstall.ps1 | iex
```

---

## Важно

- iPhone и ПК должны быть в одной локальной сети.
- Текст буфера обмена передаётся напрямую по локальной сети; облачного аккаунта буфера обмена нет.
- Используйте только в доверенных сетях.
- Удаление убирает приложение, данные сопряжения, автозапуск и правила брандмауэра. Bonjour остаётся установленным, так как его могут использовать другие программы Apple.

## Лицензия

MIT.
