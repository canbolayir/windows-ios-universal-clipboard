<div align="center">

# Windows iOS Universal Clipboard

### Copia en iPhone. Pega en Windows.

Un puente pequeño y sencillo para copiar texto del iPhone a Windows como si fuera Universal Clipboard.

[English](README.md) · [Türkçe](README.tr.md) · [Español](README.es.md) · [简体中文](README.zh-CN.md) · [हिन्दी](README.hi.md) · [العربية](README.ar.md) · [Português](README.pt-BR.md) · [Français](README.fr.md) · [Русский](README.ru.md) · [日本語](README.ja.md)

</div>

---

## 🎬 Vídeo de instalación

[Haz clic en la imagen para ver la instalación completa.](docs/setup.mp4)

[![Setup video](docs/setup-cover.jpg)](docs/setup.mp4)

---

## 1. Instalación

Abre **PowerShell** en Windows y ejecuta:

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/install.ps1 | iex
```

## 2. Configura tu iPhone

**[Añadir el Atajo de iPhone](https://www.icloud.com/shortcuts/e870980e381e4675a27af38c91db1265)**

1. Abre en tu iPhone el enlace del Atajo de abajo.
2. Toca **Get Shortcut**.
3. Toca **Add Shortcut**.
4. Abre el nuevo Atajo.
5. Toca una vez el botón **▶** de la esquina inferior derecha.
6. Cuando iPhone pida permiso, toca **Always Allow**.

> Mantén abierta la ventana de PowerShell. El iPhone se detectará y emparejará automáticamente.

## 3. Activa el doble toque

1. Abre **Ajustes** en el iPhone.
2. Ve a **Accesibilidad → Tocar → Tocar atrás**.
3. Abre **Doble toque**.
4. Elige **Windows iOS Universal Clipboard**.

## ✅ Cómo usarlo

> **Copia texto en el iPhone → toca dos veces la parte trasera del iPhone → pulsa Ctrl+V en Windows.**

Se inicia automáticamente con Windows y funciona en segundo plano.

---

## Detener / Iniciar / Desinstalar

### Detener temporalmente

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/stop.ps1 | iex
```

### Iniciar de nuevo

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/start.ps1 | iex
```

### Eliminar por completo

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/uninstall.ps1 | iex
```

---

## Información útil

- El iPhone y el PC deben estar en la misma red local.
- El texto del portapapeles viaja directamente por tu red local; no hay una cuenta de portapapeles en la nube.
- Úsalo en redes de confianza.
- La desinstalación elimina la app, el emparejamiento, el inicio automático y las reglas del firewall. Bonjour se conserva porque otras apps de Apple pueden usarlo.

## Licencia

MIT.
