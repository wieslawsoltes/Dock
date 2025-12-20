// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm.Controls;

namespace DockSplitViewSample.ViewModels;

/// <summary>
/// Settings page document view model.
/// </summary>
public partial class SettingsViewModel : Document
{
    [ObservableProperty]
    private ISplitViewDock? _splitViewDock;

    public IReadOnlyList<SplitViewDisplayMode> DisplayModes { get; } = Enum.GetValues<SplitViewDisplayMode>();
    
    public IReadOnlyList<SplitViewPanePlacement> PanePlacements { get; } = Enum.GetValues<SplitViewPanePlacement>();
}
