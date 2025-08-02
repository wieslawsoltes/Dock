# Docking Groups

Docking groups provide a powerful mechanism for controlling which dockables can be docked together. By assigning the same group identifier to related dockables, you can create isolated docking areas where only specific types of content can be arranged together.

## Overview

The `DockGroup` property is available on all dockables (`IDockable`) and allows you to:

- Restrict which dockables can be docked together
- Create separate workspaces for different content types  
- Prevent accidental mixing of incompatible content
- Build complex layouts with isolated functional areas

**Key Behavior:**
- **Both source and target must be in the same "group state"** for docking to be allowed
- **Ungrouped docking**: Both source and target have `null`/empty groups (maximum flexibility)
- **Grouped docking**: Both source and target have the same non-null group identifier (restricted within group)
- **Mixed states are rejected**: One grouped + one ungrouped = docking denied (prevents contamination and breakouts)

## Basic Usage

### Setting DockGroup in Code

```csharp
// Create documents that can only dock with other documents
var document1 = new Document
{
    Id = "Doc1",
    Title = "Document 1",
    DockGroup = "Documents"
};

var document2 = new Document
{
    Id = "Doc2", 
    Title = "Document 2",
    DockGroup = "Documents"
};

// Create tools that can only dock with other tools
var toolA = new Tool
{
    Id = "ToolA",
    Title = "Tool A", 
    DockGroup = "Tools"
};

var toolB = new Tool
{
    Id = "ToolB",
    Title = "Tool B",
    DockGroup = "Tools"
};

// Create an unrestricted dock that only accepts other unrestricted dockables
var unrestrictedDock = new ToolDock
{
    Id = "UnrestrictedDock",
    Title = "Unrestricted Dock",
    DockGroup = null // Only accepts other null-group dockables
};

// Create unrestricted tools that can only dock in unrestricted areas
var unrestrictedTool = new Tool
{
    Id = "UnrestrictedTool",
    Title = "Unrestricted Tool",
    DockGroup = null // Can only dock with other null-group targets
};
```

### Setting DockGroup in XAML

```xml
<RootDock>
  <DocumentDock DockGroup="Documents">
    <Document Id="Doc1" Title="Document 1" DockGroup="Documents" />
    <Document Id="Doc2" Title="Document 2" DockGroup="Documents" />
  </DocumentDock>
  
  <ToolDock DockGroup="LeftTools" Alignment="Left">
    <Tool Id="Explorer" Title="Explorer" DockGroup="LeftTools" />
    <Tool Id="Outline" Title="Outline" DockGroup="LeftTools" />
  </ToolDock>
  
  <ToolDock DockGroup="RightTools" Alignment="Right">
    <Tool Id="Properties" Title="Properties" DockGroup="RightTools" />
    <Tool Id="Toolbox" Title="Toolbox" DockGroup="RightTools" />
  </ToolDock>
</RootDock>
```

## Group Inheritance

Docking groups support inheritance through the ownership hierarchy. If a dockable doesn't have an explicit `DockGroup` set, the system walks up the ownership chain to find an inherited group.

### Example: Inherited Groups

```csharp
// Create a parent dock with a group
var parentDock = new ToolDock
{
    Id = "ParentDock",
    Title = "Parent Tools",
    DockGroup = "InheritedGroup"
};

// Child dockables without explicit groups inherit from parent
var childTool1 = new Tool
{
    Id = "Child1",
    Title = "Child Tool 1",
    DockGroup = null, // Will inherit "InheritedGroup"
    Owner = parentDock
};

var childTool2 = new Tool  
{
    Id = "Child2",
    Title = "Child Tool 2", 
    DockGroup = null, // Will inherit "InheritedGroup"
    Owner = parentDock
};

parentDock.VisibleDockables = new List<IDockable> { childTool1, childTool2 };
```

The inheritance mechanism allows you to set a group on a container dock and have all child dockables automatically belong to that group, simplifying configuration.

## Validation Rules

The docking system enforces the following rules when validating docking operations:

### 1. Same Group Rule
Dockables with the same non-null group can be docked together:

```csharp
// These can dock together
var tool1 = new Tool { DockGroup = "GroupA" };
var tool2 = new Tool { DockGroup = "GroupA" };
```

### 2. Null Group Rule  
Dockables with `null` or empty groups can dock with any other dockable:

```csharp
// This can dock with anything
var flexibleTool = new Tool { DockGroup = null };

// This can also dock with anything  
var anotherFlexible = new Tool { DockGroup = "" };
```

### 3. Different Group Rule
Dockables with different non-null groups cannot be docked together:

```csharp
// These CANNOT dock together
var toolA = new Tool { DockGroup = "GroupA" };
var toolB = new Tool { DockGroup = "GroupB" };
```

### 4. Empty Dock Rule
Empty docks (with no visible dockables) accept any dockable regardless of group:

```csharp
var emptyDock = new ToolDock
{
    DockGroup = "SpecificGroup",
    VisibleDockables = new List<IDockable>() // Empty
};

// Any dockable can be dropped into an empty dock
```

