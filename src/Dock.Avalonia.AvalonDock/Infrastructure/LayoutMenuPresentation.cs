using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.LogicalTree;
using Avalonia.Rendering.Composition;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace AvalonDock.Infrastructure;

// Avalonia 12 caches menu header presenters, submenu popups, and theme
// subscriptions after closing. Release generated presentation, preserving the
// application's menu items, command bindings, header controls, and themes.
internal sealed class LayoutMenuPresentation
{
    private readonly HashSet<BindingExpressionBase> _bindings = new();
    private readonly HashSet<(CompositionContainerVisual Parent, CompositionVisual Child)> _contentVisuals = new();
    private readonly IControlTemplate _emptyTemplate =
        (IControlTemplate)new LayoutMenuTemplates()["ReleasedMenuTemplate"]!;
    private readonly ControlTheme _emptyTheme = new() { TargetType = typeof(ItemsControl) };

    public void Capture(ItemsControl control)
    {
        if (control is MenuItem menuItem)
        {
            CaptureContentVisual(menuItem.Icon as Visual);
            CaptureContentVisual(menuItem.Header as Visual);
        }
        foreach (Visual part in control.GetVisualDescendants())
        {
            if (!ReferenceEquals(part.TemplatedParent, control) || IsApplicationContent(part, control)) continue;
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
        // Detach persistent content while its old presenter is still reachable;
        // disposing template bindings first can clear that visual-parent link.
        ReleaseControl(menu);
        foreach (BindingExpressionBase binding in _bindings) binding.Dispose();
        _bindings.Clear();
        // A closed popup no longer renders the invalidation that removes these
        // edges. Persistent application content must not retain its former host.
        foreach (var (parent, child) in _contentVisuals) parent.Children.Remove(child);
        _contentVisuals.Clear();
    }

    private void CaptureContentVisual(Visual? content)
    {
        // Closing the popup clears Visual.CompositionVisual, but controls such
        // as Border keep their rendering object. Capture its parent while open.
        if (content?.GetVisualParent() is { } parent &&
            ElementComposition.GetElementVisual(parent) is CompositionContainerVisual container &&
            ElementComposition.GetElementVisual(content) is { } child)
            _contentVisuals.Add((container, child));
    }

    private void CapturePopup(StyledElement part, ItemsControl owner)
    {
        if (!ReferenceEquals(part.TemplatedParent, owner)) return;
        CaptureBindings(part);
        foreach (ILogical child in part.GetLogicalChildren())
            if (child is StyledElement styled) CapturePopup(styled, owner);
    }

    private static bool IsApplicationContent(Visual part, ItemsControl owner)
    {
        if (owner is not MenuItem item) return false;
        // Avalonia can assign the menu item as an application's icon/header
        // TemplatedParent. That does not make its bindings disposable chrome.
        for (Visual? current = part; current is not null && !ReferenceEquals(current, owner); current = current.GetVisualParent())
            if (ReferenceEquals(current, item.Icon) || ReferenceEquals(current, item.Header)) return true;
        return false;
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
            // A persistent icon otherwise keeps the old Fluent icon presenter
            // and its popup tree alive after the item template is retired.
            if (menuItem.Icon is Control icon && icon.GetVisualParent() is ContentPresenter iconPresenter &&
                ReferenceEquals(iconPresenter.Child, icon))
                ReleasePresenter(iconPresenter);
            if (menuItem.HeaderPresenter is { } header)
                ReleasePresenter(header);
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

    private static void ReleasePresenter(ContentPresenter presenter)
    {
        presenter.SetCurrentValue(ContentPresenter.ContentTemplateProperty, null);
        presenter.SetCurrentValue(ContentPresenter.ContentProperty, null);
        presenter.UpdateChild();
    }
}
