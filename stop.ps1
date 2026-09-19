$ErrorActionPreference = "Stop"

$InstallDir = Join-Path $env:LOCALAPPDATA "WindowsIOSUniversalClipboard"
$PairingPath = Join-Path $InstallDir "pairing.enabled"

Remove-Item $PairingPath -Force -ErrorAction SilentlyContinue

$processes = Get-Process WindowsIOSUniversalClipboard -ErrorAction SilentlyContinue
if ($processes) {
    $processes | Stop-Process -Force
    Write-Host "Windows iOS Universal Clipboard stopped." -ForegroundColor Green
} else {
    Write-Host "Windows iOS Universal Clipboard is not running."
}

Write-Host "Automatic startup is still enabled. Use uninstall.ps1 to remove it completely."
