using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
using Dock.Model.Controls;
using Dock.Model.Core;
using DockOverlayReactiveUISample.Models;
using ReactiveUI;

namespace DockOverlayReactiveUISample.ViewModels;

[RequiresUnreferencedCode("Requires unreferenced code for ReactiveCommand.")]
[RequiresDynamicCode("Requires dynamic code for ReactiveCommand.")]
public class MainWindowViewModel : ReactiveObject
{
    private OverlayDockFactory? _factory;
    private ScenarioDefinition? _selectedScenario;
    private IRootDock? _layout;
    private IFactory? _layoutFactory;
    private bool _dark;

    public ObservableCollection<ScenarioDefinition> Scenarios { get; }

    public ScenarioDefinition? SelectedScenario
    {
        get => _selectedScenario;
        set => this.RaiseAndSetIfChanged(ref _selectedScenario, value);
    }

    public IRootDock? Layout
    {
        get => _layout;
        private set => this.RaiseAndSetIfChanged(ref _layout, value);
    }

    public IFactory? LayoutFactory
    {
        get => _layoutFactory;
        private set => this.RaiseAndSetIfChanged(ref _layoutFactory, value);
    }

    public ICommand LoadScenarioCommand { get; }

    public ICommand ResetScenarioCommand { get; }

    public ICommand CloseLayoutCommand { get; }

    public ICommand ToggleThemeCommand { get; }

    public MainWindowViewModel()
    {
        Scenarios = new ObservableCollection<ScenarioDefinition>
        {
            new(Scenario.VideoEditor, "Video Editor", "Full-size preview with floating timeline, media bin, and inspector."),
            new(Scenario.GameLevelEditor, "Game Level Editor", "Viewport background with hierarchy, properties, and asset browser panels."),
            new(Scenario.Dashboard, "Dashboard", "Widgets as free-floating overlays on top of a shared dashboard surface."),
            new(Scenario.Interop, "Interop: Groups + Global Docking", "Panels use different DockGroup values with global docking disabled on the overlay root.")
        };

        SelectedScenario = Scenarios[0];

        LoadScenarioCommand = ReactiveCommand.Create(LoadLayout);
        ResetScenarioCommand = ReactiveCommand.Create(ResetLayout);
        CloseLayoutCommand = ReactiveCommand.Create(CloseLayout);
        ToggleThemeCommand = ReactiveCommand.Create(ToggleTheme);

        LoadLayout();
    }

    public void CloseLayout()
    {
        if (Layout is IDock dock && dock.Close.CanExecute(null))
        {
            dock.Close.Execute(null);
        }

        Layout = null;
        LayoutFactory = null;
    }

    private void LoadLayout()
    {
        if (SelectedScenario is null)
        {
            return;
        }

        CloseLayout();

        _factory = new OverlayDockFactory(SelectedScenario.Scenario, SelectedScenario.Title);

        var layout = _factory.CreateLayout();
        layout.Factory = _factory;

        LayoutFactory = _factory;
        Layout = layout;
    }

    private void ResetLayout() => LoadLayout();

    private void ToggleTheme()
    {
        _dark = !_dark;
        // Toggle between Fluent (0) and Simple (1) dock themes.
        App.ThemeManager?.Switch(_dark ? 1 : 0);
    }
}
