# Avvia KBM.Api o segnala se la porta è già occupata
param(
    [int]$Port = 5262,
    [switch]$Stop
)

$project = Join-Path $PSScriptRoot "..\src\KBM.Api\KBM.Api.csproj"
$conn = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue | Select-Object -First 1

if ($conn) {
    $pid = $conn.OwningProcess
    if ($Stop) {
        Write-Host "Arresto processo $pid sulla porta $Port..."
        Stop-Process -Id $pid -Force
        Start-Sleep -Seconds 1
    }
    else {
        Write-Host "Porta $Port già in uso (PID $pid). L'API è probabilmente già avviata."
        Write-Host "Usa: .\scripts\dev-api.ps1 -Stop  per terminare il processo."
        exit 0
    }
}

Write-Host "Avvio API su http://127.0.0.1:$Port ..."
dotnet run --project $project
