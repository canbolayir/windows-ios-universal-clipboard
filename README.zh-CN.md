<div align="center">

# Windows iOS Universal Clipboard

### 在 iPhone 上复制，在 Windows 上粘贴。

一个简单的小工具，让 iPhone 到 Windows 的文字复制体验接近 Universal Clipboard。

[English](README.md) · [Türkçe](README.tr.md) · [Español](README.es.md) · [简体中文](README.zh-CN.md) · [हिन्दी](README.hi.md) · [العربية](README.ar.md) · [Português](README.pt-BR.md) · [Français](README.fr.md) · [Русский](README.ru.md) · [日本語](README.ja.md)

</div>

---

## 🎬 安装视频

[点击图片观看完整安装视频。](docs/setup.mp4)

[![Setup video](docs/setup-cover.jpg)](docs/setup.mp4)

---

## 1. 安装

在 Windows 上打开 **PowerShell**，运行：

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/install.ps1 | iex
```

## 2. 设置 iPhone

**[添加 iPhone 快捷指令](https://www.icloud.com/shortcuts/e870980e381e4675a27af38c91db1265)**

1. 在 iPhone 上打开下面的快捷指令链接。
2. 点 **Get Shortcut**。
3. 点 **Add Shortcut**。
4. 打开新添加的快捷指令。
5. 点一次右下角的 **▶** 按钮。
6. iPhone 请求权限时，选择 **Always Allow**。

> 操作时保持 PowerShell 窗口打开。电脑会自动检测并配对你的 iPhone。

## 3. 设置轻点背面两下

1. 打开 iPhone **设置**。
2. 进入 **辅助功能 → 触控 → 轻点背面**。
3. 打开 **轻点两下**。
4. 选择 **Windows iOS Universal Clipboard**。

## ✅ 使用方法

> **在 iPhone 上复制文字 → 轻点手机背面两下 → 在 Windows 上按 Ctrl+V。**

它会随 Windows 自动启动，并在后台静默运行。

---

## 停止 / 启动 / 卸载

### 临时停止

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/stop.ps1 | iex
```

### 重新启动

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/start.ps1 | iex
```

### 完全卸载

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/uninstall.ps1 | iex
```

---

## 注意

- iPhone 和电脑应连接到同一个本地网络。
- 剪贴板文字直接通过本地网络传输，不使用云剪贴板账户。
- 请在你信任的网络上使用。
- 卸载会删除应用、配对数据、启动项和防火墙规则。Bonjour 会保留，因为其他 Apple 软件可能会用到它。

## 许可证

MIT.
