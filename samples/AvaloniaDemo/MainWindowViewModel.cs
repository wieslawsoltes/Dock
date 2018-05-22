// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dock.Avalonia.Dock;
using Dock.Avalonia.Factories;
using Dock.Model;

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
            // Init

            var initView = new InitView
            {
                Dock = "",
                Width = double.NaN,
                Height = double.NaN,
                Title = "Init"
            };

            // Center

            var centerView = new CenterView
            {
                Dock = "",
                Width = double.NaN,
                Height = double.NaN,
                Title = "Center"
            };

            // Left / Top

            var leftTopView1 = new LeftTopView1
            {
                Dock = "",
                Width = double.NaN,
                Height = double.NaN,
                Title = "LeftTop1"
            };

            var leftTopView2 = new LeftTopView2
            {
                Dock = "",
                Width = 200,
                Height = 200,
                Title = "LeftTop2"
            };

            var leftTopView3 = new LeftTopView3
            {
                Dock = "",
                Width = double.NaN,
                Height = double.NaN,
                Title = "LeftTop3"
            };

            // Left / Bottom

            var leftBottomView1 = new LeftBottomView1
            {
                Dock = "",
                Width = double.NaN,
                Height = double.NaN,
                Title = "LeftBottom1"
            };

            var leftBottomView2 = new LeftBottomView2
            {
                Dock = "",
                Width = double.NaN,
                Height = double.NaN,
                Title = "LeftBottom2"
            };

            var leftBottomView3 = new LeftBottomView3
            {
                Dock = "",
                Width = double.NaN,
                Height = double.NaN,
                Title = "LeftBottom3"
            };

            // Right / Top

            var rightTopView1 = new RightTopView1
            {
                Dock = "",
                Width = double.NaN,
                Height = double.NaN,
                Title = "RightTop1"
            };

            var rightTopView2 = new RightTopView2
            {
                Dock = "",
                Width = double.NaN,
                Height = double.NaN,
                Title = "RightTop2"
            };

            var rightTopView3 = new RightTopView3
            {
                Dock = "",
                Width = double.NaN,
                Height = double.NaN,
                Title = "RightTop3"
            };

            // Right / Bottom

            var rightBottomView1 = new RightBottomView1
            {
                Dock = "",
                Width = double.NaN,
                Height = double.NaN,
                Title = "RightBottom1"
            };

            var rightBottomView2 = new RightBottomView2
            {
                Dock = "",
                Width = double.NaN,
                Height = double.NaN,
                Title = "RightBottom2"
            };

            var rightBottomView3 = new RightBottomView3
            {
                Dock = "",
                Width = double.NaN,
                Height = double.NaN,
                Title = "RightBottom3"
            };

            // Left Pane

            var leftPane = new DockLayout
            {
                Dock = "Left",
                Width = 200,
                Height = double.NaN,
                Title = "LeftPane",
                CurrentView = null,
                Views = new ObservableCollection<IDock>
                {
                    new DockStrip
                    {
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
                    new DockSplitter() { Dock = "Top", Title = "LeftPaneTopSplitter" },
                    new DockStrip
                    {
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
                Dock = "Right",
                Width = 240,
                Height = double.NaN,
                Title = "RightPane",
                CurrentView = null,
                Views = new ObservableCollection<IDock>
                {
                    new DockStrip
                    {
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
                    new DockSplitter() { Dock = "Top", Title = "RightPaneTopSplitter" },
                    new DockStrip
                    {
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
                Dock = "",
                Width = double.NaN,
                Height = double.NaN,
                Title = "MainLayout",
                CurrentView = null,
                Views = new ObservableCollection<IDock>
                {
                    leftPane,
                    new DockSplitter() { Dock = "Left", Title = "LeftSplitter" },
                    rightPane,
                    new DockSplitter() { Dock = "Right", Title = "RightSplitter" },
                    centerView
                }
            };

            var mainView = new MainView
            {
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
                Dock = "",
                Width = double.NaN,
                Height = double.NaN,
                CurrentView = initView,
                Views = new ObservableCollection<IDock>
                {
                    initView,
                    mainView,
                }
            };

            return layout;
        }
    }

    public class MainWindowViewModel : ObservableObject
    {
        private IDock _layout;

        public IDock Layout
        {
            get => _layout;
            set => Update(ref _layout, value);
        }

        public void InitLayout(IDockFactory factory, IDock layout)
        {
            factory.ContextLocator = new Dictionary<Type, Func<object>>
            {
                [typeof(DockLayout)] = () => this,
                [typeof(DockRoot)] = () => this,
                [typeof(DockSplitter)] = () => this,
                [typeof(DockStrip)] = () => this,
                [typeof(DockView)] = () => this,
                [typeof(DockWindow)] = () => this,
                [typeof(InitView)] = () => layout,
                [typeof(CenterView)] = () => this,
                [typeof(LeftTopView1)] = () => this,
                [typeof(LeftTopView2)] = () => this,
                [typeof(LeftTopView3)] = () => this,
                [typeof(LeftBottomView1)] = () => this,
                [typeof(LeftBottomView2)] = () => this,
                [typeof(LeftBottomView3)] = () => this,
                [typeof(RightTopView1)] = () => this,
                [typeof(RightTopView2)] = () => this,
                [typeof(RightTopView3)] = () => this,
                [typeof(RightBottomView1)] = () => this,
                [typeof(RightBottomView2)] = () => this,
                [typeof(RightBottomView3)] = () => this,
                [typeof(MainView)] = () => this
            };

            factory.HostLocator = () => new HostWindow();

            factory.Update(layout, this);
        }

        public MainWindowViewModel(IDock layout)
        {
            var factory = new DemoDockFactory();

            _layout = layout == null ? factory.CreateDefaultLayout() : layout;

            var init = _layout.Views.FirstOrDefault(v => v.Title == "Init");
            if (init != null)
            {
                _layout.CurrentView = init;
            }

            InitLayout(factory, _layout);
        }
    }
}
