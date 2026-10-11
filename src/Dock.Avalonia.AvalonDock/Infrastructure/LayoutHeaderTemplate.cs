using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Dock.Model.Core;
using AvalonDock.Controls;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

internal enum LayoutHeaderKind { Document, Anchorable, AnchorableTitle, DocumentTitle, DocumentMenu, MenuIcon }

// Adapt native tab and title models without changing Dock's containers or input.
internal sealed class LayoutHeaderTemplate : IRecyclingDataTemplate
{
    public LayoutHeaderKind Kind { get; set; }
    public IDataTemplate? DefaultTemplate { get; set; }
    internal AvaloniaObject? SelectionContainer { get; set; }

    public bool Match(object? data) => data is IDockable or LayoutContent;
    public Control? Build(object? data) => Build(data, null);
    public Control? Build(object? data, Control? existing)
    {
        LayoutItem? item = data switch
        {
            IDockable { Factory: LayoutFactory factory } model => factory.GetLayoutItem(model),
            LayoutContent content => content.Root?.Manager?.GetLayoutItemFromModel(content),
            _ => null
        };
        return item?.BuildHeader(Kind, DefaultTemplate, existing, SelectionContainer);
    }
}
