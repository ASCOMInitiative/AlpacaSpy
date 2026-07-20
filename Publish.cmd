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

echo *** Publishing MacOS Intel silicon
dotnet publish AlpacaSpy/AlpacaSpy.csproj -c Debug -p:Platform="Any CPU" -r osx-x64  --framework net10.0 --self-contained true /p:PublishTrimmed=false -p:PublishSingleFile=true -o ./publish/Temp/AlpacaSpyOsxX64/
rem  echo *** Creating tar file
rem tar -cJf publish/alpacaspy.macos-x64.tar.xz -C publish/Temp/AlpacaSpyOsxX64\ *
echo *** Completed MacOS Intel silicon

echo *** Publishing MacOS Apple silicon
dotnet publish AlpacaSpy/AlpacaSpy.csproj -c Debug -p:Platform="Any CPU" -r osx-arm64 --framework net10.0 --self-contained true /p:PublishTrimmed=false -p:PublishSingleFile=true -o ./publish/Temp/AlpacaSpyOsxArm64/
rem echo *** Creating tar file
rem tar -cJf publish/alpacaspy.macos-arm64.tar.xz -C publish/Temp/AlpacaSpyOsxArm64\ *
echo *** Completed MacOS Apple silicon

scp -r J:\AlpacaSpy\publish\Temp\AlpacaSpyOsxX64\* petersimpson@macmini:/Users/petersimpson/builds/AlpacaSpy/source_files/alpacaspy.macos-x64

scp -r J:\AlpacaSpy\publish\Temp\AlpacaSpyOsxArm64\* petersimpson@macmini:/Users/petersimpson/builds/AlpacaSpy/source_files/alpacaspy.macos-arm64

ssh -t petersimpson@macmini "sudo /Users/petersimpson/Builds/AlpacaSpy/makespy 0.9.0"

scp petersimpson@macmini:/Users/petersimpson/Builds/AlpacaSpy/dmg_file/* J:\AlpacaSpy\publish
pause

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

echo *** Publishing Linux builds
wsl -e bash -lc "/mnt/j/AlpacaSpy/build.bash"

echo *** Builds complete

pause
