$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

$Repo = "canbolayir/windows-ios-universal-clipboard"
$InstallDir = Join-Path $env:LOCALAPPDATA "WindowsIOSUniversalClipboard"
$ExePath = Join-Path $InstallDir "WindowsIOSUniversalClipboard.exe"
$RunKey = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run"
$RunName = "WindowsIOSUniversalClipboard"
$BonjourFallbackUrl = "https://swcdn.apple.com/content/downloads/52/06/071-03198/djcqm50b49h4o03eetqwowrdpf4o9sx71z/Bonjour64.msi"

function Test-IsAdmin {
    $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($identity)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

if (-not (Test-IsAdmin)) {
    $tempScript = Join-Path $env:TEMP "windows-ios-universal-clipboard-install.ps1"
    Invoke-WebRequest "https://raw.githubusercontent.com/$Repo/main/install.ps1" -OutFile $tempScript -UseBasicParsing
    Start-Process powershell.exe -Verb RunAs -Wait -ArgumentList @(
        "-NoProfile",
        "-ExecutionPolicy", "Bypass",
        "-File", "`"$tempScript`""
    )
    exit
}

Write-Host ""
Write-Host "Windows iOS Universal Clipboard" -ForegroundColor Cyan
Write-Host "Installing..." -ForegroundColor Gray
Write-Host ""

if (-not [Environment]::Is64BitOperatingSystem) {
    throw "This release currently supports x64 Windows only."
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

    Write-Host "Installing Apple Bonjour for local discovery..."

    $installed = $false
    $winget = Get-Command winget.exe -ErrorAction SilentlyContinue

    if ($winget) {
        $wingetExe = if ($winget.Path) { $winget.Path } elseif ($winget.Source) { $winget.Source } else { "winget.exe" }
        & $wingetExe install `
            --id Apple.Bonjour `
            --exact `
            --accept-package-agreements `
            --accept-source-agreements `
            --silent

        if ($LASTEXITCODE -eq 0) {
            $installed = $true
        }
    }

    if (-not $installed) {
        $msi = Join-Path $env:TEMP "Bonjour64.msi"
        Invoke-WebRequest $BonjourFallbackUrl -OutFile $msi -UseBasicParsing
        $proc = Start-Process msiexec.exe -Wait -PassThru -ArgumentList @(
            "/i", "`"$msi`"", "/qn", "/norestart"
        )
        Remove-Item $msi -Force -ErrorAction SilentlyContinue

        if ($proc.ExitCode -notin 0, 3010) {
            throw "Bonjour installation failed with exit code $($proc.ExitCode)."
        }
    }

    $service = Get-Service "Bonjour Service" -ErrorAction SilentlyContinue
    if (-not $service) {
        throw "Bonjour was installed but its service was not found."
    }

    if ($service.Status -ne "Running") {
        Start-Service "Bonjour Service"
    }

    if (-not (Test-Path $dll)) {
        throw "Bonjour was installed but dnssd.dll was not found."
    }
}

Ensure-Bonjour

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
    Write-Host "Downloading $($release.tag_name)..."
    Invoke-WebRequest $asset.browser_download_url -OutFile $zipPath -UseBasicParsing
    Expand-Archive $zipPath -DestinationPath $extractDir -Force

    Get-Process WindowsIOSUniversalClipboard -ErrorAction SilentlyContinue | Stop-Process -Force
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
        -LocalPort 8765 `
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

    $ready = $false
    for ($i = 0; $i -lt 40; $i++) {
        Start-Sleep -Milliseconds 250
        try {
            $health = Invoke-RestMethod "http://127.0.0.1:8765/health" -TimeoutSec 1
            if ($health.ok -and $health.discovery -eq "running") {
                $ready = $true
                break
            }
        } catch {}
    }

    if (-not $ready) {
        throw "The app was installed but local discovery did not start correctly."
    }

    Write-Host ""
    Write-Host "Installed successfully." -ForegroundColor Green
    Write-Host ""
    Write-Host "No hostname or token is required." -ForegroundColor Cyan
    Write-Host "Universal Shortcut address: http://copybridge.local:8765/copy"
    Write-Host ""
    Write-Host "First use:"
    Write-Host "1. Add the shared iPhone Shortcut."
    Write-Host "2. Copy text and run it."
    Write-Host "3. Click Yes on the Windows approval prompt once."
    Write-Host "4. After that: Copy -> Back Tap -> Ctrl+V."
    Write-Host ""

    Start-Process "http://127.0.0.1:8765/setup"
}
finally {
    Remove-Item $tempRoot -Recurse -Force -ErrorAction SilentlyContinue
}
