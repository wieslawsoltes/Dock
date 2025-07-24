# RootDockDebug window

`RootDockDebug` is a visual helper that displays the hierarchy of docks and dockables at runtime. The `AttachDockDebug` extension method makes it easy to toggle this window from any `TopLevel`.

## Usage

Call `AttachDockDebug` on your main window during application startup. The window appears when the user presses <kbd>F11</kbd> by default.

```csharp
mainWindow.AttachDockDebug(
    viewModel.Layout,
    new KeyGesture(Key.F11));
```

Pass a custom `KeyGesture` to use another shortcut. The method returns an `IDisposable` that unregisters the hotkey and closes the window when disposed.

While the window is open you can press **Ctrl+Shift** over any `DockControl` to automatically select the underlying model in the tree view.

