;
; Script to install AlpacaSpy
;

; Pre-define ISPP variables
#define FileHandle
#define FileLine
#define MyInformationVersion

; Read the informational SEMVER version string from the file created by the build process
#define FileHandle = FileOpen("..\publish\Temp\InformationVersion.txt"); 
#define FileLine = FileRead(FileHandle)
#pragma message "Informational version number: " + FileLine

; Save the SEMVER version for use in the installer filename
#define MyInformationVersion FileLine

; Close the SEMVER version file
#if FileHandle
  #expr FileClose(FileHandle)
#endif

#define MyAppName "ASCOM AlpacaSpy"
#define MyAppPublisher "ASCOM Initiative (Peter Simpson)"
#define MyAppPublisherURL "https://ascom-standards.org"
#define MyAppSupportURL "URL=https://ascomtalk.groups.io/g/Developer/topics"
#define MyAppUpdatesURL "https://github.com/ASCOMInitiative/AlpacaSpy/releases"
#define MyAppExeName "AlpacaSpy.exe"
#define MyAppAuthor "Peter Simpson"
#define MyAppCopyright "Copyright © 2026 " + MyAppAuthor
#define MyAppVersion GetVersionNumbersString("..\publish\Temp\AlpacaSpyx64\alpacaspy.exe")  ; Create version number variable

