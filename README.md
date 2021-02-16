# Dock

[![Gitter](https://badges.gitter.im/wieslawsoltes/Dock.svg)](https://gitter.im/wieslawsoltes/Dock?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)

[![Build status](https://dev.azure.com/wieslawsoltes/GitHub/_apis/build/status/Sources/Dock)](https://dev.azure.com/wieslawsoltes/GitHub/_build/latest?definitionId=55)

[![NuGet](https://img.shields.io/nuget/v/Dock.Model.svg)](https://www.nuget.org/packages/Dock.Model)
[![NuGet](https://img.shields.io/nuget/dt/Dock.Model.svg)](https://www.nuget.org/packages/Dock.Model)
[![MyGet](https://img.shields.io/myget/dock-nightly/vpre/Dock.Model.svg?label=myget)](https://www.myget.org/gallery/dock-nightly) 

A docking layout system.

## About

**Dock** is a docking layout system for [Avalonia](https://github.com/AvaloniaUI/Avalonia) applications. Use of Dock is governed by the MIT License.

[![Dock](images/Dock.png)](images/Dock.png)

## Building Dock

First, clone the repository or download the latest zip.
```
git clone https://github.com/wieslawsoltes/Dock.git
```

### Build using .NET Core

Open up a terminal prompt and execute the commands.

Target framework `netstandard2.0`:
```bash
dotnet build src/Dock.Avalonia/Dock.Avalonia.csproj -c Release -f netstandard2.0
```

## NuGet

Dock is delivered as a NuGet package.

You can find the packages here [NuGet](https://www.nuget.org/packages/Dock.Avalonia/) and install the package like this:

```powershell
Install-Package Dock.Avalonia
Install-Package Dock.Model.ReactiveUI
```

or by using nightly build feed:
* Add `https://www.myget.org/F/dock-nightly/api/v2` to your package sources
* Alternative nightly build feed `https://pkgs.dev.azure.com/wieslawsoltes/GitHub/_packaging/Nightly/nuget/v3/index.json`
* Update your package using `Dock` feed

and install the package like this:

```powershell
Install-Package Dock.Avalonia -Pre
Install-Package Dock.Model.ReactiveUI -Pre
```

## Resources

* [GitHub source code repository.](https://github.com/wieslawsoltes/Dock)
* [Sample Dock application.](https://github.com/wieslawsoltes/AvaloniaDockApplication)

## License

Dock is licensed under the [MIT license](LICENSE.TXT).
