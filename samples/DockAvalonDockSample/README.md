# AvalonDock on Dock sample

This sample is deferred and excluded from the normal solution until the required
framework APIs ship in official Avalonia. See
[#1186](https://github.com/wieslawsoltes/Dock/issues/1186) for the remaining work.
The source and license attribution are retained for reactivation; the sample
cannot currently be run against Dock's official Avalonia dependency.

The sample adapts the Xceed Live Explorer 3.7.0 layout to Avalonia while keeping
the AvalonDock layout names. It uses Avalonia's default XML namespace, so the
layout types and `AvalonDockTheme` need no AvalonDock prefix. The original Xceed
XML namespace remains supported by the compatibility package. See `NOTICE.txt` and
`LICENSE-XCEED.txt` for the source and license of the adapted XAML.

Select and edit Document 2, move or float document and tool tabs, hide/show Journal,
and open Agenda or Contacts from the left auto-hide strip,
then save/load the layout. XML is stored under the current user's local
application-data directory in `DockAvalonDockSample/layout.xml`.

Dragging a tab or a tool pane title shows its full content. The manager's
`ShowDockablePreviewOnDrag` option is enabled by default and set explicitly in
the sample XAML. Set it to `False` to try the compact caption/status preview.

The ViewModel uses ReactiveUI commands and a framework-independent workspace
interface. The application startup composes the Dock-backed service and view.
The view's code-behind only initializes its XAML.

Save/load preserves docked, hidden, auto-hide, and floating layouts, including
floating window bounds and maximization. This is an in-progress compatibility
sample; the complete remaining compatibility scope is tracked in
[the compatibility checklist](../../src/Dock.Avalonia.AvalonDock/COMPATIBILITY.md).

See the [AvalonDock compatibility guide](../../docfx/articles/dock-avalondock.md)
for XAML and C# examples, ViewModel content, XML callbacks, and the architecture
diagram. The sample's [workspace service](Services/AvalonDockWorkspace.cs)
contains the corresponding C# operations; [MainWindow.axaml](MainWindow.axaml)
declares the layout.
