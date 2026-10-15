using Avalonia.Controls;
using Avalonia.Controls.Templates;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace AvalonDock.Infrastructure;

// Pane constructors require the public model. This composition factory resolves
// the existing wrapper; all visuals remain in compiled control themes.
internal sealed class LayoutPaneControlTemplate : IRecyclingDataTemplate
{
    public bool Match(object? data) => data is IDocumentDock or IToolDock;
    public Control? Build(object? data) => Build(data, null);
    public Control? Build(object? data, Control? existing)
    {
        ILayoutElement? model = data is IDockable { Factory: LayoutFactory factory } native
            ? factory.GetLayoutContainer(native) : null;
        if (existing is ILayoutControl control && ReferenceEquals(control.Model, model)) return existing;
        return model switch
        {
            LayoutDocumentPane pane => new LayoutDocumentPaneControl(pane),
            LayoutAnchorablePane pane => new LayoutAnchorablePaneControl(pane),
            _ => null
        };
    }
}
