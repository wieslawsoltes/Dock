using System;
using Xunit;

namespace Dock.Avalonia.LeakTests;

/// <summary>
/// Release-only fact for leak tests that are skipped in Debug builds.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
internal sealed class ReleaseFactAttribute : FactAttribute
{
    public ReleaseFactAttribute()
    {
#if DEBUG
        Skip = "Only runs in Release mode";
#endif
    }
}
