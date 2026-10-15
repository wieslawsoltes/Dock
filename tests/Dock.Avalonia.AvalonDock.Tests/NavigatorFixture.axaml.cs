using AvalonDock;
using AvalonDock.Controls;

namespace Dock.Avalonia.AvalonDock.Tests;

public partial class NavigatorFixture : NavigatorWindow
{
    public NavigatorFixture() : this(new DockingManager()) { }
    public NavigatorFixture(DockingManager manager) : base(manager) => InitializeComponent();
}
