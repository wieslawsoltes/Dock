# Dock documentation

Welcome to the Dock documentation. This landing page provides a structured overview of the docking system, the conceptual model behind it, and how to approach the guides and reference material. Use the left navigation for the full table of contents.

## Dock at a glance

Dock is a docking layout system for Avalonia. It provides document and tool panes, floating windows, docking targets, and layout persistence. Dock is designed to support both declarative XAML layouts and code-first factories, with multiple model implementations to match common MVVM frameworks.

## Core concepts

Dock is built around a small set of concepts that you will see throughout the documentation:

- Dockables: documents and tools that appear inside docks and windows.
- Docks: containers that host dockables (document docks, tool docks, split docks, stack docks).
- Root dock: the top-level dock that represents the layout for a window.
- Dock windows: floating or secondary windows created from dockables.
- Dock manager and services: runtime coordination of drag/drop, focus, and docking rules.
- Layout state: serializable representation of dock positions and active items.

## Architecture overview

Dock separates the model from the view:

- Model layer (Dock.Model): abstract layout, state, and docking rules.
- View layer (Dock.Avalonia): Avalonia controls that render and interact with the model.
- Integration layers: framework-specific implementations such as ReactiveUI or Prism.

This separation lets you customize layouts, replace view models, or integrate with different MVVM frameworks without rewriting the layout engine.

## What you can build

Dock supports a wide range of UI patterns:

- Multi-document interfaces (MDI) with tabbed documents.
- Tool panes that auto-hide, pin, or float.
- Split layouts with proportional sizing and drag handles.
- Floating windows with docking indicators.
- Managed floating windows hosted inside the main window.
- Persisted layouts that restore across sessions.

## Key features

- Document and tool docking with configurable rules.
- Floating windows, docking targets, and drag gestures.
- Layout persistence and restoration.
- Custom themes and styling.
- Control recycling for performance.
- Overlay systems for busy states and dialogs.

## How to approach the documentation

Start with the Quick start, then continue with the content guides and the framework-specific integration that matches your app. The Concepts section explains behavior and terminology, while the Reference section documents the API surface in detail.

If you are building a full application, the sample projects are the best end-to-end reference for layout construction, navigation, and overlays.

## Sample applications

Sample apps demonstrate complete layouts and patterns:

- Browse the sample projects in the [samples](https://github.com/wieslawsoltes/Dock/tree/master/samples/) folder.
- Use them as reference implementations for navigation, overlays, and window orchestration.

## Packages

Dock is published on NuGet. Common packages include:

- `Dock.Avalonia`
- `Dock.Model`
- `Dock.Avalonia.Themes.Fluent`
- `Dock.Avalonia.Themes.Browser`
- Framework integrations such as `Dock.Model.ReactiveUI`

See the repository README for the full list and build instructions:
[project README](https://github.com/wieslawsoltes/Dock/blob/master/README.md).

## Getting help

For troubleshooting and common issues, see the FAQ in the docs. For questions, issues, or contributions, use the GitHub repository:
[Dock on GitHub](https://github.com/wieslawsoltes/Dock).
