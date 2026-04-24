// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia.VisualTree;

namespace Avalonia;

internal static class VisualRootCompatExtensions
{
    public static Visual? GetVisualRoot(this Visual? visual)
    {
        return visual?.GetPresentationSource()?.RootVisual;
    }
}
