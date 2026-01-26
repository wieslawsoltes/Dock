# DockControl Initialization and Defaults

`DockControl` wires up the factory and layout when `InitializeFactory` and `InitializeLayout` are enabled. This guide explains what the control sets up for you and when you might want to take over manually.

## Factory assignment

When a `Layout` is assigned, `DockControl` checks for a factory:

- If `Layout.Factory` is `null` and `DockControl.Factory` is set, it assigns that factory to the layout.
- If `Layout.Factory` is already set, it uses it as-is.

If neither is set, initialization stops and the control will not set up locators or call `InitLayout`.

## InitializeFactory behavior

When `InitializeFactory="True"`, `DockControl` populates the factory with default locators:

- `ContextLocator` is set to a new empty dictionary.
- `DockableLocator` is set to a new empty dictionary.
- `HostWindowLocator` is set with a default entry for `IDockWindow` that creates a `HostWindow`.
- `DefaultContextLocator` is set to return `DockControl.DefaultContext`.
- `DefaultHostWindowLocator` is set to return a new `HostWindow`.

This makes a minimal layout work out of the box, but it also means any locators you configured earlier will be replaced. If you want to populate locators yourself or integrate with dependency injection, set `InitializeFactory="False"` and configure the factory manually.

## InitializeLayout behavior

When `InitializeLayout="True"`, `DockControl` calls:

```csharp
layout.Factory.InitLayout(layout);
```

This initializes owners, commands, and view model state for every dockable in the layout. If your factory needs additional setup (for example, custom locators), configure it before calling `InitLayout`.

## DefaultContext usage

`DockControl.DefaultContext` feeds the factory fallback for `ContextLocator` when `InitializeFactory` is enabled. You can set it from code or bind it in XAML:

```csharp
var dockControl = new DockControl
{
    DefaultContext = new MainViewModel(),
    InitializeFactory = true,
    InitializeLayout = true
};
```

```xaml
<DockControl Layout="{Binding Layout}"
             Factory="{Binding Factory}"
             DefaultContext="{Binding}"
             InitializeFactory="True"
             InitializeLayout="True" />
```

If you prefer to set `DefaultContextLocator` directly, disable `InitializeFactory` or assign your own locator after the control initializes.

## Recommended patterns

- For quick samples and XAML-only layouts, keep both flags enabled.
- For DI-heavy apps, disable `InitializeFactory` and set locators explicitly in your factory.
- Always call `InitLayout` once after you finish building or deserializing the layout.

Related guides:

- [Context locators](dock-context-locator.md)
- [Host window locators](dock-host-window-locator.md)
- [DockManager guide](dock-manager-guide.md)

For an overview of all guides see the [documentation index](README.md).
