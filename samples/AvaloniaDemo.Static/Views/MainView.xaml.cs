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
        private readonly DockControl? _dockControl;
        private readonly IFactory? _factory;

        public MainView()
        {
            this.InitializeComponent();

            _dockControl = this.FindControl<DockControl>("dockControl");

            _factory = new Factory()
            {
                ContextLocator = new Dictionary<string, Func<object>>(),
                HostWindowLocator = new Dictionary<string, Func<IHostWindow>>
                {
                    [nameof(IDockWindow)] = () => new HostWindow()
                },
                DockableLocator = new Dictionary<string, Func<IDockable>>()
            };
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            if (_dockControl?.Layout is IDock layout)
            {
                _factory?.InitLayout(layout);
            }
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);

            if (_dockControl?.Layout is IDock layout)
            {
                layout.Close();
            }
        }
    }
}
