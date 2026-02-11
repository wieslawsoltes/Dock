using Dock.Model.Core;
using Xunit;

namespace Dock.Model.UnitTests;

public class DockCapabilityPolicyTests
{
    [Theory]
    [InlineData(DockCapability.Close)]
    [InlineData(DockCapability.Pin)]
    [InlineData(DockCapability.Float)]
    [InlineData(DockCapability.Drag)]
    [InlineData(DockCapability.Drop)]
    [InlineData(DockCapability.DockAsDocument)]
    public void Get_Returns_Configured_Value_For_Each_Capability(DockCapability capability)
    {
        var policy = new DockCapabilityPolicy
        {
            CanClose = true,
            CanPin = true,
            CanFloat = true,
            CanDrag = true,
            CanDrop = true,
            CanDockAsDocument = true
        };

        var value = policy.Get(capability);

        Assert.True(value);
    }

    [Fact]
    public void Get_Returns_Null_For_Unknown_Capability()
    {
        var policy = new DockCapabilityPolicy
        {
            CanClose = true
        };

        var value = policy.Get((DockCapability)999);

        Assert.Null(value);
    }

    [Fact]
    public void HasAnyOverride_Returns_False_When_All_Values_Are_Null()
    {
        var overrides = new DockCapabilityOverrides();

        Assert.False(overrides.HasAnyOverride);
    }

    [Theory]
    [InlineData("Close")]
    [InlineData("Pin")]
    [InlineData("Float")]
    [InlineData("Drag")]
    [InlineData("Drop")]
    [InlineData("DockAsDocument")]
    public void HasAnyOverride_Returns_True_For_Each_Override_Value(string propertyName)
    {
        var overrides = new DockCapabilityOverrides();

        switch (propertyName)
        {
            case "Close":
                overrides.CanClose = false;
                break;
            case "Pin":
                overrides.CanPin = false;
                break;
            case "Float":
                overrides.CanFloat = false;
                break;
            case "Drag":
                overrides.CanDrag = false;
                break;
            case "Drop":
                overrides.CanDrop = false;
                break;
            case "DockAsDocument":
                overrides.CanDockAsDocument = false;
                break;
        }

        Assert.True(overrides.HasAnyOverride);
    }
}
