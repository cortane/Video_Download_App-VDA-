
# Script to fix git repository structure

$StartDir = "c:\Users\developer\WorkSpace\開発プロジェクト用フォルダ\動画ダウンロードアプリ\Video_Download_App(VDA)"
Set-Location $StartDir

# 1. Remove nested .git directory if it exists
$NestedGit = "src\vda_core\.git"
if (Test-Path $NestedGit) {
    Write-Host "Removing nested git repository at $NestedGit..."
    Remove-Item -Path $NestedGit -Recurse -Force
}

# 2. Initialize git at the root
if (-not (Test-Path ".git")) {
    Write-Host "Initializing git repository at root..."
    git init
} else {
    Write-Host "Git repository already exists at root."
}

# 3. Create .gitignore
$GitIgnoreContent = @"
# Rust
target/
**/*.rs.bk

# Visual Studio / .NET
bin/
obj/
.vs/
*.user
*.suo
*.pdb
*.cache
project.assets.json
*.nuget.g.props
*.nuget.g.targets
"@

if (-not (Test-Path ".gitignore")) {
    Set-Content -Path ".gitignore" -Value $GitIgnoreContent -Encoding UTF8
    Write-Host "Created .gitignore"
}

# 4. Add and Commit
Write-Host "Adding files..."
git add .
Write-Host "Committing..."
git commit -m "Initial setup: Project initialization"

Write-Host "Git setup complete!"
