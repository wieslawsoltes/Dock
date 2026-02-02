// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using Avalonia.VisualTree;
using Dock.Avalonia.Internal;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Document model used for managed floating windows.
/// </summary>
public sealed class ManagedDockWindowDocument : ManagedDockableBase, IMdiDocument, IDocumentContent, IRecyclingDataTemplate, IDisposable
{
    private DockRect _mdiBounds;
    private MdiWindowState _mdiState = MdiWindowState.Normal;
    private int _mdiZIndex;
    private object? _content;
    private Type? _dataType;
    private IDockWindow? _window;
    private INotifyPropertyChanged? _windowSubscription;
    private INotifyPropertyChanged? _layoutSubscription;
    private INotifyPropertyChanged? _focusedDockableSubscription;
    private IDockable? _focusedDockable;

    /// <summary>
    /// Initializes a new instance of the <see cref="ManagedDockWindowDocument"/> class.
    /// </summary>
    public ManagedDockWindowDocument(IDockWindow window)
    {
        _window = window;
        Id = window.Id;
        Title = window.Title;
        Context = window;
        AttachWindow(window);
        AttachLayout(window.Layout);
        UpdateTitleFromLayout();
    }

    /// <summary>
    /// Gets the associated dock window.
    /// </summary>
    public IDockWindow? Window => _window;

    /// <summary>
    /// Gets whether the managed window hosts a tool dock.
    /// </summary>
    public bool IsToolWindow => _window?.Layout?.ActiveDockable is IToolDock;

    /// <summary>
    /// Gets the tool dock hosted by this managed window, if any.
    /// </summary>
    public IToolDock? ToolDock => _window?.Layout?.ActiveDockable as IToolDock;

    /// <summary>
    /// Gets or sets the content to display.
    /// </summary>
    public object? Content
    {
        get => _content;
        set => SetProperty(ref _content, value);
    }

    /// <summary>
    /// Gets or sets the data type for template matching.
    /// </summary>
    public Type? DataType
    {
        get => _dataType;
        set => SetProperty(ref _dataType, value);
    }

    /// <inheritdoc />
    public DockRect MdiBounds
    {
        get => _mdiBounds;
        set
        {
            if (SetProperty(ref _mdiBounds, value))
            {
                SyncBounds();
            }
        }
    }

    /// <inheritdoc />
    public MdiWindowState MdiState
    {
        get => _mdiState;
        set => SetProperty(ref _mdiState, value);
    }

    /// <inheritdoc />
    public int MdiZIndex
    {
        get => _mdiZIndex;
        set => SetProperty(ref _mdiZIndex, value);
    }

    /// <inheritdoc />
    public override bool OnClose()
    {
        if (_window is null)
        {
            return true;
        }

        if (_window.Host is ManagedHostWindow managedHost)
        {
            managedHost.Exit();
            return !managedHost.LastCloseCanceled;
        }

        _window.Exit();
        return true;
    }

    /// <inheritdoc />
    public bool Match(object? data)
    {
        if (DataType is null)
        {
            return true;
        }

        return DataType.IsInstanceOfType(data);
    }

    /// <inheritdoc />
    public Control? Build(object? data) => Build(data, null);

