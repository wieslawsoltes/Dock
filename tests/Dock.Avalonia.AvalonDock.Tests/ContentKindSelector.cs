using Avalonia;
using Avalonia.Controls.Templates;
using AvalonDock.Controls;
using ReactiveUI.Reactive;

namespace Dock.Avalonia.AvalonDock.Tests;

public sealed class AlternateContent(string description) : ReactiveObject
{
    public string Description { get; } = description;
}

public sealed class ContentKindSelector : DataTemplateSelector
{
    public IDataTemplate? Primary { get; set; }
    public IDataTemplate? Alternate { get; set; }
    public object? LastItem { get; private set; }
    public AvaloniaObject? LastContainer { get; private set; }
    public int Calls { get; private set; }

    public override IDataTemplate? SelectTemplate(object? item, AvaloniaObject container)
    {
        LastItem = item;
        LastContainer = container;
        Calls++;
        return item switch { SourceContent => Primary, AlternateContent => Alternate, _ => null };
    }
}
