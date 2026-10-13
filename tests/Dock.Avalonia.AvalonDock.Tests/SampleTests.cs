using System;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using DockAvalonDockSample;
using DockAvalonDockSample.Services;
using DockAvalonDockSample.ViewModels;
using AvalonDock;
using AvalonDock.Layout;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class SampleTests
{
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Sample_Persistence_Recovers_Identified_Content_And_Tolerates_Unidentified_Items(bool tool)
    {
        object content = new();
        LayoutDocument identified = new() { ContentId = "editor", Content = content };
        LayoutDocumentPane documents = new(identified);
        LayoutRoot root = new() { RootPanel = new LayoutPanel(documents) };
        if (tool) root.RootPanel.Children.Add(new LayoutAnchorablePane(new LayoutAnchorable { Content = new object() }));
        else documents.Children.Add(new LayoutDocument { Content = new object() });
        DockingManager manager = new() { Layout = root };
        string path = Path.Combine(Path.GetTempPath(), "dock-avalondock-" + Guid.NewGuid().ToString("N"), "layout.xml");
        try
        {
            AvalonDockWorkspace service = new(manager, path);
            Assert.Contains("Saved layout", service.Save());
            Assert.Contains("Layout restored", service.Load());
            LayoutContent[] restoredItems = manager.Layout.Descendents().OfType<LayoutContent>().ToArray();
            Assert.Equal(2, restoredItems.Length);
            LayoutContent restored = Assert.Single(restoredItems, item => item.ContentId == "editor");
            Assert.Equal("editor", restored.ContentId);
            Assert.Same(content, restored.Content);
            LayoutContent unidentified = Assert.Single(restoredItems, item => item.ContentId is null);
            Assert.Null(unidentified.Content);
            Assert.Equal(tool, unidentified is LayoutAnchorable);
        }
        finally
        {
            if (Directory.Exists(Path.GetDirectoryName(path))) Directory.Delete(Path.GetDirectoryName(path)!, true);
        }
    }

    [AvaloniaFact]
    public void Sample_Save_Load_Restores_Floating_Document_And_Journal()
    {
        MainWindow window = new();
        string path = Path.Combine(Path.GetTempPath(), "dock-avalondock-" + Guid.NewGuid().ToString("N"), "layout.xml");
        DockingManager manager = window.FindControl<DockingManager>("DockManager")!;
        try
        {
            AvalonDockWorkspace service = new(manager, path);
            window.DataContext = new MainWindowViewModel(service);
            window.Show();
            Dispatcher.UIThread.RunJobs();
            LayoutDocument document = manager.Layout.Descendents().OfType<LayoutDocument>().Single(x => x.ContentId == "document2");
            TextBox editor = Assert.IsType<TextBox>(document.Content);
            editor.Text = "Floating sample edits";
            document.Float();
            LayoutAnchorable journal = manager.Layout.Descendents().OfType<LayoutAnchorable>().Single(x => x.ContentId == "journal");
            journal.Float();
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(2, manager.Layout.FloatingWindows.Count);
            Assert.Contains("Saved layout", service.Save());
            Assert.Contains("Layout restored", service.Load());
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(2, manager.Layout.FloatingWindows.Count);
            LayoutDocument restored = manager.Layout.Descendents().OfType<LayoutDocument>().Single(x => x.ContentId == "document2");
            Assert.Same(editor, restored.Content);
            Assert.Equal("Floating sample edits", editor.Text);
            Assert.True(editor.Bounds.Width > 0);
            Assert.True(manager.Layout.Descendents().OfType<LayoutAnchorable>().Single(x => x.ContentId == "journal").IsFloating);
        }
        finally
        {
            manager.Layout.FloatingWindows.Clear();
            window.Close();
            if (Directory.Exists(Path.GetDirectoryName(path))) Directory.Delete(Path.GetDirectoryName(path)!, true);
        }
    }

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
            Assert.Contains(window.GetVisualDescendants().OfType<TextBlock>(), text => text.Text == "Agenda");
            Assert.Equal(2, Assert.Single(manager.Layout.LeftSide!.Children).Children.Count);
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
            Assert.Equal(2, Assert.Single(manager.Layout.LeftSide!.Children).Children.Count);
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

    [AvaloniaFact]
    public void Auto_Hide_Tab_Preview_Preserves_Public_Group_And_Root()
    {
        MainWindow window = new();
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            DockingManager manager = window.FindControl<DockingManager>("DockManager")!;
            LayoutAnchorGroup group = Assert.Single(manager.Layout.LeftSide!.Children);
            LayoutAnchorable agenda = group.Children[0];
            TextBlock header = window.GetVisualDescendants().OfType<TextBlock>().First(x => x.Text == "Agenda");
            Point position = header.TranslatePoint(new Point(header.Bounds.Width / 2, header.Bounds.Height / 2), window)!.Value;
            window.MouseDown(position, MouseButton.Left);
            window.MouseUp(position, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
            Assert.Same(agenda.DockModel, manager.DockLayout.PinnedDock?.ActiveDockable);
            Assert.Same(group, agenda.Parent);
            Assert.Same(manager.Layout, agenda.Root);
            Assert.True(agenda.IsAutoHidden);
            Assert.Contains(window.GetVisualDescendants().OfType<TextBlock>(), x => x.Text == "Agenda Content" && x.Bounds.Width > 0);
            agenda.ToggleAutoHide();
            Dispatcher.UIThread.RunJobs();
            Assert.False(agenda.IsAutoHidden);
            Assert.IsType<LayoutAnchorablePane>(agenda.Parent);
            Assert.Same(manager.Layout, agenda.Root);
        }
        finally { window.Close(); }
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
