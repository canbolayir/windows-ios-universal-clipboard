<div align="center">

# Windows iOS Universal Clipboard

### Copie no iPhone. Cole no Windows.

Uma ponte pequena e simples para copiar texto do iPhone para o Windows como no Universal Clipboard.

[English](README.md) · [Türkçe](README.tr.md) · [Español](README.es.md) · [简体中文](README.zh-CN.md) · [हिन्दी](README.hi.md) · [العربية](README.ar.md) · [Português](README.pt-BR.md) · [Français](README.fr.md) · [Русский](README.ru.md) · [日本語](README.ja.md)

</div>

---

## 🎬 Vídeo de instalação

[Clique na imagem para assistir ao processo completo.](docs/setup.mp4)

[![Setup video](docs/setup-cover.jpg)](docs/setup.mp4)

---

## 1. Instalação

Abra o **PowerShell** no Windows e execute:

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/install.ps1 | iex
```

## 2. Configure o iPhone

**[Adicionar o Atalho do iPhone](https://www.icloud.com/shortcuts/e870980e381e4675a27af38c91db1265)**

1. Abra no iPhone o link do Atalho abaixo.
2. Toque em **Get Shortcut**.
3. Toque em **Add Shortcut**.
4. Abra o novo Atalho.
5. Toque uma vez no botão **▶** no canto inferior direito.
6. Quando o iPhone pedir permissão, toque em **Always Allow**.

> Mantenha a janela do PowerShell aberta. O iPhone será detectado e pareado automaticamente.

## 3. Ative o toque duplo

1. Abra **Ajustes** no iPhone.
2. Vá em **Acessibilidade → Toque → Tocar Atrás**.
3. Abra **Toque Duplo**.
4. Selecione **Windows iOS Universal Clipboard**.

## ✅ Como usar

> **Copie o texto no iPhone → toque duas vezes na parte de trás do iPhone → pressione Ctrl+V no Windows.**

Ele inicia automaticamente com o Windows e roda silenciosamente em segundo plano.

---

## Parar / Iniciar / Desinstalar

### Parar temporariamente

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/stop.ps1 | iex
```

### Iniciar novamente

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/start.ps1 | iex
```

### Remover completamente

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/uninstall.ps1 | iex
```

---

## Bom saber

- O iPhone e o PC devem estar na mesma rede local.
- O texto da área de transferência vai diretamente pela rede local; não existe conta de clipboard na nuvem.
- Use em redes confiáveis.
- A desinstalação remove o app, o pareamento, a inicialização automática e as regras de firewall. O Bonjour fica instalado porque outros apps da Apple podem usá-lo.

## Licença

MIT.
