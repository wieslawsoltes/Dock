# Appium 2.0 URL Path Compatibility Guide

## Issue Description
Appium 2.0 changed the URL path structure:
- **Appium 1.x**: `http://127.0.0.1:4723/wd/hub`
- **Appium 2.x**: `http://127.0.0.1:4723`

This can cause Windows tests to fail while macOS tests work, depending on Appium versions and configurations.

## Current Solution (Modern Appium 2 Approach)
The project has been updated to use the modern Appium 2 URL format:

### Configuration Files
All `appsettings.*.json` files now use:
```json
"ServerUrl": "http://127.0.0.1:4723"
```

### Startup Scripts
Scripts start Appium without the legacy base path:
```bash
# macOS
appium --port 4723

# Windows
appium --port 4723
```

## Alternative: Backward Compatibility Approach
If you need to support legacy clients or encounter issues, you can revert to backward compatibility:

### Option A: Start Appium with Legacy Path Support
```bash
# Use this instead of the regular appium command
appium server -pa /wd/hub --port 4723
# or
appium --base-path /wd/hub --port 4723
```

### Option B: Revert Configuration Files
If using the legacy server startup, revert URLs to:
```json
"ServerUrl": "http://127.0.0.1:4723/wd/hub"
```

## Troubleshooting

### Windows-Specific Issues
1. Ensure Appium 2.x is installed: `npm install -g appium`
2. Check Windows driver: `appium driver install windows`
3. **WinAppDriver Architecture**: Windows requires TWO servers:
   - **WinAppDriver** (port 4724, uses `/wd/hub`) - Microsoft's legacy server
   - **Appium Server** (port 4723, modern URLs) - Appium Windows Driver proxies to WinAppDriver
4. **WinAppDriver Requirements**:
   - Must run as Administrator
   - Requires Windows Developer Mode enabled
   - Runs on default port 4724 with legacy `/wd/hub` URLs (this is correct!)
5. **Common Issues**:
   - WinAppDriver not running: Check if port 4724 is listening
   - Permission errors: Run PowerShell as Administrator
   - Developer Mode: Enable in Windows Settings > Privacy & Security > For developers

### macOS-Specific Issues
1. Ensure Mac2 driver is installed: `appium driver install mac2`
2. Check accessibility permissions
3. Verify bundle ID matches your app

### Mixed Environment Issues
If you have different Appium versions across platforms:
1. Standardize on Appium 2.x everywhere
2. Or use the backward compatibility approach consistently

## Testing the Fix
1. Clean stop any running Appium servers
2. Run the updated scripts
3. Verify tests pass on both platforms

## References
- [Appium Discussion on URL Changes](https://discuss.appium.io/t/issue-with-connecting-appium-to-winappdriver/39970)
- [Appium 2.0 Migration Guide](https://appium.io/docs/en/2.0/guides/migrating-1-to-2/) 