// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Avalonia.Input;
using Dock.Model;

namespace Dock.Avalonia
{
    public interface IDropHandler
    {
        bool Validate(object sourceContext, object targetContext, object sender, DockOperation operation, DragEventArgs e);
        bool Execute(object sourceContext, object targetContext, object sender, DockOperation operation, DragEventArgs e);
    }
}
