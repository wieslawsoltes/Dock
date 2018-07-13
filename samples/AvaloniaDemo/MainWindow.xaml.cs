// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaDemo.CodeGen;
using AvaloniaDemo.Serializer;
using AvaloniaDemo.ViewModels;
using AvaloniaDemo.ViewModels.Tools;
using Dock.Avalonia.Controls;
using Dock.Model;
using Dock.Model.Controls;

namespace AvaloniaDemo
{
    public class MainWindow : HostWindowBase
    {
        public MainWindow()
        {
            this.InitializeComponent();
            this.AttachDevTools();

            this.Closing += (sender, e) =>
            {
                if (this.DataContext is MainWindowViewModel vm)
                {
                    if (vm.Layout is IDock dock)
                    {
                        dock.Close();
                    }
                };
            };

        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
