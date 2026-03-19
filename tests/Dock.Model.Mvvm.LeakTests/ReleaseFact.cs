using System;
using System.Runtime.CompilerServices;
using Xunit;

namespace Dock.Model.Mvvm.LeakTests;

/// <summary>
/// Release-only fact for leak tests that are skipped in Debug builds.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
internal sealed class ReleaseFactAttribute : FactAttribute
{
    public ReleaseFactAttribute(
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
        : base(sourceFilePath, sourceLineNumber)
    {
#if DEBUG
        Skip = "Only runs in Release mode";
#endif
    }
}
