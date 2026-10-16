using System;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Model.Core;

namespace PackageConsumer;

internal static class OperationProbe
{
    internal static void Verify()
    {
        LayoutDocument document = new() { ContentId = "document", CanClose = false, CanFloat = false };
        LayoutAnchorable tool = new() { ContentId = "tool", CanAutoHide = false, CanDockAsTabbedDocument = false };
        LayoutAnchorablePane tools = new(tool);
        LayoutDocumentPane documents = new(document);
        DockingManager manager = new() { Layout = new() { RootPanel = new() { Children = { documents, tools } } } };
        LayoutAnchorableItem item = (LayoutAnchorableItem)manager.GetLayoutItemFromModel(tool)!;
        if (item.AutoHideCommand!.CanExecute(null) || item.DockAsDocumentCommand!.CanExecute(null))
            throw new Exception("The packaged commands ignored disabled tool capabilities.");
        tool.ToggleAutoHide();
        if (!tool.IsAutoHidden || !manager.DockFactory.IsDockablePinned(tool.DockModel))
            throw new Exception("The explicit auto-hide API incorrectly used the command capability.");
        tool.ToggleAutoHide();
        if (!ReferenceEquals(tools, tool.Parent) || tool.CanAutoHide)
            throw new Exception("The explicit auto-hide return lost its native pane or changed its capability.");
        tool.DockAsDocument();
        if (!ReferenceEquals(documents, tool.Parent) || tool.CanDockAsTabbedDocument)
            throw new Exception("The explicit document placement incorrectly used or changed its command capability.");

        document.Float();
        if (document.IsFloating || !document.IsActive || !document.IsSelected)
            throw new Exception("A denied new float must still activate the requested content.");
        ((IDock)documents.DockModel).CanCloseLastDockable = false;
        bool cancel = true;
        document.Closing += (_, args) => args.Cancel = cancel;
        document.Close();
        if (!ReferenceEquals(manager.Layout, document.Root))
            throw new Exception("The packaged explicit close bypassed cancellation.");
        cancel = false;
        document.Close();
        if (document.Root is not null || document.CanClose)
            throw new Exception("The packaged explicit close incorrectly used or changed its command capability.");
    }
}
