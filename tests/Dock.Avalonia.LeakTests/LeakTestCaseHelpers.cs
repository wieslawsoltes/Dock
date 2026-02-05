using Avalonia.Controls;
using Avalonia.VisualTree;
using static Dock.Avalonia.LeakTests.LeakTestHelpers;

namespace Dock.Avalonia.LeakTests;

internal static class LeakTestCaseHelpers
{
    private static readonly InputInteractionMask ButtonInteractionMask =
        InputInteractionMask.PointerEnterExit | InputInteractionMask.PointerMove | InputInteractionMask.PointerPressRelease;

    internal static void ApplyNoOpCommand(Button? button)
    {
        if (button is null)
        {
            return;
        }

        button.SetCurrentValue(Button.CommandProperty, new NoOpCommand());
    }

    internal static void ExerciseButtonInteractions(Button? button)
    {
        if (button is null)
        {
            return;
        }

        ExerciseInputInteractions(button, interactionMask: ButtonInteractionMask);
    }

    internal static T? FindTemplateChild<T>(Control root, string name) where T : Control
    {
        if (root.Name == name && root is T match)
        {
            return match;
        }

        foreach (var visual in root.GetVisualDescendants())
        {
            if (visual is T control && control.Name == name)
            {
                return control;
            }
        }

        return null;
    }
}
