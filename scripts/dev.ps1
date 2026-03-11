$ErrorActionPreference = 'Stop'

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Resolve-Path (Join-Path $scriptDir '..')

Write-Host "[INFO] Repositorio detectado en: $repoRoot"

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw "[ERROR] No se encontró 'dotnet' en el PATH. Instale .NET SDK 8.x e inténtelo nuevamente."
}

$sdkVersion = (dotnet --version).Trim()
if (-not $sdkVersion.StartsWith('8.')) {
    throw "[ERROR] SDK incompatible detectado ($sdkVersion). Se requiere .NET SDK 8.x."
}

Push-Location $repoRoot
try {
    Write-Host "[INFO] Ejecutando: dotnet --info"
    dotnet --info

    Write-Host "[INFO] Ejecutando: dotnet restore GestionDeFardos.sln"
    dotnet restore GestionDeFardos.sln

    Write-Host "[INFO] Ejecutando: dotnet build GestionDeFardos.sln -c Debug"
    dotnet build GestionDeFardos.sln -c Debug

    Write-Host "[INFO] Ejecutando: dotnet run --project src/GestionDeFardos.App/GestionDeFardos.App.csproj -c Debug"
    dotnet run --project src/GestionDeFardos.App/GestionDeFardos.App.csproj -c Debug
}
finally {
    Pop-Location
}
