# Windows iOS Universal Clipboard

[English](README.md) · [Türkçe](README.tr.md) · [Español](README.es.md) · [简体中文](README.zh-CN.md) · [हिन्दी](README.hi.md) · [العربية](README.ar.md) · [Português](README.pt-BR.md) · [Français](README.fr.md) · [Русский](README.ru.md) · [日本語](README.ja.md)

**在 iPhone 上复制，在 Windows 上粘贴。**

一个小型局域网剪贴板桥接工具，让 iPhone + Windows 获得类似 Universal Clipboard 的体验。

> 非官方项目，与 Apple 或 Microsoft 无隶属关系。“Universal Clipboard”仅作描述性使用。

## 安装

打开 PowerShell 并运行：

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/install.ps1 | iex
```

安装程序会自动完成：

1. 安装并配置 Windows 桥接程序；
2. 必要时安装 Apple Bonjour；
3. 将防火墙访问限制在本地子网；
4. 启用 Windows 登录后的后台自动启动；
5. 显示共享的 iPhone 快捷指令链接；
6. 等待第一次快捷指令请求并自动配对该 iPhone。

无需 IP 地址、电脑名或 token。

共享快捷指令：

**[添加 Windows iOS Universal Clipboard](https://www.icloud.com/shortcuts/e870980e381e4675a27af38c91db1265)**

安装时按以下步骤操作 iPhone：

1. 打开快捷指令链接；
2. 点击 **Get Shortcut**；
3. 点击 **Add Shortcut**；
4. 打开刚添加的快捷指令；
5. 点击右下角的 **> 播放** 按钮运行一次；
6. iPhone 请求权限时选择 **Always Allow**。

操作期间保持安装终端窗口开启。第一个有效请求会自动完成配对：

```text
OK - iPhone detected
OK - device paired
```

然后设置“轻点背面”：

**设置 -> 辅助功能 -> 触控 -> 轻点背面 -> 轻点两下 -> Windows iOS Universal Clipboard**

日常使用：

```text
iPhone 复制 -> 轻点背面两下 -> Windows 按 Ctrl+V
```

## 会自动启动吗？

会。Windows 登录后，`WindowsIOSUniversalClipboard.exe` 会静默在后台启动，无需保持终端窗口开启。

## 临时停止

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/stop.ps1 | iex
```

自动启动仍会保留。

## 再次启动

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/start.ps1 | iex
```

## 完全卸载

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/uninstall.ps1 | iex
```

卸载脚本会删除应用、已配对设备数据、配置和日志、自动启动项、防火墙规则以及旧版 bridge 文件/任务。

Apple Bonjour 默认保留，因为 iTunes、iCloud 或其他 Apple 软件可能仍会使用它。若也要删除：

```powershell
winget uninstall --id Apple.Bonjour
```

## 配对原理

安装程序等待时会开启一个短暂的本地配对窗口。仅在此窗口有效期间，来自电脑直连本地子网、发送有效剪贴板请求的第一个未知设备会被自动加入允许列表。首次配对后窗口立即关闭。

安装过程之外，未知设备**不会**被静默自动批准。

```text
%LOCALAPPDATA%\WindowsIOSUniversalClipboard\config.json
```

## 发现机制

Windows 应用通过 Bonjour/mDNS 发布：

```text
copybridge.local
```

快捷指令始终将文本发送到：

```text
http://copybridge.local:8765/copy
```

## 隐私与安全

剪贴板文本直接通过本地网络传输。项目没有账户系统，也没有云端剪贴板中继。

当前传输使用 HTTP，因此 LAN 上的数据**未加密**。请仅在可信网络使用。Windows 防火墙和应用本身都会把访问限制在直接连接的本地子网。

配对基于本地网络源地址，而不是加密设备身份。如果 DHCP 改变了 iPhone 的地址，可能需要重新配对。

不要将 TCP `8765` 端口暴露到公网。

## 构建

Windows 需要 .NET 10 SDK：

```powershell
dotnet build .\src\WindowsIOSUniversalClipboard\WindowsIOSUniversalClipboard.csproj
```

## 许可证

MIT.
