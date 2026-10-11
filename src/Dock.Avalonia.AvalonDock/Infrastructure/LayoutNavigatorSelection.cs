using System;
using Avalonia.Input;

namespace AvalonDock.Infrastructure;

internal static class LayoutNavigatorSelection
{
    internal static bool IsNavigationKey(Key key) => key is Key.Tab or Key.Up or Key.Down or Key.Left or Key.Right;

    internal static bool Move(Key key, int count, int otherCount, ref int index, out bool switchList)
    {
        switchList = false;
        if (!IsNavigationKey(key) || count == 0) return false;
        if (index < 0) index = 0;
        else if (key is Key.Left or Key.Right)
        {
            switchList = otherCount > 0;
            if (switchList) index = Math.Min(index, otherCount - 1);
        }
        else if (key == Key.Tab && index == count - 1 && otherCount > 0)
        {
            switchList = true;
            index = 0;
        }
        else index = (index + (key == Key.Up ? count - 1 : 1)) % count;
        return true;
    }
}
