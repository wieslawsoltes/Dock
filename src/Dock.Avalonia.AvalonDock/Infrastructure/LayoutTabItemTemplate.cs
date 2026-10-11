using Avalonia.Controls;
using Avalonia.Controls.Templates;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Model.Core;

namespace AvalonDock.Infrastructure;

// Keeps Dock's real tab containers while making their public header controls
// available to compiled application themes and selectors.
internal sealed class LayoutTabItemTemplate : IRecyclingDataTemplate
{
    public IDataTemplate? TabTemplate { get; set; }
    public bool Match(object? data) => data is IDockable or LayoutContent;
    public Control? Build(object? data) => Build(data, null);
    public Control? Build(object? data, Control? existing)
    {
        LayoutContent? model = data switch
        {
            LayoutContent content => content,
            IDockable { Factory: LayoutFactory factory } native => factory.GetLayoutItem(native)?.LayoutElement,
            _ => null
        };
        if (model is null) return null;
        Control? tab = existing ?? TabTemplate?.Build(model);
        if (tab is LayoutDocumentTabItem document) document.SetNativeModel(model);
        else if (tab is LayoutAnchorableTabItem tool) tool.SetNativeModel(model);
        return tab;
    }
}
