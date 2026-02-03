using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Dock.Settings;
using Xunit;

namespace Dock.Settings.UnitTests;

public class DockSettingsTests
{
    [Fact]
    public void ResolveFloatingWindowHostMode_Uses_RootOverride()
    {
        var originalGlobal = DockSettings.FloatingWindowHostMode;
        var originalManaged = DockSettings.UseManagedWindows;

        try
        {
            DockSettings.FloatingWindowHostMode = DockFloatingWindowHostMode.Native;
            DockSettings.UseManagedWindows = false;
            var root = new RootDock { FloatingWindowHostMode = DockFloatingWindowHostMode.Managed };

            var mode = DockSettings.ResolveFloatingWindowHostMode(root);

            Assert.Equal(DockFloatingWindowHostMode.Managed, mode);
        }
        finally
        {
            DockSettings.FloatingWindowHostMode = originalGlobal;
            DockSettings.UseManagedWindows = originalManaged;
        }
    }

    [Fact]
    public void ResolveFloatingWindowHostMode_Uses_Global_When_Set()
    {
        var originalGlobal = DockSettings.FloatingWindowHostMode;
        var originalManaged = DockSettings.UseManagedWindows;

        try
        {
            DockSettings.FloatingWindowHostMode = DockFloatingWindowHostMode.Managed;
            DockSettings.UseManagedWindows = false;
            var root = new RootDock { FloatingWindowHostMode = DockFloatingWindowHostMode.Default };

            var mode = DockSettings.ResolveFloatingWindowHostMode(root);

            Assert.Equal(DockFloatingWindowHostMode.Managed, mode);
        }
        finally
        {
            DockSettings.FloatingWindowHostMode = originalGlobal;
            DockSettings.UseManagedWindows = originalManaged;
        }
    }

    [Fact]
    public void ResolveFloatingWindowHostMode_Falls_Back_To_UseManagedWindows()
    {
        var originalGlobal = DockSettings.FloatingWindowHostMode;
        var originalManaged = DockSettings.UseManagedWindows;

        try
        {
            DockSettings.FloatingWindowHostMode = DockFloatingWindowHostMode.Default;
            DockSettings.UseManagedWindows = true;

            var mode = DockSettings.ResolveFloatingWindowHostMode(null);

            Assert.Equal(DockFloatingWindowHostMode.Managed, mode);
        }
        finally
        {
            DockSettings.FloatingWindowHostMode = originalGlobal;
            DockSettings.UseManagedWindows = originalManaged;
        }
    }

    [Fact]
    public void ShouldUseOwnerForFloatingWindows_Respects_Policy()
    {
        var originalPolicy = DockSettings.FloatingWindowOwnerPolicy;
        var originalUseOwner = DockSettings.UseOwnerForFloatingWindows;

        try
        {
            DockSettings.UseOwnerForFloatingWindows = false;
            DockSettings.FloatingWindowOwnerPolicy = DockFloatingWindowOwnerPolicy.AlwaysOwned;
            Assert.True(DockSettings.ShouldUseOwnerForFloatingWindows());

            DockSettings.UseOwnerForFloatingWindows = true;
            DockSettings.FloatingWindowOwnerPolicy = DockFloatingWindowOwnerPolicy.NeverOwned;
            Assert.False(DockSettings.ShouldUseOwnerForFloatingWindows());

            DockSettings.UseOwnerForFloatingWindows = true;
            DockSettings.FloatingWindowOwnerPolicy = DockFloatingWindowOwnerPolicy.Default;
            Assert.True(DockSettings.ShouldUseOwnerForFloatingWindows());
        }
        finally
        {
            DockSettings.FloatingWindowOwnerPolicy = originalPolicy;
            DockSettings.UseOwnerForFloatingWindows = originalUseOwner;
        }
    }
}
