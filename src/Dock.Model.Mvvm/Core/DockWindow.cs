/*
 * Dock A docking layout system.
 * Copyright (C) 2023  Wiesław Šoltés
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
using System.Runtime.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using Dock.Model.Adapters;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model.Mvvm.Core;

/// <summary>
/// Dock window.
/// </summary>
[DataContract(IsReference = true)]
public class DockWindow : ObservableObject, IDockWindow
{
    private readonly IHostAdapter _hostAdapter;
    private string _id;
    private double _x;
    private double _y;
    private double _width;
    private double _height;
    private bool _topmost;
    private string _title;
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
    public string Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = true, EmitDefaultValue = true)]
    public double X
    {
        get => _x;
        set => SetProperty(ref _x, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = true, EmitDefaultValue = true)]
    public double Y
    {
        get => _y;
        set => SetProperty(ref _y, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = true, EmitDefaultValue = true)]
    public double Width
    {
        get => _width;
        set => SetProperty(ref _width, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = true, EmitDefaultValue = true)]
    public double Height
    {
        get => _height;
        set => SetProperty(ref _height, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool Topmost
    {
        get => _topmost;
        set => SetProperty(ref _topmost, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public IDockable? Owner
    {
        get => _owner;
        set => SetProperty(ref _owner, value);
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public IFactory? Factory
    {
        get => _factory;
        set => SetProperty(ref _factory, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IRootDock? Layout
    {
        get => _layout;
        set => SetProperty(ref _layout, value);
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public IHostWindow? Host
    {
        get => _host;
        set => SetProperty(ref _host, value);
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
