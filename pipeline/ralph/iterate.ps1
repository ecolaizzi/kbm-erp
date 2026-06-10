# Avvia ciclo iterativo Ralph — resume pipeline + heartbeat background
# Usage: .\pipeline\ralph\iterate.ps1 [-Batch F] [-Message "..."]
param(
    [string]$Batch = "F",
    [string]$Message = "Week 2 - sviluppo attivo"
)

$root = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$cli = Join-Path $root "pipeline/scripts/pipeline-cli.ps1"
$heartbeat = Join-Path $root "pipeline/ralph/heartbeat.ps1"
$pidFile = Join-Path $PSScriptRoot "heartbeat.pid"

Set-Location $root

& $cli resume $Message
& $cli batch $Batch in_progress
& $cli event supervisor "iterate.ps1 avviato - Batch $Batch"

# Heartbeat in background (un solo processo)
$startHeartbeat = $true
if (Test-Path $pidFile) {
    $oldPid = Get-Content $pidFile -ErrorAction SilentlyContinue
    if ($oldPid -and (Get-Process -Id $oldPid -ErrorAction SilentlyContinue)) {
        $startHeartbeat = $false
        Write-Host "Heartbeat gia attivo (PID $oldPid)"
    }
}

if ($startHeartbeat) {
    $proc = Start-Process powershell -ArgumentList @(
        "-NoProfile", "-ExecutionPolicy", "Bypass", "-WindowStyle", "Hidden",
        "-File", $heartbeat
    ) -WorkingDirectory $root -PassThru
    $proc.Id | Out-File $pidFile -Encoding ascii -NoNewline
    Write-Host "Heartbeat avviato (PID $($proc.Id), ogni 60s)"
}

Write-Host "Pipeline: running | Batch: $Batch"
Write-Host "Dashboard: http://localhost:8765/pipeline/ralph/dashboard.html"
Write-Host "Server:    .\pipeline\ralph\serve.ps1"
