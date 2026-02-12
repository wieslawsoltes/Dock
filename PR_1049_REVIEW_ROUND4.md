# PR Review Round 4: `pr/1049`

## Scope
- Re-reviewed branch behavior around floating-window owner policy and owner-mode resolution.

## Findings

### 1. [P1] `NeverOwned` policy is not enforced for windows using `OwnerMode=Default` with global default owner modes
**File**
- `src/Dock.Avalonia/Controls/HostWindow.axaml.cs`

**Issue**
When `IDockWindow.OwnerMode == Default`, code maps to `DockSettings.DefaultFloatingWindowOwnerMode`. For modes like `ParentWindow`, `DockableWindow`, or `RootWindow`, ownership can still be assigned even when `DockSettings.FloatingWindowOwnerPolicy == NeverOwned`.

**Impact**
- Violates the global owner policy contract (`NeverOwned`) for floating windows.
- Causes inconsistent behavior: `NeverOwned` is respected in some paths but bypassed in global-default-owner-mode paths.

**Fix**
- In `ResolveOwnerWindow`, if the window uses global default owner mode and `DockSettings.ShouldUseOwnerForFloatingWindows()` is `false`, return `null` before owner resolution.
- Add regression tests verifying that `NeverOwned` suppresses owner assignment for `OwnerMode=Default` when global default mode is `ParentWindow` and `RootWindow`.
