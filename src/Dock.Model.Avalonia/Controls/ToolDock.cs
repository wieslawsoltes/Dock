// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Avalonia;
using Avalonia.Reactive;
using Dock.Model.Avalonia.Core;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Settings;

namespace Dock.Model.Avalonia.Controls;

/// <summary>
/// Tool dock.
/// </summary>
public class ToolDock : DockBase, IToolDock, IToolDockContent, IToolItemsSourceDock
{
    /// <summary>
    /// Defines the <see cref="Alignment"/> property.
    /// </summary>
    public static readonly DirectProperty<ToolDock, Alignment> AlignmentProperty =
        AvaloniaProperty.RegisterDirect<ToolDock, Alignment>(nameof(Alignment), o => o.Alignment, (o, v) => o.Alignment = v, Alignment.Unset);

    /// <summary>
    /// Defines the <see cref="IsExpanded"/> property.
    /// </summary>
    public static readonly DirectProperty<ToolDock, bool> IsExpandedProperty =
        AvaloniaProperty.RegisterDirect<ToolDock, bool>(nameof(IsExpanded), o => o.IsExpanded, (o, v) => o.IsExpanded = v);

    /// <summary>
    /// Defines the <see cref="AutoHide"/> property.
    /// </summary>
    public static readonly DirectProperty<ToolDock, bool> AutoHideProperty =
        AvaloniaProperty.RegisterDirect<ToolDock, bool>(nameof(AutoHide), o => o.AutoHide, (o, v) => o.AutoHide = v, true);

    /// <summary>
    /// Defines the <see cref="GripMode"/> property.
    /// </summary>
    public static readonly DirectProperty<ToolDock, GripMode> GripModeProperty =
        AvaloniaProperty.RegisterDirect<ToolDock, GripMode>(nameof(GripMode), o => o.GripMode, (o, v) => o.GripMode = v);

    /// <summary>
    /// Defines the <see cref="ToolTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<IToolTemplate?> ToolTemplateProperty =
        AvaloniaProperty.Register<ToolDock, IToolTemplate?>(nameof(ToolTemplate));

    /// <summary>
    /// Defines the <see cref="ItemsSource"/> property.
    /// </summary>
    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<ToolDock, IEnumerable?>(nameof(ItemsSource));

    /// <summary>
    /// Defines the <see cref="ItemContainerGenerator"/> property.
    /// </summary>
    public static readonly StyledProperty<IDockItemContainerGenerator?> ItemContainerGeneratorProperty =
        AvaloniaProperty.Register<ToolDock, IDockItemContainerGenerator?>(nameof(ItemContainerGenerator));

    /// <summary>
    /// Defines the <see cref="ToolItemContainerTheme"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> ToolItemContainerThemeProperty =
        AvaloniaProperty.Register<ToolDock, object?>(nameof(ToolItemContainerTheme));

    /// <summary>
    /// Defines the <see cref="ToolItemTemplateSelector"/> property.
    /// </summary>
    public static readonly StyledProperty<IToolItemTemplateSelector?> ToolItemTemplateSelectorProperty =
        AvaloniaProperty.Register<ToolDock, IToolItemTemplateSelector?>(nameof(ToolItemTemplateSelector));

    /// <summary>
    /// Defines the <see cref="CanUpdateItemsSourceOnUnregister"/> property.
    /// </summary>
    public static readonly StyledProperty<bool?> CanUpdateItemsSourceOnUnregisterProperty =
        AvaloniaProperty.Register<ToolDock, bool?>(nameof(CanUpdateItemsSourceOnUnregister));

    private Alignment _alignment = Alignment.Unset;
    private bool _isExpanded;
    private bool _autoHide = true;
    private GripMode _gripMode = GripMode.Visible;
    private readonly HashSet<IDockable> _generatedTools = new();
    private readonly Dictionary<IDockable, IDockItemContainerGenerator> _generatedToolGenerators = new();
    private IDisposable? _itemsSourceSubscription;
    private IDisposable? _toolTemplateSubscription;
    private IDisposable? _itemContainerGeneratorSubscription;
    private IDisposable? _toolItemContainerThemeSubscription;
    private IDisposable? _toolItemTemplateSelectorSubscription;
    private INotifyCollectionChanged? _currentCollectionChanged;

