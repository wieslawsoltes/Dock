using System;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using DockAvalonDockSample;
using DockAvalonDockSample.Services;
using DockAvalonDockSample.ViewModels;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class SampleTests
{
    [AvaloniaFact]
    public void ViewModel_Commands_Delegate_To_Workspace_And_Publish_Status()
    {
        FakeWorkspace workspace = new();
        MainWindowViewModel viewModel = new(workspace);
        ((ICommand)viewModel.SaveLayout).Execute(null);
        Assert.Equal("saved", viewModel.Status);
        ((ICommand)viewModel.LoadLayout).Execute(null);
        Assert.Equal("loaded", viewModel.Status);
        ((ICommand)viewModel.HideJournal).Execute(null);
        Assert.Equal("hidden", viewModel.Status);
        ((ICommand)viewModel.ShowJournal).Execute(null);
        Assert.Equal("shown", viewModel.Status);
        Assert.Equal(4, workspace.Operations);
    }

    [AvaloniaFact]
    public void Sample_Renders_And_Restores_Closed_Documents_With_Editor_Text()
    {
        MainWindow window = new();
        string path = Path.Combine(Path.GetTempPath(), "dock-avalondock-" + Guid.NewGuid().ToString("N"), "layout.xml");
        try
        {
            DockingManager manager = window.FindControl<DockingManager>("DockManager")!;
            AvalonDockWorkspace service = new(manager, path);
            window.DataContext = new MainWindowViewModel(service);
            window.Show();
            Dispatcher.UIThread.RunJobs();
            Assert.Contains(window.GetVisualDescendants().OfType<TextBlock>(), text => text.Text == "Properties");
            Assert.Contains("Save a layout", service.Load());
            LayoutDocument editor = manager.Layout.Descendents().OfType<LayoutDocument>().Single(x => x.ContentId == "document2");
            ((TextBox)editor.Content!).Text = "Unsaved editor text";
            Assert.Contains("Saved layout", service.Save());
            service.HideJournal();
            Assert.Single(manager.Layout.Hidden);
            service.ShowJournal();
            Assert.Empty(manager.Layout.Hidden);
            editor.Close();
            Assert.Single(manager.Layout.Descendents().OfType<LayoutDocument>());
            Assert.Contains("Layout restored", service.Load());
            Dispatcher.UIThread.RunJobs();
            LayoutDocument restored = manager.Layout.Descendents().OfType<LayoutDocument>().Single(x => x.ContentId == "document2");
            Assert.Equal("Unsaved editor text", ((TextBox)restored.Content!).Text);
            restored.IsActive = true;
            Dispatcher.UIThread.RunJobs();
            Assert.True(((TextBox)restored.Content!).Bounds.Width > 0);
        }
        finally
        {
            window.Close();
            if (Directory.Exists(Path.GetDirectoryName(path))) Directory.Delete(Path.GetDirectoryName(path)!, true);
        }
    }

    private sealed class FakeWorkspace : ILayoutWorkspace
    {
        internal int Operations { get; private set; }
        public string Save() { Operations++; return "saved"; }
        public string Load() { Operations++; return "loaded"; }
        public string HideJournal() { Operations++; return "hidden"; }
        public string ShowJournal() { Operations++; return "shown"; }
    }
}
