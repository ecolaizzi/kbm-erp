# Avvia dashboard Ralph — PowerShell HttpListener (default) o Python fallback
param([int]$Port = 8765)

$root = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
Set-Location $root

function Test-PortListening([int]$p) {
    $conn = Get-NetTCPConnection -LocalPort $p -State Listen -ErrorAction SilentlyContinue | Select-Object -First 1
    return $null -ne $conn
}

function Start-PsServer([int]$p) {
    Write-Host "Server: PowerShell HttpListener (porta $p)"
    Write-Host "Ctrl+C per fermare"

    $listener = New-Object System.Net.HttpListener
    $listener.Prefixes.Add("http://localhost:$p/")
    $listener.Start()

    $mime = @{
        '.html' = 'text/html; charset=utf-8'
        '.json' = 'application/json; charset=utf-8'
        '.md'   = 'text/plain; charset=utf-8'
        '.css'  = 'text/css'
        '.js'   = 'application/javascript'
    }

    try {
        while ($listener.IsListening) {
            $ctx = $listener.GetContext()
            $path = $ctx.Request.Url.LocalPath.TrimStart('/')
            if ([string]::IsNullOrEmpty($path)) { $path = 'pipeline/ralph/dashboard.html' }
            $file = Join-Path $root ($path -replace '/', '\')

            if ($path -like '*state.json') {
                $ctx.Response.Headers.Add('Cache-Control', 'no-store, no-cache, must-revalidate')
                $ctx.Response.Headers.Add('Pragma', 'no-cache')
            }

            if (Test-Path $file -PathType Leaf) {
                $ext = [IO.Path]::GetExtension($file).ToLower()
                $ctx.Response.ContentType = $mime[$ext]
                if (-not $ctx.Response.ContentType) { $ctx.Response.ContentType = 'application/octet-stream' }
                $bytes = [IO.File]::ReadAllBytes($file)
                $ctx.Response.OutputStream.Write($bytes, 0, $bytes.Length)
            } else {
                $ctx.Response.StatusCode = 404
                $msg = [Text.Encoding]::UTF8.GetBytes("Not found: $path")
                $ctx.Response.OutputStream.Write($msg, 0, $msg.Length)
            }
            $ctx.Response.Close()
        }
    } finally {
        $listener.Stop()
    }
}

if (Test-PortListening $Port) {
    $url = "http://localhost:$Port/pipeline/ralph/dashboard.html"
    try {
        $r = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 3
        if ($r.StatusCode -eq 200) {
            Write-Host "Dashboard gia attiva: $url"
            exit 0
        }
    } catch { }
    Write-Host "Porta $Port occupata. Prova porta alternativa..."
    $Port = 8766
}

Write-Host "KBM Ralph Dashboard: http://localhost:$Port/pipeline/ralph/dashboard.html"
Write-Host "Root: $root"

try {
    Start-PsServer $Port
} catch {
    Write-Host "PowerShell server fallito: $_"
    $python = Get-Command python -ErrorAction SilentlyContinue
    if ($python) {
        Write-Host "Fallback: Python http.server (porta $Port)"
        python -m http.server $Port
        exit $LASTEXITCODE
    }
    throw
}
