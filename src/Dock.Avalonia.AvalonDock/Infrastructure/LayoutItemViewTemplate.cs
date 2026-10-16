using Avalonia.Controls;
using Avalonia.Controls.Templates;
using AvalonDock.Controls;

namespace AvalonDock.Infrastructure;

internal sealed class LayoutItemViewTemplate : IRecyclingDataTemplate
{
    public bool Match(object? data) => data is LayoutItem;
    public Control? Build(object? data) => Build(data, null);
    public Control? Build(object? data, Control? existing) =>
        data is LayoutItem item ? LayoutItemViewTransfer.Prepare(item.View, existing) : null;
}
