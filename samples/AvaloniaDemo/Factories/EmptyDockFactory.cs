using System;
using System.Collections.Generic;
using Dock.Avalonia.Controls;
using Dock.Model;
using Dock.Model.Controls;
using Dock.Model.Controls.Editor;

namespace AvaloniaDemo.Factories
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

            var root = CreateRootDock();

            root.Id = nameof(IRootDock);
            root.Title = nameof(IRootDock);
            root.CurrentView = view;
            root.DefaultView = view;
            root.Views = CreateList<IView>(view);
            root.Top = CreatePinDock();
            root.Top.Alignment = Alignment.Top;
            root.Bottom = CreatePinDock();
            root.Bottom.Alignment = Alignment.Bottom;
            root.Left = CreatePinDock();
            root.Left.Alignment = Alignment.Left;
            root.Right = CreatePinDock();
            root.Right.Alignment = Alignment.Right;

            return root;
        }

        public override void InitLayout(IView layout)
        {
            this.ContextLocator = new Dictionary<string, Func<object>>
            {
                [nameof(IRootDock)] = () => layout,
                [nameof(IPinDock)] = () => layout,
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
