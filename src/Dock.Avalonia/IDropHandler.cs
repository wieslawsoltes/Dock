// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Avalonia.Input;

namespace Dock.Avalonia
{
    public interface IDropHandler
    {
        bool Validate(object context, object sender, DragEventArgs e);
        bool Execute(object context, object sender, DragEventArgs e);
    }
}
