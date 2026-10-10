# AvalonDock compatibility

`Dock.Avalonia.AvalonDock` lets an Avalonia application use the
`AvalonDock` C# namespaces and layout names. The implementation wraps
`Dock.Model.Avalonia` models and renders them through Dock's controls. It is an
in-progress compatibility project; the
[acceptance checklist](https://github.com/wieslawsoltes/Dock/blob/feature/avalondock-compatibility/src/Dock.Avalonia.AvalonDock/COMPATIBILITY.md)
records implemented behavior and remaining API gaps. WPF controls, dependency
properties, and assemblies require their Avalonia counterparts.

The CLR namespaces are `AvalonDock.*`; update using directives and any explicit
CLR namespace declarations when porting source from the WPF library. The
assembly/package name remains `Dock.Avalonia.AvalonDock`.

## Reference the package and theme

The project builds NuGet packages for .NET 8 and .NET 10. To try the package from
this branch before publication, build it from the Dock repository:

```sh
dotnet pack src/Dock.Avalonia.AvalonDock -c Release -o artifacts/avalondock-nuget
```

Add that directory as a local NuGet source in your consuming application and
reference the generated `Dock.Avalonia.AvalonDock` package version. Keep your
normal package sources configured for its dependencies. The package references
Dock's Avalonia model and Fluent theme; use compatible Avalonia package versions.

In `App.axaml`, with the default `xmlns="https://github.com/avaloniaui"`, add:

```xml
<Application.Styles>
  <FluentTheme />
  <AvalonDockTheme />
</Application.Styles>
```

`AvalonDockTheme` includes Dock's Fluent resources and the manager/content
templates that connect the compatibility API to Dock's controls.

## Declare a layout in XAML

Place this layout inside an Avalonia view. Declare `x:DataType` on the view for
its ViewModel when using compiled bindings. This example uses literal content:

```xml
<DockingManager x:Name="DockManager">
  <LayoutRoot>
    <LayoutPanel Orientation="Horizontal">
      <LayoutAnchorablePane DockWidth="240">
        <LayoutAnchorable ContentId="properties" Title="Properties">
          <TextBlock Text="Workspace properties" Margin="12" />
        </LayoutAnchorable>
      </LayoutAnchorablePane>
      <LayoutDocumentPane>
        <LayoutDocument ContentId="readme" Title="Readme">
          <TextBox Text="Editable document" AcceptsReturn="True" />
        </LayoutDocument>
      </LayoutDocumentPane>
    </LayoutPanel>
  </LayoutRoot>
</DockingManager>
```

The assembly maps the manager, layout, and theme namespaces to Avalonia's
default XML namespace, so these elements require no AvalonDock prefix.
The Xceed XML namespace is also supported:

```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:ad="http://schemas.xceed.com/wpf/xaml/avalondock">
  <ad:DockingManager>
    <ad:LayoutRoot>
      <ad:LayoutPanel>
        <ad:LayoutDocumentPane>
          <ad:LayoutDocument Title="Document">
            <TextBlock Text="Avalonia content" />
          </ad:LayoutDocument>
        </ad:LayoutDocumentPane>
      </ad:LayoutPanel>
    </ad:LayoutRoot>
  </ad:DockingManager>
</Window>
```

`ad` is an arbitrary alias. Using a prefix can make the library boundary
explicit in a mixed XAML file; it does not change the layout's behavior.

## Use the C# layout API

The following code belongs in a workspace service called by ViewModel commands.
`manager` is the `DockingManager` declared above, supplied by application startup.
The sample uses an `ILayoutWorkspace` interface so its ViewModel has no Avalonia
dependency and its view code-behind only initializes XAML.

```csharp
using System.Linq;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;

LayoutDocument document = manager.Layout.Descendents()
    .OfType<LayoutDocument>().Single(item => item.ContentId == "readme");
LayoutAnchorable properties = manager.Layout.Descendents()
    .OfType<LayoutAnchorable>().Single(item => item.ContentId == "properties");

document.IsActive = true;
document.Float();
document.Dock();

properties.Hide();
properties.Show();
properties.ToggleAutoHide();

new XmlLayoutSerializer(manager).Serialize("layout.xml");
new XmlLayoutSerializer(manager).Deserialize("layout.xml");
```

Content methods operate through the native Dock factory. Closing and hiding
honor the corresponding capabilities and cancelable events. A tool's return
pane and tab position are retained when it floats, docks, hides, or is restored.
After deserialization, look up layout nodes again: the manager now owns a new
layout tree, while content can retain its application identity.

## Bind item containers and commands

`DockingManager.GetLayoutItemFromModel` returns a `LayoutDocumentItem` or
`LayoutAnchorableItem` from `AvalonDock.Controls`. Its `LayoutElement` is the
layout node and its `Model` is the application content. Title, identifier,
icon, selection, active state, and capabilities synchronize with the live model.

```csharp
using AvalonDock.Controls;

LayoutItem? item = manager.GetLayoutItemFromModel(document);
if (item?.FloatCommand is { } floatCommand && floatCommand.CanExecute(null))
    floatCommand.Execute(null);
```

A view whose data context is the returned container can bind its commands
using the default XML namespace:

```xml
<StackPanel x:DataType="LayoutItem">
  <TextBlock Text="{Binding Title}" />
  <Button Content="Activate" Command="{Binding ActivateCommand}" />
  <Button Content="Float" Command="{Binding FloatCommand}" />
  <Button Content="Close" Command="{Binding CloseCommand}" />
</StackPanel>
```

The default ReactiveUI commands cover closing, floating, activation, docking as
a document, closing document sets, splitting document panes, and moving between
adjacent document panes. Tool containers add hide, auto-hide, and dock commands.
Document-group commands honor `CanMove` and the manager's
`AllowMixedOrientation` setting. Closing document sets uses each container's
current `CloseCommand`, including application replacements, and honors close
capabilities and cancellation. Individual command properties can be replaced
or bound to application commands.

Containers retain identity during moves, floating, auto-hide, and tool hiding.
Removal detaches a container after the current layout mutation; replacing or
deserializing the manager's layout detaches the old containers immediately.
Resolve containers again after loading. Detachment disables and releases
default commands without disposing application commands. `IsVisible` hides or
shows tools and closes documents, honoring cancellation; Avalonia does not
provide WPF's separate `Hidden` and `Collapsed` states.

`View` is the compiled content presenter used by the native Dock controls. It
moves with its content between docked and floating hosts. Routing native
chrome through application container commands remains part of the
compatibility work.

## Bind ViewModel content

`DocumentsSource` and `AnchorablesSource` accept observable collections of
application objects. With a view's `x:DataType` set to a ViewModel exposing
`Documents` and `Tools`, bind the sources and provide typed templates:

```xml
<DockingManager DocumentsSource="{Binding Documents}"
                AnchorablesSource="{Binding Tools}">
  <DockingManager.DataTemplates>
    <DataTemplate DataType="{x:Type vm:EditorViewModel}"
                  x:DataType="vm:EditorViewModel">
      <TextBox Text="{Binding Text, Mode=TwoWay}" AcceptsReturn="True" />
    </DataTemplate>
    <DataTemplate DataType="{x:Type vm:PropertiesViewModel}"
                  x:DataType="vm:PropertiesViewModel">
      <TextBlock Text="{Binding Description}" Margin="12" />
    </DataTemplate>
  </DockingManager.DataTemplates>
</DockingManager>
```

Here `vm` refers to the application's ViewModel namespace. The ViewModels derive
from `ReactiveObject`; editing properties must notify changes. Keep UI logic in
commands and services. Container styles can bind each generated layout item's
title and stable `ContentId`. A `LayoutUpdateStrategy` can also set these values
and choose its destination pane. There is no implicit mapping from a
ViewModel's property names to these layout properties.

Source additions, removals, replacements, and resets update the live layout.
Moving a source entry preserves the user's tab order. Resetting a source clears
all layout items of that type, including manually inserted items. Floating
hosts resolve content templates from the manager's current scope, including
ancestor templates.

## Style item containers

`LayoutItemContainerStyle` accepts an Avalonia `ControlTheme`. Its target is a
`LayoutItem`, `LayoutDocumentItem`, or `LayoutAnchorableItem`, and the container
is its own data context. Compiled bindings access the application ViewModel
through `Model`. For a document collection whose `EditorViewModel` exposes
`Title`, `ContentId`, `CanClose`, and `CanFloat`:

```xml
<DockingManager DocumentsSource="{Binding Documents}">
  <DockingManager.LayoutItemContainerStyle>
    <ControlTheme TargetType="LayoutItem" x:DataType="LayoutItem">
      <Setter Property="Title" Value="{Binding ((vm:EditorViewModel)Model).Title}" />
      <Setter Property="ContentId" Value="{Binding ((vm:EditorViewModel)Model).ContentId}" />
      <Setter Property="CanClose" Value="{Binding ((vm:EditorViewModel)Model).CanClose, Mode=TwoWay}" />
      <Setter Property="CanFloat" Value="{Binding ((vm:EditorViewModel)Model).CanFloat}" />
    </ControlTheme>
  </DockingManager.LayoutItemContainerStyle>
</DockingManager>
```

Use one-way bindings unless the ViewModel must also receive changes made by
Dock. The two-way `CanClose` binding above demonstrates that option. Titles,
identifiers, and capabilities affect the same native models that Dock renders.
Containers belong to the manager's logical tree, so themes can use manager and
ancestor resources, including `DynamicResource`, while content is floating.
Shared setters can live in a base theme referenced through `BasedOn`.

For different document and tool types, set `LayoutItemContainerStyleSelector`
to an `AvalonDock.Controls.StyleSelector` subclass. This is the counterpart of
WPF's `StyleSelector`; its result is an Avalonia `ControlTheme`:

```csharp
using Avalonia;
using Avalonia.Styling;
using AvalonDock.Controls;

public sealed class WorkspaceStyleSelector : StyleSelector
{
    public ControlTheme? EditorStyle { get; set; }
    public ControlTheme? PropertiesStyle { get; set; }

    public override ControlTheme? SelectStyle(object? item, AvaloniaObject container)
        => item switch
        {
            EditorViewModel => EditorStyle,
            PropertiesViewModel => PropertiesStyle,
            _ => null
        };
}
```

Declare the themes and selector in XAML resources and assign the selector with
`StaticResource`. The selector receives the application object and its actual
item container. An explicit `LayoutItemContainerStyle` takes precedence.
Changing either manager property reapplies the style; replacing content
selects a theme for the new object and detaches the old typed bindings first.
A null selection removes the previously selected theme while preserving the
live layout state. Styles also apply to new source entries and hidden tools.

Command setters may bind application `ReactiveCommand` properties through
`Model`, just like metadata. Replacing a theme or restoring XML releases its
bindings without disposing application commands; available default container
commands are restored when no application command is supplied. Native chrome
command overrides are still an open compatibility item, so use the container
commands from application bindings until that integration is complete.

## Select content templates

`LayoutItemTemplate` applies an explicit Avalonia `IDataTemplate` to application
content in both docked and floating panes. For example, when every document
uses the same ViewModel type:

```xml
<DockingManager DocumentsSource="{Binding Documents}">
  <DockingManager.LayoutItemTemplate>
    <DataTemplate x:DataType="vm:EditorViewModel">
      <TextBox Text="{Binding Text, Mode=TwoWay}" AcceptsReturn="True" />
    </DataTemplate>
  </DockingManager.LayoutItemTemplate>
</DockingManager>
```

For different content types, use `LayoutItemTemplateSelector`. The WPF selector
counterpart is `AvalonDock.Controls.DataTemplateSelector`: override
`SelectTemplate(object?, AvaloniaObject)` and return an Avalonia `IDataTemplate`.
The arguments are the application content and its actual `ContentPresenter`.
The selector can refer to compiled templates declared in XAML resources:

```csharp
using Avalonia;
using Avalonia.Controls.Templates;
using AvalonDock.Controls;

public sealed class WorkspaceTemplateSelector : DataTemplateSelector
{
    public IDataTemplate? EditorTemplate { get; set; }
    public IDataTemplate? PropertiesTemplate { get; set; }

    public override IDataTemplate? SelectTemplate(object? item, AvaloniaObject container)
        => item switch
        {
            EditorViewModel => EditorTemplate,
            PropertiesViewModel => PropertiesTemplate,
            _ => null
        };
}
```

Set `LayoutItemTemplateSelector` to an instance of this selector. An explicit
`LayoutItemTemplate` takes precedence. When the selector returns null, normal
manager, ancestor, and application data-template lookup applies. Inline
controls and Dock template content retain Dock's native content handling.
Changing either manager property updates existing presenters; changing content
selects the appropriate template for the new object. Recycling is limited to
controls built by the same selected template. An explicit template can also
display null content.

## Restore application content from XML

The XML describes layout structure and metadata; it does not serialize controls
or arbitrary ViewModels. A newly constructed `XmlLayoutSerializer` captures
existing content by `ContentId`. Construct it after the relevant content exists.
For a new application session, resolve IDs using a content registry:

```csharp
XmlLayoutSerializer serializer = new(manager);
serializer.LayoutSerializationCallback += (_, args) =>
{
    if (contentById.TryGetValue(args.Model.ContentId, out object? content))
        args.Content = content;
    else
        args.Cancel = true;
};
serializer.Deserialize("layout.xml");
```

`contentById` is an application-owned dictionary of stable identifiers to
ViewModels. Set `Cancel` to omit an unavailable item. Source collection
notifications are suspended while callbacks run, then resume even if loading
fails. Parsing and content resolution finish before replacing the manager's
layout. The runnable sample writes to a temporary file and replaces the saved
file after a successful save.

Docked, hidden, auto-hide, and supported floating layouts retain return
locations and floating geometry. Native grouped-document or mixed floating
shapes that the current AvalonDock hierarchy cannot represent are rejected
before an existing save file is overwritten.

Individual content, pane/group, root, and floating models also implement
`IXmlSerializable`. Their `WriteXml` methods write attributes and children inside
an element opened by the caller; `ReadXml` consumes that element. Content reads
preserve an existing `Content` object, group reads append children, and root
reads replace the hierarchy while retaining the native root and factory.
These methods do not resolve application content or return-container references;
use `XmlLayoutSerializer` for complete layout restoration and content callbacks.

## Implementation architecture

```mermaid
flowchart TB
    App["Application XAML, ViewModels, and workspace service"]
    subgraph Compatibility["Dock.Avalonia.AvalonDock"]
        Manager["DockingManager + AvalonDockTheme"]
        Layout["AvalonDock.Layout API"]
        Items["LayoutItem containers, styles, content templates, and ReactiveUI commands"]
        Bridge["Identity, collections, selection, sizing, and return history"]
        Lifecycle["Floating hosts, templates, and close/hide lifecycle"]
        Xml["XmlLayoutSerializer + explicit XML codec"]
    end
    subgraph Native["Existing Dock implementation"]
        Models["Dock.Model.Avalonia models"]
        Factory["Dock factory and docking operations"]
        Controls["DockControl, HostWindow, and Fluent controls"]
    end
    App --> Manager
    App --> Layout
    App --> Xml
    Manager --> Layout
    Manager --> Items
    Items <--> Layout
    Items --> Factory
    Items --> Controls
    Manager --> Controls
    Layout <--> Bridge
    Bridge <--> Models
    Layout --> Lifecycle
    Lifecycle --> Factory
    Lifecycle --> Controls
    Controls --> Factory
    Factory --> Models
    Models --> Controls
    Xml <--> Layout
```

The facade wraps the same native objects that Dock renders and moves.
`LayoutModelContext` preserves wrapper identity, and `LayoutChildren` projects
native child lists as observable AvalonDock collections. Native changes flow
back into public ownership, selection, and active-content state. Compatibility
metadata, such as AvalonDock return-container references, remains in the adapter.

| AvalonDock API | Native Dock implementation |
| --- | --- |
| `LayoutRoot` | `RootDock` and its factory |
| `LayoutPanel`, document/tool pane groups | `ProportionalDock` |
| `LayoutDocumentPane` | `DocumentDock` |
| `LayoutAnchorablePane` | `ToolDock` |
| `LayoutDocument`, `LayoutAnchorable` | `Document`, `Tool` |
| `LayoutAnchorSide`, `LayoutAnchorGroup` | Projections over native pinned lists |
| Document/tool floating windows | `DockWindow`, a floating `RootDock`, and `HostWindow` |
| `DockingManager` | A themed `DockControl` using the live root and factory |
| `LayoutDocumentItem`, `LayoutAnchorableItem` | Container state and commands over the same `Document`/`Tool` |

`LayoutFactory` extends the native Avalonia factory at the window-close and
batch-floating boundaries. It composes lifecycle helpers to honor AvalonDock
capabilities and return positions, then delegates actual movement to Dock.
`LayoutFloatingWindowPresenter`, the host factory, and scoped templates manage
floating presentation and preserve the manager's template lookup scope.
The native content template obtains the corresponding `LayoutItem.View` from
the factory. That presenter selects manager content templates and delegates
inline controls and Dock template content to the native model's template path.
The item registry keeps containers in the manager's logical scope and applies
selected control themes. Compiled container bindings synchronize metadata and
capabilities with those same native models.

`XmlLayoutSerializer` uses explicit codecs and content callbacks without
runtime type discovery. It restores the facade and its native models together.
The core docking algorithms, input handling, recycling, and control rendering
remain in Dock. See the [Dock architecture overview](dock-architecture.md) for
the native interaction pipeline.

## Sample and validation

From the repository root:

```sh
dotnet run --project samples/DockAvalonDockSample
dotnet test tests/Dock.Avalonia.AvalonDock.Tests -c Release
```

The [sample](https://github.com/wieslawsoltes/Dock/tree/feature/avalondock-compatibility/samples/DockAvalonDockSample)
demonstrates documents, tool panes, Agenda/Contacts auto-hide strips, floating,
and save/load commands. Its XAML adapts Xceed Live Explorer 3.7.0 and includes
the original Ms-PL license and attribution. The adapter implementation is new
MIT-licensed code.

The test project covers compiled default/Xceed namespaces, Headless input and
rendering, native model synchronization, XML, and lifecycle behavior. The
separate NuGet consumer builds against the packed assemblies. The
[compatibility checklist](https://github.com/wieslawsoltes/Dock/blob/feature/avalondock-compatibility/src/Dock.Avalonia.AvalonDock/COMPATIBILITY.md)
also describes the pinned API audit and the work still required before claiming
full compatibility.
