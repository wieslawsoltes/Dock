using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Recycling;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using DockQuickStartSample;
using DockQuickStartSample.ViewModels;
using ReactiveUI.Reactive;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class QuickStartDocumentContentTests
{
    [AvaloniaFact]
    public void Generated_Documents_With_Nested_ViewModels_Retain_Their_Views_When_Switched_And_Split()
    {
        var factory = new Dock.Model.Avalonia.Factory();
        var firstPage = new GeneratedPageViewModel { Text = "Page A" };
        var secondPage = new GeneratedPageViewModel { Text = "Page B" };
        var items = new ObservableCollection<GeneratedFileViewModel>
        {
            new() { CurrentView = firstPage },
            new() { CurrentView = secondPage }
        };
        var documents = new DocumentDock
        {
            DocumentTemplate = new DocumentTemplate
            {
                Content = new Func<IServiceProvider, object>(_ => new GeneratedDocumentContentView())
            },
            ItemsSource = items
        };
        var root = new RootDock
        {
            VisibleDockables = factory.CreateList<IDockable>(documents),
            ActiveDockable = documents,
            DefaultDockable = documents
        };
        var dockControl = new DockControl
        {
            Factory = factory, Layout = root, InitializeFactory = true, InitializeLayout = true
        };
        var first = Assert.IsType<Document>(documents.VisibleDockables![0]);
        var second = Assert.IsType<Document>(documents.VisibleDockables[1]);
        // Share the same recycling scope that XAML-generated models inherit from DockControl.
        var recycling = new ControlRecycling();
        ControlRecyclingDataTemplate.SetControlRecycling(dockControl, recycling);
        ControlRecyclingDataTemplate.SetControlRecycling(first, recycling);
        ControlRecyclingDataTemplate.SetControlRecycling(second, recycling);
        var window = new Window { Width = 900, Height = 600, Content = dockControl };
        window.Show();
        try
        {
            for (var visit = 0; visit < 4; visit++)
            {
                documents.ActiveDockable = first;
                Pump(window);
                AssertEditor(window, firstPage.Text);
                documents.ActiveDockable = second;
                Pump(window);
                AssertEditor(window, secondPage.Text);
            }

            factory.NewHorizontalDocumentDock(second);
            Pump(window);
            AssertEditor(window, firstPage.Text);
            AssertEditor(window, secondPage.Text);
            Assert.Equal(2, window.GetVisualDescendants().OfType<GeneratedDocumentContentView>().Count());

            firstPage.Text = "Edited page A";
            items[1].CurrentView = new GeneratedPageViewModel { Text = "Replacement page B" };
            Pump(window);
            AssertEditor(window, "Edited page A");
            AssertEditor(window, "Replacement page B");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Switching_Generated_Documents_Preserves_Each_Editor()
    {
        var viewModel = new MainWindowViewModel();
        var window = new MainWindow { DataContext = viewModel };
        window.Show();
        try
        {
            Pump(window);
            var dockControl = window.GetVisualDescendants().OfType<DockControl>().Single();
            var documents = Assert.IsType<DocumentDock>(dockControl.Layout!.VisibleDockables![0]);
            var first = documents.VisibleDockables![0];
            var second = Assert.IsType<Document>(documents.VisibleDockables[1]);

            for (var visit = 0; visit < 4; visit++)
            {
                SelectTab(window, first);
                AssertEditor(window, viewModel.Documents[0].Content);
                SelectTab(window, second);
                AssertEditor(window, viewModel.Documents[1].Content);
            }
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Splitting_Generated_Document_Keeps_Both_Editors_Visible()
    {
        var viewModel = new MainWindowViewModel();
        var window = new MainWindow { DataContext = viewModel };
        window.Show();
        try
        {
            Pump(window);
            var dockControl = window.GetVisualDescendants().OfType<DockControl>().Single();
            var documents = Assert.IsType<DocumentDock>(dockControl.Layout!.VisibleDockables![0]);
            var first = documents.VisibleDockables![0];
            var second = Assert.IsType<Document>(documents.VisibleDockables[1]);
            documents.ActiveDockable = second;
            Pump(window);

            var manager = new DockManager(new DockService());
            Assert.True(manager.ValidateDocument(second, documents, DragAction.Move, DockOperation.Right, true));
            Pump(window);

            Assert.NotSame(first.Owner, second.Owner);
            AssertEditor(window, viewModel.Documents[0].Content);
            AssertEditor(window, viewModel.Documents[1].Content);
        }
        finally
        {
            window.Close();
        }
    }

    private static void AssertEditor(Window window, string expected)
    {
        Assert.Contains(window.GetVisualDescendants().OfType<TextBox>(),
            editor => editor.IsEffectivelyVisible && editor.Text == expected && editor.Bounds.Width > 0);
    }

    private static void SelectTab(Window window, IDockable document)
    {
        var tab = window.GetVisualDescendants().OfType<DocumentTabStripItem>()
            .Single(item => ReferenceEquals(item.DataContext, document));
        var point = tab.TranslatePoint(new Point(tab.Bounds.Width / 2, tab.Bounds.Height / 2), window)!.Value;
        window.MouseDown(point, MouseButton.Left);
        window.MouseUp(point, MouseButton.Left);
        Pump(window);
        Assert.Same(document, ((IDock)document.Owner!).ActiveDockable);
    }

    private static void Pump(Window window)
    {
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
    }
}

public sealed class GeneratedFileViewModel : ReactiveObject
{
    private GeneratedPageViewModel? _currentView;

    public GeneratedPageViewModel? CurrentView
    {
        get => _currentView;
        set => this.RaiseAndSetIfChanged(ref _currentView, value);
    }
}

public sealed class GeneratedPageViewModel : ReactiveObject
{
    private string _text = string.Empty;

    public string Text
    {
        get => _text;
        set => this.RaiseAndSetIfChanged(ref _text, value);
    }
}
