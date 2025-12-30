# Dock Caliburn.Micro Guide

This guide explains how to use Dock with Caliburn.Micro. It mirrors the MVVM workflow but uses Caliburn.Micro's `PropertyChangedBase` for change notifications and includes a simple `RelayCommand` helper.

## Packages

Install the Dock packages and a theme:

```bash
dotnet add package Dock.Avalonia
dotnet add package Dock.Model.CaliburMicro
dotnet add package Dock.Avalonia.Themes.Fluent
```

Optional packages for serialization:

```bash
dotnet add package Dock.Serializer.Newtonsoft        # JSON (Newtonsoft.Json)
dotnet add package Dock.Serializer.SystemTextJson    # JSON (System.Text.Json)
```

## View location

Dock uses Avalonia `IDataTemplate` resolution for dockable views. You can keep your Caliburn.Micro conventions or register a simple view locator:

```csharp
using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Dock.Model.Core;

namespace MyDockApp;

public sealed class ViewLocator : IDataTemplate
{
    public Control? Build(object? data)
    {
        if (data is null)
            return null;

        var name = data.GetType().FullName!.Replace("ViewModel", "View");
        var type = Type.GetType(name);

        return type is null
            ? new TextBlock { Text = $"Not Found: {name}" }
            : (Control)Activator.CreateInstance(type)!;
    }

    public bool Match(object? data)
    {
        if (data is null)
            return false;

        if (data is IDockable)
            return true;

        var name = data.GetType().FullName?.Replace("ViewModel", "View");
        return Type.GetType(name) is not null;
    }
}
```

Register it in `App.axaml`:

```xaml
<Application.DataTemplates>
  <local:ViewLocator />
</Application.DataTemplates>

<Application.Styles>
  <FluentTheme />
  <DockFluentTheme />
</Application.Styles>
```

## Dockables and commands

Use the Caliburn.Micro flavored base classes from `Dock.Model.CaliburMicro`. They inherit from `PropertyChangedBase` and expose the same docking properties as the MVVM version.

```csharp
using Dock.Model.CaliburMicro.Controls;
using Dock.Model.CaliburMicro.Core;

public sealed class DocumentViewModel : Document
{
    private string _text = string.Empty;

    public string Text
    {
        get => _text;
        set => Set(ref _text, value);
    }
}

public sealed class ToolViewModel : Tool
{
    private bool _isEnabled;

    public ToolViewModel()
    {
        ToggleCommand = new RelayCommand(() => IsEnabled = !IsEnabled);
    }

    public bool IsEnabled
    {
        get => _isEnabled;
        set => Set(ref _isEnabled, value);
    }

    public RelayCommand ToggleCommand { get; }
}
```

`RelayCommand` lives in `Dock.Model.CaliburMicro.Core` and is the same helper used by the built-in dock commands (go back, go forward, navigate, close). You can use any other `ICommand` implementation if you prefer.

## Factory and layout

Create a factory by inheriting from `Dock.Model.CaliburMicro.Factory` and build your layout in `CreateLayout`:

```csharp
using Dock.Model.CaliburMicro;
using Dock.Model.CaliburMicro.Controls;
using Dock.Model.Core;

public sealed class DockFactory : Factory
{
    public override IRootDock CreateLayout()
    {
        var tool = new ToolViewModel { Id = "Tool1", Title = "Toolbox" };
        var document = new DocumentViewModel { Id = "Doc1", Title = "Welcome" };

        var toolDock = CreateToolDock();
        toolDock.VisibleDockables = CreateList<IDockable>(tool);
        toolDock.ActiveDockable = tool;

        var documentDock = CreateDocumentDock();
        documentDock.VisibleDockables = CreateList<IDockable>(document);
        documentDock.ActiveDockable = document;

        var root = CreateRootDock();
        root.VisibleDockables = CreateList<IDockable>(toolDock, documentDock);
        root.ActiveDockable = documentDock;
        root.DefaultDockable = documentDock;

        return root;
    }
}
```

Initialize the layout and assign it to `DockControl`:

```csharp
public sealed class MainViewModel
{
    public MainViewModel()
    {
        Factory = new DockFactory();
        Layout = Factory.CreateLayout();
        Factory.InitLayout(Layout);
    }

    public IFactory Factory { get; }
    public IRootDock Layout { get; }
}
```

```xaml
<DockControl Layout="{Binding Layout}"
             Factory="{Binding Factory}"
             InitializeFactory="True"
             InitializeLayout="True" />
```

## Notes

- The Caliburn.Micro model uses the same `IFactory` API and dockable interfaces as MVVM and ReactiveUI.
- Use `Dock.Model.CaliburMicro.Controls.Document`, `Tool`, `RootDock`, and related dock types to keep Caliburn.Micro change tracking consistent.

For an overview of other guides see the [documentation index](README.md).