## Practical Examples

### Visual Studio-Style Layout

```csharp
public class MainDockFactory : Factory
{
    public override IRootDock CreateLayout()
    {
        // Document area - only documents can dock here
        var documentsPane = new DocumentDock
        {
            Id = "DocumentsPane",
            Title = "Documents",
            DockGroup = "Documents"
        };

        // Left panel area - only left tools can dock here
        var leftPanelDock = new ToolDock
        {
            Id = "LeftPanelDock", 
            Title = "Left Tools",
            DockGroup = "LeftPanels",
            Alignment = Alignment.Left
        };

        // Right panel area - only right tools can dock here  
        var rightPanelDock = new ToolDock
        {
            Id = "RightPanelDock",
            Title = "Right Tools", 
            DockGroup = "RightPanels",
            Alignment = Alignment.Right
        };

        // Bottom panel area - only bottom tools can dock here
        var bottomPanelDock = new ToolDock
        {
            Id = "BottomPanelDock",
            Title = "Bottom Tools",
            DockGroup = "BottomPanels", 
            Alignment = Alignment.Bottom
        };

        // Create specific tools for each area
        var solutionExplorer = new Tool 
        { 
            Id = "SolutionExplorer", 
            Title = "Solution Explorer",
            DockGroup = "LeftPanels" 
        };
        
        var properties = new Tool 
        { 
            Id = "Properties", 
            Title = "Properties",
            DockGroup = "RightPanels" 
        };
        
        var errorList = new Tool 
        { 
            Id = "ErrorList", 
            Title = "Error List", 
            DockGroup = "BottomPanels" 
        };

        // Assign tools to their respective docks
        leftPanelDock.VisibleDockables = CreateList<IDockable>(solutionExplorer);
        rightPanelDock.VisibleDockables = CreateList<IDockable>(properties);  
        bottomPanelDock.VisibleDockables = CreateList<IDockable>(errorList);

        return CreateRootDock(documentsPane, leftPanelDock, rightPanelDock, bottomPanelDock);
    }
}
```

### Mixed Content with Flexible Areas

```csharp
// Create areas with specific purposes
var codeEditorsGroup = "CodeEditors";
var designersGroup = "Designers"; 
var debuggingGroup = "Debugging";

// Code editors - only code-related content
var codeEditor1 = new Document 
{ 
    Id = "Code1", 
    Title = "Program.cs", 
    DockGroup = codeEditorsGroup 
};

var codeEditor2 = new Document 
{ 
    Id = "Code2", 
    Title = "MainWindow.cs", 
    DockGroup = codeEditorsGroup 
};

// Designers - only design-related content  
var xamlDesigner = new Document 
{ 
    Id = "Designer1", 
    Title = "MainWindow.xaml", 
    DockGroup = designersGroup 
};

var formDesigner = new Document 
{ 
    Id = "Designer2", 
    Title = "Form1.cs[Design]", 
    DockGroup = designersGroup 
};

// Debugging tools - only debugging content
var debugOutput = new Tool 
{ 
    Id = "DebugOutput", 
    Title = "Debug Output", 
    DockGroup = debuggingGroup 
};

var watchWindow = new Tool 
{ 
    Id = "Watch", 
    Title = "Watch", 
    DockGroup = debuggingGroup 
};

// Flexible tools - can dock anywhere
var searchResults = new Tool 
{ 
    Id = "Search", 
    Title = "Search Results", 
    DockGroup = null // Flexible
};
```

## Attached Property Support

The `DockGroup` is also available as an attached property through `DockProperties.DockGroup`. This provides additional flexibility for setting groups in XAML templates and enables inheritance down the visual tree:

```xml
<Window xmlns:dock="clr-namespace:Dock.Settings;assembly=Dock.Settings"
        dock:DockProperties.DockGroup="WindowGroup">
  <!-- All child dockables inherit "WindowGroup" -->
  <DockControl>
    <!-- Layout content -->
  </DockControl>
</Window>
```

### Attached Property Methods

```csharp
// Set group via attached property
DockProperties.SetDockGroup(myControl, "MyGroup");

// Get group via attached property  
string? group = DockProperties.GetDockGroup(myControl);
```

The attached property supports inheritance, so setting it on a parent container automatically applies the group to all child elements unless they have their own explicit group.

## Implementation Details

### DockManager Validation

The `DockManager` class contains the core validation logic for docking groups:

```csharp
// Internal validation method
private static bool ValidateDockingGroups(IDockable sourceDockable, IDockable targetDockable)
{
    var sourceGroup = GetEffectiveDockGroup(sourceDockable);
    var targetGroup = GetEffectiveDockGroup(targetDockable);

    // Both must be in the same "group state"
    var sourceHasGroup = !string.IsNullOrEmpty(sourceGroup);
    var targetHasGroup = !string.IsNullOrEmpty(targetGroup);

    // If both have no group, allow unrestricted docking
    if (!sourceHasGroup && !targetHasGroup)
    {
        return true;
    }

    // If both have groups, they must match exactly
    if (sourceHasGroup && targetHasGroup)
    {
        return string.Equals(sourceGroup, targetGroup, StringComparison.Ordinal);
    }

    // Mixed states (one grouped, one ungrouped) are not allowed
    return false;
}
```

