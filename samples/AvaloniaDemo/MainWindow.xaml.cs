// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaDemo.ViewModels;
using Dock.Avalonia.Controls;
using Dock.Model;
using Dock.Model.Controls;
using Dock.Serializer;

namespace AvaloniaDemo
{
    public class MainWindow : HostWindowBase
    {
        public MainWindow()
        {
            this.InitializeComponent();
            this.AttachDevTools();

            this.FindControl<MenuItem>("FileOpen").Click += async (sender, e) =>
            {
                var dlg = new OpenFileDialog();
                dlg.Filters.Add(new FileDialogFilter() { Name = "Json", Extensions = { "json" } });
                dlg.Filters.Add(new FileDialogFilter() { Name = "All", Extensions = { "*" } });
                var result = await dlg.ShowAsync(this);
                if (result != null)
                {
                    if (this.DataContext is MainWindowViewModel vm)
                    {
                        IDock layout = DockSerializer.Load<RootDock>(result.FirstOrDefault());
                        vm.Layout.CurrentView.HideWindows();
                        vm.Layout = layout;
                        vm.Factory.InitLayout(vm.Layout, vm);
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
                var result = await dlg.ShowAsync(this);
                if (result != null)
                {
                    if (this.DataContext is MainWindowViewModel vm)
                    {
                        DockSerializer.Save(result, vm.Layout);
                    }
                }
            };
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
