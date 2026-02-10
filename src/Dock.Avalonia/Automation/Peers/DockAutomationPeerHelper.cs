// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Dock.Model.Core;

namespace Dock.Avalonia.Automation.Peers;

internal static class DockAutomationPeerHelper
{
    internal static string ResolveName(Control owner, string fallbackName, IDockable? dockable = null)
    {
        var explicitName = AutomationProperties.GetName(owner);
        if (!string.IsNullOrWhiteSpace(explicitName))
        {
            return explicitName!;
        }

        var dockableTitle = dockable?.Title;
        if (!string.IsNullOrWhiteSpace(dockableTitle))
        {
            return dockableTitle!;
        }

        if (owner.DataContext is IDockable dataDockable)
        {
            var dataTitle = dataDockable.Title;
            if (!string.IsNullOrWhiteSpace(dataTitle))
            {
                return dataTitle!;
            }
        }

        return fallbackName;
    }

    internal static string ResolveAutomationId(Control owner, IDockable? dockable = null)
    {
        var explicitId = AutomationProperties.GetAutomationId(owner);
        if (!string.IsNullOrWhiteSpace(explicitId))
        {
            return explicitId!;
        }

        var dockableId = dockable?.Id;
        if (!string.IsNullOrWhiteSpace(dockableId))
        {
            return dockableId!;
        }

        if (owner.DataContext is IDockable dataDockable)
        {
            var dataId = dataDockable.Id;
            if (!string.IsNullOrWhiteSpace(dataId))
            {
                return dataId!;
            }
        }

        return string.Empty;
    }

    internal static string FormatState(params (string Key, object? Value)[] states)
    {
        if (states.Length == 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder(states.Length * 24);
        for (var i = 0; i < states.Length; i++)
        {
            if (i > 0)
            {
                builder.Append("; ");
            }

            builder.Append(states[i].Key);
            builder.Append('=');
            builder.Append(FormatValue(states[i].Value));
        }

        return builder.ToString();
    }

    internal static bool TryActivateDockable(IDockable? dockable)
    {
        if (dockable?.Owner is not IDock owner || owner.Factory is null)
        {
            return false;
        }

        owner.Factory.SetActiveDockable(dockable);
        if (owner.Factory.FindRoot(dockable, _ => true) is IDock root)
        {
            owner.Factory.SetFocusedDockable(root, dockable);
        }

        owner.Factory.ActivateWindow(dockable);
        return true;
    }

    internal static AutomationPeer? TryGetContainerPeer(ItemsControl owner, object? item)
    {
        if (item is null)
        {
            return null;
        }

        if (owner.ContainerFromItem(item) is Control container)
        {
            return ControlAutomationPeer.CreatePeerForElement(container);
        }

        var items = owner.Items;
        for (var index = 0; index < items.Count; index++)
        {
            var candidate = items[index];
            if (!ReferenceEquals(candidate, item) && !Equals(candidate, item))
            {
                continue;
            }

            if (owner.ContainerFromIndex(index) is Control fallbackContainer)
            {
                return ControlAutomationPeer.CreatePeerForElement(fallbackContainer);
            }

            break;
        }

        return null;
    }

    internal static IReadOnlyList<AutomationPeer> ToSelectionList(AutomationPeer? selectionPeer)
    {
        return selectionPeer is null
            ? Array.Empty<AutomationPeer>()
            : new[] { selectionPeer };
    }

    private static string FormatValue(object? value)
    {
        return value switch
        {
            null => "null",
            bool b => b ? "true" : "false",
            Enum e => e.ToString(),
            _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty
        };
    }
}
