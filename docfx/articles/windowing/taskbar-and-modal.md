# Taskbar Visibility and Modality

Dock exposes per-window taskbar visibility and safe modal presentation.

## Taskbar Visibility
Use `IDockWindow.ShowInTaskbar` (or `DockWindowOptions.ShowInTaskbar`) to override taskbar visibility. When null, Dock leaves the platform default unchanged.

```csharp
factory.FloatDockable(tool, new DockWindowOptions
{
    ShowInTaskbar = false
});
```

## Modal Presentation
Use `IDockWindow.IsModal` or `DockWindowOptions.IsModal` to request modal presentation.

```csharp
factory.FloatDockable(tool, new DockWindowOptions
{
    OwnerMode = DockWindowOwnerMode.ParentWindow,
    IsModal = true
});
```

## Modal Fallback
If `IsModal` is true but no owner can be resolved:
- Dock attempts a safe fallback owner when allowed.
- If ownership is disallowed (`OwnerMode.None` or global policy `NeverOwned`), the window is shown non-modally and a diagnostic log entry is emitted.
