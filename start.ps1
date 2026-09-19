$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

$ExePath = Join-Path $env:LOCALAPPDATA "WindowsIOSUniversalClipboard\WindowsIOSUniversalClipboard.exe"

if (-not (Test-Path $ExePath)) {
    throw "Windows iOS Universal Clipboard is not installed. Run install.ps1 first."
}

if (Get-Process WindowsIOSUniversalClipboard -ErrorAction SilentlyContinue) {
    Write-Host "Windows iOS Universal Clipboard is already running." -ForegroundColor Green
    exit
}

Start-Process $ExePath

for ($i = 0; $i -lt 30; $i++) {
    Start-Sleep -Milliseconds 250
    try {
        $health = Invoke-RestMethod "http://127.0.0.1:8765/health" -TimeoutSec 1
        if ($health.ok) {
            Write-Host "Windows iOS Universal Clipboard started." -ForegroundColor Green
            exit
        }
    } catch {}
}

throw "The app was started but did not become ready."
