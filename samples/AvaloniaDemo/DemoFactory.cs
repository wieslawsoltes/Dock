﻿using System;
using System.Collections.Generic;
using Avalonia.Data;
using AvaloniaDemo.Models.Documents;
using AvaloniaDemo.Models.Tools;
using AvaloniaDemo.ViewModels.Documents;
using AvaloniaDemo.ViewModels.Tools;
using AvaloniaDemo.ViewModels.Views;
using Dock.Avalonia.Controls;
using Dock.Model;
using Dock.Model.Controls;

namespace AvaloniaDemo
{
    public class DemoFactory : Factory
    {
        private object _context;

        public DemoFactory(object context)
        {
            _context = context;
        }

        public override IDock CreateLayout()
        {
            var document1 = new Document1ViewModel
            {
                Id = "Document1",
                Title = "Document1"
            };

            var document2 = new Document2ViewModel
            {
                Id = "Document2",
                Title = "Document2"
            };

            var leftTopTool1 = new LeftTopTool1ViewModel
            {
                Id = "LeftTop1",
                Title = "LeftTop1"
            };

            var leftTopTool2 = new LeftTopTool2ViewModel
            {
                Id = "LeftTop2",
                Title = "LeftTop2"
            };

            var leftBottomTool1 = new LeftBottomTool1ViewModel
            {
                Id = "LeftBottom1",
                Title = "LeftBottom1"
            };

            var leftBottomTool2 = new LeftBottomTool2ViewModel
            {
                Id = "LeftBottom2",
                Title = "LeftBottom2"
            };

            var rightTopTool1 = new RightTopTool1ViewModel
            {
                Id = "RightTop1",
                Title = "RightTop1"
            };

            var rightTopTool2 = new RightTopTool2ViewModel
            {
                Id = "RightTop2",
                Title = "RightTop2"
            };

            var rightBottomTool1 = new RightBottomTool1ViewModel
            {
                Id = "RightBottom1",
                Title = "RightBottom1"
            };

            var rightBottomTool2 = new RightBottomTool2ViewModel
            {
                Id = "RightBottom2",
                Title = "RightBottom2"
            };

            var mainLayout = new ProportionalDock
            {
                Id = "MainLayout",
                Title = "MainLayout",
                Proportion = double.NaN,
                Orientation = Orientation.Horizontal,
                ActiveDockable = null,
                VisibleDockables = CreateList<IDockable>
                (
                    new ProportionalDock
                    {
                        Id = "LeftPane",
                        Title = "LeftPane",
                        Proportion = double.NaN,
                        Orientation = Orientation.Vertical,
                        ActiveDockable = null,
                        VisibleDockables = CreateList<IDockable>
                        (
                            new ToolDock
                            {
                                Id = "LeftPaneTop",
                                Title = "LeftPaneTop",
                                Proportion = double.NaN,
                                ActiveDockable = leftTopTool1,
                                VisibleDockables = CreateList<IDockable>
                                (
                                    leftTopTool1,
                                    leftTopTool2
                                )
                            },
                            new SplitterDock()
                            {
                                Id = "LeftPaneTopSplitter",
                                Title = "LeftPaneTopSplitter"
                            },
                            new ToolDock
                            {
                                Id = "LeftPaneBottom",
                                Title = "LeftPaneBottom",
                                Proportion = double.NaN,
                                ActiveDockable = leftBottomTool1,
                                VisibleDockables = CreateList<IDockable>
                                (
                                    leftBottomTool1,
                                    leftBottomTool2
                                )
                            }
                        )
                    },
                    new SplitterDock()
                    {
                        Id = "LeftSplitter",
                        Title = "LeftSplitter"
                    },
                    new DocumentDock
                    {
                        Id = "DocumentsPane",
                        Title = "DocumentsPane",
                        IsCollapsable = false,
                        Proportion = double.NaN,
                        ActiveDockable = document1,
                        VisibleDockables = CreateList<IDockable>
                        (
                            document1,
                            document2
                        )
                    },
                    new SplitterDock()
                    {
                        Id = "RightSplitter",
                        Title = "RightSplitter"
                    },
                    new ProportionalDock
                    {
                        Id = "RightPane",
                        Title = "RightPane",
                        Proportion = double.NaN,
                        Orientation = Orientation.Vertical,
                        ActiveDockable = null,
                        VisibleDockables = CreateList<IDockable>
                        (
                            new ToolDock
                            {
                                Id = "RightPaneTop",
                                Title = "RightPaneTop",
                                Proportion = double.NaN,
                                ActiveDockable = rightTopTool1,
                                VisibleDockables = CreateList<IDockable>
                                (
                                    rightTopTool1,
                                    rightTopTool2
                                )
                            },
                            new SplitterDock()
                            {
                                Id = "RightPaneTopSplitter",
                                Title = "RightPaneTopSplitter"
                            },
                            new ToolDock
                            {
                                Id = "RightPaneBottom",
                                Title = "RightPaneBottom",
                                Proportion = double.NaN,
                                ActiveDockable = rightBottomTool1,
                                VisibleDockables = CreateList<IDockable>
                                (
                                    rightBottomTool1,
                                    rightBottomTool2
                                )
                            }
                        )
                    }
                )
            };

            var dashboardView = new DashboardViewModel
            {
                Id = "Dashboard",
                Title = "Dashboard"
            };

            var homeView = new HomeViewModel
            {
                Id = "Home",
                Title = "Home",
                ActiveDockable = mainLayout,
                VisibleDockables = CreateList<IDockable>(mainLayout),
                Left = CreatePinDock(),
                Right = CreatePinDock(),
                Top = CreatePinDock(),
                Bottom = CreatePinDock()
            };

            var root = CreateRootDock();

            root.Id = "Root";
            root.Title = "Root";
            root.IsCollapsable = false;
            root.ActiveDockable = dashboardView;
            root.DefaultDockable = dashboardView;
            root.VisibleDockables = CreateList<IDockable>(dashboardView, homeView);

            return root;
        }

