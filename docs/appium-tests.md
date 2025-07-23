# Running Appium Tests

This guide explains how to install Appium and execute the UI tests that drive the `DockMvvmSample` application. The tests require an Appium server with the Windows driver installed.

## Windows

1. Install the latest **Node.js LTS** release.
2. Open a command prompt and run:
   ```bash
   npm install -g appium
   appium driver install windows
   appium
   ```
   The last command starts the Appium server on port `4723`.

## Linux

1. Install **Node.js** using your package manager or from [NodeSource](https://github.com/nodesource/distributions).
2. Run the same commands as on Windows:
   ```bash
   npm install -g appium
   appium driver install windows
   appium
   ```

## macOS

1. Install **Node.js**, for example with Homebrew:
   ```bash
   brew install node
   ```
2. Install and start Appium:
   ```bash
   npm install -g appium
   appium driver install windows
   appium
   ```

## Running the tests

1. Build the sample application:
   ```bash
   dotnet build samples/DockMvvmSample/DockMvvmSample.csproj
   ```
2. Keep the Appium server running in a terminal.
3. Execute the test project:
   ```bash
   dotnet test tests/DockMvvmSample.AppiumTests/DockMvvmSample.AppiumTests.csproj --no-build --logger:"console;verbosity=normal"
   ```
   If the server isn't reachable the tests will be skipped.
