using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Dock.Avalonia.Controls;
using Dock.Model;
using Dock.Model.Controls;

namespace AvaloniaDemo.Views
{
    public class EmptyFactory : Factory
    {
        public override IDock CreateLayout()
        {
            return new RootDock();
        }

        public override void InitLayout(IDockable layout)
        {
            this.HostWindowLocator = new Dictionary<string, Func<IHostWindow>>
            {
                [nameof(IDockWindow)] = () => new HostWindow()
            };

            base.InitLayout(layout);
        }
    }

    public class MainView : UserControl
    {
        private DockControl? _dockControl;
        private IFactory? _factory;

        public MainView()
        {
            this.InitializeComponent();

            _dockControl = this.FindControl<DockControl>("dockControl");
            _factory = new EmptyFactory();
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
