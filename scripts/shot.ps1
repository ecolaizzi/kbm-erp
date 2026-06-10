param(
    [string]$Out = "docs/ux/assets/shell-preview.png",
    [int]$WaitSec = 9,
    [string]$Extra = "",
    [string]$BaseArg = "--preview-shell"
)

$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent
Set-Location $root

# build
dotnet build src/KBM.Client/KBM.Client.csproj -nologo -clp:ErrorsOnly | Out-Null

$exe = Join-Path $root "src/KBM.Client/bin/Debug/net8.0-windows/KBM.Client.exe"
$argList = @($BaseArg)
if ($Extra) { $argList += $Extra }
$proc = Start-Process -FilePath $exe -ArgumentList $argList -PassThru
Start-Sleep -Seconds $WaitSec

Add-Type -AssemblyName System.Drawing
Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class Win {
    [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr h, IntPtr hdc, uint flags);
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out RECT r);
    [StructLayout(LayoutKind.Sequential)] public struct RECT { public int L, T, R, B; }
}
"@

# Trova l'handle della finestra principale dell'app
$hwnd = [IntPtr]::Zero
for ($i = 0; $i -lt 10; $i++) {
    $p = Get-Process -Id $proc.Id -ErrorAction SilentlyContinue
    if ($p -and $p.MainWindowHandle -ne [IntPtr]::Zero) { $hwnd = $p.MainWindowHandle; break }
    Start-Sleep -Milliseconds 600
}
if ($hwnd -eq [IntPtr]::Zero) { Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue; throw "Finestra non trovata" }

$rect = New-Object Win+RECT
[void][Win]::GetWindowRect($hwnd, [ref]$rect)
$w = $rect.R - $rect.L; $h = $rect.B - $rect.T
$bmp = New-Object System.Drawing.Bitmap $w, $h
$gfx = [System.Drawing.Graphics]::FromImage($bmp)
$hdc = $gfx.GetHdc()
# PW_RENDERFULLCONTENT = 2 (cattura contenuto WPF/DirectComposition)
[void][Win]::PrintWindow($hwnd, $hdc, 2)
$gfx.ReleaseHdc($hdc)

$outPath = Join-Path $root $Out
$dir = Split-Path $outPath -Parent
if (!(Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
$bmp.Save($outPath, [System.Drawing.Imaging.ImageFormat]::Png)
$gfx.Dispose(); $bmp.Dispose()

Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
Write-Output "saved $outPath"
