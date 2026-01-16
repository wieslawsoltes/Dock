// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace DockSplitViewSample.ViewModels;

public class MainWindowViewModel : ObservableObject
{
    private readonly DockFactory _factory;
    private IRootDock? _layout;
    private ISplitViewDock? _splitViewDock;

    public IRootDock? Layout
    {
        get => _layout;
        set => SetProperty(ref _layout, value);
    }

    public ISplitViewDock? SplitViewDock
    {
        get => _splitViewDock;
        set => SetProperty(ref _splitViewDock, value);
    }

    public ICommand NewLayoutCommand { get; }
    public ICommand TogglePaneCommand { get; }
    public ICommand NavigateCommand { get; }

    public MainWindowViewModel()
    {
        _factory = new DockFactory();

        var layout = _factory.CreateLayout();
        if (layout is not null)
        {
            _factory.InitLayout(layout);
        }
        Layout = layout;
        SplitViewDock = _factory.SplitViewDock;

        NewLayoutCommand = new RelayCommand(ResetLayout);
        TogglePaneCommand = new RelayCommand(TogglePane);
        NavigateCommand = new RelayCommand<string>(Navigate);
    }

    public void CloseLayout()
    {
        if (Layout is IDock dock)
        {
            if (dock.Close.CanExecute(null))
            {
                dock.Close.Execute(null);
            }
        }
    }

    public void ResetLayout()
    {
        if (Layout is not null)
        {
            if (Layout.Close.CanExecute(null))
            {
                Layout.Close.Execute(null);
            }
        }

        var layout = _factory.CreateLayout();
        if (layout is not null)
        {
            _factory.InitLayout(layout);
            Layout = layout;
        }
    }

    private void TogglePane()
    {
        if (SplitViewDock is not null)
        {
            SplitViewDock.IsPaneOpen = !SplitViewDock.IsPaneOpen;
        }
    }

    private void Navigate(string? pageId)
    {
        if (string.IsNullOrEmpty(pageId) || _factory.DocumentDock is null)
        {
            return;
        }

        var dockable = _factory.GetDockable(pageId);
        if (dockable is not null && _factory.DocumentDock.VisibleDockables is not null)
        {
            _factory.DocumentDock.ActiveDockable = dockable;
        }

        // Auto-close pane on navigation in overlay modes (only if light dismiss is enabled)
        if (SplitViewDock is not null && 
            SplitViewDock.UseLightDismissOverlayMode &&
            (SplitViewDock.DisplayMode == SplitViewDisplayMode.Overlay || 
             SplitViewDock.DisplayMode == SplitViewDisplayMode.CompactOverlay))
        {
            SplitViewDock.IsPaneOpen = false;
        }
    }
}
