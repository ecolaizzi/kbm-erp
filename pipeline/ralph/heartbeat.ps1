# Heartbeat Ralph — aggiorna state.json ogni 60s (mantiene dashboard "viva")
# Usage: .\pipeline\ralph\heartbeat.ps1
param([int]$IntervalSeconds = 60)

$cli = Join-Path $PSScriptRoot "..\scripts\pipeline-cli.ps1"
Write-Host "KBM Ralph heartbeat ogni ${IntervalSeconds}s (Ctrl+C per fermare)"

while ($true) {
    try {
        & $cli heartbeat
        Write-Host "[$(Get-Date -Format 'HH:mm:ss')] heartbeat OK"
    } catch {
        Write-Host "[$(Get-Date -Format 'HH:mm:ss')] heartbeat FAILED: $_"
    }
    Start-Sleep -Seconds $IntervalSeconds
}
