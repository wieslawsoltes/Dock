using System.Xml;
using Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Core;
using AvalonDock.Infrastructure;

namespace AvalonDock.Layout;

/// <summary>An AvalonDock document backed by a Dock Avalonia document.</summary>
public class LayoutDocument : LayoutContent
{
    private string? _description;
    /// <summary>Creates a document.</summary>
    public LayoutDocument() : this(new Document { Id = null!, Title = null! }) { }
    internal LayoutDocument(Document model, LayoutModelContext? context = null) : base(model, context) { }
    /// <summary>Gets or sets whether the document can be moved.</summary>
    public bool CanMove { get => DockModel.CanDrag; set => DockModel.CanDrag = value; }
    /// <summary>Gets whether the document participates in pane visibility.</summary>
    public bool IsVisible { get; internal set; } = true;
    /// <summary>Gets or sets the document description.</summary>
    public string? Description { get => _description; set => SetFieldChanged(ref _description, value); }
    /// <summary>Reads document-specific metadata before the shared content metadata.</summary>
    public override void ReadXml(XmlReader reader)
    {
        LayoutContentXml.ReadDocument(this, reader);
        base.ReadXml(reader);
    }
    /// <inheritdoc/>
    public override void Close() => CloseCore();
    /// <inheritdoc/>
    protected override void InternalDock()
    {
        if (Root is LayoutRoot root) LayoutSourcePlacement.DockDocument(root, this);
        base.InternalDock();
    }

    internal override void OnModelPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == DockableBase.CanDragProperty) RaisePropertyChanged(nameof(CanMove));
        else base.OnModelPropertyChanged(sender, e);
    }
}
