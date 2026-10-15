using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;
using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class DefaultCommandHookTests
{
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Initialization_Is_Available_Before_Attachment_And_Does_Not_Query_Virtual_Predicates(bool tool)
    {
        IProbe probe = Create(tool);
        probe.Initialize();
        Assert.Equal(0, probe.PredicateCalls);
        Assert.All(Properties(tool), property => Assert.Null(probe.Item.GetValue(property)));
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Applying_Unattached_Defaults_Fails_At_The_Model_Access_After_The_Declared_Commands(bool tool)
    {
        IProbe probe = Create(tool);
        probe.Initialize();
        List<string> changes = Observe(probe.Item);
        Assert.Throws<NullReferenceException>(probe.Apply);
        Assert.Equal(Properties(tool).Select(property => property.Name), changes);
        Assert.All(Properties(tool), property => Assert.NotNull(probe.Item.GetValue(property)));
        if (tool) Assert.Null(probe.Item.CloseCommand);
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Reinitializing_Or_Clearing_Definitions_Does_Not_Clear_Previously_Assigned_Commands(bool tool, bool clear)
    {
        IProbe probe = Initialized(tool);
        ICommand?[] commands = Values(probe, tool);
        if (clear) probe.ClearDefinitions();
        else probe.Initialize();
        probe.ClearBindings();
        Assert.Equal(commands, Values(probe, tool));
        Assert.Throws<NullReferenceException>(probe.Apply);
        Assert.Equal(commands, Values(probe, tool));
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Reinitializing_In_An_Apply_Callback_Changes_The_Remaining_Defaults(bool tool)
    {
        IProbe probe = Create(tool);
        probe.Initialize();
        StyledProperty<ICommand?> first = Properties(tool)[0];
        bool reinitialized = false;
        probe.Item.PropertyChanged += (_, e) =>
        {
            if (e.Property != first || reinitialized) return;
            reinitialized = true;
            probe.Initialize();
        };
        Assert.Throws<NullReferenceException>(probe.Apply);
        ICommand? original = probe.Item.GetValue(first);
        Assert.NotNull(original);
        probe.ClearBindings();
        Assert.Same(original, probe.Item.GetValue(first));
        Assert.All(Properties(tool).Skip(1), property => Assert.Null(probe.Item.GetValue(property)));
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Clearing_Definitions_In_An_Apply_Callback_Does_Not_Invalidate_Iteration(bool tool)
    {
        IProbe probe = Create(tool);
        probe.Initialize();
        StyledProperty<ICommand?> first = Properties(tool)[0];
        probe.Item.PropertyChanged += (_, e) => { if (e.Property == first) probe.ClearDefinitions(); };
        Assert.Throws<NullReferenceException>(probe.Apply);
        Assert.NotNull(probe.Item.GetValue(first));
        Assert.All(Properties(tool).Skip(1), property => Assert.Null(probe.Item.GetValue(property)));
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Reinitializing_In_A_Clear_Callback_Preserves_The_Remaining_Old_Commands(bool tool)
    {
        IProbe probe = Initialized(tool);
        ICommand?[] commands = Values(probe, tool);
        StyledProperty<ICommand?> first = Properties(tool)[0];
        probe.Item.PropertyChanged += (_, e) => { if (e.Property == first) probe.Initialize(); };
        probe.ClearBindings();
        Assert.Null(probe.Item.GetValue(first));
        Assert.Equal(commands.Skip(1), Values(probe, tool).Skip(1));
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Clearing_Bindings_Removes_A_Local_Binding_To_The_Current_Default(bool tool)
    {
        IProbe probe = Initialized(tool);
        StyledProperty<ICommand?> first = Properties(tool)[0];
        using BehaviorSubject<ICommand?> source = new(probe.Item.GetValue(first));
        using IDisposable binding = probe.Item.Bind(first, source.ToBinding());
        probe.ClearBindings();
        source.OnNext(new ApplicationCommand());
        Assert.Null(probe.Item.GetValue(first));
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Clearing_Bindings_Before_Initialization_Removes_A_Null_Local_Binding(bool tool)
    {
        IProbe probe = Create(tool);
        StyledProperty<ICommand?> first = Properties(tool)[0];
        using BehaviorSubject<ICommand?> source = new(null);
        using IDisposable binding = probe.Item.Bind(first, source.ToBinding());
        probe.ClearBindings();
        source.OnNext(new ApplicationCommand());
        Assert.Null(probe.Item.GetValue(first));
    }

    [AvaloniaFact]
    public void Raw_Observable_Bindings_Retain_Avalonia_Subscription_Ownership()
    {
        ToolProbe probe = new();
        using BehaviorSubject<ICommand?> source = new(null);
        using IDisposable binding = probe.Bind(LayoutAnchorableItem.HideCommandProperty, source);
        probe.ClearBindings();
        ApplicationCommand command = new();
        source.OnNext(command);
        Assert.Same(command, probe.HideCommand);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Applying_And_Clearing_Defaults_Preserves_Application_Bindings(bool tool)
    {
        IProbe probe = Create(tool);
        StyledProperty<ICommand?> first = Properties(tool)[0];
        ApplicationCommand original = new();
        using BehaviorSubject<ICommand?> source = new(original);
        using IDisposable binding = probe.Item.Bind(first, source.ToBinding());
        probe.Initialize();
        Assert.Throws<NullReferenceException>(probe.Apply);
        probe.ClearBindings();
        Assert.Same(original, probe.Item.GetValue(first));
        ApplicationCommand replacement = new();
        source.OnNext(replacement);
        Assert.Same(replacement, probe.Item.GetValue(first));
        Assert.False(original.Disposed);
    }

    [AvaloniaFact]
    public void Derived_CanExecute_Predicates_Are_Read_Live_Without_Reinitializing()
    {
        DocumentProbe probe = (DocumentProbe)Initialized(false);
        ICommand command = probe.DockAsDocumentCommand!;
        probe.Enabled = true;
        Assert.True(command.CanExecute(null));
        probe.Enabled = false;
        Assert.False(command.CanExecute(null));
        probe.ClearDefinitions();
        probe.Enabled = true;
        Assert.True(command.CanExecute(null));
    }

    [AvaloniaFact]
    public void Restyling_Clears_And_Applies_Tool_Commands_Before_Common_Commands()
    {
        (DockingManager manager, LayoutAnchorableItem item) = AttachedTool();
        List<string> changes = Observe(item, values: true);
        manager.LayoutItemContainerStyle = new ControlTheme(typeof(LayoutAnchorableItem));
        string[] ordered = Properties(true).Concat(Properties(false)).Select(property => property.Name).ToArray();
        Assert.Equal(ordered.Select(name => name + ":null").Concat(ordered.Select(name => name + ":set")), changes);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Throwing_Tool_Command_Callbacks_Stop_Before_Common_Commands_And_Can_Recover(bool applying)
    {
        (DockingManager manager, LayoutAnchorableItem item) = AttachedTool();
        ICommand common = item.CloseCommand!;
        InvalidOperationException failure = new("command callback");
        bool fail = true;
        item.PropertyChanged += (_, e) =>
        {
            if (e.Property == LayoutAnchorableItem.HideCommandProperty && fail && (item.HideCommand is not null) == applying)
                throw failure;
        };
        Assert.Same(failure, Assert.Throws<InvalidOperationException>(() =>
            manager.LayoutItemContainerStyle = new ControlTheme(typeof(LayoutAnchorableItem))));
        Assert.Equal(applying, item.HideCommand is not null);
        if (applying) Assert.Null(item.CloseCommand);
        else Assert.Same(common, item.CloseCommand);
        fail = false;
        manager.LayoutItemContainerStyle = new ControlTheme(typeof(LayoutAnchorableItem));
        Assert.Same(common, item.CloseCommand);
        Assert.NotNull(item.AutoHideCommand);
        Assert.NotNull(item.DockCommand);
    }

    [AvaloniaFact]
    public void Tool_Clear_Callback_Can_Replace_Earlier_And_Later_Commands()
    {
        (DockingManager manager, LayoutAnchorableItem item) = AttachedTool();
        ApplicationCommand replacement = new();
        item.PropertyChanged += (_, e) =>
        {
            if (e.Property != LayoutAnchorableItem.HideCommandProperty || item.HideCommand is not null) return;
            item.HideCommand = replacement;
            item.FloatCommand = replacement;
        };
        manager.LayoutItemContainerStyle = new ControlTheme(typeof(LayoutAnchorableItem));
        Assert.Same(replacement, item.HideCommand);
        Assert.Same(replacement, item.FloatCommand);
        manager.Layout = new();
        Assert.False(replacement.Disposed);
    }

    [AvaloniaFact]
    public void Tool_Hooks_Can_Suppress_Base_Initialization_Application_And_Clearing()
    {
        ToolProbe probe = new() { Suppress = true };
        probe.Initialize();
        probe.Apply();
        Assert.Null(probe.HideCommand);
        probe.Suppress = false;
        probe.Initialize();
        Assert.Throws<NullReferenceException>(probe.Apply);
        ICommand command = probe.HideCommand!;
        probe.Suppress = true;
        probe.ClearDefinitions();
        probe.ClearBindings();
        Assert.Same(command, probe.HideCommand);
        probe.Suppress = false;
        probe.ClearBindings();
        Assert.Null(probe.HideCommand);
    }

    private static IProbe Create(bool tool) => tool ? new ToolProbe() : new DocumentProbe();
    private static IProbe Initialized(bool tool)
    {
        IProbe probe = Create(tool);
        probe.Initialize();
        Assert.Throws<NullReferenceException>(probe.Apply);
        return probe;
    }
    private static ICommand?[] Values(IProbe probe, bool tool) => Properties(tool).Select(probe.Item.GetValue).ToArray();
    private static StyledProperty<ICommand?>[] Properties(bool tool) => tool
        ? [LayoutAnchorableItem.HideCommandProperty, LayoutAnchorableItem.AutoHideCommandProperty, LayoutAnchorableItem.DockCommandProperty]
        : [LayoutItem.CloseCommandProperty, LayoutItem.FloatCommandProperty, LayoutItem.DockAsDocumentCommandProperty,
            LayoutItem.CloseAllButThisCommandProperty, LayoutItem.CloseAllCommandProperty, LayoutItem.ActivateCommandProperty,
            LayoutItem.NewVerticalTabGroupCommandProperty, LayoutItem.NewHorizontalTabGroupCommandProperty,
            LayoutItem.MoveToNextTabGroupCommandProperty, LayoutItem.MoveToPreviousTabGroupCommandProperty];
    private static List<string> Observe(LayoutItem item, bool values = false)
    {
        List<string> changes = new();
        item.PropertyChanged += (_, e) =>
        {
            if (e.Property.PropertyType == typeof(ICommand))
                changes.Add(e.Property.Name + (values ? item.GetValue(e.Property) is null ? ":null" : ":set" : ""));
        };
        return changes;
    }
    private static (DockingManager, LayoutAnchorableItem) AttachedTool()
    {
        LayoutAnchorable tool = new();
        DockingManager manager = new() { Layout = new() { RootPanel = new(new LayoutAnchorablePane(tool)) } };
        return (manager, (LayoutAnchorableItem)manager.GetLayoutItemFromModel(tool)!);
    }
    private interface IProbe
    {
        LayoutItem Item { get; }
        int PredicateCalls { get; }
        void Initialize();
        void Apply();
        void ClearBindings();
        void ClearDefinitions();
    }
    private sealed class ToolProbe : LayoutAnchorableItem, IProbe
    {
        public LayoutItem Item => this;
        public int PredicateCalls { get; private set; }
        internal bool Suppress { get; set; }
        public void Initialize() => InitDefaultCommands();
        public void Apply() => SetDefaultBindings();
        public void ClearBindings() => ClearDefaultBindings();
        public void ClearDefinitions() => ClearDefaultCommands();
        protected override bool CanExecuteDockAsDocumentCommand() { PredicateCalls++; return base.CanExecuteDockAsDocumentCommand(); }
        protected override void InitDefaultCommands() { if (!Suppress) base.InitDefaultCommands(); }
        protected override void SetDefaultBindings() { if (!Suppress) base.SetDefaultBindings(); }
        protected override void ClearDefaultBindings() { if (!Suppress) base.ClearDefaultBindings(); }
        protected override void ClearDefaultCommands() { if (!Suppress) base.ClearDefaultCommands(); }
    }
    private sealed class DocumentProbe : LayoutDocumentItem, IProbe
    {
        public LayoutItem Item => this;
        public int PredicateCalls { get; private set; }
        internal bool Enabled { get; set; }
        public void Initialize() => InitDefaultCommands();
        public void Apply() => SetDefaultBindings();
        public void ClearBindings() => ClearDefaultBindings();
        public void ClearDefinitions() => ClearDefaultCommands();
        protected override bool CanExecuteDockAsDocumentCommand() { PredicateCalls++; return Enabled; }
    }
    private sealed class ApplicationCommand : ICommand, IDisposable
    {
        internal bool Disposed { get; private set; }
        public event EventHandler? CanExecuteChanged { add { } remove { } }
        public bool CanExecute(object? parameter) => !Disposed;
        public void Execute(object? parameter) { }
        public void Dispose() => Disposed = true;
    }
}
