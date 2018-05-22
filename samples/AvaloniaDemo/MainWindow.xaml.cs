// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Dock.Avalonia;

namespace AvaloniaDemo
{
    public class MainWindow : HostWindowBase
    {
        public MainWindow()
        {
            this.InitializeComponent();
            this.AttachDevTools();

            DataContextChanged += (sender, e) =>
            {
                if (DataContext is MainWindowViewModel vm)
                {
                    // TODO: Windows are not shown when layout is first loaded. They show only when you call Navigate.
                    vm.Layout?.CurrentView?.ShowWindows();
                }
            };
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
