// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Reactive;
using Avalonia.Styling;
using Dock.Model.Core;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Interaction logic for <see cref="DocumentControl"/> xaml.
/// </summary>
[PseudoClasses(":active")]
public class DocumentControl : TemplatedControl
{
    /// <summary>
    /// Define the <see cref="IconTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> IconTemplateProperty = 
        AvaloniaProperty.Register<DocumentControl, object?>(nameof(IconTemplate));

    /// <summary>
    /// Define the <see cref="HeaderTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate> HeaderTemplateProperty = 
        AvaloniaProperty.Register<DocumentControl, IDataTemplate>(nameof(HeaderTemplate));

    /// <summary>
    /// Define the <see cref="ModifiedTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate> ModifiedTemplateProperty = 
        AvaloniaProperty.Register<DocumentControl, IDataTemplate>(nameof(ModifiedTemplate));

    /// <summary>
    /// Define the <see cref="CloseTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate> CloseTemplateProperty = 
        AvaloniaProperty.Register<DocumentControl, IDataTemplate>(nameof(CloseTemplate));

    /// <summary>
    /// Defines the <see cref="EmptyContentTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> EmptyContentTemplateProperty =
        AvaloniaProperty.Register<DocumentControl, IDataTemplate?>(nameof(EmptyContentTemplate));

    /// <summary>
    /// Define the <see cref="CloseButtonTheme"/> property.
    /// </summary>
    public static readonly StyledProperty<ControlTheme?> CloseButtonThemeProperty =
        AvaloniaProperty.Register<DocumentControl, ControlTheme?>(nameof(CloseButtonTheme));

    /// <summary>
    /// Gets or sets the close button theme.
    /// </summary>
    public ControlTheme? CloseButtonTheme
    {
        get => GetValue(CloseButtonThemeProperty);
        set => SetValue(CloseButtonThemeProperty, value);
    }

    /// <summary>
    /// Define the <see cref="IsActive"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<DocumentControl, bool>(nameof(IsActive));

    /// <summary>
    /// Defines the <see cref="TabsLayout"/> property.
    /// </summary>
    public static readonly StyledProperty<DocumentTabLayout> TabsLayoutProperty =
        AvaloniaProperty.Register<DocumentControl, DocumentTabLayout>(nameof(TabsLayout));

    /// <summary>
    /// Defines the <see cref="HasVisibleDockables"/> property.
    /// </summary>
    public static readonly DirectProperty<DocumentControl, bool> HasVisibleDockablesProperty =
        AvaloniaProperty.RegisterDirect<DocumentControl, bool>(
            nameof(HasVisibleDockables),
            o => o.HasVisibleDockables);

    private INotifyPropertyChanged? _dockSubscription;
    private INotifyCollectionChanged? _dockablesSubscription;
    private IDock? _currentDock;
    private IDisposable? _dataContextSubscription;
    private bool _hasVisibleDockables;

    /// <summary>
    /// Gets or sets tab icon template.
    /// </summary>
    public object? IconTemplate
    {
        get => GetValue(IconTemplateProperty);
        set => SetValue(IconTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets tab header template.
    /// </summary>
    public IDataTemplate HeaderTemplate
    {
        get => GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets tab modified template.
    /// </summary>
    public IDataTemplate ModifiedTemplate
    {
        get => GetValue(ModifiedTemplateProperty);
        set => SetValue(ModifiedTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets tab close template.
    /// </summary>
    public IDataTemplate CloseTemplate
    {
        get => GetValue(CloseTemplateProperty);
        set => SetValue(CloseTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets template used to render empty host content.
    /// </summary>
    public IDataTemplate? EmptyContentTemplate
    {
        get => GetValue(EmptyContentTemplateProperty);
        set => SetValue(EmptyContentTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets if this is the currently active dockable.
    /// </summary>
    public bool IsActive
    {
        get => GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    /// <summary>
    /// Gets or sets tabs layout.
    /// </summary>
    public DocumentTabLayout TabsLayout
    {
        get => GetValue(TabsLayoutProperty);
        set => SetValue(TabsLayoutProperty, value);
    }

    /// <summary>
    /// Gets a value indicating whether the current tabbed host contains visible dockables.
    /// </summary>
    public bool HasVisibleDockables
    {
        get => _hasVisibleDockables;
        private set => SetAndRaise(HasVisibleDockablesProperty, ref _hasVisibleDockables, value);
    }

    /// <summary>
    /// Initializes new instance of the <see cref="DocumentControl"/> class.
    /// </summary>
    public DocumentControl()
    {
        UpdatePseudoClasses(IsActive);
    }

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        AddHandler(PointerPressedEvent, PressedHandler, RoutingStrategies.Tunnel);

        _dataContextSubscription = this.GetObservable(DataContextProperty)
            .Subscribe(new AnonymousObserver<object?>(OnDockChanged));
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        RemoveHandler(PointerPressedEvent, PressedHandler);

        _dataContextSubscription?.Dispose();
        _dataContextSubscription = null;
        DetachDockSubscriptions();
    }

    private void PressedHandler(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is IDock {Factory: { } factory} dock && dock.ActiveDockable is { })
        {
            if (factory.FindRoot(dock.ActiveDockable, _ => true) is { } root)
            {
                factory.SetFocusedDockable(root, dock.ActiveDockable);
            }
        }
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsActiveProperty)
        {
            UpdatePseudoClasses(change.GetNewValue<bool>());
        }
    }

    private void UpdatePseudoClasses(bool isActive)
    {
        PseudoClasses.Set(":active", isActive);
    }

    private void OnDockChanged(object? dataContext)
    {
        DetachDockSubscriptions();

        _currentDock = dataContext as IDock;
        if (_currentDock is null)
        {
            HasVisibleDockables = false;
            return;
        }

        if (_currentDock is INotifyPropertyChanged propertyChanged)
        {
            _dockSubscription = propertyChanged;
            _dockSubscription.PropertyChanged += DockPropertyChanged;
        }

        AttachDockablesCollection(_currentDock.VisibleDockables as INotifyCollectionChanged);
        UpdateHasVisibleDockables();
    }

    private void DockPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_currentDock is null)
        {
            return;
        }

        if (e.PropertyName != nameof(IDock.VisibleDockables))
        {
            return;
        }

        AttachDockablesCollection(_currentDock.VisibleDockables as INotifyCollectionChanged);
        UpdateHasVisibleDockables();
    }

    private void AttachDockablesCollection(INotifyCollectionChanged? collection)
    {
        if (_dockablesSubscription != null)
        {
            _dockablesSubscription.CollectionChanged -= DockablesCollectionChanged;
            _dockablesSubscription = null;
        }

        if (collection is null)
        {
            return;
        }

        _dockablesSubscription = collection;
        _dockablesSubscription.CollectionChanged += DockablesCollectionChanged;
    }

    private void DockablesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateHasVisibleDockables();
    }

    private void UpdateHasVisibleDockables()
    {
        HasVisibleDockables = _currentDock?.VisibleDockables?.Count > 0;
    }

    private void DetachDockSubscriptions()
    {
        if (_dockSubscription != null)
        {
            _dockSubscription.PropertyChanged -= DockPropertyChanged;
            _dockSubscription = null;
        }

        if (_dockablesSubscription != null)
        {
            _dockablesSubscription.CollectionChanged -= DockablesCollectionChanged;
            _dockablesSubscription = null;
        }

        _currentDock = null;
        HasVisibleDockables = false;
    }
}
