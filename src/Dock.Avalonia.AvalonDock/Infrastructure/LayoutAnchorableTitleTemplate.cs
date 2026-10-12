using Avalonia.Controls;
using Avalonia.Controls.Templates;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Model.Core;

namespace AvalonDock.Infrastructure;

internal sealed class LayoutAnchorableTitleTemplate : IRecyclingDataTemplate
{
    public IDataTemplate? TitleTemplate { get; set; }
    public bool Match(object? data) => data is IDockable or LayoutAnchorable;
    public Control? Build(object? data) => Build(data, null);
    public Control? Build(object? data, Control? existing)
    {
        LayoutAnchorable? model = data switch
        {
            LayoutAnchorable anchorable => anchorable,
            IDockable { Factory: LayoutFactory factory } native => factory.GetLayoutContent(native) as LayoutAnchorable,
            _ => null
        };
        if (model is null) return null;
        AnchorablePaneTitle? title = existing as AnchorablePaneTitle ?? TitleTemplate?.Build(model) as AnchorablePaneTitle;
        title?.SetNativeModel(model);
        return title;
    }
}
