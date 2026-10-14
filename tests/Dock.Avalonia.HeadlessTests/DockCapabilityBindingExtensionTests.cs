using System;
using System.ComponentModel;
using Avalonia.Controls;
#if AVALONIA_11
using CapabilityBinding = Avalonia.Markup.Xaml.MarkupExtensions.CompiledBindingExtension;
#else
using CapabilityBinding = Avalonia.Data.CompiledBinding;
#endif
using Avalonia.Headless.XUnit;
using Avalonia.Logging;
using Avalonia.Threading;
using Dock.Avalonia.Data;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class DockCapabilityBindingExtensionTests
{
    [AvaloniaTheory]
    [InlineData(DockCapabilityBindingSource.DockableOverrides)]
    [InlineData(DockCapabilityBindingSource.ActiveDockableOverrides)]
    [InlineData(DockCapabilityBindingSource.DockPolicy)]
    [InlineData(DockCapabilityBindingSource.OwnerDockPolicy)]
    public void All_Capabilities_Observe_Null_Replacement_And_InPlace_Notifications(DockCapabilityBindingSource source)
    {
        foreach (DockCapability capability in Enum.GetValues(typeof(DockCapability)))
        {
            var factory = new Model.Mvvm.Factory();
            var dock = factory.CreateToolDock();
            var tool = factory.CreateTool();
            tool.Owner = dock;
            dock.ActiveDockable = tool;
            var control = new Border
            {
                DataContext = source is DockCapabilityBindingSource.DockPolicy or DockCapabilityBindingSource.ActiveDockableOverrides ? dock : tool
            };
            var previousSink = Logger.Sink;
            var sink = new CapabilityPolicyBindingWarningsTests.CapabilityWarningSink();
            Logger.Sink = sink;
            try
            {
                var extension = new DockCapabilityBindingExtension { Capability = capability, Source = source };
                using var subscription = control.Bind(Control.TagProperty, Assert.IsType<CapabilityBinding>(extension.ProvideValue(new EmptyServiceProvider())));
                Dispatcher.UIThread.RunJobs();
                Assert.Null(control.Tag);

                var policy = new NotifyingOverrides();
                SetPolicy(dock, tool, source, policy);
                Assert.Null(control.Tag);

                policy.Set(capability, false);
                Assert.Equal(false, control.Tag);
                policy.Set(capability, true);
                Assert.Equal(true, control.Tag);

                var replacement = new NotifyingOverrides();
                replacement.Set(capability, false);
                SetPolicy(dock, tool, source, replacement);
                Assert.Equal(false, control.Tag);
                policy.Set(capability, null);
                Assert.Equal(false, control.Tag);

                SetPolicy(dock, tool, source, null);
                Assert.Null(control.Tag);
                replacement.Set(capability, true);
                Assert.Null(control.Tag);
                Assert.Empty(sink.Warnings);
            }
            finally
            {
                Logger.Sink = previousSink;
            }
        }
    }

    [AvaloniaTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void Replacing_Or_Removing_Owner_And_ActiveDockable_Rebinds_Without_Warnings(bool owner)
    {
        var factory = new Model.Mvvm.Factory();
        var dock = factory.CreateToolDock();
        var tool = factory.CreateTool();
        var control = new Border { DataContext = owner ? (IDockable)tool : dock };
        var previousSink = Logger.Sink;
        var sink = new CapabilityPolicyBindingWarningsTests.CapabilityWarningSink();
        Logger.Sink = sink;
        try
        {
            var extension = new DockCapabilityBindingExtension
            {
                Capability = DockCapability.Drag,
                Source = owner ? DockCapabilityBindingSource.OwnerDockPolicy : DockCapabilityBindingSource.ActiveDockableOverrides
            };
            using var subscription = control.Bind(Control.TagProperty, Assert.IsType<CapabilityBinding>(extension.ProvideValue(new EmptyServiceProvider())));
            Assert.Null(control.Tag);
            dock.DockCapabilityPolicy = new DockCapabilityPolicy { CanDrag = false };
            tool.DockCapabilityOverrides = new DockCapabilityOverrides { CanDrag = false };
            tool.Owner = dock;
            dock.ActiveDockable = tool;
            Assert.Equal(false, control.Tag);

            tool.Owner = null;
            dock.ActiveDockable = null;
            Assert.Null(control.Tag);
            Assert.Empty(sink.Warnings);
        }
        finally
        {
            Logger.Sink = previousSink;
        }
    }

    [AvaloniaFact]
    public void Invalid_Capability_And_Source_Are_Rejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new DockCapabilityBindingExtension
        {
            Capability = (DockCapability)(-1)
        }.ProvideValue(null!));
        Assert.Throws<ArgumentOutOfRangeException>(() => new DockCapabilityBindingExtension
        {
            Source = (DockCapabilityBindingSource)(-1)
        }.ProvideValue(null!));
    }

    private static void SetPolicy(IDock dock, IDockable tool, DockCapabilityBindingSource source, DockCapabilityOverrides? policy)
    {
        if (source is DockCapabilityBindingSource.DockPolicy or DockCapabilityBindingSource.OwnerDockPolicy)
            dock.DockCapabilityPolicy = policy;
        else
            tool.DockCapabilityOverrides = policy;
        Dispatcher.UIThread.RunJobs();
    }

    private sealed class EmptyServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private sealed class NotifyingOverrides : DockCapabilityOverrides, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public void Set(DockCapability capability, bool? value)
        {
            string propertyName;
            switch (capability)
            {
                case DockCapability.Close: CanClose = value; propertyName = nameof(CanClose); break;
                case DockCapability.Pin: CanPin = value; propertyName = nameof(CanPin); break;
                case DockCapability.Float: CanFloat = value; propertyName = nameof(CanFloat); break;
                case DockCapability.Drag: CanDrag = value; propertyName = nameof(CanDrag); break;
                case DockCapability.Drop: CanDrop = value; propertyName = nameof(CanDrop); break;
                case DockCapability.DockAsDocument: CanDockAsDocument = value; propertyName = nameof(CanDockAsDocument); break;
                default: throw new ArgumentOutOfRangeException(nameof(capability));
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            Dispatcher.UIThread.RunJobs();
        }
    }
}
