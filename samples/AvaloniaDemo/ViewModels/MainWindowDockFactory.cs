// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AvaloniaDemo.ViewModels.Views;
using Dock.Avalonia.Controls;
using Dock.Model;
using Dock.Model.Factories;

namespace AvaloniaDemo.ViewModels
{
    /// <inheritdoc/>
    public class MainWindowDockFactory : BaseDockFactory
    {
        /// <inheritdoc/>
        public override IDock CreateLayout()
        {
            // Home

            var homeView = new HomeView
            {
                Id = "Home",
                Dock = "",
                Width = double.NaN,
                Height = double.NaN,
                Title = "Home"
            };

            // Center

            var centerView = new CenterView
            {
                Id = "Center",
                Dock = "",
                Width = double.NaN,
                Height = double.NaN,
                Title = "Center"
            };

            // Left / Top

            var leftTopView1 = new LeftTopView1
            {
                Id = "LeftTop1",
                Dock = "LeftTop1",
                Width = double.NaN,
                Height = double.NaN,
                Title = "LeftTop1"
            };

            var leftTopView2 = new LeftTopView2
            {
                Id = "LeftTop2",
                Dock = "",
                Width = 200,
                Height = 200,
                Title = "LeftTop2"
            };

            var leftTopView3 = new LeftTopView3
            {
                Id = "LeftTop3",
                Dock = "",
                Width = double.NaN,
                Height = double.NaN,
                Title = "LeftTop3"
            };

            // Left / Bottom

            var leftBottomView1 = new LeftBottomView1
            {
                Id = "LeftBottom1",
                Dock = "",
                Width = double.NaN,
                Height = double.NaN,
                Title = "LeftBottom1"
            };

            var leftBottomView2 = new LeftBottomView2
            {
                Id = "LeftBottom2",
                Dock = "",
                Width = double.NaN,
                Height = double.NaN,
                Title = "LeftBottom2"
            };

            var leftBottomView3 = new LeftBottomView3
            {
                Id = "LeftBottom3",
                Dock = "",
                Width = double.NaN,
                Height = double.NaN,
                Title = "LeftBottom3"
            };

            // Right / Top

            var rightTopView1 = new RightTopView1
            {
                Id = "RightTop1",
                Dock = "",
                Width = double.NaN,
                Height = double.NaN,
                Title = "RightTop1"
            };

            var rightTopView2 = new RightTopView2
            {
                Id = "RightTop2",
                Dock = "",
                Width = double.NaN,
                Height = double.NaN,
                Title = "RightTop2"
            };

            var rightTopView3 = new RightTopView3
            {
                Id = "RightTop3",
                Dock = "",
                Width = double.NaN,
                Height = double.NaN,
                Title = "RightTop3"
            };

            // Right / Bottom

            var rightBottomView1 = new RightBottomView1
            {
                Id = "RightBottom1",
                Dock = "",
                Width = double.NaN,
                Height = double.NaN,
                Title = "RightBottom1"
            };

            var rightBottomView2 = new RightBottomView2
            {
                Id = "RightBottom2",
                Dock = "",
                Width = double.NaN,
                Height = double.NaN,
                Title = "RightBottom2"
            };

            var rightBottomView3 = new RightBottomView3
            {
                Id = "RightBottom3",
                Dock = "",
                Width = double.NaN,
                Height = double.NaN,
                Title = "RightBottom3"
            };

            // Left Pane

            var leftPane = new DockLayout
            {
                Id = "LeftPane",
                Dock = "Left",
                Width = 200,
                Height = double.NaN,
                Title = "LeftPane",
                CurrentView = null,
                Views = new ObservableCollection<IDock>
                {
                    new DockStrip
                    {
                        Id = "LeftPaneTop",
                        Dock = "Top",
                        Width = double.NaN,
                        Height = 340,
                        Title = "LeftPaneTop",
                        CurrentView = leftTopView1,
                        Views = new ObservableCollection<IDock>
                        {
                            leftTopView1,
                            leftTopView2,
                            leftTopView3
                        }
                    },
                    new DockSplitter()
                    {
                        Id = "LeftPaneTopSplitter",
                        Dock = "Top",
                        Title = "LeftPaneTopSplitter"
                    },
                    new DockStrip
                    {
                        Id = "LeftPaneBottom",
                        Dock = "Bottom",
                        Width = double.NaN,
                        Height = double.NaN,
                        Title = "LeftPaneBottom",
                        CurrentView = leftBottomView1,
                        Views = new ObservableCollection<IDock>
                        {
                            leftBottomView1,
                            leftBottomView2,
                            leftBottomView3
                        }
                    }
                }
            };

            // Right Pane

            var rightPane = new DockLayout
            {
                Id = "RightPane",
                Dock = "Right",
                Width = 240,
                Height = double.NaN,
                Title = "RightPane",
                CurrentView = null,
                Views = new ObservableCollection<IDock>
                {
                    new DockStrip
                    {
                        Id = "RightPaneTop",
                        Dock = "Top",
                        Width = double.NaN,
                        Height = 340,
                        Title = "RightPaneTop",
                        CurrentView = rightTopView1,
                        Views = new ObservableCollection<IDock>
                        {
                            rightTopView1,
                            rightTopView2,
                            rightTopView3
                        }
                    },
                    new DockSplitter()
                    {
                        Id = "RightPaneTopSplitter",
                        Dock = "Top",
                        Title = "RightPaneTopSplitter"
                    },
                    new DockStrip
                    {
                        Id = "RightPaneBottom",
                        Dock = "Bottom",
                        Width = double.NaN,
                        Height = double.NaN,
                        Title = "RightPaneBottom",
                        CurrentView = rightBottomView1,
                        Views = new ObservableCollection<IDock>
                        {
                            rightBottomView1,
                            rightBottomView2,
                            rightBottomView3
                        }
                    }
                }
            };

            // Main

            var mainLayout = new DockLayout
            {
                Id = "MainLayout",
                Dock = "",
                Width = double.NaN,
                Height = double.NaN,
                Title = "MainLayout",
                CurrentView = null,
                Views = new ObservableCollection<IDock>
                {
                    leftPane,
                    new DockSplitter()
                    {
                        Id = "LeftSplitter",
                        Dock = "Left",
                        Title = "LeftSplitter"
                    },
                    rightPane,
                    new DockSplitter()
                    {
                        Id = "RightSplitter",
                        Dock = "Right",
                        Title = "RightSplitter"
                    },
                    centerView
                }
            };

            var mainView = new MainView
            {
                Id = "Main",
                Dock = "",
                Width = double.NaN,
                Height = double.NaN,
                Title = "Main",
                CurrentView = mainLayout,
                Views = new ObservableCollection<IDock>
                {
                   mainLayout
                }
            };

            // Root

            var layout = new DockRoot
            {
                Id = "Root",
                Dock = "",
                Width = double.NaN,
                Height = double.NaN,
                Title = "Root",
                CurrentView = homeView,
                DefaultView = homeView,
                Views = new ObservableCollection<IDock>
                {
                    homeView,
                    mainView,
                }
            };

            return layout;
        }

        /// <inheritdoc/>
        public override void InitLayout(IDock layout, object context)
        {
            this.ContextLocator = new Dictionary<string, Func<object>>
            {
                [nameof(DockLayout)] = () => context,
                [nameof(DockStrip)] = () => context,
                [nameof(DockWindow)] = () => context,
                ["Dock"] = () => context,
                ["Home"] = () => layout,
                ["Center"] = () => context,
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
                ["MainLayout"] = () => context,
                ["LeftSplitter"] = () => context,
                ["RightSplitter"] = () => context,
                ["MainLayout"] = () => context,
                ["Main"] = () => context,
                ["Root"] = () => context
            };

            this.HostLocator = new Dictionary<string, Func<IDockHost>>
            {
                [nameof(DockWindow)] = () => new HostWindow(),
                ["Dock"] = () => new HostWindow()
            };

            this.Update(layout, context);

            layout.CurrentView = layout.DefaultView;
            layout.CurrentView.ShowWindows();
        }
    }
}
