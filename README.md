# Dock

[![Gitter](https://badges.gitter.im/wieslawsoltes/Dock.svg)](https://gitter.im/wieslawsoltes/Dock?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)

[![Build status](https://ci.appveyor.com/api/projects/status/d77dburg3ckelv40/branch/master?svg=true)](https://ci.appveyor.com/project/wieslawsoltes/dock/branch/master)
[![Build Status](https://travis-ci.org/wieslawsoltes/Dock.svg?branch=master)](https://travis-ci.org/wieslawsoltes/Dock)
[![CircleCI](https://circleci.com/gh/wieslawsoltes/Dock/tree/master.svg?style=svg)](https://circleci.com/gh/wieslawsoltes/Dock/tree/master)

[![NuGet](https://img.shields.io/nuget/v/Dock.Model.svg)](https://www.nuget.org/packages/Dock.Model)
[![MyGet](https://img.shields.io/myget/dock-nightly/vpre/Dock.Model.svg?label=myget)](https://www.myget.org/gallery/dock-nightly) 

A docking layout system.

## About

**Dock** is a docking layout system for [Avalonia](https://github.com/AvaloniaUI/Avalonia) applications. Use of Dock is governed by the MIT License.

## Demo

| Version               | Framework      | Model         | Download                                                                                                                                                                                                 |
|-----------------------|----------------|---------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Windows 7 8.1 10 x64  | netcoreapp2.1  | INPC          | [AvaloniaDemo.INPC-netcoreapp2.1-win7-x64.zip](https://ci.appveyor.com/api/projects/wieslawsoltes/Dock/artifacts/artifacts/AvaloniaDemo.INPC-netcoreapp2.1-win7-x64.zip?branch=master)                   |
| Windows 7 8.1 10 x64  | netcoreapp2.1  | ReactiveUI    | [AvaloniaDemo.ReactiveUI-netcoreapp2.1-win7-x64.zip](https://ci.appveyor.com/api/projects/wieslawsoltes/Dock/artifacts/artifacts/AvaloniaDemo.ReactiveUI-netcoreapp2.1-win7-x64.zip?branch=master)       |

## Building Dock

First, clone the repository or download the latest zip.
```
git clone https://github.com/wieslawsoltes/Dock.git
git submodule update --init --recursive
```

### Build using IDE

* [Visual Studio Community 2017](https://www.visualstudio.com/pl/vs/community/) for `Windows` builds.

Open `Dock.sln` in selected IDE and run `Build` command.

### Build using .NET Core

Open up a terminal prompt and execute the commands.

Target framework `netstandard2.0`:
```
dotnet build src/Dock.Avalonia/Dock.Avalonia.csproj -c Release -f netstandard2.0
```

## NuGet

Dock is delivered as a NuGet package.

You can find the packages here [NuGet](https://www.nuget.org/packages/Dock.Avalonia/) and install the package like this:

`Install-Package Dock.Avalonia`

or by using nightly build feed:
* Add `https://www.myget.org/F/dock-nightly/api/v2` to your package sources
* Update your package using `Dock` feed

and install the package like this:

`Install-Package Dock.Avalonia -Pre`

### NuGet Packages

* [Dock.Model](https://www.nuget.org/packages/Dock.Model/) - Core docking library.
* [Dock.Model.INPC](https://www.nuget.org/packages/Dock.Model.INPC/) - Core docking library implementation using INPC.
* [Dock.Model.ReactiveUI](https://www.nuget.org/packages/Dock.Model.ReactiveUI/) - Core docking library implementation using ReactiveUI.
* [Dock.Avalonia](https://www.nuget.org/packages/Dock.Avalonia/) - Avalonia docking implementation.

### Package Dependencies

* [Avalonia](https://www.nuget.org/packages/Avalonia/)
* [ReactiveUI](https://www.nuget.org/packages/ReactiveUI/)
* [System.Reactive](https://www.nuget.org/packages/System.Reactive/)
* [System.Reactive.Core](https://www.nuget.org/packages/System.Reactive.Core/)
* [System.Reactive.Interfaces](https://www.nuget.org/packages/System.Reactive.Interfaces/)
* [System.Reactive.Linq](https://www.nuget.org/packages/System.Reactive.Linq/)
* [System.Reactive.PlatformServices](https://www.nuget.org/packages/System.Reactive.PlatformServices/)
* [Serilog](https://www.nuget.org/packages/Serilog/)
* [Sprache](https://www.nuget.org/packages/Sprache/)

### Package Sources

* https://api.nuget.org/v3/index.json
* https://www.myget.org/F/avalonia-ci/api/v2
* https://www.myget.org/F/dock-nightly/api/v2

## Resources

* [GitHub source code repository.](https://github.com/wieslawsoltes/Dock)

## License

Dock is licensed under the [MIT license](LICENSE.TXT).
