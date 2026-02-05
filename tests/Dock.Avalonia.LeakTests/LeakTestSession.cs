using System;
using System.Linq;
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
            session.Dispatch(() =>
            {
                LeakTestHelpers.EnsureFontManager();
                action();
            }, CancellationToken.None).GetAwaiter().GetResult();
            session.Dispatch(LeakTestHelpers.DrainDispatcher, CancellationToken.None).GetAwaiter().GetResult();
        }
        finally
        {
            try
            {
                session?.Dispose();
            }
            catch (AggregateException ex) when (ex.InnerExceptions.All(IsCompletedCollectionError))
            {
            }
            LeakTestHelpers.DrainDispatcher();
        }
    }

    internal static T RunInSession<T>(Func<T> action)
    {
        HeadlessUnitTestSession? session = null;
        try
        {
            session = HeadlessUnitTestSession.StartNew(typeof(LeakTestsApp));
            var result = session.Dispatch(() =>
            {
                LeakTestHelpers.EnsureFontManager();
                return action();
            }, CancellationToken.None).GetAwaiter().GetResult();
            session.Dispatch(LeakTestHelpers.DrainDispatcher, CancellationToken.None).GetAwaiter().GetResult();
            return result;
        }
        finally
        {
            try
            {
                session?.Dispose();
            }
            catch (AggregateException ex) when (ex.InnerExceptions.All(IsCompletedCollectionError))
            {
            }
            LeakTestHelpers.DrainDispatcher();
        }
    }

    private static bool IsCompletedCollectionError(Exception exception)
    {
        return exception is InvalidOperationException { Message: not null } invalidOperation
               && invalidOperation.Message.Contains("marked as complete", StringComparison.OrdinalIgnoreCase);
    }
}
