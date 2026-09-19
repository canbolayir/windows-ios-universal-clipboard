$ErrorActionPreference = "Stop"

$Repo = "canbolayir/windows-ios-universal-clipboard"
$InstallDir = Join-Path $env:LOCALAPPDATA "WindowsIOSUniversalClipboard"
$ExePath = Join-Path $InstallDir "WindowsIOSUniversalClipboard.exe"
$RunKey = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run"
$RunName = "WindowsIOSUniversalClipboard"

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

$headers = @{
    "User-Agent" = "windows-ios-universal-clipboard-installer"
    "Accept" = "application/vnd.github+json"
}

$release = Invoke-RestMethod "https://api.github.com/repos/$Repo/releases/latest" -Headers $headers

$arch = [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture.ToString().ToLowerInvariant()
switch ($arch) {
    "x64"   { $assetName = "windows-ios-universal-clipboard-win-x64.zip" }
    "arm64" { $assetName = "windows-ios-universal-clipboard-win-arm64.zip" }
    default { throw "Unsupported Windows architecture: $arch" }
}

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
        -Profile Private | Out-Null

    New-NetFirewallRule `
        -DisplayName "Windows iOS Universal Clipboard mDNS" `
        -Direction Inbound `
        -Action Allow `
        -Program $ExePath `
        -Protocol UDP `
        -LocalPort 5353 `
        -Profile Private | Out-Null

    Start-Process $ExePath

    $healthy = $false
    for ($i = 0; $i -lt 20; $i++) {
        Start-Sleep -Milliseconds 250
        try {
            $health = Invoke-RestMethod "http://127.0.0.1:8765/health" -TimeoutSec 1
            if ($health.ok) {
                $healthy = $true
                break
            }
        } catch {}
    }

    if (-not $healthy) {
        throw "The app was installed but did not start correctly."
    }

    Write-Host ""
    Write-Host "Installed successfully." -ForegroundColor Green
    Write-Host ""
    Write-Host "iPhone setup:" -ForegroundColor White
    Write-Host "1. Add the shared Shortcut."
    Write-Host "2. Copy text on iPhone and run it."
    Write-Host "3. Click Allow on Windows the first time."
    Write-Host "4. After that: Copy -> Back Tap -> Ctrl+V."
    Write-Host ""
    Write-Host "Universal address: http://copybridge.local:8765/copy" -ForegroundColor Cyan
    Write-Host ""

    Start-Process "http://127.0.0.1:8765/setup"
}
finally {
    Remove-Item $tempRoot -Recurse -Force -ErrorAction SilentlyContinue
}
