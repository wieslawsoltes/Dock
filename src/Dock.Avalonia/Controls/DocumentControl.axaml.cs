// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia;
using Avalonia.Controls;
using System;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Reactive;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Interaction logic for <see cref="DocumentControl"/> xaml.
/// </summary>
[PseudoClasses(":active", ":floating")]
public class DocumentControl : TemplatedControl
{
    private DocumentTabStrip? _tabStrip;
    private IDisposable? _subscription;
    /// <summary>
    /// Define the <see cref="HeaderTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate> HeaderTemplateProperty = 
        AvaloniaProperty.Register<DocumentControl, IDataTemplate>(nameof(HeaderTemplate));

    /// <summary>
    /// Define the <see cref="IsActive"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<DocumentControl, bool>(nameof(IsActive));

    /// <summary>
    /// Gets or sets tab header template.
    /// </summary>
    public IDataTemplate HeaderTemplate
    {
        get => GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
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
    /// Initializes new instance of the <see cref="DocumentControl"/> class.
    /// </summary>
    public DocumentControl()
    {
        UpdatePseudoClasses(IsActive);
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _tabStrip = e.NameScope.Find<DocumentTabStrip>("PART_TabStrip");
        if (_tabStrip is not null)
        {
            _subscription = _tabStrip.GetObservable(DocumentTabStrip.HideSingleFloatingDocumentTabsProperty)
                .Subscribe(new AnonymousObserver<bool>(_ => UpdateFloatingPseudoClass()));
        }
        UpdateFloatingPseudoClass();
    }

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        AddHandler(PointerPressedEvent, PressedHandler, RoutingStrategies.Tunnel);
        UpdateFloatingPseudoClass();
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _subscription?.Dispose();
        _subscription = null;
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

        if (change.Property == DataContextProperty)
        {
            UpdateFloatingPseudoClass();
        }
    }

    private void UpdatePseudoClasses(bool isActive)
    {
        PseudoClasses.Set(":active", isActive);
    }

    private void UpdateFloatingPseudoClass()
    {
        if (_tabStrip is null || DataContext is not IDock dock)
        {
            PseudoClasses.Set(":floating", false);
            return;
        }

        bool floating = false;
        if (dock.Factory?.FindRoot(dock, _ => true) is { Window: { } })
        {
            floating = true;
        }

        bool hideTabs = _tabStrip?.HideSingleFloatingDocumentTabs ?? false;

        PseudoClasses.Set(":floating", hideTabs && floating);
    }
}
