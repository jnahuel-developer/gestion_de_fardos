#ifndef MyAppVersion
  #define MyAppVersion "1.0.0"
#endif

#ifndef MyPublishDir
  #define MyPublishDir "..\artifacts\publish\win-x64"
#endif

#ifndef MyOutputDir
  #define MyOutputDir "..\artifacts\dist"
#endif

[Setup]
AppId={{F1495D5D-8C0B-430D-8A18-0FD68A59D458}
AppName=Gestion de Fardos
AppVersion={#MyAppVersion}
AppVerName=Gestion de Fardos {#MyAppVersion}
DefaultDirName=C:\GestionDeFardos
DefaultGroupName=Gestion de Fardos
DisableProgramGroupPage=yes
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=admin
OutputDir={#MyOutputDir}
OutputBaseFilename=GestionDeFardos-Setup-{#MyAppVersion}-x64
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
UninstallDisplayIcon={app}\GestionDeFardos.App.exe

[Tasks]
Name: "desktopicon"; Description: "Crear acceso directo en el escritorio"; Flags: unchecked

[Files]
Source: "{#MyPublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Excludes: "config.template.json"
Source: "{#MyPublishDir}\config.template.json"; DestDir: "{app}"; DestName: "config.json"; Flags: onlyifdoesntexist ignoreversion
Source: "{#MyPublishDir}\config.template.json"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\Gestion de Fardos"; Filename: "{app}\GestionDeFardos.App.exe"
Name: "{group}\Desinstalar Gestion de Fardos"; Filename: "{uninstallexe}"
Name: "{commondesktop}\Gestion de Fardos"; Filename: "{app}\GestionDeFardos.App.exe"; Tasks: desktopicon
