# Dock FAQ

This page answers common questions that come up when using Dock.

## Content setup

**What's the best way to create documents dynamically?**

Use the new `ItemsSource` property on `DocumentDock` for automatic document management:

```xaml
<DocumentDock ItemsSource="{Binding Documents}">
  <DocumentDock.DocumentTemplate>
    <DocumentTemplate>
      <StackPanel x:DataType="Document">
        <TextBlock Text="{Binding Title}"/>
        <TextBox Text="{Binding Context.Content}"/>
      </StackPanel>
    </DocumentTemplate>
  </DocumentDock.DocumentTemplate>
</DocumentDock>
```

This approach automatically creates/removes documents when you add/remove items from your collection, similar to how `ListBox.ItemsSource` works.

**My document tabs are blank/empty (ItemsSource approach)**

For `ItemsSource` documents, check:
1. `DocumentTemplate` is set (no template means no documents are created)
2. Collection items expose `Title`, `Name`, or `DisplayName` (or override `ToString`) for tab headers
3. Access your model via `{Binding Context.PropertyName}` not `{Binding PropertyName}`
4. Your model implements `INotifyPropertyChanged` if you expect updates
5. If you use compiled bindings, set `x:DataType="Document"` on the template root

**My document tabs are blank/empty (ViewModel approach)**

For the ViewModel + DataTemplate approach, check:
1. DataTemplate is registered in `App.axaml` 
2. DataType matches your ViewModel type exactly
3. ViewModel namespace is imported correctly
4. View's `x:DataType` matches the ViewModel

For comprehensive setup guides, see [Document and Tool Content Guide](dock-content-guide.md).

**I get "Unexpected content" errors when adding documents**

This happens when you set a UserControl instance directly to the `Content` property. Use one of these approaches instead:
- Use `ItemsSource` with `DocumentTemplate` (recommended for most cases)
- Create a ViewModel that inherits from `Document` and use DataTemplate
- Use `Content = new Func<IServiceProvider, object>(_ => new MyView())`

See [Document and Tool Content Guide](dock-content-guide.md) for examples.

**How do I bind to collections of business objects?**

Use the `ItemsSource` property to bind your existing domain models directly:

```csharp
// Your existing model
public class FileModel : INotifyPropertyChanged
{
    public string Title { get; set; }      // Used for tab title
    public string Content { get; set; }    // Accessible via Context.Content
    public bool CanClose { get; set; }     // Controls if tab can be closed
}

// In your ViewModel
public ObservableCollection<FileModel> OpenFiles { get; } = new();
```

Then bind in XAML:
```xaml
<DocumentDock ItemsSource="{Binding OpenFiles}">
  <DocumentDock.DocumentTemplate>
    <DocumentTemplate>
      <TextBox x:DataType="Document" Text="{Binding Context.Content}"/>
    </DocumentTemplate>
  </DocumentDock.DocumentTemplate>
</DocumentDock>
```

**Missing dock types in XAML (e.g., "Unable to resolve type RootDock")**

Add the `Dock.Model.Avalonia` package and namespace:
```xaml
xmlns:dock="using:Dock.Model.Avalonia.Controls"
```

## Focus management

**Why does the active document change when I load a saved layout?**

Active and focused dockables are serialized with the layout itself, so you do not need `DockState` to restore focus. If the active document is not what you expect after loading, make sure you:

1. Assign the layout to `DockControl.Layout` before calling `InitLayout`.
2. Do not overwrite `ActiveDockable` or `DefaultDockable` after loading.

`DockState` is used for restoring document/tool content and document templates, not focus.

## Serialization pitfalls

**Deserialization fails with unknown types**

`IDockSerializer` implementations (for example `Dock.Serializer.SystemTextJson.DockSerializer`) use type information embedded in the layout. If a type cannot be resolved, ensure the assembly that defines it is loaded and referenced by the application. For dependency injection scenarios, construct the serializer with an `IServiceProvider` so it can resolve types from the container.

**What is `DockableLocator` and `ContextLocator`?**

`DockableLocator` is a dictionary of functions that create dockables when you
need to resolve them by id at runtime (for example via `GetDockable`). 
`ContextLocator` returns objects that become `dockable.Context` during
initialization when a dockable does not already have a context. Populate
these dictionaries in `InitLayout` if your app relies on id-based lookup or
needs to recreate contexts for serialized layouts.

