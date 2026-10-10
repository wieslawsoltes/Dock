# AvalonDock on Dock sample

Run `dotnet run --project samples/DockAvalonDockSample` from the repository root.
The sample adapts the Xceed Live Explorer 3.7.0 layout to Avalonia while keeping
the AvalonDock layout names. It uses Avalonia's default XML namespace, so the
layout types and `AvalonDockTheme` need no AvalonDock prefix. The original Xceed
XML namespace remains supported by the compatibility package. See `NOTICE.txt` and
`LICENSE-XCEED.txt` for the source and license of the adapted XAML.

Select and edit Document 2, move or float document and tool tabs, hide/show Journal,
and open Agenda or Contacts from the left auto-hide strip,
then save/load the layout. XML is stored under the current user's local
application-data directory in `DockAvalonDockSample/layout.xml`.

The ViewModel uses ReactiveUI commands and a framework-independent workspace
interface. The application startup composes the Dock-backed service and view.
The view's code-behind only initializes its XAML.

Save/load preserves docked, hidden, auto-hide, and floating layouts, including
floating window bounds and maximization. This is an in-progress compatibility
sample; the complete remaining compatibility scope is tracked in
`src/Dock.Avalonia.AvalonDock/COMPATIBILITY.md`.
