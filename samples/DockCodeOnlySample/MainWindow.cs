using System;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Dock.Avalonia.Controls;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace DockCodeOnlySample;

public sealed class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    private readonly DockControl _dockControl;
    private readonly CheckBox _dockingEnabledCheckBox;
    private readonly TextBlock _workspaceStatusTextBlock;
    private readonly Button _saveWorkspaceAButton;
    private readonly Button _loadWorkspaceAButton;
    private readonly Button _saveWorkspaceBButton;
    private readonly Button _loadWorkspaceBButton;

    public MainWindow()
    {
        Title = "Dock Code-Only Sample";
        Width = 1000;
        Height = 720;
        MinWidth = 900;
        MinHeight = 600;

        _saveWorkspaceAButton = CreateToolbarButton("Save Workspace A");
        _loadWorkspaceAButton = CreateToolbarButton("Load Workspace A");
        _saveWorkspaceBButton = CreateToolbarButton("Save Workspace B");
        _loadWorkspaceBButton = CreateToolbarButton("Load Workspace B");

        _dockingEnabledCheckBox = new CheckBox
        {
            Content = "Docking Enabled",
            VerticalAlignment = VerticalAlignment.Center
        };

        _workspaceStatusTextBlock = new TextBlock
        {
            Margin = new Thickness(16, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center
        };

        StackPanel toolbarPanel = new()
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                _saveWorkspaceAButton,
                _loadWorkspaceAButton,
                _saveWorkspaceBButton,
                _loadWorkspaceBButton,
                _dockingEnabledCheckBox,
                _workspaceStatusTextBlock
            }
        };

        Border toolbar = new()
        {
            Padding = new Thickness(8),
            Child = toolbarPanel
        };

        _dockControl = new DockControl
        {
            InitializeFactory = true,
            InitializeLayout = false
        };

        DockPanel root = new();
        DockPanel.SetDock(toolbar, Avalonia.Controls.Dock.Top);
        root.Children.Add(toolbar);
        root.Children.Add(_dockControl);
        Content = root;

        this.WhenActivated(disposables =>
        {
            disposables.Add(this.BindCommand(ViewModel, vm => vm.SaveWorkspaceA, v => v._saveWorkspaceAButton));
            disposables.Add(this.BindCommand(ViewModel, vm => vm.LoadWorkspaceA, v => v._loadWorkspaceAButton));
            disposables.Add(this.BindCommand(ViewModel, vm => vm.SaveWorkspaceB, v => v._saveWorkspaceBButton));
            disposables.Add(this.BindCommand(ViewModel, vm => vm.LoadWorkspaceB, v => v._loadWorkspaceBButton));

            disposables.Add(this.Bind(ViewModel, vm => vm.IsDockingEnabled, v => v._dockingEnabledCheckBox.IsChecked));
            disposables.Add(this.Bind(ViewModel, vm => vm.IsDockingEnabled, v => v._dockControl.IsDockingEnabled));

            disposables.Add(this.OneWayBind(ViewModel, vm => vm.Factory, v => v._dockControl.Factory));
            disposables.Add(this.OneWayBind(ViewModel, vm => vm.Layout, v => v._dockControl.Layout));
            disposables.Add(this.OneWayBind(ViewModel, vm => vm.WorkspaceStatus, v => v._workspaceStatusTextBlock.Text));
        });
    }

    protected override void OnClosed(EventArgs e)
    {
        ViewModel?.CloseLayout();
        base.OnClosed(e);
    }

    private static Button CreateToolbarButton(string text)
    {
        return new Button
        {
            Content = text,
            MinWidth = 132
        };
    }
}
