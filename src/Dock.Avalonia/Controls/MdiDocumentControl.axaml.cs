// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Reactive;
using Avalonia.Styling;
using Dock.Avalonia.Mdi;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Interaction logic for <see cref="MdiDocumentControl"/> xaml.
/// </summary>
[PseudoClasses(":active")]
public class MdiDocumentControl : TemplatedControl
{
    /// <summary>
    /// Define the <see cref="IconTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> IconTemplateProperty =
        AvaloniaProperty.Register<MdiDocumentControl, object?>(nameof(IconTemplate));

    /// <summary>
    /// Define the <see cref="HeaderTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate> HeaderTemplateProperty =
        AvaloniaProperty.Register<MdiDocumentControl, IDataTemplate>(nameof(HeaderTemplate));

    /// <summary>
    /// Define the <see cref="ModifiedTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate> ModifiedTemplateProperty =
        AvaloniaProperty.Register<MdiDocumentControl, IDataTemplate>(nameof(ModifiedTemplate));

    /// <summary>
    /// Define the <see cref="CloseTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate> CloseTemplateProperty =
        AvaloniaProperty.Register<MdiDocumentControl, IDataTemplate>(nameof(CloseTemplate));

    /// <summary>
    /// Defines the <see cref="EmptyContentTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> EmptyContentTemplateProperty =
        AvaloniaProperty.Register<MdiDocumentControl, IDataTemplate?>(nameof(EmptyContentTemplate));

    /// <summary>
    /// Define the <see cref="CloseButtonTheme"/> property.
    /// </summary>
    public static readonly StyledProperty<ControlTheme?> CloseButtonThemeProperty =
        AvaloniaProperty.Register<MdiDocumentControl, ControlTheme?>(nameof(CloseButtonTheme));

    /// <summary>
    /// Define the <see cref="LayoutManager"/> property.
    /// </summary>
    public static readonly StyledProperty<IMdiLayoutManager?> LayoutManagerProperty =
        AvaloniaProperty.Register<MdiDocumentControl, IMdiLayoutManager?>(nameof(LayoutManager), ClassicMdiLayoutManager.Instance);

    /// <summary>
    /// Define the <see cref="IsActive"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<MdiDocumentControl, bool>(nameof(IsActive));

    /// <summary>
    /// Defines the <see cref="HasVisibleDocuments"/> property.
    /// </summary>
    public static readonly DirectProperty<MdiDocumentControl, bool> HasVisibleDocumentsProperty =
        AvaloniaProperty.RegisterDirect<MdiDocumentControl, bool>(
            nameof(HasVisibleDocuments),
            o => o.HasVisibleDocuments);

    private INotifyPropertyChanged? _dockSubscription;
    private INotifyCollectionChanged? _dockablesSubscription;
    private IDock? _currentDock;
    private IDisposable? _dataContextSubscription;
    private bool _hasVisibleDocuments;

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
    /// Gets or sets the close button theme.
    /// </summary>
    public ControlTheme? CloseButtonTheme
    {
        get => GetValue(CloseButtonThemeProperty);
        set => SetValue(CloseButtonThemeProperty, value);
    }

    /// <summary>
    /// Gets or sets the layout manager.
    /// </summary>
    public IMdiLayoutManager? LayoutManager
    {
        get => GetValue(LayoutManagerProperty);
        set => SetValue(LayoutManagerProperty, value);
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
    /// Gets a value indicating whether the current MDI host contains visible MDI documents.
    /// </summary>
    public bool HasVisibleDocuments
    {
        get => _hasVisibleDocuments;
        private set => SetAndRaise(HasVisibleDocumentsProperty, ref _hasVisibleDocuments, value);
    }

    /// <summary>
    /// Initializes new instance of the <see cref="MdiDocumentControl"/> class.
    /// </summary>
    public MdiDocumentControl()
    {
        UpdatePseudoClasses(IsActive);
    }

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        _dataContextSubscription = this.GetObservable(DataContextProperty)
            .Subscribe(new AnonymousObserver<object?>(OnDockChanged));
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        _dataContextSubscription?.Dispose();
        _dataContextSubscription = null;
        DetachDockSubscriptions();
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
            HasVisibleDocuments = false;
            return;
        }

        if (_currentDock is INotifyPropertyChanged propertyChanged)
        {
            _dockSubscription = propertyChanged;
            _dockSubscription.PropertyChanged += DockPropertyChanged;
        }

        AttachDockablesCollection(_currentDock.VisibleDockables as INotifyCollectionChanged);
        UpdateHasVisibleDocuments();
        UpdateZOrder();
    }

    private void DockPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_currentDock is null)
        {
            return;
        }

        if (e.PropertyName == nameof(IDock.VisibleDockables))
        {
            AttachDockablesCollection(_currentDock.VisibleDockables as INotifyCollectionChanged);
            UpdateHasVisibleDocuments();
            UpdateZOrder();
            return;
        }

        if (e.PropertyName == nameof(IDock.ActiveDockable))
        {
            UpdateZOrder();
        }
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
        UpdateHasVisibleDocuments();
        UpdateZOrder();
    }

    private void UpdateHasVisibleDocuments()
    {
        if (_currentDock?.VisibleDockables is null)
        {
            HasVisibleDocuments = false;
            return;
        }

        HasVisibleDocuments = _currentDock.VisibleDockables.OfType<IMdiDocument>().Any();
    }

    private void UpdateZOrder()
    {
        if (_currentDock?.VisibleDockables is null)
        {
            return;
        }

        var documents = _currentDock.VisibleDockables.OfType<IMdiDocument>().ToList();
        var manager = LayoutManager ?? ClassicMdiLayoutManager.Instance;
        manager.UpdateZOrder(documents, _currentDock.ActiveDockable as IMdiDocument);
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
        HasVisibleDocuments = false;
    }
}
