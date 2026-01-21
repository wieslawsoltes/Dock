using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Dock.Avalonia.Behaviors;
using Dock.Model.Services;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class VisualTreeLifecycleBehaviorTests
{
    private sealed class TestLifecycleControl : Control, IVisualTreeLifecycle
    {
        public int AttachedCount { get; private set; }
        public int DetachedCount { get; private set; }

        public void OnAttachedToVisualTree()
        {
            AttachedCount++;
        }

        public void OnDetachedFromVisualTree()
        {
            DetachedCount++;
        }
    }

    private sealed class TestLifecycleViewModel : IVisualTreeLifecycle
    {
        public int AttachedCount { get; private set; }
        public int DetachedCount { get; private set; }

        public void OnAttachedToVisualTree()
        {
            AttachedCount++;
        }

        public void OnDetachedFromVisualTree()
        {
            DetachedCount++;
        }
    }

    [AvaloniaFact]
    public void VisualTreeLifecycleBehavior_Reattaches_ForControlAndDataContext()
    {
        var control = new TestLifecycleControl();
        var viewModel = new TestLifecycleViewModel();

        control.DataContext = viewModel;
        VisualTreeLifecycleBehavior.SetIsEnabled(control, true);

        var window = new Window
        {
            Width = 300,
            Height = 200,
            Content = control
        };

        try
        {
            window.Show();
            window.UpdateLayout();

            Assert.Equal(1, control.AttachedCount);
            Assert.Equal(1, viewModel.AttachedCount);

            window.Content = null;
            window.UpdateLayout();

            Assert.Equal(1, control.DetachedCount);
            Assert.Equal(1, viewModel.DetachedCount);

            window.Content = control;
            window.UpdateLayout();

            Assert.Equal(2, control.AttachedCount);
            Assert.Equal(2, viewModel.AttachedCount);
        }
        finally
        {
            window.Close();
        }
    }
}
