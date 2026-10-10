using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls.Templates;
using AvalonDock.Controls;
using AvalonDock.Layout;

namespace Dock.Avalonia.AvalonDock.Tests;

public sealed class HeaderKindSelector : DataTemplateSelector
{
    public IDataTemplate? DocumentTemplate { get; set; }
    public IDataTemplate? ToolTemplate { get; set; }
    public List<(object? Item, AvaloniaObject Container)> Calls { get; } = new();

    public override IDataTemplate? SelectTemplate(object? item, AvaloniaObject container)
    {
        Calls.Add((item, container));
        return item is LayoutDocument ? DocumentTemplate : ToolTemplate;
    }
}
