$ErrorActionPreference = "Stop"

$TaskName = "Windows iOS Universal Clipboard"
$FirewallName = "Windows iOS Universal Clipboard"
$InstallDir = Join-Path $env:LOCALAPPDATA "WindowsIOSUniversalClipboard"

function Test-Administrator {
    $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = [Security.Principal.WindowsPrincipal]::new($identity)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

if (-not (Test-Administrator)) {
    Start-Process powershell.exe -Verb RunAs -ArgumentList "-NoProfile","-ExecutionPolicy","Bypass","-File","`"$PSCommandPath`""
    exit
}

Stop-ScheduledTask -TaskName $TaskName -ErrorAction SilentlyContinue
Unregister-ScheduledTask -TaskName $TaskName -Confirm:$false -ErrorAction SilentlyContinue
Get-Process "WindowsIOSUniversalClipboard" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Get-NetFirewallRule -DisplayName $FirewallName -ErrorAction SilentlyContinue | Remove-NetFirewallRule -ErrorAction SilentlyContinue
Remove-Item $InstallDir -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "Windows iOS Universal Clipboard has been removed." -ForegroundColor Green
Write-Host "Apple Bonjour was left installed because other apps may use it." -ForegroundColor Gray
