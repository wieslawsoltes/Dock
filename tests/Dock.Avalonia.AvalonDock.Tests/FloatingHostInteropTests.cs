using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class FloatingHostInteropTests
{
    [AvaloniaFact]
    public void Derived_Drag_State_Captures_Real_Mouse_And_Releases_Only_Its_Own_Capture()
    {
        DocumentHost window = new(new LayoutDocumentFloatingWindow { RootDocument = new LayoutDocument() })
        {
            Width = 300, Height = 200, Content = new Border { Background = Brushes.White },
        };
        IPointer? pointer = null;
        window.PointerMoved += (_, e) => pointer = e.Pointer;
        try
        {
            window.Show();
            window.MouseMove(new Point(20, 20));
            Assert.NotNull(pointer);

            window.Drag(true);

            Assert.True(window.IsDragging);
            Assert.Same(window, pointer.Captured);
            window.Drag(false);
            Assert.False(window.IsDragging);
            Assert.Null(pointer.Captured);

            window.Drag(true);
            Control other = Assert.IsAssignableFrom<Control>(window.Content);
            pointer.Capture(other);
            window.Drag(false);
            Assert.Same(other, pointer.Captured);
            pointer.Capture(null);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Native_Drag_Notifications_Preserve_Docks_Existing_Capture()
    {
        Border child = new() { Background = Brushes.White };
        DocumentHost window = new(new LayoutDocumentFloatingWindow { RootDocument = new LayoutDocument() })
        {
            Width = 300, Height = 200, Content = child,
        };
        IPointer? pointer = null;
        window.PointerMoved += (_, e) => pointer = e.Pointer;
        try
        {
            window.Show();
            window.MouseMove(new Point(20, 20));
            Assert.NotNull(pointer);
            pointer.Capture(child);

            window.NativeDrag(true);
            Assert.True(window.IsDragging);
            Assert.Same(child, pointer.Captured);
            window.NativeDrag(false);
            Assert.False(window.IsDragging);
            Assert.Same(child, pointer.Captured);
            pointer.Capture(null);
        }
        finally { window.Close(); }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void External_Typed_Initialization_Sees_Model_Exactly_Once(bool tool)
    {
        LayoutFloatingWindow model = tool
            ? new LayoutAnchorableFloatingWindow { RootPanel = new LayoutAnchorablePaneGroup(new LayoutAnchorablePane(new LayoutAnchorable())) }
            : new LayoutDocumentFloatingWindow { RootDocument = new LayoutDocument() };
        IInitializationProbe probe = tool ? new ToolHost((LayoutAnchorableFloatingWindow)model) : new DocumentHost((LayoutDocumentFloatingWindow)model);
        LayoutFloatingWindowControl window = (LayoutFloatingWindowControl)probe;
        try
        {
            Assert.False(window.IsInitialized);
            Assert.Equal(0, probe.Initializations);
            window.Show();
            Assert.Same(model, probe.InitializedModel);
            Assert.Equal(1, probe.Initializations);
            window.Hide();
            window.Show();
            Assert.Equal(1, probe.Initializations);
        }
        finally { window.Close(); }
    }

    private interface IInitializationProbe
    {
        int Initializations { get; }
        ILayoutElement? InitializedModel { get; }
    }

    private sealed class DocumentHost(LayoutDocumentFloatingWindow model) : LayoutDocumentFloatingWindowControl(model), IInitializationProbe
    {
        public int Initializations { get; private set; }
        public ILayoutElement? InitializedModel { get; private set; }
        public void Drag(bool value) => SetIsDragging(value);
        public void NativeDrag(bool value) => PseudoClasses.Set(":dragging", value);
        protected override bool CanClose(object? parameter = null) => true;
        protected override void OnInitialized(EventArgs e)
        {
            InitializedModel = Model;
            ++Initializations;
            base.OnInitialized(e);
        }
    }

    private sealed class ToolHost(LayoutAnchorableFloatingWindow model) : LayoutAnchorableFloatingWindowControl(model), IInitializationProbe
    {
        public int Initializations { get; private set; }
        public ILayoutElement? InitializedModel { get; private set; }
        protected override bool CanClose(object? parameter = null) => true;
        protected override void OnInitialized(EventArgs e)
        {
            InitializedModel = Model;
            ++Initializations;
            base.OnInitialized(e);
        }
    }
}
