#!/bin/bash
# Bash script to run Appium tests on macOS

set -e

TEST_FILTER=""
VERBOSE=false
PORT=4723

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --filter)
            TEST_FILTER="$2"
            shift 2
            ;;
        --verbose)
            VERBOSE=true
            shift
            ;;
        --port)
            PORT="$2"
            shift 2
            ;;
        *)
            echo "Unknown option: $1"
            echo "Usage: $0 [--filter TEST_FILTER] [--verbose] [--port PORT]"
            exit 1
            ;;
    esac
done

echo "Running Appium tests on macOS..."

# Function to check if a port is in use
check_port() {
    local port=$1
    if lsof -Pi :$port -sTCP:LISTEN -t >/dev/null 2>&1; then
        return 0  # Port is in use
    else
        return 1  # Port is free
    fi
}

# Check if Appium is installed
if ! command -v appium >/dev/null 2>&1; then
    echo "❌ Appium is not installed. Please run setup-macos.sh first."
    exit 1
fi

# Build DockMvvmSample app bundle for macOS automation testing
echo "Building DockMvvmSample app bundle..."
cd ../../samples/DockMvvmSample
if [ ! -f "build-app-bundle.sh" ]; then
    echo "❌ build-app-bundle.sh script not found"
    exit 1
fi

# Make sure the script is executable
chmod +x build-app-bundle.sh

# Run the bundle build script
./build-app-bundle.sh

# Check if the app bundle was created
BUNDLE_PATH="bin/Debug/net9.0/DockMvvmSample.app"
if [ ! -d "$BUNDLE_PATH" ]; then
    echo "❌ DockMvvmSample.app bundle not found at: $BUNDLE_PATH"
    echo "App bundle creation failed"
    exit 1
fi

# Verify app bundle structure
EXECUTABLE_PATH="$BUNDLE_PATH/Contents/MacOS/DockMvvmSample"
INFO_PLIST_PATH="$BUNDLE_PATH/Contents/Info.plist"

if [ ! -f "$EXECUTABLE_PATH" ]; then
    echo "❌ Executable not found in app bundle: $EXECUTABLE_PATH"
    exit 1
fi

if [ ! -f "$INFO_PLIST_PATH" ]; then
    echo "❌ Info.plist not found in app bundle: $INFO_PLIST_PATH"
    exit 1
fi

# Verify executable permissions
if [ ! -x "$EXECUTABLE_PATH" ]; then
    echo "❌ Executable does not have execute permissions: $EXECUTABLE_PATH"
    exit 1
fi

echo "✅ DockMvvmSample.app bundle created successfully with correct structure"

# Go back to test directory
cd ../../tests/DockMvvmSample.AppiumTests

# Check if port is already in use
START_NEW_SERVER=true
if check_port $PORT; then
    echo "⚠️ Port $PORT is already in use. Appium server might already be running."
    read -p "Do you want to use the existing server? (y/n): " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        START_NEW_SERVER=false
    else
        echo "Please stop the existing server or use a different port with --port"
        exit 1
    fi
fi

# Start Appium server if needed
APPIUM_PID=""
if [ "$START_NEW_SERVER" = true ]; then
    echo "Starting Appium server on port $PORT..."
    appium --port $PORT &
    APPIUM_PID=$!
    echo "✅ Appium server started (PID: $APPIUM_PID)"
    
    # Wait for Appium to start
    echo "Waiting for Appium server to start..."
    sleep 5
    
    # Verify it's running
    if ! kill -0 $APPIUM_PID 2>/dev/null; then
        echo "❌ Appium server failed to start or exited immediately"
        exit 1
    fi
else
    echo "Using existing Appium server on port $PORT"
fi

# Build test arguments
TEST_ARGS=("test")
if [ -n "$TEST_FILTER" ]; then
    TEST_ARGS+=(--filter "$TEST_FILTER")
fi
if [ "$VERBOSE" = true ]; then
    TEST_ARGS+=(--logger "console;verbosity=detailed")
fi

# Run tests
echo "Running tests..."
TEST_EXIT_CODE=0
dotnet "${TEST_ARGS[@]}" || TEST_EXIT_CODE=$?

# Cleanup: Stop Appium server if we started it
if [ -n "$APPIUM_PID" ] && kill -0 $APPIUM_PID 2>/dev/null; then
    echo "Stopping Appium server..."
    kill $APPIUM_PID
    wait $APPIUM_PID 2>/dev/null
    echo "✅ Appium server stopped"
fi

# Exit with test result
if [ $TEST_EXIT_CODE -eq 0 ]; then
    echo "✅ All tests completed successfully!"
else
    echo "❌ Some tests failed or there were errors"
fi

exit $TEST_EXIT_CODE 