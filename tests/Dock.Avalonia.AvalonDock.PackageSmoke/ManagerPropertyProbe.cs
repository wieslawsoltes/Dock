using System;
using System.Collections.ObjectModel;
using System.Linq;
using AvalonDock;
using AvalonDock.Layout;
using ReactiveUI.Reactive;

namespace PackageConsumer;

internal static class ManagerPropertyProbe
{
    internal static void Verify(ConsumerWindow window)
    {
        foreach (string key in new[] { "StyledManager", "LegacyStyledManager" })
        {
            DockingManager manager = (DockingManager)window.Resources[key]!;
            LayoutRoot initial = manager.Layout;
            if (initial.RootPanel.Children.Single() is not LayoutDocumentPaneGroup)
                throw new Exception("The packaged manager did not create the default document-pane group.");
            ManagerPropertyViewModel first = new();
            ManagerPropertyViewModel second = new();
            manager.DataContext = first;
            manager.ClearValue(DockingManager.LayoutProperty);
            manager.ApplyStyling();
            manager.Classes.Add("workspace");
            VerifyValues(manager, first);
            manager.DataContext = second;
            first.Documents.Add(new object());
            first.Tools.Add(new object());
            second.Documents.Add(new object());
            second.Tools.Add(new object());
            VerifyValues(manager, second);
            manager.Classes.Remove("workspace");
            if (manager.DocumentsSource is not null || manager.AnchorablesSource is not null || manager.LayoutUpdateStrategy is not null ||
                ReferenceEquals(manager.Layout, second.Root) || second.Root.Manager is not null)
                throw new Exception("Removing packaged manager styles did not detach their values and layout.");
            LayoutRoot previous = manager.Layout;
            int changing = 0, changed = 0;
            manager.LayoutChanging += (_, _) => changing++;
            manager.LayoutChanged += (_, _) => changed++;
            manager.Layout = null;
            if (ReferenceEquals(previous, manager.Layout) || previous.Manager is not null || changing != 0 || changed != 1)
                throw new Exception("The packaged manager did not coerce a null layout with the expected notifications.");
        }
    }

    private static void VerifyValues(DockingManager manager, ManagerPropertyViewModel viewModel)
    {
        if (!ReferenceEquals(viewModel.Root, manager.Layout) || !ReferenceEquals(viewModel.Strategy, manager.LayoutUpdateStrategy) ||
            !ReferenceEquals(manager, viewModel.Root.Manager) ||
            !viewModel.Documents.SequenceEqual(manager.Layout.Descendents().OfType<LayoutDocument>().Select(content => content.Content)) ||
            !viewModel.Tools.SequenceEqual(manager.Layout.Descendents().OfType<LayoutAnchorable>().Select(content => content.Content)))
            throw new Exception("Compiled packaged styles did not update the manager layout, strategy and sources.");
    }
}

public sealed class ManagerPropertyViewModel : ReactiveObject
{
    public LayoutRoot Root { get; } = new();
    public ObservableCollection<object> Documents { get; } = new() { new object() };
    public ObservableCollection<object> Tools { get; } = new() { new object() };
    public ILayoutUpdateStrategy Strategy { get; } = new Titles();

    private sealed class Titles : ILayoutUpdateStrategy
    {
        public bool BeforeInsertDocument(LayoutRoot layout, LayoutDocument documentToShow, ILayoutContainer? destinationContainer)
        { documentToShow.Title = "Styled document"; return false; }
        public bool BeforeInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableToShow, ILayoutContainer? destinationContainer)
        { anchorableToShow.Title = "Styled tool"; return false; }
        public void AfterInsertDocument(LayoutRoot layout, LayoutDocument documentShown) { }
        public void AfterInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableShown) { }
    }
}
