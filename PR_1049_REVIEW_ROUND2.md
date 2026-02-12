# PR Review Round 2: `pr/1049`

## Scope
- Reviewed branch changes against `origin/master...HEAD` after prior fixes.
- Focused on sample-level correctness, maintainability, and repository portability.

## Findings

### 1. [P2] BrowserTabTheme runtime replaces XAML `DockControl` and keeps avoidable UI code in view code-behind
**Files**
- `samples/BrowserTabTheme/App.axaml.cs`
- `samples/BrowserTabTheme/MainWindow.axaml.cs`

**Issue**
- `MainWindow.axaml` defines a named `DockControl`, but `App.axaml.cs` overwrites `mainWindow.Content` with a newly created `DockControl`.
- `MainWindow.axaml.cs` also sets `Title`, `Width`, and `Height` even though they are already declared in XAML.

**Impact**
- The XAML-defined control is effectively discarded at runtime, reducing maintainability and making future XAML changes easy to miss.
- Redundant view code-behind UI property assignments increase divergence risk.

**Fix**
- Use the `DockControl` declared in XAML and configure it from app composition code.
- Keep view code-behind to `InitializeComponent` only.

---

### 2. [P3] Sample documentation uses machine-specific absolute paths
**Files**
- `samples/BrowserTabTheme/README.md`
- `samples/BrowserTabTheme/port.md`

**Issue**
- Build/run commands use `/Volumes/SSD/repos/StackWich/...`, which is not valid for other contributors.

**Impact**
- Copy/paste instructions fail for most users and CI-like local validation workflows.

**Fix**
- Replace absolute paths with repository-relative commands.
