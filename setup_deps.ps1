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

# Write-Host "Downloading ffmpeg..."
# Invoke-WebRequest -Uri $FfmpegUrl -OutFile $ZipPath

# Write-Host "Extracting ffmpeg..."
# if (Test-Path $ExtractPath) { Remove-Item -Recurse -Force $ExtractPath }
# Expand-Archive -Path $ZipPath -DestinationPath $ExtractPath

# $BinFolder = Get-ChildItem -Path $ExtractPath -Recurse -Filter "bin" | Select-Object -First 1
# if ($BinFolder) {
#    Copy-Item -Path (Join-Path $BinFolder.FullName "ffmpeg.exe") -Destination $TargetDir
#    Copy-Item -Path (Join-Path $BinFolder.FullName "ffprobe.exe") -Destination $TargetDir
#    Write-Host "ffmpeg.exe, ffprobe.exe downloaded." -ForegroundColor Green
# } else {
#    Write-Host "ffmpeg binary not found." -ForegroundColor Red
# }

# Remove-Item -Force $ZipPath
# Remove-Item -Recurse -Force $ExtractPath


# 3. Deno (Preferred by yt-dlp)
# Node.js was marked as unsupported by yt-dlp.
# QuickJS-NG was slow.
# Using Deno which is the default enabled runtime.
$DenoUrl = "https://github.com/denoland/deno/releases/latest/download/deno-x86_64-pc-windows-msvc.zip"

$DenoZipPath = Join-Path $PSScriptRoot "deno.zip"
$DenoExtractPath = Join-Path $PSScriptRoot "deno_temp"
$DenoDest = Join-Path $TargetDir "deno.exe"

Write-Host "Downloading Deno..."
Invoke-WebRequest -Uri $DenoUrl -OutFile $DenoZipPath

Write-Host "Extracting Deno..."
if (Test-Path $DenoExtractPath) { Remove-Item -Recurse -Force $DenoExtractPath }
Expand-Archive -Path $DenoZipPath -DestinationPath $DenoExtractPath

$SourceDeno = Join-Path $DenoExtractPath "deno.exe"
if (Test-Path $SourceDeno) {
    Copy-Item -Path $SourceDeno -Destination $DenoDest
    Write-Host "deno.exe downloaded." -ForegroundColor Green
} else {
    Write-Host "deno.exe not found in zip!" -ForegroundColor Red
}

# Cleanup
Remove-Item -Force $DenoZipPath
Remove-Item -Recurse -Force $DenoExtractPath

# Cleanup old binaries
$OldNode = Join-Path $TargetDir "node.exe"
if (Test-Path $OldNode) { Remove-Item -Force $OldNode }

Write-Host "Setup Completed." -ForegroundColor Cyan