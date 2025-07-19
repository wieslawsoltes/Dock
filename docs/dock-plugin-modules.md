# Dock plug-in modules

Dock can discover additional dockables from external assemblies. A module
implements `IDockModule` and registers its dockables with a factory.

```csharp
public class SampleModule : IDockModule
{
    public void Register(IFactory factory)
    {
        factory.DockableLocator ??= new Dictionary<string, Func<IDockable?>>();
        factory.DockableLocator["PluginDocument"] = () => new PluginDocument();
    }
}
```

Load modules at runtime and register them with the factory:

```csharp
var assemblies = new[] { Assembly.LoadFrom(path) };
factory.LoadModules(assemblies);
```

See `samples/PluginSample` for a minimal example module.