    /// <summary>
    /// Initializes new instance of the <see cref="ToolDock"/> class.
    /// </summary>
    public ToolDock()
    {
        _itemsSourceSubscription = this.GetObservable(ItemsSourceProperty)
            .Subscribe(new AnonymousObserver<IEnumerable?>(OnItemsSourceChanged));

        _toolTemplateSubscription = this.GetObservable(ToolTemplateProperty)
            .Subscribe(new AnonymousObserver<IToolTemplate?>(_ => OnToolTemplateChanged()));

        _itemContainerGeneratorSubscription = this.GetObservable(ItemContainerGeneratorProperty)
            .Subscribe(new AnonymousObserver<IDockItemContainerGenerator?>(_ => OnItemContainerGeneratorChanged()));
        _toolItemContainerThemeSubscription = this.GetObservable(ToolItemContainerThemeProperty)
            .Subscribe(new AnonymousObserver<object?>(_ => OnGeneratedItemPresentationChanged()));
        _toolItemTemplateSelectorSubscription = this.GetObservable(ToolItemTemplateSelectorProperty)
            .Subscribe(new AnonymousObserver<IToolItemTemplateSelector?>(_ => OnGeneratedItemPresentationChanged()));
    }

    /// <summary>
    /// Disposes resources used by this ToolDock.
    /// </summary>
    /// <param name="disposing">Whether managed resources should be disposed.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        _itemsSourceSubscription?.Dispose();
        _itemsSourceSubscription = null;

        _toolTemplateSubscription?.Dispose();
        _toolTemplateSubscription = null;

        _itemContainerGeneratorSubscription?.Dispose();
        _itemContainerGeneratorSubscription = null;
        _toolItemContainerThemeSubscription?.Dispose();
        _toolItemContainerThemeSubscription = null;
        _toolItemTemplateSelectorSubscription?.Dispose();
        _toolItemTemplateSelectorSubscription = null;

        if (_currentCollectionChanged != null)
        {
            _currentCollectionChanged.CollectionChanged -= OnCollectionChanged;
            _currentCollectionChanged = null;
        }

