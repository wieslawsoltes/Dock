# Dock sites

Dock sites are containers that host multiple `DockControl` instances. Sites can be arranged horizontally or vertically using `DockSiteControl`.

Each `DockControl` exposes a `DockGroup` property. Dock controls with the same group identifier are linked – windows can be dragged from one control to another.

Example usage:

```xaml
<controls:DockSiteControl DockControls="{Binding DockControls}" Orientation="Horizontal"/>
```

```csharp
var left = new DockControl { DockGroup = "main" };
var right = new DockControl { DockGroup = "main" };
var site = new DockSiteControl { DockControls = new[] { left, right }.ToList() };
```

Dragging documents or tools between the linked controls is allowed when their `DockGroup` values match.
