# PR Review Round 3: `pr/1049`

## Scope
- Re-reviewed branch diff and sample theme resources after prior round fixes.

## Findings

### 1. [P2] BrowserTabTheme still has multiple bound DataTemplates without explicit `x:DataType`
**Files**
- `samples/BrowserTabTheme/Styles/Controls/DocumentControl.axaml`
- `samples/BrowserTabTheme/Styles/Controls/DragPreviewControl.axaml`
- `samples/BrowserTabTheme/Styles/Controls/ToolControl.axaml`

**Issue**
Several `DataTemplate` scopes use bindings (`{Binding Title}`, commands, etc.) but omit explicit `x:DataType`.

**Impact**
- We lose compile-time binding validation for those scopes.
- This violates the project’s compiled-binding rule in AGENTS guidance.

**Fix**
Add `x:DataType="core:IDockable"` to each affected `DataTemplate`.
