# Windows iOS Universal Clipboard

[English](README.md) · [Türkçe](README.tr.md) · [Español](README.es.md) · [简体中文](README.zh-CN.md) · [हिन्दी](README.hi.md) · [العربية](README.ar.md) · [Português](README.pt-BR.md) · [Français](README.fr.md) · [Русский](README.ru.md) · [日本語](README.ja.md)

**Copie no iPhone. Cole no Windows.**

Uma pequena ponte de área de transferência pela rede local que oferece uma experiência semelhante ao Universal Clipboard entre iPhone e Windows.

> Projeto não oficial. Não afiliado à Apple nem à Microsoft. "Universal Clipboard" é usado apenas de forma descritiva.

## Instalação

Abra o PowerShell e execute:

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/install.ps1 | iex
```

O instalador:

1. instala e configura a ponte do Windows;
2. instala o Apple Bonjour quando necessário;
3. limita o firewall à sub-rede local;
4. ativa a inicialização automática em segundo plano;
5. mostra o link do Atalho do iPhone;
6. aguarda a primeira solicitação do Atalho e emparelha o iPhone automaticamente.

Não é necessário IP, nome do PC ou token.

Atalho compartilhado:

**[Adicionar Windows iOS Universal Clipboard](https://www.icloud.com/shortcuts/e870980e381e4675a27af38c91db1265)**

Durante a instalação, no iPhone:

1. abra o link do Atalho;
2. toque em **Get Shortcut**;
3. toque em **Add Shortcut**;
4. abra o Atalho recém-adicionado;
5. toque no botão **> play** no canto inferior direito para executá-lo uma vez;
6. quando o iPhone pedir permissão, toque em **Always Allow**.

Mantenha o terminal aberto. A primeira solicitação válida será emparelhada automaticamente:

```text
OK - iPhone detected
OK - device paired
```

Depois configure o Toque Atrás:

**Ajustes -> Acessibilidade -> Toque -> Tocar Atrás -> Toque Duplo -> Windows iOS Universal Clipboard**

Uso normal:

```text
Copiar no iPhone -> toque duplo atrás -> Ctrl+V no Windows
```

## Inicia automaticamente?

Sim. Após entrar no Windows, `WindowsIOSUniversalClipboard.exe` inicia silenciosamente em segundo plano.

## Parar temporariamente

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/stop.ps1 | iex
```

A inicialização automática permanece ativa.

## Iniciar novamente

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/start.ps1 | iex
```

## Desinstalar completamente

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/uninstall.ps1 | iex
```

O desinstalador remove o app, dispositivos emparelhados, configuração, logs, inicialização automática, regras de firewall e arquivos/tarefas antigos.

O Apple Bonjour é mantido porque outros softwares da Apple podem usá-lo. Para removê-lo também:

```powershell
winget uninstall --id Apple.Bonjour
```

## Como funciona o emparelhamento

O instalador abre uma janela local de emparelhamento por pouco tempo. Apenas enquanto ela está ativa, o primeiro dispositivo desconhecido da sub-rede local que enviar uma solicitação válida é adicionado automaticamente à lista aprovada. A janela fecha após o primeiro emparelhamento.

Fora da instalação, dispositivos desconhecidos **não** são aprovados automaticamente.

```text
%LOCALAPPDATA%\WindowsIOSUniversalClipboard\config.json
```

## Como funciona a descoberta

O aplicativo publica `copybridge.local` via Bonjour/mDNS, e o Atalho envia o texto para:

```text
http://copybridge.local:8765/copy
```

## Privacidade e segurança

O texto da área de transferência trafega diretamente pela rede local. Não há contas nem relay em nuvem.

O transporte atual usa HTTP, então o tráfego **não é criptografado** na LAN. Use apenas em redes confiáveis. O firewall e o app limitam o acesso às sub-redes locais diretamente conectadas.

O emparelhamento usa o endereço de origem da rede local, não uma identidade criptográfica do dispositivo. Se o DHCP mudar o endereço do iPhone, talvez seja necessário emparelhar novamente.

Não exponha a porta TCP `8765` à internet pública.

## Build

Requer .NET 10 SDK no Windows:

```powershell
dotnet build .\src\WindowsIOSUniversalClipboard\WindowsIOSUniversalClipboard.csproj
```

## Licença

MIT.
