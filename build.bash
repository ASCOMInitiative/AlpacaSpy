#!/bin/bash
set -e

# ------------------------------
# CONFIGURATION
# ------------------------------
REPO_URL="https://github.com/ASCOMInitiative/AlpacaSpy.git"
WORKDIR="/mnt/j/AlpacaSpy/publish"
PROJECT=""   # leave empty to auto-detect .sln or .csproj

# ------------------------------
# PREPARE WORKDIR
# ------------------------------
echo "Preparing working directory: $WORKDIR"
rm -rf "$WORKDIR"
mkdir -p "$WORKDIR"

# ------------------------------
# CLONE REPOSITORY
# ------------------------------
echo "Cloning repository..."
git clone "$REPO_URL" "$WORKDIR"

cd "$WORKDIR"

# ------------------------------
# AUTO-DETECT PROJECT FILE
# ------------------------------
if [ -z "$PROJECT" ]; then
    # Prefer .sln if present
    SLN=$(find . -maxdepth 2 -name "*.sln" | head -n 1)
    CSPROJ=$(find . -maxdepth 2 -name "*.csproj" | head -n 1)

    if [ -n "$SLN" ]; then
        PROJECT="$SLN"
    elif [ -n "$CSPROJ" ]; then
        PROJECT="$CSPROJ"
    else
        echo "ERROR: No .sln or .csproj found."
        exit 1
    fi
fi

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
# ------------------------------
RIDS=(
    "linux-x64"
    "linux-arm"
    "linux-arm64"
)

for RID in "${RIDS[@]}"; do
    OUTDIR="$WORKDIR/publish-$RID"
    echo "Publishing for $RID -> $OUTDIR"

    dotnet publish "$PROJECT" \
        -c Release \
        -r "$RID" \
        --self-contained true \
        -o "$OUTDIR"
done

echo "All builds complete."
