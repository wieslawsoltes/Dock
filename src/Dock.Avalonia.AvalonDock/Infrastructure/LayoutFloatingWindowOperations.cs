using System.Collections.Generic;
using System.Windows.Input;
using AvalonDock.Controls;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

// Resolve containers at invocation time: applications can replace commands
// while a window is open, including from another content's command handler.
internal static class LayoutFloatingWindowOperations
{
    internal static bool CanExecute(LayoutFloatingWindow model, bool hide, object? parameter)
    {
        if (model.Root?.Manager is not { } manager || hide && model is not LayoutAnchorableFloatingWindow) return false;
        foreach (LayoutContent content in Snapshot(model))
        {
            if (hide ? content is not LayoutAnchorable { CanHide: true } : !content.CanClose) return false;
            if (GetCommand(manager.GetLayoutItemFromModel(content), hide)?.CanExecute(parameter) != true) return false;
        }
        return true;
    }

    internal static void Execute(LayoutFloatingWindow model, bool hide, object? parameter)
    {
        if (model.Root?.Manager is not { } manager) return;
        foreach (LayoutContent content in Snapshot(model))
            GetCommand(manager.GetLayoutItemFromModel(content), hide)?.Execute(parameter);
    }

    internal static List<LayoutContent> Snapshot(LayoutFloatingWindow model)
    {
        List<LayoutContent> contents = new();
        foreach (ILayoutElement element in model.Descendents())
            if (element is LayoutContent content && (model is LayoutAnchorableFloatingWindow
                    ? content is LayoutAnchorable : content is LayoutDocument)) contents.Add(content);
        return contents;
    }

    private static ICommand? GetCommand(LayoutItem? item, bool hide) => hide
        ? (item as LayoutAnchorableItem)?.HideCommand : item?.CloseCommand;
}