[Setup]
AppId={{013745EF-9D2E-446F-8F5E-57CE5D670E63}  
AppCopyright={#MyAppCopyright}
AppName={#MyAppName}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppPublisherURL}
AppSupportURL={#MyAppSupportURL}
AppUpdatesURL={#MyAppUpdatesURL}
AppVerName={#MyAppName}
AppVersion={#MyAppVersion}
ArchitecturesInstallIn64BitMode=x64os arm64
Compression=lzma2/max
DefaultDirName={autopf}\ASCOM\AlpacaSpy
DefaultGroupName=ASCOMAlpacaSpy
DisableDirPage=yes
DisableProgramGroupPage=yes
MinVersion=6.1SP1
OutputBaseFilename=AlpacaSpy({#MyInformationVersion})Setup
OutputDir=..\publish
PrivilegesRequired=admin
SetupIconFile=ASCOM.ico
SetupLogging=true
ShowLanguageDialog=auto
SignToolRunMinimized=yes
SignTool = SignAll
SolidCompression=no
UninstallDisplayName=
UninstallDisplayIcon={app}\{#MyAppExeName}
VersionInfoCompany=ASCOM Initiative
VersionInfoCopyright={#MyAppAuthor}
VersionInfoDescription= {#MyAppName}
VersionInfoProductName={#MyAppName}
VersionInfoProductVersion= {#MyAppVersion}
VersionInfoVersion={#MyAppVersion}
WizardImageFile=NewWizardImage.bmp
WizardSmallImageFile=ASCOMLogo.bmp
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "armenian"; MessagesFile: "compiler:Languages\Armenian.isl"
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"
Name: "bulgarian"; MessagesFile: "compiler:Languages\Bulgarian.isl"
Name: "catalan"; MessagesFile: "compiler:Languages\Catalan.isl"
Name: "corsican"; MessagesFile: "compiler:Languages\Corsican.isl"
Name: "czech"; MessagesFile: "compiler:Languages\Czech.isl"
Name: "danish"; MessagesFile: "compiler:Languages\Danish.isl"
Name: "dutch"; MessagesFile: "compiler:Languages\Dutch.isl"
Name: "finnish"; MessagesFile: "compiler:Languages\Finnish.isl"
Name: "french"; MessagesFile: "compiler:Languages\French.isl"
Name: "german"; MessagesFile: "compiler:Languages\German.isl"
Name: "hebrew"; MessagesFile: "compiler:Languages\Hebrew.isl"
Name: "italian"; MessagesFile: "compiler:Languages\Italian.isl"
Name: "japanese"; MessagesFile: "compiler:Languages\Japanese.isl"
Name: "norwegian"; MessagesFile: "compiler:Languages\Norwegian.isl"
Name: "polish"; MessagesFile: "compiler:Languages\Polish.isl"
Name: "portuguese"; MessagesFile: "compiler:Languages\Portuguese.isl"
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"
Name: "slovak"; MessagesFile: "compiler:Languages\Slovak.isl"
Name: "slovenian"; MessagesFile: "compiler:Languages\Slovenian.isl"
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"
Name: "turkish"; MessagesFile: "compiler:Languages\Turkish.isl"
Name: "ukrainian"; MessagesFile: "compiler:Languages\Ukrainian.isl"

[Files]
; ARM 64bit OS - Install the 64bit app
Source: "..\publish\Temp\AlpacaSpyArm64\*.exe"; DestDir: "{app}"; Flags: ignoreversion signonce; Check: Is64BitInstallMode and IsARM64
Source: "..\publish\Temp\AlpacaSpyArm64\*.dll"; DestDir: "{app}"; Flags: ignoreversion signonce; Check: Is64BitInstallMode and IsARM64
Source: "..\publish\Temp\AlpacaSpyArm64\*"; DestDir: "{app}"; Flags: ignoreversion; Excludes:"*.exe,*.dll"; Check: Is64BitInstallMode and IsARM64
Source: "..\publish\Temp\AlpacaSpyArm64\wwwroot\*"; DestDir: "{app}\wwwroot"; Flags: ignoreversion recursesubdirs createallsubdirs; Check: Is64BitInstallMode and IsARM64

; Intel 64bit OS - Install the 64bit app
Source: "..\publish\Temp\AlpacaSpyX64\*.exe"; DestDir: "{app}"; Flags: ignoreversion signonce; Check: Is64BitInstallMode and IsX64OS
Source: "..\publish\Temp\AlpacaSpyX64\*.dll"; DestDir: "{app}"; Flags: ignoreversion signonce; Check: Is64BitInstallMode and IsX64OS
Source: "..\publish\Temp\AlpacaSpyX64\*"; DestDir: "{app}"; Flags: ignoreversion; Excludes:"*.exe,*.dll"; Check: Is64BitInstallMode and IsX64OS
Source: "..\publish\Temp\AlpacaSpyX64\wwwroot\*"; DestDir: "{app}\wwwroot"; Flags: ignoreversion recursesubdirs createallsubdirs; Check: Is64BitInstallMode and IsX64OS

; Intel 32bit OS - Install the 32bit app
Source: "..\publish\Temp\AlpacaSpyX86\*.exe"; DestDir: "{app}"; Flags: ignoreversion signonce; Check: IsX86 and not Is64BitInstallMode
Source: "..\publish\Temp\AlpacaSpyX86\*.dll"; DestDir: "{app}"; Flags: ignoreversion signonce; Check: IsX86 and not Is64BitInstallMode
Source: "..\publish\Temp\AlpacaSpyX86\*"; DestDir: "{app}"; Flags: ignoreversion; Excludes:"*.exe,*.dll"; Check: IsX86 and not Is64BitInstallMode
Source: "..\publish\Temp\AlpacaSpyX86\wwwroot\*"; DestDir: "{app}\wwwroot"; Flags: ignoreversion recursesubdirs createallsubdirs; Check: IsX86 and not Is64BitInstallMode

; Icon file for shortcuts
Source: "ASCOM.ico"; DestDir: "{app}"; Flags: ignoreversion;

[Icons]
Name: "{autoprograms}\ASCOM AlpacaSpy"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\ASCOM.ico"
Name: "{autodesktop}\ASCOM AlpacaSpy"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon; IconFilename: "{app}\ASCOM.ico"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent unchecked

[UninstallDelete]
Name: "{app}"; Type: dirifempty

[Code]
procedure CurPageChanged(CurPageID: Integer);
begin
  if CurPageID = wpSelectTasks then
  begin
    WizardSelectTasks('windotnet');
  end;
end;

// Code to enable the installer to uninstall previous versions of itself when a new version is installed
procedure CurStepChanged(CurStep: TSetupStep);
var
  ResultCode: Integer;
  UninstallExe: String;
  UninstallRegistry: String;
begin
  if (CurStep = ssInstall) then
	begin
      UninstallRegistry := ExpandConstant('Software\Microsoft\Windows\CurrentVersion\Uninstall\{#SetupSetting("AppId")}' + '_is1');
      if RegQueryStringValue(HKLM, UninstallRegistry, 'UninstallString', UninstallExe) then
        begin
          Exec(RemoveQuotes(UninstallExe), ' /SILENT', '', SW_SHOWNORMAL, ewWaitUntilTerminated, ResultCode);
          sleep(1000);    //Give enough time for the install screen to be repainted before continuing
        end
  end;
end;