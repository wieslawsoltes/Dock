using System.Windows.Input;
using AvaloniaDemo.Models;
using Dock.Model.Controls;
using Dock.Model.Core;
using ReactiveUI;

namespace AvaloniaDemo.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        private readonly IFactory? _factory;
        private IRootDock? _layout;

        public IRootDock? Layout
        {
            get => _layout;
            set => this.RaiseAndSetIfChanged(ref _layout, value);
        }

        public ICommand NewLayout { get; }

        public MainWindowViewModel()
        {
            _factory = new DemoFactory(new DemoData());

            Layout = _factory?.CreateLayout();
            if (Layout is { })
            {
                _factory?.InitLayout(Layout);
                if (Layout is { } root)
                {
                    root.Navigate.Execute("Home");
                }
            }

            NewLayout = ReactiveCommand.Create(ResetLayout);
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

            var layout = _factory?.CreateLayout();
            if (layout is not null)
            {
                Layout = layout as IRootDock;
                _factory?.InitLayout(layout);
            }
        }
    }
}
