param(
    [string]$Configuration = 'Release',
    [string]$Runtime = 'win-x64',
    [string]$Version = '2.0.0'
)

$ErrorActionPreference = 'Stop'

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Resolve-Path (Join-Path $scriptDir '..')
$workspaceRoot = Resolve-Path (Join-Path $repoRoot '..')
$publishDir = Join-Path $repoRoot "artifacts\publish\$Runtime"
$outputDir = Join-Path $repoRoot 'artifacts\dist'
$nativeBuildRoot = Join-Path $workspaceRoot "installer-build-temp\$Runtime-$Version"
$nativeStagingDir = Join-Path $nativeBuildRoot 'staging'
$nativePayloadDir = Join-Path $nativeStagingDir 'payload'
$nativePayloadZip = Join-Path $nativeStagingDir 'payload.zip'
$nativeOutputDir = Join-Path $nativeBuildRoot 'dist'
$bootstrapperProjectDir = Join-Path $nativeBuildRoot 'bootstrapper'
$bootstrapperProgramFile = Join-Path $bootstrapperProjectDir 'Program.cs'
$bootstrapperPayloadFile = Join-Path $bootstrapperProjectDir 'payload.zip'
$bootstrapperPublishDir = Join-Path $nativeBuildRoot 'bootstrapper-publish'
$bootstrapperBuiltExe = Join-Path $bootstrapperPublishDir 'GestionDeFardos.Setup.exe'
$installerScript = Join-Path $repoRoot 'installer\GestionDeFardos.iss'
$outputExe = Join-Path $outputDir "GestionDeFardos-Setup-$Version-x64.exe"
$installGuideSource = Join-Path $repoRoot 'docs\instalacion_cliente.md'
$installGuideOutput = Join-Path $outputDir 'instalacion_cliente.md'

Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem

function Ensure-PublishOutput {
    if (-not (Test-Path $publishDir)) {
        & (Join-Path $scriptDir 'release.ps1') -Configuration $Configuration -Runtime $Runtime -Version $Version
    }
}

function Stage-PublishPayload {
    param(
        [string]$StagingDir,
        [string]$PayloadDir,
        [string]$PayloadZip,
        [string]$DestinationOutputDir
    )

    if (Test-Path $StagingDir) {
        Remove-Item $StagingDir -Recurse -Force
    }

    New-Item -ItemType Directory -Force -Path $PayloadDir | Out-Null
    New-Item -ItemType Directory -Force -Path $DestinationOutputDir | Out-Null

    $robocopyArgs = @(
        $publishDir,
        $PayloadDir,
        '/E',
        '/XD', 'logs',
        '/NFL',
        '/NDL',
        '/NJH',
        '/NJS',
        '/NC',
        '/NS',
        '/NP'
    )

    & robocopy @robocopyArgs | Out-Null
    if ($LASTEXITCODE -ge 8) {
        throw "[ERROR] Fallo la copia de artefactos hacia el staging del instalador."
    }

    if (Test-Path $PayloadZip) {
        Remove-Item $PayloadZip -Force
    }

    $zipCreated = $false
    $zipAttemptErrors = @()
    foreach ($attempt in 1..3) {
        try {
            [System.IO.Compression.ZipFile]::CreateFromDirectory(
                $PayloadDir,
                $PayloadZip,
                [System.IO.Compression.CompressionLevel]::Optimal,
                $false
            )
            $zipCreated = $true
            break
        }
        catch {
            $zipAttemptErrors += $_.Exception.Message
            if (Test-Path $PayloadZip) {
                Remove-Item $PayloadZip -Force -ErrorAction SilentlyContinue
            }

            if ($attempt -lt 3) {
                Start-Sleep -Seconds 1
            }
        }
    }

    if (-not $zipCreated) {
        $errorSummary = ($zipAttemptErrors -join ' | ')
        throw "[ERROR] Fallo la compresion del payload del instalador. $errorSummary"
    }
}

