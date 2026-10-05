using System;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Dock.Avalonia.Controls;
using ReactiveUI.Reactive;
using ReactiveUI.Avalonia.Reactive;

namespace DockCodeOnlySample;

public sealed class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    internal DockControl DockControl { get; }
    internal CheckBox DockingEnabledCheckBox { get; }
    internal TextBlock WorkspaceStatusTextBlock { get; }
    internal Button SaveWorkspaceAButton { get; }
    internal Button LoadWorkspaceAButton { get; }
    internal Button SaveWorkspaceBButton { get; }
    internal Button LoadWorkspaceBButton { get; }

    public MainWindow()
    {
        Title = "Dock Code-Only Sample";
        Width = 1000;
        Height = 720;
        MinWidth = 900;
        MinHeight = 600;

        SaveWorkspaceAButton = CreateToolbarButton("Save Workspace A");
        LoadWorkspaceAButton = CreateToolbarButton("Load Workspace A");
        SaveWorkspaceBButton = CreateToolbarButton("Save Workspace B");
        LoadWorkspaceBButton = CreateToolbarButton("Load Workspace B");

        DockingEnabledCheckBox = new CheckBox
        {
            Content = "Docking Enabled",
            VerticalAlignment = VerticalAlignment.Center
        };

        WorkspaceStatusTextBlock = new TextBlock
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
                SaveWorkspaceAButton,
                LoadWorkspaceAButton,
                SaveWorkspaceBButton,
                LoadWorkspaceBButton,
                DockingEnabledCheckBox,
                WorkspaceStatusTextBlock
            }
        };

        Border toolbar = new()
        {
            Padding = new Thickness(8),
            Child = toolbarPanel
        };

        DockControl = new DockControl
        {
            InitializeFactory = true,
            InitializeLayout = false
        };

        DockPanel root = new();
        DockPanel.SetDock(toolbar, Avalonia.Controls.Dock.Top);
        root.Children.Add(toolbar);
        root.Children.Add(DockControl);
        Content = root;

        this.WhenActivated(disposables =>
        {
            disposables.Add(this.BindCommand(ViewModel, vm => vm.SaveWorkspaceA, v => v.SaveWorkspaceAButton));
            disposables.Add(this.BindCommand(ViewModel, vm => vm.LoadWorkspaceA, v => v.LoadWorkspaceAButton));
            disposables.Add(this.BindCommand(ViewModel, vm => vm.SaveWorkspaceB, v => v.SaveWorkspaceBButton));
            disposables.Add(this.BindCommand(ViewModel, vm => vm.LoadWorkspaceB, v => v.LoadWorkspaceBButton));

            disposables.Add(this.Bind(ViewModel, vm => vm.IsDockingEnabled, v => v.DockingEnabledCheckBox.IsChecked));
            disposables.Add(this.Bind(ViewModel, vm => vm.IsDockingEnabled, v => v.DockControl.IsDockingEnabled));

            disposables.Add(this.OneWayBind(ViewModel, vm => vm.Factory, v => v.DockControl.Factory));
            disposables.Add(this.OneWayBind(ViewModel, vm => vm.Layout, v => v.DockControl.Layout));
            disposables.Add(this.OneWayBind(ViewModel, vm => vm.WorkspaceStatus, v => v.WorkspaceStatusTextBlock.Text));
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
