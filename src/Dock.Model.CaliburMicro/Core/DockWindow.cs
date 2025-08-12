// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Runtime.Serialization;
using Dock.Model.Adapters;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model.CaliburMicro.Core;

/// <summary>
/// Dock window.
/// </summary>
[DataContract(IsReference = true)]
public class DockWindow : CaliburMicroBase, IDockWindow
{
    private readonly IHostAdapter _hostAdapter;
    private string _id = nameof(IDockWindow);
    private double _x;
    private double _y;
    private double _width;
    private double _height;
    private bool _topmost;
    private string _title = nameof(IDockWindow);
    private IDockable? _owner;
    private IFactory? _factory;
    private IRootDock? _layout;
    private IHostWindow? _host;

    /// <summary>
    /// Initializes new instance of the <see cref="DockWindow"/> class.
    /// </summary>
    public DockWindow()
    {
        _hostAdapter = new HostAdapter(this);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public string Id
    {
        get => _id;
        set => Set(ref _id, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = true, EmitDefaultValue = true)]
    public double X
    {
        get => _x;
        set => Set(ref _x, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = true, EmitDefaultValue = true)]
    public double Y
    {
        get => _y;
        set => Set(ref _y, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = true, EmitDefaultValue = true)]
    public double Width
    {
        get => _width;
        set => Set(ref _width, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = true, EmitDefaultValue = true)]
    public double Height
    {
        get => _height;
        set => Set(ref _height, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool Topmost
    {
        get => _topmost;
        set => Set(ref _topmost, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public string Title
    {
        get => _title;
        set => Set(ref _title, value);
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public IDockable? Owner
    {
        get => _owner;
        set => Set(ref _owner, value);
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public IFactory? Factory
    {
        get => _factory;
        set => Set(ref _factory, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IRootDock? Layout
    {
        get => _layout;
        set => Set(ref _layout, value);
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public IHostWindow? Host
    {
        get => _host;
        set => Set(ref _host, value);
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

    /// <inheritdoc/>
    public void SetActive()
    {
        _hostAdapter.SetActive();
    }
}