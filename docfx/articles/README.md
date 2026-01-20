# Dock documentation

## Overview

Welcome to the Dock documentation. This page is the entry point for the
documentation site and collects links to guides, reference topics and other
resources. These articles explain how to use the Dock layout system from basic
setup through advanced customization. The examples assume an Avalonia
application created with the .NET SDK. For build instructions and an overview of the repository see the [project README](https://github.com/wieslawsoltes/Dock/blob/master/README.md). If you are new to Dock, start with the guides under *Get started*.

## Get started

- [Quick start](quick-start.md) – Set up a minimal Dock layout with modern approaches.
- **[DocumentDock ItemsSource](dock-itemssource.md)** – **Automatic document management using data binding (recommended for most scenarios)**.
- **[Document and Tool Content Guide](dock-content-guide.md)** – **Comprehensive guide for setting up document and tool content with examples (START HERE for content setup)**.

### Model Implementation Guides

Choose the implementation that matches your MVVM framework:

- [MVVM guide](dock-mvvm.md) – Build layouts using MVVM view models with INotifyPropertyChanged.
- [Caliburn.Micro guide](dock-caliburn-micro.md) – Caliburn.Micro integration with Dock model base classes.
- [ReactiveUI guide](dock-reactiveui.md) – ReactiveUI integration with observables and reactive commands.
- [ReactiveProperty guide](dock-reactive-property.md) – ReactiveProperty framework integration with reactive properties and validation.
- [INPC guide](dock-inpc.md) – Basic INotifyPropertyChanged implementation without MVVM commands.
- [ReactiveUI with Dependency Injection](dock-reactiveui-di.md) – ReactiveUI with dependency injection for complex applications.
- [ReactiveUI with Routing](dock-reactiveui-routing.md) – ReactiveUI with routing for SPA-like navigation within dockables.

### Other Approaches

- [XAML guide](dock-xaml.md) – Declare layouts purely in XAML with ItemsSource support.
- [Code-only guide](dock-code-only.md) – Build Dock layouts entirely in C# with minimal dependencies.
- [User view model guide](dock-user-viewmodel.md) – Combine your own view models with Dock.
- [Views guide](dock-views.md) – Display content for documents and tools.

## How-to guides
- [Complex layout tutorials](dock-complex-layouts.md) – Multi-window and plugin walkthroughs.

## Concepts

- [Active document](dock-active-document.md) – Retrieve the currently focused document.
- [MDI document layout](dock-mdi.md) – Use classic MDI windows inside document docks.
- [MDI layout helpers](dock-mdi-layout-helpers.md) – Understand cascade/tile helpers and defaults.
- [Events guide](dock-events.md) – Subscribe to dock and window events.
- [API scenarios](dock-api-scenarios.md) – Common coding patterns.
- [DockableLocator usage](dock-dockablelocator.md) – Register and resolve dockables.
- [Serialization and persistence](dock-serialization.md) – Save and restore layouts.
- [Serialization state](dock-serialization-state.md) – Restore document/tool content and templates.
- [Dock state guide](dock-state.md) – Why and how to use `DockState`.
- [RestoreDockable behavior](dock-restore-dockable.md) – Understand dockable restoration and splitter management.
- [Dependency injection](dock-dependency-injection.md) – Register Dock services with `IServiceCollection`.
- [Context locators](dock-context-locator.md) – Provide `DataContext` objects for dockables.
- [Docking groups](dock-docking-groups.md) – Control which dockables can be docked together using group identifiers.
- [Architecture overview](dock-architecture.md) – High level design of the docking system.
- [Deep dive](dock-deep-dive.md) – Internals of `DockControl`.
- [DockManager guide](dock-manager-guide.md) – When and how to customize `DockManager`.
- [DockControl initialization](dock-control-initialization.md) – Defaults, locators, and setup flags.
- [Programmatic docking](dock-programmatic-docking.md) – Use `DockService` to validate and execute docking operations.
- [Drag actions and modifiers](dock-drag-actions.md) – How modifier keys map to docking actions.
- [Styling and theming](dock-styling.md) – Customize the appearance of Dock controls.
- [DataTemplates and custom dock types](dock-datatemplates.md) – Create custom dock types with their own visual representation.
- [Custom themes](dock-custom-theme.md) – Build and apply your own theme.
- [Context menus](dock-context-menus.md) – Localize or replace built-in menus.
- [Control recycling](dock-control-recycling.md) – Reuse visuals when dockables return.
- [Proportional StackPanel](dock-proportional-stackpanel.md) – Layout panel with adjustable proportions.
- [Sizing guide](dock-sizing.md) – Control pixel sizes and fixed dimensions.
- [Floating windows](dock-windows.md) – Detach dockables into separate windows.
- [Enable window drag](dock-window-drag.md) – Drag the host window via the document tab strip.
- [Host window locators](dock-host-window-locator.md) – Provide platform windows for floating docks.
- [Drag offset calculator](dock-drag-offset-calculator.md) – Control where the drag preview window appears.
- [Floating dock adorners](dock-floating-adorners.md) – Display drop indicators in a transparent window.
- [Dock targets and indicators](dock-targets.md) – Customize drop targets and selector visuals.
- [Debug overlay](dock-debug-overlay.md) – Visualize dock targets and drop areas.
- [Diagnostics logging](dock-diagnostics-logging.md) – Enable internal logging and route messages.
- [Selector overlay](dock-selector-overlay.md) – Switch documents and tools with keyboard gestures.
- [Command bars](dock-command-bars.md) – Merge menus and toolbars from active dockables.
- [Pinned dock window](dock-pinned-window.md) – Show auto-hidden tools in a floating window.
- [Floating window owner](dock-window-owner.md) – Keep floating windows in front of the main window.
- [RootDockDebug window](dock-root-dock-debug.md) – Toggle a runtime inspector for Dock layouts.
- [Debug views](dock-debug-views.md) – Reusable controls used by RootDockDebug.
- [Advanced guide](dock-advanced.md) – Custom factories and runtime features.
- [Advanced Content Wrapper Pattern](dock-content-wrapper-pattern.md) – Separate domain models from docking infrastructure.
- [Custom Dock.Model implementations](dock-custom-model.md) – Integrate Dock with other MVVM frameworks.

## Reference

- [Reference guide](dock-reference.md) – Overview of the core APIs.
- [Avalonia controls reference](dock-controls-reference.md) – Dock-specific properties for Avalonia controls.
- [Geometry types](dock-geometry.md) – DockPoint and DockRect usage.
- [Glossary](dock-glossary.md) – Definitions of common Dock terms.
- [Dockable property settings](dock-dockable-properties.md) – Configure per item behavior.
- [Markup extensions](dock-markup-extensions.md) – Load and reference XAML fragments.
- [Adapter classes](dock-adapters.md) – Host, navigation and tracking helpers.
- [Tracking controls](dock-tracking-controls.md) – Collections that map dockables to their visuals.
- [DockableControl tracking](dock-dockable-control.md) – How tracking modes register dockable visuals.
- [Enumerations](dock-enums.md) – Values used by Dock APIs.
- [Model control interfaces](dock-model-controls.md) – Contracts used to define documents, tools and docks.
- [Layout panels](dock-layout-panels.md) – Dock, stack, grid and wrap style docks.
- [Dock settings](dock-settings.md) – Global drag/drop options and thresholds.
- [Dock properties](dock-properties.md) – Use attached properties to mark drag areas and drop targets.
- [DockSettings in controls](dock-settings-controls.md) – Apply global drag/drop settings when writing custom controls.
- [Overlay services and host resolution](dock-overlay-services-reference.md) – Busy/dialog/confirmation services and host lookup flow.

## Samples and additional resources

See the sample applications under the [samples](https://github.com/wieslawsoltes/Dock/tree/master/samples/) directory for
real-world usage. The [project README](https://github.com/wieslawsoltes/Dock/blob/master/README.md) also lists the available
guides and provides basic build instructions.

For the complete source code visit the
[GitHub repository](https://github.com/wieslawsoltes/Dock). NuGet packages are
published on [NuGet.org](https://www.nuget.org/packages/Dock.Avalonia/),
including both `Dock.Avalonia` and `Dock.Model.Mvvm`.

## Troubleshooting

- [FAQ](dock-faq.md) – Solutions to common issues.
