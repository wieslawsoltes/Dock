using System.Collections.ObjectModel;
using ReactiveUI.Reactive;
using AvalonDock.Layout;

namespace Dock.Avalonia.AvalonDock.Tests;

public sealed class SourceContent(string text) : ReactiveObject
{
    public string Text { get; } = text;
}

public sealed class SourceViewModel : ReactiveObject
{
    public ObservableCollection<SourceContent> Documents { get; } = new() { new("Document from ViewModel") };
    public ObservableCollection<SourceContent> Tools { get; } = new() { new("Tool from ViewModel") };
    public ILayoutUpdateStrategy Strategy { get; } = new Titles();

    private sealed class Titles : ILayoutUpdateStrategy
    {
        public bool BeforeInsertDocument(LayoutRoot layout, LayoutDocument document, ILayoutContainer? destination)
        {
            document.Title = ((SourceContent)document.Content!).Text;
            return false;
        }
        public bool BeforeInsertAnchorable(LayoutRoot layout, LayoutAnchorable tool, ILayoutContainer? destination)
        {
            tool.Title = ((SourceContent)tool.Content!).Text;
            return false;
        }
        public void AfterInsertDocument(LayoutRoot layout, LayoutDocument document) { }
        public void AfterInsertAnchorable(LayoutRoot layout, LayoutAnchorable tool) { }
    }
}
