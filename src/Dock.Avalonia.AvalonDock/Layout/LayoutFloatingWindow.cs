using System.Collections.Generic;
using Avalonia;
using Avalonia.Collections;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Core;
using Dock.Model.Core;
using Xceed.Wpf.AvalonDock.Infrastructure;

namespace Xceed.Wpf.AvalonDock.Layout;

/// <summary>A floating layout backed by a native Dock window and root.</summary>
public abstract class LayoutFloatingWindow : LayoutElement, ILayoutContainer
{
    /// <summary>Creates an empty floating window.</summary>
    public LayoutFloatingWindow() : this(CreateWindow()) { }
    internal LayoutFloatingWindow(DockWindow window, LayoutModelContext? context = null)
        : base((RootDock)window.Layout!, context)
    {
        DockWindow = window;
        window.PropertyChanged += WindowChanged;
    }

    /// <summary>Gets the native window that displays this floating layout.</summary>
    public DockWindow DockWindow { get; }
    /// <summary>Gets the public children of this window.</summary>
    public abstract IEnumerable<ILayoutElement> Children { get; }
    /// <summary>Gets the number of public children.</summary>
    public abstract int ChildrenCount { get; }
    /// <summary>Gets whether this window contains a valid layout.</summary>
    public abstract bool IsValid { get; }
    /// <summary>Removes a public child.</summary>
    public abstract void RemoveChild(ILayoutElement element);
    /// <summary>Replaces a public child.</summary>
    public abstract void ReplaceChild(ILayoutElement oldElement, ILayoutElement newElement);

    internal RootDock NativeRoot => (RootDock)DockModel;
    internal void Attach(LayoutRoot root)
    {
        root.Context.Adopt(Context);
        DockWindow.Layout = NativeRoot;
        NativeRoot.Window = DockWindow;
        SetProjectedParent(root);
    }

    internal void CloseHost()
    {
        // Removing a window from the public collection detaches its layout.
        // The native host must close without executing content-close commands.
        DockWindow.Save();
        DockWindow.Layout = null;
        try { DockWindow.Exit(); }
        finally { DockWindow.Layout = NativeRoot; }
    }

    private void WindowChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.Property == DockWindow.OwnerProperty)
            SetProjectedParent(DockWindow.Owner is { } owner ? Context.Get(owner) as ILayoutContainer : null);
    }

    internal override void OnModelPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
    {
        base.OnModelPropertyChanged(sender, args);
        if (args.Property == DockBase.FocusedDockableProperty && NativeRoot.FocusedDockable is { } focused && Root is LayoutRoot root)
            root.ActiveContent = Context.Get(focused) as LayoutContent;
    }

    private static DockWindow CreateWindow()
    {
        RootDock root = new() { VisibleDockables = new AvaloniaList<IDockable>() };
        DockWindow window = new() { Id = nameof(IDockWindow), Layout = root, Width = double.NaN, Height = double.NaN };
        root.Window = window;
        return window;
    }
}
