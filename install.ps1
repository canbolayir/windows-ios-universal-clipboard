# Windows iOS Universal Clipboard installer
# https://github.com/canbolayir/windows-ios-universal-clipboard

$ErrorActionPreference = "Stop"

$Repo = "canbolayir/windows-ios-universal-clipboard"
$TaskName = "Windows iOS Universal Clipboard"
$FirewallName = "Windows iOS Universal Clipboard"
$InstallDir = Join-Path $env:LOCALAPPDATA "WindowsIOSUniversalClipboard"
$ExePath = Join-Path $InstallDir "WindowsIOSUniversalClipboard.exe"
$Port = 8765
$RawInstaller = "https://raw.githubusercontent.com/$Repo/main/install.ps1"

function Test-Administrator {
    $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = [Security.Principal.WindowsPrincipal]::new($identity)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

if (-not (Test-Administrator)) {
    Write-Host "Administrator permission is needed once for the Private-network firewall rule and Bonjour." -ForegroundColor Yellow
    $command = "irm '$RawInstaller' | iex"
    Start-Process powershell.exe -Verb RunAs -ArgumentList "-NoProfile","-ExecutionPolicy","Bypass","-Command",$command
    exit
}

Write-Host ""
Write-Host "Windows iOS Universal Clipboard" -ForegroundColor Cyan
Write-Host "Installing..." -ForegroundColor Gray

$arch = if ([Environment]::Is64BitOperatingSystem -and $env:PROCESSOR_ARCHITECTURE -eq "ARM64") { "win-arm64" } else { "win-x64" }

# Bonjour is required for the universal copybridge.local hostname.
if (-not (Get-Service "Bonjour Service" -ErrorAction SilentlyContinue) -and
    -not (Get-Service "mDNSResponder" -ErrorAction SilentlyContinue)) {

    if (-not (Get-Command winget.exe -ErrorAction SilentlyContinue)) {
        throw "Windows Package Manager (winget) is required to install Apple Bonjour automatically."
    }

    Write-Host "Installing Apple Bonjour for copybridge.local..." -ForegroundColor Gray
    & winget.exe install --id Apple.Bonjour --exact --silent --accept-package-agreements --accept-source-agreements | Out-Host

    Start-Sleep -Seconds 2

    if (-not (Get-Service "Bonjour Service" -ErrorAction SilentlyContinue) -and
        -not (Get-Service "mDNSResponder" -ErrorAction SilentlyContinue)) {
        throw "Bonjour installation did not complete successfully."
    }
}

$release = Invoke-RestMethod -Uri "https://api.github.com/repos/$Repo/releases/latest" -Headers @{ "User-Agent" = "WindowsIOSUniversalClipboard-Installer" }
$assetName = "windows-ios-universal-clipboard-$arch.zip"
$asset = $release.assets | Where-Object { $_.name -eq $assetName } | Select-Object -First 1
if (-not $asset) {
    throw "Release asset '$assetName' was not found."
}

$tempRoot = Join-Path $env:TEMP ("windows-ios-universal-clipboard-" + [guid]::NewGuid())
$zipPath = Join-Path $tempRoot $assetName
New-Item -ItemType Directory -Path $tempRoot -Force | Out-Null

try {
    Invoke-WebRequest -Uri $asset.browser_download_url -OutFile $zipPath -UseBasicParsing

    Stop-ScheduledTask -TaskName $TaskName -ErrorAction SilentlyContinue
    Get-Process "WindowsIOSUniversalClipboard" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue

    if (Test-Path $InstallDir) {
        Get-ChildItem $InstallDir -Force |
            Where-Object { $_.Name -notin @("config.json","bridge.log") } |
            Remove-Item -Recurse -Force
    } else {
        New-Item -ItemType Directory -Path $InstallDir -Force | Out-Null
    }

    Expand-Archive -Path $zipPath -DestinationPath $InstallDir -Force

    Get-NetFirewallRule -DisplayName $FirewallName -ErrorAction SilentlyContinue | Remove-NetFirewallRule -ErrorAction SilentlyContinue
    New-NetFirewallRule `
        -DisplayName $FirewallName `
        -Direction Inbound `
        -Action Allow `
        -Profile Private `
        -Protocol TCP `
        -LocalPort $Port `
        -Program $ExePath | Out-Null

    Unregister-ScheduledTask -TaskName $TaskName -Confirm:$false -ErrorAction SilentlyContinue

    $action = New-ScheduledTaskAction -Execute $ExePath -WorkingDirectory $InstallDir
    $trigger = New-ScheduledTaskTrigger -AtLogOn -User "$env:USERDOMAIN\$env:USERNAME"
    $principal = New-ScheduledTaskPrincipal -UserId "$env:USERDOMAIN\$env:USERNAME" -LogonType Interactive -RunLevel Limited
    $settings = New-ScheduledTaskSettingsSet `
        -StartWhenAvailable `
        -AllowStartIfOnBatteries `
        -DontStopIfGoingOnBatteries `
        -MultipleInstances IgnoreNew `
        -RestartCount 3 `
        -RestartInterval (New-TimeSpan -Minutes 1) `
        -ExecutionTimeLimit ([TimeSpan]::Zero)

    Register-ScheduledTask `
        -TaskName $TaskName `
        -Action $action `
        -Trigger $trigger `
        -Principal $principal `
        -Settings $settings | Out-Null

    Start-ScheduledTask -TaskName $TaskName

    $ready = $false
    for ($i = 0; $i -lt 30; $i++) {
        Start-Sleep -Milliseconds 250
        try {
            $health = Invoke-RestMethod -Uri "http://127.0.0.1:$Port/health" -TimeoutSec 1
            if ($health.ok) { $ready = $true; break }
        } catch {}
    }

    if (-not $ready) {
        throw "The bridge did not start."
    }

    Write-Host ""
    Write-Host "Installed successfully." -ForegroundColor Green
    Write-Host "iPhone Shortcut URL: http://copybridge.local:8765/copy" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "On the first copy, Windows will ask you to approve the iPhone once." -ForegroundColor Gray
}
finally {
    Remove-Item $tempRoot -Recurse -Force -ErrorAction SilentlyContinue
}
