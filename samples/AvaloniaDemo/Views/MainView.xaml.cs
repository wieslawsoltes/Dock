using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AvaloniaDemo.Factories;
using AvaloniaDemo.Model;
using AvaloniaDemo.Serializer;
using AvaloniaDemo.ViewModels;
using Dock.Avalonia;
using Dock.Model;
using Dock.Model.Controls;
using ReactiveUI.Legacy;

namespace AvaloniaDemo.Views
{
    public class MainView : UserControl
    {
#pragma warning disable CS0618 // Type or member is obsolete
        private DockJsonSerializer _serializer = new DockJsonSerializer(typeof(ReactiveList<>));
#pragma warning restore CS0618 // Type or member is obsolete

        public MainView()
        {
            this.InitializeComponent();

            this.FindControl<MenuItem>("FileNew").Click += (sender, e) =>
            {
                if (this.DataContext is MainWindowViewModel vm)
                {
                    if (vm.Layout is IDock root)
                    {
                        root.Close();
                    }
                    vm.Factory = new DemoDockFactory(new DemoData());
                    vm.Layout = vm.Factory.CreateLayout();
                    vm.Factory.InitLayout(vm.Layout);
                }
            };

            this.FindControl<MenuItem>("FileOpen").Click += async (sender, e) =>
            {
                var dlg = new OpenFileDialog();
                dlg.Filters.Add(new FileDialogFilter() { Name = "Json", Extensions = { "json" } });
                dlg.Filters.Add(new FileDialogFilter() { Name = "All", Extensions = { "*" } });
                var result = await dlg.ShowAsync(GetWindow());
                if (result != null)
                {
                    if (this.DataContext is MainWindowViewModel vm)
                    {
                        IDock layout = _serializer.Load<RootDock>(result.FirstOrDefault());
                        if (vm.Layout is IDock root)
                        {
                            root.Close();
                        }
                        vm.Layout = layout;
                        vm.Factory.InitLayout(vm.Layout);
                    }
                }
            };

            this.FindControl<MenuItem>("FileSaveAs").Click += async (sender, e) =>
            {
                var dlg = new SaveFileDialog();
                dlg.Filters.Add(new FileDialogFilter() { Name = "Json", Extensions = { "json" } });
                dlg.Filters.Add(new FileDialogFilter() { Name = "All", Extensions = { "*" } });
                dlg.InitialFileName = "Layout";
                dlg.DefaultExtension = "json";
                var result = await dlg.ShowAsync(GetWindow());
                if (result != null)
                {
                    if (this.DataContext is MainWindowViewModel vm)
                    {
                        _serializer.Save(result, vm.Layout);
                    }
                }
            };

            this.FindControl<MenuItem>("OptionsDragBehaviorIsEnabled").Click += (sender, e) =>
            {
                bool isEnabled = (bool)GetValue(DragBehavior.IsEnabledProperty);
                SetValue(DragBehavior.IsEnabledProperty, !isEnabled);
            };

            this.FindControl<MenuItem>("OptionsDropBehaviorIsEnabled").Click += (sender, e) =>
            {
                bool isEnabled = (bool)GetValue(DropBehavior.IsEnabledProperty);
                SetValue(DropBehavior.IsEnabledProperty, !isEnabled);
            };
        }

        private Window GetWindow()
        {
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                return desktopLifetime.MainWindow;
            }
            return null;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
