using System;
using System.Threading;
using Avalonia.Headless;

namespace Dock.Avalonia.LeakTests;

internal static class LeakTestSession
{
    internal static void RunInSession(Action action)
    {
        HeadlessUnitTestSession? session = null;
        try
        {
            session = HeadlessUnitTestSession.StartNew(typeof(LeakTestsApp));
            session.Dispatch(action, CancellationToken.None).GetAwaiter().GetResult();
        }
        finally
        {
            session?.Dispose();
            LeakTestHelpers.DrainDispatcher();
        }
    }

    internal static T RunInSession<T>(Func<T> action)
    {
        HeadlessUnitTestSession? session = null;
        try
        {
            session = HeadlessUnitTestSession.StartNew(typeof(LeakTestsApp));
            return session.Dispatch(action, CancellationToken.None).GetAwaiter().GetResult();
        }
        finally
        {
            session?.Dispose();
            LeakTestHelpers.DrainDispatcher();
        }
    }
}
