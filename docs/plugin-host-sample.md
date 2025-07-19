# Plug-in host sample

The `PluginHostSample` demonstrates loading dockables from external plug-ins.

1. Define a shared `IPlugin` interface returning an `IDockable` instance.
2. Build plug-in assemblies that implement this contract.
3. At startup the host scans a `Plugins` directory, loads each assembly and adds the created dockables to the layout:

```csharp
var assembly = Assembly.LoadFrom(path);
var plugins = assembly
    .GetTypes()
    .Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsAbstract)
    .Select(Activator.CreateInstance)
    .OfType<IPlugin>();
foreach (var plugin in plugins)
{
    factory.AddDockable(documentDock, plugin.CreateDockable());
}
```

Initialize the layout once and save it so that plug-in windows reappear on the next run.

