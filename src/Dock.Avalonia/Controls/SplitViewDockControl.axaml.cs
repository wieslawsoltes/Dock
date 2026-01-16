// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Interaction logic for <see cref="SplitViewDockControl"/> xaml.
/// </summary>
public class SplitViewDockControl : TemplatedControl
{
    private SplitView? _splitView;
    private bool _isPaneOpenChanging;

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_splitView != null)
        {
            _splitView.PaneClosed -= OnPaneClosed;
            _splitView.PaneOpened -= OnPaneOpened;
        }

        _splitView = e.NameScope.Find<SplitView>("PART_SplitView");

        if (_splitView != null)
        {
            _splitView.PaneClosed += OnPaneClosed;
            _splitView.PaneOpened += OnPaneOpened;
        }
    }

    private void OnPaneClosed(object? sender, RoutedEventArgs e)
    {
        if (_isPaneOpenChanging)
        {
            return;
        }

        if (DataContext is ISplitViewDock splitViewDock)
        {
            // If light dismiss is disabled and the model still thinks the pane is open,
            // this was a light dismiss attempt - prevent it by re-opening
            if (!splitViewDock.UseLightDismissOverlayMode && 
                splitViewDock.IsPaneOpen &&
                (splitViewDock.DisplayMode == Model.Core.SplitViewDisplayMode.Overlay ||
                 splitViewDock.DisplayMode == Model.Core.SplitViewDisplayMode.CompactOverlay))
            {
                _isPaneOpenChanging = true;
                try
                {
                    if (_splitView != null)
                    {
                        _splitView.IsPaneOpen = true;
                    }
                }
                finally
                {
                    _isPaneOpenChanging = false;
                }
            }
            else
            {
                splitViewDock.IsPaneOpen = false;
            }
        }
    }

    private void OnPaneOpened(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ISplitViewDock splitViewDock)
        {
            splitViewDock.IsPaneOpen = true;
        }
    }
}
