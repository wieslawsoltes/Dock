#!/bin/bash

# Wrapper script to launch DockMvvmSample with proper environment
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
APP_DIR="$SCRIPT_DIR/../../../samples/DockMvvmSample/bin/Debug/net9.0"
APP_PATH="$APP_DIR/DockMvvmSample"

echo "Launching DockMvvmSample from: $APP_PATH"

# Ensure the app directory exists
if [ ! -f "$APP_PATH" ]; then
    echo "Error: DockMvvmSample executable not found at $APP_PATH"
    exit 1
fi

# Change to the app directory
cd "$APP_DIR"

# Set environment variables that might help with GUI apps on macOS
export DISPLAY=:0
export NSHighResolutionCapable=YES

# Launch the application and keep it running
exec ./DockMvvmSample "$@" 