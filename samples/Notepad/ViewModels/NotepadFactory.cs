using System;
using System.Text;
using System.Collections.Generic;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI;
using Dock.Model.ReactiveUI.Controls;
using Notepad.ViewModels.Docks;
using Notepad.ViewModels.Documents;
using Notepad.ViewModels.Tools;

namespace Notepad.ViewModels
{
    public class NotepadFactory : Factory
    {
        public override IDocumentDock CreateDocumentDock() => new FilesDocumentDock();

        public override IDock CreateLayout()
        {
            var untitledFileViewModel = new FileViewModel()
            {
                Path = string.Empty,
                Title = "Untitled",
                Text = "",
                Encoding = Encoding.Default.WebName
            };

            var findViewModel = new FindViewModel
            {
                Id = "Find",
                Title = "Find"
            };

            var replaceViewModel = new ReplaceViewModel
            {
                Id = "Replace",
                Title = "Replace"
            };

            var files = new FilesDocumentDock()
            {
                Id = "Files",
                Title = "Files",
                IsCollapsable = false,
                Proportion = double.NaN,
                ActiveDockable = untitledFileViewModel,
                VisibleDockables = CreateList<IDockable>
                (
                    untitledFileViewModel
                ),
                CanCreateDocument = true
            };

            var tools = new ProportionalDock
            {
                Proportion = 0.2,
                Orientation = Orientation.Vertical,
                VisibleDockables = CreateList<IDockable>
                (
                    new ToolDock
                    {
                        ActiveDockable = findViewModel,
                        VisibleDockables = CreateList<IDockable>
                        (
                            findViewModel
                        )
                    },
                    new SplitterDock(),
                    new ToolDock
                    {
                        ActiveDockable = replaceViewModel,
                        VisibleDockables = CreateList<IDockable>
                        (
                            replaceViewModel
                        )
                    }
                )
            };

            var windowLayout = CreateRootDock();
            windowLayout.Title = "Default";
            var windowLayoutContent = new ProportionalDock
            {
                Orientation = Orientation.Horizontal,
                IsCollapsable = false,
                VisibleDockables = CreateList<IDockable>
                (
                    files,
                    new SplitterDock(),
                    tools
                )
            };
            windowLayout.IsCollapsable = false;
            windowLayout.VisibleDockables = CreateList<IDockable>(windowLayoutContent);
            windowLayout.ActiveDockable = windowLayoutContent;

            var root = CreateRootDock();

            root.IsCollapsable = false;
            root.VisibleDockables = CreateList<IDockable>(windowLayout);
            root.ActiveDockable = windowLayout;
            root.DefaultDockable = windowLayout;

            return root;
        }

        public override void InitLayout(IDockable layout)
        {
            ContextLocator = new Dictionary<string, Func<object>>
            {
                ["Find"] = () => layout,
                ["Replace"] = () => layout
            };

            HostWindowLocator = new Dictionary<string, Func<IHostWindow>>
            {
                [nameof(IDockWindow)] = () => new HostWindow()
            };

            DockableLocator = new Dictionary<string, Func<IDockable>>();

            base.InitLayout(layout);
        }
    }
}
