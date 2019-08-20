using System;
using System.Collections.Generic;
using Avalonia.Data;
using AvaloniaDemo.Models.Documents;
using AvaloniaDemo.Models.Tools;
using Dock.Avalonia.Controls;
using Dock.Model;
using Dock.Model.Controls;

namespace AvaloniaDemo
{
    public class DemoFactory : Factory
    {
        public override IDock CreateLayout()
        {
            return new RootDock();
        }

        public override void InitLayout(IDockable layout)
        {
            this.ContextLocator = new Dictionary<string, Func<object>>
            {
                ["Document1"] = () => new Document1(),
                ["Document2"] = () => new Document2(),
                ["LeftTop1"] = () => new LeftTopTool1(),
                ["LeftTop2"] = () => new LeftTopTool2(),
                ["LeftBottom1"] = () => new LeftBottomTool1(),
                ["LeftBottom2"] = () => new LeftBottomTool2(),
                ["RightTop1"] = () => new RightTopTool1(),
                ["RightTop2"] = () => new RightTopTool2(),
                ["RightBottom1"] = () => new RightBottomTool1(),
                ["RightBottom2"] = () => new RightBottomTool2()
            };

            this.HostLocator = new Dictionary<string, Func<IHostWindow>>
            {
                [nameof(IDockWindow)] = () =>
                {
                    var hostWindow = new HostWindow()
                    {
                        [!HostWindow.TitleProperty] = new Binding("ActiveDockable.Title")
                    };

                    hostWindow.Content = new DockControl()
                    {
                        [!DockControl.LayoutProperty] = hostWindow[!HostWindow.DataContextProperty]
                    };

                    return hostWindow;
                }
            };

            base.InitLayout(layout);
        }
    }
}
