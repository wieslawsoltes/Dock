# Dock Prism Integration

Dock includes a Prism-oriented model that mirrors the functionality exposed for the default MVVM pipeline while embracing Prism conventions such as `BindableBase` and `DelegateCommand`. The following sections describe the additional requirements, initialization patterns, and code samples needed to wire Dock with Prism.

## Requirements

- Reference the `Dock.Model.Prism` project or package in your Prism application.
  - Samples reference the project directly from `src/Dock.Model.Prism/Dock.Model.Prism.csproj`.
  - When using the NuGet packages, add `Dock.Model.Prism` and Prism's `Prism.Core` (implicitly pulled by the `build/Prism.props` import).
- Import the shared Dock Avalonia infrastructure used by all samples: `Dock.Model`, `Dock.Avalonia`, `Dock.Avalonia.Themes.Fluent`, and the diagnostics assemblies when needed.
- In multi-targeted solutions, ensure the Prism props file (`build/Prism.props`) is imported so the `Prism.Core` package version stays aligned with the repository.

## Prism Bootstrapping

Dock does not replace the Prism bootstrapper. Prism's typical `App` initialization remains the entry point and Dock components behave like any other view model graph.

```csharp
// Program.cs
public static AppBuilder BuildAvaloniaApp() =>
    AppBuilder.Configure<App>()
        .UsePlatformDetect()
        .WithInterFont()
        .LogToTrace();
```

Within the Prism-aware application, instantiate the Dock layout in your `Application` subclass. The sample (`DockPrismSample`) performs the initialization in `App.axaml.cs`:

```csharp
public override void OnFrameworkInitializationCompleted()
{
    var mainWindowViewModel = new MainWindowViewModel();

    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    {
        var mainWindow = new MainWindow { DataContext = mainWindowViewModel };
        desktop.MainWindow = mainWindow;
    }

    base.OnFrameworkInitializationCompleted();
}
```

Dock view models live in Prism view model classes and are resolved the same way as any other Prism component: you can register factories in Prism's container or instantiate them manually as shown above. The sample keeps the construction simple to focus on the docking behavior.

## View Model Integration

Use Prism types in place of the CommunityToolkit equivalents that the MVVM sample references:

- Derive view models from `Prism.Mvvm.BindableBase` (or a custom subclass) to get property change notifications.
- Use `Prism.Commands.DelegateCommand` for Dock command bindings such as layout reset or document creation.

```csharp
// MainWindowViewModel.cs
public class MainWindowViewModel : BindableBase
{
    private readonly IEventAggregator _eventAggregator;
    private readonly IFactory? _factory;
    private IRootDock? _layout;

    public IRootDock? Layout
    {
        get => _layout;
        set => SetProperty(ref _layout, value);
    }

    public ICommand NewLayout { get; }

    public MainWindowViewModel()
    {
        _eventAggregator = new EventAggregator();
        _factory = new DockFactory(new DemoData(), _eventAggregator);

        var layout = _factory?.CreateLayout();
        if (layout is not null)
        {
            _factory?.InitLayout(layout);
        }

        Layout = layout;
        if (Layout is { } root)
        {
            root.Navigate.Execute("Home");
        }

        NewLayout = new DelegateCommand(ResetLayout);
    }

    private void ResetLayout()
    {
        if (Layout?.Close.CanExecute(null) == true)
        {
            Layout.Close.Execute(null);
        }

        var layout = _factory?.CreateLayout();
        if (layout is not null)
        {
            _factory.InitLayout(layout);
            Layout = layout;
        }
    }
}
```

The Dock factory continues to derive from `Dock.Model.Prism.Factory`, which supplies Prism-aware base classes for dockables. Dockables created in this factory automatically inherit Prism features like `BindableBase`.

```csharp
// DockFactory.cs
public class DockFactory : Factory
{
    private readonly object _context;
    private readonly IEventAggregator _eventAggregator;

    public DockFactory(object context, IEventAggregator eventAggregator)
    {
        _context = context;
        _eventAggregator = eventAggregator;
    }

    public override IDocumentDock CreateDocumentDock() => new CustomDocumentDock(_eventAggregator);

    public override IRootDock CreateLayout()
    {
        var document = new DocumentViewModel(_eventAggregator) { Id = "Document1", Title = "Document1" };

        var documentDock = new CustomDocumentDock(_eventAggregator)
        {
            ActiveDockable = document,
            VisibleDockables = CreateList<IDockable>(document),
            CanCreateDocument = true
        };

        var root = CreateRootDock();
        root.ActiveDockable = documentDock;
        root.VisibleDockables = CreateList<IDockable>(documentDock);
        return root;
    }
}
```

## Custom Commands and Documents

Prism's command infrastructure makes it straightforward to wire Dock actions:

```csharp
// CustomDocumentDock.cs
public class CustomDocumentDock : DocumentDock
{
    private readonly IEventAggregator _eventAggregator;

    public CustomDocumentDock(IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;
        CreateDocument = new DelegateCommand(CreateNewDocument);
    }

    private void CreateNewDocument()
    {
        if (!CanCreateDocument)
        {
            return;
        }

        var index = VisibleDockables?.Count + 1 ?? 1;
        var document = new DocumentViewModel(_eventAggregator)
        {
            Id = $"Document{index}",
            Title = $"Document{index}"
        };

        Factory?.AddDockable(this, document);
        Factory?.SetActiveDockable(document);
        Factory?.SetFocusedDockable(this, document);
    }
}
```

