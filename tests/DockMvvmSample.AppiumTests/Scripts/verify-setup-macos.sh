#!/bin/bash
# Bash script to verify Appium test setup on macOS

echo "Verifying Appium test setup on macOS..."

all_good=true

# Check .NET
if command -v dotnet >/dev/null 2>&1; then
    dotnet_version=$(dotnet --version)
    echo "✅ .NET SDK: $dotnet_version"
else
    echo "❌ .NET SDK not found"
    all_good=false
fi

# Check Node.js
if command -v node >/dev/null 2>&1; then
    node_version=$(node --version)
    echo "✅ Node.js: $node_version"
else
    echo "❌ Node.js not found"
    all_good=false
fi

# Check npm
if command -v npm >/dev/null 2>&1; then
    npm_version=$(npm --version)
    echo "✅ npm: $npm_version"
else
    echo "❌ npm not found"
    all_good=false
fi

# Check Appium
if command -v appium >/dev/null 2>&1; then
    appium_version=$(appium --version)
    echo "✅ Appium: $appium_version"
else
    echo "❌ Appium not found"
    all_good=false
fi

# Check Xcode Command Line Tools
if command -v xcode-select >/dev/null 2>&1; then
    echo "✅ Xcode Command Line Tools: Available"
else
    echo "❌ Xcode Command Line Tools not found"
    all_good=false
fi

# Check if DockMvvmSample is built
sample_path="../../samples/DockMvvmSample/bin/Debug/net9.0/DockMvvmSample"
if [ -f "$sample_path" ]; then
    echo "✅ DockMvvmSample executable found"
else
    echo "❌ DockMvvmSample executable not found at: $sample_path"
    echo "  Run: dotnet build samples/DockMvvmSample -c Debug"
    all_good=false
fi

# Check test project build
echo "Building test project..."
if dotnet build -c Debug --verbosity quiet >/dev/null 2>&1; then
    echo "✅ Test project builds successfully"
else
    echo "❌ Test project failed to build"
    all_good=false
fi

# Check configuration files
config_files=("appsettings.json" "appsettings.windows.json" "appsettings.macos.json")
for config_file in "${config_files[@]}"; do
    if [ -f "$config_file" ]; then
        echo "✅ Configuration file: $config_file"
    else
        echo "❌ Missing configuration file: $config_file"
        all_good=false
    fi
done

# Check script files
script_files=("Scripts/setup-macos.sh" "Scripts/run-tests-macos.sh")
for script_file in "${script_files[@]}"; do
    if [ -f "$script_file" ] && [ -x "$script_file" ]; then
        echo "✅ Script file: $script_file (executable)"
    elif [ -f "$script_file" ]; then
        echo "⚠️ Script file: $script_file (not executable)"
        chmod +x "$script_file"
        echo "  → Made executable"
    else
        echo "❌ Missing script file: $script_file"
        all_good=false
    fi
done

echo ""
if [ "$all_good" = true ]; then
    echo "🎉 All checks passed! You're ready to run the tests."
    echo ""
    echo "Next steps:"
    echo "  1. Run: ./Scripts/run-tests-macos.sh"
    echo "  2. Or manually start Appium and run: dotnet test"
    echo ""
    echo "⚠️ Remember to grant accessibility permissions if prompted!"
else
    echo "❌ Some checks failed. Please fix the issues above before running tests."
    echo ""
    echo "To fix issues:"
    echo "  1. Run setup script: ./Scripts/setup-macos.sh"
    echo "  2. Build DockMvvmSample: dotnet build samples/DockMvvmSample -c Debug"
fi 