# Windows iOS Universal Clipboard

[English](README.md) · [Türkçe](README.tr.md) · [Español](README.es.md) · [简体中文](README.zh-CN.md) · [हिन्दी](README.hi.md) · [العربية](README.ar.md) · [Português](README.pt-BR.md) · [Français](README.fr.md) · [Русский](README.ru.md) · [日本語](README.ja.md)

**Copia en iPhone. Pega en Windows.**

Un pequeño puente de portapapeles por red local que ofrece un flujo tipo Universal Clipboard entre iPhone y Windows.

> Proyecto no oficial. No está afiliado con Apple ni Microsoft. "Universal Clipboard" se usa de forma descriptiva.

## Instalación

Abre PowerShell y ejecuta:

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/install.ps1 | iex
```

El instalador:

1. instala y configura el puente de Windows;
2. instala Apple Bonjour si hace falta;
3. limita el firewall a la subred local;
4. activa el inicio automático en segundo plano;
5. muestra el enlace del Atajo de iPhone;
6. espera la primera solicitud del Atajo y empareja ese iPhone automáticamente.

No necesitas IP, nombre del PC ni token.

Atajo compartido:

**[Añadir Windows iOS Universal Clipboard](https://www.icloud.com/shortcuts/e870980e381e4675a27af38c91db1265)**

Durante la instalación, sigue estos pasos en el iPhone:

1. abre el enlace del Atajo;
2. toca **Get Shortcut**;
3. toca **Add Shortcut**;
4. abre el Atajo recién añadido;
5. toca el botón **> reproducir** de la esquina inferior derecha para ejecutarlo una vez;
6. cuando iPhone pida permiso, toca **Always Allow**.

Mantén abierta la terminal del instalador. La primera solicitud válida se empareja automáticamente:

```text
OK - iPhone detected
OK - device paired
```

Después configura Toque posterior:

**Ajustes -> Accesibilidad -> Tocar -> Tocar atrás -> Tocar dos veces -> Windows iOS Universal Clipboard**

Uso normal:

```text
Copiar en iPhone -> doble toque atrás -> Ctrl+V en Windows
```

## ¿Se inicia automáticamente?

Sí. Tras iniciar sesión en Windows, `WindowsIOSUniversalClipboard.exe` se inicia silenciosamente en segundo plano.

## Detener temporalmente

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/stop.ps1 | iex
```

El inicio automático sigue activado.

## Iniciar de nuevo

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/start.ps1 | iex
```

## Desinstalar por completo

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/uninstall.ps1 | iex
```

El desinstalador elimina la app, dispositivos emparejados, configuración, logs, inicio automático, reglas de firewall y archivos/tareas antiguas.

Apple Bonjour se conserva porque otras apps de Apple pueden usarlo. Para eliminarlo también:

```powershell
winget uninstall --id Apple.Bonjour
```

## Cómo funciona el emparejamiento

Durante la instalación se abre una ventana de emparejamiento local temporal. Solo mientras está activa, el primer dispositivo desconocido de la subred local que envíe una solicitud válida se añade automáticamente a la lista aprobada. Después, la ventana se cierra.

Fuera de la instalación, los dispositivos desconocidos **no** se aprueban automáticamente.

```text
%LOCALAPPDATA%\WindowsIOSUniversalClipboard\config.json
```

## Cómo funciona el descubrimiento

La app publica `copybridge.local` mediante Bonjour/mDNS y el Atajo envía el texto a:

```text
http://copybridge.local:8765/copy
```

## Privacidad y seguridad

El texto del portapapeles viaja directamente por la red local. No hay cuentas ni relay en la nube.

El transporte actual usa HTTP, por lo que el tráfico **no está cifrado** en la LAN. Úsalo solo en redes de confianza. El firewall y la app limitan el acceso a subredes locales directamente conectadas.

El emparejamiento se basa en direcciones de origen de la red local, no en identidad criptográfica. Si DHCP cambia la IP del iPhone, puede ser necesario volver a emparejar.

No expongas el puerto TCP `8765` a Internet.

## Compilar

Requiere .NET 10 SDK en Windows:

```powershell
dotnet build .\src\WindowsIOSUniversalClipboard\WindowsIOSUniversalClipboard.csproj
```

## Licencia

MIT.
