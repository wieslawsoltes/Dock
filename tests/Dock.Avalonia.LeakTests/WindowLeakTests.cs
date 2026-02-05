using Xunit;

namespace Dock.Avalonia.LeakTests;

[Collection("LeakTests")]
public class WindowLeakTests
{
    [ReleaseFact]
    public void AllWindows_AttachDetach_DoesNotLeak()
    {
        foreach (var testCase in LeakTestCases.WindowCases)
        {
            var result = LeakTestCaseRunner.RunWindowCase(testCase, exerciseInput: false);
            LeakTestCaseRunner.AssertCollectedForCase(result, keepAlive: true);
        }
    }

    [ReleaseFact]
    public void AllWindows_InputInteractions_DoesNotLeak()
    {
        foreach (var testCase in LeakTestCases.WindowCases)
        {
            var result = LeakTestCaseRunner.RunWindowCase(testCase, exerciseInput: true);
            LeakTestCaseRunner.AssertCollectedForCase(result, keepAlive: true);
        }
    }
}
