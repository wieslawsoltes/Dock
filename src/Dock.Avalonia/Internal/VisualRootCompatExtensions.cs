// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace Avalonia;

internal static class VisualRootCompatExtensions
{
    public static Visual? GetVisualRoot(this Visual? visual)
    {
        if (visual is null)
        {
            return null;
        }

        // Avalonia 12 wraps native top levels in a TopLevelHost. Dock's root
        // consumers need the owning Window for activation, coordinates and
        // auxiliary window ownership, rather than that presentation wrapper.
        return TopLevel.GetTopLevel(visual) ?? visual.GetPresentationSource()?.RootVisual;
    }
}
