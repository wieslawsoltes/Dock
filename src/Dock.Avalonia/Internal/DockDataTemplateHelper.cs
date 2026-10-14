// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Collections.Generic;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml.Templates;

namespace Dock.Avalonia.Internal;

internal static class DockDataTemplateHelper
{
    public static IEnumerable<IDataTemplate> CreateDefaultDataTemplates()
    {
        foreach (DataTemplate template in new DockDataTemplates())
        {
            // Dock content controls register their model on attachment. Preserve their existing
            // non-recycling lifecycle while the compiled XAML template constructs each control.
            yield return new FuncDataTemplate(template.DataType!, (data, _) => template.Build(data), supportsRecycling: false);
        }
    }
}
