using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.LogicalTree;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace AvalonDock.Infrastructure;

// Avalonia 12 caches menu header presenters, submenu popups, and theme
// subscriptions after closing. Release generated presentation, preserving the
// application's menu items, command bindings, header controls, and themes.
internal sealed class LayoutMenuPresentation
{
    private readonly HashSet<BindingExpressionBase> _bindings = new();
    private readonly IControlTemplate _emptyTemplate =
        (IControlTemplate)new LayoutMenuTemplates()["ReleasedMenuTemplate"]!;
    private readonly ControlTheme _emptyTheme = new() { TargetType = typeof(ItemsControl) };

    public void Capture(ItemsControl control)
    {
        foreach (Visual part in control.GetVisualDescendants())
        {
            if (!ReferenceEquals(part.TemplatedParent, control)) continue;
            CaptureBindings(part);
            if (part is Popup { Child: { } child }) CapturePopup(child, control);
        }
        foreach (object? value in control.ItemsView)
            if (value is MenuItem item) Capture(item);
        foreach (Control container in control.GetRealizedContainers())
            if (container is MenuItem item && !control.ItemsView.Contains(item)) Capture(item);
    }

    public void Release(ContextMenu menu)
    {
        foreach (BindingExpressionBase binding in _bindings) binding.Dispose();
        _bindings.Clear();
        ReleaseControl(menu);
    }

    private void CapturePopup(StyledElement part, ItemsControl owner)
    {
        if (!ReferenceEquals(part.TemplatedParent, owner)) return;
        CaptureBindings(part);
        foreach (ILogical child in part.GetLogicalChildren())
            if (child is StyledElement styled) CapturePopup(styled, owner);
    }

    private void CaptureBindings(AvaloniaObject part)
    {
        foreach (AvaloniaProperty property in AvaloniaPropertyRegistry.Instance.GetRegistered(part))
            if (BindingOperations.GetBindingExpressionBase(part, property) is { } binding) _bindings.Add(binding);
        foreach (AvaloniaProperty property in AvaloniaPropertyRegistry.Instance.GetRegisteredAttached(part.GetType()))
            if (BindingOperations.GetBindingExpressionBase(part, property) is { } binding) _bindings.Add(binding);
    }

    private void ReleaseControl(ItemsControl control)
    {
        foreach (object? value in control.ItemsView)
            if (value is MenuItem item) ReleaseControl(item);
        foreach (Control container in control.GetRealizedContainers())
            if (container is MenuItem item && !control.ItemsView.Contains(item)) ReleaseControl(item);

        if (control is MenuItem menuItem)
        {
            if (menuItem.HeaderPresenter is { } header)
            {
                header.SetCurrentValue(ContentPresenter.ContentTemplateProperty, null);
                header.SetCurrentValue(ContentPresenter.ContentProperty, null);
                header.UpdateChild();
            }
            // Theme detachment can clear the presenter's host before it removes
            // its generated AccessText from the owner's logical children.
            if (((ILogical)control).LogicalChildren is IList<ILogical> children)
            {
                for (int i = children.Count - 1; i >= 0; i--)
                    if (children[i] is AccessText text && !ReferenceEquals(text, menuItem.Header) && !ReferenceEquals(text, menuItem.Icon))
                        children.RemoveAt(i);
            }
        }

        // Temporary priority overrides preserve local values, bindings, and
        // style precedence. Changing the theme also releases style activators
        // that observe the menu item's classes from its old template parts.
        using IDisposable? theme = control.SetValue(StyledElement.ThemeProperty, _emptyTheme, BindingPriority.Animation);
        using IDisposable? template = control.SetValue(TemplatedControl.TemplateProperty, _emptyTemplate, BindingPriority.Animation);
        control.ApplyTemplate();
    }
}
