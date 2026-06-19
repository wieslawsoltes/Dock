# Dock Quick Start Sample

This sample is the runnable version of the documentation quick start for the
`Dock.Model.Avalonia` XAML `ItemsSource` path.

The important details are:

- `App.axaml` registers Dock styles but does not register a view locator.
- `MainWindowViewModel` and `FileDocument` use ReactiveUI change
  notification and commands.
- `MainWindow.axaml` creates the `DockControl`, `Factory`, `RootDock`, and
  `DocumentDock` in XAML.
- `DocumentDock.ItemsSource` binds to the window view model through an explicit
  root source:

  ```xaml
  ItemsSource="{Binding #RootWindow.((vm:MainWindowViewModel)DataContext).Documents}"
  ```

- `DocumentTemplate` first binds to the generated Dock `Document`, then rebinds
  a nested scope to `Document.Context` for the source `FileDocument`.

Run it from the repository root:

```bash
dotnet run --project samples/DockQuickStartSample/DockQuickStartSample.csproj
```
