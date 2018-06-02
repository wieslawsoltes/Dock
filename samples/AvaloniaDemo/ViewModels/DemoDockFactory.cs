// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AvaloniaDemo.ViewModels.Documents;
using AvaloniaDemo.ViewModels.Tools;
using AvaloniaDemo.ViewModels.Views;
using Dock.Avalonia.Controls;
using Dock.Model;
using Dock.Model.Controls;

namespace AvaloniaDemo.ViewModels
{
    /// <inheritdoc/>
    public class DemoDockFactory : DockFactory
    {
        /// <inheritdoc/>
        public override IDock CreateLayout()
        {
            // Documents

            var document1 = new Document1
            {
                Id = "Document1",
                Width = double.NaN,
                Height = double.NaN,
                Title = "Document1"
            };

            var document2 = new Document2
            {
                Id = "Document2",
                Width = double.NaN,
                Height = double.NaN,
                Title = "Document2"
            };

            var document3 = new Document3
            {
                Id = "Document3",
                Width = double.NaN,
                Height = double.NaN,
                Title = "Document3"
            };

            // Left / Top

            var leftTopTool1 = new LeftTopTool1
            {
                Id = "LeftTop1",
                Width = double.NaN,
                Height = double.NaN,
                Title = "LeftTop1"
            };

            var leftTopTool2 = new LeftTopTool2
            {
                Id = "LeftTop2",
                Width = 200,
                Height = 200,
                Title = "LeftTop2"
            };

            var leftTopTool3 = new LeftTopTool3
            {
                Id = "LeftTop3",
                Width = double.NaN,
                Height = double.NaN,
                Title = "LeftTop3"
            };

            // Left / Bottom

            var leftBottomTool1 = new LeftBottomTool1
            {
                Id = "LeftBottom1",
                Width = double.NaN,
                Height = double.NaN,
                Title = "LeftBottom1"
            };

            var leftBottomTool2 = new LeftBottomTool2
            {
                Id = "LeftBottom2",
                Width = double.NaN,
                Height = double.NaN,
                Title = "LeftBottom2"
            };

            var leftBottomTool3 = new LeftBottomTool3
            {
                Id = "LeftBottom3",
                Width = double.NaN,
                Height = double.NaN,
                Title = "LeftBottom3"
            };

            // Right / Top

            var rightTopTool1 = new RightTopTool1
            {
                Id = "RightTop1",
                Width = double.NaN,
                Height = double.NaN,
                Title = "RightTop1"
            };

            var rightTopTool2 = new RightTopTool2
            {
                Id = "RightTop2",
                Width = double.NaN,
                Height = double.NaN,
                Title = "RightTop2"
            };

            var rightTopTool3 = new RightTopTool3
            {
                Id = "RightTop3",
                Width = double.NaN,
                Height = double.NaN,
                Title = "RightTop3"
            };

            // Right / Bottom

            var rightBottomTool1 = new RightBottomTool1
            {
                Id = "RightBottom1",
                Width = double.NaN,
                Height = double.NaN,
                Title = "RightBottom1"
            };

            var rightBottomTool2 = new RightBottomTool2
            {
                Id = "RightBottom2",
                Width = double.NaN,
                Height = double.NaN,
                Title = "RightBottom2"
            };

            var rightBottomTool3 = new RightBottomTool3
            {
                Id = "RightBottom3",
                Width = double.NaN,
                Height = double.NaN,
                Title = "RightBottom3"
            };

            // Left Pane

            var leftPane = new LayoutDock
            {
                Id = "LeftPane",
                Dock = "Left",
                Width = 200,
                Height = double.NaN,
                Title = "LeftPane",
                CurrentView = null,
                Views = new ObservableCollection<IView>
                {
                    new ToolDock
                    {
                        Id = "LeftPaneTop",
                        Dock = "Top",
                        Width = double.NaN,
                        Height = 340,
                        Title = "LeftPaneTop",
                        CurrentView = leftTopTool1,
                        Views = new ObservableCollection<IView>
                        {
                            leftTopTool1,
                            leftTopTool2,
                            leftTopTool3
                        }
                    },
                    new SplitterDock()
                    {
                        Id = "LeftPaneTopSplitter",
                        Dock = "Top",
                        Title = "LeftPaneTopSplitter"
                    },
                    new ToolDock
                    {
                        Id = "LeftPaneBottom",
                        Dock = "Bottom",
                        Width = double.NaN,
                        Height = double.NaN,
                        Title = "LeftPaneBottom",
                        CurrentView = leftBottomTool1,
                        Views = new ObservableCollection<IView>
                        {
                            leftBottomTool1,
                            leftBottomTool2,
                            leftBottomTool3
                        }
                    }
                }
            };

            // Right Pane

            var rightPane = new LayoutDock
            {
                Id = "RightPane",
                Dock = "Right",
                Width = 240,
                Height = double.NaN,
                Title = "RightPane",
                CurrentView = null,
                Views = new ObservableCollection<IView>
                {
                    new ToolDock
                    {
                        Id = "RightPaneTop",
                        Dock = "Top",
                        Width = double.NaN,
                        Height = 340,
                        Title = "RightPaneTop",
                        CurrentView = rightTopTool1,
                        Views = new ObservableCollection<IView>
                        {
                            rightTopTool1,
                            rightTopTool2,
                            rightTopTool3
                        }
                    },
                    new SplitterDock()
                    {
                        Id = "RightPaneTopSplitter",
                        Dock = "Top",
                        Title = "RightPaneTopSplitter"
                    },
                    new ToolDock
                    {
                        Id = "RightPaneBottom",
                        Dock = "Bottom",
                        Width = double.NaN,
                        Height = double.NaN,
                        Title = "RightPaneBottom",
                        CurrentView = rightBottomTool1,
                        Views = new ObservableCollection<IView>
                        {
                            rightBottomTool1,
                            rightBottomTool2,
                            rightBottomTool3
                        }
                    }
                }
            };

            // Documents

            var documentsPane = new DocumentDock
            {
                Id = "DocumentsPane",
                Dock = "",
                Width = double.NaN,
                Height = double.NaN,
                Title = "DocumentsPane",
                CurrentView = document1,
                Views = new ObservableCollection<IView>
                {
                    document1,
                    document2,
                    document3
                }
            };

            // Main

            var mainLayout = new LayoutDock
            {
                Id = "MainLayout",
                Dock = "",
                Width = double.NaN,
                Height = double.NaN,
                Title = "MainLayout",
                CurrentView = null,
                Views = new ObservableCollection<IView>
                {
                    leftPane,
                    new SplitterDock()
                    {
                        Id = "LeftSplitter",
                        Dock = "Left",
                        Title = "LeftSplitter"
                    },
                    rightPane,
                    new SplitterDock()
                    {
                        Id = "RightSplitter",
                        Dock = "Right",
                        Title = "RightSplitter"
                    },
                    documentsPane
                }
            };

            var mainView = new MainView
            {
                Id = "Main",
                Width = double.NaN,
                Height = double.NaN,
                Title = "Main",
                CurrentView = mainLayout,
                Views = new ObservableCollection<IView>
                {
                   mainLayout
                }
            };

            // Home

            var homeView = new HomeView
            {
                Id = "Home",
                Width = double.NaN,
                Height = double.NaN,
                Title = "Home"
            };

            // Root

            var root = new RootDock
            {
                Id = "Root",
                Width = double.NaN,
                Height = double.NaN,
                Title = "Root",
                CurrentView = homeView,
                DefaultView = homeView,
                Views = new ObservableCollection<IView>
                {
                    homeView,
                    mainView,
                }
            };

            return root;
        }

