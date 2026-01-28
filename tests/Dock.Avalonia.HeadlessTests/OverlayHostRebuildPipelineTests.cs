using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls.Overlays;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class OverlayHostRebuildPipelineTests
{
    [AvaloniaFact]
    public void RebuildPipeline_Detaches_Content_From_VisualParent()
    {
        var content = new Border();
        var existingPresenter = new ContentPresenter { Content = content };
        var panel = new StackPanel
        {
            Children =
            {
                existingPresenter
            }
        };
        var window = new Window { Content = panel };

        window.Show();
        window.UpdateLayout();

        try
        {
            Assert.Same(existingPresenter, content.GetVisualParent());

            if (content.Parent is not null)
            {
                // Simulate a visual parent without a logical parent.
                ((ISetLogicalParent)content).SetParent(null);
            }

            var host = new OverlayHost { Content = content };
            panel.Children.Add(host);
            window.UpdateLayout();

            Assert.NotNull(content.GetVisualParent());
            Assert.NotSame(existingPresenter, content.GetVisualParent());
        }
        finally
        {
            window.Close();
        }
    }
}
