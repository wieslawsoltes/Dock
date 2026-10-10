using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Metadata;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xceed.Wpf.AvalonDock.Layout;
using Xceed.Wpf.AvalonDock.Infrastructure;

namespace Xceed.Wpf.AvalonDock;

/// <summary>Hosts an AvalonDock-compatible layout using Dock's controls and models.</summary>
public class DockingManager : TemplatedControl
{
    /// <summary>Identifies the layout property.</summary>
    public static readonly DirectProperty<DockingManager, LayoutRoot> LayoutProperty =
        AvaloniaProperty.RegisterDirect<DockingManager, LayoutRoot>(nameof(Layout), x => x.Layout, (x, v) => x.Layout = v);
    /// <summary>Identifies the active content property.</summary>
    public static readonly DirectProperty<DockingManager, object?> ActiveContentProperty =
        AvaloniaProperty.RegisterDirect<DockingManager, object?>(nameof(ActiveContent), x => x.ActiveContent, (x, v) => x.ActiveContent = v, defaultBindingMode: BindingMode.TwoWay);
    /// <summary>Identifies the live root used by the control template.</summary>
    public static readonly DirectProperty<DockingManager, RootDock> DockLayoutProperty =
        AvaloniaProperty.RegisterDirect<DockingManager, RootDock>(nameof(DockLayout), x => x.DockLayout);
    /// <summary>Identifies the factory used by the control template.</summary>
    public static readonly DirectProperty<DockingManager, IFactory> DockFactoryProperty =
        AvaloniaProperty.RegisterDirect<DockingManager, IFactory>(nameof(DockFactory), x => x.DockFactory);

    private LayoutRoot _layout;
    private object? _activeContent;
    private readonly LayoutSizing _sizing = new();

    /// <summary>Creates a manager with an empty document layout.</summary>
    public DockingManager()
    {
        _layout = new LayoutRoot { Manager = this };
        _layout.PropertyChanged += LayoutPropertyChanged;
    }

    /// <summary>Gets or sets the AvalonDock layout.</summary>
    [Content]
    public LayoutRoot Layout
    {
        get => _layout;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            if (ReferenceEquals(_layout, value)) return;
            if (value.Manager is not null && !ReferenceEquals(value.Manager, this))
                throw new InvalidOperationException("A layout can belong to only one DockingManager.");
            LayoutChanging?.Invoke(this, EventArgs.Empty);
            RootDock oldDock = DockLayout;
            IFactory oldFactory = DockFactory;
            _layout.PropertyChanged -= LayoutPropertyChanged;
            _layout.Manager = null;
            SetAndRaise(LayoutProperty, ref _layout, value);
            _layout.Manager = this;
            _layout.PropertyChanged += LayoutPropertyChanged;
            RaisePropertyChanged(DockFactoryProperty, oldFactory, DockFactory);
            RaisePropertyChanged(DockLayoutProperty, oldDock, DockLayout);
            SynchronizeActiveContent();
            LayoutChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    /// <summary>Gets the live Dock root for integration and templates.</summary>
    public RootDock DockLayout => (RootDock)Layout.DockModel;
    /// <summary>Gets the factory operating the live layout.</summary>
    public IFactory DockFactory => Layout.DockFactory;
    /// <summary>Gets or sets the active document or tool content.</summary>
    public object? ActiveContent
    {
        get => _activeContent;
        set
        {
            if (ReferenceEquals(value, _activeContent)) return;
            if (value is null) Layout.ActiveContent = null;
            else
            {
                foreach (ILayoutElement element in Layout.Descendents())
                    if (element is LayoutContent content && (ReferenceEquals(content.Content, value) || ReferenceEquals(content, value)))
                    {
                        Layout.ActiveContent = content;
                        break;
                    }
            }
            SynchronizeActiveContent();
        }
    }
    /// <summary>Gets or sets the strategy used to insert source items.</summary>
    public ILayoutUpdateStrategy? LayoutUpdateStrategy { get; set; }
    /// <summary>Occurs before replacing the layout.</summary>
    public event EventHandler? LayoutChanging;
    /// <summary>Occurs after replacing the layout.</summary>
    public event EventHandler? LayoutChanged;
    /// <summary>Occurs before a document closes.</summary>
    public event EventHandler<DocumentClosingEventArgs>? DocumentClosing;
    /// <summary>Occurs after a document closes.</summary>
    public event EventHandler<DocumentClosedEventArgs>? DocumentClosed;
    /// <summary>Occurs when active content changes.</summary>
    public event EventHandler? ActiveContentChanged;

    /// <summary>Applies AvalonDock pane dimensions to Dock before measuring.</summary>
    protected override Size MeasureOverride(Size availableSize)
    {
        _sizing.Apply(Layout, availableSize);
        return base.MeasureOverride(availableSize);
    }

    private void LayoutPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LayoutRoot.ActiveContent)) SynchronizeActiveContent();
    }
    private void SynchronizeActiveContent()
    {
        if (SetAndRaise(ActiveContentProperty, ref _activeContent, Layout.ActiveContent?.Content))
            ActiveContentChanged?.Invoke(this, EventArgs.Empty);
    }
    internal void NotifyDocumentClosing(LayoutDocument document, CancelEventArgs args)
    {
        DocumentClosingEventArgs forwarded = new(document) { Cancel = args.Cancel };
        DocumentClosing?.Invoke(this, forwarded);
        args.Cancel |= forwarded.Cancel;
    }
    internal void NotifyDocumentClosed(LayoutDocument document) => DocumentClosed?.Invoke(this, new DocumentClosedEventArgs(document));
}
