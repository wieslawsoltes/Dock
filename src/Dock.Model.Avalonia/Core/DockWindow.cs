﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Metadata;
using Dock.Model.Adapters;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model.Avalonia.Core;

/// <summary>
/// Dock window.
/// </summary>
[DataContract(IsReference = true)]
public class DockWindow : ReactiveBase, IDockWindow
{
    /// <summary>
    /// Defines the <see cref="Id"/> property.
    /// </summary>
    public static readonly DirectProperty<DockWindow, string> IdProperty =
        AvaloniaProperty.RegisterDirect<DockWindow, string>(nameof(Id), o => o.Id, (o, v) => o.Id = v);

    /// <summary>
    /// Defines the <see cref="X"/> property.
    /// </summary>
    public static readonly DirectProperty<DockWindow, double> XProperty =
        AvaloniaProperty.RegisterDirect<DockWindow, double>(nameof(X), o => o.X, (o, v) => o.X = v);

    /// <summary>
    /// Defines the <see cref="Y"/> property.
    /// </summary>
    public static readonly DirectProperty<DockWindow, double> YProperty =
        AvaloniaProperty.RegisterDirect<DockWindow, double>(nameof(Y), o => o.Y, (o, v) => o.Y = v);

    /// <summary>
    /// Defines the <see cref="Width"/> property.
    /// </summary>
    public static readonly DirectProperty<DockWindow, double> WidthProperty =
        AvaloniaProperty.RegisterDirect<DockWindow, double>(nameof(Width), o => o.Width, (o, v) => o.Width = v);

    /// <summary>
    /// Defines the <see cref="Height"/> property.
    /// </summary>
    public static readonly DirectProperty<DockWindow, double> HeightProperty =
        AvaloniaProperty.RegisterDirect<DockWindow, double>(nameof(Height), o => o.Height, (o, v) => o.Height = v);

    /// <summary>
    /// Defines the <see cref="Topmost"/> property.
    /// </summary>
    public static readonly DirectProperty<DockWindow, bool> TopmostProperty =
        AvaloniaProperty.RegisterDirect<DockWindow, bool>(nameof(Topmost), o => o.Topmost, (o, v) => o.Topmost = v);

    /// <summary>
    /// Defines the <see cref="Title"/> property.
    /// </summary>
    public static readonly DirectProperty<DockWindow, string> TitleProperty =
        AvaloniaProperty.RegisterDirect<DockWindow, string>(nameof(Title), o => o.Title, (o, v) => o.Title = v);

    /// <summary>
    /// Defines the <see cref="FadeOnInactive"/> property.
    /// </summary>
    public static readonly DirectProperty<DockWindow, bool> FadeOnInactiveProperty =
        AvaloniaProperty.RegisterDirect<DockWindow, bool>(nameof(FadeOnInactive), o => o.FadeOnInactive, (o, v) => o.FadeOnInactive = v);

    /// <summary>
    /// Defines the <see cref="CloseOnFadeOut"/> property.
    /// </summary>
    public static readonly DirectProperty<DockWindow, bool> CloseOnFadeOutProperty =
        AvaloniaProperty.RegisterDirect<DockWindow, bool>(nameof(CloseOnFadeOut), o => o.CloseOnFadeOut, (o, v) => o.CloseOnFadeOut = v);

    /// <summary>
    /// Defines the <see cref="Owner"/> property.
    /// </summary>
    public static readonly DirectProperty<DockWindow, IDockable?> OwnerProperty =
        AvaloniaProperty.RegisterDirect<DockWindow, IDockable?>(nameof(Owner), o => o.Owner, (o, v) => o.Owner = v);

    /// <summary>
    /// Defines the <see cref="Factory"/> property.
    /// </summary>
    public static readonly DirectProperty<DockWindow, IFactory?> FactoryProperty =
        AvaloniaProperty.RegisterDirect<DockWindow, IFactory?>(nameof(Factory), o => o.Factory, (o, v) => o.Factory = v);

