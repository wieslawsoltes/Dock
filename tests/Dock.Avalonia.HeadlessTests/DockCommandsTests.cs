using System;
using System.Windows.Input;
using Dock.Avalonia.Commands;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class DockCommandsTests
{
    public static TheoryData<string> CommandNames => new()
    {
        nameof(DockCommands.PinDockable),
        nameof(DockCommands.PreviewPinnedDockable),
        nameof(DockCommands.TogglePreviewPinnedDockable),
        nameof(DockCommands.FloatDockable),
        nameof(DockCommands.FloatAllDockables),
        nameof(DockCommands.DockAsDocument),
        nameof(DockCommands.CloseDockable),
        nameof(DockCommands.CloseOtherDockables),
        nameof(DockCommands.CloseAllDockables),
        nameof(DockCommands.CloseLeftDockables),
        nameof(DockCommands.CloseRightDockables),
        nameof(DockCommands.SetDocumentDockTabsLayoutLeft),
        nameof(DockCommands.SetDocumentDockTabsLayoutTop),
        nameof(DockCommands.SetDocumentDockTabsLayoutRight),
        nameof(DockCommands.SetDocumentDockLayoutModeTabbed),
        nameof(DockCommands.SetDocumentDockLayoutModeMdi),
        nameof(DockCommands.NewHorizontalDocumentDock),
        nameof(DockCommands.NewVerticalDocumentDock)
    };

    [Theory]
    [MemberData(nameof(CommandNames))]
    public void Execute_Forwards_Dockable_To_Typed_Factory(string operation)
    {
        var command = GetCommand(operation);
        var factory = new RecordingFactory();
        var owner = new DocumentDock { Factory = factory };
        var document = new Document { Owner = owner };

        Assert.True(command.CanExecute(document));

        command.Execute(document);

        Assert.Equal(operation, factory.LastOperation);
        Assert.Same(document, factory.LastDockable);
    }

    [Theory]
    [MemberData(nameof(CommandNames))]
    public void Invalid_Parameters_Are_Not_Executable(string operation)
    {
        var command = GetCommand(operation);
        var dockableWithoutFactory = new Document();

        Assert.False(command.CanExecute(null));
        Assert.False(command.CanExecute(new object()));
        Assert.False(command.CanExecute(dockableWithoutFactory));

        command.Execute(null);
        command.Execute(new object());
        command.Execute(dockableWithoutFactory);
    }

    private static ICommand GetCommand(string operation) =>
        operation switch
        {
            nameof(DockCommands.PinDockable) => DockCommands.PinDockable,
            nameof(DockCommands.PreviewPinnedDockable) => DockCommands.PreviewPinnedDockable,
            nameof(DockCommands.TogglePreviewPinnedDockable) => DockCommands.TogglePreviewPinnedDockable,
            nameof(DockCommands.FloatDockable) => DockCommands.FloatDockable,
            nameof(DockCommands.FloatAllDockables) => DockCommands.FloatAllDockables,
            nameof(DockCommands.DockAsDocument) => DockCommands.DockAsDocument,
            nameof(DockCommands.CloseDockable) => DockCommands.CloseDockable,
            nameof(DockCommands.CloseOtherDockables) => DockCommands.CloseOtherDockables,
            nameof(DockCommands.CloseAllDockables) => DockCommands.CloseAllDockables,
            nameof(DockCommands.CloseLeftDockables) => DockCommands.CloseLeftDockables,
            nameof(DockCommands.CloseRightDockables) => DockCommands.CloseRightDockables,
            nameof(DockCommands.SetDocumentDockTabsLayoutLeft) => DockCommands.SetDocumentDockTabsLayoutLeft,
            nameof(DockCommands.SetDocumentDockTabsLayoutTop) => DockCommands.SetDocumentDockTabsLayoutTop,
            nameof(DockCommands.SetDocumentDockTabsLayoutRight) => DockCommands.SetDocumentDockTabsLayoutRight,
            nameof(DockCommands.SetDocumentDockLayoutModeTabbed) => DockCommands.SetDocumentDockLayoutModeTabbed,
            nameof(DockCommands.SetDocumentDockLayoutModeMdi) => DockCommands.SetDocumentDockLayoutModeMdi,
            nameof(DockCommands.NewHorizontalDocumentDock) => DockCommands.NewHorizontalDocumentDock,
            nameof(DockCommands.NewVerticalDocumentDock) => DockCommands.NewVerticalDocumentDock,
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
        };

    private sealed class RecordingFactory : Factory, IFactory
    {
        public string? LastOperation { get; private set; }

        public IDockable? LastDockable { get; private set; }

        private void Record(string operation, IDockable dockable)
        {
            LastOperation = operation;
            LastDockable = dockable;
        }

        void IFactory.PinDockable(IDockable dockable) =>
            Record(nameof(DockCommands.PinDockable), dockable);

        void IFactory.PreviewPinnedDockable(IDockable dockable) =>
            Record(nameof(DockCommands.PreviewPinnedDockable), dockable);

        void IFactory.TogglePreviewPinnedDockable(IDockable dockable) =>
            Record(nameof(DockCommands.TogglePreviewPinnedDockable), dockable);

        void IFactory.FloatDockable(IDockable dockable) =>
            Record(nameof(DockCommands.FloatDockable), dockable);

        void IFactory.FloatAllDockables(IDockable dockable) =>
            Record(nameof(DockCommands.FloatAllDockables), dockable);

        void IFactory.DockAsDocument(IDockable dockable) =>
            Record(nameof(DockCommands.DockAsDocument), dockable);

        void IFactory.CloseDockable(IDockable dockable) =>
            Record(nameof(DockCommands.CloseDockable), dockable);

        void IFactory.CloseOtherDockables(IDockable dockable) =>
            Record(nameof(DockCommands.CloseOtherDockables), dockable);

        void IFactory.CloseAllDockables(IDockable dockable) =>
            Record(nameof(DockCommands.CloseAllDockables), dockable);

        void IFactory.CloseLeftDockables(IDockable dockable) =>
            Record(nameof(DockCommands.CloseLeftDockables), dockable);

        void IFactory.CloseRightDockables(IDockable dockable) =>
            Record(nameof(DockCommands.CloseRightDockables), dockable);

        void IFactory.SetDocumentDockTabsLayoutLeft(IDockable dockable) =>
            Record(nameof(DockCommands.SetDocumentDockTabsLayoutLeft), dockable);

        void IFactory.SetDocumentDockTabsLayoutTop(IDockable dockable) =>
            Record(nameof(DockCommands.SetDocumentDockTabsLayoutTop), dockable);

        void IFactory.SetDocumentDockTabsLayoutRight(IDockable dockable) =>
            Record(nameof(DockCommands.SetDocumentDockTabsLayoutRight), dockable);

        void IFactory.SetDocumentDockLayoutModeTabbed(IDockable dockable) =>
            Record(nameof(DockCommands.SetDocumentDockLayoutModeTabbed), dockable);

        void IFactory.SetDocumentDockLayoutModeMdi(IDockable dockable) =>
            Record(nameof(DockCommands.SetDocumentDockLayoutModeMdi), dockable);

        void IFactory.NewHorizontalDocumentDock(IDockable dockable) =>
            Record(nameof(DockCommands.NewHorizontalDocumentDock), dockable);

        void IFactory.NewVerticalDocumentDock(IDockable dockable) =>
            Record(nameof(DockCommands.NewVerticalDocumentDock), dockable);
    }
}