**Are dock `Id`s unique?**

No. The `Id` string is an app-defined identifier stored in the layout and used
by helpers such as `ContextLocator` or `GetDockable`. When a document dock is
split or cloned the factory copies the source `Id`, so multiple docks can share
the same id. If you need to distinguish individual docks, store a separate
unique identifier on your view models.

## Other questions

**Floating windows appear in the wrong place**

Override `CreateWindowFrom` in your factory to configure new windows when a dockable is floated. This allows you to center windows or set their dimensions.

```csharp
public override IDockWindow? CreateWindowFrom(IDockable dockable)
{
    var window = base.CreateWindowFrom(dockable);
    if (window is null)
    {
        return null;
    }

    window.Width = 800;
    window.Height = 600;
    window.X = 100;
    window.Y = 100;
    return window;
}
```

**Can I give a tool a fixed size?**

Set `MinWidth` and `MaxWidth` (or the height equivalents) on the tool view model. When both values are the same the tool cannot be resized. `DockManager` has a `PreventSizeConflicts` flag which stops docking tools together if their fixed sizes are incompatible.

**Pinned tools show up on the wrong side**

When a tool is pinned the framework looks at the `Alignment` of its
containing `ToolDock`.  If no alignment is specified the dock defaults to
`Left`, which can make right–hand panels collapse to the left when pinned.
Set `Alignment` to `Right`, `Left`, `Top` or `Bottom` depending on where the
dock should live:

```csharp
new ToolDock
{
    VisibleDockables = CreateList<IDockable>(myTool),
    Alignment = Alignment.Right
};
```

**Can I cancel switching the active dockable or closing a dock?**

Dock currently raises `ActiveDockableChanged` only *after* the active dockable
has been updated, so the change cannot be cancelled. Closing dockables is
cancellable via `DockableClosing`, which runs before the dockable is closed.
Set `Cancel` to keep the dockable open:

```csharp
factory.DockableClosing += (_, args) =>
{
    if (!CanCloseDockable(args.Dockable))
    {
        args.Cancel = true;
    }
};
```

You can also cancel window closure with `WindowClosing`, which is fired when a
host window is about to close. Set the `Cancel` property on the event arguments
to keep the window open:

```csharp
factory.WindowClosing += (_, args) =>
{
    if (!CanShutdown())
    {
        args.Cancel = true; // prevents the window from closing
    }
};
```

Dock does not provide a pre-change hook for active dockable switching.

**How do I disable undocking or drag-and-drop?**

Disable undocking per dockable by setting its `CanDrag` or `CanDrop` property to
`false`:

```csharp
document.CanDrag = false;
tool.CanDrop = false;
```

You can still toggle drag or drop globally using the attached `DockProperties`
from [`Dock.Settings`](dock-settings.md):

```xaml
<Window xmlns:dockSettings="clr-namespace:Dock.Settings;assembly=Dock.Settings"
        dockSettings:DockProperties.IsDragEnabled="False"
        dockSettings:DockProperties.IsDropEnabled="False">
    <DockControl />
</Window>
```

The default templates bind these attached properties to the `CanDrag` and `CanDrop`
properties of each dockable. In most cases you simply toggle the boolean
properties on your view models and let the templates update `DockProperties` for
you.

Dockables may still be floated programmatically unless their `CanFloat` property
is set to `false`.

**How can I prevent certain dockables from docking together?**

Use docking groups to control which dockables can dock together. Set the `DockGroup` property on your dockables:

```csharp
// Documents can dock locally with other documents
// Can dock globally only into targets with the same group
document.DockGroup = "Documents";

// Tools can dock locally with other tools
// Can dock globally only into targets with the same group
tool.DockGroup = "Tools";

// This can dock globally anywhere and locally with other ungrouped dockables
flexibleTool.DockGroup = null;
```

Dockables with the same group can dock together locally, while different groups cannot mix. Grouped dockables can dock globally only into targets with the same group and cannot dock globally into ungrouped or different-group targets. Non-grouped dockables (null/empty groups) can dock globally anywhere and locally with other non-grouped dockables. See [Docking Groups](dock-docking-groups.md) for details.

**How do I float a dockable from its tab?**

Double-click the tab of a document or tool to detach it into a separate window.
The dockable must have `CanFloat` enabled.

For a general overview of Dock see the [documentation index](README.md).
