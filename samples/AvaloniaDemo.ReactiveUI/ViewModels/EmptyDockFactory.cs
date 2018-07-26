using System;
using System.Collections.Generic;
using Dock.Avalonia.Controls;
using Dock.Model;
using Dock.Model.Controls;
using Dock.Model.Controls.Editor;

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

        public override void InitLayout(IView layout)
        {
            this.ContextLocator = new Dictionary<string, Func<object>>
            {
                [nameof(IRootDock)] = () => layout,
                [nameof(ILayoutDock)] = () => layout,
                [nameof(IDocumentDock)] = () => layout,
                [nameof(IToolDock)] = () => layout,
                [nameof(ISplitterDock)] = () => layout,
                [nameof(IDockWindow)] = () => layout,
                [nameof(IDocumentTab)] = () => layout,
                [nameof(IToolTab)] = () => layout,
            };

            this.HostLocator = new Dictionary<string, Func<IDockHost>>
            {
                [nameof(IDockWindow)] = () => new HostWindow()
            };

            base.InitLayout(layout);
        }
    }
}
