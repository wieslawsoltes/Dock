using System;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class FactoryCommandOverloadTests
{
    public static TheoryData<string> Operations => new()
    {
        nameof(IFactory.PinDockable),
        nameof(IFactory.PreviewPinnedDockable),
        nameof(IFactory.TogglePreviewPinnedDockable),
        nameof(IFactory.FloatDockable),
        nameof(IFactory.FloatAllDockables),
        nameof(IFactory.DockAsDocument),
        nameof(IFactory.CloseDockable),
        nameof(IFactory.CloseOtherDockables),
        nameof(IFactory.CloseAllDockables),
        nameof(IFactory.CloseLeftDockables),
        nameof(IFactory.CloseRightDockables),
        nameof(IFactory.SetDocumentDockTabsLayoutLeft),
        nameof(IFactory.SetDocumentDockTabsLayoutTop),
        nameof(IFactory.SetDocumentDockTabsLayoutRight),
        nameof(IFactory.SetDocumentDockLayoutModeTabbed),
        nameof(IFactory.SetDocumentDockLayoutModeMdi),
        nameof(IFactory.NewHorizontalDocumentDock),
        nameof(IFactory.NewVerticalDocumentDock)
    };

    [Theory]
    [MemberData(nameof(Operations))]
    public void Object_Overload_Forwards_Dockable_To_Typed_Operation(string operation)
    {
        var recordingFactory = new RecordingFactory();
        IFactory factory = recordingFactory;
        var document = new Document();

        Invoke(factory, operation, document);

        Assert.Equal(operation, recordingFactory.LastOperation);
        Assert.Same(document, recordingFactory.LastDockable);
    }

    [Theory]
    [MemberData(nameof(Operations))]
    public void Object_Overload_Ignores_Null_And_Wrong_Type(string operation)
    {
        var recordingFactory = new RecordingFactory();
        IFactory factory = recordingFactory;

        Invoke(factory, operation, null);
        Invoke(factory, operation, new object());

        Assert.Null(recordingFactory.LastOperation);
        Assert.Null(recordingFactory.LastDockable);
    }

    private static void Invoke(IFactory factory, string operation, object? parameter)
    {
        switch (operation)
        {
            case nameof(IFactory.PinDockable):
                factory.PinDockable(parameter);
                break;
            case nameof(IFactory.PreviewPinnedDockable):
                factory.PreviewPinnedDockable(parameter);
                break;
            case nameof(IFactory.TogglePreviewPinnedDockable):
                factory.TogglePreviewPinnedDockable(parameter);
                break;
            case nameof(IFactory.FloatDockable):
                factory.FloatDockable(parameter);
                break;
            case nameof(IFactory.FloatAllDockables):
                factory.FloatAllDockables(parameter);
                break;
            case nameof(IFactory.DockAsDocument):
                factory.DockAsDocument(parameter);
                break;
            case nameof(IFactory.CloseDockable):
                factory.CloseDockable(parameter);
                break;
            case nameof(IFactory.CloseOtherDockables):
                factory.CloseOtherDockables(parameter);
                break;
            case nameof(IFactory.CloseAllDockables):
                factory.CloseAllDockables(parameter);
                break;
            case nameof(IFactory.CloseLeftDockables):
                factory.CloseLeftDockables(parameter);
                break;
            case nameof(IFactory.CloseRightDockables):
                factory.CloseRightDockables(parameter);
                break;
            case nameof(IFactory.SetDocumentDockTabsLayoutLeft):
                factory.SetDocumentDockTabsLayoutLeft(parameter);
                break;
            case nameof(IFactory.SetDocumentDockTabsLayoutTop):
                factory.SetDocumentDockTabsLayoutTop(parameter);
                break;
            case nameof(IFactory.SetDocumentDockTabsLayoutRight):
                factory.SetDocumentDockTabsLayoutRight(parameter);
                break;
            case nameof(IFactory.SetDocumentDockLayoutModeTabbed):
                factory.SetDocumentDockLayoutModeTabbed(parameter);
                break;
            case nameof(IFactory.SetDocumentDockLayoutModeMdi):
                factory.SetDocumentDockLayoutModeMdi(parameter);
                break;
            case nameof(IFactory.NewHorizontalDocumentDock):
                factory.NewHorizontalDocumentDock(parameter);
                break;
            case nameof(IFactory.NewVerticalDocumentDock):
                factory.NewVerticalDocumentDock(parameter);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(operation), operation, null);
        }
    }

    private sealed class RecordingFactory : Factory, IFactory
    {
        public string? LastOperation { get; private set; }

        public IDockable? LastDockable { get; private set; }

        private void Record(string operation, IDockable? dockable)
        {
            LastOperation = operation;
            LastDockable = dockable;
        }

        void IFactory.PinDockable(IDockable dockable) =>
            Record(nameof(IFactory.PinDockable), dockable);

        void IFactory.PreviewPinnedDockable(IDockable dockable) =>
            Record(nameof(IFactory.PreviewPinnedDockable), dockable);

        void IFactory.TogglePreviewPinnedDockable(IDockable dockable) =>
            Record(nameof(IFactory.TogglePreviewPinnedDockable), dockable);

        void IFactory.FloatDockable(IDockable dockable) =>
            Record(nameof(IFactory.FloatDockable), dockable);

        void IFactory.FloatAllDockables(IDockable dockable) =>
            Record(nameof(IFactory.FloatAllDockables), dockable);

        void IFactory.DockAsDocument(IDockable dockable) =>
            Record(nameof(IFactory.DockAsDocument), dockable);

        void IFactory.CloseDockable(IDockable? dockable) =>
            Record(nameof(IFactory.CloseDockable), dockable);

        void IFactory.CloseOtherDockables(IDockable dockable) =>
            Record(nameof(IFactory.CloseOtherDockables), dockable);

        void IFactory.CloseAllDockables(IDockable dockable) =>
            Record(nameof(IFactory.CloseAllDockables), dockable);

        void IFactory.CloseLeftDockables(IDockable dockable) =>
            Record(nameof(IFactory.CloseLeftDockables), dockable);

        void IFactory.CloseRightDockables(IDockable dockable) =>
            Record(nameof(IFactory.CloseRightDockables), dockable);

        void IFactory.SetDocumentDockTabsLayoutLeft(IDockable dockable) =>
            Record(nameof(IFactory.SetDocumentDockTabsLayoutLeft), dockable);

        void IFactory.SetDocumentDockTabsLayoutTop(IDockable dockable) =>
            Record(nameof(IFactory.SetDocumentDockTabsLayoutTop), dockable);

        void IFactory.SetDocumentDockTabsLayoutRight(IDockable dockable) =>
            Record(nameof(IFactory.SetDocumentDockTabsLayoutRight), dockable);

        void IFactory.SetDocumentDockLayoutModeTabbed(IDockable dockable) =>
            Record(nameof(IFactory.SetDocumentDockLayoutModeTabbed), dockable);

        void IFactory.SetDocumentDockLayoutModeMdi(IDockable dockable) =>
            Record(nameof(IFactory.SetDocumentDockLayoutModeMdi), dockable);

        void IFactory.NewHorizontalDocumentDock(IDockable dockable) =>
            Record(nameof(IFactory.NewHorizontalDocumentDock), dockable);

        void IFactory.NewVerticalDocumentDock(IDockable dockable) =>
            Record(nameof(IFactory.NewVerticalDocumentDock), dockable);
    }
}
