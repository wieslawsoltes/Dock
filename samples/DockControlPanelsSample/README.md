# Dock Control Panels Sample

This sample showcases the usage of different dock panel controls available in the Dock library, demonstrating how documents and tool docks can be combined using various layout mechanisms. The sample uses a tabbed interface where each tab demonstrates a different dock control type in full detail.

## Featured Dock Controls

### 1. ProportionalDockControl with ProportionalStackPanelSplitter
- **Layout**: Horizontal arrangement with proportional sizing
- **Features**: 
  - Resizable splitters between panels
  - Left tool dock (Solution Explorer)
  - Center document area with multiple tabs
  - Right tool dock (Properties)
- **Use case**: Classic IDE layout with adjustable panel proportions

### 2. StackDockControl
- **Layout**: Linear horizontal arrangement with consistent spacing
- **Features**:
  - Multiple toolbar sections
  - Fixed spacing between elements
  - Main document area
  - Sidebar tool panel
- **Use case**: Toolbar arrangements and fixed-order layouts

### 3. GridDockControl with GridSplitter
- **Layout**: Grid-based layout with resizable columns and rows
- **Features**:
  - Top toolbar spanning all columns
  - Left explorer panel
  - Center document area with multiple tabs
  - Right inspector panel
  - Resizable grid splitters
- **Use case**: Complex layouts requiring precise grid control

### 4. WrapDockControl
- **Layout**: Horizontal flow with automatic wrapping
- **Features**:
  - Multiple tool panels that wrap to new lines
  - Responsive layout based on available space
  - Document area with minimum width constraints
- **Use case**: Responsive layouts and dashboard-style arrangements

### 5. UniformGridDockControl
- **Layout**: Equal-sized cells in a 2x3 grid
- **Features**:
  - Six equal-sized cells
  - Mix of tool docks and document docks
  - Uniform spacing and sizing
- **Use case**: Dashboard layouts with consistent cell sizes

### 6. DockDockControl
- **Layout**: Traditional edge-docking behavior
- **Features**:
  - Top menu bar
  - Bottom status bar
  - Left toolbox
  - Right properties panel
  - Center document area fills remaining space
- **Use case**: Classic application layouts with peripheral panels

## Visual Styling

The sample includes custom styling to differentiate between:
- **Tool Content** (light green background) - Represents tool panels like explorers, properties, toolboxes
- **Document Content** (light blue background) - Represents document areas with editable content
- **Sample Containers** - Bordered sections for each dock type demonstration

## Building and Running

```bash
dotnet build
dotnet run
```

The application will open with a tabbed interface showing all six dock control types. Each tab contains a full-sized example of the dock control type, making it easy to interact with each layout and observe their different behaviors.

## Key Learning Points

1. **ProportionalDock** is ideal for resizable layouts where proportional sizing is important
2. **StackDock** works well for toolbars and linear arrangements
3. **GridDock** provides the most control for complex, grid-based layouts
4. **WrapDock** is perfect for responsive, flowing layouts
5. **UniformGridDock** ensures consistent sizing in grid arrangements
6. **DockDock** provides traditional docking behavior familiar from desktop applications

Each control can contain any combination of `DocumentDock`, `ToolDock`, and other dock types, making them highly composable for building complex application layouts.