        ClearGeneratedTools();
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("Alignment")]
    public Alignment Alignment
    {
        get => _alignment;
        set => SetAndRaise(AlignmentProperty, ref _alignment, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("IsExpanded")]
    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetAndRaise(IsExpandedProperty, ref _isExpanded, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("AutoHide")]
    public bool AutoHide
    {
        get => _autoHide;
        set => SetAndRaise(AutoHideProperty, ref _autoHide, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("GripMode")]
    public GripMode GripMode
    {
        get => _gripMode;
        set => SetAndRaise(GripModeProperty, ref _gripMode, value);
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    [JsonIgnore]
    public IToolTemplate? ToolTemplate
    {
        get => GetValue(ToolTemplateProperty);
        set => SetValue(ToolTemplateProperty, value);
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    [JsonIgnore]
    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the generator used to create and prepare containers for ItemsSource items.
    /// </summary>
    [IgnoreDataMember]
    [JsonIgnore]
    public IDockItemContainerGenerator? ItemContainerGenerator
    {
        get => GetValue(ItemContainerGeneratorProperty);
        set => SetValue(ItemContainerGeneratorProperty, value);
    }

    /// <inheritdoc />
    [IgnoreDataMember]
    [JsonIgnore]
    public object? ToolItemContainerTheme
    {
        get => GetValue(ToolItemContainerThemeProperty);
        set => SetValue(ToolItemContainerThemeProperty, value);
    }

    /// <inheritdoc />
    [IgnoreDataMember]
    [JsonIgnore]
    public IToolItemTemplateSelector? ToolItemTemplateSelector
    {
        get => GetValue(ToolItemTemplateSelectorProperty);
        set => SetValue(ToolItemTemplateSelectorProperty, value);
    }

    /// <inheritdoc />
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("CanUpdateItemsSourceOnUnregister")]
    public bool? CanUpdateItemsSourceOnUnregister
    {
        get => GetValue(CanUpdateItemsSourceOnUnregisterProperty);
        set => SetValue(CanUpdateItemsSourceOnUnregisterProperty, value);
    }

    /// <summary>
    /// Creates new tool from template.
    /// </summary>
    /// <returns>The new tool instance.</returns>
    public virtual object? CreateToolFromTemplate()
    {
        if (ToolTemplate is null)
        {
            return null;
        }

        var tool = new Tool
        {
            Title = $"Tool{VisibleDockables?.Count ?? 0}",
            Content = ToolTemplate.Content
        };

        Factory?.AddDockable(this, tool);
        Factory?.SetActiveDockable(tool);
        Factory?.SetFocusedDockable(this, tool);

        return tool;
    }

    /// <summary>
    /// Adds the specified tool to this dock and makes it active and focused.
    /// </summary>
    /// <param name="tool">The tool to add.</param>
    public virtual void AddTool(IDockable tool)
    {
        Factory?.AddDockable(this, tool);
        Factory?.SetActiveDockable(tool);
        Factory?.SetFocusedDockable(this, tool);
    }

    /// <summary>
    /// Removes an item from the ItemsSource collection if it supports removal.
    /// This method is called by the factory when a tool generated from ItemsSource is closed.
    /// </summary>
    /// <param name="item">The item to remove from the ItemsSource collection.</param>
    /// <returns>True if the item was successfully removed, false otherwise.</returns>
    public virtual bool RemoveItemFromSource(object? item)
    {
        if (item == null)
        {
            return false;
        }

        if (!ShouldUpdateItemsSourceOnUnregister())
        {
            UntrackGeneratedTool(item);
            return false;
        }

        if (ItemsSource is IList list)
        {
            if (list.Contains(item))
            {
                try
                {
                    list.Remove(item);

                    // Non-notify sources will not raise collection changed events.
                    if (_currentCollectionChanged is null)
                    {
                        UntrackGeneratedTool(item);
                    }

                    return true;
                }
                catch (NotSupportedException)
                {
                }
                catch (InvalidOperationException)
                {
                }
            }
        }

        UntrackGeneratedTool(item);
        return false;
    }

    private bool ShouldUpdateItemsSourceOnUnregister()
    {
        return CanUpdateItemsSourceOnUnregister ?? DockSettings.UpdateItemsSourceOnUnregister;
    }

    /// <summary>
    /// Checks if a tool was generated from ItemsSource.
    /// </summary>
    /// <param name="tool">The tool to check.</param>
    /// <returns>True if the tool was generated from ItemsSource, false otherwise.</returns>
    public virtual bool IsToolFromItemsSource(IDockable tool)
    {
        return _generatedTools.Contains(tool);
    }

    private IDockItemContainerGenerator ResolveItemContainerGenerator()
    {
        return ItemContainerGenerator ?? DockItemContainerGenerator.Default;
    }

    private void OnItemContainerGeneratorChanged()
    {
        if (ItemsSource is null)
        {
            return;
        }

        RegenerateGeneratedTools(ItemsSource);
    }

    private void OnGeneratedItemPresentationChanged()
    {
        if (ItemsSource is null)
        {
            return;
        }

        RegenerateGeneratedTools(ItemsSource);
    }

    private void RegenerateGeneratedTools(IEnumerable itemsSource)
    {
        ClearGeneratedTools();

        var index = 0;
        foreach (var item in itemsSource)
        {
            AddToolFromItem(item, index);
            index++;
        }
    }

    private void OnItemsSourceChanged(IEnumerable? newItemsSource)
    {
        if (_currentCollectionChanged != null)
        {
            _currentCollectionChanged.CollectionChanged -= OnCollectionChanged;
            _currentCollectionChanged = null;
        }

        if (newItemsSource is INotifyCollectionChanged notifyCollection)
        {
            _currentCollectionChanged = notifyCollection;
            _currentCollectionChanged.CollectionChanged += OnCollectionChanged;
        }

        if (newItemsSource is null)
        {
            ClearGeneratedTools();
            return;
        }

        RegenerateGeneratedTools(newItemsSource);
    }

    private void OnToolTemplateChanged()
    {
        if (ItemsSource is null)
        {
            return;
        }

        RegenerateGeneratedTools(ItemsSource);
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (e.NewItems != null)
                {
                    var addIndex = e.NewStartingIndex;
                    var offset = 0;
                    foreach (var item in e.NewItems)
                    {
                        AddToolFromItem(item, addIndex >= 0 ? addIndex + offset : -1);
                        offset++;
                    }
                }
                break;

            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems != null)
                {
                    foreach (var item in e.OldItems)
                    {
                        RemoveToolFromItem(item);
                    }
                }
                break;

            case NotifyCollectionChangedAction.Replace:
                if (e.OldItems != null)
                {
                    foreach (var item in e.OldItems)
                    {
                        RemoveToolFromItem(item);
                    }
                }

                if (e.NewItems != null)
                {
                    var replaceIndex = e.NewStartingIndex;
                    var offset = 0;
                    foreach (var item in e.NewItems)
                    {
                        AddToolFromItem(item, replaceIndex >= 0 ? replaceIndex + offset : -1);
                        offset++;
                    }
                }
                break;

            case NotifyCollectionChangedAction.Reset:
                if (ItemsSource != null)
                {
                    RegenerateGeneratedTools(ItemsSource);
                }
                else
                {
                    ClearGeneratedTools();
                }
                break;
        }
    }

    private void AddToolFromItem(object? item, int index)
    {
        if (item == null)
        {
            return;
        }

        var generator = ResolveItemContainerGenerator();
        var tool = generator.CreateToolContainer(this, item, index);
        if (tool is null)
        {
            return;
        }

        if (tool is not ITool)
        {
            generator.ClearToolContainer(this, tool, item);
            return;
        }

        generator.PrepareToolContainer(this, tool, item, index);

        _generatedTools.Add(tool);
        _generatedToolGenerators[tool] = generator;
        TrackItemsSourceTool(tool, item);

        if (Factory != null)
        {
            AddTool(tool);
            return;
        }

        if (VisibleDockables == null)
        {
            VisibleDockables = new global::Avalonia.Collections.AvaloniaList<IDockable>();
        }

        VisibleDockables.Add(tool);

        if (VisibleDockables.Count == 1)
        {
            ActiveDockable = tool;
        }
    }

    private void RemoveToolFromItem(object? item)
    {
        if (item == null)
        {
            return;
        }

        var toolToRemove = FindGeneratedTool(item);

        if (toolToRemove != null)
        {
            _generatedTools.Remove(toolToRemove);
            UntrackItemsSourceTool(toolToRemove);
            ClearGeneratedToolContainer(toolToRemove, item);
            RemoveGeneratedToolFromVisibleDockables(toolToRemove);
        }
    }

    private void ClearGeneratedTools()
    {
        var toolsToRemove = _generatedTools.ToList();
        foreach (var tool in toolsToRemove)
        {
            UntrackItemsSourceTool(tool);
            ClearGeneratedToolContainer(tool, tool.Context);
            RemoveGeneratedToolFromVisibleDockables(tool);
        }

        _generatedTools.Clear();
        _generatedToolGenerators.Clear();
    }

    private void UntrackGeneratedTool(object item)
    {
        var generatedTool = FindGeneratedTool(item);

        if (generatedTool != null)
        {
            _generatedTools.Remove(generatedTool);
            UntrackItemsSourceTool(generatedTool);
            ClearGeneratedToolContainer(generatedTool, item);
        }
    }

    private IDockable? FindGeneratedTool(object item)
    {
        foreach (var generatedTool in _generatedTools)
        {
            if (IsMatchingContext(generatedTool.Context, item))
            {
                return generatedTool;
            }
        }

        return null;
    }

    private static bool IsMatchingContext(object? context, object? item)
    {
        if (ReferenceEquals(context, item))
        {
            return true;
        }

        return Equals(context, item);
    }

    private void ClearGeneratedToolContainer(IDockable tool, object? item)
    {
        if (_generatedToolGenerators.TryGetValue(tool, out var generator))
        {
            _generatedToolGenerators.Remove(tool);
            generator.ClearToolContainer(this, tool, item);
            return;
        }

        ResolveItemContainerGenerator().ClearToolContainer(this, tool, item);
    }

    private void RemoveGeneratedToolFromVisibleDockables(IDockable tool)
    {
        if (tool.Owner is IDock owner)
        {
            if (owner.VisibleDockables?.Remove(tool) == true && ReferenceEquals(owner.ActiveDockable, tool))
            {
                owner.ActiveDockable = owner.VisibleDockables.FirstOrDefault();
            }
            return;
        }

        if (VisibleDockables?.Remove(tool) == true && ReferenceEquals(ActiveDockable, tool))
        {
            ActiveDockable = VisibleDockables.FirstOrDefault();
        }
    }

    private void TrackItemsSourceTool(IDockable tool, object item)
    {
        if (Factory is global::Dock.Model.FactoryBase factoryBase)
        {
            factoryBase.TrackItemsSourceDockable(tool, this, item);
        }
    }

    private void UntrackItemsSourceTool(IDockable tool)
    {
        if (Factory is global::Dock.Model.FactoryBase factoryBase)
        {
            factoryBase.UntrackItemsSourceDockable(tool);
        }
    }
}
