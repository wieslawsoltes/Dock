# Issue 973 Analysis: DocumentTemplate Context Binding

## Summary

Issue 973 (opened 2025-12-12, updated 2026-01-22) reports that bindings like `{Binding Context.Content}` inside a `DocumentTemplate` fail to resolve when `ItemsSource` is used, while direct `Document` properties (e.g. `{Binding Title}`) work.

## Root Cause

The `DocumentTemplate` is built with `Document` as the data context. `Document.Context` is typed as `object?`, so Avalonia compiled bindings cannot resolve `Context.Content` at compile time and report "Unable to resolve property or method" errors. This is expected for compiled bindings and is not a runtime binding failure.

## User-Facing Fixes (No Code Changes)

1. Rebind the subtree to the model and set a concrete `x:DataType`:

```xaml
<DocumentTemplate>
  <StackPanel x:DataType="Document">
    <StackPanel DataContext="{Binding Context}"
                x:DataType="models:FileDocument">
      <TextBox Text="{Binding Content}"/>
    </StackPanel>
  </StackPanel>
</DocumentTemplate>
```

2. Use an explicit type cast in the binding path:

```xaml
<TextBox Text="{Binding ((models:FileDocument)Context).Content}"/>
```

3. Disable compiled bindings for that subtree:

```xaml
<StackPanel x:CompileBindings="False">
  <TextBox Text="{Binding Context.Content}"/>
</StackPanel>
```

## Documentation Improvements

Add a short "compiled bindings + Context" note and an example in:

- `docfx/articles/dock-itemssource.md`
- `docfx/articles/dock-content-guide.md`

Update `samples/DockXamlSample/ItemsSourceExample.axaml` with a runnable example using a nested `DataContext="{Binding Context}"` + `x:DataType="models:YourModel"` pattern.

## Optional Code-Level Improvements

Consider adding `[DataType]` to `Dock.Model.Avalonia.Controls.DocumentTemplate.DataType` and `Dock.Model.Avalonia.Controls.Document.DataType` to align with Avalonia `DataTemplate` conventions for better design-time inference. This does not fix `Context.Content` directly but clarifies compiled binding metadata.
