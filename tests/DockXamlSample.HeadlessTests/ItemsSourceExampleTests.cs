using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia.Controls;
using DockXamlSample;
using Xunit;

namespace DockXamlSample.HeadlessTests;

public class ItemsSourceExampleTests
{
    [AvaloniaFact]
    public void AddDocumentCommand_AddsVisibleDocumentTab()
    {
        var view = new ItemsSourceExample();
        var window = new Window
        {
            Width = 800,
            Height = 600,
            Content = view
        };

        window.Show();
        try
        {
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var viewModel = Assert.IsType<ItemsSourceExampleViewModel>(view.DataContext);
            var dockControl = Assert.IsType<DockControl>(view.FindControl<DockControl>("DockControl"));
            var documentDock = Assert.IsType<DocumentDock>(dockControl.Layout?.ActiveDockable);
            var tabStrip = Assert.Single(view.GetVisualDescendants().OfType<DocumentTabStrip>());

            Assert.Equal(2, documentDock.VisibleDockables?.Count);
            Assert.Equal(2, tabStrip.ItemsView.Count);

            viewModel.AddDocumentCommand.Execute(null);
            window.UpdateLayout();

            Assert.Equal(3, viewModel.Documents.Count);
            Assert.Equal(3, documentDock.VisibleDockables?.Count);
            Assert.Equal(3, tabStrip.ItemsView.Count);
            Assert.Contains(
                tabStrip.GetVisualDescendants().OfType<TextBlock>(),
                textBlock => textBlock.Text == "Document 1");
        }
        finally
        {
            window.Close();
        }
    }
}
