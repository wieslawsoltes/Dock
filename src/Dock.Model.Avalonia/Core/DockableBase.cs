// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Avalonia;
using Avalonia.Controls;
using Dock.Model.Adapters;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;

namespace Dock.Model.Avalonia.Core;

/// <summary>
/// Dockable base class.
/// </summary>
[DataContract(IsReference = true)]
[JsonPolymorphic]
[JsonDerivedType(typeof(DockDock), typeDiscriminator: "DockDock")]
[JsonDerivedType(typeof(Document), typeDiscriminator: "Document")]
[JsonDerivedType(typeof(DocumentDock), typeDiscriminator: "DocumentDock")]
[JsonDerivedType(typeof(ProportionalDock), typeDiscriminator: "ProportionalDock")]
[JsonDerivedType(typeof(ProportionalDockSplitter), typeDiscriminator: "ProportionalDockSplitter")]
[JsonDerivedType(typeof(RootDock), typeDiscriminator: "RootDock")]
[JsonDerivedType(typeof(Tool), typeDiscriminator: "Tool")]
[JsonDerivedType(typeof(ToolDock), typeDiscriminator: "ToolDock")]
[JsonDerivedType(typeof(DockBase), typeDiscriminator: "DockBase")]
public abstract class DockableBase : ReactiveBase, IDockable
{
    /// <summary>
    /// Defines the <see cref="Id"/> property.
    /// </summary>
    public static readonly DirectProperty<DockableBase, string> IdProperty =
        AvaloniaProperty.RegisterDirect<DockableBase, string>(nameof(Id), o => o.Id, (o, v) => o.Id = v);

    /// <summary>
    /// Defines the <see cref="Title"/> property.
    /// </summary>
    public static readonly DirectProperty<DockableBase, string> TitleProperty =
        AvaloniaProperty.RegisterDirect<DockableBase, string>(nameof(Title), o => o.Title, (o, v) => o.Title = v);

    /// <summary>
    /// Defines the <see cref="Context"/> property.
    /// </summary>
    public static readonly DirectProperty<DockableBase, object?> ContextProperty =
        AvaloniaProperty.RegisterDirect<DockableBase, object?>(nameof(Context), o => o.Context, (o, v) => o.Context = v);

    /// <summary>
    /// Defines the <see cref="Owner"/> property.
    /// </summary>
    public static readonly DirectProperty<DockableBase, IDockable?> OwnerProperty =
        AvaloniaProperty.RegisterDirect<DockableBase, IDockable?>(nameof(Owner), o => o.Owner, (o, v) => o.Owner = v);

    /// <summary>
    /// Defines the <see cref="OriginalOwner"/> property.
    /// </summary>
    public static readonly DirectProperty<DockableBase, IDockable?> OriginalOwnerProperty =
        AvaloniaProperty.RegisterDirect<DockableBase, IDockable?>(nameof(OriginalOwner), o => o.OriginalOwner, (o, v) => o.OriginalOwner = v);

    /// <summary>
    /// Defines the <see cref="Factory"/> property.
    /// </summary>
    public static readonly DirectProperty<DockableBase, IFactory?> FactoryProperty =
        AvaloniaProperty.RegisterDirect<DockableBase, IFactory?>(nameof(Factory), o => o.Factory, (o, v) => o.Factory = v);

    /// <summary>
    /// Defines the <see cref="IsEmpty"/> property.
    /// </summary>
    public static readonly DirectProperty<DockableBase, bool> IsEmptyProperty =
        AvaloniaProperty.RegisterDirect<DockableBase, bool>(nameof(IsEmpty), o => o.IsEmpty, (o, v) => o.IsEmpty = v);

    /// <summary>
    /// Defines the <see cref="IsCollapsable"/> property.
    /// </summary>
    public static readonly DirectProperty<DockableBase, bool> IsCollapsableProperty =
        AvaloniaProperty.RegisterDirect<DockableBase, bool>(nameof(IsCollapsable), o => o.IsCollapsable, (o, v) => o.IsCollapsable = v, true);

    /// <summary>
    /// Defines the <see cref="Proportion"/> property.
    /// </summary>
    public static readonly DirectProperty<DockableBase, double> ProportionProperty =
        AvaloniaProperty.RegisterDirect<DockableBase, double>(nameof(Proportion), o => o.Proportion, (o, v) => o.Proportion = v, double.NaN);

    /// <summary>
    /// Defines the <see cref="Dock"/> property.
    /// </summary>
    public static readonly DirectProperty<DockableBase, DockMode> DockProperty =
        AvaloniaProperty.RegisterDirect<DockableBase, DockMode>(nameof(Dock), o => o.Dock, (o, v) => o.Dock = v);

