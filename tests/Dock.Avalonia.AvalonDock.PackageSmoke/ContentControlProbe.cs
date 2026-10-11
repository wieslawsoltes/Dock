using System;
using Avalonia;
using Avalonia.Input;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;

namespace PackageConsumer;

public sealed class ContentControlProbe : LayoutDocumentControl
{
    public int ModelChanges { get; private set; }

    protected override void OnModelChanged(AvaloniaPropertyChangedEventArgs e)
    {
        base.OnModelChanged(e);
        ModelChanges++;
    }

    protected override void OnPreviewGotKeyboardFocus(FocusChangedEventArgs e) => base.OnPreviewGotKeyboardFocus(e);
    protected override void OnPreviewMouseLeftButtonDown(PointerPressedEventArgs e) => base.OnPreviewMouseLeftButtonDown(e);
    protected override void OnPreviewMouseRightButtonDown(PointerPressedEventArgs e) => base.OnPreviewMouseRightButtonDown(e);

    internal static void Verify()
    {
        LayoutDocument document = new();
        DockingManager manager = new() { Layout = new() { RootPanel = new LayoutPanel(new LayoutDocumentPane(document)) } };
        ContentControlProbe control = new() { Model = document };
        if (control.ModelChanges != 1 || !ReferenceEquals(control.LayoutItem, manager.GetLayoutItemFromModel(document)))
            throw new Exception("An external content-control override did not resolve the existing layout item.");
        document.IsEnabled = false;
        if (control.IsEnabled) throw new Exception("The packaged content control ignored model enablement.");
        control.SetLayoutItem(null);
        if (control.LayoutItem is not null) throw new Exception("The protected content-control container setter did not clear the read-only property.");
        control.Model = null;
        if (control.ModelChanges != 2 || !control.IsEnabled)
            throw new Exception("Clearing the packaged control's model did not invoke the model hook and restore enablement.");
    }
}
