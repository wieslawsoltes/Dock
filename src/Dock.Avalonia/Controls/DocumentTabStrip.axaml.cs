﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Document TabStrip custom control.
/// </summary>
[PseudoClasses(":create", ":active", ":hidesingle")]
public class DocumentTabStrip : TabStrip
{
    /// <summary>
    /// Defines the <see cref="CanCreateItem"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> CanCreateItemProperty =
        AvaloniaProperty.Register<DocumentTabStrip, bool>(nameof(CanCreateItem));

    /// <summary>
    /// Define the <see cref="IsActive"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<DocumentTabStrip, bool>(nameof(IsActive));

    /// <summary>
    /// Defines the <see cref="HideSingleFloatingDocumentTabs"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> HideSingleFloatingDocumentTabsProperty =
        AvaloniaProperty.Register<DocumentTabStrip, bool>(nameof(HideSingleFloatingDocumentTabs), true);

    /// <summary>
    /// Gets or sets if tab strop dock can create new items.
    /// </summary>
    public bool CanCreateItem
    {
        get => GetValue(CanCreateItemProperty);
        set => SetValue(CanCreateItemProperty, value);
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
    /// Gets or sets whether the tab strip hides itself in floating windows when only one document is present.
    /// </summary>
    public bool HideSingleFloatingDocumentTabs
    {
        get => GetValue(HideSingleFloatingDocumentTabsProperty);
        set => SetValue(HideSingleFloatingDocumentTabsProperty, value);
    }

    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(DocumentTabStrip);

    /// <summary>
    /// Initializes new instance of the <see cref="DocumentTabStrip"/> class.
    /// </summary>
    public DocumentTabStrip()
    {
        UpdatePseudoClassesCreate(CanCreateItem);
        UpdatePseudoClassesActive(IsActive);
        UpdatePseudoClassesHideSingle(HideSingleFloatingDocumentTabs);
    }

    /// <inheritdoc/>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new DocumentTabStripItem();
    }

    /// <inheritdoc/>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<DocumentTabStripItem>(item, out recycleKey);
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == CanCreateItemProperty)
        {
            UpdatePseudoClassesCreate(change.GetNewValue<bool>());
        }

        if (change.Property == IsActiveProperty)
        {
            UpdatePseudoClassesActive(change.GetNewValue<bool>());
        }

        if (change.Property == HideSingleFloatingDocumentTabsProperty)
        {
            UpdatePseudoClassesHideSingle(change.GetNewValue<bool>());
        }
    }

    private void UpdatePseudoClassesCreate(bool canCreate)
    {
        PseudoClasses.Set(":create", canCreate);
    }

    private void UpdatePseudoClassesActive(bool isActive)
    {
        PseudoClasses.Set(":active", isActive);
    }

    private void UpdatePseudoClassesHideSingle(bool hide)
    {
        PseudoClasses.Set(":hidesingle", hide);
    }
}