    /// <summary>
    /// Defines the <see cref="Column"/> property.
    /// </summary>
    public static readonly DirectProperty<DockableBase, int> ColumnProperty =
        AvaloniaProperty.RegisterDirect<DockableBase, int>(nameof(Column), o => o.Column, (o, v) => o.Column = v);

    /// <summary>
    /// Defines the <see cref="Row"/> property.
    /// </summary>
    public static readonly DirectProperty<DockableBase, int> RowProperty =
        AvaloniaProperty.RegisterDirect<DockableBase, int>(nameof(Row), o => o.Row, (o, v) => o.Row = v);

    /// <summary>
    /// Defines the <see cref="ColumnSpan"/> property.
    /// </summary>
    public static readonly DirectProperty<DockableBase, int> ColumnSpanProperty =
        AvaloniaProperty.RegisterDirect<DockableBase, int>(nameof(ColumnSpan), o => o.ColumnSpan, (o, v) => o.ColumnSpan = v, 1);

    /// <summary>
    /// Defines the <see cref="RowSpan"/> property.
    /// </summary>
    public static readonly DirectProperty<DockableBase, int> RowSpanProperty =
        AvaloniaProperty.RegisterDirect<DockableBase, int>(nameof(RowSpan), o => o.RowSpan, (o, v) => o.RowSpan = v, 1);

    /// <summary>
    /// Defines the <see cref="IsSharedSizeScope"/> property.
    /// </summary>
    public static readonly DirectProperty<DockableBase, bool> IsSharedSizeScopeProperty =
        AvaloniaProperty.RegisterDirect<DockableBase, bool>(nameof(IsSharedSizeScope), o => o.IsSharedSizeScope, (o, v) => o.IsSharedSizeScope = v);

    /// <summary>
    /// Defines the <see cref="CollapsedProportion"/> property.
    /// </summary>
    public static readonly DirectProperty<DockableBase, double> CollapsedProportionProperty =
        AvaloniaProperty.RegisterDirect<DockableBase, double>(nameof(CollapsedProportion), o => o.CollapsedProportion, (o, v) => o.CollapsedProportion = v, double.NaN);

    /// <summary>
    /// Defines the <see cref="CanClose"/> property.
    /// </summary>
    public static readonly DirectProperty<DockableBase, bool> CanCloseProperty =
        AvaloniaProperty.RegisterDirect<DockableBase, bool>(nameof(CanClose), o => o.CanClose, (o, v) => o.CanClose = v);

    /// <summary>
    /// Defines the <see cref="CanPin"/> property.
    /// </summary>
    public static readonly DirectProperty<DockableBase, bool> CanPinProperty =
        AvaloniaProperty.RegisterDirect<DockableBase, bool>(nameof(CanPin), o => o.CanPin, (o, v) => o.CanPin = v);

    /// <summary>
    /// Defines the <see cref="CanFloat"/> property.
    /// </summary>
    public static readonly DirectProperty<DockableBase, bool> CanFloatProperty =
        AvaloniaProperty.RegisterDirect<DockableBase, bool>(nameof(CanFloat), o => o.CanFloat, (o, v) => o.CanFloat = v);

    /// <summary>
    /// Defines the <see cref="CanDrag"/> property.
    /// </summary>
    public static readonly DirectProperty<DockableBase, bool> CanDragProperty =
        AvaloniaProperty.RegisterDirect<DockableBase, bool>(nameof(CanDrag), o => o.CanDrag, (o, v) => o.CanDrag = v);

    /// <summary>
    /// Defines the <see cref="CanDrop"/> property.
    /// </summary>
    public static readonly DirectProperty<DockableBase, bool> CanDropProperty =
        AvaloniaProperty.RegisterDirect<DockableBase, bool>(nameof(CanDrop), o => o.CanDrop, (o, v) => o.CanDrop = v);

    /// <summary>
    /// Defines the <see cref="MinWidth"/> property.
    /// </summary>
    public static readonly DirectProperty<DockableBase, double> MinWidthProperty =
        AvaloniaProperty.RegisterDirect<DockableBase, double>(nameof(MinWidth), o => o.MinWidth, (o, v) => o.MinWidth = v, double.NaN);

    /// <summary>
    /// Defines the <see cref="MaxWidth"/> property.
    /// </summary>
    public static readonly DirectProperty<DockableBase, double> MaxWidthProperty =
        AvaloniaProperty.RegisterDirect<DockableBase, double>(nameof(MaxWidth), o => o.MaxWidth, (o, v) => o.MaxWidth = v, double.NaN);

