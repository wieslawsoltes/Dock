using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Windows.Input;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Metadata;
using Dock.Model.Adapters;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Internal;
using Dock.Model.Core;

namespace Dock.Model.Avalonia.Core;

/// <summary>
/// Dock base class.
/// </summary>
[DataContract(IsReference = true)]
[JsonPolymorphic]
[JsonDerivedType(typeof(DockDock), typeDiscriminator: "DockDock")]
[JsonDerivedType(typeof(DocumentDock), typeDiscriminator: "DocumentDock")]
[JsonDerivedType(typeof(ProportionalDock), typeDiscriminator: "ProportionalDock")]
[JsonDerivedType(typeof(ProportionalDockSplitter), typeDiscriminator: "ProportionalDockSplitter")]
[JsonDerivedType(typeof(RootDock), typeDiscriminator: "RootDock")]
[JsonDerivedType(typeof(ToolDock), typeDiscriminator: "ToolDock")]
public abstract class DockBase : DockableBase, IDock
{
    /// <summary>
    /// Defines the <see cref="VisibleDockables"/> property.
    /// </summary>
    public static readonly DirectProperty<DockBase, IList<IDockable>?> VisibleDockablesProperty =
        AvaloniaProperty.RegisterDirect<DockBase, IList<IDockable>?>(nameof(VisibleDockables), o => o.VisibleDockables, (o, v) => o.VisibleDockables = v);

    /// <summary>
    /// Defines the <see cref="ActiveDockable"/> property.
    /// </summary>
    public static readonly DirectProperty<DockBase, IDockable?> ActiveDockableProperty =
        AvaloniaProperty.RegisterDirect<DockBase, IDockable?>(nameof(ActiveDockable), o => o.ActiveDockable, (o, v) => o.ActiveDockable = v);

    /// <summary>
    /// Defines the <see cref="DefaultDockable"/> property.
    /// </summary>
    public static readonly DirectProperty<DockBase, IDockable?> DefaultDockableProperty =
        AvaloniaProperty.RegisterDirect<DockBase, IDockable?>(nameof(DefaultDockable), o => o.DefaultDockable, (o, v) => o.DefaultDockable = v);

    /// <summary>
    /// Defines the <see cref="FocusedDockable"/> property.
    /// </summary>
    public static readonly DirectProperty<DockBase, IDockable?> FocusedDockableProperty =
        AvaloniaProperty.RegisterDirect<DockBase, IDockable?>(nameof(FocusedDockable), o => o.FocusedDockable, (o, v) => o.FocusedDockable = v);

    /// <summary>
    /// Defines the <see cref="Proportion"/> property.
    /// </summary>
    public static readonly DirectProperty<DockBase, double> ProportionProperty =
        AvaloniaProperty.RegisterDirect<DockBase, double>(nameof(Proportion), o => o.Proportion, (o, v) => o.Proportion = v, double.NaN);

    /// <summary>
    /// Defines the <see cref="Proportion"/> property.
    /// </summary>
    public static readonly DirectProperty<DockBase, DockMode> DockProperty =
        AvaloniaProperty.RegisterDirect<DockBase, DockMode>(nameof(Dock), o => o.Dock, (o, v) => o.Dock = v);

    /// <summary>
    /// Defines the <see cref="IsActive"/> property.
    /// </summary>
    public static readonly DirectProperty<DockBase, bool> IsActiveProperty =
        AvaloniaProperty.RegisterDirect<DockBase, bool>(nameof(IsActive), o => o.IsActive, (o, v) => o.IsActive = v);

    /// <summary>
    /// Defines the <see cref="IsCollapsable"/> property.
    /// </summary>
    public static readonly DirectProperty<DockBase, bool> IsCollapsableProperty =
        AvaloniaProperty.RegisterDirect<DockBase, bool>(nameof(IsCollapsable), o => o.IsCollapsable, (o, v) => o.IsCollapsable = v, true);

