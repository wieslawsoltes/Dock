# Dock Views Guide

Dock renders `Document` and `Tool` instances using standard Avalonia data templating. This guide shows how to present your own controls for these items.

> **💡 New to Dock content setup?** For a comprehensive guide with step-by-step examples and troubleshooting, see the [Document and Tool Content Guide](dock-content-guide.md). For modern automatic document management, see [DocumentDock ItemsSource](dock-itemssource.md).

## Using `DataTemplate`

The easiest approach is to define templates for the view models representing your documents and tools. Add them to `App.axaml` or another resource dictionary:

```xaml
<Application.DataTemplates>
  <DataTemplate DataType="vm:DocumentViewModel">
    <views:DocumentView />
  </DataTemplate>
  <DataTemplate DataType="vm:ToolViewModel">
    <views:ToolView />
  </DataTemplate>
</Application.DataTemplates>
```

DockControl will automatically apply these templates when the corresponding view models are activated in the layout.

## Using a `ViewLocator`

More advanced scenarios can use a view locator. The sample `ViewLocator` in `samples/DockMvvmSample` implements `IDataTemplate` and returns views from a dictionary of factory functions:

```csharp
[StaticViewLocator]
public partial class ViewLocator : IDataTemplate
{
    public Control? Build(object? data)
    {
        if (data is null)
        {
            return null;
        }

        var type = data.GetType();

        if (s_views.TryGetValue(type, out var func))
        {
            return func.Invoke();
        }

        throw new Exception($"Unable to create view for type: {type}");
    }

    public bool Match(object? data)
    {
        return data is ObservableObject || data is IDockable;
    }
}
```

The `[StaticViewLocator]` attribute generates the `s_views` dictionary at build time. Register this locator in `App.axaml`:

```xaml
<Application.DataTemplates>
  <local:ViewLocator />
</Application.DataTemplates>
```

With either approach in place, documents and tools will show your custom controls when they are displayed.

## Common Issues

If you're experiencing problems with content setup such as:
- "Unexpected content" errors when adding documents
- Empty or blank document/tool tabs  
- Missing dock types in XAML

See the [Document and Tool Content Guide](dock-content-guide.md) for detailed troubleshooting and multiple setup approaches.

