using System;
using System.Threading;
using System.Linq;
using Avalonia.Headless;

namespace Dock.Avalonia.LeakTests;

internal static class LeakTestSession
{
    private static int s_sessionDepth;
    private static int s_pendingDispatcherReset;

    internal static void RequestDispatcherReset()
    {
        if (Volatile.Read(ref s_sessionDepth) > 0)
        {
            Interlocked.Exchange(ref s_pendingDispatcherReset, 1);
            return;
        }

        LeakTestHelpers.ResetDispatcherForUnitTestsCore();
    }

    internal static void RunInSession(Action action)
    {
        Interlocked.Increment(ref s_sessionDepth);
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
            catch (NullReferenceException ex) when (IsHeadlessSessionDisposeNullReference(ex))
            {
            }
            LeakTestHelpers.DrainDispatcher();
            FlushDeferredDispatcherReset();
        }
    }

    internal static T RunInSession<T>(Func<T> action)
    {
        Interlocked.Increment(ref s_sessionDepth);
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
            catch (NullReferenceException ex) when (IsHeadlessSessionDisposeNullReference(ex))
            {
            }
            LeakTestHelpers.DrainDispatcher();
            FlushDeferredDispatcherReset();
        }
    }

    private static bool IsCompletedCollectionError(Exception exception)
    {
        return exception is InvalidOperationException { Message: not null } invalidOperation
               && invalidOperation.Message.Contains("marked as complete", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsHeadlessSessionDisposeNullReference(NullReferenceException exception)
    {
        return exception.StackTrace?.Contains("Avalonia.Headless.HeadlessUnitTestSession.Dispose", StringComparison.Ordinal) == true;
    }

    private static void FlushDeferredDispatcherReset()
    {
        if (Interlocked.Decrement(ref s_sessionDepth) != 0)
        {
            return;
        }

        if (Interlocked.Exchange(ref s_pendingDispatcherReset, 0) == 0)
        {
            return;
        }

        LeakTestHelpers.ResetDispatcherForUnitTestsCore();
    }
}
