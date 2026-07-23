@echo off
echo *** Setup environment

rmdir /s /q "publish"
mkdir publish
mkdir publish\Temp

if defined __VCVARSALL_HOST_ARCH (
    echo __VCVARSALL_HOST_ARCH is set to "%__VCVARSALL_HOST_ARCH%"
) else (
    echo __VCVARSALL_HOST_ARCH is NOT set, setting environment
    call "C:\Program Files\Microsoft Visual Studio\18\Community\VC\Auxiliary\Build\vcvarsall.bat" x64
)

cd
cd J:\AlpacaSpy

echo *** Publishing Windows ARM 64bit
dotnet publish AlpacaSpy/AlpacaSpy.csproj -c Debug /p:Platform="Any CPU" -r win-arm64 --framework net10.0 --self-contained true /p:PublishTrimmed=false /p:PublishSingleFile=true -o ./publish/Temp/AlpacaSpyArm64/
echo *** Completed Windows ARM 64bit publish

echo *** Publishing Windows Intel 64bit
dotnet publish AlpacaSpy/AlpacaSpy.csproj -c Debug /p:Platform="Any CPU" -r win-x64   --framework net10.0 --self-contained true /p:PublishTrimmed=false /p:PublishSingleFile=true -o ./publish/Temp/AlpacaSpyx64/
echo *** Completed Windows Intel 64bit publish

rem The Intel 32bit version serves on ARM64 as well because .NET doesn't support publishing 32bit Windows-Arm executables
echo *** Publishing Windows Intel 32bit
dotnet publish AlpacaSpy/AlpacaSpy.csproj -c Debug /p:Platform="Any CPU" -r win-x86   --framework net10.0 --self-contained true /p:PublishTrimmed=false /p:PublishSingleFile=true -o ./publish/Temp/AlpacaSpyx86/
echo *** Completed Windows Intel 32bit publish

echo *** Creating Windows installer
cd J:\AlpacaSpy\Setup
"C:\Program Files (x86)\Inno Setup 6\iscc.exe" "AlpacaSpy.iss"
cd ..

echo *** Builds complete

pause
