using System;
using System.Collections.Generic;
using Dock.Avalonia.Controls;
using Dock.Model;
using Dock.Model.Controls;
using Dock.Model.Controls.Editor;
using AvaloniaDemo.ViewModels;

namespace AvaloniaDemo.ReactiveUI.ViewModels
{
    public class EmptyDockFactory : DockFactory
    {
        public override IDock CreateLayout()
        {
            var view = new ViewStub
            {
                Id = nameof(IView),
                Title = nameof(IView)
            };

            return new RootDock
            {
                Id = nameof(IRootDock),
                Title = nameof(IRootDock),
                CurrentView = view,
                DefaultView = view,
                Views = CreateList<IView>(view)
            };
        }

        public override void InitLayout(IView layout, object context)
        {
            this.ContextLocator = new Dictionary<string, Func<object>>
            {
                [nameof(IRootDock)] = () => context,
                [nameof(ILayoutDock)] = () => context,
                [nameof(IDocumentDock)] = () => context,
                [nameof(IToolDock)] = () => context,
                [nameof(ISplitterDock)] = () => context,
                [nameof(IDockWindow)] = () => context,
                [nameof(IDocumentTab)] = () => context,
                [nameof(IToolTab)] = () => context,
            };

            this.HostLocator = new Dictionary<string, Func<IDockHost>>
            {
                [nameof(IDockWindow)] = () => new HostWindow()
            };

            base.InitLayout(layout, context);
        }
    }
}
