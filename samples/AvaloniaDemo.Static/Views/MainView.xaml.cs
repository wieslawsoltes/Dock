using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Dock.Avalonia.Controls;
using Dock.Model;

namespace AvaloniaDemo.Views
{
    public class MainView : UserControl
    {
        public MainView()
        {
            this.InitializeComponent();

            var dockControl = this.FindControl<DockControl>("dockControl");

            if (dockControl != null)
            {
                dockControl.Factory = new Factory()
                {
                    ContextLocator = new Dictionary<string, Func<object>>(),
                    HostWindowLocator = new Dictionary<string, Func<IHostWindow>>
                    {
                        [nameof(IDockWindow)] = () => new HostWindow()
                    },
                    DockableLocator = new Dictionary<string, Func<IDockable>>()
                };
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
