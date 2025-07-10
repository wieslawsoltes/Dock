using Dock.Model.Core;
using Dock.Model.Mvvm.Controls;

namespace Dock.Model.UnitTests;

internal class TestDockControlState : IDockControlState
{
}

internal class TestDockControl : IDockControl
{
    public IDockManager DockManager { get; } = new DockManager();
    public IDockControlState DockControlState { get; } = new TestDockControlState();
    public IDock? Layout { get; set; }
    public object? DefaultContext { get; set; }
    public bool InitializeLayout { get; set; }
    public bool InitializeFactory { get; set; }
    public IFactory? Factory { get; set; }
}
