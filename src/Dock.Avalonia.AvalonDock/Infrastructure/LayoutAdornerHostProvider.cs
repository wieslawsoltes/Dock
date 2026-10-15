using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Avalonia.Controls;

namespace AvalonDock.Infrastructure;

internal sealed class LayoutAdornerHostProvider : IDockAdornerHostProvider
{
    public Control? GetAdornerHost(Visual surface)
    {
        ILayoutContainer? region = null;
        for (Visual? current = surface; current is not null; current = current.GetVisualParent())
        {
            if (current is LayoutDocumentPaneControl documents)
            {
                region = documents.Model.Parent;
                while (region?.Parent is LayoutDocumentPaneGroup parent) region = parent;
                if (region is not (LayoutDocumentPaneGroup or LayoutPanel)) return null;
            }
            if (region is not null && current is Control control && current is ILayoutControl view &&
                ReferenceEquals(view.Model, region)) return control;
            if (current is DockControl) break;
        }
        return null;
    }
}
