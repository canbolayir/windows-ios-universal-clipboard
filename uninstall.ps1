$ErrorActionPreference = "Stop"

$InstallDir = Join-Path $env:LOCALAPPDATA "WindowsIOSUniversalClipboard"
$RunKey = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run"
$RunName = "WindowsIOSUniversalClipboard"

function Test-IsAdmin {
    $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($identity)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

if (-not (Test-IsAdmin)) {
    $tempScript = Join-Path $env:TEMP "windows-ios-universal-clipboard-uninstall.ps1"
    Invoke-WebRequest "https://raw.githubusercontent.com/canbolayir/windows-ios-universal-clipboard/main/uninstall.ps1" -OutFile $tempScript -UseBasicParsing
    Start-Process powershell.exe -Verb RunAs -Wait -ArgumentList @(
        "-NoProfile",
        "-ExecutionPolicy", "Bypass",
        "-File", "`"$tempScript`""
    )
    exit
}

Get-Process WindowsIOSUniversalClipboard -ErrorAction SilentlyContinue | Stop-Process -Force
Remove-ItemProperty -Path $RunKey -Name $RunName -ErrorAction SilentlyContinue

Get-NetFirewallRule -DisplayName "Windows iOS Universal Clipboard*" -ErrorAction SilentlyContinue |
    Remove-NetFirewallRule -ErrorAction SilentlyContinue

Remove-Item $InstallDir -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "Windows iOS Universal Clipboard has been removed." -ForegroundColor Green
