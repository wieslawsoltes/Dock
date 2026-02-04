// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Dock.Avalonia.Controls;
using Dock.Model.Core;

namespace Dock.Avalonia.Services;

internal interface IDockControlFactoryService
{
    void InitializeControlRecycling(DockControl control);
    void CleanupFactory(DockControl control, IDock layout);
}
