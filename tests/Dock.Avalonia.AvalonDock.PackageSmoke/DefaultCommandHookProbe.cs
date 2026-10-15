using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;
using Avalonia;
using Avalonia.Styling;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;

namespace PackageConsumer;

internal static class DefaultCommandHookProbe
{
    internal static void Verify()
    {
        List<string> failures = new();
        Check("unattached initialization", () =>
        {
            ToolItem item = new();
            item.Initialize();
            if (item.Queries != 0 || item.HideCommand is not null)
                throw new Exception("Initialization queried or applied a default command.");
        });
        Check("tool apply boundary", () =>
        {
            ToolItem item = new();
            item.Initialize();
            try { item.Apply(); throw new Exception("Applying without a model did not fail."); }
            catch (NullReferenceException) { }
            if (item.HideCommand is null || item.AutoHideCommand is null || item.DockCommand is null || item.CloseCommand is not null)
                throw new Exception("Unattached application did not stop after tool defaults.");
        });
        Check("tool clear order", () => VerifyOrder(false));
        Check("tool apply order", () => VerifyOrder(true));
        Check("null default binding", () =>
        {
            ToolItem item = new();
            using BehaviorSubject<ICommand?> source = new(null);
            using IDisposable binding = item.Bind(LayoutAnchorableItem.HideCommandProperty, source.ToBinding());
            item.Clear();
            source.OnNext(new Command());
            if (item.HideCommand is not null) throw new Exception("Clearing a null default retained its local binding.");
        });
        Check("reentrant initialization", () =>
        {
            ToolItem item = new();
            item.Initialize();
            bool initialized = false;
            item.PropertyChanged += (_, change) =>
            {
                if (change.Property != LayoutAnchorableItem.HideCommandProperty || initialized) return;
                initialized = true;
                item.Initialize();
            };
            try { item.Apply(); }
            catch (NullReferenceException) { }
            ICommand original = item.HideCommand ?? throw new Exception("The first default was not assigned.");
            item.Clear();
            if (!ReferenceEquals(original, item.HideCommand) || item.AutoHideCommand is not null || item.DockCommand is not null)
                throw new Exception("Reinitialization did not replace the remaining definitions.");
        });
        Check("live derived predicate", () =>
        {
            DocumentItem item = new();
            item.Initialize();
            try { item.Apply(); }
            catch (NullReferenceException) { }
            item.Enabled = true;
            if (item.DockAsDocumentCommand?.CanExecute(null) != true)
                throw new Exception("The command retained an earlier predicate result.");
            item.Enabled = false;
            if (item.DockAsDocumentCommand.CanExecute(null)) throw new Exception("The predicate change was ignored.");
        });
        if (failures.Count != 0) throw new Exception("Packaged default-command failures: " + string.Join("; ", failures));

        void Check(string name, Action action)
        {
            try { action(); }
            catch (Exception error) { failures.Add(name + ": " + error.Message); }
        }
    }

    private static void VerifyOrder(bool applying)
    {
        LayoutAnchorable tool = new();
        DockingManager manager = new() { Layout = new() { RootPanel = new(new LayoutAnchorablePane(tool)) } };
        LayoutAnchorableItem item = (LayoutAnchorableItem)manager.GetLayoutItemFromModel(tool)!;
        List<string> changes = new();
        item.PropertyChanged += (_, e) =>
        {
            if (e.Property.PropertyType == typeof(ICommand) && (item.GetValue(e.Property) is not null) == applying)
                changes.Add(e.Property.Name);
        };
        manager.LayoutItemContainerStyle = new ControlTheme(typeof(LayoutAnchorableItem));
        if (!changes.Take(4).SequenceEqual(new[] { "HideCommand", "AutoHideCommand", "DockCommand", "CloseCommand" }) || changes.Count != 13)
            throw new Exception("Tool defaults did not precede common defaults.");
        manager.Layout = new();
    }

    private sealed class ToolItem : LayoutAnchorableItem
    {
        internal int Queries { get; private set; }
        internal void Initialize() => base.InitDefaultCommands();
        internal void Apply() => base.SetDefaultBindings();
        internal void Clear() => base.ClearDefaultBindings();
        protected override bool CanExecuteDockAsDocumentCommand() { Queries++; return false; }
    }
    private sealed class DocumentItem : LayoutDocumentItem
    {
        internal bool Enabled { get; set; }
        internal void Initialize() => base.InitDefaultCommands();
        internal void Apply() => base.SetDefaultBindings();
        protected override bool CanExecuteDockAsDocumentCommand() => Enabled;
    }
    private sealed class Command : ICommand
    {
        public event EventHandler? CanExecuteChanged { add { } remove { } }
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) { }
    }
}