        /// <inheritdoc/>
        public override void InitLayout(IView layout, object context)
        {
            this.ContextLocator = new Dictionary<string, Func<object>>
            {
                // Defaults
                [nameof(IRootDock)] = () => context,
                [nameof(ILayoutDock)] = () => context,
                [nameof(IDocumentDock)] = () => context,
                [nameof(IToolDock)] = () => context,
                [nameof(ISplitterDock)] = () => context,
                [nameof(IDockWindow)] = () => context,
                [nameof(IDocumentTab)] = () => context,
                [nameof(IToolTab)] = () => context,
                // Documents
                ["Document1"] = () => context,
                ["Document2"] = () => context,
                ["Document3"] = () => context,
                // Tools
                ["LeftTop1"] = () => context,
                ["LeftTop2"] = () => context,
                ["LeftTop3"] = () => context,
                ["LeftBottom1"] = () => context,
                ["LeftBottom2"] = () => context,
                ["LeftBottom3"] = () => context,
                ["RightTop1"] = () => context,
                ["RightTop2"] = () => context,
                ["RightTop3"] = () => context,
                ["RightBottom1"] = () => context,
                ["RightBottom2"] = () => context,
                ["RightBottom3"] = () => context,
                ["LeftPane"] = () => context,
                ["LeftPaneTop"] = () => context,
                ["LeftPaneTopSplitter"] = () => context,
                ["LeftPaneBottom"] = () => context,
                ["RightPane"] = () => context,
                ["RightPaneTop"] = () => context,
                ["RightPaneTopSplitter"] = () => context,
                ["RightPaneBottom"] = () => context,
                ["DocumentsPane"] = () => context,
                ["MainLayout"] = () => context,
                ["LeftSplitter"] = () => context,
                ["RightSplitter"] = () => context,
                // Layouts
                ["MainLayout"] = () => context,
                // Views
                ["Home"] = () => context,
                ["Main"] = () => context
            };

            this.HostLocator = new Dictionary<string, Func<IDockHost>>
            {
                [nameof(IDockWindow)] = () => new HostWindow()
            };

            this.Update(layout, context, null);

            if (layout is IWindowsHost layoutWindowsHost)
            {
                layoutWindowsHost.ShowWindows();
                if (layout is IViewsHost layoutViewsHost)
                {
                    layoutViewsHost.CurrentView = layoutViewsHost.DefaultView;
                    if (layoutViewsHost.CurrentView is IWindowsHost currentViewWindowsHost)
                    {
                        currentViewWindowsHost.ShowWindows();
                    }
                }
            }
        }
    }
}
