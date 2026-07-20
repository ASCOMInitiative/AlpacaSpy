#!/bin/bash
set -e

# ------------------------------
# CONSTANTS
# ------------------------------
HOME="/home/peter" # Root folder on the WSL file system

# ------------------------------
# CONFIGURATION
# ------------------------------
REPO_URL="https://github.com/ASCOMInitiative/AlpacaSpy.git"

# Linux file system
APPLICATIONNAME="alpacaspy" # Must be lower case!
ROOTFOLDER="$HOME/$APPLICATIONNAME"
CHECKOUTFOLDER="$ROOTFOLDER/checkout"
PUBLISHWORKFOLDER="$ROOTFOLDER/publishwork"
PROJECT="$CHECKOUTFOLDER/AlpacaSpy/AlpacaSpy.csproj" # Name and location of the project file to be built

# Windows file system
WINDOWSPROJECTFOLDER="/mnt/j/AlpacaSpy" # Location where the solution / project code is located
WINDOWSTARFOLDER="$WINDOWSPROJECTFOLDER/publish" # Location on the Windows file system to which the TAR files will be copied

# ------------------------------
# PREPARE WORKDIR
# ------------------------------
echo "Preparing work directories: $CHECKOUTFOLDER and $PUBLISHWORKFOLDER"
rm -rf "$CHECKOUTFOLDER"
mkdir -p "$CHECKOUTFOLDER"
rm -rf "$PUBLISHWORKFOLDER"
mkdir -p "$PUBLISHWORKFOLDER"

cd "$CHECKOUTFOLDER"

echo "Publishing project: $PROJECT"

# ------------------------------
# CLONE REPOSITORY
# ------------------------------
echo "Cloning repository..."
git clone "$REPO_URL" "$CHECKOUTFOLDER"

# ------------------------------
# RESTORE & BUILD
# ------------------------------
echo "Restoring packages..."
dotnet restore "$PROJECT"

echo "Building project..."
dotnet build "$PROJECT" -c Release

# ------------------------------
# PUBLISH TARGETS
#     "linux-arm"
#    "linux-arm64"
# ------------------------------
RIDS=(
    "linux-x64"
    "linux-arm"
    "linux-arm64"
)

for RID in "${RIDS[@]}"; do
    OUTDIR="$PUBLISHWORKFOLDER/publish-$RID"
    echo "Publish folder: $OUTDIR"    
    mkdir "$OUTDIR"
    
    TARFILE="$PUBLISHWORKFOLDER/$APPLICATIONNAME.$RID.tar.xz"
    echo "Tar file: $TARFILE"    
    
    dotnet publish "$PROJECT" \
        -c Release \
        -r "$RID" \
        --self-contained true \
        -p:PublishTrimmed=false \
        -p:PublishSingleFile=true \
        -p:PublishReadyToRunShowWarnings=true \
        -p:UseAppHost=true \
        -o "$OUTDIR"

    cd "$OUTDIR"
    
    echo "Starting tar..."
    time tar --create --verbose --xz --file="$TARFILE" *
    echo "Completed tar"

done

cp -f "$PUBLISHWORKFOLDER"/*.xz "$WINDOWSTARFOLDER"

echo "All builds complete."
