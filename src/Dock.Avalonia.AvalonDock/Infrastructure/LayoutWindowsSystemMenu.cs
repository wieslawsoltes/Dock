using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;

namespace AvalonDock.Infrastructure;

// Avalonia 12.1 does not open a system menu for its extended client area.
// Windows owns the menu and command labels; the native host executes commands.
internal sealed partial class LayoutWindowsSystemMenu : ILayoutSystemMenu
{
    private const uint Restore = 0xF120;

    public bool TryShow(Window window, PixelPoint position, bool canClose)
    {
        if (!OperatingSystem.IsWindows() || window.TryGetPlatformHandle() is not { HandleDescriptor: "HWND" } handle ||
            handle.Handle == IntPtr.Zero) return false;
        IntPtr menu = GetSystemMenu(handle.Handle, false);
        if (menu == IntPtr.Zero) return false;

        WindowState state = window.WindowState;
        Enable(menu, Restore, state != WindowState.Normal);
        Enable(menu, 0xF010, state is WindowState.Normal or WindowState.FullScreen);
        Enable(menu, 0xF000, state == WindowState.Normal && window.CanResize);
        Enable(menu, 0xF020, state is not (WindowState.Minimized or WindowState.FullScreen) && window.CanMinimize);
        Enable(menu, 0xF030, state is not (WindowState.Maximized or WindowState.FullScreen) && window.CanMaximize);
        // Avalonia disables SC_CLOSE while extending into the native caption.
        // The floating host still owns command enablement and close/hide vetoes.
        Enable(menu, 0xF060, canClose);
        _ = SetForegroundWindow(handle.Handle);
        uint command = TrackPopupMenu(menu, 0x0102, position.X, position.Y, 0, handle.Handle, IntPtr.Zero);
        _ = PostMessageW(handle.Handle, 0, IntPtr.Zero, IntPtr.Zero);
        if (command == Restore && window.WindowState == WindowState.FullScreen)
            window.SetCurrentValue(Window.WindowStateProperty, WindowState.Normal);
        else if (command != 0)
            _ = PostMessageW(handle.Handle, 0x0112, (IntPtr)command,
                (IntPtr)unchecked((ushort)position.X | ((ushort)position.Y << 16)));
        return true;
    }

    private static void Enable(IntPtr menu, uint command, bool enabled) =>
        _ = EnableMenuItem(menu, command, enabled ? 0u : 1u);

    [LibraryImport("user32.dll")]
    private static partial IntPtr GetSystemMenu(IntPtr window, [MarshalAs(UnmanagedType.Bool)] bool revert);
    [LibraryImport("user32.dll")]
    private static partial uint EnableMenuItem(IntPtr menu, uint item, uint flags);
    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetForegroundWindow(IntPtr window);
    [LibraryImport("user32.dll")]
    private static partial uint TrackPopupMenu(IntPtr menu, uint flags, int x, int y, int reserved, IntPtr window, IntPtr rectangle);
    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool PostMessageW(IntPtr window, uint message, IntPtr wParam, IntPtr lParam);
}
