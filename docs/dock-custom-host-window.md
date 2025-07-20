# Custom host window

`CustomHostWindow` is a convenience class that always uses a custom Avalonia title bar. It derives from `HostWindow` and enables the pseudo classes that remove the system chrome.

Use it when you want floating or main windows to share the same look and allow drag operations over the entire title bar.

```csharp
var host = new CustomHostWindow();
```

Register the type through the factory to use it for floating docks:

```csharp
factory.HostWindowLocator = new Dictionary<string, Func<IHostWindow?>>
{
    [nameof(IDockWindow)] = () => new CustomHostWindow()
};
```

The window supports dragging and dropping on dock targets via the built in pointer handlers.
