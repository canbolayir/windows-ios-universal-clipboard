<div align="center">

# Windows iOS Universal Clipboard

### iPhoneでコピー。Windowsでペースト。

iPhone から Windows へのテキストコピーを Universal Clipboard のように使える小さくシンプルなブリッジです。

[English](README.md) · [Türkçe](README.tr.md) · [Español](README.es.md) · [简体中文](README.zh-CN.md) · [हिन्दी](README.hi.md) · [العربية](README.ar.md) · [Português](README.pt-BR.md) · [Français](README.fr.md) · [Русский](README.ru.md) · [日本語](README.ja.md)

</div>

---

## 🎬 セットアップ動画

[画像をクリックするとセットアップ動画を見られます。](docs/setup.mp4)

[![Setup video](docs/setup-cover.jpg)](docs/setup.mp4)

---

## 1. インストール

Windows で **PowerShell** を開き、次を実行します：

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/install.ps1 | iex
```

## 2. iPhoneを設定

**[iPhoneショートカットを追加](https://www.icloud.com/shortcuts/e870980e381e4675a27af38c91db1265)**

1. 下のショートカットリンクを iPhone で開きます。
2. **Get Shortcut** をタップします。
3. **Add Shortcut** をタップします。
4. 追加されたショートカットを開きます。
5. 右下の **▶** ボタンを1回タップします。
6. iPhone が許可を求めたら **Always Allow** を選びます。

> 作業中は PowerShell を開いたままにしてください。iPhone は自動で検出・ペアリングされます。

## 3. 背面ダブルタップを設定

1. iPhone の **設定** を開きます。
2. **アクセシビリティ → タッチ → 背面タップ** に進みます。
3. **ダブルタップ** を開きます。
4. **Windows iOS Universal Clipboard** を選びます。

## ✅ 使い方

> **iPhoneでテキストをコピー → iPhoneの背面を2回タップ → WindowsでCtrl+V。**

Windows 起動時に自動で開始し、バックグラウンドで動作します。

---

## 停止 / 開始 / アンインストール

### 一時停止

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/stop.ps1 | iex
```

### 再開

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/start.ps1 | iex
```

### 完全に削除

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/uninstall.ps1 | iex
```

---

## 補足

- iPhone と PC は同じローカルネットワークに接続してください。
- クリップボードのテキストはローカルネットワーク上で直接送信され、クラウドアカウントは使いません。
- 信頼できるネットワークで使用してください。
- アンインストールするとアプリ、ペアリング情報、自動起動、ファイアウォールルールが削除されます。Bonjour は他の Apple ソフトが使う可能性があるため残ります。

## ライセンス

MIT.
