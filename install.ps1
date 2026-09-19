$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

$Repo = "canbolayir/windows-ios-universal-clipboard"
$ShortcutUrl = "https://www.icloud.com/shortcuts/e870980e381e4675a27af38c91db1265"
$InstallDir = Join-Path $env:LOCALAPPDATA "WindowsIOSUniversalClipboard"
$ExePath = Join-Path $InstallDir "WindowsIOSUniversalClipboard.exe"
$PairingPath = Join-Path $InstallDir "pairing.enabled"
$RunKey = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run"
$RunName = "WindowsIOSUniversalClipboard"
$Port = 8765

function Test-IsAdmin {
    $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($identity)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

function Ensure-Admin {
    if (Test-IsAdmin) { return }

    Write-Host "Administrator permission is required once for firewall setup." -ForegroundColor Yellow
    $tempScript = Join-Path $env:TEMP "windows-ios-universal-clipboard-install.ps1"
    Invoke-WebRequest "https://raw.githubusercontent.com/$Repo/main/install.ps1" -OutFile $tempScript -UseBasicParsing

    Start-Process powershell.exe -Verb RunAs -Wait -ArgumentList @(
        "-NoProfile",
        "-ExecutionPolicy", "Bypass",
        "-File", "`"$tempScript`""
    )
    exit
}

function Ensure-Bonjour {
    $dll = Join-Path $env:WINDIR "System32\dnssd.dll"
    $service = Get-Service "Bonjour Service" -ErrorAction SilentlyContinue

    if ((Test-Path $dll) -and $service) {
        if ($service.Status -ne "Running") {
            Start-Service "Bonjour Service"
        }
        return
    }

    Write-Host "[1/4] Installing Apple Bonjour for local discovery..." -ForegroundColor Cyan

    $winget = Get-Command winget.exe -ErrorAction SilentlyContinue
    if (-not $winget) {
        throw "winget is required to install Apple Bonjour automatically. Install App Installer from Microsoft Store and run this installer again."
    }

    $wingetExe = if ($winget.Path) { $winget.Path } elseif ($winget.Source) { $winget.Source } else { "winget.exe" }
    & $wingetExe install `
        --id Apple.Bonjour `
        --exact `
        --accept-package-agreements `
        --accept-source-agreements `
        --silent

    if ($LASTEXITCODE -notin 0, -1978335189) {
        throw "Apple Bonjour installation failed with exit code $LASTEXITCODE."
    }

    $service = Get-Service "Bonjour Service" -ErrorAction SilentlyContinue
    if (-not $service) {
        throw "Apple Bonjour installation completed but the Bonjour service was not found."
    }

    if ($service.Status -ne "Running") {
        Start-Service "Bonjour Service"
    }

    if (-not (Test-Path $dll)) {
        throw "Apple Bonjour is installed but dnssd.dll was not found."
    }
}

Ensure-Admin

Write-Host ""
Write-Host "Windows iOS Universal Clipboard" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor DarkGray
Write-Host ""

if (-not [Environment]::Is64BitOperatingSystem) {
    throw "This release currently supports x64 Windows only."
}

Ensure-Bonjour

Write-Host "[1/4] Installing CopyBridge..." -ForegroundColor Cyan

$headers = @{
    "User-Agent" = "windows-ios-universal-clipboard-installer"
    "Accept" = "application/vnd.github+json"
}

$release = Invoke-RestMethod "https://api.github.com/repos/$Repo/releases/latest" -Headers $headers
$assetName = "windows-ios-universal-clipboard-win-x64.zip"
$asset = $release.assets | Where-Object { $_.name -eq $assetName } | Select-Object -First 1

if (-not $asset) {
    throw "Release asset not found: $assetName"
}

$tempRoot = Join-Path $env:TEMP ("windows-ios-universal-clipboard-" + [Guid]::NewGuid().ToString("N"))
$zipPath = Join-Path $tempRoot "app.zip"
$extractDir = Join-Path $tempRoot "app"

New-Item -ItemType Directory -Path $tempRoot -Force | Out-Null

try {
    Invoke-WebRequest $asset.browser_download_url -OutFile $zipPath -UseBasicParsing
    Expand-Archive $zipPath -DestinationPath $extractDir -Force

    Get-Process WindowsIOSUniversalClipboard -ErrorAction SilentlyContinue | Stop-Process -Force
    Get-Process iPhoneClipboardBridge -ErrorAction SilentlyContinue | Stop-Process -Force
    Start-Sleep -Milliseconds 300

    New-Item -ItemType Directory -Path $InstallDir -Force | Out-Null
    Copy-Item (Join-Path $extractDir "WindowsIOSUniversalClipboard.exe") $ExePath -Force
    Unblock-File $ExePath -ErrorAction SilentlyContinue

    Set-ItemProperty -Path $RunKey -Name $RunName -Value "`"$ExePath`""

    Get-NetFirewallRule -DisplayName "Windows iOS Universal Clipboard*" -ErrorAction SilentlyContinue |
        Remove-NetFirewallRule -ErrorAction SilentlyContinue

    New-NetFirewallRule `
        -DisplayName "Windows iOS Universal Clipboard TCP" `
        -Direction Inbound `
        -Action Allow `
        -Program $ExePath `
        -Protocol TCP `
        -LocalPort $Port `
        -Profile Any `
        -RemoteAddress LocalSubnet | Out-Null

    $bonjourExe = "C:\Program Files\Bonjour\mDNSResponder.exe"
    if (Test-Path $bonjourExe) {
        New-NetFirewallRule `
            -DisplayName "Windows iOS Universal Clipboard Bonjour mDNS" `
            -Direction Inbound `
            -Action Allow `
            -Program $bonjourExe `
            -Protocol UDP `
            -LocalPort 5353 `
            -Profile Any `
            -RemoteAddress LocalSubnet | Out-Null
    }

    Start-Process $ExePath

    $health = $null
    for ($i = 0; $i -lt 60; $i++) {
        Start-Sleep -Milliseconds 250
        try {
            $health = Invoke-RestMethod "http://127.0.0.1:$Port/health" -TimeoutSec 1
            if ($health.ok -and $health.discovery -eq "running") { break }
        } catch {}
    }

    if (-not $health -or -not $health.ok -or $health.discovery -ne "running") {
        throw "The app was installed but CopyBridge local discovery did not start correctly."
    }

    Write-Host "      OK - app installed" -ForegroundColor Green
    Write-Host "      OK - copybridge.local ready" -ForegroundColor Green
    Write-Host "      OK - starts automatically with Windows" -ForegroundColor Green
    Write-Host ""

    Write-Host "[2/4] Install the iPhone Shortcut" -ForegroundColor Cyan
    Write-Host $ShortcutUrl -ForegroundColor White
    Write-Host ""

    if ([int]$health.approvedDevices -gt 0) {
        Write-Host "[3/4] Device already paired." -ForegroundColor Green
        Write-Host "[4/4] Setup complete." -ForegroundColor Green
        Write-Host ""
        Write-Host "Copy on iPhone -> run Shortcut -> Ctrl+V on Windows"
        exit
    }

    New-Item -ItemType File -Path $PairingPath -Force | Out-Null

    Write-Host "[3/4] Waiting for your iPhone..." -ForegroundColor Cyan
    Write-Host "On the iPhone:"
    Write-Host "  1. Add the Shortcut from the link above."
    Write-Host "  2. Copy any text."
    Write-Host "  3. Run the Shortcut once."
    Write-Host ""
    Write-Host "The first local device that contacts CopyBridge during this setup window will be paired automatically."
    Write-Host ""

    $paired = $false
    $pairedIp = $null
    $deadline = (Get-Date).AddMinutes(5)

    while ((Get-Date) -lt $deadline) {
        Start-Sleep -Seconds 1
        try {
            $health = Invoke-RestMethod "http://127.0.0.1:$Port/health" -TimeoutSec 1
            if ([int]$health.approvedDevices -gt 0 -and $health.lastPairedDevice) {
                $paired = $true
                $pairedIp = [string]$health.lastPairedDevice
                break
            }
        } catch {}

        Write-Host "." -NoNewline
    }

    Write-Host ""
    Remove-Item $PairingPath -Force -ErrorAction SilentlyContinue

    if ($paired) {
        Write-Host "      OK - iPhone detected: $pairedIp" -ForegroundColor Green
        Write-Host "      OK - device paired" -ForegroundColor Green
        Write-Host ""
        Write-Host "[4/4] Setup complete." -ForegroundColor Green
        Write-Host ""
        Write-Host "Copy on iPhone -> run Shortcut -> Ctrl+V on Windows"
    } else {
        Write-Warning "Pairing timed out after 5 minutes. The app is installed and will start with Windows."
        Write-Host "Run this installer again whenever you are ready to pair the iPhone:"
        Write-Host "irm https://raw.githubusercontent.com/$Repo/main/install.ps1 | iex"
    }
}
finally {
    Remove-Item $PairingPath -Force -ErrorAction SilentlyContinue
    Remove-Item $tempRoot -Recurse -Force -ErrorAction SilentlyContinue
}
