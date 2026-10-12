using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Core;
using Dock.Model.Core;
using Xunit;

namespace Dock.Model.UnitTests;

public class DockWindowClosePolicyTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Preserving_A_Window_Skips_Content_Closure_But_Still_Allows_Factory_Cancellation(bool preserve, bool cancel)
    {
        PolicyFactory factory = new() { Preserve = preserve };
        ProbeWindow window = new();
        int notifications = 0;
        factory.WindowClosing += (_, args) => { notifications++; Assert.Same(window, args.Window); args.Cancel = cancel; };
        Assert.Equal(!cancel, factory.OnWindowClosing(window));
        Assert.Equal(preserve ? 0 : 1, window.Calls);
        Assert.Equal(1, notifications);
        Assert.Same(window, factory.Requested);
    }

    [Fact]
    public void Ordinary_Factories_Keep_Model_Veto_And_Null_Window_Behavior()
    {
        Factory factory = new();
        ProbeWindow window = new() { AllowClose = false };
        Assert.False(factory.OnWindowClosing(window));
        Assert.Equal(1, window.Calls);
        Assert.True(factory.OnWindowClosing(null));
    }

    private sealed class PolicyFactory : Factory, IDockWindowClosePolicy
    {
        public bool Preserve { get; init; }
        public IDockWindow? Requested { get; private set; }
        public bool PreserveLayoutOnHostClose(IDockWindow window) { Requested = window; return Preserve; }
    }

    private sealed class ProbeWindow : DockWindow
    {
        public bool AllowClose { get; init; } = true;
        public int Calls { get; private set; }
        public override bool OnClose() { Calls++; return AllowClose; }
    }
}
