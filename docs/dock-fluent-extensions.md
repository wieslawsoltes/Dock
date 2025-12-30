### Fluent API for Dock.Model

This guide shows how to build layouts using the fluent extension methods added to `Dock.Model`. The fluent API wraps factory creation and common configuration into chainable calls. You can:
- Create objects and keep them in variables (no `out var` overloads)
- Or use `out var` overloads to capture created instances while continuing a single fluent chain

The `Document` and `Tool` fluent creators live in `Dock.Model.FluentExtensions` and call `IFactory.CreateDocument` and `IFactory.CreateTool`. When you use `Dock.Model.Mvvm.Factory`, those methods return the MVVM `Document` and `Tool` types.

### Step-by-step introduction (single document and tool)

#### 1) Without `out var` overloads (local variables)

```csharp
using Dock.Avalonia.Controls;
using Dock.Model;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm;

var factory = new Dock.Model.Mvvm.Factory();

// Leaf models
var document = factory.Document(d => d.WithId("Document1").WithTitle("Document 1"));
var tool     = factory.Tool(t => t.WithId("Tool1").WithTitle("Tool 1"));

// Docks
var documentDock = factory.DocumentDock(d => d
    .WithId("Documents")
    .WithTitle("Documents")
    .AppendDocument(document)
    .WithActiveDockable(document)
    .WithProportion(0.75));

var toolDock = factory.ToolDock(Alignment.Left, t => t
    .WithId("Tools")
    .WithTitle("Tools")
    .AppendTool(tool)
    .WithActiveDockable(tool)
    .WithProportion(0.25));

var splitter = factory.ProportionalDockSplitter();

var mainLayout = factory.ProportionalDock(Orientation.Horizontal, p => p
    .WithId("MainLayout")
    .Add(toolDock, splitter, documentDock));

var rootDock = factory.RootDock(r => r
    .WithId("Root")
    .WithTitle("Root")
    .Add(mainLayout)
    .WithActiveDockable(mainLayout));

// Host in DockControl
var dockControl = new DockControl
{
    Factory = factory,
    Layout = rootDock,
    InitializeFactory = true,
    InitializeLayout = true
};
```

#### 2) With `out var` overloads (single fluent chain)

```csharp
using Dock.Avalonia.Controls;
using Dock.Model;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm;

var factory = new Dock.Model.Mvvm.Factory();

factory
    .Document(out var document, d => d.WithId("Document1").WithTitle("Document 1"))
    .Tool(out var tool, t => t.WithId("Tool1").WithTitle("Tool 1"))
    .DocumentDock(out var documentDock, d => d
        .WithId("Documents")
        .WithTitle("Documents")
        .AppendDocument(document)
        .WithActiveDockable(document)
        .WithProportion(0.75))
    .ToolDock(out var toolDock, Alignment.Left, t => t
        .WithId("Tools")
        .WithTitle("Tools")
        .AppendTool(tool)
        .WithActiveDockable(tool)
        .WithProportion(0.25))
    .ProportionalDockSplitter(out var splitter)
    .ProportionalDock(out var mainLayout, Orientation.Horizontal, p => p
        .WithId("MainLayout")
        .Add(toolDock, splitter, documentDock))
    .RootDock(out var rootDock, r => r
        .WithId("Root")
        .WithTitle("Root")
        .Add(mainLayout)
        .WithActiveDockable(mainLayout));

var dockControl = new DockControl
{
    Factory = factory,
    Layout = rootDock,
    InitializeFactory = true,
    InitializeLayout = true
};
```

### Advanced example (multiple panes and splitters; code-only MVVM)

#### Without `out var` overloads

