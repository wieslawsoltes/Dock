# Running Appium Tests

This guide explains how to install Appium and execute the UI tests that drive the
`DockMvvmSample` application. The tests rely on Appium's Windows driver and
therefore must run on a Windows host. Linux or macOS users will need a Windows
virtual machine or remote PC to follow along.

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

The DockMvvmSample tests require the Windows Appium driver, which only runs on
Windows. Use a Windows virtual machine or a remote Windows host and follow the
instructions from the [Windows](#windows) section there.

## macOS

Like on Linux, the Appium tests can only run on Windows because they depend on
the Windows driver. Set up a Windows VM or remote machine, install Node.js there
and follow the steps from the [Windows](#windows) section.

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
