# Dock FAQ

This page answers common questions that come up when using Dock.

## Content setup

**What's the best way to create documents dynamically?**

Use the new `ItemsSource` property on `DocumentDock` for automatic document management:

```xml
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
1. `DocumentTemplate` has `x:DataType="Document"` on the root element
2. Access your model via `{Binding Context.PropertyName}` not `{Binding PropertyName}`
3. Your model implements `INotifyPropertyChanged`
4. Collection items have recognizable property names like `Title`, `Name`, or `CanClose`

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
```xml
<DocumentDock ItemsSource="{Binding OpenFiles}">
  <DocumentDock.DocumentTemplate>
    <DocumentTemplate>
      <TextBox Text="{Binding Context.Content}" x:DataType="Document"/>
    </DocumentTemplate>
  </DocumentDock.DocumentTemplate>
</DocumentDock>
```

**Missing dock types in XAML (e.g., "Unable to resolve type RootDock")**

Add the `Dock.Model.Avalonia` package and namespace:
```xml
xmlns:dock="using:Dock.Model.Avalonia.Controls"
```

## Focus management

**Why does the active document lose focus when I load a saved layout?**

After deserializing a layout you need to restore the last active and focused dockables. Call `DockState.Restore` with the root dock once the layout has been assigned to `DockControl`:

```csharp
var layout = _serializer.Load<IDock?>(stream);
if (layout is { })
{
    dock.Layout = layout;
    _dockState.Restore(layout); // restores active and focused dockables
}
```

`DockState` tracks focus changes at runtime and can reapply them after a layout is loaded.

## Serialization pitfalls

**Deserialization fails with unknown types**

`DockSerializer` relies on `DockableLocator` to map identifiers to your view models. Register all custom dockables before loading:

```csharp
ContextLocator = new Dictionary<string, Func<object?>>
{
    ["Document1"] = () => new MyDocument(),
    ["Tool1"] = () => new MyTool(),
};
DockableLocator = new Dictionary<string, Func<IDockable?>>
{
    ["Document1"] = () => new MyDocument(),
    ["Tool1"] = () => new MyTool(),
};
```

If a dockable cannot be resolved the serializer will return `null`.

**What is `DockableLocator` and `ContextLocator`?**

`DockableLocator` is a dictionary of functions that create view models. The
serializer and factory query it using the identifiers stored in a layout to
recreate dockables at runtime. `ContextLocator` works the same way but returns
objects that become the `DataContext` of the views. Populate both dictionaries
when initializing your factory so that Dock can resolve your custom documents
and tools.

**Are dock `Id`s unique?**

No. The `Id` string on a dockable acts as a lookup key for `DockSerializer`.
When a document dock is split or cloned the factory copies the source `Id` so
that both docks resolve to the same view model type when a layout is
deserialized. If you need to distinguish individual document docks, store a
separate unique identifier on your view models.

## Other questions

**Floating windows appear in the wrong place**

Override `CreateWindowFrom` in your factory to configure new windows when a dockable is floated. This allows you to center windows or set their dimensions.

```csharp
public override IHostWindow CreateWindowFrom(IDockWindow source)
{
    var window = base.CreateWindowFrom(source);
    window.Width = 800;
    window.Height = 600;
    window.Position = new PixelPoint(100, 100);
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
has been updated, so the change cannot be cancelled. Likewise there is no
pre-close event for dockables. The only cancellable closing hook is
`WindowClosing`, which is fired when a host window is about to close. Set the
`Cancel` property on the event arguments to keep the window open:

```csharp
factory.WindowClosing += (_, args) =>
{
    if (!CanShutdown())
    {
        args.Cancel = true; // prevents the window from closing
    }
};
```

Cancelling individual dockables is not supported.

**How do I disable undocking or drag-and-drop?**

Disable undocking per dockable by setting its `CanDrag` or `CanDrop` property to
`false`:

```csharp
document.CanDrag = false;
tool.CanDrop = false;
```

You can still toggle drag or drop globally using the attached `DockProperties`
from [`Dock.Settings`](dock-settings.md):

```xml
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
// Only documents can dock with other documents
document.DockGroup = "Documents";

// Only tools can dock with other tools  
tool.DockGroup = "Tools";

// This can dock anywhere (no restrictions)
flexibleTool.DockGroup = null;
```

Dockables with the same group can dock together, while different groups cannot mix. Null groups can dock with anything. See [Docking Groups](dock-docking-groups.md) for details.

**How do I float a dockable from its tab?**

Double-click the tab of a document or tool to detach it into a separate window.
The dockable must have `CanFloat` enabled.

For a general overview of Dock see the [documentation index](README.md).
