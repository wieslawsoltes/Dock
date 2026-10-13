using System;
using System.Collections.Generic;
using System.Windows.Input;
using AvalonDock.Controls;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

// Resolve containers at invocation time: applications can replace commands
// while a window is open, including from another content's command handler.
internal static class LayoutFloatingWindowOperations
{
    internal static bool CanExecute<TContent>(ILayoutElement? model, bool hide, object? parameter) where TContent : LayoutContent
    {
        if (model?.Root?.Manager is not { } manager) return false;
        foreach (TContent content in Snapshot<TContent>(model))
        {
            if (hide ? content is not LayoutAnchorable { CanHide: true } : !content.CanClose) return false;
            if (GetCommand(manager.GetLayoutItemFromModel(content), hide)?.CanExecute(parameter) != true) return false;
        }
        return true;
    }

    internal static void Execute<TContent>(ILayoutElement? model, bool hide, object? parameter) where TContent : LayoutContent
    {
        // Direct execution retains upstream's failures for a missing model,
        // root, container, or command; CanExecute is the guarded query path.
        DockingManager? manager = model!.Root!.Manager;
        foreach (TContent content in Snapshot<TContent>(model))
        {
            ICommand? command = GetCommand(manager!.GetLayoutItemFromModel(content), hide);
            if (command is null) throw new NullReferenceException();
            command.Execute(parameter);
        }
    }

    internal static List<TContent> Snapshot<TContent>(ILayoutElement model) where TContent : LayoutContent
    {
        List<TContent> contents = new();
        foreach (ILayoutElement element in model.Descendents())
            if (element is TContent content) contents.Add(content);
        return contents;
    }

    private static ICommand? GetCommand(LayoutItem? item, bool hide) => hide
        ? (item as LayoutAnchorableItem)?.HideCommand : item?.CloseCommand;
}