function Write-BootstrapperProject {
    if (Test-Path $bootstrapperProjectDir) {
        Remove-Item $bootstrapperProjectDir -Recurse -Force
    }

    New-Item -ItemType Directory -Force -Path $bootstrapperProjectDir | Out-Null
    New-Item -ItemType Directory -Force -Path $bootstrapperPublishDir | Out-Null
    Copy-Item $nativePayloadZip $bootstrapperPayloadFile -Force

    $programContent = @'
using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        bool quiet = false;
        string targetDir = @"C:\GestionDeFardos";

        foreach (string arg in args)
        {
            if (string.Equals(arg, "/quiet", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(arg, "--quiet", StringComparison.OrdinalIgnoreCase))
            {
                quiet = true;
                continue;
            }

            if (!string.IsNullOrWhiteSpace(arg))
            {
                targetDir = arg;
            }
        }

        string tempRoot = Path.Combine(
            Path.GetTempPath(),
            "GestionDeFardosInstaller_" + Guid.NewGuid().ToString("N"));

        string payloadZipPath = Path.Combine(tempRoot, "payload.zip");
        string extractedPayloadDir = Path.Combine(tempRoot, "payload");
        string preservedConfigPath = Path.Combine(tempRoot, "config.preserve.json");
        string logPath = Path.Combine(Path.GetTempPath(), "GestionDeFardos.Setup.log");

        try
        {
            WriteLog(logPath, "[INFO] Inicio de instalacion en " + targetDir);
            Directory.CreateDirectory(tempRoot);
            WritePayloadZip(payloadZipPath);
            WriteLog(logPath, "[INFO] Payload embebido copiado a " + payloadZipPath);
            ZipFile.ExtractToDirectory(payloadZipPath, extractedPayloadDir);
            WriteLog(logPath, "[INFO] Payload descomprimido en " + extractedPayloadDir);

            Directory.CreateDirectory(targetDir);
            WriteLog(logPath, "[INFO] Directorio de destino listo.");

            string configPath = Path.Combine(targetDir, "config.json");
            string templatePath = Path.Combine(targetDir, "config.template.json");
            if (File.Exists(configPath))
            {
                File.Copy(configPath, preservedConfigPath, overwrite: true);
                WriteLog(logPath, "[INFO] Configuracion existente preservada.");
            }

            CopyDirectory(extractedPayloadDir, targetDir);
            WriteLog(logPath, "[INFO] Archivos copiados al destino.");

            if (File.Exists(preservedConfigPath))
            {
                File.Copy(preservedConfigPath, configPath, overwrite: true);
                WriteLog(logPath, "[INFO] Configuracion previa restaurada.");
            }
            else if (File.Exists(templatePath) && !File.Exists(configPath))
            {
                File.Copy(templatePath, configPath, overwrite: true);
                WriteLog(logPath, "[INFO] Configuracion inicial creada desde template.");
            }

            string appExe = Path.Combine(targetDir, "GestionDeFardos.App.exe");
            if (!File.Exists(appExe))
            {
                throw new FileNotFoundException("No se encontro GestionDeFardos.App.exe despues de copiar los archivos.", appExe);
            }

            try
            {
                CreateShortcuts(targetDir, appExe);
                WriteLog(logPath, "[INFO] Accesos directos creados.");
            }
            catch (Exception shortcutEx)
            {
                WriteLog(logPath, "[WARN] No se pudieron crear los accesos directos: " + shortcutEx.Message);
            }

            if (!quiet)
            {
                MessageBox.Show(
                    "Instalacion completada en " + targetDir + Environment.NewLine + Environment.NewLine +
                    "Revise config.json antes de usar la aplicacion por primera vez.",
                    "Gestion de Fardos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }

            WriteLog(logPath, "[INFO] Instalacion completada.");
            return 0;
        }
        catch (Exception ex)
        {
            WriteLog(logPath, "[ERROR] " + ex);
            if (!quiet)
            {
                MessageBox.Show(
                    "La instalacion fallo:" + Environment.NewLine + Environment.NewLine + ex.Message,
                    "Gestion de Fardos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            return 1;
        }
        finally
        {
            if (Directory.Exists(tempRoot))
            {
                try
                {
                    Directory.Delete(tempRoot, recursive: true);
                }
                catch
                {
                }
            }
        }
    }

    private static void WriteLog(string logPath, string message)
    {
        File.AppendAllText(
            logPath,
            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + message + Environment.NewLine);
    }

    private static void WritePayloadZip(string destinationPath)
    {
        const string resourceName = "GestionDeFardos.Setup.payload.zip";
        using (Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
        {
            if (resourceStream == null)
            {
                throw new InvalidOperationException("No se encontro el payload embebido del instalador.");
            }

            using (FileStream fileStream = File.Create(destinationPath))
            {
                resourceStream.CopyTo(fileStream);
            }
        }
    }

    private static void CopyDirectory(string sourceDir, string destinationDir)
    {
        foreach (string directory in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
        {
            string destinationPath = directory.Replace(sourceDir, destinationDir);
            Directory.CreateDirectory(destinationPath);
        }

        foreach (string file in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
        {
            string destinationPath = file.Replace(sourceDir, destinationDir);
            string destinationFolder = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrWhiteSpace(destinationFolder))
            {
                Directory.CreateDirectory(destinationFolder);
            }

            File.Copy(file, destinationPath, overwrite: true);
        }
    }

    private static void CreateShortcuts(string targetDir, string appExe)
    {
        string desktopDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        string programsDir = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
        string startMenuDir = Path.Combine(programsDir, "Gestion de Fardos");

        Directory.CreateDirectory(startMenuDir);

        CreateShortcut(Path.Combine(desktopDir, "Gestion de Fardos.lnk"), appExe, targetDir);
        CreateShortcut(Path.Combine(startMenuDir, "Gestion de Fardos.lnk"), appExe, targetDir);
    }

    private static void CreateShortcut(string shortcutPath, string targetPath, string workingDirectory)
    {
        Type shellType = Type.GetTypeFromProgID("WScript.Shell");
        if (shellType == null)
        {
            throw new InvalidOperationException("No se pudo crear el acceso directo porque WScript.Shell no esta disponible.");
        }

        object shell = null;
        object shortcut = null;

        try
        {
            shell = Activator.CreateInstance(shellType);
            if (shell == null)
            {
                throw new InvalidOperationException("No se pudo inicializar WScript.Shell.");
            }

            shortcut = shellType.InvokeMember(
                "CreateShortcut",
                BindingFlags.InvokeMethod,
                binder: null,
                target: shell,
                args: new object[] { shortcutPath });

            var shortcutType = shortcut.GetType();
            shortcutType.InvokeMember("TargetPath", BindingFlags.SetProperty, null, shortcut, new object[] { targetPath });
            shortcutType.InvokeMember("WorkingDirectory", BindingFlags.SetProperty, null, shortcut, new object[] { workingDirectory });
            shortcutType.InvokeMember("Save", BindingFlags.InvokeMethod, null, shortcut, Array.Empty<object>());
        }
        finally
        {
            if (shortcut != null && Marshal.IsComObject(shortcut))
            {
                Marshal.FinalReleaseComObject(shortcut);
            }

            if (shell != null && Marshal.IsComObject(shell))
            {
                Marshal.FinalReleaseComObject(shell);
            }
        }
    }
}
'@

    Set-Content -Path $bootstrapperProgramFile -Value $programContent -Encoding ASCII
}

function Build-WithBootstrapper {
    Stage-PublishPayload `
        -StagingDir $nativeStagingDir `
        -PayloadDir $nativePayloadDir `
        -PayloadZip $nativePayloadZip `
        -DestinationOutputDir $nativeOutputDir

    Write-BootstrapperProject

    $frameworkDir = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319'
    $cscPath = Join-Path $frameworkDir 'csc.exe'
    $compressionDll = Join-Path $frameworkDir 'System.IO.Compression.dll'
    $compressionFsDll = Join-Path $frameworkDir 'System.IO.Compression.FileSystem.dll'
    $formsDll = Join-Path $frameworkDir 'System.Windows.Forms.dll'

    if (-not (Test-Path $cscPath)) {
        throw "[ERROR] No se encontro csc.exe para compilar el bootstrapper instalador."
    }

    & $cscPath `
        /nologo `
        /target:winexe `
        /platform:x64 `
        "/out:$bootstrapperBuiltExe" `
        "/resource:$bootstrapperPayloadFile,GestionDeFardos.Setup.payload.zip" `
        "/reference:$compressionDll" `
        "/reference:$compressionFsDll" `
        "/reference:$formsDll" `
        $bootstrapperProgramFile

    if ($LASTEXITCODE -ne 0) {
        throw "[ERROR] Fallo la compilacion del bootstrapper instalador."
    }

    if (-not (Test-Path $bootstrapperBuiltExe)) {
        throw "[ERROR] No se encontro el ejecutable del bootstrapper instalador."
    }

    New-Item -ItemType Directory -Force -Path $outputDir | Out-Null
    Copy-Item $bootstrapperBuiltExe $outputExe -Force

    if (Test-Path $installGuideSource) {
        Copy-Item $installGuideSource $installGuideOutput -Force
    }
}

function Build-WithInnoSetup {
    param([string]$IsccPath)

    & $IsccPath `
        "/DMyAppVersion=$Version" `
        "/DMyPublishDir=$publishDir" `
        "/DMyOutputDir=$outputDir" `
        $installerScript
}

Ensure-PublishOutput

$iscc = Get-Command iscc -ErrorAction SilentlyContinue
if ($iscc) {
    Build-WithInnoSetup -IsccPath $iscc.Path
    if (Test-Path $installGuideSource) {
        New-Item -ItemType Directory -Force -Path $outputDir | Out-Null
        Copy-Item $installGuideSource $installGuideOutput -Force
    }
    Write-Host "[INFO] Instalador generado con Inno Setup en: $outputDir"
    exit 0
}

Build-WithBootstrapper
Write-Host "[INFO] Instalador generado con bootstrapper integrado en: $outputExe"
