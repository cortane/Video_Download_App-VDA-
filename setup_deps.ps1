# Setup Script
# Download yt-dlp and ffmpeg

$ErrorActionPreference = "Stop"

$TargetDir = Join-Path $PSScriptRoot "src\VDA.System\Resources"

if (-not (Test-Path $TargetDir)) {
    New-Item -ItemType Directory -Path $TargetDir -Force
}

Write-Host "Target: $TargetDir"

# 1. yt-dlp.exe
$YtDlpUrl = "https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe"
$YtDlpPath = Join-Path $TargetDir "yt-dlp.exe"

Write-Host "Downloading yt-dlp.exe..."
Invoke-WebRequest -Uri $YtDlpUrl -OutFile $YtDlpPath
Write-Host "yt-dlp.exe downloaded." -ForegroundColor Green

# 2. ffmpeg
$FfmpegUrl = "https://github.com/yt-dlp/FFmpeg-Builds/releases/latest/download/ffmpeg-master-latest-win64-gpl.zip"
$ZipPath = Join-Path $PSScriptRoot "ffmpeg.zip"
$ExtractPath = Join-Path $PSScriptRoot "ffmpeg_temp"

Write-Host "Downloading ffmpeg..."
Invoke-WebRequest -Uri $FfmpegUrl -OutFile $ZipPath

Write-Host "Extracting ffmpeg..."
if (Test-Path $ExtractPath) { Remove-Item -Recurse -Force $ExtractPath }
Expand-Archive -Path $ZipPath -DestinationPath $ExtractPath

$BinFolder = Get-ChildItem -Path $ExtractPath -Recurse -Filter "bin" | Select-Object -First 1
if ($BinFolder) {
    Copy-Item -Path (Join-Path $BinFolder.FullName "ffmpeg.exe") -Destination $TargetDir
    Copy-Item -Path (Join-Path $BinFolder.FullName "ffprobe.exe") -Destination $TargetDir
    Write-Host "ffmpeg.exe, ffprobe.exe downloaded." -ForegroundColor Green
} else {
    Write-Host "ffmpeg binary not found." -ForegroundColor Red
}

Remove-Item -Force $ZipPath
Remove-Item -Recurse -Force $ExtractPath

# 3. QuickJS (qjs.exe)
$QjsUrl = "https://github.com/quickjs-ng/quickjs/releases/download/v0.11.0/qjs-windows-x86_64.exe"
$QjsPath = Join-Path $TargetDir "qjs.exe"

Write-Host "Downloading QuickJS (qjs.exe)..."
Invoke-WebRequest -Uri $QjsUrl -OutFile $QjsPath
Write-Host "qjs.exe downloaded." -ForegroundColor Green

Write-Host "Setup Completed." -ForegroundColor Cyan