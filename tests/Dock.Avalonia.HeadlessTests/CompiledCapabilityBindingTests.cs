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
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class CompiledCapabilityBindingTests
{
    [AvaloniaTheory]
    [InlineData(CapabilityBindingSource.DockableOverrides)]
    [InlineData(CapabilityBindingSource.ActiveDockableOverrides)]
    [InlineData(CapabilityBindingSource.DockPolicy)]
    [InlineData(CapabilityBindingSource.OwnerDockPolicy)]
    public void All_Capabilities_Observe_Null_Replacement_And_InPlace_Notifications(CapabilityBindingSource source)
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
                DataContext = source is CapabilityBindingSource.DockPolicy or CapabilityBindingSource.ActiveDockableOverrides ? dock : tool
            };
            var previousSink = Logger.Sink;
            var sink = new CapabilityPolicyBindingWarningsTests.CapabilityWarningSink();
            Logger.Sink = sink;
            try
            {
                using var subscription = control.Bind(Control.TagProperty, GetBinding(source, capability));
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
    public void Replacing_Or_Removing_Owner_And_ActiveDockable_Rebinds(bool owner)
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
            using var subscription = control.Bind(Control.TagProperty, GetBinding(
                owner ? CapabilityBindingSource.OwnerDockPolicy : CapabilityBindingSource.ActiveDockableOverrides,
                DockCapability.Drag));
            Assert.Null(control.Tag);
            dock.DockCapabilityPolicy = new DockCapabilityPolicy { CanDrag = false };
            tool.DockCapabilityOverrides = new DockCapabilityOverrides { CanDrag = false };
            tool.Owner = dock;
            dock.ActiveDockable = tool;
            Assert.Equal(false, control.Tag);

            tool.Owner = null;
            dock.ActiveDockable = null;
            Assert.Null(control.Tag);
#if AVALONIA_11
            if (owner)
            {
                // Avalonia 11 diagnoses null before the interface cast. AvaloniaUI/Avalonia#22174 fixes this.
                Assert.Equal(2, sink.Warnings.Count);
                Assert.All(sink.Warnings, warning =>
                {
                    Assert.Contains("Owner", warning);
                    Assert.Contains("DockCapabilityPolicy", warning);
                    Assert.Contains("Value is null.", warning);
                });
            }
            else
#endif
            Assert.Empty(sink.Warnings);
        }
        finally
        {
            Logger.Sink = previousSink;
        }
    }

    [AvaloniaTheory]
    [InlineData(CapabilityBindingSource.DockableOverrides)]
    [InlineData(CapabilityBindingSource.ActiveDockableOverrides)]
    [InlineData(CapabilityBindingSource.DockPolicy)]
    [InlineData(CapabilityBindingSource.OwnerDockPolicy)]
    public void DataContext_Replacement_Null_And_Disposal_Detach_Previous_Policies(CapabilityBindingSource source)
    {
        var first = CreateContext(source, true);
        var second = CreateContext(source, false);
        var control = new Border();
        var previousSink = Logger.Sink;
        var sink = new CapabilityPolicyBindingWarningsTests.CapabilityWarningSink();
        Logger.Sink = sink;
        try
        {
            using var subscription = control.Bind(Control.TagProperty, GetBinding(source, DockCapability.Drag));
            Assert.Null(control.Tag);

            control.DataContext = first.Context;
            Assert.Equal(true, control.Tag);
            control.DataContext = second.Context;
            Assert.Equal(false, control.Tag);
            Assert.Equal(0, first.Policy.Subscribers);
            first.Policy.Set(DockCapability.Drag, false);
            first.Policy.Set(DockCapability.Drag, true);
            Assert.Equal(false, control.Tag);

            second.Policy.CanDrag = null;
            second.Policy.Notify(string.Empty);
            Assert.Null(control.Tag);
            second.Policy.CanDrag = true;
            second.Policy.Notify(null);
            Assert.Equal(true, control.Tag);

            control.DataContext = null;
            Assert.Null(control.Tag);
            Assert.Equal(0, second.Policy.Subscribers);
            second.Policy.Set(DockCapability.Drag, false);
            Assert.Null(control.Tag);
            control.DataContext = first.Context;
            Assert.Equal(true, control.Tag);
            subscription.Dispose();
            Assert.Equal(0, first.Policy.Subscribers);
            Assert.Empty(sink.Warnings);
        }
        finally
        {
            Logger.Sink = previousSink;
        }
    }

    [AvaloniaFact]
    public void Real_Policy_Getter_Errors_Remain_Observable()
    {
        var control = new Border
        {
            DataContext = new ThrowingDock()
        };
        var previousSink = Logger.Sink;
        var sink = new CapabilityPolicyBindingWarningsTests.CapabilityWarningSink();
        Logger.Sink = sink;
        try
        {
            using var subscription = control.Bind(Control.TagProperty,
                GetBinding(CapabilityBindingSource.DockPolicy, DockCapability.Drag));
            Assert.Contains(sink.Warnings, message => message.Contains("Policy getter failed", StringComparison.Ordinal));
        }
        finally
        {
            Logger.Sink = previousSink;
        }
    }

    public enum CapabilityBindingSource
    {
        DockableOverrides,
        ActiveDockableOverrides,
        DockPolicy,
        OwnerDockPolicy
    }

    private CapabilityBindingTestResources? _bindings;

    private CapabilityBinding GetBinding(CapabilityBindingSource source, DockCapability capability)
    {
        _bindings ??= new CapabilityBindingTestResources();
        return Assert.IsType<CapabilityBinding>(_bindings[$"{source}{capability}"]);
    }

    private static void SetPolicy(IDock dock, IDockable tool, CapabilityBindingSource source, DockCapabilityOverrides? policy)
    {
        if (source is CapabilityBindingSource.DockPolicy or CapabilityBindingSource.OwnerDockPolicy)
            dock.DockCapabilityPolicy = policy;
        else
            tool.DockCapabilityOverrides = policy;
        Dispatcher.UIThread.RunJobs();
    }

    private static (IDockable Context, NotifyingOverrides Policy) CreateContext(CapabilityBindingSource source, bool value)
    {
        var policy = new NotifyingOverrides { CanDrag = value };
        var dock = new Model.Mvvm.Controls.ToolDock { DockCapabilityPolicy = policy };
        var tool = new Model.Mvvm.Controls.Tool { Owner = dock, DockCapabilityOverrides = policy };
        dock.ActiveDockable = tool;
        return (source is CapabilityBindingSource.DockPolicy or CapabilityBindingSource.ActiveDockableOverrides ? dock : tool, policy);
    }

    private sealed class NotifyingOverrides : DockCapabilityOverrides, INotifyPropertyChanged
    {
        private PropertyChangedEventHandler? _propertyChanged;
        public int Subscribers { get; private set; }
        public event PropertyChangedEventHandler? PropertyChanged
        {
            add { _propertyChanged += value; Subscribers++; }
            remove { _propertyChanged -= value; Subscribers--; }
        }

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
            Notify(propertyName);
        }

        public void Notify(string? propertyName)
        {
            _propertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            Dispatcher.UIThread.RunJobs();
        }
    }

    private sealed class ThrowingDock : Model.Mvvm.Controls.ToolDock, IDock
    {
        DockCapabilityPolicy? IDock.DockCapabilityPolicy
        {
            get => throw new InvalidOperationException("Policy getter failed");
            set => throw new NotSupportedException();
        }
    }
}
