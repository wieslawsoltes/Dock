using Xunit;

namespace DockMvvmSample.AppiumTests.Infrastructure;

/// <summary>
/// This collection definition ensures that Appium tests run serially rather than in parallel.
/// This is important because UI automation tests typically require exclusive access to the application
/// and can interfere with each other if run simultaneously.
/// </summary>
[CollectionDefinition("AppiumTests", DisableParallelization = true)]
public class AppiumTestCollection
{
    // This class has no code, and is never instantiated.
    // Its purpose is simply to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
} 