using System.Windows.Input;
using AvaloniaDemo.Models;
using Dock.Model.Controls;
using Dock.Model.Core;
using ReactiveUI;

namespace AvaloniaDemo.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        private IFactory? _factory;
        private IRootDock? _layout;

        public IFactory? Factory
        {
            get => _factory;
            set => this.RaiseAndSetIfChanged(ref _factory, value);
        }

        public IRootDock? Layout
        {
            get => _layout;
            set => this.RaiseAndSetIfChanged(ref _layout, value);
        }

        public ICommand NewLayout { get; }

        public MainWindowViewModel()
        {
            NewLayout = ReactiveCommand.Create(ResetLayout);

            Factory = new DemoFactory(new DemoData());;
            Layout = Factory?.CreateLayout();

            if (Layout != null)
            {
                Factory?.InitLayout(Layout);
                if (Layout is { } root)
                {
                    root.Navigate.Execute("Home");
                }
            }
        }

        public void CloseLayout()
        {
            if (Layout is IDock dock)
            {
                if (dock.Close.CanExecute(null))
                {
                    dock.Close.Execute(null);
                }
            }
        }

        public void ResetLayout()
        {
            if (Layout is not null)
            {
                if (Layout.Close.CanExecute(null))
                {
                    Layout.Close.Execute(null);
                }
            }

            var layout = Factory?.CreateLayout();
            if (layout is not null)
            {
                Layout = layout as IRootDock;
                Factory?.InitLayout(layout);
            }
        }
    }
}