        public override void InitLayout(IDockable layout)
        {
            this.ContextLocator = new Dictionary<string, Func<object>>
            {
                [nameof(IRootDock)] = () => _context,
                [nameof(IPinDock)] = () => _context,
                [nameof(IProportionalDock)] = () => _context,
                [nameof(IDocumentDock)] = () => _context,
                [nameof(IToolDock)] = () => _context,
                [nameof(ISplitterDock)] = () => _context,
                [nameof(IDockWindow)] = () => _context,
                [nameof(IDocument)] = () => _context,
                [nameof(ITool)] = () => _context,
                ["Document1"] = () => new Document1(),
                ["Document2"] = () => new Document2(),
                ["LeftTop1"] = () => new LeftTopTool1(),
                ["LeftTop2"] = () => new LeftTopTool2(),
                ["LeftBottom1"] = () => new LeftBottomTool1(),
                ["LeftBottom2"] = () => new LeftBottomTool2(),
                ["RightTop1"] = () => new RightTopTool1(),
                ["RightTop2"] = () => new RightTopTool2(),
                ["RightBottom1"] = () => new RightBottomTool1(),
                ["RightBottom2"] = () => new RightBottomTool2(),
                ["LeftPane"] = () => _context,
                ["LeftPaneTop"] = () => _context,
                ["LeftPaneTopSplitter"] = () => _context,
                ["LeftPaneBottom"] = () => _context,
                ["RightPane"] = () => _context,
                ["RightPaneTop"] = () => _context,
                ["RightPaneTopSplitter"] = () => _context,
                ["RightPaneBottom"] = () => _context,
                ["DocumentsPane"] = () => _context,
                ["MainLayout"] = () => _context,
                ["LeftSplitter"] = () => _context,
                ["RightSplitter"] = () => _context,
                ["MainLayout"] = () => _context,
                ["Dashboard"] = () => layout,
                ["Home"] = () => _context
            };

            this.HostWindowLocator = new Dictionary<string, Func<IHostWindow>>
            {
                [nameof(IDockWindow)] = () => new HostWindow()
            };

            this.DockableLocator = new Dictionary<string, Func<IDockable>>
            {
            };

            base.InitLayout(layout);
        }
    }
}
