# Dock documentation

## Overview

Welcome to the Dock documentation. This page is the entry point for the
documentation site and collects links to guides, reference topics and other
resources. These articles explain how to use the Dock layout system from basic
setup through advanced customization. The examples assume an Avalonia
application created with the .NET SDK. For build instructions and an overview of the repository see the [project README](../README.md). If you are new to Dock, start with the guides under *Get started*.

## Get started

- [Quick start](quick-start.md) – Set up a minimal Dock layout.
- [MVVM guide](dock-mvvm.md) – Build layouts using MVVM view models.
- [ReactiveUI guide](dock-reactiveui.md) – ReactiveUI equivalent of the MVVM guide.
- [XAML guide](dock-xaml.md) – Declare layouts purely in XAML.
- [Code-only guide](dock-code-only.md) – Build Dock layouts entirely in C#.
- [User view model guide](dock-user-viewmodel.md) – Combine your own view models with Dock.

## How-to guides
- [Complex layout tutorials](dock-complex-layouts.md) – Multi-window and plug-in walkthroughs.

## Concepts

- [Active document](dock-active-document.md) – Retrieve the currently focused document.
- [Events guide](dock-events.md) – Subscribe to dock and window events.
- [API scenarios](dock-api-scenarios.md) – Common coding patterns.
- [DockableLocator usage](dock-dockablelocator.md) – Register and resolve dockables.
- [Serialization and persistence](dock-serialization.md) – Save and restore layouts.
- [Serialization state](dock-serialization-state.md) – Capture focus information.
- [Dock state guide](dock-state.md) – Why and how to use `DockState`.
- [Dependency injection](dock-dependency-injection.md) – Register Dock services with `IServiceCollection`.
- [Context locators](dock-context-locator.md) – Provide `DataContext` objects for dockables.
- [Architecture overview](dock-architecture.md) – High level design of the docking system.
- [Deep dive](dock-deep-dive.md) – Internals of `DockControl`.
- [DockManager guide](dock-manager-guide.md) – When and how to customize `DockManager`.
- [Styling and theming](dock-styling.md) – Customize the appearance of Dock controls.
- [Custom themes](dock-custom-theme.md) – Build and apply your own theme.
- [Context menus](dock-context-menus.md) – Localize or replace built in menus.
- [Control recycling](dock-control-recycling.md) – Reuse visuals when dockables return.
- [Proportional StackPanel](dock-proportional-stackpanel.md) – Layout panel with adjustable proportions.
- [Sizing guide](dock-sizing.md) – Control pixel sizes and fixed dimensions.
- [Floating windows](dock-windows.md) – Detach dockables into separate windows.
- [Enable window drag](dock-window-drag.md) – Drag the host window via the document tab strip.
- [Host window locators](dock-host-window-locator.md) – Provide platform windows for floating docks.
- [Drag offset calculator](dock-drag-offset-calculator.md) – Control where the drag preview window appears.
- [Floating dock adorners](dock-floating-adorners.md) – Display drop indicators in a transparent window.
- [Pinned dock window](dock-pinned-window.md) – Show auto-hidden tools in a floating window.
- [Floating window owner](dock-window-owner.md) – Keep floating windows in front of the main window.
- [Advanced guide](dock-advanced.md) – Custom factories and runtime features.
- [Custom Dock.Model implementations](dock-custom-model.md) – Integrate Dock with other MVVM frameworks.

## Reference

- [Reference guide](dock-reference.md) – Overview of the core APIs.
- [Glossary](dock-glossary.md) – Definitions of common Dock terms.
- [Dockable property settings](dock-dockable-properties.md) – Configure per item behaviour.
- [Markup extensions](dock-markup-extensions.md) – Load and reference XAML fragments.
- [Adapter classes](dock-adapters.md) – Host, navigation and tracking helpers.
- [Tracking controls](dock-tracking-controls.md) – Collections that map dockables to their visuals.
- [Enumerations](dock-enums.md) – Values used by Dock APIs.
- [Model control interfaces](dock-model-controls.md) – Contracts used to define documents, tools and docks.
- [Layout panels](dock-layout-panels.md) – Dock, stack, grid and wrap style docks.
- [Dock settings](dock-settings.md) – Global drag/drop options and thresholds.
- [Dock properties](dock-properties.md) – Use attached properties to mark drag areas and drop targets.
- [DockSettings in controls](dock-settings-controls.md) – Apply global drag/drop settings when writing custom controls.

## Samples and additional resources

See the sample applications under the [samples](../samples/) directory for
real‑world usage. The [project README](../README.md) also lists the available
guides and provides basic build instructions.

For the complete source code visit the
[GitHub repository](https://github.com/wieslawsoltes/Dock). NuGet packages are
published on [NuGet.org](https://www.nuget.org/packages/Dock.Avalonia/),
including both `Dock.Avalonia` and `Dock.Model.Mvvm`.

## Troubleshooting

- [FAQ](dock-faq.md) – Solutions to common issues.
