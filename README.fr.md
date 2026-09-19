# Windows iOS Universal Clipboard

[English](README.md) · [Türkçe](README.tr.md) · [Español](README.es.md) · [简体中文](README.zh-CN.md) · [हिन्दी](README.hi.md) · [العربية](README.ar.md) · [Português](README.pt-BR.md) · [Français](README.fr.md) · [Русский](README.ru.md) · [日本語](README.ja.md)

**Copiez sur iPhone. Collez sur Windows.**

Un petit pont de presse-papiers sur le réseau local qui offre une expérience proche d’Universal Clipboard entre iPhone et Windows.

> Projet non officiel. Aucun lien avec Apple ou Microsoft. « Universal Clipboard » est utilisé uniquement à titre descriptif.

## Installation

Ouvrez PowerShell et exécutez :

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/install.ps1 | iex
```

L’installateur :

1. installe et configure le bridge Windows ;
2. installe Apple Bonjour si nécessaire ;
3. limite le pare-feu au sous-réseau local ;
4. active le démarrage automatique en arrière-plan ;
5. affiche le lien du Raccourci iPhone ;
6. attend la première requête du Raccourci et associe automatiquement l’iPhone.

Aucune adresse IP, nom de PC ou token n’est nécessaire.

Raccourci partagé :

**[Ajouter Windows iOS Universal Clipboard](https://www.icloud.com/shortcuts/e870980e381e4675a27af38c91db1265)**

Pendant l’installation, sur l’iPhone :

1. ouvrez le lien du Raccourci ;
2. touchez **Get Shortcut** ;
3. touchez **Add Shortcut** ;
4. ouvrez le Raccourci nouvellement ajouté ;
5. touchez le bouton **> lecture** en bas à droite pour l’exécuter une fois ;
6. quand l’iPhone demande l’autorisation, choisissez **Always Allow**.

Gardez le terminal ouvert. La première requête valide sera associée automatiquement :

```text
OK - iPhone detected
OK - device paired
```

Configurez ensuite Toucher le dos :

**Réglages -> Accessibilité -> Toucher -> Toucher le dos de l’appareil -> Toucher deux fois -> Windows iOS Universal Clipboard**

Utilisation normale :

```text
Copier sur iPhone -> double toucher au dos -> Ctrl+V sur Windows
```

## Démarre-t-il automatiquement ?

Oui. Après la connexion à Windows, `WindowsIOSUniversalClipboard.exe` démarre silencieusement en arrière-plan.

## Arrêter temporairement

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/stop.ps1 | iex
```

Le démarrage automatique reste activé.

## Redémarrer

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/start.ps1 | iex
```

## Désinstaller complètement

```powershell
irm https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/uninstall.ps1 | iex
```

Le programme de désinstallation supprime l’application, les appareils associés, la configuration, les logs, le démarrage automatique, les règles du pare-feu et les anciens fichiers/tâches.

Apple Bonjour est conservé car d’autres logiciels Apple peuvent l’utiliser. Pour le supprimer aussi :

```powershell
winget uninstall --id Apple.Bonjour
```

## Fonctionnement de l’association

L’installateur ouvre brièvement une fenêtre d’association locale. Tant qu’elle est active, le premier appareil inconnu du sous-réseau local qui envoie une requête valide est ajouté automatiquement à la liste autorisée. La fenêtre se ferme après la première association.

Hors installation, les appareils inconnus **ne sont pas approuvés automatiquement**.

```text
%LOCALAPPDATA%\WindowsIOSUniversalClipboard\config.json
```

## Fonctionnement de la découverte

L’application publie `copybridge.local` via Bonjour/mDNS et le Raccourci envoie le texte vers :

```text
http://copybridge.local:8765/copy
```

## Confidentialité et sécurité

Le texte du presse-papiers circule directement sur le réseau local. Aucun compte ni relais cloud.

Le transport actuel utilise HTTP : le trafic LAN **n’est pas chiffré**. Utilisez-le uniquement sur des réseaux de confiance. Le pare-feu Windows et l’application limitent l’accès aux sous-réseaux locaux directement connectés.

L’association repose sur l’adresse source du réseau local et non sur une identité cryptographique de l’appareil. Si DHCP change l’adresse de l’iPhone, une nouvelle association peut être nécessaire.

N’exposez pas le port TCP `8765` à Internet.

## Compilation

Nécessite .NET 10 SDK sous Windows :

```powershell
dotnet build .\src\WindowsIOSUniversalClipboard\WindowsIOSUniversalClipboard.csproj
```

## Licence

MIT.
