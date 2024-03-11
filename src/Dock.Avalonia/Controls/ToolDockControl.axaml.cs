using Avalonia;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Interaction logic for <see cref="ToolDockControl"/> xaml.
/// </summary>
public class ToolDockControl : TemplatedControl
{
    private ProportionalStackPanel? GetPanel()
    {
        if (Parent is ContentPresenter presenter)
        {
            if (presenter.GetVisualParent() is ProportionalStackPanel panel)
            {
                return panel;
            }
        }
        else if (this.GetVisualParent() is ProportionalStackPanel psp)
        {
            return psp;
        }

        return null;
    }
}
