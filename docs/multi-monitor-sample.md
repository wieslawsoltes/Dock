# Multi-monitor sample

This sample demonstrates how to use `HostWindowLocator` to open floating windows on different monitors.
The factory registers two host window creators – one for each monitor – and alternates between them
when new windows are created.

```csharp
HostWindowLocator = new Dictionary<string, Func<IHostWindow?>>
{
    ["Monitor1"] = () => new MonitorHostWindow(0),
    ["Monitor2"] = () => new MonitorHostWindow(1)
};
```

`MonitorHostWindow` derives from `HostWindow` and sets its position to the working area of the
selected screen when the window opens.
By overriding `CreateDockWindow` the factory assigns an identifier to each new `DockWindow` so the
correct host is chosen when it is presented.

See the source under [`samples/MultiMonitorSample`](../samples/MultiMonitorSample) for the full implementation.

