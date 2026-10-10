using Dock.Model.Avalonia.Controls;
using Xceed.Wpf.AvalonDock.Infrastructure;

namespace Xceed.Wpf.AvalonDock.Layout;

/// <summary>An AvalonDock document backed by a Dock Avalonia document.</summary>
public class LayoutDocument : LayoutContent
{
    private string? _description;
    /// <summary>Creates a document.</summary>
    public LayoutDocument() : this(new Document()) { }
    internal LayoutDocument(Document model, LayoutModelContext? context = null) : base(model, context) { }
    /// <summary>Gets or sets whether the document can be moved.</summary>
    public bool CanMove { get => DockModel.CanDrag; set => DockModel.CanDrag = value; }
    /// <summary>Gets whether the document belongs to a layout.</summary>
    public bool IsVisible => Parent is not null;
    /// <summary>Gets or sets the document description.</summary>
    public string? Description { get => _description; set => SetField(ref _description, value); }
    /// <inheritdoc/>
    public override void Close() => CloseCore();
    /// <inheritdoc/>
    protected override void InternalDock()
    {
        if (Root is LayoutRoot root) LayoutSourcePlacement.InsertDocument(root, this, root.Manager?.LayoutUpdateStrategy);
        base.InternalDock();
    }
}
