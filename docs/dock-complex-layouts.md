# Dock Complex Layout Tutorials

This document walks through building more advanced Dock setups. It extends the basic guides with end-to-end examples that cover multi-window layouts and plug-in based scenarios. Each section also shows how to persist and reload the resulting layout.

## Multi-window layouts

The default factory can open any dockable in a separate window. This allows you to split your application across multiple top level windows.

1. **Create the project and install packages**

   ```bash
   dotnet new avalonia.app -o MultiDockApp
   cd MultiDockApp
   dotnet add package Dock.Avalonia
   dotnet add package Dock.Model.Mvvm
   dotnet add package Dock.Serializer
   ```

2. **Define a custom factory**

   Derive from `DockFactory` and build the initial layout. Floating windows are created by calling `FloatDockable` on a dockable or `IRootDock`.

   ```csharp
   public class MultiFactory : Factory
   {
       public override IRootDock CreateLayout()
       {
           var doc1 = new DocumentViewModel { Id = "Doc1", Title = "Document" };
           return CreateRootDock().With(root =>
           {
               root.VisibleDockables = CreateList<IDockable>(
                   new DocumentDock { VisibleDockables = CreateList<IDockable>(doc1), ActiveDockable = doc1 }
               );
           });
       }
   }
   ```

3. **Open a floating window**

   ```csharp
   var floating = factory.CreateLayout();
   factory.InitLayout(floating);
   factory.FloatDockable(rootLayout, floating);
   ```

   Override `CreateWindowFrom` to set the title or dimensions of the new window.

4. **Persist and restore**

   ```csharp
   await using var write = File.OpenWrite("layout.json");
   _serializer.Save(write, dockControl.Layout);
   
   await using var read = File.OpenRead("layout.json");
   var layout = _serializer.Load<IDock?>(read);
   if (layout is { })
   {
       dockControl.Layout = layout;
       _dockState.Restore(layout);
   }
   ```

## Plug-in based layouts

Applications can load additional dockables from plug-ins at runtime. Plug-ins typically expose documents or tools that the host adds to the layout.

1. **Define a plug-in contract**

   ```csharp
   public interface IPlugin
   {
       IDockable CreateDockable();
   }
   ```

2. **Load plug-ins**

   ```csharp
   var assembly = Assembly.LoadFrom(path);
   var plugins = assembly
       .GetTypes()
       .Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsAbstract)
       .Select(t => (IPlugin)Activator.CreateInstance(t)!);
   foreach (var plugin in plugins)
   {
       factory.AddDockable(rootLayout, plugin.CreateDockable());
   }
   ```

3. **Save the layout**

   Use the same serializer calls shown above so that plug-in windows reappear on the next run.

## Summary

Multi-window layouts and plug-in loaded dockables follow the same workflow as the basic examples. After creating or loading dockables, call `InitLayout` once and persist the state with `DockSerializer` and `DockState`.

