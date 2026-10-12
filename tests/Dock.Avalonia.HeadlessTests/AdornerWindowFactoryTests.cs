using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Internal;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class AdornerWindowFactoryTests
{
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Floating_Adorners_Use_The_Inherited_Factory_And_Close_The_Previous_Window(bool fallback)
    {
        Border surface = new() { Width = 240, Height = 180 };
        RecordingFactory factory = new(fallback);
        Window owner = new() { Width = 400, Height = 300, Content = surface };
        DockAdornerWindow.SetFactory(owner, factory);
        AdornerHelper<DockTarget> helper = new(true);
        try
        {
            owner.Show(); Update();
            Assert.Same(factory, DockAdornerWindow.GetFactory(surface));
            helper.AddAdorner(surface, false); Update();
            DockAdornerWindow first = Assert.IsAssignableFrom<DockAdornerWindow>(TopLevel.GetTopLevel(helper.Adorner!));
            Assert.Equal(fallback, first is not CustomWindow);
            Assert.Same(owner, first.Owner);
            Assert.Same(surface, factory.Surface);
            Assert.Same(helper.Adorner, factory.Target);
            Assert.Same(helper.Adorner, first.Content);
            Assert.Equal(surface.Bounds.Size, new Size(first.Width, first.Height));
            Assert.Equal(surface.PointToScreen(default), first.Position);
            bool closed = false;
            first.Closed += (_, _) => closed = true;
            helper.AddAdorner(surface, false); Update();
            Assert.True(closed);
            Assert.Null(first.Content);
            Assert.Equal(2, factory.Calls);
            Assert.NotSame(first, TopLevel.GetTopLevel(helper.Adorner!));
            helper.RemoveAdorner(surface); Update();
            Assert.Null(helper.Adorner);
        }
        finally { helper.RemoveAdorner(surface); owner.Close(); }
        void Update() { Dispatcher.UIThread.RunJobs(); owner.UpdateLayout(); }
    }

    [AvaloniaFact]
    public void Inline_Adorners_Keep_The_Native_Layer_And_Do_Not_Create_A_Window()
    {
        Border surface = new() { Width = 240, Height = 180 };
        RecordingFactory factory = new(false);
        Window owner = new() { Content = surface, Width = 400, Height = 300 };
        DockAdornerWindow.SetFactory(owner, factory);
        AdornerHelper<DockTarget> helper = new(false);
        try
        {
            owner.Show(); owner.UpdateLayout();
            helper.AddAdorner(surface, false);
            Assert.Same(owner, TopLevel.GetTopLevel(helper.Adorner!));
            Assert.Equal(0, factory.Calls);
            DockAdornerWindow.SetFactory(owner, null);
            Assert.Null(DockAdornerWindow.GetFactory(surface));
        }
        finally { helper.RemoveAdorner(surface); owner.Close(); }
    }

    private sealed class RecordingFactory(bool fallback) : IDockAdornerWindowFactory
    {
        public int Calls { get; private set; }
        public Visual? Surface { get; private set; }
        public Control? Target { get; private set; }
        public DockAdornerWindow? CreateWindow(Visual adornedElement, Control adorner)
        {
            Calls++; Surface = adornedElement; Target = adorner;
            return fallback ? null : new CustomWindow();
        }
    }

    private sealed class CustomWindow : DockAdornerWindow;
}
