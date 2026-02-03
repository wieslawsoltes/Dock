using System;
using System.Linq;
using System.Threading;
using Xunit;

namespace Dock.Model.ReactiveUI.LeakTests;

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
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }
}
