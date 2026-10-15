using System.Collections.ObjectModel;
using ReactiveUI.Reactive;
using AvalonDock.Layout;

namespace Dock.Avalonia.AvalonDock.Tests;

public sealed class ManagerPropertiesViewModel : ReactiveObject
{
    public LayoutRoot Root { get; } = new();
    public ObservableCollection<object> Documents { get; } = new() { new object() };
    public ObservableCollection<object> Tools { get; } = new() { new object() };
    public ILayoutUpdateStrategy Strategy { get; } = new ManagerPropertyStrategy("compiled style");
}

public sealed class ManagerPropertyStrategy(string title) : ILayoutUpdateStrategy
{
    public bool BeforeInsertDocument(LayoutRoot layout, LayoutDocument documentToShow, ILayoutContainer? destinationContainer)
    { documentToShow.Title = title; return false; }
    public bool BeforeInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableToShow, ILayoutContainer? destinationContainer)
    { anchorableToShow.Title = title; return false; }
    public void AfterInsertDocument(LayoutRoot layout, LayoutDocument documentShown) { }
    public void AfterInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableShown) { }
}
