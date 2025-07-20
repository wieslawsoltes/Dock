# Tabbed host window

`TabbedHostWindow` combines the custom Avalonia chrome with a `DocumentTabStrip` inside the title bar. The tabs come from the active root document dock and allow dragging the window by the empty tab area.

```csharp
var host = new TabbedHostWindow();
```

Register the type through the factory to use it for floating docks:

```csharp
factory.HostWindowLocator = new Dictionary<string, Func<IHostWindow?>>
{
    [nameof(IDockWindow)] = () => new TabbedHostWindow()
};
```

