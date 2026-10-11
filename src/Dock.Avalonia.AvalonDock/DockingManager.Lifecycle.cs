using System;
using Avalonia.Controls.Primitives;

namespace AvalonDock;

public partial class DockingManager
{
    /// <summary>Dispatches Avalonia initialization to the AvalonDock extensibility hook.</summary>
    protected override void OnInitialized() => OnInitialized(EventArgs.Empty);

    /// <summary>Called once when Avalonia completes manager initialization.</summary>
    /// <remarks>Override this overload when porting an AvalonDock manager. Call the base hook
    /// to retain Avalonia initialization; the public Initialized event follows this hook.</remarks>
    protected virtual void OnInitialized(EventArgs e) => base.OnInitialized();

    /// <summary>Dispatches each applied Avalonia template to the AvalonDock template hook.</summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        OnApplyTemplate();
    }

    /// <summary>Refreshes the public layout and auto-hide presentations for the current template.</summary>
    /// <remarks>Called after the Avalonia template is applied. Calling this hook again is safe;
    /// existing presentations are reused while their models are unchanged.</remarks>
    public virtual void OnApplyTemplate()
    {
        EnsureLayoutRootPanel();
        EnsureAnchorSidePanels();
        EnsureAutoHideWindow();
    }
}
