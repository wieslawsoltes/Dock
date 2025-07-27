#!/bin/bash

set -e

# Configuration
APP_NAME="DockMvvmSample"
BUNDLE_NAME="DockMvvmSample.app"
PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BUILD_DIR="$PROJECT_DIR/bin/Debug/net9.0"
BUNDLE_DIR="$BUILD_DIR/$BUNDLE_NAME"

echo "Building $APP_NAME as macOS app bundle..."

# Clean previous build
if [ -d "$BUNDLE_DIR" ]; then
    echo "Removing existing bundle: $BUNDLE_DIR"
    rm -rf "$BUNDLE_DIR"
fi

# Build the project
echo "Building project..."
dotnet build "$PROJECT_DIR/$APP_NAME.csproj" -c Debug -f net9.0

# Create app bundle structure
echo "Creating app bundle structure..."
mkdir -p "$BUNDLE_DIR/Contents/MacOS"
mkdir -p "$BUNDLE_DIR/Contents/Resources"

# Copy all built files to MacOS directory
echo "Copying application files..."
cp -r "$BUILD_DIR"/* "$BUNDLE_DIR/Contents/MacOS/" 2>/dev/null || true

# Remove the .app directory from MacOS (avoid recursion)
if [ -d "$BUNDLE_DIR/Contents/MacOS/$BUNDLE_NAME" ]; then
    rm -rf "$BUNDLE_DIR/Contents/MacOS/$BUNDLE_NAME"
fi

# Copy Info.plist
echo "Copying Info.plist..."
cp "$PROJECT_DIR/Info.plist" "$BUNDLE_DIR/Contents/Info.plist"

# Make executable
echo "Setting executable permissions..."
chmod +x "$BUNDLE_DIR/Contents/MacOS/$APP_NAME"

# Create a simple icon file if it doesn't exist
if [ ! -f "$BUNDLE_DIR/Contents/Resources/DockMvvmSample.icns" ]; then
    echo "Creating placeholder icon..."
    # This creates a very basic icon - you can replace with a proper .icns file
    touch "$BUNDLE_DIR/Contents/Resources/DockMvvmSample.icns"
fi

echo "App bundle created successfully at: $BUNDLE_DIR"
echo "You can run it with: open \"$BUNDLE_DIR\""
echo "Or test with Appium using the bundle path in your test configuration." 