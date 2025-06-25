# Dock MVVM Guide

This guide explains how to get started with the MVVM version of Dock and describes the available features.

## Installing

Add the packages for Dock and the MVVM model to your project:

```powershell
Install-Package Dock.Avalonia
Install-Package Dock.Model.Mvvm
```

## Creating a layout

You usually derive from `Factory` and build the layout by composing docks. The sample `DockFactory` shows how tools and documents are created and added to a root layout:

```csharp
```
        public override IRootDock CreateLayout()
        {
            var document1 = new DocumentViewModel {Id = "Document1", Title = "Document1"};
            var document2 = new DocumentViewModel {Id = "Document2", Title = "Document2"};
            var document3 = new DocumentViewModel {Id = "Document3", Title = "Document3", CanClose = true};
            var tool1 = new Tool1ViewModel {Id = "Tool1", Title = "Tool1"};
            var tool2 = new Tool2ViewModel {Id = "Tool2", Title = "Tool2"};
            var tool3 = new Tool3ViewModel {Id = "Tool3", Title = "Tool3"};
            var tool4 = new Tool4ViewModel {Id = "Tool4", Title = "Tool4"};
            var tool5 = new Tool5ViewModel {Id = "Tool5", Title = "Tool5"};
            var tool6 = new Tool6ViewModel {Id = "Tool6", Title = "Tool6", CanClose = true, CanPin = true};
            var tool7 = new Tool7ViewModel {Id = "Tool7", Title = "Tool7", CanClose = false, CanPin = false};
            var tool8 = new Tool8ViewModel {Id = "Tool8", Title = "Tool8", CanClose = false, CanPin = true};
    
            var leftDock = new ProportionalDock
            {
                Proportion = 0.25,
                Orientation = Orientation.Vertical,
                ActiveDockable = null,
                VisibleDockables = CreateList<IDockable>
                (
                    new ToolDock
                    {
                        ActiveDockable = tool1,
                        VisibleDockables = CreateList<IDockable>(tool1, tool2),
                        Alignment = Alignment.Left
                    },
                    new ProportionalDockSplitter(),
                    new ToolDock
                    {
                        ActiveDockable = tool3,
                        VisibleDockables = CreateList<IDockable>(tool3, tool4),
                        Alignment = Alignment.Bottom
                    }
                )
            };
    
            var rightDock = new ProportionalDock
            {
                Proportion = 0.25,
                Orientation = Orientation.Vertical,
                ActiveDockable = null,
                VisibleDockables = CreateList<IDockable>
                (
                    new ToolDock
                    {
                        ActiveDockable = tool5,
                        VisibleDockables = CreateList<IDockable>(tool5, tool6),
                        Alignment = Alignment.Top,
                        GripMode = GripMode.Hidden
                    },
                    new ProportionalDockSplitter(),
                    new ToolDock
                    {
                        ActiveDockable = tool7,
                        VisibleDockables = CreateList<IDockable>(tool7, tool8),
                        Alignment = Alignment.Right,
                        GripMode = GripMode.AutoHide
                    }
                )
            };
```

```

Once the factory is defined, the view model creates and initializes the layout. The sample shows this logic:

```csharp
```
        public MainWindowViewModel()
        {
            _factory = new DockFactory(new DemoData());
    
            DebugFactoryEvents(_factory);
    
            Layout = _factory?.CreateLayout();
            if (Layout is { })
            {
                _factory?.InitLayout(Layout);
                if (Layout is { } root)
                {
                    root.Navigate.Execute("Home");
                }
            }
    
            NewLayout = new RelayCommand(ResetLayout);
        }
```

The helper method `DebugFactoryEvents` from the sample subscribes to a wide range of factory events so you can react to changes:

```csharp
```
        private void DebugFactoryEvents(IFactory factory)
        {
            factory.ActiveDockableChanged += (_, args) =>
            {
                Debug.WriteLine($"[ActiveDockableChanged] Title='{args.Dockable?.Title}'");
            };
    
            factory.FocusedDockableChanged += (_, args) =>
            {
                Debug.WriteLine($"[FocusedDockableChanged] Title='{args.Dockable?.Title}'");
            };
    
            factory.DockableAdded += (_, args) =>
            {
                Debug.WriteLine($"[DockableAdded] Title='{args.Dockable?.Title}'");
            };
    
            factory.DockableRemoved += (_, args) =>
            {
                Debug.WriteLine($"[DockableRemoved] Title='{args.Dockable?.Title}'");
            };
    
            factory.DockableClosed += (_, args) =>
            {
                Debug.WriteLine($"[DockableClosed] Title='{args.Dockable?.Title}'");
            };
    
            factory.DockableMoved += (_, args) =>
            {
                Debug.WriteLine($"[DockableMoved] Title='{args.Dockable?.Title}'");
            };
    
            factory.DockableSwapped += (_, args) =>
            {
                Debug.WriteLine($"[DockableSwapped] Title='{args.Dockable?.Title}'");
            };
    
            factory.DockablePinned += (_, args) =>
            {
                Debug.WriteLine($"[DockablePinned] Title='{args.Dockable?.Title}'");
            };
    
            factory.DockableUnpinned += (_, args) =>
            {
                Debug.WriteLine($"[DockableUnpinned] Title='{args.Dockable?.Title}'");
            };
    
            factory.WindowOpened += (_, args) =>
            {
                Debug.WriteLine($"[WindowOpened] Title='{args.Window?.Title}'");
            };
    
            factory.WindowClosed += (_, args) =>
            {
                Debug.WriteLine($"[WindowClosed] Title='{args.Window?.Title}'");
            };
    
            factory.WindowClosing += (_, args) =>
            {
                // NOTE: Set to True to cancel window closing.
    #if false
                    args.Cancel = true;
    #endif
                Debug.WriteLine($"[WindowClosing] Title='{args.Window?.Title}', Cancel={args.Cancel}");
            };
    
            factory.WindowAdded += (_, args) =>
            {
                Debug.WriteLine($"[WindowAdded] Title='{args.Window?.Title}'");
            };
    
            factory.WindowRemoved += (_, args) =>
            {
                Debug.WriteLine($"[WindowRemoved] Title='{args.Window?.Title}'");
            };
    
            factory.WindowMoveDragBegin += (_, args) =>
            {
                // NOTE: Set to True to cancel window dragging.
    #if false
                    args.Cancel = true;
    #endif
                Debug.WriteLine($"[WindowMoveDragBegin] Title='{args.Window?.Title}', Cancel={args.Cancel}, X='{args.Window?.X}', Y='{args.Window?.Y}'");
            };
    
            factory.WindowMoveDrag += (_, args) =>
            {
                Debug.WriteLine($"[WindowMoveDrag] Title='{args.Window?.Title}', X='{args.Window?.X}', Y='{args.Window?.Y}");
            };
    
            factory.WindowMoveDragEnd += (_, args) =>
            {
                Debug.WriteLine($"[WindowMoveDragEnd] Title='{args.Window?.Title}', X='{args.Window?.X}', Y='{args.Window?.Y}");
            };
```

## Docking operations

`FactoryBase` exposes many methods to manipulate the layout at runtime. Some of the most useful ones are:

- `AddDockable`, `InsertDockable` and `RemoveDockable` to manage content.
- `MoveDockable` and `SwapDockable` for rearranging items.
- `PinDockable`/`UnpinDockable` to keep tools in the pinned area.
- `FloatDockable` to open a dockable in a separate window.
- Commands such as `CloseDockable`, `CloseOtherDockables` or `CloseAllDockables`.

The implementation of these features lives in `FactoryBase.Dockable`. You can refer to the source for details.

## Events

`FactoryBase` also publishes events for most actions. They allow you to observe changes in active dockable, pinned state or window management. The method above subscribes to events like `ActiveDockableChanged`, `DockableAdded`, `WindowOpened` and many more.

## Next steps

Use the MVVM sample as a starting point for your application. You can extend the factory to create custom docks, documents and tools.
