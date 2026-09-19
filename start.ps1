$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

$ExePath = Join-Path $env:LOCALAPPDATA "WindowsIOSUniversalClipboard\WindowsIOSUniversalClipboard.exe"

if (-not (Test-Path $ExePath)) {
    throw "Windows iOS Universal Clipboard is not installed. Run install.ps1 first."
}

$running = Get-Process WindowsIOSUniversalClipboard -ErrorAction SilentlyContinue

if ($running) {
    Write-Host "Windows iOS Universal Clipboard is already running." -ForegroundColor Green
}
else {
    Start-Process $ExePath

    $ready = $false
    for ($i = 0; $i -lt 30; $i++) {
        Start-Sleep -Milliseconds 250
        try {
            $health = Invoke-RestMethod "http://127.0.0.1:8765/health" -TimeoutSec 1
            if ($health.ok) {
                $ready = $true
                break
            }
        }
        catch {}
    }

    if (-not $ready) {
        throw "The app was started but did not become ready."
    }

    Write-Host "Windows iOS Universal Clipboard started." -ForegroundColor Green
}
