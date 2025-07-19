// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Dock.Model.Controls;
using Dock.Model.Avalonia.Internal;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Interaction logic for <see cref="DocumentDockControl"/> xaml.
/// </summary>
public class DocumentDockControl : TemplatedControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentDockControl"/> class.
    /// </summary>
    public DocumentDockControl()
    {
        InputBindings.Add(new KeyBinding
        {
            Gesture = new KeyGesture(Key.Tab, KeyModifiers.Control),
            Command = Command.Create(() =>
            {
                if (DataContext is IDocumentDock dock)
                {
                    dock.NextDocument?.Execute(null);
                }
            })
        });

        InputBindings.Add(new KeyBinding
        {
            Gesture = new KeyGesture(Key.Tab, KeyModifiers.Control | KeyModifiers.Shift),
            Command = Command.Create(() =>
            {
                if (DataContext is IDocumentDock dock)
                {
                    dock.PreviousDocument?.Execute(null);
                }
            })
        });
    }
}
