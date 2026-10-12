using System;
using System.ComponentModel;
using Avalonia.Controls;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Model.Core;

namespace PackageConsumer;

internal static class FloatingOverrideProbe
{
    internal static void Verify()
    {
        LayoutDocument main = new() { Content = "Main" };
        DockingManager original = new() { Layout = new() { RootPanel = new LayoutPanel(new LayoutDocumentPane(main)) } };
        DockingManager current = new();
        LayoutDocument document = new() { Content = "Current document" };
        LayoutAnchorable tool = new() { Content = "Current tool", CanClose = true, CanHide = true };
        current.Layout = new() { RootPanel = new() { Children = { new LayoutDocumentPane(document), new LayoutAnchorablePane(tool) } } };
        LayoutDocumentFloatingWindow documentModel = new() { RootDocument = new LayoutDocument { Content = "Original document", CanClose = false } };
        LayoutAnchorable originalTool = new() { Content = "Original tool", CanClose = false, CanHide = false };
        LayoutAnchorableFloatingWindow toolModel = new() { RootPanel = new LayoutAnchorablePaneGroup(new LayoutAnchorablePane(originalTool)) };
        original.Layout.FloatingWindows.Add(documentModel);
        original.Layout.FloatingWindows.Add(toolModel);
        DocumentHost documentHost = new(documentModel);
        ToolHost toolHost = new(toolModel);
        try
        {
            documentHost.SetValue(LayoutFloatingWindowControl.IsMaximizedProperty, true);
            if (!documentHost.IsMaximized || documentHost.WindowState != WindowState.Normal)
                throw new Exception("The packaged maximized flag lost its writable property contract.");
            documentHost.ClearValue(LayoutFloatingWindowControl.IsMaximizedProperty);
            documentHost.Current = toolHost.Current = current.Layout;
            if (!documentHost.CheckClose() || !toolHost.CloseWindowCommand.CanExecute(null) || !toolHost.HideWindowCommand.CanExecute(null))
                throw new Exception("Packaged floating capabilities ignored the overridden Model.");
            toolHost.HideWindowCommand.Execute(null);
            if (!tool.IsHidden || originalTool.IsHidden)
                throw new Exception("The packaged floating hide command operated on its constructor model.");
            original.Layout.ActiveContent = main;
            documentHost.CaptionPress();
            if (!documentModel.RootDocument!.IsActive)
                throw new Exception("The packaged typed caption ignored its constructor model.");
        }
        finally
        {
            documentHost.Current = toolHost.Current = null;
            ((IHostWindow)documentHost).Exit();
            ((IHostWindow)toolHost).Exit();
            original.Layout.FloatingWindows.Clear();
        }

        ClosingHost closing = new(new LayoutDocumentFloatingWindow { RootDocument = new LayoutDocument() });
        try
        {
            CancelEventArgs args = new();
            closing.Check(args);
            if (!closing.HideObservedCancel || args.Cancel)
                throw new Exception("The packaged closing hook did not preserve cancellation ordering and the hide override's change.");
        }
        finally { ((IHostWindow)closing).Exit(); }
    }

    private sealed class DocumentHost(LayoutDocumentFloatingWindow model) : LayoutDocumentFloatingWindowControl(model)
    {
        public ILayoutElement? Current { get; set; }
        public override ILayoutElement Model => Current ?? base.Model;
        public bool CheckClose() => base.CanClose();
        public void CaptionPress() { bool handled = false; FilterMessage(IntPtr.Zero, 0xA1, (IntPtr)2, IntPtr.Zero, ref handled); }
    }

    private sealed class ToolHost(LayoutAnchorableFloatingWindow model) : LayoutAnchorableFloatingWindowControl(model)
    {
        public ILayoutElement? Current { get; set; }
        public override ILayoutElement Model => Current ?? base.Model;
    }

    private sealed class ClosingHost(ILayoutElement model) : LayoutFloatingWindowControl(model)
    {
        private CancelEventArgs? _arguments;
        public override ILayoutElement Model { get; } = model;
        public bool HideObservedCancel { get; private set; }
        public void Check(CancelEventArgs args)
        {
            _arguments = args;
            try { base.OnClosing(args); }
            finally { _arguments = null; }
        }
        protected override bool CanClose(object? parameter = null) => false;
        protected override bool CanHide(object? parameter = null) => true;
        protected override void DoHide()
        {
            HideObservedCancel = _arguments!.Cancel;
            _arguments.Cancel = false;
        }
    }
}
