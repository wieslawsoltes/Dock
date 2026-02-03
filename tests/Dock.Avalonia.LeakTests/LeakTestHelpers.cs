using System;
using System.Linq;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Threading;
using Xunit;

namespace Dock.Avalonia.LeakTests;

internal static class LeakTestHelpers
{
    internal static void AssertCollected(params WeakReference[] references)
    {
        for (var attempt = 0; attempt < 20 && references.Any(reference => reference.IsAlive); attempt++)
        {
            CollectGarbage();
            Thread.Sleep(10);
        }

        foreach (var reference in references)
        {
            Assert.False(reference.IsAlive);
        }
    }

    internal static void CollectGarbage()
    {
        DrainDispatcher();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        DrainDispatcher();
    }

    internal static void DrainDispatcher()
    {
        var dispatcher = Dispatcher.UIThread;
        for (var i = 0; i < 5; i++)
        {
            dispatcher.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        }
    }

    internal static void ShowWindow(Window window)
    {
        if (window.SizeToContent != SizeToContent.Manual)
        {
            window.SizeToContent = SizeToContent.Manual;
        }

        if (double.IsNaN(window.Width) || window.Width <= 0)
        {
            window.Width = 800;
        }

        if (double.IsNaN(window.Height) || window.Height <= 0)
        {
            window.Height = 600;
        }

        window.Show();
        DrainDispatcher();
        window.UpdateLayout();
        DrainDispatcher();
    }

    internal static void CleanupWindow(Window window)
    {
        window.FocusManager?.ClearFocus();
        window.Content = null;
        window.DataContext = null;
        window.Close();
        DrainDispatcher();
    }
}
