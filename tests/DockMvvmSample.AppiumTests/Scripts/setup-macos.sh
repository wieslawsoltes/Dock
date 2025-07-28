#!/bin/bash
# Bash script to set up Appium testing environment on macOS

set -e

SKIP_APPIUM_INSTALL=false
SKIP_BUILD=false

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --skip-appium-install)
            SKIP_APPIUM_INSTALL=true
            shift
            ;;
        --skip-build)
            SKIP_BUILD=true
            shift
            ;;
        *)
            echo "Unknown option: $1"
            echo "Usage: $0 [--skip-appium-install] [--skip-build]"
            exit 1
            ;;
    esac
done

echo "Setting up Appium testing environment for macOS..."

# Function to check if a command exists
command_exists() {
    command -v "$1" >/dev/null 2>&1
}

# Check prerequisites
echo "Checking prerequisites..."

# Check Node.js
if ! command_exists node; then
    echo "❌ Node.js is not installed. Please install Node.js from https://nodejs.org/"
    exit 1
fi
echo "✅ Node.js is installed: $(node --version)"

# Check npm
if ! command_exists npm; then
    echo "❌ npm is not installed. Please install npm (usually comes with Node.js)"
    exit 1
fi
echo "✅ npm is installed: $(npm --version)"

# Check .NET
if ! command_exists dotnet; then
    echo "❌ .NET is not installed. Please install .NET 9.0 SDK from https://dotnet.microsoft.com/download"
    exit 1
fi
echo "✅ .NET is installed: $(dotnet --version)"

# Check Xcode Command Line Tools
if ! command_exists xcode-select; then
    echo "❌ Xcode Command Line Tools are not installed. Installing..."
    xcode-select --install
    echo "Please complete the Xcode Command Line Tools installation and run this script again."
    exit 1
fi
echo "✅ Xcode Command Line Tools are available"

# Install Appium if not skipped
if [ "$SKIP_APPIUM_INSTALL" = false ]; then
    echo "Installing Appium..."
    npm install -g appium
    echo "✅ Appium installed successfully"

    # Install Mac2 driver
    echo "Installing Appium Mac2 driver..."
    appium driver install mac2
    echo "✅ Appium Mac2 driver installed successfully"
else
    echo "⚠️ Skipping Appium installation (--skip-appium-install specified)"
fi

# Check for Homebrew (optional but recommended)
if ! command_exists brew; then
    echo "⚠️ Homebrew is not installed. It's recommended for managing dependencies."
    echo "You can install it from https://brew.sh/"
else
    echo "✅ Homebrew is installed"
fi

# Enable Accessibility permissions (required for macOS automation)
echo "Checking macOS accessibility permissions..."
echo "⚠️ You may need to grant accessibility permissions to:"
echo "   - Terminal.app (or your IDE)"
echo "   - Appium"
echo "   - The DockMvvmSample application"
echo ""
echo "To do this:"
echo "1. Go to System Preferences > Security & Privacy > Privacy > Accessibility"
echo "2. Click the lock icon and enter your password"
echo "3. Add the applications mentioned above"
echo ""

# Build DockMvvmSample if not skipped
if [ "$SKIP_BUILD" = false ]; then
    echo "Building DockMvvmSample..."
    SAMPLE_PATH="../../samples/DockMvvmSample"
    if [ -d "$SAMPLE_PATH" ]; then
        pushd "$SAMPLE_PATH" > /dev/null
        dotnet build -c Debug
        echo "✅ DockMvvmSample built successfully"
        popd > /dev/null
    else
        echo "⚠️ Warning: DockMvvmSample path not found: $SAMPLE_PATH"
    fi
else
    echo "⚠️ Skipping build (--skip-build specified)"
fi

# Build test project
echo "Building test project..."
dotnet restore
dotnet build -c Debug
echo "✅ Test project built successfully"

# Make run script executable
chmod +x Scripts/run-tests-macos.sh

echo ""
echo "✅ Setup completed successfully!"
echo ""
echo "To run the tests:"
echo "1. Manual approach:"
echo "   - Start Appium server: appium --port 4723"
echo "   - In another terminal, run tests: dotnet test"
echo ""
echo "2. Automated approach:"
echo "   - Run: ./Scripts/run-tests-macos.sh"
echo ""
echo "⚠️ Important notes for macOS:"
echo "   - Make sure to grant accessibility permissions as mentioned above"
echo "   - The first test run may prompt for additional permissions"
echo "   - You may need to allow the DockMvvmSample app in System Preferences" 