param(
    [string]$Configuration = 'Release',
    [string]$Runtime = 'win-x64',
    [string]$Version = '2.0.0'
)

$ErrorActionPreference = 'Stop'

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Resolve-Path (Join-Path $scriptDir '..')
$publishDir = Join-Path $repoRoot "artifacts\publish\$Runtime"
$publishDocsDir = Join-Path $publishDir 'docs'
$projectPath = Join-Path $repoRoot 'src\GestionDeFardos.App\GestionDeFardos.App.csproj'
$configTemplatePath = Join-Path $repoRoot 'samples\config.example.json'
$docsSourceDir = Join-Path $repoRoot 'docs'

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw "[ERROR] No se encontro 'dotnet' en el PATH."
}

if (Test-Path $publishDir) {
    Remove-Item $publishDir -Recurse -Force
}

New-Item -ItemType Directory -Force -Path $publishDocsDir | Out-Null

Push-Location $repoRoot
try {
    Write-Host "[INFO] Publicando $projectPath en $publishDir"
    dotnet publish $projectPath `
        -c $Configuration `
        -r $Runtime `
        --self-contained true `
        -p:PublishSingleFile=false `
        -p:PublishTrimmed=false `
        -p:Version=$Version `
        -o $publishDir
}
finally {
    Pop-Location
}

Copy-Item $configTemplatePath (Join-Path $publishDir 'config.example.json') -Force
Copy-Item $configTemplatePath (Join-Path $publishDir 'config.template.json') -Force
Copy-Item (Join-Path $docsSourceDir '*.md') $publishDocsDir -Force

Write-Host "[INFO] Publicacion lista en: $publishDir"
