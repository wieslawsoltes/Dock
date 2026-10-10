# AvalonDock on Dock sample

Run `dotnet run --project samples/DockAvalonDockSample` from the repository root.
The sample adapts the Xceed Live Explorer 3.7.0 layout to Avalonia while keeping
the AvalonDock layout names and XML namespace. See `NOTICE.txt` and
`LICENSE-XCEED.txt` for the source and license of the adapted XAML.

Select and edit Document 2, move document and tool tabs, hide/show Journal,
then save/load the layout. XML is stored under the current user's local
application-data directory in `DockAvalonDockSample/layout.xml`.

The ViewModel uses ReactiveUI commands and a framework-independent workspace
interface. The application startup composes the Dock-backed service and view.
The view's code-behind only initializes its XAML.

This is an in-progress compatibility sample. Auto-hide and floating-window
serialization are not implemented yet; attempting to save those layouts
reports the limitation without overwriting the saved file. The complete
remaining compatibility scope is tracked in
`src/Dock.Avalonia.AvalonDock/COMPATIBILITY.md`.
