#!/bin/bash
set -e

# ------------------------------
# CONFIGURATION
# ------------------------------
APPLICATIONNAME="alpacaspy" # Must be lower case!
APPLICATIONFOLDER="/mnt/j/AlpacaSpy"
TARFOLDER="$APPLICATIONFOLDER/publish"
PROJECT="$APPLICATIONFOLDER/AlpacaSpy/AlpacaSpy.csproj"

# ------------------------------
# CALCULATED VARIABLES
# ------------------------------
WORKDIR="/home/peter/$APPLICATIONNAME"

# ------------------------------
# PREPARE WORKDIR
# ------------------------------
echo "Preparing working directory: $WORKDIR"
rm -rf "$WORKDIR"
mkdir -p "$WORKDIR"

cd "$WORKDIR"

echo "Using project: $PROJECT"

# ------------------------------
# RESTORE & BUILD
# ------------------------------
echo "Restoring..."
dotnet restore "$PROJECT"

echo "Building..."
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

echo "Making $TARFOLDER"    
rm -rf "$TARFOLDER"
mkdir -p "$TARFOLDER"

mkdir -p "$WORKDIR/publish"

for RID in "${RIDS[@]}"; do
    OUTDIR="$WORKDIR/publish-$RID"
    echo "Publish folder: $OUTDIR"    
    mkdir "$OUTDIR"
    
    TARFILE="$WORKDIR/publish/$APPLICATIONNAME.$RID.tar.xz"
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

cp -f "$WORKDIR/publish/"* "$TARFOLDER"

echo "All builds complete."