    /// <inheritdoc />
    public Control? Build(object? data, Control? existing)
    {
        return BuildContent(Content, this, existing);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        DetachFocusedDockable();
        DetachLayout();
        DetachWindow();
        _window = null;
        Content = null;
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(string propertyName)
    {
        base.OnPropertyChanged(propertyName);

        if (_window is null)
        {
            return;
        }

        if (propertyName == nameof(Title))
        {
            _window.Title = Title;
        }
        else if (propertyName == nameof(Id))
        {
            _window.Id = Id;
        }
    }

    private void AttachWindow(IDockWindow? window)
    {
        DetachWindow();

        if (window is not INotifyPropertyChanged windowChanged)
        {
            return;
        }

        _windowSubscription = windowChanged;
        _windowSubscription.PropertyChanged += WindowPropertyChanged;
    }

    private void DetachWindow()
    {
        if (_windowSubscription is null)
        {
            return;
        }

        _windowSubscription.PropertyChanged -= WindowPropertyChanged;
        _windowSubscription = null;
    }

    private void WindowPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IDockWindow.Layout))
        {
            AttachLayout(_window?.Layout);
            UpdateTitleFromLayout();
            return;
        }

        if (e.PropertyName == nameof(IDockWindow.Title))
        {
            if (_window?.Layout?.FocusedDockable is null)
            {
                UpdateTitleFromLayout();
            }
        }
    }

    private void AttachLayout(IDock? layout)
    {
        DetachLayout();

        if (layout is not INotifyPropertyChanged layoutChanged)
        {
            return;
        }

        _layoutSubscription = layoutChanged;
        _layoutSubscription.PropertyChanged += LayoutPropertyChanged;
    }

    private void DetachLayout()
    {
        if (_layoutSubscription is null)
        {
            return;
        }

        _layoutSubscription.PropertyChanged -= LayoutPropertyChanged;
        _layoutSubscription = null;
    }

    private void LayoutPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(IDock.FocusedDockable))
        {
            UpdateTitleFromLayout();
        }
    }

    private void UpdateTitleFromLayout()
    {
        var focusedDockable = _window?.Layout?.FocusedDockable;
        UpdateFocusedDockableSubscription(focusedDockable);

        var focusedTitle = focusedDockable?.Title;
        if (!string.IsNullOrEmpty(focusedTitle))
        {
            Title = focusedTitle;
            return;
        }

        if (_window is { } window)
        {
            Title = window.Title;
        }
    }

    private void UpdateFocusedDockableSubscription(IDockable? focusedDockable)
    {
        if (ReferenceEquals(_focusedDockable, focusedDockable))
        {
            return;
        }

        DetachFocusedDockable();

        _focusedDockable = focusedDockable;
        if (_focusedDockable is INotifyPropertyChanged focusedChanged)
        {
            _focusedDockableSubscription = focusedChanged;
            _focusedDockableSubscription.PropertyChanged += FocusedDockablePropertyChanged;
        }
    }

    private void DetachFocusedDockable()
    {
        if (_focusedDockableSubscription is null)
        {
            _focusedDockable = null;
            return;
        }

        _focusedDockableSubscription.PropertyChanged -= FocusedDockablePropertyChanged;
        _focusedDockableSubscription = null;
        _focusedDockable = null;
    }

    private void FocusedDockablePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(IDockable.Title))
        {
            UpdateTitleFromLayout();
        }
    }

    private void SyncBounds()
    {
        if (_window is null)
        {
            return;
        }

        _window.X = _mdiBounds.X;
        _window.Y = _mdiBounds.Y;
        _window.Width = _mdiBounds.Width;
        _window.Height = _mdiBounds.Height;

        if (_window.Host is ManagedHostWindow managedHost)
        {
            managedHost.UpdateBoundsFromDocument(_mdiBounds);
            return;
        }

        if (_window.Host is IHostWindow host)
        {
            host.SetPosition(_mdiBounds.X, _mdiBounds.Y);
            host.SetSize(_mdiBounds.Width, _mdiBounds.Height);
        }
    }

    private static Control? BuildContent(object? content, IDockable dockable, Control? existing)
    {
        if (DragPreviewContext.IsPreviewing(dockable))
        {
            return BuildPreviewContent(content);
        }

        if (content is null)
        {
            return null;
        }

        if (content is Control directControl)
        {
            return DetachOrFallback(directControl, existing, null);
        }

        if (content is Func<IServiceProvider, object> direct)
        {
            return DetachOrFallback(direct(null!) as Control, existing, () => direct(null!) as Control);
        }

        return DetachOrFallback(TemplateContent.Load(content)?.Result, existing, () => TemplateContent.Load(content)?.Result);
    }

    private static Control? DetachOrFallback(Control? control, Control? existing, Func<Control?>? fallbackFactory)
    {
        if (control is null || ReferenceEquals(control, existing))
        {
            return control;
        }

        if (TryDetachFromParent(control))
        {
            return control;
        }

        if (fallbackFactory is null)
        {
            return existing;
        }

        var fallback = fallbackFactory();
        if (fallback is null)
        {
            return existing;
        }

        if (ReferenceEquals(fallback, existing))
        {
            return fallback;
        }

        return TryDetachFromParent(fallback) ? fallback : existing;
    }

    private static bool TryDetachFromParent(Control control)
    {
        var parent = control.Parent ?? control.GetVisualParent();

        if (parent is null)
        {
            return true;
        }

        switch (parent)
        {
            case Panel panel:
                return panel.Children.Remove(control);
            case ContentPresenter presenter:
                return TryDetachFromContentPresenter(presenter, control);
            case ContentControl contentControl when ReferenceEquals(contentControl.Content, control):
                contentControl.SetCurrentValue(ContentControl.ContentProperty, null);
                return true;
            case Decorator decorator when ReferenceEquals(decorator.Child, control):
                decorator.Child = null;
                return true;
            default:
                return false;
        }
    }

    private static bool TryDetachFromContentPresenter(ContentPresenter presenter, Control control)
    {
        if (!ReferenceEquals(presenter.Child, control))
        {
            return false;
        }

        presenter.SetCurrentValue(ContentPresenter.ContentProperty, null);
        presenter.UpdateChild();

        return control.GetVisualParent() is null;
    }

    private static Control BuildPreviewContent(object? content)
    {
        if (content is not Control visualControl)
        {
            return new Panel();
        }

        return new Border
        {
            Background = new VisualBrush
            {
                Visual = visualControl,
                Stretch = Stretch.Uniform
            },
            ClipToBounds = true
        };
    }
}
