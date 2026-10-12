using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Styling;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;

namespace PackageConsumer;

internal static class ContainerStyleCallbackProbe
{
    internal static void Verify()
    {
        Verify(true);
        Verify(false);
    }

    private static void Verify(bool documents)
    {
        LayoutContent content = documents ? new LayoutDocument() : new LayoutAnchorable();
        ILayoutPanelElement pane = documents ? new LayoutDocumentPane((LayoutDocument)content) : new LayoutAnchorablePane((LayoutAnchorable)content);
        StyleManager manager = new() { Layout = new() { RootPanel = new LayoutPanel(pane) } };
        LayoutItem item = manager.GetLayoutItemFromModel(content)!;
        ICommand defaultClose = item.CloseCommand!;
        ControlTheme original = new(typeof(LayoutItem)) { Setters = { new Setter(LayoutItem.TitleProperty, "retained") } };
        manager.LayoutItemContainerStyle = original;
        AvaloniaPropertyChangedEventArgs captured = manager.LastChange!;
        manager.LayoutItemContainerStyle = null;
        if (!ReferenceEquals(original, item.Theme) || content.Title != "retained")
            throw new Exception("Clearing the packaged manager style removed an existing container theme.");

        manager.LayoutItemContainerStyleSelector = new ClearingSelector();
        if (item.Theme is not null || !ReferenceEquals(defaultClose, item.CloseCommand))
            throw new Exception("A packaged null selector did not clear the theme and restore default commands.");
        ControlTheme current = new(typeof(LayoutItem)) { Setters = { new Setter(LayoutItem.TitleProperty, "current") } };
        manager.Suppress = true;
        manager.LayoutItemContainerStyle = current;
        if (item.Theme is not null)
            throw new Exception("The packaged manager ignored a suppressed style callback.");
        manager.Suppress = false;
        manager.Apply(captured);
        if (!ReferenceEquals(current, item.Theme) || content.Title != "current")
            throw new Exception("Replaying a packaged style callback did not apply current manager settings.");

        ControlTheme addedTheme = new(typeof(LayoutItem)) { Setters = { new Setter(LayoutItem.TitleProperty, "added") } };
        manager.Suppress = true;
        manager.LayoutItemContainerStyle = addedTheme;
        LayoutContent added = documents ? new LayoutDocument() : new LayoutAnchorable();
        ((ILayoutGroup)pane).InsertChildAt(1, added);
        if (!ReferenceEquals(addedTheme, item.Theme) ||
            !ReferenceEquals(addedTheme, manager.GetLayoutItemFromModel(added)!.Theme))
            throw new Exception("A packaged native addition did not reapply current container styles.");
    }

    private sealed class ClearingSelector : StyleSelector
    {
        public override ControlTheme? SelectStyle(object? item, AvaloniaObject container)
        {
            LayoutItem layoutItem = (LayoutItem)container;
            if (layoutItem.CloseCommand is not null || layoutItem.ActivateCommand is not null)
                throw new Exception("The packaged style selector ran before default commands were cleared.");
            return null;
        }
    }

    private sealed class StyleManager : DockingManager
    {
        internal bool Suppress { get; set; }
        internal AvaloniaPropertyChangedEventArgs? LastChange { get; private set; }
        internal void Apply(AvaloniaPropertyChangedEventArgs e) => base.OnLayoutItemContainerStyleChanged(e);
        protected override void OnLayoutItemContainerStyleChanged(AvaloniaPropertyChangedEventArgs e)
        {
            LastChange = e;
            if (!Suppress) base.OnLayoutItemContainerStyleChanged(e);
        }
    }
}
