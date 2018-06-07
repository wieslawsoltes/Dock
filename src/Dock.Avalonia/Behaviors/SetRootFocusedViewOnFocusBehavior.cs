using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Xaml.Interactivity;
using Dock.Model;
using System;

namespace Dock.Avalonia.Behaviors
{
    class SetRootFocusedViewOnPointerPressedBehavior : Behavior<Control>
    {
        private IDisposable _disposable;

        protected override void OnAttached()
        {
            base.OnAttached();

            _disposable = AssociatedObject.AddHandler(Control.PointerPressedEvent, (sender, e) =>
            {
                if (AssociatedObject.DataContext is IViewsHost host)
                {
                    var root = FindRoot(host.CurrentView);

                    if (root is IViewsHost rootHost)
                    {
                        if (rootHost.FocusedView.Parent is IViewsHost parentRootHost)
                        {
                            parentRootHost.IsActive = false;
                        }

                        rootHost.FocusedView = host.CurrentView;

                        host.IsActive = true;
                    }
                }
            }, global::Avalonia.Interactivity.RoutingStrategies.Tunnel);
        }

        private IView FindRoot(IView view)
        {
            if (view.Parent == null)
            {
                return view;
            }

            return FindRoot(view.Parent);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            _disposable.Dispose();
        }
    }
}
