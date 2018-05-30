// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AvaloniaDemo.ViewModels.Tools;
using Dock.Avalonia.Controls;
using Dock.Model;
using Dock.Model.Controls;
using Dock.Model.Factories;

namespace AvaloniaDemo.ViewModels
{
    /// <inheritdoc/>
    public class EditorDockFactory : BaseDockFactory
    {
        /// <inheritdoc/>
        public override IDock CreateLayout()
        {
            var editorView = new EditorTool
            {
                Id = "Editor",
                Width = double.NaN,
                Height = double.NaN,
                Title = "Editor"
            };

            var layout = new ToolDock
            {
                Id = nameof(ToolDock),
                Dock = "",
                Width = double.NaN,
                Height = double.NaN,
                Title = nameof(RootDock),
                CurrentView = editorView,
                DefaultView = editorView,
                Views = new ObservableCollection<IView>
                {
                    editorView
                }
            };

            return layout;
        }

        /// <inheritdoc/>
        public override void InitLayout(IView layout, object context)
        {
            this.ContextLocator = new Dictionary<string, Func<object>>
            {
                [nameof(RootDock)] = () => context,
                [nameof(LayoutDock)] = () => context,
                [nameof(DocumentDock)] = () => context,
                [nameof(ToolDock)] = () => context,
                [nameof(SplitterDock)] = () => context,
                [nameof(DockWindow)] = () => context,
                ["Editor"] = () => context
            };

            this.HostLocator = new Dictionary<string, Func<IDockHost>>
            {
                [nameof(DockWindow)] = () => new HostWindow()
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
