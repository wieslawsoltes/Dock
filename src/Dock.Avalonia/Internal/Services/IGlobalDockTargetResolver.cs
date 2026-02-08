// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia.Controls;
using Dock.Model.Core;

namespace Dock.Avalonia.Internal;

internal interface IGlobalDockTargetResolver
{
    IDock? Resolve(Control? dropControl);
}
