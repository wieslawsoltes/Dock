using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Dock.Model.Core;

namespace AvalonDock.Infrastructure;

internal enum LayoutHeaderKind { Document, Anchorable, AnchorableTitle }

// Adapt native tab and title models without changing Dock's containers or input.
internal sealed class LayoutHeaderTemplate : IRecyclingDataTemplate
{
    public LayoutHeaderKind Kind { get; set; }
    public IDataTemplate? DefaultTemplate { get; set; }

    public bool Match(object? data) => data is IDockable;
    public Control? Build(object? data) => Build(data, null);
    public Control? Build(object? data, Control? existing) =>
        data is IDockable { Factory: LayoutFactory factory } model && factory.GetLayoutItem(model) is { } item
            ? item.BuildHeader(Kind, DefaultTemplate, existing) : null;
}
