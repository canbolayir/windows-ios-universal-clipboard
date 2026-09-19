# Windows iOS Universal Clipboard

[English](README.md) · [Türkçe](README.tr.md) · [Español](README.es.md) · [简体中文](README.zh-CN.md) · [हिन्दी](README.hi.md) · [العربية](README.ar.md) · [Português](README.pt-BR.md) · [Français](README.fr.md) · [Русский](README.ru.md) · [日本語](README.ja.md)

**iPhone でコピー。Windows でペースト。**

iPhone + Windows 間で Universal Clipboard のような操作感を実現する、小さなローカルネットワーク用クリップボードブリッジです。

> 非公式プロジェクトです。Apple または Microsoft とは関係ありません。「Universal Clipboard」は説明目的で使用しています。

## インストール

PowerShell を開いて実行します：

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/install.ps1 | iex
```

インストーラーは自動で次を行います：

1. Windows ブリッジのインストールと設定；
2. 必要に応じて Apple Bonjour をインストール；
3. ファイアウォールをローカルサブネットのみに制限；
4. Windows サインイン後のバックグラウンド自動起動を有効化；
5. iPhone ショートカットのリンクを表示；
6. 最初のショートカット要求を待ち、その iPhone を自動ペアリング。

IP アドレス、PC 名、token は不要です。

共有ショートカット：

**[Windows iOS Universal Clipboard を追加](https://www.icloud.com/shortcuts/e870980e381e4675a27af38c91db1265)**

インストール中、iPhone で次の操作を行います：

1. ショートカットのリンクを開く；
2. **Get Shortcut** をタップ；
3. **Add Shortcut** をタップ；
4. 追加したショートカットを開く；
5. 右下の **> 再生** ボタンをタップして一度実行；
6. 権限を求められたら **Always Allow** を選択。

作業中はインストーラーのターミナルを開いたままにしてください。最初の有効な要求が自動でペアリングされます：

```text
OK - iPhone detected
OK - device paired
```

続いて背面タップを設定します：

**設定 -> アクセシビリティ -> タッチ -> 背面タップ -> ダブルタップ -> Windows iOS Universal Clipboard**

通常の使い方：

```text
iPhone でコピー -> 背面をダブルタップ -> Windows で Ctrl+V
```

## 自動起動しますか？

はい。Windows にサインインすると `WindowsIOSUniversalClipboard.exe` がバックグラウンドで静かに起動します。

## 一時停止

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/stop.ps1 | iex
```

自動起動設定は残ります。

## 再開

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/start.ps1 | iex
```

## 完全アンインストール

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/uninstall.ps1 | iex
```

アプリ、ペアリング済みデバイス情報、設定とログ、自動起動、ファイアウォールルール、旧 bridge のファイル/タスクを削除します。

Apple Bonjour は、iTunes や iCloud など他の Apple ソフトが使用する可能性があるため残します。Bonjour も削除する場合：

```powershell
winget uninstall --id Apple.Bonjour
```

## ペアリングの仕組み

インストーラーは短時間だけローカルのペアリングウィンドウを開きます。その間に、直接接続されたローカルサブネットから有効なクリップボード要求を送った最初の未知デバイスを許可リストへ自動追加します。最初のペアリング後、ウィンドウは直ちに閉じます。

インストール中以外は、未知のデバイスが**自動承認されることはありません**。

```text
%LOCALAPPDATA%\WindowsIOSUniversalClipboard\config.json
```

## 検出の仕組み

Windows アプリは Bonjour/mDNS で `copybridge.local` を公開し、ショートカットは次の URL にテキストを送ります：

```text
http://copybridge.local:8765/copy
```

## プライバシーとセキュリティ

クリップボードのテキストはローカルネットワーク上を直接流れます。アカウント機能やクラウド中継はありません。

現在の転送方式は HTTP のため、LAN 上の通信は**暗号化されません**。信頼できるネットワークでのみ使用してください。Windows ファイアウォールとアプリの両方がアクセスを直接接続されたローカルサブネットに制限します。

ペアリングは暗号学的なデバイス識別ではなく、ローカルネットワークの送信元アドレスに基づきます。DHCP により iPhone のアドレスが変わった場合、再ペアリングが必要になることがあります。

TCP ポート `8765` をインターネットへ公開しないでください。

## ビルド

Windows で .NET 10 SDK が必要です：

```powershell
dotnet build .\src\WindowsIOSUniversalClipboard\WindowsIOSUniversalClipboard.csproj
```

## ライセンス

MIT.
