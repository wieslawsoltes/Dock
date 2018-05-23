// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using Dock.Avalonia.Controls;
using Dock.Model;

namespace AvaloniaDemo.ViewModels
{
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
            _factory = new MainWindowDockFactory();

            _layout = layout ?? _factory.CreateDefaultLayout();

            _factory.ContextLocator = new Dictionary<string, Func<object>>
            {
                [nameof(DockLayout)] = () => this,
                [nameof(DockStrip)] = () => this,
                [nameof(DockWindow)] = () => this,
                ["Dock"] = () => this,
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
                ["MainLayout"] = () => this,
                ["Main"] = () => this,
                ["Root"] = () => this
            };

            _factory.HostLocator = new Dictionary<string, Func<IDockHost>>
            {
                [nameof(DockWindow)] = () => new HostWindow(),
                ["Dock"] = () => new HostWindow()
            };

            _factory.Update(_layout, this);

            _layout.CurrentView = _layout.DefaultView;
            _layout.CurrentView.ShowWindows();
        }
    }
}
