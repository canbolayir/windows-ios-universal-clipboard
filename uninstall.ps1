$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

$Repo = "canbolayir/windows-ios-universal-clipboard"
$InstallDir = Join-Path $env:LOCALAPPDATA "WindowsIOSUniversalClipboard"
$LegacyInstallDir = Join-Path $env:LOCALAPPDATA "iPhoneClipboardBridge"
$RunKey = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run"

function Test-IsAdmin {
    $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($identity)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

if (-not (Test-IsAdmin)) {
    Write-Host "Administrator permission is required once to remove firewall rules." -ForegroundColor Yellow
    $tempScript = Join-Path $env:TEMP "windows-ios-universal-clipboard-uninstall.ps1"
    Invoke-WebRequest "https://raw.githubusercontent.com/$Repo/main/uninstall.ps1" -OutFile $tempScript -UseBasicParsing
    Start-Process powershell.exe -Verb RunAs -Wait -ArgumentList @(
        "-NoProfile",
        "-ExecutionPolicy", "Bypass",
        "-File", "`"$tempScript`""
    )
    exit
}

Write-Host "Removing Windows iOS Universal Clipboard..." -ForegroundColor Cyan

Get-Process WindowsIOSUniversalClipboard,iPhoneClipboardBridge -ErrorAction SilentlyContinue |
    Stop-Process -Force -ErrorAction SilentlyContinue

Remove-ItemProperty -Path $RunKey -Name "WindowsIOSUniversalClipboard" -ErrorAction SilentlyContinue
Remove-ItemProperty -Path $RunKey -Name "iPhoneClipboardBridge" -ErrorAction SilentlyContinue

Unregister-ScheduledTask -TaskName "Windows iOS Universal Clipboard" -Confirm:$false -ErrorAction SilentlyContinue
Unregister-ScheduledTask -TaskName "iPhone Clipboard Bridge" -Confirm:$false -ErrorAction SilentlyContinue

Get-NetFirewallRule -ErrorAction SilentlyContinue |
    Where-Object {
        $_.DisplayName -like "Windows iOS Universal Clipboard*" -or
        $_.DisplayName -eq "iPhone Clipboard Bridge"
    } |
    Remove-NetFirewallRule -ErrorAction SilentlyContinue

$startup = [Environment]::GetFolderPath("Startup")
Get-ChildItem $startup -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -match "WindowsIOSUniversalClipboard|iPhoneClipboardBridge|CopyBridge" } |
    Remove-Item -Recurse -Force -ErrorAction SilentlyContinue

Remove-Item $InstallDir -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item $LegacyInstallDir -Recurse -Force -ErrorAction SilentlyContinue

Remove-Item (Join-Path $env:TEMP "windows-ios-universal-clipboard-install.ps1") -Force -ErrorAction SilentlyContinue
Remove-Item (Join-Path $env:TEMP "windows-ios-universal-clipboard-uninstall.ps1") -Force -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "Removed completely." -ForegroundColor Green
Write-Host "  - app files removed"
Write-Host "  - paired devices/config/logs removed"
Write-Host "  - automatic startup removed"
Write-Host "  - firewall rules removed"
Write-Host "  - legacy bridge files/tasks removed"
Write-Host ""
Write-Host "Apple Bonjour was left installed because other Apple software may use it."
Write-Host "To remove Bonjour too: winget uninstall --id Apple.Bonjour"
