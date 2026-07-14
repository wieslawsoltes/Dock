// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Settings;

namespace Dock.Avalonia.Controls;

internal sealed class FlatProportionalDockSurface : DockableControl
{
    private IProportionalDock? _dock;
    private INotifyPropertyChanged? _propertyChanged;

    public void SetDock(IProportionalDock? dock)
    {
        if (ReferenceEquals(_dock, dock))
        {
            return;
        }

        if (_propertyChanged is not null)
        {
            _propertyChanged.PropertyChanged -= OnDockPropertyChanged;
        }

        _dock = dock;
        _propertyChanged = dock as INotifyPropertyChanged;

        if (_propertyChanged is not null)
        {
            _propertyChanged.PropertyChanged += OnDockPropertyChanged;
        }

        DataContext = dock;
        UpdateDropProperties();
    }

    private void OnDockPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.PropertyName)
            || e.PropertyName == nameof(IDockable.CanDrop)
            || e.PropertyName == nameof(IDockable.DockGroup))
        {
            UpdateDropProperties();
        }
    }

    private void UpdateDropProperties()
    {
        DockProperties.SetIsDropEnabled(this, _dock?.CanDrop ?? false);
        DockProperties.SetDockGroup(this, _dock?.DockGroup);
    }
}
