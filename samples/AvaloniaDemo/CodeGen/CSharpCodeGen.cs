// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Collections.Generic;
using System.Text;
using Dock.Model;

namespace AvaloniaDemo.CodeGen
{
    /// <summary>
    /// CSharp code generator.
    /// </summary>
    public class CSharpCodeGen : ICodeGen
    {
        private StringBuilder _sb;
        private IDictionary<IView, string> _idViews;
        private IDictionary<IDockWindow, string> _idWindows;
        private int _viewCount;
        private int _windowCount;

        private void Write(string path, string text)
        {
            using (var fs = System.IO.File.Create(path))
            {
                using (var sw = new System.IO.StreamWriter(fs, Encoding.UTF8))
                {
                    sw.Write(text);
                }
            }
        }

        private void Output(string text)
        {
            if (_sb != null)
            {
                _sb.AppendLine(text);
            }
        }

        private string FormatDouble(double value)
        {
            if (double.IsNaN(value))
            {
                return "double.NaN";
            }
            return $"{value}";
        }

        private void WriteWindow(IDockWindow window, string indent = "")
        {
            string id = $"window{_windowCount}";
            _windowCount++;

            if (!_idWindows.ContainsKey(window))
            {
                _idWindows[window] = id;
            }

            Output($"{indent}var {id} = new {window.GetType().Name}()");
            Output($"{indent}{{");
            Output($"{indent}    Id = \"{window.Id}\",");
            Output($"{indent}    X = {FormatDouble(window.X)},");
            Output($"{indent}    Y = {FormatDouble(window.Y)},");
            Output($"{indent}    Width = {FormatDouble(window.Width)},");
            Output($"{indent}    Height = {FormatDouble(window.Height)},");
            Output($"{indent}    Title = \"{window.Title}\",");

            Output($"{indent}}};");

            WriteObjects(window.Layout, indent);
        }

        private void WriteObjects(IView root, string indent = "")
        {
            string id = $"view{_viewCount}";
            _viewCount++;

            if (!_idViews.ContainsKey(root))
            {
                _idViews[root] = id;
            }

            Output($"{indent}var {id} = new {root.GetType().Name}()");
            Output($"{indent}{{");

            Output($"{indent}    Id = \"{root.Id}\",");

            if (root is IDock viewDock)
            {
                Output($"{indent}    Dock = \"{viewDock.Dock}\",");
            }

            Output($"{indent}    Width = {FormatDouble(root.Width)},");
            Output($"{indent}    Height = {FormatDouble(root.Height)},");
            Output($"{indent}    Title = \"{root.Title}\",");

            Output($"{indent}}};");

            if (root is IViewsHost viewViewsHost)
            {
                if (viewViewsHost.Views != null && viewViewsHost.Views.Count > 0)
                {
                    for (int i = 0; i < viewViewsHost.Views.Count; i++)
                    {
                        var child = viewViewsHost.Views[i];
                        WriteObjects(child, indent);
                    }
                }
            }

            if (root is IWindowsHost viewWindowsHost)
            {
                if (viewWindowsHost.Windows != null && viewWindowsHost.Windows.Count > 0)
                {
                    for (int i = 0; i < viewWindowsHost.Windows.Count; i++)
                    {
                        var child = viewWindowsHost.Windows[i];
                        WriteWindow(child, indent);
                    }
                }
            }
        }

        private void WriteLists(IView root, string indent = "")
        {
            foreach (var kvp in _idViews)
            {
                IView view = kvp.Key;
                string id = kvp.Value;

                if (view is IViewsHost viewViewsHost)
                {
                    if (viewViewsHost.Views != null && viewViewsHost.Views.Count > 0)
                    {
                        if (viewViewsHost.CurrentView != null)
                        {
                            Output($"{indent}{id}.CurrentView = {_idViews[viewViewsHost.CurrentView]};");
                        }

                        if (viewViewsHost.DefaultView != null)
                        {
                            Output($"{indent}{id}.CurrentView = {_idViews[viewViewsHost.DefaultView]};");
                        }

                        Output($"{indent}{id}.Views = new ObservableCollection<IView>");
                        Output($"{indent}{{");

                        for (int i = 0; i < viewViewsHost.Views.Count; i++)
                        {
                            var child = viewViewsHost.Views[i];
                            Output($"{indent}    {_idViews[child]},");
                        }

                        Output($"{indent}}};");
                    }
                }

                if (view is IWindowsHost viewWindowsHost)
                {
                    if (viewWindowsHost.Windows != null && viewWindowsHost.Windows.Count > 0)
                    {
                        Output($"{indent}{id}.Windows = new ObservableCollection<IDockWindow>");
                        Output($"{indent}{{");

                        for (int i = 0; i < viewWindowsHost.Windows.Count; i++)
                        {
                            var child = viewWindowsHost.Windows[i];
                            Output($"{indent}    {_idWindows[child]},");
                        }

                        Output($"{indent}}};");
                    }
                }
            }

            Output($"{indent}return {_idViews[root]};");
        }

        private void WriterHeader()
        {
            Output(@"using System;
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
        {");
        }

        private void WriteFooter()
        {
            Output(@"        }

        /// <inheritdoc/>
        public override void InitLayout(IView layout, object context)
        {
            this.ContextLocator = new Dictionary<string, Func<object>>
            {
                [nameof(IRootDock)] = () => context,
                [nameof(ILayoutDock)] = () => context,
                [nameof(IDocumentDock)] = () => context,
                [nameof(IToolDock)] = () => context,
                [nameof(ISplitterDock)] = () => context,
                [nameof(IDockWindow)] = () => context,
                [nameof(IDocumentTab)] = () => context,
                [nameof(IToolTab)] = () => context
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
}");
        }

        /// <inheritdoc/>
        public void Generate(IView view, string path)
        {
            _sb = new StringBuilder();
            _idViews = new Dictionary<IView, string>();
            _idWindows = new Dictionary<IDockWindow, string>();
            _viewCount = 0;
            _windowCount = 0;

            string indent = "            ";

            WriterHeader();

            WriteObjects(view, indent);
            WriteLists(view, indent);

            WriteFooter();

            var text = _sb.ToString();

            Write(path, text);

            _sb.Clear();
        }
    }
}
