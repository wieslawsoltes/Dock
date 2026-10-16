using AvalonDock.Controls;
using AvalonDock.Layout;

namespace Dock.Avalonia.AvalonDock.Tests;

public partial class FloatingResizeFixture : LayoutFloatingWindowControl
{
    public FloatingResizeFixture() : this(new LayoutDocumentFloatingWindow()) { }
    private FloatingResizeFixture(LayoutDocumentFloatingWindow model) : base(model)
    {
        Model = model;
        InitializeComponent();
    }
    public override ILayoutElement Model { get; }
}
