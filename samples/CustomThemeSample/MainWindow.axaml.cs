using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;

namespace CustomThemeSample;

public partial class MainWindow : Window
{
    private bool _dark;

    public MainWindow()
    {
        InitializeComponent();
        InitializeLayout();
        InitializeThemeSwitch();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void InitializeLayout()
    {
        if (this.FindControl<DockControl>("Dock") is { } dock)
        {
            var factory = new Factory();
            var documentDock = new DocumentDock
            {
                Id = "Documents",
                IsCollapsable = false
            };
            var document = new Document { Id = "Doc1", Title = "Document 1" };
            documentDock.VisibleDockables = factory.CreateList<IDockable>(document);
            documentDock.ActiveDockable = document;

            var root = factory.CreateRootDock();
            root.VisibleDockables = factory.CreateList<IDockable>(documentDock);
            root.DefaultDockable = documentDock;

            factory.InitLayout(root);
            dock.Factory = factory;
            dock.Layout = root;
        }
    }

    private void InitializeThemeSwitch()
    {
        if (this.FindControl<Button>("ThemeButton") is { } button)
        {
            button.Click += OnThemeButtonClick;
        }
    }

    private void OnThemeButtonClick(object? sender, RoutedEventArgs e)
    {
        _dark = !_dark;
        App.ThemeManager?.Switch(_dark ? 1 : 0);
    }
}