    /// <summary>
    /// Defines the <see cref="MinHeight"/> property.
    /// </summary>
    public static readonly DirectProperty<DockableBase, double> MinHeightProperty =
        AvaloniaProperty.RegisterDirect<DockableBase, double>(nameof(MinHeight), o => o.MinHeight, (o, v) => o.MinHeight = v, double.NaN);

    /// <summary>
    /// Defines the <see cref="MaxHeight"/> property.
    /// </summary>
    public static readonly DirectProperty<DockableBase, double> MaxHeightProperty =
        AvaloniaProperty.RegisterDirect<DockableBase, double>(nameof(MaxHeight), o => o.MaxHeight, (o, v) => o.MaxHeight = v, double.NaN);

    private readonly TrackingAdapter _trackingAdapter;
    private string _id = string.Empty;
    private string _title = string.Empty;
    private object? _context;
    private IDockable? _owner;
    private IDockable? _originalOwner;
    private IFactory? _factory;
    private bool _isEmpty;
    private bool _isCollapsable = true;
    private double _proportion = double.NaN;
    private DockMode _dock = DockMode.Center;
    private int _column = 0;
    private int _row = 0;
    private int _columnSpan = 1;
    private int _rowSpan = 1;
    private bool _isSharedSizeScope;
    private double _collapsedProportion = double.NaN;
    private bool _canClose = true;
    private bool _canPin = true;
    private bool _canFloat = true;
    private bool _canDrag = true;
    private bool _canDrop = true;
    private double _minWidth = double.NaN;
    private double _maxWidth = double.NaN;
    private double _minHeight = double.NaN;
    private double _maxHeight = double.NaN;

