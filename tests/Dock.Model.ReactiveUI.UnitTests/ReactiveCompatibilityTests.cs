using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reflection;
using Dock.Model.ReactiveUI.Core;
using Dock.Model.ReactiveUI.Services.Overlays.Services;
using ReactiveUI.Reactive;
using Xunit;

namespace Dock.Model.ReactiveUI.UnitTests;

public sealed class ReactiveCompatibilityTests
{
    [Fact]
    public void Model_Uses_ReactiveUI_Reactive_Without_Default_ReactiveUI_Assembly()
    {
        Assembly modelAssembly = typeof(ReactiveBase).Assembly;
        string[] references = modelAssembly.GetReferencedAssemblies()
            .Select(static reference => reference.Name ?? string.Empty)
            .ToArray();

        Assert.Contains("ReactiveUI.Reactive", references);
        Assert.DoesNotContain("ReactiveUI", references);
        Assert.Equal(typeof(ReactiveObject), typeof(ReactiveBase).BaseType);
    }

    [Fact]
    public void Services_Use_SystemReactive_Unit_And_Scheduler()
    {
        Type commandType = typeof(DockBusyService)
            .GetField("_reloadCommand", BindingFlags.Instance | BindingFlags.NonPublic)!
            .FieldType;
        Assert.Equal(typeof(Unit), commandType.GetGenericArguments()[0]);
        Assert.Equal(typeof(Unit), commandType.GetGenericArguments()[1]);

        Type dispatcherType = typeof(DockBusyService).Assembly.GetType(
            "Dock.Model.ReactiveUI.Services.Threading.ServiceDispatcher",
            throwOnError: true)!;
        Type schedulerType = dispatcherType
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Single()
            .GetParameters()[1]
            .ParameterType;

        Assert.Equal(typeof(IScheduler), Nullable.GetUnderlyingType(schedulerType) ?? schedulerType);
    }
}
