# Async Closing Confirmation

The Dock library now supports async operations when closing dockables and windows. This allows you to show confirmation dialogs or perform other async operations before a tab or window is closed.

## Overview

When a user attempts to close a dockable (like a document tab) or window, the following events are raised in order:

1. `DockableClosingAsync` or `WindowClosingAsync` - Raised first, supports async operations
2. `IDockable.OnClose()` or `IDockWindow.OnClose()` - The synchronous method on the dockable/window
3. `DockableClosing` or `WindowClosing` - The synchronous closing event

Any of these can cancel the close operation by returning `false` or setting `Cancel = true`.

## Using DockableClosingAsync Event

### Basic Usage

Subscribe to the `DockableClosingAsync` event on your factory:

```csharp
public class MyDockFactory : Factory
{
    public MyDockFactory()
    {
        // Subscribe to the async closing event
        DockableClosingAsync += OnDockableClosingAsync;
    }

    private void OnDockableClosingAsync(object? sender, DockableClosingAsyncEventArgs e)
    {
        // Set an async handler that will be executed before closing
        e.SetAsyncCancelHandler(async () =>
        {
            // Show a confirmation dialog (pseudo-code)
            var result = await ShowConfirmationDialogAsync(
                "Close Document?",
                $"Are you sure you want to close '{e.Dockable?.Title}'?");
            
            // Return true to CANCEL the close, false to ALLOW it
            return result == DialogResult.Cancel;
        });
    }
}
```

### With Avalonia MessageBox

Here's a complete example using an Avalonia message box:

```csharp
using Avalonia.Controls;
using Dock.Model;
using Dock.Model.Core.Events;
using System.Threading.Tasks;

public class MyDockFactory : Factory
{
    private readonly Window _mainWindow;

    public MyDockFactory(Window mainWindow)
    {
        _mainWindow = mainWindow;
        DockableClosingAsync += OnDockableClosingAsync;
    }

    private void OnDockableClosingAsync(object? sender, DockableClosingAsyncEventArgs e)
    {
        // Only show confirmation for documents with unsaved changes
        if (e.Dockable?.IsModified == true)
        {
            e.SetAsyncCancelHandler(async () =>
            {
                var result = await ShowSaveConfirmationAsync(e.Dockable.Title);
                
                // Cancel closing if user clicked Cancel
                return result == SaveConfirmationResult.Cancel;
            });
        }
    }

    private async Task<SaveConfirmationResult> ShowSaveConfirmationAsync(string documentName)
    {
        var dialog = new Window
        {
            Title = "Unsaved Changes",
            Width = 400,
            Height = 150,
            Content = new StackPanel
            {
                Children =
                {
                    new TextBlock { Text = $"Save changes to '{documentName}'?" },
                    new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Children =
                        {
                            new Button { Content = "Save", Tag = SaveConfirmationResult.Save },
                            new Button { Content = "Don't Save", Tag = SaveConfirmationResult.DontSave },
                            new Button { Content = "Cancel", Tag = SaveConfirmationResult.Cancel }
                        }
                    }
                }
            }
        };

        var result = await dialog.ShowDialog<SaveConfirmationResult>(_mainWindow);
        return result;
    }
}

public enum SaveConfirmationResult
{
    Save,
    DontSave,
    Cancel
}
```

### Synchronous Cancel via Cancel Property

You can also set the `Cancel` property directly for immediate cancellation without async operations:

```csharp
private void OnDockableClosingAsync(object? sender, DockableClosingAsyncEventArgs e)
{
    // Prevent closing if document has unsaved changes
    if (e.Dockable?.IsModified == true)
    {
        e.Cancel = true;
    }
}
```

## Using WindowClosingAsync Event

The same pattern works for windows:

```csharp
public MyDockFactory()
{
    WindowClosingAsync += OnWindowClosingAsync;
}

private void OnWindowClosingAsync(object? sender, WindowClosingAsyncEventArgs e)
{
    e.SetAsyncCancelHandler(async () =>
    {
        // Check if any dockables in the window have unsaved changes
        if (WindowHasUnsavedChanges(e.Window))
        {
            var result = await ShowConfirmationDialogAsync(
                "Close Window?",
                "This window contains unsaved changes. Close anyway?");
            
            return result == DialogResult.Cancel;
        }
        
        return false; // Allow close
    });
}
```

## Backward Compatibility

The existing `DockableClosing` and `WindowClosing` events still work as before. The async events are called first, allowing you to migrate gradually:

- **Legacy approach**: Override `OnClose()` method or use `DockableClosing` event
- **New approach**: Use `DockableClosingAsync` event for async operations

## Example: Overriding OnClose (Legacy Synchronous Approach)

```csharp
public class MyDocument : Document
{
    public override bool OnClose()
    {
        // This still works but blocks the UI thread
        // Not recommended for showing dialogs
        if (IsModified)
        {
            // Synchronous confirmation - blocks UI
            var result = MessageBox.Show("Save changes?");
            return result != DialogResult.Cancel;
        }
        return true;
    }
}
```

## Best Practices

1. **Use async events for UI dialogs**: Always use `DockableClosingAsync` when showing confirmation dialogs to avoid blocking the UI thread.

2. **Check for modified state**: Only show confirmation dialogs when necessary (e.g., when `IsModified` is true).

3. **Return value convention**: The async cancel handler should return `true` to CANCEL the close operation, `false` to ALLOW it.

4. **Handle both events if needed**: You can handle both `DockableClosingAsync` and `DockableClosing` if you need both async and sync logic, but the async event is called first.

5. **Clean up subscriptions**: Remember to unsubscribe from events when disposing your factory.

## See Also

- [IDockable Interface](xref:Dock.Model.Core.IDockable)
- [IFactory Events](xref:Dock.Model.Core.IFactory)
- [DockableClosingAsyncEventArgs](xref:Dock.Model.Core.Events.DockableClosingAsyncEventArgs)