    /// <summary>
    /// Defines the <see cref="CanGoBack"/> property.
    /// </summary>
    public static readonly DirectProperty<DockBase, bool> CanGoBackProperty =
        AvaloniaProperty.RegisterDirect<DockBase, bool>(nameof(CanGoBack), (o) => o.CanGoBack);

    /// <summary>
    /// Defines the <see cref="CanGoForward"/> property.
    /// </summary>
    public static readonly DirectProperty<DockBase, bool> CanGoForwardProperty =
        AvaloniaProperty.RegisterDirect<DockBase, bool>(nameof(CanGoForward), (o) => o.CanGoForward);

    internal INavigateAdapter _navigateAdapter;
    private IList<IDockable>? _visibleDockables;
    private IDockable? _activeDockable;
    private IDockable? _defaultDockable;
    private IDockable? _focusedDockable;
    private double _proportion = double.NaN;
    private DockMode _dock = DockMode.Center;
    private bool _isActive;
    private bool _isCollapsable = true;
    private bool _canGoBack;
    private bool _canGoForward;

    /// <summary>
    /// Initializes new instance of the <see cref="DockBase"/> class.
    /// </summary>
    public DockBase()
    {
        _navigateAdapter = new NavigateAdapter(this);
        _visibleDockables = new AvaloniaList<IDockable>();
        GoBack = Command.Create(() => _navigateAdapter.GoBack());
        GoForward = Command.Create(() => _navigateAdapter.GoForward());
        Navigate = Command.Create<object>(root => _navigateAdapter.Navigate(root, true));
        Close = Command.Create(() => _navigateAdapter.Close());
    }

    /// <inheritdoc/>
    [Content]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("VisibleDockables")]
    public IList<IDockable>? VisibleDockables
    {
        get => _visibleDockables;
        set => SetAndRaise(VisibleDockablesProperty, ref _visibleDockables, value);
    }

    /// <inheritdoc/>
    [ResolveByName]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("ActiveDockable")]
    public IDockable? ActiveDockable
    {
        get => _activeDockable;
        set
        {
            SetAndRaise(ActiveDockableProperty, ref _activeDockable, value);
            Factory?.InitActiveDockable(value, this);
            SetAndRaise(CanGoBackProperty, ref _canGoBack, _navigateAdapter?.CanGoBack ?? false);
            SetAndRaise(CanGoForwardProperty, ref _canGoForward, _navigateAdapter?.CanGoForward ?? false);
        }
    }

    /// <inheritdoc/>
    [ResolveByName]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("DefaultDockable")]
    public IDockable? DefaultDockable
    {
        get => _defaultDockable;
        set => SetAndRaise(DefaultDockableProperty, ref _defaultDockable, value);
    }

    /// <inheritdoc/>
    [ResolveByName]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("FocusedDockable")]
    public IDockable? FocusedDockable
    {
        get => _focusedDockable;
        set
        {
            SetAndRaise(FocusedDockableProperty, ref _focusedDockable, value);
            Factory?.OnFocusedDockableChanged(value);
        }
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
    [JsonPropertyName("IsActive")]
    public bool IsActive
    {
        get => _isActive;
        set => SetAndRaise(IsActiveProperty, ref _isActive, value);
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
    [IgnoreDataMember]
    [JsonIgnore]
    public bool CanGoBack
    {
        get => _navigateAdapter?.CanGoBack ?? false;
        private set => SetAndRaise(CanGoBackProperty, ref _canGoBack, value);
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    [JsonIgnore]
    public bool CanGoForward
    {
        get => _navigateAdapter?.CanGoForward ?? false;
        private set => SetAndRaise(CanGoForwardProperty, ref _canGoForward, value);
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    [JsonIgnore]
    public ICommand GoBack { get; }

    /// <inheritdoc/>
    [IgnoreDataMember]
    [JsonIgnore]
    public ICommand GoForward { get; }

    /// <inheritdoc/>
    [IgnoreDataMember]
    [JsonIgnore]
    public ICommand Navigate { get; }

    /// <inheritdoc/>
    [IgnoreDataMember]
    [JsonIgnore]
    public ICommand Close { get; }
}