### Effective Group Resolution

The system resolves the effective group for a dockable by walking up the ownership hierarchy:

```csharp
private static string? GetEffectiveDockGroup(IDockable dockable)
{
    var current = dockable;
    
    // Walk up the hierarchy until we find a group or reach the root
    while (current != null)
    {
        if (!string.IsNullOrEmpty(current.DockGroup))
        {
            return current.DockGroup;
        }
        
        current = current.Owner;
    }
    
    return null;
}
```

## Best Practices

### 1. Use Descriptive Group Names
Choose clear, descriptive names for your groups:

```csharp
// Good
DockGroup = "DocumentEditors"
DockGroup = "DebugTools"  
DockGroup = "DesignSurfaces"

// Avoid  
DockGroup = "Group1"
DockGroup = "A"
```

### 2. Keep Groups Logical
Group dockables that logically belong together:

```csharp
// Related editing tools
var groupEditing = "Editing";
var findTool = new Tool { DockGroup = groupEditing };
var replaceTool = new Tool { DockGroup = groupEditing };

// Related debugging tools  
var groupDebug = "Debug";
var breakpointsTool = new Tool { DockGroup = groupDebug };
var watchTool = new Tool { DockGroup = groupDebug };
```

### 3. Use Inheritance Strategically
Set groups on container docks to automatically group all children:

```csharp
var debugDock = new ToolDock
{
    DockGroup = "Debug", // All child tools inherit this
    VisibleDockables = CreateList<IDockable>(
        new Tool { Id = "Breakpoints" }, // Inherits "Debug"
        new Tool { Id = "Watch" },       // Inherits "Debug"  
        new Tool { Id = "Output" }       // Inherits "Debug"
    )
};
```

### 4. Provide Flexible Areas
Include some dockables with `null` groups for maximum flexibility:

```csharp
var flexibleTool = new Tool
{
    Id = "FlexTool",
    Title = "Flexible Tool",
    DockGroup = null // Can dock anywhere
};
```

### 5. Test Group Interactions
Verify that your group setup provides the expected docking behavior:

```csharp
// Test that validation works correctly
var dockManager = new DockManager(new DockService());

var canDock = dockManager.ValidateTool(sourceTool, targetDock, 
    DragAction.Move, DockOperation.Fill, false);

Assert.True(canDock); // or Assert.False for restricted cases
```

## Common Scenarios

### Preventing Document/Tool Mixing
```csharp
// Documents can only dock with other documents
var doc = new Document { DockGroup = "Documents" };

// Tools can only dock with other tools  
var tool = new Tool { DockGroup = "Tools" };

// They cannot be mixed due to different groups
```

### Creating Workspace Modes
```csharp
// Design mode - only design-related content
var designMode = "Design";

// Debug mode - only debugging content
var debugMode = "Debug";

// Switch groups based on current mode
foreach (var dockable in allDockables)
{
    dockable.DockGroup = isDesignMode ? designMode : debugMode;
}
```

### Role-Based Access
```csharp
// Developer tools
var devGroup = "Developer";

// Designer tools  
var designGroup = "Designer";

// Admin tools
var adminGroup = "Administrator";

// Assign based on user role
var userRole = GetCurrentUserRole();
myTool.DockGroup = userRole.ToString();
```

## Troubleshooting

### Common Issues

**Q: My dockables won't dock together even though they have the same group**
A: Check for:
- Typos in group names (case-sensitive)
- Whitespace differences
- One dockable inheriting a different group from its parent

**Q: Dockables with null groups aren't docking**  
A: Verify:
- The target dock's `CanDrop` property is true
- No other restrictions (like `CanDrag` being false)
- The dock operation is valid for the target

**Q: Group inheritance isn't working**
A: Ensure:
- The parent-child relationship is set via the `Owner` property
- The child doesn't have an explicit group that overrides inheritance
- The ownership hierarchy is correct

### Debugging Groups

Use the validation methods to test group compatibility:

```csharp
// Check effective groups
var sourceGroup = GetEffectiveDockGroup(sourceDockable);
var targetGroup = GetEffectiveDockGroup(targetDockable);

Console.WriteLine($"Source: '{sourceGroup}', Target: '{targetGroup}'");

// Test validation
var isValid = ValidateDockingGroups(sourceDockable, targetDockable);
Console.WriteLine($"Can dock: {isValid}");
```

## Related Topics

- [Dockable Properties](dock-dockable-properties.md) - Other properties that control docking behavior
- [Dock Properties](dock-properties.md) - Attached properties for visual controls  
- [DockManager Guide](dock-manager-guide.md) - Core docking logic and validation
- [Layout Panels](dock-layout-panels.md) - Container types that support groups
- [MVVM Guide](dock-mvvm.md) - Using groups in MVVM applications
