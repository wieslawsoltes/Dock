# Dock ReactiveUI Guide

This document mirrors the MVVM instructions for projects that use ReactiveUI.

## Installing

```powershell
Install-Package Dock.Avalonia
Install-Package Dock.Model.ReactiveUI
```

## Creating a layout

Create a factory exactly as with the MVVM version. The `DockFactory` in the ReactiveUI sample constructs the layout in the same way:

```csharp
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
    
            var documentDock = new CustomDocumentDock
            {
                IsCollapsable = false,
                ActiveDockable = document1,
                VisibleDockables = CreateList<IDockable>(document1, document2, document3),
                CanCreateDocument = true
            };
    
            var mainLayout = new ProportionalDock
            {
                Orientation = Orientation.Horizontal,
                VisibleDockables = CreateList<IDockable>
                (
                    leftDock,
                    new ProportionalDockSplitter(),
                    documentDock,
                    new ProportionalDockSplitter(),
                    rightDock
                )
            };
    
            var dashboardView = new DashboardViewModel
            {
                Id = "Dashboard",
                Title = "Dashboard"
            };
    
            var homeView = new HomeViewModel
            {
                Id = "Home",
                Title = "Home",
                ActiveDockable = mainLayout,
                VisibleDockables = CreateList<IDockable>(mainLayout)
            };
    
            var rootDock = CreateRootDock();
    
            rootDock.IsCollapsable = false;
            rootDock.ActiveDockable = dashboardView;
            rootDock.DefaultDockable = homeView;
            rootDock.VisibleDockables = CreateList<IDockable>(dashboardView, homeView);
    
            _documentDock = documentDock;
            _rootDock = rootDock;
                
            return rootDock;
        }
```

The main view model uses ReactiveUI helpers for property change notifications and commands:

```csharp
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
    
            NewLayout = ReactiveCommand.Create(ResetLayout);
        }
    
```

Event subscription works the same way and the sample attaches handlers to all factory events:

```csharp
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
        }
```

## Docking operations

The feature set matches the MVVM version. Methods like `AddDockable`, `MoveDockable`, `PinDockable` or `FloatDockable` are available from `FactoryBase`.

## Events

All the events shown in the MVVM guide are present here as well. Subscribe to them in the same way using ReactiveUI commands or observables.

Use the ReactiveUI sample as a template when building your own layouts.