Any Dock command property (for example `Close`, `Pin`, or custom menu items) can be backed by a Prism `DelegateCommand` or `CompositeCommand`. This aligns Dock's dynamic layout operations with Prism's well-known command aggregation features.

## Prism Concepts in Dock Layouts

Build on the base sample by layering common Prism patterns alongside Dock's layout primitives.

- **Event Aggregator**: Broadcast document state changes so tools can react without direct references.
- **Composite Commands**: Coordinate toolbar buttons that trigger multiple dock actions simultaneously.
- **Region Coordination**: Use Prism's region adapters so navigation requests populate docked areas.
- **Module Initialization**: Register dockables from Prism modules to keep the layout modular and discoverable.

### Event Aggregator and Dockables

Inject Prism's `IEventAggregator` into dockables to publish layout activity and decouple tools from documents.

```csharp
public class DocumentClosedEvent : PubSubEvent<DocumentClosedPayload>;
public record DocumentClosedPayload(string Id, string Title);

public class DocumentViewModel : Document
{
    private readonly IEventAggregator _eventAggregator;

    public DocumentViewModel(IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;
    }

    protected override bool OnClose()
    {
        _eventAggregator.GetEvent<DocumentClosedEvent>()
            .Publish(new DocumentClosedPayload(Id, Title));
        return base.OnClose();
    }
}

public class Tool1ViewModel : Tool
{
    public string? Status { get; private set; }

    public Tool1ViewModel(IEventAggregator eventAggregator)
    {
        eventAggregator.GetEvent<DocumentClosedEvent>()
            .Subscribe(payload => Status = $"Closed {payload.Title}");
    }
}
```

### Composite Commands for Layout Actions

Aggregate dock operations with Prism's `CompositeCommand` to keep UI buttons in sync with multiple dockables.

```csharp
public static class DockCommands
{
    public static readonly CompositeCommand SaveAndPin = new();
}

public class DocumentViewModel : Document
{
    public DocumentViewModel()
    {
        DockCommands.SaveAndPin.RegisterCommand(Save);
    }

    public DelegateCommand Save => new(() => /* persist content */);
}

public class ToolViewModel : Tool
{
    public ToolViewModel()
    {
        DockCommands.SaveAndPin.RegisterCommand(new DelegateCommand(() => CanPin = true));
    }
}
```

Bind `DockCommands.SaveAndPin` to a toolbar button to invoke every registered Dock command in one gesture.

### Prism Navigation Inside Docked Regions

Adapt Dock controls into Prism regions so navigation requests populate docked panes rather than replacing windows.

```csharp
using System.Linq;
using Dock.Avalonia.Controls;
using Dock.Model.Core;

public class DockRegionAdapter : RegionAdapterBase<DockControl>
{
    public DockRegionAdapter(IRegionBehaviorFactory factory) : base(factory)
    {
    }

    protected override void Adapt(IRegion region, DockControl regionTarget)
    {
        region.Views.CollectionChanged += (_, _) =>
        {
            if (regionTarget.Layout is not IDock targetDock)
            {
                return;
            }

            foreach (var dockable in region.Views.OfType<IDockable>())
            {
                if (targetDock.VisibleDockables?.Contains(dockable) == true)
                {
                    continue;
                }

                if (targetDock.Factory is { } factory)
                {
                    factory.AddDockable(targetDock, dockable);
                    factory.SetActiveDockable(dockable);
                }
                else if (targetDock.VisibleDockables is { })
                {
                    targetDock.VisibleDockables.Add(dockable);
                }
            }
        };
    }

    protected override IRegion CreateRegion() => new AllActiveRegion();
}

protected override void ConfigureRegionAdapterMappings(RegionAdapterMappings mappings)
{
    base.ConfigureRegionAdapterMappings(mappings);
    mappings.RegisterMapping(typeof(DockControl), Container.Resolve<DockRegionAdapter>());
}
```

With the adapter registered, add `IDockable` view models to the region (or resolve them from the container) and the adapter inserts them into the dock. Use Dock data templates to render those dockables in the UI.

### Loading Dockables from Prism Modules

Use `IModule` to register and insert dockables when modules initialize.

```csharp
public class ReportsModule : IModule
{
    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterForNavigation<ReportsView, ReportsViewModel>();
    }

    public void OnInitialized(IContainerProvider containerProvider)
    {
        var factory = containerProvider.Resolve<IFactory>();
        var reportsDock = containerProvider.Resolve<ReportsViewModel>();

        if (factory.GetDockable<IDockable>("RightTools") is { } toolsDock)
        {
            factory.AddDockable(toolsDock, reportsDock);
            factory.SetActiveDockable(reportsDock);
        }
    }
}
```

This keeps the Dock layout extensible and aligned with Prism's module loading lifecycle.

## Running the Sample

The `DockPrismSample` mirrors the MVVM sample but swaps in Prism infrastructure. To explore it:

```bash
cd path/to/Dock
./build.sh            # optional: restore, build, test everything
# or run the sample directly
DOTNET_CLI_TELEMETRY_OPTOUT=1 dotnet run --project samples/DockPrismSample/DockPrismSample.csproj
```

When the window opens, experiment with creating documents, docking tools, and resetting the layout to observe how Prism commands and property notifications flow through the Dock model.

## Next Steps

- Integrate the factory and view models into your Prism container using `IContainerRegistry` to enable dependency injection.
- Extend the sample with Prism navigation to swap between multiple root layouts or to persist layout state via Prism services.
- Review `dock-dependency-injection.md` in tandem with this guide when combining Prism's DI abstractions with Dock's factory interfaces.
