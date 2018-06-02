// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaDemo.CodeGen;
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

            this.FindControl<MenuItem>("FileNew").Click += (sender, e) =>
            {
                if (this.DataContext is MainWindowViewModel vm)
                {
                    if (vm.Layout is IViewsHost layoutViewsHost)
                    {
                        if (layoutViewsHost.CurrentView is IWindowsHost currentViewWindowsHost)
                        {
                            currentViewWindowsHost.HideWindows();
                        }
                    }

                    vm.Factory = new EmptyDockFactory();
                    vm.Layout = vm.Factory.CreateLayout();
                    vm.Factory.InitLayout(vm.Layout, vm);
                }
            };

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

                        if (vm.Layout is IWindowsHost layoutWindowsHost)
                        {
                            layoutWindowsHost.HideWindows();
                            if (layout is IViewsHost layoutViewsHost)
                            {
                                if (layoutViewsHost.CurrentView is IWindowsHost currentViewWindowsHost)
                                {
                                    currentViewWindowsHost.ShowWindows();
                                }
                            }
                        }

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

            this.FindControl<MenuItem>("FileGenerateCode").Click += async (sender, e) =>
            {
                var dlg = new SaveFileDialog();
                dlg.Filters.Add(new FileDialogFilter() { Name = "C#", Extensions = { "cs" } });
                dlg.Filters.Add(new FileDialogFilter() { Name = "All", Extensions = { "*" } });
                dlg.InitialFileName = "ViewModel";
                dlg.DefaultExtension = "cs";
                var result = await dlg.ShowAsync(this);
                if (result != null)
                {
                    if (this.DataContext is MainWindowViewModel vm)
                    {
                        ICodeGen codeGeb = new CSharpCodeGen();
                        codeGeb.Generate(vm.Layout, result);
                    }
                }
            };

            this.FindControl<MenuItem>("ViewEditor").Click += (sender, e) =>
            {
                if (this.DataContext is MainWindowViewModel vm)
                {
                    var factory = new EditorDockFactory();
                    var layout = factory.CreateLayout();
                    factory.InitLayout(layout, vm.Layout);

                    var window = factory.CreateWindowFrom(layout);
                    if (window != null)
                    {
                        if (vm.Layout is IWindowsHost layoutWindowsHost)
                        {
                            factory.AddWindow(layoutWindowsHost, window, vm.Layout);
                        }

                        window.X = 0;
                        window.Y = 0;
                        window.Width = 800;
                        window.Height = 600;
                        window.Present(false);
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
