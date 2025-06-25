# Dock Glossary

This page defines common terms used throughout the Dock documentation and sample code. Use it as a quick reference when reading the guides.

- **DockControl** – Avalonia control that displays the layout and processes pointer input.
- **Dockable** – Base interface for items that can appear inside a dock. Documents, tools and docks all derive from it.
- **Dock** – Container that hosts a collection of dockables and exposes navigation commands.
- **RootDock** – The top level dock that owns the complete layout and manages pinned docks and windows.
- **ProportionalDock** – Arranges its children horizontally or vertically according to their `Proportion` values.
- **ToolDock / DocumentDock** – Specialized docks intended for tools or documents respectively.
- **DockWindow / HostWindow** – Floating window created when a dockable is detached from the main layout.
- **Layout** – The arrangement of docks and dockables currently shown by `DockControl`.
- **Factory** – Class responsible for creating and modifying layouts at runtime.
- **DockSerializer** – Component that saves or loads layouts so user customisations persist across sessions.
- **DockManager** – Implements the algorithms that move or swap dockables during drag and drop.

For an overview of all guides see the [documentation index](README.md).
