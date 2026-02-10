# Dock for Avalonia

Dock is a docking layout system for Avalonia applications. It provides document and tool panes, floating windows, docking targets, and layout persistence, with multiple MVVM integrations and both XAML and code-first layout options.

## Highlights

- Document and tool docking with configurable rules.
- Floating windows, docking targets, and drag behaviors.
- Layout persistence and restoration.
- Theme support and overlay customization.
- Integrations for MVVM, ReactiveUI, Prism, and more.

## Architecture

Dock separates the model from the view:

- Dock.Model defines layout state, docking rules, and serialization.
- Dock.Avalonia provides the Avalonia controls and visual behavior.
- Framework integrations add view model bases and patterns for common MVVM stacks.

This design keeps the layout engine reusable and lets you swap view models or frameworks without rewriting docking logic.

## Install

```bash
dotnet add package Dock.Avalonia
dotnet add package Dock.Model.Mvvm
dotnet add package Dock.Avalonia.Themes.Fluent
```

## Get started

Recommended path:

1. [Quick start](articles/quick-start.md) to build a minimal layout.
2. [Document and tool content guide](articles/dock-content-guide.md) for content setup.
3. [Framework integration](articles/README.md) to choose the MVVM stack that matches your app.

## NuGet packages

| NuGet | Package | Downloads |
| --- | --- | --- |
| [![NuGet](https://img.shields.io/nuget/v/Dock.Avalonia.svg)](https://www.nuget.org/packages/Dock.Avalonia) | [`Dock.Avalonia`](https://www.nuget.org/packages/Dock.Avalonia) | [![Downloads](https://img.shields.io/nuget/dt/Dock.Avalonia.svg)](https://www.nuget.org/packages/Dock.Avalonia) |
| [![NuGet](https://img.shields.io/nuget/v/Dock.Avalonia.Diagnostics.svg)](https://www.nuget.org/packages/Dock.Avalonia.Diagnostics) | [`Dock.Avalonia.Diagnostics`](https://www.nuget.org/packages/Dock.Avalonia.Diagnostics) | [![Downloads](https://img.shields.io/nuget/dt/Dock.Avalonia.Diagnostics.svg)](https://www.nuget.org/packages/Dock.Avalonia.Diagnostics) |
| [![NuGet](https://img.shields.io/nuget/v/Dock.Avalonia.Themes.Fluent.svg)](https://www.nuget.org/packages/Dock.Avalonia.Themes.Fluent) | [`Dock.Avalonia.Themes.Fluent`](https://www.nuget.org/packages/Dock.Avalonia.Themes.Fluent) | [![Downloads](https://img.shields.io/nuget/dt/Dock.Avalonia.Themes.Fluent.svg)](https://www.nuget.org/packages/Dock.Avalonia.Themes.Fluent) |
| [![NuGet](https://img.shields.io/nuget/v/Dock.Avalonia.Themes.Simple.svg)](https://www.nuget.org/packages/Dock.Avalonia.Themes.Simple) | [`Dock.Avalonia.Themes.Simple`](https://www.nuget.org/packages/Dock.Avalonia.Themes.Simple) | [![Downloads](https://img.shields.io/nuget/dt/Dock.Avalonia.Themes.Simple.svg)](https://www.nuget.org/packages/Dock.Avalonia.Themes.Simple) |
| [![NuGet](https://img.shields.io/nuget/v/Dock.Controls.ProportionalStackPanel.svg)](https://www.nuget.org/packages/Dock.Controls.ProportionalStackPanel) | [`Dock.Controls.ProportionalStackPanel`](https://www.nuget.org/packages/Dock.Controls.ProportionalStackPanel) | [![Downloads](https://img.shields.io/nuget/dt/Dock.Controls.ProportionalStackPanel.svg)](https://www.nuget.org/packages/Dock.Controls.ProportionalStackPanel) |
| [![NuGet](https://img.shields.io/nuget/v/Dock.Controls.Recycling.svg)](https://www.nuget.org/packages/Dock.Controls.Recycling) | [`Dock.Controls.Recycling`](https://www.nuget.org/packages/Dock.Controls.Recycling) | [![Downloads](https://img.shields.io/nuget/dt/Dock.Controls.Recycling.svg)](https://www.nuget.org/packages/Dock.Controls.Recycling) |
| [![NuGet](https://img.shields.io/nuget/v/Dock.Controls.Recycling.Model.svg)](https://www.nuget.org/packages/Dock.Controls.Recycling.Model) | [`Dock.Controls.Recycling.Model`](https://www.nuget.org/packages/Dock.Controls.Recycling.Model) | [![Downloads](https://img.shields.io/nuget/dt/Dock.Controls.Recycling.Model.svg)](https://www.nuget.org/packages/Dock.Controls.Recycling.Model) |
| [![NuGet](https://img.shields.io/nuget/v/Dock.MarkupExtension.svg)](https://www.nuget.org/packages/Dock.MarkupExtension) | [`Dock.MarkupExtension`](https://www.nuget.org/packages/Dock.MarkupExtension) | [![Downloads](https://img.shields.io/nuget/dt/Dock.MarkupExtension.svg)](https://www.nuget.org/packages/Dock.MarkupExtension) |
| [![NuGet](https://img.shields.io/nuget/v/Dock.Model.svg)](https://www.nuget.org/packages/Dock.Model) | [`Dock.Model`](https://www.nuget.org/packages/Dock.Model) | [![Downloads](https://img.shields.io/nuget/dt/Dock.Model.svg)](https://www.nuget.org/packages/Dock.Model) |
| [![NuGet](https://img.shields.io/nuget/v/Dock.Model.Avalonia.svg)](https://www.nuget.org/packages/Dock.Model.Avalonia) | [`Dock.Model.Avalonia`](https://www.nuget.org/packages/Dock.Model.Avalonia) | [![Downloads](https://img.shields.io/nuget/dt/Dock.Model.Avalonia.svg)](https://www.nuget.org/packages/Dock.Model.Avalonia) |
| [![NuGet](https://img.shields.io/nuget/v/Dock.Model.CaliburMicro.svg)](https://www.nuget.org/packages/Dock.Model.CaliburMicro) | [`Dock.Model.CaliburMicro`](https://www.nuget.org/packages/Dock.Model.CaliburMicro) | [![Downloads](https://img.shields.io/nuget/dt/Dock.Model.CaliburMicro.svg)](https://www.nuget.org/packages/Dock.Model.CaliburMicro) |
| [![NuGet](https://img.shields.io/nuget/v/Dock.Model.Inpc.svg)](https://www.nuget.org/packages/Dock.Model.Inpc) | [`Dock.Model.Inpc`](https://www.nuget.org/packages/Dock.Model.Inpc) | [![Downloads](https://img.shields.io/nuget/dt/Dock.Model.Inpc.svg)](https://www.nuget.org/packages/Dock.Model.Inpc) |
| [![NuGet](https://img.shields.io/nuget/v/Dock.Model.Mvvm.svg)](https://www.nuget.org/packages/Dock.Model.Mvvm) | [`Dock.Model.Mvvm`](https://www.nuget.org/packages/Dock.Model.Mvvm) | [![Downloads](https://img.shields.io/nuget/dt/Dock.Model.Mvvm.svg)](https://www.nuget.org/packages/Dock.Model.Mvvm) |
| [![NuGet](https://img.shields.io/nuget/v/Dock.Model.Prism.svg)](https://www.nuget.org/packages/Dock.Model.Prism) | [`Dock.Model.Prism`](https://www.nuget.org/packages/Dock.Model.Prism) | [![Downloads](https://img.shields.io/nuget/dt/Dock.Model.Prism.svg)](https://www.nuget.org/packages/Dock.Model.Prism) |
| [![NuGet](https://img.shields.io/nuget/v/Dock.Model.ReactiveProperty.svg)](https://www.nuget.org/packages/Dock.Model.ReactiveProperty) | [`Dock.Model.ReactiveProperty`](https://www.nuget.org/packages/Dock.Model.ReactiveProperty) | [![Downloads](https://img.shields.io/nuget/dt/Dock.Model.ReactiveProperty.svg)](https://www.nuget.org/packages/Dock.Model.ReactiveProperty) |
| [![NuGet](https://img.shields.io/nuget/v/Dock.Model.ReactiveUI.svg)](https://www.nuget.org/packages/Dock.Model.ReactiveUI) | [`Dock.Model.ReactiveUI`](https://www.nuget.org/packages/Dock.Model.ReactiveUI) | [![Downloads](https://img.shields.io/nuget/dt/Dock.Model.ReactiveUI.svg)](https://www.nuget.org/packages/Dock.Model.ReactiveUI) |
| [![NuGet](https://img.shields.io/nuget/v/Dock.Model.ReactiveUI.Services.svg)](https://www.nuget.org/packages/Dock.Model.ReactiveUI.Services) | [`Dock.Model.ReactiveUI.Services`](https://www.nuget.org/packages/Dock.Model.ReactiveUI.Services) | [![Downloads](https://img.shields.io/nuget/dt/Dock.Model.ReactiveUI.Services.svg)](https://www.nuget.org/packages/Dock.Model.ReactiveUI.Services) |
| [![NuGet](https://img.shields.io/nuget/v/Dock.Model.ReactiveUI.Services.Avalonia.svg)](https://www.nuget.org/packages/Dock.Model.ReactiveUI.Services.Avalonia) | [`Dock.Model.ReactiveUI.Services.Avalonia`](https://www.nuget.org/packages/Dock.Model.ReactiveUI.Services.Avalonia) | [![Downloads](https://img.shields.io/nuget/dt/Dock.Model.ReactiveUI.Services.Avalonia.svg)](https://www.nuget.org/packages/Dock.Model.ReactiveUI.Services.Avalonia) |
| [![NuGet](https://img.shields.io/nuget/v/Dock.Serializer.Newtonsoft.svg)](https://www.nuget.org/packages/Dock.Serializer.Newtonsoft) | [`Dock.Serializer.Newtonsoft`](https://www.nuget.org/packages/Dock.Serializer.Newtonsoft) | [![Downloads](https://img.shields.io/nuget/dt/Dock.Serializer.Newtonsoft.svg)](https://www.nuget.org/packages/Dock.Serializer.Newtonsoft) |
| [![NuGet](https://img.shields.io/nuget/v/Dock.Serializer.Protobuf.svg)](https://www.nuget.org/packages/Dock.Serializer.Protobuf) | [`Dock.Serializer.Protobuf`](https://www.nuget.org/packages/Dock.Serializer.Protobuf) | [![Downloads](https://img.shields.io/nuget/dt/Dock.Serializer.Protobuf.svg)](https://www.nuget.org/packages/Dock.Serializer.Protobuf) |
| [![NuGet](https://img.shields.io/nuget/v/Dock.Serializer.SystemTextJson.svg)](https://www.nuget.org/packages/Dock.Serializer.SystemTextJson) | [`Dock.Serializer.SystemTextJson`](https://www.nuget.org/packages/Dock.Serializer.SystemTextJson) | [![Downloads](https://img.shields.io/nuget/dt/Dock.Serializer.SystemTextJson.svg)](https://www.nuget.org/packages/Dock.Serializer.SystemTextJson) |
| [![NuGet](https://img.shields.io/nuget/v/Dock.Serializer.Xml.svg)](https://www.nuget.org/packages/Dock.Serializer.Xml) | [`Dock.Serializer.Xml`](https://www.nuget.org/packages/Dock.Serializer.Xml) | [![Downloads](https://img.shields.io/nuget/dt/Dock.Serializer.Xml.svg)](https://www.nuget.org/packages/Dock.Serializer.Xml) |
| [![NuGet](https://img.shields.io/nuget/v/Dock.Serializer.Yaml.svg)](https://www.nuget.org/packages/Dock.Serializer.Yaml) | [`Dock.Serializer.Yaml`](https://www.nuget.org/packages/Dock.Serializer.Yaml) | [![Downloads](https://img.shields.io/nuget/dt/Dock.Serializer.Yaml.svg)](https://www.nuget.org/packages/Dock.Serializer.Yaml) |
| [![NuGet](https://img.shields.io/nuget/v/Dock.Settings.svg)](https://www.nuget.org/packages/Dock.Settings) | [`Dock.Settings`](https://www.nuget.org/packages/Dock.Settings) | [![Downloads](https://img.shields.io/nuget/dt/Dock.Settings.svg)](https://www.nuget.org/packages/Dock.Settings) |

## Documentation

- [Documentation index](articles/README.md)
- [Quick start](articles/quick-start.md)
- [Document and tool content guide](articles/dock-content-guide.md)
- [Document and tool ItemsSource guide](articles/dock-itemssource.md)
- [API documentation](api/index.md)

## Samples

- [Sample applications](https://github.com/wieslawsoltes/Dock/tree/master/samples)

## GitHub

Dock is developed in the open. Visit the repository for source code, issues, and contributions:
[Dock on GitHub](https://github.com/wieslawsoltes/Dock).

## License

Dock is licensed under the [MIT License](https://github.com/wieslawsoltes/Dock/blob/master/LICENSE.TXT).
