$ErrorActionPreference = "Stop"

$base = "C:\TGS-UnityAssets-2026"

# 外部アセットフォルダ作成
New-Item -ItemType Directory -Force -Path "$base\Textures" | Out-Null
New-Item -ItemType Directory -Force -Path "$base\Models"   | Out-Null
New-Item -ItemType Directory -Force -Path "$base\Audio"    | Out-Null

# 既存リンク/フォルダ削除
if (Test-Path "Assets\Textures") {
    Remove-Item "Assets\Textures" -Force -Recurse
}

if (Test-Path "Assets\Models") {
    Remove-Item "Assets\Models" -Force -Recurse
}

if (Test-Path "Assets\Audio") {
    Remove-Item "Assets\Audio" -Force -Recurse
}

# シンボリックリンク作成
New-Item `
    -ItemType SymbolicLink `
    -Path "Assets\Textures" `
    -Target "$base\Textures" | Out-Null

New-Item `
    -ItemType SymbolicLink `
    -Path "Assets\Models" `
    -Target "$base\Models" | Out-Null

New-Item `
    -ItemType SymbolicLink `
    -Path "Assets\Audio" `
    -Target "$base\Audio" | Out-Null

Write-Host ""
Write-Host "================================="
Write-Host " Unity asset symlinks created"
Write-Host "================================="
Write-Host ""

Get-ChildItem Assets