    /// <summary>
    /// Defines the <see cref="Layout"/> property.
    /// </summary>
    public static readonly DirectProperty<DockWindow, IRootDock?> LayoutProperty =
        AvaloniaProperty.RegisterDirect<DockWindow, IRootDock?>(nameof(Layout), o => o.Layout, (o, v) => o.Layout = v);

    /// <summary>
    /// Defines the <see cref="Host"/> property.
    /// </summary>
    public static readonly DirectProperty<DockWindow, IHostWindow?> HostProperty =
        AvaloniaProperty.RegisterDirect<DockWindow, IHostWindow?>(nameof(Host), o => o.Host, (o, v) => o.Host = v);

    private readonly IHostAdapter _hostAdapter;
    private string _id;
    private double _x;
    private double _y;
    private double _width;
    private double _height;
    private bool _topmost;
    private string _title;
    private bool _fadeOnInactive;
    private bool _closeOnFadeOut;
    private IDockable? _owner;
    private IFactory? _factory;
    private IRootDock? _layout;
    private IHostWindow? _host;

    /// <summary>
    /// Initializes new instance of the <see cref="DockWindow"/> class.
    /// </summary>
    public DockWindow()
    {
        _id = nameof(IDockWindow);
        _title = nameof(IDockWindow);
        _hostAdapter = new HostAdapter(this);
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
    [DataMember(IsRequired = true, EmitDefaultValue = true)]
    [JsonPropertyName("X")]
    public double X
    {
        get => _x;
        set => SetAndRaise(XProperty, ref _x, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = true, EmitDefaultValue = true)]
    [JsonPropertyName("Y")]
    public double Y
    {
        get => _y;
        set => SetAndRaise(YProperty, ref _y, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = true, EmitDefaultValue = true)]
    [JsonPropertyName("Width")]
    public double Width
    {
        get => _width;
        set => SetAndRaise(WidthProperty, ref _width, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = true, EmitDefaultValue = true)]
    [JsonPropertyName("Height")]
    public double Height
    {
        get => _height;
        set => SetAndRaise(HeightProperty, ref _height, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("Topmost")]
    public bool Topmost
    {
        get => _topmost;
        set => SetAndRaise(TopmostProperty, ref _topmost, value);
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
    [JsonPropertyName("FadeOnInactive")]
    public bool FadeOnInactive
    {
        get => _fadeOnInactive;
        set => SetAndRaise(FadeOnInactiveProperty, ref _fadeOnInactive, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("CloseOnFadeOut")]
    public bool CloseOnFadeOut
    {
        get => _closeOnFadeOut;
        set => SetAndRaise(CloseOnFadeOutProperty, ref _closeOnFadeOut, value);
    }

    /// <inheritdoc/>
    [ResolveByName]
    [IgnoreDataMember]
    [JsonIgnore]
    public IDockable? Owner
    {
        get => _owner;
        set => SetAndRaise(OwnerProperty, ref _owner, value);
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
    [Content]
    [ResolveByName]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("Layout")]
    public IRootDock? Layout
    {
        get => _layout;
        set => SetAndRaise(LayoutProperty, ref _layout, value);
    }

    /// <inheritdoc/>
    [ResolveByName]
    [IgnoreDataMember]
    [JsonIgnore]
    public IHostWindow? Host
    {
        get => _host;
        set => SetAndRaise(HostProperty, ref _host, value);
    }

    /// <inheritdoc/>
    public virtual bool OnClose()
    {
        return true;
    }

    /// <inheritdoc/>
    public virtual bool OnMoveDragBegin()
    {
        return true;
    }

    /// <inheritdoc/>
    public virtual void OnMoveDrag()
    {
    }

    /// <inheritdoc/>
    public virtual void OnMoveDragEnd()
    {
    }

    /// <inheritdoc/>
    public void Save()
    {
        _hostAdapter.Save();
    }

    /// <inheritdoc/>
    public void Present(bool isDialog)
    {
        _hostAdapter.Present(isDialog);
    }

    /// <inheritdoc/>
    public void Exit()
    {
        _hostAdapter.Exit();
    }
}
