using Xunit;

namespace Dock.Avalonia.LeakTests;

[Collection("LeakTests")]
public class ControlInputLeakTests
{
    [ReleaseFact]
    public void AllControls_InputInteractions_DoesNotLeak()
    {
        foreach (var testCase in LeakTestCases.ControlCases)
        {
            var result = LeakTestCaseRunner.RunControlCase(testCase, exerciseInput: true);
            LeakTestCaseRunner.AssertCollectedForCase(result, keepAlive: false);
        }
    }
}
