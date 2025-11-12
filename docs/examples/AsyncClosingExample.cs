// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Dock.Model;
using Dock.Model.Core.Events;

namespace Dock.Examples;

/// <summary>
/// Example showing how to use async closing confirmation dialogs.
/// This example demonstrates the DockableClosingAsync event which allows
/// you to show confirmation dialogs before closing a dockable.
/// </summary>
public class AsyncClosingExampleFactory : Factory
{
    private readonly Window? _mainWindow;

    public AsyncClosingExampleFactory(Window? mainWindow = null)
    {
        _mainWindow = mainWindow;
        
        // Subscribe to the async closing event
        DockableClosingAsync += OnDockableClosingAsync;
        WindowClosingAsync += OnWindowClosingAsync;
    }

    /// <summary>
    /// Handles async closing confirmation for dockables (tabs).
    /// </summary>
    private void OnDockableClosingAsync(object? sender, DockableClosingAsyncEventArgs e)
    {
        // Example 1: Simple synchronous cancel
        // You can set Cancel = true to immediately prevent closing
        // if (e.Dockable?.Id == "ProtectedDocument")
        // {
        //     e.Cancel = true;
        //     return;
        // }

        // Example 2: Async confirmation dialog for modified documents
        if (e.Dockable?.IsModified == true)
        {
            e.SetAsyncCancelHandler(async () =>
            {
                // Show confirmation dialog
                var result = await ShowSaveConfirmationAsync(e.Dockable.Title);
                
                switch (result)
                {
                    case SaveConfirmationResult.Save:
                        // Save the document (pseudo-code)
                        // await SaveDocumentAsync(e.Dockable);
                        return false; // Allow close after saving
                    
                    case SaveConfirmationResult.DontSave:
                        return false; // Allow close without saving
                    
                    case SaveConfirmationResult.Cancel:
                        return true; // CANCEL the close operation
                    
                    default:
                        return false;
                }
            });
        }
    }

    /// <summary>
    /// Handles async closing confirmation for windows.
    /// </summary>
    private void OnWindowClosingAsync(object? sender, WindowClosingAsyncEventArgs e)
    {
        e.SetAsyncCancelHandler(async () =>
        {
            // Check if the window contains any unsaved dockables
            var hasUnsavedChanges = WindowHasUnsavedChanges(e.Window);
            
            if (hasUnsavedChanges)
            {
                var result = await ShowWindowCloseConfirmationAsync();
                return result == DialogResult.Cancel; // Cancel close if user clicked Cancel
            }
            
            return false; // Allow close
        });
    }

    /// <summary>
    /// Shows a confirmation dialog for saving changes.
    /// This is a simplified example - in a real application you would use your UI framework's dialog system.
    /// </summary>
    private async Task<SaveConfirmationResult> ShowSaveConfirmationAsync(string documentName)
    {
        // In a real Avalonia application, you might use:
        // - MsBox.Avalonia for simple message boxes
        // - A custom dialog window
        // - The ContentDialog control
        
        if (_mainWindow == null)
        {
            return SaveConfirmationResult.DontSave;
        }

        // Pseudo-code for showing a dialog
        // var dialog = new SaveConfirmationDialog(documentName);
        // var result = await dialog.ShowDialog<SaveConfirmationResult>(_mainWindow);
        // return result;
        
        // For this example, we'll just simulate the dialog
        await Task.Delay(100); // Simulate async operation
        return SaveConfirmationResult.DontSave;
    }

    /// <summary>
    /// Shows a confirmation dialog for closing a window with unsaved changes.
    /// </summary>
    private async Task<DialogResult> ShowWindowCloseConfirmationAsync()
    {
        if (_mainWindow == null)
        {
            return DialogResult.OK;
        }

        // Pseudo-code for showing a dialog
        // var result = await MessageBox.Show(
        //     _mainWindow,
        //     "This window contains unsaved changes. Close anyway?",
        //     "Unsaved Changes",
        //     MessageBoxButton.OKCancel);
        // return result;
        
        await Task.Delay(100); // Simulate async operation
        return DialogResult.OK;
    }

    /// <summary>
    /// Checks if a window contains any dockables with unsaved changes.
    /// </summary>
    private bool WindowHasUnsavedChanges(IDockWindow? window)
    {
        if (window?.Layout is null)
        {
            return false;
        }

        // Check all dockables in the window for unsaved changes
        // This is simplified - in a real app you'd traverse the dock tree
        return false;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Clean up event subscriptions
            DockableClosingAsync -= OnDockableClosingAsync;
            WindowClosingAsync -= OnWindowClosingAsync;
        }
        
        base.Dispose(disposing);
    }
}

/// <summary>
/// Result of a save confirmation dialog.
/// </summary>
public enum SaveConfirmationResult
{
    Save,
    DontSave,
    Cancel
}

/// <summary>
/// Result of a generic dialog.
/// </summary>
public enum DialogResult
{
    OK,
    Cancel
}
