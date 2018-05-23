// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dock.Avalonia.Controls;
using Dock.Model;
using Dock.Model.Factories;

namespace AvaloniaDemo
{
    public class InitView : DockView
    {
    }

    public class CenterView : DockView
    {
    }

    public class LeftTopView1 : DockView
    {
    }

    public class LeftTopView2 : DockView
    {
    }

    public class LeftTopView3 : DockView
    {
    }

    public class LeftBottomView1 : DockView
    {
    }

    public class LeftBottomView2 : DockView
    {
    }

    public class LeftBottomView3 : DockView
    {
    }

    public class RightTopView1 : DockView
    {
    }

    public class RightTopView2 : DockView
    {
    }

    public class RightTopView3 : DockView
    {
    }

    public class RightBottomView1 : DockView
    {
    }

    public class RightBottomView2 : DockView
    {
    }

    public class RightBottomView3 : DockView
    {
    }

    public class MainView : DockView
    {
    }

    public class DemoDockFactory : BaseDockFactory
    {
        public override IDock CreateDefaultLayout()
        {
            // Home

            var homeView = new InitView
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

            // Main

            var layout = new DockRoot
            {
                Id = "Root",
                Dock = "",
                Width = double.NaN,
                Height = double.NaN,
                Title = "Root",
                CurrentView = homeView,
                Views = new ObservableCollection<IDock>
                {
                    homeView,
                    mainView,
                }
            };

            return layout;
        }
    }

    public class MainWindowViewModel : ObservableObject
    {
        private IDockFactory _factory;
        private IDock _layout;

        public IDockFactory Factory
        {
            get => _factory;
            set => Update(ref _factory, value);
        }

        public IDock Layout
        {
            get => _layout;
            set => Update(ref _layout, value);
        }

        public void InitLayout(IDock layout)
        {
            _factory = new DemoDockFactory();

            _layout = layout ?? _factory.CreateDefaultLayout();

            var homeView = _layout.Views.FirstOrDefault(v => v.Title == "Home");
            if (homeView != null)
            {
                _layout.CurrentView = homeView;
            }

            _factory.ContextLocator = new Dictionary<string, Func<object>>
            {
                ["DockLayout"] = () => this,
                ["DockWindow"] = () => this,
                ["Home"] = () => _layout,
                ["Center"] = () => this,
                ["LeftTop1"] = () => this,
                ["LeftTop2"] = () => this,
                ["LeftTop3"] = () => this,
                ["LeftBottom1"] = () => this,
                ["LeftBottom2"] = () => this,
                ["LeftBottom3"] = () => this,
                ["RightTop1"] = () => this,
                ["RightTop2"] = () => this,
                ["RightTop3"] = () => this,
                ["RightBottom1"] = () => this,
                ["RightBottom2"] = () => this,
                ["RightBottom3"] = () => this,
                ["LeftPane"] = () => this,
                ["LeftPaneTop"] = () => this,
                ["LeftPaneTopSplitter"] = () => this,
                ["LeftPaneBottom"] = () => this,
                ["RightPane"] = () => this,
                ["RightPaneTop"] = () => this,
                ["RightPaneTopSplitter"] = () => this,
                ["RightPaneBottom"] = () => this,
                ["MainLayout"] = () => this,
                ["LeftSplitter"] = () => this,
                ["RightSplitter"] = () => this,
                ["Main"] = () => this,
                ["Root"] = () => this
            };

            _factory.HostLocator = () => new HostWindow();

            _factory.Update(_layout, this);

            _layout.Navigate(_layout.CurrentView);
        }
    }
}