```csharp
using Dock.Avalonia.Controls;
using Dock.Model;
using Dock.Model.Core;
using Dock.Model.Mvvm;
using Dock.Model.Mvvm.Controls;

var factory = new Factory();

// Leaf models
var leftTool   = factory.Tool(t => t.WithId("Tool1").WithTitle("Tool 1"));
var bottomTool = factory.Tool(t => t.WithId("Tool2").WithTitle("Output"));
var doc1       = factory.Document(d => d.WithId("Doc1").WithTitle("Document 1"));

// Docks and layout
var documentDock = factory.DocumentDock(d => d
    .WithId("Documents")
    .WithIsCollapsable(false)
    .WithCanCreateDocument(true));

var leftPane = factory.ToolDock(Alignment.Left, t => t
    .WithId("LeftPane")
    .AppendTool(leftTool)
    .WithActiveDockable(leftTool));

var split1 = factory.ProportionalDockSplitter();
var split2 = factory.ProportionalDockSplitter();

var bottomPane = factory.ToolDock(Alignment.Bottom, t => t
    .WithId("BottomPane")
    .AppendTool(bottomTool)
    .WithActiveDockable(bottomTool));

var mainLayout = factory.ProportionalDock(Orientation.Horizontal, p => p
    .Add(leftPane, split1, documentDock, split2, bottomPane));

var root = factory.RootDock(r => r
    .Add(mainLayout)
    .WithDefaultDockable(mainLayout)
    .WithActiveDockable(mainLayout));

// Proportions
leftPane.WithProportion(0.25);
bottomPane.WithProportion(0.25);

// Optional: provide a factory for new documents
if (documentDock is DocumentDock dd)
{
    dd.DocumentFactory = () => new Document
    {
        Id = $"Doc{(documentDock.VisibleDockables?.Count ?? 0) + 1}",
        Title = $"Document {(documentDock.VisibleDockables?.Count ?? 0) + 1}"
    };
}

documentDock.AddDocument(doc1);

// Initialize and host
factory.InitLayout(root);
var dockControl = new DockControl { Factory = factory, Layout = root };
```

#### With `out var` overloads

```csharp
using Dock.Avalonia.Controls;
using Dock.Model;
using Dock.Model.Core;
using Dock.Model.Mvvm;
using Dock.Model.Mvvm.Controls;

var factory = new Factory();

factory
    .Tool(out var leftTool,   t => t.WithId("Tool1").WithTitle("Tool 1"))
    .Tool(out var bottomTool, t => t.WithId("Tool2").WithTitle("Output"))
    .Document(out var doc1,   d => d.WithId("Doc1").WithTitle("Document 1"))
    .DocumentDock(out var documentDock, d => d
        .WithId("Documents")
        .WithIsCollapsable(false)
        .WithCanCreateDocument(true))
    .ToolDock(out var leftPane, Alignment.Left, t => t
        .WithId("LeftPane")
        .AppendTool(leftTool)
        .WithActiveDockable(leftTool))
    .ProportionalDockSplitter(out var split1)
    .ProportionalDockSplitter(out var split2)
    .ToolDock(out var bottomPane, Alignment.Bottom, t => t
        .WithId("BottomPane")
        .AppendTool(bottomTool)
        .WithActiveDockable(bottomTool))
    .ProportionalDock(out var mainLayout, Orientation.Horizontal, p => p
        .Add(leftPane, split1, documentDock, split2, bottomPane))
    .RootDock(out var root, r => r
        .Add(mainLayout)
        .WithDefaultDockable(mainLayout)
        .WithActiveDockable(mainLayout));

// Proportions
leftPane.WithProportion(0.25);
bottomPane.WithProportion(0.25);

// Optional: provide a factory for new documents
if (documentDock is DocumentDock dd)
{
    dd.DocumentFactory = () => new Document
    {
        Id = $"Doc{(documentDock.VisibleDockables?.Count ?? 0) + 1}",
        Title = $"Document {(documentDock.VisibleDockables?.Count ?? 0) + 1}"
    };
}

documentDock.AddDocument(doc1);

// Initialize and host in DockControl
factory.InitLayout(root);
var dockControl = new DockControl { Factory = factory, Layout = root };
```

### Recipes

- **Capture while chaining (out var)**: Use the `IFactory`-returning overloads to continue the chain while capturing the created instance.
- **Add documents/tools**: Use `AppendDocument` and `AppendTool` to avoid name shadowing with instance methods.
- **Configure inline**: Most models support `WithId`, `WithTitle`, `WithProportion`, `WithActiveDockable`, etc., in the configuration `Action<T>`.
- **Leaf creators (MVVM)**: `factory.Document(...)` and `factory.Tool(...)` come from `Dock.Model.Mvvm`.

### Notes

- The fluent API is additive; existing `IFactory.Create*` APIs remain available.
- Prefer the configuration lambda over post-creation property sets when possible for readability.
- For Avalonia hosting, assign the `Factory` and the created `IRootDock` to `DockControl`. Set `InitializeFactory`/`InitializeLayout` if you want automatic initialization.

