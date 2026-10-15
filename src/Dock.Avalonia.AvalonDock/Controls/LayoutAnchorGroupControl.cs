using System;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls.Primitives;
using AvalonDock.Infrastructure;
using AvalonDock.Layout;

namespace AvalonDock.Controls;

/// <summary>Presents an auto-hide group's anchors through a native Dock pinned strip.</summary>
public class LayoutAnchorGroupControl : TemplatedControl, ILayoutControl
{
    private readonly LayoutAnchorGroup _model;
    private readonly LayoutAnchorChildren<LayoutAnchorable, LayoutAnchorControl> _children;
    private readonly LayoutAnchorViewBinding _binding;

    internal LayoutAnchorGroupControl(LayoutAnchorGroup model)
    {
        ArgumentNullException.ThrowIfNull(model);
        _model = model;
        _children = new(model.Children, static child => new LayoutAnchorControl(child));
        _binding = new(this, model, DockingManager.AnchorGroupTemplateProperty, static _ => { });
        _binding.Initialize();
    }

    /// <summary>Gets the existing public auto-hide group.</summary>
    public ILayoutElement Model => _model;
    /// <summary>Gets the anchor controls in model order, preserving controls during reordering.</summary>
    public ObservableCollection<LayoutAnchorControl> Children => _children.Views;
    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(LayoutAnchorGroupControl);
    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _binding.Initialize();
    }
}
