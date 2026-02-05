using System;
using System.Collections.Generic;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Themes.Fluent;
using Dock.Avalonia.Themes.Fluent;
using static Dock.Avalonia.LeakTests.LeakTestHelpers;
using static Dock.Avalonia.LeakTests.LeakTestSession;

namespace Dock.Avalonia.LeakTests;

internal static class LeakTestCaseRunner
{
    internal static LeakResult RunControlCase(ControlLeakCase testCase, bool exerciseInput)
    {
        return RunInSession(() =>
        {
            var context = LeakContext.Create();
            var setup = testCase.Create(context);

            var window = new Window { Content = setup.Control };
            var windowRef = new WeakReference(window);

            if (LeakTestHelpers.IsLeakTraceEnabled)
            {
                window.AddHandler(Window.WindowClosedEvent, (_, _) =>
                    LeakTestHelpers.Trace($"[LeakTrace:{testCase.Name}/window] WindowClosedEvent raised."));
            }
            window.Styles.Add(new FluentTheme());
            window.Styles.Add(new DockFluentTheme());

            ShowWindow(window);

            if (exerciseInput)
            {
                setup.AfterShow?.Invoke(setup.Control);
                ExerciseInputInteractions(setup.Control, interactionMask: setup.InteractionMask);
            }
            setup.BeforeCleanup?.Invoke(setup.Control);

            var result = new LeakResult(
                testCase.Name,
                new[] { new WeakReference(setup.Control) },
                setup.KeepAlive,
                windowRef);

            CleanupWindow(window);
            return result;
        });
    }

    internal static LeakResult RunWindowCase(WindowLeakCase testCase, bool exerciseInput)
    {
        return RunInSession(() =>
        {
            var context = LeakContext.Create();
            var setup = testCase.Create(context);
            setup.Window.Styles.Add(new FluentTheme());

            ShowWindow(setup.Window);
            if (exerciseInput)
            {
                ExerciseInputInteractions(setup.Window);
            }

            var windowRef = new WeakReference(setup.Window);

            if (LeakTestHelpers.IsLeakTraceEnabled)
            {
                setup.Window.AddHandler(Window.WindowClosedEvent, (_, _) =>
                    LeakTestHelpers.Trace($"[LeakTrace:{testCase.Name}/window] WindowClosedEvent raised."));
            }
            var result = new LeakResult(
                testCase.Name,
                new[] { new WeakReference(setup.Window) },
                setup.KeepAlive,
                windowRef);
            CleanupWindow(setup.Window);
            return result;
        });
    }

    internal static void AssertCollectedForCase(LeakResult result, bool keepAlive)
    {
        var references = result.References;

        LeakTestHelpers.ResetDispatcherForUnitTests();

        for (var attempt = 0; attempt < 20 && AnyAlive(references); attempt++)
        {
            CollectGarbage();
            Thread.Sleep(10);
        }

        foreach (var reference in references)
        {
            Xunit.Assert.False(reference.IsAlive, $"{result.Name} leaked");
        }

        if (keepAlive)
        {
            foreach (var keepAliveItem in result.KeepAlive)
            {
                System.GC.KeepAlive(keepAliveItem);
            }
        }
    }

    private static bool AnyAlive(IReadOnlyList<WeakReference> references)
    {
        for (var i = 0; i < references.Count; i++)
        {
            if (references[i].IsAlive)
            {
                return true;
            }
        }

        return false;
    }
}
