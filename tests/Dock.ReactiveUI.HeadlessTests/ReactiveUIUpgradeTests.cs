using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model.ReactiveUI;
using Dock.Model.ReactiveUI.Navigation.ViewModels;
using Dock.Model.ReactiveUI.Services.Avalonia.Controls;
using DockReactiveUIDiSample.ViewModels.Documents;
using DockReactiveUIDiSample.Views.Documents;
using ReactiveUI.Reactive;
using Xunit;

namespace Dock.ReactiveUI.HeadlessTests;

public class ReactiveUIUpgradeTests
{
    [AvaloniaFact]
    public void CodeOnlySample_Binds_Commands_And_Docking_State()
    {
        var viewModel = new DockCodeOnlySample.MainWindowViewModel();
        var window = new DockCodeOnlySample.MainWindow { ViewModel = viewModel };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var dock = window.GetVisualDescendants().OfType<DockControl>().Single();
            var checkBox = window.GetVisualDescendants().OfType<CheckBox>().Single();
            var saveButton = window.GetVisualDescendants().OfType<Button>()
                .Single(button => Equals(button.Content, "Save Workspace A"));

            Assert.Same(viewModel.Factory, dock.Factory);
            Assert.Same(viewModel.Layout, dock.Layout);
            Assert.Same(viewModel.SaveWorkspaceA, saveButton.Command);
            Assert.True(saveButton.Command!.CanExecute(null));

            checkBox.IsChecked = false;
            Dispatcher.UIThread.RunJobs();
            Assert.False(viewModel.IsDockingEnabled);
            Assert.False(dock.IsDockingEnabled);

            viewModel.IsDockingEnabled = true;
            Dispatcher.UIThread.RunJobs();
            Assert.True(checkBox.IsChecked);
            Assert.True(dock.IsDockingEnabled);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void DiLocator_Uses_Registered_Factory_And_Assigns_ViewModel()
    {
        var viewModel = new DocumentViewModel();
        var view = new DocumentView();
        var calls = 0;
        var locator = new DockReactiveUIDiSample.ViewLocator(new Dictionary<Type, Func<IViewFor>>
        {
            [typeof(DocumentViewModel)] = () => { calls++; return view; }
        });

        Assert.True(locator.Match(viewModel));
        Assert.Equal(0, calls);
        Assert.Same(view, locator.ResolveView(viewModel, null));
        Assert.Same(viewModel, view.ViewModel);
        Assert.Same(view, locator.ResolveView((object)viewModel, null));
        Assert.Same(view, locator.ResolveViewUnsafe(viewModel, null));
        Assert.Same(view, locator.Build(viewModel));
        Assert.Equal(4, calls);
        Assert.Null(locator.ResolveView((object?)null, null));
        Assert.Null(locator.ResolveView(new object(), null));
        Assert.False(locator.Match(new object()));
    }

    [AvaloniaFact]
    public void CanonicalLocator_Resolves_ViewModel_Held_As_Object()
    {
        var viewModel = new DockViewModel(new TestScreen(), new Factory());
        var view = DockReactiveUICanonicalSample.App.ViewLocator.ResolveView((object)viewModel, null);

        Assert.IsType<DockReactiveUICanonicalSample.Views.DockView>(view);
        Assert.Same(viewModel, view!.ViewModel);
    }

    [AvaloniaFact]
    public void GenericDockView_Activates_Replacement_ViewModel_And_Deactivates_On_Close()
    {
        var first = new TestViewModel();
        var second = new TestViewModel();
        var view = new TestView { ViewModel = first };
        var window = new Window { Content = view };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(1, first.ActiveCount);

            view.ViewModel = second;
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(0, first.ActiveCount);
            Assert.Equal(1, second.ActiveCount);
        }
        finally
        {
            window.Close();
        }

        Dispatcher.UIThread.RunJobs();
        Assert.Equal(0, second.ActiveCount);
    }

    private sealed class TestScreen : IScreen
    {
        public RoutingState Router { get; } = new();
    }

    private sealed class TestView : DockReactiveUserControl<TestViewModel>;

    private sealed class TestViewModel : ReactiveObject, IActivatableViewModel
    {
        public TestViewModel()
        {
            this.WhenActivated(disposables =>
            {
                ActiveCount++;
                disposables.Add(Disposable.Create(() => ActiveCount--));
            });
        }

        public ViewModelActivator Activator { get; } = new();
        public int ActiveCount { get; private set; }
    }
}
