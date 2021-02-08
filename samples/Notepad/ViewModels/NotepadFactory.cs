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
        private IRootDock? _rootDock;
        private IDocumentDock? _documentDock;
        private ITool? _findTool;
        private ITool? _replaceTool;

        public override IDocumentDock CreateDocumentDock() => new FilesDocumentDock();

        public override IRootDock CreateLayout()
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

            var documentDock = new FilesDocumentDock()
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
                        ),
                        Alignment = Alignment.Right,
                        GripMode = GripMode.Visible
                    },
                    new SplitterDockable(),
                    new ToolDock
                    {
                        ActiveDockable = replaceViewModel,
                        VisibleDockables = CreateList<IDockable>
                        (
                            replaceViewModel
                        ),
                        Alignment = Alignment.Right,
                        GripMode = GripMode.Visible
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
                    documentDock,
                    new SplitterDockable(),
                    tools
                )
            };
            windowLayout.IsCollapsable = false;
            windowLayout.VisibleDockables = CreateList<IDockable>(windowLayoutContent);
            windowLayout.ActiveDockable = windowLayoutContent;

            var rootDock = CreateRootDock();

            rootDock.IsCollapsable = false;
            rootDock.VisibleDockables = CreateList<IDockable>(windowLayout);
            rootDock.ActiveDockable = windowLayout;
            rootDock.DefaultDockable = windowLayout;

            _documentDock = documentDock;
            _rootDock = rootDock;
            _findTool = findViewModel;
            _replaceTool = replaceViewModel;

            return rootDock;
        }

        public override void InitLayout(IDockable layout)
        {
            ContextLocator = new Dictionary<string, Func<object>>
            {
                ["Find"] = () => layout,
                ["Replace"] = () => layout
            };

            DockableLocator = new Dictionary<string, Func<IDockable?>>()
            {
                ["Root"] = () => _rootDock,
                ["Files"] = () => _documentDock,
                ["Find"] = () => _findTool,
                ["Replace"] = () => _replaceTool
            };

            HostWindowLocator = new Dictionary<string, Func<IHostWindow>>
            {
                [nameof(IDockWindow)] = () => new HostWindow()
            };

            base.InitLayout(layout);
        }
    }
}