    /// <summary>
    /// Initializes new instance of the <see cref="DockableBase"/> class.
    /// </summary>
    protected DockableBase()
    {
        _trackingAdapter = new TrackingAdapter();
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("Id")]
    public string Id
    {
        get => _id;
        set => SetAndRaise(IdProperty, ref _id, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("Title")]
    public string Title
    {
        get => _title;
        set => SetAndRaise(TitleProperty, ref _title, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("Context")]
    public object? Context
    {
        get => _context;
        set => SetAndRaise(ContextProperty, ref _context, value);
    }

    /// <inheritdoc/>
    [ResolveByName]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("Owner")]
    public IDockable? Owner
    {
        get => _owner;
        set => SetAndRaise(OwnerProperty, ref _owner, value);
    }

    /// <inheritdoc/>
    [ResolveByName]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("OriginalOwner")]
    public IDockable? OriginalOwner
    {
        get => _originalOwner;
        set => SetAndRaise(OriginalOwnerProperty, ref _originalOwner, value);
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    [JsonIgnore]
    public IFactory? Factory
    {
        get => _factory;
        set => SetAndRaise(FactoryProperty, ref _factory, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("IsEmpty")]
    public bool IsEmpty
    {
        get => _isEmpty;
        set => SetAndRaise(IsEmptyProperty, ref _isEmpty, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("IsCollapsable")]
    public bool IsCollapsable
    {
        get => _isCollapsable;
        set => SetAndRaise(IsCollapsableProperty, ref _isCollapsable, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("Proportion")]
    public double Proportion
    {
        get => _proportion;
        set => SetAndRaise(ProportionProperty, ref _proportion, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("Dock")]
    public DockMode Dock
    {
        get => _dock;
        set => SetAndRaise(DockProperty, ref _dock, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("Column")]
    public int Column
    {
        get => _column;
        set => SetAndRaise(ColumnProperty, ref _column, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("Row")]
    public int Row
    {
        get => _row;
        set => SetAndRaise(RowProperty, ref _row, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("ColumnSpan")]
    public int ColumnSpan
    {
        get => _columnSpan;
        set => SetAndRaise(ColumnSpanProperty, ref _columnSpan, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("RowSpan")]
    public int RowSpan
    {
        get => _rowSpan;
        set => SetAndRaise(RowSpanProperty, ref _rowSpan, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("IsSharedSizeScope")]
    public bool IsSharedSizeScope
    {
        get => _isSharedSizeScope;
        set => SetAndRaise(IsSharedSizeScopeProperty, ref _isSharedSizeScope, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("CollapsedProportion")]
    public double CollapsedProportion
    {
        get => _collapsedProportion;
        set => SetAndRaise(CollapsedProportionProperty, ref _collapsedProportion, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("MinWidth")]
    public double MinWidth
    {
        get => _minWidth;
        set => SetAndRaise(MinWidthProperty, ref _minWidth, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("MaxWidth")]
    public double MaxWidth
    {
        get => _maxWidth;
        set => SetAndRaise(MaxWidthProperty, ref _maxWidth, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("MinHeight")]
    public double MinHeight
    {
        get => _minHeight;
        set => SetAndRaise(MinHeightProperty, ref _minHeight, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("MaxHeight")]
    public double MaxHeight
    {
        get => _maxHeight;
        set => SetAndRaise(MaxHeightProperty, ref _maxHeight, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("CanClose")]
    public bool CanClose
    {
        get => _canClose;
        set => SetAndRaise(CanCloseProperty, ref _canClose, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("CanPin")]
    public bool CanPin
    {
        get => _canPin;
        set => SetAndRaise(CanPinProperty, ref _canPin, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("CanFloat")]
    public bool CanFloat
    {
        get => _canFloat;
        set => SetAndRaise(CanFloatProperty, ref _canFloat, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("CanDrag")]
    public bool CanDrag
    {
        get => _canDrag;
        set => SetAndRaise(CanDragProperty, ref _canDrag, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("CanDrop")]
    public bool CanDrop
    {
        get => _canDrop;
        set => SetAndRaise(CanDropProperty, ref _canDrop, value);
    }

    /// <inheritdoc/>
    public string? GetControlRecyclingId() => _id;

    /// <inheritdoc/>
    public virtual bool OnClose()
    {
        return true;
    }

    /// <inheritdoc/>
    public virtual void OnSelected()
    {
    }

    /// <inheritdoc/>
    public void GetVisibleBounds(out double x, out double y, out double width, out double height)
    {
        _trackingAdapter.GetVisibleBounds(out x, out y, out width, out height);
    }

    /// <inheritdoc/>
    public void SetVisibleBounds(double x, double y, double width, double height)
    {
        _trackingAdapter.SetVisibleBounds(x, y, width, height);
        OnVisibleBoundsChanged(x, y, width, height);
    }

    /// <inheritdoc/>
    public virtual void OnVisibleBoundsChanged(double x, double y, double width, double height)
    {
    }

    /// <inheritdoc/>
    public void GetPinnedBounds(out double x, out double y, out double width, out double height)
    {
        _trackingAdapter.GetPinnedBounds(out x, out y, out width, out height);
    }

    /// <inheritdoc/>
    public void SetPinnedBounds(double x, double y, double width, double height)
    {
        _trackingAdapter.SetPinnedBounds(x, y, width, height);
        OnPinnedBoundsChanged(x, y, width, height);
    }

    /// <inheritdoc/>
    public virtual void OnPinnedBoundsChanged(double x, double y, double width, double height)
    {
    }

    /// <inheritdoc/>
    public void GetTabBounds(out double x, out double y, out double width, out double height)
    {
        _trackingAdapter.GetTabBounds(out x, out y, out width, out height);
    }

    /// <inheritdoc/>
    public void SetTabBounds(double x, double y, double width, double height)
    {
        _trackingAdapter.SetTabBounds(x, y, width, height);
        OnTabBoundsChanged(x, y, width, height);
    }

    /// <inheritdoc/>
    public virtual void OnTabBoundsChanged(double x, double y, double width, double height)
    {
    }

    /// <inheritdoc/>
    public void GetPointerPosition(out double x, out double y)
    {
        _trackingAdapter.GetPointerPosition(out x, out y);
    }

    /// <inheritdoc/>
    public void SetPointerPosition(double x, double y)
    {
        _trackingAdapter.SetPointerPosition(x, y);
        OnPointerPositionChanged(x, y);
    }

    /// <inheritdoc/>
    public virtual void OnPointerPositionChanged(double x, double y)
    {
    }

    /// <inheritdoc/>
    public void GetPointerScreenPosition(out double x, out double y)
    {
        _trackingAdapter.GetPointerScreenPosition(out x, out y);
    }

    /// <inheritdoc/>
    public void SetPointerScreenPosition(double x, double y)
    {
        _trackingAdapter.SetPointerScreenPosition(x, y);
        OnPointerScreenPositionChanged(x, y);
    }

    /// <inheritdoc/>
    public virtual void OnPointerScreenPositionChanged(double x, double y)
    {
    }

    /// <inheritdoc/>
    public virtual void Accept(IDockableVisitor visitor, IDockable target, DragAction action, DockOperation op, bool execute)
    {
        switch (this)
        {
            case ITool tool:
                visitor.VisitTool(tool, target, action, op, execute);
                break;
            case IDocument document:
                visitor.VisitDocument(document, target, action, op, execute);
                break;
            case IDock dock:
                visitor.VisitDock(dock, target, action, op, execute);
                break;
        }
    }
}
